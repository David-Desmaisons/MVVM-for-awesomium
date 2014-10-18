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
using MVVMAwesomium;

namespace MVVMUI.Navigation
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {

        private void SetUpRoute(INavigationBuilder iNavigationBuilder)
        {
            iNavigationBuilder.Register<MVVMAwesomium.ViewModel.Example.ForNavigation.Person>("\\HTMLUI\\index_edit.html","Edit");
            iNavigationBuilder.Register<MVVMAwesomium.ViewModel.Example.ForNavigation.Person>("\\HTMLUI\\index_view.html","View");
        }

        public MainWindow()
        {
            InitializeComponent();

            WPFBrowserNavigator bn = new WPFBrowserNavigator(this.WC) { UseINavigable = true };
            SetUpRoute(bn);

            var my = new MVVMAwesomium.ViewModel.Example.ForNavigation.Person()
            {
                Name = "O Monstro",
                LastName = "Desmaisons",
                Local = new MVVMAwesomium.ViewModel.Example.Local() { City = "Florianopolis", Region = "SC" }
            };

            bn.Navigate(my);

        }
    }
}
