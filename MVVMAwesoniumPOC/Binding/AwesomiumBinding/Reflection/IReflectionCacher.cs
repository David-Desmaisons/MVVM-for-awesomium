using MVVMAwesomium.Binding.AwesomiumBinding.Helper;
using System;
using System.Collections.Generic;


namespace MVVMAwesomium.Binding.AwesomiumBinding
{
    public interface IReflectionCacher
    {
        IEnumerable<IProperty> Properties { get; }

        IProperty this[string iPropertyName] { get; }
    }
}
