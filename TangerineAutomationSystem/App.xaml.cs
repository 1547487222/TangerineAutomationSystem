using System.Windows;
using Microsoft.Extensions.DependencyInjection;
using TangerineAutomationSystem.Services;
using TangerineAutomationSystem.ViewModels;
using TangerineAutomationSystem.Views;
using MaterialDesignColors;
using MaterialDesignThemes.Wpf;
using Microsoft.Win32;
using System.IO;
using System.Runtime.InteropServices;
using System.Windows.Media;

namespace TangerineAutomationSystem
{
    public partial class App : Application
    {
        [DllImport("shell32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        private static extern void SHChangeNotify(uint wEventId, uint uFlags, IntPtr dwItem1, IntPtr dwItem2);

        protected override void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);

            PaletteHelper paletteHelper = new();
            Theme theme = paletteHelper.GetTheme();
            theme.SetBaseTheme(BaseTheme.Light); // Light主题，工业风偏冷色

            // 工业金属灰 + 蓝色点缀
            theme.SetPrimaryColor((Color)ColorConverter.ConvertFromString("#607D8B")); // 冷灰蓝（主色）
            theme.SetSecondaryColor((Color)ColorConverter.ConvertFromString("#455A64")); // 深冷灰蓝（辅助色）

            // 背景、主色、辅助色
            theme.Background = (Color)ColorConverter.ConvertFromString("#ECEFF1"); // 淡灰白背景（类似金属磨砂感）
            theme.PrimaryMid = (Color)ColorConverter.ConvertFromString("#546E7A"); // 中灰蓝
            theme.PrimaryDark = (Color)ColorConverter.ConvertFromString("#37474F"); // 深石墨蓝
            theme.SecondaryMid = (Color)ColorConverter.ConvertFromString("#78909C"); // 辅助色中间调
            theme.SecondaryDark = (Color)ColorConverter.ConvertFromString("#546E7A"); // 辅助色深调

            paletteHelper.SetTheme(theme);

            const uint SHCNE_ASSOCCHANGED = 0x08000000; 
            const uint SHCNF_IDLIST = 0x0000;
            string extension = ".flow";
            string fileType = "TangerineAutomation.flow";
            string iconPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Images", "tangerine.ico");

            using (RegistryKey key = Registry.CurrentUser.CreateSubKey(@"Software\Classes\" + extension))
            {
                key.SetValue("", fileType);
            }

            using (RegistryKey key = Registry.CurrentUser.CreateSubKey(@"Software\Classes\" + fileType))
            {
                key.SetValue("", "TangerineAutomationFlow File");

                using RegistryKey iconKey = key.CreateSubKey("DefaultIcon");
                iconKey.SetValue("", iconPath);
            }
            SHChangeNotify(SHCNE_ASSOCCHANGED, SHCNF_IDLIST, IntPtr.Zero, IntPtr.Zero);
        }

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