using System;
using System.Threading.Tasks;
using Awesomium.Core;
namespace MVVMAwesomium
{
    public interface IAwesomiumBindingFactory
    {
        Task<IAwesomeBinding> Bind(IWebView view, object iViewModel, JavascriptBindingMode iMode);

        Task<IAwesomeBinding> Bind(IWebView view, object iViewModel, object addinfo, JavascriptBindingMode iMode);


        Task<IAwesomeBinding> Bind(IWebView view, string json);
    }
}
