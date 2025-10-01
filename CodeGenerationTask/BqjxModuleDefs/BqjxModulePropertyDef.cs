using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CodeGenerationTask.BqjxModuleDefs
{
    [TypeConverter(typeof(ExpandableObjectConverter))]
    public class BqjxModulePropertyDef
    {
        public string ModulePropertyName { get; set; }

        public string ModulePropertyType { get; set; }
    }
}
