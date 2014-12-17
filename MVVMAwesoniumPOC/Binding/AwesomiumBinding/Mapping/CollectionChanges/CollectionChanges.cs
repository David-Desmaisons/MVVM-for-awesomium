using Awesomium.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace MVVMAwesomium.AwesomiumBinding
{
    public class CollectionChanges
    {
        private IJSCBridgeCache _IJSCBridgeCache;
        private Type _TargetedType;
      
        public CollectionChanges(IJSCBridgeCache iJSCBridgeCache, JSValue[] value, JSValue[] status, JSValue[] index, Type iTargetedType)
        {
            _IJSCBridgeCache = iJSCBridgeCache;
            _TargetedType = iTargetedType;
            IndividualChanges = GetIndividualChanges(value, status, index).ToList();    
        }


        public IList<IndividualCollectionChange> IndividualChanges { get; private set; }

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
    }
}
