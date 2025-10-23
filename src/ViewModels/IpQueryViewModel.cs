using System;
using System.Net.Http;
using System.Text.Json;
using System.Threading.Tasks;
using System.Net;
using System.Text.RegularExpressions;
using System.Windows.Input;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System.Collections.ObjectModel;

namespace DevUtilities.ViewModels;

public partial class IpQueryViewModel : ObservableObject
{
    private readonly HttpClient _httpClient;

    [ObservableProperty]
    private string inputIp = "";

    [ObservableProperty]
    private string currentIp = "";

    [ObservableProperty]
    private IpInfo? ipInfo;

    [ObservableProperty]
    private bool isLoading = false;

    [ObservableProperty]
    private string errorMessage = "";

    [ObservableProperty]
    private bool hasError = false;

    [ObservableProperty]
    private string validationMessage = "";

    [ObservableProperty]
    private bool isValidIp = true;

    public ObservableCollection<IpInfo> QueryHistory { get; } = new();

    public IpQueryViewModel()
    {
        _httpClient = new HttpClient();
        _httpClient.Timeout = TimeSpan.FromSeconds(10);
        
        // 获取当前IP
        _ = Task.Run(async () =>
        {
            try
            {
                CurrentIp = await GetCurrentIpAsync();
            }
            catch (Exception ex)
            {
                // 静默处理错误，不影响UI初始化
                System.Diagnostics.Debug.WriteLine($"获取当前IP失败: {ex.Message}");
            }
        });
    }

    [RelayCommand]
    private async Task QueryIp()
    {
        var ip = string.IsNullOrWhiteSpace(InputIp) ? CurrentIp : InputIp.Trim();
        
        if (string.IsNullOrWhiteSpace(ip))
        {
            ShowError("请输入IP地址或获取当前IP");
            return;
        }

        if (!IsValidIpAddress(ip))
        {
            ShowError("请输入有效的IP地址");
            return;
        }

        try
        {
            IsLoading = true;
            HasError = false;
            ErrorMessage = "";

            var info = await QueryIpInfoAsync(ip);
            if (info != null)
            {
                IpInfo = info;
                
                // 添加到历史记录
                var existingIndex = -1;
                for (int i = 0; i < QueryHistory.Count; i++)
                {
                    if (QueryHistory[i].Ip == info.Ip)
                    {
                        existingIndex = i;
                        break;
                    }
                }
                
                if (existingIndex >= 0)
                {
                    QueryHistory.RemoveAt(existingIndex);
                }
                
                QueryHistory.Insert(0, info);
                
                // 为新添加的IpInfo订阅SelectRequested事件
                info.SelectRequested += OnIpInfoSelectRequested;
                
                // 限制历史记录数量
                while (QueryHistory.Count > 10)
                {
                    QueryHistory.RemoveAt(QueryHistory.Count - 1);
                }
            }
        }
        catch (Exception ex)
        {
            ShowError($"查询失败: {ex.Message}");
        }
        finally
        {
            IsLoading = false;
        }
    }

    [RelayCommand]
    private async Task GetCurrentIp()
    {
        try
        {
            IsLoading = true;
            HasError = false;
            ErrorMessage = "";

            CurrentIp = await GetCurrentIpAsync();
            
            if (!string.IsNullOrWhiteSpace(CurrentIp))
            {
                InputIp = CurrentIp;
            }
        }
        catch (Exception ex)
        {
            ShowError($"获取当前IP失败: {ex.Message}");
        }
        finally
        {
            IsLoading = false;
        }
    }

    [RelayCommand]
    private void SelectFromHistory(IpInfo info)
    {
        if (info != null)
        {
            InputIp = info.Ip;
            IpInfo = info;
        }
    }
    
    private void OnIpInfoSelectRequested(IpInfo ipInfo)
    {
        SelectFromHistory(ipInfo);
    }

    [RelayCommand]
    private void ClearHistory()
    {
        QueryHistory.Clear();
    }

    [RelayCommand]
    private void ClearAll()
    {
        InputIp = "";
        IpInfo = null;
        ErrorMessage = "";
        HasError = false;
        ValidationMessage = "";
        IsValidIp = true;
    }

    private async Task<string> GetCurrentIpAsync()
    {
        try
        {
            // 尝试多个服务获取当前IP
            var services = new[]
            {
                "https://api.ipify.org",
                "https://icanhazip.com",
                "https://ipecho.net/plain"
            };

            foreach (var service in services)
            {
                try
                {
                    var response = await _httpClient.GetStringAsync(service);
                    var ip = response.Trim();
                    if (IsValidIpAddress(ip))
                    {
                        return ip;
                    }
                }
                catch
                {
                    // 尝试下一个服务
                    continue;
                }
            }
            
            return "";
        }
        catch
        {
            return "";
        }
    }

