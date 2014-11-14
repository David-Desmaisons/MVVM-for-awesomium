using Awesomium.Core;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Diagnostics;

namespace MVVMAwesomium.Navigation
{
    public partial class HTMLWindow : UserControl, INavigationSolver
    {
        private WPFDoubleBrowserNavigator _WPFDoubleBrowserNavigator;
        public HTMLWindow()
            : this(null)
        {
        }

        public HTMLWindow(IUrlSolver iIUrlSolver)
        {
            _IUrlSolver = iIUrlSolver ?? new NavigationBuilder();
            _INavigationBuilder = _IUrlSolver as INavigationBuilder;

            InitializeComponent();
            _WPFDoubleBrowserNavigator = new WPFDoubleBrowserNavigator(this.First, this.Second, _IUrlSolver);
        }

        private bool _BrowserDebug = false;
        public bool BrowserDebug
        {
            get { return _BrowserDebug; }
            set { _BrowserDebug = value; if (_BrowserDebug) DoDebug(); }
        }

        private void DoDebug()
        {
            WebConfig webC = new WebConfig();
            webC.RemoteDebuggingPort = 8001;
            webC.RemoteDebuggingHost = "127.0.0.1";
            WebCore.Initialize(webC);
        }

        private bool _RunKoView = false;
        private void RunKoView()
        {
            if (_RunKoView)
                return;

            _RunKoView = true;

            using (Stream stream = Assembly.GetExecutingAssembly().
                        GetManifestResourceStream("MVVMAwesomium.Navigation.javascript.ko-view.min.js"))
            {
                using (StreamReader reader = new StreamReader(stream))
                {
                    _WPFDoubleBrowserNavigator.ExcecuteJavascript(reader.ReadToEnd());
                }
            }
        }

        public void ShowDebugWindow()
        {
            RunKoView();
            _WPFDoubleBrowserNavigator.ExcecuteJavascript("ko.dodebug();");
        }

        public void OpenDebugBrowser()
        {
            if (_BrowserDebug)
                Process.Start("http://127.0.0.1:8001/");
        }


        private INavigationBuilder _INavigationBuilder;
        public INavigationBuilder INavigationBuilder
        {
            get { return _INavigationBuilder; }
        }

        private IUrlSolver _IUrlSolver;
        public IUrlSolver IUrlSolver
        {
            get { return _IUrlSolver; }
        }

        public bool UseINavigable
        {
            get { return _WPFDoubleBrowserNavigator.UseINavigable; }
            set { _WPFDoubleBrowserNavigator.UseINavigable = value; }
        }

        public Task Navigate(object iViewModel, string Id = null, JavascriptBindingMode iMode = JavascriptBindingMode.TwoWay)
        {
            return _WPFDoubleBrowserNavigator.Navigate(iViewModel, Id, iMode);
        }

        public event EventHandler<NavigationEvent> OnNavigate
        {
            add { _WPFDoubleBrowserNavigator.OnNavigate += value; }
            remove { _WPFDoubleBrowserNavigator.OnNavigate -= value; }
        }

        public void Dispose()
        {
            _WPFDoubleBrowserNavigator.Dispose();
        }
    }
}
