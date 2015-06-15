using MVVM.Component;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace MVVMAwesomium.ViewModel.Example
{
    public class Factory : ViewModelBase
    {
        public Factory()
        {
            CreateObject = new RelayResultCommand<string,Person>((n)=> new Person(){LastName=n+"99"});
        }

        public IResultCommand CreateObject { get; set; }

        private string _Name;
        public string Name
        {
            get { return _Name; }
            set { Set(ref _Name, value, "Name"); }
        }
    }
}
