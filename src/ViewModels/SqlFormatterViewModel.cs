using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.ApplicationLifetimes;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

namespace DevUtilities.ViewModels;

public partial class SqlFormatterViewModel : BaseToolViewModel
{
    [ObservableProperty]
    private string inputSql = "";

    [ObservableProperty]
    private string outputSql = "";

    [ObservableProperty]
    private string validationMessage = "";

    [ObservableProperty]
    private bool isValidSql = true;

    [ObservableProperty]
    private int indentSize = 2;

    [ObservableProperty]
    private bool uppercaseKeywords = true;

    [ObservableProperty]
    private bool addLineBreaks = true;

    [ObservableProperty]
    private bool alignColumns = true;

    [ObservableProperty]
    private string selectedDialect = "Standard SQL";

    public List<string> AvailableDialects { get; } = new()
    {
        "Standard SQL",
        "MySQL",
        "PostgreSQL",
        "SQL Server",
        "Oracle",
        "SQLite"
    };

    public SqlFormatterViewModel()
    {
        // BaseToolViewModel只有Message属性，不需要设置Title、Icon、Description
    }

    partial void OnInputSqlChanged(string value)
    {
        if (!string.IsNullOrWhiteSpace(value))
        {
            ValidateSql();
        }
        else
        {
            ClearValidation();
        }
    }

    [RelayCommand]
    private void FormatSql()
    {
        try
        {
            if (string.IsNullOrWhiteSpace(InputSql))
            {
                OutputSql = "";
                ClearValidation();
                return;
            }

            var formatted = FormatSqlQuery(InputSql);
            OutputSql = formatted;
            SetValidation("SQL格式化成功", true);
        }
        catch (Exception ex)
        {
            SetValidation($"格式化失败: {ex.Message}", false);
        }
    }

    [RelayCommand]
    private void MinifySql()
    {
        try
        {
            if (string.IsNullOrWhiteSpace(InputSql))
            {
                OutputSql = "";
                ClearValidation();
                return;
            }

            var minified = MinifySqlQuery(InputSql);
            OutputSql = minified;
            SetValidation("SQL压缩成功", true);
        }
        catch (Exception ex)
        {
            SetValidation($"压缩失败: {ex.Message}", false);
        }
    }

    [RelayCommand]
    private void ValidateSql()
    {
        try
        {
            if (string.IsNullOrWhiteSpace(InputSql))
            {
                ClearValidation();
                return;
            }

            var validation = ValidateSqlSyntax(InputSql);
            SetValidation(validation.message, validation.isValid);
        }
        catch (Exception ex)
        {
            SetValidation($"验证失败: {ex.Message}", false);
        }
    }

    [RelayCommand]
    private void SwapInputOutput()
    {
        if (!string.IsNullOrWhiteSpace(OutputSql))
        {
            var temp = InputSql;
            InputSql = OutputSql;
            OutputSql = temp;
        }
    }

    [RelayCommand]
    private void ClearAll()
    {
        InputSql = "";
        OutputSql = "";
        ClearValidation();
    }

    [RelayCommand]
    private void UseExample()
    {
        InputSql = @"SELECT u.id, u.name, u.email, p.title, p.content, c.name as category FROM users u LEFT JOIN posts p ON u.id = p.user_id INNER JOIN categories c ON p.category_id = c.id WHERE u.status = 'active' AND p.published_at > '2023-01-01' ORDER BY p.published_at DESC, u.name ASC LIMIT 10;";
    }

