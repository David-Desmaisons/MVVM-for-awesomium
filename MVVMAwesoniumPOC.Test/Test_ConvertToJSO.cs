using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using FluentAssertions;
using NSubstitute;
using Xunit;
using MVVMAwesonium.AwesomiumBinding;
using Awesomium.Core;
using System.Collections;

namespace MVVMAwesonium.Test
{
    public class Test_ConvertToJSO
    {
        private class Test
        {
            public string S1 { get; set; }
            public int I1 { get; set; }
        }

        private class Test2
        {
            public Test T1 { get; set; }
            public Test T2 { get; set; }
        }


        private CSharpToJavascriptMapper _ConverTOJSO;
        private LocalBuilder _IJSOBuilder;
        private Test _Test;
        private Test2 _Test2;
        private List<Test> _Tests;
        private ArrayList _Tests_NG;

        private IWebView _WebView = null;
        private ICSharpMapper _ICSharpMapper;
       
        public Test_ConvertToJSO()
        { 
            _WebView = WebCore.CreateWebView(500, 500, WebViewType.Offscreen);
            _IJSOBuilder = new LocalBuilder();
            _ICSharpMapper = Substitute.For<ICSharpMapper>();
            _ICSharpMapper.GetCached(Arg.Any<object>()).Returns((IJSCBridge)null);
            _ConverTOJSO = new CSharpToJavascriptMapper(_IJSOBuilder, _ICSharpMapper);
            _Test = new Test { S1 = "string", I1 = 25 };
            _Tests = new List<Test>();
            _Tests.Add(new Test() { S1 = "string1", I1 = 1 });
            _Tests.Add(new Test() { S1 = "string2", I1 = 2 });
            _Test2 = new Test2() { T1 = _Test, T2 = _Test };

            _Tests_NG = new ArrayList();
            _Tests_NG.Add(_Tests[0]);
            _Tests_NG.Add(_Tests[0]);
        }

        [Fact]
        public void Test_Null()
        {
            JSValue res = _ConverTOJSO.Map(null).JSValue;
            res.IsNull.Should().BeTrue();
        }

        [Fact]
        public void Test_Simple()
        {
            JSObject res = _ConverTOJSO.Map(_Test).JSValue;
            res.Should().NotBeNull();
            var res1 = res["S1"];
            res1.Should().NotBeNull();
            res1.IsString.Should().BeTrue();

            var res2 = res["I1"];
            res2.Should().NotBeNull();
            res2.IsNumber.Should().BeTrue();
        }


        [Fact]
        public void Test_List()
        {
            JSValue[] resv = (JSValue[])_ConverTOJSO.Map(_Tests).JSValue;

            resv.Should().NotBeNull();
            resv.Length.Should().Be(2);

            JSObject res = resv[0];
            res.Should().NotBeNull();
            var res1 = res["S1"];
            res1.Should().NotBeNull();
            res1.IsString.Should().BeTrue();

            var jsv = ((JSValue)res["S1"]);
            jsv.Should().NotBeNull();
            jsv.IsString.Should().BeTrue();
            string stv = (string)jsv;
            stv.Should().NotBeNull();
            stv.Should().Be("string1");

            var res2 = res["I1"];
            res2.Should().NotBeNull();
            res2.IsNumber.Should().BeTrue();
            int v2 = (int)res2;
            v2.Should().Be(1);
        }

        [Fact]
        public void Test_List_Not_Generic()
        {
            JSValue[] resv = (JSValue[])_ConverTOJSO.Map(_Tests_NG).JSValue;

            resv.Should().NotBeNull();
            resv.Length.Should().Be(2);

            JSObject res = resv[0];
            res.Should().NotBeNull();
            var res1 = res["S1"];
            res1.Should().NotBeNull();
            res1.IsString.Should().BeTrue();

            var jsv = ((JSValue)res["S1"]);
            jsv.Should().NotBeNull();
            jsv.IsString.Should().BeTrue();
            string stv = (string)jsv;
            stv.Should().NotBeNull();
            stv.Should().Be("string1");

            var res2 = res["I1"];
            res2.Should().NotBeNull();
            res2.IsNumber.Should().BeTrue();
            int v2 = (int)res2;
            v2.Should().Be(1);
        }


        [Fact]
        public void Test_Double()
        {
            JSValue res = _ConverTOJSO.Map(0.2D).JSValue;
            res.Should().NotBeNull();
            res.IsNumber.Should().BeTrue();
            double resd = (double)res;

            resd.Should().Be(0.2D);
        }

        [Fact]
        public void Test_Decimal()
        {
            JSValue res = _ConverTOJSO.Map(0.2M).JSValue;
            res.Should().NotBeNull();
            res.IsNumber.Should().BeTrue();
            double resd = (double)res;

            resd.Should().Be(0.2D);
        }


        [Fact]
        public void Test_Bool()
        {
            JSValue res = _ConverTOJSO.Map(true).JSValue;
            res.Should().NotBeNull();
            res.IsBoolean.Should().BeTrue();
            bool resd = (bool)res;

            resd.Should().BeTrue();
        }

        [Fact]
        public void Test_Bool_False()
        {
            JSValue res = _ConverTOJSO.Map(false).JSValue;
            res.Should().NotBeNull();
            res.IsBoolean.Should().BeTrue();
            bool resd = (bool)res;

            resd.Should().BeFalse();
        }


        [Fact]
        public void Test_String()
        {
            JSValue res = _ConverTOJSO.Map("toto").JSValue;
            res.Should().NotBeNull();
            res.IsString.Should().BeTrue();
            string resd = (string)res;

            resd.Should().Be("toto");
        }

        [Fact]
        public void Test_Object_Double_reference()
        {
            JSObject res = _ConverTOJSO.Map(_Test2).JSValue;
            res.Should().NotBeNull();

            _ICSharpMapper.Received().Cache(_Test,Arg.Any<IJSCBridge>());
        }
    }
}
