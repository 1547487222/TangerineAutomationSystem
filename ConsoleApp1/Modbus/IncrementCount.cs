using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

namespace QStandaedPlatform.Engine.Modbus
{
    /// <summary>
    /// 自增计数
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public class IncrementCount<T>(T startValue) where T : IIncrementOperators<T>, IMinMaxValue<T>, IComparisonOperators<T, T, bool>
    {
        private T _value = startValue;
        private readonly T _startValue = startValue;
        private readonly object _lock = new();

        public IncrementCount() : this(T.MinValue) { }

        public T GetNextValue()
        {
            lock (_lock)
            {
                if (++_value < T.MaxValue)
                    return _value;
                _value = _startValue;
                return _value;
            }
        }

        public T GetCurrentValue()
        {
            lock (_lock)
            {
                return _value;
            }
        }

        public void Reset()
        {
            lock (_lock)
            {
                _value = _startValue;
            }
        }
    }
}
