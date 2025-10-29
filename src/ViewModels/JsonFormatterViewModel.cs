using System;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.ComponentModel;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.Text.Json;
using System.Linq;
using DevUtilities.Core.ViewModels.Base;
using System.IO;
using System.Text;
using Serilog;

namespace DevUtilities.ViewModels;

public partial class JsonFormatterViewModel : BaseFormatterViewModel
{
    private static readonly ILogger Logger = Log.ForContext<JsonFormatterViewModel>();

    [ObservableProperty]
    private bool _sortProperties = false;

    public JsonFormatterViewModel()
    {
        Logger.Debug("JsonFormatterViewModel: 开始初始化");
        try
        {
            Title = "JSON格式化器";
            Description = "JSON格式化和验证工具";
            Icon = "📋";
            ToolType = Models.ToolType.JsonFormatter;
            Logger.Information("JsonFormatterViewModel: 初始化完成 - Title: {Title}, Description: {Description}", Title, Description);
        }
        catch (Exception ex)
        {
            Logger.Error(ex, "JsonFormatterViewModel: 初始化失败");
            throw;
        }
    }

    protected override async Task<string> FormatContentAsync(string input)
    {
        Logger.Debug("JsonFormatterViewModel: 开始格式化JSON内容 - 输入长度: {InputLength}", input?.Length ?? 0);
        try
        {
            // 对于大文件，使用流式处理
            var inputSize = Encoding.UTF8.GetByteCount(input ?? string.Empty);
            Logger.Debug("JsonFormatterViewModel: 输入大小: {InputSize} bytes", inputSize);
            
            if (inputSize > 512 * 1024) // 512KB以上使用流式处理
            {
                Logger.Information("JsonFormatterViewModel: 使用流式处理格式化大文件 - 大小: {InputSize} bytes", inputSize);
                return await FormatLargeJsonAsync(input ?? string.Empty);
            }

            // 小文件使用标准处理
            Logger.Debug("JsonFormatterViewModel: 使用标准处理格式化小文件");
            return await FormatStandardJsonAsync(input ?? string.Empty);
        }
        catch (Newtonsoft.Json.JsonException ex)
        {
            Logger.Error(ex, "JsonFormatterViewModel: JSON格式化失败 - JSON解析错误");
            throw new DevUtilities.Core.Exceptions.JsonFormatterException(ex.Message);
        }
        catch (Exception ex)
        {
            Logger.Error(ex, "JsonFormatterViewModel: JSON格式化失败 - 未知错误");
            throw new InvalidOperationException($"格式化失败: {ex.Message}", ex);
        }
    }

    /// <summary>
    /// 标准JSON格式化（适用于小文件）
    /// </summary>
    private async Task<string> FormatStandardJsonAsync(string input)
    {
        Logger.Debug("JsonFormatterViewModel: 开始标准JSON格式化 - SortProperties: {SortProperties}, CompactOutput: {CompactOutput}", SortProperties, CompactOutput);
        return await Task.Run(() =>
        {
            try
            {
                var parsedJson = JToken.Parse(input);
                Logger.Debug("JsonFormatterViewModel: JSON解析成功 - Token类型: {TokenType}", parsedJson.Type);
                
                // 如果启用属性排序，则对JSON对象进行排序
                if (SortProperties)
                {
                    Logger.Debug("JsonFormatterViewModel: 开始排序JSON属性");
                    parsedJson = SortJsonProperties(parsedJson);
                    Logger.Debug("JsonFormatterViewModel: JSON属性排序完成");
                }
                
                if (CompactOutput)
                {
                    Logger.Debug("JsonFormatterViewModel: 生成紧凑格式输出");
                    return parsedJson.ToString(Formatting.None);
                }

                var formatted = parsedJson.ToString(Formatting.Indented);
                Logger.Debug("JsonFormatterViewModel: 生成缩进格式输出 - IndentSize: {IndentSize}", IndentSize);
                
                if (IndentSize != 2)
                {
                    Logger.Debug("JsonFormatterViewModel: 调整缩进大小");
                    // 优化缩进处理
                    return AdjustIndentation(formatted);
                }

                Logger.Information("JsonFormatterViewModel: 标准JSON格式化完成 - 输出长度: {OutputLength}", formatted.Length);
                return formatted;
            }
            catch (Exception ex)
            {
                Logger.Error(ex, "JsonFormatterViewModel: 标准JSON格式化失败");
                throw;
            }
        });
    }

