using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using DevUtilities.Models;

namespace DevUtilities.Core.ViewModels.Base;

/// <summary>
/// 格式化工具ViewModel的基类
/// </summary>
public abstract partial class BaseFormatterViewModel : BaseToolViewModel
{
    #region 输入输出

    /// <summary>
    /// 输入文本
    /// </summary>
    [ObservableProperty]
    private string inputText = string.Empty;

    /// <summary>
    /// 输出文本
    /// </summary>
    [ObservableProperty]
    private string outputText = string.Empty;

    /// <summary>
    /// 输入占位符文本
    /// </summary>
    [ObservableProperty]
    private string inputPlaceholder = "请输入要格式化的内容...";

    /// <summary>
    /// 输出占位符文本
    /// </summary>
    [ObservableProperty]
    private string outputPlaceholder = "格式化结果将显示在这里...";

    #endregion

    #region 格式化选项

    /// <summary>
    /// 缩进大小
    /// </summary>
    [ObservableProperty]
    private int indentSize = 2;

    /// <summary>
    /// 使用制表符缩进
    /// </summary>
    [ObservableProperty]
    private bool useTabsForIndent = false;

    /// <summary>
    /// 自动格式化（输入时实时格式化）
    /// </summary>
    [ObservableProperty]
    private bool autoFormat = false;

    /// <summary>
    /// 压缩输出（移除不必要的空白）
    /// </summary>
    [ObservableProperty]
    private bool compactOutput = false;

    #endregion

    #region 统计信息

    /// <summary>
    /// 输入字符数
    /// </summary>
    [ObservableProperty]
    private int inputCharCount;

    /// <summary>
    /// 输入行数
    /// </summary>
    [ObservableProperty]
    private int inputLineCount;

    /// <summary>
    /// 输出字符数
    /// </summary>
    [ObservableProperty]
    private int outputCharCount;

    /// <summary>
    /// 输出行数
    /// </summary>
    [ObservableProperty]
    private int outputLineCount;

    #endregion

    #region 验证状态

    /// <summary>
    /// 是否验证通过
    /// </summary>
    [ObservableProperty]
    private bool isValid = false;

    /// <summary>
    /// 验证消息
    /// </summary>
    [ObservableProperty]
    private string validationMessage = string.Empty;

    #endregion

    #region 字段和属性

    private readonly ObservableCollection<HistoryItem> history = new();
    private readonly DevUtilities.Core.Services.StreamProcessingService? streamProcessor;
    private CancellationTokenSource? cancellationTokenSource;
    private readonly DevUtilities.Core.Services.KeyboardShortcutService? shortcutService;
    private readonly DevUtilities.Core.Services.UserSettingsService? settingsService;

    #endregion

    #region 格式化命令

    /// <summary>
    /// 格式化命令
    /// </summary>
    public IAsyncRelayCommand FormatCommand { get; }

    /// <summary>
    /// 压缩命令
    /// </summary>
    public IAsyncRelayCommand CompressCommand { get; }

    /// <summary>
    /// 验证命令
    /// </summary>
    public IAsyncRelayCommand ValidateCommand { get; }

    /// <summary>
    /// 交换输入输出命令
    /// </summary>
    public IRelayCommand SwapInputOutputCommand { get; }

    /// <summary>
    /// 复制输出命令
    /// </summary>
    public IAsyncRelayCommand CopyCommand { get; }

    /// <summary>
    /// 清除命令
    /// </summary>
    public IRelayCommand ClearCommand { get; }

    /// <summary>
    /// 使用示例命令
    /// </summary>
    public IRelayCommand UseExampleCommand { get; }

    /// <summary>
    /// 取消操作命令
    /// </summary>
    public IRelayCommand CancelCommand { get; }

    /// <summary>
    /// 内存信息
    /// </summary>
    [ObservableProperty]
    private string memoryInfo = string.Empty;

    #endregion

    #region 构造函数

