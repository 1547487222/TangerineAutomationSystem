using QStandaedPlatform.Engine.Devices;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace QStandaedPlatform.Engine.Modbus
{
    public class ModbusTcpCommon
    {
        public const int Bit = 1;
        public const int Word = 2;

        #region ErrCode Declaration

        /// <summary>
        /// 不支持该功能码
        /// </summary>
        public const byte FunctionCodeNotSupport = 0x01;
        /// <summary>
        /// 该地址越界
        /// </summary>
        public const byte FunctionCodeOverBound = 0x02;
        /// <summary>
        /// 读取长度超过最大值
        /// </summary>
        public const byte FunctionCodeQuantityOver = 0x03;
        /// <summary>
        /// 读写异常
        /// </summary>
        public const byte FunctionCodeReadWriteException = 0x04;
        /// <summary>
        /// 字节序转换器
        /// </summary>
        public Func<byte[], byte[]>? EndianConverter { get; set; }




        #endregion
        public static byte[] BuildReadCoilsCommand(ushort tranId, byte station, string address, ushort length)
        {
            if (!TryPraseAddress(address, Bit, out var elementAddress))
                return [];

            byte[] bytes = new byte[12];
            var tranidBytes = BitConverter.GetBytes(tranId);
            if (BitConverter.IsLittleEndian)
                tranidBytes = tranidBytes.Reverse().ToArray();

            bytes[0] = tranidBytes[0];
            bytes[1] = tranidBytes[1];
            bytes[2] = 0x00;
            bytes[3] = 0x00;
            bytes[4] = 0x00;
            bytes[5] = 0x06;
            bytes[6] = station;

            bytes[7] = 0x01;

            var addressBytes = BitConverter.GetBytes((ushort)elementAddress);
            if (BitConverter.IsLittleEndian)
                addressBytes = addressBytes.Reverse().ToArray();

            bytes[8] = addressBytes[0];
            bytes[9] = addressBytes[1];

            var lengthBytes = BitConverter.GetBytes(length);
            if (BitConverter.IsLittleEndian)
                lengthBytes = lengthBytes.Reverse().ToArray();

            bytes[10] = lengthBytes[0];
            bytes[11] = lengthBytes[1];

            return bytes;
        }
        public static Result<bool[]> CheckReadCoilsResponse(byte[] actualResponse, ushort requestedQuantity)
        {
            var result = new Result<bool[]>()
            {
                IsSuccess = true
            };
            var errorMessage = VerifyErrorCode(actualResponse[7], actualResponse[8]);
            if (!string.IsNullOrEmpty(errorMessage))
            {
                result.IsSuccess = false;
                result.Message = errorMessage;
                result.ErrorCode = actualResponse[8];
                return result;
            }
            int byteCount = actualResponse[8];
            if (actualResponse.Length < 9 + byteCount)
            {
                throw new Exception("CheckReadCoilsResponse does not contain enough data bytes.");
            }
            byte[] coilsData = new byte[byteCount];
            Array.Copy(actualResponse, 9, coilsData, 0, byteCount);

            List<bool> coils = [];
            for (int i = 0; i < coilsData.Length && coils.Count < requestedQuantity; i++)
            {
                for (int j = 0; j < 8 && coils.Count < requestedQuantity; j++)
                {
                    coils.Add((coilsData[i] & 1 << j) != 0);
                }
            }
            result.Content = [.. coils];
            return result;
        }
        public static byte[] BuildReadDiscreteInputCommand(ushort tranId, byte station, string address, ushort length)
        {
            if (!TryPraseAddress(address, Bit, out var elementAddress))
                return [];

            byte[] bytes = new byte[12];
            var tranidBytes = BitConverter.GetBytes(tranId);
            if (BitConverter.IsLittleEndian)
                tranidBytes = tranidBytes.Reverse().ToArray();

            bytes[0] = tranidBytes[0];
            bytes[1] = tranidBytes[1];
            bytes[2] = 0x00;
            bytes[3] = 0x00;
            bytes[4] = 0x00;
            bytes[5] = 0x06;
            bytes[6] = station;

            bytes[7] = 0x02;

            var addressBytes = BitConverter.GetBytes((ushort)elementAddress);
            if (BitConverter.IsLittleEndian)
                addressBytes = addressBytes.Reverse().ToArray();

            bytes[8] = addressBytes[0];
            bytes[9] = addressBytes[1];

            var lengthBytes = BitConverter.GetBytes(length);
            if (BitConverter.IsLittleEndian)
                lengthBytes = lengthBytes.Reverse().ToArray();

            bytes[10] = lengthBytes[0];
            bytes[11] = lengthBytes[1];

            return bytes;
        }

        public static Result<bool[]> CheckReadDiscreteInputResponse(byte[] actualResponse, ushort requestedQuantity)
        {
            var result = new Result<bool[]>()
            {
                IsSuccess = true
            };
            var errorMessage = VerifyErrorCode(actualResponse[7], actualResponse[8]);
            if (!string.IsNullOrEmpty(errorMessage))
            {
                result.IsSuccess = false;
                result.Message = errorMessage;
                result.ErrorCode = actualResponse[8];
                return result;
            }
            int byteCount = actualResponse[8];
            if (actualResponse.Length < 9 + byteCount)
            {
                throw new Exception("CheckReadDiscreteInputResponse does not contain enough data bytes.");
            }
            byte[] coilsData = new byte[byteCount];
            Array.Copy(actualResponse, 9, coilsData, 0, byteCount);

            List<bool> coils = [];
            for (int i = 0; i < coilsData.Length && coils.Count < requestedQuantity; i++)
            {
                for (int j = 0; j < 8 && coils.Count < requestedQuantity; j++)
                {
                    coils.Add((coilsData[i] & 1 << j) != 0);
                }
            }
            result.Content = [.. coils];
            return result;
        }

        public static byte[] BuildWriteSingleCoilsCommand(ushort tranId, byte station, string address, bool coilsValue)
        {
            if (!TryPraseAddress(address, Bit, out var elementAddress))
                return [];
            byte[] bytes = new byte[12];
            var tranidBytes = BitConverter.GetBytes(tranId);
            if (BitConverter.IsLittleEndian)
                tranidBytes = tranidBytes.Reverse().ToArray();

            bytes[0] = tranidBytes[0];
            bytes[1] = tranidBytes[1];
            bytes[2] = 0x00;
            bytes[3] = 0x00;
            bytes[4] = 0x00;
            bytes[5] = 0x06;
            bytes[6] = station;
            bytes[7] = 0x05;
            var addressBytes = BitConverter.GetBytes((ushort)elementAddress);
            if (BitConverter.IsLittleEndian)
                addressBytes = addressBytes.Reverse().ToArray();

            bytes[8] = addressBytes[0];
            bytes[9] = addressBytes[1];
            bytes[10] = coilsValue ? (byte)0xff : (byte)0x00;
            bytes[11] = 0x00;
            return bytes;
        }

        public static Result CheckWriteCoilsResponse(byte[] send, byte[] actualResponse)
        {
            Result result = new();
            var errorMessage = VerifyErrorCode(actualResponse[7], actualResponse[8]);
            if (!string.IsNullOrEmpty(errorMessage))
            {
                result.IsSuccess = false;
                result.Message = errorMessage;
                result.ErrorCode = actualResponse[8];
                return result;
            }
            result.IsSuccess = send.SequenceEqual(actualResponse);
            return result;
        }

        public static byte[] BuildWriteMultiCoilsCommand(ushort tranId, byte station, string address, bool[] coilsValues)
        {
            if (!TryPraseAddress(address, Bit, out var elementAddress))
                return [];
            int byteCount = (coilsValues.Length + 7) / 8; // 计算需要多少个字节来存储这些状态
            byte[] outputValues = new byte[byteCount];

            for (int i = 0; i < coilsValues.Length; i++)
            {
                if (coilsValues[i])
                {
                    outputValues[i / 8] |= (byte)(1 << i % 8);
                }
            }
            var bytes = new byte[13 + outputValues.Length];
            var tranidBytes = BitConverter.GetBytes(tranId);
            if (BitConverter.IsLittleEndian)
                tranidBytes = tranidBytes.Reverse().ToArray();
            bytes[0] = tranidBytes[0];
            bytes[1] = tranidBytes[1];
            bytes[2] = 0x00;
            bytes[3] = 0x00;
            var lengthBytes = BitConverter.GetBytes((ushort)(7 + outputValues.Length));
            if (BitConverter.IsLittleEndian)
                lengthBytes = lengthBytes.Reverse().ToArray();
            bytes[4] = lengthBytes[0];
            bytes[5] = lengthBytes[1];
            bytes[6] = station;

            bytes[7] = 0x0f;
            var addressBytes = BitConverter.GetBytes((ushort)elementAddress);
            if (BitConverter.IsLittleEndian)
                addressBytes = addressBytes.Reverse().ToArray();

            bytes[8] = addressBytes[0];
            bytes[9] = addressBytes[1];
            var coilnumBytes = BitConverter.GetBytes((ushort)coilsValues.Length);
            if (BitConverter.IsLittleEndian)
                coilnumBytes = coilnumBytes.Reverse().ToArray();
            bytes[10] = coilnumBytes[0];
            bytes[11] = coilnumBytes[1];
            bytes[12] = (byte)byteCount;
            for (int i = 0; i < outputValues.Length; i++)
            {
                bytes[13 + i] = outputValues[i];
            }
            return bytes;
        }

        public static Result CheckWriteMultiCoilsResponse(byte[] send, byte[] actualResponse)
        {
            var result = new Result();
            var errorMessage = VerifyErrorCode(actualResponse[7], actualResponse[8]);
            if (!string.IsNullOrEmpty(errorMessage))
            {
                result.IsSuccess = false;
                result.Message = errorMessage;
                result.ErrorCode = actualResponse[8];
                return result;
            }
            var bytes = new byte[12];
            bytes[0] = send[0];
            bytes[1] = send[1];
            bytes[2] = send[2];
            bytes[3] = send[3];
            bytes[4] = send[4];
            bytes[5] = 0x06;
            bytes[6] = send[6];
            bytes[7] = send[7];
            bytes[8] = send[8];
            bytes[9] = send[9];
            bytes[10] = send[10];
            bytes[11] = send[11];
            result.IsSuccess = bytes.SequenceEqual(actualResponse);
            return result;
        }

        public static byte[] BuildReadHoldingRegisterCommand(ushort tranId, byte station, string address, ushort length)
        {
            if (!TryPraseAddress(address, Word, out var elementAddress))
                return [];
            byte[] bytes = new byte[12];
            var tranidBytes = BitConverter.GetBytes(tranId);
            if (BitConverter.IsLittleEndian)
                tranidBytes = tranidBytes.Reverse().ToArray();

            bytes[0] = tranidBytes[0];
            bytes[1] = tranidBytes[1];
            bytes[2] = 0x00;
            bytes[3] = 0x00;
            bytes[4] = 0x00;
            bytes[5] = 0x06;
            bytes[6] = station;

            bytes[7] = 0x03;

            var addressBytes = BitConverter.GetBytes((ushort)elementAddress);
            if (BitConverter.IsLittleEndian)
                addressBytes = addressBytes.Reverse().ToArray();

            bytes[8] = addressBytes[0];
            bytes[9] = addressBytes[1];

            var lengthBytes = BitConverter.GetBytes(length);
            if (BitConverter.IsLittleEndian)
                lengthBytes = lengthBytes.Reverse().ToArray();

            bytes[10] = lengthBytes[0];
            bytes[11] = lengthBytes[1];

            return bytes;
        }
        public static Result<short[]> CheckReadRegisterResponse(byte[] actualResponse)
        {
            var result = new Result<short[]>() { IsSuccess = true };
            var errorMessage = VerifyErrorCode(actualResponse[7], actualResponse[8]);
            if (!string.IsNullOrEmpty(errorMessage))
            {
                result.IsSuccess = false;
                result.Message = errorMessage;
                result.ErrorCode = actualResponse[8];
                return result;
            }
            int byteCount = actualResponse[8];
            if (actualResponse.Length < 9 + byteCount)
            {
                throw new Exception("CheckReadRegisterResponse does not contain enough data bytes.");
            }
            byte[] registerData = new byte[byteCount];
            Array.Copy(actualResponse, 9, registerData, 0, byteCount);
            var registers = new List<short>();
            for (int i = 0; i < registerData.Length; i += 2)
            {
                byte[] tempData = [registerData[i], registerData[i + 1]];
                if (BitConverter.IsLittleEndian)
                    tempData = tempData.Reverse().ToArray();
                registers.Add(BitConverter.ToInt16(tempData));
            }
            result.Content = [.. registers];
            return result;
        }

        public static byte[] BuildReadInputRegisterCommand(ushort tranId, byte station, string address, ushort length)
        {
            if (!TryPraseAddress(address, Word, out var elementAddress))
                return [];
            byte[] bytes = new byte[12];
            var tranidBytes = BitConverter.GetBytes(tranId);
            if (BitConverter.IsLittleEndian)
                tranidBytes = tranidBytes.Reverse().ToArray();

            bytes[0] = tranidBytes[0];
            bytes[1] = tranidBytes[1];
            bytes[2] = 0x00;
            bytes[3] = 0x00;
            bytes[4] = 0x00;
            bytes[5] = 0x06;
            bytes[6] = station;

            bytes[7] = 0x04;

            var addressBytes = BitConverter.GetBytes((ushort)elementAddress);
            if (BitConverter.IsLittleEndian)
                addressBytes = addressBytes.Reverse().ToArray();

            bytes[8] = addressBytes[0];
            bytes[9] = addressBytes[1];

            var lengthBytes = BitConverter.GetBytes(length);
            if (BitConverter.IsLittleEndian)
                lengthBytes = lengthBytes.Reverse().ToArray();

            bytes[10] = lengthBytes[0];
            bytes[11] = lengthBytes[1];

            return bytes;
        }

        public static byte[] BuildWriteSingleRegisterCommand(ushort tranId, byte station, string address, short register)
        {
            if (!TryPraseAddress(address, Word, out var elementAddress))
                return [];
            byte[] bytes = new byte[12];
            var tranidBytes = BitConverter.GetBytes(tranId);
            if (BitConverter.IsLittleEndian)
                tranidBytes = tranidBytes.Reverse().ToArray();

            bytes[0] = tranidBytes[0];
            bytes[1] = tranidBytes[1];
            bytes[2] = 0x00;
            bytes[3] = 0x00;
            bytes[4] = 0x00;
            bytes[5] = 0x06;
            bytes[6] = station;

            bytes[7] = 0x06;

            var addressBytes = BitConverter.GetBytes((ushort)elementAddress);
            if (BitConverter.IsLittleEndian)
                addressBytes = addressBytes.Reverse().ToArray();

            bytes[8] = addressBytes[0];
            bytes[9] = addressBytes[1];

            var registerBytes = BitConverter.GetBytes(register);
            if (BitConverter.IsLittleEndian)
                registerBytes = registerBytes.Reverse().ToArray();

            bytes[10] = registerBytes[0];
            bytes[11] = registerBytes[1];

            return bytes;
        }
        public static Result CheckWriteSingleRegisterResponse(byte[] send, byte[] actualResponse)
        {
            var result = new Result();
            var errorMessage = VerifyErrorCode(actualResponse[7], actualResponse[8]);
            if (!string.IsNullOrEmpty(errorMessage))
            {
                result.IsSuccess = false;
                result.Message = errorMessage;
                result.ErrorCode = actualResponse[8];
                return result;
            }
            return new Result
            {
                IsSuccess = send.SequenceEqual(actualResponse)
            };
        }
        public static byte[] BuildWriteMultiRegisterCommand(ushort tranId, byte station, string address, short[] registers)
        {
            if (!TryPraseAddress(address, Word, out var elementAddress))
                return [];
            byte[] outputValues = new byte[registers.Length * 2];
            for (int i = 0; i < outputValues.Length; i += 2)
            {
                var tempdata = BitConverter.GetBytes(registers[i / 2]);
                if (BitConverter.IsLittleEndian)
                    tempdata = tempdata.Reverse().ToArray();
                outputValues[i] = tempdata[0];
                outputValues[i + 1] = tempdata[1];
            }
            var bytes = new byte[13 + outputValues.Length];
            var tranidBytes = BitConverter.GetBytes(tranId);
            if (BitConverter.IsLittleEndian)
                tranidBytes = tranidBytes.Reverse().ToArray();
            bytes[0] = tranidBytes[0];
            bytes[1] = tranidBytes[1];
            bytes[2] = 0x00;
            bytes[3] = 0x00;
            var lengthBytes = BitConverter.GetBytes((ushort)(7 + outputValues.Length));
            if (BitConverter.IsLittleEndian)
                lengthBytes = lengthBytes.Reverse().ToArray();
            bytes[4] = lengthBytes[0];
            bytes[5] = lengthBytes[1];
            bytes[6] = station;
            bytes[7] = 0x10;
            var addressBytes = BitConverter.GetBytes((ushort)elementAddress);
            if (BitConverter.IsLittleEndian)
                addressBytes = addressBytes.Reverse().ToArray();

            bytes[8] = addressBytes[0];
            bytes[9] = addressBytes[1];
            var registerBytes = BitConverter.GetBytes((ushort)registers.Length);
            if (BitConverter.IsLittleEndian)
                registerBytes = registerBytes.Reverse().ToArray();
            bytes[10] = registerBytes[0];
            bytes[11] = registerBytes[1];
            bytes[12] = (byte)outputValues.Length;
            for (int i = 0; i < outputValues.Length; i++)
            {
                bytes[13 + i] = outputValues[i];
            }
            return bytes;
        }
        public static Result CheckWriteMultiRegisterResponse(byte[] send, byte[] actualResponse)
        {
            var result = new Result();
            var errorMessage = VerifyErrorCode(actualResponse[7], actualResponse[8]);
            if (!string.IsNullOrEmpty(errorMessage))
            {
                result.IsSuccess = false;
                result.Message = errorMessage;
                result.ErrorCode = actualResponse[8];
                return result;
            }
            var bytes = new byte[12];
            bytes[0] = send[0];
            bytes[1] = send[1];
            bytes[2] = send[2];
            bytes[3] = send[3];
            bytes[4] = send[4];
            bytes[5] = 0x06;
            bytes[6] = send[6];
            bytes[7] = send[7];
            bytes[8] = send[8];
            bytes[9] = send[9];
            bytes[10] = send[10];
            bytes[11] = send[11];
            result.IsSuccess = bytes.SequenceEqual(actualResponse);
            return result;
        }
        public static string VerifyErrorCode(byte errorfunctionCode, byte errorCode)
        {
            if ((errorfunctionCode & 0x80) != 0)
            {
                return GetDescriptionByErrorCode(errorCode);
            }
            return string.Empty;
        }
        public static string GetDescriptionByErrorCode(byte code)
        {
            return code switch
            {
                FunctionCodeNotSupport => "不支持的功能码",
                FunctionCodeOverBound => "功能码超出范围",
                FunctionCodeQuantityOver => "请求的数量超出范围",
                FunctionCodeReadWriteException => "读写异常",
                _ => "未知错误",
            };
        }
        public static bool TryPraseAddress(string address, int softElementType, out int elementAddress)
        {
            string header = address[0].ToString();
            string addCode = address[1..];
            elementAddress = -1;
            switch (softElementType)
            {
                case Bit:
                    if (ContainsBitSoftElement(header))
                    {
                        switch (header)
                        {
                            case "X":
                                elementAddress = (int)BitSoftElement.X + Convert.ToInt32(addCode, 8);
                                return true;
                            case "Y":
                                elementAddress = (int)BitSoftElement.Y + Convert.ToInt32(addCode, 8);
                                return true;
                            case "M":
                                elementAddress = (int)BitSoftElement.M + int.Parse(addCode);
                                return true;
                            case "B":
                                elementAddress = (int)BitSoftElement.B + int.Parse(addCode);
                                return true;
                            case "S":
                                elementAddress = (int)BitSoftElement.S + int.Parse(addCode);
                                return true;
                            default:
                                throw new Exception("位软元件格式错误,无法解析地址！");
                        }
                    }
                    return false;
                case Word:
                    if (ContainsWordSoftElement(header))
                    {
                        switch (header)
                        {
                            case "D":
                                elementAddress = (int)WordSoftElement.D + int.Parse(addCode);
                                return true;
                            case "R":
                                elementAddress = (int)WordSoftElement.R + int.Parse(addCode);
                                return true;
                            default:
                                throw new Exception("位软元件格式错误,无法解析地址！");
                        }
                    }
                    return false;
                default:
                    return false;
            }
        }
        public static bool ContainsBitSoftElement(string address)
        {
            return BitSoftElementNames
                .Any(p => p.Equals(address));
        }
        public static bool ContainsWordSoftElement(string address)
        {
            return WordSoftElementNames
                .Any(p => p.Equals(address));
        }


        public static string[] WordSoftElementNames => Enum
                .GetNames(typeof(WordSoftElement));

        public static string[] BitSoftElementNames => Enum
                .GetNames(typeof(BitSoftElement));
    }

    /// <summary>
    /// 位软元件
    /// </summary>
    public enum BitSoftElement
    {
        M = 0,
        B = 12288,
        S = 57344,
        X = 63488,
        Y = 64512
    }

    /// <summary>
    /// 字软元件
    /// </summary>
    public enum WordSoftElement
    {
        D = 0,
        R = 12288
    }
}
