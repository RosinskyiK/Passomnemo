using Avalonia.Controls.ApplicationLifetimes;
using CommunityToolkit.Mvvm.ComponentModel;
using Passomnemo.Modules;
using Passomnemo.Views;
using ReactiveUI;
using System;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using System.Reactive;
using System.Reactive.Linq;
using System.Threading.Tasks;

namespace Passomnemo.ViewModels
{
    public partial class PasswordListViewModel : ViewModelBase
    {
        private readonly MainViewModel _mainViewModel;
        [ObservableProperty]
        private AccountEntry _selectedItem;
        [ObservableProperty]
        private int _searchIndex;
        [ObservableProperty]
        private int _filterIndex;
        [ObservableProperty]
        private string _searchText;
        [ObservableProperty]
        public ObservableCollection<AccountEntry> _accountEntries;
        public PasswordSecurityReport Report => PassAnalyzerModule.AnalyzePasswords(AccountEntries);
        public ReactiveCommand<Unit, Unit> ShowBackupDialogCommand { get; }
        public PasswordListViewModel()
        {
            
        }
        public PasswordListViewModel(MainViewModel mainViewModel)
        {
            _mainViewModel = mainViewModel;
            SearchAccountEntries();
            _mainViewModel.IssueCount = Report.IssueCount;
            AccountEntries.CollectionChanged += (s, e) =>
            {
                _mainViewModel.IssueCount = Report.IssueCount;
            };
            ShowBackupDialogCommand = ReactiveCommand.CreateFromTask(ShowBackupDialogAsync);
            ShowBackupDialogCommand.Execute();
        }

        public void SearchAccountEntries()
        {
            ObservableCollection<AccountEntry> list = StorageModule.AccountEntries;
            switch (SearchIndex)
            {
                case 0:
                    SortAccountEntriesByFilter(list);
                    break;
                case 1:
                    if(!string.IsNullOrEmpty(SearchText)) list = new(AccountEntries.Where(x => x.Tags.Any(y => y.Contains(SearchText))));
                    SortAccountEntriesByFilter(list);
                    break;
                case 2:
                    if (!string.IsNullOrEmpty(SearchText)) list = new(AccountEntries.Where(x => x.ServiceName.Contains(SearchText)));
                    SortAccountEntriesByFilter(list);
                    break;
                case 3:
                    if (!string.IsNullOrEmpty(SearchText)) list = new(AccountEntries.Where(x => x.Login.Contains(SearchText)));
                    SortAccountEntriesByFilter(list);
                    break;
            }
            foreach (var entry in list)
                Debug.WriteLine(entry);
        }
        private void SortAccountEntriesByFilter(ObservableCollection<AccountEntry> entries)
        {
            switch (FilterIndex / 2)
            {
                case 0:
                    if (FilterIndex % 2 == 0) AccountEntries = new(entries.OrderBy(x => x.LastModified));
                    if (FilterIndex % 2 == 1) AccountEntries = new(entries.OrderByDescending(x => x.LastModified));
                    break;
                case 1:
                    if (FilterIndex % 2 == 0) AccountEntries = new(entries.OrderBy(x => x.Tags.FirstOrDefault()));
                    if (FilterIndex % 2 == 1) AccountEntries = new(entries.OrderByDescending(x => x.Tags.FirstOrDefault()));
                    break;
                case 2:
                    if (FilterIndex % 2 == 0) AccountEntries = new(entries.OrderBy(x => x.ServiceName));
                    if (FilterIndex % 2 == 1) AccountEntries = new(entries.OrderByDescending(x => x.ServiceName));
                    break;
                case 3:
                    if (FilterIndex % 2 == 0) AccountEntries = new(entries.OrderBy(x => x.Login));
                    if (FilterIndex % 2 == 1) AccountEntries = new(entries.OrderByDescending(x => x.Login));
                    break;
            }
        }

        public async Task ShowBackupDialogAsync()
        {
            DateTime expectedReminderDate = DateTime.Now.AddDays(StorageModule.SettingEntries.BackupRemindTime);
            if (StorageModule.SettingEntries.LastBackupDate > expectedReminderDate)
            {
                bool agreeToBackup = false;
                var timespan = StorageModule.SettingEntries.LastBackupDate - expectedReminderDate;
                agreeToBackup = await _mainViewModel.ShowConfirmDialogAsync("Резервне копіювання",
                    $"Час минулої резервної копії:\n{timespan.Days} днів {timespan.Hours} годин.\nБажаєте зробити її зараз?");
                if (agreeToBackup)
                {
                    string[] pathStrings = StorageModule.FilePath.Split('/');
                    string path = string.Join('\\', pathStrings.Take(pathStrings.Length - 1));
                    Process.Start("explorer.exe", @$"{path}");
                    StorageModule.SettingEntries.LastBackupDate = DateTime.Now;
                }
            }
        }
        public async Task ShowDialogAsync(bool isEditMode)
        {
            var entry = isEditMode ? new AccountEntry(StorageModule.GetAccountEntry(SelectedItem.ServiceName)) : new AccountEntry();
            var dialog = new AccountEntryDialogView(entry, isEditMode);
            var dialogViewModel = new AccountEntryDialogViewModel(entry, isEditMode);
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
                if (isEditMode)
                {
                    StorageModule.UpdateAccountEntry(result, SelectedItem.ServiceName);
                    SearchAccountEntries();
                }
                else
                {
                    StorageModule.AddAccountEntry(result);
                    SearchAccountEntries();
                }
            }
        }
    }
}
