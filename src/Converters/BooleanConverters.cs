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
            return boolValue ? Brushes.LightGreen : Brushes.LightCoral;
        }
        return Brushes.Transparent;
    }

    public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
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
        throw new NotImplementedException();
    }
}