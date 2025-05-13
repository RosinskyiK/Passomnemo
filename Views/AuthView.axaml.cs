using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Platform.Storage;
using Passomnemo.Modules;
using Passomnemo.ViewModels;
using System;
using System.Linq;

namespace Passomnemo.Views;

public partial class AuthView : UserControl
{
    private AuthViewModel data => (AuthViewModel)DataContext;
    public AuthView()
    {
        InitializeComponent();
        Initialized += AuthView_Initialized;
    }

    private void AuthView_Initialized(object? sender, EventArgs e)
    {
        var WordGrid = data.GetWordGrid();
        WordGrid.SetValue(Grid.RowProperty, 1);
        MainGrid.Children.Add(WordGrid);
        UpdateLayout();
    }

    private async void OpenFileButton_Clicked(object sender, RoutedEventArgs args)
    {
        // Get top level from the current control. Alternatively, you can use Window reference instead.
        var topLevel = TopLevel.GetTopLevel(this);

        // Start async operation to open the dialog.
        var files = await topLevel.StorageProvider.OpenFilePickerAsync(new FilePickerOpenOptions
        {
            Title = "Open Text File",
            AllowMultiple = false
        });

        if (files.Count >= 1)
        {
            data.FilePath = files[0].Path.AbsolutePath;
            StorageModule.FilePath = data.FilePath;
            data.IsFileSelected = true;           
        }
    }

    protected string mnemonic = "";

    private void CheckMnemonic_Click(object? sender, RoutedEventArgs e)
    {
        var arr = data.Words;
        if (!arr.Any(x => string.IsNullOrEmpty(x.Word)))
        {
            mnemonic = string.Join(" ", arr.Select(x => x.Word).ToArray());
            data.IsMnemonicValid = AuthModule.CheckFileMnemonic(mnemonic);
            CheckMnemonicLabel.Content = data.IsMnemonicValid ? "OK" : "Not valid";
            if (data.IsMnemonicValid) 
                StorageModule.Mnemonic = mnemonic;
        }
        else
        { 
            data.IsMnemonicValid = false;
            CheckMnemonicLabel.Content = "All fields must be filled";
        }
    }

    private void Enter_Click(object? sender, RoutedEventArgs e)
    {
        var isLoaded = StorageModule.LoadData(PasswordBox.Text);
        if (isLoaded)
        {
            AuthModule.Save(PasswordBox.Text);
            data.GoToPasswordListPage();
        }
        else
            CheckPasswordLabel.Content = "Not valid";
    }

    private void Paste_Click(object? sender, RoutedEventArgs e)
    {
        var text = TopLevel.GetTopLevel((Window)VisualRoot)?.Clipboard.GetTextAsync().Result;
        if (text != null)
        {
            int n = 0;
            var words = text.Split([' '], StringSplitOptions.RemoveEmptyEntries);
            string[] dataWords = new string[24];
            dataWords = data.Words.Select(x => x.Word).ToArray();
            for (int i = 0; i < 24; i++)
            {
                if (string.IsNullOrEmpty(dataWords[i]))
                {
                    data.Words[i].Word = words[n];
                    n++;
                }
            }
        }
    }
    private void Clear_Click(object? sender, RoutedEventArgs e)
    {
        data.IsMnemonicValid = false;
        mnemonic = "";
        for (int i = 0; i < 24; i++)
            data.Words[i].Word = "";
    }
}