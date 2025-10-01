using QStandaedPlatform.Engine.Common.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace QStandaedPlatform.Engine.Laboratory
{
    public class ArmModular: ITransportController
    {
        private readonly Modular _modular;
        public ArmModular(Modular modular)
        {
            _modular = modular;
        }

        public Modular Modular => _modular;

        public long TransportId { get; set; }
    }
}
