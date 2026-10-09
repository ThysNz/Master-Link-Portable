using System.Windows;
using MasterLink.Desktop.Data;
using MasterLink.Desktop.Services;
using MasterLink.Desktop.ViewModels;

namespace MasterLink.Desktop;

public partial class App : Application
{
    public static NavigationService Navigation { get; } = new();

    protected override void OnStartup(StartupEventArgs e)
    {
        base.OnStartup(e);

        // Portable app with no installer/event log to check - write crashes next
        // to the exe so a user hitting a bug has something to send back, and
        // keep the app alive instead of the default silent-terminate behavior.
        DispatcherUnhandledException += (_, args) =>
        {
            try
            {
                System.IO.File.AppendAllText(
                    System.IO.Path.Combine(AppContext.BaseDirectory, "crash.log"),
                    $"{DateTime.Now:O}\n{args.Exception}\n\n");
            }
            catch { /* best-effort logging only */ }

            MessageBox.Show(
                $"Something went wrong:\n{args.Exception.Message}\n\nDetails were written to crash.log next to the app.",
                "Master Link", MessageBoxButton.OK, MessageBoxImage.Error);
            args.Handled = true;
        };

        try
        {
            Database.Initialize();
        }
        catch (Exception ex)
        {
            MessageBox.Show(
                $"Could not open or create the local database:\n{Database.DbPath}\n\n{ex.Message}",
                "Master Link - Startup Error",
                MessageBoxButton.OK,
                MessageBoxImage.Error);
            Shutdown(-1);
            return;
        }

        var mainWindow = new MainWindow();
        MainWindow = mainWindow;
        mainWindow.Show();

        Navigation.NavigateTo(new BoardsListViewModel());
    }
}
