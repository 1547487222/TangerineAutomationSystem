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
    [DisplayName("模块工位-Multi")]
    public class ModuleMultiStationTool : ToolBase
    {
        public ModuleMultiStationData moduleMultiStationData = new ModuleMultiStationData();
        private Dictionary<int, QPosition> _dict = [];
        private volatile int _currentStationId;
        public class ModuleMultiStationData
        {
            [DisplayName("开始位置")]
            [TypeConverter(typeof(ExpandableObjectConverter))]
            public QPosition StartPos { get; set; } = new QPosition();

            [DisplayName("间距")]
            public float Space { get; set; }

            [DisplayName("模块工位数")]
            public int StationNo { get; set; }
        }
        public override string DefineName => "模块工位-Multi";

        public override bool InitPins()
        {
            InsetPin("获取下一个位置",this,typeof(QData), PinType.Input);
            InsetPin("获取指定工位位置",this,typeof(QInt), PinType.Input);
            InsetPin("输出下一个位置",this,typeof(QPosition), PinType.Output);
            InsetPin("输出指定工位位置",this,typeof(QPosition), PinType.Output);
            return base.InitPins();
        }
        public override bool InitDataContext()
        {
            DataContext = moduleMultiStationData;
            return base.InitDataContext();
        }
        public override Task<bool> ClearEphemeralDataAsync()
        {
            if (_dict.Any())
                Interlocked.Exchange(ref _currentStationId, _dict.First().Key);
            return Task.FromResult(true);
        }
        public override Task<bool> ExecuteAsync(PinInfo pinInfo, QData pinData, ToolExecutionContext toolContext)
        {
            if (pinInfo.Name == "获取下一个位置")
            {
                if (_currentStationId > _dict.Last().Key)
                {
                    Interlocked.Exchange(ref _currentStationId, _dict.First().Key);
                }
                if (_dict.TryGetValue(_currentStationId, out var position))
                {
                    Logger.LogInformation($"输出下一个位置 PosIndex:{_currentStationId}, Position:<X:{position.X} Y:{position.Y} Z:{position.Z}>");
                    SendToPin("输出下一个位置", position);
                    Interlocked.Increment(ref _currentStationId);
                    return Task.FromResult(true);
                }
            }
            else if (pinInfo.Name == "获取指定工位位置")
            {
                var index = (int)pinData;
                if (_dict.TryGetValue(index, out var position))
                {
                    Logger.LogInformation($"输出指定工位位置 PosIndex:{index}, Position:<X:{position.X} Y:{position.Y} Z:{position.Z}>");
                    SendToPin("输出指定工位位置", position);
                    return Task.FromResult(true);
                }
            }
            return Task.FromResult(false);
        }


        public override bool OnHandleContextChanged(object context, out string message)
        {
            message = string.Empty;
            moduleMultiStationData = Context<ModuleMultiStationData>();
            if (moduleMultiStationData.StationNo <= 0)
            {
                message = "工位个数不能小于等于0";
                return false;
            }
            var pos= moduleMultiStationData.StartPos;
            if (pos.X == 0 || pos.Y == 0 || pos.Z == 0)
            {
                message = "工位开始位置 X或Y或Z不能等于0";
                return false;
            }
            return true;
        }
        public override void ApplyOnContextChanged(object context)
        {
            moduleMultiStationData = Context<ModuleMultiStationData>();
            var space = moduleMultiStationData.Space;
            var pos = moduleMultiStationData.StartPos;
            for (int i = 1; i < moduleMultiStationData.StationNo + 1; i++)
            {
                _dict[i] = new QPosition
                {
                    X = moduleMultiStationData.StartPos.X + space * (i - 1),
                    Y = moduleMultiStationData.StartPos.Y,
                    Z = moduleMultiStationData.StartPos.Z,
                    Z2 = moduleMultiStationData.StartPos.Z2,
                    Angle = moduleMultiStationData.StartPos.Angle,
                    Depth = moduleMultiStationData.StartPos.Depth,
                    ZPutGetOffset = moduleMultiStationData.StartPos.ZPutGetOffset
                };
            }

            if (_dict.Any())
                _currentStationId = _dict.First().Key;
        }
    }
}
