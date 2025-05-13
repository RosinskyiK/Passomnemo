using Avalonia.Data.Converters;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Passomnemo.Modules;
using ReactiveUI;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Reactive;
using System.Reactive.Linq;
using System.Threading.Tasks;

namespace Passomnemo.ViewModels
{
    public partial class AccountEntryDialogViewModel : DialogViewModel
    {
        public bool IsEditMode { get; }
        [ObservableProperty]
        public AccountEntry _originalAccountEntry;
        [ObservableProperty]
        public AccountEntry _updatedAccountEntry;
        public Interaction<AccountEntry?, AccountEntry?> CloseDialog { get; } = new();
        public IAsyncRelayCommand OkCommand { get; }
        public IAsyncRelayCommand CancelCommand { get; }
        [ObservableProperty]
        private bool _genDiceware;
        [ObservableProperty]
        private int _dicewareWordCount;
        [ObservableProperty]
        private int _shufflePasswordLength;
        public AccountEntryDialogViewModel()
        {
            
        }
        public AccountEntryDialogViewModel(AccountEntry entry, bool isEditMode)
        {
            OriginalAccountEntry = entry;
            UpdatedAccountEntry = new AccountEntry(entry);
            GenDiceware = true;
            IsEditMode = isEditMode;
            Title = isEditMode ? "Редагувати блок" : "Додати блок";
            DicewareWordCount = 5;
            ShufflePasswordLength = 16;
            OkCommand = new AsyncRelayCommand(ExecuteOkCommandAsync);
            CancelCommand = new AsyncRelayCommand(ExecuteCancelCommandAsync);
        }
        private async Task ExecuteOkCommandAsync()
        {
            await CloseDialog.Handle(UpdatedAccountEntry);
        }

        private async Task ExecuteCancelCommandAsync()
        {
            await CloseDialog.Handle(null);
        }
    }
    public class PasswordStrengthConverter : IMultiValueConverter
    {
        public object? Convert(IList<object?> values, Type targetType, object? parameter, CultureInfo culture)
        {
            var password = values[0] as string;
            return PassAnalyzerModule.EvaluateStrength(password);
        }
    }
    public class AllTextFieldsFilledConverter : IMultiValueConverter
    {
        public object? Convert(IList<object?> values, Type targetType, object? parameter, CultureInfo culture)
        {
            var fields = values.Take(3);
            if (values[3] != null)
                if (values[3].GetType() != typeof(Avalonia.UnsetValueType))
                    return fields.All(v => !string.IsNullOrWhiteSpace(v as string) && (((bool)values[3]) ||
                           StorageModule.AccountEntries.All(e => e.ServiceName != (values[0] as string))));
                else
                    return false;
            else
                return false;
        }
    }
}
