using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Web;
using System.Windows.Input;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

namespace DevUtilities.ViewModels;

public partial class UrlToolsViewModel : ObservableObject
{
    [ObservableProperty]
    private string inputUrl = "";

    [ObservableProperty]
    private string encodedUrl = "";

    [ObservableProperty]
    private string decodedUrl = "";

    [ObservableProperty]
    private string parseResult = "";

    [ObservableProperty]
    private bool isValidUrl = false;

    [ObservableProperty]
    private string validationMessage = "";

    [ObservableProperty]
    private string selectedEncodingType = "完整URL编码";

    [ObservableProperty]
    private string customQueryParams = "";

    [ObservableProperty]
    private string builtUrl = "";

    [ObservableProperty]
    private string baseUrl = "https://example.com";

    [ObservableProperty]
    private string pathSegments = "/api/v1/users";

    public List<string> EncodingTypes { get; } = new()
    {
        "完整URL编码",
        "查询参数编码", 
        "路径编码",
        "HTML实体编码",
        "Base64编码"
    };

    public ObservableCollection<UrlComponent> UrlComponents { get; } = new();
    public ObservableCollection<QueryParameter> QueryParameters { get; } = new();

    public UrlToolsViewModel()
    {
        // 初始化示例
        InputUrl = "https://example.com/path?param1=value1&param2=hello world";
        ProcessUrl();
    }

    [RelayCommand]
    private void EncodeUrl()
    {
        if (string.IsNullOrEmpty(InputUrl))
        {
            EncodedUrl = "";
            return;
        }

        try
        {
            EncodedUrl = SelectedEncodingType switch
            {
                "完整URL编码" => Uri.EscapeDataString(InputUrl),
                "查询参数编码" => EncodeQueryParameters(InputUrl),
                "路径编码" => EncodePath(InputUrl),
                "HTML实体编码" => HttpUtility.HtmlEncode(InputUrl),
                "Base64编码" => Convert.ToBase64String(Encoding.UTF8.GetBytes(InputUrl)),
                _ => Uri.EscapeDataString(InputUrl)
            };
        }
        catch (Exception ex)
        {
            EncodedUrl = $"编码错误: {ex.Message}";
        }
    }

    [RelayCommand]
    private void DecodeUrl()
    {
        if (string.IsNullOrEmpty(InputUrl))
        {
            DecodedUrl = "";
            return;
        }

        try
        {
            DecodedUrl = SelectedEncodingType switch
            {
                "完整URL编码" => Uri.UnescapeDataString(InputUrl),
                "查询参数编码" => DecodeQueryParameters(InputUrl),
                "路径编码" => DecodePath(InputUrl),
                "HTML实体编码" => HttpUtility.HtmlDecode(InputUrl),
                "Base64编码" => Encoding.UTF8.GetString(Convert.FromBase64String(InputUrl)),
                _ => Uri.UnescapeDataString(InputUrl)
            };
        }
        catch (Exception ex)
        {
            DecodedUrl = $"解码错误: {ex.Message}";
        }
    }

    [RelayCommand]
    private void BuildUrl()
    {
        try
        {
            var uriBuilder = new UriBuilder(BaseUrl);
            
            if (!string.IsNullOrEmpty(PathSegments))
            {
                uriBuilder.Path = PathSegments;
            }

            if (QueryParameters.Any())
            {
                var queryString = string.Join("&", 
                    QueryParameters.Where(p => !string.IsNullOrEmpty(p.Key))
                                  .Select(p => $"{Uri.EscapeDataString(p.Key)}={Uri.EscapeDataString(p.Value ?? "")}"));
                uriBuilder.Query = queryString;
            }

            BuiltUrl = uriBuilder.ToString();
        }
        catch (Exception ex)
        {
            BuiltUrl = $"构建错误: {ex.Message}";
        }
    }

    [RelayCommand]
    private void AddQueryParameter()
    {
        var parameter = new QueryParameter("", "");
        parameter.RemoveRequested += RemoveQueryParameterFromEvent;
        QueryParameters.Add(parameter);
    }

    [RelayCommand]
    private void RemoveQueryParameter(QueryParameter parameter)
    {
        QueryParameters.Remove(parameter);
        BuildUrl();
    }

    private void RemoveQueryParameterFromEvent(QueryParameter parameter)
    {
        QueryParameters.Remove(parameter);
        BuildUrl();
    }

