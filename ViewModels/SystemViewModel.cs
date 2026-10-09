using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using MasterLink.Desktop.Data;

namespace MasterLink.Desktop.ViewModels;

// Wraps one System card: header fields (name/description), expand/collapse,
// its Blocks and a free-text Notes field.
public partial class SystemViewModel : ObservableObject
{
    public int Id { get; }
    private readonly BoardDetailViewModel parent;
    private bool loaded;

    [ObservableProperty] private string name = "";
    [ObservableProperty] private string? description;
    [ObservableProperty] private string? notes;
    [ObservableProperty] private bool expanded = true;
    [ObservableProperty] private bool confirmingDelete;
    [ObservableProperty] private ObservableCollection<BlockViewModel> blocks = new();

    public SystemViewModel(SystemTree tree, BoardDetailViewModel parent)
    {
        Id = tree.System.Id;
        this.parent = parent;

        name = tree.System.Name;
        description = tree.System.Description;
        notes = tree.System.Notes;
        Blocks = new ObservableCollection<BlockViewModel>(tree.Blocks.Select(b => new BlockViewModel(b, this)));

        loaded = true;
    }

    partial void OnNameChanged(string value) { if (loaded) CommitField("name", value); }
    partial void OnDescriptionChanged(string? value) { if (loaded) CommitField("description", value); }
    partial void OnNotesChanged(string? value) { if (loaded) CommitField("notes", value); }

    private void CommitField(string field, string? value)
    {
        using var c = Database.CreateConnection();
        SystemRepository.UpdateField(c, Id, field, value);
    }

    [RelayCommand]
    private void ToggleExpanded() => Expanded = !Expanded;

    [RelayCommand]
    private void AddBlock()
    {
        using var c = Database.CreateConnection();
        var block = BlockRepository.AddBlock(c, Id);
        Blocks.Add(new BlockViewModel(new BlockTree { Block = block }, this));
    }

    public void RemoveBlock(BlockViewModel block) => Blocks.Remove(block);

    [RelayCommand] private void RequestDelete() => ConfirmingDelete = true;
    [RelayCommand] private void CancelDelete() => ConfirmingDelete = false;

    [RelayCommand]
    private void ConfirmDelete()
    {
        using var c = Database.CreateConnection();
        SystemRepository.Delete(c, Id);
        parent.RemoveSystem(this);
    }
}
