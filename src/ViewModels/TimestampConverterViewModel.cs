using System;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using DevUtilities.Core.ViewModels;

namespace DevUtilities.ViewModels;

public partial class TimestampConverterViewModel : BaseViewModel
{
    [ObservableProperty]
    private string timestampInput = "";

    [ObservableProperty]
    private string dateTimeInput = "";

    [ObservableProperty]
    private string timestampResult = "";

    [ObservableProperty]
    private string dateTimeResult = "";

    [ObservableProperty]
    private string currentTimestamp = "";

    [ObservableProperty]
    private string currentDateTime = "";

    public TimestampConverterViewModel()
    {
        UpdateCurrentTime();
        
        // 每秒更新当前时间
        var timer = new System.Timers.Timer(1000);
        timer.Elapsed += (s, e) => UpdateCurrentTime();
        timer.Start();
    }

    private void UpdateCurrentTime()
    {
        var now = DateTimeOffset.Now;
        CurrentTimestamp = now.ToUnixTimeSeconds().ToString();
        CurrentDateTime = now.ToString("yyyy-MM-dd HH:mm:ss");
    }

    [RelayCommand]
    private void ConvertTimestampToDateTime()
    {
        try
        {
            if (string.IsNullOrWhiteSpace(TimestampInput))
            {
                DateTimeResult = "";
                return;
            }

            if (long.TryParse(TimestampInput, out long timestamp))
            {
                DateTimeOffset dateTime;
                
                // 判断是秒还是毫秒时间戳
                if (timestamp > 9999999999) // 毫秒时间戳
                {
                    dateTime = DateTimeOffset.FromUnixTimeMilliseconds(timestamp);
                }
                else // 秒时间戳
                {
                    dateTime = DateTimeOffset.FromUnixTimeSeconds(timestamp);
                }

                DateTimeResult = $"本地时间: {dateTime.ToLocalTime():yyyy-MM-dd HH:mm:ss}\n" +
                               $"UTC时间: {dateTime.ToUniversalTime():yyyy-MM-dd HH:mm:ss}\n" +
                               $"ISO格式: {dateTime:yyyy-MM-ddTHH:mm:ss.fffZ}";
            }
            else
            {
                DateTimeResult = "无效的时间戳格式";
            }
        }
        catch (Exception ex)
        {
            DateTimeResult = $"转换错误: {ex.Message}";
        }
    }

    [RelayCommand]
    private void ConvertDateTimeToTimestamp()
    {
        try
        {
            if (string.IsNullOrWhiteSpace(DateTimeInput))
            {
                TimestampResult = "";
                return;
            }

            if (DateTime.TryParse(DateTimeInput, out DateTime dateTime))
            {
                var dateTimeOffset = new DateTimeOffset(dateTime);
                var secondsTimestamp = dateTimeOffset.ToUnixTimeSeconds();
                var millisecondsTimestamp = dateTimeOffset.ToUnixTimeMilliseconds();

                TimestampResult = $"秒时间戳: {secondsTimestamp}\n" +
                                $"毫秒时间戳: {millisecondsTimestamp}";
            }
            else
            {
                TimestampResult = "无效的日期时间格式";
            }
        }
        catch (Exception ex)
        {
            TimestampResult = $"转换错误: {ex.Message}";
        }
    }

    [RelayCommand]
    private void UseCurrentTimestamp()
    {
        TimestampInput = CurrentTimestamp;
        ConvertTimestampToDateTime();
    }

    [RelayCommand]
    private void UseCurrentDateTime()
    {
        DateTimeInput = CurrentDateTime;
        ConvertDateTimeToTimestamp();
    }

    partial void OnTimestampInputChanged(string value)
    {
        ConvertTimestampToDateTime();
    }

    partial void OnDateTimeInputChanged(string value)
    {
        ConvertDateTimeToTimestamp();
    }
}