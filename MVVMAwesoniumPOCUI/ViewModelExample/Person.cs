using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using MVVMAwesonium.ViewModel;
using System.Collections.ObjectModel;

namespace MVVMAwesonium.ViewModelExample
{
    public class Person : ViewModelBase
    {
        public Person()
        {
            Skills = new ObservableCollection<Skill>();
        }

        private string _LastName;
        public string LastName
        {
            get { return _LastName; }
            set
            {
                Set(ref _LastName, value, "LastName");
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

        private int _Age;
        public int Age
        {
            get { return _Age; }
            set
            {
                Set(ref _Age, value, "Age");
            }
        }

        private Local _Local;
        public Local Local
        {
            get { return _Local; }
            set
            {
                Set(ref _Local, value, "Local");
            }
        }

        public IList<Skill> Skills { get; private set; }
    }
}
