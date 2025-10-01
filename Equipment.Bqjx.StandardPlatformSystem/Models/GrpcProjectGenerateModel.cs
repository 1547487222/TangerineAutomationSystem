using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Equipment.Bqjx.StandardPlatformSystem.Views;
using QStandaedPlatform.Engine.Common.Common;
using QStandaedPlatform.Engine.Extensions;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Equipment.Bqjx.StandardPlatformSystem.Models
{
    public partial class GrpcProjectGenerateModel:ObservableObject
    {

        public ObservableCollection<GrpcProjectModel> Projects { get; set; } = [];

        [RelayCommand]
        private void AddProject()
        {
            var project = new GrpcProjectModel
            {
                ProjectName = "NewProject1",
                SolutionName = "NewSolution1"
            };
            project.View = new GrpcProjectGenerateEditView(new GrpcProjectGenerateEditViewModel(project));
            Projects.Add(project);
        }
        [RelayCommand]
        private void RemoveProject(GrpcProjectModel project)
        {
            Projects.Remove(project);
        }



        public void Close()
        {
            //保存
            GrpcProjectService.Instance.ClearProject();
            foreach (var projectModel in Projects)
            {
                var grpcProjectOptions = new GrpcProjectOptions()
                {
                    ProjectName = projectModel.ProjectName,
                    SolutionName = projectModel.SolutionName,
                    SolutionRootPath = projectModel.SolutionRootPath,
                    ProjectDescription = projectModel.ProjectDescription,
                    LaboratoryId = projectModel.LaboratoryId == 0 ? SnowflakeIdGenerator.Instance.GenerateYitId() : projectModel.LaboratoryId,
                    LaboratoryCode = projectModel.LaboratoryCode,
                    LaboratoryDescription = projectModel.LaboratoryDescription,
                    LaboratoryName = projectModel.LaboratoryName,
                    ProjectVersion = projectModel.ProjectVersion,

                };
                foreach (var platformCreate in projectModel.PlatformCreates)  
                {
                    grpcProjectOptions.GrpcProjectPlatformOptions.Add(new GrpcProjectPlatformOptions 
                    {
                        PlatformId = platformCreate.PlatformId,
                        PlatformCode = platformCreate.PlatformCode,
                        PlatformName = platformCreate.PlatformName,
                        PlatformModuleId = platformCreate.PlatformModuleInfoParameterModel?.ModuleInfoParameter.ModuleInfoId ?? Guid.Empty,
                        PlatformGrabActionId = platformCreate.PlatformGrabActionParameterModel?.Parameter.ParameterId ?? Guid.Empty,
                        PlatformDescription = platformCreate.PlatformDescription,
                        PlatformSamplingFlux = platformCreate.PlatformSamplingFlux,
                        PlatformMaxCacheCount = platformCreate.PlatformMaxCacheCount,
                        PlatformMaxExecuteCount = platformCreate.PlatformMaxExecuteCount,
                        InitialFlowConfigs = [.. platformCreate.HomeFiles.Select(P => (GrpcProjectPlatformFlowConfigOptions)P.Clone())],
                        FinalizeFlowConfigs = [.. platformCreate.FinalizeFiles.Select(P => (GrpcProjectPlatformFlowConfigOptions)P.Clone())],
                        LineDrainageFlowConfigs = [.. platformCreate.LineDrainageFiles.Select(P => (GrpcProjectPlatformFlowConfigOptions)P.Clone())],
                        MaintainFlowConfigs = [.. platformCreate.MaintenanceFiles.Select(P => (GrpcProjectPlatformFlowConfigOptions)P.Clone())],
                        PlatformMonitorItems = [.. platformCreate.PlatformMonitorItems.Select(p => p.Model)],
                        PrepareFlowConfigs = [.. platformCreate.PreperExperimentFiles.Select(P => (GrpcProjectPlatformFlowConfigOptions)P.Clone())],
                        SystemStorageFlowConfigs = [.. platformCreate.SystemStorageFiles.Select(P => (GrpcProjectPlatformFlowConfigOptions)P.Clone())],
                        TaskFlowConfigs = [.. platformCreate.StartTaskFiles.Select(P => (GrpcProjectPlatformFlowConfigOptions)P.Clone())],
                       
                    });
                }

                foreach (var grpcProjectProcess in projectModel.TransferModules)
                {
                    grpcProjectOptions.GrpcProjectTransferModuleOptions.Add(grpcProjectProcess.GrpcProjectTransferModuleOptions);
                }

                foreach (var grpcProjectProcessflow in projectModel.Processflows)
                {
                    grpcProjectOptions.GrpcProjectProcessflowOptions.Add(grpcProjectProcessflow.GrpcProjectProcessflowOptinos);
                }

                foreach (var grpcProjectProduct in projectModel.ProductLines)
                {
                    grpcProjectOptions.GrpcProjectProductlineOptions.Add(grpcProjectProduct.GrpcProjectProductlineOptions);
                }
           
                GrpcProjectService.Instance.AddProject(grpcProjectOptions);
            }
            GrpcProjectService.Instance.SaveProject();
        }

        public void Load()
        {
            GrpcProjectService.Instance.ClearProject();
            GrpcProjectService.Instance.LoadProject();
            foreach (var grpcProject in GrpcProjectService.Instance.GrpcProjects)
            {
                var project = new GrpcProjectModel 
                {
                    ProjectName = grpcProject.ProjectName,
                    SolutionRootPath = grpcProject.SolutionRootPath,
                    SolutionName = grpcProject.SolutionName,
                    ProjectDescription = grpcProject.ProjectDescription,
                    LaboratoryCode = grpcProject.LaboratoryCode,
                    LaboratoryDescription = grpcProject.LaboratoryDescription,
                    LaboratoryId = grpcProject.LaboratoryId,
                    LaboratoryName = grpcProject.LaboratoryName,
                    ProjectVersion = grpcProject.ProjectVersion,
                };
                project.LoadProject();
                foreach (var grpcProjectPlatform in grpcProject.GrpcProjectPlatformOptions)
                {
                    var platformCreate = new PlatformCreateModel 
                    {
                        PlatformId = grpcProjectPlatform.PlatformId,
                        PlatformCode = grpcProjectPlatform.PlatformCode,
                        PlatformName = grpcProjectPlatform.PlatformName,
                        PlatformMaxCacheCount = grpcProjectPlatform.PlatformMaxCacheCount,
                        PlatformMaxExecuteCount = grpcProjectPlatform.PlatformMaxExecuteCount,
                        PlatformSamplingFlux = grpcProjectPlatform.PlatformSamplingFlux,
                        PlatformDescription = grpcProjectPlatform.PlatformDescription,
                        PlatformMonitorItems = grpcProjectPlatform.PlatformMonitorItems.Select(p => new PlatformMonitorItemViewModel(p)).ToObservableCollection(),
                    };
                    platformCreate.PlatformModuleInfoParameterModel = platformCreate.ModuleInfoParameterModels.FirstOrDefault(p => p.ModuleInfoParameter.ModuleInfoId == grpcProjectPlatform.PlatformModuleId);
                    platformCreate.PlatformGrabActionParameterModel = platformCreate.ModuleFuncCodeParameterModels.FirstOrDefault(p => p.Parameter.ParameterId == grpcProjectPlatform.PlatformGrabActionId);
                    foreach (var flowConfig in grpcProjectPlatform.InitialFlowConfigs)
                    {
                        platformCreate.LoadHomeFlows(flowConfig);
                    }
                    foreach (var flowConfig in grpcProjectPlatform.FinalizeFlowConfigs)
                    {
                        platformCreate.LoadFinalizeFlows(flowConfig);
                    }
                    foreach (var flowConfig in grpcProjectPlatform.LineDrainageFlowConfigs)
                    {
                        platformCreate.LoadLineDrainageFlows(flowConfig);
                    }
                    foreach (var flowConfig in grpcProjectPlatform.MaintainFlowConfigs)
                    {
                        platformCreate.LoadMaintenanceFlows(flowConfig);
                    }
                    foreach (var flowConfig in grpcProjectPlatform.PrepareFlowConfigs)
                    {
                        platformCreate.LoadPreperExperimentFlows(flowConfig);
                    }
                    foreach (var flowConfig in grpcProjectPlatform.SystemStorageFlowConfigs)
                    {
                        platformCreate.LoadSystemStorageFlows(flowConfig);
                    }
                    foreach (var flowConfig in grpcProjectPlatform.TaskFlowConfigs)
                    {
                        platformCreate.LoadStartTaskFlows(flowConfig);
                    }
                    project.PlatformCreates.Add(platformCreate);
                }
                project.View = new GrpcProjectGenerateEditView(new GrpcProjectGenerateEditViewModel(project));

                foreach (var transferModuleOptions in grpcProject.GrpcProjectTransferModuleOptions)
                {
                    var transferModule = new GrpcProjectTransferModuleModel(transferModuleOptions)
                    {
                        LeftPlatformModel = project.PlatformCreates.FirstOrDefault(p => p.PlatformId == transferModuleOptions.LeftPlatformId),
                        RightPlatformModel = project.PlatformCreates.FirstOrDefault(p => p.PlatformId == transferModuleOptions.RightPlatformId)
                    };
                    project.TransferModules.Add(transferModule);
                }    

                foreach (var item in grpcProject.GrpcProjectProcessflowOptions)
                {
                    var grpcProjectProcessflowModel = new GrpcProjectProcessflowModel(item);
                    for (int i = 0; i < item.PlatformFlowSteps.Count; i++)
                    {
                        grpcProjectProcessflowModel.PlatformsInOrder.Add(item.PlatformFlowSteps[i]);
                    }
                    project.Processflows.Add(grpcProjectProcessflowModel);
                }

                foreach (var item in grpcProject.GrpcProjectProductlineOptions)
                {
                    var grpcProjectProductlineModel = new GrpcProjectProductLineModel(item);
                    for (int i = 0; i < item.ProcessflowIds.Count; i++)
                    {
                        var processflow = project.Processflows.FirstOrDefault(p => p.ProcessId == item.ProcessflowIds[i]);
                        if (processflow != null)
                        {
                            grpcProjectProductlineModel.ProcessFlowsInOrder.Add(processflow);
                        }
                    }
                    project.ProductLines.Add(grpcProjectProductlineModel);
                }

                Projects.Add(project);
            }
        }
    }
}
