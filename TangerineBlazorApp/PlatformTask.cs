using System.Collections.Concurrent;
using System.Threading.Channels;

namespace TangerineBlazorApp
{
    public class TaskInfos
    {
        public List<string> SampleWells { get; set; } = [];
    }
    public class PlatformTask:IDisposable
    {
        private readonly CancellationTokenSource _cts = new CancellationTokenSource();
        private volatile bool isRunning = false;
        public string PlatformName { get; set; }

        public Func<TaskInfos, Task> Task { get; set; }

        public bool IsFirstTask { get; set; } = false;

        public Channel<TaskInfos> InChannel { get; set; } = Channel.CreateUnbounded<TaskInfos>();

        public Channel<TaskInfos> OutChannel { get; set; } = Channel.CreateUnbounded<TaskInfos>();

        public bool IsRunning => isRunning;

        public SemaphoreSlim CompleteSlim { get; set; }

        private volatile int taskCount = 0;
        public async Task RunAsync()
        {
            while (await InChannel.Reader.WaitToReadAsync(_cts.Token))
            {
                if (InChannel.Reader.TryRead(out var taskInfo))
                {
                    if (taskCount != 0 && !IsFirstTask)
                    {
                        if (CompleteSlim != null)
                        {
                            await CompleteSlim.WaitAsync();
                        }
                    }
                    isRunning = true;
                    await Task(taskInfo);
                    await OutChannel.Writer.WriteAsync(taskInfo);
                    isRunning = false;
                    taskCount++;
                }
            }
        }

        public async Task SendAsync(TaskInfos taskInfos)
        {
            await InChannel.Writer.WriteAsync(taskInfos);
        }

        public void Dispose() 
        {
            _cts.Cancel();
        }
    }
}
