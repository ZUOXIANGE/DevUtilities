using DevUtilities.ViewModels;
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
    public void ParseCronExpression_WithValidExpression_ShouldSetIsValidToTrue()
    {
        // Arrange
        _viewModel.CronExpression = "0 0 12 * * ?";

        // Act
        _viewModel.ParseCronExpressionCommand.Execute(null);

        // Assert
        Assert.True(_viewModel.IsValid);
        Assert.Empty(_viewModel.ValidationMessage);
        Assert.NotEmpty(_viewModel.Description);
    }

    [Fact]
    public void ParseCronExpression_WithInvalidExpression_ShouldSetIsValidToFalse()
    {
        // Arrange
        _viewModel.CronExpression = "invalid cron";

        // Act
        _viewModel.ParseCronExpressionCommand.Execute(null);

        // Assert
        Assert.False(_viewModel.IsValid);
        Assert.NotEmpty(_viewModel.ValidationMessage);
    }

    [Fact]
    public void ParseCronExpression_WithEmptyExpression_ShouldSetIsValidToFalse()
    {
        // Arrange
        _viewModel.CronExpression = "";

        // Act
        _viewModel.ParseCronExpressionCommand.Execute(null);

        // Assert
        Assert.False(_viewModel.IsValid);
        Assert.NotEmpty(_viewModel.ValidationMessage);
    }

    [Fact]
    public void GenerateCronExpression_ShouldCreateValidExpression()
    {
        // Arrange
        _viewModel.SelectedSecond = "0";
        _viewModel.SelectedMinute = "30";
        _viewModel.SelectedHour = "14";
        _viewModel.SelectedDay = "*";
        _viewModel.SelectedMonth = "*";
        _viewModel.SelectedWeek = "?";

        // Act
        _viewModel.GenerateCronExpressionCommand.Execute(null);

        // Assert
        Assert.Equal("0 30 14 * * ?", _viewModel.CronExpression);
        Assert.True(_viewModel.IsValid);
    }

    [Fact]
    public void UsePresetCommand_ShouldSetCronExpression()
    {
        // Arrange
        var preset = _viewModel.PresetExpressions.First();

        // Act
        _viewModel.UsePresetCommand.Execute(preset);

        // Assert
        Assert.Equal(preset.Expression, _viewModel.CronExpression);
        Assert.True(_viewModel.IsValid);
        Assert.NotEmpty(_viewModel.Description);
    }

    [Fact]
    public void ClearCommand_ShouldResetAllFields()
    {
        // Arrange
        _viewModel.CronExpression = "0 0 12 * * ?";
        _viewModel.ParseCronExpressionCommand.Execute(null);

        // Act
        _viewModel.ClearCommand.Execute(null);

        // Assert
        Assert.Empty(_viewModel.CronExpression);
        Assert.Empty(_viewModel.Description);
        Assert.Empty(_viewModel.ValidationMessage);
        Assert.False(_viewModel.IsValid);
        Assert.Equal("*", _viewModel.SelectedSecond);
        Assert.Equal("*", _viewModel.SelectedMinute);
        Assert.Equal("*", _viewModel.SelectedHour);
        Assert.Equal("*", _viewModel.SelectedDay);
        Assert.Equal("*", _viewModel.SelectedMonth);
        Assert.Equal("?", _viewModel.SelectedWeek);
    }

    [Theory]
    [InlineData("0 0 12 * * ?", "每天中午12点执行")]
    [InlineData("0 */5 * * * ?", "每5分钟执行一次")]
    [InlineData("0 0 9-17 * * MON-FRI", "工作日上午9点到下午5点每小时执行")]
    public void ParseCronExpression_WithKnownExpressions_ShouldGenerateCorrectDescription(string expression, string expectedDescription)
    {
        // Arrange
        _viewModel.CronExpression = expression;

        // Act
        _viewModel.ParseCronExpressionCommand.Execute(null);

        // Assert
        Assert.True(_viewModel.IsValid);
        Assert.Contains(expectedDescription, _viewModel.Description);
    }

    [Fact]
    public void NextExecutionTimes_ShouldContainFutureExecutions()
    {
        // Arrange
        _viewModel.CronExpression = "0 0 12 * * ?";

        // Act
        _viewModel.ParseCronExpressionCommand.Execute(null);

        // Assert
        Assert.NotEmpty(_viewModel.NextExecutionTimes);
        Assert.All(_viewModel.NextExecutionTimes, time => Assert.True(DateTime.Parse(time) > DateTime.Now));
    }

    [Fact]
    public void PresetExpressions_ShouldContainCommonExpressions()
    {
        // Assert
        Assert.NotEmpty(_viewModel.PresetExpressions);
        Assert.Contains(_viewModel.PresetExpressions, p => p.Name == "每分钟");
        Assert.Contains(_viewModel.PresetExpressions, p => p.Name == "每小时");
        Assert.Contains(_viewModel.PresetExpressions, p => p.Name == "每天");
        Assert.Contains(_viewModel.PresetExpressions, p => p.Name == "每周");
        Assert.Contains(_viewModel.PresetExpressions, p => p.Name == "每月");
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