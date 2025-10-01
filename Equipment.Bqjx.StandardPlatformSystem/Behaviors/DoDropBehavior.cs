using Microsoft.Xaml.Behaviors;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;

namespace Equipment.Bqjx.StandardPlatformSystem.Behaviors
{
   public abstract class DoDropBehavior<TElement>: Behavior<FrameworkElement> where TElement : FrameworkElement
    {
        protected override void OnAttached()
        {
            base.OnAttached();
            this.AssociatedObject.MouseMove += AssociatedObject_MouseMove;
        }

        private void AssociatedObject_MouseMove(object sender, System.Windows.Input.MouseEventArgs e)
        {
            if (e.LeftButton == MouseButtonState.Pressed && sender is TElement element)
            {
                var data = DoDropData(element);
                if (data != null)
                {
                    DragDrop.DoDragDrop(element, data, Effects);
                }
            }
        }
        public abstract DragDropEffects  Effects { get;}
        public abstract DataObject? DoDropData(TElement element);
        protected override void OnDetaching()
        {
            base.OnDetaching();
            this.AssociatedObject.MouseMove -= AssociatedObject_MouseMove;
        }
    }
}
