using Microsoft.Extensions.Logging;
using QStandaedPlatform.Engine.Common.Common.SampleEntitys;
using QStandaedPlatform.Engine.Components;
using QStandaedPlatform.Engine.Laboratory;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace QStandaedPlatform.Engine.Common.Common
{
    public class SampleService
    {
        private readonly Dictionary<long,SampleTaskInfo>
            _dicSampleTaskInfo = [];
        private readonly Dictionary<long,SampleTraceEntity>
            _dicSampleTraceEntity = [];
        private readonly Dictionary<long, ISampleStow>
            _dicSampleStow = [];
        private readonly object _locker = new();

        private readonly Dictionary<long,Action<SampleTraceEntity>>
            _dicSampleTraceReceiveAction = [];
        private readonly Dictionary<long,Action<SampleTaskInfo>>
            _dicSampleCompleteAction = [];

        private readonly ILogger<SampleService> _logger;

        private readonly MangoStorage _mangoStorage;

        public SampleService(MangoStorage mangoStorage)
        {
            _mangoStorage = mangoStorage;
            _logger = LoggerProviderManager.GetLoggerFactory().CreateLogger<SampleService>();
        }

        public void AddSample(SampleTaskInfo sampleTaskInfo)
        {
            lock (_locker)
            {
                _dicSampleTaskInfo[sampleTaskInfo.SamplingId] = sampleTaskInfo;
            }
        }

        /// <summary>
        /// 注册样品trace数据接收
        /// </summary>
        /// <param name="samplingId"></param>
        /// <param name="action"></param>
        public void RegisterSampleTrace(long samplingId, Action<SampleTraceEntity> action)
        {
            lock (_locker)
            {
                _dicSampleTraceReceiveAction[samplingId] = action;
            }
        }

        /// <summary>
        /// 注册样品完成事件
        /// </summary>
        /// <param name="samplingId"></param>
        /// <param name="action"></param>
        public void RegisterSampleComplete(long samplingId, Action<SampleTaskInfo> action)
        {
            lock (_locker)
            {
                _dicSampleCompleteAction[samplingId] = action;
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="samplingId"></param>
        /// <returns></returns>
        public SampleTaskInfo GetSample(long samplingId)
        {
            lock (_locker)
            {
                return _dicSampleTaskInfo[samplingId];
            }
        }

        public void RemoveSample(long samplingId)
        {
            lock (_locker) 
            {
                _dicSampleTaskInfo.Remove(samplingId);
                _dicSampleTraceReceiveAction.Remove(samplingId);
                _dicSampleCompleteAction.Remove(samplingId);
            }
        }

        public SampleTraceEntity GetSampleTrace(long samplingId)
        {
            lock (_locker)
            {
                if (_dicSampleTraceEntity.TryGetValue(samplingId, out SampleTraceEntity? value))
                {
                    return value;
                }
                else
                {
                    _dicSampleTraceEntity[samplingId] = new SampleTraceEntity() 
                    {
                        SamplingId = samplingId,
                    };
                    return _dicSampleTraceEntity[samplingId];
                }
            }
        }

        public void SaveSampleTrace(long samplingId)
        {
            lock (_locker)
            {
                if (_dicSampleTraceEntity.TryGetValue(samplingId, out var sampleTraceEntity) && _dicSampleTraceReceiveAction.TryGetValue(sampleTraceEntity.SamplingId, out Action<SampleTraceEntity>? value))
                {
                    value(sampleTraceEntity);
                  // _= _mangoStorage.SaveSampleTraceData(sampleTraceEntity);
                }
            }
        }


        public void SampleWorkComplete(long samplingId)
        {
            lock (_locker)
            {
                if (_dicSampleTaskInfo.TryGetValue(samplingId, out var sampleTaskInfo) && (_dicSampleCompleteAction.TryGetValue(samplingId, out Action<SampleTaskInfo>? value)))
                {
                    value(sampleTaskInfo);
                }
            }
        }

        public ISampleStow GetSampleStow(long samplingId)
        {
            lock (_locker)
            {
                if (_dicSampleStow.TryGetValue(samplingId, out ISampleStow? value))
                {
                    return value;
                }
                throw new Exception($"SampleStow:{samplingId} not found");
            }
        }
        public void SetSampleStow(long samplingId, ISampleStow sampleStow)
        {
            lock (_locker)
            {
                _dicSampleStow[samplingId] = sampleStow;
                _logger.LogInformation($"SetSampleStow:{samplingId},{sampleStow}");
            }
        }
    }
}
