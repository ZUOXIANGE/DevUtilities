using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.ApplicationLifetimes;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

namespace DevUtilities.ViewModels;

public partial class HtmlFormatterViewModel : ObservableObject
{
    [ObservableProperty]
    private string inputHtml = "";

    [ObservableProperty]
    private string outputHtml = "";

    [ObservableProperty]
    private string validationMessage = "";

    [ObservableProperty]
    private bool isValidHtml = true;

    [ObservableProperty]
    private int indentSize = 2;

    [ObservableProperty]
    private bool formatAttributes = true;

    [ObservableProperty]
    private bool preserveWhitespace = false;

    [ObservableProperty]
    private bool sortAttributes = false;

    [ObservableProperty]
    private bool addLineBreaks = true;

    [ObservableProperty]
    private bool removeComments = false;

    [ObservableProperty]
    private string selectedDoctype = "HTML5";

    public List<string> AvailableDoctypes { get; } = new()
    {
        "HTML5",
        "XHTML 1.0 Strict",
        "XHTML 1.0 Transitional",
        "HTML 4.01 Strict",
        "HTML 4.01 Transitional"
    };

    public HtmlFormatterViewModel()
    {
        // BaseToolViewModel只有Message属性
    }

    partial void OnInputHtmlChanged(string value)
    {
        if (!string.IsNullOrWhiteSpace(value))
        {
            ValidateHtml();
        }
        else
        {
            ClearValidation();
        }
    }

    [RelayCommand]
    private void FormatHtml()
    {
        try
        {
            if (string.IsNullOrWhiteSpace(InputHtml))
            {
                OutputHtml = "";
                ClearValidation();
                return;
            }

            var formatted = FormatHtmlContent(InputHtml);
            OutputHtml = formatted;
            SetValidation("HTML格式化成功", true);
        }
        catch (Exception ex)
        {
            SetValidation($"格式化失败: {ex.Message}", false);
        }
    }

    [RelayCommand]
    private void MinifyHtml()
    {
        try
        {
            if (string.IsNullOrWhiteSpace(InputHtml))
            {
                OutputHtml = "";
                ClearValidation();
                return;
            }

            var minified = MinifyHtmlContent(InputHtml);
            OutputHtml = minified;
            SetValidation("HTML压缩成功", true);
        }
        catch (Exception ex)
        {
            SetValidation($"压缩失败: {ex.Message}", false);
        }
    }

    [RelayCommand]
    private void ValidateHtml()
    {
        try
        {
            if (string.IsNullOrWhiteSpace(InputHtml))
            {
                ClearValidation();
                return;
            }

            var issues = ValidateHtmlContent(InputHtml);
            if (issues.Count == 0)
            {
                SetValidation("HTML语法正确", true);
            }
            else
            {
                var message = $"发现 {issues.Count} 个问题:\n" + string.Join("\n", issues.Take(5));
                if (issues.Count > 5)
                {
                    message += $"\n... 还有 {issues.Count - 5} 个问题";
                }
                SetValidation(message, false);
            }
        }
        catch (Exception ex)
        {
            SetValidation($"验证失败: {ex.Message}", false);
        }
    }

    [RelayCommand]
    private void SwapInputOutput()
    {
        (InputHtml, OutputHtml) = (OutputHtml, InputHtml);
    }

