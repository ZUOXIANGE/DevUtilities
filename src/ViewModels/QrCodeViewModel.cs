using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using QRCoder;
using ZXing;
using ZXing.Common;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia;
using Avalonia.Platform.Storage;
using SkiaSharp;

namespace DevUtilities.ViewModels;

public partial class QrCodeViewModel : ObservableObject
{
    [ObservableProperty]
    private string inputText = "";

    [ObservableProperty]
    private string decodedText = "";

    [ObservableProperty]
    private Avalonia.Media.Imaging.Bitmap? qrCodeImage;

    [ObservableProperty]
    private string validationMessage = "";

    [ObservableProperty]
    private bool isValidInput = true;

    [ObservableProperty]
    private int qrCodeSize = 300;

    [ObservableProperty]
    private string selectedErrorCorrection = "M";

    [ObservableProperty]
    private string selectedEncoding = "UTF-8";

    [ObservableProperty]
    private bool _includeQuietZone = true;

    [ObservableProperty]
    private string foregroundColor = "#000000";

    [ObservableProperty]
    private string backgroundColor = "#FFFFFF";

    [ObservableProperty]
    private string selectedQrType = "Text";

    public List<string> AvailableErrorCorrections { get; } = new()
    {
        "L", "M", "Q", "H"
    };

    public List<string> AvailableEncodings { get; } = new()
    {
        "UTF-8", "UTF-16", "ASCII", "GBK"
    };

    public List<string> AvailableQrTypes { get; } = new()
    {
        "Text", "URL", "Email", "Phone", "SMS", "WiFi", "VCard"
    };

    public QrCodeViewModel()
    {
        // 初始化示例文本
        InputText = "Hello, QR Code!";
    }

    [RelayCommand]
    private async Task GenerateQrCodeAsync()
    {
        try
        {
            if (string.IsNullOrWhiteSpace(InputText))
            {
                ValidationMessage = "请输入要生成二维码的文本";
                IsValidInput = false;
                return;
            }

            var qrGenerator = new QRCodeGenerator();
            var qrCodeData = qrGenerator.CreateQrCode(InputText, GetErrorCorrectionLevel(SelectedErrorCorrection));
            var qrCode = new PngByteQRCode(qrCodeData);
            
            // 使用SkiaSharp替代System.Drawing进行颜色转换
            var foreColor = SKColor.Parse(ForegroundColor);
            var backColor = SKColor.Parse(BackgroundColor);
            
            // 将SkiaSharp颜色转换为System.Drawing.Color（QRCoder需要）
            var drawingForeColor = System.Drawing.Color.FromArgb(foreColor.Alpha, foreColor.Red, foreColor.Green, foreColor.Blue);
            var drawingBackColor = System.Drawing.Color.FromArgb(backColor.Alpha, backColor.Red, backColor.Green, backColor.Blue);
            
            var qrCodeBytes = qrCode.GetGraphic(QrCodeSize / 10, drawingForeColor, drawingBackColor, IncludeQuietZone);
            
            using var stream = new MemoryStream(qrCodeBytes);
            QrCodeImage = new Avalonia.Media.Imaging.Bitmap(stream);

            ValidationMessage = "二维码生成成功";
            IsValidInput = true;
            
            await Task.CompletedTask;
        }
        catch (Exception ex)
        {
            ValidationMessage = $"生成二维码失败: {ex.Message}";
            IsValidInput = false;
        }
    }

    [RelayCommand]
    private async Task LoadImageAsync()
    {
        try
        {
            var topLevel = App.Current?.ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop
                ? desktop.MainWindow
                : null;

            if (topLevel == null) return;

            var files = await topLevel.StorageProvider.OpenFilePickerAsync(new FilePickerOpenOptions
            {
                Title = "选择二维码图片",
                AllowMultiple = false,
                FileTypeFilter = new[]
                {
                    new FilePickerFileType("图片文件")
                    {
                        Patterns = new[] { "*.png", "*.jpg", "*.jpeg", "*.bmp", "*.gif" }
                    }
                }
            });

            if (files.Count > 0)
            {
                var file = files[0];
                await using var stream = await file.OpenReadAsync();
                
                try
                {
                    // 使用SkiaSharp替代System.Drawing进行图像处理
                    using var skBitmap = SKBitmap.Decode(stream);
                    
                    // 将SkiaSharp位图转换为字节数组用于ZXing
                    using var image = SKImage.FromBitmap(skBitmap);
                    using var data = image.Encode(SKEncodedImageFormat.Png, 100);
                    var bytes = data.ToArray();
                    
                    var luminanceSource = new RGBLuminanceSource(bytes, skBitmap.Width, skBitmap.Height);
                    var binaryBitmap = new BinaryBitmap(new HybridBinarizer(luminanceSource));
                    var reader = new MultiFormatReader();
                    var result = reader.decode(binaryBitmap);
                
                    if (result != null)
                    {
                        DecodedText = result.Text;
                        ValidationMessage = "二维码解码成功！";
                        IsValidInput = true;
                    }
                    else
                    {
                        DecodedText = "";
                        ValidationMessage = "未能识别二维码，请确保图片清晰且包含有效的二维码。";
                        IsValidInput = false;
                    }
                }
                catch (Exception ex)
                {
                    ValidationMessage = $"解码二维码失败: {ex.Message}";
                    IsValidInput = false;
                }
            }
        }
        catch (Exception ex)
        {
            ValidationMessage = $"加载图片失败: {ex.Message}";
            IsValidInput = false;
        }
    }

