using CommunityToolkit.Mvvm.Input;

namespace MasterLink.Desktop.ViewModels;

// Static help page; the content lives in HelpView.xaml.
public partial class HelpViewModel
{
    [RelayCommand]
    private void GoBack() => App.Navigation.NavigateTo(new BoardsListViewModel());
}
