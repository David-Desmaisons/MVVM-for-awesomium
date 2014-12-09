using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace MVVMAwesomium.Binding.AwesomiumBinding.Helper
{
    internal class ReflectionCacher : IReflectionCacher
    {
        public IEnumerable<IProperty> Properties { get; private set; }

        public IProperty this[string iPropertyName]
        {
            get { return null; }
            private set{}
        }
    }
}
