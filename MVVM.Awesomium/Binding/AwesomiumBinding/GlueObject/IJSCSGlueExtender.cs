﻿using Awesomium.Core;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Linq;
using System.Text;

namespace MVVMAwesomium.AwesomiumBinding
{
    public static class IJSCSGlueExtender
    {
        private static void GetAllChildren(this IJSCSGlue @this, bool IncludeMySelf, HashSet<IJSCSGlue> res)
        {
            if (IncludeMySelf)
                res.Add(@this);

            foreach (IJSCSGlue direct in @this.GetChildren())
            {
                if (res.Add(direct))
                {
                    direct.GetAllChildren(false, res);
                }
            }
        }

        public static IEnumerable<IJSCSGlue> GetAllChildren(this IJSCSGlue @this, bool IncludeMySelf=false)
        {
            HashSet<IJSCSGlue> res = new HashSet<IJSCSGlue>();
            @this.GetAllChildren(IncludeMySelf,res);
            return res;
        }

        public static JSValue GetJSSessionValue(this IJSCSGlue @this)
        {
            IJSObservableBridge inj = @this as IJSObservableBridge;
            return (inj!=null) ?  inj.MappedJSValue : @this.JSValue;    
        }

         public static void ApplyOnListenable(this IJSCSGlue @this, IJSCSGlueListenableVisitor ivisitor)
        {
            foreach (var child in @this.GetAllChildren(true).Distinct())
            {
                var c_childvalue = child.CValue;
                var NotifyCollectionChanged = c_childvalue as INotifyCollectionChanged;
                if (NotifyCollectionChanged != null)
                {
                    ivisitor.OnCollection(NotifyCollectionChanged);
                    continue;
                }

                var NotifyPropertyChanged = c_childvalue as INotifyPropertyChanged;
                if ((NotifyPropertyChanged != null) && !(child is IEnumerable))
                    ivisitor.OnObject(NotifyPropertyChanged);

                if (child.Type==JSCSGlueType.Command)
                    ivisitor.OnCommand(child as JSCommand);
            }
        }
    }
}
