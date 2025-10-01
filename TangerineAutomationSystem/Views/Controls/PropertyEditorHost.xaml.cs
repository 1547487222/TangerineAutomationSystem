using System.Windows;
using System.Windows.Controls;

namespace TangerineAutomationSystem.Views.Controls
{
    public partial class PropertyEditorHost : UserControl
    {
        public static readonly DependencyProperty SelectedObjectProperty =
            DependencyProperty.Register(nameof(SelectedObject), typeof(object), typeof(PropertyEditorHost),
                new PropertyMetadata(null, OnSelectedObjectChanged));

        public object SelectedObject
        {
            get => GetValue(SelectedObjectProperty);
            set => SetValue(SelectedObjectProperty, value);
        }

        private static void OnSelectedObjectChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var host = (PropertyEditorHost)d;
            host.PART_PropertyGrid.SelectedObject = e.NewValue;
        }

        public PropertyEditorHost()
        {
            InitializeComponent();
        }
    }
}