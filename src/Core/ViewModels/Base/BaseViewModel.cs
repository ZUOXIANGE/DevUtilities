using System;
using System.Threading.Tasks;
using Avalonia;
using Avalonia.Controls.ApplicationLifetimes;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

namespace DevUtilities.Core.ViewModels.Base;

/// <summary>
/// 所有ViewModel的基类，提供通用功能
/// </summary>
public abstract partial class BaseViewModel : ObservableObject, IDisposable
{
    #region 通用属性

    /// <summary>
    /// 是否正在加载
    /// </summary>
    [ObservableProperty]
    private bool isLoading = false;

    /// <summary>
    /// 错误消息
    /// </summary>
    [ObservableProperty]
    private string errorMessage = string.Empty;

    /// <summary>
    /// 是否有错误
    /// </summary>
    [ObservableProperty]
    private bool hasError = false;

    /// <summary>
    /// 成功消息
    /// </summary>
    [ObservableProperty]
    private string successMessage = string.Empty;

    /// <summary>
    /// 是否有成功消息
    /// </summary>
    [ObservableProperty]
    private bool hasSuccess = false;

    /// <summary>
    /// 是否已初始化
    /// </summary>
    [ObservableProperty]
    private bool isInitialized = false;

    #endregion

    #region 通用命令

    /// <summary>
    /// 复制到剪贴板命令
    /// </summary>
    public IAsyncRelayCommand<string> CopyToClipboardCommand { get; }

    /// <summary>
    /// 清除错误命令
    /// </summary>
    public IRelayCommand ClearErrorCommand { get; }

    /// <summary>
    /// 清除成功消息命令
    /// </summary>
    public IRelayCommand ClearSuccessCommand { get; }

    #endregion

    #region 构造函数

    protected BaseViewModel()
    {
        CopyToClipboardCommand = new AsyncRelayCommand<string>(CopyToClipboardAsync);
        ClearErrorCommand = new RelayCommand(ClearError);
        ClearSuccessCommand = new RelayCommand(ClearSuccess);
    }

    #endregion

    #region 错误处理

    /// <summary>
    /// 设置错误信息
    /// </summary>
    /// <param name="message">错误消息</param>
    protected virtual void SetError(string message)
    {
        ErrorMessage = message;
        HasError = !string.IsNullOrWhiteSpace(message);
        
        if (HasError)
        {
            ClearSuccess();
        }
    }

    /// <summary>
    /// 设置错误信息（从异常）
    /// </summary>
    /// <param name="exception">异常对象</param>
    protected virtual void SetError(Exception exception)
    {
        SetError(exception?.Message ?? "发生未知错误");
    }

    /// <summary>
    /// 清除错误信息
    /// </summary>
    protected virtual void ClearError()
    {
        ErrorMessage = string.Empty;
        HasError = false;
    }

    #endregion

    #region 成功消息处理

    /// <summary>
    /// 设置成功消息
    /// </summary>
    /// <param name="message">成功消息</param>
    protected virtual void SetSuccess(string message)
    {
        SuccessMessage = message;
        HasSuccess = !string.IsNullOrWhiteSpace(message);
        
        if (HasSuccess)
        {
            ClearError();
        }
    }

    /// <summary>
    /// 清除成功消息
    /// </summary>
    protected virtual void ClearSuccess()
    {
        SuccessMessage = string.Empty;
        HasSuccess = false;
    }

    #endregion

    #region 加载状态管理

    /// <summary>
    /// 执行异步操作并管理加载状态
    /// </summary>
    /// <param name="operation">要执行的异步操作</param>
    /// <param name="clearMessages">是否清除之前的消息</param>
    protected async Task ExecuteAsync(Func<Task> operation, bool clearMessages = true)
    {
        if (IsLoading) return;

        try
        {
            IsLoading = true;
            
            if (clearMessages)
            {
                ClearError();
                ClearSuccess();
            }

            await operation();
        }
        catch (Exception ex)
        {
            SetError(ex);
        }
        finally
        {
            IsLoading = false;
        }
    }

    /// <summary>
    /// 执行异步操作并管理加载状态（带返回值）
    /// </summary>
    /// <typeparam name="T">返回值类型</typeparam>
    /// <param name="operation">要执行的异步操作</param>
    /// <param name="clearMessages">是否清除之前的消息</param>
    /// <returns>操作结果</returns>
    protected async Task<T?> ExecuteAsync<T>(Func<Task<T>> operation, bool clearMessages = true)
    {
        if (IsLoading) return default;

        try
        {
            IsLoading = true;
            
            if (clearMessages)
            {
                ClearError();
                ClearSuccess();
            }

            return await operation();
        }
        catch (Exception ex)
        {
            SetError(ex);
            return default;
        }
        finally
        {
            IsLoading = false;
        }
    }

    #endregion

    #region 剪贴板操作

    /// <summary>
    /// 复制文本到剪贴板
    /// </summary>
    /// <param name="text">要复制的文本</param>
    protected async Task CopyToClipboardAsync(string? text)
    {
        if (string.IsNullOrEmpty(text)) return;

        try
        {
            if (Application.Current?.ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
            {
                var clipboard = desktop.MainWindow?.Clipboard;
                if (clipboard != null)
                {
                    await clipboard.SetTextAsync(text);
                    SetSuccess("已复制到剪贴板");
                    
                    // 3秒后自动清除成功消息
                    _ = Task.Delay(3000).ContinueWith(_ => ClearSuccess());
                }
            }
        }
        catch (Exception ex)
        {
            SetError($"复制失败: {ex.Message}");
        }
    }

    #endregion

    #region 虚拟方法

    /// <summary>
    /// 初始化ViewModel
    /// </summary>
    public virtual async Task InitializeAsync()
    {
        if (IsInitialized) return;

        await ExecuteAsync(async () =>
        {
            await OnInitializeAsync();
            IsInitialized = true;
        });
    }

    /// <summary>
    /// 子类重写此方法实现具体的初始化逻辑
    /// </summary>
    protected virtual Task OnInitializeAsync()
    {
        return Task.CompletedTask;
    }

    /// <summary>
    /// 重置ViewModel状态
    /// </summary>
    public virtual void Reset()
    {
        ClearError();
        ClearSuccess();
        IsLoading = false;
        OnReset();
    }

    /// <summary>
    /// 子类重写此方法实现具体的重置逻辑
    /// </summary>
    protected virtual void OnReset()
    {
    }

    #endregion

    #region IDisposable

    private bool _disposed = false;

    /// <summary>
    /// 释放资源
    /// </summary>
    public void Dispose()
    {
        Dispose(true);
        GC.SuppressFinalize(this);
    }

    /// <summary>
    /// 释放资源的具体实现
    /// </summary>
    /// <param name="disposing">是否正在释放托管资源</param>
    protected virtual void Dispose(bool disposing)
    {
        if (!_disposed && disposing)
        {
            OnDispose();
            _disposed = true;
        }
    }

    /// <summary>
    /// 子类重写此方法实现具体的资源释放逻辑
    /// </summary>
    protected virtual void OnDispose()
    {
    }

    #endregion
}