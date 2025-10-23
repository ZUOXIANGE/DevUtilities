using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Platform.Storage;
using DevUtilities.Core.Services.Interfaces;
using Serilog;

namespace DevUtilities.Core.Services.Implementations;

/// <summary>
/// 文件服务实现
/// </summary>
public class FileService : IFileService
{
    /// <summary>
    /// 读取文件内容
    /// </summary>
    /// <param name="filePath">文件路径</param>
    /// <returns>文件内容</returns>
    public async Task<string> ReadTextAsync(string filePath)
    {
        Log.Debug("[FileService] 开始读取文件: {FilePath}", filePath);
        
        try
        {
            if (!File.Exists(filePath))
            {
                Log.Warning("[FileService] 文件不存在: {FilePath}", filePath);
                return string.Empty;
            }

            var content = await File.ReadAllTextAsync(filePath);
            Log.Debug("[FileService] 文件读取成功: {FilePath}, 内容长度: {ContentLength}", filePath, content.Length);
            return content;
        }
        catch (Exception ex)
        {
            Log.Error(ex, "[FileService] 读取文件失败: {FilePath}", filePath);
            return string.Empty;
        }
    }

    /// <summary>
    /// 写入文件内容
    /// </summary>
    /// <param name="filePath">文件路径</param>
    /// <param name="content">文件内容</param>
    /// <returns>是否写入成功</returns>
    public async Task<bool> WriteTextAsync(string filePath, string content)
    {
        Log.Debug("[FileService] 开始写入文件: {FilePath}, 内容长度: {ContentLength}", filePath, content?.Length ?? 0);
        
        try
        {
            var directory = Path.GetDirectoryName(filePath);
            if (!string.IsNullOrEmpty(directory) && !Directory.Exists(directory))
            {
                Log.Debug("[FileService] 创建目录: {Directory}", directory);
                Directory.CreateDirectory(directory);
            }

            await File.WriteAllTextAsync(filePath, content ?? string.Empty);
            Log.Debug("[FileService] 文件写入成功: {FilePath}", filePath);
            return true;
        }
        catch (Exception ex)
        {
            Log.Error(ex, "[FileService] 写入文件失败: {FilePath}", filePath);
            return false;
        }
    }

    /// <summary>
    /// 读取文件字节数据
    /// </summary>
    /// <param name="filePath">文件路径</param>
    /// <returns>文件字节数据</returns>
    public async Task<byte[]> ReadBytesAsync(string filePath)
    {
        Log.Debug("[FileService] 开始读取文件字节: {FilePath}", filePath);
        
        try
        {
            if (!File.Exists(filePath))
            {
                Log.Warning("[FileService] 文件不存在: {FilePath}", filePath);
                return Array.Empty<byte>();
            }

            var bytes = await File.ReadAllBytesAsync(filePath);
            Log.Debug("[FileService] 文件字节读取成功: {FilePath}, 字节数: {ByteCount}", filePath, bytes.Length);
            return bytes;
        }
        catch (Exception ex)
        {
            Log.Error(ex, "[FileService] 读取文件字节失败: {FilePath}", filePath);
            return Array.Empty<byte>();
        }
    }

    /// <summary>
    /// 写入文件字节数据
    /// </summary>
    /// <param name="filePath">文件路径</param>
    /// <param name="data">字节数据</param>
    /// <returns>是否写入成功</returns>
    public async Task<bool> WriteBytesAsync(string filePath, byte[] data)
    {
        Log.Debug("[FileService] 开始写入文件字节: {FilePath}, 字节数: {ByteCount}", filePath, data?.Length ?? 0);
        
        try
        {
            var directory = Path.GetDirectoryName(filePath);
            if (!string.IsNullOrEmpty(directory) && !Directory.Exists(directory))
            {
                Log.Debug("[FileService] 创建目录: {Directory}", directory);
                Directory.CreateDirectory(directory);
            }

            await File.WriteAllBytesAsync(filePath, data ?? Array.Empty<byte>());
            Log.Debug("[FileService] 文件字节写入成功: {FilePath}", filePath);
            return true;
        }
        catch (Exception ex)
        {
            Log.Error(ex, "[FileService] 写入文件字节失败: {FilePath}", filePath);
            return false;
        }
    }

    /// <summary>
    /// 检查文件是否存在
    /// </summary>
    /// <param name="filePath">文件路径</param>
    /// <returns>文件是否存在</returns>
    public bool FileExists(string filePath)
    {
        Log.Debug("[FileService] 检查文件是否存在: {FilePath}", filePath);
        
        try
        {
            var exists = File.Exists(filePath);
            Log.Debug("[FileService] 文件存在性检查结果: {FilePath} = {Exists}", filePath, exists);
            return exists;
        }
        catch (Exception ex)
        {
            Log.Error(ex, "[FileService] 检查文件存在性时发生错误: {FilePath}", filePath);
            return false;
        }
    }

