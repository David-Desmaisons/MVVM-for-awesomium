using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;

namespace MVVMAwesomium.Exceptions
{
    public static class ExceptionHelper
    {
        private const string _Header = "MVVM for Awesomium";

        static public void Log(string iMessageLog)
        {
            Trace.WriteLine(string.Format("{0} - {1}", _Header,iMessageLog));
        }

        static public Exception Get(string iMessage)
        {
            return new MVVMforAwesomiumException(iMessage);
        }

        static public ArgumentException GetArgument(string iMessage)
        {
            return new MVVMforAwesomiumArgumentException(iMessage);
        }

    }
}
