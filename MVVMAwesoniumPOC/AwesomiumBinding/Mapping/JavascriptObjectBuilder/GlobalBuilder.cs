using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Awesomium.Core;
using System.Threading.Tasks;

namespace MVVMAwesomium.AwesomiumBinding
{
    public class GlobalBuilder : JSOBuilderBase, IJSOBuilder
    {
        private static int _Count = 0;
        private string _NameScape;

        public GlobalBuilder(IWebView iWebView, string iNameScape)
            : base(iWebView)
        {
            _NameScape = iNameScape;
        }

        public JSObject CreateJSO()
        {
            string Name = string.Format("{0}_{1}", _NameScape, _Count++);
            return _IWebView.EvaluateSafe(() => _IWebView.CreateGlobalJavascriptObject(Name));
        }
    }
}
