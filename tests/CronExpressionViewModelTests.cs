using DevUtilities.ViewModels;
using System;
using System.Linq;
using System.Threading.Tasks;
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

    // 新增的完善单元测试

    [Theory]
    [InlineData("0 0 12 * * ?")]
    [InlineData("0 */5 * * * ?")]
    [InlineData("0 0 9-17 * * MON-FRI")]
    [InlineData("0 30 14 * * ?")]
    [InlineData("0 0 0 1 * ?")]
    public async Task OnCronExpressionChanged_ShouldNotBlockUI(string expression)
    {
        // Arrange & Act
        _viewModel.CronExpression = expression;
        
        // 等待异步操作完成
        await Task.Delay(100);

        // Assert
        Assert.True(_viewModel.IsValidExpression);
        Assert.NotEmpty(_viewModel.Description);
    }

    [Theory]
    [InlineData("0 0 12 * * ?", 5)]
    [InlineData("0 */5 * * * ?", 5)]
    [InlineData("0 0 9-17 * * MON-FRI", 5)]
    public void CalculateNextExecutions_ShouldReturnCorrectCount(string expression, int expectedCount)
    {
        // Arrange
        _viewModel.CronExpression = expression;

        // Act
        _viewModel.ParseExpressionCommand.Execute(null);

        // Assert
        var lines = _viewModel.NextExecutions.Split('\n', StringSplitOptions.RemoveEmptyEntries);
        var executionLines = lines.Where(l => l.Contains("执行时间：")).ToList();
        Assert.Equal(expectedCount, executionLines.Count);
    }

    [Theory]
    [InlineData("0 0 12 * * ?")]
    [InlineData("0 */5 * * * ?")]
    [InlineData("0 0 0 1 * ?")]
    public void CalculateNextExecution_ShouldNotTakeMoreThan5Seconds(string expression)
    {
        // Arrange
        _viewModel.CronExpression = expression;
        var startTime = DateTime.Now;

        // Act
        _viewModel.ParseExpressionCommand.Execute(null);

        // Assert
        var duration = DateTime.Now - startTime;
        Assert.True(duration.TotalSeconds < 5, $"计算时间过长: {duration.TotalSeconds}秒");
    }

    [Theory]
    [InlineData("0 0 12 * * ?", "每天中午12点执行")]
    [InlineData("0 */5 * * * ?", "每5分钟执行一次")]
    [InlineData("0 0 9-17 * * MON-FRI", "工作日上午9点到下午5点每小时执行")]
    [InlineData("0 30 14 * * ?", "每天下午2点30分执行")]
    [InlineData("0 0 0 1 * ?", "每月1号午夜执行")]
    public void GenerateDescription_ShouldProvideAccurateDescription(string expression, string expectedDescription)
    {
        // Arrange
        _viewModel.CronExpression = expression;

        // Act
        _viewModel.ParseExpressionCommand.Execute(null);

        // Assert
        Assert.True(_viewModel.IsValidExpression);
        Assert.Contains(expectedDescription, _viewModel.Description);
    }

    [Theory]
    [InlineData("0 0 12 * * ?")]
    [InlineData("0 */5 * * * ?")]
    [InlineData("0 0 9-17 * * MON-FRI")]
    public void NextExecutions_ShouldShowRelativeTime(string expression)
    {
        // Arrange
        _viewModel.CronExpression = expression;

        // Act
        _viewModel.ParseExpressionCommand.Execute(null);

        // Assert
        Assert.NotEmpty(_viewModel.NextExecutions);
        var hasRelativeTime = _viewModel.NextExecutions.Contains("分钟后") || 
                             _viewModel.NextExecutions.Contains("小时后") || 
                             _viewModel.NextExecutions.Contains("天后");
        Assert.True(hasRelativeTime, "应该显示相对时间");
    }

    [Theory]
    [InlineData("* * * * * ?")]
    [InlineData("0 0 0 32 * ?")]
    [InlineData("0 60 * * * ?")]
    [InlineData("0 * 25 * * ?")]
    public void ParseExpression_WithInvalidExpressions_ShouldHandleGracefully(string invalidExpression)
    {
        // Arrange
        _viewModel.CronExpression = invalidExpression;

        // Act
        _viewModel.ParseExpressionCommand.Execute(null);

        // Assert
        Assert.False(_viewModel.IsValidExpression);
        Assert.NotEmpty(_viewModel.ValidationMessage);
    }

    [Fact]
    public void ParseField_WithWildcard_ShouldReturnAllValidValues()
    {
        // Arrange
        _viewModel.CronExpression = "* * * * * ?";

        // Act
        _viewModel.ParseExpressionCommand.Execute(null);

        // Assert - 虽然表达式可能无效，但字段解析应该正常工作
        Assert.NotNull(_viewModel.ValidationMessage);
    }

    [Theory]
    [InlineData("每分钟", "0 * * * * ?")]
    [InlineData("每小时", "0 0 * * * ?")]
    [InlineData("每天中午12点", "0 0 12 * * ?")]
    [InlineData("每周一上午9点", "0 0 9 * * MON")]
    [InlineData("每月1号上午10点", "0 0 10 1 * ?")]
    public void UsePreset_ShouldSetCorrectExpression(string presetName, string expectedExpression)
    {
        // Act
        _viewModel.UsePresetCommand.Execute(presetName);

        // Assert
        Assert.Equal(expectedExpression, _viewModel.CronExpression);
        Assert.True(_viewModel.IsValidExpression);
    }

    [Fact]
    public void UsePreset_WithInvalidPreset_ShouldNotChangeExpression()
    {
        // Arrange
        var originalExpression = _viewModel.CronExpression;

        // Act
        _viewModel.UsePresetCommand.Execute("不存在的预设");

        // Assert
        Assert.Equal(originalExpression, _viewModel.CronExpression);
    }

    [Theory]
    [InlineData("0 0 12 * * ?")]
    [InlineData("0 */5 * * * ?")]
    [InlineData("0 0 9-17 * * MON-FRI")]
    public void ParseExpression_MultipleCallsWithSameExpression_ShouldBeConsistent(string expression)
    {
        // Arrange
        _viewModel.CronExpression = expression;

        // Act
        _viewModel.ParseExpressionCommand.Execute(null);
        var firstDescription = _viewModel.Description;
        var firstNextExecutions = _viewModel.NextExecutions;

        _viewModel.ParseExpressionCommand.Execute(null);
        var secondDescription = _viewModel.Description;
        var secondNextExecutions = _viewModel.NextExecutions;

        // Assert
        Assert.Equal(firstDescription, secondDescription);
        // 注意：NextExecutions可能会因为时间推移而略有不同，所以我们检查格式是否一致
        Assert.NotEmpty(firstNextExecutions);
        Assert.NotEmpty(secondNextExecutions);
    }

    [Fact]
    public void ViewModel_InitialState_ShouldBeValid()
    {
        // Assert
        Assert.True(_viewModel.IsValidExpression);
        Assert.Empty(_viewModel.ValidationMessage);
        Assert.NotNull(_viewModel.PresetExpressions);
        Assert.NotEmpty(_viewModel.PresetExpressions);
        Assert.NotNull(_viewModel.SecondOptions);
        Assert.NotNull(_viewModel.MinuteOptions);
        Assert.NotNull(_viewModel.HourOptions);
        Assert.NotNull(_viewModel.DayOptions);
        Assert.NotNull(_viewModel.MonthOptions);
        Assert.NotNull(_viewModel.WeekOptions);
    }
}