using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Awesomium.Core;

namespace MVVMAwesonium.AwesomiumBinding
{
    public class LocalBuilder : IJSOBuilder
    {
       private static int _MapCount = 0;         

        public JSObject CreateJSO()
        {
            JSObject res =new JSObject();
            res["_MappedId"] = new JSValue(_MapCount++);
            return res;
        }
    }
}