    protected BaseFormatterViewModel() : base()
    {
        FormatCommand = new AsyncRelayCommand(FormatAsync);
        CompressCommand = new AsyncRelayCommand(CompressAsync);
        ValidateCommand = new AsyncRelayCommand(ValidateAsync);
        SwapInputOutputCommand = new RelayCommand(SwapInputOutput);
        CopyCommand = new AsyncRelayCommand(CopyOutputAsync);
        ClearCommand = new RelayCommand(ClearAll);
        UseExampleCommand = new RelayCommand(UseExample);
        CancelCommand = new RelayCommand(CancelOperation);
        
        // 初始化流式处理服务
        streamProcessor = new DevUtilities.Core.Services.StreamProcessingService();
        
        // 初始化键盘快捷键服务
        shortcutService = new DevUtilities.Core.Services.KeyboardShortcutService();
        RegisterShortcuts();
        
        // 初始化用户设置服务
        settingsService = new DevUtilities.Core.Services.UserSettingsService();
        LoadUserSettings();
        
        // 监听属性变化
        PropertyChanged += OnPropertyChanged;

        // 监听输入文本变化
        PropertyChanged += OnPropertyChanged;
        
        // 启动内存监控
        StartMemoryMonitoring();
    }

    protected BaseFormatterViewModel(string title, string description, string icon) 
        : base(title, description, icon, ToolType.JsonFormatter)
    {
        FormatCommand = new AsyncRelayCommand(FormatAsync);
        CompressCommand = new AsyncRelayCommand(CompressAsync);
        ValidateCommand = new AsyncRelayCommand(ValidateAsync);
        SwapInputOutputCommand = new RelayCommand(SwapInputOutput);
        CopyCommand = new AsyncRelayCommand(CopyOutputAsync);
        ClearCommand = new RelayCommand(ClearAll);
        UseExampleCommand = new RelayCommand(UseExample);
        CancelCommand = new RelayCommand(CancelOperation);

        // 初始化键盘快捷键服务
        shortcutService = new DevUtilities.Core.Services.KeyboardShortcutService();
        RegisterShortcuts();

        // 初始化用户设置服务
        settingsService = new DevUtilities.Core.Services.UserSettingsService();
        LoadUserSettings();

        // 监听输入文本变化
        PropertyChanged += OnPropertyChanged;
        
        // 启动内存监控
        StartMemoryMonitoring();
    }

    #endregion

    #region 事件处理

    private void OnPropertyChanged(object? sender, System.ComponentModel.PropertyChangedEventArgs e)
    {
        switch (e.PropertyName)
        {
            case nameof(InputText):
                UpdateInputStatistics();
                // 实时验证输入内容
                _ = ValidateInputRealTimeAsync();
                if (AutoFormat && !string.IsNullOrEmpty(InputText))
                {
                    _ = FormatAsync();
                }
                break;
            case nameof(OutputText):
                UpdateOutputStatistics();
                break;
            case nameof(IndentSize):
            case nameof(UseTabsForIndent):
            case nameof(AutoFormat):
            case nameof(CompactOutput):
                // 格式化选项变化时自动保存设置
                SaveUserSettings();
                break;
        }
    }

    #endregion

    #region 统计信息更新

    /// <summary>
    /// 更新输入统计信息
    /// </summary>
    private void UpdateInputStatistics()
    {
        if (string.IsNullOrEmpty(InputText))
        {
            InputCharCount = 0;
            InputLineCount = 0;
        }
        else
        {
            InputCharCount = InputText.Length;
            InputLineCount = InputText.Split('\n').Length;
        }
    }

    /// <summary>
    /// 更新输出统计信息
    /// </summary>
    private void UpdateOutputStatistics()
    {
        if (string.IsNullOrEmpty(OutputText))
        {
            OutputCharCount = 0;
            OutputLineCount = 0;
        }
        else
        {
            OutputCharCount = OutputText.Length;
            OutputLineCount = OutputText.Split('\n').Length;
        }
    }

