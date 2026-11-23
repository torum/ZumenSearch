using Microsoft.Data.Sqlite;
using ZumenSearch.Application.SearchResults;
using ZumenSearch.Domain.Entities.Person;
using ZumenSearch.Domain.Entities.Rent.Residentials;
using ZumenSearch.Domain.Enums;
using ZumenSearch.Domain.Enums.Rent.Residentials;
using ZumenSearch.Infrastructure.DataAccess;

namespace ZumenSearch.Tests;

[TestClass]
public sealed class RentResidentialWorkflowTests
{
    [TestMethod]
    public void PropertyAttachments_SaveAndReloadMetadataForCorrectOwner()
    {
        using var db = new TestDatabase();
        var property = NewProperty("property-a", "Building A");
        property.Pictures.Add(new PropertyPicture("property-picture", "exterior.jpg", EntityStatus.New) { PictureType = PropertyPictureType.Gaikan, Description = "Exterior", IsMain = true });
        property.Pdfs.Add(new PropertyPdf("property-pdf", "plan.pdf", "plan.png", EntityStatus.New) { PdfType = PropertyPdfType.Architectural, Description = "Building plan", IsMain = true });
        Assert.IsFalse(db.Access.UpsertRentResidential(property).IsError);
        Assert.IsFalse(db.Access.UpsertRentResidential(NewProperty("property-b", "Building B")).IsError);

        var result = db.Access.SelectRentResidentialById(property.Id);
        Assert.IsFalse(result.IsError);
        Assert.IsNotNull(result.Property);
        Assert.AreEqual(1, result.Property.Pictures.Count);
        var picture = result.Property.Pictures[0];
        Assert.AreEqual("property-picture", picture.Id);
        Assert.AreEqual("exterior.jpg", picture.ImageFilename);
        Assert.AreEqual(PropertyPictureType.Gaikan, picture.PictureType);
        Assert.AreEqual("Exterior", picture.Description);
        Assert.IsTrue(picture.IsMain);
        Assert.AreEqual(1, result.Property.Pdfs.Count);
        var pdf = result.Property.Pdfs[0];
        Assert.AreEqual("property-pdf", pdf.Id);
        Assert.AreEqual("plan.pdf", pdf.PdfFilename);
        Assert.AreEqual("plan.png", pdf.ThumbnailFilename);
        Assert.AreEqual(PropertyPdfType.Architectural, pdf.PdfType);
        Assert.AreEqual("Building plan", pdf.Description);
        Assert.IsTrue(pdf.IsMain);
        var other = db.Access.SelectRentResidentialById("property-b");
        Assert.IsFalse(other.IsError);
        Assert.IsNotNull(other.Property);
        Assert.AreEqual(0, other.Property.Pictures.Count);
        Assert.AreEqual(0, other.Property.Pdfs.Count);
    }

    [TestMethod]
    public void ListingAttachments_SaveAndReloadMetadataForCorrectOwner()
    {
        using var db = new TestDatabase();
        var property = NewProperty("property-a", "Building A");
        Assert.IsFalse(db.Access.UpsertRentResidential(property).IsError);
        var listing = NewListing(property, "listing-a", "101");
        listing.Pictures.Add(new ListingPicture("listing-picture", "kitchen.jpg", EntityStatus.New) { PictureType = ListingPictureType.Kitchen, Description = "Kitchen", IsMain = true });
        listing.Pdfs.Add(new ListingPdf("listing-pdf", "listing.pdf", "listing.png", EntityStatus.New) { PdfType = ListingPdfType.Maisoku, Description = "Listing document", IsMain = true });
        Assert.IsFalse(db.Access.UpsertRentResidentialListing(property.Id, listing).IsError);
        Assert.IsFalse(db.Access.UpsertRentResidentialListing(property.Id, NewListing(property, "listing-b", "102")).IsError);

        var result = db.Access.SelectRentResidentialListingById(property.Id, listing.Id);
        Assert.IsFalse(result.IsError);
        Assert.IsNotNull(result.Listing);
        Assert.AreEqual(1, result.Listing.Pictures.Count);
        var picture = result.Listing.Pictures[0];
        Assert.AreEqual("listing-picture", picture.Id);
        Assert.AreEqual("kitchen.jpg", picture.ImageFilename);
        Assert.AreEqual(ListingPictureType.Kitchen, picture.PictureType);
        Assert.AreEqual("Kitchen", picture.Description);
        Assert.IsTrue(picture.IsMain);
        Assert.AreEqual(1, result.Listing.Pdfs.Count);
        var pdf = result.Listing.Pdfs[0];
        Assert.AreEqual("listing-pdf", pdf.Id);
        Assert.AreEqual("listing.pdf", pdf.PdfFilename);
        Assert.AreEqual("listing.png", pdf.ThumbnailFilename);
        Assert.AreEqual(ListingPdfType.Maisoku, pdf.PdfType);
        Assert.AreEqual("Listing document", pdf.Description);
        Assert.IsTrue(pdf.IsMain);
        var other = db.Access.SelectRentResidentialListingById(property.Id, "listing-b");
        Assert.IsFalse(other.IsError);
        Assert.IsNotNull(other.Listing);
        Assert.AreEqual(0, other.Listing.Pictures.Count);
        Assert.AreEqual(0, other.Listing.Pdfs.Count);
    }

