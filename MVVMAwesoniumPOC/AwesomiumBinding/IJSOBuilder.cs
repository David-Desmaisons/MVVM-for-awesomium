using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Awesomium.Core;

namespace MVVMAwesonium.AwesomiumBinding
{
    public interface IJSOBuilder
    {
        JSObject CreateJSO();

        //JSObject CreateJSOChild(JSObject father, string iPropertyName); 
    }  
}
