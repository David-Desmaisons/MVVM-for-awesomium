using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Awesomium.Core;

namespace MVVMAwesonium.AwesomiumBinding
{
    public abstract class JSOBuilderBase
    {
        protected IWebView _IWebView;
        protected JSOBuilderBase(IWebView iIWebView)
        {
            _IWebView = iIWebView;
        }

        public JSValue CreateDate(DateTime dt)
        {
            return _IWebView.EvaluateSafe(() => 
                        _IWebView.ExecuteJavascriptWithResult(string.Format("new Date({0})",
                        string.Join(",", dt.Year, dt.Month -1, dt.Day, dt.Hour, dt.Minute, dt.Second, dt.Millisecond))));
        }
    }
}
