using System;
using System.Collections.Generic;
using System.IO.Compression;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace QStandaedPlatform.Engine.Common.Common
{

    /// <summary>
    /// 压缩工具类（支持 Deflate 和 GZip）
    /// </summary>
    public class CompressionService
    {
        private readonly CompressionLevel _compressionLevel;

        /// <summary>
        /// 构造函数，可指定默认压缩等级
        /// </summary>
        /// <param name="compressionLevel">默认压缩等级</param>
        public CompressionService(CompressionLevel compressionLevel = CompressionLevel.Optimal)
        {
            _compressionLevel = compressionLevel;
        }

        /// <summary>
        /// 使用 Deflate 算法压缩字符串
        /// </summary>
        /// <param name="input">要压缩的字符串</param>
        /// <returns>压缩后的字节数组</returns>
        public byte[] CompressWithDeflate(string input)
        {
            if (string.IsNullOrEmpty(input))
                return [];

            byte[] inputBytes = Encoding.UTF8.GetBytes(input);

            using var memoryStream = new MemoryStream();
            using (var deflateStream = new DeflateStream(memoryStream, _compressionLevel))
            {
                deflateStream.Write(inputBytes, 0, inputBytes.Length);
            }

            return memoryStream.ToArray();
        }

        /// <summary>
        /// 使用 Deflate 算法解压缩字节数组为字符串
        /// </summary>
        /// <param name="compressedBytes">压缩的字节数据</param>
        /// <returns>解压缩后的字符串</returns>
        public string DecompressWithDeflate(byte[] compressedBytes)
        {
            if (compressedBytes == null)
                throw new ArgumentNullException(nameof(compressedBytes));

            if (compressedBytes.Length == 0)
                return string.Empty;

            using var memoryStream = new MemoryStream(compressedBytes);
            using var outputStream = new MemoryStream();
            using (var deflateStream = new DeflateStream(memoryStream, CompressionMode.Decompress))
            {
                deflateStream.CopyTo(outputStream);
            }

            return Encoding.UTF8.GetString(outputStream.ToArray());
        }

        /// <summary>
        /// 使用 GZip 算法压缩字符串（推荐用于网络传输）
        /// </summary>
        /// <param name="input">要压缩的字符串</param>
        /// <returns>压缩后的字节数组</returns>
        public byte[] CompressWithGZip(string input)
        {
            if (string.IsNullOrEmpty(input))
                return [];

            byte[] inputBytes = Encoding.UTF8.GetBytes(input);

            using var memoryStream = new MemoryStream();
            using (var gzipStream = new GZipStream(memoryStream, _compressionLevel))
            {
                gzipStream.Write(inputBytes, 0, inputBytes.Length);
            }

            return memoryStream.ToArray();
        }

        /// <summary>
        /// 使用 GZip 算法解压缩字节数组为字符串
        /// </summary>
        /// <param name="compressedBytes">压缩的字节数据</param>
        /// <returns>解压缩后的字符串</returns>
        public string DecompressWithGZip(byte[] compressedBytes)
        {
            if (compressedBytes == null)
                throw new ArgumentNullException(nameof(compressedBytes));

            if (compressedBytes.Length == 0)
                return string.Empty;

            using var memoryStream = new MemoryStream(compressedBytes);
            using var gzipStream = new GZipStream(memoryStream, CompressionMode.Decompress);
            using var streamReader = new StreamReader(gzipStream, Encoding.UTF8);

            return streamReader.ReadToEnd();
        }

        /// <summary>
        /// 判断数据是否为 Deflate 格式（简单校验）
        /// </summary>
        /// <param name="data">字节数组</param>
        /// <returns>是否为 Deflate 数据</returns>
        public bool IsDeflateData(byte[] data)
        {
            if (data == null || data.Length < 2) return false;

            int cmf = data[0];
            int flg = data[1];
            int cm = (cmf & 0x0F);
            int cinfo = (cmf >> 4) & 0x0F;

            return cm == 8 && cinfo <= 7 && ((cmf * 256 + flg) % 31 == 0);
        }
    }
}
