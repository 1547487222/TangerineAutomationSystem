using System;
using System.ComponentModel;

namespace TangerineAutomationSystem.Models
{
    public enum FlowNodeKind
    {
        PlatformTask,
        ModuleAction,
        Transfer,
        Resource
    }

    public class FlowNode : INotifyPropertyChanged
    {
        public string Id { get; set; } = Guid.NewGuid().ToString();
        private string _name = "Node";
        public string Name { get => _name; set { _name = value; PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(Name))); } }

        public FlowNodeKind Kind { get; set; } = FlowNodeKind.ModuleAction;
        public string? ModuleRef { get; set; }
        public string? PlatformTaskRef { get; set; }
        public string? ResourceRef { get; set; }

        public double X { get; set; }
        public double Y { get; set; }

        public object Config { get; set; } = new { };
        public string ConfigJson { get; set; } = "{}";

        public event PropertyChangedEventHandler? PropertyChanged;
    }
}