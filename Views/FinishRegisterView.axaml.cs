using Avalonia.Controls;
using Passomnemo.Modules;
using Passomnemo.ViewModels;
using System;

namespace Passomnemo.Views;

public partial class FinishRegisterView : UserControl
{
    FinishRegisterViewModel data => (FinishRegisterViewModel)DataContext;
    public FinishRegisterView()
    {
        InitializeComponent();
        Initialized += AuthView_Initialized;
    }

    private void AuthView_Initialized(object? sender, EventArgs e)
    {
        int first = RandomProvider.GetRandomInt(0, 24);
        int second = RandomProvider.GetRandomInt(0, 24);
        while (first == second)
            second = RandomProvider.GetRandomInt(0, 24);

        data.FirstWord = data._registerViewModel.Words[first].Word;
        data.SecondWord = data._registerViewModel.Words[second].Word;
        fWordId.Content = first + 1;
        sWordId.Content = second + 1;
        
    }
    private void Generate_Click(object? sender, Avalonia.Interactivity.RoutedEventArgs e)
    {
        var password = PassGeneratorModule.GenerateDiceware(data.DicewareWordCount);
        while (password.Length < 16)
            password = PassGeneratorModule.GenerateDiceware(data.DicewareWordCount);
        data.Password = password;
    }

    private void Create_Click(object? sender, Avalonia.Interactivity.RoutedEventArgs e)
    {
        StorageModule.CreateVaultFile();
        string[] words = new string[24];
        for (int i = 0; i < 24; i++)
            words[i] = data._registerViewModel.Words[i].Word;

        StorageModule.Mnemonic = string.Join(" ", words);
        StorageModule.SettingEntries.PasswordHash = EncryptionModule.Hash(data.Password);
        StorageModule.SaveData(data.Password);
        AuthModule.Save(data.Password);
        data.GoToPasswordListPage();
    }
}