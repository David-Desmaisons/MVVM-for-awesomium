using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Awesomium.Core;
using System.Reflection;

namespace MVVMAwesoniumPOC.AwesomiumBinding
{
    public class ConvertToJSO
    {
        public IDictionary<object, JSValue> _Cached = new Dictionary<object, JSValue>();
        private IJSOBuilder _IJSOBuilder;

        public ConvertToJSO(IJSOBuilder iJSOBuilder)
        {
            _IJSOBuilder = iJSOBuilder;
        }

        public JSValue Convert(object ifrom)
        {
            if (ifrom == null)
                return JSValue.Null;

            JSValue res;

            dynamic dfr = ifrom;
            JSValue value;
            if (Convert(dfr, out value))
            {
                return value;
            }

            if (_Cached.TryGetValue(ifrom, out res))
                return res;

            JSObject resobject = _IJSOBuilder.CreateJSO();
            res = new JSValue(resobject);

            PropertyInfo[] propertyInfos = ifrom.GetType().GetProperties(BindingFlags.Public | BindingFlags.Instance);
            foreach (PropertyInfo propertyInfo in propertyInfos)
            {
                resobject[propertyInfo.Name] = Convert(propertyInfo.GetValue(ifrom, null));
            }

            _Cached.Add(ifrom, res);
            return res;
        }

        private bool Convert(object source, out JSValue res)
        {
            res = new JSValue();
            return false;
        }

        private bool Convert(string source, out JSValue res)
        {
            res = new JSValue(source);
            return true;
        }

        private bool Convert(int source, out JSValue res)
        {
            res = new JSValue(source);
            return true;
        }

        private bool Convert(double source, out JSValue res)
        {
            res = new JSValue(source);
            return true;
        }

        private bool Convert(bool source, out JSValue res)
        {
            res = new JSValue(source);
            return true;
        }

        private bool Convert<T>(IEnumerable<T> source, out JSValue res)
        {
            res = new JSValue(source.Select( s=> Convert(s)).ToArray() );
            return true;
        }
    }
}
