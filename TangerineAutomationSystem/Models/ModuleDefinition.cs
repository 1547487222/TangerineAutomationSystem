using System;

namespace TangerineAutomationSystem.Models
{
    // 占位：模块目录项的最小信息，便于 ModuleCatalog/拖拽/默认配置使用
    public class ModuleDefinition
    {
        public string Id { get; set; } = Guid.NewGuid().ToString();
        public string ProviderName { get; set; } = string.Empty; // e.g. H5u provider identifier
        public string DisplayName { get; set; } = "NewModule";
        public string Description { get; set; } = string.Empty;
        // 可用于在 UI 中反序列化默认配置
        public string DefaultConfigJson { get; set; } = "{}";
        public string ConfigTypeName { get; set; } = string.Empty; // 如果需要反射生成表单
    }
}