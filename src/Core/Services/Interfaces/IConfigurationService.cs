using System.Collections.Generic;
using System.Threading.Tasks;

namespace DevUtilities.Core.Services.Interfaces;

/// <summary>
/// 配置服务接口
/// </summary>
public interface IConfigurationService
{
    /// <summary>
    /// 获取配置值
    /// </summary>
    /// <typeparam name="T">配置值类型</typeparam>
    /// <param name="key">配置键</param>
    /// <param name="defaultValue">默认值</param>
    /// <returns>配置值</returns>
    T GetValue<T>(string key, T defaultValue = default!);

    /// <summary>
    /// 设置配置值
    /// </summary>
    /// <typeparam name="T">配置值类型</typeparam>
    /// <param name="key">配置键</param>
    /// <param name="value">配置值</param>
    /// <returns>是否设置成功</returns>
    Task<bool> SetValueAsync<T>(string key, T value);

    /// <summary>
    /// 检查配置键是否存在
    /// </summary>
    /// <param name="key">配置键</param>
    /// <returns>是否存在</returns>
    bool HasKey(string key);

    /// <summary>
    /// 删除配置项
    /// </summary>
    /// <param name="key">配置键</param>
    /// <returns>是否删除成功</returns>
    Task<bool> RemoveAsync(string key);

    /// <summary>
    /// 获取所有配置键
    /// </summary>
    /// <returns>配置键列表</returns>
    IEnumerable<string> GetAllKeys();

    /// <summary>
    /// 清空所有配置
    /// </summary>
    /// <returns>是否清空成功</returns>
    Task<bool> ClearAsync();

    /// <summary>
    /// 保存配置到文件
    /// </summary>
    /// <returns>是否保存成功</returns>
    Task<bool> SaveAsync();

    /// <summary>
    /// 从文件加载配置
    /// </summary>
    /// <returns>是否加载成功</returns>
    Task<bool> LoadAsync();

    /// <summary>
    /// 获取工具特定的配置
    /// </summary>
    /// <typeparam name="T">配置对象类型</typeparam>
    /// <param name="toolName">工具名称</param>
    /// <returns>工具配置对象</returns>
    T GetToolConfiguration<T>(string toolName) where T : class, new();

    /// <summary>
    /// 设置工具特定的配置
    /// </summary>
    /// <typeparam name="T">配置对象类型</typeparam>
    /// <param name="toolName">工具名称</param>
    /// <param name="configuration">配置对象</param>
    /// <returns>是否设置成功</returns>
    Task<bool> SetToolConfigurationAsync<T>(string toolName, T configuration) where T : class;
}