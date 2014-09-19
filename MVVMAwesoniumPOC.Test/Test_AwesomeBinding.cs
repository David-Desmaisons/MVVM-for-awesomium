using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Awesomium.Core;
using Xunit;
using NSubstitute;
using FluentAssertions;
using MVVMAwesonium.AwesomiumBinding;
using MVVMAwesonium.Infra;
using System.Reflection;
using System.Threading.Tasks;
using System.Threading;

using MVVMAwesonium.ViewModel.Example;
using Newtonsoft.Json;
using System.Windows.Input;

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
                Local = new Local() { City = "Florianopolis", Region = "SC" },
                //BirthDay = new DateTime(1974,2,26)
            };

            _DataContext.Skills.Add(new Skill() { Name = "Langage", Type = "French" });
            _DataContext.Skills.Add(new Skill() { Name = "Info", Type = "C++" });
        }

        [Fact]
        public void Test_AwesomeBinding_Basic_OneWay()
        {
            using (Tester())
            {
                bool isValidSynchronizationContext = (_SynchronizationContext != null) && (_SynchronizationContext.GetType() != typeof(SynchronizationContext));
                isValidSynchronizationContext.Should().BeTrue();


                using (var mb = AwesomeBinding.Bind(_WebView, _DataContext, JavascriptBindingMode.OneWay).Result)
                {
                    var jsbridge = mb.JSRootObject;
                    var js = (JSObject)jsbridge.GetSessionValue();

                    string JSON = JsonConvert.SerializeObject(_DataContext);
                    string alm = jsbridge.ToString();

                    Person m = JsonConvert.DeserializeObject<Person>(jsbridge.ToString());
                    m.LastName.Should().Be("Desmaisons");
                    m.Name.Should().Be("O Monstro");

                    JSValue res = GetSafe(() => js.Invoke("Name"));
                    ((string)res).Should().Be("O Monstro");

                    JSValue res2 = GetSafe(() => js.Invoke("LastName"));
                    ((string)res2).Should().Be("Desmaisons");

                    _DataContext.Name = "23";

                    JSValue res3 = GetSafe(() => js.Invoke("Name"));
                    ((string)res3).Should().Be("O Monstro");

                    JSValue res4 = GetSafe(() => ((JSObject)js.Invoke("Local")).Invoke("City"));
                    ((string)res4).Should().Be("Florianopolis");

                    _DataContext.Local.City = "Paris";

                    res4 = GetSafe(() => ((JSObject)js.Invoke("Local")).Invoke("City"));
                    ((string)res4).Should().Be("Florianopolis");

                    JSValue res5 = GetSafe(() => (((JSObject)((JSValue[])js.Invoke("Skills"))[0]).Invoke("Name")));
                    ((string)res5).Should().Be("Langage");

                    _DataContext.Skills[0].Name = "Ling";

                    res5 = GetSafe(() => (((JSObject)((JSValue[])js.Invoke("Skills"))[0]).Invoke("Name")));
                    ((string)res5).Should().Be("Langage");
                }
            }

            //WebCore.Shutdown();
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
            using (Tester())
            {

                bool isValidSynchronizationContext = (_SynchronizationContext != null) && (_SynchronizationContext.GetType() != typeof(SynchronizationContext));
                isValidSynchronizationContext.Should().BeTrue();

                WaitLoad(_WebView).Wait();

                using (var mb = AwesomeBinding.Bind(_WebView, _DataContext, JavascriptBindingMode.OneWay).Result)
                {
                    mb.Should().NotBeNull();
                }
            }

            //WebCore.Shutdown();
        }

        private JSValue Get(JSObject root, string pn)
        {
            return root.Invoke(pn);
        }

        [Fact]
        public void Test_AwesomeBinding_Basic_TwoWay()
        {
            using (Tester())
            {

                bool isValidSynchronizationContext = (_SynchronizationContext != null) && (_SynchronizationContext.GetType() != typeof(SynchronizationContext));
                isValidSynchronizationContext.Should().BeTrue();

                using (var mb = AwesomeBinding.Bind(_WebView, _DataContext, JavascriptBindingMode.TwoWay).Result)
                {
                    var js = (JSObject)mb.JSRootObject.GetSessionValue();

                    JSValue res = GetSafe(() => Get(js, "Name"));
                    ((string)res).Should().Be("O Monstro");

                    JSValue res2 = GetSafe(() => js.Invoke("LastName"));
                    ((string)res2).Should().Be("Desmaisons");

                    _DataContext.Name = "23";

                    Thread.Sleep(50);
                    JSValue res3 = GetSafe(() => js.Invoke("Name"));
                    ((string)res3).Should().Be("23");

                    JSValue res4 = GetSafe(() => ((JSObject)js.Invoke("Local")).Invoke("City"));
                    ((string)res4).Should().Be("Florianopolis");

                    _DataContext.Local.City = "Paris";
                    Thread.Sleep(50);

                    res4 = GetSafe(() => ((JSObject)js.Invoke("Local")).Invoke("City"));
                    ((string)res4).Should().Be("Paris");

                    JSValue res5 = GetSafe(() => (((JSObject)((JSValue[])js.Invoke("Skills"))[0]).Invoke("Name")));
                    ((string)res5).Should().Be("Langage");

                    _DataContext.Skills[0].Name = "Ling";
                    Thread.Sleep(50);

                    res5 = GetSafe(() => (((JSObject)((JSValue[])js.Invoke("Skills"))[0]).Invoke("Name")));
                    ((string)res5).Should().Be("Ling");

                    //Teste Two Way
                    this.DoSafe(() => js.Invoke("Name", "resName"));
                    JSValue resName = GetSafe(() => js.Invoke("Name"));
                    ((string)resName).Should().Be("resName");

                    Thread.Sleep(500);

                    _DataContext.Name.Should().Be("resName");
                }
            }
        }

        private void Check(JSValue[] coll, IList<Skill> iskill)
        {
            coll.Length.Should().Be(iskill.Count);
            coll.ForEach((c, i) =>
                            {
                                ((string)(GetSafe(() => Get(c, "Name")))).Should().Be(iskill[i].Name);
                                ((string)(GetSafe(() => Get(c, "Type")))).Should().Be(iskill[i].Type);
                            });
             
        }

        private class ViewModelTest
        {
            public ICommand Command{ get; set; }
        }

        [Fact]
        public void Test_AwesomeBinding_Basic_TwoWay_Command()
        {
            using (Tester())
            {
                var command = Substitute.For<ICommand>();
                var test = new ViewModelTest() { Command = command };

                using (var mb = AwesomeBinding.Bind(_WebView, test, JavascriptBindingMode.TwoWay).Result)
                {
                    var js = (JSObject)mb.JSRootObject.GetSessionValue();

                    JSValue res = GetSafe(() =>js.Invoke("Command"));
                    Thread.Sleep(100);
                    command.Received().Execute(Arg.Any<object>());
                }
            }
        }


        [Fact]
        public void Test_AwesomeBinding_Basic_TwoWay_Command_With_Parameter()
        {
            using (Tester())
            {
                var command = Substitute.For<ICommand>();
                var test = new ViewModelTest() { Command = command };

                using (var mb = AwesomeBinding.Bind(_WebView, test, JavascriptBindingMode.TwoWay).Result)
                {
                    var js = (JSObject)mb.JSRootObject.GetSessionValue();

                    JSValue res = GetSafe(() => js.Invoke("Command",js));
                    Thread.Sleep(100);
                    command.Received().Execute(test);
                }
            }
        }

        [Fact]
        public void Test_AwesomeBinding_Basic_TwoWay_Collection()
        {
            using (Tester())
            {

                bool isValidSynchronizationContext = (_SynchronizationContext != null) && (_SynchronizationContext.GetType() != typeof(SynchronizationContext));
                isValidSynchronizationContext.Should().BeTrue();

                using (var mb = AwesomeBinding.Bind(_WebView, _DataContext, JavascriptBindingMode.TwoWay).Result)
                {
                    var js = mb.JSRootObject.GetSessionValue();

                    JSValue res = GetSafe(() => Get(js, "Skills"));
                    res.Should().NotBeNull();
                    var col = ((JSValue[])res);
                    col.Length.Should().Be(2);

                    Check(col, _DataContext.Skills);

                    _DataContext.Skills.Add(new Skill() { Name = "C++", Type = "Info" });

                    Thread.Sleep(100);
                    res = GetSafe(() => Get(js, "Skills"));
                    res.Should().NotBeNull();
                    Check((JSValue[])res, _DataContext.Skills);


                    _DataContext.Skills.Insert(0,new Skill() { Name = "C#", Type = "Info" });
                    Thread.Sleep(100);
                    res = GetSafe(() => Get(js, "Skills"));
                    res.Should().NotBeNull();
                    Check((JSValue[])res, _DataContext.Skills);

                    _DataContext.Skills.RemoveAt(1);
                    Thread.Sleep(100);
                    res = GetSafe(() => Get(js, "Skills"));
                    res.Should().NotBeNull();
                    Check((JSValue[])res, _DataContext.Skills);

                    _DataContext.Skills[0] = new Skill() { Name="HTML", Type="Info"};
                    Thread.Sleep(100);
                    res = GetSafe(() => Get(js, "Skills"));
                    res.Should().NotBeNull();
                    Check((JSValue[])res, _DataContext.Skills);

                    _DataContext.Skills[0] = new Skill() { Name = "HTML5", Type = "Info" };
                    _DataContext.Skills.Insert(0, new Skill() { Name = "HTML5", Type = "Info" });
                    Thread.Sleep(100);
                    res = GetSafe(() => Get(js, "Skills"));
                    res.Should().NotBeNull();
                    Check((JSValue[])res, _DataContext.Skills);


                    _DataContext.Skills.Clear();
                    Thread.Sleep(100);
                    res = GetSafe(() => Get(js, "Skills"));
                    res.Should().NotBeNull();
                    Check((JSValue[])res, _DataContext.Skills);

                }
            }
        }
    }
};

