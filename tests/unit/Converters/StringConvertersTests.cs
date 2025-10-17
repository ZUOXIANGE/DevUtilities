using System;
using System.Threading.Tasks;
using DevUtilities.Converters;
using FluentAssertions;
using Xunit;

namespace DevUtilities.Tests.Unit.Converters;

public class StringConvertersTests
{
    [Fact]
    public void StringIsNotNullOrEmptyConverter_WithNonEmptyString_ShouldReturnTrue()
    {
        // Arrange
        var converter = StringConverters.IsNotNullOrEmpty;
        var input = "Hello World";

        // Act
        var result = converter.Convert(input, typeof(bool), null!, null!);

        // Assert
        result.Should().Be(true);
    }

    [Fact]
    public void StringIsNotNullOrEmptyConverter_WithEmptyString_ShouldReturnFalse()
    {
        // Arrange
        var converter = StringConverters.IsNotNullOrEmpty;
        var input = "";

        // Act
        var result = converter.Convert(input, typeof(bool), null!, null!);

        // Assert
        result.Should().Be(false);
    }

    [Fact]
    public void StringIsNotNullOrEmptyConverter_WithNull_ShouldReturnFalse()
    {
        // Arrange
        var converter = StringConverters.IsNotNullOrEmpty;

        // Act
        var result = converter.Convert(null!, typeof(bool), null!, null!);

        // Assert
        result.Should().Be(false);
    }

    [Fact]
    public void StringIsNotNullOrEmptyConverter_WithWhitespace_ShouldReturnTrue()
    {
        // Arrange
        var converter = StringConverters.IsNotNullOrEmpty;
        var input = "   ";

        // Act
        var result = converter.Convert(input, typeof(bool), null!, null!);

        // Assert
        result.Should().Be(true);
    }

    [Fact]
    public void StringIsNotNullOrEmptyConverter_WithNonStringInput_ShouldReturnTrue()
    {
        // Arrange
        var converter = StringConverters.IsNotNullOrEmpty;
        var input = 123;

        // Act
        var result = converter.Convert(input, typeof(bool), null!, null!);

        // Assert
        result.Should().Be(true);
    }

    [Fact]
    public void StringIsNotNullOrEmptyConverter_ConvertBack_ShouldThrowNotImplementedException()
    {
        // Arrange
        var converter = StringConverters.IsNotNullOrEmpty;

        // Act & Assert
        Assert.Throws<NotImplementedException>(() => converter.ConvertBack(true, typeof(string), null!, null!));
    }

    [Fact]
    public void StringIsNotNullOrEmptyConverter_Instance_ShouldNotBeNull()
    {
        // Assert
        StringConverters.IsNotNullOrEmpty.Should().NotBeNull();
    }
}