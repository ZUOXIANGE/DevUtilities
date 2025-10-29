using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.ApplicationLifetimes;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Sqids;

namespace DevUtilities.ViewModels;

public partial class SqlidsGeneratorViewModel : BaseToolViewModel
{
    [ObservableProperty]
    private string _numberInput = "1";

    [ObservableProperty]
    private string _encodedId = "";

    [ObservableProperty]
    private string _idInput = "";

    [ObservableProperty]
    private string _decodedNumbers = "";

    [ObservableProperty]
    private string _alphabet = "";

    [ObservableProperty]
    private int _minLength = 0;

    [ObservableProperty]
    private string _blacklist = "";

    [ObservableProperty]
    private string _batchInput = "";

    [ObservableProperty]
    private string _validationResult = "";

    public ObservableCollection<string> EncodingHistory { get; } = new();
    public ObservableCollection<string> DecodingHistory { get; } = new();
    public ObservableCollection<string> BatchResults { get; } = new();

    private SqidsEncoder<int> _encoder = null!;
    
    [ObservableProperty]
    private bool _useCustomSettings;

    public SqlidsGeneratorViewModel()
    {
        // 设置默认值
        Alphabet = "abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789";
        Blacklist = "";
        
        // 创建默认编码器
        UpdateSqidsInstance();
        
        // 监听设置变化
        PropertyChanged += (s, e) =>
        {
            if (e.PropertyName == nameof(Alphabet) || 
                e.PropertyName == nameof(MinLength) || 
                e.PropertyName == nameof(Blacklist))
            {
                UpdateSqidsInstance();
            }
        };
    }

    private void UpdateSqidsInstance()
    {
        try
        {
            var options = new SqidsOptions();
            
            if (!string.IsNullOrWhiteSpace(Alphabet))
                options.Alphabet = Alphabet;
                
            if (MinLength > 0)
                options.MinLength = MinLength;
                
            if (!string.IsNullOrWhiteSpace(Blacklist))
            {
                var blocklist = Blacklist.Split(',').Select(s => s.Trim()).Where(s => !string.IsNullOrEmpty(s));
                foreach (var word in blocklist)
                {
                    options.BlockList.Add(word);
                }
            }
            
            _encoder = new SqidsEncoder<int>(options);
        }
        catch
        {
            // 如果配置无效，使用默认编码器
            _encoder = new SqidsEncoder<int>();
        }
    }

    private async Task CopyToClipboard(string text)
    {
        try
        {
            if (string.IsNullOrEmpty(text))
                return;

            var topLevel = TopLevel.GetTopLevel(Application.Current?.ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop 
                ? desktop.MainWindow : null);
            
            if (topLevel?.Clipboard != null)
            {
                await topLevel.Clipboard.SetTextAsync(text);
            }
        }
        catch (Exception)
        {
            // Handle clipboard error silently
        }
    }

    [RelayCommand]
    private void EncodeNumbers()
    {
        try
        {
            if (string.IsNullOrWhiteSpace(NumberInput))
            {
                EncodedId = "请输入要编码的数字";
                return;
            }

            var numbers = NumberInput.Split(',')
                .Select(s => s.Trim())
                .Where(s => !string.IsNullOrEmpty(s))
                .Select(int.Parse)
                .ToArray();

            var encoded = _encoder.Encode(numbers);
            EncodedId = encoded;
            
            // 添加到历史记录
            var historyEntry = $"{string.Join(",", numbers)} → {encoded}";
            EncodingHistory.Insert(0, historyEntry);
            
            // 限制历史记录数量
            while (EncodingHistory.Count > 50)
                EncodingHistory.RemoveAt(EncodingHistory.Count - 1);
        }
        catch (Exception ex)
        {
            EncodedId = $"编码失败: {ex.Message}";
        }
    }

