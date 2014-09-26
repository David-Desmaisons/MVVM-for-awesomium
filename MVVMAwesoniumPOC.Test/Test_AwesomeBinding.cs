using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Awesomium.Core;
using Xunit;
using NSubstitute;
using FluentAssertions;
using MVVMAwesomium.AwesomiumBinding;
using MVVMAwesomium.Infra;
using System.Reflection;
using System.Threading.Tasks;
using System.Threading;

using MVVMAwesomium.ViewModel.Example;
using Newtonsoft.Json;
using System.Windows.Input;
using MVVMAwesomium.ViewModel;
using System.Collections.ObjectModel;

namespace MVVMAwesomium.Test
{
    public class Test_AwesomeBinding : Awesomium_Test_Base
    {
        private Person _DataContext;
        private ICommand _ICommand;


        public Test_AwesomeBinding()
            : base()
        {
            _ICommand = Substitute.For<ICommand>();
            _DataContext = new Person(_ICommand)
            {
                Name = "O Monstro",
                LastName = "Desmaisons",
                Local = new Local() { City = "Florianopolis", Region = "SC" },
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
                    var js = (JSObject)jsbridge.GetJSSessionValue();

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
                    var js = (JSObject)mb.JSRootObject.GetJSSessionValue();

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

        private class ViewModelTest : ViewModelBase
        {
            public ICommand Command{ get; set; }

            public string Name { get { return "NameTest"; } }

            public string UselessName { set { } }

            public void InconsistentEventEmit()
            {
                this.OnPropertyChanged("NonProperty");
            }
        }

        [Fact]
        public void Test_AwesomeBinding_Basic_Property_Test()
        {
            using (Tester())
            {
                var command = Substitute.For<ICommand>();
                var datacontexttest = new ViewModelTest() { Command = command };

                using (var mb = AwesomeBinding.Bind(_WebView, datacontexttest, JavascriptBindingMode.TwoWay).Result)
                {
                    var js = (JSObject)mb.JSRootObject.GetJSSessionValue();

                    JSValue res = GetSafe(() => js.Invoke("Name"));
                    res.Should().NotBeNull();
                    ((string)res).Should().Be("NameTest");

                    res = GetSafe(() => js.Invoke("Name","NewName"));
                    res = GetSafe(() => js.Invoke("Name"));
                    res.Should().NotBeNull();
                    ((string)res).Should().Be("NewName");

                    Thread.Sleep(100);
                    datacontexttest.Name.Should().Be("NameTest");

                    bool resf = GetSafe(() => js.HasProperty("UselessName"));
                    resf.Should().BeFalse();

                    Action Safe = () =>  datacontexttest.InconsistentEventEmit();

                    Safe.ShouldNotThrow("Inconsistent Name in property should not throw exception");

                }
            }
        }

        [Fact]
        public void Test_AwesomeBinding_Basic_TwoWay_Command_Basic()
        {
            using (Tester())
            {
                var command = Substitute.For<ICommand>();
                var test = new ViewModelTest() { Command = command };

                using (var mb = AwesomeBinding.Bind(_WebView, test, JavascriptBindingMode.TwoWay).Result)
                {
                    var js = mb.JSRootObject as JSGenericObject;

                    var mycommand = js.Attributes["Command"] as JSCommand;
                    mycommand.Should().NotBeNull();
                    mycommand.ToString().Should().Be("{}");
                    mycommand.Type.Should().Be(JSBridgeType.Command);
                    mycommand.MappedJSValue.Should().NotBeNull();
                }
            }
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
                    var js = (JSObject)mb.JSRootObject.GetJSSessionValue();

                    JSObject mycommand = (JSObject)GetSafe(() => js.Invoke("Command"));
                    JSValue res = GetSafe(() => mycommand.Invoke("Execute"));
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
                    var js = (JSObject)mb.JSRootObject.GetJSSessionValue();

                    JSObject mycommand = (JSObject)GetSafe(() => js.Invoke("Command"));
                    JSValue res = GetSafe(() => mycommand.Invoke("Execute", js));
                    Thread.Sleep(100);
                    command.Received().Execute(test);
                }
            }
        }

        [Fact]
        public void Test_AwesomeBinding_Basic_TwoWay_Command_CanExecute_False()
        {
            using (Tester())
            {
                var command = Substitute.For<ICommand>();
                command.CanExecute(Arg.Any<object>()).Returns(false);
                var test = new ViewModelTest() { Command = command };

                using (var mb = AwesomeBinding.Bind(_WebView, test, JavascriptBindingMode.TwoWay).Result)
                {
                    var js = (JSObject)mb.JSRootObject.GetJSSessionValue();

                    JSObject mycommand = (JSObject)GetSafe(() => js.Invoke("Command"));
                    JSValue res = GetSafe(() => mycommand.Invoke("CanExecuteValue"));

                    ((bool)res).Should().BeFalse();
                }
            }
        }

