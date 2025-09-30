using Newtonsoft.Json;
using System.IO;
using TangerineAutomationSystem.Models;

namespace TangerineAutomationSystem.Services
{
    public static class ProjectPersistenceService
    {
        public static void SaveToFile(LaboratoryModel model, string filePath)
        {
            var json = JsonConvert.SerializeObject(model, Formatting.Indented);
            File.WriteAllText(filePath, json);
        }

        public static LaboratoryModel? LoadFromFile(string filePath)
        {
            var json = File.ReadAllText(filePath);
            var model = JsonConvert.DeserializeObject<LaboratoryModel>(json);
            return model;
        }
    }
}