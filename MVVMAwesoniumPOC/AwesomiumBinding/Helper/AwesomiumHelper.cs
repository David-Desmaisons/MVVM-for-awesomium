using Awesomium.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace MVVMAwesonium.AwesomiumBinding
{
    public static class AwesomiumHelper
    {
        public static object GetSimpleValue(this JSValue @this)
        {
            if (@this.IsString)
                return (string)@this;

            if (@this.IsInteger)
                return (int)@this;

            if (@this.IsBoolean)
                return (bool)@this;

            if (@this.IsDouble)
                return (double)@this;

            return null;
        }

        //private class Comparer : IEqualityComparer<JSObject>
        //{
        //    public bool Equals(JSObject x, JSObject y)
        //    {
        //        if ((x.RemoteId == 0) || (y.RemoteId == 0))
        //            throw new NotSupportedException("Only remote javascript obejcts suported");

        //        return x.RemoteId == y.RemoteId;
        //    }

        //    public int GetHashCode(JSObject obj)
        //    {
        //        if (obj.RemoteId == 0)
        //            throw new NotSupportedException("Only remote javascript obejcts suported");

        //        return obj.RemoteId.GetHashCode();
        //    }
        //}

        //private static Comparer _Comparer = new Comparer();

        //public static IEqualityComparer<JSObject> RemoteObjectComparer { get { return _Comparer; } }

    }
}
