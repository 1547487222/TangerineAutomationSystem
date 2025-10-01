using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CodeGenerationTask.BqjxModuleDefs
{
    public class BqjxModuleClassDef
    {
        /// <summary>
        /// 模块名称
        /// </summary>
        [DisplayName("模块名称")]
        public string ModuleClassName { get; set; }

        [DisplayName("模块描述")]
        public string ModuleClassDescription { get; set; }

        [DisplayName("是否抽象类")]
        public bool IsAbstract { get; set; } = true;
        /// <summary>
        /// 模块基类名称
        /// </summary>
        [DisplayName("模块基类名称")]
        public string ModuleBaseClassName { get; set; }

        [TypeConverter(typeof(ExpandableObjectConverter))]
        [DisplayName("模块属性定义")]
        public List<BqjxModulePropertyDef> BqjxModulePropertyDefs { get; set; } = [];
        [TypeConverter(typeof(ExpandableObjectConverter))]
        [DisplayName("模块方法定义")]
        public List<BqjxModuleMethodDef> BqjxModuleMethodDefs { get; set; } = [];
    }
}
