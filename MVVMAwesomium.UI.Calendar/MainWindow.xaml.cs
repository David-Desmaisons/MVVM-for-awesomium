using Awesomium.Core;
using MVVMAwesonium.AwesomiumBinding;
using MVVMAwesonium.ViewModel.Example;
using MVVMAwesonium.Infra;
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
using MVVMAwesomium.ViewModel.Example;

namespace MVVMAwesomium.UI.Calendar
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
            this.WebControl.Source = new Uri(string.Format("{0}\\HTLM\\index.html", Assembly.GetExecutingAssembly().GetPath()));
 
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            IWebView f = this.WebControl.WebSession.Views.FirstOrDefault();
            var datacontext = new DateInformation() { Date= new DateTime(1974,2,26) };
            
            AwesomeBinding.Bind(f, datacontext, JavascriptBindingMode.TwoWay);

            Window w = sender as Window;
            w.DataContext = datacontext;
        }
    }
}
