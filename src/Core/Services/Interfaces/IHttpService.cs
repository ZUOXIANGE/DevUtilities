using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;

namespace DevUtilities.Core.Services.Interfaces;

/// <summary>
/// HTTP服务接口
/// </summary>
public interface IHttpService
{
    /// <summary>
    /// 发送GET请求
    /// </summary>
    /// <param name="url">请求URL</param>
    /// <param name="headers">请求头</param>
    /// <param name="timeout">超时时间</param>
    /// <param name="cancellationToken">取消令牌</param>
    /// <returns>HTTP响应</returns>
    Task<HttpResponseInfo> GetAsync(string url, 
        Dictionary<string, string>? headers = null, 
        TimeSpan? timeout = null, 
        CancellationToken cancellationToken = default);

    /// <summary>
    /// 发送POST请求
    /// </summary>
    /// <param name="url">请求URL</param>
    /// <param name="content">请求内容</param>
    /// <param name="contentType">内容类型</param>
    /// <param name="headers">请求头</param>
    /// <param name="timeout">超时时间</param>
    /// <param name="cancellationToken">取消令牌</param>
    /// <returns>HTTP响应</returns>
    Task<HttpResponseInfo> PostAsync(string url, 
        string content, 
        string contentType = "application/json",
        Dictionary<string, string>? headers = null, 
        TimeSpan? timeout = null, 
        CancellationToken cancellationToken = default);

    /// <summary>
    /// 发送PUT请求
    /// </summary>
    /// <param name="url">请求URL</param>
    /// <param name="content">请求内容</param>
    /// <param name="contentType">内容类型</param>
    /// <param name="headers">请求头</param>
    /// <param name="timeout">超时时间</param>
    /// <param name="cancellationToken">取消令牌</param>
    /// <returns>HTTP响应</returns>
    Task<HttpResponseInfo> PutAsync(string url, 
        string content, 
        string contentType = "application/json",
        Dictionary<string, string>? headers = null, 
        TimeSpan? timeout = null, 
        CancellationToken cancellationToken = default);

    /// <summary>
    /// 发送DELETE请求
    /// </summary>
    /// <param name="url">请求URL</param>
    /// <param name="headers">请求头</param>
    /// <param name="timeout">超时时间</param>
    /// <param name="cancellationToken">取消令牌</param>
    /// <returns>HTTP响应</returns>
    Task<HttpResponseInfo> DeleteAsync(string url, 
        Dictionary<string, string>? headers = null, 
        TimeSpan? timeout = null, 
        CancellationToken cancellationToken = default);

    /// <summary>
    /// 发送PATCH请求
    /// </summary>
    /// <param name="url">请求URL</param>
    /// <param name="content">请求内容</param>
    /// <param name="contentType">内容类型</param>
    /// <param name="headers">请求头</param>
    /// <param name="timeout">超时时间</param>
    /// <param name="cancellationToken">取消令牌</param>
    /// <returns>HTTP响应</returns>
    Task<HttpResponseInfo> PatchAsync(string url, 
        string content, 
        string contentType = "application/json",
        Dictionary<string, string>? headers = null, 
        TimeSpan? timeout = null, 
        CancellationToken cancellationToken = default);

    /// <summary>
    /// 发送HEAD请求
    /// </summary>
    /// <param name="url">请求URL</param>
    /// <param name="headers">请求头</param>
    /// <param name="timeout">超时时间</param>
    /// <param name="cancellationToken">取消令牌</param>
    /// <returns>HTTP响应</returns>
    Task<HttpResponseInfo> HeadAsync(string url, 
        Dictionary<string, string>? headers = null, 
        TimeSpan? timeout = null, 
        CancellationToken cancellationToken = default);

    /// <summary>
    /// 发送OPTIONS请求
    /// </summary>
    /// <param name="url">请求URL</param>
    /// <param name="headers">请求头</param>
    /// <param name="timeout">超时时间</param>
    /// <param name="cancellationToken">取消令牌</param>
    /// <returns>HTTP响应</returns>
    Task<HttpResponseInfo> OptionsAsync(string url, 
        Dictionary<string, string>? headers = null, 
        TimeSpan? timeout = null, 
        CancellationToken cancellationToken = default);

