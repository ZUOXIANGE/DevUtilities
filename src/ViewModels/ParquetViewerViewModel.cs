using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Input;
using Avalonia.Controls;
using Avalonia.Platform.Storage;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Serilog;

namespace DevUtilities.ViewModels;

public partial class ParquetViewerViewModel : BaseToolViewModel
{
    private static readonly ILogger Logger = Log.ForContext<ParquetViewerViewModel>();

    [ObservableProperty]
    private string? _filePath;

    [ObservableProperty]
    private string? _fileName;

    [ObservableProperty]
    private long _fileSize;

    [ObservableProperty]
    private string? _fileSizeText;

    [ObservableProperty]
    private bool _isFileLoaded;

    [ObservableProperty]
    private bool _isLoading;

    [ObservableProperty]
    private string? _errorMessage;

    [ObservableProperty]
    private bool _hasError;

    [ObservableProperty]
    private int _totalRows;

    [ObservableProperty]
    private int _totalColumns;

    [ObservableProperty]
    private int _currentPage = 1;

    [ObservableProperty]
    private int _pageSize = 100;

    [ObservableProperty]
    private int _totalPages;

    [ObservableProperty]
    private string? _pageInfo;

    [ObservableProperty]
    private string? _searchText;

    [ObservableProperty]
    private string? _selectedColumn;

    [ObservableProperty]
    private bool _caseSensitive;

    [ObservableProperty]
    private int _filteredRows;

    public ObservableCollection<ParquetColumn> Columns { get; } = new();
    public ObservableCollection<Dictionary<string, object?>> Data { get; } = new();
    public ObservableCollection<string> ColumnNames { get; } = new();

    private List<Dictionary<string, object?>> _allData = new();
    private List<Dictionary<string, object?>> _filteredData = new();

    public ICommand OpenFileCommand { get; }
    public ICommand CloseFileCommand { get; }
    public ICommand RefreshCommand { get; }
    public ICommand FirstPageCommand { get; }
    public ICommand PreviousPageCommand { get; }
    public ICommand NextPageCommand { get; }
    public ICommand LastPageCommand { get; }
    public ICommand SearchCommand { get; }
    public ICommand ClearSearchCommand { get; }
    public ICommand ExportCsvCommand { get; }
    public ICommand ExportJsonCommand { get; }

    public ParquetViewerViewModel()
    {
        Logger.Debug("ParquetViewerViewModel: 开始初始化");
        try
        {
            OpenFileCommand = new AsyncRelayCommand(OpenFileAsync);
            CloseFileCommand = new RelayCommand(CloseFile);
            RefreshCommand = new AsyncRelayCommand(RefreshAsync);
            FirstPageCommand = new RelayCommand(GoToFirstPage, () => CanNavigate && CurrentPage > 1);
            PreviousPageCommand = new RelayCommand(GoToPreviousPage, () => CanNavigate && CurrentPage > 1);
            NextPageCommand = new RelayCommand(GoToNextPage, () => CanNavigate && CurrentPage < TotalPages);
            LastPageCommand = new RelayCommand(GoToLastPage, () => CanNavigate && CurrentPage < TotalPages);
            SearchCommand = new RelayCommand(PerformSearch);
            ClearSearchCommand = new RelayCommand(ClearSearch);
            ExportCsvCommand = new AsyncRelayCommand(ExportToCsvAsync, () => IsFileLoaded);
            ExportJsonCommand = new AsyncRelayCommand(ExportToJsonAsync, () => IsFileLoaded);

            PropertyChanged += OnPropertyChanged;
            
            Logger.Information("ParquetViewerViewModel: 初始化完成 - 所有命令已创建，属性变更监听已设置");
        }
        catch (Exception ex)
        {
            Logger.Error(ex, "ParquetViewerViewModel: 初始化失败");
            throw;
        }
    }

    private bool CanNavigate => IsFileLoaded && !IsLoading && TotalPages > 1;

