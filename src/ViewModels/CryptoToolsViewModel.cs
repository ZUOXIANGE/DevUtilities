using System;
using System.Collections.Generic;
using System.IO;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.ApplicationLifetimes;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

namespace DevUtilities.ViewModels;

public partial class CryptoToolsViewModel : BaseToolViewModel
{
    [ObservableProperty]
    private string inputText = "";

    [ObservableProperty]
    private string outputText = "";

    [ObservableProperty]
    private string keyText = "";

    [ObservableProperty]
    private string ivText = "";

    [ObservableProperty]
    private string selectedAlgorithm = "AES";

    [ObservableProperty]
    private string selectedMode = "CBC";

    [ObservableProperty]
    private string selectedPadding = "PKCS7";

    [ObservableProperty]
    private string selectedHashAlgorithm = "SHA256";

    [ObservableProperty]
    private string selectedEncoding = "UTF-8";

    [ObservableProperty]
    private bool isEncryptMode = true;

    [ObservableProperty]
    private string validationMessage = "";

    [ObservableProperty]
    private bool isValidInput = true;

    [ObservableProperty]
    private string hashOutput = "";

    [ObservableProperty]
    private string hmacKey = "";

    [ObservableProperty]
    private string hmacOutput = "";

    public List<string> AvailableAlgorithms { get; } = new()
    {
        "AES", "DES", "TripleDES", "RC2"
    };

    public List<string> AvailableModes { get; } = new()
    {
        "CBC", "ECB", "CFB", "OFB"
    };

    public List<string> AvailablePaddings { get; } = new()
    {
        "PKCS7", "Zeros", "None"
    };

    public List<string> AvailableHashAlgorithms { get; } = new()
    {
        "MD5", "SHA1", "SHA256", "SHA384", "SHA512"
    };

    public List<string> AvailableEncodings { get; } = new()
    {
        "UTF-8", "UTF-16", "ASCII", "GBK"
    };

    public CryptoToolsViewModel()
    {
        GenerateRandomKey();
        GenerateRandomIV();
    }

    [RelayCommand]
    private async Task EncryptAsync()
    {
        try
        {
            IsEncryptMode = true;
            await PerformCryptoOperationAsync(true);
        }
        catch (Exception ex)
        {
            ValidationMessage = $"加密失败: {ex.Message}";
            IsValidInput = false;
        }
    }

    [RelayCommand]
    private async Task DecryptAsync()
    {
        try
        {
            IsEncryptMode = false;
            await PerformCryptoOperationAsync(false);
        }
        catch (Exception ex)
        {
            ValidationMessage = $"解密失败: {ex.Message}";
            IsValidInput = false;
        }
    }

    [RelayCommand]
    private async Task ComputeHashAsync()
    {
        try
        {
            if (string.IsNullOrEmpty(InputText))
            {
                ValidationMessage = "请输入要计算哈希的文本";
                IsValidInput = false;
                return;
            }

            var encoding = GetEncoding(SelectedEncoding);
            var inputBytes = encoding.GetBytes(InputText);

            using var hashAlgorithm = CreateHashAlgorithm(SelectedHashAlgorithm);
            var hashBytes = hashAlgorithm.ComputeHash(inputBytes);
            HashOutput = Convert.ToHexString(hashBytes).ToLower();

            ValidationMessage = "哈希计算成功";
            IsValidInput = true;
            
            await Task.CompletedTask;
        }
        catch (Exception ex)
        {
            ValidationMessage = $"哈希计算失败: {ex.Message}";
            IsValidInput = false;
        }
    }

