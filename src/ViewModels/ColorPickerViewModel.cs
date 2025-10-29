using System;
using System.Collections.ObjectModel;
using System.Linq;
using Avalonia;
using Avalonia.Controls.ApplicationLifetimes;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

namespace DevUtilities.ViewModels;

public partial class ColorPickerViewModel : ObservableObject
{
    [ObservableProperty]
    private int red = 255;

    [ObservableProperty]
    private int green = 0;

    [ObservableProperty]
    private int blue = 0;

    [ObservableProperty]
    private double hue = 0;

    [ObservableProperty]
    private double saturation = 100;

    [ObservableProperty]
    private double lightness = 50;

    [ObservableProperty]
    private double hsvSaturation = 100;

    [ObservableProperty]
    private double value = 100;

    [ObservableProperty]
    private string hexColor = "#FF0000";

    [ObservableProperty]
    private string rgbString = "rgb(255, 0, 0)";

    [ObservableProperty]
    private string hslString = "hsl(0, 100%, 50%)";

    [ObservableProperty]
    private string hsvString = "hsv(0, 100%, 100%)";

    [ObservableProperty]
    private string cmykString = "cmyk(0%, 100%, 100%, 0%)";

    [ObservableProperty]
    private string colorName = "Red";

    [ObservableProperty]
    private string selectedFormat = "HEX";

    [ObservableProperty]
    private string errorMessage = "";

    [ObservableProperty]
    private bool hasError = false;

    [ObservableProperty]
    private ObservableCollection<string> colorFormats = new()
    {
        "HEX", "RGB", "HSL", "HSV", "CMYK"
    };

    [ObservableProperty]
    private ObservableCollection<ColorSwatch> colorHistory = new();
    
    [ObservableProperty]
    private ObservableCollection<ColorSwatch> presetColors = new();

    private bool _isUpdating = false;

    public ColorPickerViewModel()
    {
        InitializePresetColors();
        UpdateAllFormats();
    }

    partial void OnRedChanged(int value)
    {
        if (!_isUpdating)
        {
            Red = Math.Clamp(value, 0, 255);
            UpdateFromRgb();
        }
    }

    partial void OnGreenChanged(int value)
    {
        if (!_isUpdating)
        {
            Green = Math.Clamp(value, 0, 255);
            UpdateFromRgb();
        }
    }

    partial void OnBlueChanged(int value)
    {
        if (!_isUpdating)
        {
            Blue = Math.Clamp(value, 0, 255);
            UpdateFromRgb();
        }
    }

    partial void OnHueChanged(double value)
    {
        if (!_isUpdating)
        {
            Hue = Math.Clamp(value, 0, 360);
            UpdateFromHsl();
        }
    }

    partial void OnSaturationChanged(double value)
    {
        if (!_isUpdating)
        {
            Saturation = Math.Clamp(value, 0, 100);
            UpdateFromHsl();
        }
    }

    partial void OnLightnessChanged(double value)
    {
        if (!_isUpdating)
        {
            Lightness = Math.Clamp(value, 0, 100);
            UpdateFromHsl();
        }
    }

    partial void OnHsvSaturationChanged(double value)
    {
        if (!_isUpdating)
        {
            HsvSaturation = Math.Clamp(value, 0, 100);
            UpdateFromHsv();
        }
    }

    partial void OnValueChanged(double value)
    {
        if (!_isUpdating)
        {
            Value = Math.Clamp(value, 0, 100);
            UpdateFromHsv();
        }
    }

    partial void OnHexColorChanged(string value)
    {
        if (!_isUpdating && !string.IsNullOrEmpty(value))
        {
            UpdateFromHex(value);
        }
    }

