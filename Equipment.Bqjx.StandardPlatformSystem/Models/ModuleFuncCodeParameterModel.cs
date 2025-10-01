using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Equipment.Bqjx.StandardPlatformSystem.Common;
using Equipment.Bqjx.StandardPlatformSystem.Views;
using Microsoft.Identity.Client;
using Microsoft.WindowsAPICodePack.Dialogs;
using QStandaedPlatform.Engine.Common.Common;
using QStandaedPlatform.Engine.Laboratory;
using QStandaedPlatform.Engine.Laboratory.Documents;
using System.Collections.ObjectModel;
using QStandaedPlatform.Engine.Extensions;
using System.IO;
using System.Text;
using System.Windows;
using System.Windows.Input;

namespace Equipment.Bqjx.StandardPlatformSystem.Models
{

    public partial class PrecisionInfoItemModel : ObservableObject
    {
        private readonly ModulePrecisionInfoItem _precisionInfoItem;

        [ObservableProperty]
        private string _precisionName = string.Empty;
        [ObservableProperty]
        private float _precisionStandardValue = 0f;
        [ObservableProperty]
        private string _precisionAddress = string.Empty;

        public PrecisionInfoItemModel(ModulePrecisionInfoItem precisionInfoItem)
        {
            _precisionInfoItem = precisionInfoItem;
            PrecisionName = _precisionInfoItem.PrecisionName;
            PrecisionStandardValue = _precisionInfoItem.PrecisionStandardValue;
            PrecisionAddress = _precisionInfoItem.PrecisionAddress;
        }
        public ModulePrecisionInfoItem OwnerInfo => _precisionInfoItem;


        partial void OnPrecisionNameChanged(string value)
        {
            _precisionInfoItem.PrecisionName = value;
        }
        partial void OnPrecisionStandardValueChanged(float value)
        {
            _precisionInfoItem.PrecisionStandardValue = value;
        }
        partial void OnPrecisionAddressChanged(string value)
        {
            _precisionInfoItem.PrecisionAddress = value;
        }
    }
    public partial class EbrInfoItemModel : ObservableObject
    {
        private readonly ModuleEbrInfoItem _ebrInfoItem;

        [ObservableProperty]
        private string _ebrName = string.Empty;
        [ObservableProperty]
        private string _ebrDescription = string.Empty;
        [ObservableProperty]
        private string _ebrAddress = string.Empty;
        [ObservableProperty]
        private EbrType _ebrType = EbrType.REAL;
        [ObservableProperty]
        private string _ebrUnit = string.Empty;
        [ObservableProperty]
        private int _ebrmoduleChannel = 1;
        [ObservableProperty]
        private int _characterLength = 0;

        public EbrInfoItemModel(ModuleEbrInfoItem ebrInfoItem)
        {
            _ebrInfoItem = ebrInfoItem;
            EbrName = _ebrInfoItem.EbrName;
            EbrDescription = _ebrInfoItem.EbrDescription;
            EbrAddress = _ebrInfoItem.EbrAddress;
            EbrType = _ebrInfoItem.EbrType;
            EbrUnit = _ebrInfoItem.EbrUnit;
            EbrmoduleChannel = _ebrInfoItem.ModuleChannel;
            CharacterLength = _ebrInfoItem.CharacterLength;
            foreach (var item in EnumValuesProvider.GetEnumAll<EbrType>())
            {
                EbrTypes.Add(item);
            }
        }
        public ObservableCollection<EbrType> EbrTypes { get; set; } = [];
        public ModuleEbrInfoItem OwnerInfo => _ebrInfoItem;

        partial void OnEbrNameChanged(string value)
        {
            _ebrInfoItem.EbrName = value;   
        }

        partial void OnEbrDescriptionChanged(string value)
        {
            _ebrInfoItem.EbrDescription = value;
        }

        partial void OnEbrAddressChanged(string value)
        {
            _ebrInfoItem.EbrAddress = value;
        }

        partial void OnCharacterLengthChanged(int value)
        {
            _ebrInfoItem.CharacterLength = value;
        }