    [TestMethod]
    public void Updates_PersistChangesWithoutLosingOtherValues()
    {
        using var db = new TestDatabase();
        var property = NewProperty("property-a", "Original building");
        property.SetPropertyKind(PropertyKindType.Mansion);
        property.SetRemarks("Property remarks");
        Assert.IsFalse(db.Access.UpsertRentResidential(property).IsError);
        var listing = NewListing(property, "listing-a", "101");
        listing.SetChinryou(90000);
        listing.SetContractRemarks("Contract remarks");
        Assert.IsFalse(db.Access.UpsertRentResidentialListing(property.Id, listing).IsError);

        property.SetName("Updated building");
        Assert.IsFalse(db.Access.UpsertRentResidential(property).IsError);
        listing.SetName("102");
        listing.SetChinryou(100000);
        Assert.IsFalse(db.Access.UpsertRentResidentialListing(property.Id, listing).IsError);

        var loadedProperty = db.Access.SelectRentResidentialById(property.Id);
        Assert.IsFalse(loadedProperty.IsError);
        Assert.IsNotNull(loadedProperty.Property);
        Assert.AreEqual("Updated building", loadedProperty.Property.Name);
        Assert.AreEqual(PropertyKindType.Mansion, loadedProperty.Property.PropertyKind);
        Assert.AreEqual("Property remarks", loadedProperty.Property.Remarks);
        var loadedListing = db.Access.SelectRentResidentialListingById(property.Id, listing.Id);
        Assert.IsFalse(loadedListing.IsError);
        Assert.IsNotNull(loadedListing.Listing);
        Assert.AreEqual("102", loadedListing.Listing.Name);
        Assert.AreEqual(100000m, loadedListing.Listing.Chinryou);
        Assert.AreEqual("Contract remarks", loadedListing.Listing.ContractRemarks);
    }

    [TestMethod]
    public void Search_CombinesPropertyListingAndRentCriteria()
    {
        using var db = new TestDatabase();
        var property = NewProperty("property-a", "Sakura Building");
        property.SetPropertyKind(PropertyKindType.Mansion);
        property.HasElevator = true;
        Assert.IsFalse(db.Access.UpsertRentResidential(property).IsError);
        var matching = NewListing(property, "listing-a", "101");
        matching.SetChinryou(100000);
        matching.SetFloorPlanTypeLabel(FloorPlanType.TwoLDK);
        Assert.IsFalse(db.Access.UpsertRentResidentialListing(property.Id, matching).IsError);
        var expensive = NewListing(property, "listing-b", "102");
        expensive.SetChinryou(150000);
        expensive.SetFloorPlanTypeLabel(FloorPlanType.TwoLDK);
        Assert.IsFalse(db.Access.UpsertRentResidentialListing(property.Id, expensive).IsError);
        var other = NewProperty("property-b", "Other Building");
        other.SetPropertyKind(PropertyKindType.Mansion);
        other.HasElevator = true;
        Assert.IsFalse(db.Access.UpsertRentResidential(other).IsError);
        var otherListing = NewListing(other, "listing-c", "201");
        otherListing.SetChinryou(100000);
        otherListing.SetFloorPlanTypeLabel(FloorPlanType.TwoLDK);
        Assert.IsFalse(db.Access.UpsertRentResidentialListing(other.Id, otherListing).IsError);

        var criteria = new RentResidentialListingSearchCriteria { PropertyName = "Sakura", MinimumRent = 100000, MaximumRent = 120000, HasElevator = true };
        criteria.PropertyKinds.Add(PropertyKindType.Mansion);
        criteria.FloorPlans.Add(FloorPlanType.TwoLDK);
        var result = db.Access.SelectRentResidentialListings(criteria);
        Assert.IsFalse(result.IsError);
        Assert.AreEqual(1, result.ListingSearchResult.Count);
        Assert.AreEqual(matching.Id, result.ListingSearchResult[0].Id);
        Assert.AreEqual(property.Id, result.ListingSearchResult[0].PropertyId);
        criteria.HasParking = true;
        var noMatch = db.Access.SelectRentResidentialListings(criteria);
        Assert.IsFalse(noMatch.IsError);
        Assert.AreEqual(0, noMatch.ListingSearchResult.Count);
    }

