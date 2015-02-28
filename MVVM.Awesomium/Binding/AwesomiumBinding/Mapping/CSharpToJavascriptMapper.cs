using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Awesomium.Core;
using System.Reflection;
using System.Collections;

using MVVMAwesomium.Infra;
using System.Windows.Input;
using System.Diagnostics;

namespace MVVMAwesomium.AwesomiumBinding
{
    internal class CSharpToJavascriptMapper 
    {
        private readonly IJSOLocalBuilder _IJSOBuilder;
        private readonly IJSCBridgeCache _Cacher;
        private readonly BasicCSharpToJavascriptConverter _Basic;
        public CSharpToJavascriptMapper(IJSOLocalBuilder Builder, IJSCBridgeCache icacher)
        {
            _IJSOBuilder = Builder;
            _Cacher = icacher;
            _Basic = new BasicCSharpToJavascriptConverter(Builder);
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

           
            JSValue value;
            if (_Basic.Solve(ifrom, out value))
            {
                return new JSBasicObject(value, ifrom);
            }

            if (ifrom.GetType().IsEnum)
            {
                var trueres = new JSBasicObject(_IJSOBuilder.CreateEnum((Enum)ifrom), ifrom);
                _Cacher.CacheLocal(ifrom, trueres);
                return trueres;
            }

            IEnumerable ienfro = ifrom as IEnumerable;
            if ((ienfro!=null) && Convert(ienfro, out res))
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
                object childvalue = null;
                try
                {
                    childvalue = propertyInfo.GetValue(ifrom, null); 
                }
                catch(Exception e)
                {
                    Trace.WriteLine(string.Format("MVVM for awesomium: Unable to convert property {0} from {1} exception {2}", pn, ifrom, e));
                    continue;
                }

                IJSCSGlue childres = Map(childvalue);

                resobject[pn] = childres.JSValue;
                gres.Attributes[pn] = childres;
            }

            return gres;
        }      
 
        private bool Convert(IEnumerable source, out IJSCSGlue res)
        {
            res = new JSArray(source.Cast<object>().Select(s => Map(s)), source, _Basic.GetElementType(source));
            _Cacher.Cache(source, res);
            return true;
        }
    }
}
