﻿using System;
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

         static public Exception NoKo()
        {
            return ExceptionHelper.Get("ko object not found! You should add a link to knockout.js script to the HML document!");
        }

        static public Exception NoKoExtension()
        {
           return Get("Critical error! You should add a link to knockout_Extension.js script to the HML document!");
        }

        static public Exception TimeOut()
        {
           return Get("Critical error! Javascript timeOut! Try set IWebSession Synchroneous TimeOut to 0.");
        }


        

 

        static public ArgumentException GetArgument(string iMessage)
        {
            return new MVVMforAwesomiumArgumentException(iMessage);
        }

    }
}
