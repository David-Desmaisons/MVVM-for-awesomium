using Awesomium.Core;
using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Linq;
using System.Reflection;
using System.Text;

namespace MVVMAwesonium.AwesomiumBinding
{
    public class BidirectionalMapper : IDisposable, ICSharpMapper
    {
        private JavascriptBindingMode _BindingMode;
        private readonly IJSCBridge _Root;
        private CSharpToJavascriptMapper _CSharpToJavascriptMapper;
        private JavascriptSessionInjector _JavascriptSessionMapper;
        private IDictionary<object, IJSCBridge> _FromCSharp = new Dictionary<object, IJSCBridge>();
        private IDictionary<uint, IJSCBridge> _FromJavascript = new Dictionary<uint, IJSCBridge>();

        internal BidirectionalMapper(object iRoot, IWebView iwebview, JavascriptBindingMode iMode)
        {
            _CSharpToJavascriptMapper = new CSharpToJavascriptMapper(new LocalBuilder(), this);
            _Root = _CSharpToJavascriptMapper.Map(iRoot);
            _BindingMode = iMode;

            Action<JSObject, string, JSValue> JavascriptObjecChanges = null;
            if (iMode == JavascriptBindingMode.TwoWay)
                JavascriptObjecChanges = OnJavaScriptChanges;

            _JavascriptSessionMapper = new JavascriptSessionInjector(iwebview, JavascriptObjecChanges);
            InjectInHTLMSession(_Root, true);

            if (ListenToCSharp)
            {
                ListenToCSharpChanges(_Root);
            }
        }

        #region IJavascriptMapper

        private class JavascriptMapper : IJavascriptMapper
        {
            private IJSCBridge _Root;
            private BidirectionalMapper _LiveMapper;
            public JavascriptMapper(IJSCBridge iRoot, BidirectionalMapper iFather)
            {
                _LiveMapper = iFather;
                _Root = iRoot;
            }

            public void RegisterFirst(JSObject iRoot)
            {
                _LiveMapper.Update(_Root, iRoot);
            }

            public void RegisterMapping(JSObject iFather, string att, JSObject iChild)
            {
                _LiveMapper.RegisterMapping(iFather, att, iChild);
            }

            public void RegisterCollectionMapping(JSObject iFather, string att, int index, JSObject iChild)
            {
                _LiveMapper.RegisterCollectionMapping(iFather, att, index, iChild);
            }
        }

        private IJSCBridge GetFromJavascript(JSObject jsobject)
        {
            return _FromJavascript[jsobject.RemoteId];
        }

        private void Update(IJSCBridge ibo, JSObject jsobject)
        {
            ibo.JSValue = jsobject;
            _FromJavascript[jsobject.RemoteId] = ibo;
        }

        public void RegisterMapping(JSObject iFather, string att, JSObject iChild)
        {
            JSGenericObject jso = GetFromJavascript(iFather) as JSGenericObject;
            Update(jso.Attributes[att], iChild);
        }

        public void RegisterCollectionMapping(JSObject iFather, string att, int index, JSObject iChild)
        {
            JSGenericObject jsof = GetFromJavascript(iFather) as JSGenericObject;
            JSArray jsos = jsof.Attributes[att] as JSArray;
            Update(jsos.Items[index], iChild);
        }

        #endregion

        public bool ListenToCSharp { get { return (_BindingMode != JavascriptBindingMode.OneTime); } }

        private void ListenToCSharpChanges(IJSCBridge ibridge)
        {
            _Root.ApplyOnListenable(n => n.PropertyChanged += Object_PropertyChanged,
                                     c => c.CollectionChanged += CollectionChanged);
        }

        private void UnlistenToCSharpChanges(IJSCBridge ibridge)
        {
            _Root.ApplyOnListenable(n => n.PropertyChanged -= Object_PropertyChanged,
                                     c => c.CollectionChanged -= CollectionChanged);
        }

        public JSObject JSValueRoot { get { return _Root.JSValue; } }

        private void InjectInHTLMSession(IJSCBridge iroot, bool isroot = false)
        {
            if (iroot.Type != JSType.Object)
                return;

            _JavascriptSessionMapper.Map(iroot, new JavascriptMapper(iroot, this));
        }

