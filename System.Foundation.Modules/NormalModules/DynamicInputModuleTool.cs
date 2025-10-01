using QStandaedPlatform.Engine.Common.Common;
using QStandaedPlatform.Engine.Common.Common.Metadatas;
using QStandaedPlatform.Engine.Laboratory;
using QStandaedPlatform.Engine.Extensions;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;

namespace System.Foundation.Modules.NormalModules
{
    public class DynamicFillInputData: ModuleData
    {
        [DisplayName("D100")]
        public float D100DefaultValue { get; set; } = 0f;
        [DisplayName("D102")]
        public float D102DefaultValue { get; set; } = 0f;
        [DisplayName("D104")]
        public float D104DefaultValue { get; set; } = 0f;
        [DisplayName("D106")]
        public float D106DefaultValue { get; set;} = 0f;
        [DisplayName("D108")]
        public float D108DefaultValue { get; set; } = 0f;
        [DisplayName("D110")]
        public float D110DefaultValue { get; set; } = 0f;
        [DisplayName("D112")]
        public float D112DefaultValue { get; set; } = 0f;
        [DisplayName("D114")]
        public float D114DefaultValue { get; set; } = 0f;
        [DisplayName("D116")]
        public float D116DefaultValue { get; set; } = 0f;
        [DisplayName("D118")]
        public float D118DefaultValue { get; set; } = 0f;
        [DisplayName("D120")]
        public float D120DefaultValue { get; set; } = 0f;
        [DisplayName("D122")]
        public float D122DefaultValue { get; set; } = 0f;
        [DisplayName("D124")]
        public float D124DefaultValue { get; set; } = 0f;
        [DisplayName("D126")]
        public float D126DefaultValue { get; set; } = 0f;
        [DisplayName("D128")]
        public float D128DefaultValue { get; set; } = 0f;
        [DisplayName("D130")]
        public float D130DefaultValue { get; set; } = 0f;
        [DisplayName("D132")]
        public float D132DefaultValue { get; set; } = 0f;
        [DisplayName("D134")]
        public float D134DefaultValue { get; set; } = 0f;
        [DisplayName("D136")]
        public float D136DefaultValue { get; set; } = 0f;
        [DisplayName("D138")]
        public float D138DefaultValue { get; set; } = 0f;
        [DisplayName("D140")]
        public float D140DefaultValue { get; set; } = 0f;
        [DisplayName("D142")]
        public float D142DefaultValue { get; set; } = 0f;
        [DisplayName("D144")]
        public float D144DefaultValue { get; set; } = 0f;
        [DisplayName("D146")]
        public float D146DefaultValue { get; set; } = 0f;
        [DisplayName("D148")]
        public float D148DefaultValue { get; set; } = 0f;
        [DisplayName("D150")]
        public float D150DefaultValue { get; set; } = 0f;

        [DisplayName("启用D100")]
        public bool EnableD100 { get; set; } = false;
        [DisplayName("启用D102")]
        public bool EnableD102 { get; set; } = false;
        [DisplayName("启用D104")]
        public bool EnableD104 { get; set; } = false;
        [DisplayName("启用D106")]
        public bool EnableD106 { get; set; } = false;
        [DisplayName("启用D108")]
        public bool EnableD108 { get; set; } = false;
        [DisplayName("启用D110")]
        public bool EnableD110 { get; set; } = false;
        [DisplayName("启用D112")]
        public bool EnableD112 { get; set; } = false;
        [DisplayName("启用D114")]
        public bool EnableD114 { get; set; } = false;
        [DisplayName("启用D116")]
        public bool EnableD116 { get; set; } = false;
        [DisplayName("启用D118")]
        public bool EnableD118 { get; set; } = false;
        [DisplayName("启用D120")]
        public bool EnableD120 { get; set; } = false;
        [DisplayName("启用D122")]
        public bool EnableD122 { get; set; } = false;
        [DisplayName("启用D124")]
        public bool EnableD124 { get; set; } = false;
        [DisplayName("启用D126")]
        public bool EnableD126 { get; set; } = false;
        [DisplayName("启用D128")]
        public bool EnableD128 { get; set; } = false;
        [DisplayName("启用D130")]
        public bool EnableD130 { get; set; } = false;
        [DisplayName("启用D132")]
        public bool EnableD132 { get; set; } = false;
        [DisplayName("启用D134")]
        public bool EnableD134 { get; set; } = false;
        [DisplayName("启用D136")]
        public bool EnableD136 { get; set; } = false;
        [DisplayName("启用D138")]
        public bool EnableD138 { get; set; } = false;
        [DisplayName("启用D140")]
        public bool EnableD140 { get; set; } = false;
        [DisplayName("启用D142")]
        public bool EnableD142 { get; set; } = false;
        [DisplayName("启用D144")]
        public bool EnableD144 { get; set; } = false;
        [DisplayName("启用D146")]
        public bool EnableD146 { get; set; } = false;
        [DisplayName("启用D148")]
        public bool EnableD148 { get; set; } = false;
        [DisplayName("启用D150")]
        public bool EnableD150 { get; set; } = false;
    }

