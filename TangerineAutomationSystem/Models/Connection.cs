using System;
namespace TangerineAutomationSystem.Models
{
    // 占位：最小化 Connection 定义，便于序列化与绘图逻辑使用
    public class Connection
    {
        public string Id { get; set; } = Guid.NewGuid().ToString();
        public string FromNodeId { get; set; } = string.Empty;
        public string FromPort { get; set; } = string.Empty;
        public string ToNodeId { get; set; } = string.Empty;
        public string ToPort { get; set; } = string.Empty;
        // 可扩展元数据（比如条件/权重/序号等）
        public string? MetaJson { get; set; }
    }
}