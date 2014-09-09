using Awesomium.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace MVVMAwesonium.AwesomiumBinding
{
    internal class JavascriptSessionMapper
    {
        private IWebView _IWebView;
        private GlobalBuilder _GlobalBuilder;

        internal JavascriptSessionMapper(IWebView iWebView)
        {
            _IWebView = iWebView;
            _GlobalBuilder = new GlobalBuilder(_IWebView, "MVVMGlue");
        }

        private JSObject BindListener(JSObject iListener, IMapperListener iMapperListener)
        {
            if (iMapperListener == null)
            {
                if (OnJavascriptObjecChanges!=null)
                    throw new NotSupportedException("Should provide a mapper for two way binding");
                return iListener;
            }

            iListener.Bind("RegisterFirst", false, (o, e) => iMapperListener.RegisterFirst((JSObject)e.Arguments[0]));
            iListener.Bind("RegisterMapping", false, (o, e) => 
                   iMapperListener.RegisterMapping((JSObject)e.Arguments[0],(string)e.Arguments[1],(JSObject)e.Arguments[2]));

             iListener.Bind("RegisterCollectionMapping", false, (o, e) => 
                {
                    string pn = (string)e.Arguments[2];
                    int index = -1;
                    if (int.TryParse(pn,out index))
                        iMapperListener.RegisterCollectionMapping((JSObject)e.Arguments[0], (string)e.Arguments[1], index, (JSObject)e.Arguments[3]);
                });

             if (OnJavascriptObjecChanges != null)
                 iListener.Bind("TrackChanges", false, (o, e) => OnJavascriptObjecChanges((JSObject)e.Arguments[0], (string)e.Arguments[1], e.Arguments[2]));

             return iListener;
        }

        public Action<JSObject, string, JSValue> OnJavascriptObjecChanges { get; set; }

        public IJSListener MappToJavaScriptSession(JavascriptObjectMapper iLocaljsobject)
        {
            var listener = BindListener(_GlobalBuilder.CreateJSO(), iLocaljsobject);
            JSObject Ko = _IWebView.ExecuteJavascriptWithResult("ko");
            var Globalres = Ko.Invoke("MapToObservable", iLocaljsobject.Root.JSValue, listener); 
            Ko.Invoke("applyBindings", Globalres);
            return new JSListener(listener);
        }
    }
}
