using MQTTnet.Client;
using MQTTnet.Protocol;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace QStandaedPlatform.Engine.Mqtt
{
    public interface IBqjxMqttClient
    {
        /// <summary>
        /// 返回string topic, string payload
        /// </summary>
        event Func<string, string, Task> MessageReceivedAsync;
        /// <summary>
        /// 连接完成事件
        /// </summary>
        event Action OnConnected;
        /// <summary>
        /// 断开连接事件
        /// </summary>
        event Action OnDisconnected;
        /// <summary>
        /// 客户端连接函数
        /// </summary>
        /// <param name="ip"></param>
        /// <param name="port"></param>
        /// <param name="username"></param>
        /// <param name="password"></param>
        /// <param name="clientId"></param>
        /// <param name="timeout"></param>
        /// <param name="keepAlive"></param>
        /// <returns></returns>
        Task ConnectAsync(string ip, int? port, string username, string password, string clientId, TimeSpan timeout, TimeSpan keepAlive);
        /// <summary>
        /// 连接状态
        /// </summary>
        bool Connected { get; }
        /// <summary>
        /// 推送函数
        /// </summary>
        /// <param name="topic">主题</param>
        /// <param name="payload">数据</param>
        /// <returns></returns>
        Task PublishAsync(string topic, string payload);


        void RegisterCommand(string topic, IBqjxMqttCommand command);
        bool IsSubscribe(string topic);
        /// <summary>
        /// 订阅函数
        /// </summary>
        /// <param name="topic">主题</param>
        /// <param name="qualityOfServiceLevel">订阅等级类型</param>
        /// <returns></returns>
        Task SubscribeAsync(string topic, MqttQualityOfServiceLevel qualityOfServiceLevel = MqttQualityOfServiceLevel.AtMostOnce);
        /// <summary>
        /// 取消订阅函数
        /// </summary>
        /// <param name="topic">主题</param>
        /// <returns></returns>
        Task UnSubscribeAsync(string topic);
        /// <summary>
        ///断开连接函数
        /// </summary>
        /// <returns></returns>
        Task DisconnectAsync();
    }
}
