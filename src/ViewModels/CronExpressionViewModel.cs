using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Avalonia.Threading;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Serilog;

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

    // 字段选项 - 符合Cron表达式规范
    public string[] SecondOptions { get; } = { "*", "0", "5", "10", "15", "20", "25", "30", "35", "40", "45", "50", "55", "*/5", "*/10", "*/15", "*/30" };
    public string[] MinuteOptions { get; } = { "*", "0", "5", "10", "15", "20", "25", "30", "35", "40", "45", "50", "55", "*/5", "*/10", "*/15", "*/30" };
    public string[] HourOptions { get; } = { "*", "0", "1", "2", "3", "4", "5", "6", "7", "8", "9", "10", "11", "12", "13", "14", "15", "16", "17", "18", "19", "20", "21", "22", "23", "*/2", "*/3", "*/4", "*/6", "*/8", "*/12" };
    public string[] DayOptions { get; } = { "*", "?", "1", "2", "3", "4", "5", "6", "7", "8", "9", "10", "11", "12", "13", "14", "15", "16", "17", "18", "19", "20", "21", "22", "23", "24", "25", "26", "27", "28", "29", "30", "31", "L", "LW", "1W", "15W" };
    public string[] MonthOptions { get; } = { "*", "1", "2", "3", "4", "5", "6", "7", "8", "9", "10", "11", "12", "JAN", "FEB", "MAR", "APR", "MAY", "JUN", "JUL", "AUG", "SEP", "OCT", "NOV", "DEC", "*/2", "*/3", "*/4", "*/6" };
    public string[] WeekOptions { get; } = { "?", "*", "1", "2", "3", "4", "5", "6", "7", "SUN", "MON", "TUE", "WED", "THU", "FRI", "SAT", "MON-FRI", "SAT,SUN", "1#1", "1#2", "1#3", "1#4", "1#5", "1L", "2L", "3L", "4L", "5L", "6L", "7L" };

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
        // 初始化时生成默认表达式
        CronExpression = $"{SelectedSecond} {SelectedMinute} {SelectedHour} {SelectedDay} {SelectedMonth} {SelectedWeek}";
        ParseExpression();
    }

    private bool _isUpdatingFields = false;

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

            // 设置标志避免循环调用
            _isUpdatingFields = true;
            try
            {
                // 解析各个字段
                SelectedSecond = parts[0];
                SelectedMinute = parts[1];
                SelectedHour = parts[2];
                SelectedDay = parts[3];
                SelectedMonth = parts[4];
                SelectedWeek = parts[5];
            }
            finally
            {
                _isUpdatingFields = false;
            }

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
        if (!string.IsNullOrEmpty(preset) && PresetExpressions.ContainsValue(preset))
        {
            CronExpression = preset;
            ParseExpression();
        }
    }

    [RelayCommand]
    private void RefreshExecutions()
    {
        try
        {
            if (IsValidExpression && !string.IsNullOrWhiteSpace(CronExpression))
            {
                NextExecutions = CalculateNextExecutions();
            }
        }
        catch (Exception ex)
        {
            ValidationMessage = $"刷新执行时间错误: {ex.Message}";
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
        try
        {
            var expression = string.Join(" ", parts);
            
            // 预设表达式的直接映射
            var presetDescriptions = new Dictionary<string, string>
            {
                { "0 * * * * ?", "每分钟执行一次" },
                { "0 0 * * * ?", "每小时整点执行" },
                { "0 0 12 * * ?", "每天中午12点执行" },
                { "0 0 0 * * ?", "每天午夜执行" },
                { "0 0 9 ? * MON", "每周一上午9点执行" },
                { "0 0 10 1 * ?", "每月1号上午10点执行" },
                { "0 0 0 1 1 ?", "每年1月1号午夜执行" },
                { "0 0 9 ? * MON-FRI", "工作日（周一到周五）上午9点执行" },
                { "0 */5 * * * ?", "每5分钟执行一次" },
                { "0 */30 * * * ?", "每30分钟执行一次" }
            };

            if (presetDescriptions.TryGetValue(expression, out var presetDesc))
            {
                return presetDesc;
            }

            // 动态解析表达式
            var sb = new StringBuilder();
            
            var second = parts[0];
            var minute = parts[1];
            var hour = parts[2];
            var day = parts[3];
            var month = parts[4];
            var week = parts[5];

            // 构建时间描述
            if (second == "0" && minute == "0")
            {
                // 整点执行
                if (hour == "*")
                    sb.Append("每小时整点");
                else if (hour.StartsWith("*/"))
                    sb.Append($"每{hour[2..]}小时整点");
                else
                    sb.Append($"每天{hour}点整");
            }
            else if (second == "0")
            {
                // 整分执行
                if (minute == "*")
                    sb.Append("每分钟");
                else if (minute.StartsWith("*/"))
                    sb.Append($"每{minute[2..]}分钟");
                else
                    sb.Append($"每小时第{minute}分钟");
                
                if (hour != "*")
                {
                    if (hour.StartsWith("*/"))
                        sb.Append($"，每{hour[2..]}小时");
                    else
                        sb.Append($"，在{hour}点");
                }
            }
            else
            {
                // 包含秒的执行
                sb.Append($"在第{second}秒");
                if (minute != "*")
                    sb.Append($"第{minute}分");
                if (hour != "*")
                    sb.Append($"{hour}点");
            }

            // 添加日期限制
            if (day != "*" && day != "?" && week == "?")
            {
                if (day == "L")
                    sb.Append("，每月最后一天");
                else if (day.Contains("/"))
                    sb.Append($"，每{day.Split('/')[1]}天");
                else
                    sb.Append($"，每月{day}号");
            }
            else if (week != "*" && week != "?" && day == "?")
            {
                if (week.Contains("-"))
                    sb.Append($"，{TranslateWeekRange(week)}");
                else
                    sb.Append($"，每周{TranslateWeekday(week)}");
            }

            // 添加月份限制
            if (month != "*")
            {
                if (month.Contains("/"))
                    sb.Append($"，每{month.Split('/')[1]}个月");
                else
                    sb.Append($"，仅在{month}月");
            }

            sb.Append("执行");
            return sb.ToString();
        }
        catch
        {
            return "复杂的Cron表达式，请参考使用说明";
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

    private string TranslateWeekRange(string weekRange)
    {
        if (weekRange.Contains("-"))
        {
            var parts = weekRange.Split('-');
            if (parts.Length == 2)
            {
                var start = TranslateWeekday(parts[0]);
                var end = TranslateWeekday(parts[1]);
                return $"每周{start}到{end}";
            }
        }
        return $"每周{TranslateWeekday(weekRange)}";
    }

    private string CalculateNextExecutions()
    {
        try
        {
            // 添加调试信息
            if (string.IsNullOrWhiteSpace(CronExpression))
            {
                Log.Warning("Cron表达式为空，无法计算执行时间");
                return "表达式为空，无法计算执行时间";
            }

            var sb = new StringBuilder();
            var now = DateTime.Now;
            var executions = new List<DateTime>();

            // 添加调试信息
            Log.Information("开始计算Cron执行时间 - 表达式: {Expression}, 当前时间: {CurrentTime}", CronExpression, now);
            
            sb.AppendLine($"当前表达式: {CronExpression}");
            sb.AppendLine($"当前时间: {now:yyyy-MM-dd HH:mm:ss}");
            sb.AppendLine();

            // 计算接下来的5次执行时间
            var currentTime = now;
            for (int i = 0; i < 50 && executions.Count < 5; i++) // 最多检查50次，避免无限循环
            {
                var nextTime = CalculateNextExecution(currentTime);
                Log.Debug("第{Attempt}次尝试计算执行时间 - 从 {StartTime} 开始，结果: {Result}", 
                    i + 1, currentTime, nextTime?.ToString("yyyy-MM-dd HH:mm:ss") ?? "null");
                
                if (nextTime.HasValue && nextTime.Value > now)
                {
                    executions.Add(nextTime.Value);
                    currentTime = nextTime.Value.AddSeconds(1); // 移动到下一秒继续查找
                }
                else
                {
                    currentTime = currentTime.AddMinutes(1); // 如果没找到，向前移动1分钟
                }
            }

            if (executions.Any())
            {
                Log.Information("找到 {Count} 个执行时间", executions.Count);
                sb.AppendLine("接下来的执行时间：");
                foreach (var execution in executions)
                {
                    sb.AppendLine($"• {execution:yyyy-MM-dd HH:mm:ss} ({GetRelativeTime(execution, now)})");
                }
            }
            else
            {
                Log.Warning("没有找到任何执行时间");
                sb.AppendLine("无法计算下次执行时间，请检查表达式格式");
            }

            return sb.ToString();
        }
        catch (Exception ex)
        {
            Log.Error(ex, "计算Cron执行时间时发生异常 - 表达式: {Expression}", CronExpression);
            return $"计算执行时间时发生错误: {ex.Message}";
        }
    }

    private string GetRelativeTime(DateTime future, DateTime now)
    {
        var diff = future - now;
        if (diff.TotalMinutes < 60)
            return $"{(int)diff.TotalMinutes}分钟后";
        else if (diff.TotalHours < 24)
            return $"{(int)diff.TotalHours}小时后";
        else
            return $"{(int)diff.TotalDays}天后";
    }

    private DateTime? CalculateNextExecution(DateTime from)
    {
        try
        {
            Log.Debug("开始CalculateNextExecution - 输入时间: {FromTime}, 表达式: {Expression}", from, CronExpression);
            
            var parts = CronExpression.Split(' ');
            if (parts.Length != 6) 
            {
                Log.Warning("Cron表达式格式错误 - 期望6个字段，实际{Count}个字段: {Parts}", parts.Length, string.Join(", ", parts));
                return null;
            }

            Log.Debug("解析Cron字段 - 秒:{Sec} 分:{Min} 时:{Hour} 日:{Day} 月:{Month} 星期:{Week}", 
                parts[0], parts[1], parts[2], parts[3], parts[4], parts[5]);

            var secondValues = ParseField(parts[0], 0, 59);
            var minuteValues = ParseField(parts[1], 0, 59);
            var hourValues = ParseField(parts[2], 0, 23);
            var dayValues = ParseField(parts[3], 1, 31);
            var monthValues = ParseField(parts[4], 1, 12);

            Log.Debug("解析结果 - 秒:{Sec} 分:{Min} 时:{Hour} 日:{Day} 月:{Month}", 
                string.Join(",", secondValues), string.Join(",", minuteValues), 
                string.Join(",", hourValues), string.Join(",", dayValues), string.Join(",", monthValues));

            // 优化搜索策略：从年开始逐级递减搜索
            var searchTime = new DateTime(from.Year, from.Month, from.Day, from.Hour, from.Minute, from.Second);
            
            // 如果当前时间已经匹配，则从下一秒开始搜索
            if (IsTimeMatch(searchTime, secondValues, minuteValues, hourValues, dayValues, monthValues, parts[5]))
            {
                Log.Debug("当前时间已匹配，从下一秒开始搜索");
                searchTime = searchTime.AddSeconds(1);
            }

            // 限制搜索范围为接下来的3个月，避免长时间计算
            var endTime = from.AddMonths(3);
            var maxIterations = 100000; // 最大迭代次数限制
            var iterations = 0;

            Log.Debug("开始搜索 - 起始时间:{Start} 结束时间:{End}", searchTime, endTime);

            // 智能搜索：优先按分钟递增，然后按秒递增
            while (searchTime <= endTime && iterations < maxIterations)
            {
                iterations++;
                
                if (IsTimeMatch(searchTime, secondValues, minuteValues, hourValues, dayValues, monthValues, parts[5]))
                {
                    Log.Information("找到匹配时间 - {MatchTime} (迭代{Iterations}次)", searchTime, iterations);
                    return searchTime;
                }

                // 每1000次迭代输出一次进度信息，避免日志过于频繁
                if (iterations % 1000 == 0)
                {
                    Log.Debug("搜索进度 - 当前时间:{Current} 迭代次数:{Iterations}", searchTime, iterations);
                }

                // 优化搜索步长：如果秒字段是固定值或简单模式，可以跳过不匹配的秒
                if (CanOptimizeSecondSearch(parts[0], searchTime.Second))
                {
                    var nextValidSecond = GetNextValidSecond(parts[0], searchTime.Second);
                    if (nextValidSecond > searchTime.Second)
                    {
                        if (nextValidSecond >= 60)
                        {
                            // 需要进入下一分钟
                            searchTime = searchTime.AddMinutes(1);
                            searchTime = new DateTime(searchTime.Year, searchTime.Month, searchTime.Day, 
                                                    searchTime.Hour, searchTime.Minute, 0);
                        }
                        else
                        {
                            searchTime = new DateTime(searchTime.Year, searchTime.Month, searchTime.Day, 
                                                    searchTime.Hour, searchTime.Minute, nextValidSecond);
                        }
                        continue;
                    }
                }

                // 优化搜索步长：如果分钟字段是固定值或简单模式，可以跳过不匹配的分钟
                if (CanOptimizeMinuteSearch(parts[1], searchTime.Minute))
                {
                    var nextValidMinute = GetNextValidMinute(parts[1], searchTime.Minute);
                    if (nextValidMinute > searchTime.Minute)
                    {
                        if (nextValidMinute >= 60)
                        {
                            // 需要进入下一小时
                            searchTime = searchTime.AddHours(1);
                            searchTime = new DateTime(searchTime.Year, searchTime.Month, searchTime.Day, 
                                                    searchTime.Hour, 0, 0);
                        }
                        else
                        {
                            searchTime = new DateTime(searchTime.Year, searchTime.Month, searchTime.Day, 
                                                    searchTime.Hour, nextValidMinute, 0);
                        }
                        continue;
                    }
                }

                // 确保时间始终递增，避免无限循环
                searchTime = searchTime.AddSeconds(1);
            }

            return null;
        }
        catch (Exception ex)
        {
            Log.Error(ex, "CalculateNextExecution异常 - 输入时间:{FromTime}", from);
            return null;
        }
    }

    private bool CanOptimizeSecondSearch(string secondField, int currentSecond)
    {
        // 如果是固定值或简单的步长模式，可以优化
        return !secondField.Contains(",") && !secondField.Contains("-") && secondField != "*";
    }

    private int GetNextValidSecond(string secondField, int currentSecond)
    {
        if (int.TryParse(secondField, out var fixedSecond))
        {
            return fixedSecond > currentSecond ? fixedSecond : 60; // 返回60表示需要进入下一分钟
        }
        
        if (secondField.StartsWith("*/"))
        {
            if (int.TryParse(secondField[2..], out var step))
            {
                var nextStep = ((currentSecond / step) + 1) * step;
                return nextStep < 60 ? nextStep : 60;
            }
        }
        
        return currentSecond + 1;
    }

    private bool CanOptimizeMinuteSearch(string minuteField, int currentMinute)
    {
        return !minuteField.Contains(",") && !minuteField.Contains("-") && minuteField != "*";
    }

    private int GetNextValidMinute(string minuteField, int currentMinute)
    {
        if (int.TryParse(minuteField, out var fixedMinute))
        {
            return fixedMinute > currentMinute ? fixedMinute : 60; // 返回60表示需要进入下一小时
        }
        
        if (minuteField.StartsWith("*/"))
        {
            if (int.TryParse(minuteField[2..], out var step))
            {
                var nextStep = ((currentMinute / step) + 1) * step;
                return nextStep < 60 ? nextStep : 60;
            }
        }
        
        return currentMinute + 1;
    }

    private bool IsTimeMatch(DateTime time, List<int> seconds, List<int> minutes, List<int> hours, 
                           List<int> days, List<int> months, string weekField)
    {
        // 检查秒、分、时、月是否匹配
        var secondMatch = seconds.Contains(time.Second);
        var minuteMatch = minutes.Contains(time.Minute);
        var hourMatch = hours.Contains(time.Hour);
        var monthMatch = months.Contains(time.Month);
        
        if (!secondMatch || !minuteMatch || !hourMatch || !monthMatch)
        {
            return false;
        }

        // 处理日和周的匹配
        var dayField = CronExpression.Split(' ')[3];
        
        // 如果日字段是 "?"，则只检查周
        if (dayField == "?")
        {
            return IsWeekMatch(time, weekField);
        }
        // 如果周字段是 "?"，则只检查日
        else if (weekField == "?")
        {
            return days.Contains(time.Day);
        }
        // 如果都不是 "?"，则需要日或周匹配其中之一
        else
        {
            var dayMatch = days.Contains(time.Day);
            var weekMatch = IsWeekMatch(time, weekField);
            return dayMatch || weekMatch;
        }
    }

    private bool IsWeekMatch(DateTime time, string weekField)
    {
        if (weekField == "*" || weekField == "?") return true;

        var dayOfWeek = (int)time.DayOfWeek; // 0=Sunday, 1=Monday, etc.
        
        if (weekField.Contains("-"))
        {
            var parts = weekField.Split('-');
            if (parts.Length == 2)
            {
                var start = ConvertWeekday(parts[0]);
                var end = ConvertWeekday(parts[1]);
                
                if (start <= end)
                    return dayOfWeek >= start && dayOfWeek <= end;
                else
                    return dayOfWeek >= start || dayOfWeek <= end; // 跨周处理
            }
        }
        else
        {
            var targetDay = ConvertWeekday(weekField);
            return dayOfWeek == targetDay;
        }

        return false;
    }

    private int ConvertWeekday(string weekday)
    {
        return weekday switch
        {
            "SUN" => 0,
            "MON" => 1,
            "TUE" => 2,
            "WED" => 3,
            "THU" => 4,
            "FRI" => 5,
            "SAT" => 6,
            _ when int.TryParse(weekday, out var num) => num,
            _ => -1
        };
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

    // 内部解析方法，返回解析结果
    private (string Description, string NextExecutions, string ValidationMessage, bool IsValid) ParseExpressionInternal(string expression)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(expression))
            {
                return ("", "", "请输入Cron表达式", false);
            }

            // 验证Cron表达式格式
            var parts = expression.Trim().Split(' ', StringSplitOptions.RemoveEmptyEntries);
            if (parts.Length != 6)
            {
                return ("", "", "Cron表达式格式错误，应包含6个字段：秒 分 时 日 月 周", false);
            }

            // 生成描述
            var description = GenerateDescription(parts);
            
            // 计算下次执行时间
            var nextExecutions = CalculateNextExecutionsInternal(parts);

            return (description, nextExecutions, "表达式解析成功", true);
        }
        catch (Exception ex)
        {
            return ("", "", $"无效的Cron表达式: {ex.Message}", false);
        }
    }

    // 内部计算下次执行时间的方法
    private string CalculateNextExecutionsInternal(string[] parts)
    {
        try
        {
            var now = DateTime.Now;
            var executions = new List<string>();
            var currentTime = now;

            // 计算接下来的10次执行时间
            for (int i = 0; i < 10; i++)
            {
                var nextTime = CalculateNextExecution(currentTime);
                if (nextTime == null) break;

                var timeSpan = nextTime.Value - now;
                string relativeTime = "";

                if (timeSpan.TotalMinutes < 1)
                    relativeTime = "即将执行";
                else if (timeSpan.TotalHours < 1)
                    relativeTime = $"{(int)timeSpan.TotalMinutes}分钟后";
                else if (timeSpan.TotalDays < 1)
                    relativeTime = $"{(int)timeSpan.TotalHours}小时{(int)(timeSpan.TotalMinutes % 60)}分钟后";
                else if (timeSpan.TotalDays < 7)
                    relativeTime = $"{(int)timeSpan.TotalDays}天{(int)(timeSpan.TotalHours % 24)}小时后";
                else
                    relativeTime = $"{(int)(timeSpan.TotalDays / 7)}周{(int)(timeSpan.TotalDays % 7)}天后";

                executions.Add($"{nextTime:yyyy-MM-dd HH:mm:ss} ({relativeTime})");
                currentTime = nextTime.Value.AddSeconds(1);
            }

            return executions.Count > 0 ? string.Join("\n", executions) : "无法计算下次执行时间";
        }
        catch (Exception ex)
        {
            return $"计算执行时间时出错: {ex.Message}";
        }
    }

    partial void OnCronExpressionChanged(string value)
    {
        if (!string.IsNullOrEmpty(value))
        {
            // 使用异步方式避免UI阻塞
            _ = Task.Run(async () =>
            {
                try
                {
                    // 在后台线程计算结果
                    string tempDescription = "";
                    string tempNextExecutions = "";
                    string tempValidationMessage = "";
                    bool tempIsValid = true;
                    
                    // 执行计算
                    var result = ParseExpressionInternal(value);
                    tempDescription = result.Description;
                    tempNextExecutions = result.NextExecutions;
                    tempValidationMessage = result.ValidationMessage;
                    tempIsValid = result.IsValid;
                    
                    // 在UI线程上更新所有属性
                    await Dispatcher.UIThread.InvokeAsync(() =>
                    {
                        Description = tempDescription;
                        NextExecutions = tempNextExecutions;
                        ValidationMessage = tempValidationMessage;
                        IsValidExpression = tempIsValid;
                    });
                }
                catch (Exception ex)
                {
                    // 在UI线程上更新错误信息
                    await Dispatcher.UIThread.InvokeAsync(() =>
                    {
                        ValidationMessage = $"解析错误: {ex.Message}";
                        IsValidExpression = false;
                        Description = "";
                        NextExecutions = "";
                    });
                }
            });
        }
    }

    // 字段选择器的属性变更通知方法
    partial void OnSelectedSecondChanged(string value) => UpdateExpressionFromFields();
    partial void OnSelectedMinuteChanged(string value) => UpdateExpressionFromFields();
    partial void OnSelectedHourChanged(string value) => UpdateExpressionFromFields();
    partial void OnSelectedDayChanged(string value) => UpdateExpressionFromFields();
    partial void OnSelectedMonthChanged(string value) => UpdateExpressionFromFields();
    partial void OnSelectedWeekChanged(string value) => UpdateExpressionFromFields();

    private void UpdateExpressionFromFields()
    {
        // 避免在初始化时触发或字段更新时的循环调用
        if (_isUpdatingFields || string.IsNullOrEmpty(SelectedSecond) || string.IsNullOrEmpty(SelectedMinute) || 
            string.IsNullOrEmpty(SelectedHour) || string.IsNullOrEmpty(SelectedDay) || 
            string.IsNullOrEmpty(SelectedMonth) || string.IsNullOrEmpty(SelectedWeek))
            return;

        var newExpression = $"{SelectedSecond} {SelectedMinute} {SelectedHour} {SelectedDay} {SelectedMonth} {SelectedWeek}";
        
        // 只有当表达式真正改变时才更新
        if (CronExpression != newExpression)
        {
            CronExpression = newExpression;
            // 自动解析新生成的表达式
            ParseExpression();
        }
    }
}