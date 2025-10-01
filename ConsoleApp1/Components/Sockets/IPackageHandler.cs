using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace QStandaedPlatform.Engine.Components.Sockets
{
    /// <summary>
    /// 包信息处理接口
    /// </summary>
    /// <typeparam name="TPackageInfo"></typeparam>
    public interface IPackageHandler<TPackageInfo>
    {
        /// <summary>
        /// 编码
        /// </summary>
        /// <param name="packageInfo"></param>
        /// <returns></returns>
        byte[] Encode(TPackageInfo packageInfo);
        /// <summary>
        /// 解码
        /// </summary>
        /// <param name="data"></param>
        /// <returns></returns>
        TPackageInfo Decode(byte[] data);
    }
}