    private void OnPropertyChanged(object? sender, PropertyChangedEventArgs e)
    {
        Logger.Debug("ParquetViewerViewModel: 属性变更 - 属性名: {PropertyName}", e.PropertyName);
        try
        {
            switch (e.PropertyName)
            {
                case nameof(CurrentPage):
                case nameof(TotalPages):
                    Logger.Debug("ParquetViewerViewModel: 页面相关属性变更 - CurrentPage: {CurrentPage}, TotalPages: {TotalPages}", CurrentPage, TotalPages);
                    UpdatePageInfo();
                    UpdateNavigationCommands();
                    break;
                case nameof(IsFileLoaded):
                case nameof(IsLoading):
                    Logger.Debug("ParquetViewerViewModel: 加载状态变更 - IsFileLoaded: {IsFileLoaded}, IsLoading: {IsLoading}", IsFileLoaded, IsLoading);
                    UpdateNavigationCommands();
                    break;
                case nameof(SearchText):
                case nameof(SelectedColumn):
                case nameof(CaseSensitive):
                    Logger.Debug("ParquetViewerViewModel: 搜索相关属性变更 - SearchText: {SearchText}, SelectedColumn: {SelectedColumn}, CaseSensitive: {CaseSensitive}", 
                        SearchText, SelectedColumn, CaseSensitive);
                    if (IsFileLoaded)
                    {
                        PerformSearch();
                    }
                    break;
            }
        }
        catch (Exception ex)
        {
            Logger.Error(ex, "ParquetViewerViewModel: 处理属性变更失败 - 属性名: {PropertyName}", e.PropertyName);
        }
    }

    private async Task OpenFileAsync()
    {
        Logger.Debug("ParquetViewerViewModel: 开始打开文件");
        try
        {
            var topLevel = TopLevel.GetTopLevel(App.Current?.ApplicationLifetime is Avalonia.Controls.ApplicationLifetimes.IClassicDesktopStyleApplicationLifetime desktop 
                ? desktop.MainWindow : null);
            
            if (topLevel == null) 
            {
                Logger.Warning("ParquetViewerViewModel: 无法获取顶级窗口，取消文件打开");
                return;
            }

            var files = await topLevel.StorageProvider.OpenFilePickerAsync(new FilePickerOpenOptions
            {
                Title = "选择 Parquet 文件",
                AllowMultiple = false,
                FileTypeFilter = new[]
                {
                    new FilePickerFileType("Parquet Files")
                    {
                        Patterns = new[] { "*.parquet", "*.pq" }
                    },
                    new FilePickerFileType("All Files")
                    {
                        Patterns = new[] { "*.*" }
                    }
                }
            });

            if (files.Count > 0)
            {
                var file = files[0];
                Logger.Debug("ParquetViewerViewModel: 用户选择了文件 - 文件路径: {FilePath}", file.Path.LocalPath);
                await LoadParquetFileAsync(file.Path.LocalPath);
            }
            else
            {
                Logger.Debug("ParquetViewerViewModel: 用户取消了文件选择");
            }
        }
        catch (Exception ex)
        {
            Logger.Error(ex, "ParquetViewerViewModel: 打开文件失败");
            ShowError($"打开文件失败: {ex.Message}");
        }
    }

    private async Task LoadParquetFileAsync(string filePath)
    {
        Logger.Debug("ParquetViewerViewModel: 开始加载Parquet文件 - 文件路径: {FilePath}", filePath);
        try
        {
            IsLoading = true;
            HasError = false;
            ErrorMessage = null;

            if (!File.Exists(filePath))
            {
                Logger.Warning("ParquetViewerViewModel: 文件不存在 - 文件路径: {FilePath}", filePath);
                ShowError("文件不存在");
                return;
            }

            var fileInfo = new FileInfo(filePath);
            FilePath = filePath;
            FileName = fileInfo.Name;
            FileSize = fileInfo.Length;
            FileSizeText = FormatFileSize(fileInfo.Length);

            Logger.Information("ParquetViewerViewModel: 文件信息获取成功 - 文件名: {FileName}, 文件大小: {FileSize} bytes ({FileSizeText})", 
                FileName, FileSize, FileSizeText);

            // 模拟读取 Parquet 文件
            // 注意：这里需要实际的 Parquet 库来读取文件，如 Apache.Arrow 或 Parquet.Net
            Logger.Debug("ParquetViewerViewModel: 开始模拟Parquet文件读取");
            await Task.Delay(500); // 模拟加载时间

            // 生成示例数据用于演示
            await GenerateSampleDataAsync();

            IsFileLoaded = true;
            CurrentPage = 1;
            UpdatePageInfo();
            PerformSearch();
            
            Logger.Information("ParquetViewerViewModel: Parquet文件加载完成 - 总行数: {TotalRows}, 总列数: {TotalColumns}", TotalRows, TotalColumns);
        }
        catch (Exception ex)
        {
            Logger.Error(ex, "ParquetViewerViewModel: 加载Parquet文件失败 - 文件路径: {FilePath}", filePath);
            ShowError($"加载文件失败: {ex.Message}");
        }
        finally
        {
            IsLoading = false;
            Logger.Debug("ParquetViewerViewModel: 文件加载操作结束");
        }
    }

