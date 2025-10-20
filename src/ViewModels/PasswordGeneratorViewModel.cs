using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using Avalonia;
using Avalonia.Controls.ApplicationLifetimes;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

namespace DevUtilities.ViewModels;

public partial class PasswordGeneratorViewModel : ObservableObject
{
    [ObservableProperty]
    private string generatedPassword = "";

    [ObservableProperty]
    private int passwordLength = 12;

    [ObservableProperty]
    private bool includeUppercase = true;

    [ObservableProperty]
    private bool includeLowercase = true;

    [ObservableProperty]
    private bool includeNumbers = true;

    [ObservableProperty]
    private bool includeSymbols = true;

    [ObservableProperty]
    private bool excludeSimilar = false;

    [ObservableProperty]
    private bool excludeAmbiguous = false;

    [ObservableProperty]
    private string customCharacters = "";

    [ObservableProperty]
    private bool useCustomCharacters = false;

    [ObservableProperty]
    private string passwordStrength = "";

    [ObservableProperty]
    private string strengthColor = "#666666";

    [ObservableProperty]
    private int strengthScore = 0;

    [ObservableProperty]
    private int batchCount = 10;

    public ObservableCollection<string> GeneratedPasswords { get; } = new();
    public ObservableCollection<string> PasswordHistory { get; } = new();

    private const string UppercaseChars = "ABCDEFGHIJKLMNOPQRSTUVWXYZ";
    private const string LowercaseChars = "abcdefghijklmnopqrstuvwxyz";
    private const string NumberChars = "0123456789";
    private const string SymbolChars = "!@#$%^&*()_+-=[]{}|;:,.<>?";
    private const string SimilarChars = "il1Lo0O";
    private const string AmbiguousChars = "{}[]()/\\'\"`~,;.<>";

    public PasswordGeneratorViewModel()
    {
        GeneratePassword();
    }

    [RelayCommand]
    private void GeneratePassword()
    {
        try
        {
            var charset = BuildCharset();
            if (string.IsNullOrEmpty(charset))
            {
                GeneratedPassword = "请至少选择一种字符类型";
                return;
            }

            var password = GenerateRandomPassword(charset, PasswordLength);
            GeneratedPassword = password;
            
            AnalyzePasswordStrength(password);
            
            // 添加到历史记录
            if (!string.IsNullOrEmpty(password) && !PasswordHistory.Contains(password))
            {
                PasswordHistory.Insert(0, password);
                if (PasswordHistory.Count > 50)
                {
                    PasswordHistory.RemoveAt(PasswordHistory.Count - 1);
                }
            }
        }
        catch (Exception ex)
        {
            GeneratedPassword = $"生成错误: {ex.Message}";
        }
    }

    [RelayCommand]
    private void GenerateBatch()
    {
        try
        {
            var charset = BuildCharset();
            if (string.IsNullOrEmpty(charset))
            {
                return;
            }

            GeneratedPasswords.Clear();
            for (int i = 0; i < BatchCount; i++)
            {
                var password = GenerateRandomPassword(charset, PasswordLength);
                GeneratedPasswords.Add(password);
            }
        }
        catch (Exception ex)
        {
            GeneratedPasswords.Clear();
            GeneratedPasswords.Add($"批量生成错误: {ex.Message}");
        }
    }

    [RelayCommand]
    private void UseHistoryPassword(string password)
    {
        if (!string.IsNullOrEmpty(password))
        {
            GeneratedPassword = password;
            AnalyzePasswordStrength(password);
        }
    }

    [RelayCommand]
    private void ClearHistory()
    {
        PasswordHistory.Clear();
    }

