using CommunityToolkit.Mvvm.ComponentModel;
using ReactiveUI;
using System.Reactive.Linq;
using CommunityToolkit.Mvvm.Input;
using System.Threading.Tasks;

namespace Passomnemo.ViewModels
{
    public partial class PasswordDialogViewModel : DialogViewModel
    {
        [ObservableProperty]
        private string _password;
        [ObservableProperty]
        private string _errorMessage;
        public Interaction<string, string?> CloseDialog { get; } = new();
        public IAsyncRelayCommand OkCommand { get; }
        public IAsyncRelayCommand CancelCommand { get; }

        public PasswordDialogViewModel()
        {
            OkCommand = new AsyncRelayCommand(ExecuteOkCommandAsync);
            CancelCommand = new AsyncRelayCommand(ExecuteCancelCommandAsync);
        }

        private async Task ExecuteOkCommandAsync()
        {
            await CloseDialog.Handle(Password);
        }

        private async Task ExecuteCancelCommandAsync()
        {
            await CloseDialog.Handle("");
        }
    }
}
