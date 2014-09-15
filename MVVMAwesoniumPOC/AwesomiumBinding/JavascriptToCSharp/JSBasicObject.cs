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
            if (CValue is string)
            {
                string s = CValue as string;
                return string.Format(@"""{0}""", s.Replace(@"""",@"\"""));
            }

            return CValue.ToString();
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
