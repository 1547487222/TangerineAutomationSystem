using Microsoft.Extensions.Logging;
using System;
using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Runtime;
using System.Text;
using System.Threading.Tasks;

namespace QStandaedPlatform.Engine.Common.Common
{
    public class ToolExecuter(ToolExecutionContext toolExecutionContext) : ToolExecuterBase(toolExecutionContext)
    {
        private readonly ConcurrentQueue<PinDataTransmitEventArgs> _queue = new();


        public void Enqueue(PinDataTransmitEventArgs e)
        {

            _queue.Enqueue(e);
            if (!Running)
                _event.Set();
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
                                    if (_toolExecutionContext.Tool is ToolBase tool && await IsToolStateValid())
                                    {
                                        if (tool.Enable && await IsToolStateValid())
                                        {
                                            tool.IsProcessing = true;
                                            await tool.RaiseToolStateChangeAsync(ToolState.Running, "正在运行");
                                            var exexuteResult = false;
                                            Stopwatch stopwatch = Stopwatch.StartNew();
                                            if (await tool.EnterRequestRecvAsync())
                                            {
                                                //工具接收到数据
                                                tool.Logger?.LogInformation($"工具接收到数据：{pinDatas.TargetPin},{pinDatas.PinData}");
                                                exexuteResult = await tool.ExecuteAsync(pinDatas.TargetPin, pinDatas.PinData, _toolExecutionContext);
                                                await tool.LeaveRequestRecvAsync();
                                                tool.Logger?.LogInformation($"工具执行完成：{pinDatas.TargetPin},{pinDatas.PinData}");
                                            }
                                            stopwatch.Stop();
                                            tool.IsProcessing = false;
                                            if (exexuteResult)
                                            {
                                                _queue.TryDequeue(out _);
                                                await tool.RaiseToolStateChangeAsync(ToolState.Finish, $"{stopwatch.Elapsed.TotalMilliseconds}ms");
                                                tool.Logger?.LogInformation($"工具执行完成，耗时：{stopwatch.Elapsed.TotalMilliseconds}ms");
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
                                    if (!await _toolExecutionContext.Tool.HandleExecutedErrorAsync(ex))
                                    {
                                        await _toolExecutionContext.Flow.HandleRunErrorAsync(ex);
                                        await _toolExecutionContext.Tool.RaiseToolStateChangeAsync(ToolState.Error, ex.ToString());
                                        _toolExecutionContext.UpdateFlowState(FlowState.Error);
                                        _toolExecutionContext.Tool.Logger?.LogError($"工具执行失败:{ex}");
                                        break;
                                    }
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
                        if (!await _toolExecutionContext.Tool.HandleExecutedErrorAsync(ex))
                        {
                            await _toolExecutionContext.Tool.RaiseToolStateChangeAsync(ToolState.Error, ex.ToString());
                            await _toolExecutionContext.Flow.HandleRunErrorAsync(ex);
                            _toolExecutionContext.UpdateFlowState(FlowState.Error);
                            _toolExecutionContext.Tool.Logger?.LogError($"工具执行失败:{ex}");
                            break;
                        }
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

        public override void TryToExecuteSigal()
        {
            _event.Set();
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
