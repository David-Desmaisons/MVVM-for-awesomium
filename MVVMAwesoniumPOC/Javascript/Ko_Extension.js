/* 
 * To change this license header, choose License Headers in Project Properties.
 * To change this template file, choose Tools | Templates
 * and open the template in the editor.
 * global ko
 */

( function()
{
function MapToObservable(or,Listener)
{
    var res = {};

    //if ((Listener) && (Listener.RegisterMapping))
    //    Listener.RegisterMapping(res, or);

    for(var att in or)
    {
        if (or.hasOwnProperty(att))
        {
            var value = or[att];
            if ((value!==null)&& (typeof value==='object'))
            {
                if (!Array.isArray(value))
                {
                    res[att] = MapToObservable(value);
                }
                else
                {
                    var nar = [];
                    for (var i in value)
                    {
                        nar.push(MapToObservable(value[i]));
                    }
                    //global ko
                    var collection = ko.observableArray(nar);
                    res[att] = collection;
                    //Listener.RegisterMapping(collection, value);
                }
            }
            else
            {
                res[att] = ko.observable(value);

                if ((Listener) && (Listener.TrackChanges))
                    res[att].Subscribe(function (newvalue) { Listener.TrackChanges(value, att, newvalue); });

            }
        }
    }
        
    return res;
}


//global ko
ko.MapToObservable = function (o){ return MapToObservable(o);};

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

    //ko.MapToCommitObservable = function (o)
    //{ 
    //    function MyCommit()
    //    {
    //        for(var att in this)
    //        {   
    //            if (this.hasOwnProperty(att) && this[att].commit)
    //            {
    //                this[att].commit();
    //            }
    //        }
    //    }
    //    return MapToObservable(o,protectedObservable,MyCommit);
    //};
    //function protectedObservable(initialValue) {
    //    //private variables
    //    var _temp = initialValue;
    //    var _actual = ko.observable(initialValue);

    //    var result = ko.dependentObservable({
    //        read: _actual,
    //        write: function(newValue) {
    //            _temp = newValue;
    //        }
    //    }).extend({ notify: "always" }); //needed in KO 3.0+ for reset, as computeds no longer notify when value is the same

    //    //commit the temporary value to our observable, if it is different
    //    result.commit = function() {
    //        if (_temp !== _actual()) {
    //            _actual(_temp);
    //        }
    //    };

    //    //notify subscribers to update their value with the original
    //    result.reset = function() {
    //        _actual.valueHasMutated();
    //        _temp = _actual();
    //    };

    //    return result;
    //};

    //ko.protectedObservable = protectedObservable;
}());