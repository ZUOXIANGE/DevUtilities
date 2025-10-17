using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Web;
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

    public ObservableCollection<UrlComponent> UrlComponents { get; } = new();

    public UrlToolsViewModel()
    {
        // 初始化示例
        InputUrl = "https://example.com/path?param1=value1&param2=hello world";
        ProcessUrl();
    }

    [RelayCommand]
    private void EncodeUrl()
    {
        try
        {
            if (string.IsNullOrWhiteSpace(InputUrl))
            {
                EncodedUrl = "";
                return;
            }

            EncodedUrl = Uri.EscapeDataString(InputUrl);
        }
        catch (Exception ex)
        {
            EncodedUrl = $"编码失败: {ex.Message}";
        }
    }

    [RelayCommand]
    private void DecodeUrl()
    {
        try
        {
            if (string.IsNullOrWhiteSpace(InputUrl))
            {
                DecodedUrl = "";
                return;
            }

            DecodedUrl = Uri.UnescapeDataString(InputUrl);
        }
        catch (Exception ex)
        {
            DecodedUrl = $"解码错误: {ex.Message}";
        }
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