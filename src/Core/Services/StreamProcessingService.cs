using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace DevUtilities.Core.Services
{
    /// <summary>
    /// 流式处理服务，用于处理大文件和优化内存使用
    /// </summary>
    public class StreamProcessingService
    {
        private const int DefaultChunkSize = 64 * 1024; // 64KB
        private const int MaxMemoryThreshold = 50 * 1024 * 1024; // 50MB

        /// <summary>
        /// 处理进度回调
        /// </summary>
        public delegate void ProgressCallback(int percentage, string status);

        /// <summary>
        /// 分块处理大文本
        /// </summary>
        /// <param name="input">输入文本</param>
        /// <param name="processor">处理函数</param>
        /// <param name="chunkSize">块大小</param>
        /// <param name="progress">进度回调</param>
        /// <param name="cancellationToken">取消令牌</param>
        /// <returns>处理结果</returns>
        public async Task<string> ProcessInChunksAsync(
            string input,
            Func<string, Task<string>> processor,
            int chunkSize = DefaultChunkSize,
            ProgressCallback? progress = null,
            CancellationToken cancellationToken = default)
        {
            if (string.IsNullOrEmpty(input))
                return string.Empty;

            // 检查内存使用情况
            var inputSize = Encoding.UTF8.GetByteCount(input);
            if (inputSize <= chunkSize)
            {
                // 小文件直接处理
                progress?.Invoke(0, "开始处理...");
                var result = await processor(input);
                progress?.Invoke(100, "处理完成");
                return result;
            }

            // 大文件分块处理
            progress?.Invoke(0, "准备分块处理...");
            
            var chunks = SplitIntoChunks(input, chunkSize);
            var results = new List<string>();
            var totalChunks = chunks.Count;

            for (int i = 0; i < totalChunks; i++)
            {
                cancellationToken.ThrowIfCancellationRequested();

                var chunk = chunks[i];
                progress?.Invoke((i * 100) / totalChunks, $"处理块 {i + 1}/{totalChunks}...");

                try
                {
                    var processedChunk = await processor(chunk);
                    results.Add(processedChunk);
                }
                catch (Exception ex)
                {
                    throw new InvalidOperationException($"处理第 {i + 1} 块时出错: {ex.Message}", ex);
                }

                // 检查内存使用情况
                if (ShouldTriggerGC())
                {
                    GC.Collect();
                    GC.WaitForPendingFinalizers();
                }
            }

            progress?.Invoke(100, "合并结果...");
            return string.Join("", results);
        }

        /// <summary>
        /// 流式处理文本，适用于JSON等结构化数据
        /// </summary>
        /// <param name="input">输入文本</param>
        /// <param name="processor">处理函数</param>
        /// <param name="progress">进度回调</param>
        /// <param name="cancellationToken">取消令牌</param>
        /// <returns>处理结果</returns>
        public async Task<string> ProcessStreamAsync(
            string input,
            Func<StringReader, StringWriter, Task> processor,
            ProgressCallback? progress = null,
            CancellationToken cancellationToken = default)
        {
            if (string.IsNullOrEmpty(input))
                return string.Empty;

            progress?.Invoke(0, "开始流式处理...");

            using var reader = new StringReader(input);
            using var writer = new StringWriter();

            try
            {
                await processor(reader, writer);
                progress?.Invoke(100, "处理完成");
                return writer.ToString();
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException($"流式处理失败: {ex.Message}", ex);
            }
        }

        /// <summary>
        /// 将文本分割成块
        /// </summary>
        private List<string> SplitIntoChunks(string input, int chunkSize)
        {
            var chunks = new List<string>();
            var lines = input.Split('\n');
            var currentChunk = new StringBuilder();
            var currentSize = 0;

            foreach (var line in lines)
            {
                var lineSize = Encoding.UTF8.GetByteCount(line + "\n");
                
                if (currentSize + lineSize > chunkSize && currentChunk.Length > 0)
                {
                    // 当前块已满，保存并开始新块
                    chunks.Add(currentChunk.ToString());
                    currentChunk.Clear();
                    currentSize = 0;
                }

                currentChunk.AppendLine(line);
                currentSize += lineSize;
            }

            // 添加最后一块
            if (currentChunk.Length > 0)
            {
                chunks.Add(currentChunk.ToString());
            }

            return chunks;
        }

        /// <summary>
        /// 检查是否应该触发垃圾回收
        /// </summary>
        private bool ShouldTriggerGC()
        {
            var memoryUsage = GC.GetTotalMemory(false);
            return memoryUsage > MaxMemoryThreshold;
        }

        /// <summary>
        /// 获取内存使用情况
        /// </summary>
        public static MemoryInfo GetMemoryInfo()
        {
            var totalMemory = GC.GetTotalMemory(false);
            var gen0Collections = GC.CollectionCount(0);
            var gen1Collections = GC.CollectionCount(1);
            var gen2Collections = GC.CollectionCount(2);

            return new MemoryInfo
            {
                TotalMemoryBytes = totalMemory,
                TotalMemoryMB = totalMemory / (1024.0 * 1024.0),
                Gen0Collections = gen0Collections,
                Gen1Collections = gen1Collections,
                Gen2Collections = gen2Collections
            };
        }
    }

    /// <summary>
    /// 内存使用信息
    /// </summary>
    public class MemoryInfo
    {
        public long TotalMemoryBytes { get; set; }
        public double TotalMemoryMB { get; set; }
        public int Gen0Collections { get; set; }
        public int Gen1Collections { get; set; }
        public int Gen2Collections { get; set; }
    }
}