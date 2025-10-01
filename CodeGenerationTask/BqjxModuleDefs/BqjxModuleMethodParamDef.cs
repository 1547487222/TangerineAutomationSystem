using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CodeGenerationTask.BqjxModuleDefs
{
    [TypeConverter(typeof(ExpandableObjectConverter))]
    public class BqjxModuleMethodParamDef
    {
        [DisplayName("参数名称")]
        public string ModuleMethodParamName { get; set; }
        [DisplayName("参数类型")]
        public string ModuleMethodParamType { get; set; } = "float";
        [DisplayName("参数描述")]
        public string ModuleMethodParamDescription { get; set; }

        [DisplayName("参数下标")]
        public int Index { get; set; }

        public override string ToString()
        {
            return ModuleMethodParamName;
        }
    }
}