    [RelayCommand]
    private async Task CopyPassword()
    {
        try
        {
            if (Application.Current?.ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
            {
                var clipboard = desktop.MainWindow?.Clipboard;
                if (clipboard != null && !string.IsNullOrEmpty(GeneratedPassword))
                {
                    await clipboard.SetTextAsync(GeneratedPassword);
                }
            }
        }
        catch (Exception)
        {
            // 静默处理剪贴板错误
        }
    }

    [RelayCommand]
    private async Task CopyBatchPassword(string password)
    {
        try
        {
            if (Application.Current?.ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
            {
                var clipboard = desktop.MainWindow?.Clipboard;
                if (clipboard != null && !string.IsNullOrEmpty(password))
                {
                    await clipboard.SetTextAsync(password);
                }
            }
        }
        catch (Exception)
        {
            // 静默处理剪贴板错误
        }
    }

    private string BuildCharset()
    {
        var charset = new StringBuilder();

        if (UseCustomCharacters && !string.IsNullOrEmpty(CustomCharacters))
        {
            charset.Append(CustomCharacters);
        }
        else
        {
            if (IncludeUppercase) charset.Append(UppercaseChars);
            if (IncludeLowercase) charset.Append(LowercaseChars);
            if (IncludeNumbers) charset.Append(NumberChars);
            if (IncludeSymbols) charset.Append(SymbolChars);
        }

        var result = charset.ToString();

        // 排除相似字符
        if (ExcludeSimilar)
        {
            result = new string(result.Where(c => !SimilarChars.Contains(c)).ToArray());
        }

        // 排除模糊字符
        if (ExcludeAmbiguous)
        {
            result = new string(result.Where(c => !AmbiguousChars.Contains(c)).ToArray());
        }

        return result;
    }

    private string GenerateRandomPassword(string charset, int length)
    {
        using var rng = RandomNumberGenerator.Create();
        var password = new StringBuilder(length);
        var buffer = new byte[4];

        for (int i = 0; i < length; i++)
        {
            rng.GetBytes(buffer);
            var randomIndex = Math.Abs(BitConverter.ToInt32(buffer, 0)) % charset.Length;
            password.Append(charset[randomIndex]);
        }

        return password.ToString();
    }

    private void AnalyzePasswordStrength(string password)
    {
        if (string.IsNullOrEmpty(password))
        {
            PasswordStrength = "";
            StrengthScore = 0;
            StrengthColor = "#666666";
            return;
        }

        int score = 0;
        var feedback = new StringBuilder();

        // 长度评分
        if (password.Length >= 12) score += 25;
        else if (password.Length >= 8) score += 15;
        else if (password.Length >= 6) score += 10;
        else score += 5;

        // 字符类型评分
        bool hasUpper = password.Any(char.IsUpper);
        bool hasLower = password.Any(char.IsLower);
        bool hasDigit = password.Any(char.IsDigit);
        bool hasSymbol = password.Any(c => !char.IsLetterOrDigit(c));

        if (hasUpper) score += 15;
        if (hasLower) score += 15;
        if (hasDigit) score += 15;
        if (hasSymbol) score += 20;

        // 复杂度评分
        var uniqueChars = password.Distinct().Count();
        if (uniqueChars == password.Length) score += 10;
        else if (uniqueChars >= password.Length * 0.8) score += 5;

        StrengthScore = Math.Min(score, 100);

        // 强度描述
        if (score >= 80)
        {
            PasswordStrength = "非常强";
            StrengthColor = "#4CAF50";
        }
        else if (score >= 60)
        {
            PasswordStrength = "强";
            StrengthColor = "#8BC34A";
        }
        else if (score >= 40)
        {
            PasswordStrength = "中等";
            StrengthColor = "#FF9800";
        }
        else if (score >= 20)
        {
            PasswordStrength = "弱";
            StrengthColor = "#FF5722";
        }
        else
        {
            PasswordStrength = "非常弱";
            StrengthColor = "#F44336";
        }
    }

    partial void OnPasswordLengthChanged(int value)
    {
        if (value < 1) PasswordLength = 1;
        if (value > 128) PasswordLength = 128;
        GeneratePassword();
    }

    partial void OnIncludeUppercaseChanged(bool value) => GeneratePassword();
    partial void OnIncludeLowercaseChanged(bool value) => GeneratePassword();
    partial void OnIncludeNumbersChanged(bool value) => GeneratePassword();
    partial void OnIncludeSymbolsChanged(bool value) => GeneratePassword();
    partial void OnExcludeSimilarChanged(bool value) => GeneratePassword();
    partial void OnExcludeAmbiguousChanged(bool value) => GeneratePassword();
    partial void OnUseCustomCharactersChanged(bool value) => GeneratePassword();
    partial void OnCustomCharactersChanged(string value) => GeneratePassword();
}