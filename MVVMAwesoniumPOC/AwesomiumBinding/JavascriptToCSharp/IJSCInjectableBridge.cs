using Awesomium.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace MVVMAwesonium.AwesomiumBinding
{
    public interface IJSCInjectableBridge : IJSCBridge
    {
        JSValue MappedJSValue { get; set; }
    }

}
