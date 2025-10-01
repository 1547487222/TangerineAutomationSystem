
using AntDesign;
using Grpc.Core;
using Newtonsoft.Json;
using Smart_Lab_OS;
using System.Threading;
using TangerineBlazorApp.Models;

namespace TangerineBlazorApp
{
    public class PlatformScheduleService
    {
        private volatile int scheduleNumber = 60;
        private volatile string[] rowLabels = ["A", "B", "C", "D", "E", "F", "G", "H", "I", "J", "K"];
        private volatile string[] colLabels = ["1", "2", "3", "4", "5", "6"];

        private List<PlatformTask>  platformTasks = [];


        private readonly Smart_Lab_OS.SmartLabOSSever.SmartLabOSSeverClient _client;
        public PlatformScheduleService( Smart_Lab_OS.SmartLabOSSever.SmartLabOSSeverClient client)
        {
            _client = client;
        }
        public void ConfigScheduleNumber( string[] rowLabels, string[] colLabels)
        {
            this.rowLabels = rowLabels;
            this.colLabels = colLabels;
        }
        public event Action<PlatformStateInfo>? PlatformStateChanged;

        public event Action<string> ? PlatformMessageNotification;



        public async Task<List<FlowConfig>> GetFlowConfigsAsync()
        {
            var flowConfigString = await _client.GetFlowConfigsAsync(new Google.Protobuf.WellKnownTypes.Empty());
            var flowConfigs = JsonConvert.DeserializeObject<List<FlowConfig>>(flowConfigString.Json);
            return flowConfigs ?? [];
        }

