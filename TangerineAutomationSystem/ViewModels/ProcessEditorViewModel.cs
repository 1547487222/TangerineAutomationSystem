using Newtonsoft.Json;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
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
            foreach (var n in CurrentFlow.Nodes)
            {
                Nodes.Add(n);
                // Subscribe to node position changes
                n.PropertyChanged += Node_PropertyChanged;
            }
            foreach (var c in CurrentFlow.Connections)
            {
                Connections.Add(c);
                UpdateConnectionPosition(c);
            }
        }

        private void Node_PropertyChanged(object? sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == nameof(FlowNode.X) || e.PropertyName == nameof(FlowNode.Y))
            {
                UpdateAllConnectionPositions();
            }
        }

        private void UpdateAllConnectionPositions()
        {
            foreach (var conn in Connections)
            {
                UpdateConnectionPosition(conn);
            }
        }

        private void UpdateConnectionPosition(Connection conn)
        {
            var fromNode = Nodes.FirstOrDefault(n => n.Id == conn.FromNodeId);
            var toNode = Nodes.FirstOrDefault(n => n.Id == conn.ToNodeId);
            if (fromNode != null && toNode != null)
            {
                // Connect from center-right of fromNode to center-left of toNode
                conn.FromX = fromNode.X + 80; // approximate width
                conn.FromY = fromNode.Y + 20; // approximate height/2
                conn.ToX = toNode.X;
                conn.ToY = toNode.Y + 20;
            }
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