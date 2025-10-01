namespace QStandaedPlatform.Engine.Laboratory.Documents
{
    public enum ParameterType
    {
        INT,
        FLOAT,
        DINT,
        STRING,
        BOOL,
    }

    public class DynamicValueParameter : ICloneable
    {
        public long ParameterId { get; set; }
        public string ParameterName { get; set; } = string.Empty;

        public string ParameterAddress { get; set; } = string.Empty;

        public string ParameterUnit { get; set; } = string.Empty;

        public string ParameterDescription { get; set; } = string.Empty;

        public ParameterType ParameterType { get; set; }

        public int CharacterLength { get; set; } = 0;

        /// <summary>
        /// ToString后的参数值 根据<see cref="ParameterType"/>类型"/>
        /// </summary>
        public string Value { get; set; } = string.Empty;

        public object Clone()
        {
            var clone = new DynamicValueParameter
            {
                ParameterName = ParameterName,
                ParameterAddress = ParameterAddress,
                ParameterUnit = ParameterUnit,
                ParameterDescription = ParameterDescription,
                ParameterType = ParameterType,
                CharacterLength = CharacterLength,
                Value = Value
            };
            return clone;
        }
    }

}



