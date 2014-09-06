using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Awesomium.Core;
using Xunit;
using NSubstitute;
using FluentAssertions;
using MVVMAwesonium.ViewModelExample;
using MVVMAwesonium.AwesomiumBinding;
using MVVMAwesonium.Infra;
using System.Reflection;
using System.Threading.Tasks;
using System.Threading;

namespace MVVMAwesonium.Test
{
    public class Test_AwesomeBinding
    {
        private IWebView _WebView = null;
        private Person _DataContext;
        private SynchronizationContext _SynchronizationContext;
      
        private Task<SynchronizationContext> Init()
        {
            TaskCompletionSource<SynchronizationContext> tcs = new TaskCompletionSource<SynchronizationContext>();
            Task.Factory.StartNew(() =>
            {
                WebCore.Initialize(new WebConfig());
                WebSession session = WebCore.CreateWebSession(WebPreferences.Default);

                _WebView = WebCore.CreateWebView(500, 500);
                _WebView.Source = new Uri(string.Format("{0}\\src\\index.html", Assembly.GetExecutingAssembly().GetPath()));

                WebCore.Started += (o, e) => { tcs.SetResult(SynchronizationContext.Current); };

                while (_WebView.IsLoading)
                {
                    WebCore.Run();
                }
            }
            );

            return tcs.Task;
        }

        public Test_AwesomeBinding()
        {
            _DataContext = new Person()
            {
                Name = "O Monstro",
                LastName = "Desmaisons",
                Local = new Local() { City = "Florianopolis", Region = "SC" }
            };

            _DataContext.Skills.Add(new Skill() { Name = "Langage", Type = "French" });
            _DataContext.Skills.Add(new Skill() { Name = "Info", Type = "C++" });

            _SynchronizationContext = Init().Result;
        }

        private T GetSafe<T>(Func<T> UnsafeGet)
        {
            T res = default(T);
            _SynchronizationContext.Send(_ => res = UnsafeGet(), null);
            return res;
        }

        private void DoSafe(Action Doact)
        {
            _SynchronizationContext.Send(_ => Doact(), null);
        }

        [Fact]
        public void Test_AwesomeBinding_Basic()
        {
            bool isValidSynchronizationContext = (_SynchronizationContext != null) && (_SynchronizationContext.GetType() != typeof(SynchronizationContext));
            isValidSynchronizationContext.Should().BeTrue();


            using (var mb = AwesomeBinding.ApplyBinding(_WebView, _DataContext).Result)
            {
                var js = mb.JSRootObject;

                
                JSValue res = GetSafe( () => js.Invoke("Name"));
                ((string)res).Should().Be("O Monstro");

                JSValue res2 = GetSafe(() => js.Invoke("LastName"));
                ((string)res2).Should().Be("Desmaisons");

                _DataContext.Name = "23";

                JSValue res3 = GetSafe(() => js.Invoke("Name"));
                ((string)res3).Should().Be("23");

                JSValue res4 = GetSafe(() => ((JSObject)js["Local"]).Invoke("City"));
                ((string)res4).Should().Be("Florianopolis");

                _DataContext.Local.City = "Paris";

                res4 = GetSafe(() => ((JSObject)js["Local"]).Invoke("City"));
                ((string)res4).Should().Be("Paris");
                
            }
        }


    }
}
