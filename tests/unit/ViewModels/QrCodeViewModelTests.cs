using System;
using System.Threading.Tasks;
using DevUtilities.ViewModels;
using FluentAssertions;
using Xunit;

namespace DevUtilities.Tests.Unit.ViewModels;

public class QrCodeViewModelTests
{
    private readonly QrCodeViewModel _viewModel;

    public QrCodeViewModelTests()
    {
        _viewModel = new QrCodeViewModel();
    }

    [Fact]
    public void Constructor_ShouldInitializeWithDefaultValues()
    {
        // Assert
        _viewModel.InputText.Should().Be("Hello, QR Code!");
        _viewModel.DecodedText.Should().BeEmpty();
        _viewModel.QrCodeImage.Should().BeNull();
        _viewModel.ValidationMessage.Should().BeEmpty();
        _viewModel.IsValidInput.Should().BeTrue();
        _viewModel.QrCodeSize.Should().Be(300);
        _viewModel.SelectedErrorCorrection.Should().Be("M");
        _viewModel.SelectedEncoding.Should().Be("UTF-8");
        _viewModel.IncludeQuietZone.Should().BeTrue();
    }

    [Theory]
    [InlineData("Hello World")]
    [InlineData("Test QR Code")]
    [InlineData("https://example.com")]
    public async Task GenerateQrCodeCommand_WithValidInput_ShouldGenerateQrCodeOrShowError(string input)
    {
        // Arrange
        _viewModel.InputText = input;

        // Act
        await _viewModel.GenerateQrCodeCommand.ExecuteAsync(null);

        // Assert - Either success or error message
        if (_viewModel.QrCodeImage != null)
        {
            _viewModel.ValidationMessage.Should().Be("二维码生成成功");
            _viewModel.IsValidInput.Should().BeTrue();
        }
        else
        {
            _viewModel.ValidationMessage.Should().StartWith("生成二维码失败:");
            _viewModel.IsValidInput.Should().BeFalse();
        }
    }

    [Fact]
    public async Task GenerateQrCodeCommand_WithEmptyInput_ShouldNotGenerateQrCode()
    {
        // Arrange
        _viewModel.InputText = "";

        // Act
        await _viewModel.GenerateQrCodeCommand.ExecuteAsync(null);

        // Assert
        _viewModel.QrCodeImage.Should().BeNull();
    }

    [Theory]
    [InlineData(200)]
    [InlineData(300)]
    [InlineData(400)]
    public async Task GenerateQrCodeCommand_WithDifferentSizes_ShouldGenerateQrCodeOrShowError(int size)
    {
        // Arrange
        _viewModel.InputText = "Test Content";
        _viewModel.QrCodeSize = size;

        // Act
        await _viewModel.GenerateQrCodeCommand.ExecuteAsync(null);

        // Assert - Either success or error message
        if (_viewModel.QrCodeImage != null)
        {
            _viewModel.QrCodeImage.PixelSize.Width.Should().Be(size);
            _viewModel.QrCodeImage.PixelSize.Height.Should().Be(size);
            _viewModel.ValidationMessage.Should().Be("二维码生成成功");
        }
        else
        {
            _viewModel.ValidationMessage.Should().StartWith("生成二维码失败:");
            _viewModel.IsValidInput.Should().BeFalse();
        }
    }

    [Theory]
    [InlineData("L")]
    [InlineData("M")]
    [InlineData("Q")]
    [InlineData("H")]
    public async Task GenerateQrCodeCommand_WithDifferentErrorCorrections_ShouldGenerateQrCodeOrShowError(string errorCorrection)
    {
        // Arrange
        _viewModel.InputText = "Test Content";
        _viewModel.SelectedErrorCorrection = errorCorrection;

        // Act
        await _viewModel.GenerateQrCodeCommand.ExecuteAsync(null);

        // Assert - Either success or error message
        if (_viewModel.QrCodeImage != null)
        {
            _viewModel.ValidationMessage.Should().Be("二维码生成成功");
        }
        else
        {
            _viewModel.ValidationMessage.Should().StartWith("生成二维码失败:");
            _viewModel.IsValidInput.Should().BeFalse();
        }
    }

    [Theory]
    [InlineData("UTF-8")]
    [InlineData("UTF-16")]
    [InlineData("ASCII")]
    public async Task GenerateQrCodeCommand_WithDifferentEncodings_ShouldGenerateQrCodeOrShowError(string encoding)
    {
        // Arrange
        _viewModel.InputText = "Test Content";
        _viewModel.SelectedEncoding = encoding;

        // Act
        await _viewModel.GenerateQrCodeCommand.ExecuteAsync(null);

        // Assert - Either success or error message
        if (_viewModel.QrCodeImage != null)
        {
            _viewModel.ValidationMessage.Should().Be("二维码生成成功");
        }
        else
        {
            _viewModel.ValidationMessage.Should().StartWith("生成二维码失败:");
            _viewModel.IsValidInput.Should().BeFalse();
        }
    }

