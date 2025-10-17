using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.ApplicationLifetimes;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.IdentityModel.Tokens;

namespace DevUtilities.ViewModels;

public partial class JwtEncoderViewModel : ObservableObject
{
    [ObservableProperty]
    private string headerInput = """
    {
      "alg": "HS256",
      "typ": "JWT"
    }
    """;

    [ObservableProperty]
    private string payloadInput = """
    {
      "sub": "1234567890",
      "name": "John Doe",
      "iat": 1516239022,
      "exp": 1735689600
    }
    """;

    [ObservableProperty]
    private string secretKey = "your-256-bit-secret";

    [ObservableProperty]
    private string jwtToken = "";

    [ObservableProperty]
    private string decodedHeader = "";

    [ObservableProperty]
    private string decodedPayload = "";

    [ObservableProperty]
    private string validationMessage = "";

    [ObservableProperty]
    private bool isValidJwt = false;

    [ObservableProperty]
    private string selectedAlgorithm = "HS256";

    [ObservableProperty]
    private bool includeTimestamps = true;

    public List<string> Algorithms { get; } = new()
    {
        "HS256", "HS384", "HS512"
    };

    public JwtEncoderViewModel()
    {
        UpdateTimestamps();
    }

    [RelayCommand]
    private void EncodeJwt()
    {
        try
        {
            ValidationMessage = "";
            IsValidJwt = false;

            // 验证输入
            if (string.IsNullOrWhiteSpace(HeaderInput) || string.IsNullOrWhiteSpace(PayloadInput))
            {
                ValidationMessage = "Header和Payload不能为空";
                return;
            }

            if (string.IsNullOrWhiteSpace(SecretKey))
            {
                ValidationMessage = "密钥不能为空";
                return;
            }

            // 解析Header和Payload
            var header = JsonSerializer.Deserialize<Dictionary<string, object>>(HeaderInput);
            var payload = JsonSerializer.Deserialize<Dictionary<string, object>>(PayloadInput);

            if (header == null || payload == null)
            {
                ValidationMessage = "Header或Payload格式无效";
                return;
            }

            // 创建JWT
            var tokenHandler = new JwtSecurityTokenHandler();
            var key = Encoding.UTF8.GetBytes(SecretKey);

            // 创建Claims
            var claims = new List<Claim>();
            foreach (var kvp in payload)
            {
                if (kvp.Value != null)
                {
                    claims.Add(new Claim(kvp.Key, kvp.Value.ToString()!));
                }
            }

            // 选择算法
            string algorithm = SelectedAlgorithm switch
            {
                "HS384" => SecurityAlgorithms.HmacSha384,
                "HS512" => SecurityAlgorithms.HmacSha512,
                _ => SecurityAlgorithms.HmacSha256
            };

            var tokenDescriptor = new SecurityTokenDescriptor
            {
                Subject = new ClaimsIdentity(claims),
                SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(key), algorithm)
            };

            // 处理过期时间
            if (payload.ContainsKey("exp") && payload["exp"] is JsonElement expElement && expElement.TryGetInt64(out var exp))
            {
                tokenDescriptor.Expires = DateTimeOffset.FromUnixTimeSeconds(exp).DateTime;
            }

            // 处理签发时间
            if (payload.ContainsKey("iat") && payload["iat"] is JsonElement iatElement && iatElement.TryGetInt64(out var iat))
            {
                tokenDescriptor.IssuedAt = DateTimeOffset.FromUnixTimeSeconds(iat).DateTime;
            }

            var token = tokenHandler.CreateToken(tokenDescriptor);
            JwtToken = tokenHandler.WriteToken(token);

            IsValidJwt = true;
            ValidationMessage = "JWT编码成功";
        }
        catch (Exception ex)
        {
            ValidationMessage = $"编码失败: {ex.Message}";
            IsValidJwt = false;
        }
    }

    [RelayCommand]
    private void DecodeJwt()
    {
        try
        {
            ValidationMessage = "";
            IsValidJwt = false;

            if (string.IsNullOrWhiteSpace(JwtToken))
            {
                ValidationMessage = "JWT令牌不能为空";
                return;
            }

            var tokenHandler = new JwtSecurityTokenHandler();
            
            // 验证JWT格式
            if (!tokenHandler.CanReadToken(JwtToken))
            {
                ValidationMessage = "无效的JWT格式";
                return;
            }

            var jsonToken = tokenHandler.ReadJwtToken(JwtToken);

            // 解码Header
            var headerJson = JsonSerializer.Serialize(jsonToken.Header, new JsonSerializerOptions 
            { 
                WriteIndented = true 
            });
            DecodedHeader = headerJson;

            // 解码Payload
            var payloadDict = new Dictionary<string, object>();
            foreach (var claim in jsonToken.Claims)
            {
                if (payloadDict.ContainsKey(claim.Type))
                {
                    // 如果已存在，转换为数组
                    if (payloadDict[claim.Type] is not List<string> list)
                    {
                        list = new List<string> { payloadDict[claim.Type].ToString()! };
                        payloadDict[claim.Type] = list;
                    }
                    list.Add(claim.Value);
                }
                else
                {
                    // 尝试解析为数字或保持为字符串
                    if (long.TryParse(claim.Value, out var longValue))
                    {
                        payloadDict[claim.Type] = longValue;
                    }
                    else if (bool.TryParse(claim.Value, out var boolValue))
                    {
                        payloadDict[claim.Type] = boolValue;
                    }
                    else
                    {
                        payloadDict[claim.Type] = claim.Value;
                    }
                }
            }

            var payloadJson = JsonSerializer.Serialize(payloadDict, new JsonSerializerOptions 
            { 
                WriteIndented = true 
            });
            DecodedPayload = payloadJson;

            IsValidJwt = true;
            ValidationMessage = "JWT解码成功";
        }
        catch (Exception ex)
        {
            ValidationMessage = $"解码失败: {ex.Message}";
            IsValidJwt = false;
        }
    }

    [RelayCommand]
    private void VerifyJwt()
    {
        try
        {
            ValidationMessage = "";
            IsValidJwt = false;

            if (string.IsNullOrWhiteSpace(JwtToken))
            {
                ValidationMessage = "JWT令牌不能为空";
                return;
            }

            if (string.IsNullOrWhiteSpace(SecretKey))
            {
                ValidationMessage = "密钥不能为空";
                return;
            }

            var tokenHandler = new JwtSecurityTokenHandler();
            var key = Encoding.UTF8.GetBytes(SecretKey);

            var validationParameters = new TokenValidationParameters
            {
                ValidateIssuerSigningKey = true,
                IssuerSigningKey = new SymmetricSecurityKey(key),
                ValidateIssuer = false,
                ValidateAudience = false,
                ValidateLifetime = true,
                ClockSkew = TimeSpan.Zero
            };

            var principal = tokenHandler.ValidateToken(JwtToken, validationParameters, out var validatedToken);

            IsValidJwt = true;
            ValidationMessage = "JWT验证成功，签名有效";
        }
        catch (SecurityTokenExpiredException)
        {
            ValidationMessage = "JWT已过期";
            IsValidJwt = false;
        }
        catch (SecurityTokenInvalidSignatureException)
        {
            ValidationMessage = "JWT签名无效";
            IsValidJwt = false;
        }
        catch (Exception ex)
        {
            ValidationMessage = $"验证失败: {ex.Message}";
            IsValidJwt = false;
        }
    }

    [RelayCommand]
    private void UpdateTimestamps()
    {
        if (!IncludeTimestamps) return;

        try
        {
            var now = DateTimeOffset.UtcNow;
            var iat = now.ToUnixTimeSeconds();
            var exp = now.AddHours(1).ToUnixTimeSeconds(); // 1小时后过期

            var payload = JsonSerializer.Deserialize<Dictionary<string, object>>(PayloadInput);
            if (payload != null)
            {
                payload["iat"] = iat;
                payload["exp"] = exp;

                PayloadInput = JsonSerializer.Serialize(payload, new JsonSerializerOptions 
                { 
                    WriteIndented = true 
                });
            }
        }
        catch
        {
            // 忽略错误，保持原有内容
        }
    }

    [RelayCommand]
    private void UseExample()
    {
        HeaderInput = """
        {
          "alg": "HS256",
          "typ": "JWT"
        }
        """;

        var now = DateTimeOffset.UtcNow;
        var iat = now.ToUnixTimeSeconds();
        var exp = now.AddHours(1).ToUnixTimeSeconds();

        PayloadInput = $$"""
        {
          "sub": "1234567890",
          "name": "John Doe",
          "admin": true,
          "iat": {{iat}},
          "exp": {{exp}}
        }
        """;

        SecretKey = "your-256-bit-secret";
        ValidationMessage = "已加载示例数据";
    }

    [RelayCommand]
    private void Clear()
    {
        HeaderInput = "";
        PayloadInput = "";
        SecretKey = "";
        JwtToken = "";
        DecodedHeader = "";
        DecodedPayload = "";
        ValidationMessage = "";
        IsValidJwt = false;
    }

    [RelayCommand]
    private async Task CopyToken()
    {
        if (!string.IsNullOrWhiteSpace(JwtToken))
        {
            if (App.Current?.ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
            {
                var clipboard = desktop.MainWindow?.Clipboard;
                if (clipboard != null)
                {
                    await clipboard.SetTextAsync(JwtToken);
                    ValidationMessage = "JWT令牌已复制到剪贴板";
                }
            }
        }
    }
}