        [Fact]
        public void Test_AwesomeBinding_Basic_TwoWay_Command_CanExecute_True()
        {
            using (Tester())
            {
                var command = Substitute.For<ICommand>();
                command.CanExecute(Arg.Any<object>()).Returns(true);
                var test = new ViewModelTest() { Command = command };

                using (var mb = AwesomeBinding.Bind(_WebView, test, JavascriptBindingMode.TwoWay).Result)
                {
                    var js = (JSObject)mb.JSRootObject.GetJSSessionValue();

                    JSObject mycommand = (JSObject)GetSafe(() => js.Invoke("Command"));
                    JSValue res = GetSafe(() => mycommand.Invoke("CanExecuteValue"));

                    ((bool)res).Should().BeTrue();
                }
            }
        }

        [Fact]
        public void Test_AwesomeBinding_Basic_TwoWay_Command_CanExecute_Refresh_Ok()
        {
            using (Tester())
            {
                bool canexecute = true;
                _ICommand.CanExecute(Arg.Any<object>()).ReturnsForAnyArgs(x => canexecute);

                using (var mb = AwesomeBinding.Bind(_WebView, _DataContext, JavascriptBindingMode.TwoWay).Result)
                {
                    var js = (JSObject)mb.JSRootObject.GetJSSessionValue();

                    JSObject mycommand = (JSObject)GetSafe(() => js.Invoke("TestCommand"));
                    JSValue res = GetSafe(() => mycommand.Invoke("CanExecuteValue"));

                    ((bool)res).Should().BeTrue();

                    canexecute = false;
                    _ICommand.CanExecuteChanged += Raise.EventWith(_ICommand, new EventArgs());

                    Thread.Sleep(100);

                    res = GetSafe(() => GetValue(mycommand, "TestCommand"));
                    ((bool)res).Should().BeFalse();
                }
            }
        }
        
        [Fact]
        public void Test_AwesomeBinding_Basic_TwoWay_Command_CanExecute_Refresh_Ok_Argument()
        {
            using (Tester())
            {
                bool canexecute = true;
                _ICommand.CanExecute(Arg.Any<object>()).ReturnsForAnyArgs(x => canexecute);

                using (var mb = AwesomeBinding.Bind(_WebView, _DataContext, JavascriptBindingMode.TwoWay).Result)
                {
                    var js = (JSObject)mb.JSRootObject.GetJSSessionValue();

                    JSObject mycommand = (JSObject)GetSafe(() => js.Invoke("TestCommand"));
                    JSValue res = GetSafe(() => mycommand.Invoke("CanExecuteValue"));
                    ((bool)res).Should().BeTrue();

                    _ICommand.Received().CanExecute(_DataContext);

                    canexecute = false;
                    _ICommand.ClearReceivedCalls();

                    _ICommand.CanExecuteChanged += Raise.EventWith(_ICommand, new EventArgs());

                    Thread.Sleep(100);

                    _ICommand.Received().CanExecute(_DataContext);


                    res = GetSafe(() => GetValue(mycommand, "TestCommand"));
                    ((bool)res).Should().BeFalse();
                }
            }
        }


        [Fact]
        public void Test_AwesomeBinding_Basic_TwoWay_Command_CanExecute_Refresh_Ok_Argument_Exception()
        {
            using (Tester())
            {
                _ICommand.CanExecute(Arg.Any<object>()).Returns(x => {  if (x[0] == null) throw new Exception(); return false;  });

                using (var mb = AwesomeBinding.Bind(_WebView, _DataContext, JavascriptBindingMode.TwoWay).Result)
                {
                    _ICommand.Received().CanExecute(Arg.Any<object>());
                    var js = (JSObject)mb.JSRootObject.GetJSSessionValue();

                    JSObject mycommand = (JSObject)GetSafe(() => js.Invoke("TestCommand"));
                    JSValue res = GetSafe(() => mycommand.Invoke("CanExecuteValue"));
                    ((bool)res).Should().BeFalse();

                    _ICommand.Received().CanExecute(_DataContext);
                }
            }
        }

        private JSValue GetValue(JSObject jso, string pn)
        {
            return jso.Invoke(pn);
        }