    [DisplayName(TOONALE)]
    public class DynamicFillInModuleTool : ModuleWithParameterToolBase
    {
        public const string TOONALE = "动态写参模块";
        public override string DefineName => TOONALE;


        public override bool InitPins()
        {
            InsetPin("触发默认值信号",this,typeof(QData), PinType.Input);
            InsetPin("默认值执行完成", this, typeof(QData), PinType.Output);
            for (int i = 100; i <= 150; i += 2)
            {
                InsetPin($"D{i}",this,typeof(QFloat), PinType.Input);
            }
            InsetPin("动态写参执行完成",this,typeof(QData), PinType.Output);
            return true;
        }

        public override bool InitDataContext()
        {
            DataContext =new DynamicFillInputData();
            return true;
        }

        public DynamicFillInputData Data => Context<DynamicFillInputData>();

        public List<string> GetEnableKeys()
        {
            var result = new List<string>();
            for (int i = 100; i <= 150; i += 2)
            {
                var prop = typeof(DynamicFillInputData).GetProperty($"EnableD{i}");
                if (prop != null && (bool)prop.GetValue(Data)!)
                {
                    result.Add($"D{i}");
                }
            }
            return result;
        }

        public Dictionary<string, float> GetDefaultValues()
        {
            var result = new Dictionary<string, float>();
            for (int i = 100; i <= 150; i += 2)
            {
                var prop = typeof(DynamicFillInputData).GetProperty($"EnableD{i}");
                if (prop != null && (bool)prop.GetValue(Data)!)
                {
                    prop = typeof(DynamicFillInputData).GetProperty($"D{i}DefaultValue");
                    if (prop != null)
                    {
                        result.Add($"D{i}", (float)prop.GetValue(Data));
                    }
                }
            }
            return result;
        }

        public override Task<bool> ClearEphemeralDataAsync()
        {
            _collectValues.Clear();
          return Task.FromResult(true);
        }

        private readonly Dictionary<string, float> _collectValues = [];
        public override async Task<bool> ExecuteAsync(PinInfo pinInfo, QData pinData, ToolExecutionContext toolContext)
        {
            if (pinInfo.Name=="触发默认值信号")
            {
               await Execute(GetDefaultValues());
                return true;
            }
            _collectValues[pinInfo.Name]= (float)pinData;
            if (GetEnableKeys().All(key => _collectValues.ContainsKey(key)))
            {
                await Execute(_collectValues);
                _collectValues.Clear();
                SendToPin("动态写参执行完成", QData.Empty);
                return await Task.FromResult(true);
            }
            return true;
        }


        public async Task Execute(Dictionary<string, float> paramters)
        {
            if (GetModular().ModuleInfo == null)
            {
                throw new Exception("ModuleInfo is null");
            }
            if (GetModular().ModuleFuncCodeParameter == null)
            {
                throw new Exception("ModuleFuncCodeParameter is null");
            }
            if (string.IsNullOrEmpty(GetModular().ModuleInfo.ModuleName))
            {
                throw new Exception("ModuleName is null");
            }
            RequestCancelToken.ThrowIfCancellationRequested();
            var parameter = Extensions.GetParameter();
            foreach (var item in paramters)
            {
                if (parameter.TryGetValue(item.Key, out _))
                {
                    parameter[item.Key] = item.Value;
                }
            }
            Logger.LogInformation("写入参数值:{values}", string.Join(",", parameter.Values));

            await GetModular().VerifyModuleStatusAsync();
            await GetModular().WriteParameterAsync([.. parameter.Values]);
            if (!GetModular().VerifyModuleActivityStatus())
            {
                await GetModular().ModuleExecuteAsync();
            }
            await GetModular().CheckModuleDoneAsync(RequestCancelToken);
        }
    }