    [RelayCommand]
    private async Task ComputeHmacAsync()
    {
        try
        {
            if (string.IsNullOrEmpty(InputText))
            {
                ValidationMessage = "请输入要计算HMAC的文本";
                IsValidInput = false;
                return;
            }

            if (string.IsNullOrEmpty(HmacKey))
            {
                ValidationMessage = "请输入HMAC密钥";
                IsValidInput = false;
                return;
            }

            var encoding = GetEncoding(SelectedEncoding);
            var inputBytes = encoding.GetBytes(InputText);
            var keyBytes = encoding.GetBytes(HmacKey);

            using var hmac = CreateHmacAlgorithm(SelectedHashAlgorithm, keyBytes);
            var hmacBytes = hmac.ComputeHash(inputBytes);
            HmacOutput = Convert.ToHexString(hmacBytes).ToLower();

            ValidationMessage = "HMAC计算成功";
            IsValidInput = true;
            
            await Task.CompletedTask;
        }
        catch (Exception ex)
        {
            ValidationMessage = $"HMAC计算失败: {ex.Message}";
            IsValidInput = false;
        }
    }

    [RelayCommand]
    private void GenerateRandomKey()
    {
        try
        {
            var keySize = GetKeySize(SelectedAlgorithm);
            var keyBytes = new byte[keySize / 8];
            using var rng = RandomNumberGenerator.Create();
            rng.GetBytes(keyBytes);
            KeyText = Convert.ToHexString(keyBytes).ToLower();
        }
        catch (Exception ex)
        {
            ValidationMessage = $"生成密钥失败: {ex.Message}";
        }
    }

    [RelayCommand]
    private void GenerateRandomIV()
    {
        try
        {
            var blockSize = GetBlockSize(SelectedAlgorithm);
            var ivBytes = new byte[blockSize / 8];
            using var rng = RandomNumberGenerator.Create();
            rng.GetBytes(ivBytes);
            IvText = Convert.ToHexString(ivBytes).ToLower();
        }
        catch (Exception ex)
        {
            ValidationMessage = $"生成IV失败: {ex.Message}";
        }
    }

    [RelayCommand]
    private async Task SwapInputOutputAsync()
    {
        (InputText, OutputText) = (OutputText, InputText);
        await Task.CompletedTask;
    }

    [RelayCommand]
    private void ClearAll()
    {
        InputText = "";
        OutputText = "";
        KeyText = "";
        IvText = "";
        HashOutput = "";
        HmacOutput = "";
        ValidationMessage = "";
        IsValidInput = true;
    }

