using Awesomium.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace MVVMAwesonium.AwesomiumBinding
{
    internal class JavaScriptMapper
    {
        private IWebView _IWebView;
        private JSObject _Listener;
        private GlobalBuilder _GlobalBuilder;

        internal JavaScriptMapper(IWebView iWebView)
        {
            _IWebView = iWebView;
            _GlobalBuilder = new GlobalBuilder(_IWebView, "MVVMGlue");
        }

        private void BindListener(JSObject iListener, IMapperListener iMapperListener)
        {
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

            
        }

        internal IDisposable Subscribe(Action<JSObject,string,JSValue> OnChanges)
        {
            _Listener = _GlobalBuilder.CreateJSO();
            _Listener.Bind("TrackChanges",false,(o,e) => OnChanges((JSObject)e.Arguments[0],(string)e.Arguments[1],e.Arguments[2]));
            return _Listener;
        }

        public JSObject MappToJavaScriptSession(JSObject jsobject, IMapperListener iMapperListener)
        {
            JSObject Ko = _IWebView.ExecuteJavascriptWithResult("ko");
            JSObject res = null;
            
            if (_Listener != null)
            {
                BindListener(_Listener, iMapperListener);
                res = Ko.Invoke("MapToObservable", jsobject, _Listener);
            }
            else
            {
                using(var temp = _GlobalBuilder.CreateJSO())
                {
                    BindListener(temp, iMapperListener);
                    res = Ko.Invoke("MapToObservable", jsobject, temp);
                }
            }
            
            JSObject res2 = Ko.Invoke("applyBindings", res);
            return res;
        }
    }
}
