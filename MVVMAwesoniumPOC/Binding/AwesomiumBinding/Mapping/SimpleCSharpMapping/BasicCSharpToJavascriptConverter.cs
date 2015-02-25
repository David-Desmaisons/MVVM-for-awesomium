using Awesomium.Core;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using MVVMAwesomium.Infra;

namespace MVVMAwesomium.AwesomiumBinding
{
    internal class BasicCSharpToJavascriptConverter
    {
        private IJSOLocalBuilder _IJSOBuilder;
        internal BasicCSharpToJavascriptConverter(IJSOLocalBuilder iIJSOBuilder)
        {
            _IJSOBuilder = iIJSOBuilder;
        }

        private static IDictionary<Type, Func<object, IJSOLocalBuilder, JSValue>> _Converters = new Dictionary<Type, Func<object, IJSOLocalBuilder, JSValue>>();
        private static void Register<T>(Func<T, IJSOLocalBuilder, JSValue> Factory)
        {
            _Converters.Add(typeof(T), (o, b) => Factory((T)o,b));
        }

        public Type GetElementType(IEnumerable collection)
        {
            var typeo = collection.GetType();
            var elementtype = typeo.GetEnumerableBase();
            if (elementtype == null)
                return null;
            var almost = elementtype.GetUnderlyingNullableType() ?? elementtype;
            return _Converters.ContainsKey(almost) ? almost : null;
        }

        static BasicCSharpToJavascriptConverter()
        {
            Register<string>((source,b)=>new JSValue(source));
            Register<Int64>((source, b) => new JSValue(source));
            Register<Int32>((source,b)=>new JSValue(source));
            Register<Int16>((source, b) => new JSValue((int)source));
            Register<UInt64>((source, b) => new JSValue(source));
            Register<UInt32>((source, b) => new JSValue(source));
            Register<UInt16>((source, b) => new JSValue((int)source));
            Register<float>((source, b) => new JSValue(source));
            Register<char>((source, b) => new JSValue(source));
            Register<double>((source, b) => new JSValue(source));
            Register<decimal>((source, b) => new JSValue((double)source));
            Register<bool>((source, b) => new JSValue(source));
            Register<DateTime>((source, builder) => builder.CreateDate(source));
        }

        public bool Solve(object ifrom,out JSValue res)
        {
            Func<object, IJSOLocalBuilder, JSValue> conv=null;
            if (!_Converters.TryGetValue(ifrom.GetType(),out conv))
            {
                res = new JSValue();
                return false;
            }

            res = conv(ifrom, _IJSOBuilder);
            return true;
        }
    }
}
