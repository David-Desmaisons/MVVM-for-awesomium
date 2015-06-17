(function () {

    ko.register = function (vm) {
        vm.factotyresult = ko.observable(null);

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