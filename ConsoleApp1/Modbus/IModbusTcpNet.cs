using QStandaedPlatform.Engine.Devices;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace QStandaedPlatform.Engine.Modbus
{
    public interface IModbusTcpNet : IModbus
    {
        /// <summary>
        /// 主站IP地址
        /// </summary>
        string IpAddress { get; set; }
        /// <summary>
        /// 主站端口号
        /// </summary>
        int Port { get; set; }

        /// <summary>
        /// 读取单个线圈
        /// </summary>
        /// <param name="address"></param>
        /// <returns></returns>
        Result<bool> ReadSingleCoil(string address);

        /// <summary>
        /// 读取单个离散输入
        /// </summary>
        /// <param name="address"></param>
        /// <returns></returns>
        Result<bool> ReadSingleDiscreteInput(string address);



    }
}
