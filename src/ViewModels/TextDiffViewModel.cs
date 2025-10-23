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
using Serilog;

namespace DevUtilities.ViewModels;

public partial class TextDiffViewModel : ObservableObject
{
    private static readonly ILogger Logger = Log.ForContext<TextDiffViewModel>();

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
        Logger.Debug("TextDiffViewModel: 开始初始化");
        try
        {
            CompareTextsCommand = new AsyncRelayCommand(CompareTextsAsync);
            ClearAllCommand = new RelayCommand(ClearAll);
            SwapTextsCommand = new RelayCommand(SwapTexts);
            LoadLeftFileCommand = new AsyncRelayCommand(LoadLeftFileAsync);
            LoadRightFileCommand = new AsyncRelayCommand(LoadRightFileAsync);
            ExportDiffCommand = new AsyncRelayCommand(ExportDiffAsync);
            
            Logger.Information("TextDiffViewModel: 初始化完成 - 命令已创建，CharacterDiffService已初始化");
        }
        catch (Exception ex)
        {
            Logger.Error(ex, "TextDiffViewModel: 初始化失败");
            throw;
        }
    }

    public IAsyncRelayCommand CompareTextsCommand { get; }
    public IRelayCommand ClearAllCommand { get; }
    public IRelayCommand SwapTextsCommand { get; }
    public IAsyncRelayCommand LoadLeftFileCommand { get; }
    public IAsyncRelayCommand LoadRightFileCommand { get; }
    public IAsyncRelayCommand ExportDiffCommand { get; }

    partial void OnLeftTextChanged(string value)
    {
        Logger.Debug("TextDiffViewModel: 左侧文本已更改 - 长度: {Length}", value?.Length ?? 0);
        _ = CompareTextsAsync();
    }

    partial void OnRightTextChanged(string value)
    {
        Logger.Debug("TextDiffViewModel: 右侧文本已更改 - 长度: {Length}", value?.Length ?? 0);
        _ = CompareTextsAsync();
    }

    partial void OnIgnoreWhitespaceChanged(bool value)
    {
        Logger.Debug("TextDiffViewModel: 忽略空白字符设置已更改 - 新值: {Value}", value);
        _ = CompareTextsAsync();
    }

    partial void OnIgnoreCaseChanged(bool value)
    {
        Logger.Debug("TextDiffViewModel: 忽略大小写设置已更改 - 新值: {Value}", value);
        _ = CompareTextsAsync();
    }

    partial void OnEnableCharacterDiffChanged(bool value)
    {
        Logger.Debug("TextDiffViewModel: 字符级差异检测设置已更改 - 新值: {Value}", value);
        _ = CompareTextsAsync();
    }

    private async Task CompareTextsAsync()
    {
        Logger.Debug("TextDiffViewModel: 开始文本对比 - IgnoreWhitespace: {IgnoreWhitespace}, IgnoreCase: {IgnoreCase}, EnableCharacterDiff: {EnableCharacterDiff}", 
            IgnoreWhitespace, IgnoreCase, EnableCharacterDiff);
        try
        {
            DiffResult.Clear();

            if (string.IsNullOrEmpty(LeftText) && string.IsNullOrEmpty(RightText))
            {
                Logger.Debug("TextDiffViewModel: 两个文本都为空");
                DiffSummary = "两个文本都为空";
                HasDifferences = false;
                return;
            }

            var leftLines = ProcessText(LeftText);
            var rightLines = ProcessText(RightText);

            Logger.Debug("TextDiffViewModel: 文本预处理完成 - 左侧行数: {LeftLines}, 右侧行数: {RightLines}", leftLines.Length, rightLines.Length);

            var diff = ComputeDiff(leftLines, rightLines);
            
            foreach (var line in diff)
            {
                DiffResult.Add(line);
            }

            Logger.Information("TextDiffViewModel: 文本对比完成 - 差异行数: {DiffCount}", DiffResult.Count);
            UpdateSummary();
            
            await Task.CompletedTask;
        }
        catch (Exception ex)
        {
            Logger.Error(ex, "TextDiffViewModel: 文本对比失败");
            DiffSummary = $"对比过程中发生错误: {ex.Message}";
            HasDifferences = false;
        }
    }

    private string[] ProcessText(string text)
    {
        Logger.Debug("TextDiffViewModel: 开始处理文本 - 原始长度: {Length}", text?.Length ?? 0);
        try
        {
            if (string.IsNullOrEmpty(text))
            {
                Logger.Debug("TextDiffViewModel: 文本为空，返回空数组");
                return Array.Empty<string>();
            }

            // 使用正确的换行符分割，避免产生额外的空行
            var lines = text.Split(new[] { "\r\n", "\r", "\n" }, StringSplitOptions.None);
            Logger.Debug("TextDiffViewModel: 文本分割完成 - 行数: {LineCount}", lines.Length);

            if (IgnoreWhitespace)
            {
                Logger.Debug("TextDiffViewModel: 应用忽略空白字符处理");
                lines = lines.Select(line => line.Trim()).ToArray();
            }

            if (IgnoreCase)
            {
                Logger.Debug("TextDiffViewModel: 应用忽略大小写处理");
                lines = lines.Select(line => line.ToLowerInvariant()).ToArray();
            }

            Logger.Debug("TextDiffViewModel: 文本处理完成 - 最终行数: {FinalLineCount}", lines.Length);
            return lines;
        }
        catch (Exception ex)
        {
            Logger.Error(ex, "TextDiffViewModel: 文本处理失败");
            throw;
        }
    }

    private EnhancedDiffLine[] ComputeDiff(string[] leftLines, string[] rightLines)
    {
        Logger.Debug("TextDiffViewModel: 开始计算差异 - 左侧行数: {LeftLines}, 右侧行数: {RightLines}", leftLines.Length, rightLines.Length);
        try
        {
            var result = new List<EnhancedDiffLine>();
            var maxLines = Math.Max(leftLines.Length, rightLines.Length);
            Logger.Debug("TextDiffViewModel: 最大行数: {MaxLines}", maxLines);

            int addedCount = 0, deletedCount = 0, modifiedCount = 0, unchangedCount = 0;

            for (int i = 0; i < maxLines; i++)
            {
                var leftLine = i < leftLines.Length ? leftLines[i] : null;
                var rightLine = i < rightLines.Length ? rightLines[i] : null;

                if (leftLine == null && rightLine != null)
                {
                    // 右侧新增行
                    addedCount++;
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
                    deletedCount++;
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
                        unchangedCount++;
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
                        modifiedCount++;
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
                            Logger.Debug("TextDiffViewModel: 计算字符级差异 - 行号: {LineNumber}", i + 1);
                            var (leftCharDiffs, rightCharDiffs) = _characterDiffService.ComputeCharacterDiff(leftLine, rightLine, IgnoreCase);
                            diffLine.LeftCharacterDiffs = leftCharDiffs;
                            diffLine.RightCharacterDiffs = rightCharDiffs;
                        }

                        result.Add(diffLine);
                    }
                }
            }

            Logger.Information("TextDiffViewModel: 差异计算完成 - 新增: {Added}, 删除: {Deleted}, 修改: {Modified}, 相同: {Unchanged}", 
                addedCount, deletedCount, modifiedCount, unchangedCount);
            
            return result.ToArray();
        }
        catch (Exception ex)
        {
            Logger.Error(ex, "TextDiffViewModel: 差异计算失败");
            throw;
        }
    }

    private void UpdateSummary()
    {
        Logger.Debug("TextDiffViewModel: 开始更新摘要");
        try
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
                Logger.Information("TextDiffViewModel: 文本完全相同 - 左侧: {LeftLines} 行, 右侧: {RightLines} 行", leftTotalLines, rightTotalLines);
            }
            else
            {
                DiffSummary = $"左侧: {leftTotalLines} 行, 右侧: {rightTotalLines} 行 | 相同: {unchangedLines}, 修改: {modifiedLines}, 新增: {addedLines}, 删除: {deletedLines}";
                Logger.Information("TextDiffViewModel: 发现差异 - 左侧: {LeftLines} 行, 右侧: {RightLines} 行, 相同: {Unchanged}, 修改: {Modified}, 新增: {Added}, 删除: {Deleted}", 
                    leftTotalLines, rightTotalLines, unchangedLines, modifiedLines, addedLines, deletedLines);
            }
        }
        catch (Exception ex)
        {
            Logger.Error(ex, "TextDiffViewModel: 更新摘要失败");
        }
    }

    private void ClearAll()
    {
        Logger.Debug("TextDiffViewModel: 执行清空所有内容");
        try
        {
            LeftText = string.Empty;
            RightText = string.Empty;
            DiffResult.Clear();
            DiffSummary = string.Empty;
            HasDifferences = false;
            Logger.Information("TextDiffViewModel: 所有内容已清空");
        }
        catch (Exception ex)
        {
            Logger.Error(ex, "TextDiffViewModel: 清空内容失败");
        }
    }

    private void SwapTexts()
    {
        Logger.Debug("TextDiffViewModel: 执行交换文本");
        try
        {
            (LeftText, RightText) = (RightText, LeftText);
            Logger.Information("TextDiffViewModel: 文本交换完成 - 左侧长度: {LeftLength}, 右侧长度: {RightLength}", 
                LeftText?.Length ?? 0, RightText?.Length ?? 0);
        }
        catch (Exception ex)
        {
            Logger.Error(ex, "TextDiffViewModel: 交换文本失败");
        }
    }

    private async Task LoadLeftFileAsync()
    {
        Logger.Debug("TextDiffViewModel: 开始加载左侧文件");
        try
        {
            var topLevel = TopLevel.GetTopLevel(App.Current?.ApplicationLifetime is Avalonia.Controls.ApplicationLifetimes.IClassicDesktopStyleApplicationLifetime desktop 
                ? desktop.MainWindow : null);
            
            if (topLevel == null) 
            {
                Logger.Warning("TextDiffViewModel: 无法获取顶级窗口，取消文件加载");
                return;
            }

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
                Logger.Debug("TextDiffViewModel: 用户选择了文件 - 文件名: {FileName}", files[0].Name);
                using var stream = await files[0].OpenReadAsync();
                using var reader = new System.IO.StreamReader(stream);
                LeftText = await reader.ReadToEndAsync();
                Logger.Information("TextDiffViewModel: 左侧文件加载成功 - 文件名: {FileName}, 内容长度: {Length}", files[0].Name, LeftText.Length);
            }
            else
            {
                Logger.Debug("TextDiffViewModel: 用户取消了文件选择");
            }
        }
        catch (Exception ex)
        {
            Logger.Error(ex, "TextDiffViewModel: 加载左侧文件失败");
            DiffSummary = $"加载左侧文件失败: {ex.Message}";
        }
    }

    private async Task LoadRightFileAsync()
    {
        Logger.Debug("TextDiffViewModel: 开始加载右侧文件");
        try
        {
            var topLevel = TopLevel.GetTopLevel(App.Current?.ApplicationLifetime is Avalonia.Controls.ApplicationLifetimes.IClassicDesktopStyleApplicationLifetime desktop 
                ? desktop.MainWindow : null);
            
            if (topLevel == null) 
            {
                Logger.Warning("TextDiffViewModel: 无法获取顶级窗口，取消文件加载");
                return;
            }

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
                Logger.Debug("TextDiffViewModel: 用户选择了文件 - 文件名: {FileName}", files[0].Name);
                using var stream = await files[0].OpenReadAsync();
                using var reader = new System.IO.StreamReader(stream);
                RightText = await reader.ReadToEndAsync();
                Logger.Information("TextDiffViewModel: 右侧文件加载成功 - 文件名: {FileName}, 内容长度: {Length}", files[0].Name, RightText.Length);
            }
            else
            {
                Logger.Debug("TextDiffViewModel: 用户取消了文件选择");
            }
        }
        catch (Exception ex)
        {
            Logger.Error(ex, "TextDiffViewModel: 加载右侧文件失败");
            DiffSummary = $"加载右侧文件失败: {ex.Message}";
        }
    }

    private async Task ExportDiffAsync()
    {
        Logger.Debug("TextDiffViewModel: 开始导出差异结果");
        try
        {
            if (!DiffResult.Any())
            {
                Logger.Warning("TextDiffViewModel: 没有对比结果可导出");
                DiffSummary = "没有对比结果可导出";
                return;
            }

            var topLevel = TopLevel.GetTopLevel(App.Current?.ApplicationLifetime is Avalonia.Controls.ApplicationLifetimes.IClassicDesktopStyleApplicationLifetime desktop 
                ? desktop.MainWindow : null);
            
            if (topLevel == null) 
            {
                Logger.Warning("TextDiffViewModel: 无法获取顶级窗口，取消导出");
                return;
            }

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
                Logger.Debug("TextDiffViewModel: 用户选择了导出文件 - 文件名: {FileName}", file.Name);
                using var stream = await file.OpenWriteAsync();
                using var writer = new System.IO.StreamWriter(stream);

                await writer.WriteLineAsync($"文本对比结果 - {DateTime.Now:yyyy-MM-dd HH:mm:ss}");
                await writer.WriteLineAsync($"对比设置: 忽略空白字符={IgnoreWhitespace}, 忽略大小写={IgnoreCase}");
                await writer.WriteLineAsync($"对比摘要: {DiffSummary}");
                await writer.WriteLineAsync(new string('=', 80));
                await writer.WriteLineAsync();

                int exportedLines = 0;
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
                    exportedLines++;
                }

                Logger.Information("TextDiffViewModel: 差异结果导出成功 - 文件名: {FileName}, 导出行数: {ExportedLines}", file.Name, exportedLines);
                DiffSummary += " (已导出到文件)";
            }
            else
            {
                Logger.Debug("TextDiffViewModel: 用户取消了导出");
            }
        }
        catch (Exception ex)
        {
            Logger.Error(ex, "TextDiffViewModel: 导出差异结果失败");
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