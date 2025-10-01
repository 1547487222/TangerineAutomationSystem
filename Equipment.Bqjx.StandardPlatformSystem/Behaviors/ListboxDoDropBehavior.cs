using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;

namespace Equipment.Bqjx.StandardPlatformSystem.Behaviors
{
    public class ListboxDoDropBehavior : DoDropBehavior<ListBox>
    {
        public override DragDropEffects Effects => DragDropEffects.Copy;

        public override DataObject? DoDropData(ListBox element)
        {
            if (element.SelectedItem != null)
            {
                return new DataObject(element.SelectedItem);
            }
            return null;
        }
    }
}
