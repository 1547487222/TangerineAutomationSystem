using QStandaedPlatform.Engine.Components;
using QStandaedPlatform.Engine.Components.Modbus;
using QStandaedPlatform.Engine.Laboratory;
using QStandaedPlatform.Engine.Laboratory.Documents;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;

namespace QStandaedPlatform.Engine.Common.Common
{
    public interface IModuleTool
    {
        Modular GetModular();
    }


    public interface IArmModuleTool
    {
        public RoboticArmTaskScheduler ArmTaskScheduler { get; }
    }


    public abstract class ArmModuleToolBase : ModuleWithParameterToolBase, IArmModuleTool
    {
        public RoboticArmTaskScheduler ArmTaskScheduler { get; private set; }

        public RoboticArmTaskScheduler GetArmTaskScheduler()
        {
            if (ArmTaskScheduler == null)
                throw new Exception("ArmTaskScheduler is null");
            return ArmTaskScheduler;
        }

        //public override void OnRefPartPropertyInstalled(IRefPartProperty part)
        //{
        //    base.OnRefPartPropertyInstalled(part);
        //    if (Modular != null)
        //    {
        //        ArmTaskScheduler = RoboticArmTaskSchedulerManager.Instance.GetOrAddScheduler(GetModular().Messenger);
        //    }
        //}

        public Task ArmScheduleTaskAsync(Func<Modular, Task> action, RoboticArmTaskPriority priority)
        {
            return ArmTaskScheduler.SubmitTaskAsync(this.GetModular(), action, priority);
        }

        public override async Task<bool> OnHandleRequestCancelResetAsync()
        {
            await base.OnHandleRequestCancelResetAsync();
            ArmTaskScheduler?.NotifyTaskArrived();
            return true;
        }

        public override async Task<bool> ClearEphemeralDataAsync()
        {
            await base.ClearEphemeralDataAsync();
            ArmTaskScheduler?.ClearPendingTasks();
            return true;
        }
    }

    public abstract class SyncInputArmModuleToolBase : SyncInputModuleWithParameterToolBase, IArmModuleTool
    {
        public RoboticArmTaskScheduler ArmTaskScheduler { get;private set; }

        public RoboticArmTaskScheduler GetArmTaskScheduler()
        {
            if (ArmTaskScheduler == null)
                throw new Exception("ArmTaskScheduler is null");
            return ArmTaskScheduler;
        }
        //public override void OnRefPartPropertyInstalled(IRefPartProperty part)
        //{
        //    base.OnRefPartPropertyInstalled(part);
        //    if (Modular != null)
        //    {
        //        ArmTaskScheduler = RoboticArmTaskSchedulerManager.Instance.GetOrAddScheduler(GetModular().Messenger);
        //    }
        //}


        public Task ArmScheduleTaskAsync(Func<Modular,Task> action,RoboticArmTaskPriority priority)
        {
           return ArmTaskScheduler.SubmitTaskAsync(this.GetModular(), action, priority);
        }



        public override async Task<bool> OnHandleRequestCancelResetAsync()
        {
            await base.OnHandleRequestCancelResetAsync();
            ArmTaskScheduler?.NotifyTaskArrived();
            return true;
        }

        public override async Task<bool> ClearEphemeralDataAsync()
        {
            await base.ClearEphemeralDataAsync();
            ArmTaskScheduler?.ClearPendingTasks();
            return true;
        }
    }



    public interface IModuleWithParameterTool
    {
        ModuleFuncCodeParameter ModuleFuncCodeParameter { get; }
    }
    public class ModuleData
    {

    }

    public abstract class ModuleWithParameterToolBase : ModuleToolBase, IModuleWithParameterTool
    {
       
        [RefParameter<ModuleFuncCodeTable>]
        public ModuleFuncCodeParameter ModuleFuncCodeParameter { get; set; }

