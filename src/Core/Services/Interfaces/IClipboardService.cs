using System.Threading.Tasks;

namespace DevUtilities.Core.Services.Interfaces;

/// <summary>
/// 剪贴板服务接口
/// </summary>
public interface IClipboardService
{
    /// <summary>
    /// 复制文本到剪贴板
    /// </summary>
    /// <param name="text">要复制的文本</param>
    /// <returns>是否复制成功</returns>
    Task<bool> CopyTextAsync(string text);

    /// <summary>
    /// 从剪贴板获取文本
    /// </summary>
    /// <returns>剪贴板中的文本</returns>
    Task<string> GetTextAsync();

    /// <summary>
    /// 检查剪贴板是否包含文本
    /// </summary>
    /// <returns>是否包含文本</returns>
    Task<bool> HasTextAsync();

    /// <summary>
    /// 清空剪贴板
    /// </summary>
    /// <returns>是否清空成功</returns>
    Task<bool> ClearAsync();
}