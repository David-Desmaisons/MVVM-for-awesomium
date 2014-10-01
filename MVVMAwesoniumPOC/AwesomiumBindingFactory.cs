using Awesomium.Core;
using MVVMAwesomium.AwesomiumBinding;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MVVMAwesomium
{
    public class AwesomiumBindingFactory : MVVMAwesomium.IAwesomiumBindingFactory
    {
        public AwesomiumBindingFactory()
        {
            InjectionTimeOut = 0;
            ManageWebSession = false;
        }

        public Task<IAwesomeBinding> Bind(IWebView view, object iViewModel, JavascriptBindingMode iMode)
        {
            Action First = null;
            if (InjectionTimeOut != -1)
                First = () => view.SynchronousMessageTimeout = InjectionTimeOut;

            Action doclean = null;
            if (ManageWebSession) doclean = () => view.Dispose();

            return AwesomeBinding.Bind(view, iViewModel, iMode, First, doclean);
        }


        public int InjectionTimeOut { get;set;}

        public bool ManageWebSession { get; set; }
    }
}
