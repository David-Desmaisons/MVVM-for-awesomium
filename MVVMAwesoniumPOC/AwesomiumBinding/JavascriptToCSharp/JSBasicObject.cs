using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Awesomium.Core;

namespace MVVMAwesonium.AwesomiumBinding
{
    internal class JSBasicObject : IJSCBridge
    {
        internal JSBasicObject(JSValue value, object icValue)
        {
            JSValue = value;
            CValue = icValue;
        }

        public override string ToString()
        {
            return string.Format("<Basic Object C#:{0}>",CValue);
        }

        public JSValue JSValue { get; private set; }

        public object CValue { get; private set; }

        public JSType Type { get { return JSType.Basic; } }

        public IEnumerable<IJSCBridge> GetChildren()
        {
            return Enumerable.Empty<IJSCBridge>();
        }
    }
}