    [TestMethod]
    public void Search_AvailableForRentFiltersListings()
    {
        using var db = new TestDatabase();
        var property = NewProperty("property-a", "Building A");
        Assert.IsFalse(db.Access.UpsertRentResidential(property).IsError);
        var available = NewListing(property, "listing-a", "101");
        available.SetIsAvailableForRentNow(true);
        available.SetIsAvailableForImmediateMoveIn(false);
        Assert.IsFalse(db.Access.UpsertRentResidentialListing(property.Id, available).IsError);
        var unavailable = NewListing(property, "listing-b", "102");
        unavailable.SetIsAvailableForRentNow(false);
        unavailable.SetIsAvailableForImmediateMoveIn(true);
        Assert.IsFalse(db.Access.UpsertRentResidentialListing(property.Id, unavailable).IsError);

        var criteria = new RentResidentialListingSearchCriteria { IsRecruiting = true };
        var filtered = db.Access.SelectRentResidentialListings(criteria);
        Assert.IsFalse(filtered.IsError);
        Assert.AreEqual(1, filtered.ListingSearchResult.Count);
        Assert.AreEqual(available.Id, filtered.ListingSearchResult[0].Id);
        criteria.IsRecruiting = false;
        var unfiltered = db.Access.SelectRentResidentialListings(criteria);
        Assert.IsFalse(unfiltered.IsError);
        Assert.AreEqual(2, unfiltered.ListingSearchResult.Count);
    }

    [TestMethod]
    public void Search_AddressAndTransportRequireSameMatchingRoute()
    {
        using var db = new TestDatabase();
        var property = NewProperty("property-a", "Building A");
        property.Address.SetPrefecture("13", "130001", "Tokyo");
        property.Address.SetCountyAndCity("", "", "Chiyoda");
        property.Train1.SetRailLine("line-a", "Blue Line");
        property.Train1.SetRailStation("line-a", "station-a", "Central");
        property.Train1.SetEkiToho(6);
        property.Train2.SetRailLine("line-b", "Red Line");
        property.Train2.SetRailStation("line-b", "station-b", "North");
        property.Train2.SetEkiToho(3);
        Assert.IsFalse(db.Access.UpsertRentResidential(property).IsError);
        Assert.IsFalse(db.Access.UpsertRentResidentialListing(property.Id, NewListing(property, "listing-a", "101")).IsError);

        var criteria = new RentResidentialListingSearchCriteria { Address = "Chiyoda", Station = "Central", Line = "Blue", MaximumStationWalkMinutes = 6 };
        var match = db.Access.SelectRentResidentialListings(criteria);
        Assert.IsFalse(match.IsError);
        Assert.AreEqual(1, match.ListingSearchResult.Count);
        Assert.AreEqual("listing-a", match.ListingSearchResult[0].Id);
        criteria.Line = "Red";
        var differentRoute = db.Access.SelectRentResidentialListings(criteria);
        Assert.IsFalse(differentRoute.IsError);
        Assert.AreEqual(0, differentRoute.ListingSearchResult.Count);
        criteria.Line = "Blue";
        criteria.MaximumStationWalkMinutes = 5;
        var tooFar = db.Access.SelectRentResidentialListings(criteria);
        Assert.IsFalse(tooFar.IsError);
        Assert.AreEqual(0, tooFar.ListingSearchResult.Count);
    }

    [TestMethod]
    public void DeleteListingAndProperty_LeavesUnrelatedRecordsIntact()
    {
        using var db = new TestDatabase();
        var first = NewProperty("property-a", "Building A");
        var second = NewProperty("property-b", "Building B");
        Assert.IsFalse(db.Access.UpsertRentResidential(first).IsError);
        Assert.IsFalse(db.Access.UpsertRentResidential(second).IsError);
        var firstListing = NewListing(first, "listing-a", "101");
        firstListing.Pictures.Add(new ListingPicture("listing-picture", "image.jpg", EntityStatus.New));
        Assert.IsFalse(db.Access.UpsertRentResidentialListing(first.Id, firstListing).IsError);
        Assert.IsFalse(db.Access.UpsertRentResidentialListing(first.Id, NewListing(first, "listing-b", "102")).IsError);
        Assert.IsFalse(db.Access.UpsertRentResidentialListing(second.Id, NewListing(second, "listing-c", "201")).IsError);

        var listingDelete = db.Access.DeleteRentResidentialListing("listing-a");
        Assert.IsFalse(listingDelete.IsError);
        Assert.AreEqual(1, listingDelete.AffectedCount);
        Assert.IsNull(db.Access.SelectRentResidentialListingById(first.Id, "listing-a").Listing);
        Assert.AreEqual(0L, db.Count("SELECT COUNT(*) FROM rent_residential_listing_pictures WHERE listing_id = @id", "listing-a"));
        Assert.IsNotNull(db.Access.SelectRentResidentialListingById(first.Id, "listing-b").Listing);
        var propertyDelete = db.Access.DeleteRentResidential(first.Id);
        Assert.IsFalse(propertyDelete.IsError);
        Assert.AreEqual(1, propertyDelete.AffectedCount);
        Assert.IsNull(db.Access.SelectRentResidentialById(first.Id).Property);
        Assert.AreEqual(0L, db.Count("SELECT COUNT(*) FROM rent_residential_listings WHERE property_id = @id", first.Id));
        Assert.IsNotNull(db.Access.SelectRentResidentialById(second.Id).Property);
        Assert.IsNotNull(db.Access.SelectRentResidentialListingById(second.Id, "listing-c").Listing);
    }

