using DevUtilities.ViewModels;
using System;
using System.Linq;
using Xunit;

namespace DevUtilities.Tests;

public class CronExpressionViewModelTests
{
    private readonly CronExpressionViewModel _viewModel;

    public CronExpressionViewModelTests()
    {
        _viewModel = new CronExpressionViewModel();
    }

    [Fact]
    public void ParseExpression_WithValidExpression_ShouldSetIsValidToTrue()
    {
        // Arrange
        _viewModel.CronExpression = "0 0 12 * * ?";

        // Act
        _viewModel.ParseExpressionCommand.Execute(null);

        // Assert
        Assert.True(_viewModel.IsValidExpression);
        Assert.NotEmpty(_viewModel.Description);
    }

    [Fact]
    public void ParseExpression_WithInvalidExpression_ShouldSetIsValidToFalse()
    {
        // Arrange
        _viewModel.CronExpression = "invalid expression";

        // Act
        _viewModel.ParseExpressionCommand.Execute(null);

        // Assert
        Assert.False(_viewModel.IsValidExpression);
        Assert.NotEmpty(_viewModel.ValidationMessage);
    }

    [Fact]
    public void ParseExpression_WithEmptyExpression_ShouldSetIsValidToFalse()
    {
        // Arrange
        _viewModel.CronExpression = "";

        // Act
        _viewModel.ParseExpressionCommand.Execute(null);

        // Assert
        Assert.False(_viewModel.IsValidExpression);
        Assert.NotEmpty(_viewModel.ValidationMessage);
    }

    [Fact]
    public void GenerateExpression_ShouldCreateValidExpression()
    {
        // Arrange
        _viewModel.SelectedSecond = "0";
        _viewModel.SelectedMinute = "30";
        _viewModel.SelectedHour = "14";
        _viewModel.SelectedDay = "*";
        _viewModel.SelectedMonth = "*";
        _viewModel.SelectedWeek = "?";

        // Act
        _viewModel.GenerateExpressionCommand.Execute(null);

        // Assert
        Assert.Equal("0 30 14 * * ?", _viewModel.CronExpression);
        Assert.True(_viewModel.IsValidExpression);
    }

    [Fact]
    public void UsePresetCommand_ShouldSetCronExpressionAndParseIt()
    {
        // Arrange
        var preset = _viewModel.PresetExpressions.First();

        // Act
        _viewModel.UsePresetCommand.Execute(preset.Key);

        // Assert
        Assert.Equal(preset.Value, _viewModel.CronExpression);
        Assert.True(_viewModel.IsValidExpression);
        Assert.NotEmpty(_viewModel.Description);
    }

    [Fact]
    public void ClearCommand_ShouldResetAllFields()
    {
        // Arrange
        _viewModel.CronExpression = "0 0 12 * * ?";
        _viewModel.ParseExpressionCommand.Execute(null);

        // Act
        _viewModel.ClearCommand.Execute(null);

        // Assert
        Assert.Empty(_viewModel.CronExpression);
        Assert.Empty(_viewModel.Description);
        Assert.Empty(_viewModel.ValidationMessage);
        Assert.True(_viewModel.IsValidExpression);
        Assert.Equal("0", _viewModel.SelectedSecond);
        Assert.Equal("0", _viewModel.SelectedMinute);
        Assert.Equal("12", _viewModel.SelectedHour);
        Assert.Equal("*", _viewModel.SelectedDay);
        Assert.Equal("*", _viewModel.SelectedMonth);
        Assert.Equal("?", _viewModel.SelectedWeek);
    }

    [Theory]
    [InlineData("0 0 12 * * ?", "每天中午12点执行")]
    [InlineData("0 */5 * * * ?", "每5分钟执行一次")]
    [InlineData("0 0 9-17 * * MON-FRI", "工作日上午9点到下午5点每小时执行")]
    public void ParseExpression_WithKnownExpressions_ShouldGenerateCorrectDescription(string expression, string expectedDescription)
    {
        // Arrange
        _viewModel.CronExpression = expression;

        // Act
        _viewModel.ParseExpressionCommand.Execute(null);

        // Assert
        Assert.True(_viewModel.IsValidExpression);
        Assert.Contains(expectedDescription, _viewModel.Description);
    }

    [Fact]
    public void NextExecutions_ShouldContainFutureExecutionTimes()
    {
        // Arrange
        _viewModel.CronExpression = "0 0 12 * * ?";

        // Act
        _viewModel.ParseExpressionCommand.Execute(null);

        // Assert
        Assert.NotEmpty(_viewModel.NextExecutions);
        var lines = _viewModel.NextExecutions.Split('\n', StringSplitOptions.RemoveEmptyEntries);
        foreach (var line in lines)
        {
            if (line.Contains("执行时间：") && line.Split('：').Length > 1)
            {
                var timeStr = line.Split('：')[1].Trim();
                if (!string.IsNullOrEmpty(timeStr))
                {
                    Assert.True(DateTime.Parse(timeStr) > DateTime.Now);
                }
            }
        }
    }

    [Fact]
    public void PresetExpressions_ShouldContainCommonExpressions()
    {
        // Assert
        Assert.NotEmpty(_viewModel.PresetExpressions);
        Assert.Contains(_viewModel.PresetExpressions, p => p.Key == "每分钟");
        Assert.Contains(_viewModel.PresetExpressions, p => p.Key == "每小时");
        Assert.Contains(_viewModel.PresetExpressions, p => p.Key == "每天中午12点");
        Assert.Contains(_viewModel.PresetExpressions, p => p.Key == "每周一上午9点");
        Assert.Contains(_viewModel.PresetExpressions, p => p.Key == "每月1号上午10点");
    }

    [Fact]
    public void FieldOptions_ShouldContainValidValues()
    {
        // Assert
        Assert.NotEmpty(_viewModel.SecondOptions);
        Assert.NotEmpty(_viewModel.MinuteOptions);
        Assert.NotEmpty(_viewModel.HourOptions);
        Assert.NotEmpty(_viewModel.DayOptions);
        Assert.NotEmpty(_viewModel.MonthOptions);
        Assert.NotEmpty(_viewModel.WeekOptions);

        // Check that wildcard option exists
        Assert.Contains("*", _viewModel.SecondOptions);
        Assert.Contains("*", _viewModel.MinuteOptions);
        Assert.Contains("*", _viewModel.HourOptions);
        Assert.Contains("*", _viewModel.DayOptions);
        Assert.Contains("*", _viewModel.MonthOptions);
        Assert.Contains("?", _viewModel.WeekOptions);
    }
}