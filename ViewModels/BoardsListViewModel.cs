using System.Collections.ObjectModel;
using System.Windows;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using MasterLink.Desktop.Data;
using MasterLink.Desktop.Views;

namespace MasterLink.Desktop.ViewModels;

public partial class BoardListItem : ObservableObject
{
    public int Id { get; set; }
    public string Name { get; set; } = "";
    public int SystemCount { get; set; }
    public string CountsLabel => $"{SystemCount} system{(SystemCount == 1 ? "" : "s")}";
}

public partial class BoardsListViewModel : ObservableObject
{
    public string Title => "System Boards";

    [ObservableProperty]
    private ObservableCollection<BoardListItem> boards = new();

    public BoardsListViewModel()
    {
        Reload();
    }

    private void Reload()
    {
        using var c = Database.CreateConnection();
        var list = BoardRepository.List(c);
        Boards = new ObservableCollection<BoardListItem>(
            list.Select(x => new BoardListItem { Id = x.Board.Id, Name = x.Board.Name, SystemCount = x.SystemCount }));
    }

    [RelayCommand]
    private void OpenBoard(BoardListItem? item)
    {
        if (item == null) return;
        App.Navigation.NavigateTo(new BoardDetailViewModel(item.Id));
    }

    [RelayCommand]
    private void CreateBoard()
    {
        var name = TextInputDialog.Prompt(Application.Current.MainWindow!, "New Board", "Board name", "");
        if (string.IsNullOrWhiteSpace(name)) return;

        using var c = Database.CreateConnection();
        var result = BoardRepository.Create(c, name);
        if (result.Error != null)
        {
            MessageBox.Show(result.Error, "Master Link", MessageBoxButton.OK, MessageBoxImage.Warning);
            return;
        }
        Reload();
    }
}
