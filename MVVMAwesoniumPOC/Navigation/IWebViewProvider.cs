using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Awesomium.Core;

namespace MVVMAwesomium.Navigation
{
    public interface IWebViewProvider
    {
        IWebView WebView { get; }
    }
}
