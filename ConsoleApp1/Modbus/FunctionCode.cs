using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace QStandaedPlatform.Engine.Modbus
{
    /// <summary>
    /// Modbus 功能码
    /// </summary>
    public enum FunctionCode : byte
    {
        /// <summary>
        /// 读线圈
        /// </summary>
        ReadCoil = 0x01,
        /// <summary>
        /// 读取离散输入
        /// </summary>
        ReadDiscreteInput = 0x02,
        /// <summary>
        /// 读取保持寄存器
        /// </summary>
        ReadHoldingRegister = 0x03,
        /// <summary>
        /// 读取输入寄存器
        /// </summary>
        ReadInputRegister = 0x04,
        /// <summary>
        /// 写入单个线圈
        /// </summary>
        WriteSingleCoil = 0x05,
        /// <summary>
        /// 写单个保持寄存器
        /// </summary>
        WriteSingleRegister = 0x06,
        /// <summary>
        /// 写多个线圈
        /// </summary>
        WriteMultiCoil = 0x0f,
        /// <summary>
        /// 写多个保持寄存器
        /// </summary>
        WriteMultiRegister = 0x10,
    }
}
