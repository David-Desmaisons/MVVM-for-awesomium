/* 
 * To change this license header, choose License Headers in Project Properties.
 * To change this template file, choose Tools | Templates
 * and open the template in the editor.
 */
/// <reference path="../../../MVVMAwesoniumPOC/Javascript/knockout.js" />
/// <reference path="../../../MVVMAwesoniumPOC/Javascript//Ko_Extension.js" />


describe("Map To Observable", function () {
    var basicmaped, basicmaped2, basicmaped3, basicmaped4;


    beforeEach(function() {
        basicmaped = { Name: "Albert", LastName: "Einstein" };
        basicmaped2 = { Name: "Mickey", LastName: "Mouse" };
        basicmaped3 = { One: basicmaped2, Two: basicmaped2 };
        basicmaped4 = { One: basicmaped, Two: basicmaped2 };
    });

    it("should map basic property", function () {
        var mapped = ko.MapToObservable(basicmaped);

        expect(mapped).not.toBeNull();
        expect(mapped.Name()).toBe("Albert");
        expect(mapped.LastName()).toBe("Einstein");
    });

    it("should should use caching", function () {
        var mapped = ko.MapToObservable(basicmaped2);
        var mapped2 = ko.MapToObservable(basicmaped2);

        expect(mapped).toBe(mapped2);
    });

    it("should should work with nested", function () {
        var mapped = ko.MapToObservable(basicmaped4);

        expect(mapped.One().Name()).toBe("Albert");
        expect(mapped.Two().Name()).toBe("Mickey");
    });

    it("should should preserve references", function () {
        var mapped = ko.MapToObservable(basicmaped3);
        var mapped2 = ko.MapToObservable(basicmaped2);

        expect(mapped.One()).toBe(mapped.Two());
        expect(mapped.One()).toBe(mapped2);
    });
});
