//using Nodify.Interactivity;
using MahApps.Metro.Controls;
using Nodify;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace Equipment.Bqjx.StandardPlatformSystem
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow
    {
        public MainWindow()
        {
            InitializeComponent();
            DataContext = new WorkFlowViewModel();
            App.ToolEngine.RaisePartCollectionChanged();
            this.Loaded += (sender, e) => 
            {
                Button close = this.FindChild<Button>("PART_Close");
                close.Click += (s, args) => { Application.Current.Shutdown(0); };
                flows.SelectedIndex = 0;
            };
        }

        
    }
}