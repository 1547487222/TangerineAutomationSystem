namespace QStandaedPlatform.Engine.Common
{
    /// <summary>
    /// 一个对象
    /// </summary>
    public interface IObject
    {
        string Name { get; set; }

        string Description { get; set; }

        public IObject? Owner { get; set; }
    }
}
