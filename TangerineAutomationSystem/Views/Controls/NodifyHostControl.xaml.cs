using System;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Threading;
using TangerineAutomationSystem.Models;
using TangerineAutomationSystem.ViewModels;

namespace TangerineAutomationSystem.Views.Controls
{
    public partial class NodifyHostControl : UserControl
    {
        private ProcessEditorViewModel? _vm;
        private FlowNode? _dragNode;
        private Point _dragStart;
        private bool _isDragging;
        private FlowNode? _connectionStart;

        public NodifyHostControl()
        {
            InitializeComponent();
            Loaded += NodifyHostControl_Loaded;
        }

        private void NodifyHostControl_Loaded(object? sender, RoutedEventArgs e)
        {
            _vm = DataContext as ProcessEditorViewModel;
            PART_Items.PreviewMouseLeftButtonDown += PART_Items_PreviewMouseLeftButtonDown;
            PART_Items.PreviewMouseLeftButtonUp += PART_Items_PreviewMouseLeftButtonUp;
            PART_Items.PreviewMouseMove += PART_Items_PreviewMouseMove;
            PART_Items.PreviewMouseRightButtonDown += PART_Items_PreviewMouseRightButtonDown;
        }

        private FlowNode? GetNodeUnderMouse(Point p)
        {
            // Hit test the visual tree to find an item with DataContext FlowNode
            var result = VisualTreeHelper.HitTest(PART_Canvas, p);
            if (result == null) return null;
            var v = result.VisualHit as DependencyObject;
            while (v != null && !(v is FrameworkElement fe && fe.DataContext is FlowNode))
            {
                v = VisualTreeHelper.GetParent(v);
            }
            return (v as FrameworkElement)?.DataContext as FlowNode;
        }

        private void PART_Items_PreviewMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            var pos = e.GetPosition(PART_Canvas);
            var node = GetNodeUnderMouse(pos);
            if (node == null) return;
            _dragNode = node;
            _dragStart = pos;
            _isDragging = true;
            PART_Canvas.CaptureMouse();
            _vm!.SelectedNode = node;
            e.Handled = true;
        }

        private void PART_Items_PreviewMouseMove(object sender, MouseEventArgs e)
        {
            if (!_isDragging || _dragNode == null) return;
            var pos = e.GetPosition(PART_Canvas);
            var dx = pos.X - _dragStart.X;
            var dy = pos.Y - _dragStart.Y;
            _dragStart = pos;
            // update node position
            _dragNode.X += dx;
            _dragNode.Y += dy;
            e.Handled = true;
        }

        private void PART_Items_PreviewMouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            if (_isDragging)
            {
                _isDragging = false;
                _dragNode = null;
                PART_Canvas.ReleaseMouseCapture();
                e.Handled = true;
            }
        }

        private void PART_Items_PreviewMouseRightButtonDown(object sender, MouseButtonEventArgs e)
        {
            var pos = e.GetPosition(PART_Canvas);
            var node = GetNodeUnderMouse(pos);
            if (node == null) return;
            if (_connectionStart == null)
            {
                _connectionStart = node;
                // simple visual feedback could be added later
            }
            else
            {
                if (_connectionStart != node)
                {
                    // create connection
                    var conn = new Connection { FromNodeId = _connectionStart.Id, ToNodeId = node.Id, FromPort = "out", ToPort = "in" };
                    if (_vm?.CurrentFlow != null)
                    {
                        _vm.CurrentFlow.Connections.Add(conn);
                        _vm.Connections.Add(conn);
                    }
                }
                _connectionStart = null;
            }
            e.Handled = true;
        }

        private void AddNode_Click(object sender, RoutedEventArgs e)
        {
            if (_vm == null) return;
            var node = new FlowNode { Name = $"Node_{_vm.Nodes.Count + 1}", X = 120 + _vm.Nodes.Count * 20, Y = 80 + _vm.Nodes.Count * 20, Kind = FlowNodeKind.ModuleAction };
            if (_vm.CurrentFlow != null)
            {
                _vm.CurrentFlow.Nodes.Add(node);
                _vm.Nodes.Add(node);
                _vm.SelectedNode = node;
            }
        }

        private void AutoLayout_Click(object sender, RoutedEventArgs e)
        {
            if (_vm == null) return;
            var i = 0;
            var cols = Math.Max(1, (int)Math.Ceiling(Math.Sqrt(Math.Max(1, _vm.Nodes.Count))));
            foreach (var n in _vm.Nodes)
            {
                n.X = 40 + (i % cols) * 220;
                n.Y = 40 + (i / cols) * 120;
                i++;
            }
        }
    }
}