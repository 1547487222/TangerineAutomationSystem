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
    /// 模块上报精度数据表
    /// </summary>
    [Table("t_module_report_precision")]
    public class ModuleReportPrecisionEntity : RunDataEntity<long>
    {
        /// <summary>
        /// 精度值名称
        /// </summary>
        public string PrecitiName { get; set; }

        /// <summary>
        /// 精度值真值
        /// </summary>
        public float PrecitiRealValue { get; set; }

        /// <summary>
        /// 精度值标准值
        /// </summary>
        public float PrecitiStandardValue { get; set; }
    }
}