    private async Task GenerateSampleDataAsync()
    {
        Logger.Debug("ParquetViewerViewModel: 开始生成示例数据");
        try
        {
            await Task.Run(() =>
            {
                // 清空现有数据
                Columns.Clear();
                ColumnNames.Clear();
                _allData.Clear();

                Logger.Debug("ParquetViewerViewModel: 已清空现有数据");

                // 生成示例列定义
                var sampleColumns = new[]
                {
                    new ParquetColumn { Name = "ID", DataType = "INT64", IsNullable = false },
                    new ParquetColumn { Name = "Name", DataType = "STRING", IsNullable = true },
                    new ParquetColumn { Name = "Age", DataType = "INT32", IsNullable = true },
                    new ParquetColumn { Name = "Email", DataType = "STRING", IsNullable = true },
                    new ParquetColumn { Name = "Salary", DataType = "DOUBLE", IsNullable = true },
                    new ParquetColumn { Name = "Department", DataType = "STRING", IsNullable = true },
                    new ParquetColumn { Name = "JoinDate", DataType = "DATE", IsNullable = true },
                    new ParquetColumn { Name = "IsActive", DataType = "BOOLEAN", IsNullable = false }
                };

                foreach (var column in sampleColumns)
                {
                    Columns.Add(column);
                    ColumnNames.Add(column.Name);
                }

                Logger.Debug("ParquetViewerViewModel: 示例列定义生成完成 - 列数: {ColumnCount}", sampleColumns.Length);

                // 生成示例数据
                var random = new Random();
                var departments = new[] { "Engineering", "Marketing", "Sales", "HR", "Finance" };
                var names = new[] { "张三", "李四", "王五", "赵六", "钱七", "孙八", "周九", "吴十" };

                const int sampleRowCount = 1000;
                for (int i = 1; i <= sampleRowCount; i++)
                {
                    var row = new Dictionary<string, object?>
                    {
                        ["ID"] = i,
                        ["Name"] = names[random.Next(names.Length)] + i,
                        ["Age"] = random.Next(22, 65),
                        ["Email"] = $"user{i}@example.com",
                        ["Salary"] = Math.Round(random.NextDouble() * 50000 + 30000, 2),
                        ["Department"] = departments[random.Next(departments.Length)],
                        ["JoinDate"] = DateTime.Now.AddDays(-random.Next(1, 1000)).ToString("yyyy-MM-dd"),
                        ["IsActive"] = random.Next(2) == 1
                    };

                    _allData.Add(row);
                }

                TotalRows = _allData.Count;
                TotalColumns = Columns.Count;

                Logger.Information("ParquetViewerViewModel: 示例数据生成完成 - 总行数: {TotalRows}, 总列数: {TotalColumns}", TotalRows, TotalColumns);
            });
        }
        catch (Exception ex)
        {
            Logger.Error(ex, "ParquetViewerViewModel: 生成示例数据失败");
            throw;
        }
    }

