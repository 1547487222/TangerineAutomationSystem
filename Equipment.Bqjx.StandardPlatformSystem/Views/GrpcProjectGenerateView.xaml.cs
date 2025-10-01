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
    /// GrpcProjectGenerateView.xaml 的交互逻辑
    /// </summary>
    public partial class GrpcProjectGenerateView
    {
        private readonly GrpcProjectGenerateModel _grpcProjectGenerateModel;
        public GrpcProjectGenerateView()
        {
            InitializeComponent();
            _grpcProjectGenerateModel= new GrpcProjectGenerateModel();
            DataContext = _grpcProjectGenerateModel;
            this.Loaded += (sender, e) =>
            {
                _grpcProjectGenerateModel.Load();
            };
            this.Closed += (sender, e) => 
            {
                _grpcProjectGenerateModel.Close();
            };
        }
    }
}
