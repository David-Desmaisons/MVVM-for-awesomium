using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.ComponentModel;
using System.Threading.Tasks;

using Awesomium.Core;

using MVVMAwesoniumPOC.Infra;

namespace MVVMAwesoniumPOC.AwesomiumBinding
{
    public class AwesomeBinding : IDisposable
    {
        private ConvertToJSO _ConvertToJSO;
        private JSObject _JSObject;

        public AwesomeBinding(JSObject iJSObject, ConvertToJSO iConvertToJSO)
        {
            _ConvertToJSO = iConvertToJSO;
            _JSObject = iJSObject;
            _ConvertToJSO.Objects.ForEach(
                t =>
                {
                    var lis = t.Key as INotifyPropertyChanged;
                    if (lis != null) lis.PropertyChanged += new PropertyChangedEventHandler(lis_PropertyChanged);
                }
            );
        }

        private void lis_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            //var java = (JSObject)_ConvertToJSO.Objects[sender];
            var value = _ConvertToJSO.GetValue(sender, e.PropertyName);
            JSObject res = _JSObject.Invoke(e.PropertyName, value);
        }

        public void Dispose()
        {
            _ConvertToJSO.Objects.ForEach(
               t =>
               {
                   var lis = t.Key as INotifyPropertyChanged;
                   if (lis != null) lis.PropertyChanged -= new PropertyChangedEventHandler(lis_PropertyChanged);
               }
           );
        }

        public static Task<AwesomeBinding> ApplyBinding(IWebView view, INotifyPropertyChanged iViewModel)
        {
            TaskCompletionSource<AwesomeBinding> tcs = new TaskCompletionSource<AwesomeBinding>();

            Action ToBeApply =
                ()  =>
                {
                    //view.SynchronousMessageTimeout = 1500;
                    //ConvertToJSO ctj = new ConvertToJSO(new GlobalBuilder(view,"ViewModel"));
                    
                    ConvertToJSO ctj = new ConvertToJSO( new LocalBuilder());
                    
                    JSObject js = ctj.Convert(iViewModel);

                    JSObject vm = view.ExecuteJavascriptWithResult("ko");

                    JSObject res = vm.Invoke("MapToObservable", js);
                    JSObject res2 = vm.Invoke("applyBindings", res);

                    tcs.SetResult(new AwesomeBinding(res, ctj));
                };


            if (view.IsDocumentReady)
                ToBeApply();
            else
            {
                UrlEventHandler ea = null;
                ea = (o, e) => { ToBeApply(); view.DocumentReady -= ea; };
                view.DocumentReady += ea;
            }

            return tcs.Task;

        }

       
    }
}
