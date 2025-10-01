using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace QStandaedPlatform.Engine.Components.Sockets
{
    public class QProtocolHandler: IProtocolHandler
    {
        private const byte Pref_1 = 0xFA;
        private const byte Pref_2 = 0xCC;
        private static readonly byte[] SyncHead = new byte[] { Pref_1, Pref_2 };

        public bool TryFilter(ref Sequence<byte> buffer, out byte[] framedata)
        {
            framedata = null;

            // 查找同步头
            if (!TryFindSyncHead(ref buffer))
                return false;

            // 读取数据长度
            int dataLength = 0;
            if (!TryReadDataLength(ref buffer, out dataLength))
                return false;

            // 读取数据包
            return TryReadData(ref buffer, dataLength, out framedata);
        }

        private bool TryFindSyncHead(ref Sequence<byte> buffer)
        {
            int matchedBytes = 0;

            while (buffer.TryRead(out byte t))
            {
                if (SyncHead[matchedBytes] == t)
                {
                    matchedBytes++;
                    if (matchedBytes == SyncHead.Length)
                    {
                        return true;
                    }
                }
                else
                {
                    matchedBytes = 0;  // Reset if sync header mismatch
                }

                if (buffer.End())
                    break;
            }

            return false;
        }

        private bool TryReadDataLength(ref Sequence<byte> buffer, out int dataLength)
        {
            dataLength = 0;
            byte[] lengthBytes = new byte[4];

            // 读取4字节长度信息
            for (int i = 0; i < lengthBytes.Length; i++)
            {
                if (buffer.TryRead(out byte t))
                {
                    lengthBytes[i] = t;
                }
                else
                {
                    return false;  // 不足4字节，返回失败
                }
            }

            dataLength = BitConverter.ToInt32(lengthBytes, 0);
            return dataLength > 0;
        }

        private bool TryReadData(ref Sequence<byte> buffer, int dataLength, out byte[] framedata)
        {
            framedata = new byte[dataLength];
            int bytesRead = 0;

            // 读取数据
            while (bytesRead < dataLength && buffer.TryRead(out byte t))
            {
                framedata[bytesRead++] = t;
            }

            return bytesRead == dataLength;
        }
    }
}
