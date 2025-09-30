using System.Collections.Generic;

namespace TangerineAutomationSystem.Models
{
    public class LaboratoryConfig
    {
        public string Id { get; set; } = System.Guid.NewGuid().ToString();
        public string Name { get; set; } = "Laboratory";
        public List<ProductionLineModel> ProductionLines { get; set; } = new();
        public List<ModuleDefinition> ModuleDefinitions { get; set; } = new();
        public List<ResourceDefinition> ResourceDefinitions { get; set; } = new();
        public List<ProductionLineProcess> ProductionLineProcesses { get; set; } = new();
        public List<PlatformTask> PlatformTasks { get; set; } = new();
    }
}