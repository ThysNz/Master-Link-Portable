using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using MasterLink.Desktop.Data;

namespace MasterLink.Desktop.ViewModels;

// One row of a block's Commands sub-topic: Seq #, Command, Purpose.
public partial class CommandRowViewModel : ObservableObject
{
    public int Id { get; }
    private readonly Action<CommandRowViewModel> requestDelete;
    private bool loaded;

    [ObservableProperty] private int seq;
    [ObservableProperty] private string command = "";
    [ObservableProperty] private string purpose = "";

    public CommandRowViewModel(CommandEntry row, Action<CommandRowViewModel> requestDelete)
    {
        Id = row.Id;
        this.requestDelete = requestDelete;
        command = row.Command;
        purpose = row.Purpose;
        loaded = true;
    }

    partial void OnCommandChanged(string value) => Save("Command", value);
    partial void OnPurposeChanged(string value) => Save("Purpose", value);

    private void Save(string column, string value)
    {
        if (!loaded) return;
        using var c = Database.CreateConnection();
        BlockRepository.UpdateCommand(c, Id, column, value);
    }

    [RelayCommand] private void CopyCommand() => Clip.Copy(Command);
    [RelayCommand] private void Delete() => requestDelete(this);
}