    [TestMethod]
    public void DeletePropertyAndListing_HandlesApostrophesInIds()
    {
        using var db = new TestDatabase();
        var property = NewProperty("property'a", "Building A");
        var other = NewProperty("property-b", "Building B");
        Assert.IsFalse(db.Access.UpsertRentResidential(property).IsError);
        Assert.IsFalse(db.Access.UpsertRentResidential(other).IsError);
        var listing = NewListing(property, "listing'a", "101");
        Assert.IsFalse(db.Access.UpsertRentResidentialListing(property.Id, listing).IsError);
        var otherListing = NewListing(other, "listing-b", "201");
        Assert.IsFalse(db.Access.UpsertRentResidentialListing(other.Id, otherListing).IsError);

        var listingDelete = db.Access.DeleteRentResidentialListing(listing.Id);
        Assert.IsFalse(listingDelete.IsError);
        Assert.AreEqual(1, listingDelete.AffectedCount);
        Assert.IsNull(db.Access.SelectRentResidentialListingById(property.Id, listing.Id).Listing);
        Assert.IsNotNull(db.Access.SelectRentResidentialListingById(other.Id, otherListing.Id).Listing);
        var propertyDelete = db.Access.DeleteRentResidential(property.Id);
        Assert.IsFalse(propertyDelete.IsError);
        Assert.AreEqual(1, propertyDelete.AffectedCount);
        Assert.IsNull(db.Access.SelectRentResidentialById(property.Id).Property);
        Assert.IsNotNull(db.Access.SelectRentResidentialById(other.Id).Property);
    }

    [TestMethod]
    public void PropertySave_DeletesQueuedRecordsWithApostrophesInIds()
    {
        using var db = new TestDatabase();
        var lessor1 = new PersonNatural("lessor'1", EntityStatus.New) { NameLast = "Lessor 1" };
        var lessor2 = new PersonNatural("lessor'2", EntityStatus.New) { NameLast = "Lessor 2" };
        var broker1 = new PersonNatural("broker'1", EntityStatus.New) { NameLast = "Broker 1" };
        var broker2 = new PersonNatural("broker'2", EntityStatus.New) { NameLast = "Broker 2" };
        Assert.IsFalse(db.Access.UpsertRentLessor(lessor1).IsError);
        Assert.IsFalse(db.Access.UpsertRentLessor(lessor2).IsError);
        Assert.IsFalse(db.Access.UpsertBroker(broker1).IsError);
        Assert.IsFalse(db.Access.UpsertBroker(broker2).IsError);
        var property = NewProperty("property'a", "Building A");
        var other = NewProperty("property-b", "Building B");
        var picture1 = new PropertyPicture("picture'1", "one.jpg", EntityStatus.New);
        var picture2 = new PropertyPicture("picture'2", "two.jpg", EntityStatus.New);
        var pdf1 = new PropertyPdf("pdf'1", "one.pdf", "one.png", EntityStatus.New);
        var pdf2 = new PropertyPdf("pdf'2", "two.pdf", "two.png", EntityStatus.New);
        property.Pictures.AddRange(new[] { picture1, picture2 });
        property.Pdfs.AddRange(new[] { pdf1, pdf2 });
        property.Lessors.AddRange(new[] { lessor1, lessor2 });
        property.Brokers.AddRange(new[] { broker1, broker2 });
        other.Lessors.Add(lessor1);
        other.Brokers.Add(broker1);
        Assert.IsFalse(db.Access.UpsertRentResidential(property).IsError);
        Assert.IsFalse(db.Access.UpsertRentResidential(other).IsError);
        var listing1 = NewListing(property, "listing'1", "101");
        var listing2 = NewListing(property, "listing'2", "102");
        Assert.IsFalse(db.Access.UpsertRentResidentialListing(property.Id, listing1).IsError);
        Assert.IsFalse(db.Access.UpsertRentResidentialListing(property.Id, listing2).IsError);

        property.Pictures.Clear();
        property.PicturesToBeDeleted.AddRange(new[] { picture1, picture2 });
        property.Pdfs.Clear();
        property.PdfsToBeDeleted.AddRange(new[] { pdf1, pdf2 });
        property.Lessors.Clear();
        property.LessorsToBeDeleted.AddRange(new[] { lessor1, lessor2 });
        property.Brokers.Clear();
        property.BrokersToBeDeleted.AddRange(new[] { broker1, broker2 });
        property.ListingsToBeDeleted.AddRange(new[] { listing1.Id, listing2.Id });
        var result = db.Access.UpsertRentResidential(property);
        Assert.IsFalse(result.IsError);

        var loaded = db.Access.SelectRentResidentialById(property.Id);
        Assert.IsFalse(loaded.IsError);
        Assert.IsNotNull(loaded.Property);
        Assert.AreEqual(0, loaded.Property.Pictures.Count);
        Assert.AreEqual(0, loaded.Property.Pdfs.Count);
        Assert.AreEqual(0, loaded.Property.Lessors.Count);
        Assert.AreEqual(0, loaded.Property.Brokers.Count);
        Assert.AreEqual(0, loaded.Property.ListingSummaries.Count);
        var otherLoaded = db.Access.SelectRentResidentialById(other.Id);
        Assert.IsFalse(otherLoaded.IsError);
        Assert.IsNotNull(otherLoaded.Property);
        Assert.AreEqual(1, otherLoaded.Property.Lessors.Count);
        Assert.AreEqual(lessor1.Id, otherLoaded.Property.Lessors[0].Id);
        Assert.AreEqual(1, otherLoaded.Property.Brokers.Count);
        Assert.AreEqual(broker1.Id, otherLoaded.Property.Brokers[0].Id);
    }

