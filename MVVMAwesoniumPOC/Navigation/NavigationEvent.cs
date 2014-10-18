using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace MVVMAwesomium
{
    public class NavigationEvent : EventArgs
    {
        public object ViewModel { get; private set; }

        public NavigationEvent(object iViewModel)
        {
            ViewModel = iViewModel;
        }
    }
}
