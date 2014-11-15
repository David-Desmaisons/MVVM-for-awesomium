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
        private WebConfig _WebConfig =
                new WebConfig() { RemoteDebuggingPort = 8001, RemoteDebuggingHost = "127.0.0.1" };

        public Boolean IsDebug
        {
            get { return (Boolean)this.GetValue(IsDebugProperty); }
            set { this.SetValue(IsDebugProperty, value); }
        }

        public static readonly DependencyProperty IsDebugProperty =
            DependencyProperty.Register("IsDebug", typeof(Boolean), typeof(HTMLWindow), new PropertyMetadata(false, DebugChanged));

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


        private static bool _First = true;
        private  static void DebugChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (!_First) return;   
            HTMLWindow w = d as HTMLWindow;
            if (w.IsDebug)
            {
                WebCore.Initialize(w._WebConfig);
                _First = false;
            }
        }


        private string _KoView = null;
        private void RunKoView()
        {
            if (_KoView == null)
            {
                using (Stream stream = Assembly.GetExecutingAssembly().
                        GetManifestResourceStream("MVVMAwesomium.Navigation.javascript.ko-view.min.js"))
                {
                    using (StreamReader reader = new StreamReader(stream))
                    {
                        _KoView = reader.ReadToEnd();
                    }
                }
            }
            _WPFDoubleBrowserNavigator.ExcecuteJavascript(_KoView);
        }

        public void ShowDebugWindow()
        {
            RunKoView();
            _WPFDoubleBrowserNavigator.ExcecuteJavascript("ko.dodebug();");
        }

        public void OpenDebugBrowser()
        {
            Process.Start(string.Format("http://{0}:{1}/", _WebConfig.RemoteDebuggingHost, _WebConfig.RemoteDebuggingPort));
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

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            ShowDebugWindow();
        }

        private void Button_Click_1(object sender, RoutedEventArgs e)
        {
            OpenDebugBrowser();
        }
    }
}
