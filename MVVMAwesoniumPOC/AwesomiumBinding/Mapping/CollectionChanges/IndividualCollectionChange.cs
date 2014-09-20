using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace MVVMAwesomium.AwesomiumBinding
{
    public class IndividualCollectionChange
    {
        public IndividualCollectionChange(CollectionChangeType iCollectionChange, int iIndex, IJSCBridge iObject)
        {
            CollectionChangeType=iCollectionChange;
             Index=   iIndex;
             Object = iObject;
        }

        public CollectionChangeType  CollectionChangeType {get;private set;}

        public int Index { get; private set; }

        public IJSCBridge Object { get; private set; }
    }
}
