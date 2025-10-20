using System.Collections.Generic;
using System.Threading.Tasks;

namespace DevUtilities.Core.Services.Interfaces;

/// <summary>
/// 文件服务接口
/// </summary>
public interface IFileService
{
    /// <summary>
    /// 读取文件内容
    /// </summary>
    /// <param name="filePath">文件路径</param>
    /// <returns>文件内容</returns>
    Task<string> ReadTextAsync(string filePath);

    /// <summary>
    /// 写入文件内容
    /// </summary>
    /// <param name="filePath">文件路径</param>
    /// <param name="content">文件内容</param>
    /// <returns>是否写入成功</returns>
    Task<bool> WriteTextAsync(string filePath, string content);

    /// <summary>
    /// 读取文件字节数据
    /// </summary>
    /// <param name="filePath">文件路径</param>
    /// <returns>文件字节数据</returns>
    Task<byte[]> ReadBytesAsync(string filePath);

    /// <summary>
    /// 写入文件字节数据
    /// </summary>
    /// <param name="filePath">文件路径</param>
    /// <param name="data">字节数据</param>
    /// <returns>是否写入成功</returns>
    Task<bool> WriteBytesAsync(string filePath, byte[] data);

    /// <summary>
    /// 检查文件是否存在
    /// </summary>
    /// <param name="filePath">文件路径</param>
    /// <returns>文件是否存在</returns>
    bool FileExists(string filePath);

    /// <summary>
    /// 检查目录是否存在
    /// </summary>
    /// <param name="directoryPath">目录路径</param>
    /// <returns>目录是否存在</returns>
    bool DirectoryExists(string directoryPath);

    /// <summary>
    /// 创建目录
    /// </summary>
    /// <param name="directoryPath">目录路径</param>
    /// <returns>是否创建成功</returns>
    bool CreateDirectory(string directoryPath);

    /// <summary>
    /// 删除文件
    /// </summary>
    /// <param name="filePath">文件路径</param>
    /// <returns>是否删除成功</returns>
    bool DeleteFile(string filePath);

    /// <summary>
    /// 获取文件大小
    /// </summary>
    /// <param name="filePath">文件路径</param>
    /// <returns>文件大小（字节）</returns>
    long GetFileSize(string filePath);

    /// <summary>
    /// 获取文件扩展名
    /// </summary>
    /// <param name="filePath">文件路径</param>
    /// <returns>文件扩展名</returns>
    string GetFileExtension(string filePath);

    /// <summary>
    /// 获取文件名（不含扩展名）
    /// </summary>
    /// <param name="filePath">文件路径</param>
    /// <returns>文件名</returns>
    string GetFileNameWithoutExtension(string filePath);

    /// <summary>
    /// 获取目录中的所有文件
    /// </summary>
    /// <param name="directoryPath">目录路径</param>
    /// <param name="searchPattern">搜索模式</param>
    /// <param name="recursive">是否递归搜索</param>
    /// <returns>文件路径列表</returns>
    IEnumerable<string> GetFiles(string directoryPath, string searchPattern = "*", bool recursive = false);

    /// <summary>
    /// 选择文件对话框
    /// </summary>
    /// <param name="title">对话框标题</param>
    /// <param name="filters">文件过滤器</param>
    /// <returns>选择的文件路径，取消则返回null</returns>
    Task<string?> ShowOpenFileDialogAsync(string title = "选择文件", string filters = "所有文件|*.*");

    /// <summary>
    /// 保存文件对话框
    /// </summary>
    /// <param name="title">对话框标题</param>
    /// <param name="defaultFileName">默认文件名</param>
    /// <param name="filters">文件过滤器</param>
    /// <returns>保存的文件路径，取消则返回null</returns>
    Task<string?> ShowSaveFileDialogAsync(string title = "保存文件", string defaultFileName = "", string filters = "所有文件|*.*");
}