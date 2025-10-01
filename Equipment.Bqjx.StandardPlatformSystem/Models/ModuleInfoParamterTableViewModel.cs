using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using QStandaedPlatform.Engine.Laboratory;
using QStandaedPlatform.Engine.Laboratory.Documents;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

namespace Equipment.Bqjx.StandardPlatformSystem.Models
{
    public partial class ModuleInfoParamterTableViewModel : ObservableObject, IParamterTableChanged, ITableModel
    {
        private readonly ModuleInfoTable _moduleInfoTable;
        public BindingList<ModuleInfoParameterModel> Tables { get; set; } = [];

        public string TableName => _moduleInfoTable.Name;

        public ModuleInfoTable Table => _moduleInfoTable;

        public ICommand AddCommand => new RelayCommand(() => 
        {
            AddModuleInfoTable();
        });


        public ICommand DeleteCommand => new RelayCommand<ModuleInfoParameterModel>(parameter =>
        {
            if (parameter != null)
            {
                if (_moduleInfoTable.ModuleInfoParameters.Remove(parameter.ModuleInfoParameter))
                {
                    Tables.Remove(parameter);
                    ParamterTableChanged?.Invoke(this);
                }
            }
        });

        [ObservableProperty]
        private string _parameterDescription=string.Empty;
     

        public event ParamterTableChangedHandler? ParamterTableChanged;
        public event Action? Initialized;

        public ModuleInfoParamterTableViewModel(ModuleInfoTable moduleInfoTable)
        {
            _moduleInfoTable = moduleInfoTable;
        }

        public void AddModuleInfoTable()
        {
            var parameter = new ModuleInfoParameter { ModuleName = "未定义模块名称", ParameterId = Guid.NewGuid(), ModuleInfoId = Guid.NewGuid() };
            _moduleInfoTable.AddParameter(parameter);
            Tables.Add(new ModuleInfoParameterModel(parameter));
            ParamterTableChanged?.Invoke(this);
        }


        public void Add(ModuleInfoParameterModel moduleInfoParameterModel)
        {
            Tables.Add(moduleInfoParameterModel);
            ParamterTableChanged?.Invoke(this);
        }

        public void Initialize()
        {
            App.Current.Dispatcher.InvokeAsync(() =>
            {
                foreach (var item in _moduleInfoTable.ModuleInfoParameters)
                {
                    Tables.Add(new ModuleInfoParameterModel(item));
                }
                Initialized?.Invoke();
            }, System.Windows.Threading.DispatcherPriority.ContextIdle);
        }
    }

    public partial class ModuleFuncCodeParameterTableViewModel : ObservableObject, IParamterTableChanged, ITableModel
    {
        private readonly ModuleFuncCodeTable _moduleFuncCodeTable;
        public BindingList<ModuleFuncCodeParameterModel> Tables { get; set; } = [];

        public string TableName => _moduleFuncCodeTable.Name;

        public ModuleFuncCodeTable Table => _moduleFuncCodeTable;
        public ICommand AddCommand => new RelayCommand(() => 
        {
            AddModuleFuncCodeTable();
        });

        public ICommand DeleteCommand => new RelayCommand<ModuleFuncCodeParameterModel>(parameter => 
        {
            if (parameter != null)
            {
                if (_moduleFuncCodeTable.ModuleFuncCodeParameters.Remove(parameter.ModuleFuncCodeParameter))
                {
                    Tables.Remove(parameter);
                    ParamterTableChanged?.Invoke(this);
                }
            }
        });

        public ICommand CopyModuleInfoParameterCommand => new RelayCommand<ModuleFuncCodeParameterModel>(funcCodeParameterItem =>
        {
            if (funcCodeParameterItem != null)
            {
                var indexModel = Tables.IndexOf(funcCodeParameterItem);
                var index = _moduleFuncCodeTable.ModuleFuncCodeParameters.IndexOf(funcCodeParameterItem.ModuleFuncCodeParameter);
                var parameter = (ModuleFuncCodeParameter)funcCodeParameterItem.ModuleFuncCodeParameter.Clone();
                _moduleFuncCodeTable.ModuleFuncCodeParameters.Insert(index + 1, parameter);
                Tables.Insert(indexModel + 1, new ModuleFuncCodeParameterModel(parameter));
                ParamterTableChanged?.Invoke(this);
            }
        });

        public event ParamterTableChangedHandler? ParamterTableChanged;
        public event Action? Initialized;

        public ModuleFuncCodeParameterTableViewModel(ModuleFuncCodeTable moduleFuncCodeTable)
        {
            _moduleFuncCodeTable = moduleFuncCodeTable;
           
        }

        public void AddModuleFuncCodeTable()
        {
            var parameter = new ModuleFuncCodeParameter() { ParameterId = Guid.NewGuid() };
            _moduleFuncCodeTable.AddParameter(parameter);
            Tables.Add(new ModuleFuncCodeParameterModel(parameter));
            ParamterTableChanged?.Invoke(this);
        }


        public void Add(ModuleFuncCodeParameterModel moduleFuncCodeParameterModel)
        {
            Tables.Add(moduleFuncCodeParameterModel);
            ParamterTableChanged?.Invoke(this);
        }

        public void Initialize()
        {
            App.Current.Dispatcher.InvokeAsync(() =>
            {
                foreach (var item in _moduleFuncCodeTable.ModuleFuncCodeParameters)
                {
                    Tables.Add(new ModuleFuncCodeParameterModel(item));
                }
                Initialized?.Invoke();
            }, System.Windows.Threading.DispatcherPriority.Loaded); 
        }
    }
}
