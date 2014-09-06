/* 
 * To change this license header, choose License Headers in Project Properties.
 * To change this template file, choose Tools | Templates
 * and open the template in the editor.
 * global ko
 */

( function()
{

    function PropertyListener(object, propertyname, listener) {
        return function (newvalue) {
            listener.TrackChanges(object, propertyname, newvalue);
        };
    }

    function MapToObservable(or, Listener) {
        var res = {};

        for (var att in or) {
            if (or.hasOwnProperty(att)) {
                var value = or[att];
                if ((value !== null) && (typeof value === 'object')) {
                    if (!Array.isArray(value)) {
                        res[att] = MapToObservable(value, Listener);
                    } else {
                        var nar = [];
                        for (var i in value) {
                            nar.push(MapToObservable(value[i], Listener));
                        }

                        var collection = ko.observableArray(nar);
                        res[att] = collection;

                        if ((Listener) && (Listener.RegisterCollection)) {
                            Listener.RegisterCollection(or, att, res);
                        }
                    }
                } else {
                    res[att] = ko.observable(value);
                    if ((Listener) && (Listener.TrackChanges)) {
                        res[att].subscribe(PropertyListener(or, att, Listener));
                    }
                }
            }
        }

        return res;
    }


    //global ko
    ko.MapToObservable = function (o, list) {
        return MapToObservable(o, list);
    };

//global ko 
ko.bindingHandlers.ExecuteOnEnter = {
    init: function(element, valueAccessor, allBindings, viewModel) 
    {
        try
        {
            var value = valueAccessor();
        }
        catch(exception)
        {
            console.log(exception);
        }
        $(element).keypress(function (event){
            var keycode = (event.which ? event.which : event.keyCode);
            if (keycode===13)
            {
                value.call(viewModel);
                return false;
            }
        });
    }
};
}());