    /// <summary>
    /// 检查目录是否存在
    /// </summary>
    /// <param name="directoryPath">目录路径</param>
    /// <returns>目录是否存在</returns>
    public bool DirectoryExists(string directoryPath)
    {
        Log.Debug("[FileService] 检查目录是否存在: {DirectoryPath}", directoryPath);
        
        try
        {
            var exists = Directory.Exists(directoryPath);
            Log.Debug("[FileService] 目录存在性检查结果: {DirectoryPath} = {Exists}", directoryPath, exists);
            return exists;
        }
        catch (Exception ex)
        {
            Log.Error(ex, "[FileService] 检查目录存在性时发生错误: {DirectoryPath}", directoryPath);
            return false;
        }
    }

    /// <summary>
    /// 创建目录
    /// </summary>
    /// <param name="directoryPath">目录路径</param>
    /// <returns>是否创建成功</returns>
    public bool CreateDirectory(string directoryPath)
    {
        Log.Debug("[FileService] 开始创建目录: {DirectoryPath}", directoryPath);
        
        try
        {
            if (Directory.Exists(directoryPath))
            {
                Log.Debug("[FileService] 目录已存在: {DirectoryPath}", directoryPath);
                return true;
            }

            Directory.CreateDirectory(directoryPath);
            Log.Debug("[FileService] 目录创建成功: {DirectoryPath}", directoryPath);
            return true;
        }
        catch (Exception ex)
        {
            Log.Error(ex, "[FileService] 创建目录失败: {DirectoryPath}", directoryPath);
            return false;
        }
    }

    /// <summary>
    /// 删除文件
    /// </summary>
    /// <param name="filePath">文件路径</param>
    /// <returns>是否删除成功</returns>
    public bool DeleteFile(string filePath)
    {
        Log.Debug("[FileService] 开始删除文件: {FilePath}", filePath);
        
        try
        {
            if (!File.Exists(filePath))
            {
                Log.Debug("[FileService] 文件不存在，无需删除: {FilePath}", filePath);
                return true;
            }

            File.Delete(filePath);
            Log.Debug("[FileService] 文件删除成功: {FilePath}", filePath);
            return true;
        }
        catch (Exception ex)
        {
            Log.Error(ex, "[FileService] 删除文件失败: {FilePath}", filePath);
            return false;
        }
    }

    /// <summary>
    /// 删除目录
    /// </summary>
    /// <param name="directoryPath">目录路径</param>
    /// <param name="recursive">是否递归删除</param>
    /// <returns>是否删除成功</returns>
    public bool DeleteDirectory(string directoryPath, bool recursive = false)
    {
        Log.Debug("[FileService] 开始删除目录: {DirectoryPath}, 递归: {Recursive}", directoryPath, recursive);
        
        try
        {
            if (!Directory.Exists(directoryPath))
            {
                Log.Debug("[FileService] 目录不存在，无需删除: {DirectoryPath}", directoryPath);
                return true;
            }

            Directory.Delete(directoryPath, recursive);
            Log.Debug("[FileService] 目录删除成功: {DirectoryPath}", directoryPath);
            return true;
        }
        catch (Exception ex)
        {
            Log.Error(ex, "[FileService] 删除目录失败: {DirectoryPath}", directoryPath);
            return false;
        }
    }

    /// <summary>
    /// 获取文件信息
    /// </summary>
    /// <param name="filePath">文件路径</param>
    /// <returns>文件信息</returns>
    public FileInfo? GetFileInfo(string filePath)
    {
        Log.Debug("[FileService] 获取文件信息: {FilePath}", filePath);
        
        try
        {
            if (!File.Exists(filePath))
            {
                Log.Warning("[FileService] 文件不存在: {FilePath}", filePath);
                return null;
            }

            var fileInfo = new FileInfo(filePath);
            Log.Debug("[FileService] 文件信息获取成功: {FilePath}, 大小: {Size} 字节", filePath, fileInfo.Length);
            return fileInfo;
        }
        catch (Exception ex)
        {
            Log.Error(ex, "[FileService] 获取文件信息失败: {FilePath}", filePath);
            return null;
        }
    }

