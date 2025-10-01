using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace QStandaedPlatform.Engine.Common.Common
{
    public class PartManager:Singleton<PartManager>
    {
        private readonly Dictionary<Guid, IPart> _parts = [];

        public void AddPart(Guid partid, IPart part)
        {
            lock (_parts)
            {
                _parts[partid] = part;
            }
        }
        public IPart GetPart(Guid partid)
        {
            lock (_parts)
            {
                return _parts[partid];
            }
        }

        public Guid GetPartId(IPart part)
        {
            lock (_parts)
            {
                return _parts.FirstOrDefault(x => x.Value == part).Key;
            }
        }

        public void RemovePart(Guid partid)
        {
            lock (_parts)
            {
                _parts.Remove(partid);
            }
        }

        public void RemovePart(IPart part)
        {
            lock (_parts)
            {
                _parts.Remove(GetPartId(part));
            }
        }
    }
}
