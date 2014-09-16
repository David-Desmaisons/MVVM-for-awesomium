using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Awesomium.Core;
using MVVMAwesonium.AwesomiumBinding;
using NSubstitute;
using FluentAssertions;
using Xunit;

namespace MVVMAwesonium.Test
{
    public class DateTimeConvertion : Awesomium_Test_Base
    {
        private CSharpToJavascriptMapper _ConverTOJSO;
        private LocalBuilder _IJSOBuilder;
        private ICSharpMapper _ICSharpMapper;


        private void Init()
        {
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
        public void Test_DateTime()
        {
            using (Tester())
            {
                Init();

                var mapped = _ConverTOJSO.Map(new DateTime(1974, 2, 26));
                JSObject date = mapped.JSValue;
                date.Should().NotBeNull();

                int year = this._WebView.EvaluateSafe(() => GetYear(date));
                year.Should().Be(1974);
            }
        }
    }
}
