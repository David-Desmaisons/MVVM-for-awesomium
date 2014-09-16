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
        basicmaped = { Name: "Albert", LastName: "Einstein", Age:55 };
        basicmaped2 = { Name: "Mickey", LastName: "Mouse" };
        basicmaped3 = { One: basicmaped2, Two: basicmaped2 };
        basicmaped4 = { One: basicmaped, Two: basicmaped2 };
        basicmaped5 = { List: ['un', 'deux', 'trois'] };
        basicmaped6 = { List: [{ Name: '1' }, { Name: '2' }, { Name: '3' }] };
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

    it("should call the mapper register with good parameters: nested", function () {
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

    it("should call the mapper register with good parameters: nested and shared", function () {
        var mapper = { Register: function () { } };
        spyOn(mapper, 'Register');
        var mapped = ko.MapToObservable(basicmaped3, mapper);

        var mapped_One = ko.MapToObservable(basicmaped3.One);
  
        expect(mapper.Register).toHaveBeenCalled();
        expect(mapper.Register).toHaveBeenCalledWith(mapped, null);
        expect(mapper.Register).toHaveBeenCalledWith(mapped_One, { object: mapped, attribute: 'One' });
  
        expect(mapper.Register.calls.count()).toEqual(2);
    });

    it("should call the mapper register with good parameters: Collection", function () {
        var mapper = { Register: function () { } };
        spyOn(mapper, 'Register');
        var mapped = ko.MapToObservable(basicmaped6, mapper);

        var mapped_One = ko.MapToObservable(basicmaped6.List[0]);
        var mapped_Two = ko.MapToObservable(basicmaped6.List[1]);
        var mapped_Three = ko.MapToObservable(basicmaped6.List[2]);

        expect(mapped.List()[0]).toBe(mapped_One);
        expect(mapped.List()[1]).toBe(mapped_Two);
        expect(mapped.List()[2]).toBe(mapped_Three);

        expect(mapper.Register).toHaveBeenCalled();
        expect(mapper.Register).toHaveBeenCalledWith(mapped, null);
        expect(mapper.Register).toHaveBeenCalledWith(mapped_One, { object: mapped, attribute: 'List', index:0 });
        expect(mapper.Register).toHaveBeenCalledWith(mapped_Two, { object: mapped, attribute: 'List', index: 1 });
        expect(mapper.Register).toHaveBeenCalledWith(mapped_Three, { object: mapped, attribute: 'List', index: 2 });
        expect(mapper.Register).toHaveBeenCalledWith(mapped.List, { object: mapped, attribute: 'List'});


        expect(mapper.Register.calls.count()).toEqual(5);
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

     it("should register TrackChanges on string", function () {
         var Listener = { TrackChanges: function () { } };
         spyOn(Listener, 'TrackChanges');

         var mapped = ko.MapToObservable(basicmaped, null, Listener);

         mapped.Name("Toto");
         
         expect(mapped.Name()).toEqual("Toto");
         expect(Listener.TrackChanges).toHaveBeenCalled();
         expect(Listener.TrackChanges.calls.count()).toEqual(1);
         expect(Listener.TrackChanges).toHaveBeenCalledWith(mapped,'Name','Toto');
     });

     it("should register TrackChanges on int", function () {
         var Listener = { TrackChanges: function () { } };
         spyOn(Listener, 'TrackChanges');

         var mapped = ko.MapToObservable(basicmaped, null, Listener);

         mapped.Age(10);

         expect(mapped.Age()).toEqual(10);
         expect(Listener.TrackChanges).toHaveBeenCalled();
         expect(Listener.TrackChanges.calls.count()).toEqual(1);
         expect(Listener.TrackChanges).toHaveBeenCalledWith(mapped, 'Age', 10);
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

     it("should not register TrackChanges on nested object", function () {
         var Listener = { TrackChanges: function () { } };
         spyOn(Listener, 'TrackChanges');

         var mapped = ko.MapToObservable(basicmaped4, null, Listener);

         var newone = {Name:"Newman"};

         mapped.One(newone)

         expect(mapped.One()).toEqual(newone);
         expect(Listener.TrackChanges.calls.count()).toEqual(0);
     });

 

 
});
