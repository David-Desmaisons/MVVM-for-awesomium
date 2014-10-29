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

        private WindowTest BuildWindow(Func<WebControl> iWebControlFac)
        {
            return new WindowTest(
                (w) =>
                {
                    StackPanel stackPanel = new StackPanel();
                    w.Content = stackPanel;
                    var iWebControl = iWebControlFac();
                    w.RegisterName(iWebControl.Name, iWebControl);


                    stackPanel.Children.Add(iWebControl);
                }
                );
        }


        internal void TestNavigation(Action<WPFBrowserNavigator, WindowTest> Test)
        {
            AssemblyHelper.SetEntryAssembly();
            WebControl wc = null;
            Func<WebControl> Build = () =>
                {
                    wc = new WebControl() { Name = "WebControl" };
                    return wc;
                };

            using (var wcontext = BuildWindow(Build))
            {
                using( var nav = new WPFBrowserNavigator(wc,_INavigationBuilder))
                { 
                    Test(nav, wcontext);
                }
            }
        }

        private void SetUpRoute(INavigationBuilder builder)
        {
            builder.Register<A1>("javascript\\navigation_1.html");
            builder.Register<A2>("javascript\\navigation_2.html");
        }

        #region ViewModel

        private class A : ViewModelBase, INavigable
        {
            public INavigationSolver Navigation { get; set; }
        }

        private class A1 : A
        {
            public A1()
            {
                Change = new RelayCommand(() => Navigation.Navigate(new A1()));
                GoTo1 = new RelayCommand(() => Navigation.Navigate(new A2()));
            }

            public ICommand GoTo1 { get; private set; }
            public ICommand Change { get; private set; }
        }

        private class A2 : A
        {
            public A2()
            {
                GoTo1 = new RelayCommand(() => Navigation.Navigate(new A1()));
            }

            public ICommand GoTo1 { get; private set; }
        }

        #endregion

        [Fact]
        public void Test_WPFBrowserNavigator_Simple()
        {
            TestNavigation( (wpfnav, WindowTest)
                =>
                {
                    wpfnav.Should().NotBeNull();
                    SetUpRoute(_INavigationBuilder); 
                    wpfnav.UseINavigable = true;
                    var a = new A1();
                    var mre = new ManualResetEvent(false);

                    WindowTest.RunOnUIThread(
                    () =>
                    {
                        wpfnav.Navigate(a).ContinueWith
                   (
                       t =>
                       {
                           a.Navigation.Should().Be(wpfnav);
                           mre.Set();
                       });
                    });


                    mre.WaitOne();

                });
        }

        [Fact]
        public void Test_WPFBrowserNavigator_Register_ShouldNotAceptBadPath()
        {
            //TestNavigation((wpfnav, WindowTest)
            //    =>
            //    {
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

                //});
        }


        [Fact]
        public void Test_WPFBrowserNavigator_Navition_Simple()
        {
            TestNavigation((wpfnav, WindowTest)
                =>
            {
                wpfnav.Should().NotBeNull();
                SetUpRoute(_INavigationBuilder);
                wpfnav.UseINavigable = true;
                var a1 = new A1();
                var a2 = new A2();
                var mre = new ManualResetEvent(false);

                WindowTest.RunOnUIThread(
                () =>
                {
                    wpfnav.Navigate(a1).ContinueWith
               (
                   t =>
                   {
                       a1.Navigation.Should().Be(wpfnav);
                       mre.Set();
                   });
                });
                mre.WaitOne();

                WindowTest.RunOnUIThread(
                () =>
                wpfnav.WebControl.Source.LocalPath.Should().EndWith("javascript\\navigation_1.html"));

                mre = new ManualResetEvent(false);

                WindowTest.RunOnUIThread(
                () =>
                {
                    wpfnav.Navigate(a2).ContinueWith
               (
                   t =>
                   {
                       a2.Navigation.Should().Be(wpfnav);
                       mre.Set();
                   });
                });
                mre.WaitOne();

                WindowTest.RunOnUIThread(() =>
                     wpfnav.WebControl.Source.LocalPath.Should().EndWith("javascript\\navigation_2.html"));

            });
        }


        [Fact]
        public void Test_WPFBrowserNavigator_Navition_Simple_2()
        {
            TestNavigation((wpfnav, WindowTest)
                =>
            {
                wpfnav.Should().NotBeNull();
                SetUpRoute(_INavigationBuilder);
                wpfnav.UseINavigable = true;
                var a1 = new A1();
                var mre = new ManualResetEvent(false);

                WindowTest.RunOnUIThread(
                () =>
                {
                    wpfnav.Navigate(a1).ContinueWith
               (
                   t =>
                   {
                       a1.Navigation.Should().Be(wpfnav);
                       mre.Set();
                   });
                });
                mre.WaitOne();

                WindowTest.RunOnUIThread(
                () =>
                wpfnav.WebControl.Source.LocalPath.Should().EndWith("javascript\\navigation_1.html"));

        
                WindowTest.RunOnUIThread(
                () =>
                {
                    a1.GoTo1.Execute(null);
                });

                Thread.Sleep(200);

                WindowTest.RunOnUIThread(() =>
                     wpfnav.WebControl.Source.LocalPath.Should().EndWith("javascript\\navigation_2.html"));

            });
        }

        [Fact]
        public void Test_WPFBrowserNavigator_Navigation_ToSame()
        {
            TestNavigation((wpfnav, WindowTest)
                =>
            {
                wpfnav.Should().NotBeNull();
                SetUpRoute(_INavigationBuilder);
                wpfnav.UseINavigable = true;
                var a1 = new A1();
                var mre = new ManualResetEvent(false);

                WindowTest.RunOnUIThread(
                () =>
                {
                    wpfnav.Navigate(a1).ContinueWith
               (
                   t =>
                   {
                       a1.Navigation.Should().Be(wpfnav);
                       mre.Set();
                   });
                });
                mre.WaitOne();

                WindowTest.RunOnUIThread(
                () =>
                wpfnav.WebControl.Source.LocalPath.Should().EndWith("javascript\\navigation_1.html"));


                WindowTest.RunOnUIThread(
                () =>
                {
                    a1.Change.Execute(null);
                });

                Thread.Sleep(200);

                WindowTest.RunOnUIThread(() =>
                     wpfnav.WebControl.Source.LocalPath.Should().EndWith("javascript\\navigation_1.html"));

            });
        }

        [Fact]
        public void Test_WPFBrowserNavigator_Navigation_ToNull()
        {
            TestNavigation((wpfnav, WindowTest)
                =>
            {
                wpfnav.Should().NotBeNull();
                SetUpRoute(_INavigationBuilder);
                wpfnav.UseINavigable = true;
                var a1 = new A1();
                var mre = new ManualResetEvent(false);

                WindowTest.RunOnUIThread(
                () =>
                {
                    wpfnav.Navigate(a1).ContinueWith
               (
                   t =>
                   {
                       a1.Navigation.Should().Be(wpfnav);
                       mre.Set();
                   });
                });
                mre.WaitOne();

                WindowTest.RunOnUIThread(
                () =>
                wpfnav.WebControl.Source.LocalPath.Should().EndWith("javascript\\navigation_1.html"));


                WindowTest.RunOnUIThread(() =>
                 wpfnav.Navigate(null).ContinueWith
               (
                   t =>
                   {
                       a1.Navigation.Should().Be(wpfnav);
                       mre.Set();
                   }));

                 mre.WaitOne();

                Thread.Sleep(200);

                WindowTest.RunOnUIThread(() =>
                     wpfnav.WebControl.Source.LocalPath.Should().EndWith("javascript\\navigation_1.html"));

            });
        }

        [Fact]
        public void Test_WPFBrowserNavigator_Navition_Resolve_OnBaseType()
        {
            TestNavigation((wpfnav, WindowTest)
                =>
            {
                wpfnav.Should().NotBeNull();
                _INavigationBuilder.Register<A>("javascript\\navigation_1.html");
                _INavigationBuilder.Register<A1>("javascript\\navigation_2.html", "Special");

                wpfnav.UseINavigable = true;
                var a1 = new A1();
                var mre = new ManualResetEvent(false);

                WindowTest.RunOnUIThread(
               () =>
               {
                   wpfnav.Navigate(a1).ContinueWith
              (
                  t =>
                  {
                      a1.Navigation.Should().Be(wpfnav);
                      mre.Set();
                  });
               });

                mre.WaitOne();

                WindowTest.RunOnUIThread(
                () =>
                wpfnav.WebControl.Source.LocalPath.Should().EndWith("javascript\\navigation_1.html"));

            });
          }

        //string.Format("{0}\\{1}", Assembly.GetCallingAssembly().GetPath(), iPath)

        [Fact]
        public void Test_WPFBrowserNavigator_Navition_Resolve_OnName_alernativesignature()
        {
            TestNavigation((wpfnav, WindowTest)
                =>
            {
                wpfnav.Should().NotBeNull();
                _INavigationBuilder.RegisterAbsolute<A2>(string.Format("{0}\\{1}", Assembly.GetExecutingAssembly().GetPath(), "javascript\\navigation_1.html"), "Special1");
                _INavigationBuilder.Register<A2>(new Uri(string.Format("{0}\\{1}", Assembly.GetExecutingAssembly().GetPath(), "javascript\\navigation_2.html")), "Special2");

                wpfnav.UseINavigable = true;
                var a1 = new A2();
                var mre = new ManualResetEvent(false);

                WindowTest.RunOnUIThread(
               () =>
               {
                   wpfnav.Navigate(a1, "Special1").ContinueWith
              (
                  t =>
                  {
                      a1.Navigation.Should().Be(wpfnav);
                      mre.Set();
                  });
               });

                mre.WaitOne();

                WindowTest.RunOnUIThread(
                () =>
                wpfnav.WebControl.Source.LocalPath.Should().EndWith("javascript\\navigation_1.html"));


                WindowTest.RunOnUIThread(
             () =>
             {
                 wpfnav.Navigate(a1, "Special2").ContinueWith
            (
                t =>
                {
                    a1.Navigation.Should().Be(wpfnav);
                    mre.Set();
                });
             });

                mre.WaitOne();

                WindowTest.RunOnUIThread(
                () =>
                wpfnav.WebControl.Source.LocalPath.Should().EndWith("javascript\\navigation_2.html"));

            });
        }

        [Fact]
        public void Test_WPFBrowserNavigator_Navition_Resolve_OnName()
        {
            TestNavigation((wpfnav, WindowTest)
                =>
            {
                wpfnav.Should().NotBeNull();
                _INavigationBuilder.Register<A2>("javascript\\navigation_1.html", "Special1");
                _INavigationBuilder.Register<A2>("javascript\\navigation_2.html", "Special2");

                wpfnav.UseINavigable = true;
                wpfnav.UseINavigable = true;
                var a1 = new A2();
                var mre = new ManualResetEvent(false);

                WindowTest.RunOnUIThread(
               () =>
               {
                   wpfnav.Navigate(a1, "Special1").ContinueWith
              (
                  t =>
                  {
                      a1.Navigation.Should().Be(wpfnav);
                      mre.Set();
                  });
               });

                mre.WaitOne();

                WindowTest.RunOnUIThread(
                () =>
                wpfnav.WebControl.Source.LocalPath.Should().EndWith("javascript\\navigation_1.html"));


                WindowTest.RunOnUIThread(
             () =>
             {
                 wpfnav.Navigate(a1, "Special2").ContinueWith
            (
                t =>
                {
                    a1.Navigation.Should().Be(wpfnav);
                    mre.Set();
                });
             });

                mre.WaitOne();

                WindowTest.RunOnUIThread(
                () =>
                wpfnav.WebControl.Source.LocalPath.Should().EndWith("javascript\\navigation_2.html"));

            });
        }

        [Fact]
        public void Test_WPFBrowserNavigator_Navition_Resolve_NotFound()
        {
            TestNavigation((wpfnav, WindowTest)
                =>
            {
                wpfnav.Should().NotBeNull();
  
                wpfnav.UseINavigable = true;
                var a1 = new A2();

                WindowTest.RunOnUIThread(
               () =>
               {
                   Action wf = () =>wpfnav.Navigate(a1);
                   wf.ShouldThrow<Exception>();
               });

            });
        }

        [Fact]
        public void Test_WPFBrowserNavigator_Navition_Resolve_OnBaseType_2()
        {
            TestNavigation((wpfnav, WindowTest)
                =>
            {
                wpfnav.Should().NotBeNull();
                _INavigationBuilder.Register<A>("javascript\\navigation_1.html");
                _INavigationBuilder.Register<A1>("javascript\\navigation_2.html", "Special");

                wpfnav.UseINavigable = true;
                var a1 = new A2();
                var mre = new ManualResetEvent(false);

                WindowTest.RunOnUIThread(
               () =>
               {
                   wpfnav.Navigate(a1).ContinueWith
              (
                  t =>
                  {
                      a1.Navigation.Should().Be(wpfnav);
                      mre.Set();
                  });
               });

                mre.WaitOne();

                WindowTest.RunOnUIThread(
                () =>
                wpfnav.WebControl.Source.LocalPath.Should().EndWith("javascript\\navigation_1.html"));

            });
        }

        [Fact]
        public void Test_WPFBrowserNavigator_Navition_Resolve_OnBaseType_UsingName()
        {
            TestNavigation((wpfnav, WindowTest)
                =>
            {
                wpfnav.Should().NotBeNull();
                _INavigationBuilder.Register<A>("javascript\\navigation_1.html");
                _INavigationBuilder.Register<A1>("javascript\\navigation_2.html", "Special");

                wpfnav.UseINavigable = true;
                var a1 = new A1();
                var mre = new ManualResetEvent(false);

                WindowTest.RunOnUIThread(
               () =>
               {
                   wpfnav.Navigate(a1, "Special").ContinueWith
              (
                  t =>
                  {
                      a1.Navigation.Should().Be(wpfnav);
                      mre.Set();
                  });
               });

                mre.WaitOne();

                WindowTest.RunOnUIThread(
                () =>
                wpfnav.WebControl.Source.LocalPath.Should().EndWith("javascript\\navigation_2.html"));

            });
        }

        [Fact]
        public void Test_WPFBrowserNavigator_Navition_Resolve_OnBaseType_ShoulFailed()
        {
            TestNavigation((wpfnav, WindowTest)
                =>
            {
                wpfnav.Should().NotBeNull();
                _INavigationBuilder.Register<A>("javascript\\navigation_1.html");
                _INavigationBuilder.Register<A1>("javascript\\navigation_2.html", "Special");

                wpfnav.UseINavigable = true;
                var a1 = new object();
     
                WindowTest.RunOnUIThread(
               () =>
               {
                   Action wf = () => wpfnav.Navigate(a1);
                   wf.ShouldThrow<Exception>();
               });

            });
        }
    }

}
