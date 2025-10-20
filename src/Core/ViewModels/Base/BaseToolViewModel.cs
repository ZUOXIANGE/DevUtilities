using System;
using System.Collections.ObjectModel;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using DevUtilities.Models;

namespace DevUtilities.Core.ViewModels.Base;

/// <summary>
/// 工具ViewModel的基类
/// </summary>
public abstract partial class BaseToolViewModel : BaseViewModel
{
    #region 工具基本信息

    /// <summary>
    /// 工具标题
    /// </summary>
    [ObservableProperty]
    private string title = string.Empty;

    /// <summary>
    /// 工具描述
    /// </summary>
    [ObservableProperty]
    private string description = string.Empty;

    /// <summary>
    /// 工具图标
    /// </summary>
    [ObservableProperty]
    private string icon = string.Empty;

    /// <summary>
    /// 工具类型
    /// </summary>
    [ObservableProperty]
    private ToolType toolType;

    #endregion

    #region 历史记录

    /// <summary>
    /// 操作历史记录
    /// </summary>
    public ObservableCollection<HistoryItem> History { get; } = new();

    /// <summary>
    /// 最大历史记录数量
    /// </summary>
    [ObservableProperty]
    private int maxHistoryCount = 50;

    /// <summary>
    /// 是否启用历史记录
    /// </summary>
    [ObservableProperty]
    private bool enableHistory = true;

    #endregion

    #region 通用工具命令

    /// <summary>
    /// 重置工具命令
    /// </summary>
    public IRelayCommand ResetToolCommand { get; }

    /// <summary>
    /// 清除历史记录命令
    /// </summary>
    public IRelayCommand ClearHistoryCommand { get; }

    /// <summary>
    /// 导出结果命令
    /// </summary>
    public IAsyncRelayCommand ExportResultCommand { get; }

    /// <summary>
    /// 导入数据命令
    /// </summary>
    public IAsyncRelayCommand ImportDataCommand { get; }

    #endregion

    #region 构造函数

    protected BaseToolViewModel()
    {
        ResetToolCommand = new RelayCommand(ResetTool);
        ClearHistoryCommand = new RelayCommand(ClearHistory);
        ExportResultCommand = new AsyncRelayCommand(ExportResultAsync);
        ImportDataCommand = new AsyncRelayCommand(ImportDataAsync);
    }

    protected BaseToolViewModel(string title, string description, string icon, ToolType toolType) : this()
    {
        Title = title;
        Description = description;
        Icon = icon;
        ToolType = toolType;
    }

    #endregion

    #region 历史记录管理

    /// <summary>
    /// 添加历史记录
    /// </summary>
    /// <param name="input">输入内容</param>
    /// <param name="output">输出内容</param>
    /// <param name="operation">操作名称</param>
    protected virtual void AddToHistory(string input, string output, string operation = "")
    {
        if (!EnableHistory) return;

        var historyItem = new HistoryItem
        {
            Timestamp = DateTime.Now,
            Input = input,
            Output = output,
            Operation = operation
        };

        // 在UI线程上添加历史记录
        History.Insert(0, historyItem);

        // 限制历史记录数量
        while (History.Count > MaxHistoryCount)
        {
            History.RemoveAt(History.Count - 1);
        }
    }

    /// <summary>
    /// 清除历史记录
    /// </summary>
    protected virtual void ClearHistory()
    {
        History.Clear();
        SetSuccess("历史记录已清除");
    }

    #endregion

    #region 工具操作

    /// <summary>
    /// 重置工具状态
    /// </summary>
    protected virtual void ResetTool()
    {
        Reset();
        OnResetTool();
        SetSuccess("工具已重置");
    }

    /// <summary>
    /// 子类重写此方法实现具体的工具重置逻辑
    /// </summary>
    protected virtual void OnResetTool()
    {
    }

    #endregion

    #region 导入导出

    /// <summary>
    /// 导出结果
    /// </summary>
    protected virtual async Task ExportResultAsync()
    {
        await ExecuteAsync(async () =>
        {
            var result = await OnExportResultAsync();
            if (!string.IsNullOrEmpty(result))
            {
                SetSuccess("导出成功");
            }
        });
    }

    /// <summary>
    /// 子类重写此方法实现具体的导出逻辑
    /// </summary>
    /// <returns>导出的文件路径或结果</returns>
    protected virtual Task<string> OnExportResultAsync()
    {
        return Task.FromResult(string.Empty);
    }

    /// <summary>
    /// 导入数据
    /// </summary>
    protected virtual async Task ImportDataAsync()
    {
        await ExecuteAsync(async () =>
        {
            var success = await OnImportDataAsync();
            if (success)
            {
                SetSuccess("导入成功");
            }
        });
    }

    /// <summary>
    /// 子类重写此方法实现具体的导入逻辑
    /// </summary>
    /// <returns>是否导入成功</returns>
    protected virtual Task<bool> OnImportDataAsync()
    {
        return Task.FromResult(false);
    }

    #endregion

    #region 重写基类方法

    protected override void OnReset()
    {
        base.OnReset();
        // 工具特定的重置逻辑可以在OnResetTool中实现
    }

    protected override void OnDispose()
    {
        base.OnDispose();
        History.Clear();
    }

    #endregion
}

/// <summary>
/// 历史记录项
/// </summary>
public class HistoryItem
{
    /// <summary>
    /// 时间戳
    /// </summary>
    public DateTime Timestamp { get; set; }

    /// <summary>
    /// 输入内容
    /// </summary>
    public string Input { get; set; } = string.Empty;

    /// <summary>
    /// 输出内容
    /// </summary>
    public string Output { get; set; } = string.Empty;

    /// <summary>
    /// 操作名称
    /// </summary>
    public string Operation { get; set; } = string.Empty;

    /// <summary>
    /// 格式化的时间显示
    /// </summary>
    public string TimeDisplay => Timestamp.ToString("HH:mm:ss");

    /// <summary>
    /// 格式化的日期显示
    /// </summary>
    public string DateDisplay => Timestamp.ToString("yyyy-MM-dd");
}