using QStandaedPlatform.Engine.Common.Common;
using QStandaedPlatform.Engine.Common.Common.Metadatas;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace System.Foundation.Modules.ManualControls
{
    public class VariableCycleSignalToolData : ModuleData
    {
        //float变量预设值
        [DisplayName("float变量预设值")]
        public float FloatVariableValue { get; set; }
        //float变量读取地址
        [DisplayName("float变量读取地址")]
        public string FloatVariableAddress { get; set; }

        //float变量名
        [DisplayName("float变量名")]
        public string FloatVariableName { get; set; }

        //int变量预设值
        [DisplayName("int变量预设值")]

        public int IntVariableValue { get; set; }

        //int变量读取地址
        [DisplayName("int变量读取地址")]
        public string IntVariableAddress { get; set; }

        //int变量名
        [DisplayName("int变量名")]
        public string IntVariableName { get; set; }

        //bool变量预设值
        [DisplayName("bool变量预设值")]
        public bool BoolVariableValue { get; set; }

        //bool变量读取地址
        [DisplayName("bool变量读取地址")]
        public string BoolVariableAddress { get; set; }

        //bool变量名
        [DisplayName("bool变量名")]
        public string BoolVariableName { get; set; }

        //读取间隔
        [DisplayName("读取间隔/ms")]
        public int ReadInterval { get; set; } = 1000;

        //是否输出当前循环值
        [DisplayName("是否输出当前循环值")]
        public bool IsOutputCurrentValue { get; set; } = false;

        //是否打印文本
        [DisplayName("是否打印文本")]
        public bool IsPrintText { get; set; } = true;


        //打印文本根目录
        [DisplayName("打印文本根目录")]
        public string PrintTextRootPath { get; set; } = "D:\\QStandaedPlatform\\PrintText";

        //打印文本文件名
        [DisplayName("打印文本文件名")]
        public string PrintTextFileName { get; set; } = "VariableCycleSignalTool.csv";


    }


    [DisplayName("变量循环读取信号")]
    public class VariableCycleSignalTool : ModuleToolBase
    {
        //float变量循环读取信号
        private const string floatVariableCycleSignal = "float变量循环读取信号";
        //int变量循环读取信号
        private const string intVariableCycleSignal = "int16变量循环读取信号";
        //bool变量循环读取信号
        private const string boolVariableCycleSignal = "bool变量循环读取信号";

        //输入float当前值
        private const string floatVariableCurrentValue = "float变量当前值";
        //输入int当前值
        private const string intVariableCurrentValue = "int16变量当前值";
        //输入bool当前值
        private const string boolVariableCurrentValue = "bool变量当前值";

        //到达float变量预设值信号
        private const string floatVariableReachSignal = "到达float变量预设值信号";
        //到达int变量预设值信号
        private const string intVariableReachSignal = "到达int16变量预设值信号";
        //到达bool变量预设值信号
        private const string boolVariableReachSignal = "到达bool变量预设值信号";



        public override string DefineName => "变量循环读取信号";

        private readonly System.Collections.Concurrent.BlockingCollection<KeyValuePair<string, QData>> dataQueue = new System.Collections.Concurrent.BlockingCollection<KeyValuePair<string, QData>>();
        private ConcurrentBag<DateTime> cacheTimes = new ConcurrentBag<DateTime>();
        public override bool InitPins()
        {
            InsetPin(floatVariableCycleSignal, this, typeof(QData), PinType.Input);
            InsetPin(intVariableCycleSignal, this, typeof(QData), PinType.Input);
            InsetPin(boolVariableCycleSignal, this, typeof(QData), PinType.Input);
            InsetPin(floatVariableReachSignal, this, typeof(QData), PinType.Output);
            InsetPin(intVariableReachSignal, this, typeof(QData), PinType.Output);
            InsetPin(boolVariableReachSignal, this, typeof(QData), PinType.Output);
            InsetPin(floatVariableCurrentValue, this, typeof(QFloat), PinType.Output);
            InsetPin(intVariableCurrentValue, this, typeof(QInt16), PinType.Output);
            InsetPin(boolVariableCurrentValue, this, typeof(QBoolean), PinType.Output);
            return true;
        }

        public override bool InitDataContext()
        {
            DataContext = new VariableCycleSignalToolData();
            return true;
        }

        public string TextPrintPath =>Context< VariableCycleSignalToolData >().PrintTextRootPath;

        public string TextPrintFileName => Context<VariableCycleSignalToolData>().PrintTextFileName;

       
        public override async Task<bool> ExecuteAsync(PinInfo pinInfo, QData pinData, ToolExecutionContext toolContext)
        {
            if (h5UTcp == null)
                throw new Exception("未找到H5UTcp");
            var context = Context<VariableCycleSignalToolData>();
            if (pinInfo.Name == floatVariableCycleSignal)
            {
                var dir = Path.Combine(TextPrintPath, DateTime.Now.ToString("yyyy_MM_dd"));
                if (!Directory.Exists(dir))
                {
                    Directory.CreateDirectory(dir);
                }

                var file = Path.Combine(dir, TextPrintFileName + ".csv");

                if (!File.Exists(file))
                {
                    using (File.Create(file)) { }
                }

                using var fs = new FileStream(file, FileMode.Append, FileAccess.Write, FileShare.Read);
                using var sw = new StreamWriter(fs) { AutoFlush = true };

                var headerValues = new List<string>
                                        {
                                            "写入时间",
                                            context.FloatVariableName
                };
                sw.WriteLine(" ");
                sw.WriteLine(string.Join(",", headerValues));
                while (!RequestCancelToken.IsCancellationRequested)
                {
                    var floatValue = await h5UTcp.ReadSingleValueAsync<float>(context.FloatVariableAddress);
                    if (floatValue <= context.FloatVariableValue)
                    {
                        SendToPin(floatVariableReachSignal, new QData());
                        var dateTime = DateTime.Now;
                        cacheTimes.Add(dateTime);
                        sw.WriteLine($"{dateTime:yyyy-MM-dd HH:mm:ss},{floatValue}");
                        if (cacheTimes.Count > 1)
                        {
                            //计算耗时 写入耗时
                            var time = cacheTimes.First() - cacheTimes.Last();
                            sw.WriteLine("等待耗时/mins:" + time.TotalMinutes);
                            cacheTimes.Clear();
                        }
                        else
                        {
                            sw.WriteLine("等待耗时/mins:0");
                        }
                        return true;
                    }
                    if (context.IsOutputCurrentValue)
                    {
                        SendToPin(floatVariableCurrentValue, new QFloat(floatValue));
                    }
                    if (context.IsPrintText)
                    {
                        var dateTime = DateTime.Now;
                        cacheTimes.Add(dateTime);
                        sw.WriteLine($"{dateTime:yyyy-MM-dd HH:mm:ss},{floatValue}");
                    }
                    await Task.Delay(context.ReadInterval);
                }
                throw new Exception("Float循环读取被取消");
            }
            else if (pinInfo.Name == intVariableCycleSignal)
            {
                while (!RequestCancelToken.IsCancellationRequested)
                {
                    var intValue = await h5UTcp.ReadSingleValueAsync<short>(context.IntVariableAddress);
                    if (intValue <= context.IntVariableValue)
                    {
                        SendToPin(intVariableReachSignal, new QData());
                        return true;
                    }
                    if (context.IsOutputCurrentValue)
                        SendToPin(intVariableCurrentValue, new QInt16(intValue));
                    if (context.IsPrintText)
                    {
                        dataQueue.Add(new KeyValuePair<string, QData>(context.BoolVariableName, new QInt16(intValue)));
                    }
                    await Task.Delay(context.ReadInterval);
                }
                throw new Exception("Int16循环读取被取消");
            }
            else if (pinInfo.Name == boolVariableCycleSignal)
            {
                while (!RequestCancelToken.IsCancellationRequested)
                {
                    var boolValue = await h5UTcp.ReadSingleValueAsync<bool>(context.BoolVariableAddress);
                    if (boolValue == context.BoolVariableValue)
                    {
                        SendToPin(boolVariableReachSignal, new QData());
                        return true;
                    }
                    if (context.IsOutputCurrentValue)
                    {
                        SendToPin(boolVariableCurrentValue, new QBoolean(boolValue));
                        
                    }
                    if (context.IsPrintText)
                    {
                        dataQueue.Add(new KeyValuePair<string, QData>(context.BoolVariableName, new QBoolean(boolValue)));
                    }

                    await Task.Delay(context.ReadInterval);
                }
                throw new Exception("Boolean循环读取被取消");
            }
            throw new Exception("未找到对应的信号");
        }
    }
}
