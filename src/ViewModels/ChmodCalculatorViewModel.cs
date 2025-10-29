using System;
using System.Diagnostics;
using System.Text.RegularExpressions;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using DevUtilities.Models;

namespace DevUtilities.ViewModels;

public partial class ChmodCalculatorViewModel : DevUtilities.Core.ViewModels.Base.BaseToolViewModel
{
    [ObservableProperty] private bool ownerRead = true;
    [ObservableProperty] private bool ownerWrite = true;
    [ObservableProperty] private bool ownerExecute = true;

    [ObservableProperty] private bool groupRead = true;
    [ObservableProperty] private bool groupWrite = false;
    [ObservableProperty] private bool groupExecute = true;

    [ObservableProperty] private bool othersRead = true;
    [ObservableProperty] private bool othersWrite = false;
    [ObservableProperty] private bool othersExecute = true;

    [ObservableProperty] private bool setUserId = false; // u+s
    [ObservableProperty] private bool setGroupId = false; // g+s
    [ObservableProperty] private bool stickyBit = false;  // o+t

    [ObservableProperty] private string inputPermission = string.Empty;

    [ObservableProperty] private string octalPermission = string.Empty;
    [ObservableProperty] private string symbolicPermission = string.Empty;
    [ObservableProperty] private string numericChmodCommand = string.Empty;
    [ObservableProperty] private string symbolicChmodCommand = string.Empty;

    public IRelayCommand ApplyInputCommand { get; }
    public IRelayCommand UseExampleCommand { get; }

    public ChmodCalculatorViewModel()
    {
        Debug.WriteLine("[ChmodCalculatorViewModel] 开始初始化ViewModel");
        
        Title = "chmod计算器";
        Description = "计算并转换Linux文件权限，支持八进制与符号表示、特殊位";
        Icon = "🛡️";
        ToolType = ToolType.ChmodCalculator;

        ApplyInputCommand = new RelayCommand(ApplyInputPermission);
        UseExampleCommand = new RelayCommand(UseExample);

        PropertyChanged += OnAnyPropertyChanged;

        // 默认0755
        Debug.WriteLine("[ChmodCalculatorViewModel] 设置默认权限0755");
        UpdateResults();
        
        Debug.WriteLine("[ChmodCalculatorViewModel] ViewModel初始化完成");
    }

    partial void OnOwnerReadChanged(bool value) => UpdateResults();
    partial void OnOwnerWriteChanged(bool value) => UpdateResults();
    partial void OnOwnerExecuteChanged(bool value) => UpdateResults();
    partial void OnGroupReadChanged(bool value) => UpdateResults();
    partial void OnGroupWriteChanged(bool value) => UpdateResults();
    partial void OnGroupExecuteChanged(bool value) => UpdateResults();
    partial void OnOthersReadChanged(bool value) => UpdateResults();
    partial void OnOthersWriteChanged(bool value) => UpdateResults();
    partial void OnOthersExecuteChanged(bool value) => UpdateResults();
    partial void OnSetUserIdChanged(bool value) => UpdateResults();
    partial void OnSetGroupIdChanged(bool value) => UpdateResults();
    partial void OnStickyBitChanged(bool value) => UpdateResults();

    private void OnAnyPropertyChanged(object? sender, System.ComponentModel.PropertyChangedEventArgs e)
    {
        if (e.PropertyName == nameof(InputPermission))
        {
            // 不自动解析，手动触发
        }
    }

    protected override void OnResetTool()
    {
        Debug.WriteLine("[ChmodCalculatorViewModel] 重置工具到默认状态");
        
        OwnerRead = OwnerWrite = OwnerExecute = true;
        GroupRead = true; GroupWrite = false; GroupExecute = true;
        OthersRead = true; OthersWrite = false; OthersExecute = true;
        SetUserId = false; SetGroupId = false; StickyBit = false;
        InputPermission = string.Empty;
        UpdateResults();
        
        Debug.WriteLine("[ChmodCalculatorViewModel] 工具重置完成");
    }

    private void UseExample()
    {
        Debug.WriteLine("[ChmodCalculatorViewModel] 使用示例权限0755");
        
        // 示例：0755
        OwnerRead = OwnerWrite = OwnerExecute = true;
        GroupRead = true; GroupWrite = false; GroupExecute = true;
        OthersRead = true; OthersWrite = false; OthersExecute = true;
        SetUserId = false; SetGroupId = false; StickyBit = false;
        UpdateResults();
        SetSuccess("已加载0755示例");
        
        Debug.WriteLine("[ChmodCalculatorViewModel] 示例权限设置完成");
    }

