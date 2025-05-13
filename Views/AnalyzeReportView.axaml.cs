using Avalonia.Controls;
using Passomnemo.Modules;
using Passomnemo.ViewModels;

namespace Passomnemo.Views;

public partial class AnalyzeReportView : UserControl
{
    AnalyzeReportViewModel data => (AnalyzeReportViewModel)DataContext;
    public AnalyzeReportView()
    {
        InitializeComponent();
    }
    private async void Weak_Change_Click(object? sender, Avalonia.Interactivity.RoutedEventArgs e)
    {
        await data.ShowDialogAsync(data.SelectedWeakItem.ServiceName);
    }
    private async void Duplicate_Change_Click(object? sender, Avalonia.Interactivity.RoutedEventArgs e)
    {
        await data.ShowDialogAsync(data.SelectedDuplicateItem.ServiceName);
    }
    private async void Expired_Change_Click(object? sender, Avalonia.Interactivity.RoutedEventArgs e)
    {
        await data.ShowDialogAsync(data.SelectedExpiredItem.ServiceName);
    }
    private void Weak_Remove_Click(object? sender, Avalonia.Interactivity.RoutedEventArgs e)
    {
        StorageModule.RemoveAccountEntry(data.SelectedWeakItem.ServiceName);
    }
    private void Duplicate_Remove_Click(object? sender, Avalonia.Interactivity.RoutedEventArgs e)
    {
        StorageModule.RemoveAccountEntry(data.SelectedDuplicateItem.ServiceName);
    }
    private void Expired_Remove_Click(object? sender, Avalonia.Interactivity.RoutedEventArgs e)
    {
        StorageModule.RemoveAccountEntry(data.SelectedExpiredItem.ServiceName);
    }
}