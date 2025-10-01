using SqlSugar;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace QStandaedPlatform.Engine.Common.Common.ModuleEntitys
{
    [Table("t_module_report_alarm")]
    public class ModuleReportAlarmEntity : RunDataEntity<int>
    {
        /// <summary>
        /// PLC IP地址
        /// </summary>
        public string? IPAddress { get; set; }
        /// <summary>
        /// 报警地址
        /// </summary>
        public string? AlarmAddress { get; set; }
        /// <summary>
        /// 报警信息描述
        /// </summary>
        public string? AlarmDesc { get; set; }
        /// <summary>
        /// 报警代码
        /// </summary>
        public string? AlarmCode { get; set; }
        /// <summary>
        /// 报警时间
        /// </summary>
        public DateTime? AlarmTime { get; set; }=DateTime.Now;
    }
}
