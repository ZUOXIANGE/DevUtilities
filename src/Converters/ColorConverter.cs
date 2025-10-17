using System;
using System.Collections.Generic;
using System.Globalization;
using Avalonia.Data.Converters;
using Avalonia.Media;

namespace DevUtilities.ViewModels
{
    public class ColorConverter : IValueConverter
    {
        public static readonly ColorConverter Instance = new();

        public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
        {
            if (value is int red && parameter is int green)
            {
                // 这是一个简化的实现，实际需要三个参数
                // 在实际使用中，我们需要使用MultiValueConverter
                return new SolidColorBrush(Color.FromRgb((byte)red, (byte)green, 0));
            }

            return new SolidColorBrush(Colors.Transparent);
        }

        public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
        {
            if (value is SolidColorBrush brush)
            {
                var color = brush.Color;
                return color.R; // 返回红色分量，这是一个简化实现
            }
            return 0;
        }
    }

    public class RgbToColorConverter : IMultiValueConverter
    {
        public static readonly RgbToColorConverter Instance = new();

        public object? Convert(IList<object?> values, Type targetType, object? parameter, CultureInfo culture)
        {
            if (values.Count >= 3 && 
                values[0] is int red && 
                values[1] is int green && 
                values[2] is int blue)
            {
                return new SolidColorBrush(Color.FromRgb(
                    (byte)Math.Clamp(red, 0, 255),
                    (byte)Math.Clamp(green, 0, 255),
                    (byte)Math.Clamp(blue, 0, 255)));
            }

            return new SolidColorBrush(Colors.Transparent);
        }

        public object[] ConvertBack(object? value, Type[] targetTypes, object? parameter, CultureInfo culture)
        {
            if (value is SolidColorBrush brush)
            {
                var color = brush.Color;
                return new object[] { (int)color.R, (int)color.G, (int)color.B };
            }
            return new object[] { 0, 0, 0 };
        }
    }
}