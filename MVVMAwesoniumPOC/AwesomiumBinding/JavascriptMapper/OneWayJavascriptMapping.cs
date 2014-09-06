
using Awesomium.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace MVVMAwesonium.AwesomiumBinding.JavascriptMapper
{
    internal class OneWayJavascriptMapping : IJavaScriptMapper
    {
        private static OneWayJavascriptMapping _Mapper = new OneWayJavascriptMapping();

        internal static IJavaScriptMapper Binder { get { return _Mapper; } }

        public JSObject MappToJavaScriptSession(JSObject jsobject, IWebView iWebView)
        {
            JSObject vm = iWebView.ExecuteJavascriptWithResult("ko");
            //JSObject mapping = view.ExecuteJavascriptWithResult("ko.mapping");
            //JSObject res = mapping.Invoke("fromJS", js);
            JSObject res = vm.Invoke("MapToObservable", jsobject);
            JSObject res2 = vm.Invoke("applyBindings", res);

            return res;
        }
    }
}
