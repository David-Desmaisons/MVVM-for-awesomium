using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Awesomium.Core;

namespace MVVMAwesonium.AwesomiumBinding
{
    public class JSBasicObject : IJSCBridge
    {
        public JSBasicObject(JSValue value, object icValue)
        {
            JSValue = value;
            CValue = icValue;
        }

        public override string ToString()
        {
            return string.Format("<Basic Object C#:{0}>",CValue);
        }

        public JSValue JSValue { get; set; }

        public object CValue { get; set; }

        public JSType Type { get { return JSType.Basic; } }

        public IEnumerable<IJSCBridge> GetChildren()
        {
            return Enumerable.Empty<IJSCBridge>();
        }
    }
}
