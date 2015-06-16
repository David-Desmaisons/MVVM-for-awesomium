(function () {

    ko.register = function (vm) {
        vm.click = function () {
            executeAsPromise(vm, 'CreateObject',vm.Name()).then(
                function (res) {
                    alert(res.LastName());
                }
            ).catch(function(reason) {
                console.log(reason);
            });
        };
    };
})()