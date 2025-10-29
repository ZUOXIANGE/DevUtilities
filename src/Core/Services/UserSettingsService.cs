using System;
using System.Collections.Generic;
using System.IO;
using System.Text.Json;
using System.Threading.Tasks;

namespace DevUtilities.Core.Services;

/// <summary>
/// 用户设置服务
/// </summary>
public class UserSettingsService
{
    private readonly Dictionary<string, object> _settings;
    private readonly string _settingsFilePath;

    public UserSettingsService()
    {
        _settings = new Dictionary<string, object>();
        _settingsFilePath = Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
            "DevUtilities",
            "settings.json");
            
        // 确保目录存在
        var directory = Path.GetDirectoryName(_settingsFilePath);
        if (!string.IsNullOrEmpty(directory) && !Directory.Exists(directory))
        {
            Directory.CreateDirectory(directory);
        }
            
        // 加载现有设置
        _ = LoadSettingsAsync();
    }

    /// <summary>
    /// 获取设置值
    /// </summary>
    public T GetSetting<T>(string key, T defaultValue = default!)
    {
        if (_settings.TryGetValue(key, out var value))
        {
            try
            {
                if (value is JsonElement jsonElement)
                {
                    return JsonSerializer.Deserialize<T>(jsonElement.GetRawText()) ?? defaultValue;
                }
                return (T)value;
            }
            catch
            {
                return defaultValue;
            }
        }
        return defaultValue;
    }

    /// <summary>
    /// 设置值
    /// </summary>
    public void SetSetting<T>(string key, T value)
    {
        if (value != null)
        {
            _settings[key] = value;
        }
        else
        {
            _settings.Remove(key);
        }
    }

    /// <summary>
    /// 移除设置
    /// </summary>
    public bool RemoveSetting(string key)
    {
        return _settings.Remove(key);
    }

    /// <summary>
    /// 检查设置是否存在
    /// </summary>
    public bool HasSetting(string key)
    {
        return _settings.ContainsKey(key);
    }

    /// <summary>
    /// 保存设置到文件
    /// </summary>
    public async Task SaveSettingsAsync()
    {
        try
        {
            var json = JsonSerializer.Serialize(_settings, new JsonSerializerOptions
            {
                WriteIndented = true
            });
            await File.WriteAllTextAsync(_settingsFilePath, json);
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Failed to save settings: {ex.Message}");
        }
    }

    /// <summary>
    /// 从文件加载设置
    /// </summary>
    public async Task LoadSettingsAsync()
    {
        try
        {
            if (File.Exists(_settingsFilePath))
            {
                var json = await File.ReadAllTextAsync(_settingsFilePath);
                var loadedSettings = JsonSerializer.Deserialize<Dictionary<string, JsonElement>>(json);
                    
                if (loadedSettings != null)
                {
                    _settings.Clear();
                    foreach (var kvp in loadedSettings)
                    {
                        _settings[kvp.Key] = kvp.Value;
                    }
                }
            }
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Failed to load settings: {ex.Message}");
        }
    }

    /// <summary>
    /// 导出设置到指定文件
    /// </summary>
    public async Task ExportSettingsAsync(string filePath)
    {
        try
        {
            var json = JsonSerializer.Serialize(_settings, new JsonSerializerOptions
            {
                WriteIndented = true
            });
            await File.WriteAllTextAsync(filePath, json);
        }
        catch (Exception ex)
        {
            throw new InvalidOperationException($"导出设置失败: {ex.Message}", ex);
        }
    }

    /// <summary>
    /// 从指定文件导入设置
    /// </summary>
    public async Task ImportSettingsAsync(string filePath)
    {
        try
        {
            if (!File.Exists(filePath))
            {
                throw new FileNotFoundException("设置文件不存在");
            }

            var json = await File.ReadAllTextAsync(filePath);
            var importedSettings = JsonSerializer.Deserialize<Dictionary<string, JsonElement>>(json);
                
            if (importedSettings != null)
            {
                foreach (var kvp in importedSettings)
                {
                    _settings[kvp.Key] = kvp.Value;
                }
                await SaveSettingsAsync();
            }
        }
        catch (Exception ex)
        {
            throw new InvalidOperationException($"导入设置失败: {ex.Message}", ex);
        }
    }

    /// <summary>
    /// 重置所有设置到默认值
    /// </summary>
    public void ResetToDefaults()
    {
        _settings.Clear();
            
        // 设置默认值
        SetSetting(SettingsKeys.Formatter.IndentSize, 2);
        SetSetting(SettingsKeys.Formatter.UseTabsForIndent, false);
        SetSetting(SettingsKeys.Formatter.AutoFormat, false);
        SetSetting(SettingsKeys.Formatter.CompactOutput, false);
            
        SetSetting(SettingsKeys.Performance.MaxFileSize, 10);
        SetSetting(SettingsKeys.Performance.MaxProcessingTime, 30);
        SetSetting(SettingsKeys.Performance.EnableMemoryMonitoring, true);
        SetSetting(SettingsKeys.Performance.MemoryThreshold, 512);
            
        SetSetting(SettingsKeys.Application.Theme, "Light");
        SetSetting(SettingsKeys.Application.Language, "zh-CN");
        SetSetting(SettingsKeys.Application.AutoSave, true);
            
        // 设置默认日志级别
        SetSetting(SettingsKeys.Logging.LogLevel, "Information");
        SetSetting(SettingsKeys.Logging.EnableFileLogging, true);
        SetSetting(SettingsKeys.Logging.EnableConsoleLogging, true);
    }
}

