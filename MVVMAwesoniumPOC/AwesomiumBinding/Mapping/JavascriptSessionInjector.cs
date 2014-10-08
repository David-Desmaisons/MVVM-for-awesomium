using Awesomium.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace MVVMAwesomium.AwesomiumBinding
{
    internal class JavascriptSessionInjector : IDisposable
    {
        private IWebView _IWebView;
        private IJSOBuilder _GlobalBuilder;  
        private JSObject _Listener;
        private IJavascriptListener _IJavascriptListener;


        internal JavascriptSessionInjector(IWebView iWebView, IJSOBuilder iGlobalBuilder, IJavascriptListener iJavascriptListener)
        {
            _IWebView = iWebView;
            _GlobalBuilder = iGlobalBuilder;
            _IJavascriptListener = iJavascriptListener;


            if (_IJavascriptListener != null)
            {
                _Listener = _GlobalBuilder.CreateJSO();
                _Listener.Bind("TrackChanges", false, (o, e) => _IJavascriptListener.OnJavaScriptObjectChanges((JSObject)e.Arguments[0], (string)e.Arguments[1], e.Arguments[2]));
                _Listener.Bind("TrackCollectionChanges", false, (o, e) => _IJavascriptListener.OnJavaScriptCollectionChanges((JSObject)e.Arguments[0], (JSValue[])e.Arguments[1], (JSValue[])e.Arguments[2]));
            }
            else
                _Listener = new JSObject();
        }

        private JSObject GetMapper(IJavascriptMapper iMapperListener)
        {
            if (iMapperListener == null)
                return new JSObject();

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


        public JSObject Map(JSValue ihybridobject, IJavascriptMapper ijvm)
        {
            using (var mapp = GetMapper(ijvm))
            {
                return GetKo().Invoke("MapToObservable", ihybridobject, mapp, _Listener);
            }
        }

        public void RegisterInSession(JSObject iJSObject)
        {
            var ko = GetKo();
            if (ko.HasMethod("register"))
                ko.Invoke("register", iJSObject);
            ko.Invoke("applyBindings", iJSObject);
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
