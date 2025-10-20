using System;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.ComponentModel;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.Text.Json;
using System.Collections.Generic;
using System.Linq;
using DevUtilities.Core.ViewModels.Base;
using System.IO;
using System.Text;

namespace DevUtilities.ViewModels;

public partial class JsonFormatterViewModel : BaseFormatterViewModel
{
    [ObservableProperty]
    private bool _sortProperties = false;

    public JsonFormatterViewModel()
    {
        Title = "JSON格式化器";
        Description = "JSON格式化和验证工具";
        Icon = "📋";
        ToolType = Models.ToolType.JsonFormatter;
    }

    protected override async Task<string> FormatContentAsync(string input)
    {
        try
        {
            // 对于大文件，使用流式处理
            var inputSize = Encoding.UTF8.GetByteCount(input);
            if (inputSize > 512 * 1024) // 512KB以上使用流式处理
            {
                return await FormatLargeJsonAsync(input);
            }

            // 小文件使用标准处理
            return await FormatStandardJsonAsync(input);
        }
        catch (Newtonsoft.Json.JsonException ex)
        {
            throw new DevUtilities.Core.Exceptions.JsonFormatterException(ex.Message);
        }
        catch (Exception ex)
        {
            throw new InvalidOperationException($"格式化失败: {ex.Message}", ex);
        }
    }

    /// <summary>
    /// 标准JSON格式化（适用于小文件）
    /// </summary>
    private async Task<string> FormatStandardJsonAsync(string input)
    {
        return await Task.Run(() =>
        {
            var parsedJson = JToken.Parse(input);
            
            // 如果启用属性排序，则对JSON对象进行排序
            if (SortProperties)
            {
                parsedJson = SortJsonProperties(parsedJson);
            }
            
            if (CompactOutput)
            {
                return parsedJson.ToString(Formatting.None);
            }

            var formatted = parsedJson.ToString(Formatting.Indented);
            
            if (IndentSize != 2)
            {
                // 优化缩进处理
                return AdjustIndentation(formatted);
            }

            return formatted;
        });
    }

    /// <summary>
    /// 大文件JSON流式格式化
    /// </summary>
    private async Task<string> FormatLargeJsonAsync(string input)
    {
        return await Task.Run(() =>
        {
            // 对于大文件，如果需要排序，先解析再格式化
            if (SortProperties)
            {
                var parsedJson = JToken.Parse(input);
                parsedJson = SortJsonProperties(parsedJson);
                
                if (CompactOutput)
                {
                    return parsedJson.ToString(Formatting.None);
                }
                
                var formatted = parsedJson.ToString(Formatting.Indented);
                
                if (IndentSize != 2)
                {
                    return AdjustIndentation(formatted);
                }
                
                return formatted;
            }
            
            // 不需要排序时使用流式处理
            using var stringReader = new StringReader(input);
            using var jsonReader = new JsonTextReader(stringReader);
            using var stringWriter = new StringWriter();
            using var jsonWriter = new JsonTextWriter(stringWriter);

            // 配置输出格式
            if (CompactOutput)
            {
                jsonWriter.Formatting = Formatting.None;
            }
            else
            {
                jsonWriter.Formatting = Formatting.Indented;
                jsonWriter.IndentChar = UseTabsForIndent ? '\t' : ' ';
                jsonWriter.Indentation = UseTabsForIndent ? 1 : IndentSize;
            }

            // 流式复制JSON结构
            jsonWriter.WriteToken(jsonReader);
            
            return stringWriter.ToString();
        });
    }

