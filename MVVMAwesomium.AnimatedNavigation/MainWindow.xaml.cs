using System;
using System.Collections.Generic;
using System.Linq;
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
using Awesomium.Core;
using MVVMAwesomium.ViewModel.Infra;

namespace MVVMAwesomium.AnimatedNavigation
{
    public class Nav:  INavigable
    {
        public Nav()
        {
            DoNav = new RelayCommand(() => Navigation.Navigate(this));
        }

        public ICommand DoNav { get; private set;}

        public INavigationSolver Navigation
        {
            get;
            set;
        }
    }
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private void SetUpRoute(INavigationBuilder iNavigationBuilder)
        { 
            iNavigationBuilder.Register<Nav>("HTML\\index.html");
         }

        public MainWindow()
        {
            WebConfig webC = new WebConfig();
            webC.RemoteDebuggingPort = 8001;
            webC.RemoteDebuggingHost = "127.0.0.1";
            WebCore.Initialize(webC);


            InitializeComponent();


            var nb = new NavigationBuilder();
            SetUpRoute(nb);
            WPFDoubleBrowserNavigator bn = new WPFDoubleBrowserNavigator(this.One, this.Two, nb) { UseINavigable = true };

            var datacontext = new Nav();
            bn.Navigate(datacontext);
        }
    }
}
