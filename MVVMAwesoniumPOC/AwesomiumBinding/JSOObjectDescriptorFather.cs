using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace MVVMAwesonium.AwesomiumBinding
{
    public class JSOObjectDescriptorFather
    {
        public JSOObjectDescriptorFather(JSOObjectDescriptor iFather, string path)
        {
            Father = iFather;
            Path = path;
        }

        public string Path { get; private set; }
        public JSOObjectDescriptor Father { get; private set; }
    }
}
