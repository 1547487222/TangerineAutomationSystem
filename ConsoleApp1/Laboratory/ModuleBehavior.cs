using QStandaedPlatform.Engine.Laboratory.Documents;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace QStandaedPlatform.Engine.Laboratory
{
    public class ModuleBehavior(ModuleFuncCodeParameter moduleFuncCodeParameter)
    {
        private readonly ModuleFuncCodeParameter _funcCodeParameter = moduleFuncCodeParameter;

        public bool IsProductLegacy()
        {
            return _funcCodeParameter.IsProductLegacy;
        }

        public ModuleFuncCodeParameter Behavior => _funcCodeParameter;
    }
}
