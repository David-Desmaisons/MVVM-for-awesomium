using Awesomium.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace MVVMAwesonium.AwesomiumBinding
{
    internal class JSBasicObject : IJSCBridge
    {
        public JSBasicObject(JSValue value, object icValue)
        {
            JSValue = value;
            CValue = icValue;
        }
        public JSValue JSValue { get; set; }

        public object CValue { get; set; }

        public IEnumerable<IJSCBridge> GetChildren()
        {
            return Enumerable.Empty<IJSCBridge>();
        }

        public JSType Type { get { return JSType.Basic; } }
    }
}
