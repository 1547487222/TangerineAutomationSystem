using QStandaedPlatform.Engine.Devices;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace QStandaedPlatform.Engine.Modbus
{
    /// <summary>
    /// Modbus基类接口
    /// </summary>
    public interface IModbus
    {
        /// <summary>
        /// 站号
        /// </summary>
        byte Station { get; set; }
        /// <summary>
        /// 连接
        /// </summary>
        void Connect();

        /// <summary>
        /// 断开连接
        /// </summary>
        void Disconnect();

        /// <summary>
        /// 读取线圈
        /// </summary>
        /// <returns>bool[]</returns>
        Result<bool[]> ReadCoils(string address, ushort length);

        /// <summary>
        /// 读取离散输入
        /// </summary>
        /// <param name="address"></param>
        /// <param name="length"></param>
        /// <returns></returns>
        Result<bool[]> ReadDiscreteInput(string address, ushort length);

        /// <summary>
        /// 读取保持寄存器
        /// </summary>
        /// <param name="address"></param>
        /// <returns></returns>
        Result<short[]> ReadHoldingRegister(string address, ushort length);
        /// <summary>
        /// 读取输入寄存器
        /// </summary>
        /// <param name="address"></param>
        /// <returns></returns>
        Result<short[]> ReadInputRegister(string address, ushort length);

        /// <summary>
        /// 写入单个线圈
        /// </summary>
        /// <param name="address"></param>
        /// <param name="coils"></param>
        /// <returns></returns>
        Result WriteSingleCoil(string address, bool coils);
        /// <summary>
        /// 写入多个线圈
        /// </summary>
        /// <param name="address"></param>
        /// <param name="coils"></param>
        /// <returns></returns>
        Result WriteMultiCoils(string address, bool[] coils);
        /// <summary>
        /// 写单个寄存器
        /// </summary>
        /// <param name="address"></param>
        /// <returns></returns>
        Result WriteSingleRegister(string address, short register);
        /// <summary>
        /// 写多个寄存器
        /// </summary>
        /// <param name="address"></param>
        /// <param name="register"></param>
        /// <returns></returns>
        Result WriteMultiRegister(string address, short[] register);
    }
}
