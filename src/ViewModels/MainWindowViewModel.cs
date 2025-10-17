using System.Collections.ObjectModel;
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

    public ObservableCollection<ToolInfo> Tools { get; } = new();

    public MainWindowViewModel()
    {
        InitializeTools();
    }

    private void InitializeTools()
    {
        Tools.Add(new ToolInfo("AI Chat", "智能AI助手", "💬", "与AI进行对话，获取开发帮助", ToolType.AiChat));
        Tools.Add(new ToolInfo("AI Translate", "AI翻译", "🌐", "专业翻译工具，支持多种模式", ToolType.AiTranslate));
        Tools.Add(new ToolInfo("Timestamp Converter", "时间戳转换", "⏰", "时间戳与日期格式互转", ToolType.TimestampConverter));
        Tools.Add(new ToolInfo("Unit Converter", "单位转换", "📏", "各种单位间的转换", ToolType.UnitConverter));
        Tools.Add(new ToolInfo("Base Converter", "进制转换", "🔢", "二进制、八进制、十进制、十六进制转换", ToolType.BaseConverter));
        Tools.Add(new ToolInfo("JSON Formatter", "JSON格式化", "📋", "JSON数据格式化和验证", ToolType.JsonFormatter));
        Tools.Add(new ToolInfo("SQL Formatter", "SQL格式化", "🗃️", "SQL查询语句格式化", ToolType.SqlFormatter));
        Tools.Add(new ToolInfo("HTML Formatter", "HTML格式化", "🌐", "HTML代码格式化", ToolType.HtmlFormatter));
        Tools.Add(new ToolInfo("Base64 Encoder", "Base64编码", "🔐", "Base64编码解码", ToolType.Base64Encoder));
        Tools.Add(new ToolInfo("Hex Converter", "十六进制转换", "🔤", "十六进制与字符串互转", ToolType.HexConverter));
        Tools.Add(new ToolInfo("JWT Encoder", "JWT编码", "🎫", "JWT令牌编码解码", ToolType.JwtEncoder));
        Tools.Add(new ToolInfo("Regex Tester", "正则测试", "🔍", "正则表达式测试工具", ToolType.RegexTester));
        Tools.Add(new ToolInfo("UUID Generator", "UUID生成", "🆔", "生成各种格式的UUID", ToolType.UuidGenerator));
        Tools.Add(new ToolInfo("Password Generator", "密码生成器", "🔑", "生成安全密码", ToolType.PasswordGenerator));
        Tools.Add(new ToolInfo("URL Tools", "URL工具", "🔗", "URL编码解码和解析", ToolType.UrlTools));
        Tools.Add(new ToolInfo("HTTP Request", "HTTP请求", "🌍", "HTTP客户端工具", ToolType.HttpRequest));
        Tools.Add(new ToolInfo("IP Query", "IP查询", "🌐", "IP地址查询和地理位置", ToolType.IpQuery));
        Tools.Add(new ToolInfo("QR Code", "二维码", "📱", "二维码生成和扫描", ToolType.QrCode));
        Tools.Add(new ToolInfo("Parquet Viewer", "Parquet查看器", "📊", "查看Parquet文件数据", ToolType.ParquetViewer));
        Tools.Add(new ToolInfo("Crypto Tools", "加密工具", "🔒", "各种加密和哈希工具", ToolType.CryptoTools));
        Tools.Add(new ToolInfo("Cron Expression", "Cron表达式", "⏲️", "Cron表达式解析和生成", ToolType.CronExpression));
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
            ToolType.AiChat => new AiChatViewModel(),
            ToolType.AiTranslate => new AiTranslateViewModel(),
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