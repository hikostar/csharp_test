using JsonEditor.App.Infrastructure;

namespace JsonEditor.App.Tests;

/// <summary>
/// RelayCommand のテスト
/// </summary>
public class RelayCommandTests
{
    [Fact]
    public void Execute_CallsAction()
    {
        // Arrange
        var executed = false;
        var command = new RelayCommand(() => executed = true);

        // Act
        command.Execute(null);

        // Assert
        Assert.True(executed);
    }

    [Fact]
    public void CanExecute_DefaultReturnsTrue()
    {
        // Arrange
        var command = new RelayCommand(() => { });

        // Act
        var result = command.CanExecute(null);

        // Assert
        Assert.True(result);
    }

    [Fact]
    public void CanExecute_WithCondition_ReturnsTrueWhenConditionMet()
    {
        // Arrange
        var canExecute = true;
        var command = new RelayCommand(() => { }, () => canExecute);

        // Act
        var result = command.CanExecute(null);

        // Assert
        Assert.True(result);
    }

    [Fact]
    public void CanExecute_WithCondition_ReturnsFalseWhenConditionNotMet()
    {
        // Arrange
        var canExecute = false;
        var command = new RelayCommand(() => { }, () => canExecute);

        // Act
        var result = command.CanExecute(null);

        // Assert
        Assert.False(result);
    }

    [Fact]
    public void RaiseCanExecuteChanged_FiresCanExecuteChangedEvent()
    {
        // Arrange
        var eventFired = false;
        var command = new RelayCommand(() => { });
        command.CanExecuteChanged += (sender, e) => eventFired = true;

        // Act
        command.RaiseCanExecuteChanged();

        // Assert
        Assert.True(eventFired);
    }

    [Fact]
    public void CanExecuteChanged_FiredWhenConditionChanges()
    {
        // Arrange
        var canExecute = true;
        var eventFired = false;
        var command = new RelayCommand(() => { }, () => canExecute);
        command.CanExecuteChanged += (sender, e) => eventFired = true;

        // Act
        canExecute = false;
        command.RaiseCanExecuteChanged();

        // Assert
        Assert.True(eventFired);
    }

    [Fact]
    public void Execute_WithParameter_IgnoresParameter()
    {
        // Arrange
        var executed = false;
        var command = new RelayCommand(() => executed = true);

        // Act
        command.Execute("ignored");

        // Assert
        Assert.True(executed);
    }

    [Fact]
    public void MultipleExecute_CallsActionEachTime()
    {
        // Arrange
        var executionCount = 0;
        var command = new RelayCommand(() => executionCount++);

        // Act
        command.Execute(null);
        command.Execute(null);
        command.Execute(null);

        // Assert
        Assert.Equal(3, executionCount);
    }
}
