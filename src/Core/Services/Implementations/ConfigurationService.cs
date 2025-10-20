using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;
using DevUtilities.Core.Services.Interfaces;

namespace DevUtilities.Core.Services.Implementations;

/// <summary>
/// 配置服务实现
/// </summary>
public class ConfigurationService : IConfigurationService
{
    private readonly ConcurrentDictionary<string, object> _configurations = new();
    private readonly string _configFilePath;
    private readonly JsonSerializerOptions _jsonOptions;

    /// <summary>
    /// 构造函数
    /// </summary>
    public ConfigurationService()
    {
        var appDataPath = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
        var appFolder = Path.Combine(appDataPath, "DevUtilities");
        
        if (!Directory.Exists(appFolder))
        {
            Directory.CreateDirectory(appFolder);
        }

        _configFilePath = Path.Combine(appFolder, "config.json");
        
        _jsonOptions = new JsonSerializerOptions
        {
            WriteIndented = true,
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
            PropertyNameCaseInsensitive = true
        };

        // 启动时加载配置
        _ = LoadAsync();
    }

    /// <summary>
    /// 获取配置值
    /// </summary>
    /// <typeparam name="T">配置值类型</typeparam>
    /// <param name="key">配置键</param>
    /// <param name="defaultValue">默认值</param>
    /// <returns>配置值</returns>
    public T GetValue<T>(string key, T defaultValue = default!)
    {
        try
        {
            if (string.IsNullOrEmpty(key))
                return defaultValue;

            if (_configurations.TryGetValue(key, out var value))
            {
                if (value is T directValue)
                {
                    return directValue;
                }

                // 尝试转换类型
                if (value is JsonElement jsonElement)
                {
                    return JsonSerializer.Deserialize<T>(jsonElement.GetRawText(), _jsonOptions) ?? defaultValue;
                }

                // 尝试直接转换
                return (T)Convert.ChangeType(value, typeof(T));
            }

            return defaultValue;
        }
        catch (Exception)
        {
            return defaultValue;
        }
    }

    /// <summary>
    /// 设置配置值
    /// </summary>
    /// <typeparam name="T">配置值类型</typeparam>
    /// <param name="key">配置键</param>
    /// <param name="value">配置值</param>
    /// <returns>是否设置成功</returns>
    public async Task<bool> SetValueAsync<T>(string key, T value)
    {
        try
        {
            if (string.IsNullOrEmpty(key))
                return false;

            _configurations.AddOrUpdate(key, value!, (k, v) => value!);
            
            // 自动保存配置
            return await SaveAsync();
        }
        catch (Exception)
        {
            return false;
        }
    }

    /// <summary>
    /// 检查配置键是否存在
    /// </summary>
    /// <param name="key">配置键</param>
    /// <returns>是否存在</returns>
    public bool HasKey(string key)
    {
        return !string.IsNullOrEmpty(key) && _configurations.ContainsKey(key);
    }

    /// <summary>
    /// 删除配置项
    /// </summary>
    /// <param name="key">配置键</param>
    /// <returns>是否删除成功</returns>
    public async Task<bool> RemoveAsync(string key)
    {
        try
        {
            if (string.IsNullOrEmpty(key))
                return false;

            var removed = _configurations.TryRemove(key, out _);
            
            if (removed)
            {
                await SaveAsync();
            }

            return removed;
        }
        catch (Exception)
        {
            return false;
        }
    }

    /// <summary>
    /// 获取所有配置键
    /// </summary>
    /// <returns>配置键列表</returns>
    public IEnumerable<string> GetAllKeys()
    {
        return _configurations.Keys.ToList();
    }

    /// <summary>
    /// 清空所有配置
    /// </summary>
    /// <returns>是否清空成功</returns>
    public async Task<bool> ClearAsync()
    {
        try
        {
            _configurations.Clear();
            return await SaveAsync();
        }
        catch (Exception)
        {
            return false;
        }
    }

    /// <summary>
    /// 保存配置到文件
    /// </summary>
    /// <returns>是否保存成功</returns>
    public async Task<bool> SaveAsync()
    {
        try
        {
            var configData = _configurations.ToDictionary(kvp => kvp.Key, kvp => kvp.Value);
            var json = JsonSerializer.Serialize(configData, _jsonOptions);
            
            await File.WriteAllTextAsync(_configFilePath, json);
            return true;
        }
        catch (Exception)
        {
            return false;
        }
    }

    /// <summary>
    /// 从文件加载配置
    /// </summary>
    /// <returns>是否加载成功</returns>
    public async Task<bool> LoadAsync()
    {
        try
        {
            if (!File.Exists(_configFilePath))
                return true; // 文件不存在不算错误

            var json = await File.ReadAllTextAsync(_configFilePath);
            if (string.IsNullOrWhiteSpace(json))
                return true;

            var configData = JsonSerializer.Deserialize<Dictionary<string, JsonElement>>(json, _jsonOptions);
            if (configData == null)
                return false;

            _configurations.Clear();
            foreach (var kvp in configData)
            {
                _configurations.TryAdd(kvp.Key, kvp.Value);
            }

            return true;
        }
        catch (Exception)
        {
            return false;
        }
    }

    /// <summary>
    /// 获取工具特定的配置
    /// </summary>
    /// <typeparam name="T">配置对象类型</typeparam>
    /// <param name="toolName">工具名称</param>
    /// <returns>工具配置对象</returns>
    public T GetToolConfiguration<T>(string toolName) where T : class, new()
    {
        try
        {
            if (string.IsNullOrEmpty(toolName))
                return new T();

            var key = $"Tools.{toolName}";
            return GetValue<T>(key, new T());
        }
        catch (Exception)
        {
            return new T();
        }
    }

    /// <summary>
    /// 设置工具特定的配置
    /// </summary>
    /// <typeparam name="T">配置对象类型</typeparam>
    /// <param name="toolName">工具名称</param>
    /// <param name="configuration">配置对象</param>
    /// <returns>是否设置成功</returns>
    public async Task<bool> SetToolConfigurationAsync<T>(string toolName, T configuration) where T : class
    {
        try
        {
            if (string.IsNullOrEmpty(toolName) || configuration == null)
                return false;

            var key = $"Tools.{toolName}";
            return await SetValueAsync(key, configuration);
        }
        catch (Exception)
        {
            return false;
        }
    }
}