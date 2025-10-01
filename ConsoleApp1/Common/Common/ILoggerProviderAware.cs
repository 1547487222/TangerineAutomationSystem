using Microsoft.Extensions.Logging;
using NLog;
using NLog.Config;
using NLog.Extensions.Logging;
using NLog.LayoutRenderers;
using Serilog;
using Serilog.Enrichers.CallerInfo;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
namespace QStandaedPlatform.Engine.Common.Common
{
    public class LoggerProviderManager
    {

        private readonly static ConcurrentBag<ILoggerProviderAware> _loggerProviderAwares = [];

        static LoggerProviderManager()
        {
            DefaultLoggerProviderAware _loggerProviderAware = new();
            _loggerProviderAware.Configure();
            RegisterLoggerProviderAware(_loggerProviderAware);
        }
        public static void SetLoggerProviderAware(ILoggerProviderAware loggerProviderAware)
        {
            _loggerProviderAwares.Add(loggerProviderAware);
        }

        public static void RegisterLoggerProviderAware(ILoggerProviderAware loggerProviderAware)
        {
            ArgumentNullException.ThrowIfNull(loggerProviderAware);

            _loggerProviderAwares.Add(loggerProviderAware);
        }

        public static ILoggerProviderAware GetLoggerProviderAware()
        {
            return _loggerProviderAwares.Last();
        }

        public static ILoggerFactory GetLoggerFactory()
        {
            return GetLoggerProviderAware().LoggerFactory;
        }

        public static IEnumerable<ILoggerProviderAware> GetAllLoggerProviderAwares()
        {
            return _loggerProviderAwares.AsEnumerable();
        }
    }
    public interface ILoggerProviderAware
    {
         void Configure();
        /// <summary>
        /// Logger提供器
        /// </summary>
        ILoggerFactory LoggerFactory { get; }
    }

    public class DefaultLoggerProviderAware : ILoggerProviderAware
    {
        public void Configure()
        {
            // 初始化NLog配置
            var config = new LoggingConfiguration();

            // 添加控制台目标
            var consoleTarget = ConsoleTarget;
            config.AddTarget(consoleTarget);
            config.LoggingRules.Add(new NLog.Config.LoggingRule("*", NLog.LogLevel.Info, consoleTarget));

            // 添加文件目标
            var fileTarget = FileTarget;
            config.AddTarget(fileTarget);
            config.LoggingRules.Add(new NLog.Config.LoggingRule("*", NLog.LogLevel.Info, fileTarget));
            // 应用NLog配置
            LogManager.Configuration = config;
        }

        public NLog.Targets.ConsoleTarget ConsoleTarget { get; set; }= new NLog.Targets.ConsoleTarget("console")
        {
            Layout = "${longdate} ${level:uppercase=true} [${logger}] [${callsite}] ${message}"
        };
        public NLog.Targets.FileTarget FileTarget { get; set; } = new NLog.Targets.FileTarget("file")
        {
            FileName = "${basedir}/logs/${shortdate}.log",
            Layout = "${longdate} ${level:uppercase=true} [${logger}] [${callsite}] ${message}"
        };
        public ILoggerFactory LoggerFactory { get; } = Microsoft.Extensions.Logging.LoggerFactory.Create(builder => builder.AddNLog());
    }
}
