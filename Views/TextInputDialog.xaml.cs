using System.Windows;
using System.Windows.Input;

namespace MasterLink.Desktop.Views;

public partial class TextInputDialog : Window
{
    public string Value => ValueBox.Text;

    public TextInputDialog(string title, string prompt, string initialValue)
    {
        InitializeComponent();
        Title = title;
        PromptText.Text = prompt;
        ValueBox.Text = initialValue;
        Loaded += (_, _) => { ValueBox.Focus(); ValueBox.SelectAll(); };
    }

    private void Ok_Click(object sender, RoutedEventArgs e)
    {
        DialogResult = true;
    }

    private void Cancel_Click(object sender, RoutedEventArgs e)
    {
        DialogResult = false;
    }

    private void ValueBox_KeyDown(object sender, KeyEventArgs e)
    {
        if (e.Key == Key.Escape) DialogResult = false;
    }

    public static string? Prompt(Window owner, string title, string prompt, string initialValue)
    {
        var dialog = new TextInputDialog(title, prompt, initialValue) { Owner = owner };
        return dialog.ShowDialog() == true ? dialog.Value : null;
    }
}
