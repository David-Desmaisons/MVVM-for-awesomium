using Awesomium.Core;
using MVVMAwesomium.Exceptions;
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
                _Listener.Bind("TrackCollectionChanges", false, (o, e) => _IJavascriptListener.OnJavaScriptCollectionChanges((JSObject)e.Arguments[0], (JSValue[])e.Arguments[1], (JSValue[])e.Arguments[2], (JSValue[])e.Arguments[3]));
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

                int count = e.Arguments.Length;
                JSObject registered = (JSObject)e.Arguments[0];

                switch (count)
                {
                    case 1:
                        _Current.RegisterFirst(registered);
                        break;

                    case 3:
                        _Current.RegisterMapping((JSObject)e.Arguments[1], (string)e.Arguments[2], registered);
                        break;

                    case 4:
                        _Current.RegisterCollectionMapping((JSObject)e.Arguments[1],
                            (string)e.Arguments[2], (int)e.Arguments[3], registered);
                        break;
                }
             });

            _Mapper.Bind("End", false, (o, e) =>
                {
                    if (_PullNextMapper)
                        _Current = _IJavascriptMapper.Dequeue();

                    if (_Current!=null)
                        _Current.End((JSObject)e.Arguments[0]);
                    _Current = null;
                    _PullNextMapper = true;
                });

            return _Mapper;
        }

        private JSObject _Ko;
        private JSObject GetKo()
        {
            if (_Ko == null)
            {
                _Ko = _IWebView.ExecuteJavascriptWithResult("ko");
                if (_Ko == null)
                    throw ExceptionHelper.NoKo();
            }

            return _Ko;
        }


        public JSObject Map(JSValue ihybridobject, IJavascriptMapper ijvm,bool checknullvalue=true)
        {
            JSObject res = GetKo().Invoke("MapToObservable", ihybridobject, GetMapper(ijvm), _Listener);
            if ((res == null) && checknullvalue)
            { 
                if (_IWebView.GetLastError()==Error.TimedOut)
                    throw ExceptionHelper.TimeOut();
                
                throw ExceptionHelper.NoKoExtension();
            }
            return res;
        }

        public void RegisterInSession(JSObject iJSObject)
        {
            var ko = GetKo();
            ko.Bind("log", false, (o, e) => ExceptionHelper.Log(string.Join(" - ", e.Arguments.Select(s => ((string)s).Replace("\n", " ")))));
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
