using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CodeGenerationTask.BqjxModuleDefs
{
    [TypeConverter(typeof(ExpandableObjectConverter))]
    public class ExecuteCommandInfo
    {
        /// <summary>
        /// 命令寄存器
        /// </summary>
        public string CommandRegister { get; set; } = "D210";

        /// <summary>
        /// 状态寄存器
        /// </summary>
        public string StatusRegister { get; set; } = "D200";

        /// <summary>
        /// 参数寄存器
        /// </summary>
        public string ParametersRegister { get; set; } = "D100";

        /// <summary>
        /// 回读数据起始寄存器地址 如读码寄存器
        /// </summary>
        public string ReadDataRegister { get; set; } = "R100";

        /// <summary>
        /// EHS获取地址
        /// </summary>
        public string EHSAddress { get; set; } = "D7800";

        /// <summary>
        /// EBR实验结果地址
        /// </summary>
        public string EBRFloatAddress { get; set; } = "R200";

        /// <summary>
        /// 环境参数地址
        /// </summary>
        public string EnvironmentDataAddress { get; set; } = "D7880";

    }
    [TypeConverter(typeof(ExpandableObjectConverter))]
    public class BqjxModuleMethodDef
    {
        [DisplayName("方法名称")]
        public string ModuleMethodName { get; set; }
        [DisplayName("功能码")]
        public int FuncCode { get; set; }
        [TypeConverter(typeof(ExpandableObjectConverter))]
        [DisplayName("执行命令信息")]
        public ExecuteCommandInfo ExecuteCommandInfo { get; set; } = new ExecuteCommandInfo();
        [DisplayName("方法描述")]
        public string ModuleMethodDescription { get; set; }
        [DisplayName("返回值描述")]
        public string ReturnsValueDescription { get; set; }
        [DisplayName("是否独立参数")]
        public bool IsIndependent { get; set; }
        [TypeConverter(typeof(ExpandableObjectConverter))]
        [DisplayName("方法参数")]
        public List<BqjxModuleMethodParamDef> BqjxModuleMethodParamDefs { get; set; } = [];

        public override string ToString()
        {
            return ModuleMethodName;
        }
    }
}
