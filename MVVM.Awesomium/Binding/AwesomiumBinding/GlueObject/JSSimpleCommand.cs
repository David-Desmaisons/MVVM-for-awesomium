using Awesomium.Core;
using MVVM.Component;
using MVVMAwesomium.AwesomiumBinding;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace MVVMAwesomium.AwesomiumBinding
{
    public class JSSimpleCommand : GlueBase, IJSObservableBridge
    {
        private ISimpleCommand _JSSimpleCommand;
        public JSSimpleCommand(IJSOBuilder builder, ISimpleCommand icValue)
        {
            _JSSimpleCommand = icValue;
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

        private void Execute(JavascriptMethodEventArgs e, IJSCBridgeCache mapper)
        {
            _JSSimpleCommand.Execute(GetArguments(mapper, e));
        }

        public object CValue
        {
            get { return _JSSimpleCommand; }
        }

        public JSCSGlueType Type
        {
            get { return JSCSGlueType.SimpleCommand; }
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