    private void UpdateResults()
    {
        Debug.WriteLine("[ChmodCalculatorViewModel] 开始更新权限计算结果");
        
        var u = ToDigit(OwnerRead, OwnerWrite, OwnerExecute);
        var g = ToDigit(GroupRead, GroupWrite, GroupExecute);
        var o = ToDigit(OthersRead, OthersWrite, OthersExecute);
        var special = (SetUserId ? 4 : 0) + (SetGroupId ? 2 : 0) + (StickyBit ? 1 : 0);

        OctalPermission = special > 0 ? $"{special}{u}{g}{o}" : $"{u}{g}{o}";
        Debug.WriteLine($"[ChmodCalculatorViewModel] 计算得到八进制权限: {OctalPermission}");

        // 基础rwx字符串
        var chars = new char[9];
        chars[0] = OwnerRead ? 'r' : '-';
        chars[1] = OwnerWrite ? 'w' : '-';
        chars[2] = OwnerExecute ? 'x' : '-';
        chars[3] = GroupRead ? 'r' : '-';
        chars[4] = GroupWrite ? 'w' : '-';
        chars[5] = GroupExecute ? 'x' : '-';
        chars[6] = OthersRead ? 'r' : '-';
        chars[7] = OthersWrite ? 'w' : '-';
        chars[8] = OthersExecute ? 'x' : '-';

        // 特殊位替换
        if (SetUserId)
        {
            chars[2] = OwnerExecute ? 's' : 'S';
        }
        if (SetGroupId)
        {
            chars[5] = GroupExecute ? 's' : 'S';
        }
        if (StickyBit)
        {
            chars[8] = OthersExecute ? 't' : 'T';
        }
        SymbolicPermission = new string(chars);
        Debug.WriteLine($"[ChmodCalculatorViewModel] 计算得到符号权限: {SymbolicPermission}");

        NumericChmodCommand = $"chmod {OctalPermission}";
        var uSym = (OwnerRead ? "r" : "") + (OwnerWrite ? "w" : "") + (OwnerExecute ? (SetUserId ? "s" : "x") : (SetUserId ? "S" : ""));
        var gSym = (GroupRead ? "r" : "") + (GroupWrite ? "w" : "") + (GroupExecute ? (SetGroupId ? "s" : "x") : (SetGroupId ? "S" : ""));
        var oSym = (OthersRead ? "r" : "") + (OthersWrite ? "w" : "") + (OthersExecute ? (StickyBit ? "t" : "x") : (StickyBit ? "T" : ""));
        var specials = (SetUserId ? ",u+s" : "") + (SetGroupId ? ",g+s" : "") + (StickyBit ? ",o+t" : "");
        var baseSym = $"u={uSym},g={gSym},o={oSym}";
        SymbolicChmodCommand = specials.Length > 0 ? $"chmod {baseSym}{specials}" : $"chmod {baseSym}";
        
        Debug.WriteLine($"[ChmodCalculatorViewModel] 生成数字命令: {NumericChmodCommand}");
        Debug.WriteLine($"[ChmodCalculatorViewModel] 生成符号命令: {SymbolicChmodCommand}");
        Debug.WriteLine("[ChmodCalculatorViewModel] 权限计算结果更新完成");
    }

    private static int ToDigit(bool r, bool w, bool x) => (r ? 4 : 0) + (w ? 2 : 0) + (x ? 1 : 0);