        public override void OnRefParameterPropertyInstalled(IRefParameterProperty parameter)
        {
            if (ModuleFuncCodeParameter != null && Modular != null)
            {
                if (ModuleFuncCodeParameter.ModuleInfoParameter != null)
                {
                    Modular.SetModuleInfo(ModuleFuncCodeParameter.ModuleInfoParameter);
                }
                Modular.SetModuleFuncCodeParameter(ModuleFuncCodeParameter);
            }
        }

        public override void OnRefPartPropertyInstalled(IRefPartProperty part)
        {
            base.OnRefPartPropertyInstalled(part);
            if ( Modular != null && ModuleFuncCodeParameter != null)
            {
                if (ModuleFuncCodeParameter.ModuleInfoParameter != null)
                {
                    Modular.SetModuleInfo(ModuleFuncCodeParameter.ModuleInfoParameter);
                }
                Modular.SetModuleFuncCodeParameter(ModuleFuncCodeParameter);
            }
        }
    }
    public abstract class ModuleToolBase:ToolBase, IModuleTool
    {
        private readonly object syncRoot = new();
        public override bool Init()
        {
            TriggerPointCommands.Add(new TriggerPointCommand(1000, "模块取消"));
            TriggerPointCommands.Add(new TriggerPointCommand(1001, "模块复位"));
            return true;
        }

        public override async Task<CommandResult> ExecuteCommandAsync(ITriggerPointCommand triggerPointCommand)
        {
            if (triggerPointCommand.Id == 1000)
            {
                await OnHandleRequestCancelAsync();
            }
            else if (triggerPointCommand.Id == 1001)
            {
                await OnHandleRequestCancelResetAsync();
            }
            return CommandResult.Ok(this.DisplayName);
        }
        public Modular? Modular { internal get; set; }


        [ReferencePart]
        public IH5uTcp? h5UTcp { get; set; }

        public override async Task<bool> HandleExecutedErrorAsync(Exception toolexception)
        {
            if (toolexception is ModularException modularException)
            {
                ModularAlarmService.Instance.PostAlarm(GetModular(), modularException);
               return await HandleExecutedModuleErrorAsync(modularException);
            }
            else
                return false;
        }


        public new ModuleData DataContext { get; set; }

        public override T Context<T>()
        {
            if (DataContext is T context)
                return context;
            throw new InvalidOperationException("DataContext is not of type " + typeof(T).Name);
        }

        public override async Task RequestPauseAsync()
        {
            await GetModular().PauseAsync();
        }
        public override async Task RequestResumeAsync()
        {
            await GetModular().AutoAndStartAsync();
        }

        public override async Task<bool> OnHandleRequestCancelAsync()
        {
            return await GetModular().EmergencyStopAsync();
        }



        public override async Task<bool> OnHandleRequestCancelResetAsync()
        {
            await GetModular().ResetAsync();
            await GetModular().ResetEmergencyStopAsync();
            if (await GetModular().VerifyInitAsync())
            {
                await GetModular().AutoAndStartAsync();
            }
            return true;
        }

        public override async Task<bool> ClearEphemeralDataAsync()
        {
            await GetModular().ClearControlAndRunStateAsync();
            ToolExecutionContext.ClearPinCache();
            return await base.ClearEphemeralDataAsync();
        }
        public Modular GetModular()
        {
            lock (syncRoot)
            {
                if (Modular == null)
                    throw new ModularException(DisplayName + ": " + "Modular is null");
                return Modular;
            }
        }

        public override void OnRefPartPropertyInstalled(IRefPartProperty part)
        {
            if (part.RefPart is IH5uTcp h5UTcp)
            {
                Modular = new Modular(h5UTcp);
            }
        }
        public virtual Task<bool> HandleExecutedModuleErrorAsync(ModularException modularException)=>Task.FromResult(false);
    }

