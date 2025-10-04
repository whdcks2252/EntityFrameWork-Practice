using System.Windows.Controls;

namespace WpfApp1.Entity
{
    /// <summary>
    /// EntityFrameworkView.xaml에 대한 상호 작용 논리
    /// </summary>
    public partial class EntityFrameworkView : UserControl
    {
        public EntityFrameworkView()
        {
            InitializeComponent();
            DataContext = new EntityFrameworkVM();
        }
    }
}
