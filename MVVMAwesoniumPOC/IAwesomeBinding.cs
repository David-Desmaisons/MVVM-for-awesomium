using System;
using MVVMAwesomium.AwesomiumBinding;

namespace MVVMAwesomium
{
    public interface IAwesomeBinding : IDisposable
    {
        IJSCSGlue JSRootObject { get; }
    }
}
