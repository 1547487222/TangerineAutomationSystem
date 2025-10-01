using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CodeGenerationTask.BqjxModuleDefs
{
    [TypeConverter(typeof(ExpandableObjectConverter))]
    public class BqjxModuleDef
    {
        [DisplayName("模块测试文件导出")]
        public string ModuleTestImportRootPath { get; set; }
        public string ImportPath { get; set; }
        /// <summary>
        /// 命名空间名称
        /// </summary>
        public string NamespaceName { get; set; }

        /// <summary>
        /// 模块类定义
        /// </summary>
        [TypeConverter(typeof(ExpandableObjectConverter))]
        public BqjxModuleClassDef BqjxModuleClassDef { get; set; } = new BqjxModuleClassDef();
    }
    [TypeConverter(typeof(ExpandableObjectConverter))]
    public class BqjxModuleMake
    {
        [TypeConverter(typeof(ExpandableObjectConverter))]
        public BqjxModuleDef BqjxModuleBaseDef { get; set; } = new BqjxModuleDef() 
        {
            NamespaceName= "BQJX.Modules.AutoMod.Base",
            ImportPath= "E:\\Code\\Module.Src.Projects\\Projects.bqjx.modules\\BQJX.Modules\\BQJX.Modules\\AutoMod\\Base\\",
           BqjxModuleClassDef=new BqjxModuleClassDef 
           {
                ModuleBaseClassName= "ModuleBase",
           }
        };
        [TypeConverter(typeof(ExpandableObjectConverter))]
        public ObservableCollection<BqjxModuleDef> BqjxModuleImplDef { get; set; } = [];
    }
}