    public abstract class SyncInputModuleWithParameterToolBase : SyncInputModuleToolBase, IModuleWithParameterTool
    {
        [RefParameter<ModuleFuncCodeTable>]
        public ModuleFuncCodeParameter ModuleFuncCodeParameter { get; set; }
        public override void OnRefParameterPropertyInstalled(IRefParameterProperty parameter)
        {
            if (ModuleFuncCodeParameter != null && Modular != null)
            {
                if (ModuleFuncCodeParameter.ModuleInfoParameter != null)
                {
                    Modular.SetModuleInfo(ModuleFuncCodeParameter.ModuleInfoParameter);
                }
                Modular.SetModuleFuncCodeParameter(ModuleFuncCodeParameter);
            }
        }

        public override void OnRefPartPropertyInstalled(IRefPartProperty part)
        {
            base.OnRefPartPropertyInstalled(part);
            if (Modular != null && ModuleFuncCodeParameter != null)
            {
                if (ModuleFuncCodeParameter.ModuleInfoParameter != null)
                {
                    Modular.SetModuleInfo(ModuleFuncCodeParameter.ModuleInfoParameter);
                }
                Modular.SetModuleFuncCodeParameter(ModuleFuncCodeParameter);
            }
        }
    }
    public abstract class SyncInputModuleToolBase : SyncInputToolBase, IModuleTool
    {
        private readonly object syncRoot = new();
        public override bool Init()
        {
            TriggerPointCommands.Add(new TriggerPointCommand(1000, "模块取消"));
            TriggerPointCommands.Add(new TriggerPointCommand(1001,"模块复位"));
            return true;
        }
        public new ModuleData DataContext { get; set; }

        public override T Context<T>()
        {
            if (DataContext is T context)
                return context;
            throw new InvalidOperationException("DataContext is not of type " + typeof(T).Name);
        }
        public override async Task<CommandResult> ExecuteCommandAsync(ITriggerPointCommand triggerPointCommand)
        {
            if (triggerPointCommand.Id == 1000)
            {
                await OnHandleRequestCancelAsync();
            }
            else if (triggerPointCommand.Id == 1001)
            {
                await OnHandleRequestCancelResetAsync();
            }
            return CommandResult.Ok(this.DisplayName);
        }
        public Modular?  Modular {internal get; set; }


        [ReferencePart]
        public IH5uTcp? h5UTcp { get; set; }
        public override async Task<bool> HandleExecutedErrorAsync(Exception toolexception)
        {
            if (toolexception is ModularException modularException)
            {
                ModularAlarmService.Instance.PostAlarm(GetModular(),modularException);
                return await HandleExecutedModuleErrorAsync(modularException);
            }
            else
                return false;
        }
        public override async Task RequestPauseAsync()
        {
            await GetModular().PauseAsync();
        }
        public override async Task RequestResumeAsync()
        {
            await GetModular().AutoAndStartAsync();
        }

        public override async Task<bool> OnHandleRequestCancelAsync()
        {
            return await GetModular().EmergencyStopAsync();
        }

        public override async Task<bool> OnHandleRequestCancelResetAsync()
        {
            await GetModular().ResetAsync();
            await GetModular().ResetEmergencyStopAsync();
            if (await GetModular().VerifyInitAsync())
            {
                await GetModular().AutoAndStartAsync();
            }
            return true;
        }

        public override async Task<bool> ClearEphemeralDataAsync()
        {
            await GetModular().ClearControlAndRunStateAsync();
            this.ClearCacheData();
            ToolExecutionContext.ClearPinCache();
            return await base.ClearEphemeralDataAsync();
        }
        public Modular GetModular()
        {
            lock (syncRoot)
            {
                if (Modular == null)
                    throw new ModularException(DisplayName + ": " + "Modular is null");
                return Modular;
            }
        }

        public override void OnRefPartPropertyInstalled(IRefPartProperty part)
        {
            if (part.RefPart is IH5uTcp h5UTcp)
            {
                Modular = new Modular(h5UTcp);
            }
        }
        public virtual Task<bool> HandleExecutedModuleErrorAsync(ModularException modularException) => Task.FromResult(false);
    }
}
