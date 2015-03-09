﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Xunit;
using FluentAssertions;
using MVVMAwesomium.Infra;
using System.ComponentModel;
using System.Windows;
using MVVMAwesomium.Infra.VM;

namespace MVVMAwesomium.Test
{
    public class Test_Infra
    {

        [Fact]
        public void Test_GetEnumerableBase_int_isNotEnumerable()
        {
            typeof(int).GetEnumerableBase().Should().BeNull();
        }

        [Fact]
        public void Test_GetEnumerableBase_IEnumerable_int_isNotEnumerable()
        {
            typeof(IEnumerable<int>).GetEnumerableBase().Should().Be(typeof(int));
        }

         [Fact]
        public void Test_GetEnumerableBase_IList_int_isNotEnumerable()
        {
            typeof(IList<int>).GetEnumerableBase().Should().Be(typeof(int));
        }

         [Fact]
         public void Test_GetEnumerableBase_null_int_isNotEnumerable()
         {
             Type n = null;
             n.GetEnumerableBase().Should().BeNull();
         }

          [Fact]
         public void Test_GetUnderlyingNullableType_null()
         {
             Type n = null;
             n.GetUnderlyingNullableType().Should().BeNull();
         }

          [Fact]
          public void Test_GetUnderlyingiliststring_null()
          {
              Type n = typeof(IList<string>);
              n.GetUnderlyingNullableType().Should().BeNull();
          }

         [Fact]
          public void Test_GetUnderlyingNullableType_int()
        {
            typeof(Nullable<int>).GetUnderlyingNullableType().Should().Be(typeof(int));
        }

         [Fact]
         public void Test_GetUnderlyingNullableType_string()
         {
             typeof(string).GetUnderlyingNullableType().Should().BeNull();
         }

         [Fact]
         public void Test_GetDescription_FallBack()
         {
             Visibility vi = Visibility.Hidden;
             vi.GetDescription().Should().Be("Hidden");
         }

         enum Ex { [Description("Cute")] ex1 = 8, [Description("Cute2")] ex2 = 16 };

         [Fact]
         public void Test_GetDescription_Description()
         {
             Ex vi = Ex.ex1;
             vi.GetDescription().Should().Be("Cute");
         }


         [Fact]
         public void Test_GetDescription_Or()
         {
             Ex vi = Ex.ex1 | Ex.ex2;
             vi.GetDescription().Should().Be("24");
         }

        public class vm : NotifyPropertyChangedBase
        {
            private int _Value;
            public int Value { get { return _Value; } set { Set(ref _Value, value, "Value"); } }
        }

        [Fact]
        public void Test_NotifyPropertyChangedBase()
        {
            var vm = new vm();

            vm.MonitorEvents();

            vm.Value.Should().Be(0);
            vm.Value = 0;
            vm.Value.Should().Be(0);
            vm.ShouldNotRaisePropertyChangeFor(t=>t.Value);

            vm.Value = 2;
            vm.Value.Should().Be(2);
            vm.ShouldRaisePropertyChangeFor(t => t.Value);
        }

		
        
    }
}
