using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace QStandaedPlatform.Engine.Mqtt
{
    public interface IBqjxMqttCommand
    {
        Task Execute(string topic, string message);
    }
}
