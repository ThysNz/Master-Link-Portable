using CommunityToolkit.Mvvm.ComponentModel;

namespace MasterLink.Desktop.Services;

// Drives MainWindow's ContentControl. Views are resolved from view models via
// DataTemplates registered in App.xaml (DataType -> View), so navigating is
// just swapping which view model is "current".
public partial class NavigationService : ObservableObject
{
    [ObservableProperty]
    private object? currentViewModel;

    public void NavigateTo(object viewModel)
    {
        CurrentViewModel = viewModel;
    }
}
