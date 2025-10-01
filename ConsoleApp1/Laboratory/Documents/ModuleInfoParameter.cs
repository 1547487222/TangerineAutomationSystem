using QStandaedPlatform.Engine.Common.Common;

namespace QStandaedPlatform.Engine.Laboratory.Documents
{
    public class ModuleInfoParameter : IParameter
    {
        /// <summary>
        /// 模块信息id
        /// </summary>
        public Guid ModuleInfoId { get; set; }

        /// <summary>
        /// 模块控制器id
        /// </summary>
        public Guid ModuleControllerId { get; set; }

        /// <summary>
        /// 模块中文名称
        /// </summary>
        public string ModuleName { get; set; } = string.Empty;

        /// <summary>
        /// 模块英文键名
        /// </summary>
        public string ModuleKey { get; set; } = string.Empty;
        /// <summary>
        /// 模块描述
        /// </summary>
        public string ModuleDescription { get; set; } = string.Empty;
        /// <summary>
        /// 模块编号
        /// </summary>
        public string ModuleIdentifier { get; set; } = string.Empty;
        /// <summary>
        /// 模块规格
        /// </summary>
        public string ModuleSpec { get; set; } = string.Empty;

        /// <summary>
        /// 模块序列号
        /// </summary>
        public string ModuleSerialNumber { get; set; } = string.Empty;

        /// <summary>
        /// 模块版本
        /// </summary>
        public string ModuleVersion { get; set; } = string.Empty;

        /// <summary>
        /// 模块功能码地址
        /// </summary>
        public string ModuleFuncCodeAddress { get; set; } = "D210";

        /// <summary>
        /// 模块功能状态码地址
        /// </summary>
        public string ModuleFuncStateCodeAddress { get; set; } = "D200";

        /// <summary>
        /// 模块状态地址
        /// </summary>
        public string ModuleStateAddress { get; set; } = "D300";

        /// <summary>
        /// 模块参数地址
        /// </summary>
        public string ModuleParameterAddress { get; set; } = "D100";
        /// <summary>
        /// 模块观察数据地址
        /// </summary>
        public string ModuleObserveDataAddress { get; set; } = "D7800";
        /// <summary>
        /// 模块回原控制地址
        /// </summary>
        public string ModuleHomeControlAddress { get; set; } = "M0";

        /// <summary>
        /// 模块回原状态地址
        /// </summary>
        public string ModuleHomeStateAddress { get; set; } = "D0";

        /// <summary>
        /// 模块复位控制地址
        /// </summary>
        public string ModuleResetControlAddress { get; set; } = "M101";

        /// <summary>
        /// 模块停止控制地址
        /// </summary>
        public string ModuleStopControlAddress { get; set; } = "M102";

        /// <summary>
        /// 模块急停状态地址
        /// </summary>
        public string ModuleEmergencyControlAddress { get; set; } = "M102";

        /// <summary>
        /// 模块启动控制地址
        /// </summary>
        public string ModuleStartControlAddress { get; set; } = "M103";

        /// <summary>
        /// 模块暂停控制地址
        /// </summary>
        public string ModulePauseControlAddress { get; set; } = "M105";

        /// <summary>
        /// 模块手动自动控制地址
        /// </summary>
        public string ModuleManualAutoControlAddress { get; set; } = "M106";

        /// <summary>
        /// 模块初始化完成地址
        /// </summary>
        public string ModuleInitCompleteAddress { get; set; } = "M107";

        /// <summary>
        /// 模块报警地址 
        /// </summary>
        public string ModuleAlrmAddress { get; set; } = "B0";

        /// <summary>
        /// 模块报警长度
        /// </summary>
        public int ModuleAlrmAddressLength { get; set; } = 400;

        /// <summary>
        /// 扫码地址
        /// </summary>
        public string ModuleScanAddress { get; set; } = "D7850";

        /// <summary>
        /// 扫码长度
        /// </summary>
        public int ModuleScanAddressLength { get; set; } = 10;

        /// <summary>
        /// 重定向异常码
        /// </summary>
        public List<int> RedirectErrorCodes { get; set; } = [];

        /// <summary>
        /// 模块报警配置
        /// </summary>
        public List<ModuleAlarmItem> AlarmItems { get; set; } = [];

        /// <summary>
        /// 模块初始化动态值参数
        /// </summary>
        public List<DynamicValueParameter> InitializeModuleParameter { get; set; } = [];

        /// <summary>
        /// 清除功能码和清除功能状态码等待时间
        /// </summary>
        public int ClearWaitTime { get; set; } = 50;

        public Guid ParameterId { get; set; }


        public object Clone()
        {
            ModuleInfoParameter clone = new()
            {
                ModuleInfoId = Guid.NewGuid(),
                ModuleName = ModuleName,
                ModuleKey = ModuleKey,
                ModuleControllerId = ModuleControllerId,
                ModuleSerialNumber = ModuleSerialNumber,
                ModuleVersion = ModuleVersion,
                ClearWaitTime = ClearWaitTime,
                ModuleInitCompleteAddress = ModuleInitCompleteAddress,
                ModuleScanAddress = ModuleScanAddress,
                ModuleScanAddressLength = ModuleScanAddressLength,
                RedirectErrorCodes = [.. RedirectErrorCodes],
                ModuleDescription = ModuleDescription,
                ParameterId = Guid.NewGuid(),
                ModuleIdentifier = ModuleIdentifier,
                ModuleSpec = ModuleSpec,
                ModuleFuncCodeAddress = ModuleFuncCodeAddress,
                ModuleFuncStateCodeAddress = ModuleFuncStateCodeAddress,
                ModuleStateAddress = ModuleStateAddress,
                ModuleParameterAddress = ModuleParameterAddress,
                ModuleObserveDataAddress = ModuleObserveDataAddress,
                ModuleHomeControlAddress = ModuleHomeControlAddress,
                ModuleHomeStateAddress = ModuleHomeStateAddress,
                ModuleResetControlAddress = ModuleResetControlAddress,
                ModuleStopControlAddress = ModuleStopControlAddress,
                ModuleEmergencyControlAddress = ModuleEmergencyControlAddress,
                ModuleStartControlAddress = ModuleStartControlAddress,
                ModulePauseControlAddress = ModulePauseControlAddress,
                ModuleManualAutoControlAddress = ModuleManualAutoControlAddress,
                ModuleAlrmAddress = ModuleAlrmAddress,
                ModuleAlrmAddressLength = ModuleAlrmAddressLength,
                InitializeModuleParameter = [.. InitializeModuleParameter.Select(x => (DynamicValueParameter)x.Clone())],
                AlarmItems = [.. AlarmItems.Select(x => (ModuleAlarmItem)x.Clone())]
            };
            return clone;
        }

        public void InitlizeParameter()
        {
           
        }
    }

}



