using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using DevUtilities.Models;

namespace DevUtilities.ViewModels;

public partial class MainWindowViewModel : ObservableObject
{
    [ObservableProperty]
    private string title = "DevUtilities - 开发者工具集";

    [ObservableProperty]
    private bool isToolViewVisible = false;

    [ObservableProperty]
    private object? currentToolViewModel;

    [ObservableProperty]
    private ToolInfo? selectedTool;

    [ObservableProperty]
    private string searchText = string.Empty;

    [ObservableProperty]
    private string selectedSortOption = "默认排序";

    public ObservableCollection<ToolInfo> AllTools { get; } = new();
    public ObservableCollection<ToolInfo> FilteredTools { get; } = new();
    public ObservableCollection<string> SortOptions { get; } = new() { "默认排序", "按名称排序", "按类型排序", "自定义排序" };

    public MainWindowViewModel()
    {
        InitializeTools();
        UpdateFilteredTools();
    }

    partial void OnSearchTextChanged(string value)
    {
        UpdateFilteredTools();
    }

    partial void OnSelectedSortOptionChanged(string value)
    {
        UpdateFilteredTools();
    }

    private void UpdateFilteredTools()
    {
        var filtered = AllTools.AsEnumerable();

        // 应用搜索过滤
        if (!string.IsNullOrWhiteSpace(SearchText))
        {
            var searchLower = SearchText.ToLower();
            filtered = filtered.Where(tool => 
                tool.DisplayName.ToLower().Contains(searchLower) ||
                tool.Name.ToLower().Contains(searchLower) ||
                tool.Description.ToLower().Contains(searchLower));
        }

        // 应用排序
        filtered = SelectedSortOption switch
        {
            "按名称排序" => filtered.OrderBy(t => t.DisplayName),
            "按类型排序" => filtered.OrderBy(t => t.Type.ToString()),
            "自定义排序" => filtered.OrderBy(t => t.SortOrder).ThenBy(t => t.DisplayName),
            _ => filtered // 默认排序保持原有顺序
        };

        FilteredTools.Clear();
        foreach (var tool in filtered)
        {
            FilteredTools.Add(tool);
        }
    }

    private void InitializeTools()
    {
        AllTools.Add(new ToolInfo("Timestamp Converter", "时间戳转换", "⏰", "时间戳与日期格式互转", ToolType.TimestampConverter) { SortOrder = 1 });
        AllTools.Add(new ToolInfo("Unit Converter", "单位转换", "📏", "各种单位间的转换", ToolType.UnitConverter) { SortOrder = 2 });
        AllTools.Add(new ToolInfo("Base Converter", "进制转换", "🔢", "二进制、八进制、十进制、十六进制转换", ToolType.BaseConverter) { SortOrder = 3 });
        AllTools.Add(new ToolInfo("JSON Formatter", "JSON格式化", "📋", "JSON数据格式化和验证", ToolType.JsonFormatter) { SortOrder = 4 });
        AllTools.Add(new ToolInfo("SQL Formatter", "SQL格式化", "🗃️", "SQL查询语句格式化", ToolType.SqlFormatter) { SortOrder = 5 });
        AllTools.Add(new ToolInfo("HTML Formatter", "HTML格式化", "🌐", "HTML代码格式化", ToolType.HtmlFormatter) { SortOrder = 6 });
        AllTools.Add(new ToolInfo("Base64 Encoder", "Base64编码", "🔐", "Base64编码解码", ToolType.Base64Encoder) { SortOrder = 7 });
        AllTools.Add(new ToolInfo("Hex Converter", "十六进制转换", "🔤", "十六进制与字符串互转", ToolType.HexConverter) { SortOrder = 8 });
        AllTools.Add(new ToolInfo("JWT Encoder", "JWT编码", "🎫", "JWT令牌编码解码", ToolType.JwtEncoder) { SortOrder = 9 });
        AllTools.Add(new ToolInfo("Regex Tester", "正则测试", "🔍", "正则表达式测试工具", ToolType.RegexTester) { SortOrder = 10 });
        AllTools.Add(new ToolInfo("UUID Generator", "UUID生成", "🆔", "生成各种格式的UUID", ToolType.UuidGenerator) { SortOrder = 11 });
        AllTools.Add(new ToolInfo("Password Generator", "密码生成器", "🔑", "生成安全密码", ToolType.PasswordGenerator) { SortOrder = 12 });
        AllTools.Add(new ToolInfo("URL Tools", "URL工具", "🔗", "URL编码解码和解析", ToolType.UrlTools) { SortOrder = 13 });
        AllTools.Add(new ToolInfo("HTTP Request", "HTTP请求", "🌍", "HTTP客户端工具", ToolType.HttpRequest) { SortOrder = 14 });
        AllTools.Add(new ToolInfo("IP Query", "IP查询", "🌐", "IP地址查询和地理位置", ToolType.IpQuery) { SortOrder = 15 });
        AllTools.Add(new ToolInfo("QR Code", "二维码", "📱", "二维码生成和扫描", ToolType.QrCode) { SortOrder = 16 });
        AllTools.Add(new ToolInfo("Parquet Viewer", "Parquet查看器", "📊", "查看Parquet文件数据", ToolType.ParquetViewer) { SortOrder = 17 });
        AllTools.Add(new ToolInfo("Crypto Tools", "加密工具", "🔒", "各种加密和哈希工具", ToolType.CryptoTools) { SortOrder = 18 });
        AllTools.Add(new ToolInfo("Cron Expression", "Cron表达式", "⏲️", "Cron表达式解析和生成", ToolType.CronExpression) { SortOrder = 19 });
    }

    [RelayCommand]
    private void SelectTool(ToolInfo tool)
    {
        SelectedTool = tool;
        CurrentToolViewModel = CreateToolViewModel(tool.Type);
        IsToolViewVisible = true;
    }

    [RelayCommand]
    private void BackToHome()
    {
        IsToolViewVisible = false;
        CurrentToolViewModel = null;
        SelectedTool = null;
    }

    private object CreateToolViewModel(ToolType toolType)
    {
        return toolType switch
        {
            ToolType.TimestampConverter => new TimestampConverterViewModel(),
            ToolType.UnitConverter => new UnitConverterViewModel(),
            ToolType.BaseConverter => new BaseConverterViewModel(),
            ToolType.JsonFormatter => new JsonFormatterViewModel(),
            ToolType.SqlFormatter => new SqlFormatterViewModel(),
            ToolType.HtmlFormatter => new HtmlFormatterViewModel(),
            ToolType.Base64Encoder => new Base64EncoderViewModel(),
            ToolType.HexConverter => new HexConverterViewModel(),
            ToolType.JwtEncoder => new JwtEncoderViewModel(),
            ToolType.RegexTester => new RegexTesterViewModel(),
            ToolType.UuidGenerator => new UuidGeneratorViewModel(),
            ToolType.PasswordGenerator => new PasswordGeneratorViewModel(),
            ToolType.UrlTools => new UrlToolsViewModel(),
            ToolType.HttpRequest => new HttpRequestViewModel(),
            ToolType.IpQuery => new IpQueryViewModel(),
            ToolType.QrCode => new QrCodeViewModel(),
            ToolType.ParquetViewer => new ParquetViewerViewModel(),
            ToolType.CryptoTools => new CryptoToolsViewModel(),
            ToolType.CronExpression => new CronExpressionViewModel(),
            _ => new object()
        };
    }
}