    /// <summary>
    /// 枚举目录中的文件
    /// </summary>
    /// <param name="directoryPath">目录路径</param>
    /// <param name="searchPattern">搜索模式</param>
    /// <param name="recursive">是否递归搜索</param>
    /// <returns>文件路径列表</returns>
    public IEnumerable<string> EnumerateFiles(string directoryPath, string searchPattern = "*", bool recursive = false)
    {
        Log.Debug("[FileService] 枚举目录文件: {DirectoryPath}, 模式: {SearchPattern}, 递归: {Recursive}", 
            directoryPath, searchPattern, recursive);
        
        try
        {
            if (!Directory.Exists(directoryPath))
            {
                Log.Warning("[FileService] 目录不存在: {DirectoryPath}", directoryPath);
                return Enumerable.Empty<string>();
            }

            var searchOption = recursive ? SearchOption.AllDirectories : SearchOption.TopDirectoryOnly;
            var files = Directory.EnumerateFiles(directoryPath, searchPattern, searchOption);
            var fileList = files.ToList();
            
            Log.Debug("[FileService] 文件枚举完成: {DirectoryPath}, 找到 {FileCount} 个文件", directoryPath, fileList.Count);
            return fileList;
        }
        catch (Exception ex)
        {
            Log.Error(ex, "[FileService] 枚举目录文件失败: {DirectoryPath}", directoryPath);
            return Enumerable.Empty<string>();
        }
    }

    /// <summary>
    /// 枚举目录中的子目录
    /// </summary>
    /// <param name="directoryPath">目录路径</param>
    /// <param name="searchPattern">搜索模式</param>
    /// <param name="recursive">是否递归搜索</param>
    /// <returns>目录路径列表</returns>
    public IEnumerable<string> EnumerateDirectories(string directoryPath, string searchPattern = "*", bool recursive = false)
    {
        Log.Debug("[FileService] 枚举子目录: {DirectoryPath}, 模式: {SearchPattern}, 递归: {Recursive}", 
            directoryPath, searchPattern, recursive);
        
        try
        {
            if (!Directory.Exists(directoryPath))
            {
                Log.Warning("[FileService] 目录不存在: {DirectoryPath}", directoryPath);
                return Enumerable.Empty<string>();
            }

            var searchOption = recursive ? SearchOption.AllDirectories : SearchOption.TopDirectoryOnly;
            var directories = Directory.EnumerateDirectories(directoryPath, searchPattern, searchOption);
            var directoryList = directories.ToList();
            
            Log.Debug("[FileService] 子目录枚举完成: {DirectoryPath}, 找到 {DirectoryCount} 个目录", directoryPath, directoryList.Count);
            return directoryList;
        }
        catch (Exception ex)
        {
            Log.Error(ex, "[FileService] 枚举子目录失败: {DirectoryPath}", directoryPath);
            return Enumerable.Empty<string>();
        }
    }

    /// <summary>
    /// 显示打开文件对话框
    /// </summary>
    /// <param name="title">对话框标题</param>
    /// <param name="fileTypes">文件类型过滤器</param>
    /// <param name="allowMultiple">是否允许多选</param>
    /// <returns>选择的文件路径列表</returns>
    public async Task<IEnumerable<string>> ShowOpenFileDialogAsync(string title = "选择文件", 
        IEnumerable<FilePickerFileType>? fileTypes = null, bool allowMultiple = false)
    {
        Log.Debug("[FileService] 显示打开文件对话框: {Title}, 多选: {AllowMultiple}", title, allowMultiple);
        
        try
        {
            var topLevel = GetTopLevel();
            if (topLevel?.StorageProvider == null)
            {
                Log.Warning("[FileService] 无法获取存储提供程序");
                return Enumerable.Empty<string>();
            }

            var options = new FilePickerOpenOptions
            {
                Title = title,
                AllowMultiple = allowMultiple,
                FileTypeFilter = fileTypes?.ToList()
            };

            var result = await topLevel.StorageProvider.OpenFilePickerAsync(options);
            var filePaths = result.Select(f => f.Path.LocalPath).ToList();
            
            Log.Debug("[FileService] 文件对话框完成，选择了 {FileCount} 个文件", filePaths.Count);
            return filePaths;
        }
        catch (Exception ex)
        {
            Log.Error(ex, "[FileService] 显示打开文件对话框失败");
            return Enumerable.Empty<string>();
        }
    }

