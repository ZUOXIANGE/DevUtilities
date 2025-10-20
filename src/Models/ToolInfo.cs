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
    Base64Encoder,
    UrlTools,
    JsonFormatter,
    PasswordGenerator,
    BaseConverter,
    HttpRequest,
    CryptoTools,
    StringEscape,
    SqlFormatter,
    HtmlFormatter,
    RegexTester,
    TextDiff,
    QrCode,
    UuidGenerator,
    ColorPicker,
    HexConverter,
    JwtEncoder,
    UnitConverter,
    CronExpression,
    ParquetViewer,
    IpQuery
}