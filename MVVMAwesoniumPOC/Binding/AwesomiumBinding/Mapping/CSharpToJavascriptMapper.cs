using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Awesomium.Core;
using System.Reflection;
using System.Collections;

using MVVMAwesomium.Infra;
using System.Windows.Input;

namespace MVVMAwesomium.AwesomiumBinding
{
    internal class CSharpToJavascriptMapper 
    {
        private readonly IJSOLocalBuilder _IJSOBuilder;
        private readonly IJSCBridgeCache _Cacher;
        public CSharpToJavascriptMapper(IJSOLocalBuilder Builder, IJSCBridgeCache icacher)
        {
            _IJSOBuilder = Builder;
            _Cacher = icacher;
        }

        internal IJSCSGlue Map(object ifrom, object iadditional=null)
        {
            if (ifrom == null)
                return new JSGenericObject(_IJSOBuilder.CreateNull(), ifrom);

            IJSCSGlue res = null;
            res = _Cacher.GetCached(ifrom);
            if (res != null)
            {
                return res;
            }

            if (ifrom is ICommand)
                return new JSCommand(_IJSOBuilder, ifrom as ICommand);

            dynamic dfr = ifrom;
            JSValue value;
            if (BasicConvert(dfr, out value))
            {
                return new JSBasicObject(value, ifrom);
            }

            if (ConvertWithCahe(dfr, out value))
            {
                var trueres = new JSBasicObject(value, ifrom);
                _Cacher.CacheLocal(ifrom, trueres);
                return trueres;
            }
          
            if (Convert(dfr, out res))
            {
                return res;
            }

            JSObject resobject = _IJSOBuilder.CreateJSO();
   
            JSGenericObject gres = new JSGenericObject(new JSValue(resobject), ifrom);
            _Cacher.Cache(ifrom, gres);

            MappNested(ifrom, resobject,gres);
            MappNested(iadditional, resobject, gres);

            return gres;
        }

        private JSGenericObject MappNested(object ifrom, JSObject resobject, JSGenericObject gres)
        {
            if (ifrom == null)
                return gres;

            IEnumerable<PropertyInfo> propertyInfos = ifrom.GetType().GetProperties(BindingFlags.Public | BindingFlags.Instance);

            foreach (PropertyInfo propertyInfo in propertyInfos.Where(p => p.CanRead))
            {
                string pn = propertyInfo.Name;
                var childvalue = propertyInfo.GetValue(ifrom, null);

                IJSCSGlue childres = Map(childvalue);

                resobject[pn] = childres.JSValue;
                gres.Attributes[pn] = childres;
            }

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

        private bool BasicConvert(DateTime source, out JSValue res)
        {
            res = _IJSOBuilder.CreateDate(source);
            return true;
        }

        private bool ConvertWithCahe(Enum source, out JSValue res)
        {
            res = _IJSOBuilder.CreateEnum(source);
            return true;
        }

        private bool ConvertWithCahe(object source, out JSValue res)
        {
            res = new JSValue();
            return false;
        }

        private bool Convert(object source, out IJSCSGlue res)
        {
            res = null;
            return false;
        }

        private bool Convert<T>(IEnumerable<T> source, out IJSCSGlue res)
        {
            res = new JSArray(source.Select(s => Map(s)), source);
            _Cacher.Cache(source, res);
            return true;
        }

        private bool Convert(IEnumerable source, out IJSCSGlue res)
        {
            return Convert(source.Cast<object>(), out  res);
        }
    }
}
