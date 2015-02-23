using Awesomium.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace MVVMAwesomium.AwesomiumBinding
{
    public class CollectionChanges :  IComparer<IndividualCollectionChange>
    {
        private IJSCBridgeCache _IJSCBridgeCache;
        private Type _TargetedType;
      
        public CollectionChanges(IJSCBridgeCache iJSCBridgeCache, JSValue[] value, JSValue[] status, JSValue[] index, Type iTargetedType)
        {
            _IJSCBridgeCache = iJSCBridgeCache;
            _TargetedType = iTargetedType;
            IndividualChanges = GetIndividualChanges(value, status, index).OrderBy(idc => idc, this);
            //IndividualChanges = GetIndividualChanges(value, status, index).ToList();
        }


        public IEnumerable<IndividualCollectionChange> IndividualChanges { get; private set; }

        public IEnumerable<IndividualCollectionChange> GetIndividualChanges(JSValue[] value, JSValue[] status, JSValue[] index)
        {
            int Length = value.Length;
            for (int i=0;i<Length;i++)
            {
                yield return new IndividualCollectionChange(
                    (string)status[i] == "added" ? CollectionChangeType.Add : CollectionChangeType.Remove,
                    (int)index[i],
                    _IJSCBridgeCache.GetCachedOrCreateBasic(value[i], _TargetedType));
            }
        }

        public IEnumerable<IJSCSGlue> ConvertCollection(JSValue[] collectionvalue)
        {
            return collectionvalue.Select( cc=>  _IJSCBridgeCache.GetCachedOrCreateBasic(cc, _TargetedType));
        }

        public int Compare(IndividualCollectionChange x, IndividualCollectionChange y)
        {
            if (x.CollectionChangeType != y.CollectionChangeType)
                return (x.CollectionChangeType == CollectionChangeType.Add) ? 1 : -1;

            return ((x.CollectionChangeType == CollectionChangeType.Add)? 1 : -1) * (x.Index - y.Index);
        }
    }
}
