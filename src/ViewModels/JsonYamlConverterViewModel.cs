using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.ComponentModel;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using YamlDotNet.Serialization;
using YamlDotNet.Serialization.NamingConventions;
using DevUtilities.Core.ViewModels.Base;
using System.Text;
using Serilog;

namespace DevUtilities.ViewModels;

public partial class JsonYamlConverterViewModel : BaseFormatterViewModel
{
    private static readonly ILogger Logger = Log.ForContext<JsonYamlConverterViewModel>();

    [ObservableProperty]
    private bool _isJsonToYaml = true;

    [ObservableProperty]
    private bool _prettyPrint = true;

    [ObservableProperty]
    private bool _useQuotedStrings = false;

    [ObservableProperty]
    private string _conversionDirection = "JSON → YAML";

    public JsonYamlConverterViewModel()
    {
        Logger.Debug("JsonYamlConverterViewModel: 开始初始化");
        try
        {
            Title = "JSON/YAML转换器";
            Description = "JSON和YAML格式互相转换工具";
            Icon = "🔄";
            ToolType = Models.ToolType.JsonYamlConverter;
            
            InputPlaceholder = "请输入JSON或YAML内容...";
            OutputPlaceholder = "转换结果将显示在这里...";
            
            Logger.Information("JsonYamlConverterViewModel: 初始化完成 - Title: {Title}, Description: {Description}", Title, Description);
        }
        catch (Exception ex)
        {
            Logger.Error(ex, "JsonYamlConverterViewModel: 初始化失败");
            throw;
        }
    }

    [RelayCommand]
    private void SwitchDirection()
    {
        IsJsonToYaml = !IsJsonToYaml;
        ConversionDirection = IsJsonToYaml ? "JSON → YAML" : "YAML → JSON";
        
        // 交换输入输出内容
        if (!string.IsNullOrEmpty(InputText) || !string.IsNullOrEmpty(OutputText))
        {
            var temp = InputText;
            InputText = OutputText;
            OutputText = temp;
        }
        
        // 更新占位符
        if (IsJsonToYaml)
        {
            InputPlaceholder = "请输入JSON内容...";
            OutputPlaceholder = "YAML结果将显示在这里...";
        }
        else
        {
            InputPlaceholder = "请输入YAML内容...";
            OutputPlaceholder = "JSON结果将显示在这里...";
        }
        
        Logger.Information("JsonYamlConverterViewModel: 切换转换方向 - IsJsonToYaml: {IsJsonToYaml}", IsJsonToYaml);
    }

    /// <summary>
    /// 格式化内容的抽象方法实现
    /// </summary>
    protected override async Task<string> FormatContentAsync(string input)
    {
        Logger.Debug("JsonYamlConverterViewModel: 开始格式化内容 - 转换方向: {Direction}", ConversionDirection);
        
        if (string.IsNullOrWhiteSpace(input))
        {
            Logger.Warning("JsonYamlConverterViewModel: 输入内容为空");
            return string.Empty;
        }

        try
        {
            if (IsJsonToYaml)
            {
                Logger.Debug("JsonYamlConverterViewModel: 执行JSON到YAML转换");
                return await ConvertJsonToYamlAsync(input);
            }
            else
            {
                Logger.Debug("JsonYamlConverterViewModel: 执行YAML到JSON转换");
                return await ConvertYamlToJsonAsync(input);
            }
        }
        catch (Exception ex)
        {
            Logger.Error(ex, "JsonYamlConverterViewModel: 格式化内容失败");
            throw;
        }
    }

