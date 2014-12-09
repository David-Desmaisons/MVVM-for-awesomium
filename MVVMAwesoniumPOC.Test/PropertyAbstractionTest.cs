using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;


using FluentAssertions;
using NSubstitute;
using Xunit;
using MVVMAwesomium.AwesomiumBinding;
using Awesomium.Core;
using System.Collections;
using MVVMAwesomium.ViewModel.Example.ForNavigation;
using System.Reflection;
using MVVMAwesomium.Binding.AwesomiumBinding;
using System.Diagnostics;

namespace MVVMAwesomium.Test
{
    public class PropertyAbstractionTest
    {
        [Fact]
        public void Should_Have_Getter_And_Setter_OK()
        {
            Person p = new Person() { Name = "David", LastName = "D" };
            PropertyInfo pi = typeof(Person).GetProperty("Name", BindingFlags.Public | BindingFlags.Instance);
            pi.Should().NotBeNull();

            PropertyAbstraction pa = new PropertyAbstraction(pi);

            pa.Name.Should().Be("Name");
            pa[p].Should().Be("David");
            p.Name = "toto";
            pa[p].Should().Be("toto");
            pa.IsSettable.Should().BeTrue();

            pa[p] = "David2";
            p.Name.Should().Be("David2"); ;
        }

        [Fact]
        public void Should_Have_Getter_Perf()
        {
            Person p = new Person() { Name = "David", LastName = "D" };
            PropertyInfo pi = typeof(Person).GetProperty("Name", BindingFlags.Public | BindingFlags.Instance);
            pi.Should().NotBeNull();

            PropertyAbstraction pa = new PropertyAbstraction(pi);

            pa.Name.Should().Be("Name");
            pa[p].Should().Be("David");
            p.Name = "toto";
            pa[p].Should().Be("toto");

            GC.Collect();
            GC.WaitForFullGCComplete();

            var stopWatch = new Stopwatch();
            stopWatch.Start();

            int iis = 800000;
            for (int i = 0; i < iis; i++)
            {
                var value = pi.GetValue(p, null);
            }

            stopWatch.Stop();
            var ts = stopWatch.ElapsedMilliseconds;
            Console.WriteLine("Perf: {0} sec for {1} items with reflection", ((double)(ts)) / 1000, iis);

            GC.Collect();
            GC.WaitForFullGCComplete();

            stopWatch = new Stopwatch();
            stopWatch.Start();

            for (int i = 0; i < iis; i++)
            {
                var value = pa[p];
            }

            stopWatch.Stop();
            ts = stopWatch.ElapsedMilliseconds;
            Console.WriteLine("Perf: {0} sec for {1} items with cache", ((double)(ts)) / 1000, iis);


            stopWatch.Stop();
      
            GC.Collect();
            GC.WaitForFullGCComplete();

            stopWatch = new Stopwatch();
            stopWatch.Start();

            for (int i = 0; i < iis; i++)
            {
                var value = p.Name;
            }

            stopWatch.Stop();
            ts = stopWatch.ElapsedMilliseconds;
            Console.WriteLine("Perf: {0} sec for {1} items pure property access", ((double)(ts)) / 1000, iis);
        }

        [Fact]
        public void Should_Have_Setter_Perf()
        {
            Person p = new Person() { Name = "David", LastName = "D" };
            PropertyInfo pi = typeof(Person).GetProperty("Name", BindingFlags.Public | BindingFlags.Instance);
            pi.Should().NotBeNull();

            PropertyAbstraction pa = new PropertyAbstraction(pi);

            pa.Name.Should().Be("Name");
            pa[p].Should().Be("David");
            p.Name = "toto";
            pa[p].Should().Be("toto");

            GC.Collect();
            GC.WaitForFullGCComplete();

            var stopWatch = new Stopwatch();
            stopWatch.Start();

            int iis = 800000;
            for (int i = 0; i < iis; i++)
            {
                pi.SetValue(p,string.Format("{0}",i) ,null);
            }

            stopWatch.Stop();
            var ts = stopWatch.ElapsedMilliseconds;
            Console.WriteLine("Perf: {0} sec for {1} items with reflection", ((double)(ts)) / 1000, iis);

            GC.Collect();
            GC.WaitForFullGCComplete();

            stopWatch = new Stopwatch();
            stopWatch.Start();

            for (int i = 0; i < iis; i++)
            {
                pa[p] = string.Format("{0}", i);
            }

            stopWatch.Stop();
            ts = stopWatch.ElapsedMilliseconds;
            Console.WriteLine("Perf: {0} sec for {1} items with cache", ((double)(ts)) / 1000, iis);


            stopWatch.Stop();

            GC.Collect();
            GC.WaitForFullGCComplete();

            stopWatch = new Stopwatch();
            stopWatch.Start();

            for (int i = 0; i < iis; i++)
            {
                p.Name = string.Format("{0}", i);
            }

            stopWatch.Stop();
            ts = stopWatch.ElapsedMilliseconds;
            Console.WriteLine("Perf: {0} sec for {1} items pure property access", ((double)(ts)) / 1000, iis);
        }
    }
}
