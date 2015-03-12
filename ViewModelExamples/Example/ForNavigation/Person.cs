using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Input;
using MVVMAwesomium.ViewModel.Infra;

namespace MVVMAwesomium.ViewModel.Example.ForNavigation
{
    public class Person : MVVMAwesomium.ViewModel.Example.Person, INavigable
    {
        public Person():base()
        {
            GoCouple = new RelayCommand(() => Navigation.NavigateAsync(Couple));
        }
        public INavigationSolver Navigation { get; set; }

        private MVVMAwesomium.ViewModel.Example.Couple _Couple;
        public MVVMAwesomium.ViewModel.Example.Couple Couple
        {
            get { return _Couple; }
            set { Set(ref _Couple, value, "Couple"); }
        }

        public ICommand GoCouple { get; private set; }
    }
}
