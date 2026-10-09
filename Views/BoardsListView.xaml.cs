using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using MasterLink.Desktop.ViewModels;

namespace MasterLink.Desktop.Views;

public partial class BoardsListView : UserControl
{
    public BoardsListView()
    {
        InitializeComponent();
    }

    private void BoardCard_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
    {
        if (sender is not FrameworkElement { Tag: BoardListItem item }) return;
        if (DataContext is BoardsListViewModel vm) vm.OpenBoardCommand.Execute(item);
    }
}
