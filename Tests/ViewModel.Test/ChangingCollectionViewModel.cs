﻿using MVVMAwesomium.ViewModel;
using MVVMAwesomium.ViewModel.Infra;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Windows.Input;

namespace MVVMAwesomium.Test.ViewModel.Test
{
    public class ChangingCollectionViewModel : ViewModelBase
    {
        public ChangingCollectionViewModel()
        {
            _Items = new ObservableCollection<int>(new int[]{1,2,3,4,5});

            Replace = new RelayCommand(DoReplace);
            Add = new RelayCommand(DoAdd);
        }

        private void DoReplace()
        {
            Random rnd = new Random();
            Items = new ObservableCollection<int>();
        }

        private void DoAdd()
        {
            Items.Add(89);
        }

        public ICommand Replace { get; private set; }

        public ICommand Add { get; private set; }

        private ObservableCollection<int> _Items;
        public ObservableCollection<int> Items
        {
            get { return _Items; }
            set { Set(ref _Items, value, "Items"); }
        }
    }
}
