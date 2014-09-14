/* 
 * To change this license header, choose License Headers in Project Properties.
 * To change this template file, choose Tools | Templates
 * and open the template in the editor.
 */
/// <reference path="../../../MVVMAwesoniumPOC/Javascript/knockout.js" />
/// <reference path="../../../MVVMAwesoniumPOC/Javascript//Ko_Extension.js" />


describe("MapToObservable", function () {
    var basicmaped = { Name: "Albert", LastName: "Einstein" };
    var basicmaped2 = { Name: "Mickey", LastName: "Mouse" };

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
});
