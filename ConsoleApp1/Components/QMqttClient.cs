using QStandaedPlatform.Engine.Common;
using QStandaedPlatform.Engine.Common.Common;
using QStandaedPlatform.Engine.Mqtt;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace QStandaedPlatform.Engine.Components
{
    public class QMqttClientOptions
    {
        //string ip, int? port, string username, string password, string clientId, TimeSpan timeout, TimeSpan keepAlive

        public string Ip { get; set; } = "8.140.51.202";

        public int Port { get; set; } = 1883;

        public string Username { get; set; } = "bqjx";

        public string Password { get; set; } = "bqjx@2024";

        public string ClientId { get; set; } = "2024";

        public TimeSpan Timeout { get; set; } = TimeSpan.FromSeconds(10);

        public TimeSpan KeepAlive { get; set; } = TimeSpan.FromSeconds(60);
    }
    public class QMqttClient : IPart
    {
        private readonly QMqttClientOptions _options;
        private readonly BqjxMqttClient _bqjxMqttClient;
        public QMqttClient(QMqttClientOptions options)
        {
            _options = options;
            _bqjxMqttClient = new BqjxMqttClient();
            _bqjxMqttClient.MessageReceivedAsync += (topic, payload) => Task.Run(()=> OnMessageReceived?.Invoke(topic, payload));
        }
        public bool IsInitialized => _bqjxMqttClient.Connected;


        public void Initialize()
        {
            _=_bqjxMqttClient.ConnectAsync(_options.Ip, _options.Port, _options.Username, _options.Password, _options.ClientId, _options.Timeout, _options.KeepAlive);
        }

        public void Shutdown()
        {
            _bqjxMqttClient.DisconnectAsync();
        }

        public Task PublishAsync(string topic,string paayload)
        {
           return _bqjxMqttClient.PublishAsync(topic, paayload);
        }

        public Task SubscribeAsync(string topic)
        {
            return _bqjxMqttClient.SubscribeAsync(topic);
        }

        public event Action<string, string>? OnMessageReceived;
        public event EventHandler<EventArgs> PartCreated;
    }
}
