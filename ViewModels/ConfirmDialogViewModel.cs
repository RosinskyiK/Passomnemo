using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using ReactiveUI;
using System.Reactive.Linq;
using System.Threading.Tasks;

namespace Passomnemo.ViewModels
{
    public partial class ConfirmDialogViewModel : DialogViewModel
    {
        [ObservableProperty]
        private string _message;
        [ObservableProperty]
        private bool _onlyOkButton;
        public Interaction<bool, bool?> CloseDialog { get; } = new();
        public IAsyncRelayCommand OkCommand { get; }
        public IAsyncRelayCommand CancelCommand { get; }
        public ConfirmDialogViewModel()
        {
            Message = "Помилка";
            OnlyOkButton = false;
            OkCommand = new AsyncRelayCommand(ExecuteOkCommandAsync);
            CancelCommand = new AsyncRelayCommand(ExecuteCancelCommandAsync);
        }
        public ConfirmDialogViewModel(string title, string message, bool onlyOkButton)
        {
            Title = title;
            Message = message;
            OnlyOkButton = onlyOkButton;
            OkCommand = new AsyncRelayCommand(ExecuteOkCommandAsync);
            CancelCommand = new AsyncRelayCommand(ExecuteCancelCommandAsync);
        }
        private async Task ExecuteOkCommandAsync()
        {
            await CloseDialog.Handle(true);
        }

        private async Task ExecuteCancelCommandAsync()
        {
            await CloseDialog.Handle(false);
        }
    }
}
