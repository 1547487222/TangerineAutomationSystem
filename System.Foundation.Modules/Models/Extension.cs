using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using QStandaedPlatform.Engine.Common.Common;
using System;
using System.Collections.Generic;
using System.Foundation.Modules.Arms;
using System.Foundation.Modules.Arms.Pipettes;
using System.Foundation.Modules.NormalModules;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace System.Foundation.Modules.Models
{
    public static class Extension
    {
        private  static ILogger _logger;
        static Extension()
        {

        }
        public static IServiceCollection AddFoundationModules(this IServiceCollection services)
        {
            var toolBaseType = typeof(Tool);
            var assembly = Assembly.GetExecutingAssembly();
            var toolTypes = assembly.GetTypes()
                .Where(t => toolBaseType.IsAssignableFrom(t) && !t.IsAbstract)
                .ToList();

            foreach (var type in toolTypes)
            {
                services.AddTransient(type);
            }

            services.AddTransient<ModuleFunCodecService>();
            services.AddTransient<ArmService>();
            services.AddTransient<PipetteService>();

            return services;
        }


    }
}
