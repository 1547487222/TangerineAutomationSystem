using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Equipment.Bqjx.StandardPlatformSystem.Views;
using Microsoft.Identity.Client;
using Newtonsoft.Json;
using QStandaedPlatform.Engine.Common.Common;
using QStandaedPlatform.Engine.Laboratory;
using System.Collections.ObjectModel;
using System.Windows;

namespace Equipment.Bqjx.StandardPlatformSystem.Models
{
    public class StepDisplayModel
    {
        public string DisplayName { get; set; } = string.Empty;

        public string StepCode { get; set; } = string.Empty;

        public Guid StepId { get; set; }
    }

    public partial class FlowTaskDisplayModel: ObservableObject
    {
        [ObservableProperty]
        private string _flowTaskName = string.Empty;
        [ObservableProperty]
        private string _flowTaskDescription = string.Empty;

        public long FlowTaskId { get; set; }

        public ObservableCollection<StepDisplayModel> Steps { get; set; } = [];
    }
    public class FlowEditItem(string editItemName, PlatformCreateModel platformCreateModel, ObservableCollection<GrpcProjectPlatformFlowConfigOptions> files) : ObservableObject
    {
        public string EditItemName { get; } = editItemName;


        public PlatformCreateModel Platform { get; set; } = platformCreateModel;

        public ObservableCollection<FlowTaskDisplayModel> FlowTasks { get; set; } = [];

        public ObservableCollection<GrpcProjectPlatformFlowConfigOptions> Files { get; set; } = files;


        private Flow? _selectedFlow;

        public Flow? SelectedFlow
        {
            get { return _selectedFlow; }
            set
            {
                if (SetProperty(ref _selectedFlow, value))
                {

                }
            }
        }


        public RelayCommand AddSelectedItemCommand => new(() => 
        {
            if (SelectedFlow != null)
            {
                if (App.ToolEngine.GetFlowFileDescriptions().Any(p => p.FlowId == SelectedFlow.FlowId))
                {
                    var flowFileDescription = App.ToolEngine.GetFlowFileDescriptions().FirstOrDefault(p => p.FlowId == SelectedFlow.FlowId);
                    if (flowFileDescription != null)
                    {
                        var id = SnowflakeIdGenerator.Instance.GenerateYitId();
                        var count = Files.Count;
                        Files.Add(new GrpcProjectPlatformFlowConfigOptions
                        {
                            FlowTaskId = id,
                            FlowTaskCode = "A" + count,
                            PlatformId = Platform.PlatformId,
                            FlowConfigPath = flowFileDescription.FilePath,
                            FlowId = SelectedFlow.FlowId,
                            Flow = SelectedFlow,
                            FlowTaskDescription = SelectedFlow.FlowName,
                        });
                        FlowTasks.Add(new FlowTaskDisplayModel
                        {
                            FlowTaskId = id,
                            FlowTaskName = "A" + count,
                            FlowTaskDescription = SelectedFlow.FlowName
                        });
                    }
                }
            }
        });

        public RelayCommand<GrpcProjectPlatformFlowConfigOptions> RemoveSelectedItemCommand => new(options => 
        {
            if (options != null)
            {
               var file = Files.FirstOrDefault(p => p.FlowId == options.FlowId);
                if (file != null)
                {
                    Files.Remove(file);
                    var flowtask = FlowTasks.FirstOrDefault(p => p.FlowTaskId == options.FlowTaskId);
                    if (flowtask != null)
                    {
                        FlowTasks.Remove(flowtask);
                    }

                }
            }
        });
        

        public RelayCommand ClearSelectedItemCommand => new(() => 
        {
            SelectedFlow = null;
        });


        public static ObservableCollection<StepDisplayModel> GetSteps(Flow selectedFlow)
        {
            var steps = new ObservableCollection<StepDisplayModel>();
            if (selectedFlow != null)
            {
                var actions = selectedFlow.GetTools();
                foreach (var action in actions)
                {
                    var step = new StepDisplayModel
                    {
                        DisplayName = action.DisplayName,
                        StepId = action.UniqueId,
                        StepCode = action.DefineName
                    };
                    steps.Add(step);
                }
            }
            return steps;
        }

        public RelayCommand<GrpcProjectPlatformFlowConfigOptions> EditItemCommand => new(options =>
        {
            if (options != null)
            {
                var flowtask = FlowTasks.FirstOrDefault(p => p.FlowTaskId == options.FlowTaskId);
                if (flowtask != null && options.Flow != null)
                {
                    StepOrderEditView stepOrderEditView = new(GetSteps(options.Flow), flowtask)
                    {
                        Owner = App.Current.MainWindow
                    };
                    stepOrderEditView.Closed += (sender, e) =>
                    {
                        flowtask.Steps = stepOrderEditView.OrderSteps;
                        flowtask.FlowTaskName = stepOrderEditView.FlowTaskCode;
                        flowtask.FlowTaskDescription = stepOrderEditView.FlowTaskDescription;
                        if (Files.Any())
                        {
                            var file = Files.FirstOrDefault(p => p.FlowTaskId == flowtask.FlowTaskId);
                            if (file != null)
                            {
                                file.FlowTaskCode = stepOrderEditView.FlowTaskCode;
                                file.FlowTaskDescription = stepOrderEditView.FlowTaskDescription;
                                file.StepDisplays = stepOrderEditView.OrderSteps.Select(p => new StepDisplay
                                {
                                    Description = p.StepCode,
                                    DisplayName = p.DisplayName,
                                    StepId = p.StepId
                                }).ToList();
                            }
                        }
                    };
                    stepOrderEditView.ShowDialog();
                }
                else
                {
                    MessageBox.Show("未找到流程,无法编辑");
                }
            }
        });
    }
}
