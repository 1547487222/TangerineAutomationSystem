using Microsoft.Extensions.Logging;
using QStandaedPlatform.Engine.Common.Common;
using QStandaedPlatform.Engine.Common.Common.Metadatas;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace System.Foundation.Modules.ModuleStations
{
    [DisplayName("模块工位_1")]
    public class ModuleSingleStationTool : ToolBase
    {
        public ModuleSingleStationData moduleSingleStationData = new();
        public class ModuleSingleStationData
        {
            [TypeConverter(typeof(ExpandableObjectConverter))]
            public QPosition ModulePos { get; set; } = new QPosition();
        }
        public override string DefineName => "模块工位_1";

        public override bool InitPins()
        {
            InsetPin("获取模块位置",this,typeof(QData), PinType.Input);
            InsetPin("输出模块位置", this, typeof(QPosition), PinType.Output);
            return base.InitPins();
        }
        public override bool InitDataContext()
        {
            DataContext = moduleSingleStationData;
            return base.InitDataContext();
        }
        public override Task<bool> ExecuteAsync(PinInfo pinInfo, QData pinData, ToolExecutionContext toolContext)
        {
            if (pinInfo.Name == "获取模块位置")
            {
                var pos = Context<ModuleSingleStationData>().ModulePos;
                Logger.LogInformation($"输出一个试管孔位 , Position:<X:{pos.X} Y:{pos.Y} Z:{pos.Z}>");
                SendToPin("输出模块位置", pos);
                return Task.FromResult(true);
            }
            return Task.FromResult(false);
        }
    }
}
