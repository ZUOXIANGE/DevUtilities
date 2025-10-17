using System;
using System.Globalization;
using Avalonia.Data.Converters;

namespace DevUtilities.Converters;

public class StringConverters
{
    public static readonly StringIsNotNullOrEmptyConverter IsNotNullOrEmpty = new();
    public static readonly StringIsNullOrEmptyConverter IsNullOrEmpty = new();
}

public class StringIsNotNullOrEmptyConverter : IValueConverter
{
    public static readonly StringIsNotNullOrEmptyConverter Instance = new();

    public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        return !string.IsNullOrEmpty(value?.ToString());
    }

    public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        // 这个转换器主要用于单向绑定，ConvertBack通常不需要实现
        // 但为了避免异常，返回一个默认值
        return value is bool boolValue && boolValue ? "NotEmpty" : string.Empty;
    }
}

public class StringIsNullOrEmptyConverter : IValueConverter
{
    public static readonly StringIsNullOrEmptyConverter Instance = new();

    public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        return string.IsNullOrEmpty(value?.ToString());
    }

    public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        // 这个转换器主要用于单向绑定，ConvertBack通常不需要实现
        // 但为了避免异常，返回一个默认值
        return value is bool boolValue && boolValue ? string.Empty : "NotEmpty";
    }
}