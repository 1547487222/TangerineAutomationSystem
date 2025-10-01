using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace QStandaedPlatform.Engine.Common.Common
{
    public class ToolDescriptions
    {
        private static readonly Dictionary<string, Type> _toolRunTimeDesc = [];

        private static Assembly[] AppAssemblies => AppDomain.CurrentDomain.GetAssemblies();

        private static Func<Type, bool> FindToolRule => (p) => typeof(Tool).IsAssignableFrom(p) && p.IsClass && !p.IsAbstract;

        public static Func<Type, Tool?> StructureTool => (toolType) =>
        {
            var tool = Container.GetService<Tool>(toolType);
            if (tool == null)
            {
                ConstructorInfo? constructor = toolType
                .GetConstructors()
                .FirstOrDefault(p => p.GetParameters().Length == 0);
                if (constructor != null)
                {
                    tool = (Tool)constructor.Invoke(null);
                }
            }
            return tool;
        };
        public static void RegisterTool(string toolName, Type toolType)
        {
            _toolRunTimeDesc.Add(toolName, toolType);
        }

        public static Type GetToolType(string toolName)
        {
            return _toolRunTimeDesc[toolName];
        }

        public static void UnRegisterTool(string toolName)
        {
            _toolRunTimeDesc.Remove(toolName);
        }

        public static void InitToolRunTimeDesc()
        {
            var toolTypes = AppAssemblies.SelectMany(p => p.DefinedTypes).Where(FindToolRule).ToArray();
            foreach (var toolType in toolTypes)
            {
                var displayNameAttribute = toolType.GetCustomAttribute<DisplayNameAttribute>();
                if (displayNameAttribute != null)
                {
                    _toolRunTimeDesc.Add(displayNameAttribute.DisplayName, toolType);
                }
                else
                {
                    _toolRunTimeDesc.Add(toolType.Name, toolType);
                }
            }
        }

        public static (string toolName, string desc)[] GetToolDescriptions()
        {
            return _toolRunTimeDesc.Select(p => (p.Key, p.Value.GetCustomAttribute<DescriptionAttribute>()?.Description ?? string.Empty)).ToArray();
        }

    }
}
