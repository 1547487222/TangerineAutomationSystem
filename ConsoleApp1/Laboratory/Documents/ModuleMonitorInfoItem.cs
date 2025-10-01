namespace QStandaedPlatform.Engine.Laboratory.Documents
{
    public class ModuleMonitorInfoItem : ICloneable
    {
        public string MonitorName { get; set; } = string.Empty;
        public string MonitorAddress { get; set; } = string.Empty;
        public string MonitorUnit { get; set; } = string.Empty;
        public ReadType MonitorType { get; set; }
        public string MonitorDescription { get; set; } = string.Empty;

        public object Clone()
        {
            var clone = new ModuleMonitorInfoItem
            {
                MonitorName = MonitorName,
                MonitorAddress = MonitorAddress,
                MonitorDescription = MonitorDescription,
                MonitorUnit = MonitorUnit,
                MonitorType = MonitorType
            };
            return clone;
        }
    }

}



