using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using Awesomium.Core;
using Awesomium.Windows.Controls;
using Xunit;
using FluentAssertions;
using NSubstitute;
using System.Reflection;
using MVVMAwesomium.ViewModel;
using System.Windows.Input;

using System.Threading;
using System.Threading.Tasks;
using System.Windows.Threading;

using MVVMAwesomium.Infra;
using MVVMAwesomium.ViewModel.Infra;

namespace MVVMAwesomium.Test
{

  
    public class Test_Navigation
    {
        private NavigationBuilder _INavigationBuilder;

        public Test_Navigation()
        {
            _INavigationBuilder = new NavigationBuilder();
        }

        public class A1
        {

        }

        [Fact]
        public void Test_WPFBrowserNavigator_Register_ShouldNotAceptBadPath()
        {
            _INavigationBuilder.Should().NotBeNull();
                    Action wf = () =>
                        {
                            _INavigationBuilder.Register<A1>("javascript\\navigationk_1.html");
                        };
                    wf.ShouldThrow<Exception>();

                    wf = () =>
                    {
                        _INavigationBuilder.RegisterAbsolute<A1>("C:\\javascript\\navigationk_1.html");
                    };
                    wf.ShouldThrow<Exception>();

                    wf = () =>
                    {
                        _INavigationBuilder.Register<A1>(new Uri("C:\\navigationk_1.html"));
                    };
                    wf.ShouldThrow<Exception>();
        }

    }

}
