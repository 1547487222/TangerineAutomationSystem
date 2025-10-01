using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Equipment.Gas.Configs;
using Newtonsoft.Json.Linq;
using QStandaedPlatform.Engine.Mqtt;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading.Tasks;
using WpfApp1.Configs;

namespace WpfApp1
{
    public partial class MainWindowViewModel:ObservableObject
    {
        private readonly BqjxMqttClient _bqjxMqttClient;
        private readonly Timer _timer;
        public MainWindowViewModel()
        {
            _bqjxMqttClient = new BqjxMqttClient();
            _bqjxMqttClient.OnConnected += BqjxMqttClient_OnConnected;
            _bqjxMqttClient.OnDisconnected += BqjxMqttClient_OnDisconnected;
            Task.Delay(0).ContinueWith(async _ => 
            {
                await _bqjxMqttClient.ConnectAsync("8.140.51.202",1883, "bqjx", "bqjx@2024","2024",TimeSpan.FromSeconds(10),TimeSpan.FromSeconds(60));
            });
            message = [];
            _bqjxMqttClient.MessageReceivedAsync += (topic, message) =>
            {
                App.Current.Dispatcher.Invoke(() => 
                {
                    this.Message.Add(new MessageInfo 
                    {
                         Topic= topic,
                         Message= message
                    });
                });
                return Task.CompletedTask;
            };
            submqttSubjects = [];
            pubmqttSubjects = [];
            submqttSubjects.Add(new SubMqttSubject("订阅参数信息", "responses/gas/parameterInfo"));
            submqttSubjects.Add(new SubMqttSubject("订阅门锁密码信息", "responses/gas/doorLockPwd"));
            submqttSubjects.Add(new SubMqttSubject("订阅手动控制信息", "responses/gas/manualControl"));
            submqttSubjects.Add(new SubMqttSubject("订阅上报设备信息", "report/gas/machineInfos"));
            submqttSubjects.Add(new SubMqttSubject("订阅上报数据信息", "report/gas/dataInfos"));
            submqttSubjects.Add(new SubMqttSubject("订阅上报初始化完成", "report/gas/init_finish"));
            submqttSubjects.Add(new SubMqttSubject("订阅网关断开连接", "report/gas/disconnect"));

            _bqjxMqttClient.RegisterCommand("requests/gas/parameterInfo", new DemoCommand());

            pubmqttSubjects.Add(new PubMqttSubject("设置参数", "requests/gas/parameterInfo")
            {
                Paramter = new ParamterInfo
                {
                    preSpaceTime = "1",
                    preCoolDown = "2",
                    preGasFlowrate = "0.01",
                    preGasTemperature = "25"
                }
            });
            pubmqttSubjects.Add(new PubMqttSubject("初始化", "requests/gas/manual_control")
            {
                Paramter = new ManualControl
                {
                    manualCode = "1"
                }
            });
            pubmqttSubjects.Add(new PubMqttSubject("开始", "requests/gas/manualControl")
            {
                Paramter = new ManualControl
                {
                    manualCode = "2"
                }
            });
            pubmqttSubjects.Add(new PubMqttSubject("复位", "requests/gas/manual_control")
            {
                Paramter = new ManualControl
                {
                    manualCode = "3"
                }
            });
            pubmqttSubjects.Add(new PubMqttSubject("设置开锁密码", "requests/gas/doorLockPwd")
            {
                Paramter = new DoorlockPwd
                {
                     doorLockPwd = "3234"
                }
            });
            _timer = new Timer(async p =>
            {
                foreach (var topic in submqttSubjects)
                {
                    if (topic.IsEnable)
                    {
                        if (!_bqjxMqttClient.IsSubscribe(topic.SubjectUrl))
                        {
                            await _bqjxMqttClient.SubscribeAsync(topic.SubjectUrl);
                        }
                    }
                    else
                    {
                        if (_bqjxMqttClient.IsSubscribe(topic.SubjectUrl))
                        {
                            await _bqjxMqttClient.UnSubscribeAsync(topic.SubjectUrl);
                        }
                    }
                }
            }, this, 1000, 100);
           
        }

        private void BqjxMqttClient_OnDisconnected()
        {
            if (_bqjxMqttClient.Connected)
            {
                this.Connected = true;
            }
            else
            {
                this.Connected = false;
            }
        }

        private void BqjxMqttClient_OnConnected()
        {
            if (_bqjxMqttClient.Connected)
            {
                this.Connected = true;
            }
            else
            {
                this.Connected = false;
            }
        }

        [ObservableProperty]
        private ObservableCollection<MessageInfo> message;
        [ObservableProperty]
        private ObservableCollection<SubMqttSubject> submqttSubjects;
        [ObservableProperty]
        private ObservableCollection<PubMqttSubject> pubmqttSubjects;
        [ObservableProperty]
        private bool connected = false;

        /// <summary>
        /// 推送
        /// </summary>
        /// <param name="pubMqttSubject"></param>
        /// <returns></returns>
        [RelayCommand]
        public async Task Pub(PubMqttSubject pubMqttSubject)
        {
            if (_bqjxMqttClient.Connected)
            {
                this.Message.Add(new MessageInfo
                {
                    Topic = pubMqttSubject.PubSubjectUrl,
                    Message = JsonSerializer.Serialize(pubMqttSubject.Paramter)
                });
                await _bqjxMqttClient.PublishAsync(pubMqttSubject.PubSubjectUrl, JsonSerializer.Serialize(pubMqttSubject.Paramter));
            }
        }
        /// <summary>
        /// 连接
        /// </summary>
        /// <returns></returns>
        [RelayCommand]
        public async Task Connect()
        {
            if (!_bqjxMqttClient.Connected)
                await _bqjxMqttClient.ConnectAsync("8.140.51.202", 1883, "bqjx", "bqjx@2024", "2024", TimeSpan.FromSeconds(10), TimeSpan.FromSeconds(60));
        }
        /// <summary>
        /// 
        /// </summary>
        [RelayCommand]
        public void ClearDataInfo()
        {
            App.Current.Dispatcher.Invoke(() => 
            {
                this.Message.Clear();
            });
        }
        /// <summary>
        /// 断开连接
        /// </summary>
        /// <returns></returns>
        [RelayCommand]
        public async Task DisConnect()
        {
            if (_bqjxMqttClient.Connected)
            {
                foreach (var subMqttSubject in SubmqttSubjects) 
                {
                    subMqttSubject.IsEnable = false;
                }
                await Task.Delay(500).ContinueWith(async _ => 
                {
                    await _bqjxMqttClient.DisconnectAsync();
                });
            }
        }
    }
}
