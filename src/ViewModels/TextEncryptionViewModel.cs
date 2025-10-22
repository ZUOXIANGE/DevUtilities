using System;
using System.Collections.ObjectModel;
using System.IO;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using Avalonia.Controls.ApplicationLifetimes;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

namespace DevUtilities.ViewModels;

public partial class TextEncryptionViewModel : ObservableObject
{
    [ObservableProperty]
    private string _inputText = string.Empty;

    [ObservableProperty]
    private string _outputText = string.Empty;

    [ObservableProperty]
    private string _encryptionKey = string.Empty;

    [ObservableProperty]
    private string _initializationVector = string.Empty;

    [ObservableProperty]
    private string _selectedAlgorithm = "AES";

    [ObservableProperty]
    private string _selectedMode = "CBC";

    [ObservableProperty]
    private string _selectedPadding = "PKCS7";

    [ObservableProperty]
    private bool _isEncryptMode = true;

    [ObservableProperty]
    private string _statusMessage = string.Empty;

    public ObservableCollection<string> EncryptionAlgorithms { get; } = new()
    {
        "AES", "DES", "3DES"
    };

    public ObservableCollection<string> CipherModes { get; } = new()
    {
        "CBC", "ECB", "CFB", "OFB"
    };

    public ObservableCollection<string> PaddingModes { get; } = new()
    {
        "PKCS7", "Zeros", "None"
    };

    [RelayCommand]
    private void ProcessText()
    {
        try
        {
            if (string.IsNullOrEmpty(InputText))
            {
                StatusMessage = "请输入要处理的文本";
                return;
            }

            if (string.IsNullOrEmpty(EncryptionKey))
            {
                StatusMessage = "请输入加密密钥";
                return;
            }

            if (IsEncryptMode)
            {
                OutputText = EncryptText(InputText, EncryptionKey);
                StatusMessage = "加密成功";
            }
            else
            {
                OutputText = DecryptText(InputText, EncryptionKey);
                StatusMessage = "解密成功";
            }
        }
        catch (Exception ex)
        {
            StatusMessage = $"操作失败: {ex.Message}";
            OutputText = string.Empty;
        }
    }

    [RelayCommand]
    private void GenerateKey()
    {
        try
        {
            var keySize = SelectedAlgorithm switch
            {
                "AES" => 32, // 256 bits
                "DES" => 8,  // 64 bits
                "3DES" => 24, // 192 bits
                _ => 32
            };

            var key = new byte[keySize];
            using (var rng = RandomNumberGenerator.Create())
            {
                rng.GetBytes(key);
            }
            EncryptionKey = Convert.ToBase64String(key);

            // Generate IV if needed
            if (SelectedMode != "ECB")
            {
                var ivSize = SelectedAlgorithm switch
                {
                    "AES" => 16,
                    "DES" => 8,
                    "3DES" => 8,
                    _ => 16
                };

                var iv = new byte[ivSize];
                using (var rng = RandomNumberGenerator.Create())
                {
                    rng.GetBytes(iv);
                }
                InitializationVector = Convert.ToBase64String(iv);
            }

            StatusMessage = "密钥和初始化向量生成成功";
        }
        catch (Exception ex)
        {
            StatusMessage = $"密钥生成失败: {ex.Message}";
        }
    }

    [RelayCommand]
    private void ClearAll()
    {
        InputText = string.Empty;
        OutputText = string.Empty;
        EncryptionKey = string.Empty;
        InitializationVector = string.Empty;
        StatusMessage = string.Empty;
    }

    [RelayCommand]
    private void SwapInputOutput()
    {
        (InputText, OutputText) = (OutputText, InputText);
        IsEncryptMode = !IsEncryptMode;
        StatusMessage = IsEncryptMode ? "切换到加密模式" : "切换到解密模式";
    }

