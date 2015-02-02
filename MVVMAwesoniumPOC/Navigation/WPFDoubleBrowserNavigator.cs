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
using MVVMAwesomium.Exceptions;
using MVVMAwesomium.Navigation;

namespace MVVMAwesomium
{
    public class WPFDoubleBrowserNavigator :  INavigationSolver
    {
        private IWebViewLifeCycleManager _IWebViewLifeCycleManager;

        private IWebView _CurrentWebControl;
        private IWebView _NextWebControl;


        private IAwesomiumBindingFactory _IAwesomiumBindingFactory;
        private IAwesomeBinding _IAwesomeBinding;
        private IUrlSolver _INavigationBuilder;
        private bool _Disposed = false;
        private HTMLLogicWindow _Window;
        private bool _Navigating = false;

        internal IWebView WebControl { get { return _CurrentWebControl; } }

        public WPFDoubleBrowserNavigator(WebControl iWebControl, WebControl iWebControlSecond, IUrlSolver inb, IAwesomiumBindingFactory iAwesomiumBindingFactory = null):
            this( new WebViewSimpleLifeCycleManager(iWebControl,iWebControlSecond),inb,iAwesomiumBindingFactory)
        {
        }

        public WPFDoubleBrowserNavigator(IWebViewLifeCycleManager lifecycler, IUrlSolver inb, IAwesomiumBindingFactory iAwesomiumBindingFactory = null)
        {
            _IWebViewLifeCycleManager = lifecycler;
            _INavigationBuilder = inb;
            _IAwesomiumBindingFactory = iAwesomiumBindingFactory ?? new AwesomiumBindingFactory() { ManageWebSession = false };
        }

        private void ConsoleMessage(object sender, ConsoleMessageEventArgs e)
        {
            try
            {
                Trace.WriteLine(string.Format("MVVM for awesomium: WebSession log message: {0}, event name: {1}, event type {2}, source {3}, line number {4}", e.Message, e.EventName, e.EventType, e.Source, e.LineNumber));
            }
            catch{ }
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

        private void FireNavigate(object inewvm, object ioldvm=null) 
        {
            if (OnNavigate != null)
                OnNavigate(this, new NavigationEvent(inewvm, ioldvm));
        }

        private void Switch(Task<IAwesomeBinding> iBinding, HTMLLogicWindow iwindow, TaskCompletionSource<object> tcs = null)
        {
            object oldvm = (Binding != null) ? Binding.Root : null;
            Binding = iBinding.Result;
          
            if (_CurrentWebControl!=null)
            {
                _IWebViewLifeCycleManager.Dispose(_CurrentWebControl);
                _CurrentWebControl.ConsoleMessage -= ConsoleMessage;
            }
            else if (OnFirstLoad != null)
                OnFirstLoad(this, EventArgs.Empty);

            _CurrentWebControl = _NextWebControl;     
            _NextWebControl = null;

            _IWebViewLifeCycleManager.Display(_CurrentWebControl);
            if (_Window != null) _Window.State = WindowLogicalState.Closed;
            _Window = iwindow;
            _Window.State = WindowLogicalState.Opened;

            _Navigating = false;
            FireNavigate(Binding.Root, oldvm);
            
            if (tcs != null) tcs.SetResult(Binding);
        }
 
        public Task Navigate(Uri iUri, object iViewModel, JavascriptBindingMode iMode = JavascriptBindingMode.TwoWay)
        {
            if (iUri == null)
                throw ExceptionHelper.GetArgument(string.Format("ViewModel not registered: {0}", iViewModel.GetType()));

            _Navigating = true;           

            var wh = new WindowHelper(new HTMLLogicWindow());

            Task closetask = (_CurrentWebControl!=null) ? _Window.CloseAsync() : TaskHelper.Ended();

            _NextWebControl = _IWebViewLifeCycleManager.Create();
            _NextWebControl.ConsoleMessage += ConsoleMessage;

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
            if (object.ReferenceEquals(e.NewViewModel, e.OldViewModel))
                return;

            INavigable nv = e.NewViewModel as INavigable;
            if (nv != null)
                nv.Navigation = this;

            nv = e.OldViewModel as INavigable;
            if (nv != null)
                nv.Navigation = null;
        }

        public event EventHandler<NavigationEvent> OnNavigate;

        public event EventHandler OnFirstLoad;
    }
}
