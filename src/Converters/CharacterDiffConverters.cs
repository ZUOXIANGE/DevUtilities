using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using Avalonia.Data.Converters;
using Avalonia.Media;
using DevUtilities.Models;

namespace DevUtilities.Converters;

/// <summary>
/// 字符差异片段到内联元素的转换器
/// </summary>
public class CharacterDiffToInlinesConverter : IValueConverter
{
    public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (value is not List<CharacterDiffSegment> segments)
            return null;

        var inlines = new List<object>();
        
        foreach (var segment in segments)
        {
            var brush = segment.Type switch
            {
                CharacterDiffType.Added => new SolidColorBrush(Color.FromRgb(0x28, 0xA7, 0x45)),      // 绿色
                CharacterDiffType.Deleted => new SolidColorBrush(Color.FromRgb(0xDC, 0x35, 0x45)),    // 红色
                CharacterDiffType.Modified => new SolidColorBrush(Color.FromRgb(0xFF, 0x85, 0x00)),   // 橙色
                _ => new SolidColorBrush(Color.FromRgb(0x24, 0x29, 0x2F))                             // 默认黑色
            };

            var background = segment.Type switch
            {
                CharacterDiffType.Added => new SolidColorBrush(Color.FromArgb(0x40, 0x28, 0xA7, 0x45)),    // 半透明绿色背景
                CharacterDiffType.Deleted => new SolidColorBrush(Color.FromArgb(0x40, 0xDC, 0x35, 0x45)),  // 半透明红色背景
                CharacterDiffType.Modified => new SolidColorBrush(Color.FromArgb(0x40, 0xFF, 0x85, 0x00)), // 半透明橙色背景
                _ => (IBrush)Brushes.Transparent
            };

            inlines.Add(new
            {
                Text = segment.Text,
                Foreground = brush,
                Background = background,
                FontWeight = segment.Type != CharacterDiffType.Unchanged ? FontWeight.Bold : FontWeight.Normal
            });
        }

        return inlines;
    }

    public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}

/// <summary>
/// 字符差异类型到背景色的转换器
/// </summary>
public class CharacterDiffTypeToBackgroundConverter : IValueConverter
{
    public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (value is not CharacterDiffType diffType)
            return Brushes.Transparent;

        return diffType switch
        {
            CharacterDiffType.Added => new SolidColorBrush(Color.FromArgb(0x20, 0x28, 0xA7, 0x45)),
            CharacterDiffType.Deleted => new SolidColorBrush(Color.FromArgb(0x20, 0xDC, 0x35, 0x45)),
            CharacterDiffType.Modified => new SolidColorBrush(Color.FromArgb(0x20, 0xFF, 0x85, 0x00)),
            _ => Brushes.Transparent
        };
    }

    public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}

/// <summary>
/// 字符差异类型到前景色的转换器
/// </summary>
public class CharacterDiffTypeToForegroundConverter : IValueConverter
{
    public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (value is not CharacterDiffType diffType)
            return new SolidColorBrush(Color.FromRgb(0x24, 0x29, 0x2F));

        return diffType switch
        {
            CharacterDiffType.Added => new SolidColorBrush(Color.FromRgb(0x28, 0xA7, 0x45)),
            CharacterDiffType.Deleted => new SolidColorBrush(Color.FromRgb(0xDC, 0x35, 0x45)),
            CharacterDiffType.Modified => new SolidColorBrush(Color.FromRgb(0xFF, 0x85, 0x00)),
            _ => new SolidColorBrush(Color.FromRgb(0x24, 0x29, 0x2F))
        };
    }

    public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}