using QStandaedPlatform.Engine.Common.Common;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace System.Foundation.Modules.NormalModules
{
    public class NormalModuleData: ModuleData
    {
        /// <summary>
        /// 功能码
        /// </summary>
        [DisplayName("功能码")]
        public int FuncCode { get; set; }
        /// <summary>
        /// 命令地址
        /// </summary>
        [DisplayName("命令地址")]
        public string CommandAddress { get; set; } = "D210";
        /// <summary>
        /// 状态地址
        /// </summary>
        [DisplayName("状态地址")]
        public string StateAddress { get; set; } = "D200";
        /// <summary>
        /// 状态码
        /// </summary>
        [DisplayName("状态码")]
        public int StateCode { get; set; } = 999;

        [DisplayName("模块输入参数")]
        public List<NormalParam> NormalParams { get; set; } = [];
        /// <summary>
        /// 模块Ebr数据
        /// </summary>
        [DisplayName("模块Ebr数据")]
        public List<TaskEbrDataConfig> TaskEbrDataConfigs { get; set; } = [];
        /// <summary>
        /// 模块监控数据
        /// </summary>
        [DisplayName("模块监控数据")]
        public List<MonitorDataConfig> MonitorDataConfigs { get; set; } = [];


        [DisplayName("模块精度数据")]
        public List<PrecisionDataConfig> PrecisionDataConfigs { get; set; } = [];

        /// <summary>
        /// 监控时间间隔
        /// </summary>
        [DisplayName("监控时间间隔/s")]
        public int MonitorTimeInterval { get; set; } = 3;

        [DisplayName("Ebr延时获取/ms")]
        public int EbrDelayTime { get; set; } = 0;

        [DisplayName("导出监控数据路径")]
        public string ExportMonitorDataPath { get; set; } = Directory.GetCurrentDirectory();


        [DisplayName("导出精度数据路径")]
        public string ExportPrecisionDataPath { get; set; } = Directory.GetCurrentDirectory();
    }
    public class NormalParam
    {
        public string ParamCode { get; set; }
        public string ParamName { get; set; }

        public string Address { get; set; }

        public string DataType { get; set; } = "REAL";

        public string Value { get; set; }

        public override string ToString()
        {
            return ParamName + ":" + Address;
        }
    }

    public class TaskEbrDataConfig
    {
        /// <summary>
        /// 属性键
        /// </summary>
        [DisplayName("属性键")]
        public string EbrKey { get; set; }
        /// <summary>
        /// 属性plc地址
        /// </summary>
        [DisplayName("属性plc地址")]
        public string EbrAddress { get; set; }
        /// <summary>
        /// 数据类型
        /// </summary>
        [DisplayName("数据类型")]
        public string EbrValueType { get; set; }
        /// <summary>
        /// 单位
        /// </summary>
        [DisplayName("单位")]
        public string EbrValueUnit { get;set; }
    }

    public class MonitorDataConfig
    {
        /// <summary>
        /// 监控键
        /// </summary>
        [DisplayName("监控键")]
        public string MonitorKey { get; set; }
        /// <summary>
        /// 监控地址
        /// </summary>
        [DisplayName("监控地址")]
        public string MonitorAddress { get; set; }
        /// <summary>
        /// 监控单位
        /// </summary>
        [DisplayName("监控单位")]
        public string MonitorValueUnit { get; set; }
    }

    //精度数据配置
    public class PrecisionDataConfig
    {
        [DisplayName("精度数据键")]
        public string PrecisionKey { get; set; }

        [DisplayName("精度数据地址")]
        public string PrecisionAddress { get;set; }
        [DisplayName("精度标准值")]
        public float StandardValue { get; set; }
    }
}