    /// <summary>
    /// 优化的缩进调整算法
    /// </summary>
    private string AdjustIndentation(string formatted)
    {
        var lines = formatted.Split('\n');
        var result = new StringBuilder(formatted.Length);
        
        for (int i = 0; i < lines.Length; i++)
        {
            var line = lines[i];
            var leadingSpaces = 0;
            
            // 快速计算前导空格
            for (int j = 0; j < line.Length && line[j] == ' '; j++)
            {
                leadingSpaces++;
            }
            
            if (leadingSpaces > 0)
            {
                var indentLevel = leadingSpaces / 2;
                var newIndent = UseTabsForIndent ? 
                    new string('\t', indentLevel) : 
                    new string(' ', indentLevel * IndentSize);
                
                result.Append(newIndent);
                result.Append(line.AsSpan(leadingSpaces));
            }
            else
            {
                result.Append(line);
            }
            
            if (i < lines.Length - 1)
            {
                result.AppendLine();
            }
        }
        
        return result.ToString();
    }

    /// <summary>
    /// 递归排序JSON对象的属性
    /// </summary>
    private JToken SortJsonProperties(JToken token)
    {
        switch (token.Type)
        {
            case JTokenType.Object:
                var obj = (JObject)token;
                var sortedObj = new JObject();
                
                // 按属性名排序
                foreach (var property in obj.Properties().OrderBy(p => p.Name, StringComparer.OrdinalIgnoreCase))
                {
                    sortedObj.Add(property.Name, SortJsonProperties(property.Value));
                }
                
                return sortedObj;
                
            case JTokenType.Array:
                var array = (JArray)token;
                var sortedArray = new JArray();
                
                // 递归处理数组中的每个元素
                foreach (var item in array)
                {
                    sortedArray.Add(SortJsonProperties(item));
                }
                
                return sortedArray;
                
            default:
                // 基本类型直接返回
                return token;
        }
    }

    // JSON特定的命令
    [RelayCommand]
    private void SortJsonProperties()
    {
        SortProperties = !SortProperties;
        if (!string.IsNullOrWhiteSpace(InputText))
        {
            FormatCommand.Execute(null);
        }
    }

    [RelayCommand]
    private void MinifyJson()
    {
        CompactOutput = true;
        FormatCommand.Execute(null);
    }

    [RelayCommand]
    private void BeautifyJson()
    {
        CompactOutput = false;
        FormatCommand.Execute(null);
    }

    protected override async Task<ValidationResult> OnValidateAsync(string input)
    {
        if (string.IsNullOrWhiteSpace(input))
        {
            return new ValidationResult(false, "请输入JSON内容");
        }

        try
        {
            // 对于大文件，使用流式验证
            var inputSize = Encoding.UTF8.GetByteCount(input);
            if (inputSize > 1024 * 1024) // 1MB以上使用流式验证
            {
                return await ValidateLargeJsonAsync(input);
            }

            // 标准验证
            return await ValidateStandardJsonAsync(input);
        }
        catch (Newtonsoft.Json.JsonException ex)
        {
            // 提供更详细的错误信息
            var errorMessage = ParseJsonError(ex.Message);
            return new ValidationResult(false, $"JSON格式错误: {errorMessage}");
        }
        catch (Exception ex)
        {
            return new ValidationResult(false, $"验证失败: {ex.Message}");
        }
    }

    /// <summary>
    /// 标准JSON验证
    /// </summary>
    private async Task<ValidationResult> ValidateStandardJsonAsync(string input)
    {
        return await Task.Run(() =>
        {
            // 尝试解析JSON
            using var document = JsonDocument.Parse(input);
            
            // 检查JSON结构
            var root = document.RootElement;
            var elementCount = CountJsonElements(root);
            var depth = GetJsonDepth(root);
            
            var message = $"JSON格式正确 - 包含 {elementCount} 个元素，最大深度 {depth} 层";
            return new ValidationResult(true, message);
        });
    }

