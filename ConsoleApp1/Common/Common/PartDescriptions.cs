using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace QStandaedPlatform.Engine.Common.Common
{
    public class PartDescriptions
    {
        private static readonly Dictionary<string, Type> _partRunTimeDesc = [];

        private static Assembly[] AppAssemblies => AppDomain.CurrentDomain.GetAssemblies();

        private static Func<Type, bool> FindPartRule => (p) => typeof(IPart).IsAssignableFrom(p) && p.IsClass && !p.IsAbstract;

        public static void InitPartRunTimeDesc()
        {
            var partTypes = AppAssemblies.SelectMany(p => p.DefinedTypes).Where(FindPartRule).ToArray();
            foreach (var partType in partTypes)
            {
                var displayNameAttribute = partType.GetCustomAttribute<DisplayNameAttribute>();

                if (displayNameAttribute != null)
                {
                    if (!_partRunTimeDesc.ContainsKey(displayNameAttribute.DisplayName))
                        _partRunTimeDesc[displayNameAttribute.DisplayName] = partType;
                }
            }
        }

        public static (string partName, string desc)[] GetPartDescriptions()
        {
            return _partRunTimeDesc.Select(p => (p.Key, p.Value.GetCustomAttribute<DescriptionAttribute>()?.Description ?? string.Empty)).ToArray();
        }

        public static PartBackup? GetPartBackup(string partName)
        {
            if (_partRunTimeDesc.TryGetValue(partName, out var partType))
            {
                PartBackup partBackup = new()
                {
                    PartId = Guid.NewGuid(),
                    PartName = partType.GetCustomAttribute<DisplayNameAttribute>()?.DisplayName ?? partType.Name,
                    PartType = partType,
                    PartOption = new PartOptionImporter().Import(partType)
                };
                partBackup.PartOptionType = partBackup.PartOption?.GetType();
                partBackup.Description = partType.GetCustomAttribute<DescriptionAttribute>()?.Description ?? string.Empty;
                return partBackup;
            }
            return default;
        }
    }
}
