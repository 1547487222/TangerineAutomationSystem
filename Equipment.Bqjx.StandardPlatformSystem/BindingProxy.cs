using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace Equipment.Bqjx.StandardPlatformSystem
{
    public class BindingProxy : Freezable
    {
        public static readonly DependencyProperty DataContextProperty =
            DependencyProperty.Register(nameof(DataContext), typeof(object), typeof(BindingProxy), new UIPropertyMetadata(default(object)));

        public object DataContext
        {
            get => GetValue(DataContextProperty);
            set => SetValue(DataContextProperty, value);
        }

        protected override Freezable CreateInstanceCore()
            => new BindingProxy();
    }
}
