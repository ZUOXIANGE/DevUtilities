using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Platform.Storage;
using DevUtilities.Core.Services.Interfaces;

namespace DevUtilities.Core.Services.Implementations;

/// <summary>
/// 文件服务实现
/// </summary>
public class FileService : IFileService
{
    /// <summary>
    /// 读取文件内容
    /// </summary>
    /// <param name="filePath">文件路径</param>
    /// <returns>文件内容</returns>
    public async Task<string> ReadTextAsync(string filePath)
    {
        try
        {
            if (!File.Exists(filePath))
                return string.Empty;

            return await File.ReadAllTextAsync(filePath);
        }
        catch (Exception)
        {
            return string.Empty;
        }
    }

    /// <summary>
    /// 写入文件内容
    /// </summary>
    /// <param name="filePath">文件路径</param>
    /// <param name="content">文件内容</param>
    /// <returns>是否写入成功</returns>
    public async Task<bool> WriteTextAsync(string filePath, string content)
    {
        try
        {
            var directory = Path.GetDirectoryName(filePath);
            if (!string.IsNullOrEmpty(directory) && !Directory.Exists(directory))
            {
                Directory.CreateDirectory(directory);
            }

            await File.WriteAllTextAsync(filePath, content ?? string.Empty);
            return true;
        }
        catch (Exception)
        {
            return false;
        }
    }

    /// <summary>
    /// 读取文件字节数据
    /// </summary>
    /// <param name="filePath">文件路径</param>
    /// <returns>文件字节数据</returns>
    public async Task<byte[]> ReadBytesAsync(string filePath)
    {
        try
        {
            if (!File.Exists(filePath))
                return Array.Empty<byte>();

            return await File.ReadAllBytesAsync(filePath);
        }
        catch (Exception)
        {
            return Array.Empty<byte>();
        }
    }

    /// <summary>
    /// 写入文件字节数据
    /// </summary>
    /// <param name="filePath">文件路径</param>
    /// <param name="data">字节数据</param>
    /// <returns>是否写入成功</returns>
    public async Task<bool> WriteBytesAsync(string filePath, byte[] data)
    {
        try
        {
            var directory = Path.GetDirectoryName(filePath);
            if (!string.IsNullOrEmpty(directory) && !Directory.Exists(directory))
            {
                Directory.CreateDirectory(directory);
            }

            await File.WriteAllBytesAsync(filePath, data ?? Array.Empty<byte>());
            return true;
        }
        catch (Exception)
        {
            return false;
        }
    }

    /// <summary>
    /// 检查文件是否存在
    /// </summary>
    /// <param name="filePath">文件路径</param>
    /// <returns>文件是否存在</returns>
    public bool FileExists(string filePath)
    {
        try
        {
            return File.Exists(filePath);
        }
        catch (Exception)
        {
            return false;
        }
    }

    /// <summary>
    /// 检查目录是否存在
    /// </summary>
    /// <param name="directoryPath">目录路径</param>
    /// <returns>目录是否存在</returns>
    public bool DirectoryExists(string directoryPath)
    {
        try
        {
            return Directory.Exists(directoryPath);
        }
        catch (Exception)
        {
            return false;
        }
    }

    /// <summary>
    /// 创建目录
    /// </summary>
    /// <param name="directoryPath">目录路径</param>
    /// <returns>是否创建成功</returns>
    public bool CreateDirectory(string directoryPath)
    {
        try
        {
            if (!Directory.Exists(directoryPath))
            {
                Directory.CreateDirectory(directoryPath);
            }
            return true;
        }
        catch (Exception)
        {
            return false;
        }
    }

    /// <summary>
    /// 删除文件
    /// </summary>
    /// <param name="filePath">文件路径</param>
    /// <returns>是否删除成功</returns>
    public bool DeleteFile(string filePath)
    {
        try
        {
            if (File.Exists(filePath))
            {
                File.Delete(filePath);
            }
            return true;
        }
        catch (Exception)
        {
            return false;
        }
    }

    /// <summary>
    /// 获取文件大小
    /// </summary>
    /// <param name="filePath">文件路径</param>
    /// <returns>文件大小（字节）</returns>
    public long GetFileSize(string filePath)
    {
        try
        {
            if (!File.Exists(filePath))
                return 0;

            var fileInfo = new FileInfo(filePath);
            return fileInfo.Length;
        }
        catch (Exception)
        {
            return 0;
        }
    }

    /// <summary>
    /// 获取文件扩展名
    /// </summary>
    /// <param name="filePath">文件路径</param>
    /// <returns>文件扩展名</returns>
    public string GetFileExtension(string filePath)
    {
        try
        {
            return Path.GetExtension(filePath);
        }
        catch (Exception)
        {
            return string.Empty;
        }
    }