    private void PerformSearch()
    {
        Logger.Debug("ParquetViewerViewModel: 开始执行搜索 - SearchText: {SearchText}, SelectedColumn: {SelectedColumn}, CaseSensitive: {CaseSensitive}", 
            SearchText, SelectedColumn, CaseSensitive);
        
        if (!IsFileLoaded)
        {
            Logger.Debug("ParquetViewerViewModel: 文件未加载，跳过搜索");
            return;
        }

        try
        {
            _filteredData = _allData.ToList();
            Logger.Debug("ParquetViewerViewModel: 初始化过滤数据 - 原始行数: {OriginalRows}", _allData.Count);

            if (!string.IsNullOrWhiteSpace(SearchText))
            {
                var searchTerm = CaseSensitive ? SearchText : SearchText.ToLower();
                Logger.Debug("ParquetViewerViewModel: 应用搜索过滤 - 搜索词: {SearchTerm}", searchTerm);

                _filteredData = _allData.Where(row =>
                {
                    if (!string.IsNullOrWhiteSpace(SelectedColumn))
                    {
                        // 在指定列中搜索
                        if (row.TryGetValue(SelectedColumn, out var value) && value != null)
                        {
                            var valueStr = CaseSensitive ? value.ToString() : value.ToString()?.ToLower();
                            return valueStr?.Contains(searchTerm) == true;
                        }
                        return false;
                    }
                    else
                    {
                        // 在所有列中搜索
                        return row.Values.Any(value =>
                        {
                            if (value == null) return false;
                            var valueStr = CaseSensitive ? value.ToString() : value.ToString()?.ToLower();
                            return valueStr?.Contains(searchTerm) == true;
                        });
                    }
                }).ToList();
            }

            FilteredRows = _filteredData.Count;
            TotalPages = (int)Math.Ceiling((double)FilteredRows / PageSize);
            
            Logger.Debug("ParquetViewerViewModel: 搜索过滤完成 - 过滤后行数: {FilteredRows}, 总页数: {TotalPages}", FilteredRows, TotalPages);
            
            if (CurrentPage > TotalPages && TotalPages > 0)
            {
                Logger.Debug("ParquetViewerViewModel: 当前页超出范围，调整到最后一页 - 原页码: {OldPage}, 新页码: {NewPage}", CurrentPage, TotalPages);
                CurrentPage = TotalPages;
            }
            else if (CurrentPage < 1 && TotalPages > 0)
            {
                Logger.Debug("ParquetViewerViewModel: 当前页小于1，调整到第一页");
                CurrentPage = 1;
            }

            UpdateCurrentPageData();
            Logger.Information("ParquetViewerViewModel: 搜索执行完成 - 搜索词: {SearchText}, 过滤后行数: {FilteredRows}, 当前页: {CurrentPage}", 
                SearchText, FilteredRows, CurrentPage);
        }
        catch (Exception ex)
        {
            Logger.Error(ex, "ParquetViewerViewModel: 搜索执行失败");
            ShowError($"搜索失败: {ex.Message}");
        }
    }

    private void UpdateCurrentPageData()
    {
        Logger.Debug("ParquetViewerViewModel: 开始更新当前页数据 - 当前页: {CurrentPage}, 页大小: {PageSize}", CurrentPage, PageSize);
        try
        {
            Data.Clear();

            if (!IsFileLoaded || _filteredData.Count == 0)
            {
                Logger.Debug("ParquetViewerViewModel: 文件未加载或无过滤数据，清空显示数据");
                return;
            }

            var startIndex = (CurrentPage - 1) * PageSize;
            var endIndex = Math.Min(startIndex + PageSize, _filteredData.Count);

            Logger.Debug("ParquetViewerViewModel: 计算数据范围 - 开始索引: {StartIndex}, 结束索引: {EndIndex}", startIndex, endIndex);

            for (int i = startIndex; i < endIndex; i++)
            {
                Data.Add(_filteredData[i]);
            }

            Logger.Information("ParquetViewerViewModel: 当前页数据更新完成 - 显示行数: {DisplayRows}", Data.Count);
        }
        catch (Exception ex)
        {
            Logger.Error(ex, "ParquetViewerViewModel: 更新当前页数据失败");
        }
    }

    private void ClearSearch()
    {
        Logger.Debug("ParquetViewerViewModel: 执行清空搜索");
        try
        {
            SearchText = string.Empty;
            SelectedColumn = null;
            CaseSensitive = false;
            Logger.Information("ParquetViewerViewModel: 搜索条件已清空");
        }
        catch (Exception ex)
        {
            Logger.Error(ex, "ParquetViewerViewModel: 清空搜索失败");
        }
    }

    private void CloseFile()
    {
        Logger.Debug("ParquetViewerViewModel: 执行关闭文件");
        try
        {
            var oldFileName = FileName;
            
            FilePath = null;
            FileName = null;
            FileSize = 0;
            FileSizeText = null;
            IsFileLoaded = false;
            TotalRows = 0;
            TotalColumns = 0;
            FilteredRows = 0;
            CurrentPage = 1;
            TotalPages = 0;
            PageInfo = null;
            SearchText = null;
            SelectedColumn = null;
            CaseSensitive = false;
            HasError = false;
            ErrorMessage = null;

            Columns.Clear();
            ColumnNames.Clear();
            Data.Clear();
            _allData.Clear();
            _filteredData.Clear();
            
            Logger.Information("ParquetViewerViewModel: 文件已关闭 - 原文件名: {OldFileName}", oldFileName);
        }
        catch (Exception ex)
        {
            Logger.Error(ex, "ParquetViewerViewModel: 关闭文件失败");
        }
    }

