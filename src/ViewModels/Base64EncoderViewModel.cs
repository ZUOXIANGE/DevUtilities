using System;
using System.Text;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

namespace DevUtilities.ViewModels;

public partial class Base64EncoderViewModel : ObservableObject
{
    [ObservableProperty]
    private string inputText = "";

    [ObservableProperty]
    private string outputText = "";

    [ObservableProperty]
    private string selectedEncoding = "UTF-8";

    [ObservableProperty]
    private bool isEncodeMode = true;

    public string[] AvailableEncodings { get; } = { "UTF-8", "UTF-16", "ASCII", "GBK" };

    [RelayCommand]
    private void Encode()
    {
        try
        {
            if (string.IsNullOrEmpty(InputText))
            {
                OutputText = "";
                return;
            }

            var encoding = GetEncoding(SelectedEncoding);
            var bytes = encoding.GetBytes(InputText);
            OutputText = Convert.ToBase64String(bytes);
        }
        catch (Exception ex)
        {
            OutputText = $"编码错误: {ex.Message}";
        }
    }

    [RelayCommand]
    private void Decode()
    {
        try
        {
            if (string.IsNullOrEmpty(InputText))
            {
                OutputText = "";
                return;
            }

            var bytes = Convert.FromBase64String(InputText);
            var encoding = GetEncoding(SelectedEncoding);
            OutputText = encoding.GetString(bytes);
        }
        catch (Exception ex)
        {
            OutputText = $"解码错误: {ex.Message}";
        }
    }

    [RelayCommand]
    private void Process()
    {
        if (IsEncodeMode)
            Encode();
        else
            Decode();
    }

    [RelayCommand]
    private void SwapInputOutput()
    {
        (InputText, OutputText) = (OutputText, InputText);
        IsEncodeMode = !IsEncodeMode;
    }

    [RelayCommand]
    private void Clear()
    {
        InputText = "";
        OutputText = "";
    }

    private Encoding GetEncoding(string encodingName)
    {
        return encodingName switch
        {
            "UTF-8" => Encoding.UTF8,
            "UTF-16" => Encoding.Unicode,
            "ASCII" => Encoding.ASCII,
            "GBK" => Encoding.GetEncoding("GBK"),
            _ => Encoding.UTF8
        };
    }

    partial void OnInputTextChanged(string value)
    {
        Process();
    }

    partial void OnSelectedEncodingChanged(string value)
    {
        Process();
    }

    partial void OnIsEncodeModeChanged(bool value)
    {
        Process();
    }
}