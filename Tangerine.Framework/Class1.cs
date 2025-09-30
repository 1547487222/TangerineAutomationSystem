using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using System.ComponentModel;
using System.Reflection;

namespace Tangerine.Framework
{
    public interface IModuleConfig
    {
        Guid ModuleId { get; }
        Guid ModuleControllerId { get;}
        void InitlizeConfig();
    }


    /// <summary>
    /// 控制功能执行结果
    /// </summary>
    public class FunctionResult
    {
        public int Code { get; set; }
        public string Message { get; set; } = string.Empty;
        public object? Data { get; set; }
        public static FunctionResult Success(string message = "", object? data = null)
        {
            return new FunctionResult { Message = message, Data = data };
        }
        public static FunctionResult Failure(string message, int errorCode = -1)
        {
            return new FunctionResult { Code= errorCode, Message = message, };
        }
    }

    public enum DataType
    {
        String,
        Int16,
        Int32,
        Float,
        Double,
        Bool,
    }

    public interface IParameter
    {
        Guid ParameterId { get; set; }
        void InitlizeParameter();
    }
    public interface IFunctionInfo
    {
        long FunctionId { get; }
        /// <summary>
        /// 功能代码
        /// </summary>
        string FunctionCode { get; }
        /// <summary>
        /// 功能名称
        /// </summary>
        string FunctionName { get; }
        /// <summary>
        /// 功能描述
        /// </summary>
        string Description { get; }
        /// <summary>
        /// 是否需要参数
        /// </summary>
        bool HasParameters { get; }
        /// <summary>
        /// 功能参数
        /// </summary>
        object Parameters { get; }
    }
    /// <summary>
    /// 模块控制功能基础接口
    /// </summary>
    public interface IFunctionProvider
    {
        /// <summary>
        /// 模块功能信息表
        /// </summary>
        List<IFunctionInfo> FunctionInfos { get; }
        /// <summary>
        /// 功能参数配置类型
        /// </summary>
        Type ParametersOptionType { get; }
        /// <summary>
        /// 执行功能
        /// </summary>
        /// <param name="functionInfo"></param>
        /// <returns></returns>
        Task<FunctionResult> ExecuteAsync(long functionId);
        /// <summary>
        /// 验证合法参数
        /// </summary>
        /// <param name="parameters"></param>
        /// <returns></returns>
        bool ValidateParameters(object parameters);
    }

    /// <summary>
    /// 模块报警项基础接口
    /// </summary>
    public interface IModuleAlarmItem: ICloneable
    {
        /// <summary>
        /// // 报警代码
        /// </summary>
        string AlarmCode { get; }
        /// <summary>
        /// // 报警名称
        /// </summary>
        string AlarmName { get; }
        /// <summary>
        /// // 报警描述
        /// </summary>
        string Description { get; }
        /// <summary>
        /// // 报警严重程度
        /// </summary>
        AlarmSeverity Severity { get; }
        /// <summary>
        /// // 报警是否启用
        /// </summary>
        bool IsEnabled { get; set; }
        /// <summary>
        /// // 报警信息
        /// </summary>
        /// <param name="optionValue"></param>
        /// <returns></returns>
        string GetAlarmMessage(object optionValue);
        /// <summary>
        /// 报警配置
        /// </summary>
        object AlarmOption { get; }
    }
    /// <summary>
    /// 报警严重程度
    /// </summary>
    public enum AlarmSeverity
    {
        Info,       // 信息
        Warning,    // 警告
        Error,      // 错误
        Critical    // 严重
    }
    /// <summary>
    /// 统一模块配置基类
    /// </summary>
    public abstract class UniversalModuleConfig : IModuleConfig, ICloneable
    {
        public string ModuleName { get; set; } = string.Empty;
        public string ModuleKey { get; set; } = string.Empty;
        public string ModuleDescription { get; set; } = string.Empty;
        public string ModuleIdentifier { get; set; } = string.Empty;
        public string ModuleSpec { get; set; } = string.Empty;
        public string ModuleSerialNumber { get; set; } = string.Empty;
        public string ModuleVersion { get; set; } = string.Empty;

