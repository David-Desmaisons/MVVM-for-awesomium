using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Awesomium.Core;
using System.Reflection;
using System.Collections;

namespace MVVMAwesoniumPOC.AwesomiumBinding
{
    

    public class ConvertToJSO
    {
        private IDictionary<object, JSOObjectDescriptor> _Cached = new Dictionary<object, JSOObjectDescriptor>();
        private IJSOBuilder _IJSOBuilder;

        public ConvertToJSO(IJSOBuilder iJSOBuilder)
        {
            _IJSOBuilder = iJSOBuilder;
        }

        public IDictionary<object, JSOObjectDescriptor> Objects
        {
            get { return _Cached; }
        }


        public JSValue GetValue(object root, string iPropertyName)
        {
            PropertyInfo propertyInfo = root.GetType().GetProperty(iPropertyName,BindingFlags.Public | BindingFlags.Instance);
            return Convert(propertyInfo.GetValue(root, null));
        }

        private JSOObjectDescriptor DoConvert(object ifrom, JSOObjectDescriptorFather father = null)
        {
            if (ifrom == null)
                return new JSOObjectDescriptor(JSValue.Null, father);

            dynamic dfr = ifrom;
            JSValue value;
            if (Convert(dfr, father, out value))
            {
                return new JSOObjectDescriptor(value, father);
            }

            JSOObjectDescriptor res=null;
            if (_Cached.TryGetValue(ifrom, out res))
            {
                res.Father.Add(father);
                return res;
            }

            JSObject resobject = _IJSOBuilder.CreateJSO();

            res = new JSOObjectDescriptor(new JSValue(resobject), father);

            PropertyInfo[] propertyInfos = ifrom.GetType().GetProperties(BindingFlags.Public | BindingFlags.Instance);
            foreach (PropertyInfo propertyInfo in propertyInfos)
            {
                resobject[propertyInfo.Name] = DoConvert(propertyInfo.GetValue(ifrom, null), new JSOObjectDescriptorFather(res, propertyInfo.Name)).Value;
            }

            _Cached.Add(ifrom, res);
            return res;
        }


        public JSValue Convert(object ifrom)
        {
            return DoConvert(ifrom).Value;
        }

        private bool Convert(object source, JSOObjectDescriptorFather path, out JSValue res)
        {
            res = new JSValue();
            return false;
        }

        private bool Convert(string source, JSOObjectDescriptorFather path, out JSValue res)
        {
            res = new JSValue(source);
            return true;
        }

        private bool Convert(int source, JSOObjectDescriptorFather path, out JSValue res)
        {
            res = new JSValue(source);
            return true;
        }

        private bool Convert(double source, JSOObjectDescriptorFather path, out JSValue res)
        {
            res = new JSValue(source);
            return true;
        }

        private bool Convert(decimal source, JSOObjectDescriptorFather path, out JSValue res)
        {
            res = new JSValue((double)source);
            return true;
        }

        private bool Convert(bool source, JSOObjectDescriptorFather path, out JSValue res)
        {
            res = new JSValue(source);
            return true;
        }

        private bool Convert<T>(IEnumerable<T> source, JSOObjectDescriptorFather path, out JSValue res)
        {
            var structres = new JSOObjectDescriptor(path);
            int i=0;
            var ind = source.Select(s => DoConvert(s, new JSOObjectDescriptorFather(structres, string.Format("{0}",i++)))).ToList();
            res = new JSValue(ind.Select(des => des.Value).ToArray());
            structres.Value = res;
            _Cached.Add(source, structres);
            return true;
        }

        private bool Convert(IEnumerable source, JSOObjectDescriptorFather path, out JSValue res)
        {
            return Convert(source.Cast<object>(), path, out  res);
        }
    }
}