    [RelayCommand]
    private void ParseQueryToParameters()
    {
        if (string.IsNullOrEmpty(InputUrl))
            return;

        try
        {
            var uri = new Uri(InputUrl);
            QueryParameters.Clear();
            
            var queryParams = ParseQueryString(uri.Query);
            foreach (var param in queryParams)
            {
                var parameter = new QueryParameter(param.Key, param.Value);
                parameter.RemoveRequested += RemoveQueryParameterFromEvent;
                QueryParameters.Add(parameter);
            }
        }
        catch (Exception ex)
        {
            ValidationMessage = $"解析查询参数失败: {ex.Message}";
        }
    }

    private string EncodeQueryParameters(string input)
    {
        if (input.Contains('?'))
        {
            var parts = input.Split('?', 2);
            var baseUrl = parts[0];
            var queryString = parts[1];
            
            var encodedParams = queryString.Split('&')
                .Select(param =>
                {
                    var keyValue = param.Split('=', 2);
                    var key = Uri.EscapeDataString(keyValue[0]);
                    var value = keyValue.Length > 1 ? Uri.EscapeDataString(keyValue[1]) : "";
                    return $"{key}={value}";
                });
            
            return $"{baseUrl}?{string.Join("&", encodedParams)}";
        }
        
        return Uri.EscapeDataString(input);
    }

    private string DecodeQueryParameters(string input)
    {
        if (input.Contains('?'))
        {
            var parts = input.Split('?', 2);
            var baseUrl = parts[0];
            var queryString = parts[1];
            
            var decodedParams = queryString.Split('&')
                .Select(param =>
                {
                    var keyValue = param.Split('=', 2);
                    var key = Uri.UnescapeDataString(keyValue[0]);
                    var value = keyValue.Length > 1 ? Uri.UnescapeDataString(keyValue[1]) : "";
                    return $"{key}={value}";
                });
            
            return $"{baseUrl}?{string.Join("&", decodedParams)}";
        }
        
        return Uri.UnescapeDataString(input);
    }

    private string EncodePath(string input)
    {
        return string.Join("/", input.Split('/').Select(Uri.EscapeDataString));
    }

    private string DecodePath(string input)
    {
        return string.Join("/", input.Split('/').Select(Uri.UnescapeDataString));
    }

    [RelayCommand]
    private void ParseUrl()
    {
        try
        {
            if (string.IsNullOrWhiteSpace(InputUrl))
            {
                ClearParseResult();
                return;
            }

            if (Uri.TryCreate(InputUrl, UriKind.Absolute, out Uri? uri))
            {
                IsValidUrl = true;
                ValidationMessage = "✓ 有效的URL";
                
                UrlComponents.Clear();
                
                // 基本组件
                UrlComponents.Add(new UrlComponent("协议 (Scheme)", uri.Scheme));
                UrlComponents.Add(new UrlComponent("主机 (Host)", uri.Host));
                
                if (uri.Port != -1 && !IsDefaultPort(uri.Scheme, uri.Port))
                {
                    UrlComponents.Add(new UrlComponent("端口 (Port)", uri.Port.ToString()));
                }
                
                if (!string.IsNullOrEmpty(uri.AbsolutePath) && uri.AbsolutePath != "/")
                {
                    UrlComponents.Add(new UrlComponent("路径 (Path)", uri.AbsolutePath));
                }
                
                if (!string.IsNullOrEmpty(uri.Query))
                {
                    UrlComponents.Add(new UrlComponent("查询字符串 (Query)", uri.Query));
                    
                    // 解析查询参数
                    var queryParams = ParseQueryString(uri.Query);
                    foreach (var param in queryParams)
                    {
                        UrlComponents.Add(new UrlComponent($"  参数: {param.Key}", param.Value));
                    }
                }
                
                if (!string.IsNullOrEmpty(uri.Fragment))
                {
                    UrlComponents.Add(new UrlComponent("片段 (Fragment)", uri.Fragment));
                }
                
                // 其他信息
                UrlComponents.Add(new UrlComponent("完整URL", uri.AbsoluteUri));
                UrlComponents.Add(new UrlComponent("域名信息", GetDomainInfo(uri.Host)));
                
                BuildParseResult();
            }
            else
            {
                IsValidUrl = false;
                ValidationMessage = "✗ 无效的URL格式";
                ClearParseResult();
            }
        }
        catch (Exception ex)
        {
            IsValidUrl = false;
            ValidationMessage = $"✗ 解析错误: {ex.Message}";
            ClearParseResult();
        }
    }