        public abstract object ModuleOption { get; set; }
        /// <summary>
        /// 控制器引用
        /// </summary>
        public Guid ModuleControllerId { get; set; }
        /// <summary>
        /// 模块报警项
        /// </summary>
        public List<IModuleAlarmItem> AlarmItems { get; set; } = [];
        /// <summary>
        /// 模块ID
        /// </summary>
        public Guid ModuleId { get; set; }

        public abstract object Clone();

        public virtual void InitlizeConfig() { }
    }

    [AttributeUsage(AttributeTargets.Class)]
    public class ModuleControllerDisplayNameAttribute(string displayName) : Attribute
    {
        public string DisplayName { get; } = displayName;
    }
    public interface IModuleProvider<TModuleConfig> where TModuleConfig : UniversalModuleConfig
    {
        string ModuleProviderName { get; }
        /// <summary>
        /// 模块列表
        /// </summary>
        List<TModuleConfig> Configs { get; }
        /// <summary>
        /// 模块配置
        /// </summary>
        /// <see cref="Configs"/>
        /// <returns></returns>
        void ConfigureModule();

        bool ConfigureService(IServiceCollection services);

        bool ServiceProvider(IServiceProvider serviceProvider);

        object OptionImport();

        IPart Structurer(object option);

        IFunctionProvider BuildFunctionProvider(object moduleController);

        Type ModuleControllerType { get; }

        Type ModuleAlarmOptionType { get; }
        Type ModuleOptionType { get; }

        Type ControllerType { get; }
    }

    public interface IPart
    {
        void Initialize();

        event EventHandler<EventArgs> PartCreated;
        void Shutdown();
        bool IsInitialized { get; }
        T Part<T>();
    }


    /// <summary>
    /// 模块提供器管理器
    /// </summary>
    public interface IModuleProviderManager
    {
        IReadOnlyList<IModuleProviderInfo> AvailableProviders { get; }
        void RegisterProvider<TConfig>(IModuleProvider<TConfig> provider) where TConfig : UniversalModuleConfig;
        void UnregisterProvider(string providerName);
        IModuleProvider<TConfig> GetProvider<TConfig>(string providerName) where TConfig : UniversalModuleConfig;
        IModuleProviderInfo GetProviderInfo(string providerName);
    }

    /// <summary>
    /// 模块提供器信息
    /// </summary>
    public interface IModuleProviderInfo
    {
        string ProviderName { get; }
        string DisplayName { get; }
        string Description { get; }
        Type ConfigType { get; }
        Type ControllerType { get; }
        Type FunctionProviderType { get; }
        object CreateDefaultConfig();
        IPart CreateController(object config);
        IFunctionProvider CreateFunctionProvider(IPart controller);
    }

    /// <summary>
    /// 模块实例管理器
    /// </summary>
    public interface IModuleInstanceManager
    {
        IReadOnlyList<IModuleInstance> Instances { get; }
        IModuleInstance CreateInstance(string providerName, string instanceName, object controllerConfig, object moduleConfig);
        bool RemoveInstance(string instanceId);
        IModuleInstance GetInstance(string instanceId);
        Task<bool> InitializeInstanceAsync(string instanceId);
        Task<bool> ShutdownInstanceAsync(string instanceId);
    }

    /// <summary>
    /// 模块实例
    /// </summary>
    public interface IModuleInstance
    {
        string InstanceId { get; }
        string InstanceName { get; }
        IModuleProviderInfo ProviderInfo { get; }
        object ControllerConfig { get; }
        object ModuleConfig { get; }
        IPart Controller { get; }
        IFunctionProvider FunctionProvider { get; }
        ModuleStatus Status { get; }

        Task<bool> InitializeAsync();
        Task<bool> ShutdownAsync();
    }

    public enum ModuleStatus
    {
        NotInitialized,
        Initializing,
        Running,
        Error,
        Disposed
    }

    /// <summary>
    /// 模块提供器管理器实现
    /// </summary>
    public class ModuleProviderManager : IModuleProviderManager
    {
        private readonly Dictionary<string, IModuleProviderInfo> _providers = [];
        private readonly IServiceProvider _serviceProvider;

