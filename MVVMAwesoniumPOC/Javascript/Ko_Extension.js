function Enum(Type, intValue, name, displayName) {
    this.intValue = intValue;
    this.displayName = displayName;
    this.name = name;
    this.type = Type;
}

//to bypass awesomium limitations
function Null_reference() {
}

(function () {

    function PropertyListener(object, propertyname, listener) {
        return function (newvalue) {
            listener.TrackChanges(object, propertyname, newvalue);
        };
    }

    function CollectionListener(object, listener) {
        return function (changes) {
            listener.TrackCollectionChanges(object, object(), changes);
        };
    }

     function createSubsription(observable, tracker,res,att) {
         if (tracker.TrackChanges) {
             listener = PropertyListener(res, att, tracker);
             observable.listener = listener;
             observable.subscriber = observable.subscribe(listener);
             observable.silent = function (v) {
                 observable.subscriber.dispose();
                 observable(v);
                 observable.subscriber = observable.subscribe(observable.listener);
             };
         }
         else
             observable.silent = observable;
     }

     function createCollectionSubsription(observable, tracker, res, att) {
         if (tracker.TrackCollectionChanges) {
             var collectionlistener = CollectionListener(res[att], tracker);
             observable.listener = collectionlistener;
             observable.subscriber = observable.subscribe(collectionlistener, null, 'arrayChange');
             observable.silent = function (fn) {
                 return function () {
                     observable.subscriber.dispose();
                     fn.apply(observable, arguments);
                     observable.subscriber = observable.subscribe(collectionlistener, null, 'arrayChange');
                 };
             };
         }
         else
             observable.silent = function (fn) {
                 return function () {
                     fn.apply(observable, arguments);
                 };
             };

         observable.silentsplice = observable.silent(observable.splice);
         observable.silentremoveAll = observable.silent(observable.removeAll);
     }


    function MapToObservable(or, context, Mapper, Listener) {

        if ((typeof or !== 'object') || (or instanceof Date) || (or instanceof Enum)) return or;

        if (or instanceof Null_reference)
            return null;

        if (!MapToObservable.Cache) {
            MapToObservable.Cache = {};
            MapToObservable._MappedId = 0;
        }

        if (!Mapper) Mapper = {};
        if (!Listener) Listener = {};

        //Look in cache
        //not very clean implementation, but must handle "read-only" object with predefined _MappedId
        if (or._MappedId !== undefined) {
            var tentative = MapToObservable.Cache[or._MappedId];
            if (tentative) {
                if ((context === null) && (Mapper.End)) Mapper.End(tentative);
                return tentative;
            }
        }
        else {
            while (MapToObservable.Cache[MapToObservable._MappedId]) { MapToObservable._MappedId++; }
            or._MappedId = MapToObservable._MappedId;
        }

        var res = {};
        MapToObservable.Cache[or._MappedId] = res;
        if (Mapper.Register) Mapper.Register(res, context);

        for (var att in or) {
            if ((att !== "_MappedId") && (or.hasOwnProperty(att))) {
                var value = or[att];
                if ((value !== null) && (typeof value === 'object')) {
                    if (!Array.isArray(value)) {
                        var comp = MapToObservable(value, {
                            object: res,
                            attribute: att
                        }, Mapper, Listener);
                        res[att] = ko.observable(comp);
                        if ((comp instanceof Date) || (comp instanceof Enum) || (value instanceof Null_reference)) {
                            createSubsription(res[att], Listener, res, att);
                        }

                    } else {
                        var nar = [];
                        for (var i = 0; i < value.length; ++i) {
                            nar.push(MapToObservable(value[i], {
                                object: res,
                                attribute: att,
                                index: i
                            }, Mapper, Listener));
                        }

                        res[att] = ko.observableArray(nar);
                        if (Mapper.Register) Mapper.Register(res[att], {
                            object: res,
                            attribute: att
                        });
                        createCollectionSubsription(res[att], Listener, res, att);
                        //if (Listener.TrackCollectionChanges) {
                        //    res[att].subscribe(CollectionListener(res[att], Listener), null, 'arrayChange');
                        //}
                    }
                } else {
                    res[att] = ko.observable(value);
                    createSubsription(res[att], Listener, res, att);
                }
            }
        }

        if ((context === null) && (Mapper.End)) Mapper.End(res);

        return res;
    }

    ko.isDate = function (o) {
        return o instanceof Date;
    };

    //global ko
    ko.MapToObservable = function (o, mapper, listener) {
        return MapToObservable(o, null, mapper, listener);
    };

    ko.bindingHandlers.command = {
        preprocess: function (value, name, addBinding) {
            addBinding('enable', value + '().CanExecute($data)===undefined &&' + value + '().CanExecuteCount() &&' + value + '().CanExecuteValue()');
            addBinding('click', 'function(){' + value + '().Execute($data);}');
            return value;
        }
    };


    ko.bindingHandlers.execute = {
        preprocess: function (value, name, addBinding) {
            addBinding('click', 'function(){' + value + '().Execute($data);}');
            return value;
        }
    };

    ko.getimage = function (Enumvalue) {
        if ((!Enumvalue instanceof Enum) || (!ko.Enumimages))
            return null;

        var ec = ko.Enumimages[Enumvalue.type];
        return ec ? ec[Enumvalue.name] : null;
    };

    ko.images = function (enumtype) {
        return (!!ko.Enumimages) ? ko.Enumimages[enumtype] : null;
    };

    ko.bindingHandlers.enumimage = {
        update: function (element, valueAccessor) {
            var v = ko.utils.unwrapObservable(valueAccessor());
            var imagepath = ko.getimage(v);
            if (imagepath) element.src=imagepath;
        }
    };

    ko.bindingHandlers.onclose = {
        preprocess: function (value) {
            return '{when: $data.__window__().State, do: ' + value + '}';
        },

        init: function (element, valueAccessor,allBindings,viewModel,bindingContext) {
            bindingContext.$data.__window__().IsListeningClose(true);
        },

        update: function (element, valueAccessor,allBindings,viewModel,bindingContext) {
            var v = ko.utils.unwrapObservable(valueAccessor());
            if (v.when().name !== 'Closing')
                return;

            v.do(function () { bindingContext.$data.__window__().CloseReady().Execute(); }, element);
        }
    };

    ko.bindingHandlers.onopened= {
        preprocess: function (value) {
            return '{when: $data.__window__().State, do: ' + value + '}';
        },

        update: function (element, valueAccessor, allBindings, viewModel, bindingContext) {
            var v = ko.utils.unwrapObservable(valueAccessor());
            if (v.when().name !== 'Opened')
                return;

            v.do(element);
        }
    };

    //improve knockout binding debug
    //allow parcial binding even if somebinding are KO
    var existing = ko.bindingProvider.instance;

        ko.bindingProvider.instance = {
            nodeHasBindings: existing.nodeHasBindings,
            getBindings: function(node, bindingContext) {
                var bindings;
                try {
                   bindings = existing.getBindings(node, bindingContext);
                }
                catch (ex) {
                   if (window.console && console.log) {
                       console.log("binding error", ex.message, node, bindingContext);
                   }

                   if (ko.log)
                       ko.log("MVVM for awesomium binding error: '" + ex.message+"'", "node HTLM: " + node.outerHTML, "context:" + ko.toJSON(bindingContext.$data));
                }

                return bindings;
            }
        };


}());
