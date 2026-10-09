using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using MasterLink.Desktop.Data;

namespace MasterLink.Desktop.ViewModels;

// One collapsible block inside a System: a title and an Environment table
// (Seq #, Application, Path, User, Password). Each row carries its own
// collapsible Commands list - see EnvRowViewModel.
public partial class BlockViewModel : ObservableObject
{
    public int Id { get; }
    private readonly SystemViewModel parent;
    private bool loaded;

    [ObservableProperty] private string title = "";
    [ObservableProperty] private bool expanded = true;
    [ObservableProperty] private bool confirmingDelete;
    [ObservableProperty] private ObservableCollection<EnvRowViewModel> env = new();

    public BlockViewModel(BlockTree tree, SystemViewModel parent)
    {
        Id = tree.Block.Id;
        this.parent = parent;
        title = tree.Block.Title;
        Env = new ObservableCollection<EnvRowViewModel>(tree.Env.Select(e => new EnvRowViewModel(e, RemoveEnvRow)));
        RenumberEnv();
        loaded = true;
    }

    partial void OnTitleChanged(string value)
    {
        if (!loaded) return;
        using var c = Database.CreateConnection();
        BlockRepository.UpdateTitle(c, Id, value);
    }

    private void RenumberEnv() { for (var i = 0; i < Env.Count; i++) Env[i].Seq = i + 1; }

    [RelayCommand] private void ToggleExpanded() => Expanded = !Expanded;

    [RelayCommand]
    private void AddEnv()
    {
        using var c = Database.CreateConnection();
        Env.Add(new EnvRowViewModel(BlockRepository.AddEnv(c, Id), RemoveEnvRow));
        RenumberEnv();
    }

    private void RemoveEnvRow(EnvRowViewModel vm)
    {
        using var c = Database.CreateConnection();
        BlockRepository.DeleteEnv(c, vm.Id);
        Env.Remove(vm);
        RenumberEnv();
    }

    [RelayCommand] private void RequestDelete() => ConfirmingDelete = true;
    [RelayCommand] private void CancelDelete() => ConfirmingDelete = false;

    [RelayCommand]
    private void ConfirmDelete()
    {
        using var c = Database.CreateConnection();
        BlockRepository.DeleteBlock(c, Id);
        parent.RemoveBlock(this);
    }
}
