using Awesomium.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace MVVMAwesomium.AwesomiumBinding
{
    internal class JSArray : IJSObservableBridge
    {
        public JSArray(IEnumerable<IJSCBridge> values,object collection)
        {
            JSValue = new JSValue(values.Select(v=>v.JSValue).ToArray());
            Items = new List<IJSCBridge>(values);
            CValue = collection;
        }

        public void Add(IJSCBridge iIJSCBridge, int Index)
        {
            ((JSObject)MappedJSValue).Invoke("splice", new JSValue(Index), new JSValue(0), iIJSCBridge.GetSessionValue());
            if (Index > Items.Count - 1)
                Items.Add(iIJSCBridge);
            else
                Items.Insert(Index, iIJSCBridge);
        }

        public void Insert(IJSCBridge iIJSCBridge, int Index)
        {
            ((JSObject)MappedJSValue).Invoke("splice", new JSValue(Index), new JSValue(1), iIJSCBridge.GetSessionValue());
            Items[Index]=iIJSCBridge;
        }

        public void Remove( int Index)
        {
            ((JSObject)MappedJSValue).Invoke("splice", new JSValue(Index), new JSValue(1));
            Items.RemoveAt(Index);
        }

        public void Reset()
        {
            ((JSObject)MappedJSValue).Invoke("removeAll");
            Items.Clear();
        }

        public override string ToString()
        {
            return string.Format("[{0}]", string.Join(",", Items));
        }

        public JSValue JSValue { get; private set; }

        public object CValue { get; private set; }

        public IList<IJSCBridge> Items { get; private set; }

        public IEnumerable<IJSCBridge> GetChildren()
        {
            return Items;
        }

        public JSType Type { get { return JSType.Array; } }

        public JSValue MappedJSValue { get; private set; }

        public void SetMappedJSValue(JSValue ijsobject, ICSharpMapper mapper)
        {
            MappedJSValue = ijsobject;
        }
    }
}
