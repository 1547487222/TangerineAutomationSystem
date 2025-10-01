using CommunityToolkit.Mvvm.ComponentModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WpfApp1.Configs
{
   public class ParamterInfo: ObservableObject
    {
        public string preSpaceTime { get; set; } = "1";
        public string preCoolDown { get; set; } = "2";
        public string preGasFlowrate { get; set; } = "0.01";
        public string preGasTemperature { get; set; } = "25";
        /// <summary>
        /// 采气间隔时间
        /// </summary>
        public string pre_gasWaitTime { get; set; } = "1";
    }
}