    /// <summary>
    /// 显示保存文件对话框
    /// </summary>
    /// <param name="title">对话框标题</param>
    /// <param name="suggestedFileName">建议的文件名</param>
    /// <param name="fileTypes">文件类型过滤器</param>
    /// <returns>选择的文件路径</returns>
    public async Task<string?> ShowSaveFileDialogAsync(string title = "保存文件", 
        string? suggestedFileName = null, IEnumerable<FilePickerFileType>? fileTypes = null)
    {
        Log.Debug("[FileService] 显示保存文件对话框: {Title}, 建议文件名: {SuggestedFileName}", title, suggestedFileName);
        
        try
        {
            var topLevel = GetTopLevel();
            if (topLevel?.StorageProvider == null)
            {
                Log.Warning("[FileService] 无法获取存储提供程序");
                return null;
            }

            var options = new FilePickerSaveOptions
            {
                Title = title,
                SuggestedFileName = suggestedFileName,
                FileTypeChoices = fileTypes?.ToList()
            };

            var result = await topLevel.StorageProvider.SaveFilePickerAsync(options);
            var filePath = result?.Path.LocalPath;
            
            Log.Debug("[FileService] 保存文件对话框完成，选择路径: {FilePath}", filePath ?? "未选择");
            return filePath;
        }
        catch (Exception ex)
        {
            Log.Error(ex, "[FileService] 显示保存文件对话框失败");
            return null;
        }
    }

    /// <summary>
    /// 显示选择文件夹对话框
    /// </summary>
    /// <param name="title">对话框标题</param>
    /// <param name="allowMultiple">是否允许多选</param>
    /// <returns>选择的文件夹路径列表</returns>
    public async Task<IEnumerable<string>> ShowSelectFolderDialogAsync(string title = "选择文件夹", bool allowMultiple = false)
    {
        Log.Debug("[FileService] 显示选择文件夹对话框: {Title}, 多选: {AllowMultiple}", title, allowMultiple);
        
        try
        {
            var topLevel = GetTopLevel();
            if (topLevel?.StorageProvider == null)
            {
                Log.Warning("[FileService] 无法获取存储提供程序");
                return Enumerable.Empty<string>();
            }

            var options = new FolderPickerOpenOptions
            {
                Title = title,
                AllowMultiple = allowMultiple
            };

            var result = await topLevel.StorageProvider.OpenFolderPickerAsync(options);
            var folderPaths = result.Select(f => f.Path.LocalPath).ToList();
            
            Log.Debug("[FileService] 选择文件夹对话框完成，选择了 {FolderCount} 个文件夹", folderPaths.Count);
            return folderPaths;
        }
        catch (Exception ex)
        {
            Log.Error(ex, "[FileService] 显示选择文件夹对话框失败");
            return Enumerable.Empty<string>();
        }
    }

    /// <summary>
    /// 获取顶级窗口
    /// </summary>
    /// <returns>顶级窗口</returns>
    private TopLevel? GetTopLevel()
    {
        try
        {
            if (Application.Current?.ApplicationLifetime is Avalonia.Controls.ApplicationLifetimes.IClassicDesktopStyleApplicationLifetime desktop)
            {
                Log.Debug("[FileService] 获取主窗口成功");
                return desktop.MainWindow;
            }

            Log.Warning("[FileService] 无法获取主窗口");
            return null;
        }
        catch (Exception ex)
        {
            Log.Error(ex, "[FileService] 获取顶级窗口时发生错误");
            return null;
        }
    }

    /// <summary>
    /// 获取文件大小
    /// </summary>
    /// <param name="filePath">文件路径</param>
    /// <returns>文件大小（字节）</returns>
    public long GetFileSize(string filePath)
    {
        Log.Debug("[FileService] 获取文件大小: {FilePath}", filePath);
        
        try
        {
            if (!File.Exists(filePath))
            {
                Log.Warning("[FileService] 文件不存在: {FilePath}", filePath);
                return 0;
            }

            var fileInfo = new FileInfo(filePath);
            var size = fileInfo.Length;
            Log.Debug("[FileService] 文件大小: {FilePath} = {Size} 字节", filePath, size);
            return size;
        }
        catch (Exception ex)
        {
            Log.Error(ex, "[FileService] 获取文件大小失败: {FilePath}", filePath);
            return 0;
        }
    }

    /// <summary>
    /// 获取文件扩展名
    /// </summary>
    /// <param name="filePath">文件路径</param>
    /// <returns>文件扩展名</returns>
    public string GetFileExtension(string filePath)
    {
        Log.Debug("[FileService] 获取文件扩展名: {FilePath}", filePath);
        
        try
        {
            var extension = Path.GetExtension(filePath);
            Log.Debug("[FileService] 文件扩展名: {FilePath} = {Extension}", filePath, extension);
            return extension;
        }
        catch (Exception ex)
        {
            Log.Error(ex, "[FileService] 获取文件扩展名失败: {FilePath}", filePath);
            return string.Empty;
        }
    }

