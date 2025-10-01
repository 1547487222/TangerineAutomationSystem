using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Equipment.Bqjx.StandardPlatformSystem.Models;
using Nodify;
using QStandaedPlatform.Engine.Common.Common;
using QStandaedPlatform.Engine.Common.Common.Metadatas;
using QStandaedPlatform.Engine.Devices;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;

namespace Equipment.Bqjx.StandardPlatformSystem.WorkFlows
{
    public partial class NodeModel : ObservableObject
    {
        private readonly FlowModel _ownerFlowModel;
        public NodeModel(Tool ownerTool,FlowModel ownerFlowModel)
        {
            this.OwnerTool = ownerTool;
            this._ownerFlowModel = ownerFlowModel;

            this.OwnerTool.ToolStateChange += (state) =>
            {
                ToolState = state.State;
                ToolDescription = state.Description ?? string.Empty;
            };
            App.ToolEngine.OnPartCollectionChanged += () =>
            {
                UpdateRefPartProps();
            };
            ParameterModelRepository.ParameterTableChanged += (sender, e) => 
            {
                UpdateRefParameterProps();
            };
            this.OwnerTool.DynamicPinAddedCallback += (tool, pin) => 
            {
                if (pin.PinType == PinType.Input)
                {
                    this.InputPort?.InputPins.Add(pin);
                    this.InputPort?.MatchConnectPins.Add(new InPutMatchConnectPin { InputSourcePin = pin });
                    this.InputPort?.UpdateMatchConnectPins();
                    foreach (var outputPortModel in InputPort.ConnectedOutputPorts)
                    {
                        outputPortModel?.UpdateMatchConnectPins();
                    }
                }
                else
                {
                    this.OutputPort?.OutputPins.Add(pin);
                    this.OutputPort?.MatchConnectPins.Add(new OutputMatchConnectPin { OutputSourcePin = pin });
                    this.OutputPort?.UpdateMatchConnectPins();
                    foreach (var inputPortModel in OutputPort.ConnectedInputPorts)
                    {
                        inputPortModel?.UpdateMatchConnectPins();
                    }
                }
            };
            this.OwnerTool.DynamicPinRemovedCallback += (tool, pin) => 
            {
                if (pin.PinType == PinType.Input)
                {
                    var item = this.InputPort?.MatchConnectPins.FirstOrDefault(x => x.InputSourcePin == pin);
                    if (item != null)
                    {
                        foreach (var output in item.OutputTargetMatchPins)
                        {
                            output.OutputMatchConnected = false;
                        }
                        item.OutputPorts.Clear();
                        item.OutputTargetMatchPins.Clear();
                        this.InputPort?.InputPins.Remove(item.InputSourcePin);

                        this.InputPort?.MatchConnectPins.Remove(item);
                    }
                    this.InputPort?.UpdateMatchConnectPins();
                    foreach (var outputPortModel in InputPort.ConnectedOutputPorts)
                    {
                        outputPortModel?.UpdateMatchConnectPins();
                    }
                }
                else
                {
                    var item = this.OutputPort?.MatchConnectPins.FirstOrDefault(x => x.OutputSourcePin == pin);
                    if (item != null)
                    {
                        foreach (var input in item.InputTargetMatchPins)
                        {
                            input.InputMatchConnected = false;
                        }
                        item.InputPorts.Clear();
                        item.InputTargetMatchPins.Clear();
                        this.OutputPort?.OutputPins.Remove(item.OutputSourcePin);
                        this.OutputPort?.MatchConnectPins.Remove(item);
                    }
                    this.OutputPort?.UpdateMatchConnectPins();
                    foreach (var inputPortModel in OutputPort.ConnectedInputPorts)
                    {
                        inputPortModel?.UpdateMatchConnectPins();
                    }
                }
            };

            UpdateRefParameterProps();
        }

        private bool _isExpanded;
        public bool IsExpanded
        {
            get { return _isExpanded; }
            set => SetProperty(ref _isExpanded, value);
        }

        private string _Name;
        public string Name
        {
            get { return _Name; }
            set => SetProperty(ref _Name, value);
        }


        private string _displayName;

        public string DisplayName
        {
            get { return _displayName; }
            set
            {
                if (SetProperty(ref _displayName, value))
                {
                    if (OwnerTool != null)
                    {
                        OwnerTool.DisplayName = _displayName;
                    }
                }
            }
        }

        private string _ToolDescription;

        public string ToolDescription
        {
            get { return _ToolDescription; }
            set => SetProperty(ref _ToolDescription, value);
        }

        private ToolState _toolState;
        public ToolState ToolState
        {
            get => _toolState;
            set=>SetProperty(ref _toolState, value);
        }

