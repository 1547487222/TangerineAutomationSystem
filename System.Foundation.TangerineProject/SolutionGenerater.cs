using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace System.Foundation.TangerineProject
{
    public class ProjectOption
    {
        public string ProjectName { get; set; }

        public string ProjectDescrtion { get; set; }

        public ProjectType ProjectType { get; set; }
    }
    public enum ProjectType
    {
        GrpcServer,
        WinForm,
        Wpf,
        Console,
        BlazorServer,
        BlazorWebAssembly,
        WebApi,
        Maui,
    }
    public class ProjectGenerater
    {
        /// <summary>
        /// 项目名称
        /// </summary>
        public string ProjectName { get; set; }
        /// <summary>
        /// 项目描述
        /// </summary>
        public string ProjectDescription { get; set; }
        /// <summary>
        /// 项目输出路径
        /// </summary>
        public string OutputDirectory { get; set; }

        public string ProjectPath { get; set; }
        /// <summary>
        /// 生成Grpcserver项目
        /// </summary>
        public void DotCommandGenerateGrpcServerProject()
        {
            ProjectPath = System.IO.Path.Combine(OutputDirectory, ProjectName);
            string command = $"dotnet new grpc -n \"{ProjectName}\" -o \"{ProjectPath}\"";
            ProcessTask.ExecuteCommand(command);
        }

        public void DotCommandGenerateWinFormProject()
        {
            ProjectPath = System.IO.Path.Combine(OutputDirectory, ProjectName);
            string command = $"dotnet new winforms -n \"{ProjectName}\" -o \"{ProjectPath}\"";
            ProcessTask.ExecuteCommand(command);
        }
        public void DotCommandGenerateConsoleProject()
        {
            ProjectPath = System.IO.Path.Combine(OutputDirectory, ProjectName);
            string command = $"dotnet new console -n \"{ProjectName}\" -o \"{ProjectPath}\"";
            ProcessTask.ExecuteCommand(command);
        }
        public void DotCommandGenerateWpfProject()
        {
            ProjectPath = System.IO.Path.Combine(OutputDirectory, ProjectName);
            string command = $"dotnet new wpf -n \"{ProjectName}\" -o \"{ProjectPath}\"";
            ProcessTask.ExecuteCommand(command);
        }
        public void DotCommandGenerateBlazorServerProject()
        {
            ProjectPath = System.IO.Path.Combine(OutputDirectory, ProjectName);
            string command = $"dotnet new blazorserver -n \"{ProjectName}\" -o \"{ProjectPath}\"";
            ProcessTask.ExecuteCommand(command);
        }
        public void DotCommandGenerateBlazorWebAssemblyProject()
        {
            ProjectPath = System.IO.Path.Combine(OutputDirectory, ProjectName);
            string command = $"dotnet new blazorwasm -n \"{ProjectName}\" -o \"{ProjectPath}\"";
            ProcessTask.ExecuteCommand(command);
        }
        public void DotCommandGenerateWebApiProject()
        {
            ProjectPath = System.IO.Path.Combine(OutputDirectory, ProjectName);
            string command = $"dotnet new webapi -n \"{ProjectName}\" -o \"{ProjectPath}\"";
            ProcessTask.ExecuteCommand(command);
        }
        public void DotCommandGenerateMAUIProject()
        {
            ProjectPath = System.IO.Path.Combine(OutputDirectory, ProjectName);
            string command = $"dotnet new maui -n \"{ProjectName}\" -o \"{ProjectPath}\"";
            ProcessTask.ExecuteCommand(command);
        }
    }
    public class SolutionGenerater
    {
        private readonly List<ProjectGenerater> _projectGeneraters = new List<ProjectGenerater>();


        public List<ProjectGenerater>  ProjectGeneraters => _projectGeneraters;
        /// <summary>
        /// 解决方案名称
        /// </summary>
        public string SolutionName { get; set; }
        /// <summary>
        /// 方案输出路径
        /// </summary>
        public string OutputDirectory { get; set; }


        public string SolutionPath { get; set; }

        public void CrateSolution()
        {
            SolutionPath = Path.Combine(OutputDirectory, $"{SolutionName}.sln");
            string createSolutionCommand = $"dotnet new sln -n \"{SolutionName}\" -o \"{OutputDirectory}\"";
            ProcessTask.ExecuteCommand(createSolutionCommand);
        }

        //添加已有项目
        public void AddProject(string projectPath)
        {
            if (!File.Exists(projectPath))
                throw new FileNotFoundException("项目文件未找到，请确保已创建项目文件。", projectPath);
            string addProjectCommand = $"dotnet sln \"{SolutionPath}\" add \"{projectPath}\"";
            ProcessTask.ExecuteCommand(addProjectCommand);
            var projectGenerater = new ProjectGenerater 
            {
                 OutputDirectory = OutputDirectory,
                 ProjectName = Path.GetFileNameWithoutExtension(projectPath),
                 ProjectPath = projectPath
            };
            _projectGeneraters.Add(projectGenerater);
        }

        public void AddProject(ProjectOption option)
        {
            ProjectGenerater projectGenerater = new()
            {
                ProjectName = option.ProjectName,
                ProjectDescription = option.ProjectDescrtion,
                OutputDirectory = OutputDirectory
            };

            switch (option.ProjectType)
            {
                case ProjectType.GrpcServer:
                    projectGenerater.DotCommandGenerateGrpcServerProject();
                    break;
                case ProjectType.WinForm:
                    projectGenerater.DotCommandGenerateWinFormProject();
                    break;
                case ProjectType.Wpf:
                    projectGenerater.DotCommandGenerateWpfProject();
                    break;
                case ProjectType.Console:
                    projectGenerater.DotCommandGenerateConsoleProject();
                    break;
                case ProjectType.BlazorServer:
                    projectGenerater.DotCommandGenerateBlazorServerProject();
                    break;
                case ProjectType.BlazorWebAssembly:
                    projectGenerater.DotCommandGenerateBlazorWebAssemblyProject();
                    break;
                case ProjectType.WebApi:
                    projectGenerater.DotCommandGenerateWebApiProject();
                    break;
                case ProjectType.Maui:
                    projectGenerater.DotCommandGenerateMAUIProject();
                    break;
                default:
                    throw new NotSupportedException($"不支持的项目类型: {option.ProjectType}");
            }

            if (!File.Exists(SolutionPath))
            {
                throw new FileNotFoundException("解决方案文件未找到，请确保已创建解决方案文件。", SolutionPath);
            }

            string projectCsprojPath = Path.Combine(projectGenerater.ProjectPath, $"{projectGenerater.ProjectName}.csproj");
            if (!File.Exists(projectCsprojPath))
            {
                throw new FileNotFoundException("项目文件未找到，请确保项目已成功生成。", projectCsprojPath);
            }

            string addProjectToSolutionCommand = $"dotnet sln \"{SolutionPath}\" add \"{projectCsprojPath}\"";
            try
            {
                ProcessTask.ExecuteCommand(addProjectToSolutionCommand);
                _projectGeneraters.Add(projectGenerater);
                Console.WriteLine($"项目 {projectGenerater.ProjectName} 已成功添加到解决方案中。");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"无法将项目添加到解决方案中: {ex.Message}");
            }
        }
    }
}