        private void OnJavaScriptChanges(JSObject objectchanged, string PropertyName, JSValue newValue)
        {
            var res = GetFromJavascript(objectchanged) as JSGenericObject;
            if (res != null)
                res.UpdateCSharpProperty(PropertyName, newValue);
        }


        private void Object_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            string pn = e.PropertyName;

            PropertyInfo propertyInfo = sender.GetType().GetProperty(pn, BindingFlags.Public | BindingFlags.Instance);
            if (propertyInfo == null)
                return;

            JSGenericObject currentfather = _FromCSharp[sender] as JSGenericObject;
            object nv = propertyInfo.GetValue(sender, null);
            IJSCBridge oldbridgedchild = currentfather.Attributes[pn];

            if (Object.Equals(nv, oldbridgedchild.CValue))
                return;

            IJSCBridge newbridgedchild = _CSharpToJavascriptMapper.Map(nv);
            
            
            bool needrelisten = ListenToCSharp && ((newbridgedchild.Type == JSType.Object) ||
                                    (oldbridgedchild.Type == JSType.Object));

            WebCore.QueueWork(
                () =>
                {
                    InjectInHTLMSession(newbridgedchild);

                    using (IDisposable d = (needrelisten) ? ReListen() : null)
                    {
                        currentfather.Reroot(pn, newbridgedchild);
                        var el = (JSObject)currentfather.JSValue;
                        el.Invoke(pn, newbridgedchild.JSValue);
                    }
                });
        }

        private void Connect(IJSCBridge ibridged)
        {
            InjectInHTLMSession(ibridged);
        }

        private class ReListener : IDisposable
        {
            private BidirectionalMapper _BidirectionalMapper;
            public ReListener(BidirectionalMapper iBidirectionalMapper)
            {
                _BidirectionalMapper = iBidirectionalMapper;
                _BidirectionalMapper.UnlistenToCSharpChanges(_BidirectionalMapper._Root);
            }

            public void Dispose()
            {
                _BidirectionalMapper.ListenToCSharpChanges(_BidirectionalMapper._Root);
            }
        }

        private IDisposable ReListen()
        {
            return new ReListener(this);
        }

        private void CollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            JSArray arr = _FromCSharp[sender] as JSArray;
            if (arr == null)
                return;

            var el = (JSObject)arr.JSValue;

            switch (e.Action)
            {
                case NotifyCollectionChangedAction.Add:
                    IJSCBridge addvalue = _CSharpToJavascriptMapper.Map(e.NewItems[0]);

                    if (addvalue == null)
                        return;

                    WebCore.QueueWork(
                        () =>
                        {
                            using (ReListen())
                            {
                                InjectInHTLMSession(addvalue);
                                arr.Add(addvalue, e.NewStartingIndex);
                            }
                        });

                    break;

                case NotifyCollectionChangedAction.Remove:
                    WebCore.QueueWork(
                        () =>
                        {
                            using (ReListen())
                            {
                                arr.Remove(e.OldStartingIndex);
                            }
                        });

                    break;

                case NotifyCollectionChangedAction.Replace:
                    IJSCBridge newvalue = _CSharpToJavascriptMapper.Map(e.NewItems[0]);

                    if (newvalue == null)
                        return;

                    WebCore.QueueWork(
                        () =>
                        {
                            using (ReListen())
                            {
                                InjectInHTLMSession(newvalue);
                                arr.Insert(newvalue, e.NewStartingIndex);
                            }
                        });

                    break;

                case NotifyCollectionChangedAction.Reset:
                    WebCore.QueueWork(
                        () =>
                        {
                            using (ReListen())
                            {
                                arr.Reset();
                            }
                        });

                    break;
            }
        }

        public void Dispose()
        {
            if (ListenToCSharp)
                UnlistenToCSharpChanges(_Root);

            if (_JavascriptSessionMapper != null)
            {
                _JavascriptSessionMapper.Dispose();
                _JavascriptSessionMapper = null;
            }
        }

        void ICSharpMapper.Cache(object key, IJSCBridge value)
        {
            _FromCSharp.Add(key, value);
        }

        IJSCBridge ICSharpMapper.GetCached(object key)
        {
            IJSCBridge res = null;
            _FromCSharp.TryGetValue(key, out res);
            return res;
        }
    }
}
