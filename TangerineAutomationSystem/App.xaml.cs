using System.Windows;
using Microsoft.Extensions.DependencyInjection;
using TangerineAutomationSystem.Services;
using TangerineAutomationSystem.ViewModels;
using TangerineAutomationSystem.Views;

namespace TangerineAutomationSystem
{
    public partial class App : Application
    {
        //public static IServiceProvider ServiceProvider { get; private set; }

        //protected override void OnStartup(StartupEventArgs e)
        //{
        //    base.OnStartup(e);

        //    // 配置依赖注入
        //    var services = new ServiceCollection();
        //    ConfigureServices(services);
        //    ServiceProvider = services.BuildServiceProvider();

        //    // 显示主窗口
        //    var mainWindow = ServiceProvider.GetService<MainWindow>();
        //    mainWindow.Show();
        //}

        //private void ConfigureServices(IServiceCollection services)
        //{
        //    // 注册服务
        //    services.AddSingleton<IProjectService, ProjectService>();

        //    // 注册ViewModels
        //    services.AddTransient<MainWindowViewModel>();
        //    services.AddTransient<ProjectExplorerViewModel>();

        //    // 注册Views
        //    services.AddTransient<MainWindow>();
        //    services.AddTransient<ProjectExplorerView>();
        //}

        //private void Application_Startup(object sender, StartupEventArgs e)
        //{
        //    // 启动逻辑
        //}
    }
}