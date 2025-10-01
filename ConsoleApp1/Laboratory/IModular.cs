using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace QStandaedPlatform.Engine.Laboratory
{
    public interface IModular<TMessenger>: IModular
    {
        TMessenger Messenger { get; }
    }

    public interface IModular
    {
        /// <summary>
        /// 手动和自动
        /// </summary>
        /// <param name="enable"></param>
        /// <returns></returns>
        Task<bool> ManualAutoAsync(bool enable);

        /// <summary>
        /// 启动
        /// </summary>
        /// <returns></returns>
        Task<bool> StartAsync();

        /// <summary>
        /// 回原
        /// </summary>
        /// <returns></returns>
        Task<bool> HomeAsync();

        /// <summary>
        /// 急停
        /// </summary>
        /// <returns></returns>
        Task<bool> EmergencyStopAsync();

        /// <summary>
        /// 复位急停
        /// </summary>
        /// <returns></returns>
        Task<bool> ResetEmergencyStopAsync();

        /// <summary>
        /// 暂停
        /// </summary>
        /// <returns></returns>
        Task<bool> PauseAsync();

        /// <summary>
        /// 自动和启动
        /// </summary>
        /// <returns></returns>
        Task<bool> AutoAndStartAsync();

        /// <summary>
        /// 获取模块状态
        /// </summary>
        /// <returns></returns>
        Task<ModuleStatus> GetModuleStatusAsync();

        /// <summary>
        /// 获取模块状态
        /// </summary>
        /// <returns></returns>
        ModuleStatus GetModuleStatus();
    }
}
