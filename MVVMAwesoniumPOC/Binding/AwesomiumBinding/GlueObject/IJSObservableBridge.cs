using Awesomium.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace MVVMAwesomium.AwesomiumBinding
{
    public interface IJSObservableBridge : IJSCSGlue
    {
        JSValue MappedJSValue { get; }
        void SetMappedJSValue(JSValue ijsobject, IJSCBridgeCache mapper);
    }

}
