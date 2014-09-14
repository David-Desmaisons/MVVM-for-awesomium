using MVVMAwesonium.ViewModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace MVVMAwesonium.ViewModel.Example
{
    public class Couple : ViewModelBase
    {

        private Person _Person;
        public Person One
        {
            get { return _Person; }
            set
            {
                Set(ref _Person, value, "One");
            }
        }

        private Person _Person2;
        public Person Two
        {
            get { return _Person2; }
            set
            {
                Set(ref _Person2, value, "Two");
            }
        }
    }
}
