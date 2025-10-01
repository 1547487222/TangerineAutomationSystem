using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Equipment.Bqjx.StandardPlatformSystem.Views;
using HandyControl.Controls;
using Microsoft.Win32;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using QStandaedPlatform.Engine.Common.Common;
using QStandaedPlatform.Engine.Common.Common.Metadatas;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using MessageBox = System.Windows.MessageBox;

namespace Equipment.Bqjx.StandardPlatformSystem.WorkFlows
{
    public partial class FlowModel:ObservableObject
    {
        [ObservableProperty]
        private Guid _flowId;
        [ObservableProperty]
        private string _flowDescription;
        [ObservableProperty]
        private NodeModel _selectedNode = default!;
        [ObservableProperty]
        private ConnectionViewModel _pendingConnection = default!;
        [ObservableProperty]
        private Point _dropPoint;
        [ObservableProperty]
        private bool _isLeftDrawerOpenForNodePropertyPanel;

        public ObservableCollection<NodeModel> Nodes { get; } = [];

        public ObservableCollection<NodeMenuItemViewModel> NodeMenuItems { get; } = [];

        public ObservableCollection<ConnectionViewModel> Connections { get; } = [];

        public object FlowContent { get; set; }

        public Flow Flow { get; }


        

        public FlowModel(Flow flow)
        {
            FlowContent = new FlowView();
            Flow = flow;
            FlowId = flow.FlowId;
            this.FlowName = flow.FlowName;
            FlowDescription = flow.Description;
            var descs = App.ToolEngine.GetToolDescriptions();
            foreach (var (toolName, desc) in descs)
            {
                NodeMenuItems.Add(new NodeMenuItemViewModel() { Name = toolName, Description = desc });
            }

            var tools = flow.GetTools();
            foreach (var tool in tools)
            {
                AddNodeByTool(tool);
            }
            CheckLinks();
        }
        private string _FlowName = string.Empty;

        public string FlowName
        {
            get { return _FlowName; }
            set
            {
                if (SetProperty(ref _FlowName, value))
                {
                    Flow.FlowName = value;
                }
            }
        }

