using QStandaedPlatform.Engine.Common.Common.Metadatas;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace QStandaedPlatform.Engine.Common.Common.SampleEntitys
{
    /// <summary>
    /// 样品Ebr数据实体
    /// </summary>
    public class SampleTaskDataEntity : ICloneable
    {

        /// <summary>
        /// EBR参数名称(英文)
        /// </summary>
        public string EbrKey { get; set; } = string.Empty;
        /// <summary>
        /// Ebr参数值
        /// </summary>
        public string EbrValue { get; set; } = string.Empty;

        /// <summary>
        /// Ebr单位
        /// </summary>
        public string EbrUnit { get; set; } = string.Empty;

        /// <summary>
        /// EBR参数描述
        /// </summary>
        public string EbrKeyDescription { get; set; } = string.Empty;

        /// <summary>
        /// 创建时间
        /// </summary>
        public DateTime RecordTime { get; set; } = DateTime.Now;

        public object Clone()
        {
            var clone = new SampleTaskDataEntity
            {
                EbrKey = EbrKey,
                EbrUnit = EbrUnit,
                EbrValue = EbrValue,
                RecordTime = RecordTime,
                EbrKeyDescription = EbrKeyDescription
            };
            return clone;
        }
    }
}


