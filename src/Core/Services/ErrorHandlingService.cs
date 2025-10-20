using System;
using System.Collections.Generic;
using System.Text.Json;
using DevUtilities.Core.Exceptions;

namespace DevUtilities.Core.Services
{
    /// <summary>
    /// 错误处理服务
    /// </summary>
    public class ErrorHandlingService
    {
        private static readonly Dictionary<string, string> CommonErrorMessages = new()
        {
            { "OutOfMemoryException", "内存不足，请尝试处理较小的文件或重启应用程序" },
            { "TimeoutException", "操作超时，请检查网络连接或稍后重试" },
            { "UnauthorizedAccessException", "访问被拒绝，请检查文件权限" },
            { "FileNotFoundException", "找不到指定的文件" },
            { "DirectoryNotFoundException", "找不到指定的目录" },
            { "IOException", "文件读写错误，请检查文件是否被其他程序占用" },
            { "ArgumentException", "参数错误，请检查输入内容" },
            { "InvalidOperationException", "操作无效，请检查当前状态" },
            { "NotSupportedException", "不支持的操作或格式" }
        };

        /// <summary>
        /// 处理异常并返回用户友好的错误信息
        /// </summary>
        public static ErrorInfo HandleException(Exception exception)
        {
            return exception switch
            {
                FormatterException formatterEx => new ErrorInfo
                {
                    ErrorCode = formatterEx.ErrorCode,
                    UserMessage = formatterEx.UserFriendlyMessage,
                    TechnicalMessage = formatterEx.Message,
                    Severity = ErrorSeverity.Warning,
                    Suggestion = string.Join("; ", GetFormatterSuggestions(formatterEx))
                },
                
                JsonException jsonEx => HandleJsonException(jsonEx),
                
                OutOfMemoryException => new ErrorInfo
                {
                    ErrorCode = "OUT_OF_MEMORY",
                    UserMessage = "内存不足，无法处理如此大的文件",
                    TechnicalMessage = exception.Message,
                    Severity = ErrorSeverity.Error,
                    Suggestion = "尝试处理较小的文件; 重启应用程序释放内存; 分段处理大文件"
                },
                
                TimeoutException => new ErrorInfo
                {
                    ErrorCode = "TIMEOUT",
                    UserMessage = "操作超时，处理时间过长",
                    TechnicalMessage = exception.Message,
                    Severity = ErrorSeverity.Warning,
                    Suggestion = "减小文件大小; 检查文件格式是否正确; 稍后重试"
                },
                
                _ => HandleGenericException(exception)
            };
        }

        /// <summary>
        /// 处理JSON异常
        /// </summary>
        private static ErrorInfo HandleJsonException(JsonException jsonEx)
        {
            var lineNumber = ExtractLineNumber(jsonEx.Message);
            var columnNumber = ExtractColumnNumber(jsonEx.Message);
            
            var location = "";
            if (lineNumber.HasValue && columnNumber.HasValue)
            {
                location = $"（第{lineNumber}行，第{columnNumber}列）";
            }

            var userMessage = jsonEx.Message switch
            {
                var msg when msg.Contains("unexpected character") => $"JSON格式错误：存在意外字符{location}",
                var msg when msg.Contains("unterminated string") => $"JSON格式错误：字符串未正确结束{location}",
                var msg when msg.Contains("invalid number") => $"JSON格式错误：数字格式不正确{location}",
                var msg when msg.Contains("expected") => $"JSON格式错误：缺少必要的符号{location}",
                _ => $"JSON格式错误{location}"
            };

            return new ErrorInfo
            {
                ErrorCode = "JSON_PARSE_ERROR",
                UserMessage = userMessage,
                TechnicalMessage = jsonEx.Message,
                Severity = ErrorSeverity.Warning,
                Suggestion = string.Join("; ", GetJsonSuggestions(jsonEx.Message)),
                LineNumber = lineNumber,
                ColumnNumber = columnNumber
            };
        }

