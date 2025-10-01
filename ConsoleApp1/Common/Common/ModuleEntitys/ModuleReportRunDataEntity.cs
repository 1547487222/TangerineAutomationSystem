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
    ///  数据收集
    /// </summary>
    [Table("t_module_report_rundata")]
    public class ModuleReportRunDataEntity:RunDataEntity<int>
    {
        public ModuleReportRunDataEntity()
        {
            EbrDatas = [];
            PrecisionDatas = [];
            TaskDatas = [];
            Alarms =  [];
        }
        /// <summary>
        /// 平台名称
        /// </summary>
        public string PlatFormName { get; set; } = string.Empty;
        /// <summary>
        /// 模块名称
        /// </summary>
        public string ModuleName { get; set; } = string.Empty;

        /// <summary>
        /// EBR数据集
        /// </summary>
        public List<ModuleReportEbrDataEntity> EbrDatas { get; set; }

        /// <summary>
        /// 精度数据集
        /// </summary>
        public List<ModuleReportPrecisionEntity>  PrecisionDatas { get; set; }
        /// <summary>
        /// 任务数据集
        /// </summary>
        public List<ModuleReportMonitorTaskDataOnceEntity> TaskDatas { get; set; }




        /// <summary>
        /// 故障集
        /// </summary>
        public List<ModuleReportAlarmEntity>  Alarms { get; set; }

        /// <summary>
        /// 模块工艺参数
        /// </summary>
        public string ModuleParameter { get; set; } = string.Empty;
        /// <summary>
        /// 模块动作描述
        /// </summary>
        public string ModuleActionDescription { get; set; } = string.Empty;

        /// <summary>
        /// 开始时间
        /// </summary>
        public DateTime StartTime { get; set; }


        public DateOnly StartDay { get; set; }
        /// <summary>
        /// 运行时间/ms
        /// </summary>
        public double DurationTime { get; set; }
        /// <summary>
        /// 结束时间
        /// </summary>
        public DateTime  EndTime { get; set; }


    }
}
