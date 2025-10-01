using QStandaedPlatform.Engine.Common;
using QStandaedPlatform.Engine.Common.Common;
using QStandaedPlatform.Engine.Common.Common.Metadatas;
using QStandaedPlatform.Engine.Components;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace System.Foundation.Modules.Mqtt
{
    [DisplayName("MQTT客户端")]
    public class MqttClientTool : ToolBase
    {
        public class MqttClientData
        {
            [DisplayName("订阅主题")]
            public List<string> Topics { get; set; } = [];
        }
        public override string DefineName => "MQTT客户端";

        [ReferencePart]
        public QMqttClient Client { get; set; }

        public override bool InitPins()
        {
            InsetPin("输出Mqtt订阅主题消息",this,typeof(QString), PinType.Output);
            return base.InitPins();
        }
        public override bool InitDataContext()
        {
            DataContext = new MqttClientData();
            return true;
        }
        public override void OnRefPartPropertyInstalled(IRefPartProperty part)
        {
            if (part.RefPart is QMqttClient mqttClient)
            {
                mqttClient.OnMessageReceived += MqttClient_OnMessageReceived;
                var data = DataContext as MqttClientData;
                if (data.Topics.Count > 0)
                {
                    foreach (var topic in data.Topics) 
                    {
                        mqttClient.SubscribeAsync(topic);
                    }
                }
            }
        }

        private void MqttClient_OnMessageReceived(string arg1, string arg2)
        {
            SendToPin("输出Mqtt订阅主题消息", (QString)$"{arg1}:{arg2}");
        }

        public override void OnRefPartPropertyUnInstalled(IRefPartProperty part)
        {
            if (part.RefPart is QMqttClient mqttClient)
            {
                mqttClient.OnMessageReceived -= MqttClient_OnMessageReceived;
            }
        }
        public override Task<bool> ExecuteAsync(PinInfo pinInfo, QData pinData, ToolExecutionContext toolContext)
        {
            return Task.FromResult(false);
        }

        public override void ApplyOnContextChanged(object context)
        {
            var data = DataContext as MqttClientData;
            if (data.Topics.Count > 0 && Client != null)
            {
                foreach (var topic in data.Topics)
                {
                    Client.SubscribeAsync(topic);
                }
            }
        }
    }
}
