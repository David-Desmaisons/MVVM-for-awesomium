using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace MVVMAwesomium.Infra
{
    public static class TypeExtender
    {
        static public IEnumerable<Type> GetBaseTypes(this Type itype)
        {
            if (itype == null) throw new ArgumentNullException();
            yield return itype;

            while ((itype = itype.BaseType) != null)
            {
                yield return itype;
            }
        }
    }

}
