﻿using System;
using System.Collections;
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
using System.Diagnostics;
using MVVMAwesomium.ViewModel.Infra;
using MVVMAwesomium.Exceptions;
using MVVMAwesomium.Test.ViewModel.Test;
using MVVM.Component;

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
                PersonalState = PersonalState.Married
            };

            _DataContext.Skills.Add(new Skill() { Name = "Langage", Type = "French" });
            _DataContext.Skills.Add(new Skill() { Name = "Info", Type = "C++" });
        }

        [Fact]
        public void Test_AwesomeBinding_Basic_OneWay_JSON_ToString()
        {
            using (Tester())
            {
                bool isValidSynchronizationContext = (_SynchronizationContext != null) && (_SynchronizationContext.GetType() != typeof(SynchronizationContext));
                isValidSynchronizationContext.Should().BeTrue();


                using (var mb = AwesomeBinding.Bind(_WebView, _DataContext, JavascriptBindingMode.OneWay).Result)
                {
                    var jsbridge = (mb as AwesomeBinding).JSBrideRootObject;
                    var js = mb.JSRootObject;

                    string JSON = JsonConvert.SerializeObject(_DataContext);
                    string alm = jsbridge.ToString();

                    JSArray arr = (JSArray)jsbridge.GetAllChildren().Where(c => c is JSArray).First();

                    string stringarr = arr.ToString();

                    dynamic m = JsonConvert.DeserializeObject<dynamic>(jsbridge.ToString());
                    ((string)m.LastName).Should().Be("Desmaisons");
                    ((string)m.Name).Should().Be("O Monstro");

                    mb.ToString().Should().Be(alm);
                }
            }
        }

        [Fact]
        public void Test_AwesomeBinding_Basic_OneTime()
        {
            using (Tester())
            {
                bool isValidSynchronizationContext = (_SynchronizationContext != null) && (_SynchronizationContext.GetType() != typeof(SynchronizationContext));
                isValidSynchronizationContext.Should().BeTrue();


                using (var mb = AwesomeBinding.Bind(_WebView, _DataContext, JavascriptBindingMode.OneTime).Result)
                {
                    var jsbridge = (mb as AwesomeBinding).JSBrideRootObject;
                    var js = mb.JSRootObject;

                    string JSON = JsonConvert.SerializeObject(_DataContext);
                    string alm = jsbridge.ToString();


                    JSValue res = GetSafe(() => js.Invoke("Name"));
                    ((string)res).Should().Be("O Monstro");

                    JSValue res2 = GetSafe(() => js.Invoke("LastName"));
                    ((string)res2).Should().Be("Desmaisons");

                    _DataContext.Name = "23";
                    Thread.Sleep(200);

                    JSValue res3 = GetSafe(() => js.Invoke("Name"));
                    ((string)res3).Should().Be("O Monstro");

                    JSValue res4 = GetSafe(() => ((JSObject)js.Invoke("Local")).Invoke("City"));
                    ((string)res4).Should().Be("Florianopolis");

                    _DataContext.Local.City = "Paris";
                    Thread.Sleep(200);

                    res4 = GetSafe(() => ((JSObject)js.Invoke("Local")).Invoke("City"));
                    ((string)res4).Should().Be("Florianopolis");

                    JSValue res5 = GetSafe(() => (((JSObject)((JSValue[])UnWrapCollection(js,"Skills"))[0]).Invoke("Name")));
                    ((string)res5).Should().Be("Langage");

                    _DataContext.Skills[0].Name = "Ling";
                    Thread.Sleep(200);

                    res5 = GetSafe(() => (((JSObject)((JSValue[])UnWrapCollection(js,"Skills"))[0]).Invoke("Name")));
                    ((string)res5).Should().Be("Langage");

                    this.DoSafe(() => js.Invoke("Name", "resName"));
                    Thread.Sleep(200);
                    _DataContext.Name.Should().Be("23");
                }
            }
        }

        [Fact]
        public void Test_AwesomeBinding_Basic_HTML_Without_Correct_js_ShouldThrowException()
        {
            using (Tester("javascript/empty.html"))
            {
                var vm = new object();
                IAwesomeBinding bd = null;

                Action st = () => bd = AwesomeBinding.Bind(_WebView, _DataContext, JavascriptBindingMode.OneTime).Result;

                st.ShouldThrow<MVVMforAwesomiumException>();
            }
        }

        [Fact]
        public void Test_AwesomeBinding_Basic_HTML_Without_Correct_js_ShouldThrow_Correct_Exception2()
        {
            using (Tester("javascript/almost_empty.html"))
            {
                var vm = new object();
                IAwesomeBinding bd = null;

                Action st = () => bd = AwesomeBinding.Bind(_WebView, vm, JavascriptBindingMode.OneTime).Result;

                st.ShouldThrow<MVVMforAwesomiumException>();
            }
        }

        public void Test_AwesomeBinding_Basic_HTML_Without_Correct_js_ShouldThrow_TimeOut_Exception()
        {

            int r = 1000;
            var datacontext = new TwoList();
            datacontext.L1.AddRange(Enumerable.Range(0, r).Select(i => new Skill()));


            using (Tester())
            {
                IAwesomeBinding bd = null;
                DoSafe(() =>
                _WebView.SynchronousMessageTimeout = 10);

                Action st = () => bd = AwesomeBinding.Bind(_WebView, datacontext, JavascriptBindingMode.OneTime).Result;

                st.ShouldThrow<MVVMforAwesomiumException>();
            }
        }

        [Fact]
        public void Test_AwesomeBinding_Basic_HTML_Without_Correct_js_ShouldThrowCorrectException()
        {
            using (Tester("javascript/empty.html"))
            {
                var vm = new object();
                IAwesomeBinding bd = null;

                Action st = () => bd = AwesomeBinding.Bind(_WebView, vm, JavascriptBindingMode.OneTime).Result;

                st.ShouldThrow<MVVMforAwesomiumException>();
            }
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
                    var jsbridge = (mb as AwesomeBinding).JSBrideRootObject;
                    var js = mb.JSRootObject;

                    string JSON = JsonConvert.SerializeObject(_DataContext);
                    string alm = jsbridge.ToString();


                    JSValue res = GetSafe(() => js.Invoke("Name"));
                    ((string)res).Should().Be("O Monstro");

                    JSValue res2 = GetSafe(() => js.Invoke("LastName"));
                    ((string)res2).Should().Be("Desmaisons");

                    _DataContext.Name = "23";
                    Thread.Sleep(200);

                    JSValue res3 = GetSafe(() => js.Invoke("Name"));
                    ((string)res3).Should().Be("23");

                    JSValue res4 = GetSafe(() => ((JSObject)js.Invoke("Local")).Invoke("City"));
                    ((string)res4).Should().Be("Florianopolis");

                    _DataContext.Local.City = "Paris";
                    Thread.Sleep(200);

                    res4 = GetSafe(() => ((JSObject)js.Invoke("Local")).Invoke("City"));
                    ((string)res4).Should().Be("Paris");

                    JSValue res5 = GetSafe(() => (((JSObject)((JSValue[])UnWrapCollection(js,"Skills"))[0]).Invoke("Name")));
                    ((string)res5).Should().Be("Langage");

                    _DataContext.Skills[0].Name = "Ling";
                    Thread.Sleep(200);

                    res5 = GetSafe(() => (((JSObject)((JSValue[])UnWrapCollection(js,"Skills"))[0]).Invoke("Name")));
                    ((string)res5).Should().Be("Ling");


                    this.DoSafe(() => js.Invoke("Name", "resName"));
                    Thread.Sleep(200);
                    _DataContext.Name.Should().Be("23");
                }
            }
        }

        private class Dummy
        {
            internal Dummy()
            {
                Int = 5;
            }
            public int Int { get; set; }
            public int Explosive { get { throw new Exception();} }
        }


        [Fact]
        public void Test_AwesomeBinding_Basic_OneWay_Property_With_Exception()
        {
            using (Tester())
            {
                bool isValidSynchronizationContext = (_SynchronizationContext != null) && (_SynchronizationContext.GetType() != typeof(SynchronizationContext));
                isValidSynchronizationContext.Should().BeTrue();
                var dt = new Dummy();

                using (var mb = AwesomeBinding.Bind(_WebView, dt, JavascriptBindingMode.OneWay).Result)
                {
                    var jsbridge = (mb as AwesomeBinding).JSBrideRootObject;
                    var js = mb.JSRootObject;

                    JSValue res = GetSafe(() => js.Invoke("Int"));
                    ((int)res).Should().Be(5);
                }
            }
        }

        [Fact]
        public void Test_AwesomeBinding_Basic_Regsiter_Additional_property()
        {
            using (Tester())
            {
                bool isValidSynchronizationContext = (_SynchronizationContext != null) && (_SynchronizationContext.GetType() != typeof(SynchronizationContext));
                isValidSynchronizationContext.Should().BeTrue();


                using (var mb = AwesomeBinding.Bind(_WebView, _DataContext, JavascriptBindingMode.OneWay).Result)
                {
                    var js = mb.JSRootObject;

                    JSValue res = GetSafe(() => GetValue(js, "completeName"));
                    ((string)res).Should().Be("O Monstro Desmaisons");
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
        }

        private JSValue Get(JSObject root, string pn)
        {
            return root.Invoke(pn);
        }

        private bool _Init = false;
        private JSObject _Window;

        private JSValue UnWrapCollection(JSObject root, string pn)
        {
            if (!_Init)
            {
                _Init=true;
                _WebView.ExecuteJavascript("window.Extract=function(fn){return fn();}");
                _Window = _WebView.ExecuteJavascriptWithResult("window");
            }
            return _Window.Invoke("Extract",root.Invoke(pn));
        }

        [Fact]
        public void Test_AwesomeBinding_Basic_Null_Property()
        {
            using (Tester())
            {
                bool isValidSynchronizationContext = (_SynchronizationContext != null) && (_SynchronizationContext.GetType() != typeof(SynchronizationContext));
                isValidSynchronizationContext.Should().BeTrue();

                _DataContext.MainSkill.Should().BeNull();

                using (var mb = AwesomeBinding.Bind(_WebView, _DataContext, JavascriptBindingMode.TwoWay).Result)
                {
                    var js = mb.JSRootObject;

                    JSValue res = GetSafe(() => Get(js, "MainSkill"));
                    res.IsNull.Should().BeTrue();

                    DoSafe(()=>
                    _DataContext.MainSkill = new Skill() { Name = "C++", Type = "Info" });
                    Thread.Sleep(100);

                    res = GetSafe(() => Get(js, "MainSkill"));
                    res.IsNull.Should().BeFalse();
                    JSObject obj = res;
                    obj.Should().NotBeNull();

                    JSValue inf = GetSafe(() => obj.Invoke("Type"));
                    ((string)inf).Should().Be("Info");

                    DoSafe(()=>
                    _DataContext.MainSkill = null);
                    Thread.Sleep(100);

                    res = GetSafe(() => Get(js, "MainSkill"));
                    res.IsNull.Should().BeTrue();

                }
            }
        }

        [Fact]
        public void Test_AwesomeBinding_Basic_Circular_reference()
        {
            using (Tester())
            {
                bool isValidSynchronizationContext = (_SynchronizationContext != null) && (_SynchronizationContext.GetType() != typeof(SynchronizationContext));
                isValidSynchronizationContext.Should().BeTrue();


                var datacontext = new MVVMAwesomium.ViewModel.Example.ForNavigation.Couple();
                var my = new MVVMAwesomium.ViewModel.Example.ForNavigation.Person()
                {
                    Name = "O Monstro",
                    LastName = "Desmaisons",
                    Local = new MVVMAwesomium.ViewModel.Example.Local() { City = "Florianopolis", Region = "SC" }
                };
                my.Couple = datacontext;
                datacontext.One = my;

                using (var mb = AwesomeBinding.Bind(_WebView, datacontext, JavascriptBindingMode.TwoWay).Result)
                {
                    var js = mb.JSRootObject;

                    JSObject One = (JSObject)GetSafe(() => js.Invoke("One"));

                    JSValue res = GetSafe(() => One.Invoke("Name"));
                    ((string)res).Should().Be("O Monstro");

                    JSValue res2 = GetSafe(() => One.Invoke("LastName"));
                    ((string)res2).Should().Be("Desmaisons");

                    //Test no stackoverflow in case of circular refernce
                    var jsbridge = (mb as AwesomeBinding).JSBrideRootObject;
                    string alm = jsbridge.ToString();
                    alm.Should().NotBeNull();


                }
            }
        }

        [Fact]
        public void Test_AwesomeBinding_Basic_TwoWay_TimeOut()
        {
            using (Tester())
            {

                bool isValidSynchronizationContext = (_SynchronizationContext != null) && (_SynchronizationContext.GetType() != typeof(SynchronizationContext));
                isValidSynchronizationContext.Should().BeTrue();

                var fact = new AwesomiumBindingFactory() { InjectionTimeOut = 10 };

                int r = 20000;
                var datacontext = new TwoList();
                datacontext.L1.AddRange(Enumerable.Range(0, r).Select(i => new Skill()));

                Exception bindingex = null;

                var task = fact.Bind(_WebView, datacontext, JavascriptBindingMode.TwoWay).ContinueWith(t => bindingex = t.Exception);
                task.Wait();

                bindingex.Should().BeOfType<AggregateException>();
                var ex = (bindingex as AggregateException).InnerException;
                ex.Should().BeOfType<MVVMforAwesomiumException>();
            }
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
                    var js = mb.JSRootObject;

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

                    JSValue res5 = GetSafe(() => (((JSObject)((JSValue[])UnWrapCollection(js,"Skills"))[0]).Invoke("Name")));
                    ((string)res5).Should().Be("Langage");

                    _DataContext.Skills[0].Name = "Ling";
                    Thread.Sleep(50);

                    res5 = GetSafe(() => (((JSObject)((JSValue[])UnWrapCollection(js,"Skills"))[0]).Invoke("Name")));
                    ((string)res5).Should().Be("Ling");

                    //Teste Two Way
                    this.DoSafe(() => js.Invoke("Name", "resName"));
                    JSValue resName = GetSafe(() => js.Invoke("Name"));
                    ((string)resName).Should().Be("resName");

                    Thread.Sleep(500);

                    _DataContext.Name.Should().Be("resName");

                    _DataContext.Name = "nnnnvvvvvvv";

                    Thread.Sleep(50);
                    res3 = GetSafe(() => js.Invoke("Name"));
                    ((string)res3).Should().Be("nnnnvvvvvvv");
                }
            }
        }

        [Fact]
        public void Test_AwesomeBinding_Basic_TwoWay_Nested()
        {
            using (Tester())
            {

                bool isValidSynchronizationContext = (_SynchronizationContext != null) && (_SynchronizationContext.GetType() != typeof(SynchronizationContext));
                isValidSynchronizationContext.Should().BeTrue();

                using (var mb = AwesomeBinding.Bind(_WebView, _DataContext, JavascriptBindingMode.TwoWay).Result)
                {
                    var js = mb.JSRootObject;

                    JSValue res = GetSafe(() => Get(js, "Name"));
                    ((string)res).Should().Be("O Monstro");

                    JSValue res2 = GetSafe(() => js.Invoke("LastName"));
                    ((string)res2).Should().Be("Desmaisons");

                    JSObject local = (JSObject)GetSafe(() => js.Invoke("Local"));
                    JSValue city = GetSafe(() => local.Invoke("City"));
                    ((string)city).Should().Be("Florianopolis");

                    this.DoSafe(() =>
                    _DataContext.Local = new Local() { City = "Paris" });

                    Thread.Sleep(50);
                    JSObject local2 = (JSObject)GetSafe(() => js.Invoke("Local"));
                    JSValue city2 = GetSafe(() => local2.Invoke("City"));
                    ((string)city2).Should().Be("Paris");

                    _DataContext.Local.City = "Foz de Iguaçu";

                    Thread.Sleep(50);
                    JSObject local3 = (JSObject)GetSafe(() => js.Invoke("Local"));
                    JSValue city3 = GetSafe(() => local3.Invoke("City"));
                    ((string)city3).Should().Be("Foz de Iguaçu");

                }
            }
        }


        [Fact]
        public void Test_AwesomeBinding_TwoWay_Enum()
        {
            using (Tester())
            {

                bool isValidSynchronizationContext = (_SynchronizationContext != null) && (_SynchronizationContext.GetType() != typeof(SynchronizationContext));
                isValidSynchronizationContext.Should().BeTrue();
                _DataContext.Name = "totot";

                using (var mb = AwesomeBinding.Bind(_WebView, _DataContext, JavascriptBindingMode.TwoWay).Result)
                {
                    var js = mb.JSRootObject;

                    JSValue res = GetSafe(() => Get(js, "PersonalState"));
                    JSValue dres = GetSafe(() => ((JSObject)res)["displayName"]);
                    ((string)dres).Should().Be("Married");

                    _DataContext.PersonalState = PersonalState.Single;
                    Thread.Sleep(50);

                    res = GetSafe(() => Get(js, "PersonalState"));
                    dres = GetSafe(() => ((JSObject)res)["displayName"]);
                    ((string)dres).Should().Be("Single");
                }
            }
        }

        [Fact]
        public void Test_AwesomeBinding_TwoWay_Enum_Round_Trip()
        {
            using (Tester())
            {

                bool isValidSynchronizationContext = (_SynchronizationContext != null) && (_SynchronizationContext.GetType() != typeof(SynchronizationContext));
                isValidSynchronizationContext.Should().BeTrue();
                _DataContext.Name = "toto";

                using (var mb = AwesomeBinding.Bind(_WebView, _DataContext, JavascriptBindingMode.TwoWay).Result)
                {
                    var js = mb.JSRootObject;

                    JSValue res = GetSafe(() => Get(js, "PersonalState"));
                    JSValue dres = GetSafe(() => ((JSObject)res)["displayName"]);
                    ((string)dres).Should().Be("Married");

                    _DataContext.PersonalState = PersonalState.Single;
                    Thread.Sleep(50);

                    res = GetSafe(() => Get(js, "PersonalState"));
                    dres = GetSafe(() => ((JSObject)res)["displayName"]);
                    ((string)dres).Should().Be("Single");

                    var othervalue = GetSafe(() => UnWrapCollection(js, "States"));
                    JSValue[] coll = (JSValue[])othervalue;
                    JSValue di = coll[2];
                    var name = GetSafe(() => ((JSObject)di)["displayName"]);
                    ((string)name).Should().Be("Divorced");


                    this.DoSafe(() => js.Invoke("PersonalState", di));
                    Thread.Sleep(100);

                    _DataContext.PersonalState.Should().Be(PersonalState.Divorced);

                }
            }
        }

        private class SimplePerson : ViewModelBase
        {
            private PersonalState _PersonalState;
            public PersonalState PersonalState
            {
                get { return _PersonalState; }
                set { Set(ref _PersonalState, value, "PersonalState"); }
            }
        }

        [Fact]
        public void Test_AwesomeBinding_TwoWay_Enum_NotMapped()
        {
            using (Tester())
            {

                bool isValidSynchronizationContext = (_SynchronizationContext != null) && (_SynchronizationContext.GetType() != typeof(SynchronizationContext));
                isValidSynchronizationContext.Should().BeTrue();

                var datacontext = new SimplePerson();
                datacontext.PersonalState = PersonalState.Single;

                using (var mb = AwesomeBinding.Bind(_WebView, datacontext, JavascriptBindingMode.TwoWay).Result)
                {
                    var js = mb.JSRootObject;

                    JSValue res = GetSafe(() => Get(js, "PersonalState"));
                    JSValue dres = GetSafe(() => ((JSObject)res)["displayName"]);
                    ((string)dres).Should().Be("Single");

                    datacontext.PersonalState = PersonalState.Married;
                    Thread.Sleep(50);

                    res = GetSafe(() => Get(js, "PersonalState"));
                    dres = GetSafe(() => ((JSObject)res)["displayName"]);
                    ((string)dres).Should().Be("Married");
                }
            }
        }

        [Fact]
        public void Test_AwesomeBinding_TwoWay_Set_Object_From_Javascipt()
        {
            using (Tester())
            {

                bool isValidSynchronizationContext = (_SynchronizationContext != null) && (_SynchronizationContext.GetType() != typeof(SynchronizationContext));
                isValidSynchronizationContext.Should().BeTrue();

                var datacontext = new Couple();
                var p1 = new Person() { Name = "David" };
                datacontext.One = p1;
                var p2 = new Person() { Name = "Claudia" };
                datacontext.Two = p2;

                using (var mb = AwesomeBinding.Bind(_WebView, datacontext, JavascriptBindingMode.TwoWay).Result)
                {
                    var js = mb.JSRootObject;

                    JSValue res1 = GetSafe(() => Get(js, "One"));
                    res1.Should().NotBeNull();
                    var n1 = GetSafe(() => Get(res1, "Name"));
                    ((string)n1).Should().Be("David");

                    JSValue res2 = GetSafe(() => Get(js, "Two"));
                    res2.Should().NotBeNull();
                    var n2 = GetSafe(() => Get(res2, "Name"));
                    ((string)n2).Should().Be("Claudia");

                    DoSafe(() => js.Invoke("One", res2));

                    JSValue res3 = GetSafe(() => Get(js, "One"));
                    res3.Should().NotBeNull();
                    var n3 = GetSafe(() => Get(res3, "Name"));
                    ((string)n3).Should().Be("Claudia");

                    Thread.Sleep(100);

                    datacontext.One.Should().Be(p2);

                    JSValue res4 = GetSafe(() => Get(res3, "ChildrenNumber"));
                    res4.IsNull.Should().BeTrue();

                    JSValue five = new JSValue(5);
                    DoSafe(() => ((JSObject)res3).Invoke("ChildrenNumber", five));
                    Thread.Sleep(100);

                    datacontext.One.ChildrenNumber.Should().Be(5);

                   }
            }
        }


        [Fact]
        public void Test_AwesomeBinding_TwoWay_Set_Null_From_Javascipt()
        {
            using (Tester())
            {

                bool isValidSynchronizationContext = (_SynchronizationContext != null) && (_SynchronizationContext.GetType() != typeof(SynchronizationContext));
                isValidSynchronizationContext.Should().BeTrue();

                var datacontext = new Couple();
                var p1 = new Person() { Name = "David" };
                datacontext.One = p1;
                var p2 = new Person() { Name = "Claudia" };
                datacontext.Two = p2;

                using (var mb = AwesomeBinding.Bind(_WebView, datacontext, JavascriptBindingMode.TwoWay).Result)
                {
                    var js = mb.JSRootObject;

                    JSValue res1 = GetSafe(() => Get(js, "One"));
                    res1.Should().NotBeNull();
                    var n1 = GetSafe(() => Get(res1, "Name"));
                    ((string)n1).Should().Be("David");

                    JSValue res2 = GetSafe(() => Get(js, "Two"));
                    res2.Should().NotBeNull();
                    var n2 = GetSafe(() => Get(res2, "Name"));
                    ((string)n2).Should().Be("Claudia");

                    DoSafe(() => js.Invoke("One", JSValue.Null));

                    JSValue res3 = GetSafe(() => Get(js, "One"));
                    res3.IsNull.Should().BeTrue();

                    Thread.Sleep(100);

                    datacontext.One.Should().BeNull();
                }
            }
        }

        [Fact]
        public void Test_AwesomeBinding_TwoWay_Set_Object_From_Javascipt_Survive_MissUse()
        {
            using (Tester())
            {

                bool isValidSynchronizationContext = (_SynchronizationContext != null) && (_SynchronizationContext.GetType() != typeof(SynchronizationContext));
                isValidSynchronizationContext.Should().BeTrue();

                var datacontext = new Couple();
                var p1 = new Person() { Name = "David" };
                datacontext.One = p1;
                var p2 = new Person() { Name = "Claudia" };
                datacontext.Two = p2;

                using (var mb = AwesomeBinding.Bind(_WebView, datacontext, JavascriptBindingMode.TwoWay).Result)
                {
                    var js = mb.JSRootObject;

                    JSValue res1 = GetSafe(() => Get(js, "One"));
                    res1.Should().NotBeNull();
                    var n1 = GetSafe(() => Get(res1, "Name"));
                    ((string)n1).Should().Be("David");

                    JSValue res2 = GetSafe(() => Get(js, "Two"));
                    res2.Should().NotBeNull();
                    var n2 = GetSafe(() => Get(res2, "Name"));
                    ((string)n2).Should().Be("Claudia");

                    DoSafe(() => js.Invoke("One", new JSValue("Dede")));

                    JSValue res3 = GetSafe(() => Get(js, "One"));
                    ((string)res3).Should().Be("Dede");

                    Thread.Sleep(100);

                    datacontext.One.Should().Be(p1);
                }
            }
        }

        [Fact]
        public void Test_AwesomeBinding_TwoWay_Set_Object_From_Javascipt_Survive_MissUse_NoReset_OnAttribute()
        {
            using (Tester())
            {

                bool isValidSynchronizationContext = (_SynchronizationContext != null) && (_SynchronizationContext.GetType() != typeof(SynchronizationContext));
                isValidSynchronizationContext.Should().BeTrue();

                var datacontext = new Couple();
                var p1 = new Person() { Name = "David" };
                datacontext.One = p1;
                var p2 = new Person() { Name = "Claudia" };
                datacontext.Two = p2;

                using (var mb = AwesomeBinding.Bind(_WebView, datacontext, JavascriptBindingMode.TwoWay).Result)
                {
                    var js = mb.JSRootObject;

                    JSValue res1 = GetSafe(() => Get(js, "One"));
                    res1.Should().NotBeNull();
                    var n1 = GetSafe(() => Get(res1, "Name"));
                    ((string)n1).Should().Be("David");

                    JSValue res2 = GetSafe(() => Get(js, "Two"));
                    res2.Should().NotBeNull();
                    var n2 = GetSafe(() => Get(res2, "Name"));
                    ((string)n2).Should().Be("Claudia");

                    DoSafe(() => js.Invoke("One", new JSValue(new JSObject())));

                    JSValue res3 = GetSafe(() => Get(js, "One"));
                    res3.IsObject.Should().BeTrue();

                    Thread.Sleep(100);

                    datacontext.One.Should().Be(p1);
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
            private ICommand _ICommand;
            public ICommand Command { get { return _ICommand; } set { Set(ref _ICommand, value, "Command"); } }

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
                    var js = mb.JSRootObject;

                    JSValue res = GetSafe(() => js.Invoke("Name"));
                    res.Should().NotBeNull();
                    ((string)res).Should().Be("NameTest");

                    res = GetSafe(() => js.Invoke("Name", "NewName"));
                    res = GetSafe(() => js.Invoke("Name"));
                    res.Should().NotBeNull();
                    ((string)res).Should().Be("NewName");

                    Thread.Sleep(100);
                    datacontexttest.Name.Should().Be("NameTest");

                    bool resf = GetSafe(() => js.HasProperty("UselessName"));
                    resf.Should().BeFalse();

                    Action Safe = () => datacontexttest.InconsistentEventEmit();

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

                    var js = (mb as AwesomeBinding).JSBrideRootObject as JSGenericObject;

                    var mycommand = js.Attributes["Command"] as JSCommand;
                    mycommand.Should().NotBeNull();
                    mycommand.ToString().Should().Be("{}");
                    mycommand.Type.Should().Be(JSCSGlueType.Command);
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
                    var js = mb.JSRootObject;

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
                    var js = mb.JSRootObject;

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
                    var js = mb.JSRootObject;

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
                    var js = mb.JSRootObject;

                    JSObject mycommand = (JSObject)GetSafe(() => js.Invoke("Command"));
                    JSValue res = GetSafe(() => mycommand.Invoke("CanExecuteValue"));

                    ((bool)res).Should().BeTrue();
                }
            }
        }

        [Fact]
        public void Test_AwesomeBinding_Basic_TwoWay_Command_Uptate_From_Null()
        {
            using (Tester())
            {
                var command = Substitute.For<ICommand>();
                command.CanExecute(Arg.Any<object>()).Returns(true);
                var test = new ViewModelTest();

                using (var mb = AwesomeBinding.Bind(_WebView, test, JavascriptBindingMode.TwoWay).Result)
                {
                    var js = mb.JSRootObject;

                    JSObject mycommand = (JSObject)GetSafe(() => js.Invoke("Command"));
                    
                    DoSafe(() => test.Command = command);
                    Thread.Sleep(200);

                    mycommand = (JSObject)GetSafe(() => js.Invoke("Command"));
                    JSValue res = GetSafe(() => mycommand.Invoke("Execute", js));
                    Thread.Sleep(100);
                    command.Received().Execute(test);
                }
            }
        }

        #region SimpleCommand

        private class ViewModelSimpleCommandTest : ViewModelBase
        {
            private ISimpleCommand _ICommand;
            public ISimpleCommand SimpleCommand { get { return _ICommand; } set { Set(ref _ICommand, value, "SimpleCommand"); } }
        }

        [Fact]
        public void Test_AwesomeBinding_Basic_TwoWay_SimpleCommand_Without_Parameter()
        {
            using (Tester())
            {
                var command = Substitute.For<ISimpleCommand>();
                var test = new ViewModelSimpleCommandTest() { SimpleCommand = command };

                using (var mb = AwesomeBinding.Bind(_WebView, test, JavascriptBindingMode.TwoWay).Result)
                {
                    var js = mb.JSRootObject;

                    JSObject mycommand = (JSObject)GetSafe(() => js.Invoke("SimpleCommand"));
                    JSValue res = GetSafe(() => mycommand.Invoke("Execute"));
                    Thread.Sleep(100);
                    command.Received().Execute(null);
                }
            }
        }

        [Fact]
        public void Test_AwesomeBinding_Basic_TwoWay_SimpleCommand_With_Parameter()
        {
            using (Tester())
            {
                var command = Substitute.For<ISimpleCommand>();
                var test = new ViewModelSimpleCommandTest() { SimpleCommand = command };

                using (var mb = AwesomeBinding.Bind(_WebView, test, JavascriptBindingMode.TwoWay).Result)
                {
                    var js = mb.JSRootObject;

                    JSObject mycommand = (JSObject)GetSafe(() => js.Invoke("SimpleCommand"));
                    JSValue res = GetSafe(() => mycommand.Invoke("Execute", js));
                    Thread.Sleep(100);
                    command.Received().Execute(test);
                }
            }
        }

        [Fact]
        public void Test_AwesomeBinding_Basic_TwoWay_SimpleCommand_Name()
        {
            using (Tester())
            {
                var command = Substitute.For<ISimpleCommand>();
                var test = new ViewModelSimpleCommandTest() { SimpleCommand = command };

                using (var mb = AwesomeBinding.Bind(_WebView, test, JavascriptBindingMode.TwoWay).Result)
                {
                    var js = mb.JSRootObject;

                    mb.ToString().Should().Be(@"{""SimpleCommand"":{}}");
                }
            }
        }

        #endregion

        private void CheckIntValue(JSObject js, string pn, int value)
        {
            JSValue res = GetSafe(() => js.Invoke(pn));
            res.Should().NotBeNull();
            res.IsNumber.Should().BeTrue();
            ((int)res).Should().Be(0);
        }

        private void SetValue(JSObject js, string pn, JSValue value)
        {
            DoSafe(() => js.Invoke(pn, value));
        }

        [Fact]
        public void Test_AwesomeBinding_Basic_TwoWay_CLR_Type_FromCtojavascript()
        {
            using (Tester())
            {
                var datacontext = new ViewModelCLRTypes();

                using (var mb = AwesomeBinding.Bind(_WebView, datacontext, JavascriptBindingMode.TwoWay).Result)
                {
                    var js = mb.JSRootObject;
                    js.Should().NotBeNull();

                    CheckIntValue(js, "int64", 0);  
                    CheckIntValue(js, "int32", 0);
                    CheckIntValue(js, "int16", 0);

                    CheckIntValue(js, "uint16", 0);
                    CheckIntValue(js, "uint32", 0);
                    CheckIntValue(js, "uint64", 0);

                    CheckIntValue(js, "Char", 0);
                    CheckIntValue(js, "Double", 0);
                    CheckIntValue(js, "Decimal", 0);
                    CheckIntValue(js, "Float", 0);
                }
            }
        }

        [Fact]
        public void Test_AwesomeBinding_Basic_TwoWay_CLR_Type_FromjavascripttoCto()
        {
            using (Tester())
            {
                var datacontext = new ViewModelCLRTypes();

                using (var mb = AwesomeBinding.Bind(_WebView, datacontext, JavascriptBindingMode.TwoWay).Result)
                {
                    var js = mb.JSRootObject;
                    js.Should().NotBeNull();

                    SetValue(js, "int64", 32);
                    Thread.Sleep(200);
                    datacontext.int64.Should().Be(32);

                    SetValue(js, "uint64", 456);
                    Thread.Sleep(200);
                    datacontext.uint64.Should().Be(456);

                    SetValue(js, "int32", 5);
                    Thread.Sleep(200);
                    datacontext.int32.Should().Be(5);

                    SetValue(js, "uint32", 67);
                    Thread.Sleep(200);
                    datacontext.uint32.Should().Be(67);

                    SetValue(js, "int16", -23);
                    Thread.Sleep(200);
                    datacontext.int16.Should().Be(-23);

                    SetValue(js, "uint16", 9);
                    Thread.Sleep(200);
                    datacontext.uint16.Should().Be(9);

                    SetValue(js, "Float", 888.78);
                    Thread.Sleep(200);
                    datacontext.Float.Should().Be(888.78f);

                    SetValue(js, "Char", 128);
                    Thread.Sleep(200);
                    datacontext.Char.Should().Be((char)128);

                    SetValue(js, "Double", 66.76);
                    Thread.Sleep(200);
                    datacontext.Double.Should().Be(66.76);

                    SetValue(js, "Decimal", 0.5);
                    Thread.Sleep(200);
                    datacontext.Decimal.Should().Be(0.5m);

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
                    var js = mb.JSRootObject;

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
                    var js = mb.JSRootObject;

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
                _ICommand.CanExecute(Arg.Any<object>()).Returns(x => { if (x[0] == null) throw new Exception(); return false; });

                using (var mb = AwesomeBinding.Bind(_WebView, _DataContext, JavascriptBindingMode.TwoWay).Result)
                {
                    _ICommand.Received().CanExecute(Arg.Any<object>());
                    var js = mb.JSRootObject;

                    JSObject mycommand = (JSObject)GetSafe(() => js.Invoke("TestCommand"));
                    JSValue res = GetSafe(() => mycommand.Invoke("CanExecuteValue"));
                    ((bool)res).Should().BeFalse();

                    _ICommand.Received().CanExecute(_DataContext);
                }
            }
        }

        [Fact]
        public void Test_AwesomeBinding_Basic_TwoWay_Command_Received_javascript_variable()
        {
            using (Tester())
            {

                _ICommand.CanExecute(Arg.Any<object>()).Returns(true);

                using (var mb = AwesomeBinding.Bind(_WebView, _DataContext, JavascriptBindingMode.TwoWay).Result)
                {
                    var js = mb.JSRootObject;

                    JSObject mycommand = (JSObject)GetSafe(() => js.Invoke("TestCommand"));
                    JSValue res = GetSafe(() => mycommand.Invoke("Execute", new JSValue("titi")));

                    Thread.Sleep(150);
                    _ICommand.Received().Execute("titi");
                }
            }
        }

        [Fact]
        public void Test_AwesomeBinding_Basic_TwoWay_Command_Complete()
        {
            using (Tester())
            {
                _ICommand = new RelayCommand(() =>
                {
                    _DataContext.MainSkill = new Skill();
                    _DataContext.Skills.Add(_DataContext.MainSkill);
                });
                _DataContext.TestCommand = _ICommand;

                using (var mb = AwesomeBinding.Bind(_WebView, _DataContext, JavascriptBindingMode.TwoWay).Result)
                {
                    var js = mb.JSRootObject;

                    _DataContext.Skills.Should().HaveCount(2);

                    DoSafe(() =>
                    _ICommand.Execute(null));

                    Thread.Sleep(100);

                    JSValue res = GetSafe(() => UnWrapCollection(js, "Skills"));
                    res.Should().NotBeNull();
                    ((JSValue[])res).Should().HaveCount(3);
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
                    var js = mb.JSRootObject;

                    JSObject mycommand = (JSObject)GetSafe(() => js.Invoke("Command"));
                    JSValue res = GetSafe(() => mycommand.Invoke("Execute", null));
                    Thread.Sleep(150);
                    command.Received().Execute(null);
                }
            }
        }

        [Fact]
        public void Test_AwesomeBinding_Basic_TwoWay_ResultCommand_Should_have_ToString()
        {
            var function = NSubstitute.Substitute.For<Func<int, int>>();
            var dc = new FakeFactory<int, int>(function);
            using (Tester(@"javascript\index_promise.html"))
            {
                using (var mb = AwesomeBinding.Bind(_WebView, dc, JavascriptBindingMode.TwoWay).Result)
                {
                    mb.ToString().Should().NotBeNull();
                }
            }
        }

        [Fact]
        public void Test_AwesomeBinding_Basic_TwoWay_ResultCommand_Received_javascript_variable_and_not_crash_withoutcallback()
        {
            var function = NSubstitute.Substitute.For<Func<int, int>>();
            var dc = new FakeFactory<int, int>(function);
            using (Tester(@"javascript\index_promise.html"))
            {
                using (var mb = AwesomeBinding.Bind(_WebView, dc, JavascriptBindingMode.TwoWay).Result)
                {
                    var js = mb.JSRootObject;

                    JSObject mycommand = (JSObject)GetSafe(() => js.Invoke("CreateObject"));
                    JSValue res = GetSafe(() => mycommand.Invoke("Execute", new JSValue(25)));

                    Thread.Sleep(700);
                    function.Received(1).Invoke(25);
                }
            }
        }

          [Fact]
        public void Test_AwesomeBinding_Basic_TwoWay_ResultCommand_Received_javascript_variable()
        {
            var function = NSubstitute.Substitute.For<Func<int,int>>();
            function.Invoke(Arg.Any<int>()).Returns(255);
                  
            var dc = new FakeFactory<int,int>(function);
            using (Tester(@"javascript\index_promise.html"))
            {
                using (var mb = AwesomeBinding.Bind(_WebView, dc, JavascriptBindingMode.TwoWay).Result)
                {
                    var js = mb.JSRootObject;

                    JSObject mycommand = (JSObject)GetSafe(() => js.Invoke("CreateObject"));
                    var cb = (JSObject)GetSafe(()=>_WebView.ExecuteJavascriptWithResult("(function(){return { fullfill: function (res) {window.res=res; }, reject: function(err){window.err=err;}}; })();"));

                    JSValue resdummy = GetSafe(() => mycommand.Invoke("Execute", new JSValue(25),cb));

                    Thread.Sleep(200);
                    function.Received(1).Invoke(25);

                    var res = (JSValue)GetSafe(() => _WebView.ExecuteJavascriptWithResult("window.res"));
                    int intres = (int)res;
                    intres.Should().Be(255);

                    var error = (JSValue)GetSafe(() => _WebView.ExecuteJavascriptWithResult("window.err"));
                    error.Should().Be(JSValue.Undefined);
                }
            }
        }


          [Fact]
          public void Test_AwesomeBinding_Basic_TwoWay_ResultCommand_Received_javascript_variable_should_fault_Onexception()
          {
              string errormessage = "original error message";
              var function = NSubstitute.Substitute.For<Func<int, int>>();
              function.When(f => f.Invoke(Arg.Any<int>())).Do(_ => { throw new Exception(errormessage); });
              //function.Invoke(Arg.Any<int>()).Returns(255);

              var dc = new FakeFactory<int, int>(function);
              using (Tester(@"javascript\index_promise.html"))
              {
                  using (var mb = AwesomeBinding.Bind(_WebView, dc, JavascriptBindingMode.TwoWay).Result)
                  {
                      var js = mb.JSRootObject;

                      JSObject mycommand = (JSObject)GetSafe(() => js.Invoke("CreateObject"));
                      var cb = (JSObject)GetSafe(() => _WebView.ExecuteJavascriptWithResult("(function(){return { fullfill: function (res) {window.res=res; }, reject: function(err){window.err=err;}}; })();"));

                      JSValue resdummy = GetSafe(() => mycommand.Invoke("Execute", new JSValue(25), cb));

                      Thread.Sleep(200);
                      function.Received(1).Invoke(25);

                      var res = (JSValue)GetSafe(() => _WebView.ExecuteJavascriptWithResult("window.res"));
                      res.Should().Be(JSValue.Undefined);

                      var error = (JSValue)GetSafe(() => _WebView.ExecuteJavascriptWithResult("window.err"));
                      error.Should().NotBeNull();
                      ((string)error).Should().Be(errormessage);

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
                    var js = mb.JSRootObject;

                    JSValue res = GetSafe(() => UnWrapCollection(js, "Skills"));
                    res.Should().NotBeNull();
                    var col = ((JSValue[])res);
                    col.Length.Should().Be(2);

                    Check(col, _DataContext.Skills);

                    _DataContext.Skills.Add(new Skill() { Name = "C++", Type = "Info" });

                    Thread.Sleep(100);
                    res = GetSafe(() => UnWrapCollection(js, "Skills"));
                    res.Should().NotBeNull();
                    Check((JSValue[])res, _DataContext.Skills);


                    _DataContext.Skills.Insert(0, new Skill() { Name = "C#", Type = "Info" });
                    Thread.Sleep(100);
                    res = GetSafe(() => UnWrapCollection(js, "Skills"));
                    res.Should().NotBeNull();
                    Check((JSValue[])res, _DataContext.Skills);

                    _DataContext.Skills.RemoveAt(1);
                    Thread.Sleep(100);
                    res = GetSafe(() => UnWrapCollection(js, "Skills"));
                    res.Should().NotBeNull();
                    Check((JSValue[])res, _DataContext.Skills);

                    _DataContext.Skills[0] = new Skill() { Name = "HTML", Type = "Info" };
                    Thread.Sleep(100);
                    res = GetSafe(() => UnWrapCollection(js, "Skills"));
                    res.Should().NotBeNull();
                    Check((JSValue[])res, _DataContext.Skills);

                    _DataContext.Skills[0] = new Skill() { Name = "HTML5", Type = "Info" };
                    _DataContext.Skills.Insert(0, new Skill() { Name = "HTML5", Type = "Info" });
                    Thread.Sleep(100);
                    res = GetSafe(() => UnWrapCollection(js, "Skills"));
                    res.Should().NotBeNull();
                    Check((JSValue[])res, _DataContext.Skills);


                    _DataContext.Skills.Clear();
                    Thread.Sleep(100);
                    res = GetSafe(() => UnWrapCollection(js, "Skills"));
                    res.Should().NotBeNull();
                    Check((JSValue[])res, _DataContext.Skills);

                }
            }
        }

        [Fact]
        public void Test_AwesomeBinding_Stress_TwoWay_Collection()
        {
            using (Tester("javascript/simple.html"))
            {

                bool isValidSynchronizationContext = (_SynchronizationContext != null) && (_SynchronizationContext.GetType() != typeof(SynchronizationContext));
                isValidSynchronizationContext.Should().BeTrue();
                DoSafe(() =>
                _WebView.SynchronousMessageTimeout = 0);

                using (var mb = AwesomeBinding.Bind(_WebView, _DataContext, JavascriptBindingMode.TwoWay).Result)
                {
                    var js = mb.JSRootObject;

                    JSValue res = GetSafe(() => UnWrapCollection(js, "Skills"));
                    res.Should().NotBeNull();
                    var col = ((JSValue[])res);
                    col.Length.Should().Be(2);

                    Check(col, _DataContext.Skills);

                    _DataContext.Skills.Add(new Skill() { Name = "C++", Type = "Info" });

                    Thread.Sleep(150);
                    res = GetSafe(() => UnWrapCollection(js, "Skills"));
                    res.Should().NotBeNull();
                    Check((JSValue[])res, _DataContext.Skills);

                    _DataContext.Skills[0] = new Skill() { Name = "HTML5", Type = "Info" };
                    int iis = 500;
                    for (int i = 0; i < iis; i++)
                    {
                        _DataContext.Skills.Insert(0, new Skill() { Name = "HTML5", Type = "Info" });
                    }

                    bool notok = true;
                    int tcount = _DataContext.Skills.Count;

                    var stopWatch = new Stopwatch();
                    stopWatch.Start();

                    Thread.Sleep(10);

                    while (notok)
                    {
                        res = GetSafe(() => UnWrapCollection(js, "Skills"));
                        res.Should().NotBeNull();
                        notok = ((JSValue[])res).Length != tcount;
                    }
                    stopWatch.Stop();
                    var ts = stopWatch.ElapsedMilliseconds;

                    Console.WriteLine("Perf: {0} sec for {1} items", ((double)(ts)) / 1000, iis);
                    Check((JSValue[])res, _DataContext.Skills);

                    //TimeSpan.FromMilliseconds(ts).Should().BeLessThan(TimeSpan.FromSeconds(4.5));
                    TimeSpan.FromMilliseconds(ts).Should().BeLessThan(TimeSpan.FromSeconds(4.7));
                }
            }
        }

        private class TwoList
        {
            public TwoList()
            {
                L1 = new List<Skill>();
                L2 = new List<Skill>();
            }
            public List<Skill> L1 { get; private set; }
            public List<Skill> L2 { get; private set; }
        }

        [Fact]
        public void Test_AwesomeBinding_Stress_TwoWay_Collection_CreateBinding()
        {
            Test_AwesomeBinding_Stress_Collection_CreateBinding(JavascriptBindingMode.TwoWay, 1.5, "javascript/simple.html");
        }

        [Fact]
        public void Test_AwesomeBinding_Stress_OneWay_Collection_CreateBinding()
        {
            Test_AwesomeBinding_Stress_Collection_CreateBinding(JavascriptBindingMode.OneWay, 1.5,"javascript/simple.html");
        }

        public void Test_AwesomeBinding_Stress_Collection_CreateBinding(JavascriptBindingMode imode, double excpected,string ipath=null)
        {
            using (Tester(ipath))
            {
                int r = 100;
                var datacontext = new TwoList();
                datacontext.L1.AddRange(Enumerable.Range(0, r).Select(i => new Skill()));

                DoSafe(() =>
                _WebView.SynchronousMessageTimeout = 0);
                long ts = 0;

                var stopWatch = new Stopwatch();
                stopWatch.Start();

                using (var mb = AwesomeBinding.Bind(_WebView, datacontext, imode).Result)
                {
                    stopWatch.Stop();
                    ts = stopWatch.ElapsedMilliseconds;

                    Console.WriteLine("Perf: {0} sec for {1} items", ((double)(ts)) / 1000, r);

                    var js = mb.JSRootObject;

                    JSValue res = GetSafe(() => UnWrapCollection(js, "L1"));
                    res.Should().NotBeNull();
                    var col = ((JSValue[])res);
                    col.Length.Should().Be(r);


                }

                TimeSpan.FromMilliseconds(ts).Should().BeLessThan(TimeSpan.FromSeconds(excpected));

            }
        }

        [Fact]
        public void Test_AwesomeBinding_Stress_Collection_Update_From_Javascript()
        {
            using (Tester("javascript/simple.html"))
            {
                int r = 100;
                var datacontext = new TwoList();
                datacontext.L1.AddRange(Enumerable.Range(0, r).Select(i => new Skill()));

                DoSafe(() =>
                _WebView.SynchronousMessageTimeout = 0);
                long ts = 0;

                using (var mb = AwesomeBinding.Bind(_WebView, datacontext, JavascriptBindingMode.TwoWay).Result)
                {
                    var js = mb.JSRootObject;

                    JSValue res1 = GetSafe(() => UnWrapCollection(js, "L1"));
                    res1.Should().NotBeNull();
                    var col1 = ((JSValue[])res1);
                    col1.Length.Should().Be(r);

                    JSValue res2 = GetSafe(() => UnWrapCollection(js, "L2"));
                    res2.Should().NotBeNull();
                    var col2 = ((JSValue[])res2);
                    col2.Length.Should().Be(0);

                    JSObject l2c = (JSObject)GetSafe(() => js.Invoke("L2"));
                    l2c.Should().NotBeNull();

                    string javascript = "window.app = function(value,coll){var args = []; args.push(0);args.push(0);for (var i = 0; i < value.length; i++) { args.push(value[i]);} coll.splice.apply(coll, args);};";
                    DoSafe(() => _WebView.ExecuteJavascript(javascript));
                    JSObject win = null;
                    win = GetSafe(() => (JSObject)_WebView.ExecuteJavascriptWithResult("window"));

                    bool notok = true;
                    var stopWatch = new Stopwatch();
                    stopWatch.Start();

                    DoSafe(() => win.Invoke("app", res1, l2c));
                    Thread.Sleep(100);
                    while (notok)
                    {
                        //Thread.Sleep(30);
                        notok = datacontext.L2.Count != r;
                    }
                    stopWatch.Stop();
                    ts = stopWatch.ElapsedMilliseconds;

                    Console.WriteLine("Perf: {0} sec for {1} items", ((double)(ts)) / 1000, r);
                }

                TimeSpan.FromMilliseconds(ts).Should().BeLessThan(TimeSpan.FromSeconds(0.15));

            }
        }

        [Fact]
        public void Test_AwesomeBinding_Stress_OneTime_Collection_CreateBinding()
        {
            Test_AwesomeBinding_Stress_Collection_CreateBinding(JavascriptBindingMode.OneTime, 1.5,"javascript/simple.html");
        }


        [Fact]
        public void Test_AwesomeBinding_Stress_TwoWay_Int()
        {
            using (Tester())
            {

                bool isValidSynchronizationContext = (_SynchronizationContext != null) && (_SynchronizationContext.GetType() != typeof(SynchronizationContext));
                isValidSynchronizationContext.Should().BeTrue();

                DoSafe(() =>
                _WebView.SynchronousMessageTimeout = 0);

                using (var mb = AwesomeBinding.Bind(_WebView, _DataContext, JavascriptBindingMode.TwoWay).Result)
                {
                    var js = mb.JSRootObject;


                    int iis = 500;
                    for (int i = 0; i < iis; i++)
                    {
                        _DataContext.Age += 1;
                    }

                    bool notok = true;
                    var tg = _DataContext.Age;
                    Thread.Sleep(700);

                    var stopWatch = new Stopwatch();
                    stopWatch.Start();

                    while (notok)
                    {
                        Thread.Sleep(100);
                        JSValue res = GetSafe(() => Get(js, "Age"));
                        res.Should().NotBeNull();
                        res.IsNumber.Should().BeTrue();
                        var doublev = (int)res;
                        notok = doublev != tg;
                    }
                    stopWatch.Stop();
                    var ts = stopWatch.ElapsedMilliseconds;

                    Console.WriteLine("Perf: {0} sec for {1} iterations", ((double)(ts)) / 1000, iis);

                    TimeSpan.FromMilliseconds(ts).Should().BeLessOrEqualTo(TimeSpan.FromSeconds(3.1));

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
                    var root = (mb as AwesomeBinding).JSBrideRootObject as JSGenericObject;
                    var js = mb.JSRootObject;

                    JSValue res = GetSafe(() => UnWrapCollection(js, "Skills"));
                    res.Should().NotBeNull();
                    var col = ((JSValue[])res);
                    col.Length.Should().Be(2);

                    Check(col, _DataContext.Skills);

                    JSObject coll = GetSafe(() => ((JSObject)js).Invoke("Skills"));
                    DoSafe(() => coll.Invoke("push", (root.Attributes["Skills"] as JSArray).Items[0].GetJSSessionValue()));

                    Thread.Sleep(5000);
                    _DataContext.Skills.Should().HaveCount(3);
                    _DataContext.Skills[2].Should().Be(_DataContext.Skills[0]);
                    res = GetSafe(() => UnWrapCollection(js, "Skills"));
                    res.Should().NotBeNull();
                    Check((JSValue[])res, _DataContext.Skills);

                    DoSafe(() => coll.Invoke("pop"));

                    Thread.Sleep(100);
                    _DataContext.Skills.Should().HaveCount(2);
                    res = GetSafe(() => UnWrapCollection(js, "Skills"));
                    res.Should().NotBeNull();
                    Check((JSValue[])res, _DataContext.Skills);

                    DoSafe(() => coll.Invoke("shift"));

                    Thread.Sleep(100);
                    _DataContext.Skills.Should().HaveCount(1);
                    res = GetSafe(() => UnWrapCollection(js, "Skills"));
                    res.Should().NotBeNull();
                    Check((JSValue[])res, _DataContext.Skills);


                    DoSafe(() => coll.Invoke("unshift",
                        (root.Attributes["Skills"] as JSArray).Items[0].GetJSSessionValue()));

                    Thread.Sleep(150);
                    _DataContext.Skills.Should().HaveCount(2);
                    res = GetSafe(() => UnWrapCollection(js, "Skills"));
                    res.Should().NotBeNull();
                    Check((JSValue[])res, _DataContext.Skills);

                    _DataContext.Skills.Add(new Skill() { Type = "Langage", Name = "French" });
                    Thread.Sleep(150);
                     _DataContext.Skills.Should().HaveCount(3);
                     res = GetSafe(() => UnWrapCollection(js, "Skills"));
                    res.Should().NotBeNull();
                    Check((JSValue[])res, _DataContext.Skills);
      

                    DoSafe(() => coll.Invoke("reverse"));

                    Thread.Sleep(150);
                    _DataContext.Skills.Should().HaveCount(3);
                    res = GetSafe(() => UnWrapCollection(js, "Skills"));
                    res.Should().NotBeNull();
                    Check((JSValue[])res, _DataContext.Skills);
                }
            }
        }

        [Fact]
        public void Test_AwesomeBinding_Basic_TwoWay_Collection_JSUpdate_Should_Survive_ViewChanges()
        {
            using (Tester())
            {

                bool isValidSynchronizationContext = (_SynchronizationContext != null) && (_SynchronizationContext.GetType() != typeof(SynchronizationContext));
                isValidSynchronizationContext.Should().BeTrue();

                using (var mb = AwesomeBinding.Bind(_WebView, _DataContext, JavascriptBindingMode.TwoWay).Result)
                {
                    var root = (mb as AwesomeBinding).JSBrideRootObject as JSGenericObject;
                    var js = mb.JSRootObject;

                    JSValue res = GetSafe(() => UnWrapCollection(js, "Skills"));
                    res.Should().NotBeNull();
                    var col = ((JSValue[])res);
                    col.Length.Should().Be(2);

                    Check(col, _DataContext.Skills);

                    JSObject coll = GetSafe(() => ((JSObject)js).Invoke("Skills"));
                    DoSafe(() => coll.Invoke("push", new JSValue("Whatever")));

                    Thread.Sleep(150);
                    _DataContext.Skills.Should().HaveCount(2);
                }
            }
        }

        private class VMWithList<T> : ViewModelBase
        {
            public VMWithList()
            {
                List = new ObservableCollection<T>();
            }
            public ObservableCollection<T> List { get; private set; }

        }

        private class VMWithListNonGeneric : ViewModelBase
        {
            public VMWithListNonGeneric()
            {
                List = new ArrayList();
            }
            public ArrayList List { get; private set; }

        }

        private class VMwithdecimal : ViewModelBase
        {
            public VMwithdecimal()
            {
            }

            private decimal _DecimalValue;
            public decimal decimalValue
            {
                get { return _DecimalValue; }
                set { Set(ref _DecimalValue, value, "decimalValue"); }
            }

        }


        private void Checkstring(JSValue[] coll, IList<string> iskill)
        {
            coll.Length.Should().Be(iskill.Count);
            coll.ForEach((c, i) =>
            {
                (GetSafe(() => (string)c)).Should().Be(iskill[i]);

            });

        }

        private void Checkstring(JSValue[] coll, IList<int> iskill)
        {
            coll.Length.Should().Be(iskill.Count);
            coll.ForEach((c, i) =>
            {
                (GetSafe(() => (int)c)).Should().Be(iskill[i]);

            });

        }

        private void Checkdecimal(JSValue[] coll, IList<decimal> iskill)
        {
            coll.Length.Should().Be(iskill.Count);
            coll.ForEach((c, i) =>
            {
                ((decimal)(double)c).Should().Be(iskill[i]);

            });

        }

        [Fact]
        public void Test_AwesomeBinding_Basic_TwoWay_Decimal_ShouldOK()
        {
            using (Tester())
            {

                bool isValidSynchronizationContext = (_SynchronizationContext != null) && (_SynchronizationContext.GetType() != typeof(SynchronizationContext));
                isValidSynchronizationContext.Should().BeTrue();

                var datacontext = new VMwithdecimal();

                using (var mb = AwesomeBinding.Bind(_WebView, datacontext, JavascriptBindingMode.TwoWay).Result)
                {
                    var js = mb.JSRootObject;

                    JSValue res = GetSafe(() => Get(js, "decimalValue"));
                    res.Should().NotBeNull();
                    res.IsNumber.Should().BeTrue();
                    var doublev = (double)res;
                    doublev.Should().Be(0);

                    this.DoSafe(() => js.Invoke("decimalValue", 0.5));
                    Thread.Sleep(150);

                    datacontext.decimalValue.Should().Be(0.5m);


                    res = GetSafe(() => Get(js, "decimalValue"));
                    res.Should().NotBeNull();
                    res.IsNumber.Should().BeTrue();
                    doublev = (double)res;
                    double half = 0.5;
                    doublev.Should().Be(half);
                }
            }
        }



        private class VMwithlong : ViewModelBase
        {
            public VMwithlong()
            {
            }

            private long _LongValue;
            public long longValue
            {
                get { return _LongValue; }
                set { Set(ref _LongValue, value, "decimalValue"); }
            }

        }

        [Fact]
        public void Test_AwesomeBinding_Basic_TwoWay_Long_ShouldOK()
        {
            using (Tester())
            {

                bool isValidSynchronizationContext = (_SynchronizationContext != null) && (_SynchronizationContext.GetType() != typeof(SynchronizationContext));
                isValidSynchronizationContext.Should().BeTrue();

                var datacontext = new VMwithlong() { longValue = 45 };

                using (var mb = AwesomeBinding.Bind(_WebView, datacontext, JavascriptBindingMode.TwoWay).Result)
                {
                    var js = mb.JSRootObject;

                    JSValue res = GetSafe(() => Get(js, "longValue"));
                    res.Should().NotBeNull();
                    res.IsNumber.Should().BeTrue();
                    var doublev = (long)res;
                    doublev.Should().Be(45);

                    this.DoSafe(() => js.Invoke("longValue", 24524));
                    Thread.Sleep(100);

                    datacontext.longValue.Should().Be(24524);

                    res = GetSafe(() => Get(js, "longValue"));
                    res.Should().NotBeNull();
                    res.IsNumber.Should().BeTrue();
                    doublev = (long)res;
                    long half = 24524;
                    doublev.Should().Be(half);
                }
            }
        }

        [Fact]
        public void Test_AwesomeBinding_Basic_TwoWay_Collection_string()
        {
            using (Tester())
            {

                bool isValidSynchronizationContext = (_SynchronizationContext != null) && (_SynchronizationContext.GetType() != typeof(SynchronizationContext));
                isValidSynchronizationContext.Should().BeTrue();

                var datacontext = new VMWithList<string>();

                using (var mb = AwesomeBinding.Bind(_WebView, datacontext, JavascriptBindingMode.TwoWay).Result)
                {
                    var js = mb.JSRootObject;

                    JSValue res = GetSafe(() => UnWrapCollection(js, "List"));
                    res.Should().NotBeNull();
                    var col = ((JSValue[])res);
                    col.Length.Should().Be(0);

                    Checkstring(col, datacontext.List);

                    datacontext.List.Add("titi");

                    Thread.Sleep(100);
                    res = GetSafe(() => UnWrapCollection(js, "List"));
                    col = ((JSValue[])res);

                    Checkstring(col, datacontext.List);

                    datacontext.List.Add("kiki");
                    datacontext.List.Add("toto");

                    Thread.Sleep(100);
                    res = GetSafe(() => UnWrapCollection(js, "List"));
                    col = ((JSValue[])res);

                    Checkstring(col, datacontext.List);

                    Thread.Sleep(100);
                    res = GetSafe(() => UnWrapCollection(js, "List"));
                    col = ((JSValue[])res);

                    Checkstring(col, datacontext.List);

                    var comp = new List<string>(datacontext.List);
                    comp.Add("newvalue");

                    res = GetSafe(() => js.Invoke("List"));
                    DoSafe(() =>
                    ((JSObject)res).Invoke("push", new JSValue("newvalue")));

                    Thread.Sleep(350);

                    res = GetSafe(() => UnWrapCollection(js, "List"));
                    col = ((JSValue[])res);

                    datacontext.List.Should().Equal(comp);
                    Checkstring(col, datacontext.List);

                    datacontext.List.Clear();
                    Thread.Sleep(100);
                    res = GetSafe(() => UnWrapCollection(js, "List"));
                    col = ((JSValue[])res);

                    Checkstring(col, datacontext.List);

                }
            }
        }

        [Fact]
        public void Test_AwesomeBinding_Basic_TwoWay_Collection_should_be_observable_attribute()
        {
            using (Tester())
            {

                bool isValidSynchronizationContext = (_SynchronizationContext != null) && (_SynchronizationContext.GetType() != typeof(SynchronizationContext));
                isValidSynchronizationContext.Should().BeTrue();

                var datacontext = new ChangingCollectionViewModel();

                using (var mb = AwesomeBinding.Bind(_WebView, datacontext, JavascriptBindingMode.TwoWay).Result)
                {
                    var js = mb.JSRootObject;

                    JSValue res = GetSafe(() => UnWrapCollection(js, "Items"));
                    res.Should().NotBeNull();
                    var col = ((JSValue[])res);
                    col.Length.Should().NotBe(0);

                    Checkstring(col, datacontext.Items);

                    DoSafe(()=> datacontext.Replace.Execute(null));

                    datacontext.Items.Should().BeEmpty();

                    Thread.Sleep(300);
                    res = GetSafe(() => UnWrapCollection(js, "Items"));
                    col = ((JSValue[])res);
                    col.Length.Should().Be(0);

                    Checkstring(col, datacontext.Items);
                }
            }
        }


        [Fact]
        public void Test_AwesomeBinding_Basic_TwoWay_Collection_NoneGenericList()
        {
            using (Tester())
            {

                bool isValidSynchronizationContext = (_SynchronizationContext != null) && (_SynchronizationContext.GetType() != typeof(SynchronizationContext));
                isValidSynchronizationContext.Should().BeTrue();

                var datacontext = new VMWithListNonGeneric();
                datacontext.List.Add(888);

                using (var mb = AwesomeBinding.Bind(_WebView, datacontext, JavascriptBindingMode.TwoWay).Result)
                {
                    var js = mb.JSRootObject;

                    JSValue res = GetSafe(() => UnWrapCollection(js, "List"));
                    res.Should().NotBeNull();
                    var col = ((JSValue[])res);
                    col.Length.Should().Be(1);

                    res = GetSafe(() => js.Invoke("List"));
                    DoSafe(() =>
                    ((JSObject)res).Invoke("push", new JSValue("newvalue")));

                    res = GetSafe(() => UnWrapCollection(js, "List"));
                    res.Should().NotBeNull();
                    col = ((JSValue[])res);
                    col.Length.Should().Be(2);

                    Thread.Sleep(350);

                    datacontext.List.Should().HaveCount(2);
                    datacontext.List[1].Should().Be("newvalue");
                }
            }
        }

        [Fact]
        public void Test_AwesomeBinding_Basic_TwoWay_Collection_decimal()
        {
            using (Tester())
            {

                bool isValidSynchronizationContext = (_SynchronizationContext != null) && (_SynchronizationContext.GetType() != typeof(SynchronizationContext));
                isValidSynchronizationContext.Should().BeTrue();

                var datacontext = new VMWithList<decimal>();

                using (var mb = AwesomeBinding.Bind(_WebView, datacontext, JavascriptBindingMode.TwoWay).Result)
                {
                    var js = mb.JSRootObject;

                    JSValue res = GetSafe(() => UnWrapCollection(js, "List"));
                    res.Should().NotBeNull();
                    var col = ((JSValue[])res);
                    col.Length.Should().Be(0);

                    Checkdecimal(col, datacontext.List);

                    datacontext.List.Add(3);

                    Thread.Sleep(150);
                    res = GetSafe(() => UnWrapCollection(js, "List"));
                    col = ((JSValue[])res);

                    Checkdecimal(col, datacontext.List);

                    datacontext.List.Add(10.5m);
                    datacontext.List.Add(126);

                    Thread.Sleep(150);
                    res = GetSafe(() => UnWrapCollection(js, "List"));
                    col = ((JSValue[])res);

                    Checkdecimal(col, datacontext.List);

                    Thread.Sleep(100);
                    res = GetSafe(() => UnWrapCollection(js, "List"));
                    col = ((JSValue[])res);

                    Checkdecimal(col, datacontext.List);

                    var comp = new List<decimal>(datacontext.List);
                    comp.Add(0.55m);

                    res = GetSafe(() => js.Invoke("List"));
                    DoSafe(() =>
                    ((JSObject)res).Invoke("push", new JSValue(0.55)));

                    Thread.Sleep(350);

                    res = GetSafe(() => UnWrapCollection(js, "List"));
                    col = ((JSValue[])res);

                    comp.Should().Equal(datacontext.List);
                    Checkdecimal(col, datacontext.List);

                }
            }
        }


        [Fact]
        public void Test_AwesomeBinding_Factory_TwoWay()
        {
            using (Tester())
            {
                var fact = new AwesomiumBindingFactory() { InjectionTimeOut = 5000, ManageWebSession = true };

                bool isValidSynchronizationContext = (_SynchronizationContext != null) && (_SynchronizationContext.GetType() != typeof(SynchronizationContext));
                isValidSynchronizationContext.Should().BeTrue();

                using (var mb = fact.Bind(_WebView, _DataContext, JavascriptBindingMode.TwoWay).Result)
                {
                    var js = mb.JSRootObject;

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

                    JSValue res5 = GetSafe(() => (((JSObject)((JSValue[])UnWrapCollection(js,"Skills"))[0]).Invoke("Name")));
                    ((string)res5).Should().Be("Langage");

                    _DataContext.Skills[0].Name = "Ling";
                    Thread.Sleep(50);

                    res5 = GetSafe(() => (((JSObject)((JSValue[])UnWrapCollection(js,"Skills"))[0]).Invoke("Name")));
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

        [Fact]
        public void Test_AwesomeBinding_stringBinding()
        {
            using (Tester())
            {
                bool isValidSynchronizationContext = (_SynchronizationContext != null) && (_SynchronizationContext.GetType() != typeof(SynchronizationContext));
                isValidSynchronizationContext.Should().BeTrue();


                using (var mb = StringBinding.Bind(_WebView, "{\"LastName\":\"Desmaisons\",\"Name\":\"O Monstro\",\"BirthDay\":\"0001-01-01T00:00:00.000Z\",\"PersonalState\":\"Married\",\"Age\":0,\"Local\":{\"City\":\"Florianopolis\",\"Region\":\"SC\"},\"MainSkill\":{},\"States\":[\"Single\",\"Married\",\"Divorced\"],\"Skills\":[{\"Type\":\"French\",\"Name\":\"Langage\"},{\"Type\":\"C++\",\"Name\":\"Info\"}]}").Result)
                {
                    var js = mb.JSRootObject;

                    mb.Root.Should().BeNull();


                    JSValue res = GetSafe(() => js.Invoke("Name"));
                    ((string)res).Should().Be("O Monstro");

                    JSValue res2 = GetSafe(() => js.Invoke("LastName"));
                    ((string)res2).Should().Be("Desmaisons");


                    JSValue res4 = GetSafe(() => ((JSObject)js.Invoke("Local")).Invoke("City"));
                    ((string)res4).Should().Be("Florianopolis");


                    JSValue res5 = GetSafe(() => (((JSObject)((JSValue[])UnWrapCollection(js,"Skills"))[0]).Invoke("Name")));
                    ((string)res5).Should().Be("Langage");
                }
            }
        }


        [Fact]
        public void Test_AwesomeBinding_Factory_stringBinding()
        {
            using (Tester())
            {
                var fact = new AwesomiumBindingFactory() { InjectionTimeOut = 5000, ManageWebSession = true };

                bool isValidSynchronizationContext = (_SynchronizationContext != null) && (_SynchronizationContext.GetType() != typeof(SynchronizationContext));
                isValidSynchronizationContext.Should().BeTrue();

                using (var mb = fact.Bind(_WebView, "{\"LastName\":\"Desmaisons\",\"Name\":\"O Monstro\",\"BirthDay\":\"0001-01-01T00:00:00.000Z\",\"PersonalState\":\"Married\",\"Age\":0,\"Local\":{\"City\":\"Florianopolis\",\"Region\":\"SC\"},\"MainSkill\":{},\"States\":[\"Single\",\"Married\",\"Divorced\"],\"Skills\":[{\"Type\":\"French\",\"Name\":\"Langage\"},{\"Type\":\"C++\",\"Name\":\"Info\"}]}").Result)
                {
                    var js = mb.JSRootObject;

                    JSValue res = GetSafe(() => Get(js, "Name"));
                    ((string)res).Should().Be("O Monstro");

                    JSValue res2 = GetSafe(() => js.Invoke("LastName"));
                    ((string)res2).Should().Be("Desmaisons");

                    JSValue res4 = GetSafe(() => ((JSObject)js.Invoke("Local")).Invoke("City"));
                    ((string)res4).Should().Be("Florianopolis");

                    JSValue res5 = GetSafe(() => (((JSObject)((JSValue[])this.UnWrapCollection(js, "Skills"))[0]).Invoke("Name")));
                    ((string)res5).Should().Be("Langage");
                }
            }
        }


        [Fact]
        public void Test_AwesomeBinding_Factory_Custo_Options()
        {
            using (Tester())
            {
                var fact = new AwesomiumBindingFactory() { InjectionTimeOut = -1, ManageWebSession = false };

                bool isValidSynchronizationContext = (_SynchronizationContext != null) && (_SynchronizationContext.GetType() != typeof(SynchronizationContext));
                isValidSynchronizationContext.Should().BeTrue();

                using (var mb = fact.Bind(_WebView, "{\"LastName\":\"Desmaisons\",\"Name\":\"O Monstro\",\"BirthDay\":\"0001-01-01T00:00:00.000Z\",\"PersonalState\":\"Married\",\"Age\":0,\"Local\":{\"City\":\"Florianopolis\",\"Region\":\"SC\"},\"MainSkill\":{},\"States\":[\"Single\",\"Married\",\"Divorced\"],\"Skills\":[{\"Type\":\"French\",\"Name\":\"Langage\"},{\"Type\":\"C++\",\"Name\":\"Info\"}]}").Result)
                {
                    var js = mb.JSRootObject;

                    JSValue res = GetSafe(() => Get(js, "Name"));
                    ((string)res).Should().Be("O Monstro");

                    JSValue res2 = GetSafe(() => js.Invoke("LastName"));
                    ((string)res2).Should().Be("Desmaisons");

                    JSValue res4 = GetSafe(() => ((JSObject)js.Invoke("Local")).Invoke("City"));
                    ((string)res4).Should().Be("Florianopolis");

                    JSValue res5 = GetSafe(() => (((JSObject)((JSValue[])UnWrapCollection(js,"Skills"))[0]).Invoke("Name")));
                    ((string)res5).Should().Be("Langage");
                }
            }
        }
    }
};

