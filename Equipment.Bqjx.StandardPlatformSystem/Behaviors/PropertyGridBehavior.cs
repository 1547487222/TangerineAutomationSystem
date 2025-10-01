using Microsoft.Xaml.Behaviors;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using System.Windows.Threading;
using Xceed.Wpf.Toolkit.PropertyGrid;

namespace Equipment.Bqjx.StandardPlatformSystem.Behaviors
{
    public abstract class PropertyGridBehavior<TSender>:Behavior<PropertyGrid>
    {
        private DispatcherTimer _notificationTimer;
        private readonly object _syncRoot = new object();
        private const int _debounceDelay = 800; // Time in milliseconds
        private readonly Stack<(object sender, PropertyValueChangedEventArgs e)> _notifications = new Stack<(object sender, PropertyValueChangedEventArgs e)>();
        public ICommand PropertyChangedCommand
        {
            get { return (ICommand)GetValue(PropertyChangedCommandProperty); }
            set { SetValue(PropertyChangedCommandProperty, value); }
        }

        // Using a DependencyProperty as the backing store for MyProperty.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty PropertyChangedCommandProperty =
            DependencyProperty.Register("MyProperty", typeof(ICommand), typeof(PropertyGridBehavior<TSender>), new PropertyMetadata(null));

        public abstract void DoPropertyChanged(TSender sender, PropertyValueChangedEventArgs e);

        protected override void OnAttached()
        {
            base.OnAttached();
            this.AssociatedObject.PropertyValueChanged += AssociatedObject_PropertyValueChanged;
            _notificationTimer = new DispatcherTimer { Interval = TimeSpan.FromMilliseconds(_debounceDelay) };
            _notificationTimer.Tick += OnNotificationTimerTick;
        }

        private void OnNotificationTimerTick(object? sender, EventArgs e)
        {
            lock (_syncRoot) 
            {
                var (s, eventArgs) = _notifications.Pop();
                if (s is FrameworkElement frameworkElement)
                {
                    if (frameworkElement.DataContext is TSender  sender1)
                    {
                        PropertyChangedCommand?.Execute(eventArgs);
                        DoPropertyChanged(sender1, eventArgs);
                        if (sender is DispatcherTimer dispatcherTimer)
                        {
                            dispatcherTimer.Stop();
                        }
                    }
                }
            }
        }

        private void AssociatedObject_PropertyValueChanged(object sender, PropertyValueChangedEventArgs e)
        {
            _notificationTimer.Stop();
            _notificationTimer.Start();
            lock (_syncRoot)
            {
                _notifications.Push((sender, e));
            }
        }

        protected override void OnDetaching()
        {
            base.OnDetaching();
            this.AssociatedObject.PropertyValueChanged -= AssociatedObject_PropertyValueChanged;
        }
    }
}
