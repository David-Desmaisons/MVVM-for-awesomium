using Awesomium.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

using MVVMAwesonium.Infra;

namespace MVVMAwesonium.Test
{
    public class Awesomium_Test_Base
    {
        protected IWebView _WebView = null;
        protected SynchronizationContext _SynchronizationContext;

        public Awesomium_Test_Base()
        {
            //_SynchronizationContext = Init().Result;
           //DoSafe(()=> Console.WriteLine("Init Awesonium Thread"));
        }

        private class TestContext : IDisposable
        {
            private Task _EndTask;
            private Awesomium_Test_Base _Father;
            public TestContext(Awesomium_Test_Base Father)
            {
                _Father = Father;
                _Father._SynchronizationContext = InitTask().Result;
            }

            private Task<SynchronizationContext> InitTask()
            {
                TaskCompletionSource<SynchronizationContext> tcs = new TaskCompletionSource<SynchronizationContext>();
                TaskCompletionSource<object> complete = new TaskCompletionSource<object>();
                Task.Factory.StartNew(() =>
                {
                    WebCore.Initialize(new WebConfig());
                    WebSession session = WebCore.CreateWebSession(WebPreferences.Default);

                    _EndTask = complete.Task;

                    _Father._WebView = WebCore.CreateWebView(500, 500);
                    _Father._WebView.Source = new Uri(string.Format("{0}\\javascript\\index.html", Assembly.GetExecutingAssembly().GetPath()));

                    WebCore.Started += (o, e) => { tcs.SetResult(SynchronizationContext.Current); };

                    while (_Father._WebView.IsLoading)
                    {
                        WebCore.Run();
                    }
                    complete.SetResult(null);
                }
                );

                return tcs.Task;
            }

            void IDisposable.Dispose()
            {
                WebCore.Shutdown();
                _EndTask.Wait();
            }
        }

        protected IDisposable Tester()
        {
            return new TestContext(this);
        }
      
        //private Task<SynchronizationContext> Init()
        //{
        //    //TaskCompletionSource<SynchronizationContext> tcs = new TaskCompletionSource<SynchronizationContext>();
        //    //Task.Factory.StartNew(() =>
        //    //{
        //    //    WebCore.Initialize(new WebConfig());
        //    //    WebSession session = WebCore.CreateWebSession(WebPreferences.Default);

        //    //    _WebView = WebCore.CreateWebView(500, 500);
        //    //    _WebView.Source = new Uri(string.Format("{0}\\javascript\\index.html", Assembly.GetExecutingAssembly().GetPath()));

        //    //    WebCore.Started += (o, e) => { tcs.SetResult(SynchronizationContext.Current); };

        //    //    while (_WebView.IsLoading)
        //    //    {
        //    //        WebCore.Run();
        //    //    }
        //    //}
        //    //);

        //    //return tcs.Task;
        //}

      

        protected T GetSafe<T>(Func<T> UnsafeGet)
        {
            T res = default(T);
            _SynchronizationContext.Send(_ => res = UnsafeGet(), null);
            return res;
        }

        protected void DoSafe(Action Doact)
        {
            _SynchronizationContext.Send(_ => Doact(), null);
        }
    }
}
