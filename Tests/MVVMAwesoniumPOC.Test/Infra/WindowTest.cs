﻿using Awesomium.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Threading;

namespace MVVMAwesomium.Test
{

    internal class WindowTest : IDisposable
    {
        private WPFThreadingHelper _WPFThreadingHelper;


        public WindowTest(Action<Window> Init)
        {
            _WPFThreadingHelper = new WPFThreadingHelper(
                () =>
                {
                    var window = new Window();
                    NameScope.SetNameScope(window, new NameScope());
                    Init(window);
                    return window;
                }
                );
        }

        public Window Window { get { return _WPFThreadingHelper.MainWindow; } }

        public Dispatcher Dispatcher { get { return Window.Dispatcher; } }


        public void RunOnUIThread(Action Do)
        {
            Dispatcher.Invoke(Do);
        }

        public void Dispose()
        {
            Action End = () => { _WPFThreadingHelper.Close(); };
            Dispatcher.Invoke(End);
            WebCore.QueueWork(()  =>_WPFThreadingHelper.Dispose());
        }
    }
}
