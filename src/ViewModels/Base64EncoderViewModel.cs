using System;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Input;
using Avalonia.Media.Imaging;
using Avalonia.Platform.Storage;
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

    [ObservableProperty]
    private bool isImageMode = false;

    [ObservableProperty]
    private bool isImageToBase64Mode = true; // true: 图片转Base64, false: Base64转图片

    [ObservableProperty]
    private string selectedImagePath = "";

    [ObservableProperty]
    private Bitmap? imagePreview;

    [ObservableProperty]
    private string imageInfo = "";

    [ObservableProperty]
    private string base64Input = ""; // 用于Base64转图片的输入

    public string[] AvailableEncodings { get; } = { "UTF-8", "UTF-16", "ASCII", "GBK" };

    [RelayCommand]
    private async Task SelectImage()
    {
        var topLevel = TopLevel.GetTopLevel(App.Current?.ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop
            ? desktop.MainWindow : null);

        if (topLevel != null)
        {
            var files = await topLevel.StorageProvider.OpenFilePickerAsync(new FilePickerOpenOptions
            {
                Title = "选择图片文件",
                AllowMultiple = false,
                FileTypeFilter = new[]
                {
                    new FilePickerFileType("图片文件")
                    {
                        Patterns = new[] { "*.jpg", "*.jpeg", "*.png", "*.bmp", "*.gif", "*.webp" }
                    }
                }
            });

            if (files.Count > 0)
            {
                var file = files[0];
                await LoadImageFromFile(file.Path.LocalPath);
            }
        }
    }

    [RelayCommand]
    private Task ConvertBase64ToImage()
    {
        try
        {
            if (string.IsNullOrWhiteSpace(Base64Input))
            {
                ImagePreview = null;
                ImageInfo = "";
                OutputText = "";
                return Task.CompletedTask;
            }

            // 清理Base64字符串（移除data:image前缀等）
            var base64Data = Base64Input.Trim();
            if (base64Data.StartsWith("data:image"))
            {
                var commaIndex = base64Data.IndexOf(',');
                if (commaIndex >= 0)
                    base64Data = base64Data.Substring(commaIndex + 1);
            }

            // 转换Base64到字节数组
            var imageBytes = Convert.FromBase64String(base64Data);
            
            // 创建图片预览
            using var stream = new MemoryStream(imageBytes);
            ImagePreview = new Bitmap(stream);
            
            // 更新图片信息
            ImageInfo = $"尺寸: {ImagePreview.PixelSize.Width} × {ImagePreview.PixelSize.Height} | 大小: {GetFileSizeString(imageBytes.Length)}";
            
            // 提供保存选项
            OutputText = $"图片解码成功！\n尺寸: {ImagePreview.PixelSize.Width} × {ImagePreview.PixelSize.Height}\n大小: {GetFileSizeString(imageBytes.Length)}\n\n点击保存图片按钮可将图片保存到本地。";
        }
        catch (Exception ex)
        {
            ImagePreview = null;
            ImageInfo = "";
            OutputText = $"Base64解码失败: {ex.Message}";
        }
        
        return Task.CompletedTask;
    }

    [RelayCommand]
    private async Task SaveImageFromBase64()
    {
        try
        {
            if (string.IsNullOrWhiteSpace(Base64Input))
                return;

            var topLevel = TopLevel.GetTopLevel(Application.Current?.ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop 
                ? desktop.MainWindow : null);
            
            if (topLevel == null) return;

            var file = await topLevel.StorageProvider.SaveFilePickerAsync(new FilePickerSaveOptions
            {
                Title = "保存图片",
                FileTypeChoices = new[]
                {
                    new FilePickerFileType("PNG图片") { Patterns = new[] { "*.png" } },
                    new FilePickerFileType("JPEG图片") { Patterns = new[] { "*.jpg", "*.jpeg" } },
                    new FilePickerFileType("所有文件") { Patterns = new[] { "*.*" } }
                },
                SuggestedFileName = "decoded_image.png"
            });

            if (file != null)
            {
                // 清理Base64字符串
                var base64Data = Base64Input.Trim();
                if (base64Data.StartsWith("data:image"))
                {
                    var commaIndex = base64Data.IndexOf(',');
                    if (commaIndex >= 0)
                        base64Data = base64Data.Substring(commaIndex + 1);
                }

                var imageBytes = Convert.FromBase64String(base64Data);
                await using var stream = await file.OpenWriteAsync();
                await stream.WriteAsync(imageBytes);
                
                OutputText = $"图片已保存到: {file.Path.LocalPath}";
            }
        }
        catch (Exception ex)
        {
            OutputText = $"保存图片失败: {ex.Message}";
        }
    }
    
    [RelayCommand]
    private async Task SelectImageForBase64()
    {
        try
        {
            var topLevel = TopLevel.GetTopLevel(Application.Current?.ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop 
                ? desktop.MainWindow : null);
            
            if (topLevel == null) return;

            var files = await topLevel.StorageProvider.OpenFilePickerAsync(new FilePickerOpenOptions
            {
                Title = "选择图片文件",
                AllowMultiple = false,
                FileTypeFilter = new[]
                {
                    new FilePickerFileType("图片文件")
                    {
                        Patterns = new[] { "*.jpg", "*.jpeg", "*.png", "*.gif", "*.bmp", "*.webp", "*.ico" }
                    }
                }
            });

            if (files.Count > 0)
            {
                var file = files[0];
                SelectedImagePath = file.Path.LocalPath;
                await LoadImagePreview();
                await ConvertImageToBase64();
            }
        }
        catch (Exception ex)
        {
            OutputText = $"选择图片错误: {ex.Message}";
        }
    }

    [RelayCommand]
    private void ClearImage()
    {
        SelectedImagePath = "";
        ImagePreview = null;
        ImageInfo = "";
        if (IsImageMode)
        {
            InputText = "";
            OutputText = "";
        }
    }

    private Task LoadImagePreview()
    {
        try
        {
            if (string.IsNullOrEmpty(SelectedImagePath) || !File.Exists(SelectedImagePath))
                return Task.CompletedTask;

            using var stream = File.OpenRead(SelectedImagePath);
            ImagePreview = new Bitmap(stream);
            
            var fileInfo = new FileInfo(SelectedImagePath);
            ImageInfo = $"文件: {Path.GetFileName(SelectedImagePath)}\n" +
                       $"大小: {fileInfo.Length / 1024.0:F1} KB\n" +
                       $"尺寸: {ImagePreview.PixelSize.Width} × {ImagePreview.PixelSize.Height}";
        }
        catch (Exception ex)
        {
            ImageInfo = $"加载图片失败: {ex.Message}";
        }
        
        return Task.CompletedTask;
    }

    private async Task ConvertImageToBase64()
    {
        try
        {
            if (string.IsNullOrEmpty(SelectedImagePath) || !File.Exists(SelectedImagePath))
                return;

            var bytes = await File.ReadAllBytesAsync(SelectedImagePath);
            var base64 = Convert.ToBase64String(bytes);
            
            // 添加数据URI前缀
            var extension = Path.GetExtension(SelectedImagePath).ToLower();
            var mimeType = extension switch
            {
                ".jpg" or ".jpeg" => "image/jpeg",
                ".png" => "image/png",
                ".gif" => "image/gif",
                ".bmp" => "image/bmp",
                ".webp" => "image/webp",
                ".ico" => "image/x-icon",
                _ => "image/png"
            };

            InputText = $"data:{mimeType};base64,{base64}";
            OutputText = base64;
        }
        catch (Exception ex)
        {
            OutputText = $"转换图片错误: {ex.Message}";
        }
    }

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

    [RelayCommand]
    private void ClearInput()
    {
        InputText = "";
    }

    [RelayCommand]
    private void ClearOutput()
    {
        OutputText = "";
    }

    [RelayCommand]
    private async Task CopyToClipboard(string? text = null)
    {
        try
        {
            var textToCopy = text ?? OutputText;
            if (string.IsNullOrEmpty(textToCopy))
                return;

            var topLevel = TopLevel.GetTopLevel(Application.Current?.ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop 
                ? desktop.MainWindow : null);
            
            if (topLevel?.Clipboard != null)
            {
                await topLevel.Clipboard.SetTextAsync(textToCopy);
            }
        }
        catch (Exception)
        {
            // Handle clipboard error silently or show notification
        }
    }

    [RelayCommand]
    private void ClearAll()
    {
        if (IsImageMode)
        {
            if (IsImageToBase64Mode)
            {
                // 清空图片转Base64模式
                SelectedImagePath = string.Empty;
                ImagePreview = null;
                ImageInfo = string.Empty;
                OutputText = string.Empty;
            }
            else
            {
                // 清空Base64转图片模式
                Base64Input = string.Empty;
                ImagePreview = null;
                ImageInfo = string.Empty;
                OutputText = string.Empty;
            }
        }
        else
        {
            // 清空文本模式
            InputText = string.Empty;
            OutputText = string.Empty;
        }
    }

    [RelayCommand]
    public async Task HandleDrop(DragEventArgs e)
    {
        try
        {
            if (!IsImageMode || !IsImageToBase64Mode)
                return;

            var files = e.DataTransfer.TryGetFiles();
            if (files == null) return;

            foreach (var file in files)
            {
                var extension = Path.GetExtension(file.Name).ToLower();
                if (extension == ".png" || extension == ".jpg" || extension == ".jpeg" || 
                    extension == ".gif" || extension == ".bmp" || extension == ".webp")
                {
                    await LoadImageFromFile(file.Path.LocalPath);
                    break; // 只处理第一个图片文件
                }
            }
        }
        catch (Exception ex)
        {
            OutputText = $"拖拽处理失败: {ex.Message}";
        }
    }

    private async Task LoadImageFromFile(string filePath)
    {
        try
        {
            SelectedImagePath = filePath;
            await LoadImagePreview();
            await ConvertImageToBase64();
        }
        catch (Exception ex)
        {
            OutputText = $"加载图片失败: {ex.Message}";
        }
    }

    private string GetFileSizeString(long bytes)
    {
        string[] sizes = { "B", "KB", "MB", "GB" };
        double len = bytes;
        int order = 0;
        while (len >= 1024 && order < sizes.Length - 1)
        {
            order++;
            len = len / 1024;
        }
        return $"{len:0.##} {sizes[order]}";
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
        if (!IsImageMode)
            Process();
    }

    partial void OnSelectedEncodingChanged(string value)
    {
        if (!IsImageMode)
            Process();
    }

    partial void OnIsEncodeModeChanged(bool value)
    {
        if (!IsImageMode)
            Process();
    }

    partial void OnIsImageModeChanged(bool value)
    {
        if (value)
        {
            // 切换到图片模式时清空文本内容
            InputText = "";
            OutputText = "";
        }
        else
        {
            // 切换到文本模式时清空图片内容
            ClearImage();
            Process();
        }
    }

    partial void OnIsImageToBase64ModeChanged(bool value)
    {
        // 切换图片模式时清空相关内容
        ClearImage();
        Base64Input = "";
        OutputText = "";
        ImagePreview = null;
        ImageInfo = "";
    }

    partial void OnBase64InputChanged(string value)
    {
        if (IsImageMode && !IsImageToBase64Mode)
        {
            _ = ConvertBase64ToImage();
        }
    }
}
