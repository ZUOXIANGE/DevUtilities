using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using DevUtilities.Core.ViewModels.Base;

namespace DevUtilities.ViewModels;

public partial class HtmlFormatterViewModel : BaseFormatterViewModel
{
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
        Title = "HTML格式化器";
        Description = "HTML代码格式化和美化";
        Icon = "🌐";
        ToolType = Models.ToolType.HtmlFormatter;
    }

    protected override Task<string> FormatContentAsync(string input)
    {
        try
        {
            if (CompactOutput)
            {
                return Task.FromResult(MinifyHtmlContent(input));
            }
            else
            {
                return Task.FromResult(FormatHtmlContent(input));
            }
        }
        catch (Exception ex)
        {
            throw new DevUtilities.Core.Exceptions.HtmlFormatterException(ex.Message);
        }
    }

    // HTML特定的命令
    [RelayCommand]
    private void MinifyHtml()
    {
        CompactOutput = true;
        FormatCommand.Execute(null);
    }

    [RelayCommand]
    private void BeautifyHtml()
    {
        CompactOutput = false;
        FormatCommand.Execute(null);
    }

    private string FormatHtmlContent(string html)
    {
        if (string.IsNullOrWhiteSpace(html))
            return string.Empty;

        var result = html.Trim();

        // 移除注释（如果需要）
        if (RemoveComments)
        {
            result = Regex.Replace(result, @"<!--.*?-->", "", RegexOptions.Singleline);
        }

        // 基本格式化
        result = FormatHtmlStructure(result);

        return result;
    }

    private string FormatHtmlStructure(string html)
    {
        var lines = new List<string>();
        var currentIndent = 0;
        var indentChar = UseTabsForIndent ? "\t" : " ";
        var indentUnit = UseTabsForIndent ? 1 : IndentSize;

        // 简单的HTML标签匹配和格式化
        var tagPattern = @"<(/?)(\w+)([^>]*)>";
        var matches = Regex.Matches(html, tagPattern);

        var lastIndex = 0;
        foreach (Match match in matches)
        {
            // 添加标签前的文本内容
            var textBefore = html.Substring(lastIndex, match.Index - lastIndex).Trim();
            if (!string.IsNullOrEmpty(textBefore) && !PreserveWhitespace)
            {
                lines.Add(new string(indentChar[0], currentIndent * indentUnit) + textBefore);
            }

            var isClosingTag = !string.IsNullOrEmpty(match.Groups[1].Value);
            var tagName = match.Groups[2].Value.ToLower();
            var attributes = match.Groups[3].Value;

            // 调整缩进
            if (isClosingTag)
            {
                currentIndent = Math.Max(0, currentIndent - 1);
            }

            // 格式化标签
            var formattedTag = FormatTag(match.Value, attributes);
            lines.Add(new string(indentChar[0], currentIndent * indentUnit) + formattedTag);

            // 自闭合标签不需要调整缩进
            var isSelfClosing = attributes.EndsWith("/") || 
                               new[] { "br", "hr", "img", "input", "meta", "link", "area", "base", "col", "embed", "source", "track", "wbr" }
                               .Contains(tagName);

            if (!isClosingTag && !isSelfClosing)
            {
                currentIndent++;
            }

            lastIndex = match.Index + match.Length;
        }

        // 添加剩余的文本
        var remainingText = html.Substring(lastIndex).Trim();
        if (!string.IsNullOrEmpty(remainingText))
        {
            lines.Add(new string(indentChar[0], currentIndent * indentUnit) + remainingText);
        }

        return string.Join("\n", lines);
    }

    private string FormatTag(string tag, string attributes)
    {
        if (!FormatAttributes || string.IsNullOrWhiteSpace(attributes))
            return tag;

        // 简单的属性格式化
        if (SortAttributes)
        {
            var attrPattern = @"(\w+)=(""[^""]*""|'[^']*'|\S+)";
            var attrMatches = Regex.Matches(attributes, attrPattern);
            var sortedAttrs = attrMatches.Cast<Match>()
                .OrderBy(m => m.Groups[1].Value)
                .Select(m => m.Value);
            
            attributes = " " + string.Join(" ", sortedAttrs);
        }

        return tag;
    }

    private string MinifyHtmlContent(string html)
    {
        if (string.IsNullOrWhiteSpace(html))
            return string.Empty;

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

    protected override Task<ValidationResult> OnValidateAsync(string input)
    {
        if (string.IsNullOrWhiteSpace(input))
        {
            return Task.FromResult(new ValidationResult(false, "请输入HTML内容"));
        }

        try
        {
            var html = input.Trim();
            var issues = new List<string>();
            var warnings = new List<string>();

            // 基本HTML结构检查
            ValidateHtmlStructure(html, issues);
            
            // 分析HTML结构
            var structureInfo = AnalyzeHtmlStructure(html);
            
            // 检查最佳实践
            CheckHtmlBestPractices(html, warnings);

            var message = BuildValidationMessage(structureInfo, issues, warnings);
            var isValid = issues.Count == 0;

            return Task.FromResult(new ValidationResult(isValid, message));
        }
        catch (Exception ex)
        {
            return Task.FromResult(new ValidationResult(false, $"验证失败: {ex.Message}"));
        }
    }

    /// <summary>
    /// 验证HTML结构
    /// </summary>
    private void ValidateHtmlStructure(string html, List<string> issues)
    {
        // 检查标签匹配
        var tagStack = new Stack<string>();
        var tagPattern = @"<(/?)(\w+)([^>]*)>";
        var matches = Regex.Matches(html, tagPattern);

        foreach (Match match in matches)
        {
            var isClosingTag = !string.IsNullOrEmpty(match.Groups[1].Value);
            var tagName = match.Groups[2].Value.ToLower();
            var attributes = match.Groups[3].Value;

            // 自闭合标签
            var isSelfClosing = attributes.EndsWith("/") || 
                               new[] { "br", "hr", "img", "input", "meta", "link", "area", "base", "col", "embed", "source", "track", "wbr" }
                               .Contains(tagName);

            if (isClosingTag)
            {
                if (tagStack.Count == 0)
                {
                    issues.Add($"多余的结束标签: </{tagName}>");
                }
                else
                {
                    var expectedTag = tagStack.Pop();
                    if (expectedTag != tagName)
                    {
                        issues.Add($"标签不匹配: 期望 </{expectedTag}>，实际 </{tagName}>");
                    }
                }
            }
            else if (!isSelfClosing)
            {
                tagStack.Push(tagName);
            }
        }

        // 检查未闭合的标签
        while (tagStack.Count > 0)
        {
            var unclosedTag = tagStack.Pop();
            issues.Add($"未闭合的标签: <{unclosedTag}>");
        }

        // 检查基本HTML结构
        if (!html.Contains("<!DOCTYPE", StringComparison.OrdinalIgnoreCase))
        {
            issues.Add("缺少DOCTYPE声明");
        }
    }

    /// <summary>
    /// 分析HTML结构
    /// </summary>
    private HtmlStructureInfo AnalyzeHtmlStructure(string html)
    {
        var info = new HtmlStructureInfo();
        
        // 检查基本结构
        info.HasDoctype = html.Contains("<!DOCTYPE", StringComparison.OrdinalIgnoreCase);
        info.HasHtml = Regex.IsMatch(html, @"<html\b", RegexOptions.IgnoreCase);
        info.HasHead = Regex.IsMatch(html, @"<head\b", RegexOptions.IgnoreCase);
        info.HasBody = Regex.IsMatch(html, @"<body\b", RegexOptions.IgnoreCase);
        info.HasTitle = Regex.IsMatch(html, @"<title\b", RegexOptions.IgnoreCase);
        
        // 统计标签数量
        var tagPattern = @"<(\w+)";
        var tagMatches = Regex.Matches(html, tagPattern, RegexOptions.IgnoreCase);
        info.TagCount = tagMatches.Count;
        
        // 统计不同类型的标签
        info.HasImages = Regex.IsMatch(html, @"<img\b", RegexOptions.IgnoreCase);
        info.HasLinks = Regex.IsMatch(html, @"<a\b", RegexOptions.IgnoreCase);
        info.HasForms = Regex.IsMatch(html, @"<form\b", RegexOptions.IgnoreCase);
        info.HasTables = Regex.IsMatch(html, @"<table\b", RegexOptions.IgnoreCase);
        info.HasScripts = Regex.IsMatch(html, @"<script\b", RegexOptions.IgnoreCase);
        info.HasStyles = Regex.IsMatch(html, @"<style\b", RegexOptions.IgnoreCase);
        
        return info;
    }

    /// <summary>
    /// 检查HTML最佳实践
    /// </summary>
    private void CheckHtmlBestPractices(string html, List<string> warnings)
    {
        // 检查语言属性
        if (!Regex.IsMatch(html, @"<html[^>]*\slang=", RegexOptions.IgnoreCase))
        {
            warnings.Add("建议为html标签添加lang属性");
        }

        // 检查字符编码
        if (!Regex.IsMatch(html, @"<meta[^>]*charset=", RegexOptions.IgnoreCase))
        {
            warnings.Add("建议添加字符编码声明");
        }

        // 检查viewport设置
        if (!Regex.IsMatch(html, @"<meta[^>]*name=[""']viewport[""']", RegexOptions.IgnoreCase))
        {
            warnings.Add("建议添加viewport meta标签以支持响应式设计");
        }

        // 检查图片alt属性
        var imgMatches = Regex.Matches(html, @"<img[^>]*>", RegexOptions.IgnoreCase);
        foreach (Match match in imgMatches)
        {
            if (!match.Value.Contains("alt=", StringComparison.OrdinalIgnoreCase))
            {
                warnings.Add("建议为所有图片添加alt属性");
                break;
            }
        }

        // 检查内联样式
        if (Regex.IsMatch(html, @"\sstyle=", RegexOptions.IgnoreCase))
        {
            warnings.Add("建议避免使用内联样式，使用外部CSS文件");
        }
    }

    /// <summary>
    /// 构建验证消息
    /// </summary>
    private string BuildValidationMessage(HtmlStructureInfo info, List<string> issues, List<string> warnings)
    {
        if (issues.Count > 0)
        {
            return $"HTML结构错误: {string.Join(", ", issues)}";
        }

        var messageParts = new List<string> { "HTML结构正确" };
        
        var structureParts = new List<string>();
        if (info.TagCount > 0) structureParts.Add($"{info.TagCount}个标签");
        if (info.HasDoctype) structureParts.Add("包含DOCTYPE");
        if (info.HasTitle) structureParts.Add("包含标题");
        if (info.HasImages) structureParts.Add("包含图片");
        if (info.HasLinks) structureParts.Add("包含链接");
        if (info.HasForms) structureParts.Add("包含表单");
        if (info.HasTables) structureParts.Add("包含表格");
        if (info.HasScripts) structureParts.Add("包含脚本");
        if (info.HasStyles) structureParts.Add("包含样式");
        
        if (structureParts.Count > 0)
        {
            messageParts.Add($"({string.Join(", ", structureParts)})");
        }

        if (warnings.Count > 0)
        {
            messageParts.Add($"建议: {string.Join("; ", warnings)}");
        }

        return string.Join(" ", messageParts);
    }

    /// <summary>
    /// HTML结构信息
    /// </summary>
    private class HtmlStructureInfo
    {
        public bool HasDoctype { get; set; }
        public bool HasHtml { get; set; }
        public bool HasHead { get; set; }
        public bool HasBody { get; set; }
        public bool HasTitle { get; set; }
        public int TagCount { get; set; }
        public bool HasImages { get; set; }
        public bool HasLinks { get; set; }
        public bool HasForms { get; set; }
        public bool HasTables { get; set; }
        public bool HasScripts { get; set; }
        public bool HasStyles { get; set; }
    }

    protected override string GetExampleData()
    {
        return """
        <!DOCTYPE html><html lang="zh-CN"><head><meta charset="UTF-8"><meta name="viewport" content="width=device-width, initial-scale=1.0"><title>示例页面</title><style>body{font-family:Arial,sans-serif;margin:0;padding:20px;background-color:#f5f5f5;}.container{max-width:800px;margin:0 auto;background:white;padding:20px;border-radius:8px;box-shadow:0 2px 10px rgba(0,0,0,0.1);}</style></head><body><div class="container"><header><h1>欢迎来到我的网站</h1><nav><ul><li><a href="#home">首页</a></li><li><a href="#about">关于</a></li><li><a href="#contact">联系</a></li></ul></nav></header><main><section id="home"><h2>主页内容</h2><p>这是一个示例HTML页面，用于演示HTML格式化工具的功能。</p><div class="features"><h3>功能特点</h3><ul><li>代码格式化</li><li>语法高亮</li><li>错误检测</li></ul></div></section></main><footer><p>&copy; 2025 示例网站. 保留所有权利.</p></footer></div></body></html>
        """;
    }
}