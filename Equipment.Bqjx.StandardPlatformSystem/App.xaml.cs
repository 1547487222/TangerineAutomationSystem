
using MaterialDesignColors;
using MaterialDesignThemes.Wpf;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using CommunityToolkit.Mvvm.DependencyInjection;
using Microsoft.Win32;
using QStandaedPlatform.Engine.Common.Common;
using QStandaedPlatform.Engine.Components;
using System.Configuration;
using System.Data;
using System.Foundation.Modules.Arms;
using System.Foundation.Modules.Models;
using System.Foundation.Modules.ModuleStations;
using System.Foundation.Modules.Triggers;
using QStandaedPlatform.Engine.Extensions;
using System.Foundation.Modules.TubeRacks;
using System.IO;
using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Media;
using System.Windows.Forms.Design;
using System.Foundation.Modules.NormalModules;
using QStandaedPlatform.Engine.Common.Common.DbContexts;

namespace Equipment.Bqjx.StandardPlatformSystem
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {

        [DllImport("shell32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        private static extern void SHChangeNotify(uint wEventId, uint uFlags, IntPtr dwItem1, IntPtr dwItem2);
        private  ILogger? _logger;

        public static IWorkFlowEngine ToolEngine => WorkFlowEngine.Instance;
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

            IServiceCollection descriptors = new ServiceCollection();
            descriptors.AddLogging();
            descriptors.AddMangoStorage();
            descriptors.AddFoundationModules();
            Container.ConfigProvider(descriptors);
            ToolEngine.Initialize();
            _logger = LoggerProviderManager.GetLoggerFactory().CreateLogger(typeof(App));
            this.DispatcherUnhandledException += App_DispatcherUnhandledException;
            TaskScheduler.UnobservedTaskException += TaskScheduler_UnobservedTaskException;
            AppDomain.CurrentDomain.UnhandledException += CurrentDomain_UnhandledException;

        }

        private void CurrentDomain_UnhandledException(object sender, UnhandledExceptionEventArgs e)
        {
            _logger?.LogWarning($"{e.ExceptionObject}");

            MessageBox.Show(e.ExceptionObject.ToString());
        }

        private void TaskScheduler_UnobservedTaskException(object? sender, UnobservedTaskExceptionEventArgs e)
        {
            _logger?.LogWarning($"{e.Exception}");
            MessageBox.Show(e.Exception.ToString());
        }

        private void App_DispatcherUnhandledException(object sender, System.Windows.Threading.DispatcherUnhandledExceptionEventArgs e)
        {
            _logger?.LogWarning($"{e.Exception}");
            MessageBox.Show(e.Exception.ToString());
        }

        protected override void OnExit(ExitEventArgs e)
        {
            ToolEngine.ShutDown();
            base.OnExit(e);
        }

    }

}
