using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using System.Windows.Input;
using Avalonia.Controls;
using Avalonia.Platform.Storage;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

namespace DevUtilities.ViewModels;

public partial class HttpRequestViewModel : ObservableObject
{
    private readonly HttpClient _httpClient;

    [ObservableProperty]
    private string url = "https://api.github.com/users/octocat";

    [ObservableProperty]
    private string selectedMethod = "GET";

    [ObservableProperty]
    private string requestBody = "";

    [ObservableProperty]
    private string responseBody = "";

    [ObservableProperty]
    private string responseHeaders = "";

    [ObservableProperty]
    private string statusCode = "";

    [ObservableProperty]
    private string responseTime = "";

    [ObservableProperty]
    private bool isLoading = false;

    [ObservableProperty]
    private string errorMessage = "";

    [ObservableProperty]
    private bool hasError = false;

    [ObservableProperty]
    private string selectedContentType = "application/json";

    [ObservableProperty]
    private string authToken = "";

    [ObservableProperty]
    private bool useAuth = false;

    [ObservableProperty]
    private string selectedAuthType = "Bearer";

    public ObservableCollection<HttpHeader> RequestHeaders { get; } = new();
    public ObservableCollection<string> HttpMethods { get; } = new()
    {
        "GET", "POST", "PUT", "DELETE", "PATCH", "HEAD", "OPTIONS"
    };

    public ObservableCollection<string> ContentTypes { get; } = new()
    {
        "application/json",
        "application/xml",
        "text/plain",
        "text/html",
        "application/x-www-form-urlencoded",
        "multipart/form-data"
    };

    public ObservableCollection<string> AuthTypes { get; } = new()
    {
        "Bearer", "Basic", "API Key"
    };

    public HttpRequestViewModel()
    {
        _httpClient = new HttpClient();
        _httpClient.Timeout = TimeSpan.FromSeconds(30);
        
        // 添加默认请求头
        var contentTypeHeader = new HttpHeader("Content-Type", "application/json");
        contentTypeHeader.RemoveRequested += RemoveHeader;
        RequestHeaders.Add(contentTypeHeader);
        
        var userAgentHeader = new HttpHeader("User-Agent", "DevUtilities/1.0");
        userAgentHeader.RemoveRequested += RemoveHeader;
        RequestHeaders.Add(userAgentHeader);
    }

    [RelayCommand]
    private async Task SendRequest()
    {
        if (string.IsNullOrWhiteSpace(Url))
        {
            ShowError("请输入有效的URL");
            return;
        }

        try
        {
            IsLoading = true;
            HasError = false;
            ErrorMessage = "";
            
            var startTime = DateTime.Now;
            
            using var request = new HttpRequestMessage(new HttpMethod(SelectedMethod), Url);
            
            // 设置请求头
            foreach (var header in RequestHeaders.Where(h => !string.IsNullOrWhiteSpace(h.Name)))
            {
                try
                {
                    if (header.Name.Equals("Content-Type", StringComparison.OrdinalIgnoreCase))
                    {
                        // Content-Type 会在设置内容时自动添加
                        continue;
                    }
                    request.Headers.Add(header.Name, header.Value);
                }
                catch
                {
                    // 忽略无效的请求头
                }
            }

            // 设置认证
            if (UseAuth && !string.IsNullOrWhiteSpace(AuthToken))
            {
                switch (SelectedAuthType)
                {
                    case "Bearer":
                        request.Headers.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", AuthToken);
                        break;
                    case "Basic":
                        var basicAuth = Convert.ToBase64String(Encoding.UTF8.GetBytes(AuthToken));
                        request.Headers.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Basic", basicAuth);
                        break;
                    case "API Key":
                        request.Headers.Add("X-API-Key", AuthToken);
                        break;
                }
            }

            // 设置请求体
            if (!string.IsNullOrWhiteSpace(RequestBody) && 
                (SelectedMethod == "POST" || SelectedMethod == "PUT" || SelectedMethod == "PATCH"))
            {
                request.Content = new StringContent(RequestBody, Encoding.UTF8, SelectedContentType);
            }

            var response = await _httpClient.SendAsync(request);
            var endTime = DateTime.Now;
            
            ResponseTime = $"{(endTime - startTime).TotalMilliseconds:F0} ms";
            StatusCode = $"{(int)response.StatusCode} {response.StatusCode}";
            
            // 获取响应头
            var responseHeadersBuilder = new StringBuilder();
            foreach (var header in response.Headers)
            {
                responseHeadersBuilder.AppendLine($"{header.Key}: {string.Join(", ", header.Value)}");
            }
            if (response.Content.Headers != null)
            {
                foreach (var header in response.Content.Headers)
                {
                    responseHeadersBuilder.AppendLine($"{header.Key}: {string.Join(", ", header.Value)}");
                }
            }
            ResponseHeaders = responseHeadersBuilder.ToString();
            
            // 获取响应体
            var responseContent = await response.Content.ReadAsStringAsync();
            
            // 尝试格式化JSON响应
            var contentType = response.Content.Headers.ContentType;
            if (contentType?.MediaType?.Contains("json") == true)
            {
                try
                {
                    var jsonDocument = JsonDocument.Parse(responseContent);
                    ResponseBody = JsonSerializer.Serialize(jsonDocument, new JsonSerializerOptions 
                    { 
                        WriteIndented = true,
                        Encoder = System.Text.Encodings.Web.JavaScriptEncoder.UnsafeRelaxedJsonEscaping
                    });
                }
                catch
                {
                    ResponseBody = responseContent;
                }
            }
            else
            {
                ResponseBody = responseContent;
            }
        }
        catch (HttpRequestException ex)
        {
            ShowError($"HTTP请求错误: {ex.Message}");
        }
        catch (TaskCanceledException ex)
        {
            ShowError(ex.InnerException is TimeoutException ? "请求超时" : "请求被取消");
        }
        catch (Exception ex)
        {
            ShowError($"请求失败: {ex.Message}");
        }
        finally
        {
            IsLoading = false;
        }
    }

