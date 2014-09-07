using Awesomium.Core;
using System;
namespace MVVMAwesonium.AwesomiumBinding
{
    public interface IBridgeObject
    {
        JSValue JSValue { get; set; }

        object CValue { get; set; }
    }
}
