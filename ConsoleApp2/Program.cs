using System.Diagnostics.CodeAnalysis;
using System.Management;
using System.Net.NetworkInformation;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Runtime.Loader;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using QStandaedPlatform.Engine.Common.Common;
using QStandaedPlatform.Engine.Common.Common.ModuleEntitys;
using QStandaedPlatform.Engine.Components.Modbus;
using QStandaedPlatform.Engine.Laboratory;
namespace ConsoleApp2
{

    public class Data
    {
        public string Name { get; set; }

        public string Age { get; set; }
         
        public string Score { get; set; }



    }



    internal class Program
    {
        #region
        public class Message
        {
            public string role { get; set; }

            public string content { get; set; }
        }
        public class Data
        {
            public string model { get; set; }

            public List<Message> messages { get; set; }

            public bool stream { get; set; }
        }


        public class AlarmInfo
        {
            public AlarmInfo()
            {
                AlarmItems = new List<AlarmItem>();
            }
            public List<AlarmItem> AlarmItems { get; set; }
        }

        public class AlarmItem
        {
            /// <summary>
            /// 报警编号
            /// </summary>
            public int AlarmCode { get; set; }
            /// <summary>
            /// 报警描述
            /// </summary>
            public string Description { get; set; }
            /// <summary>
            /// 报警地址
            /// </summary>
            public string Address { get; set; }
        }

        #endregion
        
        [Flags]
        public enum AlarmStatus : ushort
        {
            None = 0,        // 无报警
            WrongPassword = 1 << 0,   // D190.1 输入密码错误
            DoorNotClosed = 1 << 1,   // D190.2 设备门没有关
            YAxisError = 1 << 2,   // D190.3 Y轴电机报警
            InitTimeout = 1 << 3,   // D190.4 初始化超时
            ZAxisTimeout = 1 << 4,   // D190.5 Z轴电机超时报警
            XAxisTimeout = 1 << 5,   // D190.6 X轴电机超时报警
            WaitFlowTimeout = 1 << 6,   // D190.7 等待流量超时报警
            //托盘监测报警
            TrayMonitor = 1 << 7,   // D190.8 托盘监测报警
        }
        [Flags]
        public enum GasSamplingState : ushort
        {
            None = 0,        // 无状态
            WakeUpPower = 1 << 0,   // D140.0 唤醒伺服电源
            WakeUpSuccess = 1 << 1,   // D140.1 唤醒伺服电源成功
            LoopWaiting = 1 << 2,   // D140.2 循环（时间）小时等待中
            WaitForCooling = 1 << 3,   // D140.3 等待风冷结束
            ZAxisOrigin = 1 << 4,   // D140.4 Z轴回原点
            YAxisOrigin = 1 << 5,   // D140.5 Y轴回原点
            YAxisToTubeBottom = 1 << 6,   // D140.6 Y轴移动到铜管下压位
            ZAxisToGasPosition = 1 << 7,   // D140.7 Z轴下压到通气位置
            WaitForFlow = 1 << 8,   // D140.8 等待流量到达
            TorqueModePress = 1 << 9,   // D140.9 扭矩模式下压
            ZAxisPressReset = 1 << 10,  // D140.10 Z轴下压完成回零点
            ServoSleep = 1 << 11,  // D140.11 伺服进入休眠
            AllCopperDone = 1 << 12,  // D140.12 9个铜管全部做完等待取料

