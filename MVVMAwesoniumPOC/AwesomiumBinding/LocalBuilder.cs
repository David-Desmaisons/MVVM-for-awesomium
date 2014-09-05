using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Awesomium.Core;

namespace MVVMAwesonium.AwesomiumBinding
{
    public class LocalBuilder : IJSOBuilder
    {
        public JSObject CreateJSO()
        {
            return new JSObject();
        }
    }
}
