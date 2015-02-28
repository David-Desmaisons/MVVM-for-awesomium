﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Input;
using MVVMAwesomium.ViewModel.Infra;
using MVVMAwesomium.ViewModel;

namespace MVVMAwesomium.ViewModel.Example
{
    public class Skill : ViewModelBase
    {
        private string _Type;
        public string Type
        {
            get { return _Type; }
            set
            {
                Set(ref _Type, value, "Type");
            }
        }

        private string _Name;
        public string Name
        {
            get { return _Name; }
            set
            {
                Set(ref _Name, value, "Name");
            }
        }
    }
}