    private async Task RefreshAsync()
    {
        Logger.Debug("ParquetViewerViewModel: 执行刷新操作 - 当前文件路径: {FilePath}", FilePath);
        try
        {
            if (!string.IsNullOrEmpty(FilePath))
            {
                await LoadParquetFileAsync(FilePath);
                Logger.Information("ParquetViewerViewModel: 文件刷新完成");
            }
            else
            {
                Logger.Warning("ParquetViewerViewModel: 无文件路径，无法刷新");
            }
        }
        catch (Exception ex)
        {
            Logger.Error(ex, "ParquetViewerViewModel: 刷新操作失败");
        }
    }

    private void GoToFirstPage()
    {
        Logger.Debug("ParquetViewerViewModel: 执行跳转到第一页");
        try
        {
            if (CanNavigate && CurrentPage > 1)
            {
                var oldPage = CurrentPage;
                CurrentPage = 1;
                UpdateCurrentPageData();
                Logger.Information("ParquetViewerViewModel: 已跳转到第一页 - 原页码: {OldPage}, 新页码: {NewPage}", oldPage, CurrentPage);
            }
            else
            {
                Logger.Debug("ParquetViewerViewModel: 无法跳转到第一页 - CanNavigate: {CanNavigate}, CurrentPage: {CurrentPage}", CanNavigate, CurrentPage);
            }
        }
        catch (Exception ex)
        {
            Logger.Error(ex, "ParquetViewerViewModel: 跳转到第一页失败");
        }
    }

    private void GoToPreviousPage()
    {
        Logger.Debug("ParquetViewerViewModel: 执行跳转到上一页");
        try
        {
            if (CanNavigate && CurrentPage > 1)
            {
                var oldPage = CurrentPage;
                CurrentPage--;
                UpdateCurrentPageData();
                Logger.Information("ParquetViewerViewModel: 已跳转到上一页 - 原页码: {OldPage}, 新页码: {NewPage}", oldPage, CurrentPage);
            }
            else
            {
                Logger.Debug("ParquetViewerViewModel: 无法跳转到上一页 - CanNavigate: {CanNavigate}, CurrentPage: {CurrentPage}", CanNavigate, CurrentPage);
            }
        }
        catch (Exception ex)
        {
            Logger.Error(ex, "ParquetViewerViewModel: 跳转到上一页失败");
        }
    }

    private void GoToNextPage()
    {
        Logger.Debug("ParquetViewerViewModel: 执行跳转到下一页");
        try
        {
            if (CanNavigate && CurrentPage < TotalPages)
            {
                var oldPage = CurrentPage;
                CurrentPage++;
                UpdateCurrentPageData();
                Logger.Information("ParquetViewerViewModel: 已跳转到下一页 - 原页码: {OldPage}, 新页码: {NewPage}", oldPage, CurrentPage);
            }
            else
            {
                Logger.Debug("ParquetViewerViewModel: 无法跳转到下一页 - CanNavigate: {CanNavigate}, CurrentPage: {CurrentPage}, TotalPages: {TotalPages}", 
                    CanNavigate, CurrentPage, TotalPages);
            }
        }
        catch (Exception ex)
        {
            Logger.Error(ex, "ParquetViewerViewModel: 跳转到下一页失败");
        }
    }

    private void GoToLastPage()
    {
        Logger.Debug("ParquetViewerViewModel: 执行跳转到最后一页");
        try
        {
            if (CanNavigate && CurrentPage < TotalPages)
            {
                var oldPage = CurrentPage;
                CurrentPage = TotalPages;
                UpdateCurrentPageData();
                Logger.Information("ParquetViewerViewModel: 已跳转到最后一页 - 原页码: {OldPage}, 新页码: {NewPage}", oldPage, CurrentPage);
            }
            else
            {
                Logger.Debug("ParquetViewerViewModel: 无法跳转到最后一页 - CanNavigate: {CanNavigate}, CurrentPage: {CurrentPage}, TotalPages: {TotalPages}", 
                    CanNavigate, CurrentPage, TotalPages);
            }
        }
        catch (Exception ex)
        {
            Logger.Error(ex, "ParquetViewerViewModel: 跳转到最后一页失败");
        }
    }

