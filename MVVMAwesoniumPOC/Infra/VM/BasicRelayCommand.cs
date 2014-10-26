using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Windows.Input;

namespace MVVMAwesomium.Infra.VM
{
    internal class BasicRelayCommand : ICommand
    {
        readonly Action _execute;

        public BasicRelayCommand(Action execute)
        {
            _execute = execute;
        }

        public bool CanExecute(object parameter)
        {
            return true;
        }

        [DebuggerStepThrough]
        public void Execute(object parameter)
        {
            _execute();
        }

        public event EventHandler CanExecuteChanged;
    }
}
