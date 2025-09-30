using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using TangerineAutomationSystem.Models;

namespace TangerineAutomationSystem.ViewModels
{
    public class ProjectExplorerViewModel : ViewModelBase
    {
        private Project _project;
        private object _selectedItem;

        public Project Project
        {
            get => _project;
            set => SetProperty(ref _project, value);
        }

        public object SelectedItem
        {
            get => _selectedItem;
            set
            {
                SetProperty(ref _selectedItem, value);
                OnPropertyChanged(nameof(CanAddLaboratory));
                OnPropertyChanged(nameof(CanAddProductionLine));
                OnPropertyChanged(nameof(CanAddPlatform));
                OnPropertyChanged(nameof(CanAddModule));
            }
        }

        public bool CanAddLaboratory => SelectedItem is Project;
        public bool CanAddProductionLine => SelectedItem is Laboratory;
        public bool CanAddPlatform => SelectedItem is ProductionLine;
        public bool CanAddModule => SelectedItem is Platform;

        public ICommand ItemSelectedCommand { get; private set; }

        public ProjectExplorerViewModel()
        {
            //ItemSelectedCommand = new RelayCommand<object>(OnItemSelected);
        }

        private void OnItemSelected(object item)
        {
            SelectedItem = item;
        }
    }
}