        private Point _location;
        public Point Location
        {
            get => _location;
            set
            {
                if (SetProperty(ref _location, value))
                {
                    if (OwnerTool != null)
                    {
                        OwnerTool.ToolPosition = new QPoint { X = _location.X, Y = _location.Y };
                    }
                }
            }
        }
        public InputPortModel InputPort { get; set; }

        public OutputPortModel OutputPort { get; set; }

        public string UniqueId { get; set; }


        public ITool OwnerTool { get; set; }

        public ICommand ApplyContextChangeCommand => new RelayCommand(() =>
        {
            App.Current.Dispatcher.Invoke(() => 
            {
                var message = string.Empty;
                if (OwnerTool is DynamicPinTool dynamicPinTool)
                {
                    if (OwnerTool?.HandleContextChanged(dynamicPinTool.DataContext, out message) == false)
                    {
                        if (string.IsNullOrEmpty(message))
                        {
                            MessageBox.Show("参数有误");
                        }
                        else
                        {
                            MessageBox.Show($"参数有误<{message}>,请检查");
                        }
                    }
                    else
                    {
                        if (!string.IsNullOrEmpty(message))
                        {
                            MessageBox.Show(message);
                        }
                    }
                    return;
                }
                else if (OwnerTool is DynamicSyncInputPinTool dynamicSyncInputPinTool)
                {
                    if (OwnerTool?.HandleContextChanged(dynamicSyncInputPinTool.DataContext, out message) == false)
                    {
                        if (string.IsNullOrEmpty(message))
                        {
                            MessageBox.Show("参数有误");
                        }
                        else
                        {
                            MessageBox.Show($"参数有误<{message}>,请检查");
                        }
                    }
                    else
                    {
                        if (!string.IsNullOrEmpty(message))
                        {
                            MessageBox.Show(message);
                        }
                    }
                    return;
                }
                else if (OwnerTool is ModuleToolBase moduleToolBase)
                {
                    if (OwnerTool?.HandleContextChanged(moduleToolBase.DataContext, out message) == false)
                    {
                        if (string.IsNullOrEmpty(message))
                        {
                            MessageBox.Show("参数有误");
                        }
                        else
                        {
                            MessageBox.Show($"参数有误<{message}>,请检查");
                        }
                    }
                    else
                    {
                        if (!string.IsNullOrEmpty(message))
                        {
                            MessageBox.Show(message);
                        }
                    }
                    return;
                }
                else if (OwnerTool is SyncInputToolBase syncInputToolBase)
                {
                    if (OwnerTool?.HandleContextChanged(syncInputToolBase.DataContext, out message) == false)
                    {
                        if (string.IsNullOrEmpty(message))
                        {
                            MessageBox.Show("参数有误");
                        }
                        else
                        {
                            MessageBox.Show($"参数有误<{message}>,请检查");
                        }
                    }
                    else
                    {
                        if (!string.IsNullOrEmpty(message))
                        {
                            MessageBox.Show(message);
                        }
                    }
                    return;
                }
                if (OwnerTool?.HandleContextChanged(OwnerTool.DataContext, out message) == false)
                {
                    if (string.IsNullOrEmpty(message))
                    {
                        MessageBox.Show("参数有误");
                    }
                    else
                    {
                        MessageBox.Show($"参数有误<{message}>,请检查");
                    }
                }
                else
                {
                    if (!string.IsNullOrEmpty(message))
                    {
                        MessageBox.Show(message);
                    }
                }
            });
        });

        public void UpdateRefPartProps()
        {
            lock (RefPartProperties)
            {
                var refProps = _ownerFlowModel.Flow.GetRefPartProperties();
                foreach (var refPartProperty in refProps)
                {
                    if (RefPartProperties.Any(p => p.OwnerRef == refPartProperty))
                    {
                        var item = RefPartProperties.FirstOrDefault(p => p.OwnerRef == refPartProperty);
                        if (item != null)
                        {
                            foreach (var partMapper in App.ToolEngine.GetPartMappers())
                            {
                                if (item.RefPartPropertyModels.Any(p => p.OwnerPart == partMapper))
                                {
                                    continue;
                                }
                                if (refPartProperty.CanRef(partMapper))
                                {
                                    var refPartModel = new RefPartModel(partMapper);
                                    item.RefPartPropertyModels.Add(refPartModel);
                                }
                            }
                        }
                    }
                    else
                    {
                        if (refPartProperty.OwnerToolId == OwnerTool.UniqueId && refPartProperty.OwnerFlowId == _ownerFlowModel.FlowId)
                        {
                            var refPartPropertyModel = new RefPartPropertyModel
                            {
                                OwnerRef = refPartProperty,

                            };
                            foreach (var partMapper in App.ToolEngine.GetPartMappers())
                            {
                                if (refPartPropertyModel.RefPartPropertyModels.Any(p => p.OwnerPart == partMapper))
                                {
                                    continue;
                                }
                                if (refPartProperty.CanRef(partMapper))
                                {
                                    var refPartModel = new RefPartModel(partMapper);
                                    refPartPropertyModel.RefPartPropertyModels.Add(refPartModel);
                                }
                            }
                            RefPartProperties.Add(refPartPropertyModel);
                        }
                    }
                }
                foreach (var refPartProperty in RefPartProperties)
                {
                    if (refPartProperty.OwnerRef.PartId != Guid.Empty)
                    {
                        var item = refPartProperty.RefPartPropertyModels.FirstOrDefault(p => p.OwnerPart.PartId == refPartProperty.OwnerRef.PartId);
                        if (item != null) 
                        {
                            refPartProperty.RefPartModel = item;
                        }
                    }
                }
            }
        }


