using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Avalonia.Controls;
using Avalonia.Platform.Storage;
using DevUtilities.Models;
using DevUtilities.Services;

namespace DevUtilities.ViewModels;

public partial class TextDiffViewModel : ObservableObject
{
    [ObservableProperty]
    private string leftText = string.Empty;

    [ObservableProperty]
    private string rightText = string.Empty;

    [ObservableProperty]
    private bool ignoreWhitespace = false;

    [ObservableProperty]
    private bool ignoreCase = false;

    [ObservableProperty]
    private bool showLineNumbers = true;

    [ObservableProperty]
    private bool enableCharacterDiff = true;

    [ObservableProperty]
    private string diffSummary = string.Empty;

    [ObservableProperty]
    private bool hasDifferences = false;

    public ObservableCollection<EnhancedDiffLine> DiffResult { get; } = new();
    
    private readonly CharacterDiffService _characterDiffService = new();

    public TextDiffViewModel()
    {
        CompareTextsCommand = new AsyncRelayCommand(CompareTextsAsync);
        ClearAllCommand = new RelayCommand(ClearAll);
        SwapTextsCommand = new RelayCommand(SwapTexts);
        LoadLeftFileCommand = new AsyncRelayCommand(LoadLeftFileAsync);
        LoadRightFileCommand = new AsyncRelayCommand(LoadRightFileAsync);
        ExportDiffCommand = new AsyncRelayCommand(ExportDiffAsync);
    }

    public IAsyncRelayCommand CompareTextsCommand { get; }
    public IRelayCommand ClearAllCommand { get; }
    public IRelayCommand SwapTextsCommand { get; }
    public IAsyncRelayCommand LoadLeftFileCommand { get; }
    public IAsyncRelayCommand LoadRightFileCommand { get; }
    public IAsyncRelayCommand ExportDiffCommand { get; }

    partial void OnLeftTextChanged(string value)
    {
        _ = CompareTextsAsync();
    }

    partial void OnRightTextChanged(string value)
    {
        _ = CompareTextsAsync();
    }

    partial void OnIgnoreWhitespaceChanged(bool value)
    {
        _ = CompareTextsAsync();
    }

    partial void OnIgnoreCaseChanged(bool value)
    {
        _ = CompareTextsAsync();
    }

    partial void OnEnableCharacterDiffChanged(bool value)
    {
        _ = CompareTextsAsync();
    }

    private async Task CompareTextsAsync()
    {
        try
        {
            DiffResult.Clear();

            if (string.IsNullOrEmpty(LeftText) && string.IsNullOrEmpty(RightText))
            {
                DiffSummary = "两个文本都为空";
                HasDifferences = false;
                return;
            }

            var leftLines = ProcessText(LeftText);
            var rightLines = ProcessText(RightText);

            var diff = ComputeDiff(leftLines, rightLines);
            
            foreach (var line in diff)
            {
                DiffResult.Add(line);
            }

            UpdateSummary();
            
            await Task.CompletedTask;
        }
        catch (Exception ex)
        {
            DiffSummary = $"对比过程中发生错误: {ex.Message}";
            HasDifferences = false;
        }
    }

    private string[] ProcessText(string text)
    {
        if (string.IsNullOrEmpty(text))
            return Array.Empty<string>();

        // 使用正确的换行符分割，避免产生额外的空行
        var lines = text.Split(new[] { "\r\n", "\r", "\n" }, StringSplitOptions.None);

        if (IgnoreWhitespace)
        {
            lines = lines.Select(line => line.Trim()).ToArray();
        }

        if (IgnoreCase)
        {
            lines = lines.Select(line => line.ToLowerInvariant()).ToArray();
        }

        return lines;
    }

