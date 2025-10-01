using Nodify;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace Equipment.Bqjx.StandardPlatformSystem.Depends
{
   public class AttachDependHelper:FrameworkElement
    {
        public static Point GetCurrentPos(DependencyObject obj)
        {
            return (Point)obj.GetValue(CurrentPosProperty);
        }

        public static void SetCurrentPos(DependencyObject obj, Point value)
        {
            obj.SetValue(CurrentPosProperty, value);
        }

        // Using a DependencyProperty as the backing store for CurrentPos.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty CurrentPosProperty =
            DependencyProperty.RegisterAttached("CurrentPos", typeof(Point), typeof(AttachDependHelper), new PropertyMetadata(new Point(), (s, e) => 
            {
                
            }));

        



    }
}
