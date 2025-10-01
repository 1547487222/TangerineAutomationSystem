using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace QStandaedPlatform.Engine.Laboratory
{
    /// <summary>
    /// 错误码
    /// </summary>
    public enum SmartLabOsErrorCode
    {
        /// <summary>
        /// 成功
        /// </summary>
        Success = 0,
        /// <summary>
        ///未知失败
        /// </summary>
        UnknownFailed = 1,
        /// <summary>
        ///平台初始化失败
        /// </summary>
         PlatformInitFailed = 1000,
        /// <summary>
        ///平台实验前准备失败
        /// </summary>
        PlatformPrepareFailed = 1001,
        /// <summary>
        ///平台复位失败
        /// </summary>
         PlatformResetFailed = 1002,
        /// <summary>
        ///平台停止失败
        /// </summary>
         PlatformStopFailed = 1003,
        /// <summary>
        ///平台暂停失败
        /// </summary>
         PlatformPauseFailed = 1004,
        /// <summary>
        ///平台获取样品EBR数据失败
        /// </summary>
         PlatformGetEbrFailed = 1005,
        /// <summary>
        ///平台获取使用信息失败
        /// </summary>
         PlatformGetUsageFailed = 1006,
        
         /// <summary>
         /// 监控异常
         /// </summary>
        MonitorException = 1007,
    }
}
