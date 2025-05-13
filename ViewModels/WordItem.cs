using CommunityToolkit.Mvvm.ComponentModel;

namespace Passomnemo.ViewModels
{
    public partial class WordItem : ObservableObject
    {
        [ObservableProperty]
        private string word;
    }
}
