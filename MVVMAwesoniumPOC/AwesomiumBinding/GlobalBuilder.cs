using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Awesomium.Core;
using System.Threading.Tasks;

namespace MVVMAwesonium.AwesomiumBinding
{
    public class GlobalBuilder : IJSOBuilder
    {
        private IWebView _IWebView;
        private static int _Count = 0;
        private string _NameScape;

        public GlobalBuilder(IWebView iWebView, string iNameScape)
        {
            _IWebView = iWebView;
            _NameScape = iNameScape;
        }

        //private Task<T> GetTask<T>(IWebView iwb, Func<T> evaluate)
        //{
        //    TaskCompletionSource<T> tcs = new TaskCompletionSource<T>();
        //    WebCore.QueueWork(iwb, () => tcs.SetResult(evaluate()));
        //    return tcs.Task;
        //}

        //public JSObject CreateJSOChild(JSObject father, string iPropertyName)
        //{
        //    return _IWebView.CreateGlobalJavascriptObject(string.Format("{0}.{1}", father.GlobalObjectName, iPropertyName));
        //}

        public JSObject CreateJSO()
        {
            //return GetTask(_IWebView, () => _IWebView.
            //    CreateGlobalJavascriptObject(string.Format("{0}{1}", _NameScape, _Count++))).Result;

            return _IWebView.CreateGlobalJavascriptObject(string.Format("{0}{1}", _NameScape, _Count++));
        }
    }
}
