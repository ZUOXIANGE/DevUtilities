using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using Avalonia;
using Avalonia.Controls.ApplicationLifetimes;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

namespace DevUtilities.ViewModels;

public partial class UuidGeneratorViewModel : ObservableObject
{
    [ObservableProperty]
    private string selectedFormat = "标准格式";

    [ObservableProperty]
    private string generatedUuid = "";

    [ObservableProperty]
    private int generateCount = 1;

    [ObservableProperty]
    private bool includeHyphens = true;

    [ObservableProperty]
    private bool upperCase = false;

    [ObservableProperty]
    private string batchResults = "";

    public ObservableCollection<string> UuidFormats { get; } = new()
    {
        "标准格式",
        "无连字符",
        "大括号格式",
        "圆括号格式",
        "Base64格式",
        "短UUID"
    };

    public ObservableCollection<string> GeneratedHistory { get; } = new();

    public UuidGeneratorViewModel()
    {
        GenerateNewUuid();
    }

    [RelayCommand]
    private void GenerateNewUuid()
    {
        try
        {
            var uuid = Guid.NewGuid();
            GeneratedUuid = FormatUuid(uuid, SelectedFormat);
            
            // 添加到历史记录
            if (!GeneratedHistory.Contains(GeneratedUuid))
            {
                GeneratedHistory.Insert(0, GeneratedUuid);
                
                // 限制历史记录数量
                while (GeneratedHistory.Count > 50)
                {
                    GeneratedHistory.RemoveAt(GeneratedHistory.Count - 1);
                }
            }
        }
        catch (Exception ex)
        {
            GeneratedUuid = $"生成失败: {ex.Message}";
        }
    }

    [RelayCommand]
    private void GenerateBatch()
    {
        try
        {
            var results = new List<string>();
            var count = Math.Max(1, Math.Min(GenerateCount, 1000)); // 限制在1-1000之间

            for (int i = 0; i < count; i++)
            {
                var uuid = Guid.NewGuid();
                var formatted = FormatUuid(uuid, SelectedFormat);
                results.Add(formatted);
                
                // 添加到历史记录
                if (!GeneratedHistory.Contains(formatted))
                {
                    GeneratedHistory.Insert(0, formatted);
                }
            }

            BatchResults = string.Join(Environment.NewLine, results);

            // 限制历史记录数量
            while (GeneratedHistory.Count > 50)
            {
                GeneratedHistory.RemoveAt(GeneratedHistory.Count - 1);
            }
        }
        catch (Exception ex)
        {
            BatchResults = $"批量生成失败: {ex.Message}";
        }
    }

    [RelayCommand]
    private void CopyUuid()
    {
        CopyToClipboard(GeneratedUuid);
    }

    [RelayCommand]
    private void CopyBatch()
    {
        CopyToClipboard(BatchResults);
    }

    [RelayCommand]
    private void CopyHistory(string uuid)
    {
        if (!string.IsNullOrEmpty(uuid))
        {
            CopyToClipboard(uuid);
        }
    }

    [RelayCommand]
    private void ClearHistory()
    {
        GeneratedHistory.Clear();
        BatchResults = "";
    }

    [RelayCommand]
    private void ValidateUuid(string input)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(input))
            {
                return;
            }

            // 尝试解析UUID
            if (Guid.TryParse(input, out var guid))
            {
                // 显示UUID信息
                var info = GetUuidInfo(guid);
                BatchResults = info;
            }
            else
            {
                BatchResults = "无效的UUID格式";
            }
        }
        catch (Exception ex)
        {
            BatchResults = $"验证失败: {ex.Message}";
        }
    }

    private string FormatUuid(Guid uuid, string format)
    {
        var result = format switch
        {
            "标准格式" => uuid.ToString("D"),
            "无连字符" => uuid.ToString("N"),
            "大括号格式" => uuid.ToString("B"),
            "圆括号格式" => uuid.ToString("P"),
            "Base64格式" => Convert.ToBase64String(uuid.ToByteArray()).TrimEnd('='),
            "短UUID" => GenerateShortUuid(uuid),
            _ => uuid.ToString("D")
        };

        return UpperCase ? result.ToUpper() : result.ToLower();
    }

    private string GenerateShortUuid(Guid uuid)
    {
        // 生成22字符的短UUID
        var bytes = uuid.ToByteArray();
        var base64 = Convert.ToBase64String(bytes);
        return base64.Replace('+', '-').Replace('/', '_').TrimEnd('=');
    }

    private string GetUuidInfo(Guid uuid)
    {
        var bytes = uuid.ToByteArray();
        var version = (bytes[7] & 0xF0) >> 4;
        var variant = (bytes[8] & 0xC0) >> 6;

        var info = $"UUID信息:\n";
        info += $"标准格式: {uuid:D}\n";
        info += $"无连字符: {uuid:N}\n";
        info += $"大括号格式: {uuid:B}\n";
        info += $"圆括号格式: {uuid:P}\n";
        info += $"Base64格式: {Convert.ToBase64String(bytes).TrimEnd('=')}\n";
        info += $"短UUID: {GenerateShortUuid(uuid)}\n";
        info += $"版本: {version}\n";
        info += $"变体: {variant}\n";
        info += $"字节数组: {BitConverter.ToString(bytes)}";

        return info;
    }

    partial void OnSelectedFormatChanged(string value)
    {
        if (!string.IsNullOrEmpty(GeneratedUuid) && !GeneratedUuid.StartsWith("生成失败"))
        {
            // 重新格式化当前UUID
            if (Guid.TryParse(GeneratedUuid, out var currentUuid))
            {
                GeneratedUuid = FormatUuid(currentUuid, value);
            }
            else
            {
                GenerateNewUuid();
            }
        }
    }

    partial void OnUpperCaseChanged(bool value)
    {
        if (!string.IsNullOrEmpty(GeneratedUuid) && !GeneratedUuid.StartsWith("生成失败"))
        {
            GeneratedUuid = value ? GeneratedUuid.ToUpper() : GeneratedUuid.ToLower();
        }

        if (!string.IsNullOrEmpty(BatchResults) && !BatchResults.StartsWith("批量生成失败"))
        {
            var lines = BatchResults.Split(Environment.NewLine);
            BatchResults = string.Join(Environment.NewLine, 
                lines.Select(line => value ? line.ToUpper() : line.ToLower()));
        }
    }

    private async void CopyToClipboard(string text)
    {
        try
        {
            if (Application.Current?.ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
            {
                var clipboard = desktop.MainWindow?.Clipboard;
                if (clipboard != null)
                {
                    await clipboard.SetTextAsync(text);
                    BatchResults = "已复制到剪贴板";
                }
            }
        }
        catch (Exception ex)
        {
            BatchResults = $"复制失败: {ex.Message}";
        }
    }
}