        public ModuleProviderManager(IServiceProvider serviceProvider)
        {
            _serviceProvider = serviceProvider;
        }

        public IReadOnlyList<IModuleProviderInfo> AvailableProviders => [.. _providers.Values];

        public void RegisterProvider<TConfig>(IModuleProvider<TConfig> provider) where TConfig : UniversalModuleConfig
        {
            var providerInfo = new ModuleProviderInfo<TConfig>(provider, _serviceProvider);
            _providers[provider.ModuleProviderName] = providerInfo;
        }

        public void UnregisterProvider(string providerName)
        {
            _providers.Remove(providerName);
        }

        public IModuleProvider<TConfig> GetProvider<TConfig>(string providerName) where TConfig : UniversalModuleConfig
        {
            if (_providers.TryGetValue(providerName, out var providerInfo) &&
                providerInfo is ModuleProviderInfo<TConfig> typedProvider)
            {
                return typedProvider.Provider;
            }
            throw new KeyNotFoundException($"Provider '{providerName}' not found");
        }

        public IModuleProviderInfo GetProviderInfo(string providerName)
        {
            return _providers.TryGetValue(providerName, out var providerInfo)
                ? providerInfo
                : throw new KeyNotFoundException($"Provider '{providerName}' not found");
        }
    }

    /// <summary>
    /// 模块提供器信息实现
    /// </summary>
    public class ModuleProviderInfo<TConfig> : IModuleProviderInfo where TConfig : UniversalModuleConfig
    {
        private readonly IServiceProvider _serviceProvider;

        public ModuleProviderInfo(IModuleProvider<TConfig> provider, IServiceProvider serviceProvider)
        {
            Provider = provider;
            _serviceProvider = serviceProvider;
        }

        public IModuleProvider<TConfig> Provider { get; }

        public string ProviderName => Provider.ModuleProviderName;
        public string DisplayName => Provider.GetType().GetCustomAttribute<DisplayNameAttribute>()?.DisplayName ?? ProviderName;
        public string Description => Provider.GetType().GetCustomAttribute<DescriptionAttribute>()?.Description ?? string.Empty;
        public Type ConfigType => typeof(TConfig);
        public Type ControllerType => Provider.ModuleControllerType;
        public Type FunctionProviderType => Provider.GetType().GetMethod("BuildFunctionProvider")?.ReturnType ?? typeof(IFunctionProvider);

        public object CreateDefaultConfig()
        {
            // 创建默认的模块配置
            return Activator.CreateInstance<TConfig>();
        }

        public IPart CreateController(object config)
        {
            // 使用提供器的Structurer方法创建控制器
            var structurerMethod = Provider.GetType().GetMethod("Structurer");
            if (structurerMethod != null)
            {
                var part = structurerMethod.Invoke(Provider, [config]) as IPart;
                return part ?? throw new InvalidOperationException($"Provider {ProviderName} does not return a valid controller instance ");
            }
            throw new InvalidOperationException($"Provider {ProviderName} does not implement Structurer method");
        }

        public IFunctionProvider CreateFunctionProvider(IPart controller)
        {
            return Provider.BuildFunctionProvider(controller);
        }
    }

    /// <summary>
    /// 模块实例管理器实现
    /// </summary>
    public class ModuleInstanceManager : IModuleInstanceManager
    {
        private readonly Dictionary<string, IModuleInstance> _instances = new();
        private readonly IModuleProviderManager _providerManager;
        private readonly ILogger<ModuleInstanceManager> _logger;

        public ModuleInstanceManager(IModuleProviderManager providerManager, ILogger<ModuleInstanceManager> logger)
        {
            _providerManager = providerManager;
            _logger = logger;
        }

        public IReadOnlyList<IModuleInstance> Instances => [.. _instances.Values];

