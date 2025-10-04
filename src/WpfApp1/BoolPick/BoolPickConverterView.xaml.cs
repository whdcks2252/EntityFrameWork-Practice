using System.Windows.Controls;

namespace WpfApp1.BoolPick
{
    /// <summary>
    /// BoolPickConverterView.xaml에 대한 상호 작용 논리
    /// </summary>
    public partial class BoolPickConverterView : UserControl
    {
        public BoolPickConverterView()
        {
            InitializeComponent();
            DataContext = new BoolPickConverterVM();
        }
    }
}
