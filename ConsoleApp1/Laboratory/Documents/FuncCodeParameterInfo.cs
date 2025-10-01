using QStandaedPlatform.Engine.Common.Common;

namespace QStandaedPlatform.Engine.Laboratory.Documents
{
    public class FuncCodeParameterInfo : ICloneable, IOpenable
    {
        public FuncCodeParameterInfo()
        {
            ParameterValueFactory["0"] = 0f;
        }
        /// <summary>
        /// 参数ID
        /// </summary>
        public long ParameterId { get; set; }
        /// <summary>
        /// 参数名称
        /// </summary>
        public string ParameterName { get; set; } = string.Empty;
        /// <summary>
        /// 参数地址
        /// </summary>
        public string ParameterAddress { get; set; } = string.Empty;

        /// <summary>
        /// 参数反馈值地址
        /// </summary>
        public string ParameterFeedbackAddress { get; set; } = string.Empty;

        /// <summary>
        /// 参数反馈的预设值
        /// </summary>
        public float ParameterFeedbackDefaultValue { get; set; } = 0f;

        /// <summary>
        /// 参数单位
        /// </summary>
        public string ParameterUnit { get; set; } = string.Empty;
        /// <summary>
        /// 参数描述
        /// </summary>
        public string ParameterDescription { get; set; } = string.Empty;
        /// <summary>
        /// 参数最小值
        /// </summary>
        public float ParameterMinValue { get; set; } = 0f;
        /// <summary>
        /// 参数最大值
        /// </summary>
        public float ParameterMaxValue { get; set; } = 0f;
        /// <summary>
        /// 参数值
        /// </summary>
        public Dictionary<string, float> ParameterValueFactory { get; set; } = [];

        public bool Openable { get; set; } = true;

        public object Clone()
        {
            var clone = new FuncCodeParameterInfo
            {
                ParameterId = SnowflakeIdGenerator.Instance.GenerateYitId(),
                ParameterName = ParameterName,
                ParameterAddress = ParameterAddress,
                ParameterUnit = ParameterUnit,
                ParameterDescription = ParameterDescription,
                ParameterMinValue = ParameterMinValue,
                ParameterMaxValue = ParameterMaxValue,
                Openable = Openable
            };
            foreach (var item in ParameterValueFactory)
            {
                clone.ParameterValueFactory[item.Key] = item.Value;
            }
            return clone;
        }
    }

}



