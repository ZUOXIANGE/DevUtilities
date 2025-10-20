using System;

namespace DevUtilities.Core.Exceptions
{
    /// <summary>
    /// 格式化工具异常基类
    /// </summary>
    public class FormatterException : Exception
    {
        public string ErrorCode { get; protected set; }
        public string UserFriendlyMessage { get; }

        public FormatterException(string message) : base(message)
        {
            ErrorCode = "FORMATTER_ERROR";
            UserFriendlyMessage = message;
        }

        public FormatterException(string message, Exception innerException) : base(message, innerException)
        {
            ErrorCode = "FORMATTER_ERROR";
            UserFriendlyMessage = message;
        }
    }

    /// <summary>
    /// JSON格式化异常
    /// </summary>
    public class JsonFormatterException : FormatterException
    {
        public int? LineNumber { get; }
        public int? ColumnNumber { get; }

        public JsonFormatterException(string message) : base(message)
        {
            ErrorCode = "JSON_FORMAT_ERROR";
        }

        public JsonFormatterException(string message, Exception innerException) : base(message, innerException)
        {
            ErrorCode = "JSON_FORMAT_ERROR";
        }

        private static string GetUserFriendlyMessage(string originalMessage, int? lineNumber, int? columnNumber)
        {
            var location = "";
            if (lineNumber.HasValue && columnNumber.HasValue)
            {
                location = $"（第{lineNumber}行，第{columnNumber}列）";
            }
            else if (lineNumber.HasValue)
            {
                location = $"（第{lineNumber}行）";
            }

            if (originalMessage.Contains("unexpected character"))
            {
                return $"JSON格式错误：存在意外字符{location}";
            }
            if (originalMessage.Contains("unterminated string"))
            {
                return $"JSON格式错误：字符串未正确结束{location}";
            }
            if (originalMessage.Contains("invalid number"))
            {
                return $"JSON格式错误：数字格式不正确{location}";
            }
            if (originalMessage.Contains("expected"))
            {
                return $"JSON格式错误：缺少必要的符号{location}";
            }

            return $"JSON格式错误：{originalMessage}{location}";
        }
    }

    /// <summary>
    /// SQL格式化异常
    /// </summary>
    public class SqlFormatterException : FormatterException
    {
        public string? SqlStatement { get; }

        public SqlFormatterException(string message, string? sqlStatement = null) : base(message)
        {
            ErrorCode = "SQL_FORMAT_ERROR";
            SqlStatement = sqlStatement;
        }

        public SqlFormatterException(string message, string? sqlStatement, Exception innerException) : base(message, innerException)
        {
            ErrorCode = "SQL_FORMAT_ERROR";
            SqlStatement = sqlStatement;
        }

        private static string GetUserFriendlyMessage(string originalMessage)
        {
            if (originalMessage.Contains("syntax error"))
            {
                return "SQL语法错误：请检查SQL语句的语法";
            }
            if (originalMessage.Contains("missing"))
            {
                return "SQL语法错误：缺少必要的关键字或符号";
            }
            if (originalMessage.Contains("unexpected"))
            {
                return "SQL语法错误：存在意外的字符或关键字";
            }

            return $"SQL处理错误：{originalMessage}";
        }
    }

    /// <summary>
    /// HTML格式化异常
    /// </summary>
    public class HtmlFormatterException : FormatterException
    {
        public string? TagName { get; }
        public int? LineNumber { get; }

        public HtmlFormatterException(string message) : base(message)
        {
            ErrorCode = "HTML_FORMAT_ERROR";
            TagName = null;
            LineNumber = null;
        }

        public HtmlFormatterException(string message, Exception innerException) : base(message, innerException)
        {
            ErrorCode = "HTML_FORMAT_ERROR";
            TagName = null;
            LineNumber = null;
        }

        private static string GetUserFriendlyMessage(string originalMessage, string tagName, int? lineNumber)
        {
            var location = lineNumber.HasValue ? $"（第{lineNumber}行）" : "";
            var tag = !string.IsNullOrEmpty(tagName) ? $"标签 <{tagName}>" : "";

            if (originalMessage.Contains("unclosed"))
            {
                return $"HTML结构错误：{tag}未正确闭合{location}";
            }
            if (originalMessage.Contains("mismatch"))
            {
                return $"HTML结构错误：{tag}不匹配{location}";
            }
            if (originalMessage.Contains("invalid"))
            {
                return $"HTML格式错误：{tag}格式不正确{location}";
            }

            return $"HTML处理错误：{originalMessage}{location}";
        }
    }

    /// <summary>
    /// 性能异常
    /// </summary>
    public class PerformanceException : FormatterException
    {
        public long ProcessingTime { get; }
        public long FileSize { get; }

        public PerformanceException(string message) : base(message)
    {
        ErrorCode = "PERFORMANCE_WARNING";
    }

    public PerformanceException(string message, Exception innerException) : base(message, innerException)
    {
        ErrorCode = "PERFORMANCE_WARNING";
    }
}
}