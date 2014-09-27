using Awesomium.Core;
using System;
using System.Collections.Generic;

namespace MVVMAwesomium.AwesomiumBinding
{
    public interface IJSCSGlue
    {
        JSValue JSValue { get; }

        object CValue { get;}

        JSCSGlueType Type { get; }

        IEnumerable<IJSCSGlue> GetChildren();
    }
}