            // D140.13, D140.14, D140.15 预留
        }
        static ushort GetAlarmStatus()
        {
            return (ushort)(AlarmStatus.DoorNotClosed | AlarmStatus.WrongPassword);
        }
        static async Task Main(string[] args)
        {
            #region
            AlarmInfo alarmInfo = new AlarmInfo();
            alarmInfo.AlarmItems.Add(new AlarmItem 
            {
                 Address="M108", AlarmCode=1, Description= "整机有报警"
            });
            alarmInfo.AlarmItems.Add(new AlarmItem
            {
                Address = "M401",
                AlarmCode = 2,
                Description = "初始化失败"
            });
            alarmInfo.AlarmItems.Add(new AlarmItem
            {
                Address = "M430",
                AlarmCode = 3,
                Description = "x步进电机报警"
            });
            alarmInfo.AlarmItems.Add(new AlarmItem
            {
                Address = "M431",
                AlarmCode = 4,
                Description = "Y步进电机报警"
            });
            alarmInfo.AlarmItems.Add(new AlarmItem
            {
                Address = "M432",
                AlarmCode = 5,
                Description = "Z步进电机报警"
            });
            alarmInfo.AlarmItems.Add(new AlarmItem
            {
                Address = "M450",
                AlarmCode = 6,
                Description = "拍照失败"
            });
            alarmInfo.AlarmItems.Add(new AlarmItem
            {
                Address = "B415",
                AlarmCode = 7,
                Description = "管路中检测不到酸液"
            });
            alarmInfo.AlarmItems.Add(new AlarmItem
            {
                Address = "B416",
                AlarmCode = 8,
                Description = "管路中检测不到温泉液体"
            });
            alarmInfo.AlarmItems.Add(new AlarmItem
            {
                Address = "B420",
                AlarmCode = 9,
                Description = "温泉加液中 检测不到液体"
            });
            alarmInfo.AlarmItems.Add(new AlarmItem
            {
                Address = "B422",
                AlarmCode = 10,
                Description = "托盘1未就绪"
            });
            alarmInfo.AlarmItems.Add(new AlarmItem
            {
                Address = "B423",
                AlarmCode = 11,
                Description = "托盘2未就绪"

            });
            alarmInfo.AlarmItems.Add(new AlarmItem
            {
                Address = "B424",
                AlarmCode = 12,
                Description = "托盘3未就绪"
            });











            //var josn = JsonConvert.SerializeObject(alarmInfo);
            //File.WriteAllText("AlarmInfo.json",josn);
            //var apiKey = "sk-4cf4682e5bf545b7ab3e1fea8149a6f9"; // 替换为你的 API Key
            //var url = "https://api.deepseek.com/chat/completions";

            //using (var client = new HttpClient())
            //{
            //    client.DefaultRequestHeaders.Add("Authorization", $"Bearer {apiKey}");

            //    var jsonData = JsonConvert.SerializeObject(new Data() 
            //    {
            //       model= "deepseek-chat",
            //       messages = new List<Message>() 
            //       {
            //           new Message{ content="You are a helpful assistant.", role="system" },
            //       }
            //    });

            //var content = new StringContent(jsonData, Encoding.UTF8, "application/json");

            //        try
            //        {
            //            var response = await client.PostAsync(url, content);
            //            response.EnsureSuccessStatusCode();

            //            var responseBody = await response.Content.ReadAsStringAsync();
            //            Console.WriteLine(responseBody);
            //        }
            //        catch (HttpRequestException e)
            //        {
            //            Console.WriteLine($"Request error: {e.Message}");
            //        }
            //    }
            //Console.ReadLine();
            //var settings = new JsonSerializerSettings
            //{
            //    PreserveReferencesHandling = PreserveReferencesHandling.Objects,
            //    ReferenceLoopHandling = ReferenceLoopHandling.Serialize
            //};
            //var json= File.ReadAllText("flow.json");
            //var flowInfoOptions = JsonConvert.DeserializeObject<FlowInfoOptions>(json, settings);


            //IServiceCollection descriptors = new ServiceCollection();
            //descriptors.AddTransient<SaltTool>();
            //descriptors.AddTransient<VibrateTool>();
            ////Container.ConfigProvider(descriptors);
            //ToolDescriptions.InitToolRunTimeDesc();
            //Flow flow = new();
            //if (flowInfoOptions != null)
            //flow.Deserialize(flowInfoOptions);
            //var guid1 = Guid.NewGuid();
            //var guid2 = Guid.NewGuid();
            //flow.CreateNewTool("加盐模块", guid1);
            //flow.CreateNewTool("振荡模块", guid2);
            //var tool1 = flow.GetTool(guid1);
            //var tool2 = flow.GetTool(guid2);
            //flow.Connect(tool2.InputPins[0].Id, tool1.OutputPins[1].Id);
            //flow.Connect(tool1.InputPins[0].Id, tool2.OutputPins[0].Id);

            //var flowInfoOptions = flow.Serialize();


            //var json = JsonConvert.SerializeObject(flowInfoOptions, settings);
            //var streamWriter= File.CreateText("flow.json");
            //streamWriter.AutoFlush = true;
            //streamWriter.Write(json);
            #endregion

            // var status = GetAlarmStatus();

            // //  var json = "[{\"Index\":1,\"ParamName\":\"scanQrCode\",\"Comment\":\"扫码\",\"Value\":0,\"Flag\":0,\"Unit\":\"\",\"Element\":\"[{\\\"type\\\":\\\"InputNumber\\\",\\\"precision\\\":0,\\\"max\\\":0,\\\"min\\\":0,\\\"disabled\\\":true,\\\"show\\\":true}]\"},{\"Index\":2,\"ParamName\":\"samplingVolume\",\"Comment\":\"血样取样量\",\"Value\":250,\"Flag\":1,\"Unit\":\"ul\",\"Element\":\"[{\\\"type\\\":\\\"InputNumber\\\",\\\"precision\\\":0,\\\"max\\\":1500,\\\"min\\\":0,\\\"disabled\\\":false}]\"},{\"Index\":3,\"ParamName\":\"addAcidAndACN\",\"Comment\":\"加酸乙腈加入量\",\"Value\":1000,\"Flag\":1,\"Unit\":\"ul\",\"Element\":\"[{\\\"type\\\":\\\"InputNumber\\\",\\\"precision\\\":0,\\\"max\\\":1500,\\\"min\\\":0,\\\"disabled\\\":false}]\"},{\"Index\":4,\"ParamName\":\"vortexTime1\",\"Comment\":\"涡旋时间\",\"Value\":120,\"Flag\":1,\"Unit\":\"s\",\"Element\":\"[{\\\"type\\\":\\\"InputNumber\\\",\\\"precision\\\":0,\\\"max\\\":1500,\\\"min\\\":0,\\\"disabled\\\":false}]\"},{\"Index\":4,\"ParamName\":\"vortexVelocity1\",\"Comment\":\"涡旋速度\",\"Value\":1500,\"Flag\":1,\"Unit\":\"rpm\",\"Element\":\"[{\\\"type\\\":\\\"InputNumber\\\",\\\"precision\\\":0,\\\"max\\\":1500,\\\"min\\\":0,\\\"disabled\\\":false}]\"},{\"Index\":5,\"ParamName\":\"addSalt\",\"Comment\":\"加盐量\",\"Value\":500,\"Flag\":1,\"Unit\":\"mg\",\"Element\":\"[{\\\"type\\\":\\\"InputNumber\\\",\\\"precision\\\":0,\\\"max\\\":1500,\\\"min\\\":0,\\\"disabled\\\":false}]\"},{\"Index\":6,\"ParamName\":\"vortexTime2\",\"Comment\":\"加盐后涡旋时间\",\"Value\":120,\"Flag\":1,\"Unit\":\"s\",\"Element\":\"[{\\\"type\\\":\\\"InputNumber\\\",\\\"precision\\\":0,\\\"max\\\":1500,\\\"min\\\":0,\\\"disabled\\\":false}]\"},{\"Index\":6,\"ParamName\":\"vortexVelocity2\",\"Comment\":\"加盐后涡旋速度\",\"Value\":1500,\"Flag\":1,\"Unit\":\"rpm\",\"Element\":\"[{\\\"type\\\":\\\"InputNumber\\\",\\\"precision\\\":0,\\\"max\\\":1500,\\\"min\\\":0,\\\"disabled\\\":false}]\"}]";
            // //  var aaa = (JArray.FromObject(JsonConvert.DeserializeObject(json)));
            // // var data=new List<int>(){1,2,3,4,5};
            // //var sss=  JsonConvert.SerializeObject(data);
            // //  JArray a = (JArray)JsonConvert.DeserializeObject(sss);

            // // 输入字符串
            // string input = "406  ";

            //var cdabBytes= ConvertToCDABString(input).Replace(" ", "").Trim(new char[] {'\0' });
            // Console.WriteLine("转换字符："+ cdabBytes);


            // 模板代码

            //BqjxModuleClassDef bqjxModuleClassDef = new()
            //{
            //    ModuleClassName = "TotalLiquidTransferModule",
            //    ModuleClassDescription = "全液量转移模块",
            //    ModuleBaseClassName = "ModuleBase"
            //};
            //bqjxModuleClassDef.BqjxModuleMethodDefs.Add(new BqjxModuleMethodDef
            //{
            //    ModuleMethodName = "TotalLiquidTransfer",
            //    BqjxModuleMethodParamDefs = new List<BqjxModuleMethodParamDef> 
            //    {
            //         new BqjxModuleMethodParamDef{ ModuleMethodParamName="liquids", ModuleMethodParamType="float" }
            //    }
            //});
            //var code=  ModuleCodeGenerator.GenerateModuleCode(bqjxModuleClassDef);

            //var path= Path.Combine(AppDomain.CurrentDomain.BaseDirectory,"Codes", bqjxModuleClassDef.ModuleClassName+".cs");

            //if (!Directory.Exists(Path.GetDirectoryName(path)))
            //{
            //    Directory.CreateDirectory(Path.GetDirectoryName(path));
            //}
            //File.WriteAllText(path, code);
            //var saltTubeRackName = "加盐试管架";
            //var vortexTubeRackName = "涡旋工位";
            //ExecutionContext executionContext = new() { TaskInfo = "" };

            //LinkNode linkNode = new();
            //linkNode.Action = new StartAction(context => 
            //{
            //    Console.WriteLine("开始");
            //    //写入
            //    context.Datas[saltTubeRackName] = "A1";
            //    context.Datas[vortexTubeRackName] = "A2";
            //});
            //linkNode.Action = new TubeRack() { TubeRackName = "加盐" };
            //BqjxModuleDef bqjxModuleDef = new();
            //bqjxModuleDef.BqjxModuleClassDef.ModuleClassName = "ConcentrationModuleBase";
            //bqjxModuleDef.BqjxModuleClassDef.ModuleBaseClassName = "ModuleBase";
            //bqjxModuleDef.BqjxModuleClassDef.ModuleClassDescription="浓缩模块基类";
            //bqjxModuleDef.BqjxModuleClassDef.BqjxModuleMethodDefs.Add(new BqjxModuleMethodDef
            //{
            //    FuncCodeName = "Concentration",
            //    ModuleMethodName = "Concentration",
            //    BqjxModuleMethodParamDefs = 
            //    {
            //        new (){ ModuleMethodParamName="decisionTime", ModuleMethodParamType="float" },
            //        new (){ ModuleMethodParamName="transitionTime", ModuleMethodParamType="float" },
            //        new (){ ModuleMethodParamName="channel1Vel", ModuleMethodParamType="float" },
            //        new (){ ModuleMethodParamName="channel2Vel", ModuleMethodParamType="float" },
            //        new (){ ModuleMethodParamName="channel1Terminalflow", ModuleMethodParamType="float" },
            //        new (){ ModuleMethodParamName="channel2Terminalflow", ModuleMethodParamType="float" },
            //        new (){ ModuleMethodParamName="channel1StartingTemperature", ModuleMethodParamType="float" },
            //        new (){ ModuleMethodParamName="channel1TransitionTemperature", ModuleMethodParamType="float" },
            //        new (){ ModuleMethodParamName="channel2StartingTemperature", ModuleMethodParamType="float" },
            //        new (){ ModuleMethodParamName="channel2TransitionTemperature", ModuleMethodParamType="float" },
            //    }
            //});
            //var code = ModuleCodeGenerator.GenerateModuleBaseCode(bqjxModuleDef);
            //Console.WriteLine(code);

            // 获取所有网络接口
            //NetworkInterface[] networkInterfaces = NetworkInterface.GetAllNetworkInterfaces();

            //foreach (NetworkInterface network in networkInterfaces)
            //{
            //    // 忽略回环接口、未启用的接口以及非以太网和无线网卡的接口
            //    if (network.OperationalStatus == OperationalStatus.Up &&
            //        network.NetworkInterfaceType != NetworkInterfaceType.Loopback &&
            //        (network.NetworkInterfaceType == NetworkInterfaceType.Ethernet ||
            //         network.NetworkInterfaceType == NetworkInterfaceType.Wireless80211))
            //    {
            //        Console.WriteLine("网络接口名称: " + network.Name);
            //        Console.WriteLine("描述: " + network.Description);
            //        Console.WriteLine("MAC 地址: " + network.GetPhysicalAddress().ToString());
            //        Console.WriteLine();
            //    }


            //}

            //using (var searcher = new ManagementObjectSearcher("SELECT SerialNumber FROM Win32_BaseBoard"))
            //{
            //    foreach (var item in searcher.Get())
            //    {
            //        var ss= item["SerialNumber"]?.ToString().Trim();
            //    }
            //}
            //var str = new string[] {"A01","B02","A02","A04","B06","C01","D01","C06","D02" };
            ////排序
            //var result = str.OrderBy(x => x).ToArray();
            //Console.WriteLine(string.Join(",", result));
            //DefaultLoggerProviderAware _loggerProviderAware = new();
            //_loggerProviderAware.Configure();
            //LoggerProviderManager.RegisterLoggerProviderAware(_loggerProviderAware);
            //ModularDev modularDev = new(new H5uModbusTcpMock("", 502));
            //await modularDev.HomeAsync();
            //await modularDev.CheckModuleHomeDoneAsync();
            //await modularDev.WriteParameterAsync([]);
            //await modularDev.ModuleExecuteAsync();
            //await modularDev.CheckModuleDoneAsync();

            //for (int a   = 0; a < 288; a++)
            //{
            //    ModuleReportRunDataEntity moduleReportRunDataEntity = new();
            //    moduleReportRunDataEntity.ModuleName = "加液模块";
            //    moduleReportRunDataEntity.ModuleActionDescription = "";
            //    moduleReportRunDataEntity.Alarms =
            //[
            //   new ModuleReportAlarmEntity
            //   {
            //       AlarmCode="ALM_01",
            //   }
            //];
            //    moduleReportRunDataEntity.ModuleParameter = "";
            //    moduleReportRunDataEntity.PlatFormName = "加液平台";
            //    moduleReportRunDataEntity.StartDay = new DateOnly(DateTime.Now.Year, DateTime.Now.Month, DateTime.Now.Day);
            //    moduleReportRunDataEntity.StartTime = DateTime.Now;
            //    moduleReportRunDataEntity.TaskDatas = [];
            //    for (int i = 0; i < 300; i++)
            //    {
            //        var taskData = new ModuleReportMonitorTaskDataOnceEntity();
            //        for (int j = 0; j < 5; j++)
            //        {
            //            taskData.ModuleReportCollectTaskDataItemEntities.Add(new ModuleReportMonitorTaskDataItemEntity
            //            {
            //                MonitorName = $"采集项{j}",
            //                MonitorTime = DateTime.Now,
            //                MonitorValue = i.ToString()
            //            });
            //        }
            //        moduleReportRunDataEntity.TaskDatas.Add(taskData);
            //    }

            //    var json = JsonConvert.SerializeObject(moduleReportRunDataEntity);
            //}
            ReentrantLockService<Data> lockService = new ReentrantLockService<Data>();

            Data data = new Data() { model="你好" };
            //Task.Run(() => 
            //{
            //    using (var lockVc = lockService.Acquire(data, "key1"))
            //    {
            //        Thread.Sleep(3000);
            //        Console.WriteLine($"key1:{data.model}");
            //    }
            //});
            //Task.Run(() =>
            //{
            //    using (var lockVc = lockService.Acquire(data, "key2"))
            //    {
            //        Thread.Sleep(2000);
            //        Console.WriteLine($"key2:{data.model}");
            //    }
            //});
            // 按顺序发起 Parallel.For 的锁请求
            //var tasks = new List<Task>();
            //for (int i = 0; i < 10; i++)
            //{
            //    int index = i; // 捕获循环变量
            //    tasks.Add(Task.Run(() =>
            //    {
            //        using var lockVc = lockService.Acquire(data, $"key{index}");
            //        Thread.Sleep(1000);
            //        Console.WriteLine($"{index}:{data.model}");
            //        var lockse= lockService.Acquire(data, $"key{index}");
            //        Thread.Sleep(1000);
            //        Console.WriteLine($"二次锁{index}:{data.model}");
            //        lockse.Dispose();
            //    }));
            //}

            //// 等待所有任务完成
            //await Task.WhenAll(tasks);
            TemporaryTimingWheel timingWheel = new TemporaryTimingWheel();
            timingWheel.RegisterNotify("1", (task) => 
            {
                Console.WriteLine($"执行任务1,{task.Id}{task.RemainingRounds}");
            });
            timingWheel.RegisterNotify("2", (task) => 
            {
                Console.WriteLine($"执行任务2,{task.Id},{DateTime.Now},{task.RemainingRounds}");
            });
            timingWheel.AddTask("1",new TemporaryTask 
            {
                 Id=1, RemainingRounds=5
            });
            Thread.Sleep(5000);
            for (int i = 0; i < 10000000; i++)
            {
                timingWheel.AddTask("2", new TemporaryTask { Id = i, RemainingRounds = 5+i/100 });
            }
            Console.ReadLine();
        }
        static string ConvertToCDABString(string input)
        {
            byte[] bytes = ASCIIEncoding.ASCII.GetBytes(input);
            int paddingLength = 2 - (bytes.Length % 2);
            if (paddingLength == 2) paddingLength = 0;
            byte[] paddedBytes = new byte[bytes.Length + paddingLength];
            Array.Copy(bytes, paddedBytes, bytes.Length);
            byte[] cdabBytes = new byte[paddedBytes.Length];
            for (int i = 0; i < paddedBytes.Length; i += 2)
            {
                if (i + 2 <= paddedBytes.Length)
                {
                    cdabBytes[i] = paddedBytes[i + 1];
                    cdabBytes[i + 1] = paddedBytes[i];
                }
                else
                {
                    cdabBytes[i] = paddedBytes[i];
                }
            }
            return ASCIIEncoding.ASCII.GetString(cdabBytes).Replace(" ", "").Trim(new char[] { '\0' });
        }
    }
    /// <summary>
    /// 模块定义
    /// </summary>
    public class BqjxModuleDef
    {
        /// <summary>
        /// 命名空间名称
        /// </summary>
        public string NamespaceName { get; set; }

