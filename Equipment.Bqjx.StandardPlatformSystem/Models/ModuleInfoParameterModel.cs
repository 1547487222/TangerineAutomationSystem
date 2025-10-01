using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Equipment.Bqjx.StandardPlatformSystem.WorkFlows;
using Microsoft.WindowsAPICodePack.Dialogs;
using QStandaedPlatform.Engine.Common.Common;
using QStandaedPlatform.Engine.Components.Modbus;
using QStandaedPlatform.Engine.Laboratory;
using QStandaedPlatform.Engine.Laboratory.Documents;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace Equipment.Bqjx.StandardPlatformSystem.Models
{
    public partial class AlarmItemModel : ObservableObject
    {
        private readonly ModuleAlarmItem _alarmItem;

        [ObservableProperty]
        private string alarmAddress=string.Empty;
        [ObservableProperty]
        private string alarmDescription=string.Empty;
        public AlarmItemModel(ModuleAlarmItem alarmItem)
        {
            _alarmItem = alarmItem;
            AlarmAddress = alarmItem.AlarmAddress;
            AlarmDescription = alarmItem.AlarmDescription;
        }

        public ModuleAlarmItem AlarmItem => _alarmItem;

        partial void OnAlarmAddressChanged(string value)
        {
            _alarmItem.AlarmAddress = value;
        }
        partial void OnAlarmDescriptionChanged(string value)
        {
            _alarmItem.AlarmDescription = value;
        }
    }
    public partial class ModuleInfoParameterModel : ObservableObject, IParameterModel
    {
        private readonly ModuleInfoParameter _moduleInfoParameter;

        public ModuleInfoParameterModel(ModuleInfoParameter moduleInfoParameter)
        {
            _moduleInfoParameter = moduleInfoParameter;
            _moduleName = moduleInfoParameter.ModuleName;
            _moduleDescription = moduleInfoParameter.ModuleDescription;
            _moduleSpec = moduleInfoParameter.ModuleSpec;
            _moduleSerialNumber = moduleInfoParameter.ModuleSerialNumber;
            _moduleIdentifier = moduleInfoParameter.ModuleIdentifier;
            _moduleFuncCodeAddress = moduleInfoParameter.ModuleFuncCodeAddress;
            _moduleFuncStateCodeAddress = moduleInfoParameter.ModuleFuncStateCodeAddress;
            _moduleStateAddress = moduleInfoParameter.ModuleStateAddress;
            _moduleParameterAddress = moduleInfoParameter.ModuleParameterAddress;
            _moduleObserveDataAddress = moduleInfoParameter.ModuleObserveDataAddress;
            _moduleHomeControlAddress = moduleInfoParameter.ModuleHomeControlAddress;
            _moduleHomeStateAddress = moduleInfoParameter.ModuleHomeStateAddress;
            _moduleResetControlAddress = moduleInfoParameter.ModuleResetControlAddress;
            _moduleStopControlAddress = moduleInfoParameter.ModuleStopControlAddress;
            _moduleEmergencyControlAddress = moduleInfoParameter.ModuleEmergencyControlAddress;
            _moduleStartControlAddress = moduleInfoParameter.ModuleStartControlAddress;
            _modulePauseControlAddress = moduleInfoParameter.ModulePauseControlAddress;
            _moduleManualAutoControlAddress = moduleInfoParameter.ModuleManualAutoControlAddress;
            _moduleAlrmAddress = moduleInfoParameter.ModuleAlrmAddress;
            _moduleAlrmAddressLength = moduleInfoParameter.ModuleAlrmAddressLength; 
            DisplayModuleName = $"{_moduleInfoParameter.ModuleSpec}{_moduleInfoParameter.ModuleName}";
            ParameterDescription = DisplayModuleName;
            App.Current.Dispatcher.InvokeAsync(() => 
            {
                foreach (var item in _moduleInfoParameter.AlarmItems)
                {
                    AlarmItemModels.Add(new AlarmItemModel(item));
                }
            }, System.Windows.Threading.DispatcherPriority.ContextIdle);

            App.ToolEngine.OnPartCollectionChanged += () =>
            {
                var h5us = App.ToolEngine.GetPartMappers().Where(p => p.Part != null && p.As<H5uModbusTcp>() != null);
                RefPartModels = new ObservableCollection<RefPartModel>(h5us.Select(p => new RefPartModel(p)));
            };
            var h5us = App.ToolEngine.GetPartMappers().Where(p => p.Part != null && p.As<H5uModbusTcp>() != null);
            RefPartModels = new ObservableCollection<RefPartModel>(h5us.Select(p => new RefPartModel(p)));

            RefPartModel = RefPartModels.FirstOrDefault(p => p.OwnerPart.PartId == _moduleInfoParameter.ModuleControllerId);
        }

        public RelayCommand ImportAlarmItemCommand => new RelayCommand(() => 
        {
            using (CommonOpenFileDialog dialog = new())
            {

                dialog.Title = "请选择模块报警 CSV 文件";
                dialog.DefaultExtension = ".csv";
                dialog.Filters.Add(new CommonFileDialogFilter("CSV 文件", "*.csv"));
                dialog.EnsureFileExists = true;

                if (dialog.ShowDialog() == CommonFileDialogResult.Ok)
                {
                    string? selectedPath = dialog.FileName;
                    if (!string.IsNullOrEmpty(selectedPath))
                    {

                        string[] lines = File.ReadAllLines(selectedPath,Encoding.Default).Skip(1).ToArray();
                        foreach (var line in lines)
                        {
                            string[] columns = line.Split(',');
                            if (columns.Length >= 2)
                            {
                                if (!string.IsNullOrEmpty(columns[0]) && !string.IsNullOrEmpty(columns[1]))
                                {
                                    var alarmItem = new ModuleAlarmItem
                                    {
                                        AlarmAddress = columns[1],
                                        AlarmDescription = columns[0]
                                    };
                                    _moduleInfoParameter.AlarmItems.Add(alarmItem);
                                    AlarmItemModels.Add(new AlarmItemModel(alarmItem));
                                }
                            }
                        }
                        MessageBox.Show("模块报警信息导入成功");
                    }
                }
            }
        });

        public RelayCommand<AlarmItemModel> RemoveAlarmItemCommand => new RelayCommand<AlarmItemModel>(AlarmItem => 
        {
            if (AlarmItem != null) 
            {
                if (_moduleInfoParameter.AlarmItems.Remove(AlarmItem.AlarmItem))
                {
                    AlarmItemModels.Remove(AlarmItem);
                }
            }
        });

        public ObservableCollection<AlarmItemModel> AlarmItemModels { get; set; } = [];

        [ObservableProperty]
        private RefPartModel? _refPartModel;


        partial void OnRefPartModelChanged(RefPartModel? value)
        {
            if (value != null && value.OwnerPart != null)
            {
                _moduleInfoParameter.ModuleControllerId = value.OwnerPart.PartId;
            }
        }
        public ObservableCollection<RefPartModel> RefPartModels { get; set; } = [];

        private string _DisplayModuleName = string.Empty;

        public string DisplayModuleName
        {
            get { return _DisplayModuleName; }
            set
            {
                if (SetProperty(ref _DisplayModuleName, value))
                {
                    ParameterDescription = DisplayModuleName;
                }
            }
        }
        [ObservableProperty]
        private string _parameterDescription;


        public ModuleInfoParameter ModuleInfoParameter => _moduleInfoParameter;

        public IParameter Parameter => _moduleInfoParameter;

        public IParameterTable Table => ParameterModelRepository.ModuleParamterTableViewModel.Table;

        [ObservableProperty]
        private string _moduleName;
        partial void OnModuleNameChanged(string value) 
        {
            _moduleInfoParameter.ModuleName = value;

            DisplayModuleName = $"{_moduleInfoParameter.ModuleSpec}{_moduleInfoParameter.ModuleName}";
        }

        [ObservableProperty]
        private string _moduleDescription;
        partial void OnModuleDescriptionChanged(string value) => _moduleInfoParameter.ModuleDescription = value;


        [ObservableProperty]
        private string _moduleSerialNumber;
        partial void OnModuleSerialNumberChanged(string value) => _moduleInfoParameter.ModuleSerialNumber = value;

        [ObservableProperty]
        private string _moduleIdentifier;
        partial void OnModuleIdentifierChanged(string value) => _moduleInfoParameter.ModuleIdentifier = value;

        [ObservableProperty]
        private string _moduleSpec;
        partial void OnModuleSpecChanged(string value) 
        {
            _moduleInfoParameter.ModuleSpec = value;
            DisplayModuleName = $"{_moduleInfoParameter.ModuleSpec}{_moduleInfoParameter.ModuleName}";
        }

        [ObservableProperty]
        private string _moduleFuncCodeAddress;
        partial void OnModuleFuncCodeAddressChanged(string value) => _moduleInfoParameter.ModuleFuncCodeAddress = value;

        [ObservableProperty]
        private string _moduleFuncStateCodeAddress;
        partial void OnModuleFuncStateCodeAddressChanged(string value) => _moduleInfoParameter.ModuleFuncStateCodeAddress = value;

        [ObservableProperty]
        private string _moduleStateAddress;
        partial void OnModuleStateAddressChanged(string value) => _moduleInfoParameter.ModuleStateAddress = value;

        [ObservableProperty]
        private string _moduleParameterAddress;
        partial void OnModuleParameterAddressChanged(string value) => _moduleInfoParameter.ModuleParameterAddress = value;

        [ObservableProperty]
        private string _moduleObserveDataAddress;
        partial void OnModuleObserveDataAddressChanged(string value) => _moduleInfoParameter.ModuleObserveDataAddress = value;

        [ObservableProperty]
        private string _moduleHomeControlAddress;
        partial void OnModuleHomeControlAddressChanged(string value) => _moduleInfoParameter.ModuleHomeControlAddress = value;

        [ObservableProperty]
        private string _moduleHomeStateAddress;
        partial void OnModuleHomeStateAddressChanged(string value) => _moduleInfoParameter.ModuleHomeStateAddress = value;

        [ObservableProperty]
        private string _moduleResetControlAddress;
        partial void OnModuleResetControlAddressChanged(string value) => _moduleInfoParameter.ModuleResetControlAddress = value;

        [ObservableProperty]
        private string _moduleStopControlAddress;
        partial void OnModuleStopControlAddressChanged(string value) => _moduleInfoParameter.ModuleStopControlAddress = value;

        [ObservableProperty]
        private string _moduleEmergencyControlAddress;
        partial void OnModuleEmergencyControlAddressChanged(string value) => _moduleInfoParameter.ModuleEmergencyControlAddress = value;

        [ObservableProperty]
        private string _moduleStartControlAddress;
        partial void OnModuleStartControlAddressChanged(string value) => _moduleInfoParameter.ModuleStartControlAddress = value;

        [ObservableProperty]
        private string _modulePauseControlAddress;
        partial void OnModulePauseControlAddressChanged(string value) => _moduleInfoParameter.ModulePauseControlAddress = value;

        [ObservableProperty]
        private string _moduleManualAutoControlAddress;
        partial void OnModuleManualAutoControlAddressChanged(string value) => _moduleInfoParameter.ModuleManualAutoControlAddress = value;

        [ObservableProperty]
        private string _moduleAlrmAddress;
        partial void OnModuleAlrmAddressChanged(string value) => _moduleInfoParameter.ModuleAlrmAddress = value;

        [ObservableProperty]
        private int _moduleAlrmAddressLength;
        partial void OnModuleAlrmAddressLengthChanged(int value) => _moduleInfoParameter.ModuleAlrmAddressLength = value;
    }
}
