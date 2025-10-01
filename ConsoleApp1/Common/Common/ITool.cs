using QStandaedPlatform.Engine.Common.Common.Metadatas;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace QStandaedPlatform.Engine.Common.Common
{

    public delegate void PinAddedCallback(Tool ownerTool, PinInfo pinInfo);
    public delegate void PinRemovedCallback(Tool ownerTool, PinInfo pinInfo);

    public interface ITool
    {
         string DefineName { get; }

         string DisplayName { get; set; }

         Guid UniqueId { get; set; }

         QPoint ToolPosition { get; set; }

        string Description { get; }

        event Action<ToolState<ToolState>> ToolStateChange;

        event PinAddedCallback PinAddedCallback;

        event PinAddedCallback DynamicPinAddedCallback;

        event PinRemovedCallback PinRemovedCallback;

        event PinRemovedCallback DynamicPinRemovedCallback;

        object DataContext { get; }

        IReadOnlyList<PinInfo> InputPins { get; }

        IReadOnlyList<PinInfo> OutputPins { get; }
        
        List<ITriggerPointCommand> TriggerPointCommands { get; }

        Task<bool> RequestRecvHandlePinAsync(ToolExecutionContext toolContext, PinDataTransmitEventArgs pinDataTransmitEventArgs);

        Task RequestPauseAsync();
 
        Task RequestResumeAsync();

        Task RequestContinueJudgerAsync();

        bool Init();

        bool InitPins();

        bool InitStates();

        bool InitDataContext();

        bool InitEnd();

        bool UnInit();

        bool HandleContextChanged(object context, out string message);
        T Context<T>() where T : class;

        Task<CommandResult> ExecuteCommandAsync(ITriggerPointCommand flowCommand);
 
        void ApplyOnContextChanged(object context);

        DateTime CreationTime { get; set; }
    }
}
