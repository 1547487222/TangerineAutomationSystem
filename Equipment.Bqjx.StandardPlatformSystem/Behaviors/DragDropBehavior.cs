using Equipment.Bqjx.StandardPlatformSystem.Depends;
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
   public abstract class DragDropBehavior<TElement, TData>: Behavior<FrameworkElement> where TElement : FrameworkElement
    {
        public ICommand DragEnterCommand
        {
            get { return (ICommand)GetValue(DragEnterCommandProperty); }
            set { SetValue(DragEnterCommandProperty, value); }
        }

        // Using a DependencyProperty as the backing store for DragEnterCommand.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty DragEnterCommandProperty =
            DependencyProperty.Register("DragEnterCommand", typeof(ICommand), typeof(DragDropBehavior<TElement, TData>), new PropertyMetadata(default(ICommand)));


        public ICommand DragOverCommand
        {
            get { return (ICommand)GetValue(DragOverCommandProperty); }
            set { SetValue(DragOverCommandProperty, value); }
        }

        // Using a DependencyProperty as the backing store for DragOverCommand.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty DragOverCommandProperty =
            DependencyProperty.Register("DragOverCommand", typeof(ICommand), typeof(DragDropBehavior<TElement, TData>), new PropertyMetadata(default(ICommand)));

        public ICommand DragLeaveCommand
        {
            get { return (ICommand)GetValue(DragLeaveCommandProperty); }
            set { SetValue(DragLeaveCommandProperty, value); }
        }

        // Using a DependencyProperty as the backing store for DragLeaveCommand.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty DragLeaveCommandProperty =
            DependencyProperty.Register("DragLeaveCommand", typeof(ICommand), typeof(DragDropBehavior<TElement, TData>), new PropertyMetadata(default(ICommand)));



        public ICommand DropCommand
        {
            get { return (ICommand)GetValue(DropCommandProperty); }
            set { SetValue(DropCommandProperty, value); }
        }

        // Using a DependencyProperty as the backing store for DragCommand.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty DropCommandProperty =
            DependencyProperty.Register("DropCommand", typeof(ICommand), typeof(DragDropBehavior<TElement, TData>), new PropertyMetadata(default(ICommand)));



        public Point DragEnterPoint
        {
            get { return (Point)GetValue(DragEnterPointProperty); }
            set { SetValue(DragEnterPointProperty, value); }
        }

        // Using a DependencyProperty as the backing store for DragEnterPoint.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty DragEnterPointProperty =
            DependencyProperty.Register("DragEnterPoint", typeof(Point), typeof(DragDropBehavior<TElement, TData>), new PropertyMetadata(default(Point)));


        public Point DropPoint
        {
            get { return (Point)GetValue(DropPointProperty); }
            set { SetValue(DropPointProperty, value); }
        }

        // Using a DependencyProperty as the backing store for DropPoint.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty DropPointProperty =
            DependencyProperty.Register("DropPoint", typeof(Point), typeof(DragDropBehavior<TElement, TData>), new PropertyMetadata(default(Point)));

        //public virtual Point GetMouseLocation(TElement element) 
        //{
        //    return new Point();
        //}

        protected override void OnAttached()
        {
            base.OnAttached();
            this.AssociatedObject.AllowDrop = true;
            this.AssociatedObject.DragEnter += AssociatedObject_DragEnter;
            this.AssociatedObject.DragOver += AssociatedObject_DragOver;
            this.AssociatedObject.DragLeave += AssociatedObject_DragLeave;
            this.AssociatedObject.Drop += AssociatedObject_Drop;
        }

        protected virtual void AssociatedObject_Drop(object sender, DragEventArgs e)
        {
           
        }

        protected virtual void AssociatedObject_DragLeave(object sender, DragEventArgs e)
        {

        }

        protected virtual void AssociatedObject_DragOver(object sender, DragEventArgs e)
        {
            //DragOverPoint= e.GetPosition(AssociatedObject);
        }

        protected virtual void AssociatedObject_DragEnter(object sender, DragEventArgs e)
        {
           // DragEnterPoint = e.GetPosition(AssociatedObject);
            
        }

        protected override void OnDetaching()
        {
            base.OnDetaching();
            this.AssociatedObject.AllowDrop = false;
            this.AssociatedObject.DragEnter -= AssociatedObject_DragEnter;
            this.AssociatedObject.DragOver -= AssociatedObject_DragOver;
            this.AssociatedObject.DragLeave -= AssociatedObject_DragLeave;
            this.AssociatedObject.Drop -= AssociatedObject_Drop;
        }
    }
}
