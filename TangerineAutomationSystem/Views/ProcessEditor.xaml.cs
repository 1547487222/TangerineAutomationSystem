using System.Windows;
using System.Windows.Controls;
using TangerineAutomationSystem.ViewModels;
using TangerineAutomationSystem.Models;

namespace TangerineAutomationSystem.Views
{
    public partial class ProcessEditor : UserControl
    {
        public ProcessEditor()
        {
            InitializeComponent();
        }

        private async void ExecutePlatformTask_Click(object sender, RoutedEventArgs e)
        {
            if (DataContext is ProcessEditorViewModel vm && vm.SelectedNode is FlowNode node)
            {
                var ok = await vm.ExecutePlatformTaskAsync(node);
                MessageBox.Show(ok ? "执行调用已发送（模拟）" : "无法执行：节点不是平台任务或未配置");
            }
        }
    }
}