using QStandaedPlatform.Engine.Common.Common;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Equipment.Bqjx.StandardPlatformSystem.WorkFlows
{
    public class FlowConfigs
    {
        public List<FlowOptions> Flows { get; set; }
    }
    public class FlowOptions
    {
        public string FlowId { get; set; }
        public string FlowName { get; set; }
        public List<NodeOptions> Nodes { get; set; }
    }
    public class NodeOptions
    {
        public string ToolName { get; set; }

        public string DisplayName { get; set; }

        public string UniqueId { get; set; }

        public Point Location { get; set; }

        public InputPortOptions  InputPortOptions { get; set; }

        public OutputPortOptions OutputPortOptions { get; set; }

        public List<TriggerCommandOptions> TriggerCommands { get; set; }

        public bool Enable { get; set; }

        public bool IsDebug { get; set; }

        public virtual string? Version { get; set; }

        public string StepTypeName { get; set; }

        public object DataContext { get; set; }
    }

    public class TriggerCommandOptions
    {
        public string OwnerToolName { get; set; }

        public object? TriggerValue { get; set; }

        public string Name { get; set; }

        public string Description { get; set; }

        public int Id { get; set; }
    }
    public class PinOptions
    {
        public string OwnerToolName { get; set; }

        public Type QDataType { get;  set; }

        public PinType PinType { get;  set; }

        public PinOptions Parent { get; set; }

        public List<PinOptions> Children { get; set; }

        public  string PinName { get; set; }

        public  string Description { get; set; }

        public  int Id { get; set; }

        public int ParentId { get; set; }

        public bool WatchChildState { get; set; }
    }


    public class InputPortOptions
    {
        public string OwnerToolId { get; set; }

        public Point Anchor { get; set; }
        public Size Size { get; set; }
        public List<PinOptions>  InputPinOptions { get; set; }

        public List<OutputPortOptions> ConnectedOutputPorts { get; set; }
    }

    public class OutputPortOptions
    {
        public string OwnerToolId { get; set; }

        public Point Anchor { get; set; }
        public Size Size { get; set; }

        public List<PinOptions> OutputPinOptions { get; set; }

        public List<InputPortOptions> ConnectedInputPorts { get; set; }
    }

}
