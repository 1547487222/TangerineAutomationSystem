using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace QStandaedPlatform.Engine.Common
{
    /// <summary>
    /// 序列化器
    /// </summary>
    public interface ISerializer<T>
    {
        string Serialize(T obj);
        T Deserialize(string data);
    }
}
