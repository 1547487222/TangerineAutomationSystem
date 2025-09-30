using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using Tangerine.Framework;
using Tangerine.H5uModule;

namespace TangerineAutomationSystem.Services
{
    /// <summary>
    /// Lightweight host to wire up Tangerine.Framework ModuleProviderManager and ModuleInstanceManager
    /// Use this to access available providers and create/manage instances.
    /// </summary>
    public sealed class TangerineHost
    {
        private static readonly Lazy<TangerineHost> _instance = new(() => new TangerineHost());
        public static TangerineHost Instance => _instance.Value;

        public IServiceProvider ServiceProvider { get; }
        public IModuleProviderManager ProviderManager { get; }
        public IModuleInstanceManager InstanceManager { get; }
        public ILoggerFactory LoggerFactory { get; }

        private TangerineHost()
        {
            var services = new ServiceCollection();
            services.AddLogging(builder =>
            {
                builder.AddDebug();
#if DEBUG
                builder.SetMinimumLevel(LogLevel.Debug);
#else
                builder.SetMinimumLevel(LogLevel.Information);
#endif
            });

            ServiceProvider = services.BuildServiceProvider();
            // LoggerFactory = ServiceProvider.GetService<ILoggerFactory>() ?? LoggerFactory.Create(builder => builder.AddDebug());
            // Create provider manager from Tangerine.Framework
            ProviderManager = new Tangerine.Framework.ModuleProviderManager(ServiceProvider);

            // Register built-in providers — you can add your own here
            try
            {
                // H5u provider (from Tangerine.H5uModule)
                var h5u = new H5uModuleProvider();
                // ModuleProviderManager.RegisterProvider<TConfig>(...) expects IModuleProvider<TConfig>.
                // ModuleProviderManager implementation accepts provider via RegisterProvider generic method.
                ProviderManager.RegisterProvider(h5u);
            }
            catch
            {
                // swallow, in case the provider type is missing — user can register later
            }

            var logger = LoggerFactory?.CreateLogger<ModuleInstanceManager>();
            InstanceManager = new ModuleInstanceManager(ProviderManager, logger);
        }

        /// <summary>
        /// Helper to get provider info list for binding
        /// </summary>
        public IReadOnlyList<IModuleProviderInfo> GetAvailableProviders() => ProviderManager.AvailableProviders;
    }
}