    #endregion

    #region 实时验证

    /// <summary>
    /// 实时验证输入内容
    /// </summary>
    private async Task ValidateInputRealTimeAsync()
    {
        if (string.IsNullOrWhiteSpace(InputText))
        {
            IsValid = false;
            ValidationMessage = string.Empty;
            return;
        }

        try
        {
            var result = await OnValidateAsync(InputText);
            IsValid = result.IsValid;
            ValidationMessage = result.Message;
        }
        catch (Exception ex)
        {
            IsValid = false;
            ValidationMessage = $"验证出错: {ex.Message}";
        }
    }

    #endregion

    #region 格式化操作

    /// <summary>
    /// 格式化文本
    /// </summary>
    public async Task FormatAsync()
    {
        if (string.IsNullOrWhiteSpace(InputText))
        {
            SetError("请输入要格式化的内容");
            return;
        }

        await ExecuteAsync(async () =>
        {
            var result = await OnFormatAsync(InputText);
            if (!string.IsNullOrEmpty(result))
            {
                OutputText = result;
                AddToHistory(InputText, OutputText, "格式化");
                SetSuccess("格式化完成");
            }
        });
    }

    /// <summary>
    /// 压缩文本
    /// </summary>
    public async Task CompressAsync()
    {
        if (string.IsNullOrWhiteSpace(InputText))
        {
            SetError("请输入要压缩的内容");
            return;
        }

        await ExecuteAsync(async () =>
        {
            var result = await OnCompressAsync(InputText);
            if (!string.IsNullOrEmpty(result))
            {
                OutputText = result;
                AddToHistory(InputText, OutputText, "压缩");
                SetSuccess("压缩完成");
            }
        });
    }

    /// <summary>
    /// 验证文本格式
    /// </summary>
    public async Task ValidateAsync()
    {
        if (string.IsNullOrWhiteSpace(InputText))
        {
            SetError("请输入要验证的内容");
            return;
        }

        await ExecuteAsync(async () =>
        {
            var result = await OnValidateAsync(InputText);
            IsValid = result.IsValid;
            ValidationMessage = result.Message;
            
            if (result.IsValid)
            {
                SetSuccess($"验证通过{(string.IsNullOrEmpty(result.Message) ? "" : $": {result.Message}")}");
            }
            else
            {
                SetError($"验证失败: {result.Message}");
            }
        });
    }

    /// <summary>
    /// 交换输入输出内容
    /// </summary>
    public void SwapInputOutput()
    {
        if (string.IsNullOrEmpty(OutputText))
        {
            SetError("输出内容为空，无法交换");
            return;
        }

        var temp = InputText;
        InputText = OutputText;
        OutputText = temp;
        SetSuccess("输入输出已交换");
    }

    #endregion

    #region 抽象方法 - 子类实现