        partial void OnEbrmoduleChannelChanged(int value)
        {
            _ebrInfoItem.ModuleChannel = value;
        }
        partial void OnEbrTypeChanged(EbrType value)
        {
            _ebrInfoItem.EbrType = value;
        }
        partial void OnEbrUnitChanged(string value)
        {
            _ebrInfoItem.EbrUnit = value;
        }


    }
    public partial class MonitorInfoItemModel : ObservableObject
    {
        private readonly ModuleMonitorInfoItem _monitorInfoItem;
        [ObservableProperty]
        private string _monitorName = string.Empty;
        [ObservableProperty]
        private string _monitorAddress = string.Empty;
        [ObservableProperty]
        private string _monitorDescription= string.Empty;

        public MonitorInfoItemModel(ModuleMonitorInfoItem moduleMonitorInfoItem)
        {
            _monitorInfoItem = moduleMonitorInfoItem;
            MonitorName = _monitorInfoItem.MonitorName;
            MonitorAddress = _monitorInfoItem.MonitorAddress;
            MonitorDescription = _monitorInfoItem.MonitorDescription;
        }
        public ModuleMonitorInfoItem OwnerInfo => _monitorInfoItem;

        partial void OnMonitorNameChanged(string value)
        {
            _monitorInfoItem.MonitorName = value;
        }
        partial void OnMonitorAddressChanged(string value)
        {
            _monitorInfoItem.MonitorAddress = value;
        }
        partial void OnMonitorDescriptionChanged(string value)
        {
            _monitorInfoItem.MonitorDescription = value;
        }

    }
    public partial class FuncCodeItemModel : ObservableObject
    {
        private readonly FuncCodeParameterInfo _funcCodeParamterInfo;

        [ObservableProperty]
        private string _parameterName = string.Empty;
        [ObservableProperty]
        private string _parameterAddress = string.Empty;
        [ObservableProperty]
        private string _parameterUnit = string.Empty;
        [ObservableProperty]
        private string _parameterDescription = string.Empty;

        [ObservableProperty]
        private string _parameterFeedbackAddress = string.Empty;

        [ObservableProperty]
        private float _parameterFeedbackDefaultValue = 0f;

        [ObservableProperty]
        private float _parameterMinValue = 0f;
        [ObservableProperty]
        private float _parameterMaxValue = 0f;
        [ObservableProperty]
        private float _parameterValue = 0f;
        public FuncCodeItemModel(FuncCodeParameterInfo funcCodeParamterInfo)
        {
            _funcCodeParamterInfo = funcCodeParamterInfo;
            ParameterName = _funcCodeParamterInfo.ParameterName;
            ParameterAddress = _funcCodeParamterInfo.ParameterAddress;
            ParameterUnit = _funcCodeParamterInfo.ParameterUnit;
            ParameterDescription = _funcCodeParamterInfo.ParameterDescription;
            ParameterFeedbackAddress = _funcCodeParamterInfo.ParameterFeedbackAddress;
            ParameterMinValue = _funcCodeParamterInfo.ParameterMinValue;
            ParameterMaxValue = _funcCodeParamterInfo.ParameterMaxValue;
            if (_funcCodeParamterInfo.ParameterValueFactory.TryGetValue("0", out var value))
            {
                ParameterValue = value;
            }
            if (funcCodeParamterInfo.ParameterId == 0)
            {
                funcCodeParamterInfo.ParameterId = SnowflakeIdGenerator.Instance.GenerateYitId();
            }
        }

        public FuncCodeParameterInfo  OwnerInfo => _funcCodeParamterInfo;

        partial void OnParameterNameChanged(string value)
        {
            
            _funcCodeParamterInfo.ParameterName = value;
        }

        partial void OnParameterAddressChanged(string value)
        {
            _funcCodeParamterInfo.ParameterAddress = value;
        }
        partial void OnParameterUnitChanged(string value)
        {
            _funcCodeParamterInfo.ParameterUnit = value;
        }
        partial void OnParameterDescriptionChanged(string value)
        {
            _funcCodeParamterInfo.ParameterDescription = value;
        }

