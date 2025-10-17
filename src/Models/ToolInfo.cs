namespace DevUtilities.Models;

public class ToolInfo
{
    public string Name { get; set; }
    public string DisplayName { get; set; }
    public string Icon { get; set; }
    public string Description { get; set; }
    public ToolType Type { get; set; }
    public int SortOrder { get; set; } = 0;

    public ToolInfo(string name, string displayName, string icon, string description, ToolType type)
    {
        Name = name;
        DisplayName = displayName;
        Icon = icon;
        Description = description;
        Type = type;
    }
}

public enum ToolType
{
    TimestampConverter,
    UnitConverter,
    BaseConverter,
    JsonFormatter,
    SqlFormatter,
    HtmlFormatter,
    Base64Encoder,
    HexConverter,
    JwtEncoder,
    RegexTester,
    UuidGenerator,
    PasswordGenerator,
    UrlTools,
    HttpRequest,
    IpQuery,
    QrCode,
    ParquetViewer,
    CryptoTools,
    CronExpression
}