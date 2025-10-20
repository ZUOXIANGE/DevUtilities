using System;
using System.Globalization;
using Avalonia.Data.Converters;

namespace DevUtilities.Converters;

/// <summary>
/// 将布尔值转换为可见性
/// </summary>
public class BooleanToVisibilityConverter : IValueConverter
{
    public static readonly BooleanToVisibilityConverter Instance = new();

    public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (value is bool boolValue)
        {
            return boolValue;
        }
        return false;
    }

    public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (value is bool boolValue)
        {
            return boolValue;
        }
        return false;
    }
}