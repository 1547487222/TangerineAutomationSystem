using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace QStandaedPlatform.Engine.Common.Common
{
    public class RefPropertyService
    {
        private static readonly Lazy<RefPropertyService> _service = new(() => new RefPropertyService(), true);

        private readonly List<RefPartProperty> _refpartProps = [];

        public static RefPropertyService Instance
        {
            get
            {
                return _service.Value;
            }
        }

        public void AddRefProperty(RefPartProperty refPartProperty)
        {
            _refpartProps.Add(refPartProperty);
        }

    }
}
