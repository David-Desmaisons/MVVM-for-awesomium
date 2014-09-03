using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.ComponentModel;
using Awesomium.Core;

namespace MVVMAwesoniumPOC.AwesomiumBinding
{
    public static class AwesomeBinding
    {
        public static void  ApplyBinding(IWebView view, INotifyPropertyChanged iViewModel)
        {
            Action ToBeApply =
                ()
                =>
                {
                    ConvertToJSO ctj = new ConvertToJSO( new LocalBuilder());
                    JSObject js = ctj.Convert(iViewModel);

                    JSObject vm = view.ExecuteJavascriptWithResult("ko");

                    JSObject res = vm.Invoke("MapToObservable", js);
                    JSObject res2 = vm.Invoke("applyBindings", res);

                };


            if (view.IsDocumentReady)
                ToBeApply();
            else
            {
                UrlEventHandler ea = null;
                ea = (o, e) => { ToBeApply(); view.DocumentReady -= ea; };
                view.DocumentReady += ea;
            }

        }
    }
}
