using Newtonsoft.Json;
using System;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using Tangerine.Framework;
using TangerineAutomationSystem.Models;
using TangerineAutomationSystem.Services;

namespace TangerineAutomationSystem.Views.Controls
{
    /// <summary>
    /// Inline editor control that renders basic JSON editors for a Tangerine.Framework IModuleProviderInfo
    /// - shows default controller options and module config as editable JSON
    /// - provides quick create/init instance buttons (uses TangerineHost.Instance)
    /// This is a simple helper for the UI; you can replace with a richer property-grid later.
    /// </summary>
    public class TangerineProviderInlineEditor : UserControl
    {
        private readonly IModuleProviderInfo _providerInfo;
        private readonly ModuleModel _module;
        private TextBox _controllerText;
        private TextBox _moduleText;

        public TangerineProviderInlineEditor(IModuleProviderInfo providerInfo, ModuleModel module)
        {
            _providerInfo = providerInfo ?? throw new ArgumentNullException(nameof(providerInfo));
            _module = module ?? throw new ArgumentNullException(nameof(module));
            BuildUi();
            LoadDefaults();
        }

        private void BuildUi()
        {
            var panel = new StackPanel();

            panel.Children.Add(new TextBlock { Text = $"提供器: {_providerInfo.DisplayName}", FontWeight = FontWeights.SemiBold });
            panel.Children.Add(new TextBlock { Text = $"配置类型: {_providerInfo.ConfigType.Name} 控制器类型: {_providerInfo.ControllerType.Name}", Margin = new Thickness(0,4,0,8) });

            panel.Children.Add(new TextBlock { Text = "控制器 JSON (编辑后可创建实例)" });
            _controllerText = new TextBox { AcceptsReturn = true, Height = 120, VerticalScrollBarVisibility = ScrollBarVisibility.Auto };
            panel.Children.Add(_controllerText);

            panel.Children.Add(new TextBlock { Text = "模块配置 JSON" , Margin = new Thickness(0,6,0,0)});
            _moduleText = new TextBox { AcceptsReturn = true, Height = 120, VerticalScrollBarVisibility = ScrollBarVisibility.Auto };
            panel.Children.Add(_moduleText);

            var btnPanel = new StackPanel { Orientation = Orientation.Horizontal, Margin = new Thickness(0,8,0,0) };
            var createBtn = new Button { Content = "创建实例", Margin = new Thickness(4,0,4,0) };
            createBtn.Click += CreateBtn_Click;
            btnPanel.Children.Add(createBtn);

            var initBtn = new Button { Content = "初始化第一个实例", Margin = new Thickness(4,0,4,0) };
            initBtn.Click += InitBtn_Click;
            btnPanel.Children.Add(initBtn);

            panel.Children.Add(btnPanel);

            Content = panel;
        }

        private void LoadDefaults()
        {
            try
            {
                var defaultController = Activator.CreateInstance(_providerInfo.ControllerType);
                var defaultConfig = _providerInfo.CreateDefaultConfig();
                _controllerText.Text = JsonConvert.SerializeObject(defaultController, Formatting.Indented);
                _moduleText.Text = JsonConvert.SerializeObject(defaultConfig, Formatting.Indented);
            }
            catch (Exception ex)
            {
                _controllerText.Text = $"// 生成默认控制器失败: {ex.Message}";
                _moduleText.Text = $"// 生成默认模块配置失败: {ex.Message}";
            }
        }

        private void CreateBtn_Click(object? sender, RoutedEventArgs e)
        {
            try
            {
                var controllerObj = JsonConvert.DeserializeObject(_controllerText.Text, _providerInfo.ControllerType);
                var moduleObj = JsonConvert.DeserializeObject(_moduleText.Text, _providerInfo.ConfigType);

                if (controllerObj == null || moduleObj == null)
                {
                    MessageBox.Show("反序列化控制器或模块配置失败，请检查 JSON。");
                    return;
                }

                var name = $"{_providerInfo.ProviderName}_{DateTime.Now:yyyyMMddHHmmss}";
                var inst = TangerineHost.Instance.InstanceManager.CreateInstance(_providerInfo.ProviderName, name, controllerObj, moduleObj);
                MessageBox.Show($"已创建实例：{inst.InstanceName}");
            }
            catch (Exception ex)
            {
                MessageBox.Show($"创建实例失败：{ex.Message}");
            }
        }

        private async void InitBtn_Click(object? sender, RoutedEventArgs e)
        {
            // 初始化第一个未初始化实例
            var inst = TangerineHost.Instance.InstanceManager.Instances.FirstOrDefault(i => i.Status == ModuleStatus.NotInitialized);
            if (inst == null)
            {
                MessageBox.Show("没有待初始化的实例。");
                return;
            }
            try
            {
                var ok = await TangerineHost.Instance.InstanceManager.InitializeInstanceAsync(inst.InstanceId);
                MessageBox.Show(ok ? "初始化成功" : "初始化失败");
            }
            catch (Exception ex)
            {
                MessageBox.Show($"初始化异常：{ex.Message}");
            }
        }
    }
}