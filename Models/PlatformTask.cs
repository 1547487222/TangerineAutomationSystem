using System;
using System.Collections.Generic;
using Newtonsoft.Json;

namespace TangerineAutomationSystem.Models
{
    public class PlatformTask
    {
        public string Id { get; set; } = Guid.NewGuid().ToString();
        public string Name { get; set; } = "NewPlatformTask";
        public string Description { get; set; } = string.Empty;
        public List<string> ModuleActionIds { get; set; } = new();
        public List<string> ResourceIds { get; set; } = new();
        [JsonIgnore]
        public object Option { get; set; } = new { };
        public string OptionJson { get; set; } = "{}";
        
        // Platform task has its own internal flow (platform-level flow)
        public ProcessFlow InternalFlow { get; set; } = new ProcessFlow { IsPlatformLevel = true };
    }
}