    [TestMethod]
    public void Duplicate_LoadsNewIdentitiesWithoutChangingOriginal()
    {
        using var db = new TestDatabase();
        var property = NewProperty("property-a", "Building A");
        property.Pictures.Add(new PropertyPicture("property-picture", "exterior.jpg", EntityStatus.New));
        Assert.IsFalse(db.Access.UpsertRentResidential(property).IsError);
        var listing = NewListing(property, "listing-a", "101");
        listing.SetChinryou(100000);
        listing.Pictures.Add(new ListingPicture("listing-picture", "kitchen.jpg", EntityStatus.New));
        Assert.IsFalse(db.Access.UpsertRentResidentialListing(property.Id, listing).IsError);

        var propertyCopy = db.Access.SelectRentResidentialById(property.Id, true);
        Assert.IsFalse(propertyCopy.IsError);
        Assert.IsNotNull(propertyCopy.Property);
        Assert.AreNotEqual(property.Id, propertyCopy.Property.Id);
        Assert.AreEqual(EntityStatus.New, propertyCopy.Property.Status);
        Assert.AreEqual("Building A", propertyCopy.Property.Name);
        Assert.AreEqual(0, propertyCopy.Property.ListingSummaries.Count);
        Assert.AreEqual(1, propertyCopy.Property.Pictures.Count);
        Assert.AreNotEqual("property-picture", propertyCopy.Property.Pictures[0].Id);
        var listingCopy = db.Access.SelectRentResidentialListingById(property.Id, listing.Id, true);
        Assert.IsFalse(listingCopy.IsError);
        Assert.IsNotNull(listingCopy.Listing);
        Assert.AreNotEqual(listing.Id, listingCopy.Listing.Id);
        Assert.AreEqual(EntityStatus.New, listingCopy.Listing.Status);
        Assert.AreEqual(property.Id, listingCopy.Listing.PropertyId);
        Assert.AreEqual(100000m, listingCopy.Listing.Chinryou);
        Assert.AreEqual(1, listingCopy.Listing.Pictures.Count);
        Assert.AreNotEqual("listing-picture", listingCopy.Listing.Pictures[0].Id);
        Assert.AreEqual("101", db.Access.SelectRentResidentialListingById(property.Id, listing.Id).Listing!.Name);
    }

    [TestMethod]
    public void SaveNewProperty_WithQueuedListing_PersistsBoth()
    {
        using var db = new TestDatabase();
        var property = NewProperty("property-a", "Building A");
        var listing = new Listing("listing-a", EntityStatus.New, property.Id, EntityStatus.New, false, property.Name, string.Empty);
        listing.SetName("101");
        listing.SetChinryou(80000);
        property.ListingsToBeSaved.Add(listing);
        Assert.IsFalse(db.Access.UpsertRentResidential(property).IsError);
        var loadedProperty = db.Access.SelectRentResidentialById(property.Id);
        Assert.IsFalse(loadedProperty.IsError);
        Assert.IsNotNull(loadedProperty.Property);
        Assert.AreEqual(1, loadedProperty.Property.ListingSummaries.Count);
        Assert.AreEqual(listing.Id, loadedProperty.Property.ListingSummaries[0].Id);
        var loadedListing = db.Access.SelectRentResidentialListingById(property.Id, listing.Id);
        Assert.IsFalse(loadedListing.IsError);
        Assert.IsNotNull(loadedListing.Listing);
        Assert.AreEqual("101", loadedListing.Listing.Name);
        Assert.AreEqual(80000m, loadedListing.Listing.Chinryou);
    }