        public void UpdateRefParameterProps()
        {
            lock (RefParameterPropertyters)
            {
                RefParameterPropertyters.Clear();
                var refProps = _ownerFlowModel.Flow.GetRefParameterProperties();
                foreach (var refParameterProperty in refProps)
                {
                    if (!RefParameterPropertyters.Any(p => p.OwnerRef == refParameterProperty))
                    {
                        if (refParameterProperty.OwnerToolId == OwnerTool.UniqueId && refParameterProperty.OwnerFlowId == _ownerFlowModel.FlowId)
                        {
                            var refParameterPropertyModel = new RefParameterPropertyterModel
                            {
                                OwnerRef = refParameterProperty,
                                Parameters = ParameterModelRepository.GetParameterModels(refParameterProperty.ModuleTableType)
                            };
                            if (refParameterProperty.Parameter != null)
                            {
                                var item = refParameterPropertyModel.Parameters.FirstOrDefault(p => p.Parameter.ParameterId == refParameterProperty.Parameter.ParameterId);
                                if (item != null)
                                {
                                    refParameterPropertyModel.SelectedParameter = item;
                                }
                                else
                                {
                                    refParameterPropertyModel.SelectedParameter = null;
                                }
                            }
                            RefParameterPropertyters.Add(refParameterPropertyModel);
                        }
                        
                    }
                }
            }
        }


        [RelayCommand]
        public void DeleteRefPartProperty(RefPartPropertyModel refPartPropertyModel)
        {
            if (refPartPropertyModel != null)
            {
                if (_ownerFlowModel.Flow.RemoveRefPartProperty(refPartPropertyModel.OwnerRef))
                {
                    RefPartProperties.Remove(refPartPropertyModel);
                }
            }
        }


        [RelayCommand]
        public void DeleteRefPropertyProperty(RefParameterPropertyterModel refParameterPropertyterModel)
        {
            if (refParameterPropertyterModel != null)
            {
                if (_ownerFlowModel.Flow.RemoveRefParameterProperty(refParameterPropertyterModel.OwnerRef))
                    RefParameterPropertyters.Remove(refParameterPropertyterModel);
            }
        }

        public ObservableCollection<RefParameterPropertyterModel> RefParameterPropertyters { get; set; } = [];

        public ObservableCollection<RefPartPropertyModel> RefPartProperties { get; set; } = [];

        public ObservableCollection<TriggerPointModel>  TriggerPointModels { get; set; } = [];


    }
    public class TriggerPointModel : ObservableObject
    {
        public string CommandName { get; set; }

        public ToolTriggerCommand ToolTriggerCommand  { get; set; }
    }
    public class ToolTriggerCommand : ICommand
    {
        private readonly ITriggerPointCommand _triggerPointCommand;
        private readonly Tool _ownerTool;
        private readonly SemaphoreSlim _semaphore = new(1, 1);
        public ToolTriggerCommand(Tool ownerTool, ITriggerPointCommand triggerPointCommand)
        {
            _ownerTool = ownerTool;
            _triggerPointCommand = triggerPointCommand;
        }

        public event EventHandler? CanExecuteChanged;

        public bool CanExecute(object? parameter)
        {
            return _semaphore.CurrentCount > 0;
        }

        public async void Execute(object? parameter)
        {
            await ExecuteAsync(parameter);
        }

        public async Task ExecuteAsync(object? parameter)
        {
            await _semaphore.WaitAsync();
            try
            {
                var result = await _ownerTool.ExecuteCommandAsync(_triggerPointCommand).ConfigureAwait(false);
                if (result != null && !string.IsNullOrEmpty(result.ResultDescription))
                {
                    MessageBox.Show(result.ResultDescription);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("An error occurred: " + ex.ToString());
            }
            finally
            {
                _semaphore.Release();
                NotifyCanExecuteChanged();
            }
        }

        public void NotifyCanExecuteChanged()
        {
            App.Current.Dispatcher.Invoke(() => 
            {
                CanExecuteChanged?.Invoke(this, EventArgs.Empty);
            });
        }
    }
}
