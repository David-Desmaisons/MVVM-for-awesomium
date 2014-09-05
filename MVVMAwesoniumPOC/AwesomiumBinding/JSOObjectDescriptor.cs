using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Awesomium.Core;

namespace MVVMAwesonium.AwesomiumBinding
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

        public IEnumerable<List<string>> GetPaths()
        {
            if (Father.Count == 0)
                yield return new List<string>();

            foreach (JSOObjectDescriptorFather f in Father)
            {
                foreach (List<string> p in f.Father.GetPaths())
                {
                    p.Add(f.Path);
                    yield return p;
                }
            }
        }

        public JSValue Value { get; set; }
        public List<JSOObjectDescriptorFather> Father { get; private set; }
    }
}
