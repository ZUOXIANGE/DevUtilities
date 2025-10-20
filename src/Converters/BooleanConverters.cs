using System;
using System.Globalization;
using Avalonia.Data.Converters;
using Avalonia.Media;

namespace DevUtilities.Converters;

public static class BooleanConverters
{
    public static readonly BooleanToColorConverter TrueToGreen = new();
    public static new readonly BooleanToStringConverter ToString = new();
}

public class BooleanToColorConverter : IValueConverter
{
    public static readonly BooleanToColorConverter Instance = new();

    public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (value is bool boolValue)
        {
            return boolValue ? Brushes.Green : Brushes.Red;
        }
        return Brushes.Gray;
    }

    public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (value is SolidColorBrush brush)
        {
            // 比较颜色值而不是引用
            return brush.Color == Colors.Green;
        }
        return false;
    }
}

public class BooleanToStringConverter : IValueConverter
{
    public static readonly BooleanToStringConverter Instance = new();

    public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (value is bool boolValue && parameter is string paramString)
        {
            var options = paramString.Split('|');
            if (options.Length == 2)
            {
                return boolValue ? options[0] : options[1];
            }
        }
        return value?.ToString();
    }

    public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (value is string stringValue && parameter is string paramString)
        {
            var options = paramString.Split('|');
            if (options.Length == 2)
            {
                return stringValue == options[0];
            }
        }
        
        if (value is string str)
        {
            return bool.TryParse(str, out var result) ? result : false;
        }
        
        return false;
    }
}