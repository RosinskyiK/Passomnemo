using CommunityToolkit.Mvvm.ComponentModel;

namespace Passomnemo.ViewModels
{
    public partial class DialogViewModel : ViewModelBase
    {
        [ObservableProperty] private string _title;
    }
}
