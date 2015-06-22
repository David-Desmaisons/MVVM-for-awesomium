﻿using MVVM.Component;
using MVVMAwesomium.ViewModel.Infra;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Windows.Input;

namespace MVVMAwesomium.ViewModel.Example
{

    public class CardViewModel : ViewModelBase
    {
        public CardViewModel()
        {
            Insert = new RelaySimpleCommand<ItemViewModel>((it) => _Items.Insert(_Items.IndexOf(it),new ItemViewModel()));
            Remove = new RelaySimpleCommand<ItemViewModel>((it) => _Items.Remove(it));
        }


        private ObservableCollection<ItemViewModel> _Items= new ObservableCollection<ItemViewModel>();
        public ObservableCollection<ItemViewModel> Items
        {
            get { return _Items; }
        }

        public ISimpleCommand Insert { private set; get; }

        public ISimpleCommand Remove { private set; get; }
    }
}
