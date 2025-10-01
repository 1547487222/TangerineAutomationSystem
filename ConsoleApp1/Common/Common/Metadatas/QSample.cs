using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace QStandaedPlatform.Engine.Common.Common.Metadatas
{
    public class QSample:QData
    {
        /// <summary>
        /// 进样任务ID
        /// </summary>
        public long SamplingTaskId { get; set; }

        /// <summary>
        /// 平台ID
        /// </summary>
        public long PlatformId { get; set; }

        /// <summary>
        /// 平台流程ID
        /// </summary>
        public long PlatformTaskId { get; set; }
        /// <summary>
        /// 进样ID
        /// </summary>
        public long SamplingId { get; set; }



        public override string ToString()
        {
            return $"SamplingTaskId:{SamplingTaskId},SamplingId:{SamplingId}";
        }

    }
}
