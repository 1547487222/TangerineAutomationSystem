using System.Text;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Formatting;
using Microsoft.CodeAnalysis.Options;
using TangerineSerivce;
namespace System.Foundation.TangerineProject
{
    public class ProgramV1Option
    {
        public string RootPath { get; set; }
        public string SolutionName { get; set; }
        public string ProjectName { get; set; }
        public string Description { get; set; }

        public string SolutionPath { get; set; }

        public string ProjectDirectory { get; set; }

        public string ProjectFile { get; set; }
    }
    public class TangerineProgramV1: ITangerineProgram
    {
        private string rootPath = string.Empty;
        private string solutionName = string.Empty;
        private string projectName = string.Empty;
        private string desc = string.Empty;
        private string solutionPath = string.Empty;
        private string projectDirectory = string.Empty;
        private string projectfile = string.Empty;
        private const string solutionExtension = ".sln";
        private const string projectExtension = ".csproj";
        private readonly string solutionExtensionPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "ProjectSystem", "ProgramV1Option.v1Option");
       // private readonly TangerineTheatreSystem _theatreSystem = new("ProjectSystem");


        public void InitProgram()
        {
            //if (File.Exists(solutionExtensionPath))
            //{
            //    var content = File.ReadAllText(solutionExtensionPath);
            //    var option = JsonConvert.DeserializeObject<ProgramV1Option>(content);
            //    if (option != null)
            //    {
            //        rootPath = option.RootPath;
            //        solutionName = option.SolutionName;
            //        projectName = option.ProjectName;
            //        solutionPath = option.SolutionPath;
            //        projectDirectory = option.ProjectDirectory;
            //        projectfile = option.ProjectFile;
            //        desc = option.Description;
            //    }
            //    CreateProgram(this.rootPath, this.solutionName, this.projectName, this.desc);
            //    CrammTemplateInfo();
            //}
        }


        public void StoreProgram()
        {
            if (string.IsNullOrEmpty(rootPath) 
                || string.IsNullOrEmpty(solutionName) 
                || string.IsNullOrEmpty(projectName) 
                || string.IsNullOrEmpty(solutionPath)
                || string.IsNullOrEmpty(projectDirectory)
                || string.IsNullOrEmpty(projectfile))
                { return; }
            //var option = new ProgramV1Option
            //{
            //    RootPath = rootPath,
            //    SolutionName = solutionName,
            //    ProjectName = projectName,
            //    Description = desc,
            //    SolutionPath = solutionPath,
            //    ProjectDirectory = projectDirectory,
            //    ProjectFile = projectfile
            //};
            //var content = JsonConvert.SerializeObject(option);
            //if (!Directory.Exists(Path.GetDirectoryName(solutionExtensionPath)))
            //{
            //    Directory.CreateDirectory(Path.GetDirectoryName(solutionExtensionPath));
            //}
            //if (!File.Exists(solutionExtensionPath))
            //{
            //    File.Create(solutionExtensionPath).Close();
            //}
            //File.WriteAllText(solutionExtensionPath, content);
        }

        public void CreateProgram(string outputDirectory, string solutionName, string projectName, string desc)
        {
            this.rootPath = outputDirectory;
            this.solutionName = solutionName;
            this.projectName = projectName;
            this.desc = desc;
            Directory.CreateDirectory(outputDirectory);
            solutionPath = Path.Combine(outputDirectory, solutionName + solutionExtension);
            var cmd = "dotnet new sln -n " + solutionName + " -o " + outputDirectory;
            ProcessTask.ExecuteCommand(cmd);
            projectDirectory = Path.Combine(outputDirectory, projectName);
            if (!Directory.Exists(projectDirectory))
            {
                Directory.CreateDirectory(projectDirectory);
            }
            projectfile = Path.Combine(projectDirectory, projectName + projectExtension);
            if (!File.Exists(projectfile))
            {
                File.Create(projectfile).Close();
            }
            string csprojContent = GenerateGrpcCsprojContent(projectName);
            File.WriteAllText(projectfile, csprojContent);
            cmd = "dotnet sln " + solutionPath + " add " + projectfile;
            ProcessTask.ExecuteCommand(cmd);
        }

        public void CrammTemplateInfo()
        {
            Directory.CreateDirectory(Path.Combine(rootPath, "TangerineEngine"));
            var engineDirectory = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "TangerineEngineCode");
            foreach (var file in Directory.GetFiles(engineDirectory))
            {
                if (Path.GetFileNameWithoutExtension(file) == "Program")
                {
                    continue;
                }
                File.Copy(file, Path.Combine(rootPath, "TangerineEngine", Path.GetFileName(file)), true);
            }
            var cmd = "dotnet sln " + solutionPath + " add " + Path.Combine(rootPath, "TangerineEngine", "TangerineEngine.csproj");
            ProcessTask.ExecuteCommand(cmd);

            Directory.CreateDirectory(Path.Combine(projectDirectory, "Properties"));
            var propertyFile = Path.Combine(projectDirectory, "Properties", "launchsettings.json");
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

            File.WriteAllText(Path.Combine(projectDirectory, "Properties", "launchsettings.json"), propertyJson);

            Directory.CreateDirectory(Path.Combine(projectDirectory, "Protos"));

