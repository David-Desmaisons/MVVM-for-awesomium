(function () {

    var getpromisebegin = 'function(argv){ return new Promise(function (fullfill, reject) { var res = { fullfill: function (res) { fullfill(res); }, reject: function (err) { reject(new Error(err)); } };';

    var getpromiseend = '().Execute(argv, res);}); }';

 ko.bindingHandlers.executeResult = {
     preprocess: function (value, name, addBinding) {
            var res = getpromisebegin + value + getpromiseend;
            console.log(res);
            return res;
        },

        update: function (element, valueAccessor, allBindings) {
            var promiseresult = allBindings.get('promiseoption'),
                then = typeof promiseresult == 'function' ? promiseresult : promiseresult.then,
                error = promiseresult.error || function () { },
                arg = promiseresult.arg,
                eventname = promiseresult.event || 'click', value = valueAccessor();
            console.log(ko.utils.unwrapObservable(arg));

            var old = ko.utils.domData.get(element,'promisehandler');

            if (!!old){
                element.removeEventListener(eventname, old);

            }

            var f = function () { value(ko.utils.unwrapObservable(arg)).then(then).catch(error); };
            ko.utils.domData.set(element,'promisehandler',f);
            element.addEventListener(eventname, f, false);
        }
    };

    ko.register = function (vm) {
        vm.factotyresult = ko.observable(null);

        vm.result = function (res) {
            vm.factotyresult(res);
        };

        vm.error = function (res) {
            console.log(reason);
        };

        vm.click = function () {
            executeAsPromise(vm, 'CreateObject',vm.Name()).then(
                function (res) {
                    alert(res.LastName());
                    vm.factotyresult(res);
                }
            ).catch(function(reason) {
                console.log(reason);
            });
        };
    };
})()