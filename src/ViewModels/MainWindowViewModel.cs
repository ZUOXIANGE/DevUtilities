using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using DevUtilities.Models;
using DevUtilities.Views;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.ApplicationLifetimes;
using Serilog;

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
        Log.Debug("[MainWindowViewModel] 开始初始化MainWindowViewModel");
        
        try
        {
            Log.Debug("[MainWindowViewModel] 开始初始化工具列表");
            InitializeTools();
            Log.Debug("[MainWindowViewModel] 工具列表初始化完成，总数: {ToolCount}", AllTools.Count);
            
            Log.Debug("[MainWindowViewModel] 开始更新过滤后的工具列表");
            UpdateFilteredTools();
            Log.Debug("[MainWindowViewModel] 过滤后的工具列表更新完成，显示数量: {FilteredCount}", FilteredTools.Count);
            
            Log.Information("[MainWindowViewModel] MainWindowViewModel初始化完成");
        }
        catch (Exception ex)
        {
            Log.Error(ex, "[MainWindowViewModel] MainWindowViewModel初始化时发生错误");
            throw;
        }
    }

    partial void OnSearchTextChanged(string value)
    {
        Log.Debug("[MainWindowViewModel] 搜索文本变化: '{SearchText}'", value);
        UpdateFilteredTools();
    }

    partial void OnSelectedSortOptionChanged(string value)
    {
        Log.Debug("[MainWindowViewModel] 排序选项变化: '{SortOption}'", value);
        UpdateFilteredTools();
    }

    private void UpdateFilteredTools()
    {
        Log.Debug("[MainWindowViewModel] 开始更新过滤工具列表");
        
        try
        {
            var filtered = AllTools.AsEnumerable();
            var originalCount = AllTools.Count;

            // 应用搜索过滤
            if (!string.IsNullOrWhiteSpace(SearchText))
            {
                var searchLower = SearchText.ToLower();
                filtered = filtered.Where(tool => 
                    tool.DisplayName.ToLower().Contains(searchLower) ||
                    tool.Name.ToLower().Contains(searchLower) ||
                    tool.Description.ToLower().Contains(searchLower));
                    
                Log.Debug("[MainWindowViewModel] 应用搜索过滤，关键词: '{SearchText}'", SearchText);
            }

            // 应用排序
            filtered = SelectedSortOption switch
            {
                "按名称排序" => filtered.OrderBy(t => t.DisplayName),
                "按类型排序" => filtered.OrderBy(t => t.Type.ToString()),
                "自定义排序" => filtered.OrderBy(t => t.SortOrder).ThenBy(t => t.DisplayName),
                _ => filtered // 默认排序保持原有顺序
            };
            
            Log.Debug("[MainWindowViewModel] 应用排序: {SortOption}", SelectedSortOption);

            FilteredTools.Clear();
            foreach (var tool in filtered)
            {
                FilteredTools.Add(tool);
            }
            
            Log.Debug("[MainWindowViewModel] 工具列表过滤完成，原始数量: {OriginalCount}，过滤后数量: {FilteredCount}", 
                originalCount, FilteredTools.Count);
        }
        catch (Exception ex)
        {
            Log.Error(ex, "[MainWindowViewModel] 更新过滤工具列表时发生错误");
        }
    }

    private void InitializeTools()
    {
        Log.Debug("[MainWindowViewModel] 开始添加工具到列表");
        
        AllTools.Add(new ToolInfo("时间戳转换", "时间戳转换", "⏰", "Unix时间戳与日期时间互转", ToolType.TimestampConverter));
        AllTools.Add(new ToolInfo("Base64编码", "Base64编码", "🔤", "Base64编码解码工具", ToolType.Base64Encoder));
        AllTools.Add(new ToolInfo("URL工具", "URL工具", "🔗", "URL编码解码和解析工具", ToolType.UrlTools));
        AllTools.Add(new ToolInfo("JSON格式化", "JSON格式化", "📋", "JSON格式化和验证工具", ToolType.JsonFormatter));
        AllTools.Add(new ToolInfo("密码生成", "密码生成", "🔑", "安全密码生成器", ToolType.PasswordGenerator));
        AllTools.Add(new ToolInfo("进制转换", "进制转换", "🔢", "数字进制转换工具", ToolType.BaseConverter));
        AllTools.Add(new ToolInfo("HTTP请求", "HTTP请求", "🌐", "HTTP请求测试工具", ToolType.HttpRequest));
        AllTools.Add(new ToolInfo("加密工具", "加密工具", "🔐", "文本加密解密工具", ToolType.CryptoTools));
        AllTools.Add(new ToolInfo("字符串转义", "字符串转义", "📝", "字符串转义和反转义", ToolType.StringEscape));
        AllTools.Add(new ToolInfo("SQL格式化", "SQL格式化", "🗃️", "SQL语句格式化工具", ToolType.SqlFormatter));
        AllTools.Add(new ToolInfo("HTML格式化", "HTML格式化", "🌐", "HTML代码格式化和美化", ToolType.HtmlFormatter));
        AllTools.Add(new ToolInfo("XML格式化", "XML格式化", "📄", "XML代码格式化和验证", ToolType.XmlFormatter));
        AllTools.Add(new ToolInfo("正则测试", "正则测试", "🔍", "正则表达式测试和验证", ToolType.RegexTester));
        AllTools.Add(new ToolInfo("文本对比", "文本对比", "📊", "文本差异对比工具", ToolType.TextDiff));
        AllTools.Add(new ToolInfo("二维码", "二维码", "📱", "二维码生成和识别", ToolType.QrCode));
        AllTools.Add(new ToolInfo("UUID生成", "UUID生成", "🆔", "UUID生成器", ToolType.UuidGenerator));
        AllTools.Add(new ToolInfo("颜色选择", "颜色选择", "🎨", "颜色选择和转换工具", ToolType.ColorPicker));
        AllTools.Add(new ToolInfo("十六进制", "十六进制", "🔢", "十六进制转换工具", ToolType.HexConverter));
        AllTools.Add(new ToolInfo("JWT编码", "JWT编码", "🔐", "JWT令牌编码解码", ToolType.JwtEncoder));
        AllTools.Add(new ToolInfo("单位转换", "单位转换", "📏", "各种单位转换工具", ToolType.UnitConverter));
        AllTools.Add(new ToolInfo("Cron表达式", "Cron表达式", "⏰", "Cron表达式生成和解析", ToolType.CronExpression));
        AllTools.Add(new ToolInfo("Parquet查看", "Parquet查看", "📄", "Parquet文件查看器", ToolType.ParquetViewer));
        AllTools.Add(new ToolInfo("IP查询", "IP查询", "🌍", "IP地址查询工具", ToolType.IpQuery));
        AllTools.Add(new ToolInfo("JSON示例生成", "JSON示例生成", "📄", "根据类定义生成JSON示例", ToolType.JsonExampleGenerator));
        AllTools.Add(new ToolInfo("哈希生成", "哈希生成", "🔐", "字符串哈希值生成工具", ToolType.HashGenerator));
        AllTools.Add(new ToolInfo("文本加解密", "文本加解密", "🔒", "AES/DES/3DES文本加解密工具", ToolType.TextEncryption));
        AllTools.Add(new ToolInfo("Docker Compose转换", "Docker Compose转换", "🐳", "Docker run命令转换为docker-compose文件", ToolType.DockerComposeConverter));
        AllTools.Add(new ToolInfo("chmod计算器", "chmod计算器", "🛡️", "Linux文件权限计算与转换", ToolType.ChmodCalculator));
        AllTools.Add(new ToolInfo("ULID生成", "ULID生成", "🆔", "ULID生成器", ToolType.UlidGenerator));
        AllTools.Add(new ToolInfo("Sqids生成", "Sqids生成", "🔤", "Sqids ID生成器", ToolType.SqlidsGenerator));
        AllTools.Add(new ToolInfo("JSON/YAML转换", "JSON/YAML转换", "🔄", "JSON和YAML格式互相转换工具", ToolType.JsonYamlConverter));
        
        Log.Information("[MainWindowViewModel] 工具列表初始化完成，共添加 {ToolCount} 个工具", AllTools.Count);
    }

    [RelayCommand]
    private void SelectTool(ToolInfo tool)
    {
        Log.Information("[MainWindowViewModel] 选择工具: {ToolName} ({ToolType})", tool.DisplayName, tool.Type);
        
        try
        {
            SelectedTool = tool;
            
            Log.Debug("[MainWindowViewModel] 开始创建工具ViewModel");
            CurrentToolViewModel = CreateToolViewModel(tool.Type);
            Log.Debug("[MainWindowViewModel] 工具ViewModel创建完成: {ViewModelType}", CurrentToolViewModel?.GetType().Name ?? "null");
            
            IsToolViewVisible = true;
            Log.Debug("[MainWindowViewModel] 工具视图已显示");
        }
        catch (Exception ex)
        {
            Log.Error(ex, "[MainWindowViewModel] 选择工具时发生错误: {ToolName}", tool.DisplayName);
        }
    }

    [RelayCommand]
    private void BackToHome()
    {
        Log.Information("[MainWindowViewModel] 返回主页");
        
        try
        {
            IsToolViewVisible = false;
            CurrentToolViewModel = null;
            SelectedTool = null;
            
            Log.Debug("[MainWindowViewModel] 已清理工具视图状态");
        }
        catch (Exception ex)
        {
            Log.Error(ex, "[MainWindowViewModel] 返回主页时发生错误");
        }
    }

    [RelayCommand]
    private async Task OpenSettings()
    {
        var settingsViewModel = new SettingsDialogViewModel();
        var settingsDialog = new SettingsDialog(settingsViewModel);
        
        // 设置对话框的父窗口（如果需要的话）
        // settingsDialog.ShowDialog(parentWindow);
        
        var mainWindow = GetMainWindow();
        if (mainWindow != null)
        {
            var result = await settingsDialog.ShowDialog<bool?>(mainWindow);
            
            if (result == true)
            {
                // 设置已保存，可以在这里处理设置更新后的逻辑
                // 例如通知其他ViewModel设置已更改
            }
        }
    }

    private Window? GetMainWindow()
    {
        if (Application.Current?.ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
        {
            return desktop.MainWindow;
        }
        return null;
    }

    private object CreateToolViewModel(ToolType toolType)
    {
        Log.Debug("[MainWindowViewModel] 创建工具ViewModel，类型: {ToolType}", toolType);
        
        try
        {
            var viewModel = toolType switch
            {
                ToolType.TimestampConverter => new TimestampConverterViewModel(),
                ToolType.Base64Encoder => new Base64EncoderViewModel(),
                ToolType.UrlTools => new UrlToolsViewModel(),
                ToolType.JsonFormatter => new JsonFormatterViewModel(),
                ToolType.PasswordGenerator => new PasswordGeneratorViewModel(),
                ToolType.BaseConverter => new BaseConverterViewModel(),
                ToolType.HttpRequest => new HttpRequestViewModel(),
                ToolType.CryptoTools => new CryptoToolsViewModel(),
                ToolType.StringEscape => new StringEscapeViewModel(),
                ToolType.SqlFormatter => new SqlFormatterViewModel(),
                ToolType.HtmlFormatter => new HtmlFormatterViewModel(),
                ToolType.XmlFormatter => new XmlFormatterViewModel(),
                ToolType.RegexTester => new RegexTesterViewModel(),
                ToolType.TextDiff => new TextDiffViewModel(),
                ToolType.QrCode => new QrCodeViewModel(),
                ToolType.UuidGenerator => new UuidGeneratorViewModel(),
                ToolType.ColorPicker => new ColorPickerViewModel(),
                ToolType.HexConverter => new HexConverterViewModel(),
                ToolType.JwtEncoder => new JwtEncoderViewModel(),
                ToolType.UnitConverter => new UnitConverterViewModel(),
                ToolType.CronExpression => new CronExpressionViewModel(),
                ToolType.ParquetViewer => new ParquetViewerViewModel(),
                ToolType.IpQuery => new IpQueryViewModel(),
                ToolType.JsonExampleGenerator => new JsonExampleGeneratorViewModel(),
                ToolType.HashGenerator => new HashGeneratorViewModel(),
                ToolType.TextEncryption => new TextEncryptionViewModel(),
                ToolType.DockerComposeConverter => new DockerComposeConverterViewModel(),
                ToolType.ChmodCalculator => new ChmodCalculatorViewModel(),
                ToolType.UlidGenerator => new UlidGeneratorViewModel(),
                ToolType.SqlidsGenerator => new SqlidsGeneratorViewModel(),
                ToolType.JsonYamlConverter => new JsonYamlConverterViewModel(),
                _ => new object()
            };
            
            Log.Debug("[MainWindowViewModel] ViewModel创建成功: {ViewModelType}", viewModel.GetType().Name);
            return viewModel;
        }
        catch (Exception ex)
        {
            Log.Error(ex, "[MainWindowViewModel] 创建工具ViewModel时发生错误: {ToolType}", toolType);
            return new object();
        }
    }
}