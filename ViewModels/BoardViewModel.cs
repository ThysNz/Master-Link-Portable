using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

namespace MasterLink.Desktop.ViewModels;

// Read-only face of a board and the default page when a board is opened.
// It reuses BoardDetailViewModel's loaded systems (so expand/collapse state
// carries across) but exposes no editing; "Edit" swaps to the editable page.
public partial class BoardViewModel : ObservableObject
{
    public BoardDetailViewModel Detail { get; }
    public string BoardName => Detail.BoardName;
    public int BoardId => Detail.BoardId;

    public BoardViewModel(int boardId)
    {
        Detail = new BoardDetailViewModel(boardId);
    }

    [RelayCommand] private void GoBack() => App.Navigation.NavigateTo(new BoardsListViewModel());
    [RelayCommand] private void Edit() => App.Navigation.NavigateTo(Detail);
    [RelayCommand] private void ExpandAll() => Detail.ExpandAllCommand.Execute(null);
    [RelayCommand] private void CollapseAll() => Detail.CollapseAllCommand.Execute(null);
}
