using Awesomium.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace MVVMAwesomium.Binding.AwesomiumBinding
{
    internal class ViewReadyExecuter
    {
        private IWebView _IWebView;
        private Action _Do;
        private int _DoneCount = 0;
        internal ViewReadyExecuter(IWebView iview, Action Do)
        {
            _IWebView = iview;
            _Do = Do;
        }

        internal void Do()
        {
            if (_IWebView.IsDocumentReady && !_IWebView.IsLoading)
                WebCore.QueueWork(_Do);
            else
            {
                if (_IWebView.IsDocumentReady)
                    _DoneCount++;
                else
                    _IWebView.DocumentReady += _IWebView_DocumentReady;

                if (_IWebView.IsLoading)
                    _IWebView.LoadingFrameComplete += _IWebView_LoadingFrameComplete;
                else
                    _DoneCount++;
            }
        }

        void _IWebView_LoadingFrameComplete(object sender, FrameEventArgs e)
        {
            if (!e.IsMainFrame)
                return;

            _IWebView.LoadingFrameComplete -= _IWebView_LoadingFrameComplete;
            CheckCompletion();
        }

        private void _IWebView_DocumentReady(object sender, UrlEventArgs e)
        {
            _IWebView.DocumentReady -= _IWebView_DocumentReady;
            CheckCompletion();
        }

        private void CheckCompletion()
        {
            if (++_DoneCount == 2)
                Do();
        }
    }
}