    [RelayCommand]
    private async Task CopyOutputAsync()
    {
        try
        {
            if (Application.Current?.ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
            {
                var clipboard = desktop.MainWindow?.Clipboard;
                if (clipboard != null && !string.IsNullOrEmpty(OutputText))
                {
                    await clipboard.SetTextAsync(OutputText);
                    ValidationMessage = "输出已复制到剪贴板";
                }
            }
        }
        catch (Exception ex)
        {
            ValidationMessage = $"复制失败: {ex.Message}";
        }
    }

    [RelayCommand]
    private async Task CopyHashAsync()
    {
        try
        {
            if (Application.Current?.ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
            {
                var clipboard = desktop.MainWindow?.Clipboard;
                if (clipboard != null && !string.IsNullOrEmpty(HashOutput))
                {
                    await clipboard.SetTextAsync(HashOutput);
                    ValidationMessage = "哈希值已复制到剪贴板";
                }
            }
        }
        catch (Exception ex)
        {
            ValidationMessage = $"复制失败: {ex.Message}";
        }
    }

    [RelayCommand]
    private async Task CopyHmacAsync()
    {
        try
        {
            if (Application.Current?.ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
            {
                var clipboard = desktop.MainWindow?.Clipboard;
                if (clipboard != null && !string.IsNullOrEmpty(HmacOutput))
                {
                    await clipboard.SetTextAsync(HmacOutput);
                    ValidationMessage = "HMAC值已复制到剪贴板";
                }
            }
        }
        catch (Exception ex)
        {
            ValidationMessage = $"复制失败: {ex.Message}";
        }
    }

    private async Task PerformCryptoOperationAsync(bool encrypt)
    {
        if (string.IsNullOrEmpty(InputText))
        {
            ValidationMessage = "请输入要处理的文本";
            IsValidInput = false;
            return;
        }

        if (string.IsNullOrEmpty(KeyText))
        {
            ValidationMessage = "请输入密钥";
            IsValidInput = false;
            return;
        }

        var encoding = GetEncoding(SelectedEncoding);
        
        using var algorithm = CreateSymmetricAlgorithm(SelectedAlgorithm);
        algorithm.Mode = GetCipherMode(SelectedMode);
        algorithm.Padding = GetPaddingMode(SelectedPadding);

        var keyBytes = Convert.FromHexString(KeyText);
        var ivBytes = !string.IsNullOrEmpty(IvText) ? Convert.FromHexString(IvText) : new byte[algorithm.BlockSize / 8];

        algorithm.Key = keyBytes;
        algorithm.IV = ivBytes;

        if (encrypt)
        {
            var inputBytes = encoding.GetBytes(InputText);
            using var encryptor = algorithm.CreateEncryptor();
            var encryptedBytes = encryptor.TransformFinalBlock(inputBytes, 0, inputBytes.Length);
            OutputText = Convert.ToHexString(encryptedBytes).ToLower();
        }
        else
        {
            var inputBytes = Convert.FromHexString(InputText);
            using var decryptor = algorithm.CreateDecryptor();
            var decryptedBytes = decryptor.TransformFinalBlock(inputBytes, 0, inputBytes.Length);
            OutputText = encoding.GetString(decryptedBytes);
        }

        ValidationMessage = encrypt ? "加密成功" : "解密成功";
        IsValidInput = true;
    }

    private SymmetricAlgorithm CreateSymmetricAlgorithm(string algorithm)
    {
        return algorithm switch
        {
            "AES" => Aes.Create(),
            "DES" => DES.Create(),
            "TripleDES" => TripleDES.Create(),
            "RC2" => RC2.Create(),
            _ => Aes.Create()
        };
    }

    private HashAlgorithm CreateHashAlgorithm(string algorithm)
    {
        return algorithm switch
        {
            "MD5" => MD5.Create(),
            "SHA1" => SHA1.Create(),
            "SHA256" => SHA256.Create(),
            "SHA384" => SHA384.Create(),
            "SHA512" => SHA512.Create(),
            _ => SHA256.Create()
        };
    }

    private HMAC CreateHmacAlgorithm(string algorithm, byte[] key)
    {
        return algorithm switch
        {
            "MD5" => new HMACMD5(key),
            "SHA1" => new HMACSHA1(key),
            "SHA256" => new HMACSHA256(key),
            "SHA384" => new HMACSHA384(key),
            "SHA512" => new HMACSHA512(key),
            _ => new HMACSHA256(key)
        };
    }

    private CipherMode GetCipherMode(string mode)
    {
        return mode switch
        {
            "CBC" => CipherMode.CBC,
            "ECB" => CipherMode.ECB,
            "CFB" => CipherMode.CFB,
            "OFB" => CipherMode.OFB,
            _ => CipherMode.CBC
        };
    }

    private PaddingMode GetPaddingMode(string padding)
    {
        return padding switch
        {
            "PKCS7" => PaddingMode.PKCS7,
            "Zeros" => PaddingMode.Zeros,
            "None" => PaddingMode.None,
            _ => PaddingMode.PKCS7
        };
    }

    private Encoding GetEncoding(string encoding)
    {
        return encoding switch
        {
            "UTF-8" => Encoding.UTF8,
            "UTF-16" => Encoding.Unicode,
            "ASCII" => Encoding.ASCII,
            "GBK" => Encoding.GetEncoding("GBK"),
            _ => Encoding.UTF8
        };
    }

    private int GetKeySize(string algorithm)
    {
        return algorithm switch
        {
            "AES" => 256,
            "DES" => 64,
            "TripleDES" => 192,
            "RC2" => 128,
            _ => 256
        };
    }

    private int GetBlockSize(string algorithm)
    {
        return algorithm switch
        {
            "AES" => 128,
            "DES" => 64,
            "TripleDES" => 64,
            "RC2" => 64,
            _ => 128
        };
    }
}