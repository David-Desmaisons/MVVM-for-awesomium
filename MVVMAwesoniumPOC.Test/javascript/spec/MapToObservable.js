/* 
 * To change this license header, choose License Headers in Project Properties.
 * To change this template file, choose Tools | Templates
 * and open the template in the editor.
 */

describe("MapToObservable", function () {
    var basicmaped = { Name: "Albert", LastName: "Einstein" };

    it("should map basic", function () {
        var mapped = ko.MapToObservable(basicmaped);

        expect(mapped).not.toBeNull();
        expect(mapped.Name()).toBe("Albert");
        expect(mapped.LastName()).toBe("Einstein");

    });
});
