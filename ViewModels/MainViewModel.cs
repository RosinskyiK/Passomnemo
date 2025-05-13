using Avalonia.Controls.ApplicationLifetimes;
using CommunityToolkit.Mvvm.ComponentModel;
using Passomnemo.Modules;
using System;
using System.Linq;
using System.Reactive.Linq;
using System.Threading.Tasks;
using System.Collections.Specialized;
using System.ComponentModel;
using Passomnemo.Views;

namespace Passomnemo.ViewModels
{
    public partial class MainViewModel : ViewModelBase
    {
        [ObservableProperty]
        private bool _isLoggedIn;
        [ObservableProperty]
        private ViewModelBase? _currentPage;
        [ObservableProperty]
        private int _issueCount;
        [ObservableProperty]
        private bool _changesSaved;

        public MainViewModel() 
        {
            GoToStartPage();  
        }
        private void AccountEntries_CollectionChanged(object? sender, NotifyCollectionChangedEventArgs e)
        {
            ChangesSaved = false;
        }
        private void SettingEntry_PropertyChanged(object? sender, PropertyChangedEventArgs e)
        {
            ChangesSaved = false;
        }
        public void GoToStartPage()
        {
            CurrentPage = new StartViewModel(this);
        }
        public async Task GoToAuthPage()
        {
            var session = AuthModule.Load();
            if (session.isValid && !session.isExpired)
            { 
                bool answer = await ShowConfirmDialogAsync("Активна сесія", "У вас є збережена минула сесія. Продовжити роботу з нею?");
                while (answer)
                { 
                    bool success = await ShowSessionDialogAsync();
                    if (!success)
                        answer = await ShowConfirmDialogAsync("Помилка.", "Спробувати ще раз?");
                    else
                        break;
                }
            }
            else
                CurrentPage = new AuthViewModel(this);
        }
        public void GoToPasswordListPage()
        {
            CurrentPage = new PasswordListViewModel(this);
            StorageModule.AccountEntries.CollectionChanged += AccountEntries_CollectionChanged;
            StorageModule.SettingEntries.PropertyChanged += SettingEntry_PropertyChanged;
            ChangesSaved = true;
        }
        public void GoToRegisterPage()
        {
            CurrentPage = new RegisterViewModel(this);
        }
        public void GoToFinishRegisterPage(RegisterViewModel register)
        {
            CurrentPage = new FinishRegisterViewModel(this, register);
        }
        public void GoToSettingsPage()
        {
            CurrentPage = new SettingsViewModel(this);
        }
        public void GoToAnalyzeReportPage()
        {
            CurrentPage = new AnalyzeReportViewModel(this);
        }

        public async Task<bool> ShowConfirmDialogAsync(string title, string message, bool onlyOkButton = false)
        {
            var dialog = new ConfirmDialogView();
            var dialogViewModel = new ConfirmDialogViewModel(title, message, onlyOkButton);
            dialog.DataContext = dialogViewModel;

            bool? result = null;

            dialogViewModel.CloseDialog.RegisterHandler(interaction =>
            {
                result = interaction.Input;
                dialog.Close();
                interaction.SetOutput(result);
            });
            await dialog.ShowDialog(App.Current.ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop ? desktop.MainWindow : null);

            return result ?? false;
        }

        public async void ShowSaveDialogAsync()
        {
            var dialog = new PasswordDialogView();
            var dialogViewModel = new PasswordDialogViewModel();
            dialog.DataContext = dialogViewModel;

            string? result = null;

            dialogViewModel.CloseDialog.RegisterHandler(interaction =>
            {
                result = interaction.Input;
                interaction.SetOutput(result);
                dialog.Close();
            });

            await dialog.ShowDialog(App.Current.ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop ? desktop.MainWindow : null);

            if (!string.IsNullOrEmpty(result))
                if (IsLoggedIn && StorageModule.SettingEntries.PasswordHash.SequenceEqual(EncryptionModule.Hash(result)))
                { 
                    ChangesSaved = true;
                    await ShowConfirmDialogAsync("Збереження", "Дані було успішно збережено!", true);
                    StorageModule.SaveData(result);
                    return;
                }
            await ShowConfirmDialogAsync("Збереження", "Неправильний пароль", true);
        }

        public async Task<bool> ShowSessionDialogAsync()
        {
            var dialog = new PasswordDialogView();
            var dialogViewModel = new PasswordDialogViewModel();
            dialog.DataContext = dialogViewModel;

            string? result = null;

            dialogViewModel.CloseDialog.RegisterHandler(interaction =>
            {
                result = interaction.Input;
                dialog.Close();
                interaction.SetOutput(result);
            });

            await dialog.ShowDialog(App.Current.ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop ? desktop.MainWindow : null);

            if (!string.IsNullOrEmpty(result))
                if (!IsLoggedIn)
                {
                    var session = AuthModule.Load(result);
                    if (session.isValid && !session.isExpired && !string.IsNullOrEmpty(session.filePath) && !string.IsNullOrEmpty(session.mnemonic))
                    {
                        StorageModule.FilePath = session.filePath;
                        StorageModule.Mnemonic = session.mnemonic;
                        if (StorageModule.LoadData(result))
                        {
                            IsLoggedIn = true;
                            GoToPasswordListPage();
                            return true;
                        }
                    }
                    if(!session.isValid)
                        await ShowConfirmDialogAsync("Помилка", "Сесія була неправильно записана");
                    if (session.isExpired)
                        await ShowConfirmDialogAsync("Помилка", "Термін дії сесії вичерпано");
                    return false;
                }
            return false;
        }
    }
}
