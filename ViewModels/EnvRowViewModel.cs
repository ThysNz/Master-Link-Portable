using System.Collections.ObjectModel;
using System.Diagnostics;
using System.IO;
using System.Windows;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using MasterLink.Desktop.Data;

namespace MasterLink.Desktop.ViewModels;

// One row of a block's Environment table: Seq #, Application, Path, User,
// Password. Seq is just the row's 1-based position (set by the block).
// Each row owns a collapsible list of Commands (Seq #, Command, Purpose).
public partial class EnvRowViewModel : ObservableObject
{
    public int Id { get; }
    private readonly Action<EnvRowViewModel> requestDelete;
    private bool loaded;

    [ObservableProperty] private int seq;
    [ObservableProperty] private string application = "";
    [ObservableProperty] private string path = "";
    [ObservableProperty] private string userName = "";
    [ObservableProperty] private string password = "";
    [ObservableProperty] private bool showPassword;
    [ObservableProperty] private bool commandsExpanded = true;
    [ObservableProperty] private ObservableCollection<CommandRowViewModel> commands = new();

    public EnvRowViewModel(EnvEntry row, Action<EnvRowViewModel> requestDelete)
    {
        Id = row.Id;
        this.requestDelete = requestDelete;
        application = row.Application;
        path = row.Path;
        userName = row.UserName;
        password = row.Password;
        Commands = new ObservableCollection<CommandRowViewModel>(row.Commands.Select(x => new CommandRowViewModel(x, RemoveCommandRow)));
        RenumberCommands();
        loaded = true;
    }

    private void RenumberCommands() { for (var i = 0; i < Commands.Count; i++) Commands[i].Seq = i + 1; }

    [RelayCommand] private void ToggleCommands() => CommandsExpanded = !CommandsExpanded;

    [RelayCommand]
    private void AddCommand()
    {
        using var c = Database.CreateConnection();
        Commands.Add(new CommandRowViewModel(BlockRepository.AddCommand(c, Id), RemoveCommandRow));
        RenumberCommands();
        CommandsExpanded = true;
    }

    private void RemoveCommandRow(CommandRowViewModel vm)
    {
        using var c = Database.CreateConnection();
        BlockRepository.DeleteCommand(c, vm.Id);
        Commands.Remove(vm);
        RenumberCommands();
    }

    partial void OnApplicationChanged(string value) => Save("Application", value);
    partial void OnPathChanged(string value) => Save("Path", value);
    partial void OnUserNameChanged(string value) => Save("UserName", value);
    partial void OnPasswordChanged(string value) => Save("Password", value);

    private void Save(string column, string value)
    {
        if (!loaded) return;
        using var c = Database.CreateConnection();
        BlockRepository.UpdateEnv(c, Id, column, value);
    }

    [RelayCommand] private void TogglePassword() => ShowPassword = !ShowPassword;
    [RelayCommand] private void CopyUser() => Clip.Copy(UserName);
    [RelayCommand] private void CopyPassword() => Clip.Copy(Password);

    // Hands the text to the shell so URLs open in the default browser and
    // file / UNC paths open in Explorer. A bare "host.tld" gets https://.
    [RelayCommand]
    private void Open()
    {
        var target = Path.Trim().Trim('"');
        if (target.Length == 0) return;

        if (!Uri.TryCreate(target, UriKind.Absolute, out _) && !System.IO.Path.IsPathRooted(target)
            && !target.Contains(' ') && target.Contains('.'))
        {
            target = "https://" + target;
        }

        try
        {
            Process.Start(new ProcessStartInfo(target) { UseShellExecute = true });
        }
        catch (Exception ex)
        {
            MessageBox.Show($"Could not open:\n{target}\n\n{ex.Message}", "Master Link",
                MessageBoxButton.OK, MessageBoxImage.Warning);
        }
    }

    [RelayCommand] private void Delete() => requestDelete(this);
}

internal static class Clip
{
    public static void Copy(string text)
    {
        if (text.Length == 0) return;
        try { Clipboard.SetText(text); }
        catch (System.Runtime.InteropServices.ExternalException) { /* clipboard briefly locked by another app */ }
    }
}