    [RelayCommand]
    private async Task CopyOutput()
    {
        if (string.IsNullOrWhiteSpace(OutputSql)) return;

        try
        {
            if (Application.Current?.ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
            {
                var clipboard = desktop.MainWindow?.Clipboard;
                if (clipboard != null)
                {
                    await clipboard.SetTextAsync(OutputSql);
                }
            }
        }
        catch (Exception ex)
        {
            SetValidation($"复制失败: {ex.Message}", false);
        }
    }

    private string FormatSqlQuery(string sql)
    {
        var result = sql.Trim();
        
        // 移除多余的空白字符
        result = Regex.Replace(result, @"\s+", " ");
        
        // SQL关键字列表
        var keywords = new[]
        {
            "SELECT", "FROM", "WHERE", "JOIN", "INNER JOIN", "LEFT JOIN", "RIGHT JOIN", "FULL JOIN",
            "ON", "AND", "OR", "ORDER BY", "GROUP BY", "HAVING", "LIMIT", "OFFSET", "UNION",
            "INSERT", "INTO", "VALUES", "UPDATE", "SET", "DELETE", "CREATE", "TABLE", "ALTER",
            "DROP", "INDEX", "VIEW", "PROCEDURE", "FUNCTION", "TRIGGER", "DATABASE", "SCHEMA",
            "AS", "DISTINCT", "COUNT", "SUM", "AVG", "MAX", "MIN", "CASE", "WHEN", "THEN", "ELSE", "END"
        };

        // 转换关键字大小写
        if (UppercaseKeywords)
        {
            foreach (var keyword in keywords.OrderByDescending(k => k.Length))
            {
                var pattern = @"\b" + Regex.Escape(keyword) + @"\b";
                result = Regex.Replace(result, pattern, keyword.ToUpper(), RegexOptions.IgnoreCase);
            }
        }

        if (AddLineBreaks)
        {
            // 添加换行符
            var lineBreakKeywords = new[] { "SELECT", "FROM", "WHERE", "JOIN", "INNER JOIN", "LEFT JOIN", "RIGHT JOIN", "FULL JOIN", "ORDER BY", "GROUP BY", "HAVING", "UNION" };
            
            foreach (var keyword in lineBreakKeywords)
            {
                var pattern = @"\b" + Regex.Escape(keyword.ToUpper()) + @"\b";
                result = Regex.Replace(result, pattern, "\n" + keyword.ToUpper(), RegexOptions.IgnoreCase);
            }

            // 处理缩进
            var lines = result.Split('\n');
            var indentedLines = new List<string>();
            var indent = new string(' ', IndentSize);

            foreach (var line in lines)
            {
                var trimmedLine = line.Trim();
                if (string.IsNullOrEmpty(trimmedLine)) continue;

                if (trimmedLine.StartsWith("SELECT", StringComparison.OrdinalIgnoreCase))
                {
                    indentedLines.Add(trimmedLine);
                }
                else if (trimmedLine.StartsWith("FROM", StringComparison.OrdinalIgnoreCase) ||
                         trimmedLine.StartsWith("WHERE", StringComparison.OrdinalIgnoreCase) ||
                         trimmedLine.StartsWith("ORDER BY", StringComparison.OrdinalIgnoreCase) ||
                         trimmedLine.StartsWith("GROUP BY", StringComparison.OrdinalIgnoreCase) ||
                         trimmedLine.StartsWith("HAVING", StringComparison.OrdinalIgnoreCase))
                {
                    indentedLines.Add(trimmedLine);
                }
                else if (trimmedLine.Contains("JOIN"))
                {
                    indentedLines.Add(indent + trimmedLine);
                }
                else
                {
                    indentedLines.Add(indent + trimmedLine);
                }
            }

            result = string.Join("\n", indentedLines);
        }

        // 处理列对齐
        if (AlignColumns && result.Contains("SELECT"))
        {
            result = AlignSelectColumns(result);
        }

        return result.Trim();
    }

    private string AlignSelectColumns(string sql)
    {
        var lines = sql.Split('\n');
        var selectIndex = -1;
        
        for (int i = 0; i < lines.Length; i++)
        {
            if (lines[i].Trim().StartsWith("SELECT", StringComparison.OrdinalIgnoreCase))
            {
                selectIndex = i;
                break;
            }
        }

        if (selectIndex == -1) return sql;

        var selectLine = lines[selectIndex];
        var selectMatch = Regex.Match(selectLine, @"SELECT\s+(.+)", RegexOptions.IgnoreCase);
        
        if (selectMatch.Success)
        {
            var columns = selectMatch.Groups[1].Value;
            var columnList = columns.Split(',').Select(c => c.Trim()).ToList();
            
            if (columnList.Count > 1)
            {
                var indent = new string(' ', IndentSize);
                lines[selectIndex] = "SELECT " + columnList[0];
                
                for (int i = 1; i < columnList.Count; i++)
                {
                    var newLine = indent + "     , " + columnList[i];
                    Array.Resize(ref lines, lines.Length + 1);
                    Array.Copy(lines, selectIndex + i, lines, selectIndex + i + 1, lines.Length - selectIndex - i - 1);
                    lines[selectIndex + i] = newLine;
                }
            }
        }

        return string.Join("\n", lines);
    }

    private string MinifySqlQuery(string sql)
    {
        var result = sql.Trim();
        
        // 移除注释
        result = Regex.Replace(result, @"--.*$", "", RegexOptions.Multiline);
        result = Regex.Replace(result, @"/\*.*?\*/", "", RegexOptions.Singleline);
        
        // 移除多余的空白字符
        result = Regex.Replace(result, @"\s+", " ");
        
        // 移除分号前的空格
        result = Regex.Replace(result, @"\s+;", ";");
        
        return result.Trim();
    }

    private (bool isValid, string message) ValidateSqlSyntax(string sql)
    {
        if (string.IsNullOrWhiteSpace(sql))
        {
            return (false, "SQL语句不能为空");
        }

        var trimmedSql = sql.Trim();

        // 基本语法检查
        var issues = new List<string>();

        // 检查括号匹配
        var openParens = trimmedSql.Count(c => c == '(');
        var closeParens = trimmedSql.Count(c => c == ')');
        if (openParens != closeParens)
        {
            issues.Add($"括号不匹配 (开括号: {openParens}, 闭括号: {closeParens})");
        }

        // 检查引号匹配
        var singleQuotes = trimmedSql.Count(c => c == '\'');
        if (singleQuotes % 2 != 0)
        {
            issues.Add("单引号不匹配");
        }

        var doubleQuotes = trimmedSql.Count(c => c == '"');
        if (doubleQuotes % 2 != 0)
        {
            issues.Add("双引号不匹配");
        }

        // 检查基本SQL语句类型
        var sqlUpper = trimmedSql.ToUpper();
        var validStarters = new[] { "SELECT", "INSERT", "UPDATE", "DELETE", "CREATE", "ALTER", "DROP", "WITH" };
        
        if (!validStarters.Any(starter => sqlUpper.StartsWith(starter)))
        {
            issues.Add("SQL语句应以有效的关键字开始 (SELECT, INSERT, UPDATE, DELETE, CREATE, ALTER, DROP, WITH)");
        }

        if (issues.Any())
        {
            return (false, "语法问题: " + string.Join("; ", issues));
        }

        return (true, "SQL语法验证通过");
    }

    private void SetValidation(string message, bool isValid)
    {
        ValidationMessage = message;
        IsValidSql = isValid;
    }

    private void ClearValidation()
    {
        ValidationMessage = "";
        IsValidSql = true;
    }
}