    [RelayCommand]
    private void UpdateFromHex(string hex)
    {
        try
        {
            ClearError();
            
            if (string.IsNullOrEmpty(hex))
                return;

            // 清理输入
            hex = hex.Trim();
            if (!hex.StartsWith("#"))
                hex = "#" + hex;

            if (hex.Length != 7 && hex.Length != 4)
            {
                SetError("十六进制颜色格式应为 #RRGGBB 或 #RGB");
                return;
            }

            // 转换短格式 #RGB 到 #RRGGBB
            if (hex.Length == 4)
            {
                hex = $"#{hex[1]}{hex[1]}{hex[2]}{hex[2]}{hex[3]}{hex[3]}";
            }

            var r = Convert.ToInt32(hex.Substring(1, 2), 16);
            var g = Convert.ToInt32(hex.Substring(3, 2), 16);
            var b = Convert.ToInt32(hex.Substring(5, 2), 16);

            _isUpdating = true;
            Red = r;
            Green = g;
            Blue = b;
            _isUpdating = false;

            UpdateAllFormats();
        }
        catch (Exception ex)
        {
            SetError($"无效的十六进制颜色: {ex.Message}");
        }
    }

    [RelayCommand]
    private void RandomColor()
    {
        var random = new Random();
        _isUpdating = true;
        Red = random.Next(0, 256);
        Green = random.Next(0, 256);
        Blue = random.Next(0, 256);
        _isUpdating = false;
        UpdateAllFormats();
    }

    [RelayCommand]
    private void UsePresetColor(ColorSwatch colorSwatch)
    {
        if (colorSwatch != null)
        {
            _isUpdating = true;
            Red = colorSwatch.Red;
            Green = colorSwatch.Green;
            Blue = colorSwatch.Blue;
            _isUpdating = false;
            UpdateAllFormats();
        }
    }

    [RelayCommand]
    private void AddToHistory()
    {
        var newColor = new ColorSwatch
        {
            Name = ColorName,
            Red = Red,
            Green = Green,
            Blue = Blue,
            HexColor = HexColor
        };

        // 避免重复
        var existing = ColorHistory.FirstOrDefault(c => c.HexColor == newColor.HexColor);
        if (existing != null)
        {
            ColorHistory.Remove(existing);
        }

        ColorHistory.Insert(0, newColor);

        // 限制历史记录数量
        while (ColorHistory.Count > 20)
        {
            ColorHistory.RemoveAt(ColorHistory.Count - 1);
        }
    }

    [RelayCommand]
    private void CopyColor(string format)
    {
        try
        {
            string colorValue = format?.ToUpper() switch
            {
                "HEX" => HexColor,
                "RGB" => RgbString,
                "HSL" => HslString,
                "HSV" => HsvString,
                "CMYK" => CmykString,
                _ => HexColor
            };

            CopyToClipboard(colorValue);
        }
        catch (Exception ex)
        {
            SetError($"复制失败: {ex.Message}");
        }
    }

    [RelayCommand]
    private void ClearHistory()
    {
        ColorHistory.Clear();
    }

    [RelayCommand]
    private void Copy(string format)
    {
        string textToCopy = format switch
        {
            "HEX" => HexColor,
            "RGB" => RgbString,
            "HSL" => HslString,
            "HSV" => HsvString,
            "CMYK" => CmykString,
            _ => HexColor
        };
        CopyToClipboard(textToCopy);
    }

    private void UpdateFromRgb()
    {
        _isUpdating = true;
        
        // 转换到HSL
        var (h, s, l) = RgbToHsl(Red, Green, Blue);
        Hue = h;
        Saturation = s;
        Lightness = l;

        // 转换到HSV
        var (hv, sv, v) = RgbToHsv(Red, Green, Blue);
        HsvSaturation = sv;
        Value = v;

        _isUpdating = false;
        UpdateAllFormats();
    }

    private void UpdateFromHsl()
    {
        _isUpdating = true;
        
        var (r, g, b) = HslToRgb(Hue, Saturation, Lightness);
        Red = r;
        Green = g;
        Blue = b;

        // 更新HSV
        var (hv, sv, v) = RgbToHsv(Red, Green, Blue);
        HsvSaturation = sv;
        Value = v;

        _isUpdating = false;
        UpdateAllFormats();
    }