    public class DynamicFillIntInputData : ModuleData
    {
        
        [DisplayName("启用D100")]
        public bool EnableD100 { get; set; } = false;
        [DisplayName("启用D101")]
        public bool EnableD101 { get; set; } = false;

        [DisplayName("启用D102")]
        public bool EnableD102 { get; set; } = false;

        [DisplayName("启用D103")]
        public bool EnableD103 { get; set; } = false;

        [DisplayName("启用D104")]
        public bool EnableD104 { get; set; } = false;

        [DisplayName("启用D105")]
        public bool EnableD105 { get; set; } = false;

        [DisplayName("启用D106")]
        public bool EnableD106 { get; set; } = false;

        [DisplayName("启用D107")]
        public bool EnableD107 { get; set; } = false;

        [DisplayName("启用D108")]
        public bool EnableD108 { get; set; } = false;

        [DisplayName("启用D109")]
        public bool EnableD109 { get; set; } = false;
        [DisplayName("启用D110")]
        public bool EnableD110 { get; set; } = false;
        [DisplayName("启用D112")]
        public bool EnableD112 { get; set; } = false;

        [DisplayName("启用D113")]
        public bool EnableD113 { get; set; } = false;

        [DisplayName("启用D114")]
        public bool EnableD114 { get; set; } = false;

        [DisplayName("启用D115")]
        public bool EnableD115 { get; set; } = false;

        [DisplayName("启用D116")]
        public bool EnableD116 { get; set; } = false;

        [DisplayName("启用D117")]
        public bool EnableD117 { get; set; } = false;

        [DisplayName("启用D118")]
        public bool EnableD118 { get; set; } = false;

        [DisplayName("启用D119")]
        public bool EnableD119 { get; set; } = false;

        [DisplayName("启用D120")]
        public bool EnableD120 { get; set; } = false;

        [DisplayName("启用D121")]
        public bool EnableD121 { get; set; } = false;

        [DisplayName("启用D122")]
        public bool EnableD122 { get; set; } = false;

        [DisplayName("启用D123")]
        public bool EnableD123 { get; set; } = false;

        [DisplayName("启用D124")]
        public bool EnableD124 { get; set; } = false;

        [DisplayName("启用D125")]
        public bool EnableD125 { get; set; } = false;

    }

    [DisplayName(intTOONALE)]
    public class DynamicFillInIntModuleTool : ModuleWithParameterToolBase
    {
        public const string intTOONALE = "动态写参int模块";
        public override string DefineName => intTOONALE;


        public override bool InitPins()
        {
            for (int i = 100; i <= 125; i += 1)
            {
                InsetPin($"D{i}", this, typeof(QInt), PinType.Input);
            }
            InsetPin("动态写参执行完成", this, typeof(QData), PinType.Output);
            return true;
        }

        public override bool InitDataContext()
        {
            DataContext = new DynamicFillIntInputData();
            return true;
        }

        public DynamicFillIntInputData Data => Context<DynamicFillIntInputData>();

        public List<string> GetEnableKeys()
        {
            var result = new List<string>();
            for (int i = 100; i <= 125; i += 1)
            {
                var prop = typeof(DynamicFillIntInputData).GetProperty($"EnableD{i}");
                if (prop != null && (bool)prop.GetValue(Data)!)
                {
                    result.Add($"D{i}");
                }
            }
            return result;
        }


        public override async Task<bool> ClearEphemeralDataAsync()
        {

            _collectValues.Clear();
            return await Task.FromResult(true);
        }


        private readonly Dictionary<string, short> _collectValues = [];
        public override async Task<bool> ExecuteAsync(PinInfo pinInfo, QData pinData, ToolExecutionContext toolContext)
        {
            _collectValues[pinInfo.Name] = (short)(int)pinData;
            if (GetEnableKeys().All(key => _collectValues.ContainsKey(key)))
            {
                await Execute(_collectValues);
                _collectValues.Clear();
                SendToPin("动态写参执行完成", QData.Empty);
                return await Task.FromResult(true);
            }
            return true;
        }


