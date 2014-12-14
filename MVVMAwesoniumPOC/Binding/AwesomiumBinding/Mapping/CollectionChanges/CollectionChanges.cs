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
        private JSValue[] _Changes;

        public CollectionChanges(IJSCBridgeCache iJSCBridgeCache, JSValue[] ichanges, Type iTargetedType)
        {
            _IJSCBridgeCache = iJSCBridgeCache;
            _Changes = ichanges;
            _TargetedType = iTargetedType;
        }

        public IEnumerable<IndividualCollectionChange> GetIndividualChanges()
        {
            foreach (JSObject ic in _Changes)
            {
                yield return new IndividualCollectionChange(
                    (string)ic["status"] == "added" ? CollectionChangeType.Add : CollectionChangeType.Remove,
                    (int)ic["index"],
                    _IJSCBridgeCache.GetCachedOrCreateBasic(ic["value"], _TargetedType));
            }
        }

        public IEnumerable<IJSCSGlue> ConvertCollection(JSValue[] collectionvalue)
        {
            return collectionvalue.Select( cc=>  _IJSCBridgeCache.GetCachedOrCreateBasic(cc, _TargetedType));
        } 
    }
}