    private void UpdateFromHsv()
    {
        _isUpdating = true;
        
        var (r, g, b) = HsvToRgb(Hue, HsvSaturation, Value);
        Red = r;
        Green = g;
        Blue = b;

        // 更新HSL
        var (h, s, l) = RgbToHsl(Red, Green, Blue);
        Saturation = s;
        Lightness = l;

        _isUpdating = false;
        UpdateAllFormats();
    }

    private void UpdateAllFormats()
    {
        HexColor = $"#{Red:X2}{Green:X2}{Blue:X2}";
        RgbString = $"rgb({Red}, {Green}, {Blue})";
        HslString = $"hsl({Hue:F0}, {Saturation:F0}%, {Lightness:F0}%)";
        HsvString = $"hsv({Hue:F0}, {HsvSaturation:F0}%, {Value:F0}%)";
        
        var (c, m, y, k) = RgbToCmyk(Red, Green, Blue);
        CmykString = $"cmyk({c:F0}%, {m:F0}%, {y:F0}%, {k:F0}%)";

        ColorName = GetColorName(Red, Green, Blue);
    }

    private void InitializePresetColors()
    {
        var presets = new[]
        {
            new { Name = "红色", R = 255, G = 0, B = 0 },
            new { Name = "绿色", R = 0, G = 255, B = 0 },
            new { Name = "蓝色", R = 0, G = 0, B = 255 },
            new { Name = "黄色", R = 255, G = 255, B = 0 },
            new { Name = "青色", R = 0, G = 255, B = 255 },
            new { Name = "洋红", R = 255, G = 0, B = 255 },
            new { Name = "黑色", R = 0, G = 0, B = 0 },
            new { Name = "白色", R = 255, G = 255, B = 255 },
            new { Name = "灰色", R = 128, G = 128, B = 128 },
            new { Name = "橙色", R = 255, G = 165, B = 0 },
            new { Name = "紫色", R = 128, G = 0, B = 128 },
            new { Name = "棕色", R = 165, G = 42, B = 42 },
            new { Name = "粉色", R = 255, G = 192, B = 203 },
            new { Name = "金色", R = 255, G = 215, B = 0 },
            new { Name = "银色", R = 192, G = 192, B = 192 },
            new { Name = "海军蓝", R = 0, G = 0, B = 128 }
        };

        foreach (var preset in presets)
        {
            PresetColors.Add(new ColorSwatch
            {
                Name = preset.Name,
                Red = preset.R,
                Green = preset.G,
                Blue = preset.B,
                HexColor = $"#{preset.R:X2}{preset.G:X2}{preset.B:X2}"
            });
        }
    }

    // 颜色转换算法
    private static (double h, double s, double l) RgbToHsl(int r, int g, int b)
    {
        double rd = r / 255.0;
        double gd = g / 255.0;
        double bd = b / 255.0;

        double max = Math.Max(rd, Math.Max(gd, bd));
        double min = Math.Min(rd, Math.Min(gd, bd));
        double delta = max - min;

        double h = 0, s = 0, l = (max + min) / 2;

        if (delta != 0)
        {
            s = l > 0.5 ? delta / (2 - max - min) : delta / (max + min);

            if (max == rd)
                h = ((gd - bd) / delta + (gd < bd ? 6 : 0)) / 6;
            else if (max == gd)
                h = ((bd - rd) / delta + 2) / 6;
            else
                h = ((rd - gd) / delta + 4) / 6;
        }

        return (h * 360, s * 100, l * 100);
    }

