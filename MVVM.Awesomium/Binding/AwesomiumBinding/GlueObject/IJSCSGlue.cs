using Awesomium.Core;
using System;
using System.Collections.Generic;
using System.Text;

namespace MVVMAwesomium.AwesomiumBinding
{
    public interface IJSCSGlue
    {
        JSValue JSValue { get; }

        object CValue { get;}

        JSCSGlueType Type { get; }

        IEnumerable<IJSCSGlue> GetChildren();

        void BuilString(StringBuilder sb, HashSet<IJSCSGlue> alreadyComputed);
    }
}
