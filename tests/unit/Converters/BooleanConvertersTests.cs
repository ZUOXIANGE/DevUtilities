using System;
using System.Globalization;
using Avalonia.Media;
using DevUtilities.Converters;
using FluentAssertions;
using Xunit;

namespace DevUtilities.Tests.Unit.Converters;

public class BooleanConvertersTests
{
    [Fact]
    public void BooleanToColorConverter_WithTrue_ShouldReturnLightGreen()
    {
        // Arrange
        var converter = new BooleanToColorConverter();

        // Act
        var result = converter.Convert(true, typeof(IBrush), null, CultureInfo.InvariantCulture);

        // Assert
        result.Should().Be(Brushes.LightGreen);
    }

    [Fact]
    public void BooleanToColorConverter_WithFalse_ShouldReturnLightCoral()
    {
        // Arrange
        var converter = new BooleanToColorConverter();

        // Act
        var result = converter.Convert(false, typeof(IBrush), null, CultureInfo.InvariantCulture);

        // Assert
        result.Should().Be(Brushes.LightCoral);
    }

    [Fact]
    public void BooleanToColorConverter_WithNull_ShouldReturnTransparent()
    {
        // Arrange
        var converter = new BooleanToColorConverter();

        // Act
        var result = converter.Convert(null, typeof(IBrush), null, CultureInfo.InvariantCulture);

        // Assert
        result.Should().Be(Brushes.Transparent);
    }

    [Fact]
    public void BooleanToColorConverter_WithNonBooleanValue_ShouldReturnTransparent()
    {
        // Arrange
        var converter = new BooleanToColorConverter();

        // Act
        var result = converter.Convert("not a boolean", typeof(IBrush), null, CultureInfo.InvariantCulture);

        // Assert
        result.Should().Be(Brushes.Transparent);
    }

    [Fact]
    public void BooleanToStringConverter_WithTrueAndParameter_ShouldReturnFirstOption()
    {
        // Arrange
        var converter = new BooleanToStringConverter();

        // Act
        var result = converter.Convert(true, typeof(string), "Yes|No", CultureInfo.InvariantCulture);

        // Assert
        result.Should().Be("Yes");
    }

    [Fact]
    public void BooleanToStringConverter_WithFalseAndParameter_ShouldReturnSecondOption()
    {
        // Arrange
        var converter = new BooleanToStringConverter();

        // Act
        var result = converter.Convert(false, typeof(string), "Yes|No", CultureInfo.InvariantCulture);

        // Assert
        result.Should().Be("No");
    }

    [Fact]
    public void BooleanToStringConverter_WithoutParameter_ShouldReturnToString()
    {
        // Arrange
        var converter = new BooleanToStringConverter();

        // Act
        var result = converter.Convert(true, typeof(string), null, CultureInfo.InvariantCulture);

        // Assert
        result.Should().Be("True");
    }

    [Fact]
    public void BooleanToStringConverter_WithInvalidParameter_ShouldReturnToString()
    {
        // Arrange
        var converter = new BooleanToStringConverter();

        // Act
        var result = converter.Convert(true, typeof(string), "InvalidParameter", CultureInfo.InvariantCulture);

        // Assert
        result.Should().Be("True");
    }

    [Fact]
    public void BooleanToColorConverter_ConvertBack_ShouldThrowNotImplementedException()
    {
        // Arrange
        var converter = new BooleanToColorConverter();

        // Act & Assert
        Assert.Throws<NotImplementedException>(() => 
            converter.ConvertBack(Brushes.LightGreen, typeof(bool), null, CultureInfo.InvariantCulture));
    }

    [Fact]
    public void BooleanToStringConverter_ConvertBack_ShouldThrowNotImplementedException()
    {
        // Arrange
        var converter = new BooleanToStringConverter();

        // Act & Assert
        Assert.Throws<NotImplementedException>(() => 
            converter.ConvertBack("Yes", typeof(bool), null, CultureInfo.InvariantCulture));
    }

    [Fact]
    public void BooleanToColorConverter_Instance_ShouldNotBeNull()
    {
        // Assert
        BooleanToColorConverter.Instance.Should().NotBeNull();
    }

    [Fact]
    public void BooleanToStringConverter_Instance_ShouldNotBeNull()
    {
        // Assert
        BooleanToStringConverter.Instance.Should().NotBeNull();
    }
}