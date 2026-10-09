using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using MasterLink.Desktop.Data;

namespace MasterLink.Desktop.ViewModels;

// One board's systems: header (rename/delete board), the "+ New System" form,
// and the list of System cards.
public partial class BoardDetailViewModel : ObservableObject
{
    public int BoardId { get; }

    [ObservableProperty] private string boardName;
    [ObservableProperty] private string? renameError;
    [ObservableProperty] private bool confirmingDeleteBoard;
    [ObservableProperty] private bool showNewSystemForm;
    [ObservableProperty] private string newSystemName = "";
    [ObservableProperty] private ObservableCollection<SystemViewModel> systems = new();

    private bool loaded;
    private string boardNameBeforeEdit;

    public BoardDetailViewModel(int boardId)
    {
        BoardId = boardId;
        using (var c = Database.CreateConnection())
        {
            var board = BoardRepository.FindById(c, boardId)!;
            boardName = board.Name;
        }
        boardNameBeforeEdit = boardName;
        Reload();
        loaded = true;
    }

    private void Reload()
    {
        using var c = Database.CreateConnection();
        var trees = SystemRepository.GetTreesForBoard(c, BoardId);
        Systems = new ObservableCollection<SystemViewModel>(trees.Select(t => new SystemViewModel(t, this)));
    }

    partial void OnBoardNameChanged(string value)
    {
        if (!loaded) return;
        if (string.IsNullOrWhiteSpace(value) || value == boardNameBeforeEdit) return;

        using var c = Database.CreateConnection();
        var result = BoardRepository.Rename(c, BoardId, value);
        if (result.Error != null)
        {
            RenameError = result.Error;
            return;
        }
        RenameError = null;
        boardNameBeforeEdit = value;
    }

    [RelayCommand]
    private void DoneEditing() => App.Navigation.NavigateTo(new BoardViewModel(BoardId));

    [RelayCommand]
    private void GoBack() => App.Navigation.NavigateTo(new BoardsListViewModel());

    [RelayCommand] private void RequestDeleteBoard() => ConfirmingDeleteBoard = true;
    [RelayCommand] private void CancelDeleteBoard() => ConfirmingDeleteBoard = false;

    [RelayCommand]
    private void ConfirmDeleteBoard()
    {
        using var c = Database.CreateConnection();
        BoardRepository.Delete(c, BoardId);
        App.Navigation.NavigateTo(new BoardsListViewModel());
    }

    [RelayCommand]
    private void ExpandAll() { foreach (var s in Systems) s.Expanded = true; }

    [RelayCommand]
    private void CollapseAll() { foreach (var s in Systems) s.Expanded = false; }

    [RelayCommand]
    private void ToggleNewSystemForm() => ShowNewSystemForm = !ShowNewSystemForm;

    [RelayCommand]
    private void CancelNewSystem() { ShowNewSystemForm = false; NewSystemName = ""; }

    [RelayCommand]
    private void CreateSystem()
    {
        if (string.IsNullOrWhiteSpace(NewSystemName)) return;
        using var c = Database.CreateConnection();
        var system = SystemRepository.Add(c, BoardId, NewSystemName.Trim());
        Systems.Add(new SystemViewModel(new SystemTree { System = system }, this));
        CancelNewSystem();
    }

    public void RemoveSystem(SystemViewModel system) => Systems.Remove(system);
}