        partial void OnParameterFeedbackAddressChanged(string value)
        {
            _funcCodeParamterInfo.ParameterFeedbackAddress = value;
        }

        partial void OnParameterFeedbackDefaultValueChanged(float value)
        {
            _funcCodeParamterInfo.ParameterFeedbackDefaultValue = value;
        }

        partial void OnParameterMinValueChanged(float value)
        {
            _funcCodeParamterInfo.ParameterMinValue = value;
        }
        partial void OnParameterMaxValueChanged(float value)
        {
            _funcCodeParamterInfo.ParameterMaxValue = value;
        }

        partial void OnParameterValueChanged(float value)
        {
            _funcCodeParamterInfo.ParameterValueFactory["0"] = value;
        }
    }
    public partial class ModuleFuncCodeParameterModel:ObservableObject, IParameterModel
    {
        private readonly ModuleFuncCodeParameter _moduleFuncCodeParameter;

        public ModuleFuncCodeParameterModel(ModuleFuncCodeParameter moduleFuncCodeParameter)
        {
            _moduleFuncCodeParameter = moduleFuncCodeParameter;
            this.ModuleInfoParameterModels = ParameterModelRepository.SharedModuleInfoParameters;
            App.Current.Dispatcher.InvokeAsync(() => 
            {
                foreach (var item in _moduleFuncCodeParameter.FuncCodeParamterInfos)
                {
                    FuncCodeItemModels.Add(new FuncCodeItemModel(item));
                }
            }, System.Windows.Threading.DispatcherPriority.ContextIdle);

            App.Current.Dispatcher.InvokeAsync(() => 
            {
                foreach (var item in _moduleFuncCodeParameter.MonitorInfoItems)
                {
                    MonitorInfoItemModels.Add(new MonitorInfoItemModel(item));
                }
            }, System.Windows.Threading.DispatcherPriority.ContextIdle);

            App.Current.Dispatcher.InvokeAsync(() =>
            {
                foreach (var item in this._moduleFuncCodeParameter.ChannelEbrInfos.Values.SelectMany(p => p))
                {
                    EbrInfoItemModels.Add(new EbrInfoItemModel(item));
                }
            }, System.Windows.Threading.DispatcherPriority.ContextIdle);

            Application.Current.Dispatcher.InvokeAsync(() => 
            {
                foreach (var item in this._moduleFuncCodeParameter.PrecisionInfoItems)
                {
                    PrecisionInfoItemModels.Add(new PrecisionInfoItemModel(item));
                }
            }, System.Windows.Threading.DispatcherPriority.ContextIdle);

            FuncCode = this._moduleFuncCodeParameter.FuncCode;
            FuncCodeDescription = this._moduleFuncCodeParameter.FuncCodeDescription;
            Openable = this._moduleFuncCodeParameter.Openable;
            RequiresParameter = this._moduleFuncCodeParameter.RequiresParameter;
            IsProductLegacy = _moduleFuncCodeParameter.IsProductLegacy;
            MonitorInterval = this._moduleFuncCodeParameter.MonitorInterval;
            EbrReadStartInterval = this._moduleFuncCodeParameter.EbrReadStartInterval;
            IsMonitorFuncCodeParameterFeedback = this._moduleFuncCodeParameter.IsMonitorFuncCodeParameterFeedback;
            SelectedModuleInfoParameter = this.ModuleInfoParameterModels.FirstOrDefault(x => x.ModuleInfoParameter.ModuleInfoId == _moduleFuncCodeParameter.ModuleInfoId);
            if (SelectedModuleInfoParameter != null)
            {
                ParameterDescription = $"{SelectedModuleInfoParameter.ModuleName}=>{moduleFuncCodeParameter.FuncCode}:{moduleFuncCodeParameter.FuncCodeDescription}";
            }
            else
            ParameterDescription = $"{moduleFuncCodeParameter.FuncCode}:{moduleFuncCodeParameter.FuncCodeDescription}";
            ParameterModelRepository.ParameterTableChanged += (sender, e) => 
            {
                this.ModuleInfoParameterModels = ParameterModelRepository.SharedModuleInfoParameters;
                SelectedModuleInfoParameter = this.ModuleInfoParameterModels.FirstOrDefault(x => x.ModuleInfoParameter.ModuleInfoId == _moduleFuncCodeParameter.ModuleInfoId);
            };
        }
        public ModuleFuncCodeParameter  ModuleFuncCodeParameter => _moduleFuncCodeParameter;
        [ObservableProperty]
        private int _funcCode;
        [ObservableProperty]
        private string _funcCodeDescription = string.Empty;
        [ObservableProperty]
        private ModuleInfoParameterModel? _selectedModuleInfoParameter;

