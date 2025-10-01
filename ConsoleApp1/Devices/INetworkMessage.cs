using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace QStandaedPlatform.Engine.Devices
{
    public interface INetMessage
    {
        /// <summary>
        /// 消息头的指令长度
        /// </summary>
        int ProtocolHeadBytesLength { get; }

        /// <summary>
        /// 从当前的头子节文件中提取出接下来需要接收的数据长度
        /// </summary>
        /// <returns>返回接下来的数据内容长度</returns>
        int GetContentLengthByHeadBytes();


        /// <summary>
        /// 消息头字节
        /// </summary>
        byte[] HeadBytes { get; set; }

        /// <summary>
        /// 消息内容字节
        /// </summary>
        byte[] ContentBytes { get; set; }

        /// <summary>
        /// 发送的字节信息
        /// </summary>
        byte[] SendBytes { get; set; }
    }
}
