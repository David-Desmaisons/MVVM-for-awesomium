﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using MVVMAwesonium.ViewModel;

namespace MVVMAwesonium.ViewModelExample
{
    public class Local : ViewModelBase
    {

        private string _City;
        public string City
        {
            get { return _City; }
            set
            {
                Set(ref _City, value, "City");
            }
        }

        private string _Region;
        public string Region
        {
            get { return _Region; }
            set
            {
                Set(ref _Region, value, "Region");
            }
        }
    }


}
