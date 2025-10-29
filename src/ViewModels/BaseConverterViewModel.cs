using System;
using System.Linq;
using Avalonia;
using Avalonia.Controls.ApplicationLifetimes;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using DevUtilities.Core.ViewModels;

namespace DevUtilities.ViewModels;

public partial class BaseConverterViewModel : BaseViewModel
{
    [ObservableProperty]
    private string binaryValue = "";

    [ObservableProperty]
    private string octalValue = "";

    [ObservableProperty]
    private string decimalValue = "";

    [ObservableProperty]
    private string hexValue = "";

    [ObservableProperty]
    private string errorMessage = "";

    [ObservableProperty]
    private bool hasError = false;

    private bool _isUpdating = false;

    public BaseConverterViewModel()
    {
        // 初始化为0
        UpdateAllValues(0);
    }

    partial void OnBinaryValueChanged(string value)
    {
        if (_isUpdating) return;
        ConvertFromBinary(value);
    }

    partial void OnOctalValueChanged(string value)
    {
        if (_isUpdating) return;
        ConvertFromOctal(value);
    }

    partial void OnDecimalValueChanged(string value)
    {
        if (_isUpdating) return;
        ConvertFromDecimal(value);
    }

    partial void OnHexValueChanged(string value)
    {
        if (_isUpdating) return;
        ConvertFromHex(value);
    }

    private void ConvertFromBinary(string binary)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(binary))
            {
                ClearAllValues();
                return;
            }

            // 验证二进制格式
            if (!binary.All(c => c == '0' || c == '1'))
            {
                SetError("二进制数只能包含0和1");
                return;
            }

            long decimalValue = Convert.ToInt64(binary, 2);
            UpdateAllValues(decimalValue);
            ClearError();
        }
        catch (Exception ex)
        {
            SetError($"二进制转换错误: {ex.Message}");
        }
    }

    private void ConvertFromOctal(string octal)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(octal))
            {
                ClearAllValues();
                return;
            }

            // 验证八进制格式
            if (!octal.All(c => c >= '0' && c <= '7'))
            {
                SetError("八进制数只能包含0-7的数字");
                return;
            }

            long decimalValue = Convert.ToInt64(octal, 8);
            UpdateAllValues(decimalValue);
            ClearError();
        }
        catch (Exception ex)
        {
            SetError($"八进制转换错误: {ex.Message}");
        }
    }

    private void ConvertFromDecimal(string decimalStr)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(decimalStr))
            {
                ClearAllValues();
                return;
            }

            if (!long.TryParse(decimalStr, out long decimalValue))
            {
                SetError("请输入有效的十进制数字");
                return;
            }

            UpdateAllValues(decimalValue);
            ClearError();
        }
        catch (Exception ex)
        {
            SetError($"十进制转换错误: {ex.Message}");
        }
    }

    private void ConvertFromHex(string hex)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(hex))
            {
                ClearAllValues();
                return;
            }

            // 移除可能的0x前缀
            if (hex.StartsWith("0x", StringComparison.OrdinalIgnoreCase))
            {
                hex = hex.Substring(2);
            }

            // 验证十六进制格式
            if (!hex.All(c => char.IsDigit(c) || (char.ToUpper(c) >= 'A' && char.ToUpper(c) <= 'F')))
            {
                SetError("十六进制数只能包含0-9和A-F的字符");
                return;
            }

            long decimalValue = Convert.ToInt64(hex, 16);
            UpdateAllValues(decimalValue);
            ClearError();
        }
        catch (Exception ex)
        {
            SetError($"十六进制转换错误: {ex.Message}");
        }
    }

    private void UpdateAllValues(long decimalValue)
    {
        _isUpdating = true;
        try
        {
            BinaryValue = Convert.ToString(decimalValue, 2);
            OctalValue = Convert.ToString(decimalValue, 8);
            DecimalValue = decimalValue.ToString();
            HexValue = Convert.ToString(decimalValue, 16).ToUpper();
        }
        finally
        {
            _isUpdating = false;
        }
    }

    private void ClearAllValues()
    {
        _isUpdating = true;
        try
        {
            BinaryValue = "";
            OctalValue = "";
            DecimalValue = "";
            HexValue = "";
        }
        finally
        {
            _isUpdating = false;
        }
    }

    private void SetError(string message)
    {
        ErrorMessage = message;
        HasError = true;
    }

    private void ClearError()
    {
        ErrorMessage = "";
        HasError = false;
    }

    [RelayCommand]
    private void Clear()
    {
        ClearAllValues();
        ClearError();
    }

    [RelayCommand]
    private void CopyBinary()
    {
        CopyToClipboard(BinaryValue);
    }

    [RelayCommand]
    private void CopyOctal()
    {
        CopyToClipboard(OctalValue);
    }

    [RelayCommand]
    private void CopyDecimal()
    {
        CopyToClipboard(DecimalValue);
    }

    [RelayCommand]
    private void CopyHex()
    {
        CopyToClipboard(HexValue);
    }

    private async void CopyToClipboard(string text)
    {
        try
        {
            if (Application.Current?.ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
            {
                var clipboard = desktop.MainWindow?.Clipboard;
                if (clipboard != null)
                {
                    await clipboard.SetTextAsync(text);
                    SetError("已复制到剪贴板");
                    // 清除错误状态，显示成功消息
                    HasError = false;
                }
            }
        }
        catch (Exception ex)
        {
            SetError($"复制失败: {ex.Message}");
        }
    }
}