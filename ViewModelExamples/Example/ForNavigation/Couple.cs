using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Input;
using MVVMAwesomium.ViewModel.Infra;

namespace MVVMAwesomium.ViewModel.Example.ForNavigation
{
    public class Couple : MVVMAwesomium.ViewModel.Example.Couple, INavigable
    {
         public Couple():base()
        {
            GoOne = new RelayCommand(() => 
                Navigation.NavigateAsync(One));
            GoTwo = new RelayCommand(() => 
                Navigation.NavigateAsync(Two));
        }

        public INavigationSolver Navigation { get; set; }

        public ICommand GoOne { get; private set; }

        public ICommand GoTwo { get; private set; }
    }
}
