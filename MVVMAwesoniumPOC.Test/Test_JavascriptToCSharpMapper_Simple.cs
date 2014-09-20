using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using FluentAssertions;
using NSubstitute;
using Xunit;
using MVVMAwesomium.AwesomiumBinding;
using Awesomium.Core;

namespace MVVMAwesomium.Test
{
    public class Test_JavascriptToCSharpMapper_Simple
    {
        private JavascriptToCSharpMapper _JavascriptToCSharpMapper;
        public Test_JavascriptToCSharpMapper_Simple()
        {
            _JavascriptToCSharpMapper = new JavascriptToCSharpMapper(null);
        }
        
        [Fact]
        public void Test_GetSimpleValue_String()
        {
            _JavascriptToCSharpMapper.GetSimpleValue( new JSValue("titi")).Should().Be("titi");
        }

        [Fact]
        public void Test_GetSimpleValue_Int()
        {
           _JavascriptToCSharpMapper.GetSimpleValue(  new JSValue(10)).Should().Be(10);
        }

        [Fact]
        public void Test_GetSimpleValue_Bool()
        {
           _JavascriptToCSharpMapper.GetSimpleValue(  new JSValue(false)).Should().Be(false);
           _JavascriptToCSharpMapper.GetSimpleValue(  new JSValue(true)).Should().Be(true);
        }

        [Fact]
        public void Test_GetSimpleValue_Double()
        {
            _JavascriptToCSharpMapper.GetSimpleValue( new JSValue(0.5)).Should().Be(0.5D);
        }

        [Fact]
        public void Test_GetSimpleValue_Null()
        {
           _JavascriptToCSharpMapper.GetSimpleValue(  new JSValue()).Should().Be(null);
        }

      

        //[Fact]
        //public void Test_RemoteObjectComparer_OnlyForRemote_Equals()
        //{
        //    bool res = false;
        //    Action comp = () => res = this.GetSafe(()=>AwesomiumHelper.RemoteObjectComparer.Equals(new JSObject(), new JSObject()));
        //    comp();
        //    res.Should().BeFalse();
        //}

        //[Fact]
        //public void Test_RemoteObjectComparer_OnlyForRemote_HK()
        //{
        //    int hkey = 0;
        //    Action comp = () => hkey = this.GetSafe(() => AwesomiumHelper.RemoteObjectComparer.GetHashCode(new JSObject()));
        //    comp();
        //    hkey.Should().Be(0);
        //}

    }
}
