using QStandaedPlatform.Engine.Laboratory.Documents;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace QStandaedPlatform.Engine.Laboratory
{
    /// <summary>
    /// 托盘服务
    /// </summary>
    public interface ILabTrayService
    {
        List<LabTray> LabTrays { get; }
    }
}
