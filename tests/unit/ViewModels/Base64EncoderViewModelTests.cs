using System;
using System.Text;
using DevUtilities.ViewModels;
using FluentAssertions;
using Xunit;

namespace DevUtilities.Tests.Unit.ViewModels;

public class Base64EncoderViewModelTests
{
    private readonly Base64EncoderViewModel _viewModel;

    public Base64EncoderViewModelTests()
    {
        _viewModel = new Base64EncoderViewModel();
    }

    [Fact]
    public void Constructor_ShouldInitializeWithDefaultValues()
    {
        // Assert
        _viewModel.InputText.Should().BeEmpty();
        _viewModel.OutputText.Should().BeEmpty();
        _viewModel.SelectedEncoding.Should().Be("UTF-8");
        _viewModel.IsEncodeMode.Should().BeTrue();
        _viewModel.AvailableEncodings.Should().BeEquivalentTo(new[] { "UTF-8", "UTF-16", "ASCII", "GBK" });
    }

    [Theory]
    [InlineData("Hello World", "SGVsbG8gV29ybGQ=")]
    [InlineData("测试中文", "5rWL6K+V5Lit5paH")]
    [InlineData("", "")]
    [InlineData("A", "QQ==")]
    public void EncodeCommand_WithValidInput_ShouldEncodeCorrectly(string input, string expected)
    {
        // Arrange
        _viewModel.InputText = input;
        _viewModel.SelectedEncoding = "UTF-8";

        // Act
        _viewModel.EncodeCommand.Execute(null);

        // Assert
        _viewModel.OutputText.Should().Be(expected);
    }

    [Theory]
    [InlineData("SGVsbG8gV29ybGQ=", "Hello World")]
    [InlineData("5rWL6K+V5Lit5paH", "测试中文")]
    [InlineData("", "")]
    [InlineData("QQ==", "A")]
    public void DecodeCommand_WithValidInput_ShouldDecodeCorrectly(string input, string expected)
    {
        // Arrange
        _viewModel.InputText = input;
        _viewModel.SelectedEncoding = "UTF-8";

        // Act
        _viewModel.DecodeCommand.Execute(null);

        // Assert
        _viewModel.OutputText.Should().Be(expected);
    }

    [Fact]
    public void DecodeCommand_WithInvalidBase64_ShouldShowError()
    {
        // Arrange
        _viewModel.InputText = "Invalid Base64!@#";

        // Act
        _viewModel.DecodeCommand.Execute(null);

        // Assert
        _viewModel.OutputText.Should().StartWith("解码错误:");
    }

    [Theory]
    [InlineData("UTF-8")]
    [InlineData("UTF-16")]
    [InlineData("ASCII")]
    public void EncodeCommand_WithDifferentEncodings_ShouldWork(string encoding)
    {
        // Arrange
        _viewModel.InputText = "Test";
        _viewModel.SelectedEncoding = encoding;

        // Act
        _viewModel.EncodeCommand.Execute(null);

        // Assert
        _viewModel.OutputText.Should().NotBeEmpty();
        _viewModel.OutputText.Should().NotStartWith("编码错误:");
    }

    [Fact]
    public void EncodeCommand_WithGBKEncoding_ShouldShowError()
    {
        // Arrange
        _viewModel.InputText = "Test";
        _viewModel.SelectedEncoding = "GBK";

        // Act
        _viewModel.EncodeCommand.Execute(null);

        // Assert
        _viewModel.OutputText.Should().StartWith("编码错误:");
    }

    [Fact]
    public void ClearCommand_ShouldClearAllFields()
    {
        // Arrange
        _viewModel.InputText = "Test Input";
        _viewModel.OutputText = "Test Output";

        // Act
        _viewModel.ClearCommand.Execute(null);

        // Assert
        _viewModel.InputText.Should().BeEmpty();
        _viewModel.OutputText.Should().BeEmpty();
    }

    [Fact]
    public void SwapInputOutputCommand_ShouldSwapInputAndOutput()
    {
        // Arrange - 先设置为编码模式，输入普通文本
        _viewModel.IsEncodeMode = true;
        _viewModel.InputText = "Hello World";
        // 等待编码完成
        var expectedBase64 = _viewModel.OutputText;
        
        // Act
        _viewModel.SwapInputOutputCommand.Execute(null);

        // Assert
        _viewModel.InputText.Should().Be(expectedBase64);
        _viewModel.OutputText.Should().Be("Hello World");
        _viewModel.IsEncodeMode.Should().BeFalse();
    }

    [Fact]
    public void ProcessCommand_ShouldNotBeNull()
    {
        // Assert
        _viewModel.ProcessCommand.Should().NotBeNull();
    }

    [Fact]
    public void PropertyChanged_ShouldBeRaisedWhenPropertiesChange()
    {
        // Arrange
        var propertyChangedRaised = false;
        _viewModel.PropertyChanged += (sender, args) => propertyChangedRaised = true;

        // Act
        _viewModel.InputText = "New Value";

        // Assert
        propertyChangedRaised.Should().BeTrue();
    }
}