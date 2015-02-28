using Awesomium.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace MVVMAwesomium.Navigation
{
    public interface IWebViewLifeCycleManager
    {
        IWebView Create();

        void Display(IWebView webview);

        void Dispose(IWebView ioldwebview);
    }
}
