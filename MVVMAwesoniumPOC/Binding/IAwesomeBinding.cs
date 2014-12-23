using System;
using Awesomium.Core;
using MVVMAwesomium.AwesomiumBinding;

namespace MVVMAwesomium
{
    public interface IAwesomeBinding : IDisposable
    {
        JSObject JSRootObject { get; }

        object Root { get; }
    }
}
