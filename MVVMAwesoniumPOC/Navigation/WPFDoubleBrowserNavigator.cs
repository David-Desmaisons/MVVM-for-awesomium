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
using System.Windows;
using MVVMAwesomium.Navigation.Window;
using System.Diagnostics;

namespace MVVMAwesomium
{
    public class WPFDoubleBrowserNavigator :  INavigationSolver
    {
        private WebControl _CurrentWebControl;
        private WebControl _NextWebControl;
        private IAwesomiumBindingFactory _IAwesomiumBindingFactory;
        private IAwesomeBinding _IAwesomeBinding;
        private IUrlSolver _INavigationBuilder;
        private bool _Disposed = false;
        private HTMLLogicWindow _Window;
        private bool _Navigating = false;

        internal WebControl WebControl { get { return _CurrentWebControl; } }

        public WPFDoubleBrowserNavigator(WebControl iWebControl, WebControl iWebControlSecond, IUrlSolver inb, IAwesomiumBindingFactory iAwesomiumBindingFactory = null)
        {
            _CurrentWebControl = iWebControl;
            _NextWebControl = iWebControlSecond;

            _CurrentWebControl.Visibility = Visibility.Hidden;
            _NextWebControl.Visibility = Visibility.Hidden;

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

        private void Switch(Task<IAwesomeBinding> iBinding, HTMLLogicWindow iwindow, TaskCompletionSource<object> tcs = null)
        {
            Binding = iBinding.Result;
            if (tcs!=null) tcs.SetResult(Binding);
            _CurrentWebControl.Visibility = Visibility.Hidden;
            _NextWebControl.Visibility = Visibility.Visible;
            var tmp = _NextWebControl;
            _NextWebControl = _CurrentWebControl;
            _CurrentWebControl = tmp;
            if (_Window != null) _Window.State = WindowLogicalState.Closed;
            _Window = iwindow;
            _Window.State = WindowLogicalState.Opened;
            _NextWebControl.Source = null;
            _Navigating = false;
        }
 
        public Task Navigate(Uri iUri, object iViewModel, JavascriptBindingMode iMode = JavascriptBindingMode.TwoWay)
        {
            if (iUri == null)
                throw new Exception("ViewModel not registered");

            _Navigating = true;

            if (OnNavigate != null)
                OnNavigate(this, new NavigationEvent(iViewModel));

            var wh = new WindowHelper(new HTMLLogicWindow());

            if (!_CurrentWebControl.IsDocumentReady)
            {
                _CurrentWebControl.Source = iUri;
                return _IAwesomiumBindingFactory.Bind(_CurrentWebControl, iViewModel, wh, iMode).
                                ContinueWith(t =>
                                {
                                    Binding = t.Result;
                                    _Window = wh.__window__;
                                    _Window.State = WindowLogicalState.Opened;
                                    _CurrentWebControl.Visibility = Visibility.Visible;
                                    _Navigating = false;
                                }, TaskScheduler.FromCurrentSynchronizationContext());
            }

            Task closetask = _Window.CloseAsync();

            if (!_NextWebControl.IsDocumentReady)
            {
                _NextWebControl.Source = iUri;

                return _IAwesomiumBindingFactory.Bind(_NextWebControl, iViewModel, wh, iMode).WaitWith(closetask,
                                t => Switch(t, wh.__window__));

            }
                
            TaskCompletionSource<object> tcs = new TaskCompletionSource<object>();

            UrlEventHandler sourceupdate = null;
            sourceupdate = (o, e) =>
            {
                _NextWebControl.AddressChanged -= sourceupdate;
                _IAwesomiumBindingFactory.Bind(_NextWebControl, iViewModel, wh, iMode).WaitWith(closetask,
                     t => Switch(t, wh.__window__,tcs));
            };

            _NextWebControl.AddressChanged += sourceupdate;

            if (_NextWebControl.Source == iUri)
                _NextWebControl.Reload(false);
             else
                _NextWebControl.Source = iUri;

            return tcs.Task;
        }

        public void ExcecuteJavascript(string icode)
        {
            try 
            {
                _CurrentWebControl.ExecuteJavascript(icode);
            }
            catch(Exception e)
            {
                Trace.WriteLine(string.Format("MVVM for awesomium: Can not execute javascript: {0}, reason: {1}",icode,e));
            }
            
        }

        public Task NavigateAsync(object iViewModel, string Id = null, JavascriptBindingMode iMode = JavascriptBindingMode.TwoWay)
        {
            if ((iViewModel == null) || (_Navigating))
                return TaskHelper.Ended();

            return Navigate(_INavigationBuilder.Solve(iViewModel, Id), iViewModel, iMode);
        }

        public void Dispose()
        {
            _Disposed = true;
            Binding = null;
            UseINavigable = false;
        }


        public event EventHandler<NavigationEvent> OnNavigate;


        private bool _UseINavigable = false;
        public bool UseINavigable
        {
            get { return _UseINavigable; }
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
    }
}
