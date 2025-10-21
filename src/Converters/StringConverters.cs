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