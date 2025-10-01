namespace QStandaedPlatform.Engine.Laboratory.Documents
{
    public class ModuleEbrInfoItem : ICloneable
    {
        /// <summary>
        /// Ebr名称
        /// </summary>
        public string EbrName { get; set; } = string.Empty;
        /// <summary>
        /// Ebr地址
        /// </summary>
        public string EbrAddress { get; set; } = string.Empty;
        /// <summary>
        /// Ebr描述
        /// </summary>
        public string EbrDescription { get; set; } = string.Empty;

        /// <summary>
        /// Ebr类型
        /// </summary>
        public EbrType EbrType { get; set; } = EbrType.REAL;
        /// <summary>
        /// Ebr单位
        /// </summary>
        public string EbrUnit { get; set; } = string.Empty;

        /// <summary>
        /// 模块通道
        /// </summary>
        public int ModuleChannel { get; set; } = 1;

        /// <summary>
        /// Ebr字符串读取长度
        /// </summary>
        public int CharacterLength { get; set; } = 0;

        public object Clone()
        {
            var clone = new ModuleEbrInfoItem
            {
                EbrName = EbrName,
                EbrAddress = EbrAddress,
                EbrDescription = EbrDescription,
                EbrType = EbrType,
                EbrUnit = EbrUnit,
                CharacterLength = CharacterLength,
                ModuleChannel = ModuleChannel
            };
            return clone;
        }
    }

}