        [RelayCommand]
        private void CreateConnection((object Source, object? Target) connecter)
        {
            if (connecter.Source is OutputPortModel output
                && connecter.Target is InputPortModel input)
            {

                if (output.OwnerId == input.OwnerId)
                {
                    return;
                }

                if (Connections.Any(p => p.Source == output && p.Target == input))
                {
                    return;
                }

                output.ConnectedInputPorts.Add(input);
                input.ConnectedOutputPorts.Add(output);

                output.UpdateMatchConnectPins();
                input.UpdateMatchConnectPins();
                Connections.Add(new ConnectionViewModel(output, input));
                Flow.AddToolConnecter(new ToolConnecter
                {
                    SourceToolId = Guid.Parse(output.OwnerId),
                    TargetToolId = Guid.Parse(input.OwnerId)
                });
            }
            else if (connecter.Source is InputPortModel input1
                && connecter.Target is OutputPortModel output1)
            {
                if (output1.OwnerId == input1.OwnerId)
                {
                    return;
                }
                if (Connections.Any(p => p.Source == output1 && p.Target == input1))
                {
                    return;
                }
                output1.ConnectedInputPorts.Add(input1);
                input1.ConnectedOutputPorts.Add(output1);
                output1.UpdateMatchConnectPins();
                input1.UpdateMatchConnectPins();
                Connections.Add(new ConnectionViewModel(output1, input1));
                Flow.AddToolConnecter(new ToolConnecter
                {
                    SourceToolId = Guid.Parse(output1.OwnerId),
                    TargetToolId = Guid.Parse(input1.OwnerId)
                });
            }
        }
        [RelayCommand]
        private void DeleteConnecttion(object connecter)
        {
            if (connecter is ConnectionViewModel connection)
            {
                foreach (var item in connection.Target.ConnectedOutputPorts)
                {
                    if (item == connection.Source)
                    {
                        for (var i = item.OutputPins.Count - 1; i >= 0; i--)
                        {
                            var pin = item.OutputPins[i];
                            foreach (var inputPin in connection.Target.InputPins)
                            {
                                if (inputPin.LinkPins.Any(p => p.Id == pin.Id))
                                {
                                    inputPin.UnLink(pin);
                                    pin.UnLink(inputPin);
                                }
                            }
                        }
                    }
                }
                foreach (var item in connection.Source.ConnectedInputPorts)
                {
                    if (item == connection.Target)
                    {
                        for (var i = item.InputPins.Count - 1; i >= 0; i--)
                        {
                            var pin = item.InputPins[i];
                            foreach (var outputPin in connection.Source.OutputPins)
                            {
                                if (outputPin.LinkPins.Any(p => p.Id == pin.Id))
                                {
                                    outputPin.UnLink(pin);
                                    pin.UnLink(outputPin);
                                }
                            }
                        }
                    }
                }
                connection.Target.ConnectedOutputPorts.Remove(connection.Source);
                connection.Target.UpdateMatchConnectPins();
                connection.Source.ConnectedInputPorts.Remove(connection.Target);
                connection.Source.UpdateMatchConnectPins();
                Connections.Remove(connection);
                Flow.RemoveToolConnecter(new ToolConnecter 
                {
                     SourceToolId = Guid.Parse(connection.Source.OwnerId),
                     TargetToolId = Guid.Parse(connection.Target.OwnerId)
                });
            }
        }
        [RelayCommand]
        private void NodeDrop(NodeMenuItemViewModel nodeMenuItem)
        {
            if (nodeMenuItem == null)
                return;

            if (Flow.TryCreateTool(nodeMenuItem.Name, Guid.NewGuid(), out var tool))
            {
                if (tool != null)
                {
                    tool.ToolPosition = new QPoint { X = DropPoint.X, Y = DropPoint.Y };
                    AddNodeByTool(tool);
                    App.ToolEngine.RaisePartCollectionChanged();
                }
            }
            else
            {
                MessageBox.Show("创建工具失败");
            }
        }
        private void AddNodeByTool(Tool tool)
        {
            if (tool != null)
            {
                var node = new NodeModel(tool,this)
                {
                    Name = tool.DefineName,
                    DisplayName = tool.DisplayName,
                    Location = new Point(tool.ToolPosition.X, tool.ToolPosition.Y),
                    UniqueId = tool.UniqueId.ToString(),
                };
                var inputPort = new InputPortModel() { OwnerId = node.UniqueId };
                var outputPort = new OutputPortModel() { OwnerId = node.UniqueId, };
                foreach (var input in tool.InputPins)
                {
                    inputPort.InputPins.Add(input);
                }
                foreach (var output in tool.OutputPins)
                {
                    outputPort.OutputPins.Add(output);
                }
                if (tool.TriggerPointCommands.Count != 0)
                {
                    foreach (var triggerPointCommand in tool.TriggerPointCommands.OrderBy(p=>p.Id))
                    {
                        node.TriggerPointModels.Add(new TriggerPointModel
                        {
                            CommandName = triggerPointCommand.Name,
                            ToolTriggerCommand = new ToolTriggerCommand(tool, triggerPointCommand)
                        });
                    }
                }
                inputPort.UpdateMatchConnectPins();
                node.InputPort = inputPort;
                outputPort.UpdateMatchConnectPins();
                node.OutputPort = outputPort;
                Nodes.Add(node);
            }
        }
        private void CheckLinks()
        {
            foreach (var toolConnecter in Flow.GetToolConnecters())
            {
                var sourceNode = Nodes.FirstOrDefault(n => n.UniqueId == toolConnecter.SourceToolId.ToString());
                var targetNode = Nodes.FirstOrDefault(n => n.UniqueId == toolConnecter.TargetToolId.ToString());
                if (sourceNode != null && targetNode != null)
                {
                    sourceNode.OutputPort.ConnectedInputPorts.Add(targetNode.InputPort);
                    targetNode.InputPort.ConnectedOutputPorts.Add(sourceNode.OutputPort);
                    sourceNode.OutputPort.UpdateMatchConnectPins();
                    targetNode.InputPort.UpdateMatchConnectPins();
                    Connections.Add(new ConnectionViewModel(sourceNode.OutputPort, targetNode.InputPort));
                }
            }
        }
        [RelayCommand]
        private void CopyNode(NodeModel nodeModel)
        {
            if (nodeModel.OwnerTool is DynamicPinTool dynamicPinTool)
            {
                Clipboard.SetText(JsonConvert.SerializeObject(new { name = nodeModel.OwnerTool.DefineName, data = dynamicPinTool.DataContext, point = nodeModel.Location }));
            }
            else if (nodeModel.OwnerTool is DynamicSyncInputPinTool dynamicSyncInputPin)
            {
                Clipboard.SetText(JsonConvert.SerializeObject(new { name = nodeModel.OwnerTool.DefineName, data = dynamicSyncInputPin.DataContext, point = nodeModel.Location }));
            }
            else if (nodeModel.OwnerTool is ModuleToolBase moduleToolBase)
            {
                Clipboard.SetText(JsonConvert.SerializeObject(new { name = nodeModel.OwnerTool.DefineName, data = moduleToolBase.DataContext, point = nodeModel.Location }));
            }
            else if (nodeModel.OwnerTool is SyncInputModuleToolBase syncInputModuleToolBase)
            {
                Clipboard.SetText(JsonConvert.SerializeObject(new { name = nodeModel.OwnerTool.DefineName, data = syncInputModuleToolBase.DataContext, point = nodeModel.Location }));
            }
            else if (nodeModel.OwnerTool is SyncInputToolBase syncInputToolBase)
            {
                Clipboard.SetText(JsonConvert.SerializeObject(new { name = nodeModel.OwnerTool.DefineName, data = syncInputToolBase.DataContext, point = nodeModel.Location }));
            }
            else
            {
                Clipboard.SetText(JsonConvert.SerializeObject(new { name = nodeModel.OwnerTool.DefineName, data = nodeModel.OwnerTool.DataContext, point = nodeModel.Location }));
            }
        }

