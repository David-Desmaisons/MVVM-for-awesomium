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


    public class RelayCommand<T> : ICommand where T:class
    {
        readonly Action<T> _execute;

        public RelayCommand(Action<T> execute)
        {
            _execute = execute;
        }

        [DebuggerStepThrough]
        public bool CanExecute(object parameter)
        {
            return true;
        }

        [DebuggerStepThrough]
        public void Execute(object parameter)
        {
            _execute(parameter as T);
        }

        public event EventHandler CanExecuteChanged
        { add { CommandManager.RequerySuggested += value; } remove { CommandManager.RequerySuggested -= value; } }
    }

    public class ToogleRelayCommand : ICommand
    {
        readonly Action _execute;

        public ToogleRelayCommand(Action execute)
        {
            _execute = execute;
        }

        private bool _ShouldExecute=true;
        public bool ShouldExecute 
        {
            get { return _ShouldExecute; }
            set { if (_ShouldExecute != value) { _ShouldExecute = value; FireCanExecuteChanged(); } }
        }

        [DebuggerStepThrough]
        public bool CanExecute(object parameter)
        {
            return _ShouldExecute;
        }

        [DebuggerStepThrough]
        public void Execute(object parameter)
        {
            _execute();
        }

        private void FireCanExecuteChanged()
        {
            if (CanExecuteChanged != null)
                CanExecuteChanged(this, EventArgs.Empty);
        }


        public event EventHandler CanExecuteChanged;
    }

}