            File.Copy(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Protos", "Smart_Lab_OS.proto"), Path.Combine(projectDirectory, "Protos", "Smart_Lab_OS.proto"), true);

            Directory.CreateDirectory(Path.Combine(projectDirectory, "Services"));

            File.Copy(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Services", "Smart_Lab_OSService.cs"), Path.Combine(projectDirectory, "Services", "Smart_Lab_OSService.cs"), true);

            var programFile = Path.Combine(projectDirectory, "Program.cs");
            if (!File.Exists(programFile))
            {
                File.Create(programFile).Close();
            }
            File.WriteAllText(programFile, GererateProgramCsContent());

            var appsettingsFile = Path.Combine(projectDirectory, "appsettings.json");
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
    ""EndpointDefaults"": {
      ""Protocols"": ""Http2""
    }
  }
}
";
            File.WriteAllText(Path.Combine(projectDirectory, "appsettings.json"), appJson);
        }


        public void InitializeCinemaDesignDslSource()
        {
            //_theatreSystem.RenderCapture();
            //_theatreSystem.AllocateFundStageCinemaDesign();
            //if (_theatreSystem.GetAllCineDesigns().Length <= 0)
            //{
            //    _theatreSystem.CrateStageCinema("HomeCineDesign", AppSerivce.HomeId);
            //    _theatreSystem.CrateStageCinema("PreperExperimentCineDesign", AppSerivce.PreperExperimentId);
            //    _theatreSystem.CrateStageCinema("StartTaskCineDesign", AppSerivce.StartTaskId);
            //    _theatreSystem.CrateStageCinema("FinalizeCineDesign", AppSerivce.FinalizeId);
            //    _theatreSystem.CrateStageCinema("MaintenanceCineDesign", AppSerivce.MaintenanceId);
            //    _theatreSystem.CrateStageCinema("SystemStorageCineDesign", AppSerivce.SystemStorageId);

            //    _theatreSystem.CrateStageCinema("ExperimentCineDesign",Guid.NewGuid().ToString());
            //    _theatreSystem.CrateStageCinema("ExperimentCineDesign111",Guid.NewGuid().ToString());
            //    _theatreSystem.CrateStageCinema("ExperimentCineDesign222",Guid.NewGuid().ToString());
            //    _theatreSystem.CrateStageCinema("ExperimentCineDesign333",Guid.NewGuid().ToString());
            //    _theatreSystem.CrateStageCinema("ExperimentCineDesign444",Guid.NewGuid().ToString());
            //}
        }

        public void UnInitializeCinemaDesignDslSource()
        {
           // _theatreSystem.RefundTangerineStageCinemaDesign();
        }

       // public TangerineTheatreSystem TheatreSystem => _theatreSystem;

        private static string GenerateGrpcCsprojContent(string projectName)
        {

            StringBuilder sb = new();
            sb.AppendLine("<Project Sdk=\"Microsoft.NET.Sdk.Web\">");
            sb.AppendLine("  <PropertyGroup>");
            sb.AppendLine("   <TargetFramework>net8.0</TargetFramework>");
            sb.AppendLine("  <Nullable>enable</Nullable>");
            sb.AppendLine("    <ImplicitUsings>enable</ImplicitUsings>");
            sb.AppendLine("  </PropertyGroup>");
            sb.AppendLine();
            sb.AppendLine("  <ItemGroup>");
            sb.AppendLine("    <PackageReference Include=\"Grpc.AspNetCore\" Version=\"2.64.0\" />");
            sb.AppendLine("  </ItemGroup>\r\n");

            sb.AppendLine("  <ItemGroup>");
            sb.AppendLine("    <None Remove=\"Protos\\Smart_Lab_OS.proto\" />");
            sb.AppendLine("  </ItemGroup>");

            sb.AppendLine("  <ItemGroup>");
            sb.AppendLine("    <Protobuf Include=\"Protos\\Smart_Lab_OS.proto\" GrpcServices=\"Server\" />");
            sb.AppendLine("  </ItemGroup>");

            sb.AppendLine("  <ItemGroup>");
            sb.AppendLine("    <ProjectReference Include=\"..\\TangerineEngine\\TangerineEngine.csproj\" />");
            sb.AppendLine("  </ItemGroup>");

            sb.AppendLine("</Project>");
            return sb.ToString();
        }

        public string  GererateProgramCsContent()
        {
            StringBuilder sb = new();
            sb.AppendLine("using Smart.Lab.OS.Services;");
            sb.AppendLine("using TangerineSerivce;");
            sb.AppendLine("namespace Smart.Lab.OS");
            sb.AppendLine("{");
            sb.AppendLine("    public class Program");
            sb.AppendLine("{");
            sb.AppendLine("        public static void Main(string[] args)");
            sb.AppendLine("{");
            sb.AppendLine("var builder = WebApplication.CreateBuilder(args);");
            sb.AppendLine("     builder.Services.AddGrpc();");
            sb.AppendLine("            builder.Services.AddHostedService<AppSerivce>();");
            sb.AppendLine("     var app = builder.Build();");
            sb.AppendLine("     app.MapGrpcService<Smart_Lab_OSService>();");
            sb.AppendLine("   app.Run();");
            sb.AppendLine("}");
            sb.AppendLine("}");
            sb.AppendLine("}");
            return FormatCode(sb.ToString());
        }

        static string FormatCode(string code)
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
    }
}
