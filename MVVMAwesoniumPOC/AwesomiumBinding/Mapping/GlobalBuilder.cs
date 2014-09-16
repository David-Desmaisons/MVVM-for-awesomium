using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Awesomium.Core;
using System.Threading.Tasks;

namespace MVVMAwesonium.AwesomiumBinding
{
    public class GlobalBuilder : JSOBuilderBase, IJSOBuilder
    {
        private static int _Count = 0;
        private string _NameScape;

        public GlobalBuilder(IWebView iWebView, string iNameScape)
            : base(iWebView)
        {
            _NameScape = iNameScape;
        }

        //private Task<T> GetTask<T>(IWebView iwb, Func<T> evaluate)
        //{
        //    TaskCompletionSource<T> tcs = new TaskCompletionSource<T>();
        //    WebCore.QueueWork(iwb, () => tcs.SetResult(evaluate()));
        //    return tcs.Task;
        //}

        public JSObject CreateJSO()
        {
            string Name = string.Format("{0}{1}", _NameScape, _Count++);
            return _IWebView.EvaluateSafe(() => _IWebView.CreateGlobalJavascriptObject(Name));
            //try
            //{
            //    return _IWebView.CreateGlobalJavascriptObject(Name);
            //}
            //catch (InvalidOperationException)
            //{
            //    return _IWebView.EvaluateSafe(() => _IWebView.CreateGlobalJavascriptObject(Name)).Result;
            //    //return GetTask(_IWebView, ).Result;
            //}
        }


       
    }
}
