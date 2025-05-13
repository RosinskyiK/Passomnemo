using CommunityToolkit.Mvvm.Input;

namespace Passomnemo.ViewModels
{
    public partial class StartViewModel : ViewModelBase
    {
        private readonly MainViewModel _mainViewModel;
        public StartViewModel()
        {
            
        }
        public StartViewModel(MainViewModel mainViewModel)
        {
            _mainViewModel = mainViewModel;
        }

        [RelayCommand]
        public void GoToStartPage() => _mainViewModel.GoToStartPage();

        [RelayCommand]
        public void GoToAuthPage() => _mainViewModel.GoToAuthPage();

        [RelayCommand]
        public void GoToRegisterPage() => _mainViewModel.GoToRegisterPage();
    }
}
