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
        private IWebView _IWebView;

        public AwesomeBinding(JSObject iJSObject, ConvertToJSO iConvertToJSO, IWebView iWebView)
        {
            _IWebView = iWebView;
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
            string pn =  e.PropertyName;

            var value = _ConvertToJSO.GetValue(sender, pn);

            JSOObjectDescriptor desc = _ConvertToJSO.Objects[sender];
            desc.GetPaths().ForEach(p =>
                {
                    JSObject js = _JSObject;
                    int count = p.Count;
                    for (int i = 0; i < count; i++)
                    { 
                        int index = -1;
                        if ( (i+1<count) && (int.TryParse(p[i+1], out index)))
                        {
                            js = ((JSValue[])js.Invoke(p[i++]))[index];
                        }
                        else
                            js = (JSObject)js[p[i]];
                    }

                    js.Invoke(pn, value);
                });
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
                    //JSObject mapping = view.ExecuteJavascriptWithResult("ko.mapping");
                    //JSObject res = mapping.Invoke("fromJS", js);
                    JSObject res = vm.Invoke("MapToObservable", js);
                    JSObject res2 = vm.Invoke("applyBindings", res);
                    
                    tcs.SetResult(new AwesomeBinding(res, ctj,view));
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
