using Awesomium.Core;
using MVVMAwesonium.AwesomiumBinding;
using MVVMAwesonium.ViewModelExample;
using System;
using System.Collections.Generic;
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
using MVVMAwesonium.Infra;

namespace MVVMAwesomium.UI2
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
            this.wcBrowser.Source = new Uri(string.Format("{0}\\HTMLUI\\index.html", Assembly.GetExecutingAssembly().GetPath()));
        }


        private Couple _DT = null;
        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            IWebView f = this.wcBrowser.WebSession.Views.FirstOrDefault();
            var datacontext = new Couple();
            datacontext.One =  new Person()
            {
                Name = "O Monstro",
                LastName = "Desmaisons",
                Local = new Local() { City = "Florianopolis", Region = "SC" }
            };
            //datacontext.Two = datacontext.One;
            datacontext.Two = null;

            AwesomeBinding.Bind(f, datacontext, JavascriptBindingMode.TwoWay);

            Window w = sender as Window;
            w.DataContext = datacontext;

            _DT = datacontext;
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            _DT.Two = _DT.One;
                //;new Person() { Name = "Claudia", LastName = "Vicente" };
        }
    }
}
