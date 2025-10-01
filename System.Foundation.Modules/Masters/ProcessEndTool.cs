using QStandaedPlatform.Engine.Common.Common;
using QStandaedPlatform.Engine.Common.Common.Metadatas;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace System.Foundation.Modules.Masters
{
    [DisplayName("流程结束标识")]
    public class ProcessEndTool : ToolBase, ITangerineConcludeAngryService
    {
        public override string DefineName => "流程结束标识";


        private CancellationTokenSource  _waitForCts = new();


        public override bool InitPins()
        {
            InsetPin("流程结束标识",this,typeof(QData), PinType.Input);
            return true;
        }

        public override Task<bool> ClearEphemeralDataAsync()
        {
            _waitForCts = new();
            return Task.FromResult(true);
        }

        public override Task<bool> ExecuteAsync(PinInfo pinInfo, QData pinData, ToolExecutionContext toolContext)
        {
            if (!_waitForCts.IsCancellationRequested)
            {
                _waitForCts.Cancel();

            }
            return Task.FromResult(true);
        }

        public async Task<ConcludeResult> WaitForConcludeAsync()
        {
            var concludeResult = new ConcludeResult();
            var signal = ToolExecutionContext.ToolStowSignal(_waitForCts.Token.WaitHandle);
            if (signal == 0)
            {
                concludeResult.IsCancel = true;
            }
            else if (signal == 1)
            {
                concludeResult.IsError = true;
                if (ToolExecutionContext.ErrorMessages.Count > 0)
                {
                    concludeResult.ErrorMessage = ToolExecutionContext.ErrorMessages.Values.OrderByDescending(p => p.recordTime).FirstOrDefault().message;
                }
                else
                {
                    concludeResult.ErrorMessage = "未知错误";
                }
            }
            else if (signal == 2)
            {
                concludeResult.IsSuccess = true;
            }
            return await Task.FromResult(concludeResult);
        }
    }
}
