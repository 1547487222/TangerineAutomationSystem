using QStandaedPlatform.Engine.Common.Common;
using QStandaedPlatform.Engine.Laboratory.Documents;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Equipment.Bqjx.StandardPlatformSystem.Models
{
    public static class ParameterModelRepository
    {

       private static readonly Dictionary<Type, ObservableCollection<IParameterModel>> _parameterModels = [];
        public static ObservableCollection<ModuleInfoParameterModel> SharedModuleInfoParameters { get; private set; }
       = [];
        public static ModuleInfoParamterTableViewModel ModuleParamterTableViewModel { get; private set; }

        public static ModuleFuncCodeParameterTableViewModel ModuleFuncCodeParameterTableViewModel { get; private set; } 

        public static LabTrayParameterTableViewModel LabTrayParameterTableViewModel { get; private set; }

        public static ModuleChannelGroupParameterTableModel ModuleChannelGroupParameterTableModel { get; private set; }
         static ParameterModelRepository()
        {
            SharedModuleInfoParameters.Clear();
            ModuleParamterTableViewModel = new ModuleInfoParamterTableViewModel(ParameterTableManager.ModuleInfoTable);
            ModuleParamterTableViewModel.Initialized += () => 
            {
                SharedModuleInfoParameters.Clear();
                foreach (var m in ModuleParamterTableViewModel.Tables)
                    SharedModuleInfoParameters.Add(m);
                ParameterTableChanged?.Invoke(ModuleParamterTableViewModel, EventArgs.Empty);
                if (_parameterModels.TryGetValue(typeof(ModuleInfoTable), out var parameterModels))
                {
                    parameterModels.Clear();
                    foreach (var moduleInfoParameter in ModuleParamterTableViewModel.Tables)
                    {
                        parameterModels.Add(moduleInfoParameter);
                    }
                }
            };
            ModuleParamterTableViewModel.Initialize();
            
            ModuleParamterTableViewModel.ParamterTableChanged += (sender) =>
            {
                SharedModuleInfoParameters.Clear();
                foreach (var m in ModuleParamterTableViewModel.Tables)
                    SharedModuleInfoParameters.Add(m);
                ParameterTableChanged?.Invoke(ModuleParamterTableViewModel, EventArgs.Empty);
                if (_parameterModels.TryGetValue(typeof(ModuleInfoTable), out var parameterModels))
                {
                    parameterModels.Clear();
                    foreach (var moduleInfoParameter in ModuleParamterTableViewModel.Tables)
                    {
                        parameterModels.Add(moduleInfoParameter);
                    }
                }
            };
            var moduleInfoParameters = new ObservableCollection<IParameterModel>();
            foreach (var moduleInfoParameter in ModuleParamterTableViewModel.Tables)
            {
                moduleInfoParameters.Add(moduleInfoParameter);
            }
            _parameterModels[typeof(ModuleInfoTable)] = moduleInfoParameters;

            ModuleFuncCodeParameterTableViewModel = new ModuleFuncCodeParameterTableViewModel(ParameterTableManager.ModuleFuncCodeTable);
            ModuleFuncCodeParameterTableViewModel.Initialized += () => 
            {
                ParameterTableChanged?.Invoke(ModuleFuncCodeParameterTableViewModel, EventArgs.Empty);
                if (_parameterModels.TryGetValue(typeof(ModuleFuncCodeTable), out var parameterModels))
                {
                    parameterModels.Clear();
                    foreach (var moduleFuncCodeParameter in ModuleFuncCodeParameterTableViewModel.Tables)
                    {
                        parameterModels.Add(moduleFuncCodeParameter);
                    }
                }
            };
            ModuleFuncCodeParameterTableViewModel.Initialize();
            ModuleFuncCodeParameterTableViewModel.ParamterTableChanged += (sender) =>
            {
                ParameterTableChanged?.Invoke(ModuleFuncCodeParameterTableViewModel, EventArgs.Empty);
                if (_parameterModels.TryGetValue(typeof(ModuleFuncCodeTable), out var parameterModels))
                {
                    parameterModels.Clear();
                    foreach (var moduleFuncCodeParameter in ModuleFuncCodeParameterTableViewModel.Tables)
                    {
                        parameterModels.Add(moduleFuncCodeParameter);
                    }
                }
            };

            var moduleFuncCodeParameters = new ObservableCollection<IParameterModel>();
            foreach (var moduleFuncCodeParameter in ModuleFuncCodeParameterTableViewModel.Tables)
            {
                moduleFuncCodeParameters.Add(moduleFuncCodeParameter);
            }
            _parameterModels[typeof(ModuleFuncCodeTable)] = moduleFuncCodeParameters;

            LabTrayParameterTableViewModel = new LabTrayParameterTableViewModel(ParameterTableManager.LabTrayTable);
            LabTrayParameterTableViewModel.Initialized += () => 
            {
                ParameterTableChanged?.Invoke(LabTrayParameterTableViewModel, EventArgs.Empty);
                if (_parameterModels.TryGetValue(typeof(LabTrayTable), out var parameterModels))
                {
                    parameterModels.Clear();
                    foreach (var labTrayParameter in LabTrayParameterTableViewModel.Tables)
                    {
                        parameterModels.Add(labTrayParameter);
                    }
                }
            };
            LabTrayParameterTableViewModel.Initialize();
            LabTrayParameterTableViewModel.ParamterTableChanged += (sender) =>
            {
                ParameterTableChanged?.Invoke(LabTrayParameterTableViewModel, EventArgs.Empty);
                if (_parameterModels.TryGetValue(typeof(LabTrayTable), out var parameterModels))
                {
                    parameterModels.Clear();
                    foreach (var labTrayParameter in LabTrayParameterTableViewModel.Tables)
                    {
                        parameterModels.Add(labTrayParameter);
                    }
                }
            };
            var labTrayParameters = new ObservableCollection<IParameterModel>();
            foreach (var labTrayParameter in LabTrayParameterTableViewModel.Tables)
            {
                labTrayParameters.Add(labTrayParameter);
            }
            _parameterModels[typeof(LabTrayTable)] = labTrayParameters;

            ModuleChannelGroupParameterTableModel = new ModuleChannelGroupParameterTableModel(ParameterTableManager.ModuleChannelGroupTable);
            ModuleChannelGroupParameterTableModel.Initialized += () => 
            {
                ParameterTableChanged?.Invoke(ModuleChannelGroupParameterTableModel, EventArgs.Empty);
                if (_parameterModels.TryGetValue(typeof(ModuleChannelGroupTable), out var parameterModels))
                {
                    parameterModels.Clear();
                    foreach (var moduleChannelGroupParameter in ModuleChannelGroupParameterTableModel.Tables)
                    {
                        parameterModels.Add(moduleChannelGroupParameter);
                    }
                }
            };
            ModuleChannelGroupParameterTableModel.Initialize();
            ModuleChannelGroupParameterTableModel.ParamterTableChanged += (sender) =>
            {
                ParameterTableChanged?.Invoke(ModuleChannelGroupParameterTableModel, EventArgs.Empty);
                if (_parameterModels.TryGetValue(typeof(ModuleChannelGroupTable), out var parameterModels))
                {
                    parameterModels.Clear();
                    foreach (var moduleChannelGroupParameter in ModuleChannelGroupParameterTableModel.Tables)
                    {
                        parameterModels.Add(moduleChannelGroupParameter);
                    }
                }
            };
            var moduleChannelGroupParameters = new ObservableCollection<IParameterModel>();
            foreach (var moduleChannelGroupParameter in ModuleChannelGroupParameterTableModel.Tables)
            {
                moduleChannelGroupParameters.Add(moduleChannelGroupParameter);
            }
            _parameterModels[typeof(ModuleChannelGroupTable)] = moduleChannelGroupParameters;
        }

        public static ObservableCollection<IParameterModel> GetParameterModels(Type type)
        {
            if (!_parameterModels.TryGetValue(type, out ObservableCollection<IParameterModel>? value))
                return [];
            return value;
        }

        public static void Save() 
        {
            ParameterTableManager.SaveTable();
            RaiseChanged();
        }


        public static event EventHandler? ParameterTableChanged;


        public static void RaiseChanged()
        {
            ParameterTableChanged?.Invoke(ModuleParamterTableViewModel, EventArgs.Empty);
            ParameterTableChanged?.Invoke(ModuleFuncCodeParameterTableViewModel, EventArgs.Empty);
            ParameterTableChanged?.Invoke(LabTrayParameterTableViewModel, EventArgs.Empty);
            ParameterTableChanged?.Invoke(ModuleChannelGroupParameterTableModel, EventArgs.Empty);
        }
    }
}
