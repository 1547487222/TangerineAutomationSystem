using Microsoft.Extensions.Logging;
using QStandaedPlatform.Engine.Common.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace QStandaedPlatform.Engine.Laboratory
{
    public class ModularAlarmService : Singleton<ModularAlarmService>
    {
        private ILogger<ModularAlarmService>? _logger;
        protected override void Initialize()
        {
            _logger = LoggerProviderManager.GetLoggerFactory().CreateLogger<ModularAlarmService>();
        }
        public void PostAlarm(Modular modular, ModularException modularException)
        {
            var moduleAlarmRecord = new ModuleAlarmRecord
            {
                ModuleIp = modular.Messenger.Ip,
                ActionDescription = modularException.Action,
                ModuleName = modular.ModuleName,
                DetailsAlarmMessage = modularException.Message,
                InternalAlarmMessage = modularException.InternalMessage
            };
            if (modularException.IsAlarm)
            {
                var modularAlarms = modular.GetModuleAlarmInfo();
                if (modularAlarms != null && modularAlarms.Count > 0)
                {
                    foreach (var item in modularAlarms)
                    {
                        var moduleAlarmRecordClone = moduleAlarmRecord.Clone();
                        moduleAlarmRecordClone.AlarmCode = item.AlarmCode;
                        moduleAlarmRecordClone.InternalAlarmMessage = item.AlarmDescription?? modularException.InternalMessage;
                        _logger?.LogWarning($"模块{modular.ModuleName}发生报警,IP:{modular.Messenger.Ip},报警码:{item.AlarmCode},报警描述:{item.AlarmDescription},内部描述:{modularException.InternalMessage},报警时间:{DateTime.Now}");
                        OnAlarm?.Invoke(moduleAlarmRecordClone);
                    }
                }
            }
            else
            {
                _logger?.LogWarning($"模块{modular.ModuleName}发生异常,IP:{modular.Messenger.Ip},异常描述:{modularException.Message},内部描述:{modularException.InternalMessage},报警时间:{DateTime.Now}");
                OnAlarm?.Invoke(moduleAlarmRecord);
            }
        }
        public event Action<ModuleAlarmRecord>? OnAlarm;
    }

    public class ModuleAlarmRecord
    {
        public string ModuleIp { get; set; } = string.Empty;

        public string ModuleName { get; set; } = string.Empty;

        public string AlarmCode { get; set; } = string.Empty;

        public string DetailsAlarmMessage { get; set; } = string.Empty;

        public string InternalAlarmMessage { get; set; } = string.Empty;

        public string ActionDescription { get; set; } = string.Empty;

        public DateTime AlarmTime { get; set; } = DateTime.Now;


        public ModuleAlarmRecord Clone()
        {
            var clone = new ModuleAlarmRecord
            {
                ModuleIp = ModuleIp,
                ModuleName = ModuleName,
                AlarmCode = AlarmCode,
                DetailsAlarmMessage = DetailsAlarmMessage,
                InternalAlarmMessage = InternalAlarmMessage,
                ActionDescription = ActionDescription
            };
            return clone;
        }

        public override string ToString()
        {
            return $"模块IP:{ModuleIp},模块名称:{ModuleName},报警码:{AlarmCode},报警描述:{DetailsAlarmMessage},内部描述:{InternalAlarmMessage},动作描述:{ActionDescription},报警时间:{AlarmTime}";
        }
    }
}
