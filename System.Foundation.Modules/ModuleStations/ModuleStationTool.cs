using Microsoft.Extensions.Logging;
using QStandaedPlatform.Engine.Common.Common;
using QStandaedPlatform.Engine.Common.Common.Metadatas;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Foundation.Modules.Models;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace System.Foundation.Modules.ModuleStations
{
    [DisplayName("模块工位_2")]
    public class ModuleStationTool : ToolBase
    {
        private QPosition _firstPos;
        private QPosition _lastPos;
        public  ModuleStationData _moduleStationData = new ModuleStationData();
        private volatile bool _isFirst = true;
        public override string DefineName => "模块工位_2";

        public override bool InitPins()
        {
            InsetPin("获取工位位置", this, typeof(QData), PinType.Input);
            InsetPin("复位模块工位",this,typeof(QData), PinType.Input);
            InsetPin("输出工位位置", this, typeof(QPosition), PinType.Output);
            InsetPin("输出工位左位置",this,typeof(QPosition), PinType.Output);
            InsetPin("输出工位左标识",this,typeof(QInt), PinType.Output);
            InsetPin("输出工位右位置",this,typeof(QPosition), PinType.Output);
            InsetPin("输出工位右标识",this,typeof(QInt), PinType.Output);
            TriggerPointCommands.Add(new TriggerPointCommand(1,"复位模块工位"));
            return true;
        }

        public override bool InitDataContext()
        {
            DataContext = _moduleStationData;
            return base.InitDataContext();
        }
        public override Task<bool> ClearEphemeralDataAsync()
        {
            _isFirst = true;
            return Task.FromResult(true);
        }
        public override Task<CommandResult> ExecuteCommandAsync(ITriggerPointCommand triggerPointCommand)
        {
            if (triggerPointCommand.Id == 1)
            {
                _isFirst = true;
            }
            return base.ExecuteCommandAsync(triggerPointCommand);
        }
        public override Task<bool> ExecuteAsync(PinInfo pinInfo, QData pinData, ToolExecutionContext toolContext)
        {
            if (pinInfo.Name == "获取工位位置")
            {
                if (_isFirst)
                {
                    Logger.LogInformation($"输出一个试管孔位 , Position:<X:{_firstPos.X} Y:{_firstPos.Y} Z:{_firstPos.Z}>");
                    var pos = new QPosition
                    {
                        X = _firstPos.X,
                        Y = _firstPos.Y,
                        Z = _firstPos.Z,
                        Z2 = _firstPos.Z2,
                        Angle = _firstPos.Angle,
                        Depth = _firstPos.Depth,
                        ZPutGetOffset = _firstPos.ZPutGetOffset
                    };
                    SendToPin("输出工位位置", pos);
                    SendToPin("输出工位左位置", pos);
                    SendToPin("输出工位左标识", (QInt)1);
                    _isFirst = false;
                }
                else
                {
                    Logger.LogInformation($"输出一个试管孔位 , Position:<X:{_lastPos.X} Y:{_lastPos.Y} Z:{_lastPos.Z}>");
                    var pos = new QPosition
                    {
                        X = _lastPos.X,
                        Y = _lastPos.Y,
                        Z = _lastPos.Z,
                        Z2 = _lastPos.Z2,
                        Angle = _lastPos.Angle,
                        Depth = _lastPos.Depth,
                        ZPutGetOffset = _lastPos.ZPutGetOffset
                    };
                    SendToPin("输出工位位置", pos);
                    SendToPin("输出工位右位置", pos);
                    SendToPin("输出工位右标识", (QInt)2);
                    _isFirst = true;
                }
                return Task.FromResult(true);
            }
            else if (pinInfo.Name == "复位模块工位")
            {
                _isFirst = true;
            }
            return Task.FromResult(true);
        }
        
        public override void ApplyOnContextChanged(object context)
        {
            _moduleStationData = Context<ModuleStationData>();
            _firstPos = _moduleStationData.StartPosition;
            _lastPos = new QPosition
            {
                X = _moduleStationData.StartPosition.X + _moduleStationData.Space,
                Y = _moduleStationData.StartPosition.Y+_moduleStationData.YSpace,
                Z = _moduleStationData.StartPosition.Z,
                Angle = _moduleStationData.StartPosition.Angle,
                Depth = _moduleStationData.StartPosition.Depth,
                Z2 = _moduleStationData.StartPosition.Z2,
                ZPutGetOffset = _moduleStationData.StartPosition.ZPutGetOffset
            };
            base.ApplyOnContextChanged(context);
        }
    }
}
