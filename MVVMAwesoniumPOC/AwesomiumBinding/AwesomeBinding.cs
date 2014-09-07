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
        private JavascriptObjectMapper _ConvertToJSO;
        private JSObject _JSObject;
        private IWebView _IWebView;

        private AwesomeBinding(IWebView iWebView)
        {
            _IWebView = iWebView;
        }

        private IDisposable JavaScripterListener { get; set; }

        private void Init(JSObject iJSObject, JavascriptObjectMapper iConvertToJSO)
        {
            _ConvertToJSO = iConvertToJSO;
            _JSObject = iJSObject;

        }

        private bool _IsListening = false;
        private void ListenToCSharpObject()
        {
            if (_IsListening)
                return;

            _ConvertToJSO.Objects.ForEach(
                        t =>
                        {
                            var lis = t.Key as INotifyPropertyChanged;
                            if (lis != null) lis.PropertyChanged += new PropertyChangedEventHandler(lis_PropertyChanged);
                        }
                    );
            _IsListening = true;
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

        private void OnJavaScriptChanges(JSObject objectchanged, string PropertyName, JSValue newValue)
        {
            INotifyPropertyChanged inpc = _ConvertToJSO.FromJSToCS[objectchanged].CValue as INotifyPropertyChanged;
            if (inpc == null)
                return;

            _ConvertToJSO.SetValue(inpc, PropertyName, newValue);
        }

        public void Dispose()
        {
            if (_IsListening)
            {
                _IsListening = false;

                _ConvertToJSO.Objects.ForEach(
                   t =>
                   {
                       var lis = t.Key as INotifyPropertyChanged;
                       if (lis != null) lis.PropertyChanged -= new PropertyChangedEventHandler(lis_PropertyChanged);
                   }
               );
            }

            if (JavaScripterListener != null)
                WebCore.QueueWork(() => JavaScripterListener.Dispose());
        }

        public static Task<AwesomeBinding> Bind(IWebView view, object iViewModel, JavascriptBindingMode iMode)
        {
            TaskCompletionSource<AwesomeBinding> tcs = new TaskCompletionSource<AwesomeBinding>();

            Action ToBeApply = () =>
                    {
                        AwesomeBinding binding = new AwesomeBinding(view);

                        JavascriptObjectMapper ctj = new JavascriptObjectMapper(new LocalBuilder());
                        JSObject js = ctj.CreateLocalJSValue(iViewModel);

                        var mapper = new JavascriptSessionMapper(view);

                        if (iMode == JavascriptBindingMode.TwoWay)
                        {
                            binding.JavaScripterListener = mapper.SubscribeToJavascriptObjectChange(binding.OnJavaScriptChanges);
                        }

                        JSObject res = mapper.MappToJavaScriptSession(js, (iMode == JavascriptBindingMode.OneTime) ? null : ctj);

                        binding.Init(res, ctj);
                        if (iMode != JavascriptBindingMode.OneTime)
                            binding.ListenToCSharpObject();

                        tcs.SetResult(binding);
                    };

            if (view.IsDocumentReady)
            {
                WebCore.QueueWork(() => ToBeApply());
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
