using System;
using System.Collections.Generic;
using Avalonia.Input;

namespace DevUtilities.Core.Services;

/// <summary>
/// 键盘快捷键服务
/// </summary>
public class KeyboardShortcutService
{
    private readonly Dictionary<KeyGesture, Action> _shortcuts = new();
    private readonly Dictionary<string, KeyGesture> _namedShortcuts = new();

    /// <summary>
    /// 注册快捷键
    /// </summary>
    /// <param name="name">快捷键名称</param>
    /// <param name="key">按键</param>
    /// <param name="modifiers">修饰键</param>
    /// <param name="action">执行的操作</param>
    public void RegisterShortcut(string name, Key key, KeyModifiers modifiers, Action action)
    {
        var gesture = new KeyGesture(key, modifiers);
        _shortcuts[gesture] = action;
        _namedShortcuts[name] = gesture;
    }

    /// <summary>
    /// 注册快捷键（使用KeyGesture）
    /// </summary>
    /// <param name="name">快捷键名称</param>
    /// <param name="gesture">键盘手势</param>
    /// <param name="action">执行的操作</param>
    public void RegisterShortcut(string name, KeyGesture gesture, Action action)
    {
        _shortcuts[gesture] = action;
        _namedShortcuts[name] = gesture;
    }

    /// <summary>
    /// 取消注册快捷键
    /// </summary>
    /// <param name="name">快捷键名称</param>
    public void UnregisterShortcut(string name)
    {
        if (_namedShortcuts.TryGetValue(name, out var gesture))
        {
            _shortcuts.Remove(gesture);
            _namedShortcuts.Remove(name);
        }
    }

    /// <summary>
    /// 处理按键事件
    /// </summary>
    /// <param name="e">按键事件参数</param>
    /// <returns>是否处理了快捷键</returns>
    public bool HandleKeyDown(KeyEventArgs e)
    {
        var gesture = new KeyGesture(e.Key, e.KeyModifiers);
        
        if (_shortcuts.TryGetValue(gesture, out var action))
        {
            try
            {
                action.Invoke();
                e.Handled = true;
                return true;
            }
            catch (Exception ex)
            {
                // 记录错误但不抛出异常
                System.Diagnostics.Debug.WriteLine($"快捷键执行错误: {ex.Message}");
            }
        }

        return false;
    }

    /// <summary>
    /// 获取快捷键描述
    /// </summary>
    /// <param name="name">快捷键名称</param>
    /// <returns>快捷键描述文本</returns>
    public string GetShortcutDescription(string name)
    {
        if (_namedShortcuts.TryGetValue(name, out var gesture))
        {
            return FormatKeyGesture(gesture);
        }
        return string.Empty;
    }

    /// <summary>
    /// 获取所有已注册的快捷键
    /// </summary>
    /// <returns>快捷键字典</returns>
    public Dictionary<string, string> GetAllShortcuts()
    {
        var result = new Dictionary<string, string>();
        foreach (var kvp in _namedShortcuts)
        {
            result[kvp.Key] = FormatKeyGesture(kvp.Value);
        }
        return result;
    }

    /// <summary>
    /// 格式化键盘手势为可读文本
    /// </summary>
    /// <param name="gesture">键盘手势</param>
    /// <returns>格式化后的文本</returns>
    private string FormatKeyGesture(KeyGesture gesture)
    {
        var parts = new List<string>();

        if (gesture.KeyModifiers.HasFlag(KeyModifiers.Control))
            parts.Add("Ctrl");
        if (gesture.KeyModifiers.HasFlag(KeyModifiers.Alt))
            parts.Add("Alt");
        if (gesture.KeyModifiers.HasFlag(KeyModifiers.Shift))
            parts.Add("Shift");
        if (gesture.KeyModifiers.HasFlag(KeyModifiers.Meta))
            parts.Add("Win");

        parts.Add(gesture.Key.ToString());

        return string.Join("+", parts);
    }

    /// <summary>
    /// 清除所有快捷键
    /// </summary>
    public void ClearAll()
    {
        _shortcuts.Clear();
        _namedShortcuts.Clear();
    }

    /// <summary>
    /// 检查快捷键是否已注册
    /// </summary>
    /// <param name="name">快捷键名称</param>
    /// <returns>是否已注册</returns>
    public bool IsShortcutRegistered(string name)
    {
        return _namedShortcuts.ContainsKey(name);
    }

    /// <summary>
    /// 检查键盘手势是否已被使用
    /// </summary>
    /// <param name="gesture">键盘手势</param>
    /// <returns>是否已被使用</returns>
    public bool IsGestureInUse(KeyGesture gesture)
    {
        return _shortcuts.ContainsKey(gesture);
    }
}

/// <summary>
/// 常用快捷键定义
/// </summary>
public static class CommonShortcuts
{
    // 应用程序级别快捷键
    public static readonly KeyGesture NewFile = new(Key.N, KeyModifiers.Control);
    public static readonly KeyGesture OpenFile = new(Key.O, KeyModifiers.Control);
    public static readonly KeyGesture SaveFile = new(Key.S, KeyModifiers.Control);
    public static readonly KeyGesture SaveAsFile = new(Key.S, KeyModifiers.Control | KeyModifiers.Shift);
    public static readonly KeyGesture Exit = new(Key.F4, KeyModifiers.Alt);
    
    // 编辑快捷键
    public static readonly KeyGesture Undo = new(Key.Z, KeyModifiers.Control);
    public static readonly KeyGesture Redo = new(Key.Y, KeyModifiers.Control);
    public static readonly KeyGesture Cut = new(Key.X, KeyModifiers.Control);
    public static readonly KeyGesture Copy = new(Key.C, KeyModifiers.Control);
    public static readonly KeyGesture Paste = new(Key.V, KeyModifiers.Control);
    public static readonly KeyGesture SelectAll = new(Key.A, KeyModifiers.Control);
    public static readonly KeyGesture Find = new(Key.F, KeyModifiers.Control);
    public static readonly KeyGesture Replace = new(Key.H, KeyModifiers.Control);
    
    // 格式化工具快捷键
    public static readonly KeyGesture Format = new(Key.F, KeyModifiers.Control | KeyModifiers.Shift);
    public static readonly KeyGesture Compress = new(Key.M, KeyModifiers.Control | KeyModifiers.Shift);
    public static readonly KeyGesture Validate = new(Key.V, KeyModifiers.Control | KeyModifiers.Shift);
    public static readonly KeyGesture Clear = new(Key.Delete, KeyModifiers.Control);
    public static readonly KeyGesture Swap = new(Key.S, KeyModifiers.Control | KeyModifiers.Alt);
    
    // 视图快捷键
    public static readonly KeyGesture ZoomIn = new(Key.OemPlus, KeyModifiers.Control);
    public static readonly KeyGesture ZoomOut = new(Key.OemMinus, KeyModifiers.Control);
    public static readonly KeyGesture ZoomReset = new(Key.D0, KeyModifiers.Control);
    public static readonly KeyGesture FullScreen = new(Key.F11, KeyModifiers.None);
    
    // 工具快捷键
    public static readonly KeyGesture Settings = new(Key.OemComma, KeyModifiers.Control);
    public static readonly KeyGesture Help = new(Key.F1, KeyModifiers.None);
    public static readonly KeyGesture About = new(Key.F1, KeyModifiers.Shift);
    
    // 新增的快捷键
    public static readonly KeyGesture UseExample = new(Key.E, KeyModifiers.Control | KeyModifiers.Shift);
    public static readonly KeyGesture CancelOperation = new(Key.Escape, KeyModifiers.None);
}