    private void ApplyInputPermission()
    {
        var text = (InputPermission ?? string.Empty).Trim();
        Debug.WriteLine($"[ChmodCalculatorViewModel] 开始解析输入权限: '{text}'");
        
        if (string.IsNullOrEmpty(text)) 
        {
            Debug.WriteLine("[ChmodCalculatorViewModel] 输入为空，跳过解析");
            return;
        }

        try
        {
            if (Regex.IsMatch(text, "^[0-7]{3,4}$"))
            {
                Debug.WriteLine("[ChmodCalculatorViewModel] 识别为八进制格式");
                // 八进制解析
                ParseOctal(text);
                SetSuccess("已解析八进制权限");
                return;
            }

            // 支持形如 rwxr-xr-x 或者 u=rwx,g=rx,o=rx
            if (Regex.IsMatch(text, "^[rwxstST-]{9}$"))
            {
                Debug.WriteLine("[ChmodCalculatorViewModel] 识别为9位符号格式");
                ParseSymbolicTriplet(text);
                SetSuccess("已解析符号权限");
                return;
            }

            if (Regex.IsMatch(text, "^u=[rwxstST-]*,g=[rwxstST-]*,o=[rwxstST-]*$"))
            {
                Debug.WriteLine("[ChmodCalculatorViewModel] 识别为u=,g=,o=符号格式");
                var parts = text.Split(',');
                var uPart = parts[0].Split('=')[1];
                var gPart = parts[1].Split('=')[1];
                var oPart = parts[2].Split('=')[1];
                ParseSymbolicParts(uPart, gPart, oPart);
                SetSuccess("已解析符号权限");
                return;
            }

            Debug.WriteLine($"[ChmodCalculatorViewModel] 无法识别权限格式: {text}");
            SetError("无法识别的权限格式，请输入如 0755 或 rwxr-xr-x 或 u=rwx,g=rx,o=rx");
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"[ChmodCalculatorViewModel] 解析权限时发生异常: {ex.Message}");
            Debug.WriteLine($"[ChmodCalculatorViewModel] 异常堆栈: {ex.StackTrace}");
            SetError($"解析失败: {ex.Message}");
        }
    }

    private void ParseOctal(string octal)
    {
        Debug.WriteLine($"[ChmodCalculatorViewModel] 开始解析八进制权限: {octal}");
        
        // 处理前导特殊位
        var digits = octal.ToCharArray();
        int index = 0;
        int special = 0;
        if (digits.Length == 4)
        {
            special = digits[0] - '0';
            index = 1;
            Debug.WriteLine($"[ChmodCalculatorViewModel] 检测到特殊位: {special}");
        }
        var u = digits[index] - '0';
        var g = digits[index + 1] - '0';
        var o = digits[index + 2] - '0';
        
        Debug.WriteLine($"[ChmodCalculatorViewModel] 解析权限位 - 用户:{u}, 组:{g}, 其他:{o}");

        SetUserId = (special & 4) != 0;
        SetGroupId = (special & 2) != 0;
        StickyBit = (special & 1) != 0;

        OwnerRead = (u & 4) != 0; OwnerWrite = (u & 2) != 0; OwnerExecute = (u & 1) != 0;
        GroupRead = (g & 4) != 0; GroupWrite = (g & 2) != 0; GroupExecute = (g & 1) != 0;
        OthersRead = (o & 4) != 0; OthersWrite = (o & 2) != 0; OthersExecute = (o & 1) != 0;

        UpdateResults();
        Debug.WriteLine("[ChmodCalculatorViewModel] 八进制权限解析完成");
    }

    private void ParseSymbolicTriplet(string sym)
    {
        Debug.WriteLine($"[ChmodCalculatorViewModel] 开始解析9位符号权限: {sym}");
        
        if (sym.Length != 9) throw new ArgumentException("符号权限长度必须为9");
        
        OwnerRead = sym[0] != '-';
        OwnerWrite = sym[1] != '-';
        OwnerExecute = sym[2] == 'x' || sym[2] == 's';
        GroupRead = sym[3] != '-';
        GroupWrite = sym[4] != '-';
        GroupExecute = sym[5] == 'x' || sym[5] == 's';
        OthersRead = sym[6] != '-';
        OthersWrite = sym[7] != '-';
        OthersExecute = sym[8] == 'x' || sym[8] == 't';

        SetUserId = sym[2] == 's' || sym[2] == 'S';
        SetGroupId = sym[5] == 's' || sym[5] == 'S';
        StickyBit = sym[8] == 't' || sym[8] == 'T';

        UpdateResults();
        Debug.WriteLine("[ChmodCalculatorViewModel] 9位符号权限解析完成");
    }

    private void ParseSymbolicParts(string u, string g, string o)
    {
        Debug.WriteLine($"[ChmodCalculatorViewModel] 开始解析分段符号权限 - u:{u}, g:{g}, o:{o}");
        
        OwnerRead = u.Contains('r'); OwnerWrite = u.Contains('w'); OwnerExecute = u.Contains('x') || u.Contains('s');
        GroupRead = g.Contains('r'); GroupWrite = g.Contains('w'); GroupExecute = g.Contains('x') || g.Contains('s');
        OthersRead = o.Contains('r'); OthersWrite = o.Contains('w'); OthersExecute = o.Contains('x') || o.Contains('t');

        SetUserId = u.Contains('s') || u.Contains('S');
        SetGroupId = g.Contains('s') || g.Contains('S');
        StickyBit = o.Contains('t') || o.Contains('T');

        UpdateResults();
        Debug.WriteLine("[ChmodCalculatorViewModel] 分段符号权限解析完成");
    }
}