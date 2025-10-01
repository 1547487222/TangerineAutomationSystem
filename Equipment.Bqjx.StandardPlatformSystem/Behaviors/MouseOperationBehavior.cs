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
    /// <summary>
    /// 封装鼠标操作的行为
    /// </summary>
   public class MouseOperationBehavior: Behavior<FrameworkElement>
    {

        public ICommand MouseLeftDoubleClickCommand
        {
            get { return (ICommand)GetValue(MouseLeftDoubleClickCommandProperty); }
            set { SetValue(MouseLeftDoubleClickCommandProperty, value); }
        }

        // Using a DependencyProperty as the backing store for DoubleClickCommand.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty MouseLeftDoubleClickCommandProperty =
            DependencyProperty.Register("MouseLeftDoubleClickCommand", typeof(ICommand), typeof(MouseOperationBehavior), new PropertyMetadata(default(ICommand)));



        public ICommand ClickCommand
        {
            get { return (ICommand)GetValue(ClickCommandProperty); }
            set { SetValue(ClickCommandProperty, value); }
        }

        // Using a DependencyProperty as the backing store for ClickCommand.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty ClickCommandProperty =
            DependencyProperty.Register("ClickCommand", typeof(ICommand), typeof(MouseOperationBehavior), new PropertyMetadata(default(ICommand)));

        protected override void OnAttached()
        {
            base.OnAttached();
            this.AssociatedObject.MouseLeftButtonDown += AssociatedObject_MouseLeftButtonDown;
            this.AssociatedObject.MouseMove += AssociatedObject_MouseMove;
        }
        private void AssociatedObject_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (e.ClickCount == 2)
            {
                MouseLeftDoubleClickCommand?.Execute(AssociatedObject.DataContext);
            }
        }

        private void AssociatedObject_MouseMove(object sender, MouseEventArgs e)
        {
           
        }

        protected override void OnDetaching()
        {
            base.OnDetaching();
            this.AssociatedObject.MouseLeftButtonDown -= AssociatedObject_MouseLeftButtonDown;
            this.AssociatedObject.MouseMove -= AssociatedObject_MouseMove;
        }
    }
}
