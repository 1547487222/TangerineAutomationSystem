using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace QStandaedPlatform.Engine.Common.Common
{
    public interface ITriggerPointCommand : IPartObject, IMarker<int>
    {
        Guid OwnerToolId { get; set; }
        object? TriggerValue { get; set; }
    }

    public class TriggerPointCommand : ITriggerPointCommand
    {
        public TriggerPointCommand(int id, string name, object? value = null, string desc = "")
        {
            Id = id;
            Name = name;
            TriggerValue = value;
            Description = desc;
        }
        public object? TriggerValue { get; set; }

        public string Name { get; set; } = string.Empty;

        public string Description { get; set; } = string.Empty;

        public int Id { get; set; }
        public Guid OwnerToolId { get; set; }
    }
}
