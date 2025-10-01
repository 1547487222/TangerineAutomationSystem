using System.Collections.Generic;
using TangerineAutomationSystem.Models;

namespace TangerineAutomationSystem.Services
{
    /// <summary>
    /// 模块功能目录初始化服务
    /// 提供默认的模块功能定义
    /// </summary>
    public static class ModuleFunctionCatalogInitializer
    {
        /// <summary>
        /// 初始化默认的模块功能目录
        /// </summary>
        public static ModuleFunctionCatalog CreateDefaultCatalog()
        {
            var catalog = new ModuleFunctionCatalog();

            // H5U模块的默认功能
            catalog.Functions.Add(new ModuleFunction
            {
                Name = "H5U_MoveToPosition",
                DisplayName = "移动到位置",
                ModuleType = "H5U",
                Description = "控制H5U模块移动到指定位置",
                Parameters = new Dictionary<string, string>
                {
                    { "TargetX", "double" },
                    { "TargetY", "double" },
                    { "TargetZ", "double" },
                    { "Speed", "double" }
                }
            });

            catalog.Functions.Add(new ModuleFunction
            {
                Name = "H5U_PickItem",
                DisplayName = "拾取物品",
                ModuleType = "H5U",
                Description = "使用H5U模块拾取物品",
                Parameters = new Dictionary<string, string>
                {
                    { "ItemId", "string" },
                    { "GripForce", "double" }
                }
            });

            catalog.Functions.Add(new ModuleFunction
            {
                Name = "H5U_PlaceItem",
                DisplayName = "放置物品",
                ModuleType = "H5U",
                Description = "使用H5U模块放置物品",
                Parameters = new Dictionary<string, string>
                {
                    { "TargetLocation", "string" }
                }
            });

            // 仓库模块的默认功能
            catalog.Functions.Add(new ModuleFunction
            {
                Name = "Warehouse_Store",
                DisplayName = "存储到仓库",
                ModuleType = "Warehouse",
                Description = "将物品存储到仓库指定位置",
                Parameters = new Dictionary<string, string>
                {
                    { "ItemId", "string" },
                    { "Slot", "string" }
                }
            });

            catalog.Functions.Add(new ModuleFunction
            {
                Name = "Warehouse_Retrieve",
                DisplayName = "从仓库取出",
                ModuleType = "Warehouse",
                Description = "从仓库取出物品",
                Parameters = new Dictionary<string, string>
                {
                    { "ItemId", "string" }
                }
            });

            // AGV模块的默认功能
            catalog.Functions.Add(new ModuleFunction
            {
                Name = "AGV_MoveTo",
                DisplayName = "AGV移动",
                ModuleType = "AGV",
                Description = "控制AGV移动到指定工位",
                Parameters = new Dictionary<string, string>
                {
                    { "StationId", "string" },
                    { "Priority", "int" }
                }
            });

            catalog.Functions.Add(new ModuleFunction
            {
                Name = "AGV_LoadCargo",
                DisplayName = "AGV装载",
                ModuleType = "AGV",
                Description = "AGV装载货物",
                Parameters = new Dictionary<string, string>
                {
                    { "CargoId", "string" },
                    { "Weight", "double" }
                }
            });

            catalog.Functions.Add(new ModuleFunction
            {
                Name = "AGV_UnloadCargo",
                DisplayName = "AGV卸载",
                ModuleType = "AGV",
                Description = "AGV卸载货物",
                Parameters = new Dictionary<string, string>
                {
                    { "TargetStation", "string" }
                }
            });

            // 机器人模块的默认功能
            catalog.Functions.Add(new ModuleFunction
            {
                Name = "Robot_Initialize",
                DisplayName = "机器人初始化",
                ModuleType = "Robot",
                Description = "初始化机器人到home位置",
                Parameters = new Dictionary<string, string>()
            });

            catalog.Functions.Add(new ModuleFunction
            {
                Name = "Robot_ExecuteProgram",
                DisplayName = "执行程序",
                ModuleType = "Robot",
                Description = "执行预定义的机器人程序",
                Parameters = new Dictionary<string, string>
                {
                    { "ProgramName", "string" },
                    { "LoopCount", "int" }
                }
            });

            // 通用模块功能
            catalog.Functions.Add(new ModuleFunction
            {
                Name = "Common_Wait",
                DisplayName = "等待",
                ModuleType = "Common",
                Description = "等待指定时间",
                Parameters = new Dictionary<string, string>
                {
                    { "Duration", "double" }
                }
            });

            catalog.Functions.Add(new ModuleFunction
            {
                Name = "Common_Signal",
                DisplayName = "发送信号",
                ModuleType = "Common",
                Description = "发送信号给其他模块",
                Parameters = new Dictionary<string, string>
                {
                    { "SignalName", "string" },
                    { "TargetModule", "string" }
                }
            });

            return catalog;
        }

        /// <summary>
        /// 获取指定模块类型的功能列表
        /// </summary>
        public static List<ModuleFunction> GetFunctionsForModuleType(string moduleType)
        {
            var catalog = CreateDefaultCatalog();
            return catalog.GetFunctionsByModuleType(moduleType);
        }
    }
}
