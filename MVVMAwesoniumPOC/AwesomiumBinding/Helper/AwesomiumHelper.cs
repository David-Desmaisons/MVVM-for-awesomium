using Awesomium.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MVVMAwesonium.AwesomiumBinding
{
    public static class AwesomiumHelper
    {
        private static Task<T> EvaluateSafeAsync<T>(this IWebView iwb, Func<T> evaluate)
        {
            TaskCompletionSource<T> tcs = new TaskCompletionSource<T>();
            WebCore.QueueWork(iwb, () => tcs.SetResult(evaluate()));
            return tcs.Task;
        }

        public static T EvaluateSafe<T>(this IWebView iwb, Func<T> evaluate)
        {
            try
            {
                return evaluate();
            }
            catch (InvalidOperationException)
            {
                return iwb.EvaluateSafeAsync(evaluate).Result;
            }
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
