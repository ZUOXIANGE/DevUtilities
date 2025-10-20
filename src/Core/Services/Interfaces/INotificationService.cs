using System;
using System.Threading.Tasks;

namespace DevUtilities.Core.Services.Interfaces;

/// <summary>
/// 通知服务接口
/// </summary>
public interface INotificationService
{
    /// <summary>
    /// 显示成功通知
    /// </summary>
    /// <param name="message">通知消息</param>
    /// <param name="title">通知标题</param>
    /// <param name="duration">显示时长（毫秒），0表示不自动关闭</param>
    Task ShowSuccessAsync(string message, string title = "成功", int duration = 3000);

    /// <summary>
    /// 显示错误通知
    /// </summary>
    /// <param name="message">通知消息</param>
    /// <param name="title">通知标题</param>
    /// <param name="duration">显示时长（毫秒），0表示不自动关闭</param>
    Task ShowErrorAsync(string message, string title = "错误", int duration = 5000);

    /// <summary>
    /// 显示警告通知
    /// </summary>
    /// <param name="message">通知消息</param>
    /// <param name="title">通知标题</param>
    /// <param name="duration">显示时长（毫秒），0表示不自动关闭</param>
    Task ShowWarningAsync(string message, string title = "警告", int duration = 4000);

    /// <summary>
    /// 显示信息通知
    /// </summary>
    /// <param name="message">通知消息</param>
    /// <param name="title">通知标题</param>
    /// <param name="duration">显示时长（毫秒），0表示不自动关闭</param>
    Task ShowInfoAsync(string message, string title = "信息", int duration = 3000);

    /// <summary>
    /// 显示确认对话框
    /// </summary>
    /// <param name="message">对话框消息</param>
    /// <param name="title">对话框标题</param>
    /// <param name="confirmText">确认按钮文本</param>
    /// <param name="cancelText">取消按钮文本</param>
    /// <returns>用户是否确认</returns>
    Task<bool> ShowConfirmAsync(string message, string title = "确认", string confirmText = "确定", string cancelText = "取消");

    /// <summary>
    /// 显示输入对话框
    /// </summary>
    /// <param name="message">对话框消息</param>
    /// <param name="title">对话框标题</param>
    /// <param name="defaultValue">默认输入值</param>
    /// <param name="placeholder">输入框占位符</param>
    /// <returns>用户输入的内容，取消则返回null</returns>
    Task<string?> ShowInputAsync(string message, string title = "输入", string defaultValue = "", string placeholder = "");

    /// <summary>
    /// 显示进度通知
    /// </summary>
    /// <param name="message">进度消息</param>
    /// <param name="title">进度标题</param>
    /// <returns>进度通知的ID，用于更新进度</returns>
    Task<string> ShowProgressAsync(string message, string title = "处理中");

    /// <summary>
    /// 更新进度通知
    /// </summary>
    /// <param name="notificationId">通知ID</param>
    /// <param name="progress">进度百分比（0-100）</param>
    /// <param name="message">进度消息</param>
    Task UpdateProgressAsync(string notificationId, int progress, string message = "");

    /// <summary>
    /// 完成进度通知
    /// </summary>
    /// <param name="notificationId">通知ID</param>
    /// <param name="message">完成消息</param>
    /// <param name="isSuccess">是否成功完成</param>
    Task CompleteProgressAsync(string notificationId, string message = "完成", bool isSuccess = true);

    /// <summary>
    /// 关闭通知
    /// </summary>
    /// <param name="notificationId">通知ID</param>
    Task CloseNotificationAsync(string notificationId);

    /// <summary>
    /// 关闭所有通知
    /// </summary>
    Task CloseAllNotificationsAsync();
}

/// <summary>
/// 通知类型
/// </summary>
public enum NotificationType
{
    /// <summary>
    /// 信息
    /// </summary>
    Info,

    /// <summary>
    /// 成功
    /// </summary>
    Success,

    /// <summary>
    /// 警告
    /// </summary>
    Warning,

    /// <summary>
    /// 错误
    /// </summary>
    Error,

    /// <summary>
    /// 进度
    /// </summary>
    Progress
}