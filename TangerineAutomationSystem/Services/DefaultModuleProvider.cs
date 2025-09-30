using System.Windows;
using System.Windows.Controls;
using TangerineAutomationSystem.Models;

namespace TangerineAutomationSystem.Services
{
    // 一个简单的默认模块提供器示例
    public class DefaultModuleProvider : IModuleProvider
    {
        public string Name => "Default";
        public string Description => "默认模块提供器：展示配置描述";

        public FrameworkElement? GetEditor(ModuleModel module)
        {
            var panel = new StackPanel();
            panel.Children.Add(new TextBlock { Text = $"提供器: {Name}", FontWeight = FontWeights.SemiBold });
            panel.Children.Add(new TextBlock { Text = "模块描述:" });
            var tb = new TextBox();
            tb.Text = module.Config.Description ?? "";
            tb.AcceptsReturn = true;
            tb.Height = 80;
            tb.TextChanged += (s, e) => module.Config.Description = tb.Text;
            panel.Children.Add(tb);
            return panel;
        }
    }
}