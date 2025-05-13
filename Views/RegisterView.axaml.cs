using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Platform.Storage;
using Passomnemo.Modules;
using Passomnemo.ViewModels;
using System;
using System.Diagnostics;
using System.IO;
using System.Linq;

namespace Passomnemo.Views;

public partial class RegisterView : UserControl
{
    RegisterViewModel data => (RegisterViewModel)DataContext;
    public RegisterView()
    {
        InitializeComponent();
        Initialized += RegisterView_Initialized;
    }

    private void RegisterView_Initialized(object? sender, EventArgs e)
    {
        var WordGrid = data.GetWordGrid();
        WordGrid.SetValue(Grid.RowProperty, 2);
        MainGrid.Children.Add(WordGrid);
        UpdateLayout();
    }

    private async void OpenFolderButton_Clicked(object sender, RoutedEventArgs args)
    {
        var topLevel = TopLevel.GetTopLevel(this);

        var folder = await topLevel.StorageProvider.OpenFolderPickerAsync(new FolderPickerOpenOptions
        {
            Title = "Select Folder",
            AllowMultiple = false
        });

        if (folder.Count >= 1)
        {
            data.FolderPath = folder[0].Path.AbsolutePath;
            data.IsFolderSelected = true;
            Debug.WriteLine($"Selected folder: {folder[0].Path.AbsolutePath}");
            GenerateMnemonic();
        }
    }

    private async void GenerateMnemonic()
    {
        var text = await AuthModule.CreateMnemonicPhrase();
        Debug.WriteLine($"Generated mnemonic: {text}");
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
        WarningLabel.Content = "Будь ласка, запевніться щоб ваша мнемонічна фраза була надійно збережена";
    }

    private async void Continue_Click(object? sender, Avalonia.Interactivity.RoutedEventArgs e)
    {
        var filePath = $"{data.FolderPath}{data.FileName}.dat";
        if (File.Exists(filePath))
        { 
            bool isUserSure = await data.DoOverwrite();
            if (!isUserSure)
                return;
        }
        StorageModule.FilePath = filePath;
        data.GoToFinishRegisterPage();
    }
}