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
        private JavascriptToCSharpMapper _JavascriptToCSharpMapper;
        public CollectionChanges(IJSCBridgeCache iJSCBridgeCache, JavascriptToCSharpMapper iJavascriptToCSharpMapper)
        {
            _IJSCBridgeCache = iJSCBridgeCache;
            _JavascriptToCSharpMapper = iJavascriptToCSharpMapper;
        }

        public IJSCSGlue GetBridgeValue(JSValue iobject)
        {
            return _IJSCBridgeCache.GetCached(iobject) ?? new JSBasicObject(iobject, _JavascriptToCSharpMapper.GetSimpleValue(iobject));
        }

        public IEnumerable<IndividualCollectionChange> GetIndividualChanges(JSValue[] iChanges)
        {
            foreach(JSObject ic in iChanges)
            {
                yield return new IndividualCollectionChange(
                    (string)ic["status"] == "added" ? CollectionChangeType.Add : CollectionChangeType.Remove,
                    (int)ic["index"],
                    GetBridgeValue(ic["value"]));
            }
        }

    }
}
