using System.Windows;
using System.Windows.Controls;
using TangerineAutomationSystem.ViewModels;

namespace TangerineAutomationSystem.Views
{
    public partial class ProjectExplorerView : UserControl
    {
        public ProjectExplorerView()
        {
            InitializeComponent();
        }

        private void TreeView_SelectedItemChanged(object sender, RoutedPropertyChangedEventArgs<object> e)
        {
            //if (DataContext is MainWindowViewModel mainViewModel)
            //{
            //    mainViewModel.SelectedItem = e.NewValue;

               
            //}
        }
    }
}