        [ObservableProperty]
        private string _parameterDescription;
        [ObservableProperty]
        private bool _openable = false;
        [ObservableProperty]
        private bool _requiresParameter = true;

        [ObservableProperty]
        private bool _isMonitorFuncCodeParameterFeedback = false;

        [ObservableProperty]
        private bool _isProductLegacy= false;
        [ObservableProperty]
        private int _monitorInterval = 1000;
        [ObservableProperty]
        private int _ebrReadStartInterval = 1000;



        public IParameter Parameter => _moduleFuncCodeParameter;

        partial void OnIsProductLegacyChanged(bool value)
        {
            _moduleFuncCodeParameter.IsProductLegacy = value;
        }

        partial void OnRequiresParameterChanged(bool value)
        {
            _moduleFuncCodeParameter.RequiresParameter = value;
        }

        partial void OnIsMonitorFuncCodeParameterFeedbackChanged(bool value)
        {
           _moduleFuncCodeParameter.IsMonitorFuncCodeParameterFeedback = value;
        }

        partial void OnEbrReadStartIntervalChanged(int value)
        {
            _moduleFuncCodeParameter.EbrReadStartInterval = value;
        }

        partial void OnMonitorIntervalChanged(int value)
        {
            _moduleFuncCodeParameter.MonitorInterval = value;
        }

        partial void OnOpenableChanged(bool value)
        {
            _moduleFuncCodeParameter.Openable = value;
        }

        partial void OnFuncCodeChanged(int value)
        {
            _moduleFuncCodeParameter.FuncCode = value;
            if (SelectedModuleInfoParameter != null)
            {
                ParameterDescription = $"{SelectedModuleInfoParameter.ModuleName}=>{_moduleFuncCodeParameter.FuncCode}:{_moduleFuncCodeParameter.FuncCodeDescription}";
            }
            else
                ParameterDescription = $"{_moduleFuncCodeParameter.FuncCode}:{_moduleFuncCodeParameter.FuncCodeDescription}";
        }

        partial void OnFuncCodeDescriptionChanged(string value)
        {
            _moduleFuncCodeParameter.FuncCodeDescription = value;
            if (SelectedModuleInfoParameter != null)
            {
                ParameterDescription = $"{SelectedModuleInfoParameter.ModuleName}=>{_moduleFuncCodeParameter.FuncCode}:{_moduleFuncCodeParameter.FuncCodeDescription}";
            }
            else
                ParameterDescription = $"{_moduleFuncCodeParameter.FuncCode}:{_moduleFuncCodeParameter.FuncCodeDescription}";
        }
        

        partial void OnSelectedModuleInfoParameterChanged(ModuleInfoParameterModel? value)
        {
            if (value != null)
                _moduleFuncCodeParameter.ModuleInfoId = value.ModuleInfoParameter.ModuleInfoId;
        }