    [TestMethod]
    public void ListingLessorAssociation_CanBeAddedAndRemoved()
    {
        using var db = new TestDatabase();
        var lessor = new PersonNatural("lessor-a", EntityStatus.New) { NameLast = "Yamada", PhoneNumber = "03-1111-2222" };
        Assert.IsFalse(db.Access.UpsertRentLessor(lessor).IsError);
        var property = NewProperty("property-a", "Building A");
        Assert.IsFalse(db.Access.UpsertRentResidential(property).IsError);
        var listing = NewListing(property, "listing-a", "101");
        listing.Lessors.Add(lessor);
        Assert.IsFalse(db.Access.UpsertRentResidentialListing(property.Id, listing).IsError);
        var loaded = db.Access.SelectRentResidentialListingById(property.Id, listing.Id);
        Assert.IsFalse(loaded.IsError);
        Assert.IsNotNull(loaded.Listing);
        Assert.AreEqual(1, loaded.Listing.Lessors.Count);
        Assert.AreEqual(lessor.Id, loaded.Listing.Lessors[0].Id);
        Assert.AreEqual("03-1111-2222", loaded.Listing.Lessors[0].PhoneNumber);
        loaded.Listing.LessorsToBeDeleted.Add(loaded.Listing.Lessors[0]);
        loaded.Listing.Lessors.Clear();
        Assert.IsFalse(db.Access.UpsertRentResidentialListing(property.Id, loaded.Listing).IsError);
        var afterRemoval = db.Access.SelectRentResidentialListingById(property.Id, listing.Id);
        Assert.IsFalse(afterRemoval.IsError);
        Assert.IsNotNull(afterRemoval.Listing);
        Assert.AreEqual(0, afterRemoval.Listing.Lessors.Count);
        Assert.IsNotNull(db.Access.SelectRentLessorById(lessor.Id).Person);
    }

    [TestMethod]
    public void PropertyFacilitiesAndManagement_RoundTrip()
    {
        using var db = new TestDatabase();
        var property = NewProperty("property-a", "Building A");
        property.SetPropertyStructure(StructureType.RC);
        property.SetFloorCountAboveGround(5);
        property.SetFloorCountBasement(1);
        property.SetTotalUnitCount(24);
        property.SetKanriShutai(Property.EnumKanriShutai.Tasya);
        property.SetKanriNameOfCompanyIfIsKanriTasya("Manager");
        property.HasGas = true;
        property.GasType = FacilityGasType.CityGas;
        property.GasDetail = "Gas details";
        property.TapWaterType = FacilityTapWaterType.Public;
        property.SewerageType = FacilitySewerageType.SepticTank;
        property.InternetFree = true;
        property.InternetDetail = "Fiber details";
        property.HasParking = true;
        property.ParkingContractType = FacilityParkingContractType.Required;
        property.ParkingAvailability = FacilityAvailabilityType.Available;
        property.ParkingFee = "15000";
        property.ParkingFeeTaxType = FacilityTaxType.Excluded;
        property.ParkingSpaces = "2";
        property.HasBicycleParking = true;
        property.BicycleParkingFee = "500";
        property.HasMotorcycleParking = true;
        property.MotorcycleParkingFee = "3000";
        Assert.IsFalse(db.Access.UpsertRentResidential(property).IsError);

        var result = db.Access.SelectRentResidentialById(property.Id);
        Assert.IsFalse(result.IsError);
        Assert.IsNotNull(result.Property);
        var loaded = result.Property;
        Assert.AreEqual(StructureType.RC, loaded.PropertyStructure);
        Assert.AreEqual(5, loaded.FloorCountAboveGround);
        Assert.AreEqual(1, loaded.FloorCountBasement);
        Assert.AreEqual(24, loaded.TotalUnitCount);
        Assert.AreEqual(Property.EnumKanriShutai.Tasya, loaded.KanriShutai);
        Assert.AreEqual("Manager", loaded.KanriNameOfCompanyIfIsKanriTasya);
        Assert.IsTrue(loaded.HasGas);
        Assert.AreEqual(FacilityGasType.CityGas, loaded.GasType);
        Assert.AreEqual("Gas details", loaded.GasDetail);
        Assert.AreEqual(FacilityTapWaterType.Public, loaded.TapWaterType);
        Assert.AreEqual(FacilitySewerageType.SepticTank, loaded.SewerageType);
        Assert.IsTrue(loaded.InternetFree);
        Assert.AreEqual("Fiber details", loaded.InternetDetail);
        Assert.IsTrue(loaded.HasParking);
        Assert.AreEqual(FacilityParkingContractType.Required, loaded.ParkingContractType);
        Assert.AreEqual(FacilityAvailabilityType.Available, loaded.ParkingAvailability);
        Assert.AreEqual("15000", loaded.ParkingFee);
        Assert.AreEqual(FacilityTaxType.Excluded, loaded.ParkingFeeTaxType);
        Assert.AreEqual("2", loaded.ParkingSpaces);
        Assert.IsTrue(loaded.HasBicycleParking);
        Assert.AreEqual("500", loaded.BicycleParkingFee);
        Assert.IsTrue(loaded.HasMotorcycleParking);
        Assert.AreEqual("3000", loaded.MotorcycleParkingFee);
    }

