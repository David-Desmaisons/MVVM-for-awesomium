using Awesomium.Core;
using MVVMAwesomium.AwesomiumBinding;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MVVMAwesomium
{
    public class AwesomiumBindingFactory : IAwesomiumBindingFactory
    {
        public AwesomiumBindingFactory()
        {
            InjectionTimeOut = 0;
            ManageWebSession = false;
        }

        private Action GetFirst(IWebView view)
        {
            if (InjectionTimeOut != -1)
                return () => view.SynchronousMessageTimeout = InjectionTimeOut;

            return null;
        }

        private Action GetLast(IWebView view)
        {
            if (ManageWebSession) 
                return () => view.Dispose();

            return null;
        }

        public Task<IAwesomeBinding> Bind(IWebView view, object iViewModel, JavascriptBindingMode iMode)
        {
            return AwesomeBinding.Bind(view, iViewModel, null,iMode, GetFirst(view), GetLast(view));
        }

        public Task<IAwesomeBinding> Bind(IWebView view, string json)
        {
            return StringBinding.Bind(view, json, GetFirst(view), GetLast(view));
        }

        public Task<IAwesomeBinding> Bind(IWebView view, object iViewModel, object addinfo, JavascriptBindingMode iMode)
        {
            return AwesomeBinding.Bind(view, iViewModel, addinfo, iMode, GetFirst(view), GetLast(view));
        }


        public int InjectionTimeOut { get;set;}

        public bool ManageWebSession { get; set; }


     
    }
}
