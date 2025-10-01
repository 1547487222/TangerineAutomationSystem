using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace QStandaedPlatform.Engine.Extensions
{
    public static class TypeExtensions
    {

        public static PropertyInfo AddProperty(this Type type, string propertyName, Type propertyType)
        {
            try
            {
                var dynamicType = type.GetType();
                var property = dynamicType.GetProperty(propertyName, BindingFlags.Public | BindingFlags.Instance);
                var proterty2 = dynamicType.GetTypeInfo().DeclaredProperties.FirstOrDefault(p => p.Name == propertyName);

                if (property == null && proterty2 == null)
                {
                    property = dynamicType.GetTypeInfo().AddProperty(propertyName, propertyType);
                }
                return property;
            }
            catch (Exception ex)
            {

                throw;
            }
   
        }
    }
}
