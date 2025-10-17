using System;
using System.Text.Json;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace DevUtilities.ViewModels;

public partial class JsonFormatterViewModel : ObservableObject
{
    [ObservableProperty]
    private string inputJson = "";

    [ObservableProperty]
    private string outputJson = "";

    [ObservableProperty]
    private string validationMessage = "";

    [ObservableProperty]
    private bool isValidJson = false;

    [ObservableProperty]
    private int indentSize = 2;

    [RelayCommand]
    private void FormatJson()
    {
        try
        {
            if (string.IsNullOrWhiteSpace(InputJson))
            {
                OutputJson = "";
                ValidationMessage = "";
                IsValidJson = false;
                return;
            }

            // 尝试解析JSON
            var parsedJson = JToken.Parse(InputJson);
            
            // 格式化JSON
            var formatted = parsedJson.ToString(IndentSize == 2 ? Formatting.Indented : Formatting.None);
            
            if (IndentSize != 2 && IndentSize > 0)
            {
                // 自定义缩进
                var settings = new JsonSerializerSettings
                {
                    Formatting = Formatting.Indented
                };
                formatted = JsonConvert.SerializeObject(parsedJson, settings);
                
                // 替换默认的2空格缩进为自定义缩进
                var lines = formatted.Split('\n');
                for (int i = 0; i < lines.Length; i++)
                {
                    var line = lines[i];
                    var leadingSpaces = 0;
                    foreach (char c in line)
                    {
                        if (c == ' ') leadingSpaces++;
                        else break;
                    }
                    
                    if (leadingSpaces > 0)
                    {
                        var indentLevel = leadingSpaces / 2;
                        var newIndent = new string(' ', indentLevel * IndentSize);
                        lines[i] = newIndent + line.Substring(leadingSpaces);
                    }
                }
                formatted = string.Join('\n', lines);
            }

            OutputJson = formatted;
            ValidationMessage = "✅ JSON格式正确";
            IsValidJson = true;
        }
        catch (JsonReaderException ex)
        {
            OutputJson = "";
            ValidationMessage = $"❌ JSON格式错误: {ex.Message}";
            IsValidJson = false;
        }
        catch (Exception ex)
        {
            OutputJson = "";
            ValidationMessage = $"❌ 处理错误: {ex.Message}";
            IsValidJson = false;
        }
    }

    [RelayCommand]
    private void MinifyJson()
    {
        try
        {
            if (string.IsNullOrWhiteSpace(InputJson))
            {
                OutputJson = "";
                ValidationMessage = "";
                IsValidJson = false;
                return;
            }

            var parsedJson = JToken.Parse(InputJson);
            var minified = parsedJson.ToString(Formatting.None);

            OutputJson = minified;
            ValidationMessage = "✅ JSON已压缩";
            IsValidJson = true;
        }
        catch (JsonReaderException ex)
        {
            OutputJson = "";
            ValidationMessage = $"❌ JSON格式错误: {ex.Message}";
            IsValidJson = false;
        }
        catch (Exception ex)
        {
            OutputJson = "";
            ValidationMessage = $"❌ 处理错误: {ex.Message}";
            IsValidJson = false;
        }
    }

    [RelayCommand]
    private void ValidateJson()
    {
        try
        {
            if (string.IsNullOrWhiteSpace(InputJson))
            {
                ValidationMessage = "";
                IsValidJson = false;
                return;
            }

            JToken.Parse(InputJson);
            ValidationMessage = "✅ JSON格式正确";
            IsValidJson = true;
        }
        catch (JsonReaderException ex)
        {
            ValidationMessage = $"❌ JSON格式错误: {ex.Message}";
            IsValidJson = false;
        }
        catch (Exception ex)
        {
            ValidationMessage = $"❌ 验证错误: {ex.Message}";
            IsValidJson = false;
        }
    }

    [RelayCommand]
    private void ClearAll()
    {
        InputJson = "";
        OutputJson = "";
        ValidationMessage = "";
        IsValidJson = false;
    }

    [RelayCommand]
    private void CopyOutput()
    {
        if (!string.IsNullOrWhiteSpace(OutputJson))
        {
            // 这里需要实现剪贴板功能
            // 在实际应用中需要使用Avalonia的剪贴板API
        }
    }

    partial void OnInputJsonChanged(string value)
    {
        ValidateJson();
    }
}