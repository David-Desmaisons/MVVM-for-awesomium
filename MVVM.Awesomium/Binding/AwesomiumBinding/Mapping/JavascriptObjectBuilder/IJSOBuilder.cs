using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Awesomium.Core;

namespace MVVMAwesomium.AwesomiumBinding
{
    public interface IJSOBuilder
    {
        JSObject CreateJSO();

        uint GetID(JSObject iJSObject);

        bool HasRelevantId(JSObject iJSObject);
    }  
}