    private async Task<IpInfo?> QueryIpInfoAsync(string ip)
    {
        try
        {
            // 使用免费的IP地理位置API (使用HTTP协议，因为HTTPS需要付费)
            var url = $"http://ip-api.com/json/{ip}?fields=status,message,country,countryCode,region,regionName,city,zip,lat,lon,timezone,isp,org,as,query";
            
            var response = await _httpClient.GetStringAsync(url);
            var jsonDoc = JsonDocument.Parse(response);
            var root = jsonDoc.RootElement;

            if (root.GetProperty("status").GetString() == "success")
            {
                return new IpInfo
                {
                    Ip = root.GetProperty("query").GetString() ?? ip,
                    Country = root.TryGetProperty("country", out var country) ? country.GetString() ?? "" : "",
                    CountryCode = root.TryGetProperty("countryCode", out var countryCode) ? countryCode.GetString() ?? "" : "",
                    Region = root.TryGetProperty("regionName", out var regionName) ? regionName.GetString() ?? "" : "",
                    City = root.TryGetProperty("city", out var city) ? city.GetString() ?? "" : "",
                    ZipCode = root.TryGetProperty("zip", out var zip) ? zip.GetString() ?? "" : "",
                    Latitude = root.TryGetProperty("lat", out var lat) ? lat.GetDouble() : 0,
                    Longitude = root.TryGetProperty("lon", out var lon) ? lon.GetDouble() : 0,
                    Timezone = root.TryGetProperty("timezone", out var timezone) ? timezone.GetString() ?? "" : "",
                    Isp = root.TryGetProperty("isp", out var isp) ? isp.GetString() ?? "" : "",
                    Organization = root.TryGetProperty("org", out var org) ? org.GetString() ?? "" : "",
                    AsNumber = root.TryGetProperty("as", out var asNum) ? asNum.GetString() ?? "" : "",
                    QueryTime = DateTime.Now
                };
            }
            else
            {
                var message = root.TryGetProperty("message", out var msg) ? msg.GetString() : "查询失败";
                throw new Exception(message);
            }
        }
        catch (HttpRequestException)
        {
            throw new Exception("网络连接失败，请检查网络连接");
        }
        catch (JsonException)
        {
            throw new Exception("响应数据格式错误");
        }
    }

    private bool IsValidIpAddress(string ip)
    {
        if (string.IsNullOrWhiteSpace(ip))
            return false;

        // IPv4 正则表达式
        var ipv4Pattern = @"^((25[0-5]|2[0-4][0-9]|[01]?[0-9][0-9]?)\.){3}(25[0-5]|2[0-4][0-9]|[01]?[0-9][0-9]?)$";
        
        // IPv6 正则表达式（简化版）
        var ipv6Pattern = @"^([0-9a-fA-F]{1,4}:){7}[0-9a-fA-F]{1,4}$|^::1$|^::$";

        return Regex.IsMatch(ip, ipv4Pattern) || Regex.IsMatch(ip, ipv6Pattern) || IPAddress.TryParse(ip, out _);
    }

    private void ShowError(string message)
    {
        ErrorMessage = message;
        HasError = true;
    }

    partial void OnInputIpChanged(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            IsValidIp = true;
            ValidationMessage = "";
            return;
        }

        IsValidIp = IsValidIpAddress(value.Trim());
        ValidationMessage = IsValidIp ? "" : "请输入有效的IP地址";
        
        if (HasError && IsValidIp)
        {
            HasError = false;
            ErrorMessage = "";
        }
    }
}

public class IpInfo
{
    public string Ip { get; set; } = "";
    public string Country { get; set; } = "";
    public string CountryCode { get; set; } = "";
    public string Region { get; set; } = "";
    public string City { get; set; } = "";
    public string ZipCode { get; set; } = "";
    public double Latitude { get; set; }
    public double Longitude { get; set; }
    public string Timezone { get; set; } = "";
    public string Isp { get; set; } = "";
    public string Organization { get; set; } = "";
    public string AsNumber { get; set; } = "";
    public DateTime QueryTime { get; set; }

    public string Location => $"{City}, {Region}, {Country}";
    public string Coordinates => $"{Latitude:F6}, {Longitude:F6}";
    public string DisplayText => $"{Ip} - {Location}";
    
    public ICommand SelectCommand { get; }

    public IpInfo()
    {
        SelectCommand = new RelayCommand(() => 
        {
            // 通过事件通知父级ViewModel选择此项
            SelectRequested?.Invoke(this);
        });
    }

    public event Action<IpInfo>? SelectRequested;
}