using Awesomium.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using MVVMAwesomium.Infra;
using System.Collections;

namespace MVVMAwesomium.AwesomiumBinding
{
    internal class JSArray : IJSObservableBridge
    {
        public JSArray(IEnumerable<IJSCBridge> values,object collection)
        {
            JSValue = new JSValue(values.Select(v=>v.JSValue).ToArray());
            Items = new List<IJSCBridge>(values);
            CValue = collection;
        }

        private void ReplayChanges(IndividualCollectionChange change, IList ilist)
        {
            switch (change.CollectionChangeType)
            {
                case CollectionChangeType.Add:
                    if (change.Index == ilist.Count)
                    {
                        ilist.Add(change.Object.CValue);
                        Items.Add(change.Object);
                    }
                    else
                    {
                        ilist.Insert(change.Index, change.Object.CValue);
                        Items.Insert(change.Index, change.Object);
                    }
                    break;

                case CollectionChangeType.Remove:
                    ilist.RemoveAt(change.Index);
                    Items.RemoveAt(change.Index);
                    break;
            }
        }


        public void UpdateEventArgsFromJavascript(IEnumerable<IndividualCollectionChange> iChanges, IEnumerable<IJSCBridge> Current)
        {
            if (_UnderJavascriptUpdate>0)
            {
                _UnderJavascriptUpdate--;
                return;
            }

            IList ilist = CValue as IList;
            if (ilist == null) return;

            var old = Items.ToList();

            iChanges.Where(c => c.CollectionChangeType == CollectionChangeType.Remove).OrderByDescending(c => c.Index).ForEach(c => ReplayChanges(c, ilist));
            iChanges.Where(c => c.CollectionChangeType == CollectionChangeType.Add).OrderBy(c => c.Index).ForEach(c => ReplayChanges(c, ilist));

#if DEBUG
            if (!ilist.Cast<object>().SequenceEqual(Current.Select(c => c.CValue)))
                throw new Exception("Unable to track collection changes");
#endif
        }

        private int _UnderJavascriptUpdate = 0;

        public void Add(IJSCBridge iIJSCBridge, int Index)
        {
            _UnderJavascriptUpdate++;

            ((JSObject)MappedJSValue).Invoke("splice", new JSValue(Index), new JSValue(0), iIJSCBridge.GetJSSessionValue());
            if (Index > Items.Count - 1)
                Items.Add(iIJSCBridge);
            else
                Items.Insert(Index, iIJSCBridge);
        }

        public void Insert(IJSCBridge iIJSCBridge, int Index)
        {
            _UnderJavascriptUpdate++;

            ((JSObject)MappedJSValue).Invoke("splice", new JSValue(Index), new JSValue(1), iIJSCBridge.GetJSSessionValue());
            Items[Index]=iIJSCBridge;
        }

        public void Remove( int Index)
        {
            _UnderJavascriptUpdate++;

            ((JSObject)MappedJSValue).Invoke("splice", new JSValue(Index), new JSValue(1));
            Items.RemoveAt(Index);
        }

        public void Reset()
        {
            _UnderJavascriptUpdate++;

            ((JSObject)MappedJSValue).Invoke("removeAll");
            Items.Clear();
        }

        public override string ToString()
        {
            return string.Format("[{0}]", string.Join(",", Items));
        }

        public JSValue JSValue { get; private set; }

        public object CValue { get; private set; }

        public IList<IJSCBridge> Items { get; private set; }

        public IEnumerable<IJSCBridge> GetChildren()
        {
            return Items;
        }

        public JSType Type { get { return JSType.Array; } }

        public JSValue MappedJSValue { get; private set; }

        public void SetMappedJSValue(JSValue ijsobject, IJSCBridgeCache mapper)
        {
            MappedJSValue = ijsobject;
        }
    }
}