        public ICommand AddFuncCodeItemCommand => new RelayCommand(() =>
        {
            if (_moduleFuncCodeParameter.FuncCodeParamterInfos.Count > 0)
            {
                string? address = null;
                var lastInfo = _moduleFuncCodeParameter.FuncCodeParamterInfos.Last();
                if (!string.IsNullOrEmpty(lastInfo.ParameterAddress)
                && lastInfo.ParameterAddress.Length >= 4)
                {
                    var number = int.Parse(lastInfo.ParameterAddress.Substring(1, 3));
                    address = $"D{number + 2}";

                }
                var info = new FuncCodeParameterInfo()
                {
                    
                    ParameterAddress = address ?? $"D{100 + (_moduleFuncCodeParameter.FuncCodeParamterInfos.Count * 2)}"
                };
                _moduleFuncCodeParameter.FuncCodeParamterInfos.Add(info);
                FuncCodeItemModels.Add(new FuncCodeItemModel(info));
            }
            else
            {
                var info = new FuncCodeParameterInfo()
                {
                    ParameterAddress = $"D{100 + (_moduleFuncCodeParameter.FuncCodeParamterInfos.Count * 2)}"
                };
                _moduleFuncCodeParameter.FuncCodeParamterInfos.Add(info);
                FuncCodeItemModels.Add(new FuncCodeItemModel(info));
            }
        });

        public RelayCommand ImportFuncCodeItemCommand => new(() =>
        {
            App.Current.Dispatcher.InvokeAsync( () => 
            {
                using CommonOpenFileDialog dialog = new();

                dialog.Title = "请选择模块功能码参数 CSV 文件";
                dialog.DefaultExtension = ".csv";
                dialog.Filters.Add(new CommonFileDialogFilter("CSV 文件", "*.csv"));
                dialog.EnsureFileExists = true;


                if (dialog.ShowDialog() == CommonFileDialogResult.Ok)
                {
                    string? selectedPath = dialog.FileName;
                    if (!string.IsNullOrEmpty(selectedPath))
                    {

                        string[] lines = File.ReadAllLines(selectedPath, Encoding.Default).Skip(1).ToArray();
                        foreach (var line in lines)
                        {
                            string[] columns = line.Split(',');
                            if (columns.Length >= 2)
                            {
                                if (!string.IsNullOrEmpty(columns[0]) && !string.IsNullOrEmpty(columns[1]))
                                {
                                    var info = new FuncCodeParameterInfo()
                                    {
                                        ParameterAddress = columns[0],
                                        ParameterName = columns[1]
                                    };
                                    _moduleFuncCodeParameter.FuncCodeParamterInfos.Add(info);
                                    FuncCodeItemModels.Add(new FuncCodeItemModel(info));
                                }
                            }
                        }
                        MessageBox.Show("模块功能码参数导入成功");
                    }
                }
            });
        });

        public RelayCommand ExportFuncCodeItemCommand => new(() =>
        {
            App.Current.Dispatcher.InvokeAsync(() =>
            { 
                if(FuncCodeItemModels != null)
                {
                    
                  CSVHelper.ExportToCsv(FuncCodeItemModels.ToList(), $"{_moduleFuncCodeParameter.FuncCodeDescription}_{_moduleFuncCodeParameter.FuncCode}_Params");
                    
                }
                
            });
        });

