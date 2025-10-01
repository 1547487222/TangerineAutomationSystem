using Microsoft.EntityFrameworkCore.Query.Internal;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Demo
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading;
    using System.Threading.Tasks;

    public class Workflow
    {
        // 工具依赖图，存储工具ID及其依赖的工具ID列表
        private readonly Dictionary<long, List<long>> _toolGraph = [];
        // 取消令牌源，用于控制工作流取消
        private readonly CancellationTokenSource _cancellationTokenSource = new();
        // 工具字典，存储工具ID到工具对象的映射
        private readonly Dictionary<long, Tool> _tools = [];
        // 属性字典，存储属性ID到属性对象的映射
        private readonly Dictionary<long, Property> _properties = [];

        public Workflow()
        {
        }

        // 动态添加工具到工作流
        public void AddTool(Tool tool)
        {
            lock (_tools)
            {
                // 添加工具到工具字典
                _tools.Add(tool.ToolId, tool);

                // 将工具添加到依赖图
                if (!_toolGraph.ContainsKey(tool.ToolId))
                {
                    _toolGraph[tool.ToolId] = [];
                }

                // 注册所有输入和输出属性
                foreach (var prop in tool.InputProperties.Concat(tool.OutputProperties))
                {
                    _properties[prop.PropertyId] = prop;
                }

                // 根据NextProperties构建工具之间的依赖关系
                foreach (var outputProp in tool.OutputProperties)
                {
                    foreach (var nextPropId in outputProp.NextProperties)
                    {
                        var dependentTool = _tools.Values.FirstOrDefault(t =>
                            t.InputProperties.Any(p => p.PropertyId == nextPropId));
                        if (dependentTool != null && dependentTool.ToolId != tool.ToolId)
                        {
                            _toolGraph[tool.ToolId].Add(dependentTool.ToolId);
                        }
                    }
                }
            }
        }

        // 异步运行工作流
        public async Task RunAsync()
        {
            // 跟踪已执行的工具（每轮清空，允许重复执行以支持无限循环）
            var executedTools = new HashSet<long>();

            while (!_cancellationTokenSource.IsCancellationRequested)
            {
                var executableTools = GetExecutableTools(executedTools);

                // 如果没有可执行工具，选择任意未执行的工具以处理环形依赖
                if (executableTools.Count == 0)
                {
                    lock (_tools)
                    {
                        executableTools = _toolGraph.Keys
                            .Where(id => !executedTools.Contains(id))
                            .Take(1)
                            .ToList();
                    }
                }

                // 如果没有可执行工具，重置已执行工具集合并继续
                if (executableTools.Count == 0)
                {
                    lock (_tools)
                    {
                        executedTools.Clear();
                        executableTools = _toolGraph.Keys.Take(1).ToList();
                    }
                }

                if (executableTools.Count == 0)
                {
                    await Task.Delay(100, _cancellationTokenSource.Token); // 避免CPU过载
                    continue;
                }

                var tasks = new List<Task>();

                foreach (var toolId in executableTools)
                {
                    var tool = _tools[toolId];


                    if (tool is EndTool)
                    {
                        _cancellationTokenSource.Cancel();
                        break;
                    }

                    tasks.Add(Task.Run(async () =>
                    {
                        try
                        {
                            // 从黑板中传输输入属性值
                            foreach (var inputProp in tool.InputProperties)
                            {
                                var sourceProps = _properties.Values
                                    .Where(p => p.NextProperties.Contains(inputProp.PropertyId))
                                    .ToList();

                                if (sourceProps.Count != 0)
                                {
                                    // 获取最新的源属性值
                                    var sourceProp = sourceProps.First();
                                    var value = tool.Blackboard.Get(sourceProp.Name);
                                    tool.Blackboard.Set(inputProp.Name, value);
                                }
                            }

                            await tool.Execute();

                            foreach (var outputProp in tool.OutputProperties)
                            {
                                var value = tool.Blackboard.Get(outputProp.Name);
                                foreach (var nextPropId in outputProp.NextProperties)
                                {
                                    if (_properties.TryGetValue(nextPropId, out var nextProp))
                                    {
                                        var targetTool = _tools.Values.FirstOrDefault(t =>
                                            t.InputProperties.Any(p => p.PropertyId == nextPropId));
                                        if (targetTool != null)
                                        {
                                            targetTool.Blackboard.Set(nextProp.Name, value);
                                        }
                                    }
                                }
                            }
                        }
                        catch (Exception ex)
                        {
                            Console.WriteLine($"执行工具 {tool.ToolId} 时出错: {ex.Message}");
                            _cancellationTokenSource.Cancel();
                        }
                    }, _cancellationTokenSource.Token));
                }

                await Task.WhenAll(tasks);
                foreach (var toolId in executableTools)
                {
                    executedTools.Add(toolId);
                }
            }
            if (_cancellationTokenSource.IsCancellationRequested)
            {
                throw new OperationCanceledException();
            }

            return;
        }

        private List<long> GetExecutableTools(HashSet<long> executedTools)
        {
            lock (_tools)
            {
                return _toolGraph.Keys
                    .Where(toolId => !executedTools.Contains(toolId) &&
                        _toolGraph[toolId].All(dep => executedTools.Contains(dep)))
                    .ToList();
            }
        }

        public void Cancel()
        {
            _cancellationTokenSource.Cancel();
        }
    }
    public class EndTool : Tool
    {
        public EndTool(long toolId)
        {
            ToolId = toolId;
        }

        public override Task Execute()
        {
            return Task.CompletedTask;
        }
    }

    public abstract class Tool
    {
        public Tool()
        {
            Blackboard = new Blackboard();
        }
        public Blackboard Blackboard { get; set; }
        public long ToolId { get; set; }
        public List<Property> InputProperties { get; set; } = [];
        public List<Property> OutputProperties { get; set; } = [];

        public abstract Task Execute();
    }

    public class Blackboard
    {
        private readonly Dictionary<string, object> _data = [];

        public void Set(string key, object value)
        {
            _data[key] = value;
        }
        public object? Get(string key)
        {
            return _data.TryGetValue(key, out var value) ? value : null;
        }
    }

    public class Property
    {
        public string Name { get; set; } = string.Empty;
        public long PropertyId { get; set; }
        public PropertyType Type { get; set; }
        public List<long> NextProperties { get; set; } = [];
    }

    public enum PropertyType
    {
        String,
        Int,
        Bool,
        Float,
        Double,
        DateTime,
        Object
    }
}