        public async Task Execute(Dictionary<string, short> paramters)
        {
            if (GetModular().ModuleInfo == null)
            {
                throw new Exception("ModuleInfo is null");
            }
            if (GetModular().ModuleFuncCodeParameter == null)
            {
                throw new Exception("ModuleFuncCodeParameter is null");
            }
            if (string.IsNullOrEmpty(GetModular().ModuleInfo.ModuleName))
            {
                throw new Exception("ModuleName is null");
            }
            RequestCancelToken.ThrowIfCancellationRequested();
            var parameter = Enumerable.Range(100, 25).ToDictionary(i => $"D{i}", i => (short)0);
            foreach (var item in paramters)
            {
                if (parameter.TryGetValue(item.Key, out _))
                {
                    parameter[item.Key] = item.Value;
                }
            }
            Logger.LogInformation("写入参数值:{values}", string.Join(",", parameter.Values));
            await GetModular().WriteParameterAsync(parameter.Values.ToArray());
            await GetModular().VerifyModuleStatusAsync();
            if (!GetModular().VerifyModuleActivityStatus())
            {
                await GetModular().ModuleExecuteAsync();
            }
            await GetModular().CheckModuleDoneAsync(RequestCancelToken);
        }
    }





    //类型转换器
    [DisplayName(TOOL_NAME)]
    public class QTypeConverter : ToolBase
    {
        public const string TOOL_NAME = "类型转换器";

        public override string DefineName => TOOL_NAME;


        public override bool InitPins()
        {
            InsetPin("Int转Float", this, typeof(QInt), PinType.Input);
            InsetPin("String转Int", this, typeof(QString), PinType.Input);
            InsetPin("Float转Int", this, typeof(QFloat), PinType.Input);
            InsetPin("Int转String", this, typeof(QInt), PinType.Input);
            InsetPin("String转Float", this, typeof(QString), PinType.Input);
            InsetPin("Float转String", this, typeof(QFloat), PinType.Input);

            InsetPin("Int转换Float结果", this, typeof(QFloat), PinType.Output);
            InsetPin("String转换Int结果", this, typeof(QInt), PinType.Output);
            InsetPin("Float转换Int结果", this, typeof(QInt), PinType.Output);
            InsetPin("Int转换String结果", this, typeof(QString), PinType.Output);
            InsetPin("Float转换String结果", this, typeof(QString), PinType.Output);
            InsetPin("String转换Float结果", this, typeof(QFloat), PinType.Output);

            return true;
        }

        public override Task<bool> ExecuteAsync(PinInfo pinInfo, QData pinData, ToolExecutionContext toolContext)
        {
            if (pinInfo.Name == "Int转Float")
            {
                var result = (float)(int)pinData;
                SendToPin("Int转换Float结果", (QFloat)result);
                return Task.FromResult(true);
            }
            if (pinInfo.Name == "String转Int")
            {
                if (int.TryParse((string)pinData, out var result))
                {
                    SendToPin("String转换Int结果", (QInt)result);
                    return Task.FromResult(true);
                }
                else
                {
                    Logger.LogError("String转Int失败");
                }
            }
            if (pinInfo.Name == "Float转Int")
            {
                var result = (int)(float)pinData;
                SendToPin("Float转换Int结果", (QInt)result);
                return Task.FromResult(true);
            }
            if (pinInfo.Name == "Int转String")
            {
                var result = ((int)pinData).ToString();
                SendToPin("Int转换String结果", (QString)result);
            }
            if (pinInfo.Name == "String转Float")
            {
                if (float.TryParse((string)pinData, out var result))
                {
                    SendToPin("String转换Float结果", (QFloat)result);
                }
                else
                {
                    Logger.LogError("String转Float失败");
                }
            }
            if (pinInfo.Name == "Float转String")
            {
                var result = ((float)pinData).ToString();
                SendToPin("Float转换String结果", (QString)result);
                return Task.FromResult(true);
            }
            return Task.FromResult(false);
        }
    }

}
