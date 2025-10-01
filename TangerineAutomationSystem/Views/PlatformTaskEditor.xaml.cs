using System.Windows;
using System.Windows.Controls;
using TangerineAutomationSystem.Models;

namespace TangerineAutomationSystem.Views
{
    public partial class PlatformTaskEditor : UserControl
    {
        private PlatformModel? _platform;
        private PlatformTask? _selectedTask;

        public PlatformTaskEditor()
        {
            InitializeComponent();
            Loaded += PlatformTaskEditor_Loaded;
        }

        private void PlatformTaskEditor_Loaded(object sender, RoutedEventArgs e)
        {
            _platform = DataContext as PlatformModel;
            if (_platform != null)
            {
                TaskList.ItemsSource = _platform.PlatformTasks;
            }
        }

        private void AddPlatformTask_Click(object sender, RoutedEventArgs e)
        {
            if (_platform == null) return;
            var task = new PlatformTask
            {
                Name = $"平台任务_{_platform.PlatformTasks.Count + 1}",
                Description = "新建的平台任务"
            };
            _platform.PlatformTasks.Add(task);
            TaskList.SelectedItem = task;
        }

        private void DeletePlatformTask_Click(object sender, RoutedEventArgs e)
        {
            if (_platform == null || _selectedTask == null) return;
            _platform.PlatformTasks.Remove(_selectedTask);
            _selectedTask = null;
            TaskList.SelectedItem = null;
        }

        private void TaskList_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            _selectedTask = TaskList.SelectedItem as PlatformTask;
            if (_selectedTask != null)
            {
                TaskNameBox.Text = _selectedTask.Name;
                TaskDescBox.Text = _selectedTask.Description;
                TaskNameBox.TextChanged += (s, args) => { if (_selectedTask != null) _selectedTask.Name = TaskNameBox.Text; };
                TaskDescBox.TextChanged += (s, args) => { if (_selectedTask != null) _selectedTask.Description = TaskDescBox.Text; };
                
                // Set up the flow editor for this task
                // The task would have its own ProcessFlow representing its internal workflow
            }
        }
    }
}