    /// <summary>
    /// 大文件JSON流式格式化
    /// </summary>
    private async Task<string> FormatLargeJsonAsync(string input)
    {
        Logger.Debug("JsonFormatterViewModel: 开始大文件JSON流式格式化");
        return await Task.Run(() =>
        {
            try
            {
                // 对于大文件，如果需要排序，先解析再格式化
                if (SortProperties)
                {
                    Logger.Debug("JsonFormatterViewModel: 大文件需要排序，使用解析方式");
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
                
                Logger.Debug("JsonFormatterViewModel: 使用流式处理大文件 - UseTabsForIndent: {UseTabsForIndent}, IndentSize: {IndentSize}", UseTabsForIndent, IndentSize);
                
                // 不需要排序时使用流式处理
                using var stringReader = new StringReader(input);
                using var jsonReader = new JsonTextReader(stringReader);
                using var stringWriter = new StringWriter();
                using var jsonWriter = new JsonTextWriter(stringWriter);

                // 配置输出格式
                if (CompactOutput)
                {
                    jsonWriter.Formatting = Formatting.None;
                    Logger.Debug("JsonFormatterViewModel: 配置为紧凑输出格式");
                }
                else
                {
                    jsonWriter.Formatting = Formatting.Indented;
                    jsonWriter.IndentChar = UseTabsForIndent ? '\t' : ' ';
                    jsonWriter.Indentation = UseTabsForIndent ? 1 : IndentSize;
                    Logger.Debug("JsonFormatterViewModel: 配置为缩进输出格式 - IndentChar: {IndentChar}, Indentation: {Indentation}", 
                        jsonWriter.IndentChar, jsonWriter.Indentation);
                }

                // 流式复制JSON结构
                jsonWriter.WriteToken(jsonReader);
                
                var result = stringWriter.ToString();
                Logger.Information("JsonFormatterViewModel: 大文件JSON流式格式化完成 - 输出长度: {OutputLength}", result.Length);
                return result;
            }
            catch (Exception ex)
            {
                Logger.Error(ex, "JsonFormatterViewModel: 大文件JSON流式格式化失败");
                throw;
            }
        });
    }

    /// <summary>
    /// 优化的缩进调整算法
    /// </summary>
    private string AdjustIndentation(string formatted)
    {
        Logger.Debug("JsonFormatterViewModel: 开始调整缩进 - UseTabsForIndent: {UseTabsForIndent}, IndentSize: {IndentSize}", UseTabsForIndent, IndentSize);
        try
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
            
            Logger.Debug("JsonFormatterViewModel: 缩进调整完成 - 处理行数: {LineCount}", lines.Length);
            return result.ToString();
        }
        catch (Exception ex)
        {
            Logger.Error(ex, "JsonFormatterViewModel: 缩进调整失败");
            throw;
        }
    }

