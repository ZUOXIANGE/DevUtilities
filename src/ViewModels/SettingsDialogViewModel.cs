using System;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using DevUtilities.Core.Services;

namespace DevUtilities.ViewModels;

/// <summary>
/// 设置对话框ViewModel
/// </summary>
public partial class SettingsDialogViewModel : ObservableObject
{
    private readonly UserSettingsService _settingsService;

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
    /// 自动格式化
    /// </summary>
    [ObservableProperty]
    private bool autoFormat = false;

    /// <summary>
    /// 压缩输出
    /// </summary>
    [ObservableProperty]
    private bool compactOutput = false;

    #endregion

    #region 性能设置

    /// <summary>
    /// 最大文件大小（MB）
    /// </summary>
    [ObservableProperty]
    private int maxFileSize = 10;

    /// <summary>
    /// 最大处理时间（秒）
    /// </summary>
    [ObservableProperty]
    private int maxProcessingTime = 30;

    /// <summary>
    /// 启用内存监控
    /// </summary>
    [ObservableProperty]
    private bool enableMemoryMonitoring = true;

    /// <summary>
    /// 内存阈值（MB）
    /// </summary>
    [ObservableProperty]
    private int memoryThreshold = 100;

    #endregion

    #region 对话框结果

    /// <summary>
    /// 对话框结果
    /// </summary>
    public bool DialogResult { get; private set; }
    
    /// <summary>
    /// 关闭请求事件
    /// </summary>
    public event EventHandler<bool>? CloseRequested;

    #endregion

    #region 构造函数

    public SettingsDialogViewModel()
    {
        _settingsService = new UserSettingsService();
        LoadSettings();
    }

    #endregion

    #region 命令

    /// <summary>
    /// 确定命令
    /// </summary>
    [RelayCommand]
    private void Ok()
    {
        SaveSettings();
        DialogResult = true;
        CloseRequested?.Invoke(this, true);
    }

    /// <summary>
    /// 取消命令
    /// </summary>
    [RelayCommand]
    private void Cancel()
    {
        DialogResult = false;
        CloseRequested?.Invoke(this, false);
    }

    /// <summary>
    /// 重置为默认值命令
    /// </summary>
    [RelayCommand]
    private void ResetToDefault()
    {
        try
        {
            _settingsService.ResetToDefaults();
            LoadSettings();
        }
        catch (Exception ex)
        {
            // 这里可以显示错误消息
            System.Diagnostics.Debug.WriteLine($"重置设置失败: {ex.Message}");
        }
    }

    /// <summary>
    /// 导出设置命令
    /// </summary>
    [RelayCommand]
    private async Task ExportSettings()
    {
        try
        {
            // 这里应该打开文件保存对话框
            // 暂时使用固定路径
            var filePath = System.IO.Path.Combine(
                Environment.GetFolderPath(Environment.SpecialFolder.Desktop),
                $"DevUtilities_Settings_{DateTime.Now:yyyyMMdd_HHmmss}.json");

            await _settingsService.ExportSettingsAsync(filePath);
            
            // 这里可以显示成功消息
            System.Diagnostics.Debug.WriteLine($"设置已导出到: {filePath}");
        }
        catch (Exception ex)
        {
            // 这里可以显示错误消息
            System.Diagnostics.Debug.WriteLine($"导出设置失败: {ex.Message}");
        }
    }

    /// <summary>
    /// 导入设置命令
    /// </summary>
    [RelayCommand]
    private async Task ImportSettings()
    {
        try
        {
            // 这里应该打开文件选择对话框
            // 暂时跳过文件选择
            System.Diagnostics.Debug.WriteLine("导入设置功能需要文件选择对话框");
        }
        catch (Exception ex)
        {
            // 这里可以显示错误消息
            System.Diagnostics.Debug.WriteLine($"导入设置失败: {ex.Message}");
        }
    }

    #endregion

    #region 私有方法

    /// <summary>
    /// 加载设置
    /// </summary>
    private void LoadSettings()
    {
        try
        {
            // 加载格式化选项
            IndentSize = _settingsService.GetSetting(SettingsKeys.Formatter.IndentSize, 2);
            UseTabsForIndent = _settingsService.GetSetting(SettingsKeys.Formatter.UseTabsForIndent, false);
            AutoFormat = _settingsService.GetSetting(SettingsKeys.Formatter.AutoFormat, false);
            CompactOutput = _settingsService.GetSetting(SettingsKeys.Formatter.CompactOutput, false);

            // 加载性能设置
            MaxFileSize = _settingsService.GetSetting(SettingsKeys.Performance.MaxFileSize, 10);
            MaxProcessingTime = _settingsService.GetSetting(SettingsKeys.Performance.MaxProcessingTime, 30);
            EnableMemoryMonitoring = _settingsService.GetSetting(SettingsKeys.Performance.EnableMemoryMonitoring, true);
            MemoryThreshold = _settingsService.GetSetting(SettingsKeys.Performance.MemoryThreshold, 100);
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"加载设置失败: {ex.Message}");
        }
    }

    /// <summary>
    /// 保存设置
    /// </summary>
    private void SaveSettings()
    {
        try
        {
            // 保存格式化选项
            _settingsService.SetSetting(SettingsKeys.Formatter.IndentSize, IndentSize);
            _settingsService.SetSetting(SettingsKeys.Formatter.UseTabsForIndent, UseTabsForIndent);
            _settingsService.SetSetting(SettingsKeys.Formatter.AutoFormat, AutoFormat);
            _settingsService.SetSetting(SettingsKeys.Formatter.CompactOutput, CompactOutput);

            // 保存性能设置
            _settingsService.SetSetting(SettingsKeys.Performance.MaxFileSize, MaxFileSize);
            _settingsService.SetSetting(SettingsKeys.Performance.MaxProcessingTime, MaxProcessingTime);
            _settingsService.SetSetting(SettingsKeys.Performance.EnableMemoryMonitoring, EnableMemoryMonitoring);
            _settingsService.SetSetting(SettingsKeys.Performance.MemoryThreshold, MemoryThreshold);

            // 异步保存到文件
            _ = Task.Run(async () =>
            {
                try
                {
                    await _settingsService.SaveSettingsAsync();
                }
                catch (Exception ex)
                {
                    System.Diagnostics.Debug.WriteLine($"保存设置到文件失败: {ex.Message}");
                }
            });
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"保存设置失败: {ex.Message}");
        }
    }

    #endregion
}