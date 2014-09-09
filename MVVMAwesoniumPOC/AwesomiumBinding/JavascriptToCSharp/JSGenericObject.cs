using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Awesomium.Core;

namespace MVVMAwesonium.AwesomiumBinding
{
    public class JSGenericObject : IBridgeObject
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

        private Dictionary<string, IBridgeObject> _Children = new Dictionary<string, IBridgeObject>();

        public IDictionary<string, IBridgeObject> Children { get { return _Children; } }

        public JSValue JSValue { get; set; }

        public object CValue { get; set; }

        public IEnumerable<IBridgeObject> GetChildren()
        {
            return _Children.Values; 
        }
    }
}
