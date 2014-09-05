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

namespace MVVMAwesonium.Test
{
    public class Test_AwesomeBinding
    {
        private static IWebView _WebView = null;
        private Person _DataContext;

        static Test_AwesomeBinding()
        {
            _WebView = WebCore.CreateWebView(500, 500, WebViewType.Window);
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
        }

        [Fact]
        public void Test_AwesomeBinding_Basic()
        {
            var mb = AwesomeBinding.ApplyBinding(_WebView, _DataContext).Result;
        }


    }
}
