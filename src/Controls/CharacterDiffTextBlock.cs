using System.Collections.Generic;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Documents;
using Avalonia.Media;
using DevUtilities.Models;

namespace DevUtilities.Controls;

/// <summary>
/// 用于显示字符级别差异的自定义TextBlock
/// </summary>
public class CharacterDiffTextBlock : TextBlock
{
    public static readonly StyledProperty<List<CharacterDiffSegment>?> CharacterDiffsProperty =
        AvaloniaProperty.Register<CharacterDiffTextBlock, List<CharacterDiffSegment>?>(nameof(CharacterDiffs));

    public List<CharacterDiffSegment>? CharacterDiffs
    {
        get => GetValue(CharacterDiffsProperty);
        set => SetValue(CharacterDiffsProperty, value);
    }

    static CharacterDiffTextBlock()
    {
        CharacterDiffsProperty.Changed.AddClassHandler<CharacterDiffTextBlock>((x, e) => x.OnCharacterDiffsChanged());
    }

    private void OnCharacterDiffsChanged()
    {
        UpdateInlines();
    }

    private void UpdateInlines()
    {
        Inlines?.Clear();

        if (CharacterDiffs == null || CharacterDiffs.Count == 0)
            return;

        foreach (var segment in CharacterDiffs)
        {
            var run = new Run(segment.Text);

            // 设置前景色
            run.Foreground = segment.Type switch
            {
                CharacterDiffType.Added => new SolidColorBrush(Color.FromRgb(0x28, 0xA7, 0x45)),      // 绿色
                CharacterDiffType.Deleted => new SolidColorBrush(Color.FromRgb(0xDC, 0x35, 0x45)),    // 红色
                CharacterDiffType.Modified => new SolidColorBrush(Color.FromRgb(0xFF, 0x85, 0x00)),   // 橙色
                _ => new SolidColorBrush(Color.FromRgb(0x24, 0x29, 0x2F))                             // 默认黑色
            };

            // 设置背景色
            run.Background = segment.Type switch
            {
                CharacterDiffType.Added => new SolidColorBrush(Color.FromArgb(0x40, 0x28, 0xA7, 0x45)),    // 半透明绿色背景
                CharacterDiffType.Deleted => new SolidColorBrush(Color.FromArgb(0x40, 0xDC, 0x35, 0x45)),  // 半透明红色背景
                CharacterDiffType.Modified => new SolidColorBrush(Color.FromArgb(0x40, 0xFF, 0x85, 0x00)), // 半透明橙色背景
                _ => Brushes.Transparent
            };

            // 设置字体粗细
            if (segment.Type != CharacterDiffType.Unchanged)
            {
                run.FontWeight = FontWeight.Bold;
            }

            Inlines?.Add(run);
        }
    }
}