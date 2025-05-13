using Avalonia.Controls.ApplicationLifetimes;
using CommunityToolkit.Mvvm.ComponentModel;
using Passomnemo.Modules;
using Passomnemo.Views;
using System;
using System.Collections.ObjectModel;
using System.Threading.Tasks;

namespace Passomnemo.ViewModels
{
    public partial class AnalyzeReportViewModel : ViewModelBase
    {
        private readonly MainViewModel _mainViewModel;
        public ObservableCollection<AccountEntry> AccountEntries => StorageModule.AccountEntries;
        [ObservableProperty]
        public PasswordSecurityReport _report;
        [ObservableProperty]
        private AccountEntry _selectedWeakItem;
        [ObservableProperty]
        private AccountEntry _selectedDuplicateItem;
        [ObservableProperty]
        private AccountEntry _selectedExpiredItem;
        public AnalyzeReportViewModel()
        {

        }
        public AnalyzeReportViewModel(MainViewModel mainViewModel)
        {
            _mainViewModel = mainViewModel;
            Report = PassAnalyzerModule.AnalyzePasswords(AccountEntries);
            _mainViewModel.IssueCount = Report.IssueCount;
        }
        public async Task ShowDialogAsync(string name)
        {
            var entry = new AccountEntry(StorageModule.GetAccountEntry(name));
            var dialog = new AccountEntryDialogView(entry, true);
            var dialogViewModel = new AccountEntryDialogViewModel(entry, true);
            dialog.DataContext = dialogViewModel;

            AccountEntry? result = null;

            dialogViewModel.CloseDialog.RegisterHandler(interaction =>
            {
                result = interaction.Input;
                dialog.Close();
                interaction.SetOutput(result);
            });

            await dialog.ShowDialog(App.Current.ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop ? desktop.MainWindow : null);

            if (result != null)
            {
                result.LastModified = DateTime.Now;
                StorageModule.UpdateAccountEntry(result, name);
                Report = PassAnalyzerModule.AnalyzePasswords(AccountEntries);
            }
        }
    }
}