        public async Task SetFlowConfigsAsync(List<FlowConfig> flowConfigs)
        {
            var json = JsonConvert.SerializeObject(flowConfigs);
            await _client.SetFlowConfigsAsync(new Smart_Lab_OS.FlowConfigString { Json = json });
            PlatformMessageNotification?.Invoke("参数更新成功！");
        }

        
        public async Task InitPlatformAsync()
        {
           var asyncServerStreamingCall = _client.Home(new Smart_Lab_OS.HomeRequest());
            try
            {
                while (await asyncServerStreamingCall.ResponseStream.MoveNext())
                {
                    Console.WriteLine($"Received: {asyncServerStreamingCall.ResponseStream.Current.StepInfo}");
                    if (asyncServerStreamingCall.ResponseStream.Current.StepNo == -300)
                    {
                        PlatformMessageNotification?.Invoke("平台初始化中,请勿重复操作");
                    }
                    else if (asyncServerStreamingCall.ResponseStream.Current.StepNo == 99999)
                    {
                        PlatformMessageNotification?.Invoke("平台初始化完成");
                    }
                    else
                    {
                        PlatformStateChanged?.Invoke(new PlatformStateInfo 
                        {
                             Message = asyncServerStreamingCall.ResponseStream.Current.StepInfo,
                             PlatformName= asyncServerStreamingCall.ResponseStream.Current.PlatformName,
                             State= TimelineDotColor.Green
                        });
                    }
                }
            }
            catch (OperationCanceledException)
            {
                PlatformMessageNotification?.Invoke("Stream was canceled by user.");
            }
            catch (RpcException ex) when (ex.StatusCode == StatusCode.Cancelled)
            {
                PlatformMessageNotification?.Invoke("Stream was cancelled.");
            }
            catch (Exception ex)
            {
                PlatformMessageNotification?.Invoke($"Error: {ex.Message}");
            }
        }
        /// <summary>
        /// 开始任务
        /// </summary>
        /// <returns></returns>
        public async Task StartPlatformTaskAsync()
        {
            var configs = await GetFlowConfigsAsync();
            if (platformTasks.Count > 0)
            {
                foreach (var item in platformTasks)
                {
                    item.Dispose();
                }
            }
            platformTasks.Clear();
            for (int i = 0; i < configs.Count; i++)
            {
                var config = configs[i];
                PlatformTask platformTask = new()
                {
                    PlatformName = config.FlowName,
                    Task = new Func<TaskInfos, Task>(async taskinfo =>
                    {
                        TaskInfo taskInfo = new();
                        foreach (var item in taskinfo.SampleWells)
                        {
                            taskInfo.SampleWellName.Add(item);
                        }
                        var asyncServerStreamingCall = _client.StartTask(new Smart_Lab_OS.StartTaskRequest
                        {
                            PlatformName = config.FlowName,
                            TaskInfo = taskInfo
                        });
                        while (await asyncServerStreamingCall.ResponseStream.MoveNext())
                        {
                            Console.WriteLine($"Received: {asyncServerStreamingCall.ResponseStream.Current.StepInfo}");
                            if (asyncServerStreamingCall.ResponseStream.Current.StepNo == 99999)
                            {
                                Console.WriteLine(config.FlowName + ":" + "Task finished");
                                PlatformMessageNotification?.Invoke($"{config.FlowName},任务完成");
                            }
                            else if (asyncServerStreamingCall.ResponseStream.Current.StepNo == -100)
                            {
                                PlatformMessageNotification?.Invoke($"{config.FlowName},{asyncServerStreamingCall.ResponseStream.Current.StepInfo}");
                            }
                            else
                            {
                                PlatformStateChanged?.Invoke(new PlatformStateInfo
                                {
                                    Message = asyncServerStreamingCall.ResponseStream.Current.StepInfo,
                                    PlatformName = asyncServerStreamingCall.ResponseStream.Current.PlatformName,
                                    State = TimelineDotColor.Green
                                });
                            }
                        }
                    })
                };
                platformTasks.Add(platformTask);
            }

            if (platformTasks.Count > 1)
            {
                for (int i = 0; i < platformTasks.Count - 1; i++)
                {
                    var current = platformTasks[i];
                    var next = platformTasks[i + 1];
                    current.CompleteSlim = new SemaphoreSlim(0, 1);
                    if (i == 0)
                    {
                        current.IsFirstTask = true;
                    }
                    if (i + 1 == platformTasks.Count - 1)
                    {
                        next.CompleteSlim = null;
                    }
                    _ = Task.Run(async () =>
                    {
                        await foreach (var item in current.OutChannel.Reader.ReadAllAsync(CancellationToken.None))
                        {
                            while (next.IsRunning)
                            {
                                await Task.Delay(100);
                            }
                            await next.SendAsync(item);

                            if (current.CompleteSlim != null)
                            {
                                if (current.CompleteSlim.CurrentCount == 0)
                                {
                                    current.CompleteSlim.Release();
                                }
                            }
                        }
                    });
                }
            }
            foreach (var item in platformTasks)
            {
                _ = item.RunAsync();
            }

            for (int i = 0; i < rowLabels.Length; i++)
            {
                for (int j = 0; j < colLabels.Length ; j+=2)
                {
                    var taskInfo = new TaskInfos();
                    taskInfo.SampleWells.Add($"{rowLabels[i]}{colLabels[j]}");
                    taskInfo.SampleWells.Add($"{rowLabels[i]}{colLabels[j + 1]}");
                    await platformTasks[0].SendAsync(taskInfo);
                }
            }
        }
        /// <summary>
        /// 流程停止 
        /// </summary>
        /// <returns></returns>
        public async Task StopPlatformTaskAsync()
        {
            var asyncServerStreamingCall = _client.StopTask(new Smart_Lab_OS.StopTaskRequest());
            while (await asyncServerStreamingCall.ResponseStream.MoveNext())
            {
                Console.WriteLine($"Received: {asyncServerStreamingCall.ResponseStream.Current.StepInfo}");
                if (asyncServerStreamingCall.ResponseStream.Current.StepNo == 99999)
                {
                    Console.WriteLine("StopTask finished");
                    PlatformMessageNotification?.Invoke($"{asyncServerStreamingCall.ResponseStream.Current.PlatformName},StopTask.");
                }
                else if (asyncServerStreamingCall.ResponseStream.Current.StepNo == -200)
                {
                    PlatformMessageNotification?.Invoke($"{asyncServerStreamingCall.ResponseStream.Current.PlatformName},{asyncServerStreamingCall.ResponseStream.Current.StepInfo}");
                }
            }
        }


        //平台暂停
        public async Task PausePlatformTaskAsync()
        {
           await _client.PauseTaskAsync(new Smart_Lab_OS.PauseTaskRequest());
        }


        //平台继续

        public async Task ContinuePlatformTaskAsync()
        {
            await _client.ContinueTaskAsync(new Smart_Lab_OS.ContinueTaskRequest());
        }

        //平台复位
        public async Task ResetPlatformTaskAsync()
        {
            await _client.ResetAsync(new Smart_Lab_OS.ResetRequest());
        }

    }
}
