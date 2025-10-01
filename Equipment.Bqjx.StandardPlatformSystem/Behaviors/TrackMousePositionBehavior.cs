using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using System.Windows;
using Microsoft.Xaml.Behaviors;

namespace Equipment.Bqjx.StandardPlatformSystem.Behaviors
{
    /// <summary>
    /// 跟踪控件鼠标位置行为
    /// </summary>
    public class TrackMousePositionBehavior : Behavior<FrameworkElement>
    {
        public Point CurrentPoint
        {
            get { return (Point)GetValue(CurrentPointProperty); }
            set { SetValue(CurrentPointProperty, value); }
        }

        // Using a DependencyProperty as the backing store for CurrentPoint.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty CurrentPointProperty =
            DependencyProperty.Register("CurrentPoint", typeof(Point), typeof(TrackMousePositionBehavior), new PropertyMetadata(default(Point)));


        

        protected override void OnAttached()
        {
            base.OnAttached();
            AssociatedObject.MouseMove += OnMouseMove;
            UpdateMousePosition(); // Initial update
        }

        protected override void OnDetaching()
        {
            base.OnDetaching();
            AssociatedObject.MouseMove -= OnMouseMove;
        }

        private void OnMouseMove(object sender, MouseEventArgs e)
        {
            UpdateMousePosition();
        }

        private void UpdateMousePosition()
        {
            if (AssociatedObject != null)
            {
                Point mousePosition = Mouse.GetPosition(AssociatedObject);
                this.CurrentPoint = mousePosition;
            }
        }
    }
}
