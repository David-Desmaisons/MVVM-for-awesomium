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


     function MapToObservable(or, first, Mapper, Listener) {
         var res = {};

         if (!Mapper) Mapper = {};
         if (!Listener) Listener = {};

         if (first && (Mapper.RegisterFirst)) Mapper.RegisterFirst(res);

         for (var att in or) {
             if (or.hasOwnProperty(att)) {
                 var value = or[att];
                 if ((value !== null) && (typeof value === 'object')) {
                     if (!Array.isArray(value)) {
                         res[att] = ko.observable(MapToObservable(value, false, Mapper, Listener));
                         if (Mapper.RegisterMapping) Mapper.RegisterMapping(res, att, -1,res[att]());
                     } else {
                         var nar = [];
                         for (var i in value) {
                             var eli = MapToObservable(value[i], false, Mapper, Listener);
                             nar.push(eli);
                             if (Mapper.RegisterMapping) Mapper.RegisterMapping(res, att, i, eli);
                         }

                         res[att] = ko.observableArray(nar);
                         if ( Mapper.RegisterMapping) Mapper.RegisterMapping(res, att, -1, res[att]);
                     }
                 } else {
                     res[att] = ko.observable(value);
                     if (Listener.TrackChanges) {
                         res[att].subscribe(PropertyListener(res, att, Listener));
                     }
                 }
             }
         }

         if (first && (Mapper.End)) Mapper.End(res);

         return res;
     }


    //global ko
     ko.MapToObservable = function (o, mapper, listener) {
         return MapToObservable(o, true, mapper, listener);
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
