using System;
using System.Collections.Generic;
using System.Windows.Controls;
using System.Windows;


namespace Equipment.Bqjx.StandardPlatformSystem.Models
{
    public class TableTemplateSelector : DataTemplateSelector
    {
        public DataTemplate ModuleInfoTableTemplate { get; set; }
        public DataTemplate ModuleFuncCodeTableTemplate { get; set; }
        public DataTemplate LabTrayParameterTableTemplate { get; set; }
        public DataTemplate ModuleChannelGroupParameterTableTemplate { get; set; }

        public override DataTemplate SelectTemplate(object item, DependencyObject container)
        {
            if (item is ModuleInfoParamterTableViewModel) return ModuleInfoTableTemplate;
            if (item is ModuleFuncCodeParameterTableViewModel) return ModuleFuncCodeTableTemplate;
            if (item is LabTrayParameterTableViewModel) return LabTrayParameterTableTemplate;
            if (item is ModuleChannelGroupParameterTableModel) return ModuleChannelGroupParameterTableTemplate;
            return base.SelectTemplate(item, container);
        }
    }
}
