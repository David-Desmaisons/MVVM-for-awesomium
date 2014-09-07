using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.ComponentModel;
using System.Threading.Tasks;

using Awesomium.Core;

using MVVMAwesonium.Infra;

namespace MVVMAwesonium.AwesomiumBinding
{
    public class AwesomeBinding : IDisposable
    {
        private ConvertToJSO _ConvertToJSO;
        private JSObject _JSObject;
        private IWebView _IWebView;
        
        private AwesomeBinding(IWebView iWebView)
        {
            _IWebView = iWebView;
        }

        private IDisposable JavaScripterListener { get; set; }

        private void Init(JSObject iJSObject, ConvertToJSO iConvertToJSO)
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

        internal JSObject JSRootObject
        {
            get { return _JSObject; }
        }

        private void lis_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            string pn = e.PropertyName;

            var value = _ConvertToJSO.GetValue(sender, pn);

            WebCore.QueueWork(
                () =>
                {
                    var el = (JSObject)_ConvertToJSO.Objects[sender].JSValue;
                    el.Invoke(pn, value);
                });
        }

        private void OnJavaScriptChanges(JSObject objectchanged, string PropertyName ,JSValue newValue)
        {
            INotifyPropertyChanged inpc = _ConvertToJSO.FromJSToCS[objectchanged].CValue as INotifyPropertyChanged;
            if (inpc == null)
                return;

            _ConvertToJSO.SetValue(inpc, PropertyName, newValue);
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
            WebCore.QueueWork( ()=>JavaScripterListener.Dispose());
        }

        public static Task<AwesomeBinding> Bind(IWebView view, object iViewModel, JavascriptBindingMode iMode)
        {
            TaskCompletionSource<AwesomeBinding> tcs = new TaskCompletionSource<AwesomeBinding>();
   
            Action ToBeApply = () =>
                    {
                        AwesomeBinding binding = new AwesomeBinding(view);

                        ConvertToJSO ctj = new ConvertToJSO(new LocalBuilder());
                        JSObject js = ctj.Convert(iViewModel);

                        var mapper = new JavaScriptMapper(view);

                        if (iMode == JavascriptBindingMode.TwoWay)
                        {
                            binding.JavaScripterListener = mapper.Subscribe(binding.OnJavaScriptChanges);
                        }

                        JSObject res = mapper.MappToJavaScriptSession(js, ctj);

                        binding.Init(res, ctj);

                        tcs.SetResult(binding);
                    };

            if (view.IsDocumentReady)
            {
                WebCore.QueueWork( ()=>ToBeApply()) ;           
            }
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
