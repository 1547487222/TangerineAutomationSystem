using System;
using System.Collections.Generic;

namespace TangerineAutomationSystem.Models
{
    public class ProductionLineProcess
    {
        public string Id { get; set; } = Guid.NewGuid().ToString();
        public string Name { get; set; } = "NewLineProcess";
        public string Description { get; set; } = string.Empty;
        public List<ProcessFlow> Flows { get; set; } = new();
        public string ProductionLineId { get; set; } = string.Empty;
    }
}