        /// <summary>
        /// 模块类定义
        /// </summary>
        public BqjxModuleClassDef BqjxModuleClassDef { get; set; } = new BqjxModuleClassDef();
    }
    public class BqjxModuleClassDef
    {
        /// <summary>
        /// 模块名称
        /// </summary>
        public string ModuleClassName { get; set; }

        public string ModuleClassDescription { get; set; }
        /// <summary>
        /// 模块基类名称
        /// </summary>
        public string ModuleBaseClassName { get; set; }


        public List<BqjxModulePropertyDef> BqjxModulePropertyDefs { get; set; } = [];
        public List<BqjxModuleMethodDef> BqjxModuleMethodDefs { get; set; } = [];
    }

    public class BqjxModulePropertyDef
    {
        public string ModulePropertyName { get; set; }

        public string ModulePropertyType { get; set; }
    }

    public class BqjxModuleMethodDef
    {
        public string ModuleMethodName { get; set; }

        public string FuncCodeName { get; set; }
        public List<BqjxModuleMethodParamDef> BqjxModuleMethodParamDefs { get; set; } = [];
    }

    public class BqjxModuleMethodParamDef
    {
        public string ModuleMethodParamName { get; set; }

        public string ModuleMethodParamType { get; set; }
    }

    public static class ModuleCodeGenerator
    {
        /// <summary>
        /// 生成模块基类代码
        /// </summary>
        /// <param name="bqjxModuleClassDef"></param>
        /// <returns></returns>
        public static  string GenerateModuleBaseCode(BqjxModuleDef  bqjxModuleDef)
        {
            StringBuilder sb = new ();

            //命名空间
            sb.AppendLine("using BQJX.Core.Common.Common.JsonAccess;");
            sb.AppendLine("using BQJX.Core.Common.Interface;");
            sb.AppendLine("using BQJX.Modules.Common;");
            sb.AppendLine("using System;");
            sb.AppendLine("using System.Collections.Generic;");
            sb.AppendLine("using System.Linq;");
            sb.AppendLine("using System.Text;");
            sb.AppendLine("using System.Threading.Tasks;");
            sb.AppendLine();
            sb.AppendLine("namespace "+ bqjxModuleDef.NamespaceName);

            // 类头部注释
            sb.AppendLine("/// <summary>");
            sb.AppendLine("/// " + bqjxModuleDef.BqjxModuleClassDef.ModuleClassDescription);
            sb.AppendLine("/// </summary>");
            // 类声明
            sb.AppendLine($"public abstract class {bqjxModuleDef.BqjxModuleClassDef.ModuleClassName} : {bqjxModuleDef.BqjxModuleClassDef.ModuleBaseClassName}");
            sb.AppendLine("{");

            // 构造函数
            //sb.AppendLine($"    public {bqjxModuleClassDef.ModuleClassName}(IModuleEntity moduleEntity) : base()");
            //sb.AppendLine("    {");
            //sb.AppendLine("        this.ModuleName = moduleEntity.ModuleName;");
            //sb.AppendLine("        this.PlatFormName = moduleEntity.PlatFormName;");
            //sb.AppendLine("        _plc = moduleEntity.PLC;");
            //sb.AppendLine("        _autoCtlInfo = new AutoControlInfo();");
            //sb.AppendLine($"        _logger = MyLoggerFactory.GetLogger(typeof({bqjxModuleClassDef.ModuleClassName}));");
            //sb.AppendLine("    }");
            sb.AppendLine();

            // Execute 方法
            sb.AppendLine("    public override object Execute(Dictionary<string, object> parameters, string methodName, IGlobalStatus gs)");
            sb.AppendLine("    {");
            sb.AppendLine("        switch (methodName)");
            sb.AppendLine("        {");
            foreach (var item in bqjxModuleDef.BqjxModuleClassDef.BqjxModuleMethodDefs)
            {
                sb.AppendLine($"            case nameof({item.ModuleMethodName}):");
                if (item.BqjxModuleMethodParamDefs.Count > 0)
                {
                    foreach (var paramDef in item.BqjxModuleMethodParamDefs)
                    {
                        sb.AppendLine($"            var {paramDef.ModuleMethodParamName.ToLower()} =({paramDef.ModuleMethodParamType})parameters[{paramDef.ModuleMethodParamName}] ");
                    }
                }
                if (item.BqjxModuleMethodParamDefs.Count > 0)
                {
                    sb.AppendLine($"                return {item.ModuleMethodName}({string.Join(",", item.BqjxModuleMethodParamDefs.Select(p => p.ModuleMethodParamName.ToLower()))},gs);");
                }
                else
                {
                    sb.AppendLine($"                return {item.ModuleMethodName}(gs);");
                }
            }
            sb.AppendLine("            default:");
            sb.AppendLine("                _logger?.Error($\"method is not exist name :{methodName}\");");
            sb.AppendLine("                return 0;");
            sb.AppendLine("        }");
            sb.AppendLine("    }");
            sb.AppendLine();

            //  方法
            foreach (var item in bqjxModuleDef.BqjxModuleClassDef.BqjxModuleMethodDefs)
            {
                if (item.BqjxModuleMethodParamDefs.Count > 0)
                {
                    sb.AppendLine($"    public int {item.ModuleMethodName}({string.Join(",", item.BqjxModuleMethodParamDefs.Select(p =>$"{p.ModuleMethodParamType} {p.ModuleMethodParamName.ToLower()}"))},IGlobalStatus gs)");
                }
                else
                    sb.AppendLine($"    public int {item.ModuleMethodName}(IGlobalStatus gs)");
                sb.AppendLine("    {");
                sb.AppendLine("#if VIRTUAL");
                sb.AppendLine($"        _logger?.Info($\"{item.ModuleMethodName} \");");
                sb.AppendLine("        Thread.Sleep(5000);");
                sb.AppendLine("        return Task.FromResult(true);");
                sb.AppendLine("#endif");
                sb.AppendLine("           try");
                sb.AppendLine("           {");
                if (item.BqjxModuleMethodParamDefs.Count > 0)
                {
                    sb.AppendLine($"        var result = ExecuteCommand<float>(new float[]{string.Join(",", item.BqjxModuleMethodParamDefs.Select(p => p.ModuleMethodParamName.ToLower()))}, FuncCodeEnum.{item.FuncCodeName}, gs);");
                }
                else
                {
                    sb.AppendLine($"        var result = ExecuteCommand<float>(null, FuncCodeEnum.{item.FuncCodeName}, gs);");
                }
                sb.AppendLine("        if (result != 99999)");
                sb.AppendLine("        {");
                sb.AppendLine($"            _logger?.Warn($\"unable to {item.ModuleMethodName} code:{{result}}\");");
                sb.AppendLine("        }");
                sb.AppendLine("        return result;");
                sb.AppendLine("            }");
                sb.AppendLine("            catch (Exception ex)");
                sb.AppendLine("            {");
                sb.AppendLine("                _logger?.Error(ex.ToString());");
                sb.AppendLine("                return (int)CommandExecuteErrCode.InnerErr;");
                sb.AppendLine("            }");
                sb.AppendLine("    }");
            }
            sb.AppendLine();

            // GetEnvironmentData 方法
            sb.AppendLine("    public override Task<Dictionary<string, float>> GetEnvironmentData()");
            sb.AppendLine("    {");
            sb.AppendLine("        return Task.FromResult(new Dictionary<string, float>());");
            sb.AppendLine("    }");
            sb.AppendLine();

            // GetMonitorData 方法
            sb.AppendLine("    public override Task<Dictionary<string, float>> GetMonitorData()");
            sb.AppendLine("    {");
            sb.AppendLine("        return Task.FromResult(new Dictionary<string, float>());");
            sb.AppendLine("    }");
            sb.AppendLine();

            // SetEnvironmentData 方法
            sb.AppendLine("    public override Task<bool> SetEnvironmentData(Dictionary<string, float> floatDatas)");
            sb.AppendLine("    {");
            sb.AppendLine("        return Task.FromResult(false);");
            sb.AppendLine("    }");
            sb.AppendLine();

            // GetErrorInfoEntities 方法
            sb.AppendLine("    protected override List<ErrInfoEntity>? GetErrorInfoEntities()");
            sb.AppendLine("    {");
            sb.AppendLine("        try");
            sb.AppendLine("        {");
            sb.AppendLine($"            var stringInfo = File.ReadAllText($\"./{{nameof({bqjxModuleDef.BqjxModuleClassDef.ModuleClassName})}}AlarmInfo.json\");");
            sb.AppendLine("            return JsonHelper.ConvertToObject<List<ErrInfoEntity>>(stringInfo);");
            sb.AppendLine("        }");
            sb.AppendLine("        catch (Exception ex)");
            sb.AppendLine("        {");
            sb.AppendLine("            _logger?.Error(ex.ToString());");
            sb.AppendLine("            return null;");
            sb.AppendLine("        }");
            sb.AppendLine("    }");

            sb.AppendLine("}");

            return sb.ToString();
        }
    }


