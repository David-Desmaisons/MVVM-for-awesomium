using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Awesomium.Core;
using System.Reflection;
using System.Collections;

using MVVMAwesonium.Infra;

namespace MVVMAwesonium.AwesomiumBinding
{
    public class JavascriptObjectMapper : IMapperListener
    {
        private IBridgeObject _Root;
        private LocalBuilder _IJSOBuilder;
        private IDictionary<object, IBridgeObject> _Cached = new Dictionary<object, IBridgeObject>();
        private IDictionary<JSObject, IBridgeObject> _Reverted =
            new Dictionary<JSObject, IBridgeObject>(AwesomiumHelper.RemoteObjectComparer);

        public JavascriptObjectMapper()
        {
            _IJSOBuilder = new LocalBuilder();
        }

        public IBridgeObject Root { get { return _Root; } }

        public IDictionary<object, IBridgeObject> Objects
        {
            get { return _Cached; }
        }

        public IDictionary<JSObject, IBridgeObject> FromJSToCS
        {
            get { return _Reverted; }
        }

        public JSValue? GetValue(object root, string iPropertyName)
        {
            PropertyInfo propertyInfo = root.GetType().GetProperty(iPropertyName,BindingFlags.Public | BindingFlags.Instance);
            return (propertyInfo==null) ? new Nullable<JSValue>() :CreateLocalJSValue(propertyInfo.GetValue(root, null));
        }

        public void SetValue(object root, string iPropertyName,JSValue value)
        {
            PropertyInfo propertyInfo = root.GetType().GetProperty(iPropertyName, BindingFlags.Public | BindingFlags.Instance);
            propertyInfo.SetValue(root, value.GetSimpleValue(),null);
        }

        public JSValue CreateLocalJSValue(object ifrom)
        {
            _Root = DoConvert(ifrom);
            return _Root.JSValue;
        }

        private IBridgeObject DoConvert(object ifrom)
        {
            if (ifrom == null)
                return new JSGenericObject(JSValue.Null, ifrom);

            dynamic dfr = ifrom;
            JSValue value;
            if (Convert(dfr, out value))
            {
                return new JSGenericObject(value, ifrom);
            }

            IBridgeObject res = null;
            if (Convert(dfr, out res))
            {
                return res;
            }

            if (_Cached.TryGetValue(ifrom, out res))
            {
                return res;
            }

            JSObject resobject = _IJSOBuilder.CreateJSO();

            JSGenericObject gres = new JSGenericObject(new JSValue(resobject), ifrom);

            PropertyInfo[] propertyInfos = ifrom.GetType().GetProperties(BindingFlags.Public | BindingFlags.Instance);
            foreach (PropertyInfo propertyInfo in propertyInfos)
            {
                string pn = propertyInfo.Name;
                var child = DoConvert(propertyInfo.GetValue(ifrom, null));
                resobject[pn] = child.JSValue;
                gres.Children[pn]=child;
            }

            _Cached.Add(ifrom, gres);
            return gres;
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

        private bool Convert(decimal source, out JSValue res)
        {
            res = new JSValue((double)source);
            return true;
        }

        private bool Convert(bool source, out JSValue res)
        {
            res = new JSValue(source);
            return true;
        }

        private bool Convert(object source, out IBridgeObject res)
        {
            res = null;
            return false;
        }

        private bool Convert<T>(IEnumerable<T> source, out IBridgeObject res)
        {
            res = new JSArray(source.Select(s => DoConvert(s)), source);
            _Cached.Add(source, res);
            return true;
        }

        private bool Convert(IEnumerable source, out IBridgeObject res)
        {
            return Convert(source.Cast<object>(), out  res);
        }

        private void Update(IBridgeObject ibo, JSObject jsobject)
        {
            ibo.JSValue = jsobject;
            _Reverted[jsobject] = ibo;
        }

        public void RegisterFirst(JSObject iRoot)
        {
            Update(_Root, iRoot);
        }

        public void RegisterMapping(JSObject iFather, string att, JSObject iChild)
        {
            JSGenericObject jso = _Reverted[iFather] as JSGenericObject;
            Update( jso.Children[att], iChild);
        }

        public void RegisterCollectionMapping(JSObject iFather, string att, int index, JSObject iChild)
        {
            JSGenericObject jsof = _Reverted[iFather] as JSGenericObject;
            JSArray jsos = jsof.Children[att] as JSArray;
            Update(jsos.Children[index], iChild);
        }
    }
}
