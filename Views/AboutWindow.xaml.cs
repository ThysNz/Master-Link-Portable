using System.Windows;
using MasterLink.Desktop.Data;
using MasterLink.Desktop.Services;

namespace MasterLink.Desktop.Views;

public partial class AboutWindow : Window
{
    public AboutWindow()
    {
        InitializeComponent();
        VersionText.Text = $"Version {AppInfo.Version}";
        DbText.Text = $"Data file:\n{Database.DbPath}";
    }

    private void Close_Click(object sender, RoutedEventArgs e) => Close();
}
