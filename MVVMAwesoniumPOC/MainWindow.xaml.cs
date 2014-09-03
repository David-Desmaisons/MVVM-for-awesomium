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

namespace MVVMAwesoniumPOC
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

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            IWebView f = this.wcBrowser.WebSession.Views.FirstOrDefault();
            var datacontext = new Person() 
                {   Name = "O Monstro", 
                    LastName = "Desmaisons",
                    Local = new Local() { City = "Florianopolis", Region = "SC" }
                };
            
            datacontext.Skills.Add(new Skill(){ Name="Langage", Type="French"});
            datacontext.Skills.Add(new Skill(){ Name="Info", Type="C++"});

            AwesomeBinding.ApplyBinding(f, datacontext);

            Window w = sender as Window;
            w.DataContext = datacontext;
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
