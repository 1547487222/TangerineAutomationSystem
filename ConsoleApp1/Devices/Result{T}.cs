using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace QStandaedPlatform.Engine.Devices
{
    public interface IResult<T> : IResult
    {
        /// <summary>
        /// 用户自定义的泛型数据
        /// </summary>
        T Content { get; }


    }
    public interface IResult
    {
        /// <summary>
        /// 指示本次访问是否成功
        /// </summary>
        bool IsSuccess { get; }

        /// <summary>
        /// 具体的错误描述
        /// </summary>
        string Message { get; }

        /// <summary>
        /// 具体的错误代码
        /// </summary>
        int ErrorCode { get; }
    }
}
