using System;
using System.Threading.Tasks;
namespace MVVMAwesomium
{
    interface IAwesomiumBindingFactory
    {
        Task<IAwesomeBinding> Bind(Awesomium.Core.IWebView view, object iViewModel, JavascriptBindingMode iMode);
    }
}