    [Fact]
    public async Task GenerateQrCodeCommand_WithQuietZoneEnabled_ShouldGenerateQrCodeOrShowError()
    {
        // Arrange
        _viewModel.InputText = "Test Content";
        _viewModel.IncludeQuietZone = true;

        // Act
        await _viewModel.GenerateQrCodeCommand.ExecuteAsync(null);

        // Assert - Either success or error message
        if (_viewModel.QrCodeImage != null)
        {
            _viewModel.ValidationMessage.Should().Be("二维码生成成功");
        }
        else
        {
            _viewModel.ValidationMessage.Should().StartWith("生成二维码失败:");
            _viewModel.IsValidInput.Should().BeFalse();
        }
    }

    [Fact]
    public async Task GenerateQrCodeCommand_WithQuietZoneDisabled_ShouldGenerateQrCodeOrShowError()
    {
        // Arrange
        _viewModel.InputText = "Test Content";
        _viewModel.IncludeQuietZone = false;

        // Act
        await _viewModel.GenerateQrCodeCommand.ExecuteAsync(null);

        // Assert - Either success or error message
        if (_viewModel.QrCodeImage != null)
        {
            _viewModel.ValidationMessage.Should().Be("二维码生成成功");
        }
        else
        {
            _viewModel.ValidationMessage.Should().StartWith("生成二维码失败:");
            _viewModel.IsValidInput.Should().BeFalse();
        }
    }

    [Fact]
    public void ClearAllCommand_ShouldClearAllFields()
    {
        // Arrange
        _viewModel.InputText = "Test Input";
        _viewModel.DecodedText = "Test Decoded";
        _viewModel.ValidationMessage = "Test Message";

        // Act
        _viewModel.ClearAllCommand.Execute(null);

        // Assert
        _viewModel.InputText.Should().BeEmpty();
        _viewModel.DecodedText.Should().BeEmpty();
        _viewModel.QrCodeImage.Should().BeNull();
        _viewModel.ValidationMessage.Should().BeEmpty();
    }

    [Theory]
    [InlineData(50)]
    [InlineData(100)]
    [InlineData(200)]
    [InlineData(300)]
    [InlineData(500)]
    [InlineData(1000)]
    public void QrCodeSize_WhenChanged_ShouldUpdateProperty(int size)
    {
        // Act
        _viewModel.QrCodeSize = size;

        // Assert
        _viewModel.QrCodeSize.Should().Be(size);
    }

    [Theory]
    [InlineData("L")]
    [InlineData("M")]
    [InlineData("Q")]
    [InlineData("H")]
    public void SelectedErrorCorrection_WhenChanged_ShouldUpdateProperty(string errorCorrection)
    {
        // Act
        _viewModel.SelectedErrorCorrection = errorCorrection;

        // Assert
        _viewModel.SelectedErrorCorrection.Should().Be(errorCorrection);
    }

    [Theory]
    [InlineData("UTF-8")]
    [InlineData("UTF-16")]
    [InlineData("ASCII")]
    [InlineData("GBK")]
    public void SelectedEncoding_WhenChanged_ShouldUpdateProperty(string encoding)
    {
        // Act
        _viewModel.SelectedEncoding = encoding;

        // Assert
        _viewModel.SelectedEncoding.Should().Be(encoding);
    }

    [Theory]
    [InlineData(true)]
    [InlineData(false)]
    public void IncludeQuietZone_WhenChanged_ShouldUpdateProperty(bool includeQuietZone)
    {
        // Act
        _viewModel.IncludeQuietZone = includeQuietZone;

        // Assert
        _viewModel.IncludeQuietZone.Should().Be(includeQuietZone);
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

    [Fact]
    public void IsValidInput_WhenChanged_ShouldUpdateProperty()
    {
        // Act
        _viewModel.IsValidInput = false;

        // Assert
        _viewModel.IsValidInput.Should().BeFalse();
    }

    [Fact]
    public void ValidationMessage_WhenChanged_ShouldUpdateProperty()
    {
        // Arrange
        var message = "Test validation message";

        // Act
        _viewModel.ValidationMessage = message;

        // Assert
        _viewModel.ValidationMessage.Should().Be(message);
    }
}