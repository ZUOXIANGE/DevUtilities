using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Text.RegularExpressions;
using System.Web;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

namespace DevUtilities.ViewModels;

public partial class StringEscapeViewModel : ObservableObject
{
    [ObservableProperty]
    private string inputText = "";

    [ObservableProperty]
    private string outputText = "";

    [ObservableProperty]
    private string selectedEscapeType = "JSON转义";

    [ObservableProperty]
    private bool isEscapeMode = true;

    public List<string> EscapeTypes { get; } = new()
    {
        "JSON转义",
        "JavaScript转义",
        "C#字符串转义",
        "Java字符串转义",
        "Python字符串转义",
        "XML/HTML转义",
        "URL转义",
        "SQL转义",
        "正则表达式转义",
        "CSV转义",
        "Unicode转义"
    };

    public StringEscapeViewModel()
    {
        // 初始化示例
        InputText = "Hello \"World\"\nNew Line\tTab";
        ProcessString();
    }

    partial void OnInputTextChanged(string value)
    {
        ProcessString();
    }

    partial void OnSelectedEscapeTypeChanged(string value)
    {
        ProcessString();
    }

    partial void OnIsEscapeModeChanged(bool value)
    {
        ProcessString();
    }

    [RelayCommand]
    private void ProcessString()
    {
        if (IsEscapeMode)
        {
            EscapeString();
        }
        else
        {
            UnescapeString();
        }
    }

    [RelayCommand]
    private void EscapeString()
    {
        try
        {
            if (string.IsNullOrEmpty(InputText))
            {
                OutputText = "";
                return;
            }

            OutputText = SelectedEscapeType switch
            {
                "JSON转义" => EscapeJson(InputText),
                "JavaScript转义" => EscapeJavaScript(InputText),
                "C#字符串转义" => EscapeCSharp(InputText),
                "Java字符串转义" => EscapeJava(InputText),
                "Python字符串转义" => EscapePython(InputText),
                "XML/HTML转义" => EscapeXmlHtml(InputText),
                "URL转义" => EscapeUrl(InputText),
                "SQL转义" => EscapeSql(InputText),
                "正则表达式转义" => EscapeRegex(InputText),
                "CSV转义" => EscapeCsv(InputText),
                "Unicode转义" => EscapeUnicode(InputText),
                _ => InputText
            };
        }
        catch (Exception ex)
        {
            OutputText = $"转义错误: {ex.Message}";
        }
    }

    [RelayCommand]
    private void UnescapeString()
    {
        try
        {
            if (string.IsNullOrEmpty(InputText))
            {
                OutputText = "";
                return;
            }

            OutputText = SelectedEscapeType switch
            {
                "JSON转义" => UnescapeJson(InputText),
                "JavaScript转义" => UnescapeJavaScript(InputText),
                "C#字符串转义" => UnescapeCSharp(InputText),
                "Java字符串转义" => UnescapeJava(InputText),
                "Python字符串转义" => UnescapePython(InputText),
                "XML/HTML转义" => UnescapeXmlHtml(InputText),
                "URL转义" => UnescapeUrl(InputText),
                "SQL转义" => UnescapeSql(InputText),
                "正则表达式转义" => UnescapeRegex(InputText),
                "CSV转义" => UnescapeCsv(InputText),
                "Unicode转义" => UnescapeUnicode(InputText),
                _ => InputText
            };
        }
        catch (Exception ex)
        {
            OutputText = $"反转义错误: {ex.Message}";
        }
    }

    [RelayCommand]
    private void SwapInputOutput()
    {
        var temp = InputText;
        InputText = OutputText;
        OutputText = temp;
    }

    [RelayCommand]
    private void ClearAll()
    {
        InputText = "";
        OutputText = "";
    }

    [RelayCommand]
    private void UseExample(string example)
    {
        InputText = example;
        ProcessString();
    }

    // JSON转义
    private string EscapeJson(string input)
    {
        return JsonSerializer.Serialize(input);
    }

    private string UnescapeJson(string input)
    {
        // 如果输入已经是带引号的JSON字符串，直接反序列化
        if (input.StartsWith("\"") && input.EndsWith("\""))
        {
            return JsonSerializer.Deserialize<string>(input) ?? "";
        }
        // 否则尝试作为JSON字符串处理
        return JsonSerializer.Deserialize<string>($"\"{input}\"") ?? "";
    }

    // JavaScript转义
    private string EscapeJavaScript(string input)
    {
        return input
            .Replace("\\", "\\\\")
            .Replace("\"", "\\\"")
            .Replace("'", "\\'")
            .Replace("\n", "\\n")
            .Replace("\r", "\\r")
            .Replace("\t", "\\t")
            .Replace("\b", "\\b")
            .Replace("\f", "\\f")
            .Replace("/", "\\/");
    }