    public class ActionResult
    {
       //是否成功
       public bool IsSuccess { get; set; }


        public object ReturnValue { get; set; }

        public static ActionResult Success(object returnValue)
        {
            return new ActionResult { IsSuccess = true, ReturnValue = returnValue };
        }

        public static ActionResult Fail()
        {
            return new ActionResult { IsSuccess = false };
        }
    }

    public class ExecutionContext
    {
        public string TaskInfo { get; set; }
        public Dictionary<string,object> Datas { get; set; } = new Dictionary<string, object>();
    }

    public interface IAction
    {
        Task Execute(ExecutionContext executionContext);
    }

    /// <summary>
    /// 条件节点
    /// </summary>
    public class Condition : IAction
    {
        public IAction TrueAction { get; set; }

        public IAction FalseAction { get; set; }

        public Func<ExecutionContext, bool> ConditionFunc { get; set; }
        public async Task Execute(ExecutionContext executionContext)
        {
            if (ConditionFunc(executionContext))
            {
                await TrueAction.Execute(executionContext);
            }
            else
            {
                await FalseAction.Execute(executionContext);
            }
        }
    }

    public class LinkNode : IAction
    {
        public IAction Action { get; set; }

        public LinkNode Next { get; set; }

        public async Task Execute(ExecutionContext executionContext)
        {
            await Action.Execute(executionContext);
            var root = Next;
            if (root != null)
            {
                while (root != null)
                {
                    await Next.Execute(executionContext);
                    root = Next.Next;
                }
            }
        }
    }

