using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Awesomium.Core;

namespace MVVMAwesoniumPOC.AwesomiumBinding
{
    public class JSOObjectDescriptor
    {

        public JSOObjectDescriptor(JSOObjectDescriptorFather iFather)
        {
            Father = new List<JSOObjectDescriptorFather>();
            if (iFather != null)
                Father.Add(iFather);
        }

        public JSOObjectDescriptor(JSValue value, JSOObjectDescriptorFather iFather)
            : this(iFather)
        {
            Value = value;
        }

        public static string Concat(string f, string s)
        {
            if (string.IsNullOrEmpty(f))
                return s;

            if (s.StartsWith("["))
                return string.Format("{0}{1}", f, s);

            return string.Format("{0}.{1}",f,s);
        }

        public IEnumerable<string> GetPaths()
        {
            if (Father.Count == 0)
                yield return string.Empty;

            foreach (JSOObjectDescriptorFather f in Father)
            {
                foreach (string p in f.Father.GetPaths())
                {
                    yield return Concat( p, f.Path);
                }
            }
        }

        public JSValue Value { get; set; }
        public List<JSOObjectDescriptorFather> Father { get; private set; }
    }
}
