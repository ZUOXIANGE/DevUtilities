using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using DevUtilities.Core.ViewModels.Base;
using DevUtilities.Models;

namespace DevUtilities.ViewModels;

public partial class JsonExampleGeneratorViewModel : BaseFormatterViewModel
{
    [ObservableProperty]
    private string selectedLanguage = "C#";

    [ObservableProperty]
    private bool includeNullValues = false;

    [ObservableProperty]
    private bool useRandomValues = true;

    [ObservableProperty]
    private bool prettyFormat = true;

    [ObservableProperty]
    private bool isClassToJsonMode = true;

    public List<string> SupportedLanguages { get; } = new() { "C#", "Java" };

    public string ConvertButtonText => IsClassToJsonMode ? "🚀 生成JSON" : "🔧 生成类";
    public string ConvertButtonTooltip => IsClassToJsonMode ? "生成JSON示例 (Ctrl+F)" : "生成类定义 (Ctrl+F)";
    public string InputAreaTitle => IsClassToJsonMode ? "类定义输入" : "JSON输入";
    public string OutputAreaTitle => IsClassToJsonMode ? "JSON示例输出" : "类定义输出";

    public JsonExampleGeneratorViewModel()
    {
        Title = "JSON示例生成器";
        Description = "支持类定义与JSON之间的双向转换";
        Icon = "📄";
        ToolType = ToolType.JsonExampleGenerator;
        UpdatePlaceholders();
    }

    partial void OnIsClassToJsonModeChanged(bool value)
    {
        UpdatePlaceholders();
        OnPropertyChanged(nameof(ConvertButtonText));
        OnPropertyChanged(nameof(ConvertButtonTooltip));
        OnPropertyChanged(nameof(InputAreaTitle));
        OnPropertyChanged(nameof(OutputAreaTitle));
    }

    private void UpdatePlaceholders()
    {
        if (IsClassToJsonMode)
        {
            InputPlaceholder = "请输入C#或Java类定义...";
            OutputPlaceholder = "生成的JSON示例将显示在这里...";
        }
        else
        {
            InputPlaceholder = "请输入JSON数据...";
            OutputPlaceholder = "生成的类定义将显示在这里...";
        }
    }

    protected override async Task<string> FormatContentAsync(string input)
    {
        if (string.IsNullOrWhiteSpace(input))
        {
            throw new ArgumentException(IsClassToJsonMode ? "请输入类定义" : "请输入JSON数据");
        }

        try
        {
            return await Task.Run(() =>
            {
                if (IsClassToJsonMode)
                {
                    // 类定义 → JSON示例
                    var classInfo = ParseClassDefinition(input, SelectedLanguage);
                    return GenerateJsonExample(classInfo);
                }
                else
                {
                    // JSON → 类定义
                    return GenerateClassDefinition(input, SelectedLanguage);
                }
            });
        }
        catch (Exception ex)
        {
            var operation = IsClassToJsonMode ? "生成JSON示例" : "生成类定义";
            throw new InvalidOperationException($"{operation}失败: {ex.Message}", ex);
        }
    }