    [TestMethod]
    public void ListingContractAndApplianceFields_RoundTrip()
    {
        using var db = new TestDatabase();
        var property = NewProperty("property-a", "Building A");
        Assert.IsFalse(db.Access.UpsertRentResidential(property).IsError);
        var listing = NewListing(property, "listing-a", "101");
        listing.SetFloorGroundTypeLabel(FloorGroundType.BelowGround);
        listing.SetFloorNumber("2");
        listing.SetExposureDirectionTypeLabel(ExposureDirectionType.East);
        listing.SetAvailabilityMonthTypeLabel(AvailabilityMonthType.March);
        listing.SetAvailabilityPeriodTypeLabel(AvailabilityPeriodType.Late);
        listing.SetKeyMoney("3");
        listing.SetKeyMoneyUnitTypeLabel(FeeUnitType.Yen);
        listing.SetRenewalFee("4");
        listing.SetRenewalFeeUnitTypeLabel(FeeUnitType.Months);
        listing.SetRecontractFee("5000");
        listing.SetRecontractFeeUnitTypeLabel(FeeUnitType.Yen);
        listing.SetGuarantorCompany("Guarantor");
        listing.SetHousingInsurance("Insurance");
        listing.SetOtherFees("Other fees");
        listing.SetTwoPersonOccupancy(true);
        listing.SetNoGuarantorNeeded(true);
        listing.SetOfficeUseAllowed(true);
        listing.SetOtherConditions("Other conditions");
        listing.SetLesseeCommission("25");
        listing.SetLesseeCommissionUnitTypeLabel(ListingCommissionUnitType.Yen);
        listing.SetListingBrokerCommission("30");
        listing.SetTenantBrokerCommission("45");
        listing.SetTransactionRemarks("Transaction remarks");
        listing.SetHasWoodFlooring(true);
        listing.SetHasVideoIntercom(true);
        listing.SetHasBathReheating(true);
        listing.SetHasGasStove(true);
        listing.SetHasInductionCooktop(true);
        listing.SetHasMultipleBurners(true);
        listing.SetHasSystemKitchen(true);
        listing.SetHasLoft(true);
        listing.SetHasIndoorWashingMachineSpace(true);
        listing.SetHasBathroomDryer(true);
        listing.SetHasSeparateWashbasin(true);
        listing.SetHasWarmWaterToiletSeat(true);
        Assert.IsFalse(db.Access.UpsertRentResidentialListing(property.Id, listing).IsError);
        var result = db.Access.SelectRentResidentialListingById(property.Id, listing.Id);
        Assert.IsFalse(result.IsError);
        Assert.IsNotNull(result.Listing);
        var loaded = result.Listing;
        Assert.AreEqual(FloorGroundType.BelowGround, loaded.FloorGround);
        Assert.AreEqual("2", loaded.FloorNumber);
        Assert.AreEqual(ExposureDirectionType.East, loaded.SelectedExposureDirectionType);
        Assert.AreEqual(AvailabilityMonthType.March, loaded.SelectedAvailabilityMonthType);
        Assert.AreEqual(AvailabilityPeriodType.Late, loaded.SelectedAvailabilityPeriodType);
        Assert.AreEqual("3", loaded.KeyMoney);
        Assert.AreEqual(FeeUnitType.Yen, loaded.KeyMoneyUnit);
        Assert.AreEqual("4", loaded.RenewalFee);
        Assert.AreEqual(FeeUnitType.Months, loaded.RenewalFeeUnit);
        Assert.AreEqual("5000", loaded.RecontractFee);
        Assert.AreEqual(FeeUnitType.Yen, loaded.RecontractFeeUnit);
        Assert.AreEqual("Guarantor", loaded.GuarantorCompany);
        Assert.AreEqual("Insurance", loaded.HousingInsurance);
        Assert.AreEqual("Other fees", loaded.OtherFees);
        Assert.IsTrue(loaded.TwoPersonOccupancy);
        Assert.IsTrue(loaded.NoGuarantorNeeded);
        Assert.IsTrue(loaded.OfficeUseAllowed);
        Assert.AreEqual("Other conditions", loaded.OtherConditions);
        Assert.AreEqual("25", loaded.LesseeCommission);
        Assert.AreEqual(ListingCommissionUnitType.Yen, loaded.LesseeCommissionUnit);
        Assert.AreEqual("30", loaded.ListingBrokerCommission);
        Assert.AreEqual("45", loaded.TenantBrokerCommission);
        Assert.AreEqual("Transaction remarks", loaded.TransactionRemarks);
        Assert.IsTrue(loaded.HasWoodFlooring);
        Assert.IsTrue(loaded.HasVideoIntercom);
        Assert.IsTrue(loaded.HasBathReheating);
        Assert.IsTrue(loaded.HasGasStove);
        Assert.IsTrue(loaded.HasInductionCooktop);
        Assert.IsTrue(loaded.HasMultipleBurners);
        Assert.IsTrue(loaded.HasSystemKitchen);
        Assert.IsTrue(loaded.HasLoft);
        Assert.IsTrue(loaded.HasIndoorWashingMachineSpace);
        Assert.IsTrue(loaded.HasBathroomDryer);
        Assert.IsTrue(loaded.HasSeparateWashbasin);
        Assert.IsTrue(loaded.HasWarmWaterToiletSeat);
    }

