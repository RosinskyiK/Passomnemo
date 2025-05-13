using Avalonia.Controls;
using Passomnemo.Modules;
using Passomnemo.ViewModels;

namespace Passomnemo.Views;

public partial class PasswordListView : UserControl
{
    PasswordListViewModel data => (PasswordListViewModel)DataContext;
    public PasswordListView()
    {
        InitializeComponent();
    }
    private async void Add_Click(object? sender, Avalonia.Interactivity.RoutedEventArgs e)
    {
        await data.ShowDialogAsync(false);
    }
    private async void Change_Click(object? sender, Avalonia.Interactivity.RoutedEventArgs e)
    {
        await data.ShowDialogAsync(true);
    }
    private void Remove_Click(object? sender, Avalonia.Interactivity.RoutedEventArgs e)
    {
        StorageModule.RemoveAccountEntry(data.SelectedItem.ServiceName);
        data.SearchAccountEntries();
    }

    private void Search_Click(object? sender, Avalonia.Interactivity.RoutedEventArgs e)
    {
        data.SearchAccountEntries();
    }
}