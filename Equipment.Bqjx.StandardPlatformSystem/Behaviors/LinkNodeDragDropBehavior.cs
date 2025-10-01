using Equipment.Bqjx.StandardPlatformSystem.WorkFlows;
using HandyControl.Controls;
using Nodify;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace Equipment.Bqjx.StandardPlatformSystem.Behaviors
{
    public class LinkNodeDragDropBehavior : DragDropBehavior<NodifyEditor, NodeMenuItemViewModel>
    {
        protected override void AssociatedObject_Drop(object sender, DragEventArgs e)
        {
            if (e.Effects != DragDropEffects.None && !e.Handled)
            {
                Task.Run(() => 
                {
                    Thread.Sleep(50);
                    App.Current.Dispatcher.Invoke(() => 
                    {
                        if (AssociatedObject is NodifyEditor nodifyEditor)
                        {
                            var point = nodifyEditor.MouseLocation;
                            point.Y -= 20;
                            this.DropPoint = point;
                            DropCommand?.Execute(e.Data.GetData(typeof(NodeMenuItemViewModel)));
                        }
                    });
                });
            }
        }
    }
}
