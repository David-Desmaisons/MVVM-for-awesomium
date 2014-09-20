using Awesomium.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace MVVMAwesomium.AwesomiumBinding
{
    public interface IJavascriptListener
    {
        void OnJavaScriptObjectChanges(JSObject objectchanged, string PropertyName, JSValue newValue);

        void OnJavaScriptCollectionChanges(JSObject collectionchanged, JSValue[] collectionvalue, JSValue[] changes);
    }
}
