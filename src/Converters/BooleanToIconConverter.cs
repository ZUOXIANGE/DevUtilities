using System;
using System.Globalization;
using Avalonia.Data.Converters;

namespace DevUtilities.Converters;

/// <summary>
/// 将布尔值转换为图标字符
/// </summary>
public class BooleanToIconConverter : IValueConverter
{
    public static readonly BooleanToIconConverter Instance = new();

    public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (value is bool boolValue)
        {
            return boolValue ? "✅" : "❌";
        }
        return "❓";
    }

    public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (value is string iconValue)
        {
            return iconValue switch
            {
                "✅" => true,
                "❌" => false,
                _ => null
            };
        }
        return null;
    }
}