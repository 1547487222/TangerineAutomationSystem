using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace QStandaedPlatform.Engine.Common.Common.Metadatas
{
    public class QFlowResult : QData
    {
        /// <summary>
        /// 流程名
        /// </summary>
        public QString FlowName { get; set; }
        /// <summary>
        /// 工作Id
        /// </summary>
        public QString WorkId { get; set; }
        /// <summary>
        /// 产出数据
        /// </summary>
        public Dictionary<string, QData> Datas { get; set; } = [];
    }
}
