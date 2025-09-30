using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using Tangerine.Framework;
using TangerineAutomationSystem.Services;

namespace TangerineAutomationSystem.ViewModels
{
    public class ModuleProviderEditorViewModel : INotifyPropertyChanged
    {
        public ObservableCollection<IModuleProviderInfo> Providers { get; } = new ObservableCollection<IModuleProviderInfo>();

        private IModuleProviderInfo? _selectedProvider;
        public IModuleProviderInfo? SelectedProvider
        {
            get => _selectedProvider;
            set
            {
                _selectedProvider = value;
                OnPropertyChanged(nameof(SelectedProvider));
                LoadDefaultConfigs();
            }
        }

        private string _controllerJson = string.Empty;
        public string ControllerJson
        {
            get => _controllerJson;
            set { _controllerJson = value; OnPropertyChanged(nameof(ControllerJson)); }
        }

        private string _moduleJson = string.Empty;
        public string ModuleJson
        {
            get => _moduleJson;
            set { _moduleJson = value; OnPropertyChanged(nameof(ModuleJson)); }
        }

        public ObservableCollection<IModuleInstance> Instances { get; } = new ObservableCollection<IModuleInstance>();

        public ICommand RefreshProvidersCommand { get; }
        public ICommand CreateInstanceCommand { get; }
        public ICommand InitializeInstanceCommand { get; }
        public ICommand ShutdownInstanceCommand { get; }
        public ICommand RemoveInstanceCommand { get; }

        public ModuleProviderEditorViewModel()
        {
            RefreshProvidersCommand = new RelayCommand(_ => RefreshProviders());
            CreateInstanceCommand = new RelayCommand(_ => CreateInstance(), _ => SelectedProvider != null);
            InitializeInstanceCommand = new RelayCommand(async p => await InitializeInstanceAsync(p), p => p is IModuleInstance);
            ShutdownInstanceCommand = new RelayCommand(async p => await ShutdownInstanceAsync(p), p => p is IModuleInstance);
            RemoveInstanceCommand = new RelayCommand(p => RemoveInstance(p), p => p is IModuleInstance);

            RefreshProviders();
            RefreshInstances();
        }

        private void RefreshProviders()
        {
            Providers.Clear();
            foreach (var info in TangerineHost.Instance.GetAvailableProviders())
            {
                Providers.Add(info);
            }
            if (Providers.Count > 0 && SelectedProvider == null) SelectedProvider = Providers.First();
        }

        private void LoadDefaultConfigs()
        {
            if (SelectedProvider == null) return;
            try
            {
                var controllerType = SelectedProvider.ControllerType;
                var configType = SelectedProvider.ConfigType;

                var defaultController = Activator.CreateInstance(controllerType);
                var defaultConfig = SelectedProvider.CreateDefaultConfig();

                ControllerJson = JsonConvert.SerializeObject(defaultController, Formatting.Indented);
                ModuleJson = JsonConvert.SerializeObject(defaultConfig, Formatting.Indented);
            }
            catch (Exception ex)
            {
                ControllerJson = $"// 生成默认控制器失败：{ex.Message}";
                ModuleJson = $"// 生成默认模块配置失败：{ex.Message}";
            }
        }

        private void RefreshInstances()
        {
            Instances.Clear();
            foreach (var inst in TangerineHost.Instance.InstanceManager.Instances)
            {
                Instances.Add(inst);
            }
        }

        private void CreateInstance()
        {
            if (SelectedProvider == null)
            {
                MessageBox.Show("请先选择提供器。");
                return;
            }
            try
            {
                // Deserialize controller and module JSON into correct types
                var controllerObj = JsonConvert.DeserializeObject(ControllerJson, SelectedProvider.ControllerType);
                var moduleConfigObj = JsonConvert.DeserializeObject(ModuleJson, SelectedProvider.ConfigType);

                if (controllerObj == null || moduleConfigObj == null)
                {
                    MessageBox.Show("控制器或模块配置反序列化失败，请检查 JSON。");
                    return;
                }

                var name = $"{SelectedProvider.ProviderName}_{DateTime.Now:yyyyMMddHHmmss}";
                var instance = TangerineHost.Instance.InstanceManager.CreateInstance(SelectedProvider.ProviderName, name, controllerObj, moduleConfigObj);

                RefreshInstances();
                MessageBox.Show($"实例创建成功：{instance.InstanceName}");
            }
            catch (Exception ex)
            {
                MessageBox.Show($"创建实例失败：{ex.Message}");
            }
        }

        private async Task InitializeInstanceAsync(object? param)
        {
            if (param is IModuleInstance inst)
            {
                try
                {
                    var ok = await TangerineHost.Instance.InstanceManager.InitializeInstanceAsync(inst.InstanceId);
                    if (ok) MessageBox.Show($"实例 {inst.InstanceName} 初始化成功。");
                    else MessageBox.Show($"实例 {inst.InstanceName} 初始化未生效。");
                    RefreshInstances();
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"初始化失败：{ex.Message}");
                }
            }
        }

        private async Task ShutdownInstanceAsync(object? param)
        {
            if (param is IModuleInstance inst)
            {
                try
                {
                    var ok = await TangerineHost.Instance.InstanceManager.ShutdownInstanceAsync(inst.InstanceId);
                    if (ok) MessageBox.Show($"实例 {inst.InstanceName} 已关闭。");
                    else MessageBox.Show($"实例 {inst.InstanceName} 关闭失败或不在运行中。");
                    RefreshInstances();
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"关闭失败：{ex.Message}");
                }
            }
        }

        private void RemoveInstance(object? param)
        {
            if (param is IModuleInstance inst)
            {
                try
                {
                    var ok = TangerineHost.Instance.InstanceManager.RemoveInstance(inst.InstanceId);
                    if (ok) MessageBox.Show($"实例 {inst.InstanceName} 已移除。");
                    else MessageBox.Show($"移除失败。");
                    RefreshInstances();
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"移除失败：{ex.Message}");
                }
            }
        }

        public event PropertyChangedEventHandler? PropertyChanged;
        protected void OnPropertyChanged(string p) => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(p));
    }
}