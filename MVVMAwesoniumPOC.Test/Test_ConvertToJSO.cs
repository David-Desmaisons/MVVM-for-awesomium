using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using FluentAssertions;
using NSubstitute;
using Xunit;
using MVVMAwesoniumPOC.AwesomiumBinding;
using Awesomium.Core;

namespace MVVMAwesoniumPOC.Test
{
    public class Test_ConvertToJSO
    {
        private static IWebView _WebView  = null;
        static Test_ConvertToJSO()
        {
            _WebView = WebCore.CreateWebView(500, 500, WebViewType.Offscreen);
        }

        private class Test
        {
            public string S1 { get; set; }
            public int I1 { get; set; }
        }


        private ConvertToJSO _ConverTOJSO;
        private IJSOBuilder _IJSOBuilder;
        private Test _Test;

        public Test_ConvertToJSO()
        {
            _IJSOBuilder = new LocalBuilder();
            _ConverTOJSO = new ConvertToJSO(_IJSOBuilder);
            _Test = new Test { S1 = "string", I1 = 25 };
        }

        [Fact]
        public void Test_Null()
        {
            JSObject res = _ConverTOJSO.Convert(null);
            res.Should().BeNull();
        }


        [Fact]
        public void Test_Simple()
        {
            JSObject res = _ConverTOJSO.Convert(_Test);
            res.Should().NotBeNull();
            var res1 = res["S1"];
            res1.Should().NotBeNull();
            res1.IsString.Should().BeTrue();
            
            var res2 = res["I1"];
            res2.Should().NotBeNull();
            res2.IsNumber.Should().BeTrue();
        }


       
    }
}