    [RelayCommand]
    private void AddHeader()
    {
        var header = new HttpHeader("", "");
        header.RemoveRequested += RemoveHeader;
        RequestHeaders.Add(header);
    }

    [RelayCommand]
    private void RemoveHeader(HttpHeader header)
    {
        RequestHeaders.Remove(header);
    }

    [RelayCommand]
    private void ClearAll()
    {
        Url = "";
        RequestBody = "";
        ResponseBody = "";
        ResponseHeaders = "";
        StatusCode = "";
        ResponseTime = "";
        ErrorMessage = "";
        HasError = false;
        AuthToken = "";
        UseAuth = false;
        RequestHeaders.Clear();
        
        // 重新添加默认请求头
        var contentTypeHeader = new HttpHeader("Content-Type", "application/json");
        contentTypeHeader.RemoveRequested += RemoveHeader;
        RequestHeaders.Add(contentTypeHeader);
        
        var userAgentHeader = new HttpHeader("User-Agent", "DevUtilities/1.0");
        userAgentHeader.RemoveRequested += RemoveHeader;
        RequestHeaders.Add(userAgentHeader);
    }

    [RelayCommand]
    private void FormatJson()
    {
        if (string.IsNullOrWhiteSpace(RequestBody))
            return;

        try
        {
            var jsonDocument = JsonDocument.Parse(RequestBody);
            RequestBody = JsonSerializer.Serialize(jsonDocument, new JsonSerializerOptions 
            { 
                WriteIndented = true,
                Encoder = System.Text.Encodings.Web.JavaScriptEncoder.UnsafeRelaxedJsonEscaping
            });
        }
        catch (JsonException)
        {
            ShowError("无效的JSON格式");
        }
    }

    [RelayCommand]
    private void LoadExample()
    {
        Url = "https://jsonplaceholder.typicode.com/posts/1";
        SelectedMethod = "GET";
        RequestBody = "";
        SelectedContentType = "application/json";
    }

    [RelayCommand]
    private void LoadPostExample()
    {
        Url = "https://jsonplaceholder.typicode.com/posts";
        SelectedMethod = "POST";
        RequestBody = JsonSerializer.Serialize(new
        {
            title = "foo",
            body = "bar",
            userId = 1
        }, new JsonSerializerOptions { WriteIndented = true });
        SelectedContentType = "application/json";
    }

    private void ShowError(string message)
    {
        ErrorMessage = message;
        HasError = true;
    }

    partial void OnSelectedMethodChanged(string value)
    {
        // 当方法改变时，清除之前的响应
        ResponseBody = "";
        ResponseHeaders = "";
        StatusCode = "";
        ResponseTime = "";
        HasError = false;
        ErrorMessage = "";
    }

    partial void OnUrlChanged(string value)
    {
        // URL改变时清除错误状态
        HasError = false;
        ErrorMessage = "";
    }
}

public partial class HttpHeader : ObservableObject
{
    [ObservableProperty]
    private string name = "";

    [ObservableProperty]
    private string value = "";

    public ICommand RemoveCommand { get; }

    public HttpHeader(string name, string value)
    {
        Name = name;
        Value = value;
        RemoveCommand = new RelayCommand(() => 
        {
            // 通过事件通知父级ViewModel删除此项
            RemoveRequested?.Invoke(this);
        });
    }

    public event Action<HttpHeader>? RemoveRequested;
}