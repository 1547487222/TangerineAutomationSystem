using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace QStandaedPlatform.Engine.Common.Common
{
    public class RefParameterProperty: IRefParameterProperty, IRefProperty
    {
        private readonly ILogger _logger;
        public RefParameterProperty()
        {
            _logger = LoggerProviderManager.GetLoggerFactory().CreateLogger<RefParameterProperty>();
        }
        public Guid OwnerFlowId { get; set; }

        public Guid OwnerToolId { get; set; }

        [JsonIgnore]
        public Tool? OwnerTool { get; set; }

        public string PropertyName { get; set; } = string.Empty;

        public Type? PropertyType { get; set; }

        [JsonIgnore]
        public System.Reflection.PropertyInfo? Property { get; set; }

        public Type? ModuleTableType { get; set; }

        public Guid RefParameterTableId { get; set; }

        public Guid RefParameterId { get; set; }

        [JsonIgnore]
        public IParameter?  Parameter { get; set; }

        
        public void InstallRef()
        {
            try
            {
                if (RefParameterTableId != Guid.Empty && RefParameterId != Guid.Empty)
                {
                    var table = ParameterTableManager.Table(RefParameterTableId);
                    Parameter = table.GetParameter(RefParameterId);
                    if (Parameter != null
                        && Property != null
                        && OwnerTool != null
                        && PropertyType != null)
                    {
                        Property.SetValue(OwnerTool, Parameter);
                        OwnerTool.OnRefParameterPropertyInstalled(this);
                        //记录工具的参数引用情况
                        _logger.LogInformation($"工具{OwnerTool.DisplayName}引用了参数{RefParameterId}，参数类型{PropertyType.Name}，参数名称{PropertyName}");
                    }
                }
            }
            catch (Exception e)
            {
                _logger.LogError($"工具{OwnerTool?.DisplayName}引用参数{RefParameterId}失败,{e}");
                throw;
            }

        }

        public void UnInstallRef()
        {
            if (RefParameterTableId != Guid.Empty && RefParameterId != Guid.Empty)
            {
                if (Parameter != null
                    && Property != null
                    && OwnerTool != null)
                {
                    Property.SetValue(OwnerTool, null);
                    OwnerTool.OnRefParameterPropertyUnInstalled(this);
                    RefParameterTableId = Guid.Empty;
                    RefParameterId = Guid.Empty;
                }
            }
        }

    }
}
