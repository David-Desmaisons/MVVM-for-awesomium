using Awesomium.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace MVVMAwesonium.AwesomiumBinding.JavascriptMapper
{
    internal interface IJavaScriptMapper
    {
        JSObject MappToJavaScriptSession(JSObject jsobject, IWebView iWebView);
    }
}
