﻿using MVVMAwesomium.ViewModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace MVVMAwesomium.ViewModel.Example
{
    public class DateInformation : ViewModelBase
    {
        public DateInformation()
        {
        }
        private DateTime _Date;
        public DateTime Date
        {
            get { return _Date; }
            set
            {
                Set(ref _Date, value, "Date");
            }
        }
    }
}
