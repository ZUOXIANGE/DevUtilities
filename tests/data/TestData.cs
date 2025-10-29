using DevUtilities.Models;

namespace DevUtilities.Tests.Data;

public static class TestData
{
    public static class Base64
    {
        public const string PlainText = "Hello, World!";
        public const string EncodedText = "SGVsbG8sIFdvcmxkIQ==";
        public const string EmptyString = "";
        public const string UnicodeText = "你好，世界！";
        public const string UnicodeEncoded = "5L2g5aW977yM5LiW55WM77yB";
        public const string InvalidBase64 = "Invalid@Base64!";
    }

    public static class Crypto
    {
        public const string PlainText = "Hello, World!";
        public const string Md5Hash = "65a8e27d8879283831b664bd8b7f0ad4";
        public const string Sha1Hash = "0a0a9f2a6772942557ab5355d76af442f8f65e01";
        public const string Sha256Hash = "dffd6021bb2bd5b0af676290809ec3a53191dd81c7f70a4b28688a362182986f";
        public const string EmptyStringMd5 = "d41d8cd98f00b204e9800998ecf8427e";
        public const string EmptyStringSha1 = "da39a3ee5e6b4b0d3255bfef95601890afd80709";
        public const string EmptyStringSha256 = "e3b0c44298fc1c149afbf4c8996fb92427ae41e4649b934ca495991b7852b855";
    }

    public static class QrCode
    {
        public const string SimpleText = "Hello QR";
        public const string UrlText = "https://www.example.com";
        public const string EmailText = "mailto:test@example.com";
        public const string PhoneText = "tel:+1234567890";
        public const string WifiText = "WIFI:T:WPA;S:MyNetwork;P:MyPassword;;";
        public const string LongText = "This is a very long text that should still be encodable in a QR code but might require a larger size or different error correction level to fit properly.";
        public const string UnicodeText = "你好世界 🌍";
        public const string EmptyText = "";
    }

    public static class Regex
    {
        public const string EmailPattern = @"^[a-zA-Z0-9._%+-]+@[a-zA-Z0-9.-]+\.[a-zA-Z]{2,}$";
        public const string PhonePattern = @"^\+?[\d\s\-\(\)]+$";
        public const string UrlPattern = @"^https?://[^\s/$.?#].[^\s]*$";
        public const string ValidEmail = "test@example.com";
        public const string InvalidEmail = "invalid-email";
        public const string ValidPhone = "+1 (555) 123-4567";
        public const string InvalidPhone = "not-a-phone";
        public const string ValidUrl = "https://www.example.com";
        public const string InvalidUrl = "not-a-url";
    }

    public static class Json
    {
        public const string ValidJson = @"{""name"":""John"",""age"":30,""city"":""New York""}";
        public const string FormattedJson = @"{
  ""name"": ""John"",
  ""age"": 30,
  ""city"": ""New York""
}";
        public const string InvalidJson = @"{""name"":""John"",""age"":30,""city"":""New York""";
        public const string EmptyJson = "{}";
        public const string ArrayJson = @"[1,2,3,4,5]";
        public const string FormattedArrayJson = @"[
  1,
  2,
  3,
  4,
  5
]";
    }

    public static class Timestamps
    {
        public const long UnixTimestamp = 1640995200; // 2022-01-01 00:00:00 UTC
        public const string IsoDateTime = "2022-01-01T00:00:00.000Z";
        public const string LocalDateTime = "2022-01-01 00:00:00";
        public const long InvalidTimestamp = -1;
        public const long FutureTimestamp = 4102444800; // 2100-01-01 00:00:00 UTC
    }

    public static class Units
    {
        public const double Meters = 1000;
        public const double Kilometers = 1;
        public const double Feet = 3280.84;
        public const double Miles = 0.621371;
        public const double Celsius = 0;
        public const double Fahrenheit = 32;
        public const double Kelvin = 273.15;
    }

    public static class Passwords
    {
        public const string WeakPassword = "123456";
        public const string MediumPassword = "Password123";
        public const string StrongPassword = "P@ssw0rd!2023#Secure";
        public const int MinLength = 8;
        public const int MaxLength = 128;
        public const int DefaultLength = 16;
    }

    public static ToolInfo CreateSampleToolInfo(ToolType type = ToolType.Base64Encoder)
    {
        return new ToolInfo(
            name: "sample-tool",
            displayName: "Sample Tool",
            icon: "sample-icon",
            description: "This is a sample tool for testing",
            type: type
        );
    }

    public static class Urls
    {
        public const string ValidUrl = "https://www.example.com/path?param=value#fragment";
        public const string EncodedUrl = "https%3A//www.example.com/path%3Fparam%3Dvalue%23fragment";
        public const string InvalidUrl = "not-a-valid-url";
        public const string HttpUrl = "http://example.com";
        public const string HttpsUrl = "https://example.com";
        public const string FtpUrl = "ftp://files.example.com";
    }

    public static class Hex
    {
        public const string PlainText = "Hello";
        public const string HexString = "48656c6c6f";
        public const string UpperHexString = "48656C6C6F";
        public const string InvalidHex = "Hello World";
        public const string EmptyString = "";
        public const string EmptyHex = "";
    }
}
