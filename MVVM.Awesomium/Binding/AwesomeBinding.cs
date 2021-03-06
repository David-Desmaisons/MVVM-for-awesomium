﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.ComponentModel;
using System.Threading.Tasks;
using System.Collections;
using System.Collections.Specialized;

using Awesomium.Core;

using MVVMAwesomium.Infra;
using MVVMAwesomium.AwesomiumBinding;

namespace MVVMAwesomium
{
    public class AwesomeBinding : IDisposable, IAwesomeBinding
    {
        private BidirectionalMapper _BirectionalMapper;
        private Action _CleanUp;

        private AwesomeBinding(BidirectionalMapper iConvertToJSO, Action CleanUp = null)
        {
            _BirectionalMapper = iConvertToJSO;
            _CleanUp = CleanUp;
        }

        public JSObject JSRootObject
        {
            get { return _BirectionalMapper.JSValueRoot.GetJSSessionValue(); }
        }

        public object Root
        {
            get { return _BirectionalMapper.JSValueRoot.CValue; }
        }

        public IJSCSGlue JSBrideRootObject
        {
            get { return _BirectionalMapper.JSValueRoot; }
        }

        public override string ToString()
        {
            return _BirectionalMapper.JSValueRoot.ToString();
        }

        public void Dispose()
        {
            _BirectionalMapper.Dispose();
            if (_CleanUp != null)
            {
                WebCore.QueueWork(() =>
                    {
                        _CleanUp();
                        _CleanUp = null;
                    }
                );
            }
        }

        internal static Task<IAwesomeBinding> Bind(IWebView view, object iViewModel, object additional, JavascriptBindingMode iMode,
                                                    Action First, Action CleanUp)
        {
            TaskCompletionSource<IAwesomeBinding> tcs = new TaskCompletionSource<IAwesomeBinding>();

            view.ExecuteWhenReady(() =>
                    {
                        try 
                        { 
                            if (First != null) First();
                            var mapper = new BidirectionalMapper(iViewModel, view, iMode, additional);
                            mapper.Init().ContinueWith(_ => tcs.SetResult(new AwesomeBinding(mapper, CleanUp)));
                        }
                        catch (Exception e)
                        {
                            tcs.SetException(e);
                        }
                    });

            return tcs.Task;
        }

        public static Task<IAwesomeBinding> Bind(IWebView view, object iViewModel, JavascriptBindingMode iMode)
        {
            return Bind(view, iViewModel, null, iMode, null, null);
        }

    }
}
