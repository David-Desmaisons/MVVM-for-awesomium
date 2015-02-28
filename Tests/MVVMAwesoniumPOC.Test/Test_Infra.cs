using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Xunit;
using FluentAssertions;
using MVVMAwesomium.Infra;

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
          public void Test_GetUnderlyingNullableType_int()
        {
            typeof(Nullable<int>).GetUnderlyingNullableType().Should().Be(typeof(int));
        }

         [Fact]
         public void Test_GetUnderlyingNullableType_string()
         {
             typeof(string).GetUnderlyingNullableType().Should().BeNull();
         }


        
    }
}
