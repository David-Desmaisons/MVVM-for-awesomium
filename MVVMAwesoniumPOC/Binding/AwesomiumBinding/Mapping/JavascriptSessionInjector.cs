using Awesomium.Core;
using System;
using System.Collections.Generic;
using System.Diagnostics;
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

        private Queue<IJavascriptMapper> _IJavascriptMapper = new Queue<IJavascriptMapper>();
        private IJavascriptMapper _Current;
        private bool _PullNextMapper = true;
        private JSObject _Mapper;

        private JSObject GetMapper(IJavascriptMapper iMapperListener)
        {
            _IJavascriptMapper.Enqueue(iMapperListener);
    
            if (_Mapper != null)
                return _Mapper;

            _Mapper = _GlobalBuilder.CreateJSO();

             _Mapper.Bind("Register", false, (o, e) =>
            {
                if (_PullNextMapper)
                { 
                    _Current = _IJavascriptMapper.Dequeue();
                    _PullNextMapper = false;
                }

                if (_Current == null)
                    return;

                JSObject registered = (JSObject)e.Arguments[0];
                JSObject Context = (JSObject)e.Arguments[1];

                if (Context == null)
                {
                    _Current.RegisterFirst(registered);
                    return;
                }

                if (Context.HasProperty("index"))
                    _Current.RegisterCollectionMapping((JSObject)Context["object"],
                        (string)Context["attribute"], (int)Context["index"], registered);
                else
                    _Current.RegisterMapping((JSObject)Context["object"], (string)Context["attribute"], registered);
            });

            _Mapper.Bind("End", false, (o, e) =>
                {
                    if (_Current!=null)
                        _Current.End((JSObject)e.Arguments[0]);
                    _Current = null;
                    _PullNextMapper = true;
                });

            return _Mapper;
        }

        private JSObject GetKo()
        {
            return _IWebView.ExecuteJavascriptWithResult("ko");
        }


        public JSObject Map(JSValue ihybridobject, IJavascriptMapper ijvm)
        {
            return GetKo().Invoke("MapToObservable", ihybridobject, GetMapper(ijvm), _Listener);
        }

        public void RegisterInSession(JSObject iJSObject)
        {
            var ko = GetKo();
            ko.Bind("log", false, (o, e) => Trace.WriteLine(string.Join(" - ", e.Arguments.Select(s=>((string)s).Replace("\n"," ")))));
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
