﻿using System;
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

using MVVMAwesonium.AwesomiumBinding;
using MVVMAwesonium.Infra;
using MVVMAwesonium.ViewModel.Example;

namespace MVVMAwesonium.UI
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
            this.wcBrowser.Source = new Uri(string.Format("{0}\\src\\index.html", Assembly.GetExecutingAssembly().GetPath()));
        }

        private Skill _FirstSkill;
        private Person _Person;

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            IWebView f = this.wcBrowser.WebSession.Views.FirstOrDefault();
            var datacontext = new Person()
                {
                    Name = "O Monstro",
                    LastName = "Desmaisons",
                    Local = new Local() { City = "Florianopolis", Region = "SC" }
                };

            _FirstSkill = new Skill() { Name = "Langage", Type = "French" };

            datacontext.Skills.Add(_FirstSkill);
            datacontext.Skills.Add(new Skill() { Name = "Info", Type = "C++" });

            AwesomeBinding.Bind(f, datacontext, JavascriptBindingMode.TwoWay);

            Window w = sender as Window;
            w.DataContext = datacontext;
            _Person = datacontext;
        }

      

  //LastName:"Desmaisons",
 //   Local:{ City:'Florianopolis', Region:'SC'},
 //   Skills: [{Type:'Langage', Name:'French'},{Type:'Info', Name:'C++'}

    }
}
