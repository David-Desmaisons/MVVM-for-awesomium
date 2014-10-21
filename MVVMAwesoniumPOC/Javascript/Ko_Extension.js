function Enum(Type, intValue, Name, displayName) {
    this.intValue = intValue;
    this.Name = Name;
    this.displayName = displayName;
    this.type = Type;
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


    function MapToObservable(or, context, Mapper, Listener) {

        if ((typeof or !== 'object') || (or instanceof Date) || (or instanceof Enum)) return or;

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
                        if ((Listener.TrackChanges) && ((comp instanceof Date) || comp instanceof Enum)) {
                            res[att].subscribe(PropertyListener(res, att, Listener));
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
                        if (Listener.TrackCollectionChanges) {
                            res[att].subscribe(CollectionListener(res[att], Listener), null, 'arrayChange');
                        }
                    }
                } else {
                    res[att] = ko.observable(value);
                    if (Listener.TrackChanges) {
                        res[att].subscribe(PropertyListener(res, att, Listener));
                    }
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
        if (!Enumvalue instanceof Enum)
            return null;

        var ec = ko.EnumImages[Enumvalue.type];
        return ec ? ec[Enumvalue.Name] : null;
    };

    ko.enumHasImage = function (Enumtype) {
        return ko.EnumImages[Enumtype];
   };

    ko.bindingHandlers.enumimage = {
        preprocess: function (value, name, addBinding) {
            addBinding('attr', '{src: ko.getimage('+value+')}');
            return value;
        }
    };

}());
