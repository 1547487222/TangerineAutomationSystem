
using Castle.DynamicProxy;
using QStandaedPlatform.Engine.Components;
using QStandaedPlatform.Engine.Components.Modbus;
using QStandaedPlatform.Engine.Laboratory;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace QStandaedPlatform.Engine.Common.Common
{
    public class PartOptionImporter
    {
        public  object? Import(Type part)
        {
            if (part == typeof(H5uTcp))
            {
                return new H5uTcpOption();
            }
            else if (part.IsAssignableTo(typeof(H5uModbusTcp)))
            {
                return new H5uTcpOption();
            }
            else if (part == typeof(QMqttClient))
            {
                return new QMqttClientOptions();
            }
            return default;
        }
    }

    public class PartStructurer
    {
        private static readonly ProxyGenerator _proxyGenerator = new();
        private static readonly AsyncLockInterceptor _asyncLockInterceptor = new();

        public static IPart? Structurer(Type part, object option)
        {
            if (part == typeof(H5uTcp) && option is H5uTcpOption h5UTcpOption)
            {
              return  _proxyGenerator.CreateClassProxy<H5uModbusTcp>(constructorArguments: [h5UTcpOption.Ip, h5UTcpOption.Port,(byte)1,500], ProxyGeneratorExtensions.ToInterceptor(_asyncLockInterceptor));
            }
            else if (part == typeof(H5uModbusTcp) && option is H5uTcpOption h5uTcpOption)
            {
                return _proxyGenerator.CreateClassProxy<H5uModbusTcp>(constructorArguments: [h5uTcpOption.Ip, h5uTcpOption.Port,(byte)1, 500], ProxyGeneratorExtensions.ToInterceptor(_asyncLockInterceptor));
            }
            else if (part == typeof(QMqttClient) && option is QMqttClientOptions qMqttClientOptions)
            {
                return new QMqttClient(qMqttClientOptions);
            }
            return default;
        }
    }
}
