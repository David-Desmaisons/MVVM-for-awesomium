using Awesomium.Core;
using System;
using System.Collections.Generic;

namespace MVVMAwesomium.AwesomiumBinding
{
    public interface IJSCBridge
    {
        JSValue JSValue { get; }

        object CValue { get;}

        JSType Type { get; }

        IEnumerable<IJSCBridge> GetChildren();
    }
}
