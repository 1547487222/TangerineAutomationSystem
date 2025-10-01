using QStandaedPlatform.Engine.Common.Common.Metadatas;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace QStandaedPlatform.Engine.Common.Common
{

    public class ToolTriggerCommandOptions
    {
        public Guid OwnerToolId { get; set; }

        public object? TriggerValue { get; set; }

        public string Name { get; set; }

        public string Description { get; set; }

        public int Id { get; set; }
    }

    public class PinInfoOptions
    {
        public PinInfoOptions()
        {
            ConnectedPins = [];
        }
        public Guid OwnerToolId { get; set; }

        public Type PinDataType { get; set; }

        public PinType PinType { get; set; }

        public List<Guid> ConnectedPins { get; set; }

        public string PinName { get; set; }

        public string Description { get; set; }

        public Guid Id { get; set; }

        public Guid ParentId { get; set; }

        public bool MonitorChildPinStates { get; set; }
    }

    public class ToolInfoOptions
    {
        public ToolInfoOptions()
        {
            ToolTriggerCommandOptions = [];
            InputToolPinInfos = [];
            OutputToolPinInfos = [];
        }
        public string DefineName { get; set; }

        public string DataContext { get; set; }

        public bool Enable { get; set; }

        public bool IsDebug { get; set; }

        public Guid UniqueId { get; set; }

        public QPoint ToolPosition { get; set; }
        public string DisplayName { get; set; }
        public List<ToolTriggerCommandOptions> ToolTriggerCommandOptions { get; set; }

        public List<PinInfoOptions> InputToolPinInfos { get; set; }
        public List<PinInfoOptions> OutputToolPinInfos { get; set; }
        /// <summary>
        /// 工具创建时间
        /// </summary>
        public DateTime CreationTime { get; set; }
    }

    public class FlowInfoOptions
    {
        public FlowInfoOptions()
        {
            Tools = new List<ToolInfoOptions>();
            ToolConnecters = new List<ToolConnecter>();
            PartProperties = new List<RefPartProperty>();
            ParameterProperties= new List<RefParameterProperty>();
        }
        /// <summary>
        /// 流程名称
        /// </summary>
        public string FlowName { get; set; }
        /// <summary>
        /// 流程唯一标识
        /// </summary>
        public Guid FlowId { get; set; }

        public string Description { get; set; }
        /// <summary>
        /// 流程工具信息
        /// </summary>
        public List<ToolInfoOptions> Tools { get; set; }
        public List<ToolConnecter> ToolConnecters { get; set; }

        public List<RefPartProperty>  PartProperties { get; set; }

        public List<RefParameterProperty> ParameterProperties { get; set; }
    }
}
