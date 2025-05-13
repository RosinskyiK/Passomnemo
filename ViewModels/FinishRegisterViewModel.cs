using Avalonia.Data.Converters;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System;
using System.Collections.Generic;
using System.Globalization;

namespace Passomnemo.ViewModels
{
    public partial class FinishRegisterViewModel : ViewModelBase
    {
        private readonly MainViewModel _mainViewModel;
        public readonly RegisterViewModel _registerViewModel;
       
        [ObservableProperty]
        public string _firstWord;
        [ObservableProperty]
        public string _firstEnteredWord;
        [ObservableProperty]
        public string _secondWord;
        [ObservableProperty]
        public string _secondEnteredWord;
        [ObservableProperty]
        protected string _password;
        [ObservableProperty]
        private int _dicewareWordCount;
        public FinishRegisterViewModel()
        {
            DicewareWordCount = 4;
        }
        public FinishRegisterViewModel(MainViewModel mainViewModel, RegisterViewModel registerViewModel)
        {
            DicewareWordCount = 4;
            _mainViewModel = mainViewModel;
            _registerViewModel = registerViewModel;
        }
        [RelayCommand]
        public void GoToPasswordListPage()
        { 
            _mainViewModel.IsLoggedIn = true;
            _mainViewModel.GoToPasswordListPage();
        } 
    }
    public class WordCorrectnessConverter : IMultiValueConverter
    {
        public object? Convert(IList<object?> values, Type targetType, object? parameter, CultureInfo culture)
        {
            return (values[0] as string == values[1] as string) && (values[2] as string == values[3] as string);
        }
    }
    public class PasswordLengthConverter : IValueConverter
    {
        public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
        {
            if (value != null)
                return (value as string).Length >= 16;
            else
                return false;
        }

        public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