        /// <summary>
        /// 处理通用异常
        /// </summary>
        private static ErrorInfo HandleGenericException(Exception exception)
        {
            var exceptionType = exception.GetType().Name;
            var userMessage = CommonErrorMessages.TryGetValue(exceptionType, out var message) 
                ? message 
                : "发生未知错误，请稍后重试";

            return new ErrorInfo
            {
                ErrorCode = "GENERIC_ERROR",
                UserMessage = userMessage,
                TechnicalMessage = exception.Message,
                Severity = ErrorSeverity.Error,
                Suggestion = "检查输入内容; 稍后重试; 联系技术支持"
            };
        }

        /// <summary>
        /// 获取格式化工具异常的建议
        /// </summary>
        private static string[] GetFormatterSuggestions(FormatterException formatterEx)
        {
            return formatterEx.ErrorCode switch
            {
                "JSON_FORMAT_ERROR" => new[] { "检查JSON语法", "使用JSON验证工具", "确保括号和引号匹配" },
                "SQL_FORMAT_ERROR" => new[] { "检查SQL语法", "确保关键字拼写正确", "检查表名和字段名" },
                "HTML_FORMAT_ERROR" => new[] { "检查HTML标签", "确保标签正确闭合", "验证属性格式" },
                "PERFORMANCE_ERROR" => new[] { "减小文件大小", "分段处理", "优化输入内容" },
                _ => new[] { "检查输入格式", "参考示例数据", "稍后重试" }
            };
        }

        /// <summary>
        /// 获取JSON异常的建议
        /// </summary>
        private static string[] GetJsonSuggestions(string errorMessage)
        {
            if (errorMessage.Contains("unexpected character"))
            {
                return new[] { "检查是否有多余的逗号", "确保字符串用双引号包围", "检查特殊字符是否正确转义" };
            }
            if (errorMessage.Contains("unterminated string"))
            {
                return new[] { "检查字符串的引号是否匹配", "确保字符串正确结束", "检查是否缺少结束引号" };
            }
            if (errorMessage.Contains("invalid number"))
            {
                return new[] { "检查数字格式", "确保数字不以0开头（除了0本身）", "检查小数点格式" };
            }
            if (errorMessage.Contains("expected"))
            {
                return new[] { "检查是否缺少逗号", "确保对象和数组的语法正确", "检查括号是否匹配" };
            }

            return new[] { "使用JSON验证工具检查格式", "参考标准JSON语法", "检查整体结构" };
        }

        /// <summary>
        /// 从异常消息中提取行号
        /// </summary>
        private static int? ExtractLineNumber(string message)
        {
            var patterns = new[] { @"line (\d+)", @"第(\d+)行", @"LineNumber: (\d+)" };
            
            foreach (var pattern in patterns)
            {
                var match = System.Text.RegularExpressions.Regex.Match(message, pattern, System.Text.RegularExpressions.RegexOptions.IgnoreCase);
                if (match.Success && int.TryParse(match.Groups[1].Value, out var lineNumber))
                {
                    return lineNumber;
                }
            }
            
            return null;
        }

        /// <summary>
        /// 从异常消息中提取列号
        /// </summary>
        private static int? ExtractColumnNumber(string message)
        {
            var patterns = new[] { @"column (\d+)", @"第(\d+)列", @"BytePositionInLine: (\d+)" };
            
            foreach (var pattern in patterns)
            {
                var match = System.Text.RegularExpressions.Regex.Match(message, pattern, System.Text.RegularExpressions.RegexOptions.IgnoreCase);
                if (match.Success && int.TryParse(match.Groups[1].Value, out var columnNumber))
                {
                    return columnNumber;
                }
            }
            
            return null;
        }
    }

    /// <summary>
    /// 错误信息
    /// </summary>
    public class ErrorInfo
    {
        public string ErrorCode { get; set; } = string.Empty;
        public string UserMessage { get; set; } = string.Empty;
        public string TechnicalMessage { get; set; } = string.Empty;
        public ErrorSeverity Severity { get; set; }
        public string? Suggestion { get; set; }
        public int? LineNumber { get; set; }
        public int? ColumnNumber { get; set; }
    }

    /// <summary>
    /// 错误严重程度
    /// </summary>
    public enum ErrorSeverity
    {
        Info,
        Warning,
        Error,
        Critical
    }
}