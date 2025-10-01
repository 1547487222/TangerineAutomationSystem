using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace QStandaedPlatform.Engine.Common.Common.SampleEntitys
{

    /// <summary>
    /// 表示一次分样操作中的单条分样记录
    /// </summary>
    public class AliquotRecord
    {
        /// <summary>
        /// 序号（自增或顺序编号）
        /// </summary>
        public int Id { get; set; }

        /// <summary>
        /// 上样容器二维码
        /// </summary>
        public string UpperContainerQRCode { get; set; } = string.Empty;

        /// <summary>
        /// 原始样品编码
        /// </summary>
        public string OriginalSampleCode { get; set; }= string.Empty;

        /// <summary>
        /// 分样容器二维码
        /// </summary>
        public string SubsampleContainerQRCode { get; set; } = string.Empty;

        /// <summary>
        /// 样品重量（单位：g）
        /// </summary>
        public double SampleWeight { get; set; }

        /// <summary>
        /// 样品体积（单位：ml）
        /// </summary>
        public double SampleVolume { get; set; }

        /// <summary>
        /// 分样容器类型（如：15ml离心管、500ml萃取瓶）
        /// </summary>
        public string ContainerType { get; set; } = string.Empty;

        /// <summary>
        /// 容器在设备或货架中的位置
        /// </summary>
        public string ContainerLocation { get; set; } = string.Empty;

        /// <summary>
        /// 分样过程中使用的工具（如移液枪、自动分液器等）
        /// </summary>
        public string ToolUsed { get; set; } = string.Empty;

        /// <summary>
        /// 样品当前状态（如：待检测、已冻结、已出库）
        /// </summary>
        public string SampleStatus { get; set; } = string.Empty;

        /// <summary>
        /// 下料仓库或存储位置
        /// </summary>
        public string Warehouse { get; set; } = string.Empty;

        /// <summary>
        /// 备注信息
        /// </summary>
        public string Remarks { get; set; } = string.Empty;
    }
}
