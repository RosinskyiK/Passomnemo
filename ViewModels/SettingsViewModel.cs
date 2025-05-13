using Passomnemo.Modules;

namespace Passomnemo.ViewModels
{
    public partial class SettingsViewModel : ViewModelBase
    {
        private readonly MainViewModel _mainViewModel;
        public SettingsEntry SettingEntries => StorageModule.SettingEntries;
        public SettingsViewModel()
        {

        }
        public SettingsViewModel(MainViewModel mainViewModel)
        {
            _mainViewModel = mainViewModel;
        }
    }
}
