using Microsoft.Extensions.Logging;
using QStandaedPlatform.Engine.Common.Common;
using QStandaedPlatform.Engine.Common.Common.Metadatas;
using QStandaedPlatform.Engine.Common.Common.ModuleEntitys;
using QStandaedPlatform.Engine.Laboratory;
using QStandaedPlatform.Engine.Extensions;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Foundation.Modules.Models;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace System.Foundation.Modules.ManualControls
{
    [DisplayName("模块初始化")]
    public class ModuleManualStructurer : ModuleWithParameterToolBase
    {
        private const string MODULE_NAME = "模块初始化";

        //触发模块初始化
        private const string TRIGGER_MODULE_INIT = "触发模块初始化";
        //模块初始化完成
        private const string MODULE_INIT_COMPLETE = "模块初始化完成";

        public override string DefineName => MODULE_NAME;

        public override bool InitPins()
        {
            InsetPin(TRIGGER_MODULE_INIT,this,typeof(QData), PinType.Input);
            InsetPin(MODULE_INIT_COMPLETE, this, typeof(QData), PinType.Output);
            return true;
        }

        public override async Task<bool> ExecuteAsync(PinInfo pinInfo, QData pinData, ToolExecutionContext toolContext)
        {
            var parameter = Extensions.GetParameter();
            foreach (var item in GetModular().ModuleFuncCodeParameter.FuncCodeParamterInfos)
            {
                if (parameter.TryGetValue(item.ParameterAddress, out _))
                {
                    parameter[item.ParameterAddress] = item.ParameterValueFactory["0"];
                }
            }
            Logger.LogInformation("写入参数值:{values}", string.Join(",", parameter.Values));
            await GetModular().WriteParameterAsync([.. parameter.Values]);
            await GetModular().HomeAsync();
            Logger.LogInformation("模块初始化完成");
            SendToPin(MODULE_INIT_COMPLETE, new QData());
            return true;
        }
    }
}
