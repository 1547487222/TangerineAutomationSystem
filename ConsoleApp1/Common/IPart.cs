namespace QStandaedPlatform.Engine.Common
{
    /// <summary>
    /// 组件
    /// </summary>
    public interface IPart 
    {
        void Initialize();

        event EventHandler<EventArgs> PartCreated;

        //event EventHandler<PartStatusEvent> PartStatusChanged;
        void Shutdown();

        bool IsInitialized { get; }
    }

    public enum PartStatusEvent
    {
        Initialized,
        Error,
        Working,
        Idle,
        Shutdown,
    }
}
