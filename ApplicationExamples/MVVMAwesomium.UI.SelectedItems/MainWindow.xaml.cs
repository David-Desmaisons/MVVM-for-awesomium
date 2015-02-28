using Awesomium.Core;
using MVVMAwesomium.AwesomiumBinding;
using MVVMAwesomium.Infra;
using MVVMAwesomium.ViewModel.Example;
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

namespace MVVMAwesomium.UI.SelectedItems
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
            this.WebControl.Source = new Uri(string.Format("{0}\\HTMLUI\\index.html", Assembly.GetExecutingAssembly().GetPath()));
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            var datacontext = new SkillsViewModel();

            datacontext.Skills.Add(new Skill() {Name="knockout", Type="Info" });

            datacontext.SelectedSkills.Add(datacontext.Skills[0]);

            IWebView f = this.WebControl.WebSession.Views.FirstOrDefault();
     
            AwesomeBinding.Bind(f, datacontext, JavascriptBindingMode.TwoWay);

            Window w = sender as Window;
            w.DataContext = datacontext;
        }
    }
}
