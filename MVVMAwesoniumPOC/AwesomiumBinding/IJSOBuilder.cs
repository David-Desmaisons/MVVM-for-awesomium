using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Awesomium.Core;

namespace MVVMAwesoniumPOC.AwesomiumBinding
{
    public interface IJSOBuilder
    {
        JSObject CreateJSO(); 
    }

    public class LocalBuilder : IJSOBuilder
    {
        public JSObject CreateJSO()
        {
            return new JSObject();
        }
    }
}