    /// <summary>
    /// 递归排序JSON对象的属性
    /// </summary>
    private JToken SortJsonProperties(JToken token)
    {
        Logger.Debug("JsonFormatterViewModel: 开始排序JSON属性 - Token类型: {TokenType}", token.Type);
        try
        {
            switch (token.Type)
            {
                case JTokenType.Object:
                    var obj = (JObject)token;
                    var sortedObj = new JObject();
                    var propertyCount = obj.Properties().Count();
                    Logger.Debug("JsonFormatterViewModel: 排序JSON对象 - 属性数量: {PropertyCount}", propertyCount);
                    
                    // 按属性名排序
                    foreach (var property in obj.Properties().OrderBy(p => p.Name, StringComparer.OrdinalIgnoreCase))
                    {
                        sortedObj.Add(property.Name, SortJsonProperties(property.Value));
                    }
                    
                    return sortedObj;
                    
                case JTokenType.Array:
                    var array = (JArray)token;
                    var sortedArray = new JArray();
                    Logger.Debug("JsonFormatterViewModel: 排序JSON数组 - 元素数量: {ElementCount}", array.Count);
                    
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
        catch (Exception ex)
        {
            Logger.Error(ex, "JsonFormatterViewModel: JSON属性排序失败 - Token类型: {TokenType}", token.Type);
            throw;
        }
    }

    // JSON特定的命令
    [RelayCommand]
    private void SortJsonProperties()
    {
        Logger.Debug("JsonFormatterViewModel: 切换JSON属性排序 - 当前状态: {CurrentState}", SortProperties);
        try
        {
            SortProperties = !SortProperties;
            Logger.Information("JsonFormatterViewModel: JSON属性排序状态已切换 - 新状态: {NewState}", SortProperties);
            
            if (!string.IsNullOrWhiteSpace(InputText))
            {
                Logger.Debug("JsonFormatterViewModel: 重新格式化JSON（属性排序切换）");
                FormatCommand.Execute(null);
            }
        }
        catch (Exception ex)
        {
            Logger.Error(ex, "JsonFormatterViewModel: 切换JSON属性排序失败");
        }
    }

    [RelayCommand]
    private void MinifyJson()
    {
        Logger.Debug("JsonFormatterViewModel: 执行JSON压缩命令");
        try
        {
            CompactOutput = true;
            Logger.Information("JsonFormatterViewModel: 已设置为紧凑输出模式");
            FormatCommand.Execute(null);
        }
        catch (Exception ex)
        {
            Logger.Error(ex, "JsonFormatterViewModel: JSON压缩命令执行失败");
        }
    }

    [RelayCommand]
    private void BeautifyJson()
    {
        Logger.Debug("JsonFormatterViewModel: 执行JSON美化命令");
        try
        {
            CompactOutput = false;
            Logger.Information("JsonFormatterViewModel: 已设置为格式化输出模式");
            FormatCommand.Execute(null);
        }
        catch (Exception ex)
        {
            Logger.Error(ex, "JsonFormatterViewModel: JSON美化命令执行失败");
        }
    }

    protected override async Task<ValidationResult> OnValidateAsync(string input)
    {
        Logger.Debug("JsonFormatterViewModel: 开始验证JSON - 输入长度: {InputLength}", input?.Length ?? 0);
        
        if (string.IsNullOrWhiteSpace(input))
        {
            Logger.Warning("JsonFormatterViewModel: JSON验证失败 - 输入为空");
            return new ValidationResult(false, "请输入JSON内容");
        }

        try
        {
            // 对于大文件，使用流式验证
            var inputSize = Encoding.UTF8.GetByteCount(input);
            Logger.Debug("JsonFormatterViewModel: JSON验证 - 输入大小: {InputSize} bytes", inputSize);
            
            if (inputSize > 1024 * 1024) // 1MB以上使用流式验证
            {
                Logger.Information("JsonFormatterViewModel: 使用流式验证大文件JSON");
                return await ValidateLargeJsonAsync(input);
            }

            // 标准验证
            Logger.Debug("JsonFormatterViewModel: 使用标准验证JSON");
            return await ValidateStandardJsonAsync(input);
        }
        catch (Newtonsoft.Json.JsonException ex)
        {
            Logger.Error(ex, "JsonFormatterViewModel: JSON验证失败 - JSON格式错误");
            // 提供更详细的错误信息
            var errorMessage = ParseJsonError(ex.Message);
            return new ValidationResult(false, $"JSON格式错误: {errorMessage}");
        }
        catch (Exception ex)
        {
            Logger.Error(ex, "JsonFormatterViewModel: JSON验证失败 - 未知错误");
            return new ValidationResult(false, $"验证失败: {ex.Message}");
        }
    }

    /// <summary>
    /// 标准JSON验证
    /// </summary>
    private async Task<ValidationResult> ValidateStandardJsonAsync(string input)
    {
        Logger.Debug("JsonFormatterViewModel: 开始标准JSON验证");
        return await Task.Run(() =>
        {
            try
            {
                // 尝试解析JSON
                using var document = JsonDocument.Parse(input);
                
                // 检查JSON结构
                var root = document.RootElement;
                var elementCount = CountJsonElements(root);
                var depth = GetJsonDepth(root);
                
                Logger.Information("JsonFormatterViewModel: 标准JSON验证成功 - 元素数量: {ElementCount}, 最大深度: {Depth}", elementCount, depth);
                
                var message = $"JSON格式正确 - 包含 {elementCount} 个元素，最大深度 {depth} 层";
                return new ValidationResult(true, message);
            }
            catch (Exception ex)
            {
                Logger.Error(ex, "JsonFormatterViewModel: 标准JSON验证失败");
                throw;
            }
        });
    }

    /// <summary>
    /// 大文件JSON流式验证
    /// </summary>
    private async Task<ValidationResult> ValidateLargeJsonAsync(string input)
    {
        Logger.Debug("JsonFormatterViewModel: 开始大文件JSON流式验证");
        return await Task.Run(() =>
        {
            try
            {
                using var stringReader = new StringReader(input);
                using var jsonReader = new JsonTextReader(stringReader);
                
                int elementCount = 0;
                int maxDepth = 0;
                int currentDepth = 0;
                
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
                
                Logger.Information("JsonFormatterViewModel: 大文件JSON流式验证成功 - 文件大小: {SizeText}, 元素数量: {ElementCount}, 最大深度: {MaxDepth}", 
                    sizeText, elementCount, maxDepth);
                    
                var message = $"JSON格式正确 - 文件大小 {sizeText}，包含 {elementCount} 个元素，最大深度 {maxDepth} 层";
                return new ValidationResult(true, message);
            }
            catch (JsonReaderException ex)
            {
                Logger.Error(ex, "JsonFormatterViewModel: 大文件JSON流式验证失败 - JSON读取错误");
                var errorMessage = ParseJsonError(ex.Message);
                return new ValidationResult(false, $"JSON格式错误: {errorMessage}");
            }
            catch (Exception ex)
            {
                Logger.Error(ex, "JsonFormatterViewModel: 大文件JSON流式验证失败");
                throw;
            }
        });
    }

    /// <summary>
    /// 解析JSON错误信息，提供更友好的提示
    /// </summary>
    private string ParseJsonError(string originalError)
    {
        Logger.Debug("JsonFormatterViewModel: 解析JSON错误信息 - 原始错误: {OriginalError}", originalError);
        
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
        Logger.Debug("JsonFormatterViewModel: 获取示例数据");
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