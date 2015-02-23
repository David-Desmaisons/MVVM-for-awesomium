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
using MVVMAwesomium.Infra;
using MVVMAwesomium.Exceptions;

namespace MVVMAwesomium.AwesomiumBinding
{
    public class BidirectionalMapper : IDisposable, IJSCBridgeCache, IJavascriptListener
    {
        private readonly JavascriptBindingMode _BindingMode;
        private readonly IJSCSGlue _Root;
        private readonly IWebView _IWebView;

        private CSharpToJavascriptMapper _JSObjectBuilder;
        private JavascriptSessionInjector _SessionInjector;
        private JavascriptToCSharpMapper _JavascriptToCSharpMapper;

        private IJSOLocalBuilder _LocalBuilder;
        private IJSOBuilder _GlobalBuilder;
        private bool _IsListening = false;

        private IDictionary<object, IJSCSGlue> _FromCSharp = new Dictionary<object, IJSCSGlue>();
        private IDictionary<uint, IJSCSGlue> _FromJavascript_Global = new Dictionary<uint, IJSCSGlue>();
        private IDictionary<uint, IJSCSGlue> _FromJavascript_Local = new Dictionary<uint, IJSCSGlue>();

        internal BidirectionalMapper(object iRoot, IWebView iwebview, JavascriptBindingMode iMode, object iadd)
        {
            _IWebView = iwebview;
            _LocalBuilder = new LocalBuilder(iwebview);
            _JSObjectBuilder = new CSharpToJavascriptMapper(_LocalBuilder, this);
            _JavascriptToCSharpMapper = new JavascriptToCSharpMapper(iwebview);
            _Root = _JSObjectBuilder.Map(iRoot, iadd);
            _BindingMode = iMode;

            IJavascriptListener JavascriptObjecChanges = null;
            if (iMode == JavascriptBindingMode.TwoWay)
                JavascriptObjecChanges = this;

            _GlobalBuilder = new GlobalBuilder(_IWebView, "MVVMGlue");

            _SessionInjector = new JavascriptSessionInjector(iwebview, _GlobalBuilder, JavascriptObjecChanges);
        }

        internal Task Init()
        {
            TaskCompletionSource<object> tcs = new TaskCompletionSource<object>();

            InjectInHTLMSession(_Root, true).ContinueWith(_ =>
                {
                    WebCore.QueueWork(() =>
                   {
                       if (ListenToCSharp)
                       {
                           ListenToCSharpChanges(_Root);
                       }
                       _IsListening = true;
                       tcs.SetResult(null);
                   });
                }
            );

            return tcs.Task;
        }

        #region IJavascriptMapper

        private class JavascriptMapper : IJavascriptMapper
        {
            private IJSObservableBridge _Root;
            private BidirectionalMapper _LiveMapper;
            private TaskCompletionSource<object> _TCS = new TaskCompletionSource<object>();
            public JavascriptMapper(IJSObservableBridge iRoot, BidirectionalMapper iFather)
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

        private IJSCSGlue GetFromJavascript(JSObject jsobject)
        {
            return _FromJavascript_Global[_GlobalBuilder.GetID(jsobject)];
        }

        private void Update(IJSObservableBridge ibo, JSObject jsobject)
        {
            ibo.SetMappedJSValue(jsobject, this);
            _FromJavascript_Global[_GlobalBuilder.GetID(jsobject)] = ibo;
        }

        public void RegisterMapping(JSObject iFather, string att, JSObject iChild)
        {
            JSGenericObject jso = GetFromJavascript(iFather) as JSGenericObject;
            Update(jso.Attributes[att] as IJSObservableBridge, iChild);
        }

        public void RegisterCollectionMapping(JSObject iFather, string att, int index, JSObject iChild)
        {
            JSGenericObject jsof = GetFromJavascript(iFather) as JSGenericObject;
            JSArray jsos = jsof.Attributes[att] as JSArray;
            Update(jsos.Items[index] as IJSObservableBridge, iChild);
        }

        #endregion

        public bool ListenToCSharp { get { return (_BindingMode != JavascriptBindingMode.OneTime); } }

        private void ListenToCSharpChanges(IJSCSGlue ibridge)
        {
            var list = new JSCBridgeListenableVisitor(n => n.PropertyChanged += Object_PropertyChanged,
                                     c => c.CollectionChanged += CollectionChanged, co => co.ListenChanges());
            _Root.ApplyOnListenable(list);
        }

