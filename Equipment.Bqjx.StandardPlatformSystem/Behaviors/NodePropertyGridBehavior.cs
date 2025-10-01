using Equipment.Bqjx.StandardPlatformSystem.WorkFlows;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xceed.Wpf.Toolkit.PropertyGrid;

namespace Equipment.Bqjx.StandardPlatformSystem.Behaviors
{
    public class NodePropertyGridBehavior : PropertyGridBehavior<NodeModel>
    {
        public override void DoPropertyChanged(NodeModel sender, PropertyValueChangedEventArgs e)
        {
            sender.OwnerTool.ApplyOnContextChanged(new QStandaedPlatform.Engine.Common.Common.PropertyChangeItem 
            {
                 ChangeDateTime = DateTime.Now,
                 NewValue = e.NewValue,
                 OriginalValue = e.OldValue,
                 PropertyName = e.OriginalSource is PropertyItem propertyItem ? propertyItem.PropertyName : string.Empty
            });
        }
    }
}
