using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace QStandaedPlatform.Engine.Common.Common
{
    public static class EnumValuesProvider
    {
        public static T[] GetEnumAll<T>() where T : struct,Enum
        {
            return [.. Enum.GetValues(typeof(T)).Cast<T>()];
        }

        public static string[] GetEnumAllDescription<T>() where T : struct
        {
            return [.. typeof(T)
                .GetFields(BindingFlags.Public | BindingFlags.Static)
                .Select(field =>
                {
                    var attribute = field.GetCustomAttribute<DescriptionAttribute>();
                    return attribute?.Description ?? field.Name;
                })];
        }

        public static Dictionary<string, T> GetEnumDescriptionToMap<T>() where T : struct
        {
            return typeof(T)
                .GetFields(BindingFlags.Public | BindingFlags.Static)
                .ToDictionary(
                    field =>
                    {
                        var attribute = field.GetCustomAttribute<DescriptionAttribute>();
                        return attribute?.Description ?? field.Name;
                    },
                    field =>Enum.Parse<T>(field.Name)
                );
        }

        public static List<KeyValuePair<string, T>> GetEnumDescriptionToList<T>() where T : struct
        {
            return [.. GetEnumDescriptionToMap<T>()];
        }
    }
}
