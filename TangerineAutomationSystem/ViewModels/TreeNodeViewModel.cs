using System.Collections.ObjectModel;
using System.ComponentModel;

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

        public event PropertyChangedEventHandler? PropertyChanged;
        protected void OnPropertyChanged(string p) => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(p));
    }
}