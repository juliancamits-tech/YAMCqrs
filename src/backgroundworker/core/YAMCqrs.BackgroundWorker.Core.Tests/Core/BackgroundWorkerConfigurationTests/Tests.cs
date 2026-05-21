using FluentAssertions;
using YAMCqrs.BackgroundWorker.Core.Configuration;

namespace YAMCqrs.BackgroundWorker.Core.Tests.Core.BackgroundWorkerConfigurationTests;

public class Tests
{
    [Fact]
    public void BackgroundWorkerConfiguration_Should_Have_Default_Values()
    {
        // Arrange & Act
        var config = new BackgroundWorkerConfiguration();

        // Assert
        config.MinutesToKeepSuccesTask.Should().Be(60);
        config.MinutesToKeepFailedTask.Should().Be(60);
    }

    [Fact]
    public void BackgroundWorkerConfiguration_Should_Allow_Custom_Values()
    {
        // Arrange & Act
        var config = new BackgroundWorkerConfiguration
        {
            MinutesToKeepSuccesTask = 120,
            MinutesToKeepFailedTask = 1440
        };

        // Assert
        config.MinutesToKeepSuccesTask.Should().Be(120);
        config.MinutesToKeepFailedTask.Should().Be(1440);
    }

    [Theory]
    [InlineData(1, 60)]
    [InlineData(2, 120)]
    [InlineData(24, 1440)]
    [InlineData(0, 0)]
    public void HourToMinutes_Should_Convert_Hours_To_Minutes(int hours, int expectedMinutes)
    {
        // Act
        var result = BackgroundWorkerConfiguration.HourToMinutes(hours);

        // Assert
        result.Should().Be(expectedMinutes);
    }

    [Theory]
    [InlineData(1, 1440)]
    [InlineData(2, 2880)]
    [InlineData(7, 10080)]
    [InlineData(0, 0)]
    public void DayToMinutes_Should_Convert_Days_To_Minutes(int days, int expectedMinutes)
    {
        // Act
        var result = BackgroundWorkerConfiguration.DayToMinutes(days);

        // Assert
        result.Should().Be(expectedMinutes);
    }

    [Fact]
    public void Configuration_Can_Use_Helper_Methods()
    {
        // Arrange & Act
        var config = new BackgroundWorkerConfiguration
        {
            MinutesToKeepSuccesTask = BackgroundWorkerConfiguration.HourToMinutes(2),
            MinutesToKeepFailedTask = BackgroundWorkerConfiguration.DayToMinutes(7)
        };

        // Assert
        config.MinutesToKeepSuccesTask.Should().Be(120); // 2 hours
        config.MinutesToKeepFailedTask.Should().Be(10080); // 7 days
    }

    [Fact]
    public void Configuration_Parameter_Constructor()
    {
        var random1 = Random.Shared.Next(1, 80);
        var random2 = Random.Shared.Next(1, 80);
        var config = new BackgroundWorkerConfiguration(random1, random2);

        // Assert
        config.MinutesToKeepSuccesTask.Should().Be(random1);
        config.MinutesToKeepFailedTask.Should().Be(random2);

    }
}
