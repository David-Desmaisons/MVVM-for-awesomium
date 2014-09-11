using Awesomium.Core;
using System;
using System.Collections.Generic;

namespace MVVMAwesonium.AwesomiumBinding
{
    public interface IJSCBridge
    {
        JSValue JSValue { get; set; }

        object CValue { get; set; }

        JSType Type { get; }

        IEnumerable<IJSCBridge> GetChildren();
    }
}
