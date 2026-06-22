using System.Collections.ObjectModel;

namespace JsonEditor.App.ViewModels;

public sealed class JsonTreeNodeViewModel
{
    public required string Label { get; init; }

    public ObservableCollection<JsonTreeNodeViewModel> Children { get; } = new();
}