    /// <summary>
    /// 格式化操作
    /// </summary>
    protected virtual async Task<string> OnFormatAsync(string input)
    {
        if (string.IsNullOrWhiteSpace(input))
            return string.Empty;

        try
        {
            // 检查文件大小和性能
            var inputSize = System.Text.Encoding.UTF8.GetByteCount(input);
            var startTime = DateTime.Now;

            // 创建取消令牌
            cancellationTokenSource?.Cancel();
            cancellationTokenSource = new CancellationTokenSource();

            // 如果文件过大，使用流式处理
            if (inputSize > 1024 * 1024) // 1MB
            {
                ValidationMessage = $"文件较大（{inputSize / (1024 * 1024):F1}MB），使用流式处理...";
                OnPropertyChanged(nameof(ValidationMessage));

                var result = await streamProcessor!.ProcessInChunksAsync(
                    input,
                    FormatContentAsync,
                    progress: (percentage, status) =>
                    {
                        ValidationMessage = $"{status} ({percentage}%)";
                        OnPropertyChanged(nameof(ValidationMessage));
                    },
                    cancellationToken: cancellationTokenSource.Token);

                var processingTime = (DateTime.Now - startTime).TotalMilliseconds;
                ValidationMessage = $"处理完成，耗时 {processingTime:F0}ms";
                OnPropertyChanged(nameof(ValidationMessage));

                return result;
            }
            else
            {
                // 小文件直接处理
                var result = await FormatContentAsync(input);
                var processingTime = (DateTime.Now - startTime).TotalMilliseconds;
                
                // 检查处理时间
                if (processingTime > 5000) // 5秒
                {
                    var sizeText = inputSize > 1024 * 1024 ? $"{inputSize / (1024 * 1024):F1}MB" : $"{inputSize / 1024:F0}KB";
                    throw new DevUtilities.Core.Exceptions.PerformanceException(
                        $"处理超时：文件大小 {sizeText}，处理时间 {processingTime:F0}ms。建议减小文件大小或分段处理。");
                }

                return result;
            }
        }
        catch (OperationCanceledException)
        {
            ValidationMessage = "操作已取消";
            OnPropertyChanged(nameof(ValidationMessage));
            return string.Empty;
        }
        catch (Exception ex)
        {
            var errorInfo = DevUtilities.Core.Services.ErrorHandlingService.HandleException(ex);
            ValidationMessage = errorInfo.UserMessage;
            IsValid = false;
            OnPropertyChanged(nameof(ValidationMessage));
            OnPropertyChanged(nameof(IsValid));
            
            // 记录技术错误信息（可以添加日志记录）
            System.Diagnostics.Debug.WriteLine($"Formatting error: {errorInfo.TechnicalMessage}");
            
            return string.Empty;
        }
    }

    /// <summary>
    /// 子类实现具体的格式化逻辑
    /// </summary>
    protected abstract Task<string> FormatContentAsync(string input);

    /// <summary>
    /// 子类实现具体的压缩逻辑
    /// </summary>
    /// <param name="input">输入文本</param>
    /// <returns>压缩后的文本</returns>
    protected virtual Task<string> OnCompressAsync(string input)
    {
        // 默认实现：移除多余空白
        var lines = input.Split('\n')
            .Select(line => line.Trim())
            .Where(line => !string.IsNullOrEmpty(line));
        
        return Task.FromResult(string.Join("", lines));
    }

    /// <summary>
    /// 子类实现具体的验证逻辑
    /// </summary>
    /// <param name="input">输入文本</param>
    /// <returns>验证结果</returns>
    protected virtual Task<ValidationResult> OnValidateAsync(string input)
    {
        // 默认实现：检查是否为空
        var isValid = !string.IsNullOrWhiteSpace(input);
        var message = isValid ? "内容不为空" : "内容为空";
        
        return Task.FromResult(new ValidationResult(isValid, message));
    }

    #endregion

    #region 新增命令方法

    /// <summary>
    /// 复制输出内容
    /// </summary>
    protected async Task CopyOutputAsync()
    {
        if (string.IsNullOrEmpty(OutputText))
        {
            SetError("没有可复制的内容");
            return;
        }

        await CopyToClipboardAsync(OutputText);
    }

    /// <summary>
    /// 清除所有内容
    /// </summary>
    protected virtual void ClearAll()
    {
        InputText = string.Empty;
        OutputText = string.Empty;
        IsValid = false;
        ValidationMessage = string.Empty;
        ClearError();
        ClearSuccess();
        SetSuccess("已清除所有内容");
    }

    /// <summary>
    /// 使用示例数据
    /// </summary>
    protected virtual void UseExample()
    {
        var example = GetExampleData();
        if (!string.IsNullOrEmpty(example))
        {
            InputText = example;
            SetSuccess("已加载示例数据");
        }
    }

    /// <summary>
    /// 取消当前操作
    /// </summary>
    protected virtual void CancelOperation()
    {
        try
        {
            cancellationTokenSource?.Cancel();
            ValidationMessage = "操作已取消";
            SetSuccess("操作已取消");
        }
        catch (Exception ex)
        {
            SetError($"取消操作失败: {ex.Message}");
        }
    }