    [TestMethod]
    public void InvalidListingOwner_IsRejectedWithoutWriting()
    {
        using var db = new TestDatabase();
        var property = NewProperty("property-a", "Building A");
        Assert.IsFalse(db.Access.UpsertRentResidential(property).IsError);
        var listing = NewListing(property, "listing-a", "101");
        Assert.IsTrue(db.Access.UpsertRentResidentialListing("wrong-property", listing).IsError);
        Assert.IsNull(db.Access.SelectRentResidentialListingById(property.Id, listing.Id).Listing);
        Assert.AreEqual(EntityStatus.New, listing.Status);
    }

    [TestMethod]
    public void ConflictingAttachmentId_RollsBackListingChanges()
    {
        using var db = new TestDatabase();
        var property = NewProperty("property-a", "Building A");
        Assert.IsFalse(db.Access.UpsertRentResidential(property).IsError);
        var first = NewListing(property, "listing-a", "101");
        first.Pictures.Add(new ListingPicture("shared-picture", "original.jpg", EntityStatus.New));
        Assert.IsFalse(db.Access.UpsertRentResidentialListing(property.Id, first).IsError);
        var second = NewListing(property, "listing-b", "102");
        second.Pictures.Add(new ListingPicture("shared-picture", "conflict.jpg", EntityStatus.New));

        var result = db.Access.UpsertRentResidentialListing(property.Id, second);
        Assert.IsTrue(result.IsError);
        Assert.AreEqual(EntityStatus.New, second.Status);
        var missing = db.Access.SelectRentResidentialListingById(property.Id, second.Id);
        Assert.IsFalse(missing.IsError);
        Assert.IsNull(missing.Listing);
        var unchanged = db.Access.SelectRentResidentialListingById(property.Id, first.Id);
        Assert.IsFalse(unchanged.IsError);
        Assert.IsNotNull(unchanged.Listing);
        Assert.AreEqual("original.jpg", unchanged.Listing.Pictures[0].ImageFilename);
    }

    private static Property NewProperty(string id, string name)
    {
        var property = new Property(id, EntityStatus.New);
        property.SetName(name);
        return property;
    }

    private static Listing NewListing(Property property, string id, string name)
    {
        var listing = new Listing(id, EntityStatus.New, property.Id, EntityStatus.Saved, false, property.Name, property.FudousanId);
        listing.SetName(name);
        return listing;
    }

    private sealed class TestDatabase : IDisposable
    {
        private readonly string _path = Path.Combine(Path.GetTempPath(), $"ZumenSearch.Tests-{Guid.NewGuid():N}.db");
        public DataAccessService Access { get; } = new();

        public TestDatabase()
        {
            Assert.IsFalse(Access.InitializeDatabase(_path).IsError, "Database initialization should succeed.");
        }

        public long Count(string sql, string id)
        {
            using var connection = new SqliteConnection($"Data Source={_path}");
            connection.Open();
            using var command = connection.CreateCommand();
            command.CommandText = sql;
            command.Parameters.AddWithValue("@id", id);
            return (long)command.ExecuteScalar()!;
        }

        public void Dispose()
        {
            Access.Dispose();
            SqliteConnection.ClearAllPools();
            File.Delete(_path);
        }
    }
}
