using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using Avalonia;
using Avalonia.Controls.ApplicationLifetimes;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

namespace DevUtilities.ViewModels;

public partial class UlidGeneratorViewModel : ObservableObject
{
    [ObservableProperty]
    private string selectedFormat = "标准格式";

    [ObservableProperty]
    private string generatedUlid = "";

    [ObservableProperty]
    private int generateCount = 1;

    [ObservableProperty]
    private bool upperCase = true;

    [ObservableProperty]
    private string batchResults = "";

    [ObservableProperty]
    private string customTimestamp = "";

    [ObservableProperty]
    private bool useCustomTimestamp = false;

    public ObservableCollection<string> UlidFormats { get; } = new()
    {
        "标准格式",
        "小写格式",
        "Base32格式",
        "时间戳+随机数分离"
    };

    public ObservableCollection<string> GeneratedHistory { get; } = new();

    public UlidGeneratorViewModel()
    {
        GenerateNewUlid();
    }

    [RelayCommand]
    private void GenerateNewUlid()
    {
        try
        {
            var ulid = UseCustomTimestamp && DateTime.TryParse(CustomTimestamp, out var customTime)
                ? Ulid.NewUlid(customTime)
                : Ulid.NewUlid();
            
            GeneratedUlid = FormatUlid(ulid, SelectedFormat);
            
            // 添加到历史记录
            if (!GeneratedHistory.Contains(GeneratedUlid))
            {
                GeneratedHistory.Insert(0, GeneratedUlid);
                
                // 限制历史记录数量
                while (GeneratedHistory.Count > 50)
                {
                    GeneratedHistory.RemoveAt(GeneratedHistory.Count - 1);
                }
            }
        }
        catch (Exception ex)
        {
            GeneratedUlid = $"生成失败: {ex.Message}";
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
                var ulid = UseCustomTimestamp && DateTime.TryParse(CustomTimestamp, out var customTime)
                    ? Ulid.NewUlid(customTime)
                    : Ulid.NewUlid();
                
                var formatted = FormatUlid(ulid, SelectedFormat);
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
    private void CopyUlid()
    {
        CopyToClipboard(GeneratedUlid);
    }

    [RelayCommand]
    private void CopyBatch()
    {
        CopyToClipboard(BatchResults);
    }

    [RelayCommand]
    private void CopyHistory(string ulid)
    {
        if (!string.IsNullOrEmpty(ulid))
        {
            CopyToClipboard(ulid);
        }
    }

    [RelayCommand]
    private void ClearHistory()
    {
        GeneratedHistory.Clear();
        BatchResults = "";
    }

    [RelayCommand]
    private void ValidateUlid(string input)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(input))
            {
                return;
            }

            // 尝试解析ULID
            if (Ulid.TryParse(input, out var ulid))
            {
                var timestamp = ulid.Time;
                var randomPart = ulid.Random;
                
                // 显示解析结果
                var info = $"有效的ULID\n时间戳: {timestamp:yyyy-MM-dd HH:mm:ss.fff}\n随机部分: {Convert.ToHexString(ulid.ToByteArray().Skip(6).ToArray())}";
                // 这里可以通过事件或其他方式通知UI显示验证结果
            }
            else
            {
                // 无效的ULID格式
            }
        }
        catch (Exception)
        {
            // 验证失败
        }
    }

    [RelayCommand]
    private void ParseUlid(string input)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(input))
            {
                return;
            }

            if (Ulid.TryParse(input, out var ulid))
            {
                var timestamp = ulid.Time;
                var info = $"ULID解析结果:\n" +
                          $"时间戳: {timestamp:yyyy-MM-dd HH:mm:ss.fff} UTC\n" +
                          $"Unix时间戳: {((DateTimeOffset)timestamp).ToUnixTimeMilliseconds()}\n" +
                          $"标准格式: {ulid}\n" +
                          $"小写格式: {ulid.ToString().ToLower()}\n" +
                          $"字节数组: {Convert.ToHexString(ulid.ToByteArray())}";
                
                BatchResults = info;
            }
            else
            {
                BatchResults = "无效的ULID格式";
            }
        }
        catch (Exception ex)
        {
            BatchResults = $"解析失败: {ex.Message}";
        }
    }

    private string FormatUlid(Ulid ulid, string format)
    {
        return format switch
        {
            "标准格式" => ulid.ToString(),
            "小写格式" => ulid.ToString().ToLower(),
            "Base32格式" => ulid.ToString(),
            "时间戳+随机数分离" => $"时间戳: {ulid.Time:yyyy-MM-dd HH:mm:ss.fff}\nULID: {ulid}",
            _ => ulid.ToString()
        };
    }

    private void CopyToClipboard(string text)
    {
        try
        {
            if (Application.Current?.ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
            {
                desktop.MainWindow?.Clipboard?.SetTextAsync(text);
            }
        }
        catch (Exception)
        {
            // 复制失败时静默处理
        }
    }

    partial void OnSelectedFormatChanged(string value)
    {
        if (!string.IsNullOrEmpty(GeneratedUlid) && !GeneratedUlid.StartsWith("生成失败"))
        {
            // 重新格式化当前ULID
            try
            {
                if (Ulid.TryParse(GeneratedUlid.Split('\n')[0], out var ulid))
                {
                    GeneratedUlid = FormatUlid(ulid, value);
                }
            }
            catch
            {
                // 格式化失败时重新生成
                GenerateNewUlid();
            }
        }
    }

    partial void OnUseCustomTimestampChanged(bool value)
    {
        if (!value)
        {
            CustomTimestamp = "";
        }
        else
        {
            CustomTimestamp = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
        }
    }
}