using Awesomium.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace MVVMAwesomium.AwesomiumBinding
{
    public interface IJSOLocalBuilder : IJSOBuilder
    {
        JSValue CreateDate(DateTime dt);

        JSValue CreateEnum(Enum ienum);

        JSValue CreateNull();
    }
}
