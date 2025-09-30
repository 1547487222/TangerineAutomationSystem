using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using TangerineAutomationSystem.ViewModels;

namespace TangerineAutomationSystem.Views
{
    public partial class SolutionExplorer : UserControl
    {
        public SolutionExplorer()
        {
            InitializeComponent();
        }

        private void ExplorerTree_SelectedItemChanged(object sender, RoutedPropertyChangedEventArgs<object> e)
        {
            if (DataContext is MainWindowViewModel vm)
            {
                vm.SelectedNode = e.NewValue as TreeNodeViewModel;
            }
        }

        private void TreeViewItem_PreviewMouseRightButtonDown(object sender, MouseButtonEventArgs e)
        {
            var tvi = VisualUpwardSearch(e.OriginalSource as DependencyObject);
            if (tvi != null)
            {
                tvi.IsSelected = true;
                tvi.Focus();
                Keyboard.Focus(tvi);
                e.Handled = true;
            }
        }

        private static TreeViewItem? VisualUpwardSearch(DependencyObject? source)
        {
            while (source != null && !(source is TreeViewItem))
                source = VisualTreeHelper.GetParent(source);

            return source as TreeViewItem;
        }

        private void SearchBox_TextChanged(object sender, TextChangedEventArgs e)
        {

        }
    }
}