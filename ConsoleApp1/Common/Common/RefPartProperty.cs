
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace QStandaedPlatform.Engine.Common.Common
{
    public class RefPartProperty: IRefPartProperty, IRefProperty
    {
        public Guid OwnerFlowId  { get; set; }

        public Guid OwnerToolId { get; set; }
        [JsonIgnore]
        public Tool? OwnerTool { get; set; }

        public string PropertyName { get; set; } = string.Empty;

        public Type PropertyType { get; set; }

        public Guid PartId { get; set; }

        [JsonIgnore]
        public System.Reflection.PropertyInfo? Property { get; set; }

        [JsonIgnore]
       private readonly ILogger? Logger = LoggerProviderManager.GetLoggerFactory().CreateLogger<RefPartProperty>();
        public bool CanRef(PartMapper partMapper)
        {
            if (Property == null)
                return false;
            if (partMapper.Part == null)
                return false;
            return Property.PropertyType == partMapper.PartType || Property.PropertyType.IsAssignableFrom(partMapper.PartType);
        }
        [JsonIgnore]
        public IPart?  RefPart { get; set; }

        public void InstallRef()
        {
            if (this.PartId != Guid.Empty)
            {
                Logger?.LogInformation($"InstallRef {this.PartId}");
                var partMapper = WorkFlowEngine.Instance.GetPartMappers().FirstOrDefault(p => p.PartId == this.PartId);
                if (partMapper != null && Property != null)
                {
                    Logger?.LogInformation($"InstallRef {partMapper.PartId} {partMapper.PartType?.Name}");
                    if (CanRef(partMapper))
                    {
                        Property.SetValue(this.OwnerTool, partMapper.Part);
                        RefPart = partMapper.Part;
                        if (RefPart != null&& OwnerTool!=null)
                        {
                            OwnerTool.OnRefPartPropertyInstalled(this);
                        }
                        Logger?.LogInformation($"InstallRef {partMapper.PartId} {partMapper.PartType?.Name} success");
                    }
                }
            }
            else
            {
                Logger?.LogInformation($"InstallRef {this.PartId} is empty");
            }
        }
        public void UnInstallRef()
        {
            if (Property?.GetValue(OwnerTool) != null)
            {
                Property.SetValue (this.OwnerTool, null);
                RefPart = null;
            }
        }
    }
}
