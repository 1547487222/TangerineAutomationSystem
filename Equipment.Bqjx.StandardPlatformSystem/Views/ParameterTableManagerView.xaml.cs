using Equipment.Bqjx.StandardPlatformSystem.Models;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using System.Windows.Threading;

namespace Equipment.Bqjx.StandardPlatformSystem.Views
{
    /// <summary>
    /// ParameterTableManagerView.xaml 的交互逻辑
    /// </summary>
    public partial class ParameterTableManagerView
    {
        private  readonly ParameterTableManagerViewModel parameterTableManagerViewModel = new();
        public ParameterTableManagerView()
        {
            InitializeComponent();
            Loaded += async (sender, e) =>
            {
                await Dispatcher.BeginInvoke(new Action(() =>
                {
                    DataContext = parameterTableManagerViewModel;
                    parameterTableManagerViewModel.Load();
                }), DispatcherPriority.Background);
            };
            Unloaded += (sender, e) => 
            {
                App.Current.Dispatcher.Invoke(() => 
                {
                    parameterTableManagerViewModel.UnLoad();
                });
            };
        }

        public void OpenTable()
        {
            this.Visibility = Visibility.Visible;
            parameterTableManagerViewModel.Load();
        }

        protected override void OnClosing(CancelEventArgs e)
        {
            e.Cancel = true;
            this.Visibility = Visibility.Collapsed;
            parameterTableManagerViewModel.UnLoad();
        }
    }
}