    private async Task<string> ConvertJsonToYamlAsync(string jsonInput)
    {
        Logger.Debug("JsonYamlConverterViewModel: 开始JSON到YAML转换");
        Logger.Debug($"JsonYamlConverterViewModel: 输入JSON: {jsonInput}");
        
        try
        {
            // 使用JsonConvert反序列化为动态对象，然后转换为Dictionary
            var dynamicObj = JsonConvert.DeserializeObject(jsonInput);
            
            // 递归转换JToken为原生.NET类型
            var convertedObj = ConvertJTokenToNative(dynamicObj);
            
            Logger.Debug($"JsonYamlConverterViewModel: 转换后的对象类型: {convertedObj?.GetType().Name}");
            
            // 创建YAML序列化器
            var serializerBuilder = new SerializerBuilder();
            
            // 根据美化选项设置格式
            if (PrettyPrint)
            {
                serializerBuilder = serializerBuilder.WithIndentedSequences();
            }
            
            // 根据引号字符串选项设置引号策略
            if (UseQuotedStrings)
            {
                serializerBuilder = serializerBuilder.WithQuotingNecessaryStrings();
            }
            
            var serializer = serializerBuilder.Build();
            
            // 转换为YAML
            var yaml = serializer.Serialize(convertedObj);
            
            Logger.Information("JsonYamlConverterViewModel: JSON到YAML转换成功");
            Logger.Debug($"JsonYamlConverterViewModel: 生成的YAML: {yaml}");
            Logger.Debug($"JsonYamlConverterViewModel: 生成的YAML长度: {yaml?.Length}");
            
            return await Task.FromResult(yaml);
        }
        catch (JsonException ex)
        {
            Logger.Error(ex, "JsonYamlConverterViewModel: JSON解析失败");
            throw new InvalidOperationException($"JSON格式错误: {ex.Message}", ex);
        }
        catch (Exception ex)
        {
            Logger.Error(ex, "JsonYamlConverterViewModel: JSON到YAML转换失败");
            throw new InvalidOperationException($"转换失败: {ex.Message}", ex);
        }
    }

    /// <summary>
    /// 递归转换JToken为原生.NET类型
    /// </summary>
    private object? ConvertJTokenToNative(object? obj)
    {
        if (obj == null) return null;

        switch (obj)
        {
            case JObject jObject:
                var dict = new Dictionary<string, object?>();
                foreach (var prop in jObject.Properties())
                {
                    dict[prop.Name] = ConvertJTokenToNative(prop.Value);
                }
                return dict;

            case JArray jArray:
                var list = new List<object?>();
                foreach (var item in jArray)
                {
                    list.Add(ConvertJTokenToNative(item));
                }
                return list;

            case JValue jValue:
                return jValue.Value;

            default:
                return obj;
        }
    }

    private async Task<string> ConvertYamlToJsonAsync(string yamlInput)
    {
        Logger.Debug("JsonYamlConverterViewModel: 开始YAML到JSON转换");
        
        try
        {
            // 创建YAML反序列化器
            var deserializer = new DeserializerBuilder()
                .WithNamingConvention(CamelCaseNamingConvention.Instance)
                .Build();
            
            // 解析YAML
            var obj = deserializer.Deserialize(yamlInput);
            
            // 转换为JSON
            var jsonSettings = new JsonSerializerSettings
            {
                Formatting = PrettyPrint ? Formatting.Indented : Formatting.None,
                NullValueHandling = NullValueHandling.Include
            };
            
            var json = JsonConvert.SerializeObject(obj, jsonSettings);
            
            Logger.Information("JsonYamlConverterViewModel: YAML到JSON转换成功");
            return await Task.FromResult(json);
        }
        catch (YamlDotNet.Core.YamlException ex)
        {
            Logger.Error(ex, "JsonYamlConverterViewModel: YAML解析失败");
            throw new InvalidOperationException($"YAML格式错误: {ex.Message}", ex);
        }
        catch (Exception ex)
        {
            Logger.Error(ex, "JsonYamlConverterViewModel: YAML到JSON转换失败");
            throw new InvalidOperationException($"转换失败: {ex.Message}", ex);
        }
    }

