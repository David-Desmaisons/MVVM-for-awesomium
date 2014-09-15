/* 
 * To change this license header, choose License Headers in Project Properties.
 * To change this template file, choose Tools | Templates
 * and open the template in the editor.
 */
/// <reference path="../../../MVVMAwesoniumPOC/Javascript/knockout.js" />
/// <reference path="../../../MVVMAwesoniumPOC/Javascript//Ko_Extension.js" />


describe("Map To Observable", function () {
    var basicmaped, basicmaped2, basicmaped3, basicmaped4, basicmaped5;


    beforeEach(function() {
        basicmaped = { Name: "Albert", LastName: "Einstein" };
        basicmaped2 = { Name: "Mickey", LastName: "Mouse" };
        basicmaped3 = { One: basicmaped2, Two: basicmaped2 };
        basicmaped4 = { One: basicmaped, Two: basicmaped2 };
        basicmaped5 = { List: ['un', 'deux', 'trois'] };
    });

    it("should map basic property", function () {
        var mapped = ko.MapToObservable(basicmaped);

        expect(mapped).not.toBeNull();
        expect(mapped.Name()).toBe("Albert");
        expect(mapped.LastName()).toBe("Einstein");
    });

    it("should map collection", function () {
        var mapped = ko.MapToObservable(basicmaped5);

        expect(mapped).not.toBeNull();
        expect(mapped.List()).not.toBeNull();
        expect(mapped.List().length).toBe(3);
        expect(mapped.List()).toContain("un");
        expect(mapped.List()).toContain("deux");
        expect(mapped.List()).toContain("trois");
    });

    it("should preserve references", function () {
        var mapped = ko.MapToObservable(basicmaped3);
        var mapped2 = ko.MapToObservable(basicmaped2);

        expect(mapped.One()).toBe(mapped.Two());
        expect(mapped.One()).toBe(mapped2);
    });

    it("should use caching", function () {
        var mapped = ko.MapToObservable(basicmaped2);
        var mapped2 = ko.MapToObservable(basicmaped2);

        expect(mapped).toBe(mapped2);
    });

    it("should work with nested", function () {
        var mapped = ko.MapToObservable(basicmaped4);

        expect(mapped.One().Name()).toBe("Albert");
        expect(mapped.Two().Name()).toBe("Mickey");
    });

    it("should call the mapper register", function () {
        var mapper = { Register: function () { } };

        spyOn(mapper, 'Register');

        var mapped = ko.MapToObservable(basicmaped, mapper);

        expect(mapper.Register).toHaveBeenCalled();
        expect(mapper.Register).toHaveBeenCalledWith(mapped, null);
        expect(mapper.Register.calls.count()).toEqual(1);
    });

    it("should call the mapper register with good parameters", function () {
        var mapper = { Register: function () { } };

        spyOn(mapper, 'Register');

        var mapped = ko.MapToObservable(basicmaped4, mapper);
        var mapped_One = ko.MapToObservable(basicmaped4.One, mapper);
        var mapped_Two = ko.MapToObservable(basicmaped4.Two, mapper);

        expect(mapper.Register).toHaveBeenCalled();
        expect(mapper.Register).toHaveBeenCalledWith(mapped, null);
        expect(mapper.Register).toHaveBeenCalledWith(mapped_One, { object: mapped, attribute: 'One' });
        expect(mapper.Register).toHaveBeenCalledWith(mapped_Two, { object: mapped, attribute: 'Two' });

        expect(mapper.Register.calls.count()).toEqual(3);
    });

    it("should call the mapper register with good parameters 2", function () {
        var mapper = { Register: function () { } };
        spyOn(mapper, 'Register');
        var mapped = ko.MapToObservable(basicmaped3, mapper);

        var mapped_One = ko.MapToObservable(basicmaped3.One);
  
        expect(mapper.Register).toHaveBeenCalled();
        expect(mapper.Register).toHaveBeenCalledWith(mapped, null);
        expect(mapper.Register).toHaveBeenCalledWith(mapped_One, { object: mapped, attribute: 'One' });
  
        expect(mapper.Register.calls.count()).toEqual(2);
    });


    it("should call not register when object is cached", function () {
        var mapped = ko.MapToObservable(basicmaped);

        var mapper = { Register: function () { } };
        spyOn(mapper, 'Register');

        var mapped2 = ko.MapToObservable(basicmaped, mapper);

        expect(mapper.Register).not.toHaveBeenCalled();
    });


     it("should call the mapper End", function () {
        var mapper = { End: function () { } };

        spyOn(mapper, 'End');

        var mapped = ko.MapToObservable(basicmaped, mapper);

        expect(mapper.End).toHaveBeenCalled();
        expect(mapper.End.calls.count()).toEqual(1);
        expect(mapper.End).toHaveBeenCalledWith(mapped);
     });

     it("should call the mapper End even when cached", function () {
         var mapped0 = ko.MapToObservable(basicmaped);

         var mapper = { End: function () { } };
         spyOn(mapper, 'End');

         var mapped1 = ko.MapToObservable(basicmaped, mapper);

         expect(mapper.End).toHaveBeenCalled();
         expect(mapper.End.calls.count()).toEqual(1);
         expect(mapper.End).toHaveBeenCalledWith(mapped0);
     });

     it("should register TrackChanges", function () {
         var Listener = { TrackChanges: function () { } };
         spyOn(Listener, 'TrackChanges');

         var mapped = ko.MapToObservable(basicmaped, null, Listener);

         mapped.Name("Toto");
         
         expect(mapped.Name()).toEqual("Toto");
         expect(Listener.TrackChanges).toHaveBeenCalled();
         expect(Listener.TrackChanges.calls.count()).toEqual(1);
         expect(Listener.TrackChanges).toHaveBeenCalledWith(mapped,'Name','Toto');
     });

     it("should register nested TrackChanges", function () {
         var Listener = { TrackChanges: function () { } };
         spyOn(Listener, 'TrackChanges');

         var mapped = ko.MapToObservable(basicmaped4, null, Listener);

         mapped.One().Name("Titi");

         expect(mapped.One().Name()).toEqual("Titi");
         expect(Listener.TrackChanges).toHaveBeenCalled();
         expect(Listener.TrackChanges.calls.count()).toEqual(1);
         expect(Listener.TrackChanges).toHaveBeenCalledWith(mapped.One(), 'Name', 'Titi');
     });

 

 
});