    /// <summary>
    /// 发送自定义HTTP请求
    /// </summary>
    /// <param name="request">HTTP请求信息</param>
    /// <param name="cancellationToken">取消令牌</param>
    /// <returns>HTTP响应</returns>
    Task<HttpResponseInfo> SendAsync(HttpRequestInfo request, CancellationToken cancellationToken = default);

    /// <summary>
    /// 下载文件
    /// </summary>
    /// <param name="url">文件URL</param>
    /// <param name="filePath">保存路径</param>
    /// <param name="headers">请求头</param>
    /// <param name="progress">进度回调</param>
    /// <param name="cancellationToken">取消令牌</param>
    /// <returns>是否下载成功</returns>
    Task<bool> DownloadFileAsync(string url, 
        string filePath, 
        Dictionary<string, string>? headers = null,
        IProgress<DownloadProgress>? progress = null,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// 上传文件
    /// </summary>
    /// <param name="url">上传URL</param>
    /// <param name="filePath">文件路径</param>
    /// <param name="fieldName">表单字段名</param>
    /// <param name="headers">请求头</param>
    /// <param name="progress">进度回调</param>
    /// <param name="cancellationToken">取消令牌</param>
    /// <returns>HTTP响应</returns>
    Task<HttpResponseInfo> UploadFileAsync(string url, 
        string filePath, 
        string fieldName = "file",
        Dictionary<string, string>? headers = null,
        IProgress<UploadProgress>? progress = null,
        CancellationToken cancellationToken = default);
}

/// <summary>
/// HTTP请求信息
/// </summary>
public class HttpRequestInfo
{
    /// <summary>
    /// 请求URL
    /// </summary>
    public string Url { get; set; } = string.Empty;

    /// <summary>
    /// HTTP方法
    /// </summary>
    public HttpMethod Method { get; set; } = HttpMethod.Get;

    /// <summary>
    /// 请求头
    /// </summary>
    public Dictionary<string, string> Headers { get; set; } = new();

    /// <summary>
    /// 请求内容
    /// </summary>
    public string? Content { get; set; }

    /// <summary>
    /// 内容类型
    /// </summary>
    public string? ContentType { get; set; }

    /// <summary>
    /// 超时时间
    /// </summary>
    public TimeSpan? Timeout { get; set; }
}

/// <summary>
/// HTTP响应信息
/// </summary>
public class HttpResponseInfo
{
    /// <summary>
    /// 状态码
    /// </summary>
    public int StatusCode { get; set; }

    /// <summary>
    /// 状态描述
    /// </summary>
    public string StatusDescription { get; set; } = string.Empty;

    /// <summary>
    /// 响应头
    /// </summary>
    public Dictionary<string, string> Headers { get; set; } = new();

    /// <summary>
    /// 响应内容
    /// </summary>
    public string Content { get; set; } = string.Empty;

    /// <summary>
    /// 响应字节数据
    /// </summary>
    public byte[]? ContentBytes { get; set; }

    /// <summary>
    /// 内容类型
    /// </summary>
    public string? ContentType { get; set; }

    /// <summary>
    /// 内容长度
    /// </summary>
    public long ContentLength { get; set; }

    /// <summary>
    /// 请求耗时
    /// </summary>
    public TimeSpan Duration { get; set; }

    /// <summary>
    /// 是否成功
    /// </summary>
    public bool IsSuccess { get; set; }

    /// <summary>
    /// 错误消息
    /// </summary>
    public string? ErrorMessage { get; set; }
}

/// <summary>
/// 下载进度
/// </summary>
public class DownloadProgress
{
    /// <summary>
    /// 已下载字节数
    /// </summary>
    public long BytesReceived { get; set; }

    /// <summary>
    /// 总字节数
    /// </summary>
    public long TotalBytes { get; set; }

    /// <summary>
    /// 进度百分比
    /// </summary>
    public double ProgressPercentage => TotalBytes > 0 ? (double)BytesReceived / TotalBytes * 100 : 0;
}

/// <summary>
/// 上传进度
/// </summary>
public class UploadProgress
{
    /// <summary>
    /// 已上传字节数
    /// </summary>
    public long BytesSent { get; set; }

    /// <summary>
    /// 总字节数
    /// </summary>
    public long TotalBytes { get; set; }

    /// <summary>
    /// 进度百分比
    /// </summary>
    public double ProgressPercentage => TotalBytes > 0 ? (double)BytesSent / TotalBytes * 100 : 0;
}