using System;
namespace MVVMAwesomium.Binding.AwesomiumBinding
{
    public interface IProperty
    {
        bool IsSettable { get; }

        string Name { get; }

        object this[object ifather] { get; set; }
    }
}
