using System;
using System.Globalization;
using Avalonia.Data.Converters;

namespace DevUtilities.Converters;

public class StringConverters
{
    public static readonly StringIsNotNullOrEmptyConverter IsNotNullOrEmpty = new();
    public static readonly StringIsNullOrEmptyConverter IsNullOrEmpty = new();
    public static readonly StringIsNotEqualConverter IsNotEqual = new();
    public static readonly BoolToStringConverter BoolToString = new();
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
        throw new NotImplementedException();
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
        throw new NotImplementedException();
    }
}

public class StringIsNotEqualConverter : IValueConverter
{
    public static readonly StringIsNotEqualConverter Instance = new();

    public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        var valueStr = value?.ToString();
        var parameterStr = parameter?.ToString();
        return !string.Equals(valueStr, parameterStr, StringComparison.OrdinalIgnoreCase);
    }

    public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}

public class BoolToStringConverter : IValueConverter
{
    public static readonly BoolToStringConverter Instance = new();

    public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (value is bool boolValue && parameter is string parameterStr)
        {
            var options = parameterStr.Split('|');
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