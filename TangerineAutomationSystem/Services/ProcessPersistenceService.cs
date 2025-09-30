using System.IO;
using Newtonsoft.Json;
using TangerineAutomationSystem.Models;

namespace TangerineAutomationSystem.Services
{
    public static class ProcessPersistenceService
    {
        public static void SaveProcessFlow(ProcessFlow flow, string filePath)
        {
            var json = JsonConvert.SerializeObject(flow, Formatting.Indented);
            File.WriteAllText(filePath, json);
        }

        public static ProcessFlow LoadProcessFlow(string filePath)
        {
            var json = File.ReadAllText(filePath);
            return JsonConvert.DeserializeObject<ProcessFlow>(json) ?? new ProcessFlow();
        }
    }
}