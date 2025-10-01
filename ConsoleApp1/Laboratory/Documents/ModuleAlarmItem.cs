namespace QStandaedPlatform.Engine.Laboratory.Documents
{
    public class ModuleAlarmItem : ICloneable
    {
        public string AlarmAddress { get; set; } = string.Empty;

        public string AlarmDescription { get; set; } = string.Empty;

        public object Clone()
        {
            ModuleAlarmItem clone = new()
            {
                AlarmAddress = AlarmAddress,
                AlarmDescription = AlarmDescription
            };
            return clone;
        }
    }

}



