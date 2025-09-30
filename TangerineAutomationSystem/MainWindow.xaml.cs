using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;

namespace TangerineAutomationSystem
{
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
        }

        private void TreeView_SelectedItemChanged(object sender, RoutedPropertyChangedEventArgs<object> e)
        {
            if (DataContext is ViewModels.MainWindowViewModel vm)
            {
                vm.SelectedNode = e.NewValue as ViewModels.TreeNodeViewModel;
            }
        }

        // 右键点击时选中对应 TreeViewItem，并把焦点给它（确保上下文命令可以正常获取选中项）
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
    }
}