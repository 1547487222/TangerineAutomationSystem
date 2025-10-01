using QStandaedPlatform.Engine.Common.Common;
using QStandaedPlatform.Engine.Common.Common.Metadatas;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace System.Foundation.Modules.CSharps
{
    [DisplayName("CSharp脚本工具")]
    public class CSharpScriptTool : ToolBase
    {
        public override string DefineName => "CSharp脚本工具";

        public override Task<bool> ExecuteAsync(PinInfo pinInfo, QData pinData, ToolExecutionContext toolContext)
        {
            return Task.FromResult(true);
           
        }
    }
}
