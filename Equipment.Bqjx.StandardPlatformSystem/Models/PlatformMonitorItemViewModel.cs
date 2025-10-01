using CommunityToolkit.Mvvm.ComponentModel;
using Equipment.Bqjx.StandardPlatformSystem.WorkFlows;
using QStandaedPlatform.Engine.Common.Common;
using QStandaedPlatform.Engine.Components;
using QStandaedPlatform.Engine.Components.Modbus;
using QStandaedPlatform.Engine.Laboratory;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms.VisualStyles;

namespace Equipment.Bqjx.StandardPlatformSystem.Models
{
    public partial class PlatformMonitorItemViewModel:ObservableObject
    {
        private readonly PlatformMonitorItem _platformMonitorItem;
        public PlatformMonitorItemViewModel(PlatformMonitorItem platformMonitorItem)
        {
            _platformMonitorItem = platformMonitorItem;
            ModuleInfoParameterModels = ParameterModelRepository.SharedModuleInfoParameters;
            App.ToolEngine.OnPartCollectionChanged += () =>
            {
                var h5us = App.ToolEngine.GetPartMappers().Where(p => p.Part != null && p.As<H5uModbusTcp>() != null);
                RefPartModels = new ObservableCollection<RefPartModel>(h5us.Select(p => new RefPartModel(p)));
            };
            var h5us = App.ToolEngine.GetPartMappers().Where(p => p.Part != null && p.As<H5uModbusTcp>() != null);
            RefPartModels = new ObservableCollection<RefPartModel>(h5us.Select(p => new RefPartModel(p)));

            foreach (var item in EnumValuesProvider.GetEnumAll<ReadType>())
            {
                MonitorTypes.Add(item);
            }
            MonitorKey = _platformMonitorItem.MonitorKey;
            MonitorKeyDescription = _platformMonitorItem.MonitorKeyDescription;
            MonitorType = _platformMonitorItem.MonitorType;
            MonitorAddress = _platformMonitorItem.MonitorAddress;
            MonitorUnit = _platformMonitorItem.MonitorUnit;
            CharacterSize = _platformMonitorItem.CharacterSize;
            RefPartModel= RefPartModels.FirstOrDefault(p => p.OwnerPart.PartId == _platformMonitorItem.ClientId);
            MonitorModuleInfoParameterModel = ModuleInfoParameterModels.FirstOrDefault(p => p.ModuleInfoParameter.ParameterId == _platformMonitorItem.ModuleInfoId);
            MonitorType = _platformMonitorItem.MonitorType;
        }

        public PlatformMonitorItem Model => _platformMonitorItem;

        [ObservableProperty]
        private string _monitorKey = string.Empty;
        [ObservableProperty]
        private string _monitorKeyDescription=string.Empty;
        [ObservableProperty]
        private ReadType _monitorType;
        [ObservableProperty]
        private string _monitorAddress = string.Empty;
        [ObservableProperty]
        private string _monitorUnit = string.Empty;
        [ObservableProperty]
        private int _characterSize;
        [ObservableProperty]
        private ModuleInfoParameterModel? _monitorModuleInfoParameterModel;

        [ObservableProperty]
        private RefPartModel? _refPartModel;


        partial void OnCharacterSizeChanged(int value)
        {
           _platformMonitorItem.CharacterSize = value;
        }
        partial void OnMonitorAddressChanged(string value)
        {
            _platformMonitorItem.MonitorAddress = value;
        }
        partial void OnMonitorKeyChanged(string value)
        {
            _platformMonitorItem.MonitorKey = value;
        }

        partial void OnMonitorKeyDescriptionChanged(string value)
        {
            _platformMonitorItem.MonitorKeyDescription = value;
        }

        partial void OnMonitorModuleInfoParameterModelChanged(ModuleInfoParameterModel? value)
        {
            if (value != null)
            {
                _platformMonitorItem.ModuleInfoId = value.ModuleInfoParameter.ParameterId;
                _platformMonitorItem.ModuleName = value.ModuleInfoParameter.ModuleName;
            }
        }

        partial void OnMonitorTypeChanged(ReadType value)
        {
            _platformMonitorItem.MonitorType = value;
        }

        partial void OnRefPartModelChanged(RefPartModel? value)
        {
            if (value != null && value.OwnerPart != null)
            {
                _platformMonitorItem.ClientId = value.OwnerPart.PartId;
            }
        }

        public ObservableCollection<RefPartModel> RefPartModels { get; set; } = [];

        public ObservableCollection<ModuleInfoParameterModel> ModuleInfoParameterModels { get; set; } = [];

        public ObservableCollection<ReadType> MonitorTypes { get; set; } = [];
    }
}