        public ICommand ImportMonitorInfoItemCommand => new RelayCommand(() => 
        {
            App.Current.Dispatcher.InvokeAsync( () =>
            {
                using (CommonOpenFileDialog dialog = new())
                {

                    dialog.Title = "请选择模块监控参数 CSV 文件";
                    dialog.DefaultExtension = ".csv";
                    dialog.Filters.Add(new CommonFileDialogFilter("CSV 文件", "*.csv"));
                    dialog.EnsureFileExists = true;

                    if (dialog.ShowDialog() == CommonFileDialogResult.Ok)
                    {
                        string? selectedPath = dialog.FileName;
                        if (!string.IsNullOrEmpty(selectedPath))
                        {

                            string[] lines = File.ReadAllLines(selectedPath, Encoding.Default).Skip(1).ToArray();
                            foreach (var line in lines)
                            {
                                string[] columns = line.Split(',');
                                if (columns.Length >= 2)
                                {
                                    if (!string.IsNullOrEmpty(columns[0]) && !string.IsNullOrEmpty(columns[1]))
                                    {
                                        var info = new ModuleMonitorInfoItem() 
                                        {
                                             MonitorAddress= columns[0],
                                             MonitorName= columns[1],
                                        };
                                        _moduleFuncCodeParameter.MonitorInfoItems.Add(info);
                                        MonitorInfoItemModels.Add(new MonitorInfoItemModel(info));
                                    }
                                }
                            }
                            MessageBox.Show("模块监控参数导入成功");
                        }
                    }
                }
            });
        });
        public ICommand ImportEbrInfoItemCommand => new RelayCommand(() => 
        {
            App.Current.Dispatcher.InvokeAsync(() =>
            {
                using (CommonOpenFileDialog dialog = new())
                {

                    dialog.Title = "请选择模块EBR参数 CSV 文件";
                    dialog.DefaultExtension = ".csv";
                    dialog.Filters.Add(new CommonFileDialogFilter("CSV 文件", "*.csv"));
                    dialog.EnsureFileExists = true;

                    if (dialog.ShowDialog() == CommonFileDialogResult.Ok)
                    {
                        string? selectedPath = dialog.FileName;
                        if (!string.IsNullOrEmpty(selectedPath))
                        {

                            string[] lines = File.ReadAllLines(selectedPath, Encoding.Default).Skip(1).ToArray();
                            foreach (var line in lines)
                            {
                                string[] columns = line.Split(',');
                                if (columns.Length >= 2)
                                {
                                    if (!string.IsNullOrEmpty(columns[0]) && !string.IsNullOrEmpty(columns[1]))
                                    {
                                        var info = new ModuleEbrInfoItem() 
                                        {
                                             EbrAddress = columns[0],
                                             EbrName = columns[1]
                                        };
                                        if (_moduleFuncCodeParameter.ChannelEbrInfos.TryGetValue(info.ModuleChannel, out List<ModuleEbrInfoItem>? value))
                                        {
                                            value.Add(info);
                                        }
                                        else
                                        {
                                            _moduleFuncCodeParameter.ChannelEbrInfos.Add(info.ModuleChannel, [info]);
                                        }
                                        EbrInfoItemModels.Add(new EbrInfoItemModel(info));
                                    }
                                }
                            }
                            MessageBox.Show("模块EBR参数导入成功");
                        }
                    }
                }
            });
        });

        public ICommand ImportPrecisionInfoItemCommand => new RelayCommand(() => 
        {
            App.Current.Dispatcher.InvokeAsync(() =>
            {
                using (CommonOpenFileDialog dialog = new())
                {

                    dialog.Title = "请选择模块精度参数 CSV 文件";
                    dialog.DefaultExtension = ".csv";
                    dialog.Filters.Add(new CommonFileDialogFilter("CSV 文件", "*.csv"));
                    dialog.EnsureFileExists = true;

                    if (dialog.ShowDialog() == CommonFileDialogResult.Ok)
                    {
                        string? selectedPath = dialog.FileName;
                        if (!string.IsNullOrEmpty(selectedPath))
                        {

                            string[] lines = File.ReadAllLines(selectedPath, Encoding.Default).Skip(1).ToArray();
                            foreach (var line in lines)
                            {
                                string[] columns = line.Split(',');
                                if (columns.Length >= 2)
                                {
                                    if (!string.IsNullOrEmpty(columns[0]) && !string.IsNullOrEmpty(columns[1]))
                                    {
                                        var info = new ModulePrecisionInfoItem() 
                                        {
                                             PrecisionAddress= columns[0],
                                             PrecisionName= columns[1],
                                             PrecisionStandardValue=float.Parse(columns[2])
                                        };
                                        _moduleFuncCodeParameter.PrecisionInfoItems.Add(info);
                                        PrecisionInfoItemModels.Add(new PrecisionInfoItemModel(info));
                                    }
                                }
                            }
                            MessageBox.Show("模块精度参数导入成功");
                        }
                    }
                }
            });
        });

