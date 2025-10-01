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
    [DisplayName("模块工位架参数")]
    public class ModuleRackStationData
    {
        /// <summary>
        /// 列数
        /// </summary>
        [DisplayName("列数")]
        public int Cols { get; set; }
        /// <summary>
        /// 行数
        /// </summary>
        [DisplayName("行数")]
        public int Rows { get; set; }
        /// <summary>
        /// 行间距
        /// </summary>
        [DisplayName("Y方向间距")]
        public float SpaceRow { get; set; }
        /// <summary>
        /// 列间距
        /// </summary>
        [DisplayName("X方向间距")]
        public float SpaceCol { get; set; }

        [DisplayName("开始位置")]
        [TypeConverter(typeof(ExpandableObjectConverter))]
        public QPosition StartPosition { get; set; } = new QPosition();

        [DisplayName("行优先")]
        public bool Rowfirst { get; set; } = true;

    }
    /// <summary>
    /// 模块架工位
    /// </summary>
    [DisplayName("模块工位架")]
    public class ModuleRackStationTool : ToolBase
    {
        private const string TriggerTakePosPinName = "触发取出模块位置";
        private const string OutputTrggerTakePosPinName = "输出触发取出模块位置";

        private readonly Dictionary<int,QPosition> _positions = new();
        private volatile int _currentPosition;
        public override string DefineName => "模块工位架";


        public override bool InitDataContext()
        {
            DataContext = new ModuleRackStationData();
            return true;
        }

        public override bool InitPins()
        {
            InsetPin(TriggerTakePosPinName, this,typeof(QData), PinType.Input);
            InsetPin(OutputTrggerTakePosPinName, this, typeof(QPosition), PinType.Output);
            return true;
        }
        public override Task<bool> ClearEphemeralDataAsync()
        {
            if (_positions.Count != 0)
                Interlocked.Exchange(ref _currentPosition, _positions.First().Key);
            return Task.FromResult(true);
        }
        public override Task<bool> ExecuteAsync(PinInfo pinInfo, QData pinData, ToolExecutionContext toolContext)
        {
            if (pinInfo.Name == TriggerTakePosPinName)
            {
                if (_positions != null && _positions.Count != 0)
                {
                    if (_currentPosition > _positions.Last().Key)
                    {
                        Interlocked.Exchange(ref _currentPosition, _positions.First().Key);
                    }
                    if (_positions.TryGetValue(_currentPosition, out var qPosition))
                    {
                        Logger.LogInformation($"输出一个试管孔位 PosIndex:{_currentPosition}, Position:<X:{qPosition.X} Y:{qPosition.Y} Z:{qPosition.Z}>");
                        SendToPin(OutputTrggerTakePosPinName, qPosition);
                        Interlocked.Increment(ref _currentPosition);
                        return Task.FromResult(true);
                    }
                }
            }
            return Task.FromResult(false);
        }

        public override void ApplyOnContextChanged(object context)
        {
            var datas = ModuleRackCalculator.ModuleStationPosCalculates(Context<ModuleRackStationData>());
            if (datas != null && datas.Count != 0)
            {
                if (_positions.Count > 0)
                {
                    _positions.Clear();
                }
                for (int i = 0; i < datas.Count; i++)
                {
                    _positions.Add(i, datas[i]);
                }
                if (_positions.Count > 0)
                {
                    _currentPosition = _positions.Keys.First();
                }
            }
        }
    }
}