    [RelayCommand]
    private void UseExample()
    {
        InputHtml = @"<!DOCTYPE html>
<html><head><title>示例页面</title><meta charset=""UTF-8""><style>body{margin:0;padding:20px;font-family:Arial,sans-serif;}h1{color:#333;}.container{max-width:800px;margin:0 auto;}</style></head><body><div class=""container""><h1>欢迎使用HTML格式化器</h1><p>这是一个示例HTML文档，包含了常见的HTML元素。</p><ul><li>列表项目1</li><li>列表项目2</li><li>列表项目3</li></ul><div><p>嵌套的段落内容。</p><a href=""#"">链接示例</a></div><form><input type=""text"" name=""username"" placeholder=""用户名""><input type=""password"" name=""password"" placeholder=""密码""><button type=""submit"">提交</button></form></div></body></html>";
    }

    [RelayCommand]
    private void ClearAll()
    {
        InputHtml = "";
        OutputHtml = "";
        ClearValidation();
    }

    [RelayCommand]
    private async Task CopyOutput()
    {
        try
        {
            if (Application.Current?.ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
            {
                var clipboard = desktop.MainWindow?.Clipboard;
                if (clipboard != null && !string.IsNullOrEmpty(OutputHtml))
                {
                    await clipboard.SetTextAsync(OutputHtml);
                    SetValidation("已复制到剪贴板", true);
                }
            }
        }
        catch (Exception ex)
        {
            SetValidation($"复制失败: {ex.Message}", false);
        }
    }

    private string FormatHtmlContent(string html)
    {
        var result = html.Trim();

        // 移除注释（如果选择了移除注释）
        if (RemoveComments)
        {
            result = Regex.Replace(result, @"<!--.*?-->", "", RegexOptions.Singleline);
        }

        // 基本的HTML格式化
        result = FormatBasicHtml(result);

        return result;
    }

    private string FormatBasicHtml(string html)
    {
        var result = new StringBuilder();
        var indentLevel = 0;
        var indent = new string(' ', IndentSize);
        
        // 简单的HTML标签匹配
        var tagPattern = @"<(/?)(\w+)([^>]*)>";
        var matches = Regex.Matches(html, tagPattern);
        var lastIndex = 0;

        foreach (Match match in matches)
        {
            // 添加标签前的文本内容
            var textBefore = html.Substring(lastIndex, match.Index - lastIndex).Trim();
            if (!string.IsNullOrEmpty(textBefore))
            {
                if (AddLineBreaks)
                {
                    result.AppendLine(new string(' ', indentLevel * IndentSize) + textBefore);
                }
                else
                {
                    result.Append(textBefore);
                }
            }

            var isClosingTag = !string.IsNullOrEmpty(match.Groups[1].Value);
            var tagName = match.Groups[2].Value.ToLower();
            var attributes = match.Groups[3].Value;

            // 处理缩进
            if (isClosingTag)
            {
                indentLevel = Math.Max(0, indentLevel - 1);
            }

            // 格式化标签
            var formattedTag = FormatTag(match.Value, attributes);
            
            if (AddLineBreaks)
            {
                result.AppendLine(new string(' ', indentLevel * IndentSize) + formattedTag);
            }
            else
            {
                result.Append(formattedTag);
            }

            // 自闭合标签不需要增加缩进
            if (!isClosingTag && !IsSelfClosingTag(tagName) && !match.Value.EndsWith("/>"))
            {
                indentLevel++;
            }

            lastIndex = match.Index + match.Length;
        }

        // 添加剩余的文本
        var remainingText = html.Substring(lastIndex).Trim();
        if (!string.IsNullOrEmpty(remainingText))
        {
            if (AddLineBreaks)
            {
                result.AppendLine(new string(' ', indentLevel * IndentSize) + remainingText);
            }
            else
            {
                result.Append(remainingText);
            }
        }

        return result.ToString().Trim();
    }

    private string FormatTag(string tag, string attributes)
    {
        if (!FormatAttributes || string.IsNullOrEmpty(attributes))
        {
            return tag;
        }

        // 简单的属性格式化
        var formattedAttributes = attributes.Trim();
        
        if (SortAttributes)
        {
            // 简单的属性排序（基于属性名）
            var attrPattern = @"(\w+)=(""[^""]*""|'[^']*'|\S+)";
            var attrMatches = Regex.Matches(formattedAttributes, attrPattern);
            var sortedAttrs = attrMatches.Cast<Match>()
                .OrderBy(m => m.Groups[1].Value)
                .Select(m => m.Value);
            formattedAttributes = " " + string.Join(" ", sortedAttrs);
        }

        return tag.Replace(attributes, formattedAttributes);
    }

    private bool IsSelfClosingTag(string tagName)
    {
        var selfClosingTags = new HashSet<string>
        {
            "area", "base", "br", "col", "embed", "hr", "img", "input",
            "link", "meta", "param", "source", "track", "wbr"
        };
        return selfClosingTags.Contains(tagName.ToLower());
    }

    private string MinifyHtmlContent(string html)
    {
        var result = html.Trim();

        // 移除注释
        result = Regex.Replace(result, @"<!--.*?-->", "", RegexOptions.Singleline);

        // 移除多余的空白字符
        result = Regex.Replace(result, @"\s+", " ");

        // 移除标签间的空白
        result = Regex.Replace(result, @">\s+<", "><");

        // 移除行首行尾空白
        result = Regex.Replace(result, @"^\s+|\s+$", "", RegexOptions.Multiline);

        return result.Trim();
    }

    private List<string> ValidateHtmlContent(string html)
    {
        var issues = new List<string>();

        try
        {
            // 基本的HTML验证
            
            // 检查DOCTYPE
            if (!html.TrimStart().StartsWith("<!DOCTYPE", StringComparison.OrdinalIgnoreCase))
            {
                issues.Add("缺少DOCTYPE声明");
            }

            // 检查基本结构
            if (!Regex.IsMatch(html, @"<html[^>]*>", RegexOptions.IgnoreCase))
            {
                issues.Add("缺少<html>标签");
            }

            if (!Regex.IsMatch(html, @"<head[^>]*>", RegexOptions.IgnoreCase))
            {
                issues.Add("缺少<head>标签");
            }

            if (!Regex.IsMatch(html, @"<body[^>]*>", RegexOptions.IgnoreCase))
            {
                issues.Add("缺少<body>标签");
            }

            // 检查标签配对
            var tagPattern = @"<(/?)(\w+)([^>]*)>";
            var matches = Regex.Matches(html, tagPattern);
            var tagStack = new Stack<string>();

            foreach (Match match in matches)
            {
                var isClosingTag = !string.IsNullOrEmpty(match.Groups[1].Value);
                var tagName = match.Groups[2].Value.ToLower();

                if (IsSelfClosingTag(tagName) || match.Value.EndsWith("/>"))
                {
                    continue; // 自闭合标签跳过
                }

                if (isClosingTag)
                {
                    if (tagStack.Count == 0)
                    {
                        issues.Add($"多余的闭合标签: </{tagName}>");
                    }
                    else if (tagStack.Peek() != tagName)
                    {
                        issues.Add($"标签不匹配: 期望 </{tagStack.Peek()}>, 实际 </{tagName}>");
                    }
                    else
                    {
                        tagStack.Pop();
                    }
                }
                else
                {
                    tagStack.Push(tagName);
                }
            }

            // 检查未闭合的标签
            while (tagStack.Count > 0)
            {
                issues.Add($"未闭合的标签: <{tagStack.Pop()}>");
            }

            // 检查必需的meta标签
            if (!Regex.IsMatch(html, @"<meta[^>]*charset[^>]*>", RegexOptions.IgnoreCase))
            {
                issues.Add("建议添加字符编码声明 <meta charset=\"UTF-8\">");
            }

            if (!Regex.IsMatch(html, @"<title[^>]*>", RegexOptions.IgnoreCase))
            {
                issues.Add("建议添加<title>标签");
            }
        }
        catch (Exception ex)
        {
            issues.Add($"验证过程出错: {ex.Message}");
        }

        return issues;
    }

    private void SetValidation(string message, bool isValid)
    {
        ValidationMessage = message;
        IsValidHtml = isValid;
    }

    private void ClearValidation()
    {
        ValidationMessage = "";
        IsValidHtml = true;
    }
}