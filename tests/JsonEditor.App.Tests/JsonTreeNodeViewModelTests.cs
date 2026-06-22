using System.Collections.ObjectModel;
using JsonEditor.App.ViewModels;

namespace JsonEditor.App.Tests;

/// <summary>
/// JsonTreeNodeViewModel のテスト
/// </summary>
public class JsonTreeNodeViewModelTests
{
    [Fact]
    public void Constructor_SetsLabel()
    {
        // Arrange & Act
        var viewModel = new JsonTreeNodeViewModel { Label = "Test" };

        // Assert
        Assert.Equal("Test", viewModel.Label);
    }

    [Fact]
    public void Children_InitializesEmptyCollection()
    {
        // Arrange & Act
        var viewModel = new JsonTreeNodeViewModel { Label = "Root" };

        // Assert
        Assert.NotNull(viewModel.Children);
        Assert.Empty(viewModel.Children);
    }

    [Fact]
    public void Children_CanAddItems()
    {
        // Arrange
        var parent = new JsonTreeNodeViewModel { Label = "Parent" };
        var child = new JsonTreeNodeViewModel { Label = "Child" };

        // Act
        parent.Children.Add(child);

        // Assert
        Assert.Single(parent.Children);
        Assert.Equal(child, parent.Children[0]);
    }

    [Fact]
    public void Children_CanAddMultipleItems()
    {
        // Arrange
        var parent = new JsonTreeNodeViewModel { Label = "Parent" };
        var child1 = new JsonTreeNodeViewModel { Label = "Child1" };
        var child2 = new JsonTreeNodeViewModel { Label = "Child2" };
        var child3 = new JsonTreeNodeViewModel { Label = "Child3" };

        // Act
        parent.Children.Add(child1);
        parent.Children.Add(child2);
        parent.Children.Add(child3);

        // Assert
        Assert.Equal(3, parent.Children.Count);
        Assert.Equal("Child1", parent.Children[0].Label);
        Assert.Equal("Child2", parent.Children[1].Label);
        Assert.Equal("Child3", parent.Children[2].Label);
    }

    [Fact]
    public void Children_IsObservableCollection()
    {
        // Arrange
        var parent = new JsonTreeNodeViewModel { Label = "Parent" };

        // Act & Assert
        Assert.IsType<ObservableCollection<JsonTreeNodeViewModel>>(parent.Children);
    }

    [Fact]
    public void Children_CanRemoveItems()
    {
        // Arrange
        var parent = new JsonTreeNodeViewModel { Label = "Parent" };
        var child = new JsonTreeNodeViewModel { Label = "Child" };
        parent.Children.Add(child);

        // Act
        parent.Children.Remove(child);

        // Assert
        Assert.Empty(parent.Children);
    }

    [Fact]
    public void Label_IsInitOnly()
    {
        // Arrange
        var viewModel = new JsonTreeNodeViewModel { Label = "Initial" };

        // Act & Assert
        // Label は init-only なので、作成時にのみ設定可能
        Assert.Equal("Initial", viewModel.Label);
        // 以下はコンパイル時エラーなので、実行時には確認できない
        // viewModel.Label = "Modified"; // ✗ コンパイルエラー
    }

    [Fact]
    public void NestedHierarchy_BuildsCorrectly()
    {
        // Arrange
        var root = new JsonTreeNodeViewModel { Label = "Root" };
        var level1 = new JsonTreeNodeViewModel { Label = "Level1" };
        var level2 = new JsonTreeNodeViewModel { Label = "Level2" };

        // Act
        root.Children.Add(level1);
        level1.Children.Add(level2);

        // Assert
        Assert.Single(root.Children);
        Assert.Single(level1.Children);
        Assert.Equal("Level1", root.Children[0].Label);
        Assert.Equal("Level2", level1.Children[0].Label);
    }

    [Fact]
    public void Children_CanClear()
    {
        // Arrange
        var parent = new JsonTreeNodeViewModel { Label = "Parent" };
        parent.Children.Add(new JsonTreeNodeViewModel { Label = "Child1" });
        parent.Children.Add(new JsonTreeNodeViewModel { Label = "Child2" });

        // Act
        parent.Children.Clear();

        // Assert
        Assert.Empty(parent.Children);
    }

    [Fact]
    public void EmptyLabel_IsAccepted()
    {
        // Arrange & Act
        var viewModel = new JsonTreeNodeViewModel { Label = "" };

        // Assert
        Assert.Equal("", viewModel.Label);
    }

    [Fact]
    public void MultipleInstances_AreIndependent()
    {
        // Arrange
        var vm1 = new JsonTreeNodeViewModel { Label = "VM1" };
        var vm2 = new JsonTreeNodeViewModel { Label = "VM2" };

        // Act
        vm1.Children.Add(new JsonTreeNodeViewModel { Label = "Child1" });
        vm2.Children.Add(new JsonTreeNodeViewModel { Label = "Child2" });

        // Assert
        Assert.Single(vm1.Children);
        Assert.Single(vm2.Children);
        Assert.Equal("Child1", vm1.Children[0].Label);
        Assert.Equal("Child2", vm2.Children[0].Label);
    }
}