        private void UnlistenToCSharpChanges(IJSCSGlue ibridge)
        {
            var list = new JSCBridgeListenableVisitor(n => n.PropertyChanged -= Object_PropertyChanged,
                           c => c.CollectionChanged -= CollectionChanged, co => co.UnListenChanges());

            _Root.ApplyOnListenable(list);
        }

        public IJSCSGlue JSValueRoot { get { return _Root; } }

        private Task InjectInHTLMSession(IJSCSGlue iroot, bool isroot = false)
        {
            if ((iroot == null) || (iroot.Type != JSCSGlueType.Object) && ((iroot.Type != JSCSGlueType.Command)))
            {
                return TaskHelper.Ended();
            }

            var jvm = new JavascriptMapper(iroot as IJSObservableBridge, this);
            var res = _SessionInjector.Map(iroot.JSValue, jvm,(iroot.CValue!=null));
            if (!isroot)
                return jvm.UpdateTask;
            else
                return jvm.UpdateTask.ContinueWith(_ => _SessionInjector.RegisterInSession(res),
                            TaskScheduler.FromCurrentSynchronizationContext());
        }

        public void OnJavaScriptObjectChanges(JSObject objectchanged, string PropertyName, JSValue newValue)
        {
            try
            {
                var res = GetFromJavascript(objectchanged) as JSGenericObject;
                if (res == null)
                    return;

                INotifyPropertyChanged inc = (!_IsListening)? null :res.CValue as INotifyPropertyChanged;
                if (inc != null) inc.PropertyChanged -= Object_PropertyChanged;
                res.UpdateCSharpProperty(PropertyName, this, newValue);
                if (inc != null) inc.PropertyChanged += Object_PropertyChanged;
            }
            catch (Exception e)
            {
                ExceptionHelper.Log(string.Format("Unable to update ViewModel from View, exception raised: {0}", e));
            }
        }

     
        public void OnJavaScriptCollectionChanges(JSObject collectionchanged, JSValue[] value, JSValue[] status, JSValue[] index)
        {
            try
            {
                var res = GetFromJavascript(collectionchanged) as JSArray;
                if (res == null) return;

                CollectionChanges cc = res.GetChanger(value, status, index,this);

                using (ReListen(null))
                {
                    INotifyCollectionChanged inc = res.CValue as INotifyCollectionChanged;
                    if (inc != null) inc.CollectionChanged -= CollectionChanged;
                    res.UpdateEventArgsFromJavascript(cc);
                    if (inc != null) inc.CollectionChanged += CollectionChanged;
                }
            }
            catch (Exception e)
            {
                ExceptionHelper.Log(string.Format("Unable to update ViewModel from View, exception raised: {0}", e));
            }
        }


        private void Object_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            string pn = e.PropertyName;

            PropertyInfo propertyInfo = sender.GetType().GetProperty(pn, BindingFlags.Public | BindingFlags.Instance);
            if (propertyInfo == null)
                return;

            JSGenericObject currentfather = _FromCSharp[sender] as JSGenericObject;

            object nv = propertyInfo.GetValue(sender, null);
            IJSCSGlue oldbridgedchild = currentfather.Attributes[pn];

            if (Object.Equals(nv, oldbridgedchild.CValue))
                return;

            IJSCSGlue newbridgedchild = _JSObjectBuilder.Map(nv);

            RegisterAndDo(newbridgedchild, () => currentfather.Reroot(pn, newbridgedchild));
        }

        #region Relisten

        private class ReListener : IDisposable
        {
            private HashSet<INotifyPropertyChanged> _OldObject = new HashSet<INotifyPropertyChanged>();
            private HashSet<INotifyCollectionChanged> _OldCollections = new HashSet<INotifyCollectionChanged>();
            private HashSet<JSCommand> _OldCommands = new HashSet<JSCommand>();

            private int _Count = 1;

