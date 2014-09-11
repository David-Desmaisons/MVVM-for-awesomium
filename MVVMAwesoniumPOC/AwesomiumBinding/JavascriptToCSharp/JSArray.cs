using Awesomium.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace MVVMAwesonium.AwesomiumBinding
{
    internal class JSArray : IJSCBridge
    {
        public JSArray(IEnumerable<IJSCBridge> values,object collection)
        {
            JSValue = new JSValue(values.Select(v=>v.JSValue).ToArray());
            Items = new List<IJSCBridge>(values);
            CValue = collection;
        }

        public void Add(IJSCBridge iIJSCBridge, int Index)
        {
            ((JSObject)JSValue).Invoke("splice", new JSValue(Index), new JSValue(0), iIJSCBridge.JSValue);
            if (Index > Items.Count - 1)
                Items.Add(iIJSCBridge);
            else
                Items.Insert(Index, iIJSCBridge);
        }

        public void Insert(IJSCBridge iIJSCBridge, int Index)
        {
            ((JSObject)JSValue).Invoke("splice", new JSValue(Index), new JSValue(1), iIJSCBridge.JSValue);
            Items[Index]=iIJSCBridge;
        }

        public void Remove( int Index)
        {
            ((JSObject)JSValue).Invoke("splice", new JSValue(Index), new JSValue(1));
            Items.RemoveAt(Index);
        }

        public JSValue JSValue { get; set; }

        public object CValue { get; set; }

        public IList<IJSCBridge> Items { get; private set; }

        public IEnumerable<IJSCBridge> GetChildren()
        {
            return Items;
        }

        public JSType Type { get { return JSType.Array; } }
    }
}
