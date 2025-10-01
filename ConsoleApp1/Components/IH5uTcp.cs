using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace QStandaedPlatform.Engine.Components
{

    public interface IH5uTcp
    {

        string Ip { get; }

        int Port { get; }
        Task<(bool, string)> OpenAsync();
        (bool, string) Open();
        /// <summary>
        /// 读取单个bool变量
        /// </summary>
        /// <param name="element">软元件地址</param>
        /// <returns></returns>
        bool ReadSingleBoolean(string element);

        /// <summary>
        /// 写入单个bool变量
        /// </summary>
        /// <param name="element">软元件地址</param>
        /// <param name="value">写入值</param>
        /// <returns></returns>
        bool WriteSingleBoolean(string element, bool value);

        /// <summary>
        /// 读取单个类型数据
        /// </summary>
        /// <typeparam name="T">类型</typeparam>
        /// <param name="element">软元件地址</param>
        /// <returns></returns>
        T ReadSingleValue<T>(string element);

        /// <summary>
        /// 写入单个类型数据
        /// </summary>
        /// <typeparam name="T">类型</typeparam>
        /// <param name="element">软元件地址</param>
        /// <param name="Value">写入值</param>
        /// <returns></returns>
        bool WriteSingleValue<T>(string element, T Value);

        /// <summary>
        /// 读取多个bool变量
        /// </summary>
        /// <param name="element">软元件起始地址</param>
        /// <param name="count">数量</param>
        /// <returns></returns>
        List<bool> ReadMultiBoolean(string element, int count);

        /// <summary>
        /// 写入多个bool变量
        /// </summary>
        /// <param name="element">软元件起始地址</param>
        /// <param name="value">写入值</param>
        /// <returns></returns>
        bool WriteMultiBoolean(string element, params bool[] value);

        /// <summary>
        /// 读取多个类型数据
        /// </summary>
        /// <typeparam name="T">数据类型</typeparam>
        /// <param name="element">软元件起始地址</param>
        /// <param name="count">数量(读取数量)</param>
        /// <returns></returns>
        List<T> ReadMultiValue<T>(string element, int count);

        /// <summary>
        /// 写入多个类型数据
        /// </summary>
        /// <typeparam name="T">数据类型</typeparam>
        /// <param name="element">软元件起始地址</param>
        /// <param name="value">写入值</param>
        /// <returns></returns>
        bool WriteMultiValue<T>(string element, params T[] value);

        /// <summary>
        /// 翻转输出
        /// </summary>
        /// <param name="element"></param>
        /// <returns></returns>
        bool ReverseOutput(string element);

        /// <summary>
        /// 脉冲输出
        /// </summary>
        /// <param name="element"></param>
        /// <param name="pulsTime">单位毫秒</param>
        /// <returns></returns>
        bool PlusOutput(string element, int pulsTime);

        /// <summary>
        /// 读取一个byte的线圈
        /// </summary>
        /// <param name="element"></param>
        /// <returns></returns>
        byte ReadByteFromMultiBoolean(string element);

        /// <summary>
        /// 点动输出
        /// </summary>
        /// <param name="element"></param>
        void PressOutput(string element);

        /// <summary>
        /// 读取单个bool变量
        /// </summary>
        /// <param name="element">软元件地址</param>
        /// <returns></returns>
        Task<bool> ReadSingleBooleanAsync(string element);

        /// <summary>
        /// 写入单个bool变量
        /// </summary>
        /// <param name="element">软元件地址</param>
        /// <param name="value">写入值</param>
        /// <returns></returns>
        Task<bool> WriteSingleBooleanAsync(string element, bool value);

        /// <summary>
        /// 读取单个类型数据
        /// </summary>
        /// <typeparam name="T">类型</typeparam>
        /// <param name="element">软元件地址</param>
        /// <returns></returns>
        Task<T> ReadSingleValueAsync<T>(string element);

        /// <summary>
        /// 写入单个类型数据
        /// </summary>
        /// <typeparam name="T">类型</typeparam>
        /// <param name="element">软元件地址</param>
        /// <param name="Value">写入值</param>
        /// <returns></returns>
        Task<bool> WriteSingleValueAsync<T>(string element, T Value);

        /// <summary>
        /// 读取多个bool变量
        /// </summary>
        /// <param name="element">软元件起始地址</param>
        /// <param name="count">数量</param>
        /// <returns></returns>
        Task<List<bool>> ReadMultiBooleanAsync(string element, int count);

        /// <summary>
        /// 写入多个bool变量
        /// </summary>
        /// <param name="element">软元件起始地址</param>
        /// <param name="value">写入值</param>
        /// <returns></returns>
        Task<bool> WriteMultiBooleanAsync(string element, params bool[] value);

        /// <summary>
        /// 读取多个类型数据
        /// </summary>
        /// <typeparam name="T">数据类型</typeparam>
        /// <param name="element">软元件起始地址</param>
        /// <param name="count">数量(读取数量)</param>
        /// <returns></returns>
        Task<List<T>> ReadMultiValueAsync<T>(string element, int count);

        string ReadString(string element, int length);

        Task<string> ReadStringAsync(string element, int length);

        /// <summary>
        /// 写入多个类型数据
        /// </summary>
        /// <typeparam name="T">数据类型</typeparam>
        /// <param name="element">软元件起始地址</param>
        /// <param name="value">写入值</param>
        /// <returns></returns>
        Task<bool> WriteMultiValueAsync<T>(string element, params T[] value);

        /// <summary>
        /// 翻转输出
        /// </summary>
        /// <param name="element"></param>
        /// <returns></returns>
        Task<bool> ReverseOutputAsync(string element);

        /// <summary>
        /// 脉冲输出
        /// </summary>
        /// <param name="element"></param>
        /// <param name="pulsTime">单位毫秒</param>
        /// <returns></returns>
        Task<bool> PlusOutputAsync(string element, int pulsTime);

        /// <summary>
        /// 读取一个byte的线圈
        /// </summary>
        /// <param name="element"></param>
        /// <returns></returns>
        Task<byte> ReadByteFromMultiBooleanAsync(string element);

        /// <summary>
        /// 点动输出
        /// </summary>
        /// <param name="element"></param>
        Task PressOutputAsync(string element);
    }
}
