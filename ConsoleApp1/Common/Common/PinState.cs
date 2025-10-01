using static QStandaedPlatform.Engine.Common.Common.PinStateTable;

namespace QStandaedPlatform.Engine.Common.Common
{
    public class PinStateChangedEventArgs : EventArgs
    {
        public PinStateChangedEventArgs()
        {
            PinStates = new Stack<PinState>();
        }
        public Stack<PinState> PinStates { get; set; }
    }

    /// <summary>
    /// 表示引脚状态的类
    /// </summary>
    public class PinState
    {
        /// <summary>
        /// 初始化一个新的 PinState 实例
        /// </summary>
        /// <param name="pinInfo">引脚信息</param>
        public PinState(PinInfo pinInfo)
        {
            Pin = pinInfo ?? throw new ArgumentNullException(nameof(pinInfo));
            State = 0;
            Description = string.Empty;
        }

        /// <summary>
        /// 使用指定的引脚信息创建一个新的 PinState 实例
        /// </summary>
        /// <param name="pinInfo">引脚信息</param>
        /// <returns>新的 PinState 实例</returns>
        public static PinState CreatePinState(PinInfo pinInfo)
        {
            return new PinState(pinInfo);
        }

        /// <summary>
        /// 获取或设置引脚信息
        /// </summary>
        public PinInfo Pin { get; private set; }

        /// <summary>
        /// 获取或设置引脚状态
        /// </summary>
        public int State { get; private set; }

        /// <summary>
        /// 获取或设置引脚状态的描述
        /// </summary>
        public string Description { get; private set; }

        /// <summary>
        /// 更改引脚状态及其描述
        /// </summary>
        /// <param name="state">新的状态值</param>
        /// <param name="description">新的描述</param>
        public void Change(int state, string description)
        {
            if (string.IsNullOrWhiteSpace(description))
                throw new ArgumentException("Description cannot be null or whitespace.", nameof(description));

            State = state;
            Description = description;
        }

        public void Change(PinStateDescription pinStateDescription)
        {
            if (pinStateDescription != null)
            {
                Change(pinStateDescription.State, pinStateDescription.Description);
            }
            throw new ArgumentException($"{nameof(pinStateDescription)}is null, change error");
        }
    }



    /// <summary>
    /// 表示引脚状态表的类，用于管理引脚状态及其描述
    /// </summary>
    public class PinStateTable
    {
        public static PinStateTable Instance => Table;

        private static readonly PinStateTable Table = new PinStateTable();

        private readonly Dictionary<int, PinStateDescription> _stateDescriptions = new Dictionary<int, PinStateDescription>();

        private PinStateTable()
        {

        }

        /// <summary>
        /// 设置指定状态及其描述
        /// </summary>
        /// <param name="state">状态值</param>
        /// <param name="description">状态描述</param>
        /// <exception cref="ArgumentException">如果状态已定义，则抛出异常</exception>
        public void SetState(int state, string description)
        {
            if (string.IsNullOrWhiteSpace(description))
            {
                throw new ArgumentException("Description cannot be null or whitespace.", nameof(description));
            }

            if (_stateDescriptions.ContainsKey(state))
            {
                throw new ArgumentException($"{nameof(state)} {state} 已定义。");
            }

            _stateDescriptions[state] = new PinStateDescription { State = state, Description = description };
        }

        /// <summary>
        /// 查找指定枚举状态的描述
        /// </summary>
        /// <typeparam name="T">枚举类型</typeparam>
        /// <param name="state">要查找的状态</param>
        /// <returns>状态描述对象，如果未找到则返回 null</returns>
        public PinStateDescription? Find<T>(T state) where T : Enum
        {
            int stateValue = Convert.ToInt32(state);

            if (_stateDescriptions.TryGetValue(stateValue, out var stateDesc))
            {
                return stateDesc;
            }
            return null;
        }

        /// <summary>
        /// 表示引脚状态描述的类
        /// </summary>
        public class PinStateDescription
        {
            public int State { get; set; }

            public string Description { get; set; } = string.Empty;
        }
    }

}