/// <summary>
/// 设置键常量
/// </summary>
public static class SettingsKeys
{
    /// <summary>
    /// 应用程序设置
    /// </summary>
    public static class Application
    {
        public const string Theme = "Application.Theme";
        public const string Language = "Application.Language";
        public const string AutoSave = "Application.AutoSave";
        public const string WindowWidth = "Application.WindowWidth";
        public const string WindowHeight = "Application.WindowHeight";
        public const string WindowState = "Application.WindowState";
    }

    /// <summary>
    /// 格式化工具设置
    /// </summary>
    public static class Formatter
    {
        public const string IndentSize = "Formatter.IndentSize";
        public const string UseTabsForIndent = "Formatter.UseTabsForIndent";
        public const string AutoFormat = "Formatter.AutoFormat";
        public const string CompactOutput = "Formatter.CompactOutput";
        public const string ShowLineNumbers = "Formatter.ShowLineNumbers";
        public const string WordWrap = "Formatter.WordWrap";
    }

    /// <summary>
    /// 性能设置
    /// </summary>
    public static class Performance
    {
        public const string MaxFileSize = "Performance.MaxFileSize";
        public const string MaxProcessingTime = "Performance.MaxProcessingTime";
        public const string EnableMemoryMonitoring = "Performance.EnableMemoryMonitoring";
        public const string MemoryThreshold = "Performance.MemoryThreshold";
        public const string ChunkSize = "Performance.ChunkSize";
        public const string MaxConcurrency = "Performance.MaxConcurrency";
    }

    /// <summary>
    /// JSON格式化器设置
    /// </summary>
    public static class JsonFormatter
    {
        public const string SortKeys = "JsonFormatter.SortKeys";
        public const string EscapeNonAscii = "JsonFormatter.EscapeNonAscii";
        public const string ValidateOnInput = "JsonFormatter.ValidateOnInput";
    }

    /// <summary>
    /// XML格式化器设置
    /// </summary>
    public static class XmlFormatter
    {
        public const string OmitXmlDeclaration = "XmlFormatter.OmitXmlDeclaration";
        public const string NewLineOnAttributes = "XmlFormatter.NewLineOnAttributes";
    }

    /// <summary>
    /// 日志设置
    /// </summary>
    public static class Logging
    {
        public const string LogLevel = "Logging.LogLevel";
        public const string EnableFileLogging = "Logging.EnableFileLogging";
        public const string EnableConsoleLogging = "Logging.EnableConsoleLogging";
    }
}