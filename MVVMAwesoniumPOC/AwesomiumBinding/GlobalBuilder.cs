using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Awesomium.Core;

namespace MVVMAwesoniumPOC.AwesomiumBinding
{
    public class GlobalBuilder : IJSOBuilder
    {
        private IWebView _IWebView;
        private static int _Count = 0;
        private string _NameScape;

        public GlobalBuilder(IWebView iWebView, string iNameScape)
        {
            _IWebView = iWebView;
            _NameScape = iNameScape;
        }

        public JSObject CreateJSO()
        {
            return _IWebView.CreateGlobalJavascriptObject(string.Format("{0}{1}", _NameScape, _Count++));
        }
    }
}
