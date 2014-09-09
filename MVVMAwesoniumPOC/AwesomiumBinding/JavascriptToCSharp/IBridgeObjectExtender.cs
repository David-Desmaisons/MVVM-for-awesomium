using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Linq;
using System.Text;

namespace MVVMAwesonium.AwesomiumBinding
{
    public static class IBridgeObjectExtender
    {
        public static IEnumerable<IBridgeObject> GetAllChildren(this IBridgeObject @this, bool IncludeMySelf=false)
        {
            if (IncludeMySelf)
                yield return @this;

            foreach(IBridgeObject direct in @this.GetChildren())
            {
                yield return direct;

                foreach (IBridgeObject indirect in direct.GetAllChildren())
                {
                    yield return indirect;
                }
            }
        }

         public static void ApplyOnListenable(this IBridgeObject @this, Action<INotifyPropertyChanged> OnObject,
                            Action<INotifyCollectionChanged> OnCollection)
        {
            foreach (var child in @this.GetAllChildren(true))
            {
                var NotifyCollectionChanged = child.CValue as INotifyCollectionChanged;
                if (NotifyCollectionChanged != null)
                {
                    OnCollection(NotifyCollectionChanged);
                    continue;
                }

                var NotifyPropertyChanged = child.CValue as INotifyPropertyChanged;
                if ((NotifyPropertyChanged != null) && !(child is IEnumerable))
                    OnObject(NotifyPropertyChanged);
            }
        }
    }
}
