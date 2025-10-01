using System.Windows;
using System.Windows.Controls;
using TangerineAutomationSystem.Models;

namespace TangerineAutomationSystem.Views
{
    public partial class ModuleFunctionCatalogEditor : UserControl
    {
        private ModuleFunctionCatalog? _catalog;

        public ModuleFunctionCatalogEditor()
        {
            InitializeComponent();
            Loaded += ModuleFunctionCatalogEditor_Loaded;
        }

        private void ModuleFunctionCatalogEditor_Loaded(object sender, RoutedEventArgs e)
        {
            _catalog = DataContext as ModuleFunctionCatalog;
            if (_catalog != null)
            {
                FunctionGrid.ItemsSource = _catalog.Functions;
            }
        }

        private void AddFunction_Click(object sender, RoutedEventArgs e)
        {
            if (_catalog == null) return;
            var func = new ModuleFunction
            {
                Name = $"Function_{_catalog.Functions.Count + 1}",
                DisplayName = "新功能",
                ModuleType = "Default"
            };
            _catalog.Functions.Add(func);
        }

        private void DeleteFunction_Click(object sender, RoutedEventArgs e)
        {
            if (_catalog == null || FunctionGrid.SelectedItem == null) return;
            var func = FunctionGrid.SelectedItem as ModuleFunction;
            if (func != null)
            {
                _catalog.Functions.Remove(func);
            }
        }
    }
}
