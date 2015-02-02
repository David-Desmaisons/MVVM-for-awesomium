using Awesomium.Core;
using Awesomium.Windows.Controls;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;

namespace MVVMAwesomium.Navigation
{
    public class WebViewSimpleLifeCycleManager : IWebViewLifeCycleManager
    {
        private WebControl _First;
        private WebControl _Second;
        private bool _FirstElement = true;
        public WebViewSimpleLifeCycleManager(WebControl First, WebControl Second)
        {
            _First=First;
            _Second = Second;
            _First.Visibility = Visibility.Hidden;
            _Second.Visibility = Visibility.Hidden;
        }

        public IWebView Create()
        {      
            var res = _FirstElement ? _First : _Second;
            _FirstElement = !_FirstElement;
            if (res.WebSession!=null)
                res.WebSession.ClearCache();
            return res;
        }

        public void Dispose(IWebView ioldwebview)
        {
            ioldwebview.Source = new Uri("about:blank");
            (ioldwebview as WebControl).Visibility = Visibility.Hidden;
        }


        public void Display(IWebView webview)
        {
            (webview as WebControl).Visibility = Visibility.Visible;
        }
    }
}
