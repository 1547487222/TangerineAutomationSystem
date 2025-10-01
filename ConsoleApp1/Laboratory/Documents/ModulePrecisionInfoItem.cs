namespace QStandaedPlatform.Engine.Laboratory.Documents
{
    public class ModulePrecisionInfoItem : ICloneable
    {
        public string PrecisionName { get; set; } = string.Empty;
        public string PrecisionAddress { get; set; } = string.Empty;
        public string PrecisionDescription { get; set; } = string.Empty;
        public float PrecisionStandardValue { get; set; } = 0f;

        public object Clone()
        {
            var clone = new ModulePrecisionInfoItem
            {
                PrecisionName = PrecisionName,
                PrecisionAddress = PrecisionAddress,
                PrecisionDescription = PrecisionDescription,
                PrecisionStandardValue = PrecisionStandardValue
            };
            return clone;
        }
    }

}



