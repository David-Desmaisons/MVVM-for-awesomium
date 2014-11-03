using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Xunit;
using FluentAssertions;

namespace MVVMAwesomium.Test
{
    public class Test_NavigationBuilder
    {
        private NavigationBuilder _NavigationBuilder;
        public Test_NavigationBuilder()
        {
            _NavigationBuilder = new NavigationBuilder();
        }

        [Fact]
        public void Test_DoubleRegister_Shoul_Generate_Error()
        {
            _NavigationBuilder.Register<object>("javascript\\index.html");

            Action Failed = () => _NavigationBuilder.Register<object>("javascript\\index.html");

            Failed.ShouldThrow<ArgumentException>();
        }
    }
}