    /// <summary>
    /// 启动内存监控
    /// </summary>
    private void StartMemoryMonitoring()
    {
        _ = Task.Run(async () =>
        {
            while (true)
            {
                try
                {
                    var memoryUsed = GC.GetTotalMemory(false);
                    var memoryMB = memoryUsed / (1024.0 * 1024.0);
                    
                    MemoryInfo = $"内存使用: {memoryMB:F1}MB";
                    OnPropertyChanged(nameof(MemoryInfo));
                    
                    // 如果内存使用超过阈值，触发垃圾回收
                    if (memoryMB > 100) // 100MB阈值
                    {
                        GC.Collect();
                        GC.WaitForPendingFinalizers();
                        GC.Collect();
                    }
                    
                    await Task.Delay(2000); // 每2秒更新一次
                }
                catch
                {
                    // 忽略监控错误
                    break;
                }
            }
        });
    }

    /// <summary>
    /// 获取示例数据 - 子类可重写
    /// </summary>
    /// <returns>示例数据</returns>
    protected virtual string GetExampleData()
    {
        return string.Empty;
    }

    /// <summary>
    /// 注册键盘快捷键
    /// </summary>
    private void RegisterShortcuts()
    {
        shortcutService?.RegisterShortcut("Format", DevUtilities.Core.Services.CommonShortcuts.Format, () => FormatCommand?.Execute(null));
        shortcutService?.RegisterShortcut("Compress", DevUtilities.Core.Services.CommonShortcuts.Compress, () => CompressCommand?.Execute(null));
        shortcutService?.RegisterShortcut("Validate", DevUtilities.Core.Services.CommonShortcuts.Validate, () => ValidateCommand?.Execute(null));
        shortcutService?.RegisterShortcut("SwapInputOutput", DevUtilities.Core.Services.CommonShortcuts.Swap, () => SwapInputOutputCommand?.Execute(null));
        shortcutService?.RegisterShortcut("CopyOutput", DevUtilities.Core.Services.CommonShortcuts.Copy, () => CopyCommand?.Execute(null));
        shortcutService?.RegisterShortcut("ClearAll", DevUtilities.Core.Services.CommonShortcuts.Clear, () => ClearCommand?.Execute(null));
        shortcutService?.RegisterShortcut("UseExample", DevUtilities.Core.Services.CommonShortcuts.UseExample, () => UseExampleCommand?.Execute(null));
        shortcutService?.RegisterShortcut("CancelOperation", DevUtilities.Core.Services.CommonShortcuts.CancelOperation, () => CancelCommand?.Execute(null));
    }

    #endregion

    #region 用户设置管理

    /// <summary>
    /// 加载用户设置
    /// </summary>
    private void LoadUserSettings()
    {
        try
        {
            // 加载格式化选项设置
            IndentSize = settingsService!.GetSetting(DevUtilities.Core.Services.SettingsKeys.Formatter.IndentSize, 2);
            UseTabsForIndent = settingsService!.GetSetting(DevUtilities.Core.Services.SettingsKeys.Formatter.UseTabsForIndent, false);
            AutoFormat = settingsService!.GetSetting(DevUtilities.Core.Services.SettingsKeys.Formatter.AutoFormat, false);
            CompactOutput = settingsService!.GetSetting(DevUtilities.Core.Services.SettingsKeys.Formatter.CompactOutput, false);

            // 加载性能设置
            var maxFileSize = settingsService!.GetSetting(DevUtilities.Core.Services.SettingsKeys.Performance.MaxFileSize, 10);
            var maxProcessingTime = settingsService!.GetSetting(DevUtilities.Core.Services.SettingsKeys.Performance.MaxProcessingTime, 30);
            var enableMemoryMonitoring = settingsService!.GetSetting(DevUtilities.Core.Services.SettingsKeys.Performance.EnableMemoryMonitoring, true);

            // 应用性能设置到流处理服务
            if (streamProcessor != null)
            {
                // 这里可以配置流处理服务的参数
                System.Diagnostics.Debug.WriteLine($"Performance settings loaded: MaxFileSize={maxFileSize}MB, MaxProcessingTime={maxProcessingTime}s");
            }
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Failed to load user settings: {ex.Message}");
        }
    }

