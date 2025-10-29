using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;
using DevUtilities.Core.Services.Interfaces;
using Serilog;

namespace DevUtilities.Core.Services.Implementations;

/// <summary>
/// 配置服务实现
/// </summary>
public class ConfigurationService : IConfigurationService
{
    private readonly string _configFilePath;
    private AppConfiguration? _configuration;

    public ConfigurationService()
    {
        _configFilePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "config.json");
        Log.Debug("[ConfigurationService] 初始化配置服务，配置文件路径: {ConfigFilePath}", _configFilePath);
    }

    /// <summary>
    /// 获取配置
    /// </summary>
    /// <returns>应用配置</returns>
    public async Task<AppConfiguration> GetConfigurationAsync()
    {
        Log.Debug("[ConfigurationService] 获取配置");
        
        try
        {
            if (_configuration == null)
            {
                Log.Debug("[ConfigurationService] 配置未加载，开始加载配置");
                await LoadConfigurationAsync();
            }

            Log.Debug("[ConfigurationService] 配置获取成功");
            return _configuration ?? new AppConfiguration();
        }
        catch (Exception ex)
        {
            Log.Error(ex, "[ConfigurationService] 获取配置失败");
            return new AppConfiguration();
        }
    }

    /// <summary>
    /// 保存配置
    /// </summary>
    /// <param name="configuration">应用配置</param>
    /// <returns>是否保存成功</returns>
    public async Task<bool> SaveConfigurationAsync(AppConfiguration configuration)
    {
        Log.Debug("[ConfigurationService] 开始保存配置");
        
        try
        {
            if (configuration == null)
            {
                Log.Warning("[ConfigurationService] 配置对象为空，无法保存");
                return false;
            }

            var options = new JsonSerializerOptions
            {
                WriteIndented = true,
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase
            };

            var json = JsonSerializer.Serialize(configuration, options);
            Log.Debug("[ConfigurationService] 配置序列化成功，JSON长度: {JsonLength}", json.Length);

            var directory = Path.GetDirectoryName(_configFilePath);
            if (!string.IsNullOrEmpty(directory) && !Directory.Exists(directory))
            {
                Log.Debug("[ConfigurationService] 创建配置目录: {Directory}", directory);
                Directory.CreateDirectory(directory);
            }

            await File.WriteAllTextAsync(_configFilePath, json);
            _configuration = configuration;
            
            Log.Debug("[ConfigurationService] 配置保存成功: {ConfigFilePath}", _configFilePath);
            return true;
        }
        catch (Exception ex)
        {
            Log.Error(ex, "[ConfigurationService] 保存配置失败: {ConfigFilePath}", _configFilePath);
            return false;
        }
    }

    /// <summary>
    /// 重置配置为默认值
    /// </summary>
    /// <returns>是否重置成功</returns>
    public async Task<bool> ResetToDefaultAsync()
    {
        Log.Information("[ConfigurationService] 开始重置配置为默认值");
        
        try
        {
            var defaultConfig = new AppConfiguration();
            var result = await SaveConfigurationAsync(defaultConfig);
            
            if (result)
            {
                Log.Information("[ConfigurationService] 配置重置为默认值成功");
            }
            else
            {
                Log.Warning("[ConfigurationService] 配置重置为默认值失败");
            }
            
            return result;
        }
        catch (Exception ex)
        {
            Log.Error(ex, "[ConfigurationService] 重置配置为默认值时发生错误");
            return false;
        }
    }

    /// <summary>
    /// 加载配置
    /// </summary>
    /// <returns>加载任务</returns>
    private async Task LoadConfigurationAsync()
    {
        Log.Debug("[ConfigurationService] 开始加载配置文件: {ConfigFilePath}", _configFilePath);
        
        try
        {
            if (!File.Exists(_configFilePath))
            {
                Log.Debug("[ConfigurationService] 配置文件不存在，创建默认配置: {ConfigFilePath}", _configFilePath);
                _configuration = new AppConfiguration();
                await SaveConfigurationAsync(_configuration);
                return;
            }

            var json = await File.ReadAllTextAsync(_configFilePath);
            Log.Debug("[ConfigurationService] 配置文件读取成功，JSON长度: {JsonLength}", json.Length);

            if (string.IsNullOrWhiteSpace(json))
            {
                Log.Warning("[ConfigurationService] 配置文件为空，使用默认配置");
                _configuration = new AppConfiguration();
                return;
            }

            var options = new JsonSerializerOptions
            {
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
                PropertyNameCaseInsensitive = true
            };

            _configuration = JsonSerializer.Deserialize<AppConfiguration>(json, options);
            
            if (_configuration == null)
            {
                Log.Warning("[ConfigurationService] 配置反序列化结果为空，使用默认配置");
                _configuration = new AppConfiguration();
            }
            else
            {
                Log.Debug("[ConfigurationService] 配置加载成功");
            }
        }
        catch (JsonException ex)
        {
            Log.Error(ex, "[ConfigurationService] 配置文件JSON格式错误，使用默认配置: {ConfigFilePath}", _configFilePath);
            _configuration = new AppConfiguration();
        }
        catch (Exception ex)
        {
            Log.Error(ex, "[ConfigurationService] 加载配置文件失败，使用默认配置: {ConfigFilePath}", _configFilePath);
            _configuration = new AppConfiguration();
        }
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
        Log.Debug("[ConfigurationService] 获取配置值: {Key}, 默认值: {DefaultValue}", key, defaultValue);
        
        try
        {
            if (_configuration == null)
            {
                Log.Warning("[ConfigurationService] 配置未加载，返回默认值");
                return defaultValue;
            }

            // 简单的键值对存储实现
            if (_configuration.Settings != null && _configuration.Settings.ContainsKey(key))
            {
                var value = _configuration.Settings[key];
                if (value is T typedValue)
                {
                    Log.Debug("[ConfigurationService] 配置值获取成功: {Key} = {Value}", key, value);
                    return typedValue;
                }
                
                // 尝试转换类型
                try
                {
                    var convertedValue = (T)Convert.ChangeType(value, typeof(T));
                    Log.Debug("[ConfigurationService] 配置值类型转换成功: {Key} = {Value}", key, convertedValue);
                    return convertedValue;
                }
                catch (Exception ex)
                {
                    Log.Warning(ex, "[ConfigurationService] 配置值类型转换失败: {Key}, 返回默认值", key);
                }
            }

            Log.Debug("[ConfigurationService] 配置键不存在，返回默认值: {Key}", key);
            return defaultValue;
        }
        catch (Exception ex)
        {
            Log.Error(ex, "[ConfigurationService] 获取配置值失败: {Key}, 返回默认值", key);
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
        Log.Debug("[ConfigurationService] 设置配置值: {Key} = {Value}", key, value);
        
        try
        {
            if (_configuration == null)
            {
                await LoadConfigurationAsync();
            }

            if (_configuration?.Settings == null)
            {
                _configuration = new AppConfiguration();
            }

            _configuration.Settings[key] = value?.ToString() ?? string.Empty;
            var result = await SaveConfigurationAsync(_configuration);
            
            if (result)
            {
                Log.Debug("[ConfigurationService] 配置值设置成功: {Key} = {Value}", key, value);
            }
            else
            {
                Log.Warning("[ConfigurationService] 配置值设置失败: {Key}", key);
            }
            
            return result;
        }
        catch (Exception ex)
        {
            Log.Error(ex, "[ConfigurationService] 设置配置值失败: {Key}", key);
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
        Log.Debug("[ConfigurationService] 检查配置键是否存在: {Key}", key);
        
        try
        {
            if (_configuration?.Settings == null)
            {
                Log.Debug("[ConfigurationService] 配置未加载或为空，键不存在: {Key}", key);
                return false;
            }

            var exists = _configuration.Settings.ContainsKey(key);
            Log.Debug("[ConfigurationService] 配置键存在性检查结果: {Key} = {Exists}", key, exists);
            return exists;
        }
        catch (Exception ex)
        {
            Log.Error(ex, "[ConfigurationService] 检查配置键存在性失败: {Key}", key);
            return false;
        }
    }

    /// <summary>
    /// 删除配置项
    /// </summary>
    /// <param name="key">配置键</param>
    /// <returns>是否删除成功</returns>
    public async Task<bool> RemoveAsync(string key)
    {
        Log.Debug("[ConfigurationService] 删除配置项: {Key}", key);
        
        try
        {
            if (_configuration?.Settings == null)
            {
                Log.Warning("[ConfigurationService] 配置未加载或为空，无法删除: {Key}", key);
                return false;
            }

            var removed = _configuration.Settings.Remove(key);
            if (removed)
            {
                var result = await SaveConfigurationAsync(_configuration);
                if (result)
                {
                    Log.Debug("[ConfigurationService] 配置项删除成功: {Key}", key);
                }
                else
                {
                    Log.Warning("[ConfigurationService] 配置项删除后保存失败: {Key}", key);
                }
                return result;
            }
            else
            {
                Log.Warning("[ConfigurationService] 配置项不存在，无法删除: {Key}", key);
                return false;
            }
        }
        catch (Exception ex)
        {
            Log.Error(ex, "[ConfigurationService] 删除配置项失败: {Key}", key);
            return false;
        }
    }

    /// <summary>
    /// 获取所有配置键
    /// </summary>
    /// <returns>配置键列表</returns>
    public IEnumerable<string> GetAllKeys()
    {
        Log.Debug("[ConfigurationService] 获取所有配置键");
        
        try
        {
            if (_configuration?.Settings == null)
            {
                Log.Debug("[ConfigurationService] 配置未加载或为空，返回空列表");
                return Enumerable.Empty<string>();
            }

            var keys = _configuration.Settings.Keys.ToList();
            Log.Debug("[ConfigurationService] 获取到 {KeyCount} 个配置键", keys.Count);
            return keys;
        }
        catch (Exception ex)
        {
            Log.Error(ex, "[ConfigurationService] 获取所有配置键失败");
            return Enumerable.Empty<string>();
        }
    }

    /// <summary>
    /// 清空所有配置
    /// </summary>
    /// <returns>是否清空成功</returns>
    public async Task<bool> ClearAsync()
    {
        Log.Information("[ConfigurationService] 清空所有配置");
        
        try
        {
            _configuration = new AppConfiguration();
            var result = await SaveConfigurationAsync(_configuration);
            
            if (result)
            {
                Log.Information("[ConfigurationService] 配置清空成功");
            }
            else
            {
                Log.Warning("[ConfigurationService] 配置清空失败");
            }
            
            return result;
        }
        catch (Exception ex)
        {
            Log.Error(ex, "[ConfigurationService] 清空配置失败");
            return false;
        }
    }

    /// <summary>
    /// 保存配置到文件
    /// </summary>
    /// <returns>是否保存成功</returns>
    public async Task<bool> SaveAsync()
    {
        Log.Debug("[ConfigurationService] 保存配置到文件");
        
        try
        {
            if (_configuration == null)
            {
                Log.Warning("[ConfigurationService] 配置为空，无法保存");
                return false;
            }

            return await SaveConfigurationAsync(_configuration);
        }
        catch (Exception ex)
        {
            Log.Error(ex, "[ConfigurationService] 保存配置失败");
            return false;
        }
    }

    /// <summary>
    /// 从文件加载配置
    /// </summary>
    /// <returns>是否加载成功</returns>
    public async Task<bool> LoadAsync()
    {
        Log.Debug("[ConfigurationService] 从文件加载配置");
        
        try
        {
            await LoadConfigurationAsync();
            Log.Debug("[ConfigurationService] 配置加载成功");
            return true;
        }
        catch (Exception ex)
        {
            Log.Error(ex, "[ConfigurationService] 加载配置失败");
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
        Log.Debug("[ConfigurationService] 获取工具配置: {ToolName}", toolName);
        
        try
        {
            if (_configuration?.ToolConfigurations == null)
            {
                Log.Debug("[ConfigurationService] 工具配置为空，返回默认配置: {ToolName}", toolName);
                return new T();
            }

            if (_configuration.ToolConfigurations.ContainsKey(toolName))
            {
                var configJson = _configuration.ToolConfigurations[toolName];
                if (configJson is JsonElement jsonElement)
                {
                    var config = JsonSerializer.Deserialize<T>(jsonElement.GetRawText());
                    Log.Debug("[ConfigurationService] 工具配置获取成功: {ToolName}", toolName);
                    return config ?? new T();
                }
            }

            Log.Debug("[ConfigurationService] 工具配置不存在，返回默认配置: {ToolName}", toolName);
            return new T();
        }
        catch (Exception ex)
        {
            Log.Error(ex, "[ConfigurationService] 获取工具配置失败: {ToolName}", toolName);
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
        Log.Debug("[ConfigurationService] 设置工具配置: {ToolName}", toolName);
        
        try
        {
            if (_configuration == null)
            {
                await LoadConfigurationAsync();
            }

            if (_configuration?.ToolConfigurations == null)
            {
                _configuration = new AppConfiguration();
            }

            var configJson = JsonSerializer.Serialize(configuration);
            _configuration.ToolConfigurations[toolName] = JsonSerializer.Deserialize<JsonElement>(configJson);
            
            var result = await SaveConfigurationAsync(_configuration);
            
            if (result)
            {
                Log.Debug("[ConfigurationService] 工具配置设置成功: {ToolName}", toolName);
            }
            else
            {
                Log.Warning("[ConfigurationService] 工具配置设置失败: {ToolName}", toolName);
            }
            
            return result;
        }
        catch (Exception ex)
        {
            Log.Error(ex, "[ConfigurationService] 设置工具配置失败: {ToolName}", toolName);
            return false;
        }
    }

/// <summary>
/// 应用配置
/// </summary>
public class AppConfiguration
{
    /// <summary>
    /// 主题设置
    /// </summary>
    public string Theme { get; set; } = "Light";

    /// <summary>
    /// 语言设置
    /// </summary>
    public string Language { get; set; } = "zh-CN";

    /// <summary>
    /// 自动保存设置
    /// </summary>
    public bool AutoSave { get; set; } = true;

    /// <summary>
    /// 窗口设置
    /// </summary>
    public WindowSettings Window { get; set; } = new();

    /// <summary>
    /// 通用设置字典
    /// </summary>
    public Dictionary<string, object> Settings { get; set; } = new();

    /// <summary>
    /// 工具特定配置
    /// </summary>
    public Dictionary<string, object> ToolConfigurations { get; set; } = new();
}

/// <summary>
/// 窗口设置
/// </summary>
public class WindowSettings
{
    /// <summary>
    /// 窗口宽度
    /// </summary>
    public double Width { get; set; } = 1200;

    /// <summary>
    /// 窗口高度
    /// </summary>
    public double Height { get; set; } = 800;

    /// <summary>
    /// 窗口位置X
    /// </summary>
    public double X { get; set; } = 100;

    /// <summary>
    /// 窗口位置Y
    /// </summary>
    public double Y { get; set; } = 100;

    /// <summary>
    /// 是否最大化
    /// </summary>
    public bool IsMaximized { get; set; } = false;
}
}