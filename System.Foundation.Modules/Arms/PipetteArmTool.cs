using Microsoft.Extensions.Logging;
using QStandaedPlatform.Engine.Common.Common;
using QStandaedPlatform.Engine.Common.Common.Metadatas;
using QStandaedPlatform.Engine.Laboratory;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Foundation.Modules.Arms.Pipettes;
using System.Foundation.Modules.Models;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace System.Foundation.Modules.Arms
{
   
  

    [DisplayName("移液枪搬运工具")]
    public class PipetteArmTool : SyncInputModuleWithParameterToolBase
    {
        public PipetteArmToolData PipetteArmToolData { get; set; } = new PipetteArmToolData();
        public override string DefineName => "移液枪搬运工具";

        private readonly PipetteService _pipetteService;
        public PipetteArmTool(PipetteService pipetteService)
        {
            _pipetteService = pipetteService;
        }

        public override bool InitPins()
        {
            InsetPin("输入搬运信号", this, typeof(QData), PinType.Input);
            InsetPin("输入搬运位置", this, typeof(QPosition), PinType.Input);
            InsetPin("输出搬运完成信号", this, typeof(QData), PinType.Output);
            return true;
        }

        public override bool InitDataContext()
        {
            DataContext = PipetteArmToolData;
            return true;
        }

        public override async Task<bool> ExexuteAsync(Dictionary<PinInfo, QData> pinDatas, ToolExecutionContext toolContext)
        {
            var dataContext = Context<PipetteArmToolData>();
            if (pinDatas.FirstOrDefault(p => p.Key.PinType == PinType.Input && p.Key.Name == "输入搬运位置").Value is QPosition position)
            {
                var temp = (QPosition)position.Clone();

                temp.X += dataContext.XOffset;
                temp.Y += dataContext.YOffset;
                temp.Z += dataContext.ZOffset;
                temp.Z2 += dataContext.Z2Offset;
                Logger.LogInformation($"移液枪搬运工具，目标位置：{position}");

               await _pipetteService.PipetteAsync(GetModular(),temp, PipetteType.LiquorRelief,Logger);
           
                SendToPin("输出搬运完成信号", new QData());
                return true;
            }
            return false;
        }
    }
}

