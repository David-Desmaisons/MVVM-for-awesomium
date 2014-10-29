using MVVMAwesomium.Infra;
using MVVMAwesomium.Infra.VM;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

namespace MVVMAwesomium.Navigation.Window
{
    public class HTMLWindow : NotifyPropertyChangedBase, INotifyPropertyChanged
    {
        static private int _GId = 0;
        private int _Id;

        internal HTMLWindow()
        {
            _Id = _GId++;
            _State = WindowLogicalState.Loading;
            _IsLiteningClose = false;
            CloseReady = new BasicRelayCommand(() => State = WindowLogicalState.Closed);
        }

        private WindowLogicalState _State;
        public WindowLogicalState State
        {
            get { return _State; }
            set { Set(ref _State, value, "State"); }
        }


        private bool _IsLiteningClose;
        public bool IsListeningClose
        {
            get { return _IsLiteningClose; }
            set { Set(ref _IsLiteningClose, value, "IsListeningClose"); }
        }

        public Task CloseAsync()
        {
            if (!IsListeningClose)
                return TaskHelper.Ended();

           
            TaskCompletionSource<object> tcs = new TaskCompletionSource<object>();

            PropertyChangedEventHandler echa = null;

            echa = (o, e) =>
                {
                    if (State == WindowLogicalState.Closed)
                    {
                        tcs.SetResult(null);
                        this.PropertyChanged -= echa;
                    }
                };

            this.PropertyChanged += echa;
            State = WindowLogicalState.Closing;

            return tcs.Task;
        }

        public ICommand CloseReady { get; private set; }
    }
}