        public IModuleInstance CreateInstance(string providerName, string instanceName, object controllerConfig, object moduleConfig)
        {
            var providerInfo = _providerManager.GetProviderInfo(providerName);

            // 创建控制器
            var controller = providerInfo.CreateController(controllerConfig);

            // 创建功能提供器
            var functionProvider = providerInfo.CreateFunctionProvider(controller);

            var instance = new ModuleInstance(
                Guid.NewGuid().ToString(),
                instanceName,
                providerInfo,
                controllerConfig,
                moduleConfig,
                controller,
                functionProvider
            );

            _instances[instance.InstanceId] = instance;
            _logger.LogInformation("Created module instance: {InstanceName} using {ProviderName}", instanceName, providerName);

            return instance;
        }

        public bool RemoveInstance(string instanceId)
        {
            if (_instances.TryGetValue(instanceId, out var instance))
            {
                instance.ShutdownAsync().Wait(5000);
                _instances.Remove(instanceId);
                _logger.LogInformation("Removed module instance: {InstanceName}", instance.InstanceName);
                return true;
            }
            return false;
        }

        public IModuleInstance GetInstance(string instanceId)
        {
            return (_instances.TryGetValue(instanceId, out var instance) ? instance : null)?? throw new InvalidOperationException($"Module instance {instanceId} not found ");
        }

        public async Task<bool> InitializeInstanceAsync(string instanceId)
        {
            var instance = GetInstance(instanceId);
            if (instance != null)
            {
                return await instance.InitializeAsync();
            }
            return false;
        }

        public async Task<bool> ShutdownInstanceAsync(string instanceId)
        {
            var instance = GetInstance(instanceId);
            if (instance != null)
            {
                return await instance.ShutdownAsync();
            }
            return false;
        }
    }

    /// <summary>
    /// 模块实例实现
    /// </summary>
    public class ModuleInstance : IModuleInstance
    {
        public string InstanceId { get; }
        public string InstanceName { get; }
        public IModuleProviderInfo ProviderInfo { get; }
        public object ControllerConfig { get; }
        public object ModuleConfig { get; }
        public IPart Controller { get; }
        public IFunctionProvider FunctionProvider { get; }
        public ModuleStatus Status { get; private set; } = ModuleStatus.NotInitialized;

        public ModuleInstance(string instanceId, string instanceName, IModuleProviderInfo providerInfo,
                             object controllerConfig, object moduleConfig, IPart controller,
                             IFunctionProvider functionProvider)
        {
            InstanceId = instanceId;
            InstanceName = instanceName;
            ProviderInfo = providerInfo;
            ControllerConfig = controllerConfig;
            ModuleConfig = moduleConfig;
            Controller = controller;
            FunctionProvider = functionProvider;
        }

        public async Task<bool> InitializeAsync()
        {
            if (Status != ModuleStatus.NotInitialized)
                return await Task.FromResult(false);

            Status = ModuleStatus.Initializing;
            try
            {
                Controller.Initialize();
                Status = ModuleStatus.Running;
                return await Task.FromResult(true);
            }
            catch (Exception ex)
            {
                Status = ModuleStatus.Error;
                throw new ModuleInitializationException($"Failed to initialize module '{InstanceName}'", ex);
            }
        }

        public async Task<bool> ShutdownAsync()
        {
            await Task.CompletedTask;
            if (Status != ModuleStatus.Running)
                return false;

            try
            {
                Controller.Shutdown();
                Status = ModuleStatus.NotInitialized;
                return true;
            }
            catch (Exception ex)
            {
                Status = ModuleStatus.Error;
                throw new ModuleShutdownException($"Failed to shutdown module '{InstanceName}'", ex);
            }
        }
    }


    [Serializable]
    internal class ModuleShutdownException : Exception
    {
        public ModuleShutdownException()
        {
        }

        public ModuleShutdownException(string? message) : base(message)
        {
        }

        public ModuleShutdownException(string? message, Exception? innerException) : base(message, innerException)
        {
        }
    }

    [Serializable]
    internal class ModuleInitializationException : Exception
    {
        public ModuleInitializationException()
        {
        }

        public ModuleInitializationException(string? message) : base(message)
        {
        }

        public ModuleInitializationException(string? message, Exception? innerException) : base(message, innerException)
        {
        }
    }
}