    /// <summary>
    /// 验证内容
    /// </summary>
    private async Task<ValidationResult> ValidateContentAsync(string content)
    {
        Logger.Debug("JsonYamlConverterViewModel: 开始验证内容 - 转换方向: {Direction}", ConversionDirection);
        
        if (string.IsNullOrWhiteSpace(content))
        {
            Logger.Debug("JsonYamlConverterViewModel: 内容为空，验证通过");
            return new ValidationResult(true, "内容为空");
        }

        try
        {
            await Task.Delay(1); // 让UI有机会更新

            if (IsJsonToYaml)
            {
                // 验证JSON格式
                Logger.Debug("JsonYamlConverterViewModel: 验证JSON格式");
                JToken.Parse(content);
                Logger.Debug("JsonYamlConverterViewModel: JSON格式验证通过");
                return new ValidationResult(true, "JSON格式正确");
            }
            else
            {
                // 验证YAML格式
                Logger.Debug("JsonYamlConverterViewModel: 验证YAML格式");
                var deserializer = new YamlDotNet.Serialization.Deserializer();
                deserializer.Deserialize(content);
                Logger.Debug("JsonYamlConverterViewModel: YAML格式验证通过");
                return new ValidationResult(true, "YAML格式正确");
            }
        }
        catch (JsonException ex)
        {
            Logger.Warning(ex, "JsonYamlConverterViewModel: JSON格式验证失败");
            return new ValidationResult(false, $"JSON格式错误: {ex.Message}");
        }
        catch (YamlDotNet.Core.YamlException ex)
        {
            Logger.Warning(ex, "JsonYamlConverterViewModel: YAML格式验证失败");
            return new ValidationResult(false, $"YAML格式错误: {ex.Message}");
        }
        catch (Exception ex)
        {
            Logger.Error(ex, "JsonYamlConverterViewModel: 验证过程中发生未知错误");
            return new ValidationResult(false, $"验证失败: {ex.Message}");
        }
    }

    /// <summary>
    /// 重写压缩方法，正确处理JSON和YAML的压缩
    /// </summary>
    protected override async Task<string> OnCompressAsync(string input)
    {
        Logger.Debug("JsonYamlConverterViewModel: 开始压缩内容 - 转换方向: {Direction}", ConversionDirection);
        
        if (string.IsNullOrWhiteSpace(input))
        {
            Logger.Warning("JsonYamlConverterViewModel: 压缩输入内容为空");
            return string.Empty;
        }

        try
        {
            // 首先执行转换，获得输出内容
            string convertedOutput;
            if (IsJsonToYaml)
            {
                // JSON到YAML转换
                Logger.Debug("JsonYamlConverterViewModel: 执行JSON到YAML转换后压缩");
                convertedOutput = await ConvertJsonToYamlAsync(input);
                
                // 压缩YAML输出：移除多余的空行和缩进
                var lines = convertedOutput.Split('\n')
                    .Where(line => !string.IsNullOrWhiteSpace(line))
                    .Select(line => line.Trim());
                
                return string.Join("\n", lines);
            }
            else
            {
                // YAML到JSON转换
                Logger.Debug("JsonYamlConverterViewModel: 执行YAML到JSON转换后压缩");
                convertedOutput = await ConvertYamlToJsonAsync(input);
                
                // 压缩JSON输出：移除格式化
                var jsonObject = JToken.Parse(convertedOutput);
                return jsonObject.ToString(Formatting.None);
            }
        }
        catch (Exception ex)
        {
            Logger.Error(ex, "JsonYamlConverterViewModel: 压缩内容失败");
            throw new InvalidOperationException($"压缩失败: {ex.Message}", ex);
        }
    }

    /// <summary>
    /// 获取示例数据 - 重写基类方法
    /// </summary>
    protected override string GetExampleData()
    {
        Logger.Debug("JsonYamlConverterViewModel: 获取示例内容 - 转换方向: {Direction}", ConversionDirection);
        
        if (IsJsonToYaml)
        {
            // JSON示例
            return @"{
  ""name"": ""张三"",
  ""age"": 30,
  ""city"": ""北京"",
  ""hobbies"": [
    ""阅读"",
    ""游泳"",
    ""编程""
  ],
  ""address"": {
    ""street"": ""长安街1号"",
    ""zipcode"": ""100000""
  }
}";
        }
        else
        {
            // YAML示例
            return @"name: 张三
age: 30
city: 北京
hobbies:
  - 阅读
  - 游泳
  - 编程
address:
  street: 长安街1号
  zipcode: '100000'";
        }
    }
}