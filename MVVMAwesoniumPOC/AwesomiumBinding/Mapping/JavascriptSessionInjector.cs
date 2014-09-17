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
        private JSValue _Globalres;
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

            mapper.Bind("Register", false, (o, e) =>
            {
                JSObject registered = (JSObject)e.Arguments[0];
                JSObject Context = (JSObject)e.Arguments[1];

                if (Context == null)
                {
                    iMapperListener.RegisterFirst(registered);
                    return;
                }

                if (Context.HasProperty("index"))
                    iMapperListener.RegisterCollectionMapping((JSObject)Context["object"],
                        (string)Context["attribute"], (int)Context["index"], registered);
                else
                    iMapperListener.RegisterMapping((JSObject)Context["object"], (string)Context["attribute"], registered);
            });

            mapper.Bind("End", false, (o, e) => iMapperListener.End((JSObject)e.Arguments[0]));

            return mapper;
        }

        private JSObject GetKo()
        {
            return _IWebView.ExecuteJavascriptWithResult("ko");
        }

        private bool _IsFirst=true;
        public void Map(IJSCBridge ihybridobject, IJavascriptMapper ijvm)
        {
            using (var mapp = GetMapper(ijvm))
            {
                JSValue Globalres = GetKo().Invoke("MapToObservable", ihybridobject.JSValue, mapp, _Listener);
                if (_IsFirst)
                    _Globalres = Globalres;
            }
        }

        public void RegisterInSession()
        {
            GetKo().Invoke("applyBindings", _Globalres);
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
