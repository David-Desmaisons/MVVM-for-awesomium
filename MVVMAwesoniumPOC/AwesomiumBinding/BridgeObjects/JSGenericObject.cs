using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Awesomium.Core;
using System.Reflection;
using System.Windows.Input;

using MVVMAwesomium.Infra;

namespace MVVMAwesomium.AwesomiumBinding
{
    public class JSGenericObject : IJSObservableBridge
    {
        public JSGenericObject(JSValue value, object icValue)
        {
            JSValue = value;
            CValue = icValue;
        }

        public override string ToString()
        {
            return "{"+ string.Join(",", _Attributes.Where(kvp=>kvp.Value.Type!=JSBridgeType.Command).Select(kvp=>string.Format(@"""{0}"":{1}",kvp.Key,kvp.Value)))+"}";
        }

        private Dictionary<string, IJSCBridge> _Attributes = new Dictionary<string, IJSCBridge>();

        public IDictionary<string, IJSCBridge> Attributes { get { return _Attributes; } }

        public JSValue JSValue { get; private set; }

        private JSValue _MappedJSValue;

        public JSValue MappedJSValue { get { return _MappedJSValue; } }

        public void SetMappedJSValue(JSValue ijsobject, IJSCBridgeCache mapper)
        {
            _MappedJSValue = ijsobject;
        }

        public object CValue { get; private set; }

        public JSBridgeType Type { get { return JSBridgeType.Object; } }

        public IEnumerable<IJSCBridge> GetChildren()
        {
            return _Attributes.Values; 
        }

        public void UpdateCSharpProperty(string PropertyName, JSValue newValue, object simplevalue)
        {
            if ((simplevalue == null) || (Object.Equals(simplevalue, _Attributes[PropertyName].CValue)))
                return;

            PropertyInfo propertyInfo = CValue.GetType().GetProperty(PropertyName, BindingFlags.Public | BindingFlags.Instance);
            if (!propertyInfo.CanWrite)
                return;
           
            _Attributes[PropertyName] = new JSBasicObject(newValue, simplevalue); 
            propertyInfo.SetValue(CValue, simplevalue, null);
        }

        public void Reroot(string PropertyName, IJSCBridge newValue)
        { 
            _Attributes[PropertyName]=newValue;
            ((JSObject)_MappedJSValue).Invoke(PropertyName, newValue.GetJSSessionValue());    
        }     
    }
}