        public ICommand RemoveFuncCodeItemCommand => new RelayCommand<FuncCodeItemModel>(item => 
        {
            if (item != null)
            {
                if (_moduleFuncCodeParameter.FuncCodeParamterInfos.Remove(item.OwnerInfo))
                {
                    FuncCodeItemModels.Remove(item);
                }
            }
        });

        public ICommand AddMonitorInfoItemCommand => new RelayCommand(() => 
        {
            var info = new ModuleMonitorInfoItem();
            _moduleFuncCodeParameter.MonitorInfoItems.Add(info);
            MonitorInfoItemModels.Add(new MonitorInfoItemModel(info));
        });

        public ICommand RemoveMonitorInfoItemCommand => new RelayCommand<MonitorInfoItemModel>(item => 
        {
            if (item != null)
            {
                if (_moduleFuncCodeParameter.MonitorInfoItems.Remove(item.OwnerInfo))
                {
                    MonitorInfoItemModels.Remove(item);
                }
            }
        });

        public ICommand AddEbrInfoItemCommand => new RelayCommand(() => 
        {
            var info = new ModuleEbrInfoItem();
            EbrInfoItemModels.Add(new EbrInfoItemModel(info));
        });

        public ICommand RemoveEbrInfoItemCommand => new RelayCommand<EbrInfoItemModel>(item => 
        {
            if (item != null)
            {
                if (_moduleFuncCodeParameter.ChannelEbrInfos.TryGetValue(item.OwnerInfo.ModuleChannel, out var moduleEbrInfoItems))
                {
                    if (moduleEbrInfoItems.Remove(item.OwnerInfo))
                    {
                        EbrInfoItemModels.Remove(item);
                    }
                    else
                    {
                        EbrInfoItemModels.Remove(item);
                    }
                }
                else
                {
                    EbrInfoItemModels.Remove(item);
                }
            }
        });

        public ICommand AddPrecisionInfoItemCommand => new RelayCommand(() =>
        {
            var info = new ModulePrecisionInfoItem();
            _moduleFuncCodeParameter.PrecisionInfoItems.Add(info);
            PrecisionInfoItemModels.Add(new PrecisionInfoItemModel(info));
        });
        public ICommand RemovePrecisionInfoItemCommand => new RelayCommand<PrecisionInfoItemModel>(item =>
        {
            if (item != null)
            {
                if (_moduleFuncCodeParameter.PrecisionInfoItems.Remove(item.OwnerInfo))
                {
                    PrecisionInfoItemModels.Remove(item);
                }
            }
        });
        public ICommand OpenEditModuleFuncCodeParameterViewCommand => new RelayCommand<ModuleFuncCodeParameterModel>(moduleFuncCodeParameter => 
        {
        if (moduleFuncCodeParameter != null)
        {
                new Action(() =>
                {
                    EditModuleFuncCodeParameterView editModuleFuncCodeParameterView = new EditModuleFuncCodeParameterView();
                    editModuleFuncCodeParameterView.Owner = App.Current.MainWindow;
                    editModuleFuncCodeParameterView.Datas = moduleFuncCodeParameter.FuncCodeItemModels;
                    editModuleFuncCodeParameterView.DeleteItemCommand = RemoveFuncCodeItemCommand;
                    editModuleFuncCodeParameterView.InsertItemCommand = AddFuncCodeItemCommand;
                    editModuleFuncCodeParameterView.ImportItemCommand = ImportFuncCodeItemCommand;
                    editModuleFuncCodeParameterView.ExportItemCommand = ExportFuncCodeItemCommand;
                    editModuleFuncCodeParameterView.ShowDialog();
             
                }).TryCatch(ex => 
                {
                    MessageBox.Show(ex.Message);
                });
            }
        });

