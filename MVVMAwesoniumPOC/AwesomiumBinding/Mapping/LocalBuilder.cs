using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Awesomium.Core;

namespace MVVMAwesonium.AwesomiumBinding
{
    public class LocalBuilder : JSOBuilderBase, IJSOBuilder
    {
       private static int _MapCount = 0;    
     
       public LocalBuilder(IWebView iIWebView):base(iIWebView)
       {
       }

        public JSObject CreateJSO()
        {
            JSObject res =new JSObject();
            res["_MappedId"] = new JSValue(_MapCount++);
            return res;
        }
    }
}