    private string UnescapeJavaScript(string input)
    {
        return input
            .Replace("\\/", "/")
            .Replace("\\f", "\f")
            .Replace("\\b", "\b")
            .Replace("\\t", "\t")
            .Replace("\\r", "\r")
            .Replace("\\n", "\n")
            .Replace("\\'", "'")
            .Replace("\\\"", "\"")
            .Replace("\\\\", "\\");
    }

    // C#字符串转义
    private string EscapeCSharp(string input)
    {
        return input
            .Replace("\\", "\\\\")
            .Replace("\"", "\\\"")
            .Replace("\n", "\\n")
            .Replace("\r", "\\r")
            .Replace("\t", "\\t")
            .Replace("\0", "\\0")
            .Replace("\a", "\\a")
            .Replace("\b", "\\b")
            .Replace("\f", "\\f")
            .Replace("\v", "\\v");
    }

    private string UnescapeCSharp(string input)
    {
        return input
            .Replace("\\v", "\v")
            .Replace("\\f", "\f")
            .Replace("\\b", "\b")
            .Replace("\\a", "\a")
            .Replace("\\0", "\0")
            .Replace("\\t", "\t")
            .Replace("\\r", "\r")
            .Replace("\\n", "\n")
            .Replace("\\\"", "\"")
            .Replace("\\\\", "\\");
    }

    // Java字符串转义
    private string EscapeJava(string input)
    {
        return EscapeCSharp(input); // Java和C#的字符串转义规则基本相同
    }

    private string UnescapeJava(string input)
    {
        return UnescapeCSharp(input);
    }

    // Python字符串转义
    private string EscapePython(string input)
    {
        return input
            .Replace("\\", "\\\\")
            .Replace("\"", "\\\"")
            .Replace("'", "\\'")
            .Replace("\n", "\\n")
            .Replace("\r", "\\r")
            .Replace("\t", "\\t")
            .Replace("\0", "\\0")
            .Replace("\a", "\\a")
            .Replace("\b", "\\b")
            .Replace("\f", "\\f")
            .Replace("\v", "\\v");
    }

    private string UnescapePython(string input)
    {
        return input
            .Replace("\\v", "\v")
            .Replace("\\f", "\f")
            .Replace("\\b", "\b")
            .Replace("\\a", "\a")
            .Replace("\\0", "\0")
            .Replace("\\t", "\t")
            .Replace("\\r", "\r")
            .Replace("\\n", "\n")
            .Replace("\\'", "'")
            .Replace("\\\"", "\"")
            .Replace("\\\\", "\\");
    }

    // XML/HTML转义
    private string EscapeXmlHtml(string input)
    {
        return HttpUtility.HtmlEncode(input);
    }

    private string UnescapeXmlHtml(string input)
    {
        return HttpUtility.HtmlDecode(input);
    }

    // URL转义
    private string EscapeUrl(string input)
    {
        return Uri.EscapeDataString(input);
    }

    private string UnescapeUrl(string input)
    {
        return Uri.UnescapeDataString(input);
    }

    // SQL转义
    private string EscapeSql(string input)
    {
        return input.Replace("'", "''");
    }

    private string UnescapeSql(string input)
    {
        return input.Replace("''", "'");
    }

    // 正则表达式转义
    private string EscapeRegex(string input)
    {
        return Regex.Escape(input);
    }

    private string UnescapeRegex(string input)
    {
        // 正则表达式反转义比较复杂，这里提供基本实现
        return input
            .Replace("\\.", ".")
            .Replace("\\^", "^")
            .Replace("\\$", "$")
            .Replace("\\*", "*")
            .Replace("\\+", "+")
            .Replace("\\?", "?")
            .Replace("\\(", "(")
            .Replace("\\)", ")")
            .Replace("\\[", "[")
            .Replace("\\]", "]")
            .Replace("\\{", "{")
            .Replace("\\}", "}")
            .Replace("\\|", "|")
            .Replace("\\\\", "\\");
    }

    // CSV转义
    private string EscapeCsv(string input)
    {
        if (input.Contains(",") || input.Contains("\"") || input.Contains("\n") || input.Contains("\r"))
        {
            return "\"" + input.Replace("\"", "\"\"") + "\"";
        }
        return input;
    }

    private string UnescapeCsv(string input)
    {
        if (input.StartsWith("\"") && input.EndsWith("\""))
        {
            return input.Substring(1, input.Length - 2).Replace("\"\"", "\"");
        }
        return input;
    }

    // Unicode转义
    private string EscapeUnicode(string input)
    {
        var sb = new StringBuilder();
        foreach (char c in input)
        {
            if (c > 127)
            {
                sb.Append($"\\u{(int)c:x4}");
            }
            else
            {
                sb.Append(c);
            }
        }
        return sb.ToString();
    }

    private string UnescapeUnicode(string input)
    {
        return Regex.Replace(input, @"\\u([0-9a-fA-F]{4})", 
            match => ((char)Convert.ToInt32(match.Groups[1].Value, 16)).ToString());
    }
}