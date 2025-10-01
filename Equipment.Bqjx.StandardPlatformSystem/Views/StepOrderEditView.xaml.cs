using Equipment.Bqjx.StandardPlatformSystem.Models;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace Equipment.Bqjx.StandardPlatformSystem.Views
{
    /// <summary>
    /// StepOrderEditView.xaml 的交互逻辑
    /// </summary>
    public partial class StepOrderEditView : Window
    {
        // 左侧和右侧步骤列表
        public ObservableCollection<StepDisplayModel> SourceSteps { get; set; }
        public ObservableCollection<StepDisplayModel> OrderSteps { get; set; }

        public string FlowTaskCode { get; set; } = string.Empty;

        public string FlowTaskDescription { get; set; } = string.Empty;

        private readonly FlowTaskDisplayModel _flowTaskDisplayModel;
        public StepOrderEditView(ObservableCollection<StepDisplayModel> sourceSteps, FlowTaskDisplayModel flowTaskDisplayModel)
        {
            InitializeComponent();
            _flowTaskDisplayModel = flowTaskDisplayModel;
            FlowTaskCode= _flowTaskDisplayModel.FlowTaskName;
            FlowTaskDescription = _flowTaskDisplayModel.FlowTaskDescription;
            ProcessTaskIdTextBox.Text = _flowTaskDisplayModel.FlowTaskName;
            ProcessTaskDescTextBox.Text = _flowTaskDisplayModel.FlowTaskDescription;
            SourceSteps = sourceSteps;
            OrderSteps = flowTaskDisplayModel.Steps;
            LeftStepsListBox.ItemsSource = SourceSteps;
            RightStepsListBox.ItemsSource = OrderSteps;
        }

        private void AddStepButton_Click(object sender, RoutedEventArgs e)
        {
            if (LeftStepsListBox.SelectedItem != null)
            {
                if (LeftStepsListBox.SelectedItem is StepDisplayModel selectedStep)
                {
                    if (OrderSteps.Any(p => p.StepId == selectedStep.StepId))
                    {
                        return;
                    }
                    if (RightStepsListBox.SelectedItem != null)
                    {
                        if (RightStepsListBox.SelectedItem is StepDisplayModel selectedOrderStep)
                        {
                            int selectedIndex = OrderSteps.IndexOf(selectedOrderStep);
                            OrderSteps.Insert(selectedIndex + 1, selectedStep);
                        }
                    }
                    else
                    {
                        OrderSteps.Add(selectedStep);
                    }
                    RightStepsListBox.SelectedItem = selectedStep;
                }
            }
        }

        private void DeleteStepButton_Click(object sender, RoutedEventArgs e)
        {
            if (RightStepsListBox.SelectedItem != null)
            {
                if (RightStepsListBox.SelectedItem is StepDisplayModel selectedStep)
                {
                    OrderSteps.Remove(selectedStep);
                }
            }
        }

        private void ProcessTaskIdTextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            //赋值给 FlowTaskCode
            FlowTaskCode = ProcessTaskIdTextBox.Text;
        }

        private void ProcessTaskDescTextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            FlowTaskDescription = ProcessTaskDescTextBox.Text;
        }
    }
}
