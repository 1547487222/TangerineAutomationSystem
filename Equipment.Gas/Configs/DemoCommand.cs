using QStandaedPlatform.Engine.Mqtt;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Equipment.Gas.Configs
{
    internal class DemoCommand : IBqjxMqttCommand
    {
        public async Task Execute(string topic, string message)
        {
           await Task.CompletedTask;
        }
    }
}
