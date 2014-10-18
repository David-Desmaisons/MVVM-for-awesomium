using Awesomium.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Input;

namespace MVVMAwesomium.AwesomiumBinding
{
    public class JSCommand : IJSObservableBridge
    {
        private int _Count = 1;
        public JSCommand(IJSOBuilder builder,  ICommand  icValue)
        {
            _Command = icValue;
       
            bool canexecute = true;
            try
            {
                canexecute = _Command.CanExecute(null);
            }
            catch { }
 
            JSObject res =  builder.CreateJSO();
            res["CanExecuteValue"] = new JSValue(canexecute);
            res["CanExecuteCount"] = new JSValue(_Count);
            JSValue = res;       
        }

        public void ListenChanges()
        {
            _Command.CanExecuteChanged += _Command_CanExecuteChanged;
        }

        public void UnListenChanges()
        {
            _Command.CanExecuteChanged -= _Command_CanExecuteChanged;
        }

        private void _Command_CanExecuteChanged(object sender, EventArgs e)
        {
            WebCore.QueueWork(() =>
                    ((JSObject)_MappedJSValue).Invoke("CanExecuteCount", new JSValue(++_Count))
            );
        }

        public override string ToString()
        {
            return "{}";
        }

        public JSValue JSValue { get; private set; }

        private JSValue _MappedJSValue;

        public JSValue MappedJSValue { get { return _MappedJSValue; } }

        public void SetMappedJSValue(JSValue ijsobject, IJSCBridgeCache mapper)
        {
            _MappedJSValue = ijsobject;
            JSObject mapped = ((JSObject)_MappedJSValue);
            mapped.Bind("Execute", false, (o, e) => ExcecuteCommand(e, mapper));
            mapped.Bind("CanExecute", false, (o, e) => CanExecuteCommand(e, mapper));
        }

        private object Convert(IJSCBridgeCache mapper, JSValue value)
        {
            var found = mapper.GetCached(value);
            return (found != null) ? found.CValue : null;
        }

        private object GetArguments(IJSCBridgeCache mapper, JavascriptMethodEventArgs e)
        {
            if (e.Arguments.Length == 0)
                return null;
            else
                return Convert(mapper, e.Arguments[0]);
        }

        private void ExcecuteCommand(JavascriptMethodEventArgs e, IJSCBridgeCache mapper)
        {
            _Command.Execute(GetArguments(mapper,e));
        }

        private void CanExecuteCommand(JavascriptMethodEventArgs e, IJSCBridgeCache mapper)
        {
            bool res =_Command.CanExecute(GetArguments(mapper,e));
            ((JSObject)_MappedJSValue).Invoke("CanExecuteValue", new JSValue(res));
        }

        private ICommand _Command;
        public object CValue { get { return _Command; } }

        public JSCSGlueType Type { get { return JSCSGlueType.Command; } }

        public IEnumerable<IJSCSGlue> GetChildren()
        {
            return Enumerable.Empty<IJSCSGlue>();
        }

    }
}
