using QStandaedPlatform.Engine.Common.Common;
using QStandaedPlatform.Engine.Common.Common.Metadatas;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace System.Foundation.Modules.ManualControls
{
    [DisplayName("文本打印工具")]
    public class TextPrintTool : DynamicSyncInputPinTool
    {
        public class TextPrintToolConfig:DynamicPinToolData
        {
            [DisplayName("文本打印根路径")]
            public string TextPrintPath { get; set; }

            [DisplayName("文本打印文件名")]
            public string TextPrintFileName { get; set; }

            [DisplayName("表头")]
            public List<string> Headers { get; set; } = [];
        }


        private readonly System.Collections.Concurrent.BlockingCollection<Dictionary<PinInfo, QData>> dataQueue = new System.Collections.Concurrent.BlockingCollection<Dictionary<PinInfo, QData>>();
        public override string DefineName => "文本打印工具";


        public override bool InitPins()
        {
            InsetPin("输入打印值",this,typeof(QData), PinType.Input);
            return true;
        }


        public override bool InitDataContext()
        {
            DataContext = new TextPrintToolConfig();
            return true;
        }

        public override bool InitEnd()
        {
            //Task.Run(async () => 
            //{
            //    while (!RequestCancelToken.IsCancellationRequested)
            //    {
            //        foreach (var pinDatas in dataQueue.GetConsumingEnumerable(RequestCancelToken))
            //        {
            //            if (pinDatas != null)
            //            {
            //                while (!RequestCancelToken.IsCancellationRequested)
            //                {
            //                    try
            //                    {
            //                        var dir = Path.Combine(TextPrintPath, DateTime.Now.ToString("yyyy_MM_dd"));
            //                        if (!Directory.Exists(dir))
            //                        {
            //                            Directory.CreateDirectory(dir);
            //                        }

            //                        var file = Path.Combine(dir, TextPrintFileName + ".csv");

            //                        if (!File.Exists(file))
            //                        {
            //                            using (File.Create(file)) { }
            //                        }

            //                        using var fs = new FileStream(file, FileMode.Append, FileAccess.Write, FileShare.Read);
            //                        using var sw = new StreamWriter(fs) { AutoFlush = true };

            //                        if (new FileInfo(file).Length == 0 && Headers.Count > 0)
            //                        {
            //                            var headerValues = new List<string>
            //    {
            //        "写入时间"
            //    };
            //                            headerValues.AddRange(Headers);
            //                            sw.WriteLine(string.Join(",", headerValues));
            //                        }

            //                        var line = new List<string>
            //{
            //    DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss")
            //};
            //                        foreach (var header in Headers)
            //                        {
            //                            var pin = pinDatas.Keys.FirstOrDefault(p => p.Name == header);
            //                            if (pin != null && pinDatas.TryGetValue(pin, out var data))
            //                            {
            //                                line.Add(data?.ToString() ?? "");
            //                            }
            //                            else
            //                            {
            //                                line.Add("");
            //                            }
            //                        }

            //                        sw.WriteLine(string.Join(",", line));
            //                        break;
            //                    }
            //                    catch (Exception ex) when (ex is IOException iOException)
            //                    {
            //                        await this.RaiseToolStateChangeAsync(ToolState.Forbidden, "请勿采用WPS打开文件，否则会报错:" + iOException.ToString());
            //                        Thread.Sleep(1000);
            //                    }
            //                }
            //            }
            //        }
            //    }
            //});
            return true;
        }

        public string TextPrintPath => Context<TextPrintToolConfig>().TextPrintPath;

        public string TextPrintFileName => Context<TextPrintToolConfig>().TextPrintFileName;

        public List<string> Headers => Context<TextPrintToolConfig>().Headers;

        

        public override void HandleRequestAddNewPin(object dynamicPinData)
        {
            if (Headers.Count > 0)
            {
                foreach (var item in Headers)
                {
                    InsetPin(item, this, typeof(QData), PinType.Input,true);
                }
            }
        }

        public override async Task<bool> ExexuteAsync(Dictionary<PinInfo, QData> pinDatas, ToolExecutionContext toolContext)
        {
            dataQueue.Add(pinDatas);
            return await Task.FromResult(true);
        }
    }
}
