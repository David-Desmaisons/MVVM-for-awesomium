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
        public CollectionChanges(IJSCBridgeCache iJSCBridgeCache)
        {
            _IJSCBridgeCache = iJSCBridgeCache;
        }

        public IEnumerable<IndividualCollectionChange> GetIndividualChanges(JSValue[] iChanges)
        {
            foreach(JSObject ic in iChanges)
            {
                yield return new IndividualCollectionChange(
                    (string)ic["status"] == "added" ? CollectionChangeType.Add : CollectionChangeType.Remove,
                    (int)ic["index"],
                    _IJSCBridgeCache.GetCachedOrCreateBasic(ic["value"]));
            }
        }

    }
}
