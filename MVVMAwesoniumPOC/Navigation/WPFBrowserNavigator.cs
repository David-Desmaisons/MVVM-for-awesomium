using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Data;
using Awesomium.Core;
using Awesomium.Windows.Controls;
using MVVMAwesomium.Infra;
using System.IO;

namespace MVVMAwesomium
{
    public class WPFBrowserNavigator :  INavigationSolver
    {
        private WebControl _WebControl;
        private IAwesomiumBindingFactory _IAwesomiumBindingFactory;
        private IAwesomeBinding _IAwesomeBinding;
        private IUrlSolver _INavigationBuilder;
        private bool _Disposed = false;

        internal WebControl WebControl { get { return _WebControl; } }

        public WPFBrowserNavigator(WebControl iWebControl, IUrlSolver inb, IAwesomiumBindingFactory iAwesomiumBindingFactory = null)
        {
            _WebControl = iWebControl;
            _INavigationBuilder = inb;
            _IAwesomiumBindingFactory = iAwesomiumBindingFactory ?? new AwesomiumBindingFactory() { ManageWebSession = false };
        }

        private IAwesomeBinding Binding
        {
            get { return _IAwesomeBinding; }
            set
            {
                if (_IAwesomeBinding != null)
                    _IAwesomeBinding.Dispose();

                _IAwesomeBinding = value;
                if (_Disposed && (_IAwesomeBinding!=null))
                {
                    _IAwesomeBinding.Dispose();
                    _IAwesomeBinding = null;
                }
            }
        }
 
        public Task Navigate(Uri iUri, object iViewModel, JavascriptBindingMode iMode = JavascriptBindingMode.TwoWay)
        {
            if (iUri == null)
                throw new Exception("ViewModel not registered");

            if (OnNavigate != null)
                OnNavigate(this, new NavigationEvent(iViewModel));

            if (!_WebControl.IsDocumentReady)
            {
                _WebControl.Source = iUri;
                return _IAwesomiumBindingFactory.Bind(_WebControl, iViewModel, iMode)
                            .ContinueWith(t =>
                                {
                                    if (OnFirstLoad != null)
                                        OnFirstLoad(this, EventArgs.Empty);
                                    Binding = t.Result;
                                });
            }
                
            TaskCompletionSource<IAwesomeBinding> tcs = new TaskCompletionSource<IAwesomeBinding>();

            UrlEventHandler sourceupdate = null;
            sourceupdate = (o, e) =>
            {
                _WebControl.AddressChanged -= sourceupdate;
                _IAwesomiumBindingFactory.Bind(_WebControl, iViewModel, iMode).ContinueWith(t =>
                {
                    Binding = t.Result;
                    tcs.SetResult(Binding);
                });
            };

            _WebControl.AddressChanged += sourceupdate;

             if (_WebControl.Source == iUri)
                 _WebControl.Reload(false);
             else
                  _WebControl.Source = iUri;

            return tcs.Task;
        }

        public Task NavigateAsync(object iViewModel, string Id = null, JavascriptBindingMode iMode = JavascriptBindingMode.TwoWay)
        {
            if (iViewModel == null)
                return TaskHelper.Ended();

            return Navigate(_INavigationBuilder.Solve(iViewModel, Id), iViewModel, iMode);
        }

        public void Dispose()
        {
            _Disposed = true;
            Binding = null;
            UseINavigable = false;
        }

        private bool _UseINavigable = false;
        public bool UseINavigable
        {
            get
            {
                return _UseINavigable;
            }
            set
            {
                if (_UseINavigable == value)
                    return;

                if (_UseINavigable=value)
                    OnNavigate += WPFBrowserNavigator_OnNavigate;
                else
                    OnNavigate -= WPFBrowserNavigator_OnNavigate;
            }
        }

        private void WPFBrowserNavigator_OnNavigate(object sender, NavigationEvent e)
        {
            INavigable nv = e.ViewModel as INavigable;
            if (nv != null)
                nv.Navigation = this;
        }

        public event EventHandler OnFirstLoad;

        public event EventHandler<NavigationEvent> OnNavigate;

    }
}
