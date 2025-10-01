

namespace Tangerine.Infrastructure
{

    public class H5uModularProvider : IFuncServiceProvider
    {
        public H5uModularProvider()
        {
        }
        private readonly List<FuncDescriptor> _funcDescriptors = [];
        public async Task<FuncResult> ExecuteAsync(string funcCode, CancellationToken cts = default)
        {
            var funcDescriptor = GetFuncDescriptorByCode(funcCode);
            //TODO: 执行功能
            //如果模块包含功能，则执行功能
            //await _modular.ModuleExecuteAsync();
            //var result = await _modular.CheckModuleDoneAsync(cts);
            return new FuncResult
            {
                Success = true,
                Message = "功能执行成功",
                Exception = null,
            };
        }

        public FuncDescriptor GetFuncDescriptorByCode(string funcCode)
        {
            return _funcDescriptors.FirstOrDefault(x => x.FuncCode == funcCode)??throw new NotImplementedException();
        }

        public IReadOnlyList<FuncDescriptor> GetFuncDescriptors()
        {
            return _funcDescriptors;
        }
    }

    public interface IFuncServiceProvider
    {
        /// <summary>
        /// 获取功能描述
        /// </summary>
        /// <returns></returns>
        IReadOnlyList<FuncDescriptor> GetFuncDescriptors();

        /// <summary>
        /// 根据功能代码获取功能描述
        /// </summary>
        /// <param name="funcCode"></param>
        /// <returns></returns>
        FuncDescriptor GetFuncDescriptorByCode(string funcCode);

        /// <summary>
        /// 执行功能
        /// </summary>
        /// <param name="funcCode"></param>
        /// <param name="parameters"></param>
        /// <param name="cts"></param>
        /// <returns></returns>
        Task<FuncResult> ExecuteAsync(string funcCode, CancellationToken cts = default);
    }

    public class FuncResult
    {
        public bool Success { get; set; } = false;

        public string Message { get; set; } = string.Empty;

        public object? Data { get; set; } = null;

        public Dictionary<string, object> Extra { get; set; } = [];

        public Exception? Exception { get; set; } = null;
    }


    public class FuncDescriptor
    {
        /// <summary>
        /// 功能代码
        /// </summary>
        public string FuncCode { get; set; } = string.Empty;
        /// <summary>
        /// 功能名称
        /// </summary>
        public string FuncName { get; set; } = string.Empty;
        /// <summary>
        /// 功能描述
        /// </summary>
        public string FuncDescription { get; set; } = string.Empty;
        /// <summary>
        /// 功能参数
        /// </summary>
        public List<FuncParamDescriptor> FuncParams { get; set; } = [];
        /// <summary>
        /// 功能返回值
        /// </summary>
        public FuncResultDescriptor FuncResult { get; set; } = new FuncResultDescriptor();
    }

    public class FuncParamDescriptor
    {
        /// <summary>
        /// 参数名称
        /// </summary>
        public string ParamName { get; set; } = string.Empty;

        public object? ParamValue { get; set; } = null;
        /// <summary>
        /// 参数类型
        /// </summary>
        public TypeCode ParamType { get; set; } = TypeCode.Null;
        /// <summary>
        /// 参数描述
        /// </summary>
        public string ParamDescription { get; set; } = string.Empty;
        /// <summary>
        /// 参数默认值
        /// </summary>
        public object? DefaultValue { get; set; } = null;

        /// <summary>
        /// 是否必填参数
        /// </summary>
        public bool IsRequired { get; set; } = true;
        /// <summary>
        /// 参数单位
        /// </summary>
        public string ParamUnit { get; set; } = string.Empty;
    }

    public enum TypeCode
    {
        Null,
        Void,
        String,
        Int,
        Double,
        Bool,
        DateTime,
        Object,
        Array,
        File,
        Image,
    }

    public class FuncResultDescriptor
    {
        public string ResultName { get; set; } = string.Empty;

        public TypeCode ResultType { get; set; } = TypeCode.Void;

        public string ResultDescription { get; set; } = string.Empty;

        public string ResultUnit { get; set; } = string.Empty;

        public List<FuncResultFieldDescriptor> ResultFields { get; set; } = [];
    }

    public class FuncResultFieldDescriptor
    {
        /// <summary>
        /// 字段名称
        /// </summary>
        public string FieldName { get; set; } = string.Empty;
        /// <summary>
        /// 字段类型
        /// </summary>
        public TypeCode FieldType { get; set; } = TypeCode.Null;
        /// <summary>
        /// 字段描述
        /// </summary>
        public string FieldDescription { get; set; } = string.Empty;
        /// <summary>
        /// 字段单位
        /// </summary>
        public string FieldUnit { get; set; } = string.Empty;
    }





    public interface IWriteOnlyFuncExecutor
    {
        Task<FuncResult> WriteOnlyAsync(FuncDescriptor funcDescriptor, CancellationToken cts = default);
    }
    /// <summary>
    /// 
    /// </summary>
    public interface IReadOnlyFuncExecutor
    {
        Task<FuncResult> ReadOnlyAsync(FuncDescriptor funcDescriptor, CancellationToken cts = default);
    }

    /// <summary>
    /// 流式读取
    /// </summary>
    public interface IReadStreamFuncExecutor
    {
        IAsyncEnumerable<FuncResult> ReadStreamAsync(FuncDescriptor funcDescriptor, CancellationToken cts = default);
    }

    public interface IWriteReadFuncExecutor
    {
        Task<FuncResult> WriteReadAsync(FuncDescriptor funcDescriptor, CancellationToken cts = default);
    }

    public interface IControlFuncExecutor
    {
        Task<FuncResult> ControlAsync(FuncDescriptor funcDescriptor, CancellationToken cts = default);
    }
}
