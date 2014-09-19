using Awesomium.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace MVVMAwesonium.AwesomiumBinding
{
    public class JavascriptToCSharpMapper
    {
        private IWebView _IWebView;
        public JavascriptToCSharpMapper(IWebView iIWebView)
        {
            _IWebView = iIWebView;
        }

        public object GetSimpleValue(JSValue ijsvalue)
        {
            if (ijsvalue.IsString)
                return (string)ijsvalue;

            if (ijsvalue.IsInteger)
                return (int)ijsvalue;

            if (ijsvalue.IsBoolean)
                return (bool)ijsvalue;

            if (ijsvalue.IsDouble)
                return (double)ijsvalue;

            var resdate =  GetDate(ijsvalue);
            if (resdate.HasValue)
                return resdate.Value;

            return null;
        }

        private DateTime? GetDate(JSValue iJSValue)
        {
            if (!iJSValue.IsObject)
                return null;

            JSObject ob = iJSValue;

            if (ob == null)
                return null;

            JSObject ko = _IWebView.ExecuteJavascriptWithResult("ko");
            if ((bool)ko.Invoke("isDate", iJSValue) == false)
                return null;

            int year = (int)ob.Invoke("getFullYear", null);
            int month = (int)ob.Invoke("getMonth", null) + 1;
            int day = (int)ob.Invoke("getDate", null);
            int hour = (int)ob.Invoke("getHours",null);
            int minute = (int)ob.Invoke("getMinutes",null);
            int second = (int)ob.Invoke("getSeconds",null);
            int millisecond = (int)ob.Invoke("getMilliseconds",null);

            return new DateTime(year, month, day, hour, minute, second, millisecond);
        }

    }
}
