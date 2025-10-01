using System.Collections.ObjectModel;
using System.ComponentModel;
using TangerineAutomationSystem.Models;

namespace TangerineAutomationSystem.ViewModels
{
    public class TreeNodeViewModel : INotifyPropertyChanged
    {
        private string _displayName = "节点";
        public string DisplayName { get => _displayName; set { _displayName = value; OnPropertyChanged(nameof(DisplayName)); } }
        public string NodeType { get; set; } = "Unknown"; // Lab, Line, Platform, Module, LabResource, PlatformResource
        public object Model { get; set; } = null!;
        public ObservableCollection<TreeNodeViewModel> Children { get; set; } = new();
        public TreeNodeViewModel? Parent { get; set; }

        // For Lab nodes, provide access to module function catalog
        public ModuleFunctionCatalog? ModuleFunctionCatalog
        {
            get
            {
                if (Model is LaboratoryModel lab)
                {
                    return lab.ModuleFunctionCatalog;
                }
                return null;
            }
        }

        public event PropertyChangedEventHandler? PropertyChanged;
        protected void OnPropertyChanged(string p) => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(p));
    }
}