using System;
using System.Linq;
using System.Text;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

namespace DevUtilities.ViewModels;

public partial class HexConverterViewModel : ObservableObject
{
    [ObservableProperty]
    private string inputText = "";

    [ObservableProperty]
    private string hexOutput = "";

    [ObservableProperty]
    private string textOutput = "";

    [ObservableProperty]
    private string selectedEncoding = "UTF-8";

    [ObservableProperty]
    private bool useUppercase = true;

    [ObservableProperty]
    private bool addSpaces = true;

    [ObservableProperty]
    private bool addPrefix = false;

    [ObservableProperty]
    private string validationMessage = "";

    [ObservableProperty]
    private bool isValidInput = true;

    public string[] AvailableEncodings { get; } = { "UTF-8", "ASCII", "Unicode", "UTF-32", "GB2312" };

    public HexConverterViewModel()
    {
        // 监听属性变化
        PropertyChanged += (s, e) =>
        {
            if (e.PropertyName == nameof(InputText) || 
                e.PropertyName == nameof(SelectedEncoding) ||
                e.PropertyName == nameof(UseUppercase) ||
                e.PropertyName == nameof(AddSpaces) ||
                e.PropertyName == nameof(AddPrefix))
            {
                ConvertToHex();
            }
        };
    }

    [RelayCommand]
    private void ConvertToHex()
    {
        try
        {
            if (string.IsNullOrEmpty(InputText))
            {
                HexOutput = "";
                ValidationMessage = "";
                IsValidInput = true;
                return;
            }

            var encoding = GetEncoding(SelectedEncoding);
            var bytes = encoding.GetBytes(InputText);
            
            var hexString = Convert.ToHexString(bytes);
            
            if (!UseUppercase)
            {
                hexString = hexString.ToLower();
            }

            if (AddSpaces)
            {
                hexString = string.Join(" ", hexString.Select((c, i) => i % 2 == 0 ? c.ToString() : c.ToString()).Where((s, i) => i % 2 == 0).Select((s, i) => hexString.Substring(i * 2, 2)));
            }

            if (AddPrefix)
            {
                if (AddSpaces)
                {
                    hexString = string.Join(" ", hexString.Split(' ').Select(h => "0x" + h));
                }
                else
                {
                    hexString = "0x" + hexString;
                }
            }

            HexOutput = hexString;
            ValidationMessage = $"成功转换 {bytes.Length} 字节";
            IsValidInput = true;
        }
        catch (Exception ex)
        {
            HexOutput = "";
            ValidationMessage = $"转换失败: {ex.Message}";
            IsValidInput = false;
        }
    }

    [RelayCommand]
    private void ConvertFromHex()
    {
        try
        {
            if (string.IsNullOrEmpty(InputText))
            {
                TextOutput = "";
                ValidationMessage = "";
                IsValidInput = true;
                return;
            }

            // 清理输入：移除空格、0x前缀等
            var cleanHex = InputText.Replace(" ", "")
                                   .Replace("0x", "")
                                   .Replace("0X", "")
                                   .Replace("-", "")
                                   .Replace(":", "");

            // 验证是否为有效的十六进制字符串
            if (!IsValidHexString(cleanHex))
            {
                TextOutput = "";
                ValidationMessage = "输入不是有效的十六进制字符串";
                IsValidInput = false;
                return;
            }

            // 确保长度为偶数
            if (cleanHex.Length % 2 != 0)
            {
                cleanHex = "0" + cleanHex;
            }

            var bytes = Convert.FromHexString(cleanHex);
            var encoding = GetEncoding(SelectedEncoding);
            
            TextOutput = encoding.GetString(bytes);
            ValidationMessage = $"成功解析 {bytes.Length} 字节";
            IsValidInput = true;
        }
        catch (Exception ex)
        {
            TextOutput = "";
            ValidationMessage = $"解析失败: {ex.Message}";
            IsValidInput = false;
        }
    }

    [RelayCommand]
    private void SwapInputOutput()
    {
        if (!string.IsNullOrEmpty(HexOutput))
        {
            InputText = HexOutput;
            ConvertFromHex();
        }
        else if (!string.IsNullOrEmpty(TextOutput))
        {
            InputText = TextOutput;
            ConvertToHex();
        }
    }

    [RelayCommand]
    private void ClearAll()
    {
        InputText = "";
        HexOutput = "";
        TextOutput = "";
        ValidationMessage = "";
        IsValidInput = true;
    }

    [RelayCommand]
    private void UseExample(string example)
    {
        InputText = example;
    }

    private static bool IsValidHexString(string hex)
    {
        return hex.All(c => "0123456789ABCDEFabcdef".Contains(c));
    }

    private static Encoding GetEncoding(string encodingName)
    {
        return encodingName switch
        {
            "UTF-8" => Encoding.UTF8,
            "ASCII" => Encoding.ASCII,
            "Unicode" => Encoding.Unicode,
            "UTF-32" => Encoding.UTF32,
            "GB2312" => Encoding.GetEncoding("GB2312"),
            _ => Encoding.UTF8
        };
    }

    [RelayCommand]
    private void CopyHex()
    {
        if (!string.IsNullOrEmpty(HexOutput))
        {
            // 这里可以添加复制到剪贴板的功能
            // 由于Avalonia的剪贴板API需要在UI线程中调用，这里先留空
        }
    }

    [RelayCommand]
    private void CopyText()
    {
        if (!string.IsNullOrEmpty(TextOutput))
        {
            // 这里可以添加复制到剪贴板的功能
        }
    }
}