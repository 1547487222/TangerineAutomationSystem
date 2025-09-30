using System.Linq;
using System.Windows;
using System.Windows.Controls;
using TangerineAutomationSystem.Models;
using TangerineAutomationSystem.Services;

namespace TangerineAutomationSystem.Views
{
    public partial class ModuleEditor : UserControl
    {
        public ModuleEditor()
        {
            InitializeComponent();
            Loaded += ModuleEditor_Loaded;
        }

        private void ModuleEditor_Loaded(object sender, RoutedEventArgs e)
        {
            var module = DataContext as ModuleModel;
            if (module == null) return;

            // 填充提供器列表（来自 TangerineHost，如果可用）
            var providers = TangerineHost.Instance.GetAvailableProviders().ToList();
            if (providers.Count > 0)
            {
                ProviderCombo.ItemsSource = providers;
                var provider = providers.FirstOrDefault(p => p.ProviderName == module.ModuleType) ?? providers.First();
                ProviderCombo.SelectedItem = provider;
                RenderProviderEditor(provider, module);
            }
            else
            {
                // fallback: keep earlier LabProject ModuleProviderManager (simple)
                ProviderCombo.ItemsSource = ModuleProviderManager.GetProviders();
                var p = ModuleProviderManager.GetProviders().FirstOrDefault(pr => pr.Name == module.ModuleType) ??
                        ModuleProviderManager.GetProviders().FirstOrDefault();
                ProviderCombo.SelectedItem = p;
                RenderProviderEditorFallback(p as IModuleProvider, module);
            }
        }

        private void ApplyProvider_Click(object sender, RoutedEventArgs e)
        {
            var module = DataContext as ModuleModel;
            if (module == null) return;

            if (ProviderCombo.SelectedItem is Tangerine.Framework.IModuleProviderInfo tfInfo)
            {
                module.ModuleType = tfInfo.ProviderName;
                RenderProviderEditor(tfInfo, module);
            }
            else if (ProviderCombo.SelectedItem is IModuleProvider lp)
            {
                module.ModuleType = lp.Name;
                RenderProviderEditorFallback(lp, module);
            }
        }

        private void RenderProviderEditor(Tangerine.Framework.IModuleProviderInfo? providerInfo, ModuleModel module)
        {
            ProviderEditorArea.Content = null;
            if (providerInfo == null) return;
            // Simple editor: show default config JSON and controller options JSON as editable textBox
            var editor = new Controls.TangerineProviderInlineEditor(providerInfo, module);
            ProviderEditorArea.Content = editor;
        }

        private void RenderProviderEditorFallback(IModuleProvider? provider, ModuleModel module)
        {
            ProviderEditorArea.Content = null;
            if (provider == null) return;
            var editor = provider.GetEditor(module);
            if (editor != null)
            {
                ProviderEditorArea.Content = editor;
            }
            else
            {
                ProviderEditorArea.Content = new TextBlock { Text = "该提供器没有自定义编辑器，使用默认界面。" };
            }
        }
    }
}