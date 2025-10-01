using Microsoft.Extensions.Logging;
using QStandaedPlatform.Engine.Common;
using QStandaedPlatform.Engine.Common.Common;
using QStandaedPlatform.Engine.Common.Common.Metadatas;
using QStandaedPlatform.Engine.Laboratory.Documents;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace QStandaedPlatform.Engine.Laboratory
{

    public interface ICallStatus
    {
        /// <summary>
        /// 调用状态
        /// </summary>
         int Code { get;}//Code==0表示成功,其他表示失败
        /// <summary>
        /// 调用状态描述
        /// </summary>
         string Message { get; } 
    }


    public interface IInitializable
    {
        Task<IModuleCallStatus> InitializeAsync();
    }

    public interface IControllable
    {
        Task<IModuleCallStatus> StartAsync();
        Task<IModuleCallStatus> StopAsync();
    }

    public interface IPausable
    {
        Task<IModuleCallStatus> PauseAsync();
        Task<IModuleCallStatus> ResumeAsync();
    }

    public interface IResettable
    {
        Task<IModuleCallStatus> ResetAsync();
    }

    public interface IModuleCallStatus : ICallStatus
    {
        long ModuleId { get; }
    }

    public class ModuleCallStatus : IModuleCallStatus
    {
        public long ModuleId { get; set; }

        public string ModuleName { get; set; } = string.Empty;

        public int Code { get; set; }

        public string Message { get; set; } = string.Empty;

        public static IModuleCallStatus Success(long moduleId)
        {
            return new ModuleCallStatus() { ModuleId = moduleId, Code = 0, Message = "Success" };
        }
        public static IModuleCallStatus Fail(long moduleId, string message)
        {
            return new ModuleCallStatus() { ModuleId = moduleId, Code = -1, Message = message };
        }
    }

    public abstract class ModuleBase : TreeNode
    {

    }
    public interface IModuleStatus
    {
        
    }

    /// <summary>
    /// H5u扫码器
    /// </summary>
    public class H5uScanner : ModuleBase
    {
        /// <summary>
        /// H5u读码
        /// </summary>
        /// <returns></returns>
        public Task<string> ReadCodeAsync()
        {
            return Task.FromResult("123456");
        }
    }

    /// <summary>
    /// 模块
    /// </summary>
    public class H5uModule : ModuleBase, IInitializable, IControllable, IPausable, IResettable
    {

        public IModuleStatus ModuleStatus => throw new NotImplementedException();

        public Task<IModuleCallStatus> InitializeAsync()
        {
            throw new NotImplementedException();
        }

        public Task<IModuleCallStatus> PauseAsync()
        {
            throw new NotImplementedException();
        }

        public Task<IModuleCallStatus> PreStartTaskAsync()
        {
            throw new NotImplementedException();
        }

        public Task<IModuleCallStatus> ResetAsync()
        {
            throw new NotImplementedException();
        }

        public Task<IModuleCallStatus> ResumeAsync()
        {
            throw new NotImplementedException();
        }

        public Task<IModuleCallStatus> StartAsync()
        {
            throw new NotImplementedException();
        }

        public Task<IModuleCallStatus> StopAsync()
        {
            throw new NotImplementedException();
        }
    }

    public interface IArmController:ITreeNode,IInitializable, IControllable, IPausable, IResettable
    {

    }
    public class ArmH5uModule : H5uModule, IArmController
    {
        
    }


    public interface IResource
    {
        long ResourceId { get; }
        string ResourceDescription { get; }
    }


    public interface ITreeNode: IObject,IMarker<long>
    {
        bool CanAddChild(ITreeNode treeNode);
        void AddChild(ITreeNode treeNode);

        void RemoveChild(ITreeNode treeNode);

        bool CanAddResource(IResource resource);
        void AddResource(IResource resource);

        void RemoveResource(IResource resource);

        List<ITreeNode> Children { get; }
        List<IResource> Resources { get; }

        event EventHandler? AddChildNodeEvent;

        event EventHandler? RemoveChildNodeEvent;

        event EventHandler? AddResourceEvent;

        event EventHandler? RemoveResourceEvent;
        
    }

    public interface ILoggerObject
    {
        /// <summary>
        /// 
        /// </summary>
        ILogger? Logger { get; }
    }

    public class TreeNode : ITreeNode, ILoggerObject
    {
        private readonly List<ITreeNode> _children = [];
        private readonly List<IResource> _resources = [];
        public long Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public IObject? Owner { get; set; }
        public object? Tag { get; set; }
        public ILogger? Logger { get; set; }
        public List<IResource> Resources => _resources;
        public List<ITreeNode> Children => _children;

        public event EventHandler? AddChildNodeEvent;
        public event EventHandler? RemoveChildNodeEvent;
        public event EventHandler? AddResourceEvent;
        public event EventHandler? RemoveResourceEvent;

        public virtual void InitializeTag() { }

        public T GetTag<T>()
        {
            if (Tag is T tag)
            {
                return tag;
            }
            throw new Exception($"Tag is not {typeof(T).Name}");
        }

        public virtual bool CanAddChild(ITreeNode treeNode) 
        {
            return true;
        }
        public virtual bool CanAddResource(IResource resource)
        {
            return true;
        }
        public void AddChild(ITreeNode treeNode)
        {
            if (CanAddChild(treeNode))
            {
                _children.Add(treeNode);
                treeNode.Owner = this;
                AddChildNodeEvent?.Invoke(treeNode, EventArgs.Empty);
                Logger?.LogDebug($"AddChild {treeNode.Name}");
            }
            else
            {
                Logger?.LogError($"Can not add child node {treeNode.Name}");
            }
        }

        public void RemoveChild(ITreeNode treeNode)
        {
            _children.Remove(treeNode);
            treeNode.Owner = null;
            RemoveChildNodeEvent?.Invoke(treeNode, EventArgs.Empty);
            Logger?.LogDebug($"RemoveChild {treeNode.Name}");
        }

        public void AddResource(IResource resource)
        {
            if (CanAddResource(resource))
            {
                _resources.Add(resource);
                AddResourceEvent?.Invoke(resource, EventArgs.Empty);
                Logger?.LogDebug($"AddResource {resource.GetType().Name}");
            }
            else
            {
                Logger?.LogError($"Can not add resource {resource.ResourceDescription}");
            }
        }

        public void RemoveResource(IResource resource)
        {
            _resources.Remove(resource);
            RemoveResourceEvent?.Invoke(resource, EventArgs.Empty);
            Logger?.LogDebug($"RemoveResource {resource.ResourceDescription}");
        }
    }

    [AttributeUsage(AttributeTargets.Method)]
    public class ClickMethodAttribute(string name) : Attribute
    {
        public string Name { get; } = name;
    }

    public class PlatformTag
    {
        /// <summary>
        /// 平台名称
        /// </summary>
        public string PlatformName { get; set; } = string.Empty;

        /// <summary>
        /// 平台编号
        /// </summary>
        public string PlatformCode { get; set; } = string.Empty;

        /// <summary>
        /// 平台描述
        /// </summary>
        public string PlatformDescription { get; set; } = string.Empty;

        /// <summary>
        /// 平台进样通量
        /// </summary>
        public int PlatformSamplingFlux { get; set; } = 2;

        /// <summary>
        /// 平台最大执行组
        /// </summary>
        public int PlatformMaxExecuteCount { get; set; } = 2;

        /// <summary>
        /// 平台最大缓存数
        /// </summary>
        public int PlatformMaxCacheCount { get; set; } = 2;
    }

    public class Platform : TreeNode
    {
        /// <summary>
        /// 平台ID
        /// </summary>
        public long PlatformId { get; set; }

        public override void InitializeTag()
        {
            Tag = new PlatformTag();
        }
        public string PlatformName => GetTag<PlatformTag>().PlatformName;

        public string PlatformCode => GetTag<PlatformTag>().PlatformCode;

        public string PlatformDescription => GetTag<PlatformTag>().PlatformDescription;

        public int PlatformSamplingFlux => GetTag<PlatformTag>().PlatformSamplingFlux;

        public int PlatformMaxExecuteCount => GetTag<PlatformTag>().PlatformMaxExecuteCount;

        public int PlatformMaxCacheCount => GetTag<PlatformTag>().PlatformMaxCacheCount;

        /// <summary>
        /// 平台任务集合
        /// </summary>

        public List<PlatformTask> Tasks { get; set; } = [];


        public override bool CanAddChild(ITreeNode treeNode)
        {
            return treeNode is H5uModule;
        }

        public override bool CanAddResource(IResource resource)
        {
            return resource is ILabTray or IModuleChnelGroup;
        }

        /// <summary>
        /// 平台模块集合
        /// </summary>
        public List<H5uModule> Modules => [.. Children.OfType<H5uModule>()];
        /// <summary>
        /// 平台托盘集合
        /// </summary>
        public List<ILabTray> LabTrays => [.. Resources.OfType<ILabTray>()];
        /// <summary>
        /// 平台通道组集合
        /// </summary>
        public List<IModuleChnelGroup> ModuleChnelGroups => [.. Resources.OfType<IModuleChnelGroup>()];

        /// <summary>
        /// 初始化平台
        /// </summary>
        /// <returns></returns>
        [ClickMethod("初始化")]
        public async Task<PlatformCallStatus> InitializeAsync()
        {
            Logger?.LogDebug($"{PlatformId} {PlatformName} Enter InitializeAsync ");
            try
            {
                var arms = Modules.Where(m => m is IArmController).Cast<IArmController>();
                foreach (var arm in arms)
                {
                    await arm.InitializeAsync();
                }
                var initializables = Modules.Where(m => m is IInitializable).DistinctBy(p => arms.Any(x => x.Id == p.Id)).Cast<IInitializable>();

                foreach (var initializable in initializables)
                {
                    await initializable.InitializeAsync();
                }
                Logger?.LogDebug($"InitializeAsync {PlatformId} {PlatformName} success ");
                return PlatformCallStatus.Success(PlatformId, PlatformName);
            }
            catch (Exception ex) when (ex is ModularException modularException)
            {
                Logger?.LogError($"InitializeAsync {PlatformId} {PlatformName} failed {ex}");
                return PlatformCallStatus.Failed(PlatformId, PlatformName, PlatformException.
                    Create(PlatformId
                    , PlatformName
                    , modularException.Message
                    , modularException.Action
                    , modularException.ModuleErrorCode
                    , modularException.Message));
            }
            catch (Exception ex)
            {
                Logger?.LogError($"InitializeAsync {PlatformId} {PlatformName} failed {ex}");
                return PlatformCallStatus.Failed(PlatformId, PlatformName, PlatformException.Create(PlatformId, PlatformName, "未知模块", "InitializeAsync", -1, ex.Message));
            }
        }

        /// <summary>
        /// 启动
        /// </summary>
        /// <returns></returns>
        [ClickMethod("启动")]
        public async Task<PlatformCallStatus> StartAsync()
        {
            Logger?.LogDebug($"{PlatformId} {PlatformName} Enter StartAsync ");
            try
            {
                foreach (var module in Modules)
                {
                    if (module is IControllable controllable)
                    {
                        await controllable.StartAsync();
                    }
                }
                Logger?.LogDebug($"StartAsync {PlatformId} {PlatformName} success ");
                return PlatformCallStatus.Success(PlatformId, PlatformName);
            }
            catch (Exception ex) when (ex is ModularException modularException)
            {
                Logger?.LogError($"StartAsync {PlatformId} {PlatformName} failed {ex}");
                return PlatformCallStatus.Failed(PlatformId, PlatformName, PlatformException.Create(PlatformId, PlatformName, modularException.Message, modularException.Action, modularException.ModuleErrorCode, modularException.Message));
            }
            catch (Exception ex)
            {
                Logger?.LogError($"StartAsync {PlatformId} {PlatformName} failed {ex}");
                return PlatformCallStatus.Failed(PlatformId, PlatformName, PlatformException.Create(PlatformId, PlatformName, "未知模块", "StartAsync", -1, ex.Message));
            }
        }


        /// <summary>
        /// 停止
        /// </summary>
        /// <returns></returns>
        [ClickMethod("停止")]
        public async Task<PlatformCallStatus> StopAsync()
        {
            Logger?.LogDebug($"{PlatformId} {PlatformName} Enter StopAsync ");
            try
            {
                foreach (var module in Modules)
                {
                    if (module is IControllable controllable)
                    {
                        await controllable.StopAsync();
                    }
                }
                Logger?.LogDebug($"StopAsync {PlatformId} {PlatformName} success ");
                return PlatformCallStatus.Success(PlatformId, PlatformName);
            }
            catch (Exception ex) when (ex is ModularException modularException)
            {
                Logger?.LogError($"StopAsync {PlatformId} {PlatformName} failed {ex}");
                return PlatformCallStatus.Failed(PlatformId, PlatformName, PlatformException.Create(PlatformId, PlatformName, modularException.Message, modularException.Action, modularException.ModuleErrorCode, modularException.Message));
            }
            catch (Exception ex)
            {
                Logger?.LogError($"StopAsync {PlatformId} {PlatformName} failed {ex}");
                return PlatformCallStatus.Failed(PlatformId, PlatformName, PlatformException.Create(PlatformId, PlatformName, "未知模块", "StopAsync", -1, ex.Message));
            }
        }



        //运行平台流程任务

    }


    public class PlatformTask : TreeNode
    {

    }

    public class TransferModule : H5uModule
    {

    }

    public interface IPlatformResource : IResource
    {

    }

    public interface ILabTray : IPlatformResource
    {
        
    }

    public interface IModuleChnelGroup : IPlatformResource
    {
        
    }

    /// <summary>
    /// 平台返回调用状态
    /// </summary>
    public class PlatformCallStatus : ICallStatus
    {
        public long PlatformId { get; set; }
        public string PlatformName { get; set; } = string.Empty;
        public int Code { get; set; }
        public string Message { get; set; } = string.Empty;
        public PlatformException? Exception { get; set; }

        public static PlatformCallStatus Success(long platformId, string platformName, string message = "")
        {
            return new PlatformCallStatus
            {
                PlatformId = platformId,
                PlatformName = platformName,
                Code = 0,
                Message = message
            };
        }

        public static PlatformCallStatus Failed(long platformId, string platformName, PlatformException exception)
        {
            return new PlatformCallStatus
            {
                PlatformId = platformId,
                PlatformName = platformName,
                Code = -1,
                Message = exception.Message,
                Exception = exception
            };
        }
    }

    /// <summary>
    /// 结构化异常信息
    /// </summary>
    public class PlatformException(string errorMessage) : Exception(errorMessage)
    {

        /// <summary>
        /// 平台ID
        /// </summary>
        public long PlatformId { get; set; }
        /// <summary>
        /// 平台名称
        /// </summary>
        public string PlatformName { get; set; } = string.Empty;
        /// <summary>
        /// 软件本身异常划分为未知模块
        /// </summary>
        public string ModuleName { get; set; } = string.Empty;
        /// <summary>
        /// 动作
        /// </summary>
        public string Action { get; set; } = string.Empty;
        /// <summary>
        /// 错误码
        /// </summary>
        public int ErrorCode { get; set; }
        /// <summary>
        /// 错误信息
        /// </summary>
        public string ErrorMessage { get; } = errorMessage;


        public static PlatformException Create(long platformId, string platformName, string moduleName, string action, int errorCode, string errorMessage)
        {
            return new PlatformException(errorMessage)
            {
                PlatformId = platformId,
                PlatformName = platformName,
                ModuleName = moduleName,
                Action = action,
                ErrorCode = errorCode,
            };
        }
    }



    public class ModuleCategory
    {
        /// <summary>
        /// 模块类别描述
        /// </summary>
        public string CategoryDescription { get; set; } = string.Empty;
        /// <summary>
        /// 模块类名
        /// </summary>
        public string ModuleFullName { get; set; } = string.Empty;
        /// <summary>
        /// 模块功能提供器名称
        /// </summary>
        public string ModuleFuncionProvider { get; set; } = string.Empty;
    }

    public class Entity<T> where T : unmanaged
    {
        public T Id { get; set; }
    }

    public class ModuleInfo: Entity<long>
    {
        /// <summary>
        /// 模块类别
        /// </summary>
        public ModuleCategory Category { get; set; } = new ModuleCategory();
        /// <summary>
        /// 模块名称
        /// </summary>
        public string ModuleName { get; set; } = string.Empty;
        /// <summary>
        /// 模块描述
        /// </summary>
        public string ModuleDescription { get; set; } = string.Empty;
        /// <summary>
        /// 成员信息
        /// </summary>
        public object MemberInfo { get; set; } = new object();
   }


    public class ModuleFunctionInfo:Entity<long>
    {
        /// <summary>
        /// 模块信息
        /// </summary>
        public ModuleInfo? ModuleInfo { get; set; }
        /// <summary>
        /// 模块ID
        /// </summary>
        public long ModuleId { get; set; }
        /// <summary>
        /// 功能名称
        /// </summary>
        public string FunctionName { get; set; } = string.Empty;
        /// <summary>
        /// 功能描述
        /// </summary>
        public string FunctionDescription { get; set; } = string.Empty;
        /// <summary>
        /// 功能信息
        /// </summary>
        public object FunctionInfo { get; set; } = new object();
    }
}
