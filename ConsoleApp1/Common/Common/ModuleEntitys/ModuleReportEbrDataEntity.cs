using SqlSugar;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace QStandaedPlatform.Engine.Common.Common.ModuleEntitys
{
    /// <summary>
    /// 模块上报常量数据表
    /// </summary>
    [Table("t_module_report_constant_data")]
    public class ModuleReportEbrDataEntity:RunDataEntity<int>
    {
        /// <summary>
        /// ebr数据键名
        /// </summary>
        public string EbrKey { get; set; }
        /// <summary>
        /// ebr数据值
        /// </summary>
        public string EbrValue { get; set; }

        /// <summary>
        /// ebr数据地址
        /// </summary>
        public string EbrAddress { get; set; }

        /// <summary>
        /// ebr数据单位
        /// </summary>
        public string EbrValueUnit { get; set; }
    }
}
