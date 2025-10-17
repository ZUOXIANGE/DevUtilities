using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

namespace DevUtilities.ViewModels;

public partial class CronExpressionViewModel : ObservableObject
{
    [ObservableProperty]
    private string cronExpression = "0 0 12 * * ?";

    [ObservableProperty]
    private string description = "";

    [ObservableProperty]
    private string validationMessage = "";

    [ObservableProperty]
    private bool isValidExpression = true;

    [ObservableProperty]
    private string nextExecutions = "";

    // 预设的常用Cron表达式
    public Dictionary<string, string> PresetExpressions { get; } = new()
    {
        { "每分钟", "0 * * * * ?" },
        { "每小时", "0 0 * * * ?" },
        { "每天中午12点", "0 0 12 * * ?" },
        { "每天午夜", "0 0 0 * * ?" },
        { "每周一上午9点", "0 0 9 ? * MON" },
        { "每月1号上午10点", "0 0 10 1 * ?" },
        { "每年1月1号", "0 0 0 1 1 ?" },
        { "工作日上午9点", "0 0 9 ? * MON-FRI" },
        { "每5分钟", "0 */5 * * * ?" },
        { "每30分钟", "0 */30 * * * ?" }
    };

    // 字段选项
    public string[] SecondOptions { get; } = { "*", "0", "15", "30", "45" };
    public string[] MinuteOptions { get; } = { "*", "0", "15", "30", "45" };
    public string[] HourOptions { get; } = { "*", "0", "6", "9", "12", "18", "23" };
    public string[] DayOptions { get; } = { "*", "?", "1", "15", "L" };
    public string[] MonthOptions { get; } = { "*", "1", "3", "6", "12" };
    public string[] WeekOptions { get; } = { "?", "*", "MON", "TUE", "WED", "THU", "FRI", "SAT", "SUN" };

    [ObservableProperty]
    private string selectedSecond = "0";

    [ObservableProperty]
    private string selectedMinute = "0";

    [ObservableProperty]
    private string selectedHour = "12";

    [ObservableProperty]
    private string selectedDay = "*";

    [ObservableProperty]
    private string selectedMonth = "*";

    [ObservableProperty]
    private string selectedWeek = "?";

    public CronExpressionViewModel()
    {
        ParseExpression();
    }

    [RelayCommand]
    private void ParseExpression()
    {
        try
        {
            if (string.IsNullOrWhiteSpace(CronExpression))
            {
                ValidationMessage = "请输入Cron表达式";
                IsValidExpression = false;
                return;
            }

            var parts = CronExpression.Trim().Split(' ', StringSplitOptions.RemoveEmptyEntries);
            
            if (parts.Length != 6)
            {
                ValidationMessage = "Cron表达式格式错误，应包含6个字段：秒 分 时 日 月 周";
                IsValidExpression = false;
                return;
            }

            // 解析各个字段
            SelectedSecond = parts[0];
            SelectedMinute = parts[1];
            SelectedHour = parts[2];
            SelectedDay = parts[3];
            SelectedMonth = parts[4];
            SelectedWeek = parts[5];

            // 生成描述
            Description = GenerateDescription(parts);
            
            // 计算下次执行时间
            NextExecutions = CalculateNextExecutions();

            ValidationMessage = "表达式解析成功";
            IsValidExpression = true;
        }
        catch (Exception ex)
        {
            ValidationMessage = $"解析错误: {ex.Message}";
            IsValidExpression = false;
            Description = "";
            NextExecutions = "";
        }
    }

    [RelayCommand]
    private void GenerateExpression()
    {
        try
        {
            CronExpression = $"{SelectedSecond} {SelectedMinute} {SelectedHour} {SelectedDay} {SelectedMonth} {SelectedWeek}";
            ParseExpression();
        }
        catch (Exception ex)
        {
            ValidationMessage = $"生成错误: {ex.Message}";
            IsValidExpression = false;
        }
    }

    [RelayCommand]
    private void UsePreset(string preset)
    {
        if (PresetExpressions.TryGetValue(preset, out var expression))
        {
            CronExpression = expression;
            ParseExpression();
        }
    }

    [RelayCommand]
    private void Clear()
    {
        CronExpression = "";
        Description = "";
        ValidationMessage = "";
        NextExecutions = "";
        IsValidExpression = true;
        
        SelectedSecond = "0";
        SelectedMinute = "0";
        SelectedHour = "12";
        SelectedDay = "*";
        SelectedMonth = "*";
        SelectedWeek = "?";
    }

