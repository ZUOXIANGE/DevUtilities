using System;
using System.Threading.Tasks;
using Avalonia;
using Avalonia.Controls;
using DevUtilities.Core.Services.Interfaces;
using Serilog;

namespace DevUtilities.Core.Services.Implementations;

/// <summary>
/// 剪贴板服务实现
/// </summary>
public class ClipboardService : IClipboardService
{
    /// <summary>
    /// 获取剪贴板文本内容
    /// </summary>
    /// <returns>剪贴板文本内容</returns>
    public async Task<string> GetTextAsync()
    {
        Log.Debug("[ClipboardService] 开始获取剪贴板文本内容");
        
        try
        {
            var topLevel = GetTopLevel();
            if (topLevel?.Clipboard == null)
            {
                Log.Warning("[ClipboardService] 无法获取剪贴板实例");
                return string.Empty;
            }

            var text = await topLevel.Clipboard.GetTextAsync();
            Log.Debug("[ClipboardService] 剪贴板文本获取成功，长度: {TextLength}", text?.Length ?? 0);
            return text ?? string.Empty;
        }
        catch (Exception ex)
        {
            Log.Error(ex, "[ClipboardService] 获取剪贴板文本失败");
            return string.Empty;
        }
    }

    /// <summary>
    /// 设置剪贴板文本内容
    /// </summary>
    /// <param name="text">要设置的文本内容</param>
    /// <returns>是否设置成功</returns>
    public async Task<bool> SetTextAsync(string? text)
    {
        Log.Debug("[ClipboardService] 开始设置剪贴板文本内容，长度: {TextLength}", text?.Length ?? 0);
        
        try
        {
            var topLevel = GetTopLevel();
            if (topLevel?.Clipboard == null)
            {
                Log.Warning("[ClipboardService] 无法获取剪贴板实例");
                return false;
            }

            await topLevel.Clipboard.SetTextAsync(text ?? string.Empty);
            Log.Debug("[ClipboardService] 剪贴板文本设置成功");
            return true;
        }
        catch (Exception ex)
        {
            Log.Error(ex, "[ClipboardService] 设置剪贴板文本失败");
            return false;
        }
    }

    /// <summary>
    /// 清空剪贴板内容
    /// </summary>
    /// <returns>是否清空成功</returns>
    public async Task<bool> ClearAsync()
    {
        Log.Debug("[ClipboardService] 开始清空剪贴板内容");
        
        try
        {
            var topLevel = GetTopLevel();
            if (topLevel?.Clipboard == null)
            {
                Log.Warning("[ClipboardService] 无法获取剪贴板实例");
                return false;
            }

            await topLevel.Clipboard.ClearAsync();
            Log.Debug("[ClipboardService] 剪贴板清空成功");
            return true;
        }
        catch (Exception ex)
        {
            Log.Error(ex, "[ClipboardService] 清空剪贴板失败");
            return false;
        }
    }

    /// <summary>
    /// 检查剪贴板是否包含文本
    /// </summary>
    /// <returns>是否包含文本</returns>
    public async Task<bool> HasTextAsync()
    {
        Log.Debug("[ClipboardService] 检查剪贴板是否包含文本");
        
        try
        {
            var text = await GetTextAsync();
            var hasText = !string.IsNullOrEmpty(text);
            Log.Debug("[ClipboardService] 剪贴板文本检查结果: {HasText}", hasText);
            return hasText;
        }
        catch (Exception ex)
        {
            Log.Error(ex, "[ClipboardService] 检查剪贴板文本时发生错误");
            return false;
        }
    }

    /// <summary>
    /// 复制文本到剪贴板（SetTextAsync的别名）
    /// </summary>
    /// <param name="text">要复制的文本</param>
    /// <returns>是否复制成功</returns>
    public async Task<bool> CopyTextAsync(string? text)
    {
        Log.Debug("[ClipboardService] 复制文本到剪贴板，长度: {TextLength}", text?.Length ?? 0);
        return await SetTextAsync(text);
    }

    /// <summary>
    /// 从剪贴板粘贴文本（GetTextAsync的别名）
    /// </summary>
    /// <returns>粘贴的文本内容</returns>
    public async Task<string?> PasteTextAsync()
    {
        Log.Debug("[ClipboardService] 从剪贴板粘贴文本");
        return await GetTextAsync();
    }

    /// <summary>
    /// 获取顶级窗口
    /// </summary>
    /// <returns>顶级窗口</returns>
    private TopLevel? GetTopLevel()
    {
        try
        {
            if (Application.Current?.ApplicationLifetime is Avalonia.Controls.ApplicationLifetimes.IClassicDesktopStyleApplicationLifetime desktop)
            {
                Log.Debug("[ClipboardService] 获取主窗口成功");
                return desktop.MainWindow;
            }

            Log.Warning("[ClipboardService] 无法获取主窗口");
            return null;
        }
        catch (Exception ex)
        {
            Log.Error(ex, "[ClipboardService] 获取顶级窗口时发生错误");
            return null;
        }
    }
}