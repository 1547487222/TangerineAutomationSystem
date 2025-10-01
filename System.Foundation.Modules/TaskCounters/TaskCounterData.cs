using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace System.Foundation.Modules.TaskCounters
{
    public class TaskCounterData
    {
        [DisplayName("任务数")]
        public int TaskCounter { get; set; }
        [DisplayName("开始累加值")]
        /// <summary>
        /// 开始累加值
        /// </summary>
        public int StartIndex { get; set; }

        //累加间隔
        [DisplayName("累加间隔")]
        public int Interval { get; set; } = 0;

        /// <summary>
        /// 跳过列表
        /// </summary>
        public List<int> SkipList { get; set; } = [];
    }
}
