using System;
using System.Collections.Generic;
using System.Dynamic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WpfApp1
{
   public class DynamicParamter:DynamicObject
    {
        private readonly Dictionary<string, object> _dict=new Dictionary<string, object>();

        public override bool TryGetMember(GetMemberBinder binder, out object? result)
        {
            return _dict.TryGetValue(binder.Name, out result);
        }

        public override bool TrySetMember(SetMemberBinder binder, object? value)
        {
           return _dict.TryAdd(binder.Name, value);
        }
    }
}
