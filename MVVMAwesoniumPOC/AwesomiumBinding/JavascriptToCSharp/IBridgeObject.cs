using Awesomium.Core;
using System;
using System.Collections.Generic;

namespace MVVMAwesonium.AwesomiumBinding
{
    public interface IBridgeObject
    {
        JSValue JSValue { get; set; }

        object CValue { get; set; }

        IEnumerable<IBridgeObject> GetChildren();
    }
}
