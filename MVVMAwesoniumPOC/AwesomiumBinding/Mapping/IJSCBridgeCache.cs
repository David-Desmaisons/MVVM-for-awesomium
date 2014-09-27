using Awesomium.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace MVVMAwesomium.AwesomiumBinding
{
    public interface IJSCBridgeCache
    {
        void Cache(object key, IJSCSGlue value);

        IJSCSGlue GetCached(object key);

        IJSCSGlue GetCached(JSObject key);
    }
}
