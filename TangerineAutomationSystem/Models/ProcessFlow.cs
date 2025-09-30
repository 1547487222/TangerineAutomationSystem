using System;
using System.Collections.Generic;

namespace TangerineAutomationSystem.Models
{
    public class ProcessFlow
    {
        public string Id { get; set; } = Guid.NewGuid().ToString();
        public string Name { get; set; } = "NewProcessFlow";
        public string Description { get; set; } = string.Empty;
        public List<FlowNode> Nodes { get; set; } = new();
        public List<Connection> Connections { get; set; } = new();
        public bool IsPlatformLevel { get; set; } = false;
        public Dictionary<string, string> Meta { get; set; } = new();
    }
}