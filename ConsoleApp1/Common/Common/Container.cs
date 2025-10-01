using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace QStandaedPlatform.Engine.Common.Common
{
    public static class Container
    {
        private static IServiceProvider? _serviceProvider;
        public static void ConfigProvider(IServiceCollection serviceDescriptors)
        {
            _serviceProvider = serviceDescriptors.BuildServiceProvider();
        }
        public static void ConfigProvider(IServiceProvider serviceProvider)
        {
            _serviceProvider = serviceProvider;
        }

        public static void ConfigProvider(Func<IServiceCollection, IServiceProvider> func)
        {
            IServiceCollection serviceDescriptors = new ServiceCollection();
            _serviceProvider = func(serviceDescriptors);
        }

        public static object? GetService(Type type)
        {
            return _serviceProvider?.GetService(type);
        }

        public static T? GetService<T>()
        {
            if (_serviceProvider == null)
                return default;
            return _serviceProvider.GetService<T>();
        }

        public static T? GetService<T>(Type type)
        {
            if (_serviceProvider == null)
                return default;
            return (T)_serviceProvider.GetService(type);
        }

        public static IEnumerable<object?> GetServices(Type type)
        {
            if (_serviceProvider == null)
                return default;
            return _serviceProvider.GetServices(type);
        }
        public static IEnumerable<T?> GetServices<T>()
        {
            if (_serviceProvider == null)
                return default;
            return _serviceProvider.GetServices(typeof(T)).Select(p => (T)p);
        }

    }
}
