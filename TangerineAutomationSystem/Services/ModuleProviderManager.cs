using System.Collections.Generic;
using System.Linq;

namespace TangerineAutomationSystem.Services
{
    // 简单管理器，用于注册和获取提供器（可以扩展为反射发现插件）
    public static class ModuleProviderManager
    {
        private static readonly List<IModuleProvider> _providers = new();

        public static void RegisterProvider(IModuleProvider provider)
        {
            if (!_providers.Any(p => p.Name == provider.Name))
                _providers.Add(provider);
        }

        public static IEnumerable<IModuleProvider> GetProviders() => _providers;

        public static IModuleProvider? GetProviderByName(string name) => _providers.FirstOrDefault(p => p.Name == name);
    }
}