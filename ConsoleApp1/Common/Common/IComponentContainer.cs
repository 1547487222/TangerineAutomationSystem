using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace QStandaedPlatform.Engine.Common.Common
{
    /// <summary>
    /// 组件容器接口
    /// </summary>
    public interface IComponentContainer
    {
        object? GetComponent(Type componentType);
        T GetComponent<T>();
        T GetComponent<T>(string componentName);
    }
}
