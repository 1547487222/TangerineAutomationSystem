using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Yitter.IdGenerator;

namespace QStandaedPlatform.Engine.Common.Common
{
    public class SnowflakeIdGenerator:Singleton<SnowflakeIdGenerator>
    {

        private const long Twepoch = 1288834974657L;

        private const int WorkerIdBits = 5;         // 工作节点 ID 所占位数
        private const int DatacenterIdBits = 5;     // 数据中心 ID 所占位数
        private const int SequenceBits = 12;        // 同一毫秒内的序列号位数

        private const long MaxWorkerId = ~(-1L << WorkerIdBits);      // 最大工作节点 ID
        private const long MaxDatacenterId = ~(-1L << DatacenterIdBits); // 最大数据中心 ID
        private const long SequenceMask = ~(-1L << SequenceBits);     // 序列号掩码

        private const int WorkerIdShift = SequenceBits;
        private const int DatacenterIdShift = SequenceBits + WorkerIdBits;
        private const int TimestampLeftShift = SequenceBits + WorkerIdBits + DatacenterIdBits;

        private long _lastTimestamp = -1L;
        private long _sequence = 0L;

        private readonly long _workerId = 1;
        private readonly long _datacenterId = 1;

        private readonly object _lock = new();


        protected override void Initialize()
        {
            YitIdHelper.SetIdGenerator(new IdGeneratorOptions() 
            {

            });
        }

        /// <summary>
        /// 生成一个新的唯一雪花 ID
        /// </summary>
        public long GenerateId()
        {
            lock (_lock)
            {
                long currentTimestamp = GetCurrentTimestamp();

                if (currentTimestamp < _lastTimestamp)
                    throw new InvalidOperationException("时钟回拨：无法生成 ID");

                if (currentTimestamp == _lastTimestamp)
                {
                    _sequence = (_sequence + 1) & SequenceMask;
                    if (_sequence == 0)
                        currentTimestamp = WaitUntilNextMillis(_lastTimestamp);
                }
                else
                {
                    _sequence = 0;
                }

                _lastTimestamp = currentTimestamp;

                return ((currentTimestamp - Twepoch) << TimestampLeftShift)
                       | (_datacenterId << DatacenterIdShift)
                       | (_workerId << WorkerIdShift)
                       | _sequence;
            }
        }


        public long GenerateYitId()
        {
            return YitIdHelper.NextId();
        }

        /// <summary>
        /// 获取当前时间戳（毫秒）
        /// </summary>
        private static long GetCurrentTimestamp() => DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();

        /// <summary>
        /// 等待直到下一毫秒
        /// </summary>
        private static long WaitUntilNextMillis(long lastTimestamp)
        {
            long timestamp = GetCurrentTimestamp();
            while (timestamp <= lastTimestamp)
            {
                timestamp = GetCurrentTimestamp();
            }
            return timestamp;
        }
    }

}