    /// <summary>
    /// 保存用户设置
    /// </summary>
    private void SaveUserSettings()
    {
        try
        {
            // 保存格式化选项设置
            settingsService!.SetSetting(DevUtilities.Core.Services.SettingsKeys.Formatter.IndentSize, IndentSize);
            settingsService!.SetSetting(DevUtilities.Core.Services.SettingsKeys.Formatter.UseTabsForIndent, UseTabsForIndent);
            settingsService!.SetSetting(DevUtilities.Core.Services.SettingsKeys.Formatter.AutoFormat, AutoFormat);
            settingsService!.SetSetting(DevUtilities.Core.Services.SettingsKeys.Formatter.CompactOutput, CompactOutput);

            // 异步保存设置到文件
            _ = Task.Run(async () =>
            {
                try
                {
                    await settingsService.SaveSettingsAsync();
                }
                catch (Exception ex)
                {
                    System.Diagnostics.Debug.WriteLine($"Failed to save settings to file: {ex.Message}");
                }
            });
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Failed to save user settings: {ex.Message}");
        }
    }

    /// <summary>
    /// 重置设置到默认值
    /// </summary>
    public void ResetSettingsToDefault()
    {
        try
        {
            settingsService!.ResetToDefaults();
            LoadUserSettings();
            SetSuccess("设置已重置为默认值");
        }
        catch (Exception ex)
        {
            SetError($"重置设置失败: {ex.Message}");
        }
    }

    /// <summary>
    /// 导出设置到文件
    /// </summary>
    public async Task<bool> ExportSettingsAsync(string filePath)
    {
        try
        {
            await settingsService!.ExportSettingsAsync(filePath);
            SetSuccess("设置已导出");
            return true;
        }
        catch (Exception ex)
        {
            SetError($"导出设置失败: {ex.Message}");
            return false;
        }
    }

    /// <summary>
    /// 从文件导入设置
    /// </summary>
    public async Task<bool> ImportSettingsAsync(string filePath)
    {
        try
        {
            await settingsService!.ImportSettingsAsync(filePath);
            LoadUserSettings();
            SetSuccess("设置已导入");
            return true;
        }
        catch (Exception ex)
        {
            SetError($"导入设置失败: {ex.Message}");
            return false;
        }
    }

    #endregion

    #region 重写基类方法

    protected override void OnResetTool()
    {
        base.OnResetTool();
        InputText = string.Empty;
        OutputText = string.Empty;
        IndentSize = 2;
        UseTabsForIndent = false;
        AutoFormat = false;
        CompactOutput = false;
        IsValid = false;
        ValidationMessage = string.Empty;
    }

    protected override async Task<string> OnExportResultAsync()
    {
        if (string.IsNullOrEmpty(OutputText))
        {
            SetError("没有可导出的结果");
            return string.Empty;
        }

        // 这里可以实现文件保存逻辑
        // 暂时返回输出内容
        await CopyToClipboardAsync(OutputText);
        return "已复制到剪贴板";
    }

    protected override void OnDispose()
    {
        base.OnDispose();
        PropertyChanged -= OnPropertyChanged;
    }

    #endregion
}

/// <summary>
/// 验证结果
/// </summary>
public class ValidationResult
{
    /// <summary>
    /// 是否验证通过
    /// </summary>
    public bool IsValid { get; }

    /// <summary>
    /// 验证消息
    /// </summary>
    public string Message { get; }

    public ValidationResult(bool isValid, string message)
    {
        IsValid = isValid;
        Message = message;
    }
}