    private static (int r, int g, int b) HslToRgb(double h, double s, double l)
    {
        h /= 360;
        s /= 100;
        l /= 100;

        double r, g, b;

        if (s == 0)
        {
            r = g = b = l;
        }
        else
        {
            double hue2rgb(double p, double q, double t)
            {
                if (t < 0) t += 1;
                if (t > 1) t -= 1;
                if (t < 1.0 / 6) return p + (q - p) * 6 * t;
                if (t < 1.0 / 2) return q;
                if (t < 2.0 / 3) return p + (q - p) * (2.0 / 3 - t) * 6;
                return p;
            }

            double q = l < 0.5 ? l * (1 + s) : l + s - l * s;
            double p = 2 * l - q;
            r = hue2rgb(p, q, h + 1.0 / 3);
            g = hue2rgb(p, q, h);
            b = hue2rgb(p, q, h - 1.0 / 3);
        }

        return ((int)Math.Round(r * 255), (int)Math.Round(g * 255), (int)Math.Round(b * 255));
    }

    private static (double h, double s, double v) RgbToHsv(int r, int g, int b)
    {
        double rd = r / 255.0;
        double gd = g / 255.0;
        double bd = b / 255.0;

        double max = Math.Max(rd, Math.Max(gd, bd));
        double min = Math.Min(rd, Math.Min(gd, bd));
        double delta = max - min;

        double h = 0, s = max == 0 ? 0 : delta / max, v = max;

        if (delta != 0)
        {
            if (max == rd)
                h = ((gd - bd) / delta + (gd < bd ? 6 : 0)) / 6;
            else if (max == gd)
                h = ((bd - rd) / delta + 2) / 6;
            else
                h = ((rd - gd) / delta + 4) / 6;
        }

        return (h * 360, s * 100, v * 100);
    }

    private static (int r, int g, int b) HsvToRgb(double h, double s, double v)
    {
        h /= 360;
        s /= 100;
        v /= 100;

        int i = (int)Math.Floor(h * 6);
        double f = h * 6 - i;
        double p = v * (1 - s);
        double q = v * (1 - f * s);
        double t = v * (1 - (1 - f) * s);

        double r, g, b;
        switch (i % 6)
        {
            case 0: r = v; g = t; b = p; break;
            case 1: r = q; g = v; b = p; break;
            case 2: r = p; g = v; b = t; break;
            case 3: r = p; g = q; b = v; break;
            case 4: r = t; g = p; b = v; break;
            default: r = v; g = p; b = q; break;
        }

        return ((int)Math.Round(r * 255), (int)Math.Round(g * 255), (int)Math.Round(b * 255));
    }

    private static (double c, double m, double y, double k) RgbToCmyk(int r, int g, int b)
    {
        double rd = r / 255.0;
        double gd = g / 255.0;
        double bd = b / 255.0;

        double k = 1 - Math.Max(rd, Math.Max(gd, bd));
        
        if (k == 1)
            return (0, 0, 0, 100);

        double c = (1 - rd - k) / (1 - k);
        double m = (1 - gd - k) / (1 - k);
        double y = (1 - bd - k) / (1 - k);

        return (c * 100, m * 100, y * 100, k * 100);
    }

    private static string GetColorName(int r, int g, int b)
    {
        // 简单的颜色名称识别
        if (r == 255 && g == 0 && b == 0) return "红色";
        if (r == 0 && g == 255 && b == 0) return "绿色";
        if (r == 0 && g == 0 && b == 255) return "蓝色";
        if (r == 255 && g == 255 && b == 0) return "黄色";
        if (r == 0 && g == 255 && b == 255) return "青色";
        if (r == 255 && g == 0 && b == 255) return "洋红";
        if (r == 0 && g == 0 && b == 0) return "黑色";
        if (r == 255 && g == 255 && b == 255) return "白色";
        if (r == g && g == b) return "灰色";
        
        // 根据主要颜色分量判断
        if (r > g && r > b) return "红色系";
        if (g > r && g > b) return "绿色系";
        if (b > r && b > g) return "蓝色系";
        
        return "混合色";
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

public class ColorSwatch
{
    public string Name { get; set; } = "";
    public int Red { get; set; }
    public int Green { get; set; }
    public int Blue { get; set; }
    public string HexColor { get; set; } = "";
}