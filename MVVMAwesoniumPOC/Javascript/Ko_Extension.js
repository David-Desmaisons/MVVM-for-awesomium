/* 
 * To change this license header, choose License Headers in Project Properties.
 * To change this template file, choose Tools | Templates
 * and open the template in the editor.
 * global ko
 */

(function () {

    function PropertyListener(object, propertyname, listener) {
        return function (newvalue) {
            listener.TrackChanges(object, propertyname, newvalue);
        };
    }


    function MapToObservable(or, context, Mapper, Listener) {

        if (typeof or !== 'object') return or;

        if (!MapToObservable.Cache) {
            MapToObservable.Cache = {};
            MapToObservable._MappedId = 0;
        }

        //not very clean, but must handle "read-only" object with predefined _MappedId
        if (or._MappedId)
        {
            var tentative = MapToObservable.Cache[or._MappedId];
            if (tentative) return tentative;
        }
        else
        {
            while (MapToObservable.Cache[MapToObservable._MappedId]) { MapToObservable._MappedId++;};
            or._MappedId = MapToObservable._MappedId;
        }

        if (!Mapper) Mapper = {};
        if (!Listener) Listener = {};

        var res = {};     
        MapToObservable.Cache[or._MappedId] = res;
        if (Mapper.Register) Mapper.Register(res, context);

        for (var att in or) {
            if ((att!="_MappedId") && (or.hasOwnProperty(att))) {
                var value = or[att];
                if ((value !== null) && (typeof value === 'object')) {
                    if (!Array.isArray(value)) {
                        res[att] = ko.observable(MapToObservable(value, {
                            object: res,
                            attribute: att
                        }, Mapper, Listener));
                    } else {
                        debugger;
                        var nar = [];
                        for (var i in value) {
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

    //global ko
    ko.MapToObservable = function (o, mapper, listener) {
        return MapToObservable(o, null, mapper, listener);
    };

    //global ko 
    ko.bindingHandlers.ExecuteOnEnter = {
        init: function (element, valueAccessor, allBindings, viewModel) {
            try {
                var value = valueAccessor();
            }
            catch (exception) {
                console.log(exception);
            }
            $(element).keypress(function (event) {
                var keycode = (event.which ? event.which : event.keyCode);
                if (keycode === 13) {
                    value.call(viewModel);
                    return false;
                }
            });
        }
    };
} ());