    /// <summary>
    /// 获取文件名（不含扩展名）
    /// </summary>
    /// <param name="filePath">文件路径</param>
    /// <returns>文件名</returns>
    public string GetFileNameWithoutExtension(string filePath)
    {
        Log.Debug("[FileService] 获取文件名（不含扩展名）: {FilePath}", filePath);
        
        try
        {
            var fileName = Path.GetFileNameWithoutExtension(filePath);
            Log.Debug("[FileService] 文件名（不含扩展名）: {FilePath} = {FileName}", filePath, fileName);
            return fileName;
        }
        catch (Exception ex)
        {
            Log.Error(ex, "[FileService] 获取文件名失败: {FilePath}", filePath);
            return string.Empty;
        }
    }

    /// <summary>
    /// 获取目录中的所有文件
    /// </summary>
    /// <param name="directoryPath">目录路径</param>
    /// <param name="searchPattern">搜索模式</param>
    /// <param name="recursive">是否递归搜索</param>
    /// <returns>文件路径列表</returns>
    public IEnumerable<string> GetFiles(string directoryPath, string searchPattern = "*", bool recursive = false)
    {
        Log.Debug("[FileService] 获取目录文件: {DirectoryPath}, 模式: {SearchPattern}, 递归: {Recursive}", 
            directoryPath, searchPattern, recursive);
        
        try
        {
            if (!Directory.Exists(directoryPath))
            {
                Log.Warning("[FileService] 目录不存在: {DirectoryPath}", directoryPath);
                return Enumerable.Empty<string>();
            }

            var searchOption = recursive ? SearchOption.AllDirectories : SearchOption.TopDirectoryOnly;
            var files = Directory.GetFiles(directoryPath, searchPattern, searchOption);
            
            Log.Debug("[FileService] 找到 {FileCount} 个文件", files.Length);
            return files;
        }
        catch (Exception ex)
        {
            Log.Error(ex, "[FileService] 获取目录文件失败: {DirectoryPath}", directoryPath);
            return Enumerable.Empty<string>();
        }
    }

    /// <summary>
    /// 选择文件对话框
    /// </summary>
    /// <param name="title">对话框标题</param>
    /// <param name="filters">文件过滤器</param>
    /// <returns>选择的文件路径，取消则返回null</returns>
    public async Task<string?> ShowOpenFileDialogAsync(string title = "选择文件", string filters = "所有文件|*.*")
    {
        Log.Debug("[FileService] 显示打开文件对话框: {Title}, 过滤器: {Filters}", title, filters);
        
        try
        {
            var topLevel = GetTopLevel();
            if (topLevel?.StorageProvider == null)
            {
                Log.Warning("[FileService] 无法获取存储提供程序");
                return null;
            }

            var options = new FilePickerOpenOptions
            {
                Title = title,
                AllowMultiple = false
            };

            var result = await topLevel.StorageProvider.OpenFilePickerAsync(options);
            var filePath = result.FirstOrDefault()?.Path.LocalPath;
            
            Log.Debug("[FileService] 打开文件对话框完成，选择路径: {FilePath}", filePath ?? "未选择");
            return filePath;
        }
        catch (Exception ex)
        {
            Log.Error(ex, "[FileService] 显示打开文件对话框失败");
            return null;
        }
    }

    /// <summary>
    /// 保存文件对话框
    /// </summary>
    /// <param name="title">对话框标题</param>
    /// <param name="defaultFileName">默认文件名</param>
    /// <param name="filters">文件过滤器</param>
    /// <returns>保存的文件路径，取消则返回null</returns>
    public async Task<string?> ShowSaveFileDialogAsync(string title = "保存文件", string defaultFileName = "", string filters = "所有文件|*.*")
    {
        Log.Debug("[FileService] 显示保存文件对话框: {Title}, 默认文件名: {DefaultFileName}, 过滤器: {Filters}", 
            title, defaultFileName, filters);
        
        try
        {
            var topLevel = GetTopLevel();
            if (topLevel?.StorageProvider == null)
            {
                Log.Warning("[FileService] 无法获取存储提供程序");
                return null;
            }

            var options = new FilePickerSaveOptions
            {
                Title = title,
                SuggestedFileName = defaultFileName
            };

            var result = await topLevel.StorageProvider.SaveFilePickerAsync(options);
            var filePath = result?.Path.LocalPath;
            
            Log.Debug("[FileService] 保存文件对话框完成，选择路径: {FilePath}", filePath ?? "未选择");
            return filePath;
        }
        catch (Exception ex)
        {
            Log.Error(ex, "[FileService] 显示保存文件对话框失败");
            return null;
        }
    }
}