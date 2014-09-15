using Awesomium.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace MVVMAwesonium.AwesomiumBinding
{
    public interface ICSharpMapper
    {
        void Cache(object key, IJSCBridge value);

        IJSCBridge GetCached(object key);

        IJSCBridge GetCached(JSObject key);
    }
}
