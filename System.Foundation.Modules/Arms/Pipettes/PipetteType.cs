using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace System.Foundation.Modules.Arms.Pipettes
{
    public enum PipetteType
    {
        /// <summary>
        /// 取枪头
        /// </summary>
        PipettePick,
        /// <summary>
        /// 吸液
        /// </summary>
        PipetteSuck,
        /// <summary>
        /// 吐液
        /// </summary>
        PipetteDispense,
        /// <summary>
        /// 丢枪头
        /// </summary>
        PipetteRelease,
        /// <summary>
        /// 移液
        /// </summary>
        LiquorRelief,
    }
}
