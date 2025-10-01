using QStandaedPlatform.Engine.Common.Common.Metadatas;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace QStandaedPlatform.Engine.Common.Common
{
    public abstract class ToolExecuterBase:BackgroundWork
    {
        protected readonly ToolExecutionContext _toolExecutionContext;
        protected readonly AutoResetEvent _event = new(false);
        private volatile bool _running = false;
        protected ToolExecuterBase(ToolExecutionContext toolExecutionContext)
        {
            _toolExecutionContext = toolExecutionContext;
            _toolExecutionContext.TryToExecute += ToolExecutionContext_TryToExecute;
        }

        private void ToolExecutionContext_TryToExecute(object? sender, EventArgs e)
        {
            TryToExecuteSigal();
        }

        public virtual void TryToExecuteSigal()
        {
            if (!Running)
            {
                _ = _event.Set();
            }
        }
        public override void Cancel()
        {
            _ = _event.Set();
        }

        public async Task<bool> IsToolStateValid() => await _toolExecutionContext.VerifyToolStateAsync();


        public static Dictionary<PinInfo, QData> PreparePinData(Dictionary<PinInfo, PinDataTransmitEventArgs> pinDatas)
        {
            Dictionary<PinInfo, QData> pinData = [];
            foreach (var data in pinDatas)
            {
                pinData.Add(data.Key, data.Value.PinData);
            }
            return pinData;
        }

        public abstract void ClearData();


        public bool Running
        {
            get { return _running; }

            set { _running = value; }
        }
    }
}
