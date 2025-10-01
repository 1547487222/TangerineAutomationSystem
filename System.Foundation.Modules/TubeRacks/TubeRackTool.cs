using Microsoft.Extensions.Logging;
using QStandaedPlatform.Engine.Common.Common;
using QStandaedPlatform.Engine.Common.Common.Metadatas;
using QStandaedPlatform.Engine.Components;
using QStandaedPlatform.Engine.Laboratory;
using System.Collections.Generic;
using System.ComponentModel;
using System.Foundation.Modules.Models;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace System.Foundation.Modules.TubeRacks
{
    [DisplayName("试管架")]
    public class TubeRackTool : ToolBase
    {
        private volatile int _currentWellId;
        private Dictionary<int, Well> _wellMap;

        private const string _takeTubeofOne = "取出一个试管孔位";
        private const string _takeTubeofOneByLabel = "输入标签取出一个试管孔位";
        private const string _resetWellId = "重置试管架孔位Id";
        private const string _outputWell = "输出一个试管孔位";
        private const string outputSampleName = "输出样品";
        //输出一组试管
        private const string _outputTubeGroup = "输出一组试管孔位";

        public override string DefineName => "试管架";

        public string TrayModel => Context<TubeRackData>().RackName;


        public int SampleGroupCount => Context<TubeRackData>().OutputSampleGroup;

        public override bool InitPins()
        {
            InsetPin(_takeTubeofOne, this,typeof(QData), PinType.Input);
            InsetPin(_takeTubeofOneByLabel, this,typeof(QString),PinType.Input);
            InsetPin(_resetWellId, this,typeof(QData), PinType.Input);
            InsetPin(_outputWell, this, typeof(QPosition), PinType.Output);
            InsetPin(outputSampleName, this, typeof(QSample), PinType.Output);


            TriggerPointCommands.Add(new TriggerPointCommand(1, _resetWellId));
            TriggerPointCommands.Add(new TriggerPointCommand(2, _outputTubeGroup));
            return true;
        }
        public override bool InitDataContext()
        {
            DataContext = new TubeRackData();
            return true;
        }
        public override Task<bool> ClearEphemeralDataAsync()
        {
            if (_wellMap.Count != 0)
            {
                Interlocked.Exchange(ref _currentWellId, _wellMap.First().Key);
            }
            return Task.FromResult(true);
        }
        public override Task<CommandResult> ExecuteCommandAsync(ITriggerPointCommand triggerPointCommand)
        {
            if (triggerPointCommand.Id == 1)
            {
                Interlocked.Exchange(ref _currentWellId, _wellMap.First().Key);
            }
            return base.ExecuteCommandAsync(triggerPointCommand);
        }
        public override  Task<bool> ExecuteAsync(PinInfo pinInfo, QData pinData, ToolExecutionContext toolContext)
        {
            if (pinInfo.Name == _takeTubeofOne)
            {
                if (_wellMap != null && _wellMap.Count != 0)
                {
                    if (_currentWellId > _wellMap.Last().Key)
                    {
                        Interlocked.Exchange(ref _currentWellId, _wellMap.First().Key);
                    }
                    if (_wellMap.TryGetValue(_currentWellId, out var well))
                    {
                        var pos = new QPosition
                        {
                            X = well.Pos.X,
                            Y = well.Pos.Y,
                            Z = well.Pos.Z,
                            Z2 = well.Pos.Z2,
                            Angle = well.Pos.Angle,
                            Depth = well.Pos.Depth,
                            ZPutGetOffset = well.Pos.ZPutGetOffset
                        };
                        SendToPin(_outputWell, pos);
                        Interlocked.Increment(ref _currentWellId);
                        return Task.FromResult(true);
                    }
                }
            }
            else if (pinInfo.Name == _takeTubeofOneByLabel)
            {
                if (_wellMap != null && _wellMap.Count != 0)
                {
                    if (_wellMap.Values.Any(p => p.WellName == (string)pinData))
                    {
                        var well = _wellMap.Values.First(p => p.WellName == (string)pinData);
                        var pos = new QPosition
                        {
                            X = well.Pos.X,
                            Y = well.Pos.Y,
                            Z = well.Pos.Z,
                            Z2 = well.Pos.Z2,
                            Angle = well.Pos.Angle,
                            Depth = well.Pos.Depth,
                            ZPutGetOffset = well.Pos.ZPutGetOffset
                        };
                        SendToPin(_outputWell, pos);
                        return Task.FromResult(true);
                    }
                }
            }
            else if (pinInfo.Name == _resetWellId)
            {
                Interlocked.Exchange(ref _currentWellId, _wellMap.First().Key);
            }
            return Task.FromResult(true);
        }

        public override bool OnHandleContextChanged(object context, out string message)
        {
            message = string.Empty;
            var tubeRackData = Context<TubeRackData>();
             if (tubeRackData.Rows == 0 || tubeRackData.Cols == 0)
            {
                message = "列数或行数不能为0";
                return false;
            }
            else if (tubeRackData.Cols != tubeRackData.ColLable.Split(",").Length)
            {
                message = "列数和列标签长度不相等";
                return false;
            }
            else if (tubeRackData.Rows != tubeRackData.RowLable.Split(",").Length)
            {
                message = "行数和行标签长度不相等";
                return false;
            }
            return true;
        }
        public override void ApplyOnContextChanged(object context)
        {
            var wells = WellCalculator.CalculateWells(Context<TubeRackData>());
            if (wells.Count != 0)
            {
                _wellMap = wells.ToDictionary(k => k.WellId, v => v);
                _currentWellId = wells.First().WellId;
            }
        }
    }
}
