namespace TangerineAutomationSystem.Models
{
    // 占位：资源定义（仓库、AGV、机器人等）
    public class ResourceDefinition
    {
        public string Id { get; set; } = System.Guid.NewGuid().ToString();
        public string Name { get; set; } = "NewResource";
        public string ResourceType { get; set; } = "Warehouse";
        public string Description { get; set; } = string.Empty;
        public string ConfigJson { get; set; } = "{}";
    }
}