    /// <summary>
    /// 获取文件名（不含扩展名）
    /// </summary>
    /// <param name="filePath">文件路径</param>
    /// <returns>文件名</returns>
    public string GetFileNameWithoutExtension(string filePath)
    {
        try
        {
            return Path.GetFileNameWithoutExtension(filePath);
        }
        catch (Exception)
        {
            return string.Empty;
        }
    }

    /// <summary>
    /// 获取目录中的所有文件
    /// </summary>
    /// <param name="directoryPath">目录路径</param>
    /// <param name="searchPattern">搜索模式</param>
    /// <param name="recursive">是否递归搜索</param>
    /// <returns>文件路径列表</returns>
    public IEnumerable<string> GetFiles(string directoryPath, string searchPattern = "*", bool recursive = false)
    {
        try
        {
            if (!Directory.Exists(directoryPath))
                return Enumerable.Empty<string>();

            var searchOption = recursive ? SearchOption.AllDirectories : SearchOption.TopDirectoryOnly;
            return Directory.GetFiles(directoryPath, searchPattern, searchOption);
        }
        catch (Exception)
        {
            return Enumerable.Empty<string>();
        }
    }

    /// <summary>
    /// 选择文件对话框
    /// </summary>
    /// <param name="title">对话框标题</param>
    /// <param name="filters">文件过滤器</param>
    /// <returns>选择的文件路径，取消则返回null</returns>
    public async Task<string?> ShowOpenFileDialogAsync(string title = "选择文件", string filters = "所有文件|*.*")
    {
        try
        {
            var topLevel = GetTopLevel();
            if (topLevel?.StorageProvider == null)
                return null;

            var fileTypes = ParseFileFilters(filters);
            var options = new FilePickerOpenOptions
            {
                Title = title,
                AllowMultiple = false,
                FileTypeFilter = fileTypes
            };

            var result = await topLevel.StorageProvider.OpenFilePickerAsync(options);
            return result?.FirstOrDefault()?.Path.LocalPath;
        }
        catch (Exception)
        {
            return null;
        }
    }

    /// <summary>
    /// 保存文件对话框
    /// </summary>
    /// <param name="title">对话框标题</param>
    /// <param name="defaultFileName">默认文件名</param>
    /// <param name="filters">文件过滤器</param>
    /// <returns>保存的文件路径，取消则返回null</returns>
    public async Task<string?> ShowSaveFileDialogAsync(string title = "保存文件", string defaultFileName = "", string filters = "所有文件|*.*")
    {
        try
        {
            var topLevel = GetTopLevel();
            if (topLevel?.StorageProvider == null)
                return null;

            var fileTypes = ParseFileFilters(filters);
            var options = new FilePickerSaveOptions
            {
                Title = title,
                SuggestedFileName = defaultFileName,
                FileTypeChoices = fileTypes
            };

            var result = await topLevel.StorageProvider.SaveFilePickerAsync(options);
            return result?.Path.LocalPath;
        }
        catch (Exception)
        {
            return null;
        }
    }

    /// <summary>
    /// 获取顶级窗口
    /// </summary>
    /// <returns>顶级窗口</returns>
    private TopLevel? GetTopLevel()
    {
        if (Application.Current?.ApplicationLifetime is Avalonia.Controls.ApplicationLifetimes.IClassicDesktopStyleApplicationLifetime desktop)
        {
            return desktop.MainWindow;
        }

        return null;
    }

    /// <summary>
    /// 解析文件过滤器
    /// </summary>
    /// <param name="filters">过滤器字符串</param>
    /// <returns>文件类型列表</returns>
    private List<FilePickerFileType> ParseFileFilters(string filters)
    {
        var fileTypes = new List<FilePickerFileType>();

        try
        {
            var filterPairs = filters.Split('|');
            for (int i = 0; i < filterPairs.Length; i += 2)
            {
                if (i + 1 < filterPairs.Length)
                {
                    var name = filterPairs[i];
                    var pattern = filterPairs[i + 1];
                    
                    var extensions = pattern.Split(';')
                        .Select(p => p.Trim())
                        .Where(p => !string.IsNullOrEmpty(p))
                        .ToArray();

                    if (extensions.Length > 0)
                    {
                        fileTypes.Add(new FilePickerFileType(name)
                        {
                            Patterns = extensions
                        });
                    }
                }
            }
        }
        catch (Exception)
        {
            // 如果解析失败，返回默认的所有文件类型
        }

        if (fileTypes.Count == 0)
        {
            fileTypes.Add(FilePickerFileTypes.All);
        }

        return fileTypes;
    }
}