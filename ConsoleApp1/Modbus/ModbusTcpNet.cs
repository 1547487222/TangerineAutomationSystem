using QStandaedPlatform.Engine.Devices;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace QStandaedPlatform.Engine.Modbus
{
    /// <summary>
    /// ModbusTcp
    /// </summary>
    public class ModbusTcpNet : NetworkDeviceBase, IModbusTcpNet
    {
        private readonly IncrementCount<ushort> _messageId;
        private readonly object _lock = new();
        public ModbusTcpNet(string ipAddress, byte station = 0x01, int port = 502)
        {
            _messageId = new IncrementCount<ushort>(0);
            IpAddress = ipAddress;
            Station = station;
            Port = port;
        }

        /// <summary>
        /// 站号
        /// </summary>
        public byte Station { get; set; }

        public IncrementCount<ushort> MessageId => _messageId;

        public void Connect()
        {
            lock (_lock)
            {
                ConnectServer();
            }
        }

        public void Disconnect()
        {
            lock (_lock)
            {
                if (Connected)
                    ConnectClose();
            }
        }

        public Result<bool[]> ReadCoils(string address, ushort length)
        {
            lock (_lock)
            {
                var buffer = ModbusTcpCommon.BuildReadCoilsCommand(MessageId.GetNextValue(),
               Station, address, length);
                if (buffer.Length == 0)
                {
                    return Result<bool[]>.CreateErrorResult("BuildReadCoilsCommand Error.");
                }
                var data = ReadFromServer(buffer);
                return ModbusTcpCommon.CheckReadCoilsResponse(data, length);
            }
        }

        public Result<bool[]> ReadDiscreteInput(string address, ushort length)
        {
            lock (_lock)
            {
                var buffer = ModbusTcpCommon.BuildReadDiscreteInputCommand(MessageId.GetNextValue(),
                Station, address, length);
                if (buffer.Length == 0)
                {
                    return Result<bool[]>.CreateErrorResult("BuildReadCoilsCommand Error.");
                }
                var data = ReadFromServer(buffer);
                return ModbusTcpCommon.CheckReadDiscreteInputResponse(data, length);
            }
        }
        public Result<bool> ReadSingleDiscreteInput(string address)
        {
            var result = ReadDiscreteInput(address, 1);
            if (result.IsSuccess)
                return new Result<bool>
                {
                    IsSuccess = result.IsSuccess,
                    Content = result.Content[0],
                };
            else
                return new Result<bool>
                {
                    IsSuccess = result.IsSuccess,
                    ErrorCode = result.ErrorCode,
                    Message = result.Message,
                };
        }

        public Result<short[]> ReadHoldingRegister(string address, ushort length)
        {
            lock (_lock)
            {
                var buffer = ModbusTcpCommon.BuildReadHoldingRegisterCommand(MessageId.GetNextValue(),
               Station, address, length);
                if (buffer.Length == 0)
                {
                    return Result<short[]>.CreateErrorResult("BuildReadHoldingRegisterCommand Error.");
                }
                var data = ReadFromServer(buffer);
                return ModbusTcpCommon.CheckReadRegisterResponse(data);
            }
        }

        public Result<short[]> ReadInputRegister(string address, ushort length)
        {
            lock (_lock)
            {
                var buffer = ModbusTcpCommon.BuildReadInputRegisterCommand(MessageId.GetNextValue(),
               Station, address, length);
                if (buffer.Length == 0)
                {
                    return Result<short[]>.CreateErrorResult("BuildReadHoldingRegisterCommand Error.");
                }
                var data = ReadFromServer(buffer);
                return ModbusTcpCommon.CheckReadRegisterResponse(data);
            }
        }

        public Result WriteMultiCoils(string address, bool[] coils)
        {
            lock (_lock)
            {
                var buffer = ModbusTcpCommon.BuildWriteMultiCoilsCommand(
                MessageId.GetNextValue(), Station, address, coils);
                if (buffer.Length == 0)
                {
                    return Result.CreateErrorResult("BuildWriteMultiCoilsCommand Error.");
                }
                var data = ReadFromServer(buffer);
                return ModbusTcpCommon.CheckWriteMultiCoilsResponse(buffer, data);
            }
        }

        public Result WriteMultiRegister(string address, short[] register)
        {
            lock (_lock)
            {
                var buffer = ModbusTcpCommon.BuildWriteMultiRegisterCommand(
                MessageId.GetNextValue(), Station, address, register);
                if (buffer.Length == 0)
                {
                    return Result.CreateErrorResult("BuildWriteMultiCoilsCommand Error.");
                }
                var data = ReadFromServer(buffer);
                return ModbusTcpCommon.CheckWriteMultiRegisterResponse(buffer, data);
            }
        }

        public Result WriteSingleCoil(string address, bool coils)
        {
            lock (_lock)
            {
                var buffer = ModbusTcpCommon.BuildWriteSingleCoilsCommand(
                MessageId.GetNextValue(), Station, address, coils);
                if (buffer.Length == 0)
                {
                    return Result.CreateErrorResult("BuildWriteSingleCoilsCommand Error.");
                }
                var data = ReadFromServer(buffer);
                return ModbusTcpCommon.CheckWriteCoilsResponse(buffer, data);
            }
        }

        public Result WriteSingleRegister(string address, short register)
        {
            lock (_lock)
            {
                var buffer = ModbusTcpCommon.BuildWriteSingleRegisterCommand(
                MessageId.GetNextValue(), Station, address, register);
                if (buffer.Length == 0)
                {
                    return Result.CreateErrorResult("BuildWriteSingleCoilsCommand Error.");
                }
                var data = ReadFromServer(buffer);
                return ModbusTcpCommon.CheckWriteSingleRegisterResponse(buffer, data);
            }
        }

        public Result<bool> ReadSingleCoil(string address)
        {
            var result = ReadCoils(address, 1);
            if (result.IsSuccess)
                return new Result<bool>
                {
                    IsSuccess = result.IsSuccess,
                    Content = result.Content[0],
                };
            else
                return new Result<bool>
                {
                    IsSuccess = result.IsSuccess,
                    ErrorCode = result.ErrorCode,
                    Message = result.Message,
                };
        }

        public Result<bool[]> ReadMultiCoils(string address, ushort length)
        {
            return ReadCoils(address, length);
        }
    }
}
