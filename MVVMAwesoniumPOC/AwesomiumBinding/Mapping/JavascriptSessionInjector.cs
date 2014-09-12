using Awesomium.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace MVVMAwesonium.AwesomiumBinding
{
    internal class JavascriptSessionInjector : IDisposable
    {
        private IWebView _IWebView;    
        private GlobalBuilder _GlobalBuilder;  
        private JSObject _Listener;
        private Action<JSObject, string, JSValue> _OnJavascriptObjecChanges;

        internal JavascriptSessionInjector(IWebView iWebView, Action<JSObject, string, JSValue> OnJavascriptObjecChanges)
        {
            _IWebView = iWebView;
            _GlobalBuilder = new GlobalBuilder(_IWebView, "MVVMGlue");
            _OnJavascriptObjecChanges = OnJavascriptObjecChanges;

            _Listener = _GlobalBuilder.CreateJSO();
            if (_OnJavascriptObjecChanges != null)
            {
                _Listener.Bind("TrackChanges", false, (o, e) => _OnJavascriptObjecChanges((JSObject)e.Arguments[0], (string)e.Arguments[1], e.Arguments[2]));
            }
        }

        private JSObject GetMapper(IJavascriptMapper iMapperListener)
        {
            var mapper = _GlobalBuilder.CreateJSO();

            mapper.Bind("RegisterFirst", false, (o, e) => 
                iMapperListener.RegisterFirst((JSObject)e.Arguments[0]));

            mapper.Bind("RegisterMapping", false, (o, e) =>
            {
                string pn = (string)e.Arguments[2];
                int index = -1;
                if ((int.TryParse(pn, out index)) && (index != -1))
                    iMapperListener.RegisterCollectionMapping((JSObject)e.Arguments[0], (string)e.Arguments[1], index, (JSObject)e.Arguments[3]);
                else
                    iMapperListener.RegisterMapping((JSObject)e.Arguments[0], (string)e.Arguments[1], (JSObject)e.Arguments[3]);
            });

            mapper.Bind("End", false, (o, e) => iMapperListener.End((JSObject)e.Arguments[0]));

            return mapper;
        }

        public void Map(IJSCBridge ihybridobject, IJavascriptMapper ijvm, bool IsRoot = true)
        {
            using (var mapp = GetMapper(ijvm))
            {
                JSObject Ko = _IWebView.ExecuteJavascriptWithResult("ko");

                JSValue Globalres = Ko.Invoke("MapToObservable", ihybridobject.JSValue, mapp, _Listener);
                if (IsRoot)
                    Ko.Invoke("applyBindings", Globalres);
            }
        }

        public void Dispose()
        {
            WebCore.QueueWork(()
                =>
                {
                    if (_Listener != null)
                    {
                        _Listener.Dispose();
                        _Listener = null;
                    }
                });
        }
    }
}
