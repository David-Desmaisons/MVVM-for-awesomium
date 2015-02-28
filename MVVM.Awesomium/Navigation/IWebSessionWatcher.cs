using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace MVVMAwesomium
{
    public interface IWebSessionWatcher
    {
        void LogCritical(string iInformation);

        void LogBrowser(string iInformation);

        void OnSessionError(Exception iexception, Action Cancel);
    }
}
