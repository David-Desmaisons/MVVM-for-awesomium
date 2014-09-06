using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.ComponentModel;
using System.Threading.Tasks;

using Awesomium.Core;

using MVVMAwesonium.Infra;
using MVVMAwesonium.AwesomiumBinding.JavascriptMapper;

namespace MVVMAwesonium.AwesomiumBinding
{
    public class AwesomeBinding : IDisposable
    {
        private ConvertToJSO _ConvertToJSO;
        private JSObject _JSObject;
        private IWebView _IWebView;
        private bool _InitReentry = false;

        private AwesomeBinding(IWebView iWebView)
        {
            _IWebView = iWebView;
        }

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
            if (_InitReentry)
                return;

            string pn = e.PropertyName;

            var value = _ConvertToJSO.GetValue(sender, pn);

            WebCore.QueueWork(
                () =>
                {
                    JSOObjectDescriptor desc = _ConvertToJSO.Objects[sender];
                    desc.GetPaths().ForEach(p =>
                        {
                            JSObject js = _JSObject;
                            int count = p.Count;
                            for (int i = 0; i < count; i++)
                            {
                                int index = -1;
                                if ((i + 1 < count) && (int.TryParse(p[i + 1], out index)))
                                {
                                    js = ((JSValue[])js.Invoke(p[i++]))[index];
                                }
                                else
                                    js = (JSObject)js[p[i]];
                            }

                            js.Invoke(pn, value);
                        });
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

        public static Task<AwesomeBinding> Bind(IWebView view, object iViewModel, JavascriptBindingMode iMode)
        {
            TaskCompletionSource<AwesomeBinding> tcs = new TaskCompletionSource<AwesomeBinding>();
   
            Action ToBeApply = () =>
                    {
                        AwesomeBinding binding = new AwesomeBinding(view);

                        IJavaScriptMapper mapper = (iMode == JavascriptBindingMode.OneWay) ?
                            OneWayJavascriptMapping.Binder : OneWayJavascriptMapping.Binder;

                        ConvertToJSO ctj = new ConvertToJSO(new LocalBuilder());
                        JSObject js = ctj.Convert(iViewModel);
                        JSObject res = mapper.MappToJavaScriptSession(js, view);

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
