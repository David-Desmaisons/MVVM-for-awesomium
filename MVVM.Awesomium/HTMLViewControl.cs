using Awesomium.Core;
using System;
using System.Collections.Generic;
using System.IO;
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
using System.Diagnostics;
using MVVMAwesomium.Infra.VM;
using MVVMAwesomium.Navigation;
using Awesomium.Windows.Controls;
using MVVMAwesomium.Infra;

namespace MVVMAwesomium
{
    public partial class HTMLViewControl : HTMLControlBase, IWebViewLifeCycleManager, IDisposable
    {
        public static readonly DependencyProperty UriProperty = DependencyProperty.Register("Uri", typeof(Uri), typeof(HTMLViewControl));

        public Uri Uri
        {
            get { return (Uri)this.GetValue(UriProperty); }
            set { this.SetValue(UriProperty, value); }
        }

        public string RelativePath
        {
            set { Uri = new Uri(string.Format("{0}\\{1}", Assembly.GetExecutingAssembly().GetPath(), value)); }          
        }

        public static readonly DependencyProperty ModeProperty = DependencyProperty.Register("Mode", typeof(JavascriptBindingMode), typeof(HTMLViewControl), new PropertyMetadata(JavascriptBindingMode.TwoWay));

        public JavascriptBindingMode Mode
        {
            get { return (JavascriptBindingMode)this.GetValue(ModeProperty); }
            set { this.SetValue(ModeProperty, value); }
        }

        private UrlSolver _UrlSolver;

        public HTMLViewControl():this(new UrlSolver())
        {
        }

        private HTMLViewControl(UrlSolver iIUrlSolver) : base(iIUrlSolver)
        {
            _UrlSolver= iIUrlSolver;
            _UrlSolver.Solver = this;
            this.DataContextChanged += HTMLViewControl_DataContextChanged;
        }

        private void HTMLViewControl_DataContextChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            this.NavigateAsyncBase(DataContext, null, Mode);
        }

        private class UrlSolver : IUrlSolver
        {
            public HTMLViewControl Solver { get; set; }

            Uri IUrlSolver.Solve(object iViewModel, string Id)
            {
                return Solver.Uri;
            }
        }

    }
}