    /// <summary>
    /// 大文件JSON流式验证
    /// </summary>
    private async Task<ValidationResult> ValidateLargeJsonAsync(string input)
    {
        return await Task.Run(() =>
        {
            using var stringReader = new StringReader(input);
            using var jsonReader = new JsonTextReader(stringReader);
            
            int elementCount = 0;
            int maxDepth = 0;
            int currentDepth = 0;
            
            try
            {
                while (jsonReader.Read())
                {
                    switch (jsonReader.TokenType)
                    {
                        case JsonToken.StartObject:
                        case JsonToken.StartArray:
                            currentDepth++;
                            maxDepth = Math.Max(maxDepth, currentDepth);
                            break;
                        case JsonToken.EndObject:
                        case JsonToken.EndArray:
                            currentDepth--;
                            break;
                        case JsonToken.PropertyName:
                        case JsonToken.String:
                        case JsonToken.Integer:
                        case JsonToken.Float:
                        case JsonToken.Boolean:
                        case JsonToken.Null:
                            elementCount++;
                            break;
                    }
                }
                
                var sizeText = input.Length > 1024 * 1024 ? 
                    $"{input.Length / (1024.0 * 1024.0):F1}MB" : 
                    $"{input.Length / 1024.0:F0}KB";
                    
                var message = $"JSON格式正确 - 文件大小 {sizeText}，包含 {elementCount} 个元素，最大深度 {maxDepth} 层";
                return new ValidationResult(true, message);
            }
            catch (JsonReaderException ex)
            {
                var errorMessage = ParseJsonError(ex.Message);
                return new ValidationResult(false, $"JSON格式错误: {errorMessage}");
            }
        });
    }

    /// <summary>
    /// 解析JSON错误信息，提供更友好的提示
    /// </summary>
    private string ParseJsonError(string originalError)
    {
        if (originalError.Contains("unexpected character"))
        {
            return "存在意外字符，请检查语法";
        }
        if (originalError.Contains("unterminated string"))
        {
            return "字符串未正确结束，请检查引号";
        }
        if (originalError.Contains("invalid number"))
        {
            return "数字格式不正确";
        }
        if (originalError.Contains("expected"))
        {
            return "缺少必要的符号（如逗号、冒号、括号等）";
        }
        
        return originalError;
    }

    /// <summary>
    /// 计算JSON元素数量（优化版本）
    /// </summary>
    private int CountJsonElements(JsonElement element)
    {
        int count = 1;
        
        switch (element.ValueKind)
        {
            case JsonValueKind.Object:
                foreach (var property in element.EnumerateObject())
                {
                    count += CountJsonElements(property.Value);
                }
                break;
            case JsonValueKind.Array:
                foreach (var item in element.EnumerateArray())
                {
                    count += CountJsonElements(item);
                }
                break;
        }
        
        return count;
    }

    /// <summary>
    /// 计算JSON最大深度（优化版本）
    /// </summary>
    private int GetJsonDepth(JsonElement element, int currentDepth = 1)
    {
        int maxDepth = currentDepth;
        
        switch (element.ValueKind)
        {
            case JsonValueKind.Object:
                foreach (var property in element.EnumerateObject())
                {
                    var depth = GetJsonDepth(property.Value, currentDepth + 1);
                    maxDepth = Math.Max(maxDepth, depth);
                }
                break;
            case JsonValueKind.Array:
                foreach (var item in element.EnumerateArray())
                {
                    var depth = GetJsonDepth(item, currentDepth + 1);
                    maxDepth = Math.Max(maxDepth, depth);
                }
                break;
        }
        
        return maxDepth;
    }

    protected override string GetExampleData()
    {
        return """
        {
          "user": {
            "name": "张三",
            "age": 30,
            "email": "zhangsan@example.com",
            "isActive": true
          },
          "address": {
            "street": "中关村大街1号",
            "city": "北京",
            "zipCode": "100080",
            "country": "中国"
          },
          "skills": ["C#", "JavaScript", "Python", "SQL"],
          "projects": [
            {
              "name": "项目A",
              "status": "completed",
              "startDate": "2024-01-15T09:00:00Z",
              "endDate": "2024-06-30T18:00:00Z"
            },
            {
              "name": "项目B", 
              "status": "in-progress",
              "startDate": "2024-07-01T09:00:00Z",
              "endDate": null
            }
          ],
          "metadata": {
            "createdAt": "2025-01-15T10:30:00Z",
            "updatedAt": "2025-01-15T14:20:00Z",
            "version": "1.2.3"
          }
        }
        """;
    }
}