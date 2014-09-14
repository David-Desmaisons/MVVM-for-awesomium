using Awesomium.Core;
using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using MVVMAwesonium.Infra;

namespace MVVMAwesonium.AwesomiumBinding
{
    public class BidirectionalMapper : IDisposable, ICSharpMapper
    {
        private readonly JavascriptBindingMode _BindingMode;
        private readonly IJSCBridge _Root;
        private CSharpToJavascriptMapper _JSObjectBuilder;
        private JavascriptSessionInjector _SessionInjector;
        private IDictionary<object, IJSCBridge> _FromCSharp = new Dictionary<object, IJSCBridge>();
        private IDictionary<uint, IJSCBridge> _FromJavascript = new Dictionary<uint, IJSCBridge>();

        internal BidirectionalMapper(object iRoot, IWebView iwebview, JavascriptBindingMode iMode)
        {
            _JSObjectBuilder = new CSharpToJavascriptMapper(new LocalBuilder(), this);
            _Root = _JSObjectBuilder.Map(iRoot);
            _BindingMode = iMode;

            Action<JSObject, string, JSValue> JavascriptObjecChanges = null;
            if (iMode == JavascriptBindingMode.TwoWay)
                JavascriptObjecChanges = OnJavaScriptChanges;

            _SessionInjector = new JavascriptSessionInjector(iwebview, JavascriptObjecChanges);      
        }

        internal Task Init()
        {
            return InjectInHTLMSession(_Root, true).ContinueWith(_ =>
                {
                    if (ListenToCSharp)
                    {
                        ListenToCSharpChanges(_Root);
                    }
                }
            );
        }

        #region IJavascriptMapper

        private class JavascriptMapper : IJavascriptMapper
        {
            private IJSCInjectableBridge _Root;
            private BidirectionalMapper _LiveMapper;
            private TaskCompletionSource<object> _TCS = new TaskCompletionSource<object>();
            public JavascriptMapper(IJSCInjectableBridge iRoot, BidirectionalMapper iFather)
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

            internal Task UpdateTask { get { return _TCS.Task; } }

            public void End(JSObject iRoot)
            {
                _TCS.TrySetResult(null);
            }
        }

        private IJSCBridge GetFromJavascript(JSObject jsobject)
        {
            return _FromJavascript[jsobject.RemoteId];
        }

        private void Update(IJSCInjectableBridge ibo, JSObject jsobject)
        {
            ibo.MappedJSValue = jsobject;
            _FromJavascript[jsobject.RemoteId] = ibo;
        }

        public void RegisterMapping(JSObject iFather, string att, JSObject iChild)
        {
            JSGenericObject jso = GetFromJavascript(iFather) as JSGenericObject;
            Update(jso.Attributes[att] as IJSCInjectableBridge, iChild);
        }

        public void RegisterCollectionMapping(JSObject iFather, string att, int index, JSObject iChild)
        {
            JSGenericObject jsof = GetFromJavascript(iFather) as JSGenericObject;
            JSArray jsos = jsof.Attributes[att] as JSArray;
            Update(jsos.Items[index] as IJSCInjectableBridge, iChild);
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

        private Task InjectInHTLMSession(IJSCBridge iroot, bool isroot = false)
        {
            if ((iroot==null) || (iroot.Type != JSType.Object))
            {
                return TaskHelper.FromResult<object>(null);
            }

            var jvm = new JavascriptMapper(iroot as IJSCInjectableBridge, this);
            _SessionInjector.Map(iroot, jvm, isroot);
            return jvm.UpdateTask;
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

            IJSCBridge newbridgedchild = _JSObjectBuilder.Map(nv);

            RegisterAndDo(newbridgedchild, () => currentfather.Reroot(pn, newbridgedchild));
        }

        #region Relisten

        private class ReListener : IDisposable
        {
            private HashSet<INotifyPropertyChanged> _OldObject = new HashSet<INotifyPropertyChanged>();
            private HashSet<INotifyCollectionChanged> _OldCollections = new HashSet<INotifyCollectionChanged>();
            private int _Count = 1;

            private BidirectionalMapper _BidirectionalMapper;
            public ReListener(BidirectionalMapper iBidirectionalMapper)
            {
                _BidirectionalMapper = iBidirectionalMapper;
                _BidirectionalMapper._Root.ApplyOnListenable((e) => _OldObject.Add(e), (e) => _OldCollections.Add(e));
            }

            public void AddRef()
            {
                _Count++;
            }

            public void Dispose()
            {
                if (--_Count == 0)
                {
                    Clean();
                }
            }

            private void Clean()
            {
                var newObject = new HashSet<INotifyPropertyChanged>();
                var new_Collections = new HashSet<INotifyCollectionChanged>();

                _BidirectionalMapper._Root.ApplyOnListenable((e) => newObject.Add(e), (e) => new_Collections.Add(e));

                _OldObject.Where(o => !newObject.Contains(o)).ForEach(o => o.PropertyChanged -= _BidirectionalMapper.Object_PropertyChanged);
                newObject.Where(o => !_OldObject.Contains(o)).ForEach(o => o.PropertyChanged += _BidirectionalMapper.Object_PropertyChanged);

                _OldCollections.Where(o => !new_Collections.Contains(o)).ForEach(o => o.CollectionChanged -= _BidirectionalMapper.CollectionChanged);
                new_Collections.Where(o => !_OldCollections.Contains(o)).ForEach(o => o.CollectionChanged += _BidirectionalMapper.CollectionChanged);

                _BidirectionalMapper._ReListen = null;
            }
        }

        private ReListener _ReListen = null;
        private IDisposable ReListen()
        {
            if (_ReListen!=null)
                _ReListen.AddRef();
            else
                _ReListen = new ReListener(this);

            return _ReListen;
        }

        #endregion

        private void CollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            JSArray arr = _FromCSharp[sender] as JSArray;
            if (arr == null)
                return;

            switch (e.Action)
            {
                case NotifyCollectionChangedAction.Add:
                    IJSCBridge addvalue = _JSObjectBuilder.Map(e.NewItems[0]);

                    if (addvalue == null)
                        return;

                    RegisterAndDo(addvalue, () => arr.Add(addvalue, e.NewStartingIndex));
                    break;

                case NotifyCollectionChangedAction.Replace:
                    IJSCBridge newvalue = _JSObjectBuilder.Map(e.NewItems[0]);

                    if (newvalue == null)
                        return;

                    RegisterAndDo(newvalue, () => arr.Insert(newvalue, e.NewStartingIndex) );
                    break;

                case NotifyCollectionChangedAction.Remove:
                    RegisterAndDo(null, () => arr.Remove(e.OldStartingIndex));
                    break;

                case NotifyCollectionChangedAction.Reset:
                    RegisterAndDo(null, () => arr.Reset());
                    break;
            }
        }

        private void RegisterAndDo(IJSCBridge ivalue, Action Do)
        {
            var idisp = ReListen();

            InjectInHTLMSession(ivalue).ContinueWith(_ =>
                WebCore.QueueWork(() =>
                    {
                        using (idisp)
                        {
                            Do();
                        }
                    }
                ));
        }

        public void Dispose()
        {
            if (ListenToCSharp)
                UnlistenToCSharpChanges(_Root);

            if (_SessionInjector != null)
            {
                _SessionInjector.Dispose();
                _SessionInjector = null;
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
