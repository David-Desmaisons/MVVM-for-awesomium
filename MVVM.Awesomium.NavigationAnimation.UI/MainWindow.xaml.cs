using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace MVVM.Awesomium.NavigationAnimation.UI
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
        }
    }
}




//namespace MVVM.AwesomiumNavigationAnimation.UI
//{


//    public class Simple
//    {
//        public Simple()
//        {
//            Go = new RelayCommand(() => Navigation.Navigate(this));
//        }
//        private INavigationSolver _Navigation;
//        public INavigationSolver Navigation
//        {
//            get { return _Navigation; }
//            set { _Navigation = value; }
//        }

//        public ICommand Go { get; private set; }

//    }
//    /// <summary>
//    /// Interaction logic for MainWindow.xaml
//    /// </summary>
//    public partial class MainWindow : Window
//    {
//        private void SetUpRoute(INavigationBuilder iNavigationBuilder)
//        {
//            iNavigationBuilder.Register<Simple>("HTMLUI\\index.html");
//        }

//        public MainWindow()
//        {
//            WebConfig webC = new WebConfig();
//            webC.RemoteDebuggingPort = 8001;
//            webC.RemoteDebuggingHost = "127.0.0.1";
//            WebCore.Initialize(webC);

//            InitializeComponent();

//            var nb = new NavigationBuilder();
//            SetUpRoute(nb);
//            WPFDoubleBrowserNavigator bn = new WPFDoubleBrowserNavigator(this.f1, this.f2, nb) { UseINavigable = true };

//            var datacontext = new Simple();
//            bn.Navigate(datacontext);
//        }
//    }
//}
