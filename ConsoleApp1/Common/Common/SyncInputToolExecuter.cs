using Microsoft.Extensions.Logging;
using QStandaedPlatform.Engine.Common.Common.Metadatas;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace QStandaedPlatform.Engine.Common.Common
{
    public class SyncInputToolExecuter(ToolExecutionContext toolExecutionContext) : ToolExecuterBase(toolExecutionContext)
    {
        private readonly ConcurrentQueue<Dictionary<PinInfo, PinDataTransmitEventArgs>> _queue = new();

        public void Enqueue(Dictionary<PinInfo, PinDataTransmitEventArgs> pinDatas)
        {
            _queue.Enqueue(pinDatas);
            if (!Running)
            {
                _ = _event.Set();
            }
        }


        public override async Task DoWorkAsync(CancellationToken cancellationToken)
        {
            while (!TokenSource.IsCancellationRequested)
            {
                var num = WaitHandle.WaitAny([_event, cancellationToken.WaitHandle]);

                if (num == 0)
                {
                    if (Running)
                    {
                        await _toolExecutionContext.Tool.RaiseToolStateChangeAsync(ToolState.SyncWaiting, $"工具处理排队中：{_queue.Count}");
                        continue;
                    }
                    else
                    {
                        Running = true;
                    }
                    try
                    {
                        while (await IsToolStateValid() && !_queue.IsEmpty && !cancellationToken.IsCancellationRequested)
                        {
                            if (!_queue.IsEmpty && _queue.TryPeek(out var pinDatas) && await IsToolStateValid())
                            {
                                try
                                {
                                    if (_toolExecutionContext.Tool is SyncInputToolBase tool && await IsToolStateValid())
                                    {
                                        Dictionary<PinInfo, QData> pinData = PreparePinData(pinDatas);
                                        if (tool.Enable && await IsToolStateValid())
                                        {
                                            tool.IsProcessing = true;
                                            await tool.RaiseToolStateChangeAsync(ToolState.Running, "正在运行");
                                            var exexuteResult = false;
                                            Stopwatch stopwatch = Stopwatch.StartNew();
                                            if (await tool.EnterRequestRecvAsync())
                                            {
                                                exexuteResult = await tool.ExexuteAsync(pinData, _toolExecutionContext);
                                                await tool.LeaveRequestRecvAsync();
                                            }
                                            stopwatch.Stop();
                                            tool.IsProcessing = false;
                                            if (exexuteResult)
                                            {

                                                _queue.TryDequeue(out _);

                                                await tool.RaiseToolStateChangeAsync(ToolState.Finish, $"{stopwatch.Elapsed.TotalMilliseconds}ms");
                                                if (_toolExecutionContext.Flow.OnToolTaskCompletedCallbask != null)
                                                {
                                                    await _toolExecutionContext.Flow.OnToolTaskCompletedCallbask(tool);
                                                }
                                            }
                                            else
                                            {
                                                await tool.RaiseToolStateChangeAsync(ToolState.Error, "未知失败,请检查log");
                                                _toolExecutionContext.UpdateFlowState(FlowState.Error);
                                                break;
                                            }
                                        }
                                        else
                                        {
                                            await tool.RaiseToolStateChangeAsync(ToolState.Forbidden, "工具已禁用");
                                        }
                                    }
                                }
                                catch (Exception ex)
                                {
                                    await _toolExecutionContext.Tool.RaiseToolStateChangeAsync(ToolState.Error, ex.ToString());
                                    _toolExecutionContext.UpdateFlowState(FlowState.Error);
                                    _toolExecutionContext.Tool.Logger?.LogInformation($"工具执行失败:{ex}");
                                    break;
                                }
                                if (!_queue.IsEmpty && await IsToolStateValid())
                                {
                                    continue;
                                }
                                else
                                {
                                    break;
                                }
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        await _toolExecutionContext.Tool.RaiseToolStateChangeAsync(ToolState.Error, ex.ToString());
                        _toolExecutionContext.UpdateFlowState(FlowState.Error);
                        _toolExecutionContext.Tool.Logger?.LogInformation($"工具执行失败:{ex}");
                    }
                    finally
                    {
                        Running = false;
                    }
                }
                else if (num == 1)
                {
                    break;
                }
            }
        }

        public override void ClearData()
        {
            lock (_queue)
            {
                _queue.Clear();
            }
        }
    }
}
