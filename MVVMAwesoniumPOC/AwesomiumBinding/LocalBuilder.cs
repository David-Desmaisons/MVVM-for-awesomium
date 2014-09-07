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


        //public JSObject CreateJSOChild(JSObject father,string iPropertyName)
        //{
        //    JSObject child = new JSObject();
        //    father[iPropertyName] = child;
        //    return child;
        //}
    }
}