    private EnhancedDiffLine[] ComputeDiff(string[] leftLines, string[] rightLines)
    {
        var result = new List<EnhancedDiffLine>();
        var maxLines = Math.Max(leftLines.Length, rightLines.Length);

        for (int i = 0; i < maxLines; i++)
        {
            var leftLine = i < leftLines.Length ? leftLines[i] : null;
            var rightLine = i < rightLines.Length ? rightLines[i] : null;

            if (leftLine == null && rightLine != null)
            {
                // 右侧新增行
                var diffLine = new EnhancedDiffLine
                {
                    LineNumber = i + 1,
                    LeftContent = string.Empty,
                    RightContent = rightLine,
                    DiffType = DiffType.Added,
                    LeftLineNumber = null,
                    RightLineNumber = i + 1
                };

                if (EnableCharacterDiff)
                {
                    diffLine.RightCharacterDiffs = new List<CharacterDiffSegment>
                    {
                        new CharacterDiffSegment
                        {
                            Type = CharacterDiffType.Added,
                            Text = rightLine,
                            StartIndex = 0,
                            EndIndex = rightLine.Length - 1
                        }
                    };
                }

                result.Add(diffLine);
            }
            else if (leftLine != null && rightLine == null)
            {
                // 左侧删除行
                var diffLine = new EnhancedDiffLine
                {
                    LineNumber = i + 1,
                    LeftContent = leftLine,
                    RightContent = string.Empty,
                    DiffType = DiffType.Deleted,
                    LeftLineNumber = i + 1,
                    RightLineNumber = null
                };

                if (EnableCharacterDiff)
                {
                    diffLine.LeftCharacterDiffs = new List<CharacterDiffSegment>
                    {
                        new CharacterDiffSegment
                        {
                            Type = CharacterDiffType.Deleted,
                            Text = leftLine,
                            StartIndex = 0,
                            EndIndex = leftLine.Length - 1
                        }
                    };
                }

                result.Add(diffLine);
            }
            else if (leftLine != null && rightLine != null)
            {
                if (leftLine.Equals(rightLine, IgnoreCase ? StringComparison.OrdinalIgnoreCase : StringComparison.Ordinal))
                {
                    // 相同行
                    var diffLine = new EnhancedDiffLine
                    {
                        LineNumber = i + 1,
                        LeftContent = leftLine,
                        RightContent = rightLine,
                        DiffType = DiffType.Unchanged,
                        LeftLineNumber = i + 1,
                        RightLineNumber = i + 1
                    };

                    if (EnableCharacterDiff)
                    {
                        diffLine.LeftCharacterDiffs = new List<CharacterDiffSegment>
                        {
                            new CharacterDiffSegment
                            {
                                Type = CharacterDiffType.Unchanged,
                                Text = leftLine,
                                StartIndex = 0,
                                EndIndex = leftLine.Length - 1
                            }
                        };
                        diffLine.RightCharacterDiffs = new List<CharacterDiffSegment>
                        {
                            new CharacterDiffSegment
                            {
                                Type = CharacterDiffType.Unchanged,
                                Text = rightLine,
                                StartIndex = 0,
                                EndIndex = rightLine.Length - 1
                            }
                        };
                    }

                    result.Add(diffLine);
                }
                else
                {
                    // 修改行 - 进行字符级别差异检测
                    var diffLine = new EnhancedDiffLine
                    {
                        LineNumber = i + 1,
                        LeftContent = leftLine,
                        RightContent = rightLine,
                        DiffType = DiffType.Modified,
                        LeftLineNumber = i + 1,
                        RightLineNumber = i + 1
                    };

                    if (EnableCharacterDiff)
                    {
                        var (leftCharDiffs, rightCharDiffs) = _characterDiffService.ComputeCharacterDiff(leftLine, rightLine, IgnoreCase);
                        diffLine.LeftCharacterDiffs = leftCharDiffs;
                        diffLine.RightCharacterDiffs = rightCharDiffs;
                    }

                    result.Add(diffLine);
                }
            }
        }

        return result.ToArray();
    }

    private void UpdateSummary()
    {
        // 计算左右文本的实际行数
        var leftLines = ProcessText(LeftText);
        var rightLines = ProcessText(RightText);
        var leftTotalLines = leftLines.Length;
        var rightTotalLines = rightLines.Length;
        
        var unchangedLines = DiffResult.Count(d => d.DiffType == DiffType.Unchanged);
        var modifiedLines = DiffResult.Count(d => d.DiffType == DiffType.Modified);
        var addedLines = DiffResult.Count(d => d.DiffType == DiffType.Added);
        var deletedLines = DiffResult.Count(d => d.DiffType == DiffType.Deleted);

        HasDifferences = modifiedLines > 0 || addedLines > 0 || deletedLines > 0;

        if (!HasDifferences)
        {
            DiffSummary = $"文本完全相同 (左侧: {leftTotalLines} 行, 右侧: {rightTotalLines} 行)";
        }
        else
        {
            DiffSummary = $"左侧: {leftTotalLines} 行, 右侧: {rightTotalLines} 行 | 相同: {unchangedLines}, 修改: {modifiedLines}, 新增: {addedLines}, 删除: {deletedLines}";
        }
    }

    private void ClearAll()
    {
        LeftText = string.Empty;
        RightText = string.Empty;
        DiffResult.Clear();
        DiffSummary = string.Empty;
        HasDifferences = false;
    }

    private void SwapTexts()
    {
        (LeftText, RightText) = (RightText, LeftText);
    }

