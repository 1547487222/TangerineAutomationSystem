using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace QStandaedPlatform.Engine.Common.Common
{
    public class PartBackup
    {
        public Guid  PartId { get; set; }

        public string PartName { get; set; }

        public Type  PartType { get; set; }

        public string Description { get; set; }

        public object PartOption { get; set; }

        public Type PartOptionType { get; set; }
    }
}
