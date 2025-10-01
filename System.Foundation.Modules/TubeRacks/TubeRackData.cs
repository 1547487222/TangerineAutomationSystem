using QStandaedPlatform.Engine.Common.Common;
using QStandaedPlatform.Engine.Common.Common.Metadatas;
using QStandaedPlatform.Engine.Laboratory;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace System.Foundation.Modules.TubeRacks
{
    public class TubeRackData
    {
        /// <summary>
        /// 列数
        /// </summary>
        [DisplayName("列数")]
        public int Cols { get; set; }
        /// <summary>
        /// 行数
        /// </summary>
        [DisplayName("行数")]
        public int Rows { get; set; }
        /// <summary>
        /// 行间距
        /// </summary>
        [DisplayName("Y方向间距")]
        public float SpaceRow { get; set; }
        /// <summary>
        /// 列间距
        /// </summary>
        [DisplayName("X方向间距")]
        public float SpaceCol { get; set; }

        [DisplayName("行标签")]
        public string RowLable { get; set; }

        [DisplayName("列标签")]
        public string ColLable { get; set; }

        [DisplayName("开始位置")]
        [TypeConverter(typeof(ExpandableObjectConverter))]
        public QPosition StartPosition { get; set; } = new QPosition();

        [DisplayName("行优先")]
        public bool Rowfirst { get; set; } = true;

        //输出样品组数
        [DisplayName("输出样品组数")]
        public int OutputSampleGroup { get; set; } = 2;

        /// <summary>
        /// 管架类别
        /// </summary>
        [DisplayName("管架类别")]
        public string RackCategory { get; set; }

        /// <summary>
        /// 管架名称
        /// </summary>
        [DisplayName("管架名称")]
        public string RackName { get; set; }

        public long MaterialId { get; set; }

        public string MaterialName { get; set; }
        /// <summary>
        /// 管架类型
        /// </summary>
        [DisplayName("管架类型")]
        public LabTrayDefaultType RackType { get; set; }
        /// <summary>
        /// 管架位置编号
        /// </summary>
        [DisplayName("管架位置编号")]
        public string PostionNo { get; set; }
        public string RackStandard { get; internal set; }
    }
}