    private void UpdatePageInfo()
    {
        Logger.Debug("ParquetViewerViewModel: 开始更新页面信息");
        try
        {
            if (TotalPages > 0)
            {
                var displayRows = Math.Min(PageSize, FilteredRows - (CurrentPage - 1) * PageSize);
                PageInfo = $"第 {CurrentPage} 页，共 {TotalPages} 页 (显示 {FilteredRows} 行中的 {displayRows} 行)";
                Logger.Debug("ParquetViewerViewModel: 页面信息更新完成 - {PageInfo}", PageInfo);
            }
            else
            {
                PageInfo = "无数据";
                Logger.Debug("ParquetViewerViewModel: 页面信息更新完成 - 无数据");
            }
        }
        catch (Exception ex)
        {
            Logger.Error(ex, "ParquetViewerViewModel: 更新页面信息失败");
        }
    }

    private void UpdateNavigationCommands()
    {
        Logger.Debug("ParquetViewerViewModel: 开始更新导航命令状态");
        try
        {
            ((RelayCommand)FirstPageCommand).NotifyCanExecuteChanged();
            ((RelayCommand)PreviousPageCommand).NotifyCanExecuteChanged();
            ((RelayCommand)NextPageCommand).NotifyCanExecuteChanged();
            ((RelayCommand)LastPageCommand).NotifyCanExecuteChanged();
            ((AsyncRelayCommand)ExportCsvCommand).NotifyCanExecuteChanged();
            ((AsyncRelayCommand)ExportJsonCommand).NotifyCanExecuteChanged();
            Logger.Debug("ParquetViewerViewModel: 导航命令状态更新完成");
        }
        catch (Exception ex)
        {
            Logger.Error(ex, "ParquetViewerViewModel: 更新导航命令状态失败");
        }
    }

    private async Task ExportToCsvAsync()
    {
        Logger.Debug("ParquetViewerViewModel: 开始导出CSV");
        try
        {
            var topLevel = TopLevel.GetTopLevel(App.Current?.ApplicationLifetime is Avalonia.Controls.ApplicationLifetimes.IClassicDesktopStyleApplicationLifetime desktop 
                ? desktop.MainWindow : null);
            
            if (topLevel == null) 
            {
                Logger.Warning("ParquetViewerViewModel: 无法获取顶级窗口，取消CSV导出");
                return;
            }

            var file = await topLevel.StorageProvider.SaveFilePickerAsync(new FilePickerSaveOptions
            {
                Title = "导出为 CSV",
                DefaultExtension = "csv",
                SuggestedFileName = Path.GetFileNameWithoutExtension(FileName) + "_export.csv",
                FileTypeChoices = new[]
                {
                    new FilePickerFileType("CSV Files")
                    {
                        Patterns = new[] { "*.csv" }
                    }
                }
            });

            if (file != null)
            {
                Logger.Debug("ParquetViewerViewModel: 用户选择了CSV导出文件 - 文件路径: {FilePath}", file.Path.LocalPath);
                await ExportDataToCsvAsync(file.Path.LocalPath);
            }
            else
            {
                Logger.Debug("ParquetViewerViewModel: 用户取消了CSV导出");
            }
        }
        catch (Exception ex)
        {
            Logger.Error(ex, "ParquetViewerViewModel: CSV导出失败");
            ShowError($"导出 CSV 失败: {ex.Message}");
        }
    }

