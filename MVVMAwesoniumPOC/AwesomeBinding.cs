using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.ComponentModel;
using System.Threading.Tasks;

using Awesomium.Core;

using MVVMAwesonium.Infra;
using System.Collections;
using System.Collections.Specialized;

namespace MVVMAwesonium.AwesomiumBinding
{
    public class AwesomeBinding : IDisposable
    {
        private BidirectionalMapper _BirectionalMapper;

        private AwesomeBinding(BidirectionalMapper iConvertToJSO)
        {
            _BirectionalMapper = iConvertToJSO;
        }

        internal IJSCBridge JSRootObject
        {
            get { return _BirectionalMapper.JSValueRoot; }
        }


        public void Dispose()
        {
            _BirectionalMapper.Dispose();
        }

        public static Task<AwesomeBinding> Bind(IWebView view, object iViewModel, JavascriptBindingMode iMode)
        {
            TaskCompletionSource<AwesomeBinding> tcs = new TaskCompletionSource<AwesomeBinding>();

            Action ToBeApply = () =>
                    {
                        var mapper = new BidirectionalMapper(iViewModel, view, iMode);
                        mapper.Init().ContinueWith(_ => tcs.SetResult(new AwesomeBinding(mapper)));
                    };

            if (view.IsDocumentReady)
            {
                WebCore.QueueWork(() => ToBeApply());
            }
            else
            {
                UrlEventHandler ea = null;
                ea = (o, e) => { ToBeApply(); view.DocumentReady -= ea; };
                view.DocumentReady += ea;
            }

            return tcs.Task;
        }
    }
}