    protected override string GetExampleData()
    {
        return SelectedLanguage switch
        {
            "C#" => @"public class User
{
    public int Id { get; set; }
    public string Name { get; set; }
    public string Email { get; set; }
    public DateTime CreatedAt { get; set; }
    public bool IsActive { get; set; }
    public List<string> Tags { get; set; }
    public Address Address { get; set; }
}

public class Address
{
    public string Street { get; set; }
    public string City { get; set; }
    public string Country { get; set; }
    public int ZipCode { get; set; }
}",
            "Java" => @"public class User {
    private int id;
    private String name;
    private String email;
    private Date createdAt;
    private boolean isActive;
    private List<String> tags;
    private Address address;
    
    // getters and setters...
}

public class Address {
    private String street;
    private String city;
    private String country;
    private int zipCode;
    
    // getters and setters...
}",
            _ => ""
        };
    }

    private ClassInfo ParseClassDefinition(string input, string language)
    {
        return language switch
        {
            "C#" => ParseCSharpClass(input),
            "Java" => ParseJavaClass(input),
            _ => throw new NotSupportedException($"不支持的语言: {language}")
        };
    }

    private ClassInfo ParseCSharpClass(string input)
    {
        var classInfo = new ClassInfo();
        
        // 匹配类名
        var classMatch = Regex.Match(input, @"public\s+class\s+(\w+)", RegexOptions.IgnoreCase);
        if (classMatch.Success)
        {
            classInfo.Name = classMatch.Groups[1].Value;
        }

        // 匹配属性
        var propertyMatches = Regex.Matches(input, 
            @"public\s+(\w+(?:<\w+>)?(?:\[\])?)\s+(\w+)\s*{\s*get;\s*set;\s*}", 
            RegexOptions.IgnoreCase);

        foreach (Match match in propertyMatches)
        {
            var type = match.Groups[1].Value;
            var name = match.Groups[2].Value;
            classInfo.Properties.Add(new PropertyInfo(name, type));
        }

        return classInfo;
    }

    private ClassInfo ParseJavaClass(string input)
    {
        var classInfo = new ClassInfo();
        
        // 匹配类名
        var classMatch = Regex.Match(input, @"public\s+class\s+(\w+)", RegexOptions.IgnoreCase);
        if (classMatch.Success)
        {
            classInfo.Name = classMatch.Groups[1].Value;
        }

        // 匹配字段
        var fieldMatches = Regex.Matches(input, 
            @"private\s+(\w+(?:<\w+>)?(?:\[\])?)\s+(\w+);", 
            RegexOptions.IgnoreCase);

        foreach (Match match in fieldMatches)
        {
            var type = match.Groups[1].Value;
            var name = match.Groups[2].Value;
            classInfo.Properties.Add(new PropertyInfo(name, type));
        }

        return classInfo;
    }

    private string GenerateJsonExample(ClassInfo classInfo)
    {
        var json = new StringBuilder();
        json.AppendLine("{");

        for (int i = 0; i < classInfo.Properties.Count; i++)
        {
            var property = classInfo.Properties[i];
            var value = GenerateValueForType(property.Type);
            
            if (!IncludeNullValues && value == "null")
                continue;

            var indent = PrettyFormat ? "  " : "";
            var comma = i < classInfo.Properties.Count - 1 ? "," : "";
            var spacing = PrettyFormat ? " " : "";
            var newline = PrettyFormat ? "\n" : "";

            json.Append($"{indent}\"{ToCamelCase(property.Name)}\":{spacing}{value}{comma}{newline}");
        }

        json.Append("}");
        return json.ToString();
    }

    private string GenerateValueForType(string type)
    {
        // 处理泛型类型
        if (type.Contains("<") && type.Contains(">"))
        {
            if (type.StartsWith("List<") || type.StartsWith("ArrayList<") || type.Contains("[]"))
            {
                var innerType = ExtractGenericType(type);
                var sampleValue = GenerateValueForType(innerType);
                return $"[{sampleValue}, {sampleValue}]";
            }
        }

        // 处理数组类型
        if (type.EndsWith("[]"))
        {
            var elementType = type.Substring(0, type.Length - 2);
            var sampleValue = GenerateValueForType(elementType);
            return $"[{sampleValue}, {sampleValue}]";
        }

        return type.ToLower() switch
        {
            "int" or "integer" or "long" => UseRandomValues ? Random.Shared.Next(1, 1000).ToString() : "123",
            "double" or "float" or "decimal" => UseRandomValues ? (Random.Shared.NextDouble() * 100).ToString("F2") : "123.45",
            "string" => UseRandomValues ? $"\"示例文本{Random.Shared.Next(1, 100)}\"" : "\"示例文本\"",
            "bool" or "boolean" => UseRandomValues ? (Random.Shared.Next(0, 2) == 1 ? "true" : "false") : "true",
            "datetime" or "date" => $"\"{DateTime.Now:yyyy-MM-ddTHH:mm:ss}\"",
            "guid" => $"\"{Guid.NewGuid()}\"",
            _ when IsCustomType(type) => "{ /* 自定义对象 */ }",
            _ => "null"
        };
    }

    private string ExtractGenericType(string type)
    {
        var start = type.IndexOf('<') + 1;
        var end = type.LastIndexOf('>');
        if (start > 0 && end > start)
        {
            return type.Substring(start, end - start);
        }
        return "string";
    }

    private bool IsCustomType(string type)
    {
        var primitiveTypes = new[] { "int", "long", "double", "float", "decimal", "string", "bool", "boolean", "datetime", "date", "guid" };
        return !primitiveTypes.Contains(type.ToLower());
    }

    private string ToCamelCase(string input)
    {
        if (string.IsNullOrEmpty(input))
            return input;

        return char.ToLower(input[0]) + input.Substring(1);
    }

    #region JSON到类定义转换

    private string GenerateClassDefinition(string jsonInput, string language)
    {
        var classInfo = ParseJsonToClassInfo(jsonInput);
        return language.ToLower() switch
        {
            "c#" => GenerateCSharpClass(classInfo),
            "java" => GenerateJavaClass(classInfo),
            _ => throw new ArgumentException($"不支持的语言: {language}")
        };
    }

    private ClassInfo ParseJsonToClassInfo(string jsonInput)
    {
        try
        {
            using var document = JsonDocument.Parse(jsonInput);
            var root = document.RootElement;
            
            if (root.ValueKind != JsonValueKind.Object)
            {
                throw new ArgumentException("JSON必须是一个对象");
            }

            var classInfo = new ClassInfo
            {
                Name = "GeneratedClass"
            };

            foreach (var property in root.EnumerateObject())
            {
                var propertyInfo = new PropertyInfo(
                    ToPascalCase(property.Name),
                    InferTypeFromJsonElement(property.Value)
                );
                classInfo.Properties.Add(propertyInfo);
            }

            return classInfo;
        }
        catch (JsonException ex)
        {
            throw new ArgumentException($"无效的JSON格式: {ex.Message}");
        }
    }

    private string InferTypeFromJsonElement(JsonElement element)
    {
        return element.ValueKind switch
        {
            JsonValueKind.String => "string",
            JsonValueKind.Number => element.TryGetInt32(out _) ? "int" : "double",
            JsonValueKind.True or JsonValueKind.False => "bool",
            JsonValueKind.Array => InferArrayType(element),
            JsonValueKind.Object => "object",
            JsonValueKind.Null => "string?",
            _ => "object"
        };
    }

    private string InferArrayType(JsonElement arrayElement)
    {
        if (arrayElement.GetArrayLength() == 0)
        {
            return "List<object>";
        }

        var firstElement = arrayElement.EnumerateArray().First();
        var elementType = InferTypeFromJsonElement(firstElement);
        return $"List<{elementType}>";
    }

    private string GenerateCSharpClass(ClassInfo classInfo)
    {
        var sb = new StringBuilder();
        sb.AppendLine("using System;");
        sb.AppendLine("using System.Collections.Generic;");
        sb.AppendLine();
        sb.AppendLine($"public class {classInfo.Name}");
        sb.AppendLine("{");

        foreach (var prop in classInfo.Properties)
        {
            sb.AppendLine($"    public {prop.Type} {prop.Name} {{ get; set; }}");
        }

        sb.AppendLine("}");
        return sb.ToString();
    }

    private string GenerateJavaClass(ClassInfo classInfo)
    {
        var sb = new StringBuilder();
        sb.AppendLine("import java.util.List;");
        sb.AppendLine();
        sb.AppendLine($"public class {classInfo.Name} {{");

        // 生成字段
        foreach (var prop in classInfo.Properties)
        {
            var javaType = ConvertToJavaType(prop.Type);
            sb.AppendLine($"    private {javaType} {ToCamelCase(prop.Name)};");
        }

        sb.AppendLine();

        // 生成getter和setter
        foreach (var prop in classInfo.Properties)
        {
            var javaType = ConvertToJavaType(prop.Type);
            var fieldName = ToCamelCase(prop.Name);
            var methodName = prop.Name;

            sb.AppendLine($"    public {javaType} get{methodName}() {{");
            sb.AppendLine($"        return {fieldName};");
            sb.AppendLine("    }");
            sb.AppendLine();
            sb.AppendLine($"    public void set{methodName}({javaType} {fieldName}) {{");
            sb.AppendLine($"        this.{fieldName} = {fieldName};");
            sb.AppendLine("    }");
            sb.AppendLine();
        }

        sb.AppendLine("}");
        return sb.ToString();
    }

    private string ConvertToJavaType(string csharpType)
    {
        return csharpType switch
        {
            "int" => "int",
            "double" => "double", 
            "string" => "String",
            "string?" => "String",
            "bool" => "boolean",
            "object" => "Object",
            var type when type.StartsWith("List<") => ConvertListType(type),
            _ => "Object"
        };
    }

    private string ConvertListType(string listType)
    {
        // 提取泛型类型
        var start = listType.IndexOf('<') + 1;
        var end = listType.LastIndexOf('>');
        if (start > 0 && end > start)
        {
            var innerType = listType.Substring(start, end - start);
            var javaInnerType = ConvertToJavaType(innerType);
            return $"List<{javaInnerType}>";
        }
        return "List<Object>";
    }

    private string ToPascalCase(string input)
    {
        if (string.IsNullOrEmpty(input))
            return input;

        return char.ToUpper(input[0]) + input.Substring(1);
    }

    #endregion
}

public class ClassInfo
{
    public string Name { get; set; } = "";
    public List<PropertyInfo> Properties { get; set; } = new();
}

public class PropertyInfo
{
    public string Name { get; set; }
    public string Type { get; set; }

    public PropertyInfo(string name, string type)
    {
        Name = name;
        Type = type;
    }
}