using System.Diagnostics;
using System.IO;
using System.Windows;
using System.Windows.Controls.Primitives;
using Microsoft.Data.Sqlite;
using Microsoft.Win32;
using MasterLink.Desktop.Data;
using MasterLink.Desktop.ViewModels;
using MasterLink.Desktop.Views;

namespace MasterLink.Desktop;

public partial class MainWindow : Window
{
    public MainWindow()
    {
        InitializeComponent();
        DataContext = App.Navigation;
    }

    // Opening via a Click handler bypasses ContextMenuService's automatic
    // placement wiring, so PlacementTarget is set explicitly each time.
    private void NavMenuButton_Click(object sender, RoutedEventArgs e)
    {
        NavMenuContextMenu.PlacementTarget = NavMenuButton;
        NavMenuContextMenu.Placement = PlacementMode.Custom;
        NavMenuContextMenu.IsOpen = true;
    }

    // The button sits at the top-right corner, so anchor the menu's right edge
    // to the button's right edge to keep it inside the window.
    private CustomPopupPlacement[] NavMenuContextMenu_PlacementCallback(Size popupSize, Size targetSize, Point offset)
    {
        var point = new Point(targetSize.Width - popupSize.Width, targetSize.Height);
        return new[] { new CustomPopupPlacement(point, PopupPrimaryAxis.None) };
    }

    private void GoBoards_Click(object sender, RoutedEventArgs e) =>
        App.Navigation.NavigateTo(new BoardsListViewModel());

    private void GoHelp_Click(object sender, RoutedEventArgs e) =>
        App.Navigation.NavigateTo(new HelpViewModel());

    private void GoAbout_Click(object sender, RoutedEventArgs e) =>
        new AboutWindow { Owner = this }.ShowDialog();

    // Uses SQLite's online backup API rather than a plain file copy, so a
    // backup taken while the app is open is always a consistent snapshot.
    private void BackupDatabase_Click(object sender, RoutedEventArgs e)
    {
        var dialog = new SaveFileDialog
        {
            FileName = $"MasterLink_Backup_{DateTime.Now:yyyy-MM-dd_HHmm}.db",
            Filter = "SQLite database (*.db)|*.db",
        };
        if (dialog.ShowDialog() != true) return;

        using var source = Database.CreateConnection();
        using var destination = new SqliteConnection($"Data Source={dialog.FileName}");
        destination.Open();
        source.BackupDatabase(destination);

        MessageBox.Show($"Database backed up to:\n{dialog.FileName}", "Backup Complete", MessageBoxButton.OK, MessageBoxImage.Information);
    }

    // Overwrites the live db with a chosen backup, then restarts so every
    // already-open page (which cache their data) picks up the new content.
    private void ImportDatabase_Click(object sender, RoutedEventArgs e)
    {
        var dialog = new OpenFileDialog
        {
            Filter = "SQLite database (*.db)|*.db",
        };
        if (dialog.ShowDialog() != true) return;

        if (!LooksLikeMasterLinkDatabase(dialog.FileName))
        {
            MessageBox.Show(
                $"{dialog.FileName}\n\ndoesn't look like a Master Link database (no SystemItem table found). Import cancelled.",
                "Import Database", MessageBoxButton.OK, MessageBoxImage.Error);
            return;
        }

        var confirm = MessageBox.Show(
            "This replaces all current data with the selected file and cannot be undone.\n\n" +
            "If you're not sure, back up the current database first. Continue with the import?",
            "Import Database", MessageBoxButton.YesNo, MessageBoxImage.Warning);
        if (confirm != MessageBoxResult.Yes) return;

        // Pooled native connections can still hold the file open - clear first.
        SqliteConnection.ClearAllPools();
        File.Copy(dialog.FileName, Database.DbPath, overwrite: true);

        MessageBox.Show("Database imported. Master Link will now restart to load it.", "Import Complete", MessageBoxButton.OK, MessageBoxImage.Information);

        Process.Start(Environment.ProcessPath!);
        Application.Current.Shutdown();
    }

    private static bool LooksLikeMasterLinkDatabase(string path)
    {
        using var c = new SqliteConnection($"Data Source={path}");
        c.Open();
        using var cmd = c.CreateCommand();
        cmd.CommandText = "SELECT COUNT(*) FROM sqlite_master WHERE type='table' AND name='SystemItem';";
        return Convert.ToInt32(cmd.ExecuteScalar()) > 0;
    }
}
