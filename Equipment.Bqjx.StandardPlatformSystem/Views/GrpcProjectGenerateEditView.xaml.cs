using Equipment.Bqjx.StandardPlatformSystem.Models;
using System;
using System.Collections.Generic;
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
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace Equipment.Bqjx.StandardPlatformSystem.Views
{
    /// <summary>
    /// GrpcProjectGenerateEditView.xaml 的交互逻辑
    /// </summary>
    public partial class GrpcProjectGenerateEditView : UserControl
    {
        private readonly GrpcProjectGenerateEditViewModel _viewModel;
        public GrpcProjectGenerateEditView(GrpcProjectGenerateEditViewModel viewModel)
        {
            InitializeComponent();
            _viewModel = viewModel;
            this.DataContext = _viewModel;
        }
    }
}
