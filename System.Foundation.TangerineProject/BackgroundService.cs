namespace TangerineSerivce
{
    public abstract class BackgroundService
    {
        protected abstract Task ExecuteAsync(CancellationToken stoppingToken);
    }
}