    private string GenerateDescription(string[] parts)
    {
        var sb = new StringBuilder();
        
        try
        {
            // 秒
            var second = parts[0];
            if (second == "*")
                sb.Append("每秒");
            else if (second.StartsWith("*/"))
                sb.Append($"每{second[2..]}秒");
            else
                sb.Append($"第{second}秒");

            sb.Append(" ");

            // 分
            var minute = parts[1];
            if (minute == "*")
                sb.Append("每分钟");
            else if (minute.StartsWith("*/"))
                sb.Append($"每{minute[2..]}分钟");
            else
                sb.Append($"第{minute}分钟");

            sb.Append(" ");

            // 时
            var hour = parts[2];
            if (hour == "*")
                sb.Append("每小时");
            else if (hour.StartsWith("*/"))
                sb.Append($"每{hour[2..]}小时");
            else
                sb.Append($"{hour}点");

            sb.Append(" ");

            // 日和周
            var day = parts[3];
            var week = parts[5];
            
            if (day != "?" && week == "?")
            {
                if (day == "*")
                    sb.Append("每天");
                else if (day == "L")
                    sb.Append("每月最后一天");
                else
                    sb.Append($"每月{day}号");
            }
            else if (day == "?" && week != "?")
            {
                if (week == "*")
                    sb.Append("每天");
                else
                    sb.Append($"每周{TranslateWeekday(week)}");
            }
            else if (day == "*" && week == "*")
            {
                sb.Append("每天");
            }

            sb.Append(" ");

            // 月
            var month = parts[4];
            if (month == "*")
                sb.Append("每月");
            else
                sb.Append($"{month}月");

            return sb.ToString().Trim();
        }
        catch
        {
            return "复杂表达式";
        }
    }

    private string TranslateWeekday(string weekday)
    {
        return weekday.ToUpper() switch
        {
            "MON" or "1" => "一",
            "TUE" or "2" => "二", 
            "WED" or "3" => "三",
            "THU" or "4" => "四",
            "FRI" or "5" => "五",
            "SAT" or "6" => "六",
            "SUN" or "0" or "7" => "日",
            "MON-FRI" => "一到五",
            "SAT-SUN" => "六到日",
            _ => weekday
        };
    }

    private string CalculateNextExecutions()
    {
        try
        {
            var sb = new StringBuilder();
            var now = DateTime.Now;
            var executions = new List<DateTime>();

            // 简化的下次执行时间计算（实际项目中建议使用专业的Cron库如Quartz.NET）
            for (int i = 0; i < 5; i++)
            {
                var nextTime = CalculateNextExecution(now.AddMinutes(i));
                if (nextTime.HasValue)
                {
                    executions.Add(nextTime.Value);
                }
            }

            if (executions.Any())
            {
                sb.AppendLine("接下来的执行时间：");
                foreach (var execution in executions.Take(5))
                {
                    sb.AppendLine($"• {execution:yyyy-MM-dd HH:mm:ss}");
                }
            }
            else
            {
                sb.AppendLine("无法计算下次执行时间");
            }

            return sb.ToString();
        }
        catch
        {
            return "计算执行时间时出错";
        }
    }

    private DateTime? CalculateNextExecution(DateTime from)
    {
        // 这是一个简化的实现，实际项目中建议使用专业的Cron解析库
        try
        {
            var parts = CronExpression.Split(' ');
            if (parts.Length != 6) return null;

            var second = ParseField(parts[0], 0, 59);
            var minute = ParseField(parts[1], 0, 59);
            var hour = ParseField(parts[2], 0, 23);

            // 简单处理：如果是具体的时间值，返回下一个匹配的时间
            if (int.TryParse(parts[0], out var sec) && 
                int.TryParse(parts[1], out var min) && 
                int.TryParse(parts[2], out var hr))
            {
                var next = new DateTime(from.Year, from.Month, from.Day, hr, min, sec);
                if (next <= from)
                {
                    next = next.AddDays(1);
                }
                return next;
            }

            return from.AddHours(1); // 默认返回一小时后
        }
        catch
        {
            return null;
        }
    }

    private List<int> ParseField(string field, int min, int max)
    {
        var result = new List<int>();
        
        if (field == "*")
        {
            for (int i = min; i <= max; i++)
                result.Add(i);
        }
        else if (field.Contains("/"))
        {
            var parts = field.Split('/');
            if (parts.Length == 2 && int.TryParse(parts[1], out var step))
            {
                var start = parts[0] == "*" ? min : int.Parse(parts[0]);
                for (int i = start; i <= max; i += step)
                    result.Add(i);
            }
        }
        else if (int.TryParse(field, out var value))
        {
            result.Add(value);
        }

        return result;
    }

    partial void OnCronExpressionChanged(string value)
    {
        if (!string.IsNullOrEmpty(value))
        {
            ParseExpression();
        }
    }
}