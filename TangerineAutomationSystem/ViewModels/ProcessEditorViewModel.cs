using Newtonsoft.Json;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Windows.Input;
using TangerineAutomationSystem.Models;
using TangerineAutomationSystem.Services;

namespace TangerineAutomationSystem.ViewModels
{
    public class ProcessEditorViewModel : INotifyPropertyChanged
    {
        public ObservableCollection<ProcessFlow> Flows { get; } = new();
        public ObservableCollection<FlowNode> Nodes { get; } = new();
        public ObservableCollection<Connection> Connections { get; } = new();

        private ProcessFlow? _currentFlow;
        public ProcessFlow? CurrentFlow { get => _currentFlow; set { _currentFlow = value; OnPropertyChanged(nameof(CurrentFlow)); RefreshNodesFromFlow(); } }

        private FlowNode? _selectedNode;
        public FlowNode? SelectedNode { get => _selectedNode; set { _selectedNode = value; OnPropertyChanged(nameof(SelectedNode)); } }

        public PlatformCallGrpcClient PlatformClient { get; }

        public ProcessEditorViewModel()
        {
            PlatformClient = new PlatformCallGrpcClient();
            var f = new ProcessFlow { Name = "DefaultFlow" };
            Flows.Add(f);
            CurrentFlow = f;
        }

        private void RefreshNodesFromFlow()
        {
            Nodes.Clear(); Connections.Clear();
            if (CurrentFlow == null) return;
            foreach (var n in CurrentFlow.Nodes) Nodes.Add(n);
            foreach (var c in CurrentFlow.Connections) Connections.Add(c);
        }

        public ICommand AddPlatformTaskCommand => new RelayCommand(_ =>
        {
            if (CurrentFlow == null) return;
            var t = new PlatformTask { Name = $"PlatformTask-{CurrentFlow.Nodes.Count + 1}" };
            var node = new FlowNode { Name = t.Name, Kind = FlowNodeKind.PlatformTask, PlatformTaskRef = t.Id, X = 100, Y = 100, Config = t, ConfigJson = JsonConvert.SerializeObject(t, Formatting.Indented) };
            CurrentFlow.Nodes.Add(node);
            Nodes.Add(node);
        });

        public ICommand SaveFlowCommand => new RelayCommand(_ =>
        {
            if (CurrentFlow != null)
            {
                var json = JsonConvert.SerializeObject(CurrentFlow, Formatting.Indented);
            }
        });

        public async System.Threading.Tasks.Task<bool> ExecutePlatformTaskAsync(FlowNode node)
        {
            if (node.Kind != FlowNodeKind.PlatformTask || node.PlatformTaskRef == null) return false;
            return await PlatformClient.CallExecutePlatformTaskAsync(node.PlatformTaskRef, node.PlatformTaskRef, node.ConfigJson);
        }

        public event PropertyChangedEventHandler? PropertyChanged;
        protected void OnPropertyChanged(string p) => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(p));
    }
}