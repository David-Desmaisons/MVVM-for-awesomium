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
using System.Windows.Navigation;
using System.Windows.Shapes;
using Awesomium.Core;
using MVVMAwesoniumPOC.ViewModel;
using MVVMAwesoniumPOC.ViewModelExample;
using MVVMAwesoniumPOC.AwesomiumBinding;
using System.Reflection;

namespace MVVMAwesoniumPOC
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private static string AssemblyDirectory
        {
            get
            {
                var lCodeBase = Assembly.GetExecutingAssembly().CodeBase;
                var lUri = new UriBuilder(lCodeBase);
                var lPath = Uri.UnescapeDataString(lUri.Path);
                return System.IO.Path.GetDirectoryName(lPath);
            }
        }

        public MainWindow()
        {
            InitializeComponent();
            this.wcBrowser.Source = new Uri(string.Format("{0}\\src\\index.html", AssemblyDirectory));
        }

        private Skill _FirstSkill;
        private Person _Person;

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            IWebView f = this.wcBrowser.WebSession.Views.FirstOrDefault();
            var datacontext = new Person() 
                {   Name = "O Monstro", 
                    LastName = "Desmaisons",
                    Local = new Local() { City = "Florianopolis", Region = "SC" }
                };

            _FirstSkill = new Skill() { Name = "Langage", Type = "French" };

            datacontext.Skills.Add(_FirstSkill);
            datacontext.Skills.Add(new Skill(){ Name="Info", Type="C++"});

            AwesomeBinding.ApplyBinding(f, datacontext);

            Window w = sender as Window;
            w.DataContext = datacontext;
            _Person = datacontext;
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            _FirstSkill.Name = "Lingua";
            _FirstSkill.Type = "Frances";
            _Person.Local.City = "ded";

        }

  //LastName:"Desmaisons",
 //   Local:{ City:'Florianopolis', Region:'SC'},
 //   Skills: [{Type:'Langage', Name:'French'},{Type:'Info', Name:'C++'}]

        //void f_DocumentReady(object sender, UrlEventArgs e)
        //{
        //    IWebView f = this.wcBrowser.WebSession.Views.FirstOrDefault();
        //    var res2 = f.ExecuteJavascriptWithResult("viewmodel.Name()");
        //    f.ExecuteJavascript("viewmodel.Name('UHU');viewmodel.commit();");
        //    //JSValue v = new JSValue("SuperConquerant");
        //    //JSObject vm = f.ExecuteJavascriptWithResult("viewmodel");
        //    //JSValue res = vm.Invoke( "Name", v );
        //    //vm.Invoke("commit", v);
        //    var res1 = f.ExecuteJavascriptWithResult("viewmodel.Name()");
        //}

    }
}