    private async Task ExportToJsonAsync()
    {
        Logger.Debug("ParquetViewerViewModel: 开始导出JSON");
        try
        {
            var topLevel = TopLevel.GetTopLevel(App.Current?.ApplicationLifetime is Avalonia.Controls.ApplicationLifetimes.IClassicDesktopStyleApplicationLifetime desktop 
                ? desktop.MainWindow : null);
            
            if (topLevel == null) 
            {
                Logger.Warning("ParquetViewerViewModel: 无法获取顶级窗口，取消JSON导出");
                return;
            }

            var file = await topLevel.StorageProvider.SaveFilePickerAsync(new FilePickerSaveOptions
            {
                Title = "导出为 JSON",
                DefaultExtension = "json",
                SuggestedFileName = Path.GetFileNameWithoutExtension(FileName) + "_export.json",
                FileTypeChoices = new[]
                {
                    new FilePickerFileType("JSON Files")
                    {
                        Patterns = new[] { "*.json" }
                    }
                }
            });

            if (file != null)
            {
                Logger.Debug("ParquetViewerViewModel: 用户选择了JSON导出文件 - 文件路径: {FilePath}", file.Path.LocalPath);
                await ExportDataToJsonAsync(file.Path.LocalPath);
            }
            else
            {
                Logger.Debug("ParquetViewerViewModel: 用户取消了JSON导出");
            }
        }
        catch (Exception ex)
        {
            Logger.Error(ex, "ParquetViewerViewModel: JSON导出失败");
            ShowError($"导出 JSON 失败: {ex.Message}");
        }
    }

    private async Task ExportDataToCsvAsync(string filePath)
    {
        Logger.Debug("ParquetViewerViewModel: 开始执行CSV数据导出 - 文件路径: {FilePath}, 数据行数: {DataRows}", filePath, _filteredData.Count);
        try
        {
            await Task.Run(() =>
            {
                using var writer = new StreamWriter(filePath);
                
                // 写入标题行
                writer.WriteLine(string.Join(",", ColumnNames.Select(name => $"\"{name}\"")));
                Logger.Debug("ParquetViewerViewModel: CSV标题行已写入 - 列数: {ColumnCount}", ColumnNames.Count);

                // 写入数据行
                int exportedRows = 0;
                foreach (var row in _filteredData)
                {
                    var values = ColumnNames.Select(col => 
                    {
                        var value = row.TryGetValue(col, out var val) ? val?.ToString() ?? "" : "";
                        return $"\"{value.Replace("\"", "\"\"")}\"";
                    });
                    writer.WriteLine(string.Join(",", values));
                    exportedRows++;
                }
                
                Logger.Information("ParquetViewerViewModel: CSV数据导出完成 - 文件路径: {FilePath}, 导出行数: {ExportedRows}", filePath, exportedRows);
            });
        }
        catch (Exception ex)
        {
            Logger.Error(ex, "ParquetViewerViewModel: CSV数据导出失败 - 文件路径: {FilePath}", filePath);
            throw;
        }
    }

    private async Task ExportDataToJsonAsync(string filePath)
    {
        Logger.Debug("ParquetViewerViewModel: 开始执行JSON数据导出 - 文件路径: {FilePath}, 数据行数: {DataRows}", filePath, _filteredData.Count);
        try
        {
            await Task.Run(() =>
            {
                var json = System.Text.Json.JsonSerializer.Serialize(_filteredData, new System.Text.Json.JsonSerializerOptions
                {
                    WriteIndented = true,
                    Encoder = System.Text.Encodings.Web.JavaScriptEncoder.UnsafeRelaxedJsonEscaping
                });
                
                File.WriteAllText(filePath, json);
                Logger.Information("ParquetViewerViewModel: JSON数据导出完成 - 文件路径: {FilePath}, JSON长度: {JsonLength}", filePath, json.Length);
            });
        }
        catch (Exception ex)
        {
            Logger.Error(ex, "ParquetViewerViewModel: JSON数据导出失败 - 文件路径: {FilePath}", filePath);
            throw;
        }
    }

    private void ShowError(string message)
    {
        Logger.Warning("ParquetViewerViewModel: 显示错误信息 - 错误消息: {ErrorMessage}", message);
        ErrorMessage = message;
        HasError = true;
    }

    private static string FormatFileSize(long bytes)
    {
        string[] sizes = { "B", "KB", "MB", "GB", "TB" };
        double len = bytes;
        int order = 0;
        while (len >= 1024 && order < sizes.Length - 1)
        {
            order++;
            len = len / 1024;
        }
        return $"{len:0.##} {sizes[order]}";
    }
}

public class ParquetColumn
{
    public string Name { get; set; } = string.Empty;
    public string DataType { get; set; } = string.Empty;
    public bool IsNullable { get; set; }
    public string DisplayText => $"{Name} ({DataType}){(IsNullable ? " NULL" : " NOT NULL")}";
}