    private async Task LoadLeftFileAsync()
    {
        try
        {
            var topLevel = TopLevel.GetTopLevel(App.Current?.ApplicationLifetime is Avalonia.Controls.ApplicationLifetimes.IClassicDesktopStyleApplicationLifetime desktop 
                ? desktop.MainWindow : null);
            
            if (topLevel == null) return;

            var files = await topLevel.StorageProvider.OpenFilePickerAsync(new FilePickerOpenOptions
            {
                Title = "选择左侧文件",
                AllowMultiple = false,
                FileTypeFilter = new[]
                {
                    new FilePickerFileType("文本文件")
                    {
                        Patterns = new[] { "*.txt", "*.log", "*.json", "*.xml", "*.csv", "*.md" }
                    },
                    new FilePickerFileType("所有文件")
                    {
                        Patterns = new[] { "*.*" }
                    }
                }
            });

            if (files?.Count > 0)
            {
                using var stream = await files[0].OpenReadAsync();
                using var reader = new System.IO.StreamReader(stream);
                LeftText = await reader.ReadToEndAsync();
            }
        }
        catch (Exception ex)
        {
            DiffSummary = $"加载左侧文件失败: {ex.Message}";
        }
    }

    private async Task LoadRightFileAsync()
    {
        try
        {
            var topLevel = TopLevel.GetTopLevel(App.Current?.ApplicationLifetime is Avalonia.Controls.ApplicationLifetimes.IClassicDesktopStyleApplicationLifetime desktop 
                ? desktop.MainWindow : null);
            
            if (topLevel == null) return;

            var files = await topLevel.StorageProvider.OpenFilePickerAsync(new FilePickerOpenOptions
            {
                Title = "选择右侧文件",
                AllowMultiple = false,
                FileTypeFilter = new[]
                {
                    new FilePickerFileType("文本文件")
                    {
                        Patterns = new[] { "*.txt", "*.log", "*.json", "*.xml", "*.csv", "*.md" }
                    },
                    new FilePickerFileType("所有文件")
                    {
                        Patterns = new[] { "*.*" }
                    }
                }
            });

            if (files?.Count > 0)
            {
                using var stream = await files[0].OpenReadAsync();
                using var reader = new System.IO.StreamReader(stream);
                RightText = await reader.ReadToEndAsync();
            }
        }
        catch (Exception ex)
        {
            DiffSummary = $"加载右侧文件失败: {ex.Message}";
        }
    }

    private async Task ExportDiffAsync()
    {
        try
        {
            if (!DiffResult.Any())
            {
                DiffSummary = "没有对比结果可导出";
                return;
            }

            var topLevel = TopLevel.GetTopLevel(App.Current?.ApplicationLifetime is Avalonia.Controls.ApplicationLifetimes.IClassicDesktopStyleApplicationLifetime desktop 
                ? desktop.MainWindow : null);
            
            if (topLevel == null) return;

            var file = await topLevel.StorageProvider.SaveFilePickerAsync(new FilePickerSaveOptions
            {
                Title = "导出对比结果",
                DefaultExtension = "txt",
                SuggestedFileName = $"diff_result_{DateTime.Now:yyyyMMdd_HHmmss}.txt",
                FileTypeChoices = new[]
                {
                    new FilePickerFileType("文本文件")
                    {
                        Patterns = new[] { "*.txt" }
                    }
                }
            });

            if (file != null)
            {
                using var stream = await file.OpenWriteAsync();
                using var writer = new System.IO.StreamWriter(stream);

                await writer.WriteLineAsync($"文本对比结果 - {DateTime.Now:yyyy-MM-dd HH:mm:ss}");
                await writer.WriteLineAsync($"对比设置: 忽略空白字符={IgnoreWhitespace}, 忽略大小写={IgnoreCase}");
                await writer.WriteLineAsync($"对比摘要: {DiffSummary}");
                await writer.WriteLineAsync(new string('=', 80));
                await writer.WriteLineAsync();

                foreach (var line in DiffResult)
                {
                    var prefix = line.DiffType switch
                    {
                        DiffType.Added => "+ ",
                        DiffType.Deleted => "- ",
                        DiffType.Modified => "* ",
                        _ => "  "
                    };

                    var lineNum = ShowLineNumbers ? $"{line.LineNumber:D4}: " : "";
                    await writer.WriteLineAsync($"{prefix}{lineNum}左侧: {line.LeftContent}");
                    
                    if (line.DiffType != DiffType.Unchanged)
                    {
                        await writer.WriteLineAsync($"{prefix}{lineNum}右侧: {line.RightContent}");
                    }
                }

                DiffSummary += " (已导出到文件)";
            }
        }
        catch (Exception ex)
        {
            DiffSummary = $"导出失败: {ex.Message}";
        }
    }
}

public class DiffLine
{
    public int LineNumber { get; set; }
    public string LeftContent { get; set; } = string.Empty;
    public string RightContent { get; set; } = string.Empty;
    public DiffType DiffType { get; set; }
    public int? LeftLineNumber { get; set; }
    public int? RightLineNumber { get; set; }
}

public enum DiffType
{
    Unchanged,
    Added,
    Deleted,
    Modified
}