using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System.Collections.ObjectModel;

namespace WpfApp1
{
    public partial class BoolPickConverterVM : ObservableObject
    {

        [ObservableProperty]
        private string _aA = "@@";


        private ObservableCollection<TestModel> testModels = new();
        public IEnumerable<TestModel> Test => testModels;

        private ObservableCollection<TestModel> testModels2 = new();
        public IEnumerable<TestModel> Test2 => testModels2;

        [ObservableProperty]
        public bool _boolTest = true;
        [ObservableProperty]
        public bool _boolTest1 = false;
        [ObservableProperty]
        public bool _boolTest2 = true;
        [ObservableProperty]
        public bool _boolTest3 = false;

        [RelayCommand]
        public void Button()
        {


        }
        [RelayCommand]
        public void Button2()
        {


        }

        public BoolPickConverterVM()
        {
            testModels.Add(new()
            {
                A = "testA",
            });
            testModels.Add(new()
            {
                A = "testB",
            });
            testModels2.Add(new()
            {
                A = "testC",
            });
            testModels2.Add(new()
            {
                A = "testD",
            });
        }




    }

    public partial class TestModel : ObservableObject
    {
        [ObservableProperty]
        public string _a;

        [ObservableProperty]
        public double? _b = 0;
    }
}
