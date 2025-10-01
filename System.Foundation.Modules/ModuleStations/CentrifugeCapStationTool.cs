using Microsoft.Extensions.Logging;
using QStandaedPlatform.Engine.Common.Common;
using QStandaedPlatform.Engine.Common.Common.Metadatas;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace System.Foundation.Modules.ModuleStations
{
    [DisplayName("模块双工位-Alone")]
    public partial class CentrifugeCapStationTool : ToolBase
    {
        public CentrifugeCapStationData centrifugeStationData = new CentrifugeCapStationData();
        public override string DefineName => "模块双工位-Alone";

        private volatile bool _isfirst = true;
        public override bool InitPins()
        {
            InsetPin("获取双位置",this,typeof(QData), PinType.Input);
            InsetPin("获取位置1", this, typeof(QData), PinType.Input);
            InsetPin("获取位置2", this, typeof(QData), PinType.Input);
            InsetPin("触发获取位置",this,typeof(QData), PinType.Input);
            InsetPin("输出位置1", this,typeof(QPosition), PinType.Output);
            InsetPin("输出位置2", this, typeof(QPosition), PinType.Output);
            InsetPin("输出触发位置",this,typeof(QPosition), PinType.Output);
            return base.InitPins();
        }

        public override bool InitDataContext()
        {
            DataContext = centrifugeStationData;
            return base.InitDataContext();
        }

        public override Task<bool> ExecuteAsync(PinInfo pinInfo, QData pinData, ToolExecutionContext toolContext)
        {
            if (pinInfo.Name == "获取双位置")
            {
                var input = Context<CentrifugeCapStationData>().InputPos;
                var load = Context<CentrifugeCapStationData>().LoadPos;
                Logger.LogInformation($"输出位置1 Position:<X:{input.X} Y:{input.Y} Z:{input.Z}>");

                var pos1 = new QPosition
                {
                    X = input.X,
                    Y = input.Y,
                    Z = input.Z,
                    Z2 = input.Z2,
                    Angle = input.Angle,
                    Depth = input.Depth,
                    ZPutGetOffset = input.ZPutGetOffset
                };
                SendToPin("输出位置1", pos1);
                Logger.LogInformation($"输出位置2 Position:<X:{load.X} Y:{load.Y} Z:{load.Z}>");
                var pos2 = new QPosition
                {
                    X = load.X,
                    Y = load.Y,
                    Z = load.Z,
                    Z2 = load.Z2,
                    Angle = load.Angle,
                    Depth = load.Depth,
                    ZPutGetOffset = load.ZPutGetOffset
                };
                SendToPin("输出位置2", pos2);
                return Task.FromResult(true);
            }
            else if (pinInfo.Name == "获取位置1")
            {
                var input = Context<CentrifugeCapStationData>().InputPos;
                Logger.LogInformation($"输出位置1 Position:<X:{input.X} Y:{input.Y} Z:{input.Z}>");
                var pos1 = new QPosition
                {
                    X = input.X,
                    Y = input.Y,
                    Z = input.Z,
                    Z2 = input.Z2,
                    Angle = input.Angle,
                    Depth = input.Depth,
                    ZPutGetOffset = input.ZPutGetOffset
                };
                SendToPin("输出位置1", pos1);
                return Task.FromResult(true);
            }
            else if (pinInfo.Name == "获取位置2")
            {
                var load = Context<CentrifugeCapStationData>().LoadPos;
                Logger.LogInformation($"输出位置2 Position:<X:{load.X} Y:{load.Y} Z:{load.Z}>");
                var pos2 = new QPosition
                {
                    X = load.X,
                    Y = load.Y,
                    Z = load.Z,
                    Z2 = load.Z2,
                    Angle = load.Angle,
                    Depth = load.Depth,
                    ZPutGetOffset = load.ZPutGetOffset
                };
                SendToPin("输出位置2", pos2);
                return Task.FromResult(true);
            }

            else if (pinInfo.Name == "触发获取位置")
            {
                if (_isfirst)
                {
                    _isfirst = false;
                    var input = Context<CentrifugeCapStationData>().InputPos;
                    Logger.LogInformation($"输出触发位置 Position:<X:{input.X} Y:{input.Y} Z:{input.Z}>");
                    var pos1 = new QPosition();
                    pos1.X = input.X;
                    pos1.Y = input.Y;
                    pos1.Z = input.Z;
                    pos1.Z2 = input.Z2;
                    pos1.Angle = input.Angle;
                    pos1.Depth = input.Depth;
                    pos1.ZPutGetOffset = input.ZPutGetOffset;
                    SendToPin("输出触发位置", pos1);
                    return Task.FromResult(true);
                }
                else
                {
                    _isfirst = true;
                    var load = Context<CentrifugeCapStationData>().LoadPos;
                    Logger.LogInformation($"输出触发位置 Position:<X:{load.X} Y:{load.Y} Z:{load.Z}>");
                    var pos2 = new QPosition();
                    pos2.X = load.X;
                    pos2.Y = load.Y;
                    pos2.Z = load.Z;
                    pos2.Z2 = load.Z2;
                    pos2.Angle = load.Angle;
                    pos2.Depth = load.Depth;
                    pos2.ZPutGetOffset = load.ZPutGetOffset;
                    SendToPin("输出触发位置", pos2);
                    return Task.FromResult(true);
                }
            }
            else
            {
                return Task.FromResult(false);
            }
        }
    }
}