        [RelayCommand]
        private void PasteNode()
        {
            try
            {
                var o = JObject.Parse(System.Windows.Clipboard.GetText());
                if (o.ContainsKey("name") && o.ContainsKey("data"))
                {
                    if (Flow.TryCreateTool(o["name"].ToString(), Guid.NewGuid(), out var tool))
                    {
                        if (tool != null)
                        {
                            var point = o["point"].ToObject<Point>();
                            tool.ToolPosition = new QPoint { X = point.X, Y = point.Y };

                            if (tool is DynamicPinTool dynamicPinTool)
                            {
                                dynamicPinTool.DataContext = (DynamicPinToolData)JObject.Parse(o["data"].ToString()).ToObject(dynamicPinTool.DataContext.GetType());
                                dynamicPinTool.ApplyOnContextChanged(tool.DataContext);
                            }
                            else if (tool is DynamicSyncInputPinTool dynamicSyncInputPin)
                            {
                                dynamicSyncInputPin.DataContext = (DynamicPinToolData)JObject.Parse(o["data"].ToString()).ToObject(dynamicSyncInputPin.DataContext.GetType());
                                dynamicSyncInputPin.ApplyOnContextChanged(tool.DataContext);
                            }
                            else if (tool is ModuleToolBase moduleToolBase)
                            {
                                moduleToolBase.DataContext = (ModuleData)JObject.Parse(o["data"].ToString()).ToObject(moduleToolBase.DataContext.GetType());
                                moduleToolBase.ApplyOnContextChanged(tool.DataContext);
                            }
                            else if (tool is SyncInputModuleToolBase syncInputModuleToolBase)
                            {
                                syncInputModuleToolBase.DataContext = (ModuleData)JObject.Parse(o["data"].ToString()).ToObject(syncInputModuleToolBase.DataContext.GetType());
                                syncInputModuleToolBase.ApplyOnContextChanged(tool.DataContext);
                            }
                            else if (tool is SyncInputToolBase syncInputToolBase)
                            {
                                syncInputToolBase.DataContext = JObject.Parse(o["data"].ToString()).ToObject(syncInputToolBase.DataContext.GetType());
                                syncInputToolBase.ApplyOnContextChanged(tool.DataContext);
                            }
                            else
                            {
                                if (tool.DataContext != null)
                                {
                                    tool.DataContext = JObject.Parse(o["data"].ToString()).ToObject(tool.DataContext.GetType());
                                    tool.ApplyOnContextChanged(tool.DataContext);
                                }
                            }

                            AddNodeByTool(tool);
                            App.ToolEngine.RaisePartCollectionChanged();
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("粘贴失败" + ex.Message, "提示");
            }
        }
        [RelayCommand]
        private void DeleteLinkNode(NodeModel nodeViewModel)
        {
            App.Current.Dispatcher.Invoke(() =>
            {
                if (nodeViewModel != null)
                {
                    for (int i = Connections.Count - 1; i >= 0; i--)
                    {
                        var item = Connections[i];
                        if (item.Source.OwnerId == nodeViewModel.UniqueId || item.Target.OwnerId == nodeViewModel.UniqueId)
                        {
                            if (item.Source.OwnerId == nodeViewModel.UniqueId)
                            {
                                foreach (var outputPin in item.Source.OutputPins)
                                {
                                    foreach (var linkPin in outputPin.LinkPins)
                                    {
                                        linkPin.UnLink(outputPin);
                                    }
                                    outputPin.LinkPins.Clear();
                                }
                                item.Source.OutputPins.Clear();
                            }
                            if (item.Target.OwnerId == nodeViewModel.UniqueId)
                            {
                                foreach (var inputPin in item.Target.InputPins)
                                {
                                    foreach (var linkPin in inputPin.LinkPins)
                                    {
                                        linkPin.UnLink(inputPin);
                                    }
                                    inputPin.LinkPins.Clear();
                                }
                                item.Target.InputPins.Clear();
                            }
                            DeleteConnecttion(item);
                        }
                    }
                    Nodes.Remove(nodeViewModel);
                    Flow.RemoveTool(Guid.Parse(nodeViewModel.UniqueId));

                }
            });
        }

        [RelayCommand]
        private void OpenLinkNodeSettingPanel(NodeModel node)
        {
            if (!IsLeftDrawerOpenForNodePropertyPanel)
            {
                IsLeftDrawerOpenForNodePropertyPanel = true;
            }

        }

        public void ResetPanelExpanded()
        {
            foreach (var item in Nodes)
            {
                item.IsExpanded = false;
            }
        }

        [RelayCommand]
        private void SaveFlow()
        {
            try
            {
                if (App.ToolEngine.GetFlowFileDescriptions().Any(p => p.FlowId == Flow.FlowId))
                {
                    var flowFileDescription = App.ToolEngine.GetFlowFileDescriptions().FirstOrDefault(p => p.FlowId == Flow.FlowId);
                    if (flowFileDescription != null)
                    {
                        App.ToolEngine.SaveFlow(this.Flow, flowFileDescription.FilePath);
                        MessageBox.Show("保存成功");
                    }
                }
                else
                {
                    SaveFileDialog saveFileDialog = new()
                    {
                        FileName = Flow.FlowName,
                    };
                    if (saveFileDialog.ShowDialog() == true)
                    {
                        var path = saveFileDialog.FileName;
                        App.ToolEngine.SaveFlow(this.Flow, path + ".flow");
                        MessageBox.Show("保存成功");
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("流程保存异常：" + ex.ToString());
            }
        }

        [RelayCommand]
        private async Task RequestPause()
        {
            try
            {
                await Flow.RequestPauseAsync();
                MessageBox.Show("流程暂停成功！");
            }
            catch (Exception ex)
            {
                MessageBox.Show($"流程暂停失败:{ex.Message}");
            }
        }
        [RelayCommand]
        private async Task RequestPauseReset()
        {
            try
            {
                await Flow.RequestContinueAsync();
                MessageBox.Show("流程继续成功！");
            }
            catch (Exception ex)
            {
                MessageBox.Show($"流程继续失败:{ex.Message}");
            }
        }
        [RelayCommand]
        private async Task ClearEphemeralData()
        {
            try
            {
                await Flow.ClearEphemeralDataAsync();
                MessageBox.Show("临时数据清除成功！");
            }
            catch (Exception ex)
            {
                MessageBox.Show($"临时数据清除失败:{ex.Message}");
            }
        }
        [RelayCommand]
        private async Task RequestExecutionCancel()
        {
            try
            {
                await Flow.RequestCancelAsync();
                MessageBox.Show("流程取消成功！");
            }
            catch (Exception ex)
            {
                MessageBox.Show($"流程取消失败:{ex.Message}");
            }
        }
        [RelayCommand]
        private async Task RequestExecutionReset()
        {
            try
            {
                await Flow.RequestResetAsync();
                await Flow.RequestStartAsync();
                MessageBox.Show("流程复位成功！");
            }
            catch (Exception ex)
            {
                MessageBox.Show($"流程复位失败:{ex.Message}");
            }
        }
    }
}
