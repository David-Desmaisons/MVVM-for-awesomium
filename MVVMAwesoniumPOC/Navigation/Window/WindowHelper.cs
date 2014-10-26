using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace MVVMAwesomium.Navigation.Window
{
    public class WindowHelper
    {
        public HTMLWindow __window__ { get;private set;}

        public WindowHelper(HTMLWindow iwindow)
        {
            __window__ = iwindow;
        }
    }
}
