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

namespace DevUtilities.ViewModels;

public partial class ParquetViewerViewModel : BaseToolViewModel
{
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
    }

    private bool CanNavigate => IsFileLoaded && !IsLoading && TotalPages > 1;

    private void OnPropertyChanged(object? sender, PropertyChangedEventArgs e)
    {
        switch (e.PropertyName)
        {
            case nameof(CurrentPage):
            case nameof(TotalPages):
                UpdatePageInfo();
                UpdateNavigationCommands();
                break;
            case nameof(IsFileLoaded):
            case nameof(IsLoading):
                UpdateNavigationCommands();
                break;
            case nameof(SearchText):
            case nameof(SelectedColumn):
            case nameof(CaseSensitive):
                if (IsFileLoaded)
                {
                    PerformSearch();
                }
                break;
        }
    }

    private async Task OpenFileAsync()
    {
        try
        {
            var topLevel = TopLevel.GetTopLevel(App.Current?.ApplicationLifetime is Avalonia.Controls.ApplicationLifetimes.IClassicDesktopStyleApplicationLifetime desktop 
                ? desktop.MainWindow : null);
            
            if (topLevel == null) return;

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
                await LoadParquetFileAsync(file.Path.LocalPath);
            }
        }
        catch (Exception ex)
        {
            ShowError($"打开文件失败: {ex.Message}");
        }
    }

    private async Task LoadParquetFileAsync(string filePath)
    {
        try
        {
            IsLoading = true;
            HasError = false;
            ErrorMessage = null;

            if (!File.Exists(filePath))
            {
                ShowError("文件不存在");
                return;
            }

            var fileInfo = new FileInfo(filePath);
            FilePath = filePath;
            FileName = fileInfo.Name;
            FileSize = fileInfo.Length;
            FileSizeText = FormatFileSize(fileInfo.Length);

            // 模拟读取 Parquet 文件
            // 注意：这里需要实际的 Parquet 库来读取文件，如 Apache.Arrow 或 Parquet.Net
            await Task.Delay(500); // 模拟加载时间

            // 生成示例数据用于演示
            await GenerateSampleDataAsync();

            IsFileLoaded = true;
            CurrentPage = 1;
            UpdatePageInfo();
            PerformSearch();
        }
        catch (Exception ex)
        {
            ShowError($"加载文件失败: {ex.Message}");
        }
        finally
        {
            IsLoading = false;
        }
    }

    private async Task GenerateSampleDataAsync()
    {
        await Task.Run(() =>
        {
            // 清空现有数据
            Columns.Clear();
            ColumnNames.Clear();
            _allData.Clear();

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

            // 生成示例数据
            var random = new Random();
            var departments = new[] { "Engineering", "Marketing", "Sales", "HR", "Finance" };
            var names = new[] { "张三", "李四", "王五", "赵六", "钱七", "孙八", "周九", "吴十" };

            for (int i = 1; i <= 1000; i++)
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
        });
    }

    private void PerformSearch()
    {
        if (!IsFileLoaded)
            return;

        try
        {
            _filteredData = _allData.ToList();

            if (!string.IsNullOrWhiteSpace(SearchText))
            {
                var searchTerm = CaseSensitive ? SearchText : SearchText.ToLower();

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
            
            if (CurrentPage > TotalPages && TotalPages > 0)
            {
                CurrentPage = TotalPages;
            }
            else if (CurrentPage < 1 && TotalPages > 0)
            {
                CurrentPage = 1;
            }

            UpdateCurrentPageData();
        }
        catch (Exception ex)
        {
            ShowError($"搜索失败: {ex.Message}");
        }
    }

    private void UpdateCurrentPageData()
    {
        Data.Clear();

        if (!IsFileLoaded || _filteredData.Count == 0)
            return;

        var startIndex = (CurrentPage - 1) * PageSize;
        var endIndex = Math.Min(startIndex + PageSize, _filteredData.Count);

        for (int i = startIndex; i < endIndex; i++)
        {
            Data.Add(_filteredData[i]);
        }
    }

    private void ClearSearch()
    {
        SearchText = string.Empty;
        SelectedColumn = null;
        CaseSensitive = false;
    }

    private void CloseFile()
    {
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
    }

    private async Task RefreshAsync()
    {
        if (!string.IsNullOrEmpty(FilePath))
        {
            await LoadParquetFileAsync(FilePath);
        }
    }

    private void GoToFirstPage()
    {
        if (CanNavigate && CurrentPage > 1)
        {
            CurrentPage = 1;
            UpdateCurrentPageData();
        }
    }

    private void GoToPreviousPage()
    {
        if (CanNavigate && CurrentPage > 1)
        {
            CurrentPage--;
            UpdateCurrentPageData();
        }
    }

    private void GoToNextPage()
    {
        if (CanNavigate && CurrentPage < TotalPages)
        {
            CurrentPage++;
            UpdateCurrentPageData();
        }
    }

    private void GoToLastPage()
    {
        if (CanNavigate && CurrentPage < TotalPages)
        {
            CurrentPage = TotalPages;
            UpdateCurrentPageData();
        }
    }

    private void UpdatePageInfo()
    {
        if (TotalPages > 0)
        {
            PageInfo = $"第 {CurrentPage} 页，共 {TotalPages} 页 (显示 {FilteredRows} 行中的 {Math.Min(PageSize, FilteredRows - (CurrentPage - 1) * PageSize)} 行)";
        }
        else
        {
            PageInfo = "无数据";
        }
    }

    private void UpdateNavigationCommands()
    {
        ((RelayCommand)FirstPageCommand).NotifyCanExecuteChanged();
        ((RelayCommand)PreviousPageCommand).NotifyCanExecuteChanged();
        ((RelayCommand)NextPageCommand).NotifyCanExecuteChanged();
        ((RelayCommand)LastPageCommand).NotifyCanExecuteChanged();
        ((AsyncRelayCommand)ExportCsvCommand).NotifyCanExecuteChanged();
        ((AsyncRelayCommand)ExportJsonCommand).NotifyCanExecuteChanged();
    }

    private async Task ExportToCsvAsync()
    {
        try
        {
            var topLevel = TopLevel.GetTopLevel(App.Current?.ApplicationLifetime is Avalonia.Controls.ApplicationLifetimes.IClassicDesktopStyleApplicationLifetime desktop 
                ? desktop.MainWindow : null);
            
            if (topLevel == null) return;

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
                await ExportDataToCsvAsync(file.Path.LocalPath);
            }
        }
        catch (Exception ex)
        {
            ShowError($"导出 CSV 失败: {ex.Message}");
        }
    }

    private async Task ExportToJsonAsync()
    {
        try
        {
            var topLevel = TopLevel.GetTopLevel(App.Current?.ApplicationLifetime is Avalonia.Controls.ApplicationLifetimes.IClassicDesktopStyleApplicationLifetime desktop 
                ? desktop.MainWindow : null);
            
            if (topLevel == null) return;

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
                await ExportDataToJsonAsync(file.Path.LocalPath);
            }
        }
        catch (Exception ex)
        {
            ShowError($"导出 JSON 失败: {ex.Message}");
        }
    }

    private async Task ExportDataToCsvAsync(string filePath)
    {
        await Task.Run(() =>
        {
            using var writer = new StreamWriter(filePath);
            
            // 写入标题行
            writer.WriteLine(string.Join(",", ColumnNames.Select(name => $"\"{name}\"")));

            // 写入数据行
            foreach (var row in _filteredData)
            {
                var values = ColumnNames.Select(col => 
                {
                    var value = row.TryGetValue(col, out var val) ? val?.ToString() ?? "" : "";
                    return $"\"{value.Replace("\"", "\"\"")}\"";
                });
                writer.WriteLine(string.Join(",", values));
            }
        });
    }

    private async Task ExportDataToJsonAsync(string filePath)
    {
        await Task.Run(() =>
        {
            var json = System.Text.Json.JsonSerializer.Serialize(_filteredData, new System.Text.Json.JsonSerializerOptions
            {
                WriteIndented = true,
                Encoder = System.Text.Encodings.Web.JavaScriptEncoder.UnsafeRelaxedJsonEscaping
            });
            
            File.WriteAllText(filePath, json);
        });
    }

    private void ShowError(string message)
    {
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