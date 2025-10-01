using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace QStandaedPlatform.Engine.Common.Common
{
    public interface IContext<T> where T : class, IContextNotifyPropertyChanged
    {
        /// <summary>
        /// 具有属性通知的数据上下文
        /// </summary>
        T DataContext { get; }
    }
}
