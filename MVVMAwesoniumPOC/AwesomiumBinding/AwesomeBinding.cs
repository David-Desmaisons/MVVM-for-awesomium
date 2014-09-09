using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.ComponentModel;
using System.Threading.Tasks;

using Awesomium.Core;

using MVVMAwesonium.Infra;
using System.Collections;
using System.Collections.Specialized;

namespace MVVMAwesonium.AwesomiumBinding
{
    public class AwesomeBinding : IDisposable
    {
        private JavascriptObjectMapper _MappedObject;
        private IWebView _IWebView;

        private AwesomeBinding(IWebView iWebView)
        {
            _IWebView = iWebView;
        }

        private IJSListener JavaScripterListener { get; set; }

        private void Init(JavascriptObjectMapper iConvertToJSO, IJSListener iJavaScripterListener)
        {
            _MappedObject = iConvertToJSO;
            JavaScripterListener = iJavaScripterListener;
        }

        private bool _IsListening = false;
        private void ListenToCSharpObject()
        {
            if (_IsListening)
                return;

            _MappedObject.Root.ApplyOnListenable(n => n.PropertyChanged += Object_PropertyChanged,
                                                    c => c.CollectionChanged += CollectionChanged);
            _IsListening = true;
        }

       

        internal JSObject JSRootObject
        {
            get { return _MappedObject.Root.JSValue; }
        }

        private void Object_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            string pn = e.PropertyName;

            JSValue? value = _MappedObject.GetValue(sender, pn);

            if (value == null)
                return;

            WebCore.QueueWork(
                () =>
                {
                    var el = (JSObject)_MappedObject.Objects[sender].JSValue;
                    el.Invoke(pn, value.Value);
                });
        }

        private void CollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            switch (e.Action)
            {
                case NotifyCollectionChangedAction.Add:
                    JSValue? addvalue = _MappedObject.CreateLocalJSValue(e.NewItems[0]);

                    if (addvalue == null)
                        return;

                    WebCore.QueueWork(
                        () =>
                        {
                            var el = (JSObject)_MappedObject.Objects[sender].JSValue;
                            el.Invoke("splice",new JSValue(e.NewStartingIndex), new JSValue(0),addvalue.Value);
                        });
                    
                        break;

                case NotifyCollectionChangedAction.Remove:
                        WebCore.QueueWork(
                            () =>
                            {
                                var el = (JSObject)_MappedObject.Objects[sender].JSValue;
                                el.Invoke("splice", new JSValue(e.OldStartingIndex), new JSValue(e.OldItems.Count));
                            });

                        break;

                case NotifyCollectionChangedAction.Replace:
                       JSValue? newvalue = _MappedObject.CreateLocalJSValue(e.NewItems[0]);

                       if (newvalue == null)
                        return;

                    WebCore.QueueWork(
                        () =>
                        {
                            var el = (JSObject)_MappedObject.Objects[sender].JSValue;
                            el.Invoke("splice", new JSValue(e.NewStartingIndex), new JSValue(1), newvalue.Value);
                        });
                    
                        break;
            }
        }

        private void OnJavaScriptChanges(JSObject objectchanged, string PropertyName, JSValue newValue)
        {
            INotifyPropertyChanged inpc = _MappedObject.FromJSToCS[objectchanged].CValue as INotifyPropertyChanged;
            if (inpc == null)
                return;

            _MappedObject.SetValue(inpc, PropertyName, newValue);
        }

        public void Dispose()
        {
            if (_IsListening)
            {
                _MappedObject.Root.ApplyOnListenable(n => n.PropertyChanged -= Object_PropertyChanged,
                                                    c => c.CollectionChanged -= CollectionChanged);
                
                _IsListening = false;
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

                        JavascriptObjectMapper ObjectMapper = JavascriptObjectMapper.CreateMapping(iViewModel);

                        var mapper = new JavascriptSessionMapper(view);

                        if (iMode == JavascriptBindingMode.TwoWay)
                            mapper.OnJavascriptObjecChanges = binding.OnJavaScriptChanges;

                        var listener =  mapper.MappToJavaScriptSession(ObjectMapper);

                        binding.Init( ObjectMapper, listener);
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
