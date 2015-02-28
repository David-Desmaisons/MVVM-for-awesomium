using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace MVVMAwesomium.Navigation
{
    internal class NullWatcher : IWebSessionWatcher
    {
        void IWebSessionWatcher.LogCritical(string iInformation)
        {
        }

        void IWebSessionWatcher.LogBrowser(string iInformation)
        {
        }

        void IWebSessionWatcher.OnSessionError(Exception iexception, Action Cancel)
        {
        }
    }
}
