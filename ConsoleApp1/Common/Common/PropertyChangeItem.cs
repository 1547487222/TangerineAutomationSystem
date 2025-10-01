using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace QStandaedPlatform.Engine.Common.Common
{
    public struct PropertyChangeItem(string propName, object oldValue, object newValue)
    {
        /// <summary>
        /// 属性名
        /// </summary>
        public string PropertyName { get; set; } = propName;
        /// <summary>
        /// 属性当前值
        /// </summary>
        public object NewValue { get; set; } = newValue;
        /// <summary>
        /// 属性原来值
        /// </summary>
        public object OriginalValue { get; set; } = oldValue;
        /// <summary>
        /// 属性改变时间
        /// </summary>
        public DateTime ChangeDateTime { get; set; } = DateTime.Now;
    }
}
