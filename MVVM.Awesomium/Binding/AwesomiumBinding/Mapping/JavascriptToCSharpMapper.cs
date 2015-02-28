﻿using Awesomium.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace MVVMAwesomium.AwesomiumBinding
{
    public class JavascriptToCSharpMapper
    {
        private IWebView _IWebView;
        public JavascriptToCSharpMapper(IWebView iIWebView)
        {
            _IWebView = iIWebView;
        }

        public object GetSimpleValue(JSValue ijsvalue, Type iTargetType=null)
        {
            if (ijsvalue.IsString)
                return (string)ijsvalue;

            if (ijsvalue.IsBoolean)
                return (bool)ijsvalue;

            object res =null;

            if (ijsvalue.IsNumber)
            {
                if (ijsvalue.IsInteger)
                    res = (int)ijsvalue;
                else if (ijsvalue.IsDouble)
                    res = (double)ijsvalue;

                if (iTargetType == null)
                    return res;
                else
                    return Convert.ChangeType(res, iTargetType);
            }

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
