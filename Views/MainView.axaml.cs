using Avalonia;
using Avalonia.Controls;
using Passomnemo.Modules;
using Passomnemo.ViewModels;
using System;
using System.Linq;

namespace Passomnemo.Views
{
    public partial class MainView : Window
    {
        MainViewModel data => (MainViewModel)DataContext;
        public MainView(bool loadXaml = true, bool attachDevTools = true)
        {
            InitializeComponent();
#if DEBUG
            if (attachDevTools)
            {
                this.AttachDevTools();
            }
#endif
        }

        private void Save_Click(object? sender, Avalonia.Interactivity.RoutedEventArgs e)
        {
            data.ShowSaveDialogAsync();
        }

        private void Analize_Click(object? sender, Avalonia.Interactivity.RoutedEventArgs e)
        {
            data.GoToAnalyzeReportPage();
        }

        private void Home_Click(object? sender, Avalonia.Interactivity.RoutedEventArgs e)
        {
            if ((data.CurrentPage.GetType() != typeof(StartViewModel)) && 
                (data.CurrentPage.GetType() != typeof(PasswordListViewModel)))
            { 
                Type[] start = [typeof(AuthViewModel), typeof(RegisterViewModel), typeof(FinishRegisterViewModel)];
                if (start.Any(x => x == data.CurrentPage.GetType()))
                { 
                    data.GoToStartPage();
                    StorageModule.FlushAppData();
                    return;
                }
                if (!start.Any(x => x == data.CurrentPage.GetType()))
                { 
                    data.GoToPasswordListPage();
                    return;
                }
            }
        }

        private void Settings_Click(object? sender, Avalonia.Interactivity.RoutedEventArgs e)
        {
            data.GoToSettingsPage();
        }

        private async void Door_Click(object? sender, Avalonia.Interactivity.RoutedEventArgs e)
        {
            string confirmationMessage = "Ви впевнені, що хочете вийти зі сховища?";
            if (!data.ChangesSaved)
                confirmationMessage += "\nУ вас є незбережені дані!";
            bool isUserSure = await data.ShowConfirmDialogAsync("Вийти", confirmationMessage);
            if (!isUserSure)
                return;
            data.GoToStartPage();
            StorageModule.FlushAppData();
            data.IsLoggedIn = false;
            data.IssueCount = 0;
            data.ChangesSaved = true;
            AuthModule.InvalidateSession();
        }
    }
}