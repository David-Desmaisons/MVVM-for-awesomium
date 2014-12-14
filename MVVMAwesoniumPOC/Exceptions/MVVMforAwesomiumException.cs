using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace MVVMAwesomium.Exceptions
{
    public class MVVMforAwesomiumArgumentException : ArgumentException
    {
        public MVVMforAwesomiumArgumentException(string iM)
            : base(iM)
        {
        }
    }

    public class MVVMforAwesomiumException : Exception
    {
        public MVVMforAwesomiumException(string iM)
            : base(iM)
        {
        }
    }
    
}
