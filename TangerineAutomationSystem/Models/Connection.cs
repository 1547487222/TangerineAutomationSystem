using System;
using System.ComponentModel;

namespace TangerineAutomationSystem.Models
{
    // 占位：最小化 Connection 定义，便于序列化与绘图逻辑使用
    public class Connection : INotifyPropertyChanged
    {
        public string Id { get; set; } = Guid.NewGuid().ToString();
        public string FromNodeId { get; set; } = string.Empty;
        public string FromPort { get; set; } = string.Empty;
        public string ToNodeId { get; set; } = string.Empty;
        public string ToPort { get; set; } = string.Empty;
        // 可扩展元数据（比如条件/权重/序号等）
        public string? MetaJson { get; set; }
        
        // For visual rendering (computed from node positions)
        private double _fromX;
        public double FromX { get => _fromX; set { _fromX = value; OnPropertyChanged(nameof(FromX)); } }
        
        private double _fromY;
        public double FromY { get => _fromY; set { _fromY = value; OnPropertyChanged(nameof(FromY)); } }
        
        private double _toX;
        public double ToX { get => _toX; set { _toX = value; OnPropertyChanged(nameof(ToX)); } }
        
        private double _toY;
        public double ToY { get => _toY; set { _toY = value; OnPropertyChanged(nameof(ToY)); } }
        
        public event PropertyChangedEventHandler? PropertyChanged;
        protected void OnPropertyChanged(string p) => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(p));
    }
}