using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using MQTTnet;
using System.Threading.Tasks;
using MQTTnet.Client;
using MQTTnet.Server;
using MQTTnet.Packets;
using MQTTnet.Protocol;
using System.Collections.Concurrent;
using System.Threading;
using System.Threading.Channels;

namespace QStandaedPlatform.Engine.Mqtt
{
    public class BqjxMqttClient : IBqjxMqttClient, IDisposable
    {
        private class Data
        {
            public Data(string topic, string message)
            {
                Topic = topic;
                Message = message;
            }
            public string Topic { get; set; }

            public string Message { get; set; }
        }
        private readonly IMqttClient _client;
        private readonly Channel<Data> _channel;
        private readonly CancellationTokenSource _cancellationTokenSource;
        private readonly ConcurrentDictionary<string, IBqjxMqttCommand> _topicCommands = new();
        private readonly ConcurrentDictionary<string, MqttQualityOfServiceLevel> _topicDict = new();

        public BqjxMqttClient()
        {
            _cancellationTokenSource = new CancellationTokenSource();
            _channel = Channel.CreateUnbounded<Data>();
            _client = new MqttFactory().CreateMqttClient();
            _client.ConnectedAsync += async (e) =>
            {
                await Task.Delay(0).ContinueWith(_ =>
                {
                    OnConnected?.Invoke();
                });
            };
            _client.DisconnectedAsync += async (e) =>
            {
                await Task.Delay(0).ContinueWith(_ =>
                {
                    OnDisconnected?.Invoke();
                });
            };
            new Thread(async () =>
            {
                while (await _channel.Reader.WaitToReadAsync(_cancellationTokenSource.Token))
                {
                    while (_channel.Reader.TryRead(out var item))
                    {
                        var tasks = new List<Task>();
                        if (_topicCommands.TryGetValue(item.Topic, out var command))
                        {
                            tasks.Add(command.Execute(item.Topic, item.Message));
                        }
                        if (MessageReceivedAsync != null)
                        {
                            tasks.Add(MessageReceivedAsync.Invoke(item.Topic, item.Message));
                        }
                        if (tasks.Count != 0)
                            await Task.WhenAll(tasks);
                    }
                }
            })
            { IsBackground=true }.Start();
            _client.ApplicationMessageReceivedAsync += async (arg) =>
            {
                await _channel.Writer.WriteAsync(new Data(arg.ApplicationMessage.Topic, Encoding.UTF8.GetString(arg.ApplicationMessage.PayloadSegment)), _cancellationTokenSource.Token);
            };
        }

        public bool Connected => _client.IsConnected;

        public event Func<string, string, Task>? MessageReceivedAsync;

        public event Action? OnConnected;

        public event Action? OnDisconnected;

        public async Task ConnectAsync(string ip, int? port, string username, string password, string clientId, TimeSpan timeout, TimeSpan keepAlive)
        {
            _ = await _client.ConnectAsync(new MqttClientOptionsBuilder()
                .WithTcpServer(ip, port)
                .WithCredentials(username, password)
                .WithCleanSession(true)
                .WithClientId(clientId)
                .WithTimeout(timeout)
                .WithKeepAlivePeriod(keepAlive)
                .Build());
        }

        public Task DisconnectAsync()
        {
            return _client.DisconnectAsync(new MqttClientDisconnectOptionsBuilder().WithReason(MqttClientDisconnectOptionsReason.NormalDisconnection).Build());
        }

        public void Dispose()
        {
            if (Connected)
            {
                _cancellationTokenSource.Cancel();
                DisconnectAsync().Wait();
            }
        }

        public async Task PublishAsync(string topic, string payload)
        {
            var applicationMessage = new MqttApplicationMessageBuilder()
                .WithTopic(topic)
                .WithPayload(payload)
                .Build();
            await _client.PublishAsync(applicationMessage, CancellationToken.None);
        }
        public bool IsSubscribe(string topic)
        {
            return _topicDict.ContainsKey(topic);
        }
        public async Task SubscribeAsync(string topic, MqttQualityOfServiceLevel qualityOfServiceLevel = MqttQualityOfServiceLevel.AtMostOnce)
        {
            var mqttClientSubscribeOptionsBuilder = new MqttClientSubscribeOptionsBuilder();
            mqttClientSubscribeOptionsBuilder.WithTopicFilter(
            new MqttTopicFilter
            {
                Topic = topic,
                QualityOfServiceLevel = qualityOfServiceLevel,
                NoLocal = false,
                RetainAsPublished = false,
                RetainHandling = MqttRetainHandling.SendAtSubscribe
            });
            var mqttSubscribeOptions = mqttClientSubscribeOptionsBuilder.Build();
            await _client.SubscribeAsync(mqttSubscribeOptions, CancellationToken.None);
            _topicDict.TryAdd(topic, qualityOfServiceLevel);
        }
        public async Task UnSubscribeAsync(string topic)
        {
            var mqttClientUnsubscribeOptionsBuilder = new MqttClientUnsubscribeOptionsBuilder();
            mqttClientUnsubscribeOptionsBuilder.WithTopicFilter(topic);
            var mqttClientUnsubscribeOptions = mqttClientUnsubscribeOptionsBuilder.Build();
            await _client.UnsubscribeAsync(mqttClientUnsubscribeOptions);
            _topicDict.TryRemove(topic, out _);
        }

        public void RegisterCommand(string topic, IBqjxMqttCommand command)
        {
            _topicCommands[topic] = command;
        }
    }
}
