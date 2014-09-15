using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Awesomium.Core;
using System.Reflection;
using System.Collections;

using MVVMAwesonium.Infra;
using System.Windows.Input;

namespace MVVMAwesonium.AwesomiumBinding
{
    internal class CSharpToJavascriptMapper 
    {
        private readonly IJSOBuilder _IJSOBuilder;
        private readonly ICSharpMapper _Cacher;
        public CSharpToJavascriptMapper(IJSOBuilder Builder, ICSharpMapper icacher)
        {
            _IJSOBuilder = Builder;
            _Cacher = icacher;
        }

        internal IJSCBridge Map(object ifrom)
        {
            if (ifrom == null)
                return new JSGenericObject(_IJSOBuilder.CreateJSO(), ifrom);

            dynamic dfr = ifrom;
            JSValue value;
            if (BasicConvert(dfr, out value))
            {
                return new JSBasicObject(value, ifrom);
            }

            IJSCBridge res = null;
            if (Convert(dfr, out res))
            {
                return res;
            }

            res = _Cacher.GetCached(ifrom);
            if (res!=null)
            {
                return res;
            }

            JSObject resobject = _IJSOBuilder.CreateJSO();
   
            JSGenericObject gres = new JSGenericObject(new JSValue(resobject), ifrom);

            PropertyInfo[] propertyInfos = ifrom.GetType().GetProperties(BindingFlags.Public | BindingFlags.Instance);
            foreach (PropertyInfo propertyInfo in propertyInfos.Where(p => p.CanRead))
            {
                string pn = propertyInfo.Name;
                var childvalue = propertyInfo.GetValue(ifrom, null);

                if (!(childvalue is ICommand))
                { 
                    var child = Map(childvalue);
                    resobject[pn] = child.JSValue;
                    gres.Attributes[pn]=child;
                }
                else
                {
                    gres.Commands[pn] = childvalue as ICommand;
                }
            }
      
            _Cacher.Cache(ifrom, gres);
            return gres;
        }


        private bool BasicConvert(object source, out JSValue res)
        {
            res = new JSValue();
            return false;
        }

        private bool BasicConvert(string source, out JSValue res)
        {
            res = new JSValue(source);
            return true;
        }

        private bool BasicConvert(int source, out JSValue res)
        {
            res = new JSValue(source);
            return true;
        }

        private bool BasicConvert(double source, out JSValue res)
        {
            res = new JSValue(source);
            return true;
        }

        private bool BasicConvert(decimal source, out JSValue res)
        {
            res = new JSValue((double)source);
            return true;
        }

        private bool BasicConvert(bool source, out JSValue res)
        {
            res = new JSValue(source);
            return true;
        }

        private bool Convert(object source, out IJSCBridge res)
        {
            res = null;
            return false;
        }

        private bool Convert<T>(IEnumerable<T> source, out IJSCBridge res)
        {
            res = new JSArray(source.Select(s => Map(s)), source);
            _Cacher.Cache(source, res);
            return true;
        }

        private bool Convert(IEnumerable source, out IJSCBridge res)
        {
            return Convert(source.Cast<object>(), out  res);
        }
    }
}
