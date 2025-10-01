using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.CodeAnalysis.CSharp.Formatting;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.Options;
using Microsoft.CodeAnalysis;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Foundation.TangerineProject;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using QStandaedPlatform.Engine.Common.Common;
using Newtonsoft.Json;
using Grpc.Net.Client;
using System.Net.Http;
using QStandaedPlatform.Engine.Laboratory;
using System.Diagnostics;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using QStandaedPlatform.Engine.Laboratory.Documents;
using QStandaedPlatform.Engine.Common;

namespace Equipment.Bqjx.StandardPlatformSystem.Models
{
    public partial class GrpcProjectModel : ObservableObject
    {
        //解决方案根目录
        [ObservableProperty]
        private string _solutionRootPath = string.Empty;

        //解决方案名称
        [ObservableProperty]
        private string _solutionName = string.Empty;

        //项目名称
        [ObservableProperty]
        private string _projectName = string.Empty;

        //项目描述
        [ObservableProperty]
        private string _projectDescription = string.Empty;

        //项目版本
        [ObservableProperty]
        private string _projectVersion = string.Empty;


        //实验室名称
        [ObservableProperty]
        private string _laboratoryName = string.Empty;

        //实验室编号
        [ObservableProperty]
        private string _laboratoryCode = string.Empty;

        //实验室描述
        [ObservableProperty]
        private string _laboratoryDescription = string.Empty;


        public string SolutionPath { get; set; } = string.Empty;

        public string ProjectDirectory { get; set; } = string.Empty;

        public string Projectfile { get; set; } = string.Empty;


        public object? View { get; set; }

        public long LaboratoryId { get; set; }

        public ObservableCollection<PlatformCreateModel> PlatformCreates { get; set; } = [];

        public ObservableCollection<GrpcProjectTransferModuleModel> TransferModules { get; set; } = [];

        public ObservableCollection<GrpcProjectProcessflowModel> Processflows { get; set; } = [];

        public ObservableCollection<GrpcProjectProductLineModel> ProductLines { get; set; } = [];

        public GrpcProjectModel()
        {
            LaboratoryId = SnowflakeIdGenerator.Instance.GenerateYitId();
        }

        public void LoadProject()
        {
            SolutionPath = Path.Combine(SolutionRootPath, SolutionName + ".sln");
            ProjectDirectory = Path.Combine(SolutionRootPath, ProjectName);
            Projectfile = Path.Combine(ProjectDirectory, ProjectName + ".csproj");
        }


        [RelayCommand]
        private void AddPlatform()
        {
            PlatformCreates.Add(new PlatformCreateModel());
        }
        [RelayCommand]
        private void DeletePlatform(PlatformCreateModel model)
        {
            PlatformCreates.Remove(model);
        }


        [RelayCommand]
        private void AddTransferModule()
        {
            TransferModules.Add(new GrpcProjectTransferModuleModel(new GrpcProjectTransferModuleOptions { TransferModuleId = SnowflakeIdGenerator.Instance.GenerateYitId() }));
        }

        [RelayCommand]
        private void DeleteTransferModule(GrpcProjectTransferModuleModel model) 
        {
            TransferModules.Remove(model);
        }

        [RelayCommand]
        private void AddProcessflow()
        {
            Processflows.Add(new GrpcProjectProcessflowModel(new GrpcProjectProcessflowOptions { ProcessId = Guid.NewGuid() }));
        }

        [RelayCommand]
        private void DeleteProcessflow(GrpcProjectProcessflowModel model)
        {
            Processflows.Remove(model);
        }

        [RelayCommand]
        private void AddProductLine()
        {
            ProductLines.Add(new GrpcProjectProductLineModel(new GrpcProjectProductlineOptions { ProductlineId = SnowflakeIdGenerator.Instance.GenerateYitId() }));
        }

        [RelayCommand]
        private void DeleteProductLine(GrpcProjectProductLineModel model)
        {
            ProductLines.Remove(model);
        }



