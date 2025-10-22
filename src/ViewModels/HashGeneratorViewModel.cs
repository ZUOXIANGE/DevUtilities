using System;
using System.Collections.Generic;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using Avalonia.Controls.ApplicationLifetimes;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

namespace DevUtilities.ViewModels;

public partial class HashGeneratorViewModel : ObservableObject
{
    [ObservableProperty]
    private string inputText = string.Empty;

    [ObservableProperty]
    private string md5Hash = string.Empty;

    [ObservableProperty]
    private string sha1Hash = string.Empty;

    [ObservableProperty]
    private string sha256Hash = string.Empty;

    [ObservableProperty]
    private string sha384Hash = string.Empty;

    [ObservableProperty]
    private string sha512Hash = string.Empty;

    [ObservableProperty]
    private bool isUpperCase = false;

    [ObservableProperty]
    private string selectedEncoding = "UTF-8";

    public List<string> EncodingOptions { get; } = new()
    {
        "UTF-8",
        "UTF-16",
        "ASCII",
        "GB2312"
    };

    public HashGeneratorViewModel()
    {
        // 监听输入文本变化
        PropertyChanged += (sender, e) =>
        {
            if (e.PropertyName == nameof(InputText) || 
                e.PropertyName == nameof(IsUpperCase) || 
                e.PropertyName == nameof(SelectedEncoding))
            {
                GenerateAllHashes();
            }
        };
    }

    [RelayCommand]
    private void GenerateAllHashes()
    {
        if (string.IsNullOrEmpty(InputText))
        {
            ClearAllHashes();
            return;
        }

        try
        {
            var encoding = GetSelectedEncoding();
            var bytes = encoding.GetBytes(InputText);

            Md5Hash = ComputeHash(MD5.Create(), bytes);
            Sha1Hash = ComputeHash(SHA1.Create(), bytes);
            Sha256Hash = ComputeHash(SHA256.Create(), bytes);
            Sha384Hash = ComputeHash(SHA384.Create(), bytes);
            Sha512Hash = ComputeHash(SHA512.Create(), bytes);
        }
        catch (Exception)
        {
            ClearAllHashes();
            // 可以添加错误处理逻辑
        }
    }

    [RelayCommand]
    private void ClearAll()
    {
        InputText = string.Empty;
        ClearAllHashes();
    }

    [RelayCommand]
    private async Task CopyToClipboard(string hashValue)
    {
        if (!string.IsNullOrEmpty(hashValue))
        {
            // 复制到剪贴板的逻辑
            try
            {
                if (Avalonia.Application.Current?.ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
                {
                    var clipboard = desktop.MainWindow?.Clipboard;
                    if (clipboard != null)
                    {
                        await clipboard.SetTextAsync(hashValue);
                    }
                }
            }
            catch
            {
                // 忽略剪贴板错误
            }
        }
    }

    private void ClearAllHashes()
    {
        Md5Hash = string.Empty;
        Sha1Hash = string.Empty;
        Sha256Hash = string.Empty;
        Sha384Hash = string.Empty;
        Sha512Hash = string.Empty;
    }

    private string ComputeHash(HashAlgorithm algorithm, byte[] bytes)
    {
        using (algorithm)
        {
            var hashBytes = algorithm.ComputeHash(bytes);
            var result = BitConverter.ToString(hashBytes).Replace("-", "");
            return IsUpperCase ? result.ToUpper() : result.ToLower();
        }
    }

    private Encoding GetSelectedEncoding()
    {
        return SelectedEncoding switch
        {
            "UTF-8" => Encoding.UTF8,
            "UTF-16" => Encoding.Unicode,
            "ASCII" => Encoding.ASCII,
            "GB2312" => Encoding.GetEncoding("GB2312"),
            _ => Encoding.UTF8
        };
    }
}