using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Text.RegularExpressions;
using Avalonia;
using Avalonia.Controls.ApplicationLifetimes;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

namespace DevUtilities.ViewModels;

public partial class RegexTesterViewModel : ObservableObject
{
    [ObservableProperty]
    private string pattern = "";

    [ObservableProperty]
    private string testText = "";

    [ObservableProperty]
    private string replaceText = "";

    [ObservableProperty]
    private string replaceResult = "";

    [ObservableProperty]
    private bool ignoreCase = false;

    [ObservableProperty]
    private bool multiline = false;

    [ObservableProperty]
    private bool singleline = false;

    [ObservableProperty]
    private bool explicitCapture = false;

    [ObservableProperty]
    private bool ignoreWhitespace = false;

    [ObservableProperty]
    private string errorMessage = "";

    [ObservableProperty]
    private bool hasError = false;

    [ObservableProperty]
    private string matchSummary = "";

    public ObservableCollection<MatchResult> Matches { get; } = new();
    public ObservableCollection<string> CommonPatterns { get; } = new()
    {
        @"\d+",                    // 数字
        @"[a-zA-Z]+",             // 字母
        @"\w+",                   // 单词字符
        @"\s+",                   // 空白字符
        @"^.+$",                  // 整行
        @"\b\w+\b",               // 完整单词
        @"[0-9]{1,3}\.[0-9]{1,3}\.[0-9]{1,3}\.[0-9]{1,3}", // IP地址
        @"^[a-zA-Z0-9._%+-]+@[a-zA-Z0-9.-]+\.[a-zA-Z]{2,}$", // 邮箱
        @"^https?://[^\s/$.?#].[^\s]*$", // URL
        @"^1[3-9]\d{9}$",         // 手机号
        @"^\d{4}-\d{2}-\d{2}$",   // 日期 YYYY-MM-DD
        @"^#([A-Fa-f0-9]{6}|[A-Fa-f0-9]{3})$", // 十六进制颜色
    };

    public ObservableCollection<string> PatternHistory { get; } = new();

    public RegexTesterViewModel()
    {
        // 初始化示例
        Pattern = @"\d+";
        TestText = "今天是2024年1月15日，温度是25度";
        TestRegex();
    }

    partial void OnPatternChanged(string value)
    {
        TestRegex();
    }

    partial void OnTestTextChanged(string value)
    {
        TestRegex();
    }

    partial void OnIgnoreCaseChanged(bool value)
    {
        TestRegex();
    }

    partial void OnMultilineChanged(bool value)
    {
        TestRegex();
    }

    partial void OnSinglelineChanged(bool value)
    {
        TestRegex();
    }

    partial void OnExplicitCaptureChanged(bool value)
    {
        TestRegex();
    }

    partial void OnIgnoreWhitespaceChanged(bool value)
    {
        TestRegex();
    }

    [RelayCommand]
    private void TestRegex()
    {
        try
        {
            ClearError();
            Matches.Clear();

            if (string.IsNullOrEmpty(Pattern) || string.IsNullOrEmpty(TestText))
            {
                MatchSummary = "请输入正则表达式和测试文本";
                return;
            }

            var options = GetRegexOptions();
            var regex = new Regex(Pattern, options);
            var matches = regex.Matches(TestText);

            foreach (Match match in matches)
            {
                var matchResult = new MatchResult
                {
                    Value = match.Value,
                    Index = match.Index,
                    Length = match.Length,
                    Success = match.Success
                };

                // 添加捕获组
                for (int i = 0; i < match.Groups.Count; i++)
                {
                    var group = match.Groups[i];
                    matchResult.Groups.Add(new GroupResult
                    {
                        Name = i == 0 ? "整个匹配" : $"组 {i}",
                        Value = group.Value,
                        Index = group.Index,
                        Length = group.Length,
                        Success = group.Success
                    });
                }

                Matches.Add(matchResult);
            }

            MatchSummary = $"找到 {matches.Count} 个匹配项";

            // 添加到历史记录
            AddToHistory(Pattern);
        }
        catch (ArgumentException ex)
        {
            SetError($"正则表达式语法错误: {ex.Message}");
            MatchSummary = "正则表达式无效";
        }
        catch (Exception ex)
        {
            SetError($"测试失败: {ex.Message}");
            MatchSummary = "测试失败";
        }
    }

    [RelayCommand]
    private void ReplaceTextContent()
    {
        try
        {
            ClearError();

            if (string.IsNullOrEmpty(Pattern) || string.IsNullOrEmpty(TestText))
            {
                ReplaceResult = "请输入正则表达式和测试文本";
                return;
            }

            var options = GetRegexOptions();
            var regex = new Regex(Pattern, options);
            ReplaceResult = regex.Replace(TestText, ReplaceText ?? "");
        }
        catch (ArgumentException ex)
        {
            SetError($"正则表达式语法错误: {ex.Message}");
            ReplaceResult = "替换失败";
        }
        catch (Exception ex)
        {
            SetError($"替换失败: {ex.Message}");
            ReplaceResult = "替换失败";
        }
    }

    [RelayCommand]
    private void UseCommonPattern(string pattern)
    {
        if (!string.IsNullOrEmpty(pattern))
        {
            Pattern = pattern;
        }
    }

    [RelayCommand]
    private void UseHistoryPattern(string pattern)
    {
        if (!string.IsNullOrEmpty(pattern))
        {
            Pattern = pattern;
        }
    }

    [RelayCommand]
    private void ClearAll()
    {
        Pattern = "";
        TestText = "";
        ReplaceText = "";
        ReplaceResult = "";
        Matches.Clear();
        ClearError();
        MatchSummary = "";
    }

    [RelayCommand]
    private void CopyMatch(MatchResult match)
    {
        if (match != null)
        {
            CopyToClipboard(match.Value);
        }
    }

    [RelayCommand]
    private void CopyReplaceResult()
    {
        CopyToClipboard(ReplaceResult);
    }

    private RegexOptions GetRegexOptions()
    {
        var options = RegexOptions.None;

        if (IgnoreCase) options |= RegexOptions.IgnoreCase;
        if (Multiline) options |= RegexOptions.Multiline;
        if (Singleline) options |= RegexOptions.Singleline;
        if (ExplicitCapture) options |= RegexOptions.ExplicitCapture;
        if (IgnoreWhitespace) options |= RegexOptions.IgnorePatternWhitespace;

        return options;
    }

    private void AddToHistory(string pattern)
    {
        if (string.IsNullOrEmpty(pattern) || PatternHistory.Contains(pattern))
            return;

        PatternHistory.Insert(0, pattern);

        // 限制历史记录数量
        while (PatternHistory.Count > 20)
        {
            PatternHistory.RemoveAt(PatternHistory.Count - 1);
        }
    }

    private void SetError(string message)
    {
        ErrorMessage = message;
        HasError = true;
    }

    private void ClearError()
    {
        ErrorMessage = "";
        HasError = false;
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
                    SetError("已复制到剪贴板");
                    // 清除错误状态，显示成功消息
                    HasError = false;
                }
            }
        }
        catch (Exception ex)
        {
            SetError($"复制失败: {ex.Message}");
        }
    }
}

public class MatchResult
{
    public string Value { get; set; } = "";
    public int Index { get; set; }
    public int Length { get; set; }
    public bool Success { get; set; }
    public List<GroupResult> Groups { get; set; } = new();
}

public class GroupResult
{
    public string Name { get; set; } = "";
    public string Value { get; set; } = "";
    public int Index { get; set; }
    public int Length { get; set; }
    public bool Success { get; set; }
}