        [Fact]
        public void Test_AwesomeBinding_Basic_TwoWay_Command_With_Null_Parameter()
        {
            using (Tester())
            {
                var command = Substitute.For<ICommand>();
                var test = new ViewModelTest() { Command = command };

                using (var mb = AwesomeBinding.Bind(_WebView, test, JavascriptBindingMode.TwoWay).Result)
                {
                    var js = (JSObject)mb.JSRootObject.GetJSSessionValue();

                    JSObject mycommand = (JSObject)GetSafe(() => js.Invoke("Command"));
                    JSValue res = GetSafe(() => mycommand.Invoke("Execute",null));
                    Thread.Sleep(100);
                    command.Received().Execute(null);
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
                    var js = mb.JSRootObject.GetJSSessionValue();

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


                    _DataContext.Skills.Insert(0, new Skill() { Name = "C#", Type = "Info" });
                    Thread.Sleep(100);
                    res = GetSafe(() => Get(js, "Skills"));
                    res.Should().NotBeNull();
                    Check((JSValue[])res, _DataContext.Skills);

                    _DataContext.Skills.RemoveAt(1);
                    Thread.Sleep(100);
                    res = GetSafe(() => Get(js, "Skills"));
                    res.Should().NotBeNull();
                    Check((JSValue[])res, _DataContext.Skills);

                    _DataContext.Skills[0] = new Skill() { Name = "HTML", Type = "Info" };
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

        [Fact]
        public void Test_AwesomeBinding_Basic_TwoWay_Collection_FromJSUpdate()
        {
            using (Tester())
            {

                bool isValidSynchronizationContext = (_SynchronizationContext != null) && (_SynchronizationContext.GetType() != typeof(SynchronizationContext));
                isValidSynchronizationContext.Should().BeTrue();

                using (var mb = AwesomeBinding.Bind(_WebView, _DataContext, JavascriptBindingMode.TwoWay).Result)
                {
                    var root = mb.JSRootObject as JSGenericObject;
                    var js = root.GetJSSessionValue();

                    JSValue res = GetSafe(() => Get(js, "Skills"));
                    res.Should().NotBeNull();
                    var col = ((JSValue[])res);
                    col.Length.Should().Be(2);

                    Check(col, _DataContext.Skills);

                    JSObject coll = GetSafe(() => ((JSObject)js)["Skills"]);
                    DoSafe(() => coll.Invoke("push", (root.Attributes["Skills"] as JSArray).Items[0].GetJSSessionValue()));

                    Thread.Sleep(100);
                    _DataContext.Skills.Should().HaveCount(3);
                    _DataContext.Skills[2].Should().Be(_DataContext.Skills[0]);
                    res = GetSafe(() => Get(js, "Skills"));
                    res.Should().NotBeNull();
                    Check((JSValue[])res, _DataContext.Skills);

                    DoSafe(() => coll.Invoke("pop"));

                    Thread.Sleep(100);
                    _DataContext.Skills.Should().HaveCount(2);
                    res = GetSafe(() => Get(js, "Skills"));
                    res.Should().NotBeNull();
                    Check((JSValue[])res, _DataContext.Skills);

                    DoSafe(() => coll.Invoke("shift"));

                    Thread.Sleep(100);
                    _DataContext.Skills.Should().HaveCount(1);
                    res = GetSafe(() => Get(js, "Skills"));
                    res.Should().NotBeNull();
                    Check((JSValue[])res, _DataContext.Skills);


                    DoSafe(() => coll.Invoke("unshift",
                        (root.Attributes["Skills"] as JSArray).Items[0].GetJSSessionValue()));
                    
                    Thread.Sleep(100);
                    _DataContext.Skills.Should().HaveCount(2);
                    res = GetSafe(() => Get(js, "Skills"));
                    res.Should().NotBeNull();
                    Check((JSValue[])res, _DataContext.Skills);
                }
            }
        }

        private class VMWithList : ViewModelBase
        {
            public VMWithList()
            {
                List = new ObservableCollection<string>();
            }
            public ObservableCollection<string> List { get; private set; }

        }


        private void Checkstring(JSValue[] coll, IList<string> iskill)
        {
            coll.Length.Should().Be(iskill.Count);
            coll.ForEach((c, i) =>
            {
                (GetSafe(() => (string)c)).Should().Be(iskill[i]);
               
            });

        }

        [Fact]
        public void Test_AwesomeBinding_Basic_TwoWay_Collection_string()
        {
            using (Tester())
            {

                bool isValidSynchronizationContext = (_SynchronizationContext != null) && (_SynchronizationContext.GetType() != typeof(SynchronizationContext));
                isValidSynchronizationContext.Should().BeTrue();

                var datacontext = new VMWithList();

                using (var mb = AwesomeBinding.Bind(_WebView, datacontext, JavascriptBindingMode.TwoWay).Result)
                {
                    var js = mb.JSRootObject.GetJSSessionValue();

                    JSValue res = GetSafe(() => Get(js, "List"));
                    res.Should().NotBeNull();
                    var col = ((JSValue[])res);
                    col.Length.Should().Be(0);

                    Checkstring(col, datacontext.List);

                    datacontext.List.Add("titi");

                    Thread.Sleep(100);
                    res = GetSafe(() => Get(js, "List"));
                    col = ((JSValue[])res);

                    Checkstring(col, datacontext.List);

                    datacontext.List.Add("kiki");
                    datacontext.List.Add("toto");

                    Thread.Sleep(100);
                    res = GetSafe(() => Get(js, "List"));
                    col = ((JSValue[])res);

                    Checkstring(col, datacontext.List);

                    Thread.Sleep(100);
                    res = GetSafe(() => Get(js, "List"));
                    col = ((JSValue[])res);

                    Checkstring(col, datacontext.List);

                }
            }
        }
    }
};