        public ICommand OpenEditModuleMonitorInfoItemViewCommand => new RelayCommand<ModuleFuncCodeParameterModel>(moduleFuncCodeParameter => 
        {
            if (moduleFuncCodeParameter != null)
            {
                new Action(() =>
                {
                    EditModuleFuncCodeParameterView editModuleFuncCodeParameterView = new()
                    {
                        Owner = App.Current.MainWindow,
                        Datas = moduleFuncCodeParameter.MonitorInfoItemModels,
                        DeleteItemCommand = RemoveMonitorInfoItemCommand,
                        InsertItemCommand = AddMonitorInfoItemCommand,
                        ImportItemCommand = ImportMonitorInfoItemCommand
                    };

                    editModuleFuncCodeParameterView.ShowDialog();

                }).TryCatch(ex =>
                {
                    MessageBox.Show(ex.Message);
                });
            }
        });

        public ICommand OpenEditModuleEbrInfoItemViewCommand => new RelayCommand<ModuleFuncCodeParameterModel>(moduleFuncCodeParameter => 
        {
            if (moduleFuncCodeParameter != null)
            {
                new Action(() =>
                {
                    EditModuleFuncCodeParameterView editModuleFuncCodeParameterView = new()
                    {
                        Owner = App.Current.MainWindow,
                        Datas = moduleFuncCodeParameter.EbrInfoItemModels,
                        DeleteItemCommand = RemoveEbrInfoItemCommand,
                        InsertItemCommand = AddEbrInfoItemCommand,
                        ImportItemCommand = ImportEbrInfoItemCommand
                    };
                    editModuleFuncCodeParameterView.Closed += (sender, args) => 
                    {
                        if (EbrInfoItemModels.Count > 0)
                        {
                            foreach (var item in _moduleFuncCodeParameter.ChannelEbrInfos)
                            {
                                item.Value.Clear();
                            }
                            foreach (var model in EbrInfoItemModels)
                            {
                                if (model.OwnerInfo != null)
                                {
                                    if (_moduleFuncCodeParameter.ChannelEbrInfos.TryGetValue(model.OwnerInfo.ModuleChannel, out List<ModuleEbrInfoItem>? moduleEbrInfoItems))
                                    {
                                        moduleEbrInfoItems?.Add(model.OwnerInfo);
                                    }
                                    else
                                    {
                                        _moduleFuncCodeParameter.ChannelEbrInfos.Add(model.OwnerInfo.ModuleChannel, [model.OwnerInfo]);
                                    }
                                }
                            }
                        }
                    };
                    editModuleFuncCodeParameterView.ShowDialog();

                }).TryCatch(ex =>
                {
                    MessageBox.Show(ex.Message);
                });
            }
        });
        public ICommand OpenEditModulePrecisionInfoItemViewCommand => new RelayCommand<ModuleFuncCodeParameterModel>(moduleFuncCodeParameter => 
        {
            if (moduleFuncCodeParameter != null)
            {
                new Action(() =>
                {
                    EditModuleFuncCodeParameterView editModuleFuncCodeParameterView = new()
                    {
                        Owner = App.Current.MainWindow,
                        Datas = moduleFuncCodeParameter.PrecisionInfoItemModels,
                        DeleteItemCommand = RemovePrecisionInfoItemCommand,
                        InsertItemCommand = AddPrecisionInfoItemCommand,
                        ImportItemCommand = ImportPrecisionInfoItemCommand,
                    };
                    editModuleFuncCodeParameterView.ShowDialog();

                }).TryCatch(ex =>
                {
                    MessageBox.Show(ex.Message);
                });
            }
        });

        public ObservableCollection<FuncCodeItemModel> FuncCodeItemModels { get; set; } = [];

        public ObservableCollection<MonitorInfoItemModel> MonitorInfoItemModels { get; set; } = [];

        public ObservableCollection<EbrInfoItemModel> EbrInfoItemModels { get; set; } = [];

        public ObservableCollection<PrecisionInfoItemModel> PrecisionInfoItemModels { get; set; } = [];
        public ObservableCollection<ModuleInfoParameterModel> ModuleInfoParameterModels { get; set; }

        public IParameterTable Table => ParameterModelRepository.ModuleFuncCodeParameterTableViewModel.Table;
    }
}