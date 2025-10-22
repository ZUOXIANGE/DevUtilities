using System;
using System.Linq;
using System.Threading.Tasks;
using Avalonia;
using Avalonia.Input.Platform;
using Avalonia.Input;
using DevUtilities.Core.Services.Interfaces;

namespace DevUtilities.Core.Services.Implementations;

/// <summary>
/// 剪贴板服务实现
/// </summary>
public class ClipboardService : IClipboardService
{
    private IClipboard? _clipboard;

    /// <summary>
    /// 获取剪贴板实例
    /// </summary>
    private IClipboard? GetClipboard()
    {
        if (_clipboard == null)
        {
            var topLevel = Application.Current?.ApplicationLifetime is Avalonia.Controls.ApplicationLifetimes.IClassicDesktopStyleApplicationLifetime desktop
                ? desktop.MainWindow
                : null;

            _clipboard = topLevel?.Clipboard;
        }

        return _clipboard;
    }

    /// <summary>
    /// 复制文本到剪贴板
    /// </summary>
    /// <param name="text">要复制的文本</param>
    /// <returns>是否复制成功</returns>
    public async Task<bool> CopyTextAsync(string text)
    {
        try
        {
            var clipboard = GetClipboard();
            if (clipboard == null)
                return false;

            await clipboard.SetTextAsync(text ?? string.Empty);
            return true;
        }
        catch (Exception)
        {
            return false;
        }
    }

    /// <summary>
    /// 获取剪贴板文本
    /// </summary>
    /// <returns>剪贴板中的文本</returns>
    public async Task<string> GetTextAsync()
    {
        try
        {
            var clipboard = GetClipboard();
            if (clipboard == null)
                return string.Empty;

            var text = await clipboard.TryGetTextAsync();
            return text ?? string.Empty;
        }
        catch (Exception)
        {
            return string.Empty;
        }
    }

    /// <summary>
    /// 检查剪贴板是否包含文本
    /// </summary>
    /// <returns>是否包含文本</returns>
    public async Task<bool> HasTextAsync()
    {
        try
        {
            var clipboard = GetClipboard();
            if (clipboard == null)
                return false;

            var formats = await clipboard.GetDataFormatsAsync();
            return formats.Any(f => f.ToString() == "text/plain" || f.ToString() == "Text");
        }
        catch (Exception)
        {
            return false;
        }
    }

    /// <summary>
    /// 清空剪贴板
    /// </summary>
    /// <returns>是否清空成功</returns>
    public async Task<bool> ClearAsync()
    {
        try
        {
            var clipboard = GetClipboard();
            if (clipboard == null)
                return false;

            await clipboard.ClearAsync();
            return true;
        }
        catch (Exception)
        {
            return false;
        }
    }
}