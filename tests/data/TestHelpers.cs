using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;
using FluentAssertions;

namespace DevUtilities.Tests.Data;

/// <summary>
/// 测试辅助方法集合
/// </summary>
public static class TestHelpers
{
    /// <summary>
    /// 验证PropertyChanged事件是否被触发
    /// </summary>
    public static void VerifyPropertyChanged(INotifyPropertyChanged viewModel, string propertyName, Action action)
    {
        var eventRaised = false;
        string? raisedPropertyName = null;

        viewModel.PropertyChanged += (sender, e) =>
        {
            eventRaised = true;
            raisedPropertyName = e.PropertyName;
        };

        action();

        eventRaised.Should().BeTrue($"PropertyChanged event should be raised for {propertyName}");
        raisedPropertyName.Should().Be(propertyName);
    }

    /// <summary>
    /// 验证多个PropertyChanged事件是否被触发
    /// </summary>
    public static void VerifyPropertyChanged(INotifyPropertyChanged viewModel, string[] propertyNames, Action action)
    {
        var raisedProperties = new List<string>();

        viewModel.PropertyChanged += (sender, e) =>
        {
            if (e.PropertyName != null)
                raisedProperties.Add(e.PropertyName);
        };

        action();

        foreach (var propertyName in propertyNames)
        {
            raisedProperties.Should().Contain(propertyName, $"PropertyChanged event should be raised for {propertyName}");
        }
    }

    /// <summary>
    /// 验证PropertyChanged事件不应该被触发
    /// </summary>
    public static void VerifyPropertyNotChanged(INotifyPropertyChanged viewModel, Action action)
    {
        var eventRaised = false;

        viewModel.PropertyChanged += (sender, e) =>
        {
            eventRaised = true;
        };

        action();

        eventRaised.Should().BeFalse("PropertyChanged event should not be raised");
    }

    /// <summary>
    /// 等待异步操作完成
    /// </summary>
    public static async Task WaitForAsync(Func<Task> asyncAction, int timeoutMs = 5000)
    {
        var task = asyncAction();
        var completedTask = await Task.WhenAny(task, Task.Delay(timeoutMs));
        
        if (completedTask != task)
        {
            throw new TimeoutException($"Async operation did not complete within {timeoutMs}ms");
        }

        await task; // Re-await to get any exceptions
    }

    /// <summary>
    /// 创建临时文件用于测试
    /// </summary>
    public static string CreateTempFile(string content = "")
    {
        var tempFile = Path.GetTempFileName();
        File.WriteAllText(tempFile, content);
        return tempFile;
    }

    /// <summary>
    /// 清理临时文件
    /// </summary>
    public static void CleanupTempFile(string filePath)
    {
        if (File.Exists(filePath))
        {
            try
            {
                File.Delete(filePath);
            }
            catch
            {
                // 忽略删除错误
            }
        }
    }

    /// <summary>
    /// 验证字符串是否为有效的Base64
    /// </summary>
    public static bool IsValidBase64(string input)
    {
        if (string.IsNullOrEmpty(input))
            return false;

        try
        {
            Convert.FromBase64String(input);
            return true;
        }
        catch
        {
            return false;
        }
    }

    /// <summary>
    /// 验证字符串是否为有效的十六进制
    /// </summary>
    public static bool IsValidHex(string input)
    {
        if (string.IsNullOrEmpty(input))
            return false;

        return input.All(c => "0123456789ABCDEFabcdef".Contains(c));
    }

    /// <summary>
    /// 验证字符串是否为有效的JSON
    /// </summary>
    public static bool IsValidJson(string input)
    {
        if (string.IsNullOrEmpty(input))
            return false;

        try
        {
            JsonDocument.Parse(input);
            return true;
        }
        catch
        {
            return false;
        }
    }

    /// <summary>
    /// 生成随机字符串
    /// </summary>
    public static string GenerateRandomString(int length)
    {
        const string chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789";
        var random = new Random();
        return new string(Enumerable.Repeat(chars, length)
            .Select(s => s[random.Next(s.Length)]).ToArray());
    }

    /// <summary>
    /// 生成随机数字
    /// </summary>
    public static int GenerateRandomNumber(int min = 0, int max = 1000)
    {
        var random = new Random();
        return random.Next(min, max);
    }

    /// <summary>
    /// 比较两个时间戳是否在容差范围内相等
    /// </summary>
    public static bool AreTimestampsEqual(long timestamp1, long timestamp2, long toleranceSeconds = 1)
    {
        return Math.Abs(timestamp1 - timestamp2) <= toleranceSeconds;
    }

    /// <summary>
    /// 比较两个DateTime是否在容差范围内相等
    /// </summary>
    public static bool AreDateTimesEqual(DateTime dateTime1, DateTime dateTime2, TimeSpan? tolerance = null)
    {
        tolerance ??= TimeSpan.FromSeconds(1);
        return Math.Abs((dateTime1 - dateTime2).TotalMilliseconds) <= tolerance.Value.TotalMilliseconds;
    }
}
