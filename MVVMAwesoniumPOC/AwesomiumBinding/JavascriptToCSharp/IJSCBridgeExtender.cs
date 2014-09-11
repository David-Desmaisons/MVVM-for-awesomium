using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Linq;
using System.Text;

namespace MVVMAwesonium.AwesomiumBinding
{
    public static class IJSCBridgeExtender
    {
        public static IEnumerable<IJSCBridge> GetAllChildren(this IJSCBridge @this, bool IncludeMySelf=false)
        {
            if (IncludeMySelf)
                yield return @this;

            foreach(IJSCBridge direct in @this.GetChildren())
            {
                yield return direct;

                foreach (IJSCBridge indirect in direct.GetAllChildren())
                {
                    yield return indirect;
                }
            }
        }

         public static void ApplyOnListenable(this IJSCBridge @this, Action<INotifyPropertyChanged> OnObject,
                            Action<INotifyCollectionChanged> OnCollection)
        {
            foreach (var child in @this.GetAllChildren(true).Select(c => c.CValue).Distinct())
            {
                var NotifyCollectionChanged = child as INotifyCollectionChanged;
                if (NotifyCollectionChanged != null)
                {
                    OnCollection(NotifyCollectionChanged);
                    continue;
                }

                var NotifyPropertyChanged = child as INotifyPropertyChanged;
                if ((NotifyPropertyChanged != null) && !(child is IEnumerable))
                    OnObject(NotifyPropertyChanged);
            }
        }
    }
}
