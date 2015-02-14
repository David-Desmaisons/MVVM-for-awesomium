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
using MVVMAwesomium.Infra.VM;
using MVVMAwesomium.Navigation;
using Awesomium.Windows.Controls;

namespace MVVMAwesomium
{
    public partial class HTMLWindow : UserControl, INavigationSolver, IWebViewLifeCycleManager
    {
        private WebConfig _WebConfig =
                new WebConfig() { RemoteDebuggingPort = 8001, RemoteDebuggingHost = "127.0.0.1" };

        private Boolean _EnableBrowserDebug;
        public Boolean EnableBrowserDebug
        {
            get { return _EnableBrowserDebug; }
            set { _EnableBrowserDebug = value; if (_EnableBrowserDebug) InitForDebug(); }
        }

        public Boolean IsDebug
        {
            get { return (Boolean)this.GetValue(IsDebugProperty); }
            set { this.SetValue(IsDebugProperty, value); }
        }

        public static readonly DependencyProperty IsDebugProperty =
            DependencyProperty.Register("IsDebug", typeof(Boolean), typeof(HTMLWindow), new PropertyMetadata(false));


        public Boolean IsHTMLLoaded
        {
            get { return (Boolean)this.GetValue(IsHTMLLoadedProperty); }
            private set { this.SetValue(IsHTMLLoadedProperty, value); }
        }

        public static readonly DependencyProperty IsHTMLLoadedProperty =
            DependencyProperty.Register("IsHTMLLoaded", typeof(Boolean), typeof(HTMLWindow), new PropertyMetadata(false));


        public ICommand DebugWindow { get; private set; }

        public ICommand DebugBrowser { get; private set; }


        private IUrlSolver _IUrlSolver;

        private WPFDoubleBrowserNavigator _WPFDoubleBrowserNavigator;
        public HTMLWindow()
            : this(null)
        {
        }

        public HTMLWindow(IUrlSolver iIUrlSolver)
        {
            _IUrlSolver = iIUrlSolver ?? new NavigationBuilder();
            _INavigationBuilder = _IUrlSolver as INavigationBuilder;

            DebugWindow = new BasicRelayCommand(() => ShowDebugWindow());

            DebugBrowser = new BasicRelayCommand(() => OpenDebugBrowser()); 

            InitializeComponent();
            _WPFDoubleBrowserNavigator = new WPFDoubleBrowserNavigator(this, _IUrlSolver);
            _WPFDoubleBrowserNavigator.OnFirstLoad += FirstLoad;
           
        }

        private void FirstLoad(object sender, EventArgs e)
        {
            IsHTMLLoaded = true;
            _WPFDoubleBrowserNavigator.OnFirstLoad -= FirstLoad;
        }


        private void InitForDebug()
        {
            if (!WebCore.IsInitialized)
                WebCore.Initialize(_WebConfig);
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
            if (EnableBrowserDebug)
                Process.Start(string.Format("http://{0}:{1}/", _WebConfig.RemoteDebuggingHost, _WebConfig.RemoteDebuggingPort));
            else
                MessageBox.Show("EnableBrowserDebug should be set to true to enable debugging in a Webrowser!");
        }

        private INavigationBuilder _INavigationBuilder;
        public INavigationBuilder NavigationBuilder
        {
            get { return _INavigationBuilder; }
        }

        public Uri Source
        {
            get { return _WPFDoubleBrowserNavigator.WebControl.Source; }
        }

        public bool UseINavigable
        {
            get { return _WPFDoubleBrowserNavigator.UseINavigable; }
            set { _WPFDoubleBrowserNavigator.UseINavigable = value; }
        }

        public Task NavigateAsync(object iViewModel, string Id = null, JavascriptBindingMode iMode = JavascriptBindingMode.TwoWay)
        {
            return _WPFDoubleBrowserNavigator.NavigateAsync(iViewModel, Id, iMode);
        }

      

        public void Dispose()
        {
            _WPFDoubleBrowserNavigator.Dispose();
        }

        public event EventHandler<NavigationEvent> OnNavigate
        {
            add { _WPFDoubleBrowserNavigator.OnNavigate += value; }
            remove { _WPFDoubleBrowserNavigator.OnNavigate -= value; }
        }

        public event EventHandler OnFirstLoad
        {
            add { _WPFDoubleBrowserNavigator.OnFirstLoad += value; }
            remove { _WPFDoubleBrowserNavigator.OnFirstLoad -= value; }
        }

        private void Root_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.F5) e.Handled = true;
        }

        private static WebSession _Session = null;

        IWebView IWebViewLifeCycleManager.Create()
        {
            if (_Session==null)
                _Session = WebCore.CreateWebSession(new WebPreferences());
     
            WebControl nw = new WebControl()
            {
                WebSession = _Session,
                Visibility = Visibility.Hidden,
                HorizontalAlignment = HorizontalAlignment.Stretch,
                VerticalAlignment = VerticalAlignment.Stretch,
                ContextMenu = new ContextMenu() { Visibility=Visibility.Collapsed}
            };
            Grid.SetColumnSpan(nw, 2);
            Grid.SetRowSpan(nw, 2);
            Panel.SetZIndex(nw,0);
            this.MainGrid.Children.Add(nw);
            return nw;
        }

        void IWebViewLifeCycleManager.Display(IWebView webview)
        {
            (webview as WebControl).Visibility = Visibility.Visible;
        }

        void IWebViewLifeCycleManager.Dispose(IWebView ioldwebview)
        {
            var wb = (ioldwebview as WebControl);
            wb.Visibility = Visibility.Hidden;

            if (!ioldwebview.IsCrashed)
            {
                ioldwebview.Source = new Uri("about:blank");
            }
                 
            this.MainGrid.Children.Remove(wb);

            wb.Dispose();
            
        }
    }
}
