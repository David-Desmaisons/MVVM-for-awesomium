using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Windows.Input;

namespace MVVMAwesomium.ViewModel.Infra
{
   
    public class RelayCommand : ICommand
    {
        readonly Action<object> _execute;

        public RelayCommand(Action<object> execute)
        {
            _execute = execute;
        }

        public RelayCommand(Action execute)
            : this((_) => execute())
        {
        }

        [DebuggerStepThrough]
        public bool CanExecute(object parameter)
        {
            return true;
        }

        [DebuggerStepThrough]
        public void Execute(object parameter)
        {
            _execute(parameter);
        }

        public event EventHandler CanExecuteChanged
        { add { CommandManager.RequerySuggested += value; } remove { CommandManager.RequerySuggested -= value; } }
    }

}