    [RelayCommand]
    private void ProcessUrl()
    {
        EncodeUrl();
        DecodeUrl();
        ParseUrl();
    }

    [RelayCommand]
    private void ClearAll()
    {
        InputUrl = "";
        EncodedUrl = "";
        DecodedUrl = "";
        BuiltUrl = "";
        BaseUrl = "https://example.com";
        PathSegments = "/api/v1/users";
        QueryParameters.Clear();
        ClearParseResult();
    }

    [RelayCommand]
    private void UseExample(string example)
    {
        InputUrl = example;
        ProcessUrl();
    }

    private void ClearParseResult()
    {
        ParseResult = "";
        UrlComponents.Clear();
        IsValidUrl = false;
        ValidationMessage = "";
    }

    private void BuildParseResult()
    {
        var sb = new StringBuilder();
        sb.AppendLine("URL 解析结果:");
        sb.AppendLine(new string('=', 40));
        
        foreach (var component in UrlComponents)
        {
            if (component.Name.StartsWith("  "))
            {
                sb.AppendLine($"{component.Name}: {component.Value}");
            }
            else
            {
                sb.AppendLine($"{component.Name}: {component.Value}");
            }
        }
        
        ParseResult = sb.ToString();
    }

    private Dictionary<string, string> ParseQueryString(string query)
    {
        var result = new Dictionary<string, string>();
        
        if (string.IsNullOrEmpty(query))
            return result;
        
        // 移除开头的 '?'
        if (query.StartsWith("?"))
            query = query.Substring(1);
        
        var pairs = query.Split('&');
        foreach (var pair in pairs)
        {
            var keyValue = pair.Split('=', 2);
            var key = Uri.UnescapeDataString(keyValue[0]);
            var value = keyValue.Length > 1 ? Uri.UnescapeDataString(keyValue[1]) : "";
            
            if (!result.ContainsKey(key))
            {
                result[key] = value;
            }
        }
        
        return result;
    }

    private bool IsDefaultPort(string scheme, int port)
    {
        return scheme.ToLower() switch
        {
            "http" => port == 80,
            "https" => port == 443,
            "ftp" => port == 21,
            "ftps" => port == 990,
            _ => false
        };
    }

    private string GetDomainInfo(string host)
    {
        if (string.IsNullOrEmpty(host))
            return "无";
        
        var parts = host.Split('.');
        if (parts.Length < 2)
            return "本地主机或IP地址";
        
        if (parts.Length == 2)
            return $"顶级域名: {parts[1]}";
        
        return $"子域名: {string.Join(".", parts.Take(parts.Length - 2))}, 域名: {parts[parts.Length - 2]}, 顶级域名: {parts[parts.Length - 1]}";
    }

    partial void OnInputUrlChanged(string value)
    {
        ProcessUrl();
    }

    partial void OnSelectedEncodingTypeChanged(string value)
    {
        if (!string.IsNullOrEmpty(InputUrl))
        {
            EncodeUrl();
            DecodeUrl();
        }
    }

    partial void OnBaseUrlChanged(string value)
    {
        BuildUrl();
    }

    partial void OnPathSegmentsChanged(string value)
    {
        BuildUrl();
    }
}

public class UrlComponent
{
    public string Name { get; set; }
    public string Value { get; set; }

    public UrlComponent(string name, string value)
    {
        Name = name;
        Value = value;
    }
}

public class QueryParameter : ObservableObject
{
    private string _key = "";
    private string _value = "";

    public string Key
    {
        get => _key;
        set => SetProperty(ref _key, value);
    }

    public string Value
    {
        get => _value;
        set => SetProperty(ref _value, value);
    }

    public ICommand RemoveCommand { get; }

    public QueryParameter(string key, string value)
    {
        Key = key;
        Value = value;
        RemoveCommand = new RelayCommand(() => 
        {
            // 通过事件通知父级ViewModel删除此项
            RemoveRequested?.Invoke(this);
        });
    }

    public event Action<QueryParameter>? RemoveRequested;
}