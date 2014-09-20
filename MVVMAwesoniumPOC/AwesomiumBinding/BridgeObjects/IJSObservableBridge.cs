using Awesomium.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace MVVMAwesomium.AwesomiumBinding
{
    public interface IJSObservableBridge : IJSCBridge
    {
        JSValue MappedJSValue { get; }
        void SetMappedJSValue(JSValue ijsobject, ICSharpMapper mapper);
    }

}