    [RelayCommand]
    private async Task CopyToClipboard(string text)
    {
        try
        {
            if (Avalonia.Application.Current?.ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
            {
                var clipboard = desktop.MainWindow?.Clipboard;
                if (clipboard != null)
                {
                    await clipboard.SetTextAsync(text ?? string.Empty);
                    StatusMessage = "已复制到剪贴板";
                }
            }
        }
        catch (Exception ex)
        {
            StatusMessage = $"复制失败: {ex.Message}";
        }
    }

    private string EncryptText(string plainText, string key)
    {
        var keyBytes = Convert.FromBase64String(key);
        var plainBytes = Encoding.UTF8.GetBytes(plainText);

        return SelectedAlgorithm switch
        {
            "AES" => EncryptAES(plainBytes, keyBytes),
            "DES" => EncryptDES(plainBytes, keyBytes),
            "3DES" => Encrypt3DES(plainBytes, keyBytes),
            _ => throw new NotSupportedException($"不支持的加密算法: {SelectedAlgorithm}")
        };
    }

    private string DecryptText(string cipherText, string key)
    {
        var keyBytes = Convert.FromBase64String(key);
        var cipherBytes = Convert.FromBase64String(cipherText);

        var plainBytes = SelectedAlgorithm switch
        {
            "AES" => DecryptAES(cipherBytes, keyBytes),
            "DES" => DecryptDES(cipherBytes, keyBytes),
            "3DES" => Decrypt3DES(cipherBytes, keyBytes),
            _ => throw new NotSupportedException($"不支持的加密算法: {SelectedAlgorithm}")
        };

        return Encoding.UTF8.GetString(plainBytes);
    }

    private string EncryptAES(byte[] plainBytes, byte[] keyBytes)
    {
        using var aes = Aes.Create();
        aes.Key = keyBytes;
        aes.Mode = GetCipherMode();
        aes.Padding = GetPaddingMode();

        if (aes.Mode != CipherMode.ECB)
        {
            if (!string.IsNullOrEmpty(InitializationVector))
            {
                aes.IV = Convert.FromBase64String(InitializationVector);
            }
        }

        using var encryptor = aes.CreateEncryptor();
        using var msEncrypt = new MemoryStream();
        using var csEncrypt = new CryptoStream(msEncrypt, encryptor, CryptoStreamMode.Write);
        csEncrypt.Write(plainBytes, 0, plainBytes.Length);
        csEncrypt.FlushFinalBlock();

        return Convert.ToBase64String(msEncrypt.ToArray());
    }

    private byte[] DecryptAES(byte[] cipherBytes, byte[] keyBytes)
    {
        using var aes = Aes.Create();
        aes.Key = keyBytes;
        aes.Mode = GetCipherMode();
        aes.Padding = GetPaddingMode();

        if (aes.Mode != CipherMode.ECB)
        {
            if (!string.IsNullOrEmpty(InitializationVector))
            {
                aes.IV = Convert.FromBase64String(InitializationVector);
            }
        }

        using var decryptor = aes.CreateDecryptor();
        using var msDecrypt = new MemoryStream(cipherBytes);
        using var csDecrypt = new CryptoStream(msDecrypt, decryptor, CryptoStreamMode.Read);
        using var msPlain = new MemoryStream();
        csDecrypt.CopyTo(msPlain);

        return msPlain.ToArray();
    }

    private string EncryptDES(byte[] plainBytes, byte[] keyBytes)
    {
        using var des = DES.Create();
        des.Key = keyBytes;
        des.Mode = GetCipherMode();
        des.Padding = GetPaddingMode();

        if (des.Mode != CipherMode.ECB)
        {
            if (!string.IsNullOrEmpty(InitializationVector))
            {
                des.IV = Convert.FromBase64String(InitializationVector);
            }
        }

        using var encryptor = des.CreateEncryptor();
        using var msEncrypt = new MemoryStream();
        using var csEncrypt = new CryptoStream(msEncrypt, encryptor, CryptoStreamMode.Write);
        csEncrypt.Write(plainBytes, 0, plainBytes.Length);
        csEncrypt.FlushFinalBlock();

        return Convert.ToBase64String(msEncrypt.ToArray());
    }

    private byte[] DecryptDES(byte[] cipherBytes, byte[] keyBytes)
    {
        using var des = DES.Create();
        des.Key = keyBytes;
        des.Mode = GetCipherMode();
        des.Padding = GetPaddingMode();

        if (des.Mode != CipherMode.ECB)
        {
            if (!string.IsNullOrEmpty(InitializationVector))
            {
                des.IV = Convert.FromBase64String(InitializationVector);
            }
        }

        using var decryptor = des.CreateDecryptor();
        using var msDecrypt = new MemoryStream(cipherBytes);
        using var csDecrypt = new CryptoStream(msDecrypt, decryptor, CryptoStreamMode.Read);
        using var msPlain = new MemoryStream();
        csDecrypt.CopyTo(msPlain);

        return msPlain.ToArray();
    }

    private string Encrypt3DES(byte[] plainBytes, byte[] keyBytes)
    {
        using var des3 = TripleDES.Create();
        des3.Key = keyBytes;
        des3.Mode = GetCipherMode();
        des3.Padding = GetPaddingMode();

        if (des3.Mode != CipherMode.ECB)
        {
            if (!string.IsNullOrEmpty(InitializationVector))
            {
                des3.IV = Convert.FromBase64String(InitializationVector);
            }
        }

        using var encryptor = des3.CreateEncryptor();
        using var msEncrypt = new MemoryStream();
        using var csEncrypt = new CryptoStream(msEncrypt, encryptor, CryptoStreamMode.Write);
        csEncrypt.Write(plainBytes, 0, plainBytes.Length);
        csEncrypt.FlushFinalBlock();

        return Convert.ToBase64String(msEncrypt.ToArray());
    }

    private byte[] Decrypt3DES(byte[] cipherBytes, byte[] keyBytes)
    {
        using var des3 = TripleDES.Create();
        des3.Key = keyBytes;
        des3.Mode = GetCipherMode();
        des3.Padding = GetPaddingMode();

        if (des3.Mode != CipherMode.ECB)
        {
            if (!string.IsNullOrEmpty(InitializationVector))
            {
                des3.IV = Convert.FromBase64String(InitializationVector);
            }
        }

        using var decryptor = des3.CreateDecryptor();
        using var msDecrypt = new MemoryStream(cipherBytes);
        using var csDecrypt = new CryptoStream(msDecrypt, decryptor, CryptoStreamMode.Read);
        using var msPlain = new MemoryStream();
        csDecrypt.CopyTo(msPlain);

        return msPlain.ToArray();
    }

    private CipherMode GetCipherMode()
    {
        return SelectedMode switch
        {
            "CBC" => CipherMode.CBC,
            "ECB" => CipherMode.ECB,
            "CFB" => CipherMode.CFB,
            "OFB" => CipherMode.OFB,
            _ => CipherMode.CBC
        };
    }

    private PaddingMode GetPaddingMode()
    {
        return SelectedPadding switch
        {
            "PKCS7" => PaddingMode.PKCS7,
            "Zeros" => PaddingMode.Zeros,
            "None" => PaddingMode.None,
            _ => PaddingMode.PKCS7
        };
    }
}