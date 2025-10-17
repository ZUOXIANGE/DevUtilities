using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Globalization;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

namespace DevUtilities.ViewModels;

public partial class UnitConverterViewModel : ObservableObject
{
    [ObservableProperty]
    private string selectedCategory = "长度";

    [ObservableProperty]
    private string selectedFromUnit = "米";

    [ObservableProperty]
    private string selectedToUnit = "厘米";

    [ObservableProperty]
    private string inputValue = "1";

    [ObservableProperty]
    private string outputValue = "100";

    [ObservableProperty]
    private string conversionFormula = "1 米 = 100 厘米";

    public ObservableCollection<string> Categories { get; } = new()
    {
        "长度", "重量", "温度", "面积", "体积", "时间", "速度", "压力"
    };

    public ObservableCollection<string> FromUnits { get; } = new();
    public ObservableCollection<string> ToUnits { get; } = new();

    private readonly Dictionary<string, Dictionary<string, double>> _conversionFactors = new()
    {
        ["长度"] = new()
        {
            ["毫米"] = 0.001,
            ["厘米"] = 0.01,
            ["分米"] = 0.1,
            ["米"] = 1.0,
            ["千米"] = 1000.0,
            ["英寸"] = 0.0254,
            ["英尺"] = 0.3048,
            ["码"] = 0.9144,
            ["英里"] = 1609.344
        },
        ["重量"] = new()
        {
            ["毫克"] = 0.000001,
            ["克"] = 0.001,
            ["千克"] = 1.0,
            ["吨"] = 1000.0,
            ["盎司"] = 0.0283495,
            ["磅"] = 0.453592,
            ["英石"] = 6.35029
        },
        ["温度"] = new()
        {
            ["摄氏度"] = 1.0,
            ["华氏度"] = 1.0,
            ["开尔文"] = 1.0
        },
        ["面积"] = new()
        {
            ["平方毫米"] = 0.000001,
            ["平方厘米"] = 0.0001,
            ["平方分米"] = 0.01,
            ["平方米"] = 1.0,
            ["平方千米"] = 1000000.0,
            ["公顷"] = 10000.0,
            ["亩"] = 666.667,
            ["平方英寸"] = 0.00064516,
            ["平方英尺"] = 0.092903,
            ["平方码"] = 0.836127
        },
        ["体积"] = new()
        {
            ["毫升"] = 0.001,
            ["升"] = 1.0,
            ["立方米"] = 1000.0,
            ["立方厘米"] = 0.001,
            ["加仑"] = 3.78541,
            ["品脱"] = 0.473176,
            ["夸脱"] = 0.946353,
            ["液体盎司"] = 0.0295735
        },
        ["时间"] = new()
        {
            ["毫秒"] = 0.001,
            ["秒"] = 1.0,
            ["分钟"] = 60.0,
            ["小时"] = 3600.0,
            ["天"] = 86400.0,
            ["周"] = 604800.0,
            ["月"] = 2629746.0,
            ["年"] = 31556952.0
        },
        ["速度"] = new()
        {
            ["米/秒"] = 1.0,
            ["千米/小时"] = 0.277778,
            ["英里/小时"] = 0.44704,
            ["节"] = 0.514444,
            ["马赫"] = 343.0
        },
        ["压力"] = new()
        {
            ["帕斯卡"] = 1.0,
            ["千帕"] = 1000.0,
            ["兆帕"] = 1000000.0,
            ["巴"] = 100000.0,
            ["大气压"] = 101325.0,
            ["毫米汞柱"] = 133.322,
            ["磅/平方英寸"] = 6894.76
        }
    };

    public UnitConverterViewModel()
    {
        UpdateUnits();
        Convert();
    }

    partial void OnSelectedCategoryChanged(string value)
    {
        UpdateUnits();
        Convert();
    }

    partial void OnSelectedFromUnitChanged(string value)
    {
        Convert();
    }

    partial void OnSelectedToUnitChanged(string value)
    {
        Convert();
    }

    partial void OnInputValueChanged(string value)
    {
        Convert();
    }

    private void UpdateUnits()
    {
        FromUnits.Clear();
        ToUnits.Clear();

        if (_conversionFactors.ContainsKey(SelectedCategory))
        {
            var units = _conversionFactors[SelectedCategory].Keys;
            foreach (var unit in units)
            {
                FromUnits.Add(unit);
                ToUnits.Add(unit);
            }

            if (FromUnits.Count > 0)
            {
                SelectedFromUnit = FromUnits[0];
                SelectedToUnit = FromUnits.Count > 1 ? FromUnits[1] : FromUnits[0];
            }
        }
    }

    [RelayCommand]
    private void Convert()
    {
        try
        {
            if (string.IsNullOrWhiteSpace(InputValue) || 
                !double.TryParse(InputValue, NumberStyles.Float, CultureInfo.InvariantCulture, out double input))
            {
                OutputValue = "0";
                ConversionFormula = $"请输入有效数字";
                return;
            }

            if (!_conversionFactors.ContainsKey(SelectedCategory))
            {
                OutputValue = "0";
                ConversionFormula = "不支持的单位类型";
                return;
            }

            var factors = _conversionFactors[SelectedCategory];
            if (!factors.ContainsKey(SelectedFromUnit) || !factors.ContainsKey(SelectedToUnit))
            {
                OutputValue = "0";
                ConversionFormula = "不支持的单位";
                return;
            }

            double result;
            if (SelectedCategory == "温度")
            {
                result = ConvertTemperature(input, SelectedFromUnit, SelectedToUnit);
            }
            else
            {
                // 先转换为基准单位，再转换为目标单位
                double baseValue = input * factors[SelectedFromUnit];
                result = baseValue / factors[SelectedToUnit];
            }

            OutputValue = result.ToString("G15", CultureInfo.InvariantCulture);
            ConversionFormula = $"{InputValue} {SelectedFromUnit} = {OutputValue} {SelectedToUnit}";
        }
        catch (Exception ex)
        {
            OutputValue = "错误";
            ConversionFormula = $"转换失败: {ex.Message}";
        }
    }

    private double ConvertTemperature(double value, string fromUnit, string toUnit)
    {
        // 先转换为摄氏度
        double celsius = fromUnit switch
        {
            "摄氏度" => value,
            "华氏度" => (value - 32) * 5 / 9,
            "开尔文" => value - 273.15,
            _ => value
        };

        // 再从摄氏度转换为目标单位
        return toUnit switch
        {
            "摄氏度" => celsius,
            "华氏度" => celsius * 9 / 5 + 32,
            "开尔文" => celsius + 273.15,
            _ => celsius
        };
    }

    [RelayCommand]
    private void SwapUnits()
    {
        (SelectedFromUnit, SelectedToUnit) = (SelectedToUnit, SelectedFromUnit);
        (InputValue, OutputValue) = (OutputValue, InputValue);
        Convert();
    }

    [RelayCommand]
    private void Clear()
    {
        InputValue = "0";
        Convert();
    }

    [RelayCommand]
    private void CopyResult()
    {
        try
        {
            // 这里可以实现复制到剪贴板的功能
            // 由于Avalonia的剪贴板API需要在UI线程中调用，这里先留空
        }
        catch
        {
            // 忽略复制错误
        }
    }
}