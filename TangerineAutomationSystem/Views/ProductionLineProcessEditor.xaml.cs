using System.Windows;
using System.Windows.Controls;
using TangerineAutomationSystem.Models;

namespace TangerineAutomationSystem.Views
{
    public partial class ProductionLineProcessEditor : UserControl
    {
        private ProductionLineModel? _productionLine;
        private ProductionLineProcess? _selectedProcess;

        public ProductionLineProcessEditor()
        {
            InitializeComponent();
            Loaded += ProductionLineProcessEditor_Loaded;
        }

        private void ProductionLineProcessEditor_Loaded(object sender, RoutedEventArgs e)
        {
            _productionLine = DataContext as ProductionLineModel;
            if (_productionLine != null)
            {
                ProcessList.ItemsSource = _productionLine.ProductionLineProcesses;
            }
        }

        private void AddProcess_Click(object sender, RoutedEventArgs e)
        {
            if (_productionLine == null) return;
            var process = new ProductionLineProcess
            {
                Name = $"工艺流程_{_productionLine.ProductionLineProcesses.Count + 1}",
                Description = "新建的工艺流程"
            };
            _productionLine.ProductionLineProcesses.Add(process);
            ProcessList.SelectedItem = process;
        }

        private void DeleteProcess_Click(object sender, RoutedEventArgs e)
        {
            if (_productionLine == null || _selectedProcess == null) return;
            _productionLine.ProductionLineProcesses.Remove(_selectedProcess);
            _selectedProcess = null;
            ProcessList.SelectedItem = null;
        }

        private void ProcessList_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            _selectedProcess = ProcessList.SelectedItem as ProductionLineProcess;
            if (_selectedProcess != null)
            {
                ProcessNameBox.Text = _selectedProcess.Name;
                ProcessDescBox.Text = _selectedProcess.Description;
                ProcessNameBox.TextChanged += (s, args) => { if (_selectedProcess != null) _selectedProcess.Name = ProcessNameBox.Text; };
                ProcessDescBox.TextChanged += (s, args) => { if (_selectedProcess != null) _selectedProcess.Description = ProcessDescBox.Text; };
                
                // Set up the flow editor for this process
                // The process consists of platform tasks, transfers, and module actions
            }
        }
    }
}
