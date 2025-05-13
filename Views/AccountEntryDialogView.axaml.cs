using Avalonia.Controls;
using Passomnemo.Modules;
using Passomnemo.ViewModels;
using System.Linq;

namespace Passomnemo.Views;

public partial class AccountEntryDialogView : Window
{
    AccountEntryDialogViewModel data => (AccountEntryDialogViewModel)DataContext;
    public AccountEntryDialogView()
    {
        InitializeComponent();
    }

    public AccountEntryDialogView(AccountEntry entry, bool isEditMode)
    {
        InitializeComponent();
        DataContext = new AccountEntryDialogViewModel(entry, isEditMode);
    }

    private void AddTag_Click(object? sender, Avalonia.Interactivity.RoutedEventArgs e)
    {
        var tags = data.UpdatedAccountEntry.Tags;
        if (!tags.Any(x => x == tagbox.Text))
            data.UpdatedAccountEntry.Tags.Add(tagbox.Text);
    }

    private void Generate_Click(object? sender, Avalonia.Interactivity.RoutedEventArgs e)
    {
        var isDiceware = data.GenDiceware;
        var password = "";
        if (isDiceware)
            password = PassGeneratorModule.GenerateDiceware(data.DicewareWordCount);
        else
            password = PassGeneratorModule.GenerateSequential(data.ShufflePasswordLength);
        data.UpdatedAccountEntry.Password = password;
    }
}