        [RelayCommand]
        public void GenerateGrpcProject()
        {
            if (string.IsNullOrEmpty(SolutionRootPath)
                || string.IsNullOrEmpty(SolutionName)
                || string.IsNullOrEmpty(ProjectName)
                || string.IsNullOrEmpty(LaboratoryName))
            {
                MessageBox.Show("请输入完整信息:解决方案根目录、解决方案名称、项目名称,实验室名称");
                return;
            }
            Directory.CreateDirectory(SolutionRootPath);
            SolutionPath = Path.Combine(SolutionRootPath, SolutionName + ".sln");
            var cmd = "dotnet new sln -n " + SolutionName + " -o " + SolutionRootPath;
            ProcessTask.ExecuteCommand(cmd);
            ProjectDirectory = Path.Combine(SolutionRootPath, ProjectName);
            if (!Directory.Exists(ProjectDirectory))
            {
                Directory.CreateDirectory(ProjectDirectory);
            }
            Projectfile = Path.Combine(ProjectDirectory, ProjectName + ".csproj");
            if (!File.Exists(Projectfile))
            {
                File.Create(Projectfile).Close();
            }
            string csprojContent = GenerateGrpcCsprojContent(ProjectName);
            File.WriteAllText(Projectfile, csprojContent);
            CrammTemplateInfo();
            cmd = "dotnet sln " + SolutionPath + " add " + Projectfile;
            ProcessTask.ExecuteCommand(cmd);

            string arguments = $"dotnet publish \"{Projectfile}\" -c Release -r win-x64 --self-contained false";
            ProcessTask.ExecuteCommand(arguments);

        }
        public static string GenerateGrpcCsprojContent(string projectName)
        {

            StringBuilder sb = new();
            sb.AppendLine("<Project Sdk=\"Microsoft.NET.Sdk.Web\">");
            sb.AppendLine("  <PropertyGroup>");
            sb.AppendLine("   <TargetFramework>net8.0</TargetFramework>");
            sb.AppendLine("  <Nullable>enable</Nullable>");
            sb.AppendLine("    <ImplicitUsings>enable</ImplicitUsings>");
            sb.AppendLine(
                @"<!-- 生成单个可执行文件 -->
	  <PublishSingleFile>true</PublishSingleFile>

	  <!-- 自包含模式（不依赖系统 .NET） -->
	  <SelfContained>true</SelfContained>

	  <!-- 指定运行时为 Windows x64 -->
	  <RuntimeIdentifier>win-x64</RuntimeIdentifier>");
            sb.AppendLine("  </PropertyGroup>");
            sb.AppendLine();
            sb.AppendLine("  <ItemGroup>");
            sb.AppendLine("    <PackageReference Include=\"Grpc.AspNetCore\" Version=\"2.64.0\" />");
            sb.AppendLine("  </ItemGroup>\r\n");

            sb.AppendLine("  <ItemGroup>");
            sb.AppendLine("    <PackageReference Include=\"Grpc.AspNetCore.Web\" Version=\"2.71.0\" />");
            sb.AppendLine("  </ItemGroup>\r\n");

            sb.AppendLine(@"  <ItemGroup>
    <PackageReference Include=""Castle.Core"" Version=""5.2.1"" />
	<PackageReference Include=""Google.Protobuf"" Version=""3.31.1"" />
    <PackageReference Include=""Microsoft.Extensions.Hosting.WindowsServices"" Version=""9.0.7"" />
    <PackageReference Include=""Castle.Core.AsyncInterceptor"" Version=""2.1.0"" />
    <PackageReference Include=""Microsoft.CodeAnalysis.Common"" Version=""4.13.0"" />
    <PackageReference Include=""Microsoft.CodeAnalysis.CSharp"" Version=""4.13.0"" />
    <PackageReference Include=""Microsoft.Data.Sqlite.Core"" Version=""9.0.7"" />
    <PackageReference Include=""Microsoft.EntityFrameworkCore.Sqlite.Core"" Version=""9.0.7"" />
    <PackageReference Include=""Microsoft.EntityFrameworkCore.SqlServer"" Version=""9.0.7"" />
    <PackageReference Include=""Microsoft.Extensions.Logging"" Version=""9.0.7"" />
    <PackageReference Include=""Microsoft.Extensions.Logging.Abstractions"" Version=""9.0.7"" />
    <PackageReference Include=""Microsoft.Extensions.Logging.Console"" Version=""9.0.7"" />
    <PackageReference Include=""Microsoft.Extensions.Logging.Debug"" Version=""9.0.7"" />
    <PackageReference Include=""MQTTnet"" Version=""4.3.4.1084"" />
    <PackageReference Include=""Newtonsoft.Json"" Version=""13.0.3"" />
    <PackageReference Include=""NLog.Extensions.Logging"" Version=""5.3.15"" />
    <PackageReference Include=""NModbus"" Version=""3.0.81"" />
    <PackageReference Include=""Serilog"" Version=""4.2.0"" />
    <PackageReference Include=""Serilog.Enrichers.CallerInfo"" Version=""1.0.5"" />
    <PackageReference Include=""Serilog.Extensions.Logging"" Version=""9.0.0"" />
    <PackageReference Include=""Serilog.Sinks.File"" Version=""6.0.0"" />
    <PackageReference Include=""SqlSugarCore"" Version=""5.1.4.182"" />
    <PackageReference Include=""System.Drawing.Common"" Version=""9.0.0"" />
    <PackageReference Include=""System.Text.Encoding"" Version=""4.3.0"" />
    <PackageReference Include=""Yitter.IdGenerator"" Version=""1.0.14"" />
  </ItemGroup>");

            sb.AppendLine(@"<ItemGroup>
  <Reference Include=""QStandaedPlatform.Engine"">
    <HintPath>..\Libraries\QStandaedPlatform.Engine.dll</HintPath>
  </Reference>
  <Reference Include=""System.Foundation.Modules"">
    <HintPath>..\Libraries\System.Foundation.Modules.dll</HintPath>
    </Reference>
  <Reference Include=""Tangerine.Grpc.Foundation"">
    <HintPath>..\Libraries\Tangerine.Grpc.Foundation.dll</HintPath>
  </Reference>
  <Reference Include=""Tangerine.Grpc.Protos"">
    <HintPath>..\Libraries\Tangerine.Grpc.Protos.dll</HintPath>
  </Reference>
  </ItemGroup>");

            sb.AppendLine(@"<!-- ========== Publish 阶段复制文件 ========== -->
	<Target Name=""CopyFilesAfterPublish"" AfterTargets=""AfterPublish"">
		<ItemGroup>
			<MyFiles Include=""**""
					 Exclude=""**/*.json;
                        **/*.user;
                        **/*.suo;
                        **/.git/**;
                        **/.gitignore;
                        **/.vs/**;
                        **/bin/**;
                        **/obj/**;
                        **/DS_Store"" />
		</ItemGroup>

		<Copy SourceFiles=""@(MyFiles)""
			  DestinationFolder=""$(OutputPath)\publish\%(RecursiveDir)""
			  SkipUnchangedFiles=""true"" />
	</Target>");

//            sb.AppendLine(@"<Target Name=""CreateInstallScriptAfterPublish"" AfterTargets=""AfterPublish"">
//  <!-- 获取项目输出的主文件名（如 HAIHE_ONE） -->
//  <PropertyGroup>
//    <ExeFileName>$([System.IO.Path]::GetFileNameWithoutExtension('$(TargetPath)'))</ExeFileName>
//    <PublishDir>$(PublishDir.TrimEnd('\'))</PublishDir>
//    <ScriptPath>$(PublishDir)\install-service.ps1</ScriptPath>
//  </PropertyGroup>

//  <!-- 定义要写入 .ps1 的内容，使用实际的 EXE 名称 -->
// <ItemGroup>
//  <PsScriptLines Include=""## 自动创建的服务安装脚本"" />
//  <PsScriptLines Include=""# 使用方式：右键 '以管理员身份运行' 此脚本"" />
//  <PsScriptLines Include=""`$serviceName = '${ExeFileName}_AppService'"" />
//  <PsScriptLines Include=""`$exePath = Join-Path (Get-Location) '${ExeFileName}.exe'"" />
//  <PsScriptLines Include=""`$displayName = '${ExeFileName} Windows Service'"" />
  
//  <!-- 空行：用一个空格代替 -->
//  <PsScriptLines Include="" "" />
  
//  <PsScriptLines Include=""if (-not (Test-Path `$exePath)) {"" />
//  <PsScriptLines Include=""    Write-Error '找不到程序文件: `$exePath'"" />
//  <PsScriptLines Include=""    exit 1"" />
//  <PsScriptLines Include=""}"" />
  
//  <!-- 空行 -->
//  <PsScriptLines Include="" "" />
  
//  <PsScriptLines Include=""try {"" />
//  <PsScriptLines Include=""    if (Get-Service `$serviceName -ErrorAction SilentlyContinue) {"" />
//  <PsScriptLines Include=""        Write-Host '服务已存在: `$serviceName，跳过安装。' -ForegroundColor Yellow"" />
//  <PsScriptLines Include=""    } else {"" />
//  <PsScriptLines Include=""        New-Service -Name `$serviceName -BinaryPathName `$exePath -DisplayName `$displayName -StartupType Automatic"" />
//  <PsScriptLines Include=""        Write-Host '✅ 服务已安装: `$serviceName' -ForegroundColor Green"" />
//  <PsScriptLines Include=""    }"" />
//  <PsScriptLines Include=""} catch {"" />
//  <PsScriptLines Include=""    Write-Error '安装失败: `$($_.Exception.Message)'"" />
//  <PsScriptLines Include=""}"" />
//</ItemGroup>

//  <!-- 将内容写入 .ps1 文件 -->
//  <WriteLinesToFile
//    File=""$(ScriptPath)""
//    Lines=""@(PsScriptLines)""
//    Overwrite=""true""
//    Encoding=""UTF-8"" />
  
//  <!-- 输出提示 -->
//  <Message Text=""✅ 已生成服务安装脚本: $(ScriptPath)"" Importance=""high"" />
//</Target>");

            sb.AppendLine("</Project>");
            return sb.ToString();
        }
        public void CrammTemplateInfo()
        {
            Directory.CreateDirectory(Path.Combine(SolutionRootPath, "Libraries"));
            string[] SearchPatterns = ["*.dll", "*.pdf"];
            var dlls = SearchPatterns.SelectMany(pattern =>
                Directory.GetFiles(AppDomain.CurrentDomain.BaseDirectory, pattern))
                .ToArray();
            foreach (var file in dlls)
            {
                var fileName = Path.GetFileNameWithoutExtension(file);
                if (fileName == "System.Foundation.Modules"
                    || fileName == "QStandaedPlatform.Engine"
                    || fileName == "Tangerine.Grpc.Protos"
                    || fileName == "Tangerine.Grpc.Foundation")
                {
                    File.Copy(file, Path.Combine(SolutionRootPath, "Libraries", Path.GetFileName(file)), true);
                }
            }

            Directory.CreateDirectory(Path.Combine(ProjectDirectory, "Properties"));
            var propertyFile = Path.Combine(ProjectDirectory, "Properties", "launchsettings.json");
            if (!File.Exists(propertyFile))
            {
                File.Create(propertyFile).Close();
            }
            var propertyJson = @"{
  ""$schema"": ""http://json.schemastore.org/launchsettings.json"",
  ""profiles"": {
    ""http"": {
      ""commandName"": ""Project"",
      ""dotnetRunMessages"": true,
      ""launchBrowser"": false,
      ""applicationUrl"": ""http://localhost:5118"",
      ""environmentVariables"": {
        ""ASPNETCORE_ENVIRONMENT"": ""Development""
      }
    },
    ""https"": {
      ""commandName"": ""Project"",
      ""dotnetRunMessages"": true,
      ""launchBrowser"": false,
      ""applicationUrl"": ""https://localhost:7197;http://localhost:5118"",
      ""environmentVariables"": {
        ""ASPNETCORE_ENVIRONMENT"": ""Development""
      }
    }
  }
}
";
            File.WriteAllText(Path.Combine(ProjectDirectory, "Properties", "launchsettings.json"), propertyJson);

            Directory.CreateDirectory(Path.Combine(ProjectDirectory, "GrpcProjectOptions"));

            var grpcProjectOptions = new GrpcProjectOptions()
            {
                ProjectName = ProjectName,
                SolutionName = SolutionName,
                SolutionRootPath = SolutionRootPath,
                ProjectDescription = ProjectDescription,
                LaboratoryCode = LaboratoryCode,
                LaboratoryDescription = LaboratoryDescription,
                LaboratoryId = LaboratoryId,
                LaboratoryName = LaboratoryName,
                ProjectVersion = ProjectVersion,
            };
            foreach (var platformCreate in PlatformCreates)
            {
                grpcProjectOptions.GrpcProjectPlatformOptions.Add(new GrpcProjectPlatformOptions
                {
                    PlatformId = platformCreate.PlatformId,
                    PlatformCode = platformCreate.PlatformCode,
                    PlatformName = platformCreate.PlatformName,
                    PlatformMaxCacheCount = platformCreate.PlatformMaxCacheCount,
                    PlatformMaxExecuteCount = platformCreate.PlatformMaxExecuteCount,
                    PlatformModuleId = platformCreate.PlatformModuleInfoParameterModel?.ModuleInfoParameter.ModuleInfoId ?? Guid.Empty,
                    PlatformGrabActionId = platformCreate.PlatformGrabActionParameterModel?.Parameter.ParameterId ?? Guid.Empty,
                    PlatformSamplingFlux = platformCreate.PlatformSamplingFlux,
                    PlatformDescription = platformCreate.PlatformDescription,
                    InitialFlowConfigs = [.. platformCreate.HomeFiles.Select(P => (GrpcProjectPlatformFlowConfigOptions)P.Clone())],
                    FinalizeFlowConfigs = [.. platformCreate.FinalizeFiles.Select(P => (GrpcProjectPlatformFlowConfigOptions)P.Clone())],
                    LineDrainageFlowConfigs = [.. platformCreate.LineDrainageFiles.Select(P => (GrpcProjectPlatformFlowConfigOptions)P.Clone())],
                    MaintainFlowConfigs = [.. platformCreate.MaintenanceFiles.Select(P => (GrpcProjectPlatformFlowConfigOptions)P.Clone())],
                    PlatformMonitorItems = [.. platformCreate.PlatformMonitorItems.Select(p => (PlatformMonitorItem)p.Model.Clone())],
                    PrepareFlowConfigs = [.. platformCreate.PreperExperimentFiles.Select(P => (GrpcProjectPlatformFlowConfigOptions)P.Clone())],
                    SystemStorageFlowConfigs = [.. platformCreate.SystemStorageFiles.Select(P => (GrpcProjectPlatformFlowConfigOptions)P.Clone())],
                    TaskFlowConfigs = [.. platformCreate.StartTaskFiles.Select(P => (GrpcProjectPlatformFlowConfigOptions)P.Clone())],
                    LabTrayConfigs = platformCreate.StartTaskFiles.Where(p => p.Flow != null).SelectMany(p => p.Flow!.GetLabTrayConfigs()).DistinctBy(p => p.LabTrayId).ToList(),
                });
            }
            foreach (var  grpcProjectPlatformOptions in grpcProjectOptions.GrpcProjectPlatformOptions)
            {
                foreach (var flowConfig in grpcProjectPlatformOptions.InitialFlowConfigs)
                {
                    File.Copy(flowConfig.FlowConfigPath, Path.Combine(ProjectDirectory, "GrpcProjectOptions", Path.GetFileName(flowConfig.FlowConfigPath)), true);

                    flowConfig.FlowConfigPath = Path.Combine("GrpcProjectOptions", Path.GetFileName(flowConfig.FlowConfigPath));

                    if (flowConfig.Flow != null)
                    {
                        var index = 0;
                        foreach (var step in flowConfig.StepDisplays)
                        {
                            var action = flowConfig.Flow.GetTool(step.StepId);
                            if (action != null)
                            {
                                var moduleWithParameterTool = action as IModuleWithParameterTool;
                                if (moduleWithParameterTool != null
                                    && moduleWithParameterTool.ModuleFuncCodeParameter != null
                                    && moduleWithParameterTool.ModuleFuncCodeParameter.ModuleInfoParameter != null)
                                {
                                    var moduleConfig = new SequentialActionConfig
                                    {
                                        Sequential = ++index,
                                        ModuleName = moduleWithParameterTool.ModuleFuncCodeParameter.ModuleInfoParameter.ModuleName,
                                        ModuleSerialNumber = moduleWithParameterTool.ModuleFuncCodeParameter.ModuleInfoParameter.ModuleSerialNumber,
                                        ActionId = action.UniqueId,
                                        ActionDescription = moduleWithParameterTool.ModuleFuncCodeParameter.FuncCodeDescription,
                                        ActionName = action.DisplayName,
                                    };
                                    foreach (var funcCodeParameter in moduleWithParameterTool.ModuleFuncCodeParameter.FuncCodeParamterInfos)
                                    {
                                        moduleConfig.Parameters.Add(new ParameterItem
                                        {
                                            Description = funcCodeParameter.ParameterDescription,
                                            MaxValue = funcCodeParameter.ParameterMaxValue,
                                            MinValue = funcCodeParameter.ParameterMinValue,
                                            Name = funcCodeParameter.ParameterName,
                                            Unit = funcCodeParameter.ParameterUnit,
                                            ParameterId = funcCodeParameter.ParameterId,
                                            Value = funcCodeParameter.ParameterValueFactory.First().Value
                                        });
                                    }
                                    flowConfig.ActionConfigs.Add(moduleConfig);
                                }
                            }
                        }
                    }
                }
                foreach (var flowConfig in grpcProjectPlatformOptions.FinalizeFlowConfigs)
                {
                    File.Copy(flowConfig.FlowConfigPath, Path.Combine(ProjectDirectory, "GrpcProjectOptions", Path.GetFileName(flowConfig.FlowConfigPath)), true);

                    flowConfig.FlowConfigPath = Path.Combine("GrpcProjectOptions", Path.GetFileName(flowConfig.FlowConfigPath));
                    if (flowConfig.Flow != null)
                    {
                        var index = 0;
                        foreach (var step in flowConfig.StepDisplays)
                        {
                            var action = flowConfig.Flow.GetTool(step.StepId);
                            if (action != null)
                            {
                                var moduleWithParameterTool = action as IModuleWithParameterTool;
                                if (moduleWithParameterTool != null
                                    && moduleWithParameterTool.ModuleFuncCodeParameter != null
                                    && moduleWithParameterTool.ModuleFuncCodeParameter.ModuleInfoParameter != null)
                                {
                                    var moduleConfig = new SequentialActionConfig
                                    {
                                        Sequential = ++index,
                                        ModuleName = moduleWithParameterTool.ModuleFuncCodeParameter.ModuleInfoParameter.ModuleName,
                                        ModuleSerialNumber = moduleWithParameterTool.ModuleFuncCodeParameter.ModuleInfoParameter.ModuleSerialNumber,
                                        ActionId = action.UniqueId,
                                        ActionDescription = moduleWithParameterTool.ModuleFuncCodeParameter.FuncCodeDescription,
                                        ActionName = action.DisplayName,
                                    };
                                    foreach (var funcCodeParameter in moduleWithParameterTool.ModuleFuncCodeParameter.FuncCodeParamterInfos)
                                    {
                                        moduleConfig.Parameters.Add(new ParameterItem
                                        {
                                            Description = funcCodeParameter.ParameterDescription,
                                            MaxValue = funcCodeParameter.ParameterMaxValue,
                                            MinValue = funcCodeParameter.ParameterMinValue,
                                            Name = funcCodeParameter.ParameterName,
                                            Unit = funcCodeParameter.ParameterUnit,
                                            ParameterId = funcCodeParameter.ParameterId,
                                            Value = funcCodeParameter.ParameterValueFactory.First().Value
                                        });
                                    }
                                    flowConfig.ActionConfigs.Add(moduleConfig);
                                }
                            }
                        }
                    }
                }
                foreach (var flowConfig in grpcProjectPlatformOptions.MaintainFlowConfigs)
                {
                    File.Copy(flowConfig.FlowConfigPath, Path.Combine(ProjectDirectory, "GrpcProjectOptions", Path.GetFileName(flowConfig.FlowConfigPath)), true);

                    flowConfig.FlowConfigPath = Path.Combine( "GrpcProjectOptions", Path.GetFileName(flowConfig.FlowConfigPath));
                    if (flowConfig.Flow != null)
                    {
                        var index = 0;
                        foreach (var step in flowConfig.StepDisplays)
                        {
                            var action = flowConfig.Flow.GetTool(step.StepId);
                            if (action != null)
                            {
                                var moduleWithParameterTool = action as IModuleWithParameterTool;
                                if (moduleWithParameterTool != null
                                    && moduleWithParameterTool.ModuleFuncCodeParameter != null
                                    && moduleWithParameterTool.ModuleFuncCodeParameter.ModuleInfoParameter != null)
                                {
                                    var moduleConfig = new SequentialActionConfig
                                    {
                                        Sequential = ++index,
                                        ModuleName = moduleWithParameterTool.ModuleFuncCodeParameter.ModuleInfoParameter.ModuleName,
                                        ModuleSerialNumber = moduleWithParameterTool.ModuleFuncCodeParameter.ModuleInfoParameter.ModuleSerialNumber,
                                        ActionId = action.UniqueId,
                                        ActionDescription = moduleWithParameterTool.ModuleFuncCodeParameter.FuncCodeDescription,
                                        ActionName = action.DisplayName,
                                    };
                                    foreach (var funcCodeParameter in moduleWithParameterTool.ModuleFuncCodeParameter.FuncCodeParamterInfos)
                                    {
                                        moduleConfig.Parameters.Add(new ParameterItem
                                        {
                                            Description = funcCodeParameter.ParameterDescription,
                                            MaxValue = funcCodeParameter.ParameterMaxValue,
                                            MinValue = funcCodeParameter.ParameterMinValue,
                                            Name = funcCodeParameter.ParameterName,
                                            Unit = funcCodeParameter.ParameterUnit,
                                            ParameterId = funcCodeParameter.ParameterId,
                                            Value = funcCodeParameter.ParameterValueFactory.First().Value
                                        });
                                    }
                                    flowConfig.ActionConfigs.Add(moduleConfig);
                                }
                            }
                        }
                    }
                }

                foreach (var flowConfig in grpcProjectPlatformOptions.LineDrainageFlowConfigs)
                {
                    File.Copy(flowConfig.FlowConfigPath, Path.Combine(ProjectDirectory, "GrpcProjectOptions", Path.GetFileName(flowConfig.FlowConfigPath)), true);

                    flowConfig.FlowConfigPath = Path.Combine("GrpcProjectOptions", Path.GetFileName(flowConfig.FlowConfigPath));
                    if (flowConfig.Flow != null)
                    {
                        var index = 0;
                        foreach (var step in flowConfig.StepDisplays)
                        {
                            var action = flowConfig.Flow.GetTool(step.StepId);
                            if (action != null)
                            {
                                var moduleWithParameterTool = action as IModuleWithParameterTool;
                                if (moduleWithParameterTool != null
                                    && moduleWithParameterTool.ModuleFuncCodeParameter != null
                                    && moduleWithParameterTool.ModuleFuncCodeParameter.ModuleInfoParameter != null)
                                {
                                    var moduleConfig = new SequentialActionConfig
                                    {
                                        Sequential = ++index,
                                        ModuleName = moduleWithParameterTool.ModuleFuncCodeParameter.ModuleInfoParameter.ModuleName,
                                        ModuleSerialNumber = moduleWithParameterTool.ModuleFuncCodeParameter.ModuleInfoParameter.ModuleSerialNumber,
                                        ActionId = action.UniqueId,
                                        ActionDescription = moduleWithParameterTool.ModuleFuncCodeParameter.FuncCodeDescription,
                                        ActionName = action.DisplayName,
                                    };
                                    foreach (var funcCodeParameter in moduleWithParameterTool.ModuleFuncCodeParameter.FuncCodeParamterInfos)
                                    {
                                        moduleConfig.Parameters.Add(new ParameterItem
                                        {
                                            Description = funcCodeParameter.ParameterDescription,
                                            MaxValue = funcCodeParameter.ParameterMaxValue,
                                            MinValue = funcCodeParameter.ParameterMinValue,
                                            Name = funcCodeParameter.ParameterName,
                                            Unit = funcCodeParameter.ParameterUnit,
                                            ParameterId = funcCodeParameter.ParameterId,
                                            Value = funcCodeParameter.ParameterValueFactory.First().Value
                                        });
                                    }
                                    flowConfig.ActionConfigs.Add(moduleConfig);
                                }
                            }
                        }
                    }
                }

                foreach (var flowConfig in grpcProjectPlatformOptions.PrepareFlowConfigs)
                {
                    File.Copy(flowConfig.FlowConfigPath, Path.Combine(ProjectDirectory, "GrpcProjectOptions", Path.GetFileName(flowConfig.FlowConfigPath)), true);

                    flowConfig.FlowConfigPath = Path.Combine( "GrpcProjectOptions", Path.GetFileName(flowConfig.FlowConfigPath));
                    if (flowConfig.Flow != null)
                    {
                        var index = 0;
                        foreach (var step in flowConfig.StepDisplays)
                        {
                            var action = flowConfig.Flow.GetTool(step.StepId);
                            if (action != null)
                            {
                                var moduleWithParameterTool = action as IModuleWithParameterTool;
                                if (moduleWithParameterTool != null
                                    && moduleWithParameterTool.ModuleFuncCodeParameter != null
                                    && moduleWithParameterTool.ModuleFuncCodeParameter.ModuleInfoParameter != null)
                                {
                                    var moduleConfig = new SequentialActionConfig
                                    {
                                        Sequential = ++index,
                                        ModuleName = moduleWithParameterTool.ModuleFuncCodeParameter.ModuleInfoParameter.ModuleName,
                                        ModuleSerialNumber = moduleWithParameterTool.ModuleFuncCodeParameter.ModuleInfoParameter.ModuleSerialNumber,
                                        ActionId = action.UniqueId,
                                        ActionDescription = moduleWithParameterTool.ModuleFuncCodeParameter.FuncCodeDescription,
                                        ActionName = action.DisplayName,
                                    };
                                    foreach (var funcCodeParameter in moduleWithParameterTool.ModuleFuncCodeParameter.FuncCodeParamterInfos)
                                    {
                                        moduleConfig.Parameters.Add(new ParameterItem
                                        {
                                            Description = funcCodeParameter.ParameterDescription,
                                            MaxValue = funcCodeParameter.ParameterMaxValue,
                                            MinValue = funcCodeParameter.ParameterMinValue,
                                            Name = funcCodeParameter.ParameterName,
                                            Unit = funcCodeParameter.ParameterUnit,
                                            ParameterId = funcCodeParameter.ParameterId,
                                            Value = funcCodeParameter.ParameterValueFactory.First().Value
                                        });
                                    }
                                    flowConfig.ActionConfigs.Add(moduleConfig);
                                    if (moduleWithParameterTool.ModuleFuncCodeParameter.IsMonitorFuncCodeParameterFeedback)
                                    {
                                        var moduleTool = action as IModuleTool;
                                        if (moduleTool != null)
                                        {
                                            foreach (var item in moduleWithParameterTool.ModuleFuncCodeParameter.FuncCodeParamterInfos)
                                            {
                                                if (!string.IsNullOrEmpty(item.ParameterFeedbackAddress))
                                                {
                                                    grpcProjectPlatformOptions.PlatformMonitorItems.Add(new PlatformMonitorItem
                                                    {
                                                        MonitorType = ReadType.REAL,
                                                        ClientId = PartManager.Instance.GetPartId((IPart)moduleTool.GetModular().Messenger),
                                                        ModuleInfoId = moduleWithParameterTool.ModuleFuncCodeParameter.ModuleInfoParameter.ModuleInfoId,
                                                        ModuleName = moduleWithParameterTool.ModuleFuncCodeParameter.ModuleInfoParameter.ModuleName,
                                                        MonitorAddress = item.ParameterFeedbackAddress,
                                                        MonitorKey = item.ParameterName,
                                                        MonitorKeyDescription = item.ParameterDescription,
                                                        MonitorUnit = item.ParameterUnit,
                                                    });
                                                }
                                            }
                                        }
                                    }
                                }
                            }
                        }
                    }
                }
                foreach (var flowConfig in grpcProjectPlatformOptions.SystemStorageFlowConfigs)
                {
                    File.Copy(flowConfig.FlowConfigPath, Path.Combine(ProjectDirectory, "GrpcProjectOptions", Path.GetFileName(flowConfig.FlowConfigPath)), true);

                    flowConfig.FlowConfigPath = Path.Combine("GrpcProjectOptions", Path.GetFileName(flowConfig.FlowConfigPath));
                    if (flowConfig.Flow != null)
                    {
                        var index = 0;
                        foreach (var step in flowConfig.StepDisplays)
                        {
                            var action = flowConfig.Flow.GetTool(step.StepId);
                            if (action != null)
                            {
                                var moduleWithParameterTool = action as IModuleWithParameterTool;
                                if (moduleWithParameterTool != null
                                    && moduleWithParameterTool.ModuleFuncCodeParameter != null
                                    && moduleWithParameterTool.ModuleFuncCodeParameter.ModuleInfoParameter != null)
                                {
                                    var moduleConfig = new SequentialActionConfig
                                    {
                                        Sequential = ++index,
                                        ModuleName = moduleWithParameterTool.ModuleFuncCodeParameter.ModuleInfoParameter.ModuleName,
                                        ModuleSerialNumber = moduleWithParameterTool.ModuleFuncCodeParameter.ModuleInfoParameter.ModuleSerialNumber,
                                        ActionId = action.UniqueId,
                                        ActionDescription = moduleWithParameterTool.ModuleFuncCodeParameter.FuncCodeDescription,
                                        ActionName = action.DisplayName,
                                    };
                                    foreach (var funcCodeParameter in moduleWithParameterTool.ModuleFuncCodeParameter.FuncCodeParamterInfos)
                                    {
                                        moduleConfig.Parameters.Add(new ParameterItem
                                        {
                                            Description = funcCodeParameter.ParameterDescription,
                                            MaxValue = funcCodeParameter.ParameterMaxValue,
                                            MinValue = funcCodeParameter.ParameterMinValue,
                                            Name = funcCodeParameter.ParameterName,
                                            Unit = funcCodeParameter.ParameterUnit,
                                            ParameterId = funcCodeParameter.ParameterId,
                                            Value = funcCodeParameter.ParameterValueFactory.First().Value
                                        });
                                    }
                                    flowConfig.ActionConfigs.Add(moduleConfig);
                                }
                            }
                        }
                    }
                }
                foreach (var flowConfig in grpcProjectPlatformOptions.TaskFlowConfigs)
                {
                    File.Copy(flowConfig.FlowConfigPath, Path.Combine(ProjectDirectory, "GrpcProjectOptions", Path.GetFileName(flowConfig.FlowConfigPath)), true);

                    flowConfig.FlowConfigPath = Path.Combine( "GrpcProjectOptions", Path.GetFileName(flowConfig.FlowConfigPath));

                    if (flowConfig.Flow != null)
                    {
                        var index = 0;
                        flowConfig.ActionConfigs.Clear();
                        flowConfig.TaskEbrParameterConfigs.Clear();
                        foreach (var step in flowConfig.StepDisplays)
                        {
                            var action = flowConfig.Flow.GetTool(step.StepId);
                            if (action != null)
                            {
                                var moduleWithParameterTool = action as IModuleWithParameterTool;
                                if (moduleWithParameterTool != null
                                    && moduleWithParameterTool.ModuleFuncCodeParameter != null
                                    && moduleWithParameterTool.ModuleFuncCodeParameter.ModuleInfoParameter != null)
                                {
                                    var moduleConfig = new SequentialActionConfig
                                    {
                                        Sequential = ++index,
                                        ModuleName = moduleWithParameterTool.ModuleFuncCodeParameter.ModuleInfoParameter.ModuleName,
                                        ModuleSerialNumber = moduleWithParameterTool.ModuleFuncCodeParameter.ModuleInfoParameter.ModuleSerialNumber,
                                        ActionId = moduleWithParameterTool.ModuleFuncCodeParameter.ParameterId,
                                        ActionDescription = moduleWithParameterTool.ModuleFuncCodeParameter.FuncCodeDescription,
                                        ActionName = action.DisplayName,
                                    };
                                    foreach (var funcCodeParameter in moduleWithParameterTool.ModuleFuncCodeParameter.FuncCodeParamterInfos)
                                    {
                                        moduleConfig.Parameters.Add(new ParameterItem
                                        {
                                            Description = funcCodeParameter.ParameterDescription,
                                            MaxValue = funcCodeParameter.ParameterMaxValue,
                                            MinValue = funcCodeParameter.ParameterMinValue,
                                            Name = funcCodeParameter.ParameterName,
                                            Unit = funcCodeParameter.ParameterUnit,
                                            ParameterId = funcCodeParameter.ParameterId,
                                            Value = funcCodeParameter.ParameterValueFactory.First().Value
                                        });
                                        EbrParameterConfig config = new()
                                        {
                                            ModuleName = moduleWithParameterTool.ModuleFuncCodeParameter.ModuleInfoParameter.ModuleName,
                                            ModuleActionDescription = moduleWithParameterTool.ModuleFuncCodeParameter.FuncCodeDescription,
                                            ModuleActionId = funcCodeParameter.ParameterId.ToString(),
                                            EbrUnit = funcCodeParameter.ParameterUnit,
                                            EbrName = funcCodeParameter.ParameterName,
                                            EbrType =  EbrType.REAL,
                                            EbrDescription = funcCodeParameter.ParameterDescription
                                        };
                                        flowConfig.TaskEbrParameterConfigs.Add(config);
                                    }
                                    flowConfig.ActionConfigs.Add(moduleConfig);
                                   var moduleEbrInfoItems = moduleWithParameterTool.ModuleFuncCodeParameter.ChannelEbrInfos.SelectMany(p => p.Value).ToArray();
                                    foreach (var moduleEbrInfoItem in moduleEbrInfoItems)
                                    {
                                        EbrParameterConfig config = new()
                                        {
                                            ModuleName = moduleWithParameterTool.ModuleFuncCodeParameter.ModuleInfoParameter.ModuleName,
                                            ModuleActionDescription = moduleWithParameterTool.ModuleFuncCodeParameter.FuncCodeDescription,
                                            ModuleActionId = moduleWithParameterTool.ModuleFuncCodeParameter.ParameterId.ToString(),
                                            EbrUnit = moduleEbrInfoItem.EbrUnit,
                                            EbrName = moduleEbrInfoItem.EbrName,
                                            EbrType = moduleEbrInfoItem.EbrType,
                                            EbrDescription = moduleEbrInfoItem.EbrDescription
                                        };
                                        flowConfig.TaskEbrParameterConfigs.Add(config);
                                    }

                                    if (moduleWithParameterTool.ModuleFuncCodeParameter.IsMonitorFuncCodeParameterFeedback)
                                    {
                                        var moduleTool = action as IModuleTool;
                                        if (moduleTool != null)
                                        {
                                            foreach (var item in moduleWithParameterTool.ModuleFuncCodeParameter.FuncCodeParamterInfos)
                                            {
                                                if (!string.IsNullOrEmpty(item.ParameterFeedbackAddress))
                                                {
                                                    grpcProjectPlatformOptions.PlatformMonitorItems.Add(new PlatformMonitorItem
                                                    {
                                                        MonitorType = ReadType.REAL,
                                                        ClientId = PartManager.Instance.GetPartId((IPart)moduleTool.GetModular().Messenger),
                                                        ModuleInfoId = moduleWithParameterTool.ModuleFuncCodeParameter.ModuleInfoParameter.ModuleInfoId,
                                                        ModuleName = moduleWithParameterTool.ModuleFuncCodeParameter.ModuleInfoParameter.ModuleName,
                                                        MonitorAddress = item.ParameterFeedbackAddress,
                                                        MonitorKey = item.ParameterName,
                                                        MonitorKeyDescription = item.ParameterDescription,
                                                        MonitorUnit = item.ParameterUnit,
                                            });
                                                }
                                            }
                                        }
                                    }
                                    if (moduleWithParameterTool.ModuleFuncCodeParameter.MonitorInfoItems.Count > 0)
                                    {
                                        var moduleTool = action as IModuleTool;
                                        if (moduleTool != null)
                                        {
                                            foreach (var item in moduleWithParameterTool.ModuleFuncCodeParameter.MonitorInfoItems)
                                            {
                                                grpcProjectPlatformOptions.PlatformMonitorItems.Add(new PlatformMonitorItem 
                                                {
                                                    MonitorType = item.MonitorType,
                                                    ClientId = PartManager.Instance.GetPartId((IPart)moduleTool.GetModular().Messenger),
                                                    ModuleInfoId = moduleWithParameterTool.ModuleFuncCodeParameter.ModuleInfoParameter.ModuleInfoId,
                                                    ModuleName = moduleWithParameterTool.ModuleFuncCodeParameter.ModuleInfoParameter.ModuleName,
                                                    MonitorAddress = item.MonitorAddress,
                                                    MonitorKey = item.MonitorName,
                                                    MonitorKeyDescription = item.MonitorDescription,
                                                    MonitorUnit = item.MonitorUnit
                                                });
                                            }
                                        }
                                    }
                                }
                            }
                        }
                    }
                }
            }

            foreach (var grpcProjectProcessflow in Processflows)
            {
                grpcProjectOptions.GrpcProjectProcessflowOptions.Add(grpcProjectProcessflow.GrpcProjectProcessflowOptinos);
            }
            foreach (var grpcProjectProduct in ProductLines)
            {
                grpcProjectOptions.GrpcProjectProductlineOptions.Add(grpcProjectProduct.GrpcProjectProductlineOptions);
            }
            foreach (var transferModule in this.TransferModules)
            {
                grpcProjectOptions.GrpcProjectTransferModuleOptions.Add(transferModule.GrpcProjectTransferModuleOptions);
            }

            if (!File.Exists(Path.Combine(ProjectDirectory, "GrpcProjectOptions", "GrpcProjectOptions.json")))
            {
                File.Create(Path.Combine(ProjectDirectory, "GrpcProjectOptions", "GrpcProjectOptions.json")).Close();
            }
            File.WriteAllText(Path.Combine(ProjectDirectory, "GrpcProjectOptions", "GrpcProjectOptions.json"), JsonConvert.SerializeObject(grpcProjectOptions));


            Directory.CreateDirectory(Path.Combine(ProjectDirectory, "Tables"));
            var files = Directory.GetFiles(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Tables"));
            foreach (var file in files)
            {
                File.Copy(file, Path.Combine(ProjectDirectory, "Tables", Path.GetFileName(file)), true);
            }

//            string protosDir = Path.Combine(ProjectDirectory, "Protos");
//            Directory.CreateDirectory(protosDir);

//            string protoFilePath = Path.Combine(protosDir, "hello.proto");

//            string protoContent = @"syntax = ""proto3"";

//option csharp_namespace = ""Tangerine.Grpc.Hello"";

//package hello;

//// 请求
//message HelloRequest {
//  string name = 1;
//}

//// 响应
//message HelloResponse {
//  string message = 1;
//}

//// 服务
//service Greeter {
//  rpc SayHello (HelloRequest) returns (HelloResponse);
//}
//";

//            // 写入文件
//            File.WriteAllText(protoFilePath, protoContent, System.Text.Encoding.UTF8);

            //Directory.CreateDirectory(Path.Combine(ProjectDirectory, "Services"));

            //File.Copy(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Services", "Smart_Lab_OSService.cs"), Path.Combine(ProjectDirectory, "Services", "Smart_Lab_OSService.cs"), true);

            //File.Copy(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Services", "AppService.cs"), Path.Combine(ProjectDirectory, "Services", "AppService.cs"), true);

            File.Copy(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "WorkFlowEngine.engine"),Path.Combine(ProjectDirectory, "WorkFlowEngine.engine"),true);

            var programFile = Path.Combine(ProjectDirectory, "Program.cs");
            if (!File.Exists(programFile))
            {
                File.Create(programFile).Close();
            }
            File.WriteAllText(programFile, GererateProgramCsContent());

            var appsettingsFile = Path.Combine(ProjectDirectory, "appsettings.json");
            if (!File.Exists(appsettingsFile))
            {
                File.Create(appsettingsFile).Close();
            }
            var appJson = @"{
  ""Logging"": {
    ""LogLevel"": {
      ""Default"": ""Information"",
      ""Microsoft.AspNetCore"": ""Warning""
    }
  },
  ""AllowedHosts"": ""*"",
  ""Kestrel"": {
    ""Endpoints"": {
      ""Http"": {
        ""Url"": ""http://*:5118"",
        ""Protocols"": ""Http2""
      }
    }
  }
}";
            File.WriteAllText(Path.Combine(ProjectDirectory, "appsettings.json"), appJson);
        }



        public static string GererateProgramCsContent()
        {
            StringBuilder sb = new();
            sb.AppendLine(@"
using Newtonsoft.Json;
using QStandaedPlatform.Engine.Common.Common;
using QStandaedPlatform.Engine.Extensions;
using QStandaedPlatform.Engine.Laboratory;
using System.Foundation.Modules.Models;
using Tangerine.Grpc.Foundation.Services;
namespace Smart.Lab.OS
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            builder.Services.AddMangoStorage();
            builder.Services.AddFoundationModules();
            builder.Services.AddSingleton<AppLabOsLogicService>();
            builder.Services.AddGrpc();
            builder.Services.AddCors(options =>
            {
                options.AddPolicy(""AllowAll"", policy =>
                {
                    policy.AllowAnyOrigin()
        .AllowAnyMethod()
        .AllowAnyHeader()
        .WithExposedHeaders(""grpc-status"", ""grpc-message"");
                });
            });
            if (OperatingSystem.IsWindows())
            {
                builder.Host.UseWindowsService();
            }
            var app = builder.Build();
            Container.ConfigProvider(app.Services);
            app.UseRouting();
            app.UseCors(""AllowAll"");
            app.UseGrpcWeb(new GrpcWebOptions { DefaultEnabled = true });

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapGrpcService<Tangerine.Grpc.Foundation.Services.ConfigServiceImpl>();
                endpoints.MapGrpcService<Tangerine.Grpc.Foundation.Services.PlatformInitServiceImpl>();
                endpoints.MapGrpcService<Tangerine.Grpc.Foundation.Services.PlatformMonitorServiceImpl>();
                endpoints.MapGrpcService<Tangerine.Grpc.Foundation.Services.PlatformControlServiceImpl>();
                endpoints.MapGrpcService<Tangerine.Grpc.Foundation.Services.PlatformTaskServiceImpl>();
                endpoints.MapGrpcService<Tangerine.Grpc.Foundation.Services.PlatformTransferServiceImpl>();

            });
            app.Lifetime.ApplicationStarted.Register(() =>
            {
                var appSerivce = app.Services.GetRequiredService<AppLabOsLogicService>();
                appSerivce.Initialize();
                var optionsPath = Path.Combine(AppContext.BaseDirectory, ""GrpcProjectOptions"", ""GrpcProjectOptions.json"");
                var options = JsonConvert.DeserializeObject<GrpcProjectOptions>(File.ReadAllText(optionsPath));
                if (options != null)
                {
                    appSerivce.Configure(options);
                }
                else
                    throw new Exception(""<GrpcProjectOptions.json>配置文件不存在"");
            });
            app.Lifetime.ApplicationStopping.Register(() =>
            {
                var appSerivce = app.Services.GetRequiredService<AppLabOsLogicService>();
                appSerivce.Shutdown();
            });
            app.Run();
        }
    }
}//dotnet publish -c Release -r win-x64 --self-contained false




");
            return FormatCode(sb.ToString());
        }

        public static string FormatCode(string code)
        {
            SyntaxTree syntaxTree = CSharpSyntaxTree.ParseText(code);
            SyntaxNode root = syntaxTree.GetRoot();
            using var workspace = new AdhocWorkspace();

            OptionSet options = workspace.Options
                .WithChangedOption(CSharpFormattingOptions.NewLinesForBracesInMethods, true)
                .WithChangedOption(CSharpFormattingOptions.IndentBraces, false);

            SyntaxNode formattedNode = Microsoft.CodeAnalysis.Formatting.Formatter.Format(root, workspace, options);
            return formattedNode.ToFullString();
        }

        [RelayCommand]
        private void RunProject()
        {
           
        }

        [RelayCommand]
        public void CloseProject()
        {
           
        }

        public ObservableCollection<ProcessDebug> ProcessDebugs { get; set; } = [];

    }



}