    [RelayCommand]
    private void DecodeId()
    {
        try
        {
            if (string.IsNullOrWhiteSpace(IdInput))
            {
                DecodedNumbers = "请输入要解码的ID";
                return;
            }

            var decoded = _encoder.Decode(IdInput);
            DecodedNumbers = string.Join(",", decoded);
            
            // 添加到历史记录
            var historyEntry = $"{IdInput} → {string.Join(",", decoded)}";
            DecodingHistory.Insert(0, historyEntry);
            
            // 限制历史记录数量
            while (DecodingHistory.Count > 50)
                DecodingHistory.RemoveAt(DecodingHistory.Count - 1);
        }
        catch (Exception ex)
        {
            DecodedNumbers = $"解码失败: {ex.Message}";
        }
    }

    [RelayCommand]
    private async Task CopyEncoded()
    {
        await CopyToClipboard(EncodedId);
    }

    [RelayCommand]
    private async Task CopyDecoded()
    {
        await CopyToClipboard(DecodedNumbers);
    }

    [RelayCommand]
    private void ValidateId()
    {
        try
        {
            if (string.IsNullOrWhiteSpace(IdInput))
            {
                ValidationResult = "请输入要验证的ID";
                return;
            }

            var decoded = _encoder.Decode(IdInput);
            var reencoded = _encoder.Encode(decoded.ToArray());
            
            if (reencoded == IdInput)
            {
                ValidationResult = $"✓ 有效的Sqids ID (解码为: {string.Join(",", decoded)})";
            }
            else
            {
                ValidationResult = "✗ 无效的Sqids ID";
            }
        }
        catch (Exception)
        {
            ValidationResult = "✗ 无效的Sqids ID";
        }
    }

    [RelayCommand]
    private void ProcessBatch()
    {
        BatchResults.Clear();
        
        if (string.IsNullOrWhiteSpace(BatchInput))
            return;

        var lines = BatchInput.Split('\n', StringSplitOptions.RemoveEmptyEntries);
        
        foreach (var line in lines)
        {
            try
            {
                var trimmed = line.Trim();
                if (string.IsNullOrEmpty(trimmed))
                    continue;

                // 尝试作为数字编码
                if (trimmed.All(c => char.IsDigit(c) || c == ',' || c == ' '))
                {
                    var numbers = trimmed.Split(',')
                        .Select(s => s.Trim())
                        .Where(s => !string.IsNullOrEmpty(s))
                        .Select(int.Parse)
                        .ToArray();
                    
                    var encoded = _encoder.Encode(numbers);
                    BatchResults.Add($"编码: {trimmed} → {encoded}");
                }
                else
                {
                    // 尝试作为ID解码
                    var decoded = _encoder.Decode(trimmed);
                    BatchResults.Add($"解码: {trimmed} → {string.Join(",", decoded)}");
                }
            }
            catch (Exception ex)
            {
                BatchResults.Add($"错误: {line.Trim()} - {ex.Message}");
            }
        }
    }

    [RelayCommand]
    private async Task CopyBatchResults()
    {
        if (BatchResults.Count > 0)
        {
            var results = string.Join("\n", BatchResults);
            await CopyToClipboard(results);
        }
    }

    [RelayCommand]
    private async Task CopyEncodingHistory()
    {
        if (EncodingHistory.Count > 0)
        {
            var history = string.Join("\n", EncodingHistory);
            await CopyToClipboard(history);
        }
    }

    [RelayCommand]
    private async Task CopyDecodingHistory()
    {
        if (DecodingHistory.Count > 0)
        {
            var history = string.Join("\n", DecodingHistory);
            await CopyToClipboard(history);
        }
    }

    [RelayCommand]
    private void ClearHistory()
    {
        EncodingHistory.Clear();
        DecodingHistory.Clear();
    }

    [RelayCommand]
    private void ResetSettings()
    {
        Alphabet = "abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789";
        MinLength = 0;
        Blacklist = "";
    }
}