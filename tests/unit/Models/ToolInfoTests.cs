using DevUtilities.Models;
using FluentAssertions;
using Xunit;

namespace DevUtilities.Tests.Unit.Models;

public class ToolInfoTests
{
    [Fact]
    public void Constructor_WithValidParameters_ShouldSetProperties()
    {
        // Arrange
        var name = "TestTool";
        var displayName = "Test Tool";
        var icon = "TestIcon";
        var description = "Test Description";
        var type = ToolType.Base64Encoder;

        // Act
        var toolInfo = new ToolInfo(name, displayName, icon, description, type);

        // Assert
        toolInfo.Name.Should().Be(name);
        toolInfo.DisplayName.Should().Be(displayName);
        toolInfo.Icon.Should().Be(icon);
        toolInfo.Description.Should().Be(description);
        toolInfo.Type.Should().Be(type);
    }

    [Theory]
    [InlineData("", "", "", "")]
    public void Constructor_WithEmptyStrings_ShouldSetProperties(string name, string displayName, string icon, string description)
    {
        // Arrange
        var type = ToolType.Base64Encoder;

        // Act
        var toolInfo = new ToolInfo(name, displayName, icon, description, type);

        // Assert
        toolInfo.Name.Should().Be(name);
        toolInfo.DisplayName.Should().Be(displayName);
        toolInfo.Icon.Should().Be(icon);
        toolInfo.Description.Should().Be(description);
        toolInfo.Type.Should().Be(type);
    }

    [Fact]
    public void Properties_ShouldBeSettable()
    {
        // Arrange
        var toolInfo = new ToolInfo("Original", "Original Display", "OriginalIcon", "Original Description", ToolType.Base64Encoder);

        // Act
        toolInfo.Name = "Updated";
        toolInfo.DisplayName = "Updated Display";
        toolInfo.Icon = "UpdatedIcon";
        toolInfo.Description = "Updated Description";
        toolInfo.Type = ToolType.QrCode;

        // Assert
        toolInfo.Name.Should().Be("Updated");
        toolInfo.DisplayName.Should().Be("Updated Display");
        toolInfo.Icon.Should().Be("UpdatedIcon");
        toolInfo.Description.Should().Be("Updated Description");
        toolInfo.Type.Should().Be(ToolType.QrCode);
    }

    [Fact]
    public void ToolType_ShouldHaveExpectedValues()
    {
        // Assert - 验证所有枚举值都存在
        var expectedValues = new[]
        {
            ToolType.TimestampConverter,
            ToolType.UnitConverter,
            ToolType.BaseConverter,
            ToolType.JsonFormatter,
            ToolType.SqlFormatter,
            ToolType.HtmlFormatter,
            ToolType.Base64Encoder,
            ToolType.HexConverter,
            ToolType.JwtEncoder,
            ToolType.RegexTester,
            ToolType.UuidGenerator,
            ToolType.PasswordGenerator,
            ToolType.UrlTools,
            ToolType.HttpRequest,
            ToolType.IpQuery,
            ToolType.QrCode,
            ToolType.ParquetViewer,
            ToolType.CryptoTools,
            ToolType.CronExpression,
            ToolType.StringEscape,
            ToolType.TextDiff,
            ToolType.ColorPicker
        };

        foreach (var expectedValue in expectedValues)
        {
            System.Enum.IsDefined(typeof(ToolType), expectedValue).Should().BeTrue($"ToolType should contain {expectedValue}");
        }
    }

    [Fact]
    public void ToolType_ShouldHaveCorrectNumberOfValues()
    {
        // Arrange
        var enumValues = System.Enum.GetValues<ToolType>();

        // Assert
        enumValues.Length.Should().Be(22, "ToolType enum should have exactly 22 values");
    }

    [Theory]
    [InlineData(ToolType.TimestampConverter)]
    [InlineData(ToolType.UnitConverter)]
    [InlineData(ToolType.BaseConverter)]
    [InlineData(ToolType.JsonFormatter)]
    [InlineData(ToolType.SqlFormatter)]
    [InlineData(ToolType.HtmlFormatter)]
    [InlineData(ToolType.Base64Encoder)]
    [InlineData(ToolType.HexConverter)]
    [InlineData(ToolType.JwtEncoder)]
    [InlineData(ToolType.RegexTester)]
    [InlineData(ToolType.UuidGenerator)]
    [InlineData(ToolType.PasswordGenerator)]
    [InlineData(ToolType.UrlTools)]
    [InlineData(ToolType.HttpRequest)]
    [InlineData(ToolType.IpQuery)]
    [InlineData(ToolType.QrCode)]
    [InlineData(ToolType.ParquetViewer)]
    [InlineData(ToolType.CryptoTools)]
    [InlineData(ToolType.CronExpression)]
    [InlineData(ToolType.StringEscape)]
    [InlineData(ToolType.TextDiff)]
    [InlineData(ToolType.ColorPicker)]
    public void ToolType_AllValues_ShouldBeValid(ToolType toolType)
    {
        // Act & Assert
        System.Enum.IsDefined(typeof(ToolType), toolType).Should().BeTrue($"{toolType} should be a valid ToolType value");
    }
}