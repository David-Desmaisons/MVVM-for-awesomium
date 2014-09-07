using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using FluentAssertions;
using NSubstitute;
using Xunit;
using MVVMAwesonium.AwesomiumBinding;
using Awesomium.Core;

namespace MVVMAwesonium.Test
{
    public class Test_AwesomiumHelper
    {
        [Fact]
        public void Test_GetSimpleValue_String()
        {
            new JSValue("titi").GetSimpleValue().Should().Be("titi");
        }

        [Fact]
        public void Test_GetSimpleValue_Int()
        {
            new JSValue(10).GetSimpleValue().Should().Be(10);
        }

        [Fact]
        public void Test_GetSimpleValue_Bool()
        {
            new JSValue(false).GetSimpleValue().Should().Be(false);
            new JSValue(true).GetSimpleValue().Should().Be(true);
        }

        [Fact]
        public void Test_GetSimpleValue_Double()
        {
            new JSValue(0.5).GetSimpleValue().Should().Be(0.5D);
        }

        [Fact]
        public void Test_GetSimpleValue_Null()
        {
            new JSValue().GetSimpleValue().Should().Be(null);
        }

        [Fact]
        public void Test_GetSimpleValue_Object()
        {
            new JSValue(new JSObject()).GetSimpleValue().Should().Be(null);
        }

        [Fact]
        public void Test_GetSimpleValue_Undefined()
        {
            new JSValue((JSObject)null).GetSimpleValue().Should().Be(null);
        }

    }
}
