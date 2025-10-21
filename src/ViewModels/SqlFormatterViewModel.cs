using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using DevUtilities.Core.ViewModels.Base;

namespace DevUtilities.ViewModels;

public partial class SqlFormatterViewModel : BaseFormatterViewModel
{
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
        Title = "SQL格式化器";
        Description = "SQL语句格式化和美化";
        Icon = "🗃️";
        ToolType = Models.ToolType.SqlFormatter;
    }

    protected override Task<string> FormatContentAsync(string input)
    {
        try
        {
            if (CompactOutput)
            {
                return Task.FromResult(MinifySqlQuery(input));
            }
            else
            {
                return Task.FromResult(FormatSqlQuery(input));
            }
        }
        catch (Exception ex)
        {
            return Task.FromResult($"格式化失败: {ex.Message}");
        }
    }

    // SQL特定的命令
    [RelayCommand]
    private void MinifySql()
    {
        CompactOutput = true;
        FormatCommand.Execute(null);
    }

    [RelayCommand]
    private void BeautifySql()
    {
        CompactOutput = false;
        FormatCommand.Execute(null);
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
            var indentChar = UseTabsForIndent ? "\t" : " ";
            var indent = new string(indentChar[0], UseTabsForIndent ? 1 : IndentSize);

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

    protected override Task<ValidationResult> OnValidateAsync(string input)
    {
        if (string.IsNullOrWhiteSpace(input))
        {
            return Task.FromResult(new ValidationResult(false, "请输入SQL语句"));
        }

        try
        {
            // 简单的SQL语法检查
            var trimmedInput = input.Trim();
            
            // 检查是否包含基本的SQL关键字
            var sqlKeywords = new[] { "SELECT", "INSERT", "UPDATE", "DELETE", "CREATE", "DROP", "ALTER", "WITH" };
            var upperInput = trimmedInput.ToUpper();
            
            bool containsSqlKeyword = sqlKeywords.Any(keyword => upperInput.Contains(keyword));
            
            if (!containsSqlKeyword)
            {
                return Task.FromResult(new ValidationResult(false, "输入内容不像是有效的SQL语句"));
            }

            // 检查基本的括号匹配
            int openParens = trimmedInput.Count(c => c == '(');
            int closeParens = trimmedInput.Count(c => c == ')');
            
            if (openParens != closeParens)
            {
                return Task.FromResult(new ValidationResult(false, "括号不匹配"));
            }

            return Task.FromResult(new ValidationResult(true, "SQL语句格式正确"));
        }
        catch (Exception ex)
        {
            return Task.FromResult(new ValidationResult(false, $"验证失败: {ex.Message}"));
        }
    }

    /// <summary>
    /// 验证基本SQL语法
    /// </summary>
    private void ValidateBasicSyntax(string sql, List<string> issues)
    {
        // 检查括号匹配
        var openParens = sql.Count(c => c == '(');
        var closeParens = sql.Count(c => c == ')');
        if (openParens != closeParens)
        {
            issues.Add("括号不匹配");
        }

        // 检查引号匹配
        var singleQuotes = sql.Count(c => c == '\'');
        if (singleQuotes % 2 != 0)
        {
            issues.Add("单引号不匹配");
        }

        // 检查基本SQL关键字
        var hasValidKeyword = Regex.IsMatch(sql, @"\b(SELECT|INSERT|UPDATE|DELETE|CREATE|ALTER|DROP)\b", RegexOptions.IgnoreCase);
        if (!hasValidKeyword)
        {
            issues.Add("未找到有效的SQL关键字");
        }

        // 检查SELECT语句的基本结构
        if (Regex.IsMatch(sql, @"\bSELECT\b", RegexOptions.IgnoreCase))
        {
            if (!Regex.IsMatch(sql, @"\bFROM\b", RegexOptions.IgnoreCase) && 
                !Regex.IsMatch(sql, @"SELECT\s+\d+", RegexOptions.IgnoreCase))
            {
                issues.Add("SELECT语句缺少FROM子句");
            }
        }
    }

    /// <summary>
    /// 分析SQL结构
    /// </summary>
    private SqlStructureInfo AnalyzeSqlStructure(string sql)
    {
        var info = new SqlStructureInfo();
        
        // 统计各种子句
        info.HasSelect = Regex.IsMatch(sql, @"\bSELECT\b", RegexOptions.IgnoreCase);
        info.HasFrom = Regex.IsMatch(sql, @"\bFROM\b", RegexOptions.IgnoreCase);
        info.HasWhere = Regex.IsMatch(sql, @"\bWHERE\b", RegexOptions.IgnoreCase);
        info.HasJoin = Regex.IsMatch(sql, @"\b(JOIN|INNER JOIN|LEFT JOIN|RIGHT JOIN|FULL JOIN)\b", RegexOptions.IgnoreCase);
        info.HasOrderBy = Regex.IsMatch(sql, @"\bORDER BY\b", RegexOptions.IgnoreCase);
        info.HasGroupBy = Regex.IsMatch(sql, @"\bGROUP BY\b", RegexOptions.IgnoreCase);
        
        // 统计表数量
        var fromMatches = Regex.Matches(sql, @"\bFROM\s+(\w+)", RegexOptions.IgnoreCase);
        var joinMatches = Regex.Matches(sql, @"\bJOIN\s+(\w+)", RegexOptions.IgnoreCase);
        info.TableCount = fromMatches.Count + joinMatches.Count;
        
        // 统计字段数量
        if (info.HasSelect)
        {
            var selectMatch = Regex.Match(sql, @"SELECT\s+(.*?)\s+FROM", RegexOptions.IgnoreCase | RegexOptions.Singleline);
            if (selectMatch.Success)
            {
                var fields = selectMatch.Groups[1].Value;
                if (fields.Trim() == "*")
                {
                    info.FieldCount = -1; // 表示使用了 *
                }
                else
                {
                    info.FieldCount = fields.Split(',').Length;
                }
            }
        }
        
        return info;
    }

    /// <summary>
    /// 检查性能问题
    /// </summary>
    private void CheckPerformanceIssues(string sql, List<string> warnings)
    {
        // 检查SELECT *
        if (Regex.IsMatch(sql, @"SELECT\s+\*", RegexOptions.IgnoreCase))
        {
            warnings.Add("建议避免使用 SELECT *，明确指定需要的字段");
        }

        // 检查缺少WHERE子句
        if (Regex.IsMatch(sql, @"\b(UPDATE|DELETE)\b", RegexOptions.IgnoreCase) &&
            !Regex.IsMatch(sql, @"\bWHERE\b", RegexOptions.IgnoreCase))
        {
            warnings.Add("UPDATE/DELETE语句缺少WHERE条件，可能影响所有记录");
        }

        // 检查LIKE的使用
        if (Regex.IsMatch(sql, @"LIKE\s+'%.*%'", RegexOptions.IgnoreCase))
        {
            warnings.Add("使用前后通配符的LIKE查询可能影响性能");
        }
    }

    /// <summary>
    /// 构建验证消息
    /// </summary>
    private string BuildValidationMessage(SqlStructureInfo info, List<string> issues, List<string> warnings)
    {
        if (issues.Count > 0)
        {
            return $"SQL语法错误: {string.Join(", ", issues)}";
        }

        var messageParts = new List<string> { "SQL语法正确" };
        
        if (info.HasSelect)
        {
            var structureParts = new List<string>();
            if (info.TableCount > 0) structureParts.Add($"{info.TableCount}个表");
            if (info.FieldCount > 0) structureParts.Add($"{info.FieldCount}个字段");
            else if (info.FieldCount == -1) structureParts.Add("所有字段(*)");
            if (info.HasJoin) structureParts.Add("包含连接");
            if (info.HasWhere) structureParts.Add("包含条件");
            if (info.HasOrderBy) structureParts.Add("包含排序");
            if (info.HasGroupBy) structureParts.Add("包含分组");
            
            if (structureParts.Count > 0)
            {
                messageParts.Add($"({string.Join(", ", structureParts)})");
            }
        }

        if (warnings.Count > 0)
        {
            messageParts.Add($"建议: {string.Join("; ", warnings)}");
        }

        return string.Join(" ", messageParts);
    }

    /// <summary>
    /// SQL结构信息
    /// </summary>
    private class SqlStructureInfo
    {
        public bool HasSelect { get; set; }
        public bool HasFrom { get; set; }
        public bool HasWhere { get; set; }
        public bool HasJoin { get; set; }
        public bool HasOrderBy { get; set; }
        public bool HasGroupBy { get; set; }
        public int TableCount { get; set; }
        public int FieldCount { get; set; }
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
                var indentChar = UseTabsForIndent ? "\t" : " ";
                var indent = new string(indentChar[0], UseTabsForIndent ? 1 : IndentSize);
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

    protected override string GetExampleData()
    {
        return """
        SELECT u.id, u.name, u.email, p.title, p.content, c.name as category_name 
        FROM users u 
        INNER JOIN posts p ON u.id = p.user_id 
        LEFT JOIN categories c ON p.category_id = c.id 
        WHERE u.status = 'active' AND p.published_at IS NOT NULL 
        ORDER BY p.created_at DESC, u.name ASC 
        LIMIT 10 OFFSET 0;
        """;
    }
}