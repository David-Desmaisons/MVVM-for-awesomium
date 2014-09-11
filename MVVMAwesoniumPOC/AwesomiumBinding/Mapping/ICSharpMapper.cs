using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace MVVMAwesonium.AwesomiumBinding
{
    internal interface ICSharpMapper
    {
        void Cache(object key, IJSCBridge value);

        IJSCBridge GetCached(object key);
    }
}
