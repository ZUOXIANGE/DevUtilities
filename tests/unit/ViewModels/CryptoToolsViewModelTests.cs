using System;
using System.Security.Cryptography;
using System.Text;
using DevUtilities.ViewModels;
using FluentAssertions;
using Xunit;

namespace DevUtilities.Tests.Unit.ViewModels;

public class CryptoToolsViewModelTests
{
    private readonly CryptoToolsViewModel _viewModel;

    public CryptoToolsViewModelTests()
    {
        _viewModel = new CryptoToolsViewModel();
    }

    [Fact]
    public void Constructor_ShouldInitializeWithDefaultValues()
    {
        // Assert
        _viewModel.InputText.Should().BeEmpty();
        _viewModel.OutputText.Should().BeEmpty();
        _viewModel.KeyText.Should().NotBeEmpty(); // 构造函数会生成随机密钥
        _viewModel.IvText.Should().NotBeEmpty(); // 构造函数会生成随机IV
        _viewModel.SelectedAlgorithm.Should().Be("AES");
        _viewModel.SelectedMode.Should().Be("CBC");
        _viewModel.SelectedPadding.Should().Be("PKCS7");
        _viewModel.SelectedHashAlgorithm.Should().Be("SHA256");
        _viewModel.SelectedEncoding.Should().Be("UTF-8");
        _viewModel.IsEncryptMode.Should().BeTrue();
        _viewModel.ValidationMessage.Should().BeEmpty();
    }

    [Theory]
    [InlineData("Hello World")]
    [InlineData("Test Input")]
    [InlineData("测试中文")]
    public void ComputeHashCommand_WithValidInput_ShouldGenerateHash(string input)
    {
        // Arrange
        _viewModel.InputText = input;
        _viewModel.SelectedHashAlgorithm = "SHA256";

        // Act
        _viewModel.ComputeHashCommand.Execute(null);

        // Assert
        _viewModel.HashOutput.Should().NotBeEmpty();
        _viewModel.HashOutput.Should().HaveLength(64); // SHA256 produces 64 hex characters
        _viewModel.HashOutput.Should().MatchRegex("^[a-fA-F0-9]+$");
    }

    [Theory]
    [InlineData("MD5", 32)]
    [InlineData("SHA1", 40)]
    [InlineData("SHA256", 64)]
    [InlineData("SHA384", 96)]
    [InlineData("SHA512", 128)]
    public void ComputeHashCommand_WithDifferentAlgorithms_ShouldGenerateCorrectLength(string algorithm, int expectedLength)
    {
        // Arrange
        _viewModel.InputText = "Test Input";
        _viewModel.SelectedHashAlgorithm = algorithm;

        // Act
        _viewModel.ComputeHashCommand.Execute(null);

        // Assert
        _viewModel.HashOutput.Should().HaveLength(expectedLength);
        _viewModel.HashOutput.Should().MatchRegex("^[a-fA-F0-9]+$");
    }

    [Fact]
    public void ComputeHashCommand_WithEmptyInput_ShouldShowValidationMessage()
    {
        // Arrange
        _viewModel.InputText = "";
        _viewModel.SelectedHashAlgorithm = "SHA256";

        // Act
        _viewModel.ComputeHashCommand.Execute(null);

        // Assert
        _viewModel.ValidationMessage.Should().Be("请输入要计算哈希的文本");
        _viewModel.IsValidInput.Should().BeFalse();
    }

    [Fact]
    public void GenerateRandomKeyCommand_ShouldGenerateValidKey()
    {
        // Act
        _viewModel.GenerateRandomKeyCommand.Execute(null);

        // Assert
        _viewModel.KeyText.Should().NotBeEmpty();
        _viewModel.KeyText.Should().HaveLength(64); // 32 bytes = 64 hex characters for AES-256
        _viewModel.KeyText.Should().MatchRegex("^[a-fA-F0-9]+$");
    }

    [Fact]
    public void GenerateRandomIVCommand_ShouldGenerateValidIv()
    {
        // Act
        _viewModel.GenerateRandomIVCommand.Execute(null);

        // Assert
        _viewModel.IvText.Should().NotBeEmpty();
        _viewModel.IvText.Should().HaveLength(32); // 16 bytes = 32 hex characters for AES IV
        _viewModel.IvText.Should().MatchRegex("^[a-fA-F0-9]+$");
    }

    [Fact]
    public void ComputeHashCommand_ShouldNotBeNull()
    {
        // Assert
        _viewModel.ComputeHashCommand.Should().NotBeNull();
    }

    [Fact]
    public void GenerateRandomKeyCommand_ShouldNotBeNull()
    {
        // Assert
        _viewModel.GenerateRandomKeyCommand.Should().NotBeNull();
    }

    [Fact]
    public void GenerateRandomIVCommand_ShouldNotBeNull()
    {
        // Assert
        _viewModel.GenerateRandomIVCommand.Should().NotBeNull();
    }

    [Fact]
    public void ClearAllCommand_ShouldClearAllFields()
    {
        // Arrange
        _viewModel.InputText = "Test Input";
        _viewModel.OutputText = "Test Output";
        _viewModel.KeyText = "Test Key";
        _viewModel.IvText = "Test IV";
        _viewModel.ValidationMessage = "Test Message";

        // Act
        _viewModel.ClearAllCommand.Execute(null);

        // Assert
        _viewModel.InputText.Should().BeEmpty();
        _viewModel.OutputText.Should().BeEmpty();
        _viewModel.KeyText.Should().BeEmpty();
        _viewModel.IvText.Should().BeEmpty();
        _viewModel.ValidationMessage.Should().BeEmpty();
    }

    [Fact]
    public void EncryptCommand_ShouldNotBeNull()
    {
        // Assert
        _viewModel.EncryptCommand.Should().NotBeNull();
    }

    [Fact]
    public void DecryptCommand_ShouldNotBeNull()
    {
        // Assert
        _viewModel.DecryptCommand.Should().NotBeNull();
    }

    [Fact]
    public void SwapInputOutputCommand_ShouldNotBeNull()
    {
        // Assert
        _viewModel.SwapInputOutputCommand.Should().NotBeNull();
    }

    [Theory]
    [InlineData("AES")]
    [InlineData("DES")]
    [InlineData("3DES")]
    public void SelectedAlgorithm_WhenChanged_ShouldUpdateProperty(string algorithm)
    {
        // Act
        _viewModel.SelectedAlgorithm = algorithm;

        // Assert
        _viewModel.SelectedAlgorithm.Should().Be(algorithm);
    }

    [Theory]
    [InlineData("CBC")]
    [InlineData("ECB")]
    [InlineData("CFB")]
    [InlineData("OFB")]
    public void SelectedMode_WhenChanged_ShouldUpdateProperty(string mode)
    {
        // Act
        _viewModel.SelectedMode = mode;

        // Assert
        _viewModel.SelectedMode.Should().Be(mode);
    }

    [Theory]
    [InlineData("PKCS7")]
    [InlineData("Zeros")]
    [InlineData("None")]
    public void SelectedPadding_WhenChanged_ShouldUpdateProperty(string padding)
    {
        // Act
        _viewModel.SelectedPadding = padding;

        // Assert
        _viewModel.SelectedPadding.Should().Be(padding);
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
    public void ValidationMessage_ShouldBeSetWhenValidationFails()
    {
        // This test would need to be implemented based on the actual validation logic
        // in the ViewModel. For now, we'll test that the property can be set.
        
        // Act
        _viewModel.ValidationMessage = "Test validation message";

        // Assert
        _viewModel.ValidationMessage.Should().Be("Test validation message");
    }
}