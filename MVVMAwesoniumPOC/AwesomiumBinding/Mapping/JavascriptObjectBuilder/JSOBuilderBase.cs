using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Awesomium.Core;



namespace MVVMAwesomium.AwesomiumBinding
{
    public abstract class JSOBuilderBase
    {
        protected IWebView _IWebView;
        protected JSOBuilderBase(IWebView iIWebView)
        {
            _IWebView = iIWebView;
        } 
    }
}
