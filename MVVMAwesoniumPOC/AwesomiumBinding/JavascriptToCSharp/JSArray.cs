using Awesomium.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace MVVMAwesonium.AwesomiumBinding
{
    internal class JSArray : IBridgeObject
    {
        public JSArray(IEnumerable<IBridgeObject> values,object collection)
        {
            JSValue = new JSValue(values.Select(v=>v.JSValue).ToArray());
            Children = new List<IBridgeObject>(values);
            CValue = collection;
        }

        public JSValue JSValue { get; set; }

        public object CValue { get; set; }

        public IList<IBridgeObject> Children { get; private set; }

        public IEnumerable<IBridgeObject> GetChildren()
        {
            return Children;
        }
    }
}
