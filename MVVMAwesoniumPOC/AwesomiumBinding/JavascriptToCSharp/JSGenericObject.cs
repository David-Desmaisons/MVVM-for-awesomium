using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Awesomium.Core;
using System.Reflection;

namespace MVVMAwesonium.AwesomiumBinding
{
    public class JSGenericObject : IJSCInjectableBridge
    {
        public JSGenericObject(JSValue value, object icValue)
        {
            JSValue = value;
            CValue = icValue;
        }

        public override string ToString()
        {
            return string.Format("<Object C#:{0}>",CValue);
        }

        private Dictionary<string, IJSCBridge> _Attributes = new Dictionary<string, IJSCBridge>();

        public IDictionary<string, IJSCBridge> Attributes { get { return _Attributes; } }

        public JSValue JSValue { get; private set; }

        public JSValue MappedJSValue { get; set; }

        public object CValue { get; private set; }

        public JSType Type { get { return JSType.Object; } }

        public IEnumerable<IJSCBridge> GetChildren()
        {
            return _Attributes.Values; 
        }

        public void UpdateCSharpProperty(string PropertyName, JSValue newValue)
        {
            var simplevalue = newValue.GetSimpleValue();
            if (simplevalue == null)
                return;

            if (Object.Equals(simplevalue, _Attributes[PropertyName].CValue))
                return;

            PropertyInfo propertyInfo = CValue.GetType().GetProperty(PropertyName, BindingFlags.Public | BindingFlags.Instance);
           
            _Attributes[PropertyName] = new JSBasicObject(newValue, simplevalue); 
            propertyInfo.SetValue(CValue, simplevalue, null);
        }

        public void Reroot(string PropertyName, IJSCBridge newValue)
        { 
            _Attributes[PropertyName]=newValue;
            ((JSObject)MappedJSValue).Invoke(PropertyName, newValue.GetSessionValue() );    
        }
    }
}