            private BidirectionalMapper _BidirectionalMapper;
            public ReListener(BidirectionalMapper iBidirectionalMapper)
            {
                _BidirectionalMapper = iBidirectionalMapper;
                var list = new JSCBridgeListenableVisitor((e) => _OldObject.Add(e),
                    (e) => _OldCollections.Add(e), e => _OldCommands.Add(e));

                _BidirectionalMapper._Root.ApplyOnListenable(list);
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
                var new_Commands = new HashSet<JSCommand>();

                var list = new JSCBridgeListenableVisitor((e) => newObject.Add(e),
                                (e) => new_Collections.Add(e), e => new_Commands.Add(e));


                _BidirectionalMapper._Root.ApplyOnListenable(list);

                _OldObject.Where(o => !newObject.Contains(o)).ForEach(o => o.PropertyChanged -= _BidirectionalMapper.Object_PropertyChanged);
                newObject.Where(o => !_OldObject.Contains(o)).ForEach(o => o.PropertyChanged += _BidirectionalMapper.Object_PropertyChanged);

                _OldCollections.Where(o => !new_Collections.Contains(o)).ForEach(o => o.CollectionChanged -= _BidirectionalMapper.CollectionChanged);
                new_Collections.Where(o => !_OldCollections.Contains(o)).ForEach(o => o.CollectionChanged += _BidirectionalMapper.CollectionChanged);

                _OldCommands.Where(o => !new_Commands.Contains(o)).ForEach(o => o.UnListenChanges());
                new_Commands.Where(o => !_OldCommands.Contains(o)).ForEach(o => o.ListenChanges());

                _BidirectionalMapper._ReListen = null;
            }
        }

        private ReListener _ReListen = null;
        private IDisposable ReListen(IJSCSGlue ivalue)
        {
            if (_ReListen != null)
                _ReListen.AddRef();
            else
            {
                //((ivalue!=null) && (ivalue.Type==JSCSGlueType.Basic)) ? null :
                _ReListen = new ReListener(this);
            }

            return _ReListen;
        }

        #endregion

        private void CollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            WebCore.QueueWork(() =>
                {
                    UnsafeCollectionChanged(sender, e);
                });
        }

        private void UnsafeCollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            JSArray arr = _FromCSharp[sender] as JSArray;
            if (arr == null) return;

            switch (e.Action)
            {
                case NotifyCollectionChangedAction.Add:
                    IJSCSGlue addvalue = _JSObjectBuilder.Map(e.NewItems[0]);

                    if (addvalue == null) return;

                    RegisterAndDo(addvalue, () => arr.Add(addvalue, e.NewStartingIndex));
                    break;

                case NotifyCollectionChangedAction.Replace:
                    IJSCSGlue newvalue = _JSObjectBuilder.Map(e.NewItems[0]);

                    if (newvalue == null) return;

                    RegisterAndDo(newvalue, () => arr.Insert(newvalue, e.NewStartingIndex));
                    break;

                case NotifyCollectionChangedAction.Remove:
                    RegisterAndDo(null, () => arr.Remove(e.OldStartingIndex));
                    break;

                case NotifyCollectionChangedAction.Reset:
                    RegisterAndDo(null, () => arr.Reset());
                    break;
            }
        }

        private void RegisterAndDo(IJSCSGlue ivalue, Action Do)
        {
            var idisp = ReListen(ivalue);

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

        void IJSCBridgeCache.Cache(object key, IJSCSGlue value)
        {
            _FromCSharp.Add(key, value);
        }

        void IJSCBridgeCache.CacheLocal(object key, IJSCSGlue value)
        {
            _FromCSharp.Add(key, value);
            _FromJavascript_Local.Add(_LocalBuilder.GetID(value.JSValue), value);
        }

        IJSCSGlue IJSCBridgeCache.GetCached(object key)
        {
            IJSCSGlue res = null;
            _FromCSharp.TryGetValue(key, out res);
            return res;
        }

        public IJSCSGlue GetCached(JSObject globalkey)
        {
            if (!_GlobalBuilder.HasRelevantId(globalkey))
                return null;

            IJSCSGlue res = null;
            _FromJavascript_Global.TryGetValue(_GlobalBuilder.GetID(globalkey), out res);
            return res;
        }

        public IJSCSGlue GetCachedOrCreateBasic(JSValue globalkey,Type iTargetType)
        {
            IJSCSGlue res = null;
            JSObject obj = globalkey;

            //Use local cache for objet not created in javascript session such as enum
            if ((obj != null) &&  ((res = GetCached(globalkey) ?? GetCachedLocal(globalkey)) != null) )
                    return res;

            object targetvalue = _JavascriptToCSharpMapper.GetSimpleValue(globalkey, iTargetType);
            if ((targetvalue == null) && (!globalkey.IsNull))
                throw ExceptionHelper.Get(string.Format("Unable to convert javascript object: {0}", globalkey));

            return new JSBasicObject(globalkey, targetvalue);
        }

        private IJSCSGlue GetCachedLocal(JSObject localkey)
        {
            if (!_LocalBuilder.HasRelevantId(localkey))
                return null;

            IJSCSGlue res = null;
            _FromJavascript_Local.TryGetValue(_LocalBuilder.GetID(localkey), out res);
            return res;
        }



    }
}
