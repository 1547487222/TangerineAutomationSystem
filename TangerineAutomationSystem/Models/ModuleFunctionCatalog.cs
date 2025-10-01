using System;
using System.Collections.Generic;

namespace TangerineAutomationSystem.Models
{
    /// <summary>
    /// 模块功能定义 - 定义单个模块可执行的功能/动作
    /// </summary>
    public class ModuleFunction
    {
        public string Id { get; set; } = Guid.NewGuid().ToString();
        public string Name { get; set; } = "NewFunction";
        public string DisplayName { get; set; } = "新功能";
        public string Description { get; set; } = string.Empty;
        public string ModuleType { get; set; } = string.Empty; // 关联到模块类型
        public Dictionary<string, string> Parameters { get; set; } = new();
        public string ParametersSchemaJson { get; set; } = "{}";
    }

    /// <summary>
    /// 模块功能目录 - 全局管理所有可用的模块功能
    /// </summary>
    public class ModuleFunctionCatalog
    {
        public List<ModuleFunction> Functions { get; set; } = new();

        public ModuleFunction? GetFunction(string id)
        {
            return Functions.Find(f => f.Id == id);
        }

        public List<ModuleFunction> GetFunctionsByModuleType(string moduleType)
        {
            return Functions.FindAll(f => f.ModuleType == moduleType);
        }
    }
}
