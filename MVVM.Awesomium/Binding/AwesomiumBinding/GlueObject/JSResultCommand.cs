using Awesomium.Core;
using MVVM.Component;
using MVVMAwesomium.AwesomiumBinding;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MVVMAwesomium.AwesomiumBinding
{
    public class JSResultCommand : GlueBase, IJSObservableBridge
    {
        private IResultCommand _JSResultCommand;
        public JSResultCommand(IJSOBuilder builder, IResultCommand icValue)
        {
            _JSResultCommand = icValue;
            JSValue = builder.CreateJSO();    
        }

        public JSValue JSValue { get; private set; }

        private JSValue _MappedJSValue;

        public JSValue MappedJSValue { get { return _MappedJSValue; } }

        public void SetMappedJSValue(JSValue ijsobject, IJSCBridgeCache mapper)
        {
            _MappedJSValue = ijsobject;
            ((JSObject)_MappedJSValue).Bind("Execute", false, (o, e) => Execute(e, mapper));
        }

        private object Convert(IJSCBridgeCache mapper, JSValue value)
        {
            var found = mapper.GetCachedOrCreateBasic(value, null);
            return (found != null) ? found.CValue : null;
        }

        private object GetArguments(IJSCBridgeCache mapper, JavascriptMethodEventArgs e)
        {
            return (e.Arguments.Length == 0) ? null : Convert(mapper, e.Arguments[0]);
        }

        private void SetResult(JavascriptMethodEventArgs e, IJSCBridgeCache bridge, Task<object> resulttask)
        {
            WebCore.QueueWork(()=>
                {
                    var exception = resulttask.Exception;
                    JSObject promise = e.Arguments[1];
                    if (exception != null)
                    {
                        promise.Invoke("reject", exception.Message);
                    }
                    else
                    {
                        bridge.RegisterInSession(resulttask.Result, (bridgevalue) =>
                            {
                                promise.Invoke("fullfill", bridgevalue.GetJSSessionValue());
                            });
                    }
                });
        }

        private void Execute(JavascriptMethodEventArgs e, IJSCBridgeCache mapper)
        {
           _JSResultCommand.Execute(GetArguments(mapper, e))
               .ContinueWith(t => SetResult(e, mapper,t));
        }

        public object CValue
        {
            get { return _JSResultCommand; }
        }

        public JSCSGlueType Type
        {
            get { return JSCSGlueType.ResultCommand; }
        }

        public IEnumerable<IJSCSGlue> GetChildren()
        {
            return Enumerable.Empty<IJSCSGlue>();
        }

        protected override void ComputeString(StringBuilder sb, HashSet<IJSCSGlue> alreadyComputed)
        {
            sb.Append("{}");
        }
    }
}
