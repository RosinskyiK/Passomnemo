using Avalonia.Controls;
using Avalonia.Data;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System.Collections.ObjectModel;
using System.Threading.Tasks;

namespace Passomnemo.ViewModels
{
    public partial class RegisterViewModel : ViewModelBase
    {
        private readonly MainViewModel _mainViewModel;
        //input
        [ObservableProperty]
        private string _folderPath;
        [ObservableProperty]
        private string _fileName;
        public ObservableCollection<WordItem> Words { get; } = new();
        //checks
        [ObservableProperty]
        public bool _isFolderSelected;
        public RegisterViewModel()
        {
            FileName = "vault";
        }
        public RegisterViewModel(MainViewModel mainViewModel)
        {
            _mainViewModel = mainViewModel;
            FileName = "vault";
            for (int i = 0; i < 24; i++)
                Words.Add(new WordItem());
        }
        [RelayCommand]
        public void GoToFinishRegisterPage() => _mainViewModel.GoToFinishRegisterPage(this);

        public async Task<bool> DoOverwrite()
        {
            return await _mainViewModel.ShowConfirmDialogAsync("Перезапис", "Файл з цією назвою вже існує. Ви бажаєте його перезаписати?");
        }

        public Grid GetWordGrid()
        {
            int count = 0;
            Grid mainGrid = new();
            mainGrid.ColumnDefinitions.Add(new ColumnDefinition(1, GridUnitType.Star));
            mainGrid.ColumnDefinitions.Add(new ColumnDefinition(1, GridUnitType.Star));
            mainGrid.ColumnDefinitions.Add(new ColumnDefinition(1, GridUnitType.Star));
            mainGrid.ColumnDefinitions.Add(new ColumnDefinition(1, GridUnitType.Star));
            mainGrid.HorizontalAlignment = Avalonia.Layout.HorizontalAlignment.Center;
            mainGrid.Margin = new Avalonia.Thickness(0, 0, 10, 0);

            for (int i = 0; i < 4; i++)
            {
                Grid columnGrid = new();
                columnGrid.RowDefinitions.Add(new RowDefinition(1, GridUnitType.Star));
                columnGrid.RowDefinitions.Add(new RowDefinition(1, GridUnitType.Star));
                columnGrid.RowDefinitions.Add(new RowDefinition(1, GridUnitType.Star));
                columnGrid.RowDefinitions.Add(new RowDefinition(1, GridUnitType.Star));
                columnGrid.RowDefinitions.Add(new RowDefinition(1, GridUnitType.Star));
                columnGrid.RowDefinitions.Add(new RowDefinition(1, GridUnitType.Star));
                columnGrid.Margin = new Avalonia.Thickness(10);

                for (int j = 0; j < 6; j++)
                {
                    count++;
                    Grid rowGrid = new();
                    rowGrid.ColumnDefinitions.Add(new ColumnDefinition(1, GridUnitType.Star));
                    rowGrid.ColumnDefinitions.Add(new ColumnDefinition(1, GridUnitType.Star));

                    Label label = new();
                    label.SetValue(Grid.ColumnProperty, 0);
                    label.Content = count.ToString();
                    label.VerticalAlignment = Avalonia.Layout.VerticalAlignment.Center;
                    TextBox textBox = new();
                    textBox.SetValue(Grid.ColumnProperty, 1);
                    textBox.Width = 100;
                    textBox.Height = 20;
                    textBox.Name = "Word" + count.ToString();
                    textBox.Bind(TextBox.TextProperty, new Binding($"Words[{count - 1}].Word", BindingMode.TwoWay));
                    textBox.Bind(TextBox.IsEnabledProperty, new Binding("IsFolderSelected"));

                    rowGrid.Children.Add(label);
                    rowGrid.Children.Add(textBox);
                    rowGrid.SetValue(Grid.RowProperty, j);
                    columnGrid.Children.Add(rowGrid);
                }
                columnGrid.SetValue(Grid.ColumnProperty, i);
                mainGrid.Children.Add(columnGrid);
            }
            return mainGrid;
        }
    }
}