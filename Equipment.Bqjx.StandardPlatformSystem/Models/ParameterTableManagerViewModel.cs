using QStandaedPlatform.Engine.Common.Common;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Equipment.Bqjx.StandardPlatformSystem.Models
{
   public class ParameterTableManagerViewModel
    {
        public ObservableCollection<ParameterTabItem> TabItems { get; set; } = [];

        public ParameterTableManagerViewModel()
        {
            App.Current.Dispatcher.InvokeAsync(() => 
            {
               
                TabItems.Clear();
                var temp= new ObservableCollection<ParameterTabItem>
                {
                    new (ParameterModelRepository.ModuleParamterTableViewModel.TableName, ParameterModelRepository.ModuleParamterTableViewModel),
                    new (ParameterModelRepository.ModuleFuncCodeParameterTableViewModel.TableName, ParameterModelRepository.ModuleFuncCodeParameterTableViewModel),
                    new (ParameterModelRepository.LabTrayParameterTableViewModel.TableName, ParameterModelRepository.LabTrayParameterTableViewModel),
                    new (ParameterModelRepository.ModuleChannelGroupParameterTableModel.TableName, ParameterModelRepository.ModuleChannelGroupParameterTableModel)
                };
                foreach (var item in temp) 
                {
                    TabItems.Add(item);
                }
            }, System.Windows.Threading.DispatcherPriority.Loaded);
        }


        public void Load()
        {

        }

        public void UnLoad()
        {
            ParameterModelRepository.Save();
        }
    }


    public class ParameterTabItem(string name, ITableModel tableModel)
    {
        public string TableName { get; set; } = name;

        public ITableModel TableModel { get; set; } = tableModel;
    }
}
