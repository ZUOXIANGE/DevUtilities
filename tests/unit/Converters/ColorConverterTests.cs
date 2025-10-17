using System;
using System.Collections.Generic;
using System.Globalization;
using Avalonia.Media;
using DevUtilities.ViewModels;
using FluentAssertions;
using Xunit;

namespace DevUtilities.Tests.Unit.Converters;

public class ColorConverterTests
{
    [Fact]
    public void ColorConverter_WithValidRedAndGreenParameters_ShouldReturnColorBrush()
    {
        // Arrange
        var converter = new ColorConverter();

        // Act
        var result = converter.Convert(255, typeof(IBrush), 128, CultureInfo.InvariantCulture);

        // Assert
        result.Should().BeOfType<SolidColorBrush>();
        var brush = (SolidColorBrush)result!;
        brush.Color.R.Should().Be(255);
        brush.Color.G.Should().Be(128);
        brush.Color.B.Should().Be(0);
    }

    [Fact]
    public void ColorConverter_WithInvalidValue_ShouldReturnTransparent()
    {
        // Arrange
        var converter = new ColorConverter();

        // Act
        var result = converter.Convert("invalid", typeof(IBrush), 128, CultureInfo.InvariantCulture);

        // Assert
        result.Should().BeOfType<SolidColorBrush>();
        var brush = (SolidColorBrush)result!;
        brush.Color.Should().Be(Colors.Transparent);
    }

    [Fact]
    public void ColorConverter_WithInvalidParameter_ShouldReturnTransparent()
    {
        // Arrange
        var converter = new ColorConverter();

        // Act
        var result = converter.Convert(255, typeof(IBrush), "invalid", CultureInfo.InvariantCulture);

        // Assert
        result.Should().BeOfType<SolidColorBrush>();
        var brush = (SolidColorBrush)result!;
        brush.Color.Should().Be(Colors.Transparent);
    }

    [Fact]
    public void ColorConverter_WithNullValue_ShouldReturnTransparent()
    {
        // Arrange
        var converter = new ColorConverter();

        // Act
        var result = converter.Convert(null, typeof(IBrush), 128, CultureInfo.InvariantCulture);

        // Assert
        result.Should().BeOfType<SolidColorBrush>();
        var brush = (SolidColorBrush)result!;
        brush.Color.Should().Be(Colors.Transparent);
    }

    [Fact]
    public void ColorConverter_ConvertBack_ShouldThrowNotImplementedException()
    {
        // Arrange
        var converter = new ColorConverter();

        // Act & Assert
        Assert.Throws<NotImplementedException>(() => 
            converter.ConvertBack(Brushes.Red, typeof(int), null, CultureInfo.InvariantCulture));
    }

    [Fact]
    public void ColorConverter_Instance_ShouldNotBeNull()
    {
        // Assert
        ColorConverter.Instance.Should().NotBeNull();
    }

    [Fact]
    public void RgbToColorConverter_WithValidRgbValues_ShouldReturnColorBrush()
    {
        // Arrange
        var converter = new RgbToColorConverter();
        var values = new object[] { 255, 128, 64 };

        // Act
        var result = converter.Convert(values, typeof(IBrush), null, CultureInfo.InvariantCulture);

        // Assert
        result.Should().BeOfType<SolidColorBrush>();
        var brush = (SolidColorBrush)result!;
        brush.Color.R.Should().Be(255);
        brush.Color.G.Should().Be(128);
        brush.Color.B.Should().Be(64);
    }

    [Fact]
    public void RgbToColorConverter_WithClampedValues_ShouldReturnClampedColorBrush()
    {
        // Arrange
        var converter = new RgbToColorConverter();
        var values = new object[] { 300, -50, 128 }; // Values outside 0-255 range

        // Act
        var result = converter.Convert(values, typeof(IBrush), null, CultureInfo.InvariantCulture);

        // Assert
        result.Should().BeOfType<SolidColorBrush>();
        var brush = (SolidColorBrush)result!;
        brush.Color.R.Should().Be(255); // Clamped from 300
        brush.Color.G.Should().Be(0);   // Clamped from -50
        brush.Color.B.Should().Be(128);
    }

    [Fact]
    public void RgbToColorConverter_WithInsufficientValues_ShouldReturnTransparent()
    {
        // Arrange
        var converter = new RgbToColorConverter();
        var values = new object[] { 255, 128 }; // Only 2 values instead of 3

        // Act
        var result = converter.Convert(values, typeof(IBrush), null, CultureInfo.InvariantCulture);

        // Assert
        result.Should().BeOfType<SolidColorBrush>();
        var brush = (SolidColorBrush)result!;
        brush.Color.Should().Be(Colors.Transparent);
    }

    [Fact]
    public void RgbToColorConverter_WithInvalidValues_ShouldReturnTransparent()
    {
        // Arrange
        var converter = new RgbToColorConverter();
        var values = new object[] { "red", "green", "blue" };

        // Act
        var result = converter.Convert(values, typeof(IBrush), null, CultureInfo.InvariantCulture);

        // Assert
        result.Should().BeOfType<SolidColorBrush>();
        var brush = (SolidColorBrush)result!;
        brush.Color.Should().Be(Colors.Transparent);
    }

    [Fact]
    public void RgbToColorConverter_WithEmptyValues_ShouldReturnTransparent()
    {
        // Arrange
        var converter = new RgbToColorConverter();
        var values = new List<object?>();

        // Act
        var result = converter.Convert(values, typeof(IBrush), null, CultureInfo.InvariantCulture);

        // Assert
        result.Should().BeOfType<SolidColorBrush>();
        var brush = (SolidColorBrush)result!;
        brush.Color.Should().Be(Colors.Transparent);
    }

    [Fact]
    public void RgbToColorConverter_ConvertBack_ShouldThrowNotImplementedException()
    {
        // Arrange
        var converter = new RgbToColorConverter();

        // Act & Assert
        Assert.Throws<NotImplementedException>(() => 
            converter.ConvertBack(Brushes.Red, new[] { typeof(int), typeof(int), typeof(int) }, null, CultureInfo.InvariantCulture));
    }

    [Fact]
    public void RgbToColorConverter_Instance_ShouldNotBeNull()
    {
        // Assert
        RgbToColorConverter.Instance.Should().NotBeNull();
    }
}