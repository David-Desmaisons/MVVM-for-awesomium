﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Xunit;
using FluentAssertions;
using NSubstitute;
using MVVMAwesomium.AwesomiumBinding;
using System.Threading;
using Awesomium.Core;

namespace MVVMAwesomium.Test
{
    public class Test_GlobalBuilder : Awesomium_Test_Base
    {
        public Test_GlobalBuilder(): base()
        {
        }

        [Fact]
        public void Test_GlobalBuilder_Basic()
        {
            using (Tester())
            { 
                bool isValidSynchronizationContext = (_SynchronizationContext != null) && (_SynchronizationContext.GetType() != typeof(SynchronizationContext));
                isValidSynchronizationContext.Should().BeTrue();


                var res0 = GetSafe(()=> _WebView.CreateGlobalJavascriptObject("teste"));

                GlobalBuilder gb = new GlobalBuilder(_WebView,"Test");
                var res = gb.CreateJSO();
                res.Should().NotBeNull();
                string name = GetSafe( () => res.GlobalObjectName);
                name.Should().Be("Test_1");
                var type = GetSafe(() => res.Type);
                type.Should().Be(JSObjectType.RemoteGlobal);
            }
        }
    }
}
