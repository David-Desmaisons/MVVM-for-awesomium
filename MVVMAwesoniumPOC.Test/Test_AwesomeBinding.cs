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

using System.Reflection;
using System.Threading.Tasks;
using System.Threading;

namespace MVVMAwesonium.Test
{
    public class Test_AwesomeBinding : Awesomium_Test_Base
    {
        private Person _DataContext;


        public Test_AwesomeBinding()
            : base()
        {
            _DataContext = new Person()
            {
                Name = "O Monstro",
                LastName = "Desmaisons",
                Local = new Local() { City = "Florianopolis", Region = "SC" }
            };

            _DataContext.Skills.Add(new Skill() { Name = "Langage", Type = "French" });
            _DataContext.Skills.Add(new Skill() { Name = "Info", Type = "C++" });
        }

        [Fact]
        public void Test_AwesomeBinding_Basic_OneWay()
        {
            bool isValidSynchronizationContext = (_SynchronizationContext != null) && (_SynchronizationContext.GetType() != typeof(SynchronizationContext));
            isValidSynchronizationContext.Should().BeTrue();


            using (var mb = AwesomeBinding.Bind(_WebView, _DataContext,JavascriptBindingMode.OneWay).Result)
            {  
                Thread.Sleep(500);
                var js = mb.JSRootObject;
              

                JSValue res = GetSafe(() => js.Invoke("Name"));
                ((string)res).Should().Be("O Monstro");

                JSValue res2 = GetSafe(() => js.Invoke("LastName"));
                ((string)res2).Should().Be("Desmaisons");

                _DataContext.Name = "23";

                JSValue res3 = GetSafe(() => js.Invoke("Name"));
                ((string)res3).Should().Be("23");

                JSValue res4 = GetSafe(() => ((JSObject)js.Invoke("Local")).Invoke("City"));
                ((string)res4).Should().Be("Florianopolis");

                _DataContext.Local.City = "Paris";

                res4 = GetSafe(() => ((JSObject)js.Invoke("Local")).Invoke("City"));
                ((string)res4).Should().Be("Paris");

                JSValue res5 = GetSafe(() => (((JSObject)((JSValue[])js.Invoke("Skills"))[0]).Invoke("Name")));
                ((string)res5).Should().Be("Langage");

                _DataContext.Skills[0].Name = "Ling";

                res5 = GetSafe(() => (((JSObject)((JSValue[])js.Invoke("Skills"))[0]).Invoke("Name")));
                ((string)res5).Should().Be("Ling");
            }
        }

        private Task WaitLoad(IWebView view)
        {
            TaskCompletionSource<object> tcs = new TaskCompletionSource<object>();

            UrlEventHandler ea = null;
            ea = (o, e) => { tcs.SetResult(null); view.DocumentReady -= ea; };
            view.DocumentReady += ea;

            return tcs.Task;
        }


        [Fact]
        public void Test_AwesomeBinding_BasicAlreadyLoaded_OneWay()
        {
            bool isValidSynchronizationContext = (_SynchronizationContext != null) && (_SynchronizationContext.GetType() != typeof(SynchronizationContext));
            isValidSynchronizationContext.Should().BeTrue();

            WaitLoad(_WebView).Wait();

            using (var mb = AwesomeBinding.Bind(_WebView, _DataContext, JavascriptBindingMode.OneWay).Result)
            {
                mb.Should().NotBeNull();
            }
        }

        private JSValue Get(JSObject root, string pn)
        {
            return root.Invoke(pn);
        }

        [Fact]
        public void Test_AwesomeBinding_Basic_TwoWay()
        {
            bool isValidSynchronizationContext = (_SynchronizationContext != null) && (_SynchronizationContext.GetType() != typeof(SynchronizationContext));
            isValidSynchronizationContext.Should().BeTrue();


            using (var mb = AwesomeBinding.Bind(_WebView, _DataContext, JavascriptBindingMode.TwoWay).Result)
            {
                Thread.Sleep(500);
                //Teste One Way
                var js = mb.JSRootObject;

                JSValue res = GetSafe(()=> Get(js, "Name"));
                ((string)res).Should().Be("O Monstro");

                JSValue res2 = GetSafe(() => js.Invoke("LastName"));
                ((string)res2).Should().Be("Desmaisons");

                _DataContext.Name = "23";

                JSValue res3 = GetSafe(() => js.Invoke("Name"));
                ((string)res3).Should().Be("23");

                JSValue res4 = GetSafe(() => ((JSObject)js.Invoke("Local")).Invoke("City"));
                ((string)res4).Should().Be("Florianopolis");

                _DataContext.Local.City = "Paris";

                res4 = GetSafe(() => ((JSObject)js.Invoke("Local")).Invoke("City"));
                ((string)res4).Should().Be("Paris");

                JSValue res5 = GetSafe(() => (((JSObject)((JSValue[])js.Invoke("Skills"))[0]).Invoke("Name")));
                ((string)res5).Should().Be("Langage");

                _DataContext.Skills[0].Name = "Ling";

                res5 = GetSafe(() => (((JSObject)((JSValue[])js.Invoke("Skills"))[0]).Invoke("Name")));
                ((string)res5).Should().Be("Ling");

                //Teste Two Way
                this.DoSafe(() => mb.JSRootObject.Invoke("Name", "resName"));
                JSValue resName = GetSafe(() => mb.JSRootObject.Invoke("Name"));
                ((string)resName).Should().Be("resName");

                Thread.Sleep(500);

                _DataContext.Name.Should().Be("resName");
            }
        }
    }
};