    [RelayCommand]
    private async Task SaveImageAsync()
    {
        if (QrCodeImage == null)
        {
            ValidationMessage = "没有可保存的二维码图片。";
            IsValidInput = false;
            return;
        }

        try
        {
            var topLevel = App.Current?.ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop
                ? desktop.MainWindow
                : null;

            if (topLevel == null) return;

            var file = await topLevel.StorageProvider.SaveFilePickerAsync(new FilePickerSaveOptions
            {
                Title = "保存二维码图片",
                DefaultExtension = "png",
                FileTypeChoices = new[]
                {
                    new FilePickerFileType("PNG图片")
                    {
                        Patterns = new[] { "*.png" }
                    }
                }
            });

            if (file != null)
            {
                await using var stream = await file.OpenWriteAsync();
                QrCodeImage.Save(stream);
                ValidationMessage = "二维码图片保存成功！";
                IsValidInput = true;
            }
        }
        catch (Exception ex)
        {
            ValidationMessage = $"保存图片失败: {ex.Message}";
            IsValidInput = false;
        }
    }

    [RelayCommand]
    private async Task UseExampleAsync()
    {
        var examples = SelectedQrType switch
        {
            "Text" => "这是一个示例文本二维码",
            "URL" => "https://www.example.com",
            "Email" => "mailto:example@email.com?subject=主题&body=邮件内容",
            "Phone" => "tel:+86-138-0013-8000",
            "SMS" => "sms:+86-138-0013-8000?body=短信内容",
            "WiFi" => "WIFI:T:WPA;S:网络名称;P:密码;H:false;;",
            "VCard" => "BEGIN:VCARD\nVERSION:3.0\nFN:张三\nORG:示例公司\nTEL:+86-138-0013-8000\nEMAIL:zhangsan@example.com\nEND:VCARD",
            _ => "这是一个示例文本二维码"
        };

        InputText = examples;
        ValidationMessage = $"已加载{SelectedQrType}示例内容";
        IsValidInput = true;
        await GenerateQrCodeAsync();
    }

    [RelayCommand]
    private void ClearAll()
    {
        InputText = "";
        DecodedText = "";
        QrCodeImage = null;
        ValidationMessage = "";
        IsValidInput = true;
    }

    [RelayCommand]
    private async Task CopyDecodedTextAsync()
    {
        try
        {
            if (Application.Current?.ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
            {
                var clipboard = desktop.MainWindow?.Clipboard;
                if (clipboard != null && !string.IsNullOrEmpty(DecodedText))
                {
                    await clipboard.SetTextAsync(DecodedText);
                    ValidationMessage = "解码结果已复制到剪贴板！";
                    IsValidInput = true;
                }
            }
        }
        catch (Exception ex)
        {
            ValidationMessage = $"复制失败: {ex.Message}";
            IsValidInput = false;
        }
    }

    private async Task DecodeQrCodeFromStream(Stream stream)
    {
        try
        {
            // 使用SkiaSharp替代System.Drawing进行图像处理
            using var skBitmap = SKBitmap.Decode(stream);
            
            // 将SkiaSharp位图转换为字节数组
            using var image = SKImage.FromBitmap(skBitmap);
            using var data = image.Encode(SKEncodedImageFormat.Png, 100);
            var bytes = data.ToArray();
            
            var luminanceSource = new RGBLuminanceSource(bytes, skBitmap.Width, skBitmap.Height);
            var binaryBitmap = new BinaryBitmap(new HybridBinarizer(luminanceSource));
            var reader = new MultiFormatReader();
            var result = reader.decode(binaryBitmap);
            
            if (result != null)
            {
                DecodedText = result.Text;
                ValidationMessage = "二维码解码成功";
                IsValidInput = true;
            }
            else
            {
                DecodedText = "";
                ValidationMessage = "未能识别二维码";
                IsValidInput = false;
            }
            
            await Task.CompletedTask;
        }
        catch (Exception ex)
        {
            DecodedText = "";
            ValidationMessage = $"解码失败: {ex.Message}";
            IsValidInput = false;
        }
    }

    private QRCodeGenerator.ECCLevel GetErrorCorrectionLevel(string level)
    {
        return level switch
        {
            "L" => QRCodeGenerator.ECCLevel.L,
            "M" => QRCodeGenerator.ECCLevel.M,
            "Q" => QRCodeGenerator.ECCLevel.Q,
            "H" => QRCodeGenerator.ECCLevel.H,
            _ => QRCodeGenerator.ECCLevel.M
        };
    }

    partial void OnSelectedQrTypeChanged(string value)
    {
        // 当二维码类型改变时，可以更新示例文本
    }

    partial void OnQrCodeSizeChanged(int value)
    {
        // 当尺寸改变时，如果已有二维码，重新生成
        if (QrCodeImage != null && !string.IsNullOrEmpty(InputText))
        {
            _ = GenerateQrCodeAsync();
        }
    }

    partial void OnForegroundColorChanged(string value)
    {
        // 当颜色改变时，如果已有二维码，重新生成
        if (QrCodeImage != null && !string.IsNullOrEmpty(InputText))
        {
            _ = GenerateQrCodeAsync();
        }
    }

    partial void OnBackgroundColorChanged(string value)
    {
        // 当颜色改变时，如果已有二维码，重新生成
        if (QrCodeImage != null && !string.IsNullOrEmpty(InputText))
        {
            _ = GenerateQrCodeAsync();
        }
    }
}