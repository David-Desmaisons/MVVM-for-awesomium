using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Awesomium.Core;
using MVVMAwesomium.AwesomiumBinding;

namespace MVVMAwesomium
{
    public class StringBinding : IDisposable, IAwesomeBinding
    {
        private Action _CleanUp;
        private JavascriptSessionInjector _JavascriptSessionInjector;
        private JSObject _Root;

        internal StringBinding( JSObject root, JavascriptSessionInjector iJavascriptSessionInjector, Action CleanUp)
        {
            _JavascriptSessionInjector = iJavascriptSessionInjector;
            _CleanUp = CleanUp;
            _Root = root;
        }

        public void Dispose()
        {
            WebCore.QueueWork(() =>
            {
                if (_CleanUp != null)
                {
                    _CleanUp();
                    _CleanUp = null;
                }

                if (_JavascriptSessionInjector != null)
                {
                    _JavascriptSessionInjector.Dispose();
                    _JavascriptSessionInjector = null;
                }
            }
            );
        }

        public JSObject JSRootObject
        {
            get { return _Root; }
        }

        public object Root
        {
            get { return null; }
        }

        public static Task<IAwesomeBinding> Bind(IWebView view, string iViewModel, Action First = null, Action CleanUp = null)
        {
            TaskCompletionSource<IAwesomeBinding> tcs = new TaskCompletionSource<IAwesomeBinding>();

            view.ExecuteWhenReady(() =>
            {
                if (First != null) First();
                JSObject json = view.ExecuteJavascriptWithResult("JSON");
                var root = json.Invoke("parse", new JSValue(iViewModel));

                var injector = new JavascriptSessionInjector(view, new GlobalBuilder(view, "MVVMGlue"), null);
                var mappedroot = injector.Map(root, null);
                injector.RegisterInSession(mappedroot);

                tcs.SetResult(new StringBinding(mappedroot, injector, CleanUp));
            });

            return tcs.Task;
        }
    }
}
