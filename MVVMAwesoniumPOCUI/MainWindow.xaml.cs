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
using System.Reflection;

using MVVMAwesomium.AwesomiumBinding;
using MVVMAwesomium.Infra;
using MVVMAwesomium.ViewModel.Example;

namespace MVVMAwesomium.UI
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            WebConfig webC = new WebConfig();
            webC.RemoteDebuggingPort = 8001;
            webC.RemoteDebuggingHost = "127.0.0.1";
            WebCore.Initialize(webC);

            InitializeComponent();
            this.wcBrowser.Source = new Uri(string.Format("{0}\\src\\index.html", Assembly.GetExecutingAssembly().GetPath()));
        }

        private Skill _FirstSkill;
        private Person _Person;

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            var datacontext = new Person()
                {
                    Name = "O Monstro",
                    LastName = "Desmaisons",
                    Local = new Local() { City = "Florianopolis", Region = "SC" },
                    PersonalState = PersonalState.Married
                };

            _FirstSkill = new Skill() { Name = "Langage", Type = "French" };

            datacontext.Skills.Add(_FirstSkill);
            datacontext.Skills.Add(new Skill() { Name = "Info", Type = "C++" });

            AwesomeBinding.Bind(this.wcBrowser, datacontext, new Test(){_IsAnimatedClosing_=true}, JavascriptBindingMode.TwoWay);

            Window w = sender as Window;
            w.DataContext = datacontext;
            _Person = datacontext;
        }

        private class Test
        {
            public bool _IsClosing_ { get; set; }

            public bool _IsAnimatedClosing_ { get; set; }

            public bool _IsAnimatedClosed_ { get; set; }
        }

  //LastName:"Desmaisons",
 //   Local:{ City:'Florianopolis', Region:'SC'},
 //   Skills: [{Type:'Langage', Name:'French'},{Type:'Info', Name:'C++'}

    }
}
