using System;
using System.Collections.Generic;
using System.Globalization;
using Avalonia.Data.Converters;
using Avalonia.Media;
using DevUtilities.ViewModels;

namespace DevUtilities.Converters;

/// <summary>
/// 将差异类型转换为背景颜色
/// </summary>
public class DiffTypeToBackgroundConverter : IValueConverter
{
    public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (value is DiffType diffType)
        {
            return diffType switch
            {
                DiffType.Added => new SolidColorBrush(Color.FromRgb(220, 252, 231)),      // 浅绿色
                DiffType.Deleted => new SolidColorBrush(Color.FromRgb(255, 235, 233)),    // 浅红色
                DiffType.Modified => new SolidColorBrush(Color.FromRgb(255, 248, 220)),   // 浅黄色
                DiffType.Unchanged => new SolidColorBrush(Colors.White),                  // 白色
                _ => new SolidColorBrush(Colors.White)
            };
        }
        return new SolidColorBrush(Colors.White);
    }

    public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}

/// <summary>
/// 将差异类型转换为左侧指示器符号
/// </summary>
public class DiffTypeToLeftIndicatorConverter : IValueConverter
{
    public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (value is DiffType diffType)
        {
            return diffType switch
            {
                DiffType.Added => "",      // 左侧没有内容
                DiffType.Deleted => "-",   // 删除符号
                DiffType.Modified => "~",  // 修改符号
                DiffType.Unchanged => "",  // 无变化
                _ => ""
            };
        }
        return "";
    }

    public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}

/// <summary>
/// 将差异类型转换为右侧指示器符号
/// </summary>
public class DiffTypeToRightIndicatorConverter : IValueConverter
{
    public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (value is DiffType diffType)
        {
            return diffType switch
            {
                DiffType.Added => "+",     // 添加符号
                DiffType.Deleted => "",    // 右侧没有内容
                DiffType.Modified => "~",  // 修改符号
                DiffType.Unchanged => "",  // 无变化
                _ => ""
            };
        }
        return "";
    }

    public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}

/// <summary>
/// 将差异类型转换为前景色
/// </summary>
public class DiffTypeToForegroundConverter : IValueConverter
{
    public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (value is DiffType diffType)
        {
            return diffType switch
            {
                DiffType.Added => new SolidColorBrush(Color.FromRgb(22, 163, 74)),     // 绿色
                DiffType.Deleted => new SolidColorBrush(Color.FromRgb(220, 38, 38)),   // 红色
                DiffType.Modified => new SolidColorBrush(Color.FromRgb(245, 158, 11)), // 橙色
                DiffType.Unchanged => new SolidColorBrush(Color.FromRgb(107, 114, 126)), // 灰色
                _ => new SolidColorBrush(Color.FromRgb(107, 114, 126))
            };
        }
        return new SolidColorBrush(Color.FromRgb(107, 114, 126));
    }

    public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}

/// <summary>
/// 将差异类型转换为内容文本的前景色
/// </summary>
public class DiffTypeToContentForegroundConverter : IValueConverter
{
    public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (value is DiffType diffType)
        {
            return diffType switch
            {
                DiffType.Added => new SolidColorBrush(Color.FromRgb(21, 128, 61)),     // 深绿色
                DiffType.Deleted => new SolidColorBrush(Color.FromRgb(185, 28, 28)),   // 深红色
                DiffType.Modified => new SolidColorBrush(Color.FromRgb(217, 119, 6)),  // 深橙色
                DiffType.Unchanged => new SolidColorBrush(Color.FromRgb(55, 65, 81)),  // 深灰色
                _ => new SolidColorBrush(Color.FromRgb(55, 65, 81))
            };
        }
        return new SolidColorBrush(Color.FromRgb(55, 65, 81));
    }

    public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}