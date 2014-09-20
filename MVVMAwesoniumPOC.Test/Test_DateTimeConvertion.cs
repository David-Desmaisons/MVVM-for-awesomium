using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Awesomium.Core;
using MVVMAwesomium.AwesomiumBinding;
using NSubstitute;
using FluentAssertions;
using Xunit;

namespace MVVMAwesomium.Test
{
    public class Test_DateTimeConvertion : Awesomium_Test_Base
    {
        private CSharpToJavascriptMapper _ConverTOJSO;
        private LocalBuilder _IJSOBuilder;
        private ICSharpMapper _ICSharpMapper;
        private JavascriptToCSharpMapper _JavascriptToCSharpMapper;


        private void Init()
        {
            _JavascriptToCSharpMapper = new JavascriptToCSharpMapper(_WebView);
            _IJSOBuilder = new LocalBuilder(_WebView);
            _ICSharpMapper = Substitute.For<ICSharpMapper>();
            _ICSharpMapper.GetCached(Arg.Any<object>()).Returns((IJSCBridge)null);
            _ConverTOJSO = new CSharpToJavascriptMapper(_IJSOBuilder, _ICSharpMapper);
        }
    
        private int GetYear(JSObject idate)
        {
            return (int)idate.Invoke("getFullYear",null);
        }

        [Fact]
        public void Test_DateTime_FromCSharp()
        {
            using (Tester())
            {
                Init();

                var mapped = _ConverTOJSO.Map(new DateTime(1974, 2, 26));
                mapped.Type.Should().Be(JSType.Basic);
                JSObject date = mapped.JSValue;
                date.Should().NotBeNull();

                int year = this._WebView.EvaluateSafe(() => GetYear(date));
                year.Should().Be(1974);

                int month = this._WebView.EvaluateSafe(() => (int)date.Invoke("getMonth", null));
                month.Should().Be(1);

                int day = this._WebView.EvaluateSafe(() => (int)date.Invoke("getDate", null));
                day.Should().Be(26);
            }
        }

        [Fact]
        public void Test_DateTime_FromCSharp_Back()
        {
            using (Tester())
            {
                Init();

                var mapped = _ConverTOJSO.Map(new DateTime(1974, 2, 26));
                JSObject date = mapped.JSValue;
                date.Should().NotBeNull();

                var res = _WebView.EvaluateSafe(() => _JavascriptToCSharpMapper.GetSimpleValue(date));
                    
                res.Should().NotBeNull();
                (res is DateTime).Should().BeTrue();
                DateTime mdt = (DateTime)res;

                mdt.Year.Should().Be(1974);
                mdt.Day.Should().Be(26);
                mdt.Month.Should().Be(2);
            }
        }

        [Fact]
        public void Test_GetSimpleValue_Object()
        {
            using (Tester())
            {
                Init();
                var res = _WebView.EvaluateSafe(() => _JavascriptToCSharpMapper.GetSimpleValue(new JSValue(new JSObject())));
         
                res.Should().Be(null);
            }
        }

        [Fact]
        public void Test_GetSimpleValue_Undefined()
        {
            using (Tester())
            {
                Init();
                var res = _WebView.EvaluateSafe(() => _JavascriptToCSharpMapper.GetSimpleValue(new JSValue((JSObject)null)));
 
                res.Should().Be(null);
            }
        }

    }
}
