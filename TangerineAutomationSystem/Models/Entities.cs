using System.Collections.ObjectModel;
using System.ComponentModel;

namespace TangerineAutomationSystem.Models
{
    public class LaboratoryModel : INotifyPropertyChanged
    {
        private string _name = "新实验室";
        public string Name { get => _name; set { _name = value; OnPropertyChanged(nameof(Name)); } }
        public ObservableCollection<ProductionLineModel> ProductionLines { get; set; } = new();
        public ObservableCollection<LabResourceModel> LabResources { get; set; } = new();
        public ObservableCollection<ModuleDefinition> ModuleDefinitions { get; set; } = new();
        public ModuleFunctionCatalog ModuleFunctionCatalog { get; set; } = new();
        public event PropertyChangedEventHandler? PropertyChanged;
        protected void OnPropertyChanged(string p) => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(p));
    }

    public class ProductionLineModel : INotifyPropertyChanged
    {
        private string _name = "新产线";
        public string Name { get => _name; set { _name = value; OnPropertyChanged(nameof(Name)); } }
        public ObservableCollection<PlatformModel> Platforms { get; set; } = new();
        public ObservableCollection<ProductionLineProcess> ProductionLineProcesses { get; set; } = new();
        public event PropertyChangedEventHandler? PropertyChanged;
        protected void OnPropertyChanged(string p) => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(p));
    }

    public class PlatformModel : INotifyPropertyChanged
    {
        private string _name = "新平台";
        public string Name { get => _name; set { _name = value; OnPropertyChanged(nameof(Name)); } }
        public ObservableCollection<ModuleModel> Modules { get; set; } = new();
        public ObservableCollection<PlatformResourceModel> PlatformResources { get; set; } = new();
        public ObservableCollection<PlatformTask> PlatformTasks { get; set; } = new();
        public event PropertyChangedEventHandler? PropertyChanged;
        protected void OnPropertyChanged(string p) => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(p));
    }

    public class ModuleModel : INotifyPropertyChanged
    {
        private string _name = "新模块";
        public string Name { get => _name; set { _name = value; OnPropertyChanged(nameof(Name)); } }
        private string _moduleType = "Default";
        public string ModuleType { get => _moduleType; set { _moduleType = value; OnPropertyChanged(nameof(ModuleType)); } }
        public ModuleConfigModel Config { get; set; } = new();
        public event PropertyChangedEventHandler? PropertyChanged;
        protected void OnPropertyChanged(string p) => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(p));
    }

    public class ModuleConfigModel : INotifyPropertyChanged
    {
        private string _description = "";
        public string Description { get => _description; set { _description = value; OnPropertyChanged(nameof(Description)); } }
        public event PropertyChangedEventHandler? PropertyChanged;
        protected void OnPropertyChanged(string p) => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(p));
    }

    public class LabResourceModel
    {
        public string ResourceType { get; set; } = "Warehouse";
        public string Name { get; set; } = "新仓库";
    }

    public class PlatformResourceModel
    {
        public string ResourceType { get; set; } = "Tray";
        public string Name { get; set; } = "新托盘";
    }
}