    public class TreeNode : IAction
    {
        public IAction Action { get; set; }

        public List<TreeNode> Children { get; set; } = [];

        public async Task Execute(ExecutionContext executionContext)
        {
           await Traverse(this,executionContext);
        }
        internal static async Task Traverse(TreeNode treeNode, ExecutionContext executionContext)
        {
           await treeNode.Execute(executionContext);
            foreach (var item in treeNode.Children)
            {
                await Traverse(item, executionContext);
            }
        }
    }

    public class ForEach : IAction
    {
        public List<IAction> Actions { get; set; } = [];

        public bool IsParallel { get; set; }

        public async Task Execute(ExecutionContext executionContext)
        {
            if (IsParallel)
            {
                foreach (var item in Actions.AsParallel())
                {
                    await item.Execute(executionContext);
                }
            }
            else
                foreach (var item in Actions)
                {
                    await item.Execute(executionContext);
                }
        }
    }

    public class StartAction : IAction
    {
        private readonly Action<ExecutionContext> _startAction;
        public StartAction(Action<ExecutionContext> action)
        {
            _startAction = action;
        }
        public Task Execute(ExecutionContext executionContext)
        {
            _startAction(executionContext);
            return Task.CompletedTask;
        }
    }

    public class FlowChart
    {
        private readonly List<IAction> nodes = [];
        public void AddRootNode(IAction node)
        {
            nodes.Add(node);
        }
        public void Execute(ExecutionContext executionContext)
        {
            nodes.AsParallel().ForAll(async node =>
            {
                await node.Execute(executionContext);
            });
        }
    }



    //public class Node
    //{
    //    public string Name { get; set; }

    //    public List<Pin> InputPins { get; set; } = [];

    //    public List<Pin> OutputPins { get; set; } = [];
    //}

    //public class Pin
    //{
    //    public List<Node> Connections { get; set; } = [];
    //}


    public class Sample
    {
        public Sample(string name)
        {
            SampleName = name;
        }
        public string SampleName { get; set; }
    }

    public interface IBlackboard
    {
        
    }

    public interface IBlock
    {
        bool Execute(IBlackboard blackboard);
    }

    public class Module : IBlock
    {

        public bool Execute(IBlackboard blackboard)
        {
            return true;
        }
    }

    public class Platform : IBlock
    {
        public bool Execute(IBlackboard blackboard)
        {
            return true;
        }
    }

}
