using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Awesomium.Core;

namespace MVVMAwesonium.AwesomiumBinding
{
    internal class JSListener : IJSListener
    {
        private JSObject _JSO;
        public JSListener(JSObject iJSO)
        {
            _JSO = iJSO;
        }

        internal JSObject JSObject { get { return _JSO; } }

        public void Dispose()
        {
            if (_JSO != null)
            {
                _JSO.Dispose();
                _JSO = null;
            }
        }
    }
}
