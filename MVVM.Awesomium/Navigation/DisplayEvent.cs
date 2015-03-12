﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace MVVMAwesomium
{
    public class DisplayEvent : EventArgs
    {
        public object DisplayedViewModel { get; private set; }

        public DisplayEvent(object iNewViewModel)
        {
            DisplayedViewModel = iNewViewModel;
        }
    }
}
