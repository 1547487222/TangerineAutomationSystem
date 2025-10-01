using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace QStandaedPlatform.Engine.Common.Common
{
    public class ToolState<T>(Tool ownerTool, T state, string desc = "")
    {
        public Tool OwnerTool { get; } = ownerTool;

        public T State { get; } = state;

        public string? Description { get; } = desc;
    }


    public enum ToolState
    {
        /// <summary>
        /// 未运行
        /// </summary>
        None,
        /// <summary>
        /// 同步等待中
        /// </summary>
        SyncWaiting,
        /// <summary>
        /// 预警
        /// </summary>
        Forewarn,
        /// <summary>
        /// 错误
        /// </summary>
        Error,
        /// <summary>
        /// 运行中
        /// </summary>
        Running,
        /// <summary>
        /// 完成
        /// </summary>
        Finish,
        /// <summary>
        /// 禁止
        /// </summary>
        Forbidden,
    }
}
