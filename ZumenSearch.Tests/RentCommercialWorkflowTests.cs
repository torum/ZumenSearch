using System.Globalization;
using CommunityToolkit.Mvvm.Messaging;
using Microsoft.Data.Sqlite;
using Microsoft.UI.Xaml.Controls;
using ZumenSearch.Application.Contracts;
using ZumenSearch.Application.SearchResults;
using ZumenSearch.Application.Services.Rent.Commercials;
using ZumenSearch.Domain.Entities.Person;
using ZumenSearch.Domain.Entities.Rent.Commercials;
using ZumenSearch.Domain.Entities.Rent.Commercials.Listing;
using ZumenSearch.Domain.Enums;
using ZumenSearch.Domain.Enums.Rent.Commercials;
using ZumenSearch.Infrastructure.DataAccess;
using ZumenSearch.Presentation.Features.Rent.Commercials.Models;
using ZumenSearch.Presentation.Features.Rent.Commercials.ViewModels;
using ZumenSearch.Presentation.Messaging;
using ZumenSearch.Presentation.Services.Contracts;

namespace ZumenSearch.Tests;

[TestClass]
public sealed class RentCommercialWorkflowTests
{
    [TestMethod]
    public void PropertyAndListing_RoundTripCommercialFieldsAndLinks()
    {
        using var db = new TestDatabase();
        var lessor = new PersonNatural("lessor", EntityStatus.New) { NameLast = "Lessor" };
        var broker = new PersonNatural("broker", EntityStatus.New) { NameLast = "Broker" };
        Assert.IsFalse(db.Access.UpsertRentLessor(lessor).IsError);
        Assert.IsFalse(db.Access.UpsertBroker(broker).IsError);
        var property = NewProperty("property", "Commercial building");
        property.SetTotalFloorArea(420.5m);
        property.SetFudousanId("1234567890123");
        property.SetFudousanIdAdditionalCode("0000");
        property.Lessors.Add(lessor);
        property.Brokers.Add(broker);
        property.Pictures.Add(new PropertyPicture("property-picture", "property.jpg", EntityStatus.New) { PictureType = PropertyPictureType.Exterior, IsMain = true });
        property.Pdfs.Add(new PropertyPdf("property-pdf", "property.pdf", "property.png", EntityStatus.New) { PdfType = PropertyPdfType.Architectural, IsMain = true });
        Assert.IsFalse(db.Properties.Upsert(property).IsError);
        var listing = NewListing(property, "listing", "Unit A");
        listing.SetFloorArea(52.5m);
        listing.SetUseDetails("Retail space");
        listing.SetFudousanIdAdditionalCode("A123");
        listing.SetChinryou(120000m);
        listing.SetCurrentStatusTypeLabel(ListingAvailabilityStatusType.Vacant);
        listing.Lessors.Add(lessor);
        listing.Brokers.Add(broker);
        var listingSave = db.Listings.Upsert(property.Id, listing);
        Assert.IsFalse(listingSave.IsError, listingSave.Error.FullDump);

        var propertyResult = db.Properties.SelectById(property.Id);
        Assert.IsFalse(propertyResult.IsError);
        Assert.IsNotNull(propertyResult.Property);
        Assert.AreEqual("Commercial building", propertyResult.Property.Name);
        Assert.AreEqual(420.5m, propertyResult.Property.TotalFloorArea);
        Assert.AreEqual("1234567890123", propertyResult.Property.FudousanId);
        Assert.AreEqual("0000", propertyResult.Property.FudousanIdAdditionalCode);
        Assert.AreEqual(lessor.Id, propertyResult.Property.Lessors[0].Id);
        Assert.AreEqual(broker.Id, propertyResult.Property.Brokers[0].Id);
        Assert.AreEqual(PropertyPictureType.Exterior, propertyResult.Property.Pictures.Single().PictureType);
        Assert.AreEqual(PropertyPdfType.Architectural, propertyResult.Property.Pdfs.Single().PdfType);
        var listingResult = db.Listings.SelectById(property.Id, listing.Id);
        Assert.IsFalse(listingResult.IsError);
        Assert.IsNotNull(listingResult.Listing);
        Assert.AreEqual("Unit A", listingResult.Listing.Name);
        Assert.AreEqual(52.5m, listingResult.Listing.FloorArea);
        Assert.AreEqual("Retail space", listingResult.Listing.UseDetails);
        Assert.AreEqual("A123", listingResult.Listing.FudousanIdAdditionalCode);
        Assert.AreEqual("1234567890123", listingResult.Listing.FudousanId);
        Assert.AreEqual(120000m, listingResult.Listing.Chinryou);
        Assert.AreEqual(ListingAvailabilityStatusType.Vacant, listingResult.Listing.CurrentStatus);
        Assert.AreEqual(lessor.Id, listingResult.Listing.Lessors[0].Id);
        Assert.AreEqual(broker.Id, listingResult.Listing.Brokers[0].Id);
    }

    [TestMethod]
    public void PropertySaveFailure_RollsBackMediaAndPreservesDirtyStateForRetry()
    {
        using var db = new TestDatabase();
        var property = NewProperty("commercial-media-rollback", "Commercial building");
        var picture = new PropertyPicture("commercial-picture-rollback", "property.jpg", EntityStatus.New) { Description = "Exterior" };
        var pdf = new PropertyPdf("commercial-pdf-rollback", "property.pdf", "property.png", EntityStatus.New) { Description = "Plan" };
        var missingLessor = new PersonNatural("missing-commercial-lessor", EntityStatus.New);
        property.Pictures.Add(picture);
        property.Pdfs.Add(pdf);
        property.Lessors.Add(missingLessor);

        var failedSave = db.Properties.Upsert(property);

        Assert.IsTrue(failedSave.IsError, "The missing lessor should cause a foreign-key failure after media writes.");
        Assert.AreEqual(EntityStatus.New, picture.Status);
        Assert.IsTrue(picture.IsModified);
        Assert.AreEqual(EntityStatus.New, pdf.Status);
        Assert.IsTrue(pdf.IsModified);
        Assert.AreEqual(0L, db.Count("SELECT COUNT(*) FROM rent_commercial_pictures WHERE picture_id = @id", picture.Id));
        Assert.AreEqual(0L, db.Count("SELECT COUNT(*) FROM rent_commercial_pdfs WHERE pdf_id = @id", pdf.Id));

        property.Lessors.Remove(missingLessor);
        var retrySave = db.Properties.Upsert(property);

        Assert.IsFalse(retrySave.IsError, retrySave.Error.FullDump);
        Assert.AreEqual(EntityStatus.Saved, picture.Status);
        Assert.IsFalse(picture.IsModified);
        Assert.AreEqual(EntityStatus.Saved, pdf.Status);
        Assert.IsFalse(pdf.IsModified);
        Assert.AreEqual(1L, db.Count("SELECT COUNT(*) FROM rent_commercial_pictures WHERE picture_id = @id", picture.Id));
        Assert.AreEqual(1L, db.Count("SELECT COUNT(*) FROM rent_commercial_pdfs WHERE pdf_id = @id", pdf.Id));
    }

    [TestMethod]
    public void ListingUpdate_RoundTripsFeeAndCommissionUnits()
    {
        using var db = new TestDatabase();
        var property = NewProperty("property", "Commercial building");
        Assert.IsFalse(db.Properties.Upsert(property).IsError);
        var listing = NewListing(property, "listing", "Unit A");
        Assert.IsFalse(db.Listings.Upsert(property.Id, listing).IsError);

        listing.SetFudousanIdAdditionalCode("B456");
        listing.SetSecurityDepositUnitTypeLabel(ListingFeeUnitType.Yen);
        listing.SetGuaranteeDepositUnitTypeLabel(ListingFeeUnitType.Yen);
        listing.SetKeyMoneyUnitTypeLabel(ListingFeeUnitType.Yen);
        listing.SetRightsMoneyUnitTypeLabel(ListingFeeUnitType.Yen);
        listing.SetDepositDeductionUnitTypeLabel(ListingFeeUnitType.Yen);
        listing.SetRenewalFeeUnitTypeLabel(ListingFeeUnitType.Yen);
        listing.SetRecontractFeeUnitTypeLabel(ListingFeeUnitType.Months);
        listing.SetLessorCommissionUnitTypeLabel(ListingCommissionUnitType.Yen);
        listing.SetLesseeCommissionUnitTypeLabel(ListingCommissionUnitType.Yen);
        listing.SetListingBrokerCommissionUnitTypeLabel(ListingCommissionUnitType.Yen);
        listing.SetTenantBrokerCommissionUnitTypeLabel(ListingCommissionUnitType.Yen);
        var update = db.Listings.Upsert(property.Id, listing);
        Assert.IsFalse(update.IsError, update.Error.FullDump);

        var result = db.Listings.SelectById(property.Id, listing.Id);
        Assert.IsFalse(result.IsError);
        Assert.IsNotNull(result.Listing);
        Assert.AreEqual("B456", result.Listing.FudousanIdAdditionalCode);
        Assert.AreEqual(ListingFeeUnitType.Yen, result.Listing.SecurityDepositUnit);
        Assert.AreEqual(ListingFeeUnitType.Yen, result.Listing.GuaranteeDepositUnit);
        Assert.AreEqual(ListingFeeUnitType.Yen, result.Listing.KeyMoneyUnit);
        Assert.AreEqual(ListingFeeUnitType.Yen, result.Listing.RightsMoneyUnit);
        Assert.AreEqual(ListingFeeUnitType.Yen, result.Listing.DepositDeductionUnit);
        Assert.AreEqual(ListingFeeUnitType.Yen, result.Listing.RenewalFeeUnit);
        Assert.AreEqual(ListingFeeUnitType.Months, result.Listing.RecontractFeeUnit);
        Assert.AreEqual(ListingCommissionUnitType.Yen, result.Listing.LessorCommissionUnit);
        Assert.AreEqual(ListingCommissionUnitType.Yen, result.Listing.LesseeCommissionUnit);
        Assert.AreEqual(ListingCommissionUnitType.Yen, result.Listing.ListingBrokerCommissionUnit);
        Assert.AreEqual(ListingCommissionUnitType.Yen, result.Listing.TenantBrokerCommissionUnit);
    }

    [TestMethod]
    public void PropertyMediaClones_PreserveValuesWithoutSharingMutableMetadata()
    {
        var picture = new PropertyPicture("picture", "picture.jpg", EntityStatus.New) { Description = "Original photo", IsMain = true };
        picture.PictureType = PropertyPictureType.Exterior;
        var pdf = new PropertyPdf("pdf", "document.pdf", "thumbnail.jpg", EntityStatus.New) { Description = "Original PDF", IsMain = true };
        pdf.PdfType = PropertyPdfType.Architectural;

        var pictureCopy = picture.Clone();
        var pdfCopy = pdf.Clone();
        Assert.AreEqual(picture.Id, pictureCopy.Id);
        Assert.AreEqual(picture.ImageFilename, pictureCopy.ImageFilename);
        Assert.AreEqual(PropertyPictureType.Exterior, pictureCopy.PictureType);
        Assert.AreEqual(pdf.Id, pdfCopy.Id);
        Assert.AreEqual(pdf.ThumbnailFilename, pdfCopy.ThumbnailFilename);
        Assert.AreEqual(PropertyPdfType.Architectural, pdfCopy.PdfType);
        pictureCopy.Description = "Edited photo";
        pdfCopy.Description = "Edited PDF";
        Assert.AreEqual("Original photo", picture.Description);
        Assert.AreEqual("Original PDF", pdf.Description);
    }

    [TestMethod]
    public void Duplicate_GeneratesNewIdentitiesWithoutChangingOriginal()
    {
        using var db = new TestDatabase();
        var property = NewProperty("property", "Commercial building");
        Assert.IsFalse(db.Properties.Upsert(property).IsError);
        var listing = NewListing(property, "listing", "Unit A");
        listing.SetChinryou(120000m);
        listing.SetFudousanIdAdditionalCode("C789");
        Assert.IsFalse(db.Listings.Upsert(property.Id, listing).IsError);

        var propertyCopy = db.Properties.SelectById(property.Id, true);
        Assert.IsFalse(propertyCopy.IsError);
        Assert.IsNotNull(propertyCopy.Property);
        Assert.AreNotEqual(property.Id, propertyCopy.Property.Id);
        Assert.AreEqual(EntityStatus.New, propertyCopy.Property.Status);
        Assert.AreEqual(property.Name, propertyCopy.Property.Name);
        var listingCopy = db.Listings.SelectById(property.Id, listing.Id, true);
        Assert.IsFalse(listingCopy.IsError);
        Assert.IsNotNull(listingCopy.Listing);
        Assert.AreNotEqual(listing.Id, listingCopy.Listing.Id);
        Assert.AreEqual(EntityStatus.New, listingCopy.Listing.Status);
        Assert.AreEqual(property.Id, listingCopy.Listing.PropertyId);
        Assert.AreEqual(120000m, listingCopy.Listing.Chinryou);
        Assert.AreEqual("C789", listingCopy.Listing.FudousanIdAdditionalCode);
        Assert.AreEqual("Unit A", db.Listings.SelectById(property.Id, listing.Id).Listing!.Name);
    }

    [TestMethod]
    public void InvalidListingOwner_IsRejectedWithoutWriting()
    {
        using var db = new TestDatabase();
        var property = NewProperty("property", "Commercial building");
        Assert.IsFalse(db.Properties.Upsert(property).IsError);
        var listing = NewListing(property, "listing", "Unit A");

        Assert.IsTrue(db.Listings.Upsert("wrong-property", listing).IsError);
        Assert.IsNull(db.Listings.SelectById(property.Id, listing.Id).Listing);
        Assert.AreEqual(EntityStatus.New, listing.Status);
    }

    [TestMethod]
    public void Search_FiltersByPropertyRentAndCurrentStatus()
    {
        using var db = new TestDatabase();
        var property = NewProperty("property", "Sakura commercial building");
        Assert.IsFalse(db.Properties.Upsert(property).IsError);
        var matching = NewListing(property, "listing-a", "Unit A");
        matching.SetChinryou(120000m);
        matching.SetCurrentStatusTypeLabel(ListingAvailabilityStatusType.Vacant);
        Assert.IsFalse(db.Listings.Upsert(property.Id, matching).IsError);
        var other = NewListing(property, "listing-b", "Unit B");
        other.SetChinryou(160000m);
        Assert.IsFalse(db.Listings.Upsert(property.Id, other).IsError);

        var criteria = new RentCommercialListingSearchCriteria { PropertyName = "Sakura", MinimumRent = 100000, MaximumRent = 130000, CurrentStatus = ListingAvailabilityStatusType.Vacant };
        var result = db.Listings.Search(criteria);
        Assert.IsFalse(result.IsError);
        Assert.AreEqual(1, result.ListingSearchResult.Count);
        Assert.AreEqual(matching.Id, result.ListingSearchResult[0].Id);
        criteria.CurrentStatus = ListingAvailabilityStatusType.ContractedAndOccupied;
        var noMatch = db.Listings.Search(criteria);
        Assert.IsFalse(noMatch.IsError);
        Assert.AreEqual(0, noMatch.ListingSearchResult.Count);
    }

    [TestMethod]
    public void Search_KeywordMatchesSixFieldsAndOwnership()
    {
        using var db = new TestDatabase();
        var buildingLessor = new PersonNatural("building-lessor", EntityStatus.New) { NameLast = "Building%_Owner" };
        var unitLessor = new PersonNatural("unit-lessor", EntityStatus.New) { NameLast = "UnitOwner" };
        foreach (var lessor in new[] { buildingLessor, unitLessor }) Assert.IsFalse(db.Access.UpsertRentLessor(lessor).IsError);

        var building = NewProperty("building", "SakuraBuilding");
        building.SetFudousanId("1234567890123");
        building.Address.SetPrefecture("13", "130001", "TokyoAddress");
        building.Train1.SetRailStation("line1", "station1", "FirstStation");
        building.Train2.SetRailStation("line2", "station2", "SecondStation");
        building.Train3.SetRailStation("line3", "station3", "ThirdStation");
        building.Train4.SetRailStation("line4", "station4", "Fourth%_Station");
        building.Lessors.Add(buildingLessor);
        var units = NewProperty("units", "OtherBuilding");
        units.SetIsUnitOwnership(true);
        units.Lessors.Add(buildingLessor);
        Assert.IsFalse(db.Properties.Upsert(building).IsError);
        Assert.IsFalse(db.Properties.Upsert(units).IsError);
        var buildingA = NewListing(building, "building-a", "UniqueRoom");
        var buildingB = NewListing(building, "building-b", "OtherRoom");
        var unitA = NewListing(units, "unit-a", "UnitA");
        var unitB = NewListing(units, "unit-b", "UnitB");
        buildingA.Lessors.Add(unitLessor);
        unitA.Lessors.Add(unitLessor);
        foreach (var listing in new[] { buildingA, buildingB, unitA, unitB }) Assert.IsFalse(db.Listings.Upsert(listing.PropertyId, listing).IsError);

        (string Keyword, string[] Expected)[] cases =
        [
            ("Sakura", ["building-a", "building-b"]),
            ("UniqueRoom", ["building-a"]),
            ("TokyoAddress", ["building-a", "building-b"]),
            ("456789", ["building-a", "building-b"]),
            ("FirstStation", ["building-a", "building-b"]),
            ("SecondStation", ["building-a", "building-b"]),
            ("ThirdStation", ["building-a", "building-b"]),
            ("Fourth%_", ["building-a", "building-b"]),
            ("Building%_Owner", ["building-a", "building-b"]),
            ("%_", ["building-a", "building-b"]),
            ("UnitOwner", ["unit-a"]),
            ("BuildingXXOwner", []),
            ("FourthXXStation", []),
            ("not found", []),
            ("*", ["building-a", "building-b", "unit-a", "unit-b"]),
            (" ", ["building-a", "building-b", "unit-a", "unit-b"])
        ];
        foreach (var (keyword, expected) in cases)
        {
            var result = db.Listings.Search(new RentCommercialListingSearchCriteria { Keyword = keyword });
            Assert.IsFalse(result.IsError, result.Error.FullDump);
            CollectionAssert.AreEquivalent(expected, result.ListingSearchResult.Select(item => item.Id).ToArray(), keyword);
        }
        var filtered = db.Listings.Search(new RentCommercialListingSearchCriteria { Keyword = "  FirstStation  ", PropertyName = "OtherBuilding" });
        Assert.IsFalse(filtered.IsError, filtered.Error.FullDump);
        Assert.AreEqual(0, filtered.ListingSearchResult.Count);
    }

    [TestMethod]
    public void Search_AmountFiltersExcludeUnspecifiedValues()
    {
        using var db = new TestDatabase();
        var property = NewProperty("property", "Commercial building");
        Assert.IsFalse(db.Properties.Upsert(property).IsError);
        var unspecified = NewListing(property, "unspecified", "Unspecified");
        var listings = new[]
        {
            NewListing(property, "rent-zero", "Rent zero"),
            NewListing(property, "management-zero", "Management zero"),
            NewListing(property, "security-zero", "Security zero"),
            NewListing(property, "guarantee-zero", "Guarantee zero"),
            NewListing(property, "key-zero", "Key zero")
        };
        foreach (var listing in listings)
        {
            listing.SetChinryou(10);
            listing.SetManagementFee(10);
            listing.SetSecurityDeposit(10);
            listing.SetGuaranteeDeposit(10);
            listing.SetKeyMoney(10);
        }
        listings[0].SetChinryou(0);
        listings[1].SetManagementFee(0);
        listings[2].SetSecurityDeposit(0);
        listings[3].SetGuaranteeDeposit(0);
        listings[4].SetKeyMoney(0);
        foreach (var listing in listings.Append(unspecified))
        {
            var save = db.Listings.Upsert(property.Id, listing);
            Assert.IsFalse(save.IsError, save.Error.FullDump);
        }

        (RentCommercialListingSearchCriteria Criteria, string ExpectedId)[] zeroOnly =
        [
            (new() { MaximumRent = 0 }, "rent-zero"),
            (new() { MaximumManagementFee = 0 }, "management-zero"),
            (new() { NoSecurityDeposit = true }, "security-zero"),
            (new() { NoGuaranteeDeposit = true }, "guarantee-zero"),
            (new() { NoKeyMoney = true }, "key-zero")
        ];
        foreach (var (criteria, expectedId) in zeroOnly)
        {
            var result = db.Listings.Search(criteria);
            Assert.IsFalse(result.IsError, result.Error.FullDump);
            Assert.AreEqual(1, result.ListingSearchResult.Count);
            Assert.AreEqual(expectedId, result.ListingSearchResult.Single().Id);
        }

        var minimum = db.Listings.Search(new RentCommercialListingSearchCriteria { MinimumRent = 0 });
        Assert.IsFalse(minimum.IsError, minimum.Error.FullDump);
        CollectionAssert.AreEquivalent(listings.Select(item => item.Id).ToArray(), minimum.ListingSearchResult.Select(item => item.Id).ToArray());
        var all = db.Listings.Search(new RentCommercialListingSearchCriteria());
        Assert.IsFalse(all.IsError, all.Error.FullDump);
        CollectionAssert.AreEquivalent(listings.Select(item => item.Id).Append(unspecified.Id).ToArray(), all.ListingSearchResult.Select(item => item.Id).ToArray());
    }

    [TestMethod]
    public void Search_TextFiltersTreatWildcardCharactersLiterally()
    {
        using var db = new TestDatabase();
        var literal = NewProperty("literal", "Building%_\\Marker");
        var lookalike = NewProperty("lookalike", "BuildingXXMarker");
        foreach (var property in new[] { literal, lookalike })
        {
            var save = db.Properties.Upsert(property);
            Assert.IsFalse(save.IsError, save.Error.FullDump);
            var listing = NewListing(property, property.Id + "-listing", property == literal ? "Unit%_\\Marker" : "UnitXXMarker");
            var listingSave = db.Listings.Upsert(property.Id, listing);
            Assert.IsFalse(listingSave.IsError, listingSave.Error.FullDump);
        }

        RentCommercialListingSearchCriteria[] criteria =
        [
            new() { PropertyName = "%" },
            new() { PropertyName = "_" },
            new() { PropertyName = "\\" },
            new() { PropertyName = "%_\\" },
            new() { Keyword = "Unit%_\\" }
        ];
        foreach (var query in criteria)
        {
            var result = db.Listings.Search(query);
            Assert.IsFalse(result.IsError, result.Error.FullDump);
            Assert.AreEqual(1, result.ListingSearchResult.Count);
            Assert.AreEqual("literal-listing", result.ListingSearchResult.Single().Id);
        }
    }

    [TestMethod]
    public void Search_StationLineAndWalkMustMatchOneRoute()
    {
        using var db = new TestDatabase();
        var matching = NewProperty("matching", "Matching");
        matching.Train3.SetRailLine("line-a", "Alpha Line");
        matching.Train3.SetRailStation("line-a", "station-a", "Central Station");
        matching.Train3.SetEkiToho(8);
        var split = NewProperty("split", "Split");
        split.Train1.SetRailLine("line-a", "Alpha Line");
        split.Train1.SetRailStation("line-a", "station-b", "Other Station");
        split.Train1.SetEkiToho(8);
        split.Train2.SetRailLine("line-b", "Beta Line");
        split.Train2.SetRailStation("line-b", "station-a", "Central Station");
        split.Train2.SetEkiToho(8);
        var tooFar = NewProperty("far", "Too far");
        tooFar.Train4.SetRailLine("line-a", "Alpha Line");
        tooFar.Train4.SetRailStation("line-a", "station-a", "Central Station");
        tooFar.Train4.SetEkiToho(9);
        foreach (var property in new[] { matching, split, tooFar })
        {
            var save = db.Properties.Upsert(property);
            Assert.IsFalse(save.IsError, save.Error.FullDump);
            var listingSave = db.Listings.Upsert(property.Id, NewListing(property, property.Id + "-listing", "Unit"));
            Assert.IsFalse(listingSave.IsError, listingSave.Error.FullDump);
        }

        var result = db.Listings.Search(new RentCommercialListingSearchCriteria { Station = "Central", Line = "Alpha", MaximumStationWalkMinutes = 8 });
        Assert.IsFalse(result.IsError, result.Error.FullDump);
        Assert.AreEqual(1, result.ListingSearchResult.Count);
        Assert.AreEqual("matching-listing", result.ListingSearchResult.Single().Id);
        var withoutWalk = db.Listings.Search(new RentCommercialListingSearchCriteria { Station = "Central", Line = "Alpha" });
        Assert.IsFalse(withoutWalk.IsError, withoutWalk.Error.FullDump);
        CollectionAssert.AreEquivalent(new[] { "matching-listing", "far-listing" }, withoutWalk.ListingSearchResult.Select(item => item.Id).ToArray());
    }

    [TestMethod]
    public void Search_CommercialPropertyAndListingFiltersSelectMatchingRecords()
    {
        using var db = new TestDatabase();
        var matching = NewProperty("matching", "Matching");
        matching.SetCommercialKindTypeLabel(PropertyKindType.Retail);
        matching.SetPropertyStructureTypeLabel(StructureType.RC);
        matching.HasParking = true;
        var other = NewProperty("other", "Other");
        other.SetCommercialKindTypeLabel(PropertyKindType.Office);
        other.SetPropertyStructureTypeLabel(StructureType.Wood);
        foreach (var property in new[] { matching, other })
        {
            var save = db.Properties.Upsert(property);
            Assert.IsFalse(save.IsError, save.Error.FullDump);
            var listing = NewListing(property, property.Id + "-listing", "Unit");
            listing.SetUseTypeLabel(property == matching ? ListingUseType.Retail : ListingUseType.Office);
            listing.SetCurrentStatusTypeLabel(property == matching ? ListingAvailabilityStatusType.Vacant : ListingAvailabilityStatusType.ScheduledVacating);
            listing.SetPermitsHeavyFoodService(property == matching);
            listing.SetFloorArea(property == matching ? 50m : 100m);
            var listingSave = db.Listings.Upsert(property.Id, listing);
            Assert.IsFalse(listingSave.IsError, listingSave.Error.FullDump);
        }

        RentCommercialListingSearchCriteria[] filters =
        [
            new() { PropertyKinds = [PropertyKindType.Retail] },
            new() { PropertyStructures = [StructureType.RC] },
            new() { Use = ListingUseType.Retail },
            new() { CurrentStatus = ListingAvailabilityStatusType.Vacant },
            new() { HasParking = true },
            new() { PermitsHeavyFoodService = true },
            new() { MinimumFloorArea = 50, MaximumFloorArea = 50 }
        ];
        foreach (var criteria in filters)
        {
            var result = db.Listings.Search(criteria);
            Assert.IsFalse(result.IsError, result.Error.FullDump);
            Assert.AreEqual(1, result.ListingSearchResult.Count);
            Assert.AreEqual("matching-listing", result.ListingSearchResult.Single().Id);
        }
    }

    [TestMethod]
    public void Search_LessorMatchesOwnershipLevelAndListing()
    {
        using var db = new TestDatabase();
        var buildingLessor = new PersonNatural("building-lessor", EntityStatus.New) { NameLast = "BuildingOwner" };
        var unitLessor = new PersonNatural("unit-lessor", EntityStatus.New) { NameLast = "UnitOwner" };
        foreach (var lessor in new[] { buildingLessor, unitLessor })
        {
            var save = db.Access.UpsertRentLessor(lessor);
            Assert.IsFalse(save.IsError, save.Error.FullDump);
        }

        var building = NewProperty("building", "Whole building");
        building.Lessors.Add(buildingLessor);
        var units = NewProperty("units", "Unit owned");
        units.SetIsUnitOwnership(true);
        units.Lessors.Add(buildingLessor);
        foreach (var property in new[] { building, units })
        {
            var save = db.Properties.Upsert(property);
            Assert.IsFalse(save.IsError, save.Error.FullDump);
        }

        var buildingA = NewListing(building, "building-a", "Building A");
        var buildingB = NewListing(building, "building-b", "Building B");
        var unitA = NewListing(units, "unit-a", "Unit A");
        var unitB = NewListing(units, "unit-b", "Unit B");
        unitA.Lessors.Add(unitLessor);
        buildingA.Lessors.Add(unitLessor);
        foreach (var listing in new[] { buildingA, buildingB, unitA, unitB })
        {
            var save = db.Listings.Upsert(listing.PropertyId, listing);
            Assert.IsFalse(save.IsError, save.Error.FullDump);
        }

        var buildingResult = db.Listings.Search(new RentCommercialListingSearchCriteria { LessorName = "BuildingOwner" });
        Assert.IsFalse(buildingResult.IsError, buildingResult.Error.FullDump);
        CollectionAssert.AreEquivalent(new[] { buildingA.Id, buildingB.Id }, buildingResult.ListingSearchResult.Select(item => item.Id).ToArray());
        var unitResult = db.Listings.Search(new RentCommercialListingSearchCriteria { LessorName = "UnitOwner" });
        Assert.IsFalse(unitResult.IsError, unitResult.Error.FullDump);
        CollectionAssert.AreEquivalent(new[] { unitA.Id }, unitResult.ListingSearchResult.Select(item => item.Id).ToArray());
        var absentResult = db.Listings.Search(new RentCommercialListingSearchCriteria { LessorName = "MissingOwner" });
        Assert.IsFalse(absentResult.IsError, absentResult.Error.FullDump);
        Assert.AreEqual(0, absentResult.ListingSearchResult.Count);
    }

    [TestMethod]
    public void SearchCriteria_CloneAndQueryKeepIndependentSelections()
    {
        var criteria = new ListingSearchCriteria
        {
            Keyword = "  retail  ",
            Use = new ListingUseTypeLabel(ListingUseType.Retail),
            CurrentStatus = new ListingAvailabilityStatusTypeLabel(ListingAvailabilityStatusType.Vacant),
            NoGuaranteeDeposit = true,
            PermitsRetail = true,
            MaximumRent = 100
        };
        criteria.PropertyKinds.Add(PropertyKindType.Retail);
        criteria.PropertyStructures.Add(StructureType.RC);
        var clone = criteria.Clone();
        var query = clone.ToApplicationQuery();
        criteria.PropertyKinds.Clear();
        criteria.PropertyStructures.Clear();
        clone.PropertyKinds.Add(PropertyKindType.Office);
        clone.PropertyStructures.Add(StructureType.Wood);

        Assert.AreEqual("retail", query.Keyword);
        Assert.AreEqual(ListingUseType.Retail, query.Use);
        Assert.AreEqual(ListingAvailabilityStatusType.Vacant, query.CurrentStatus);
        Assert.AreEqual(100d, query.MaximumRent);
        Assert.IsTrue(query.NoGuaranteeDeposit);
        Assert.IsTrue(query.PermitsRetail);
        CollectionAssert.AreEquivalent(new[] { PropertyKindType.Retail }, query.PropertyKinds.ToArray());
        CollectionAssert.AreEquivalent(new[] { StructureType.RC }, query.PropertyStructures.ToArray());
        Assert.AreEqual(0, criteria.PropertyKinds.Count);
        Assert.AreEqual(0, criteria.PropertyStructures.Count);
        Assert.AreEqual(2, clone.PropertyKinds.Count);
        Assert.AreEqual(2, clone.PropertyStructures.Count);
        clone.Use = new ListingUseTypeLabel(ListingUseType.Unspecified);
        clone.CurrentStatus = new ListingAvailabilityStatusTypeLabel(ListingAvailabilityStatusType.Unspecified);
        clone.Keyword = "  ";
        var unfiltered = clone.ToApplicationQuery();
        Assert.AreEqual("*", unfiltered.Keyword);
        Assert.IsNull(unfiltered.Use);
        Assert.IsNull(unfiltered.CurrentStatus);
    }

    [TestMethod]
    public void SaveNewProperty_WithQueuedListing_PersistsBoth()
    {
        using var db = new TestDatabase();
        var lessor = new PersonNatural("lessor", EntityStatus.New) { NameLast = "Lessor" };
        var broker = new PersonNatural("broker", EntityStatus.New) { NameLast = "Broker" };
        Assert.IsFalse(db.Access.UpsertRentLessor(lessor).IsError);
        Assert.IsFalse(db.Access.UpsertBroker(broker).IsError);
        var property = NewProperty("property", "Commercial building");
        var listing = new Listing("listing", EntityStatus.New, property.Id, EntityStatus.New, false, property.Name, property.FudousanId);
        listing.SetName("Unit A");
        listing.SetChinryou(120000m);
        listing.SetFudousanIdAdditionalCode("D012");
        listing.Lessors.Add(lessor);
        listing.Brokers.Add(broker);
        listing.Pictures.Add(new ListingPicture("picture", "picture.jpg", EntityStatus.New) { PictureType = ListingPictureType.Interior, IsMain = true });
        listing.Pdfs.Add(new ListingPdf("pdf", "document.pdf", "thumbnail.jpg", EntityStatus.New) { PdfType = ListingPdfType.Architectural, IsMain = true });
        property.ListingsToBeSaved.Add(listing);

        var save = db.Properties.Upsert(property);
        Assert.IsFalse(save.IsError, save.Error.Description);
        var loadedProperty = db.Properties.SelectById(property.Id);
        Assert.IsFalse(loadedProperty.IsError);
        Assert.IsNotNull(loadedProperty.Property);
        Assert.AreEqual("Commercial building", loadedProperty.Property.Name);
        Assert.AreEqual(1, loadedProperty.Property.ListingSummaries.Count);
        Assert.AreEqual(listing.Id, loadedProperty.Property.ListingSummaries[0].Id);
        var loaded = db.Listings.SelectById(property.Id, listing.Id);
        Assert.IsFalse(loaded.IsError);
        Assert.IsNotNull(loaded.Listing);
        Assert.AreEqual("Unit A", loaded.Listing.Name);
        Assert.AreEqual(120000m, loaded.Listing.Chinryou);
        Assert.AreEqual("D012", loaded.Listing.FudousanIdAdditionalCode);
        Assert.AreEqual(lessor.Id, loaded.Listing.Lessors.Single().Id);
        Assert.AreEqual(broker.Id, loaded.Listing.Brokers.Single().Id);
        Assert.AreEqual("picture.jpg", loaded.Listing.Pictures.Single().ImageFilename);
        Assert.AreEqual(ListingPictureType.Interior, loaded.Listing.Pictures.Single().PictureType);
        Assert.AreEqual("document.pdf", loaded.Listing.Pdfs.Single().PdfFilename);
        Assert.AreEqual(ListingPdfType.Architectural, loaded.Listing.Pdfs.Single().PdfType);
    }

    [TestMethod]
    public void QueuedListingAndDuplicate_PreserveUnspecifiedAndZeroAmounts()
    {
        using var db = new TestDatabase();
        var property = NewProperty("property", "Commercial building");
        var listing = new Listing("listing", EntityStatus.New, property.Id, EntityStatus.New, false, property.Name, property.FudousanId);
        listing.SetName("Unit A");
        listing.SetChinryou(0);
        listing.SetSecurityDeposit(0);
        listing.SetKeyMoney(0);
        listing.SetDepositDeduction(0);
        listing.SetRecontractFee(0);
        property.ListingsToBeSaved.Add(listing);
        var save = db.Properties.Upsert(property);
        Assert.IsFalse(save.IsError, save.Error.FullDump);

        var original = db.Listings.SelectById(property.Id, listing.Id);
        var duplicate = db.Listings.SelectById(property.Id, listing.Id, true);
        Assert.IsFalse(original.IsError, original.Error.FullDump);
        Assert.IsFalse(duplicate.IsError, duplicate.Error.FullDump);
        Assert.IsNotNull(original.Listing);
        Assert.IsNotNull(duplicate.Listing);
        Assert.AreNotEqual(original.Listing.Id, duplicate.Listing.Id);
        Assert.AreEqual(EntityStatus.New, duplicate.Listing.Status);
        AssertAmounts(original.Listing);
        AssertAmounts(duplicate.Listing);

        duplicate.Listing.SetChinryou(null);
        duplicate.Listing.SetManagementFee(0);
        var duplicateSave = db.Listings.Upsert(property.Id, duplicate.Listing);
        Assert.IsFalse(duplicateSave.IsError, duplicateSave.Error.FullDump);
        var reloaded = db.Listings.SelectById(property.Id, duplicate.Listing.Id);
        Assert.IsFalse(reloaded.IsError, reloaded.Error.FullDump);
        Assert.IsNotNull(reloaded.Listing);
        Assert.IsNull(reloaded.Listing.Chinryou);
        Assert.AreEqual(0m, reloaded.Listing.ManagementFee);
        Assert.AreEqual(0m, reloaded.Listing.SecurityDeposit);
        Assert.IsNull(reloaded.Listing.GuaranteeDeposit);
        var unchanged = db.Listings.SelectById(property.Id, listing.Id);
        Assert.IsFalse(unchanged.IsError, unchanged.Error.FullDump);
        Assert.IsNotNull(unchanged.Listing);
        AssertAmounts(unchanged.Listing);

        static void AssertAmounts(Listing actual)
        {
            Assert.AreEqual(0m, actual.Chinryou);
            Assert.IsNull(actual.ManagementFee);
            Assert.AreEqual(0m, actual.SecurityDeposit);
            Assert.IsNull(actual.GuaranteeDeposit);
            Assert.AreEqual(0m, actual.KeyMoney);
            Assert.IsNull(actual.RightsMoney);
            Assert.AreEqual(0m, actual.DepositDeduction);
            Assert.IsNull(actual.RenewalFee);
            Assert.AreEqual(0m, actual.RecontractFee);
        }
    }

    [TestMethod]
    public void DeleteListingAndProperty_LeavesUnrelatedRecordsIntact()
    {
        using var db = new TestDatabase();
        var propertyLessor = new PersonNatural("property-lessor", EntityStatus.New) { NameLast = "Property Lessor" };
        var listingLessor = new PersonNatural("listing-lessor", EntityStatus.New) { NameLast = "Listing Lessor" };
        var propertyBroker = new PersonNatural("property-broker", EntityStatus.New) { NameLast = "Property Broker" };
        var listingBroker = new PersonNatural("listing-broker", EntityStatus.New) { NameLast = "Listing Broker" };
        Assert.IsFalse(db.Access.UpsertRentLessor(propertyLessor).IsError);
        Assert.IsFalse(db.Access.UpsertRentLessor(listingLessor).IsError);
        Assert.IsFalse(db.Access.UpsertBroker(propertyBroker).IsError);
        Assert.IsFalse(db.Access.UpsertBroker(listingBroker).IsError);
        var first = NewProperty("property-a", "Building A");
        first.Lessors.Add(propertyLessor);
        first.Brokers.Add(propertyBroker);
        var second = NewProperty("property-b", "Building B");
        Assert.IsFalse(db.Properties.Upsert(first).IsError);
        Assert.IsFalse(db.Properties.Upsert(second).IsError);
        var firstListing = NewListing(first, "listing-a", "Unit A");
        firstListing.Pictures.Add(new ListingPicture("listing-picture", "image.jpg", EntityStatus.New));
        firstListing.Lessors.Add(listingLessor);
        firstListing.Brokers.Add(listingBroker);
        Assert.IsFalse(db.Listings.Upsert(first.Id, firstListing).IsError);
        Assert.IsFalse(db.Listings.Upsert(first.Id, NewListing(first, "listing-b", "Unit B")).IsError);
        Assert.IsFalse(db.Listings.Upsert(second.Id, NewListing(second, "listing-c", "Unit C")).IsError);
        Assert.AreEqual(1, db.Access.SelectPropertiesByKeyword("Listing Lessor", PropertySearchTarget.LessorName).PropertySearchResult.Count);
        Assert.AreEqual(1, db.Access.SelectPropertiesByKeyword("Listing Broker", PropertySearchTarget.BrokerName).PropertySearchResult.Count);

        var listingDelete = db.Listings.Delete(firstListing.Id);
        Assert.IsFalse(listingDelete.IsError);
        Assert.AreEqual(1, listingDelete.AffectedCount);
        Assert.IsNull(db.Listings.SelectById(first.Id, firstListing.Id).Listing);
        Assert.AreEqual(0L, db.Count("SELECT COUNT(*) FROM rent_lessors_commercial_listings WHERE listing_id = @id", firstListing.Id));
        Assert.AreEqual(0L, db.Count("SELECT COUNT(*) FROM brokers_commercial_listings WHERE listing_id = @id", firstListing.Id));
        Assert.AreEqual(1L, db.Count("SELECT COUNT(*) FROM rent_lessors_properties WHERE property_id = @id", first.Id));
        Assert.AreEqual(1L, db.Count("SELECT COUNT(*) FROM brokers_properties WHERE property_id = @id", first.Id));
        Assert.AreEqual(0, db.Access.SelectPropertiesByKeyword("Listing Lessor", PropertySearchTarget.LessorName).PropertySearchResult.Count);
        Assert.AreEqual(0, db.Access.SelectPropertiesByKeyword("Listing Broker", PropertySearchTarget.BrokerName).PropertySearchResult.Count);
        Assert.AreEqual(1, db.Access.SelectPropertiesByKeyword("Property Lessor", PropertySearchTarget.LessorName).PropertySearchResult.Count);
        Assert.AreEqual(1, db.Access.SelectPropertiesByKeyword("Property Broker", PropertySearchTarget.BrokerName).PropertySearchResult.Count);
        Assert.IsNotNull(db.Listings.SelectById(first.Id, "listing-b").Listing);
        var propertyDelete = db.Properties.Delete(first.Id);
        Assert.IsFalse(propertyDelete.IsError);
        Assert.AreEqual(1, propertyDelete.AffectedCount);
        Assert.IsNull(db.Properties.SelectById(first.Id).Property);
        Assert.IsNull(db.Listings.SelectById(first.Id, "listing-b").Listing);
        Assert.IsNotNull(db.Properties.SelectById(second.Id).Property);
        Assert.IsNotNull(db.Listings.SelectById(second.Id, "listing-c").Listing);
    }

    [TestMethod]
    public void PropertySave_DeletesQueuedListingContacts()
    {
        using var db = new TestDatabase();
        var lessor = new PersonNatural("lessor", EntityStatus.New) { NameLast = "Lessor" };
        var broker = new PersonNatural("broker", EntityStatus.New) { NameLast = "Broker" };
        Assert.IsFalse(db.Access.UpsertRentLessor(lessor).IsError);
        Assert.IsFalse(db.Access.UpsertBroker(broker).IsError);
        var property = NewProperty("property", "Commercial building");
        property.Lessors.Add(lessor);
        property.Brokers.Add(broker);
        Assert.IsFalse(db.Properties.Upsert(property).IsError);
        var listing = NewListing(property, "listing", "Unit A");
        listing.Lessors.Add(lessor);
        listing.Brokers.Add(broker);
        Assert.IsFalse(db.Listings.Upsert(property.Id, listing).IsError);

        property.ListingsToBeDeleted.Add(listing.Id);
        Assert.IsFalse(db.Properties.Upsert(property).IsError);
        Assert.IsNull(db.Listings.SelectById(property.Id, listing.Id).Listing);
        Assert.AreEqual(0L, db.Count("SELECT COUNT(*) FROM rent_lessors_commercial_listings WHERE listing_id = @id", listing.Id));
        Assert.AreEqual(0L, db.Count("SELECT COUNT(*) FROM brokers_commercial_listings WHERE listing_id = @id", listing.Id));
        var loadedProperty = db.Properties.SelectById(property.Id);
        Assert.IsFalse(loadedProperty.IsError);
        Assert.IsNotNull(loadedProperty.Property);
        Assert.AreEqual(lessor.Id, loadedProperty.Property.Lessors.Single().Id);
        Assert.AreEqual(broker.Id, loadedProperty.Property.Brokers.Single().Id);
    }

    [TestMethod]
    public void PropertyAndListing_TrackChangesLikeResidentials()
    {
        var property = new Property("property", EntityStatus.New);
        Assert.AreEqual("0000", property.FudousanIdAdditionalCode);
        property.SetIsModified(false);
        property.SetFloorCountAboveGround(0);
        property.SetFloorCountBasement(0);
        Assert.IsFalse(property.IsModified);
        property.SetFloorCountAboveGround(1);
        Assert.IsTrue(property.IsModified);
        property.SetIsModified(false);
        property.SetFloorCountBasement(1);
        Assert.IsTrue(property.IsModified);
        property.SetIsModified(false);
        property.SetPictures([]);
        Assert.IsTrue(property.IsModified);
        property.SetIsModified(false);
        property.SetPdfs([]);
        Assert.IsTrue(property.IsModified);
        property.SetIsModified(false);
        property.SetLessors([]);
        Assert.IsTrue(property.IsModified);
        property.SetIsModified(false);
        property.SetBrokers([]);
        Assert.IsTrue(property.IsModified);

        var listing = NewListing(property, "listing", "Unit A");
        listing.SetChinryou(0);
        listing.SetIsModified(false);
        listing.SetPropertyName("Renamed property");
        listing.SetChinryou(0);
        Assert.IsFalse(listing.IsModified);
        listing.SetChinryou(null);
        Assert.IsTrue(listing.IsModified);
        listing.SetIsModified(false);
        listing.Pictures = [];
        Assert.IsTrue(listing.IsModified);
        listing.SetIsModified(false);
        listing.Pdfs = [];
        Assert.IsTrue(listing.IsModified);
        listing.SetIsModified(false);
        listing.Lessors = [];
        Assert.IsTrue(listing.IsModified);
        listing.SetIsModified(false);
        listing.Brokers = [];
        Assert.IsTrue(listing.IsModified);
    }

    [TestMethod]
    [DataRow(false)]
    [DataRow(true)]
    public void PropertyEditor_InitializesModifiedStateLikeResidentials(bool isDuplicate)
    {
        var property = NewProperty($"initial-state-{Guid.NewGuid():N}", "Commercial building");
        Assert.IsTrue(property.IsModified);
        using var editor = new PropertyViewModel(property, isDuplicate, new NavigationStub(), null!, null!, null!, null!, null!, null!, null!);
        Assert.IsFalse(editor.IsInfoBarErrorOpen, editor.InfoBarErrorMessage);
        Assert.AreEqual(isDuplicate, property.IsModified);
        Assert.AreEqual(isDuplicate, editor.IsDirty);
        Assert.AreEqual(isDuplicate ? "複製_Commercial building" : "Commercial building", property.Name);
        Assert.AreEqual(isDuplicate ? "複製_Commercial building" : "Commercial building", editor.Name);
    }

    [TestMethod]
    [DataRow(false)]
    [DataRow(true)]
    public void PropertyEditor_PopulationFailureReportsErrorAndResetsModifiedState(bool isDuplicate)
    {
        var property = NewProperty($"population-failure-{Guid.NewGuid():N}", "Commercial building");
        var directory = Path.Combine(ZumenSearch.App.PropertyBlobDataFolder, property.Id);
        var sourceDirectory = Path.Combine(Path.GetTempPath(), $"ZumenSearch.Tests-{Guid.NewGuid():N}");
        var sourceFile = Path.Combine(sourceDirectory, "invalid.jpg");
        var destinationFile = Path.Combine(directory, "invalid.jpg");
        Assert.IsFalse(Directory.Exists(directory));
        Assert.IsFalse(Directory.Exists(sourceDirectory));
        try
        {
            Directory.CreateDirectory(sourceDirectory);
            File.WriteAllText(sourceFile, "picture");
            property.Pictures.Add(new PropertyPicture("invalid-picture", "invalid.jpg", EntityStatus.New) { BasePath = sourceDirectory, PictureType = (PropertyPictureType)int.MaxValue });

            using var editor = new PropertyViewModel(property, isDuplicate, new NavigationStub(), null!, null!, null!, null!, null!, null!, null!);
            Assert.IsTrue(editor.IsInfoBarErrorOpen);
            Assert.AreEqual(InfoBarSeverity.Error, editor.InfoBarSeverity);
            Assert.AreEqual("建物のデータの取得中に予期しないエラーが発生しました。", editor.InfoBarErrorTitle);
            StringAssert.Contains(editor.InfoBarErrorMessage, "このまま保存するとデータに欠損が生じる可能性があります。");
            StringAssert.Contains(editor.InfoBarErrorMessage, "PopulateValues");
            StringAssert.Contains(editor.InfoBarErrorMessage, "InvalidOperationException");
            Assert.AreEqual(0, editor.Pictures.Count);
            Assert.AreEqual(isDuplicate, property.IsModified);
            Assert.AreEqual(isDuplicate, editor.IsDirty);
            Assert.AreEqual(isDuplicate ? "複製_Commercial building" : "Commercial building", editor.Name);
            Assert.AreEqual(isDuplicate ? "複製_Commercial building" : "Commercial building", property.Name);
        }
        finally
        {
            if (File.Exists(destinationFile)) File.Delete(destinationFile);
            if (Directory.Exists(directory)) Directory.Delete(directory);
            if (File.Exists(sourceFile)) File.Delete(sourceFile);
            if (Directory.Exists(sourceDirectory)) Directory.Delete(sourceDirectory);
        }
    }

    [TestMethod]
    [DataRow(false)]
    [DataRow(true)]
    public void ListingEditor_PopulationFailureReportsErrorAndResetsModifiedState(bool isDuplicate)
    {
        var property = NewProperty($"listing-population-failure-{Guid.NewGuid():N}", "Commercial building");
        var listing = NewListing(property, "listing", "Unit A");
        var propertyDirectory = Path.Combine(ZumenSearch.App.PropertyBlobDataFolder, property.Id);
        var directory = Path.Combine(propertyDirectory, listing.Id);
        var sourceDirectory = Path.Combine(Path.GetTempPath(), $"ZumenSearch.Tests-{Guid.NewGuid():N}");
        var sourceFile = Path.Combine(sourceDirectory, "invalid.jpg");
        var destinationFile = Path.Combine(directory, "invalid.jpg");
        Assert.IsFalse(Directory.Exists(propertyDirectory));
        Assert.IsFalse(Directory.Exists(sourceDirectory));
        try
        {
            Directory.CreateDirectory(sourceDirectory);
            File.WriteAllText(sourceFile, "picture");
            listing.Pictures.Add(new ListingPicture("invalid-picture", "invalid.jpg", EntityStatus.New) { BasePath = sourceDirectory, PictureType = (ListingPictureType)int.MaxValue });

            using var editor = new ListingViewModel(listing, isDuplicate, new NavigationStub(), null!, null!, null!, null!);
            Assert.IsTrue(editor.IsInfoBarErrorOpen);
            Assert.AreEqual(InfoBarSeverity.Error, editor.InfoBarSeverity);
            Assert.AreEqual("募集物件のデータの取得中に予期しないエラーが発生しました。", editor.InfoBarErrorTitle);
            StringAssert.Contains(editor.InfoBarErrorMessage, "このまま保存するとデータに欠損が生じる可能性があります。");
            StringAssert.Contains(editor.InfoBarErrorMessage, "PopulateValues");
            StringAssert.Contains(editor.InfoBarErrorMessage, "InvalidOperationException");
            Assert.AreEqual(0, editor.Pictures.Count);
            Assert.AreEqual(isDuplicate, listing.IsModified);
            Assert.AreEqual(isDuplicate, editor.IsDirty);
            Assert.AreEqual(isDuplicate ? "複製_Unit A" : "Unit A", editor.Name);
            Assert.AreEqual(editor.Name, listing.Name);
        }
        finally
        {
            if (File.Exists(destinationFile)) File.Delete(destinationFile);
            if (Directory.Exists(directory)) Directory.Delete(directory);
            if (Directory.Exists(propertyDirectory)) Directory.Delete(propertyDirectory);
            if (File.Exists(sourceFile)) File.Delete(sourceFile);
            if (Directory.Exists(sourceDirectory)) Directory.Delete(sourceDirectory);
        }
    }

    [TestMethod]
    public async Task PropertyEditor_FacilitiesSaveReloadAndPreserveNullableParkingValues()
    {
        using var db = new TestDatabase();
        var property = NewProperty($"facilities-{Guid.NewGuid():N}", "Commercial building");
        var directory = Path.Combine(ZumenSearch.App.PropertyBlobDataFolder, property.Id);
        var originalCulture = CultureInfo.CurrentCulture;
        Assert.IsFalse(Directory.Exists(directory));
        try
        {
            CultureInfo.CurrentCulture = CultureInfo.InvariantCulture;
            using var editor = new PropertyViewModel(property, false, new NavigationStub(), null!, null!, null!, new RentCommercialPropertyService(db.Properties), null!, null!, null!);
            Assert.IsFalse(editor.IsInfoBarErrorOpen, editor.InfoBarErrorMessage);
            Assert.IsFalse(editor.IsDirty);
            editor.HasElectricity = true;
            editor.ElectricityDetails = "Electricity details";
            editor.HasGas = true;
            editor.SelectedGasType = editor.GasTypeOptions.First(item => item.Key == GasSupplyType.CityGas);
            editor.GasDetails = "Gas details";
            editor.HasWater = true;
            editor.SelectedWaterType = editor.WaterTypeOptions.First(item => item.Key == WaterSupplyType.Public);
            editor.WaterDetails = "Water details";
            editor.SelectedSewerageType = editor.SewerageTypeOptions.First(item => item.Key == SewerageType.SepticTank);
            editor.SewerageDetails = "Sewerage details";
            editor.HasInternet = true;
            editor.HasFiber = true;
            editor.InternetDetails = "Internet details";
            editor.HasFireSafety = true;
            editor.FireSafetyDetails = "Fire safety details";
            editor.HasParking = true;
            editor.SelectedParkingVacancy = editor.ParkingVacancyOptions.First(item => item.Key == ParkingVacancyType.Available);
            editor.ParkingSpaces = "2";
            editor.ParkingMonthlyFee = "15000.5";
            editor.SelectedParkingTax = editor.ParkingTaxOptions.First(item => item.Key == ParkingTaxType.Excluded);
            editor.IsParkingFree = true;
            editor.ParkingDetails = "Parking details";
            Assert.IsTrue(editor.IsDirty);
            Assert.IsFalse(editor.IsParkingFeeEnabled);
            Assert.AreEqual("現在の駐車場料金：無料", editor.ParkingFreeDescription);
            await editor.Save();
            Assert.IsFalse(editor.IsInfoBarErrorOpen, editor.InfoBarErrorMessage);
            Assert.IsFalse(editor.IsDirty);

            var result = db.Properties.SelectById(property.Id);
            Assert.IsFalse(result.IsError, result.Error.FullDump);
            Assert.IsNotNull(result.Property);
            var saved = result.Property;
            Assert.IsTrue(saved.HasElectricity);
            Assert.AreEqual("Electricity details", saved.ElectricityDetails);
            Assert.IsTrue(saved.HasGas);
            Assert.AreEqual(GasSupplyType.CityGas, saved.GasType);
            Assert.AreEqual("Gas details", saved.GasDetails);
            Assert.IsTrue(saved.HasWater);
            Assert.AreEqual(WaterSupplyType.Public, saved.WaterType);
            Assert.AreEqual("Water details", saved.WaterDetails);
            Assert.AreEqual(SewerageType.SepticTank, saved.SewerageType);
            Assert.AreEqual("Sewerage details", saved.SewerageDetails);
            Assert.IsTrue(saved.HasInternet);
            Assert.IsTrue(saved.HasFiber);
            Assert.AreEqual("Internet details", saved.InternetDetails);
            Assert.IsTrue(saved.HasFireSafety);
            Assert.AreEqual("Fire safety details", saved.FireSafetyDetails);
            Assert.IsTrue(saved.HasParking);
            Assert.AreEqual(ParkingVacancyType.Available, saved.ParkingVacancy);
            Assert.AreEqual(2, saved.ParkingSpaces);
            Assert.AreEqual(15000.5m, saved.ParkingMonthlyFee);
            Assert.AreEqual(ParkingTaxType.Excluded, saved.ParkingTax);
            Assert.IsTrue(saved.IsParkingFree);
            Assert.AreEqual("Parking details", saved.ParkingDetails);

            using var reopened = new PropertyViewModel(saved, false, new NavigationStub(), null!, null!, null!, new RentCommercialPropertyService(db.Properties), null!, null!, null!);
            Assert.IsFalse(reopened.IsInfoBarErrorOpen, reopened.InfoBarErrorMessage);
            Assert.IsFalse(reopened.IsDirty);
            Assert.IsTrue(reopened.HasElectricity);
            Assert.AreEqual("Electricity details", reopened.ElectricityDetails);
            Assert.IsTrue(reopened.HasGas);
            Assert.AreEqual(GasSupplyType.CityGas, reopened.SelectedGasType.Key);
            Assert.AreEqual("Gas details", reopened.GasDetails);
            Assert.IsTrue(reopened.HasWater);
            Assert.AreEqual(WaterSupplyType.Public, reopened.SelectedWaterType.Key);
            Assert.AreEqual("Water details", reopened.WaterDetails);
            Assert.AreEqual(SewerageType.SepticTank, reopened.SelectedSewerageType.Key);
            Assert.AreEqual("Sewerage details", reopened.SewerageDetails);
            Assert.IsTrue(reopened.HasInternet);
            Assert.IsTrue(reopened.HasFiber);
            Assert.AreEqual("Internet details", reopened.InternetDetails);
            Assert.IsTrue(reopened.HasFireSafety);
            Assert.AreEqual("Fire safety details", reopened.FireSafetyDetails);
            Assert.IsTrue(reopened.HasParking);
            Assert.AreEqual(ParkingVacancyType.Available, reopened.SelectedParkingVacancy.Key);
            Assert.AreEqual("2", reopened.ParkingSpaces);
            Assert.AreEqual("15000.5", reopened.ParkingMonthlyFee);
            Assert.AreEqual(ParkingTaxType.Excluded, reopened.SelectedParkingTax.Key);
            Assert.IsTrue(reopened.IsParkingFree);
            Assert.IsFalse(reopened.IsParkingFeeEnabled);
            Assert.AreEqual("Parking details", reopened.ParkingDetails);

            reopened.IsParkingFree = false;
            reopened.ParkingSpaces = "0";
            reopened.ParkingMonthlyFee = "0";
            await reopened.Save();
            Assert.IsFalse(reopened.IsInfoBarErrorOpen, reopened.InfoBarErrorMessage);
            result = db.Properties.SelectById(property.Id);
            Assert.IsFalse(result.IsError, result.Error.FullDump);
            Assert.IsNotNull(result.Property);
            Assert.AreEqual(0, result.Property.ParkingSpaces);
            Assert.AreEqual(0m, result.Property.ParkingMonthlyFee);

            reopened.ParkingSpaces = "";
            reopened.ParkingMonthlyFee = "";
            await reopened.Save();
            Assert.IsFalse(reopened.IsInfoBarErrorOpen, reopened.InfoBarErrorMessage);
            result = db.Properties.SelectById(property.Id);
            Assert.IsFalse(result.IsError, result.Error.FullDump);
            Assert.IsNotNull(result.Property);
            Assert.IsNull(result.Property.ParkingSpaces);
            Assert.IsNull(result.Property.ParkingMonthlyFee);
            using var reopenedBlank = new PropertyViewModel(result.Property, false, new NavigationStub(), null!, null!, null!, new RentCommercialPropertyService(db.Properties), null!, null!, null!);
            Assert.IsFalse(reopenedBlank.IsInfoBarErrorOpen, reopenedBlank.InfoBarErrorMessage);
            Assert.AreEqual("", reopenedBlank.ParkingSpaces);
            Assert.AreEqual("", reopenedBlank.ParkingMonthlyFee);
        }
        finally
        {
            CultureInfo.CurrentCulture = originalCulture;
            if (Directory.Exists(directory)) Directory.Delete(directory);
        }
    }

    [TestMethod]
    public async Task PropertyEditor_InvalidNameBlocksSaveUntilCorrected()
    {
        using var db = new TestDatabase();
        var property = NewProperty($"invalid-name-{Guid.NewGuid():N}", "Commercial building");
        using var editor = new PropertyViewModel(property, false, new NavigationStub(), null!, null!, null!, new RentCommercialPropertyService(db.Properties), null!, null!, null!);
        editor.Name = "";
        await editor.Save();
        Assert.IsTrue(editor.IsNameHasError);
        Assert.IsTrue(editor.IsInfoBarErrorOpen);
        Assert.IsTrue(editor.IsDirty);
        Assert.IsNull(db.Properties.SelectById(property.Id).Property);

        editor.Name = new string('A', 101);
        await editor.Save();
        Assert.IsTrue(editor.IsNameHasError);
        StringAssert.Contains(editor.InfoBarErrorMessage, "100");
        Assert.IsNull(db.Properties.SelectById(property.Id).Property);

        editor.Name = "Corrected name";
        await editor.Save();
        Assert.IsFalse(editor.IsNameHasError);
        Assert.IsFalse(editor.IsDirty);
        var saved = db.Properties.SelectById(property.Id);
        Assert.IsFalse(saved.IsError, saved.Error.FullDump);
        Assert.IsNotNull(saved.Property);
        Assert.AreEqual("Corrected name", saved.Property.Name);
    }

    [TestMethod]
    [DataRow(true)]
    [DataRow(false)]
    public async Task PropertyEditor_ValidatesFloorCountsLikeResidentials(bool aboveGround)
    {
        using var db = new TestDatabase();
        var property = NewProperty($"floor-count-{Guid.NewGuid():N}", "Commercial building");
        using var editor = new PropertyViewModel(property, false, new NavigationStub(), null!, null!, null!, new RentCommercialPropertyService(db.Properties), null!, null!, null!);
        if (aboveGround) editor.FloorCountAboveGround = "10000";
        else editor.FloorCountBasement = "10000";
        await editor.Save();
        Assert.IsTrue(editor.IsInfoBarErrorOpen);
        StringAssert.Contains(editor.InfoBarErrorMessage, aboveGround ? "地上階数" : "地下階数");
        StringAssert.Contains(editor.InfoBarErrorMessage, "4桁");
        Assert.IsTrue(editor.IsDirty);
        Assert.IsNull(db.Properties.SelectById(property.Id).Property);

        if (aboveGround) editor.FloorCountAboveGround = "+1";
        else editor.FloorCountBasement = "+1";
        await editor.Save();
        Assert.IsTrue(editor.IsDirty);
        Assert.IsNull(db.Properties.SelectById(property.Id).Property);

        editor.FloorCountAboveGround = "９９９９";
        editor.FloorCountBasement = "２";
        await editor.Save();
        Assert.IsFalse(editor.IsDirty);
        var saved = db.Properties.SelectById(property.Id);
        Assert.IsFalse(saved.IsError, saved.Error.FullDump);
        Assert.IsNotNull(saved.Property);
        Assert.AreEqual(9999, saved.Property.FloorCountAboveGround);
        Assert.AreEqual(2, saved.Property.FloorCountBasement);

        using var reopened = new PropertyViewModel(saved.Property, false, new NavigationStub(), null!, null!, null!, new RentCommercialPropertyService(db.Properties), null!, null!, null!);
        Assert.AreEqual("9999", reopened.FloorCountAboveGround);
        Assert.AreEqual("2", reopened.FloorCountBasement);
        reopened.FloorCountAboveGround = "";
        reopened.FloorCountBasement = "";
        await reopened.Save();
        Assert.IsFalse(reopened.IsDirty);
        var cleared = db.Properties.SelectById(property.Id);
        Assert.IsFalse(cleared.IsError, cleared.Error.FullDump);
        Assert.IsNotNull(cleared.Property);
        Assert.AreEqual(0, cleared.Property.FloorCountAboveGround);
        Assert.AreEqual(0, cleared.Property.FloorCountBasement);
    }

    [TestMethod]
    public async Task PropertyEditor_ConstructionDatePreviewUpdatesAndSurvivesReopen()
    {
        using var db = new TestDatabase();
        var property = NewProperty($"built-date-{Guid.NewGuid():N}", "Commercial building");
        using var editor = new PropertyViewModel(property, false, new NavigationStub(), null!, null!, null!, new RentCommercialPropertyService(db.Properties), null!, null!, null!);
        Assert.IsNull(editor.BuiltYearAndMonth);
        Assert.AreEqual("", editor.BuiltYearAndMonthPreview);
        var previewNotifications = 0;
        editor.PropertyChanged += (sender, args) =>
        {
            if (args.PropertyName == nameof(editor.BuiltYearAndMonthPreview)) previewNotifications++;
        };

        editor.BuiltYearAndMonth = new DateTimeOffset(2000, 2, 1, 0, 0, 0, TimeSpan.Zero);
        Assert.AreEqual("平成12年2月", editor.BuiltYearAndMonthPreview);
        editor.BuiltYearAndMonth = new DateTimeOffset(2022, 5, 1, 0, 0, 0, TimeSpan.Zero);
        Assert.AreEqual("令和4年5月", editor.BuiltYearAndMonthPreview);
        Assert.AreEqual(2, previewNotifications);
        await editor.Save();
        Assert.IsFalse(editor.IsDirty);
        var saved = db.Properties.SelectById(property.Id);
        Assert.IsFalse(saved.IsError, saved.Error.FullDump);
        Assert.IsNotNull(saved.Property);
        using var reopened = new PropertyViewModel(saved.Property, false, new NavigationStub(), null!, null!, null!, new RentCommercialPropertyService(db.Properties), null!, null!, null!);
        Assert.AreEqual(2022, reopened.BuiltYearAndMonth!.Value.Year);
        Assert.AreEqual(5, reopened.BuiltYearAndMonth.Value.Month);
        Assert.AreEqual("令和4年5月", reopened.BuiltYearAndMonthPreview);
        reopened.BuiltYearAndMonth = null;
        Assert.AreEqual("", reopened.BuiltYearAndMonthPreview);
        await reopened.Save();
        Assert.IsFalse(reopened.IsDirty);
        var cleared = db.Properties.SelectById(property.Id);
        Assert.IsFalse(cleared.IsError, cleared.Error.FullDump);
        Assert.IsNotNull(cleared.Property);
        using var reopenedCleared = new PropertyViewModel(cleared.Property, false, new NavigationStub(), null!, null!, null!, new RentCommercialPropertyService(db.Properties), null!, null!, null!);
        Assert.IsNull(reopenedCleared.BuiltYearAndMonth);
        Assert.AreEqual("", reopenedCleared.BuiltYearAndMonthPreview);
    }

    [TestMethod]
    public async Task PropertyEditor_RemarksSurviveReopenAndCanBeCleared()
    {
        using var db = new TestDatabase();
        var property = NewProperty($"remarks-{Guid.NewGuid():N}", "Commercial building");
        using var editor = new PropertyViewModel(property, false, new NavigationStub(), null!, null!, null!, new RentCommercialPropertyService(db.Properties), null!, null!, null!);
        Assert.AreEqual("", editor.Remarks);

        editor.Remarks = "123456789012345";
        Assert.IsTrue(editor.IsDirty);
        await editor.Save();
        Assert.IsFalse(editor.IsDirty);
        var saved = db.Properties.SelectById(property.Id);
        Assert.IsFalse(saved.IsError, saved.Error.FullDump);
        Assert.IsNotNull(saved.Property);
        Assert.AreEqual("123456789012345", saved.Property.Remarks);
        using var reopened = new PropertyViewModel(saved.Property, false, new NavigationStub(), null!, null!, null!, new RentCommercialPropertyService(db.Properties), null!, null!, null!);
        Assert.AreEqual("123456789012345", reopened.Remarks);
        reopened.Remarks = "";
        Assert.IsTrue(reopened.IsDirty);
        await reopened.Save();
        Assert.IsFalse(reopened.IsDirty);
        var cleared = db.Properties.SelectById(property.Id);
        Assert.IsFalse(cleared.IsError, cleared.Error.FullDump);
        Assert.IsNotNull(cleared.Property);
        Assert.AreEqual("", cleared.Property.Remarks);
    }

    [TestMethod]
    public async Task PropertyEditor_IdentifiersNormalizeValidateAndSurviveReopen()
    {
        using var db = new TestDatabase();
        var property = NewProperty($"identifiers-{Guid.NewGuid():N}", "Commercial building");
        using var editor = new PropertyViewModel(property, false, new NavigationStub(), null!, null!, null!, new RentCommercialPropertyService(db.Properties), null!, null!, null!);
        editor.FudousanId = "１２３";
        Assert.AreEqual("123", editor.FudousanId);
        await editor.Save();
        Assert.IsTrue(editor.IsDirty);
        Assert.IsTrue(editor.IsInfoBarErrorOpen);
        Assert.AreEqual("入力値にエラーがあります。物件情報の保存が出来ませんでした。", editor.InfoBarErrorTitle);
        Assert.AreEqual(InfoBarSeverity.Error, editor.InfoBarSeverity);
        StringAssert.Contains(editor.InfoBarErrorMessage, "不動産番号");
        StringAssert.Contains(editor.InfoBarErrorMessage, "SetValues");
        Assert.IsNull(db.Properties.SelectById(property.Id).Property);

        editor.FudousanId = "１２３４５６７８９０１２３";
        editor.FudousanIdAdditionalCode = "ａ１";
        Assert.AreEqual("1234567890123", editor.FudousanId);
        Assert.AreEqual("A1", editor.FudousanIdAdditionalCode);
        await editor.Save();
        Assert.IsTrue(editor.IsDirty);
        StringAssert.Contains(editor.InfoBarErrorMessage, "特定コード");
        Assert.IsNull(db.Properties.SelectById(property.Id).Property);

        editor.FudousanId = "invalid";
        editor.FudousanIdAdditionalCode = "A-12";
        Assert.AreEqual("1234567890123", editor.FudousanId);
        Assert.AreEqual("A1", editor.FudousanIdAdditionalCode);
        editor.FudousanIdAdditionalCode = "ａｂ１２";
        Assert.AreEqual("AB12", editor.FudousanIdAdditionalCode);
        await editor.Save();
        Assert.IsFalse(editor.IsDirty);
        var saved = db.Properties.SelectById(property.Id);
        Assert.IsFalse(saved.IsError, saved.Error.FullDump);
        Assert.IsNotNull(saved.Property);
        Assert.AreEqual("1234567890123", saved.Property.FudousanId);
        Assert.AreEqual("AB12", saved.Property.FudousanIdAdditionalCode);
        using var reopened = new PropertyViewModel(saved.Property, false, new NavigationStub(), null!, null!, null!, new RentCommercialPropertyService(db.Properties), null!, null!, null!);
        Assert.AreEqual("1234567890123", reopened.FudousanId);
        Assert.AreEqual("AB12", reopened.FudousanIdAdditionalCode);
        reopened.FudousanId = "";
        reopened.FudousanIdAdditionalCode = "";
        await reopened.Save();
        Assert.IsFalse(reopened.IsDirty);
        var cleared = db.Properties.SelectById(property.Id);
        Assert.IsFalse(cleared.IsError, cleared.Error.FullDump);
        Assert.IsNotNull(cleared.Property);
        Assert.AreEqual("", cleared.Property.FudousanId);
        Assert.AreEqual("", cleared.Property.FudousanIdAdditionalCode);
    }

    [TestMethod]
    public async Task PropertyEditor_SavesAndReopensBasicSelections()
    {
        using var db = new TestDatabase();
        var property = NewProperty($"selections-{Guid.NewGuid():N}", "Commercial building");
        using var editor = new PropertyViewModel(property, false, new NavigationStub(), null!, null!, null!, new RentCommercialPropertyService(db.Properties), null!, null!, null!);
        Assert.IsFalse(editor.IsInfoBarErrorOpen);
        Assert.IsFalse(editor.IsDirty);
        CollectionAssert.AreEqual(Enum.GetValues<PropertyKindType>(), editor.PropertyKinds.Select(x => x.Key).ToArray());
        CollectionAssert.AreEqual(Enum.GetValues<StructureType>(), editor.StructureTypes.Select(x => x.Key).ToArray());
        Assert.AreSame(editor.PropertyKinds.First(x => x.Key == PropertyKindType.Unspecified), editor.SelectedPropertyKind);
        Assert.AreSame(editor.StructureTypes.First(x => x.Key == StructureType.Unspecified), editor.SelectedStructureType);

        editor.SelectedPropertyKind = editor.PropertyKinds.First(x => x.Key == PropertyKindType.Retail);
        editor.SelectedStructureType = editor.StructureTypes.First(x => x.Key == StructureType.RC);
        Assert.IsTrue(editor.IsDirty);
        await editor.Save();
        Assert.IsFalse(editor.IsInfoBarErrorOpen, editor.InfoBarErrorMessage);
        Assert.IsFalse(editor.IsDirty);

        var result = db.Properties.SelectById(property.Id);
        Assert.IsFalse(result.IsError, result.Error.FullDump);
        Assert.IsNotNull(result.Property);
        Assert.AreEqual(PropertyKindType.Retail, result.Property.CommercialKind);
        Assert.AreEqual(StructureType.RC, result.Property.PropertyStructure);

        using var reopened = new PropertyViewModel(result.Property, false, new NavigationStub(), null!, null!, null!, new RentCommercialPropertyService(db.Properties), null!, null!, null!);
        Assert.IsFalse(reopened.IsInfoBarErrorOpen);
        Assert.IsFalse(reopened.IsDirty);
        Assert.AreEqual(PropertyKindType.Retail, reopened.SelectedPropertyKind.Key);
        Assert.AreEqual(StructureType.RC, reopened.SelectedStructureType.Key);
        reopened.SelectedPropertyKind = new PropertyTypeLabel(PropertyKindType.Retail);
        reopened.SelectedStructureType = new PropertyStructureTypeLabel(StructureType.RC);
        Assert.IsFalse(reopened.IsDirty);
        reopened.SelectedPropertyKind = reopened.PropertyKinds.First(x => x.Key == PropertyKindType.Unspecified);
        reopened.SelectedStructureType = reopened.StructureTypes.First(x => x.Key == StructureType.Unspecified);
        await reopened.Save();
        var reset = db.Properties.SelectById(property.Id);
        Assert.IsFalse(reset.IsError, reset.Error.FullDump);
        Assert.IsNotNull(reset.Property);
        Assert.AreEqual(PropertyKindType.Unspecified, reset.Property.CommercialKind);
        Assert.AreEqual(StructureType.Unspecified, reset.Property.PropertyStructure);
    }

    [TestMethod]
    public void ListingAvailabilityMonths_UseResidentialLabels()
    {
        var options = ListingAvailabilityMonthTypeLabel.GetAll();
        CollectionAssert.AreEqual(Enum.GetValues<ListingAvailabilityMonthType>(), options.Select(option => option.Key).ToArray());
        CollectionAssert.AreEqual(new[] { "未指定", "１月", "２月", "３月", "４月", "５月", "６月", "７月", "８月", "９月", "１０月", "１１月", "１２月" }, options.Select(option => option.Label).ToArray());
    }

    [TestMethod]
    public void SharedLabels_MatchResidentialValuesAndInvalidKeys()
    {
        CollectionAssert.AreEqual(ZumenSearch.Presentation.Features.Rent.Residentials.Models.ListingCurrentStatusTypeLabel.GetAll().Select(item => item.Label).ToArray(), ListingAvailabilityStatusTypeLabel.GetAll().Select(item => item.Label).ToArray());
        CollectionAssert.AreEqual(ZumenSearch.Presentation.Features.Rent.Residentials.Models.ListingAvailabilityPeriodTypeLabel.GetAll().Select(item => item.Label).ToArray(), ListingAvailabilityPeriodTypeLabel.GetAll().Select(item => item.Label).ToArray());
        CollectionAssert.AreEqual(ZumenSearch.Presentation.Features.Rent.Residentials.Models.ListingFloorGroundTypeLabel.GetAll().Select(item => item.Label).ToArray(), ListingFloorGroundTypeLabel.GetAll().Select(item => item.Label).ToArray());
        CollectionAssert.AreEqual(ZumenSearch.Presentation.Features.Rent.Residentials.Models.PropertyStructureTypeLabel.GetAll().Select(item => item.Label).ToArray(), PropertyStructureTypeLabel.GetAll().Select(item => item.Label).ToArray());
        Assert.ThrowsExactly<KeyNotFoundException>(() => new ListingAvailabilityStatusTypeLabel((ListingAvailabilityStatusType)999).Label);
        Assert.ThrowsExactly<KeyNotFoundException>(() => new ListingAvailabilityPeriodTypeLabel((ListingAvailabilityPeriodType)999).Label);
        Assert.ThrowsExactly<KeyNotFoundException>(() => new ListingFloorGroundTypeLabel((ListingFloorGroundType)999).Label);
        Assert.ThrowsExactly<KeyNotFoundException>(() => new PropertyStructureTypeLabel((StructureType)999).Label);
    }

    [TestMethod]
    public void ListingLabelParsers_MatchResidentialInputsAndFallbacks()
    {
        AssertParsers(ListingFeeUnitTypeLabel.ParseFeeUnitType, ZumenSearch.Presentation.Features.Rent.Residentials.Models.ListingFeeUnitTypeLabel.ParseFeeUnitType);
        AssertParsers(ListingContractTypeLabel.ParseContractType, ZumenSearch.Presentation.Features.Rent.Residentials.Models.ListingContractTypeLabel.ParseContractType);
        AssertParsers(ListingAvailabilityStatusTypeLabel.ParseCurrentStatus, ZumenSearch.Presentation.Features.Rent.Residentials.Models.ListingCurrentStatusTypeLabel.ParseCurrentStatus);
        AssertParsers(ListingAvailabilityMonthTypeLabel.ParseAvailabilityMonth, ZumenSearch.Presentation.Features.Rent.Residentials.Models.ListingAvailabilityMonthTypeLabel.ParseAvailabilityMonth);
        AssertParsers(ListingAvailabilityPeriodTypeLabel.ParseAvailabilityPeriod, ZumenSearch.Presentation.Features.Rent.Residentials.Models.ListingAvailabilityPeriodTypeLabel.ParseAvailabilityPeriod);
        AssertParsers(ListingFloorGroundTypeLabel.ParseFloorGroundType, ZumenSearch.Presentation.Features.Rent.Residentials.Models.ListingFloorGroundTypeLabel.ParseFloorGroundType);

        static void AssertParsers<TCommercial, TResidential>(Func<string?, TCommercial> commercial, Func<string?, TResidential> residential) where TCommercial : struct, Enum where TResidential : struct, Enum
        {
            var names = Enum.GetNames<TCommercial>();
            string?[] inputs = [null, "", " ", "invalid", "999", "-1", .. names, .. names.Select(name => name.ToLowerInvariant()), .. names.Select(name => $" {name} "), .. Enum.GetValues<TCommercial>().Select(value => Convert.ToInt32(value, CultureInfo.InvariantCulture).ToString(CultureInfo.InvariantCulture))];
            foreach (var input in inputs)
            {
                Assert.AreEqual(Convert.ToInt32(residential(input), CultureInfo.InvariantCulture), Convert.ToInt32(commercial(input), CultureInfo.InvariantCulture), $"{typeof(TCommercial).Name}: {input ?? "<null>"}");
            }
        }
    }

    [TestMethod]
    [DataRow(Property.EnumKanriShutai.Unspecified)]
    [DataRow(Property.EnumKanriShutai.Jisya)]
    [DataRow(Property.EnumKanriShutai.Tasya)]
    [DataRow(Property.EnumKanriShutai.Kashinushi)]
    public async Task PropertyEditor_ManagementSelectionsAndDetailsSurviveReopen(Property.EnumKanriShutai selection)
    {
        using var db = new TestDatabase();
        var property = NewProperty($"management-{Guid.NewGuid():N}", "Commercial building");
        using var editor = new PropertyViewModel(property, false, new NavigationStub(), null!, null!, null!, new RentCommercialPropertyService(db.Properties), null!, null!, null!);
        editor.SelectedKanriShutai = editor.KanriShutais.First(item => item.Key == selection);
        editor.KanriIsMaintenanceManagementIfIsKanriJisya = true;
        editor.KanriIsTenantManagementIfIsKanriJisya = true;
        editor.KanriIsPaymentManagementIfIsKanriJisya = true;
        editor.KanriIsOtherManagementIfIsKanriJisya = true;
        editor.KanriRemarkIfIsKanriJisya = "Self-managed details";
        editor.KanriNameOfCompanyIfIsKanriTasya = "Company";
        editor.KanriContactInfoOfCompanyIfIsKanriTasya = "Contact";
        editor.KanriRemarkIfIsKanriTasya = "Company details";
        editor.KanriRemarkIfIsKanriKashinushi = "Lessor details";
        await editor.Save();
        Assert.IsFalse(editor.IsInfoBarErrorOpen, editor.InfoBarErrorMessage);
        var saved = db.Properties.SelectById(property.Id);
        Assert.IsFalse(saved.IsError, saved.Error.FullDump);
        Assert.IsNotNull(saved.Property);
        using var reopened = new PropertyViewModel(saved.Property, false, new NavigationStub(), null!, null!, null!, new RentCommercialPropertyService(db.Properties), null!, null!, null!);
        Assert.IsFalse(reopened.IsInfoBarErrorOpen, reopened.InfoBarErrorMessage);
        Assert.AreEqual(selection, reopened.SelectedKanriShutai?.Key);
        Assert.AreEqual(selection == Property.EnumKanriShutai.Unspecified, reopened.IsKanriUnspecified);
        Assert.AreEqual(selection == Property.EnumKanriShutai.Jisya, reopened.IsKanriJisya);
        Assert.AreEqual(selection == Property.EnumKanriShutai.Tasya, reopened.IsKanriTasya);
        Assert.AreEqual(selection == Property.EnumKanriShutai.Kashinushi, reopened.IsKanriKashinushi);
        Assert.IsTrue(reopened.KanriIsMaintenanceManagementIfIsKanriJisya);
        Assert.IsTrue(reopened.KanriIsTenantManagementIfIsKanriJisya);
        Assert.IsTrue(reopened.KanriIsPaymentManagementIfIsKanriJisya);
        Assert.IsTrue(reopened.KanriIsOtherManagementIfIsKanriJisya);
        Assert.AreEqual("Self-managed details", reopened.KanriRemarkIfIsKanriJisya);
        Assert.AreEqual("Company", reopened.KanriNameOfCompanyIfIsKanriTasya);
        Assert.AreEqual("Contact", reopened.KanriContactInfoOfCompanyIfIsKanriTasya);
        Assert.AreEqual("Company details", reopened.KanriRemarkIfIsKanriTasya);
        Assert.AreEqual("Lessor details", reopened.KanriRemarkIfIsKanriKashinushi);
    }

    [TestMethod]
    public async Task PropertyEditor_NullManagementSelectionRetainsVisibilityAndSavesUnspecified()
    {
        using var db = new TestDatabase();
        var property = NewProperty("management-null", "Commercial building");
        using var editor = new PropertyViewModel(property, false, new NavigationStub(), null!, null!, null!, new RentCommercialPropertyService(db.Properties), null!, null!, null!);
        editor.SelectedKanriShutai = editor.KanriShutais.First(item => item.Key == Property.EnumKanriShutai.Tasya);
        editor.SelectedKanriShutai = null;

        Assert.IsTrue(editor.IsDirty);
        Assert.IsFalse(editor.IsKanriUnspecified);
        Assert.IsFalse(editor.IsKanriJisya);
        Assert.IsTrue(editor.IsKanriTasya);
        Assert.IsFalse(editor.IsKanriKashinushi);
        await editor.Save();
        Assert.IsFalse(editor.IsInfoBarErrorOpen, editor.InfoBarErrorMessage);
        var saved = db.Properties.SelectById(property.Id);
        Assert.IsFalse(saved.IsError, saved.Error.FullDump);
        Assert.IsNotNull(saved.Property);
        Assert.AreEqual(Property.EnumKanriShutai.Unspecified, saved.Property.KanriShutai);
    }

    [TestMethod]
    public async Task PropertyEditor_SavesAndReopensAddressSelections()
    {
        using var db = new TestDatabase();
        var property = NewProperty($"address-{Guid.NewGuid():N}", "Commercial building");
        Assert.IsFalse(db.Properties.Upsert(property).IsError);
        var loaded = db.Properties.SelectById(property.Id);
        Assert.IsFalse(loaded.IsError, loaded.Error.FullDump);
        Assert.IsNotNull(loaded.Property);
        var location = new LocationStub();
        using var editor = new PropertyViewModel(loaded.Property, false, new NavigationStub(), null!, null!, null!, new RentCommercialPropertyService(db.Properties), null!, null!, location);
        editor.SelectedPrefecture = editor.Prefectures.First();
        Assert.IsNotNull(editor.Cities);
        editor.SelectedCity = editor.Cities.Single();
        Assert.IsNotNull(editor.Towns);
        editor.SelectedTown = editor.Towns.Single();
        Assert.IsNotNull(editor.Chous);
        editor.SelectedChou = editor.Chous.Single();
        editor.Edaban = "10";
        var expectedPreview = $"{editor.SelectedPrefecture.Name}CityWardTown1-10";
        Assert.AreEqual(expectedPreview, editor.AddressPreview);
        Assert.IsTrue(editor.IsDirty);
        await editor.Save();
        Assert.IsFalse(editor.IsInfoBarErrorOpen, editor.InfoBarErrorMessage);
        var saved = db.Properties.SelectById(property.Id);
        Assert.IsFalse(saved.IsError, saved.Error.FullDump);
        Assert.IsNotNull(saved.Property);
        using var reopened = new PropertyViewModel(saved.Property, false, new NavigationStub(), null!, null!, null!, new RentCommercialPropertyService(db.Properties), null!, null!, location);
        Assert.IsFalse(reopened.IsInfoBarErrorOpen, reopened.InfoBarErrorMessage);
        Assert.IsFalse(reopened.IsDirty);
        Assert.AreEqual(editor.SelectedPrefecture.MunicipalityCode, reopened.SelectedPrefecture?.MunicipalityCode);
        Assert.AreEqual("City", reopened.SelectedCity?.City);
        Assert.AreEqual("Ward", reopened.SelectedTown?.Ward);
        Assert.AreEqual("Town", reopened.SelectedTown?.Oaza);
        Assert.AreEqual("1", reopened.SelectedChou?.Chou);
        Assert.AreEqual(expectedPreview, reopened.AddressPreview);
    }

    [TestMethod]
    public async Task PropertyEditor_PostalCodeLoadsValidatesAndSurvivesReopen()
    {
        using var db = new TestDatabase();
        var property = NewProperty($"postal-{Guid.NewGuid():N}", "Commercial building");
        property.Address.SetPostalCode("1234567");
        Assert.IsFalse(db.Properties.Upsert(property).IsError);
        var loaded = db.Properties.SelectById(property.Id);
        Assert.IsFalse(loaded.IsError, loaded.Error.FullDump);
        Assert.IsNotNull(loaded.Property);
        using var editor = new PropertyViewModel(loaded.Property, false, new NavigationStub(), null!, null!, null!, new RentCommercialPropertyService(db.Properties), null!, null!, null!);
        Assert.AreEqual("123-4567", editor.PostalCode);
        Assert.IsFalse(editor.IsDirty);
        var previewNotifications = 0;
        editor.PropertyChanged += (sender, args) =>
        {
            if (args.PropertyName == nameof(editor.AddressPreview)) previewNotifications++;
        };
        editor.PostalCode = "invalid";
        Assert.IsTrue(editor.IsDirty);
        Assert.AreEqual(1, previewNotifications);
        await editor.Save();
        Assert.IsTrue(editor.IsDirty);
        Assert.IsTrue(editor.IsInfoBarErrorOpen);
        StringAssert.Contains(editor.InfoBarErrorMessage, "Postal code");
        var unchanged = db.Properties.SelectById(property.Id);
        Assert.IsFalse(unchanged.IsError, unchanged.Error.FullDump);
        Assert.IsNotNull(unchanged.Property);
        Assert.AreEqual("123-4567", unchanged.Property.Address.PostalCode);

        editor.PostalCode = "7654321";
        await editor.Save();
        Assert.IsFalse(editor.IsDirty);
        var saved = db.Properties.SelectById(property.Id);
        Assert.IsFalse(saved.IsError, saved.Error.FullDump);
        Assert.IsNotNull(saved.Property);
        Assert.AreEqual("765-4321", saved.Property.Address.PostalCode);
        using var reopened = new PropertyViewModel(saved.Property, false, new NavigationStub(), null!, null!, null!, new RentCommercialPropertyService(db.Properties), null!, null!, null!);
        Assert.AreEqual("765-4321", reopened.PostalCode);
        reopened.Remarks = "Unrelated change";
        await reopened.Save();
        var resaved = db.Properties.SelectById(property.Id);
        Assert.IsFalse(resaved.IsError, resaved.Error.FullDump);
        Assert.IsNotNull(resaved.Property);
        Assert.AreEqual("765-4321", resaved.Property.Address.PostalCode);
    }

    [TestMethod]
    public async Task PropertyEditor_TransportationDialogsKeepBusyStateAndPersistSelectedEntry()
    {
        using var db = new TestDatabase();
        var property = NewProperty($"transportation-{Guid.NewGuid():N}", "Commercial building");
        Assert.IsFalse(db.Properties.Upsert(property).IsError);
        var loaded = db.Properties.SelectById(property.Id);
        Assert.IsFalse(loaded.IsError, loaded.Error.FullDump);
        Assert.IsNotNull(loaded.Property);
        var dialog = new DeleteDialogStub();
        using var editor = new PropertyViewModel(loaded.Property, false, new NavigationStub(), dialog, null!, null!, new RentCommercialPropertyService(db.Properties), null!, null!, null!);
        editor.SelectedTransportationIndex = 2;
        var lineChoice = new TaskCompletionSource<ZumenSearch.Domain.ValueObjects.Transportation.RailLine?>();
        dialog.RailLine = lineChoice.Task;
        var pendingLine = editor.SelectedTransportation.ShowRailLineSelectCommand.ExecuteAsync(null);
        Assert.IsTrue(editor.IsBusy);
        Assert.IsFalse(editor.IsEditorEnabled);
        editor.SelectedTransportationIndex = 1;
        lineChoice.SetResult(new("line-3", "Line 3"));
        await pendingLine;
        Assert.IsFalse(editor.IsBusy);
        Assert.AreEqual(string.Empty, editor.SelectedRailLine2.LineCode);
        editor.SelectedTransportationIndex = 2;
        Assert.AreEqual("line-3", editor.SelectedRailLine3.LineCode);
        Assert.IsTrue(editor.IsDirty);

        var stationChoice = new TaskCompletionSource<ZumenSearch.Domain.ValueObjects.Transportation.RailStation?>();
        dialog.RailStation = stationChoice.Task;
        var pendingStation = editor.SelectedTransportation.ShowRailStationSelectCommand.ExecuteAsync(null);
        Assert.IsTrue(editor.IsBusy);
        Assert.AreEqual("line-3", dialog.RequestedLineCode);
        stationChoice.SetResult(new("line-3", "station-3", "Station 3"));
        await pendingStation;
        Assert.IsFalse(editor.IsBusy);
        editor.SelectedTransportation.StationWalk = "１２";
        editor.SelectedTransportation.BusStop = "Stop 3";
        editor.SelectedTransportation.BusRide = "２３";
        editor.SelectedTransportation.BusWalk = "４";
        await editor.Save();
        Assert.IsFalse(editor.IsInfoBarErrorOpen, editor.InfoBarErrorMessage);
        var saved = db.Properties.SelectById(property.Id);
        Assert.IsFalse(saved.IsError, saved.Error.FullDump);
        Assert.IsNotNull(saved.Property);
        using var reopened = new PropertyViewModel(saved.Property, false, new NavigationStub(), dialog, null!, null!, new RentCommercialPropertyService(db.Properties), null!, null!, null!);
        reopened.SelectedTransportationIndex = 2;
        Assert.AreEqual("line-3", reopened.SelectedRailLine3.LineCode);
        Assert.AreEqual("station-3", reopened.SelectedRailStation3.StationCode);
        Assert.AreEqual("12", reopened.SelectedTransportation.StationWalk);
        Assert.AreEqual("Stop 3", reopened.SelectedTransportation.BusStop);
        Assert.AreEqual("23", reopened.SelectedTransportation.BusRide);
        Assert.AreEqual("4", reopened.SelectedTransportation.BusWalk);
        reopened.SelectedTransportationIndex = 1;
        Assert.AreEqual(string.Empty, reopened.SelectedRailLine2.LineCode);
    }

    [TestMethod]
    [DataRow(0)]
    [DataRow(1)]
    [DataRow(2)]
    [DataRow(3)]
    public async Task PropertyEditor_TransportationCommandsTargetTheirOwnSlot(int index)
    {
        var property = NewProperty($"transport-slot-{Guid.NewGuid():N}", "Commercial building");
        var dialog = new DeleteDialogStub();
        using var editor = new PropertyViewModel(property, false, new NavigationStub(), dialog, null!, null!, null!, null!, null!, null!);
        editor.SelectedTransportationIndex = index;
        var entry = editor.SelectedTransportation;
        var changed = new List<string?>();
        entry.PropertyChanged += (_, e) => changed.Add(e.PropertyName);
        var lineChoice = new TaskCompletionSource<ZumenSearch.Domain.ValueObjects.Transportation.RailLine?>();
        dialog.RailLine = lineChoice.Task;
        var pendingLine = entry.ShowRailLineSelectCommand.ExecuteAsync(null);
        editor.SelectedTransportationIndex = (index + 1) % 4;
        lineChoice.SetResult(new("line", "Line"));
        await pendingLine;
        Assert.AreEqual("Line", entry.RailLineName);
        Assert.AreEqual(string.Empty, editor.SelectedTransportation.RailLineName);
        CollectionAssert.Contains(changed, nameof(entry.RailLineName));

        var stationChoice = new TaskCompletionSource<ZumenSearch.Domain.ValueObjects.Transportation.RailStation?>();
        dialog.RailStation = stationChoice.Task;
        var pendingStation = entry.ShowRailStationSelectCommand.ExecuteAsync(null);
        stationChoice.SetResult(new("line", "station", "Station"));
        await pendingStation;
        Assert.AreEqual("Station", entry.RailStationName);
        Assert.AreEqual(string.Empty, editor.SelectedTransportation.RailStationName);
        CollectionAssert.Contains(changed, nameof(entry.RailStationName));
        entry.StationWalk = "7";
        entry.BusStop = "Stop";
        entry.BusRide = "8";
        entry.BusWalk = "9";
        CollectionAssert.Contains(changed, nameof(entry.StationWalk));
        CollectionAssert.Contains(changed, nameof(entry.BusStop));
        CollectionAssert.Contains(changed, nameof(entry.BusRide));
        CollectionAssert.Contains(changed, nameof(entry.BusWalk));

        dialog.RailLine = Task.FromResult<ZumenSearch.Domain.ValueObjects.Transportation.RailLine?>(new("other", "Other"));
        await entry.ShowRailLineSelectCommand.ExecuteAsync(null);
        Assert.AreEqual("Other", entry.RailLineName);
        Assert.AreEqual(string.Empty, entry.RailStationName);
        Assert.IsFalse(editor.IsBusy);
    }

    [TestMethod]
    public async Task PropertyEditor_TransportationDropsMismatchedStationOnLoadAndSave()
    {
        using var db = new TestDatabase();
        var property = NewProperty($"transport-mismatch-{Guid.NewGuid():N}", "Commercial building");
        property.Train1.SetRailLine("line", "Line");
        property.Train1.SetRailStation("other", "station", "Station");
        using var editor = new PropertyViewModel(property, false, new NavigationStub(), null!, null!, null!, new RentCommercialPropertyService(db.Properties), null!, null!, null!);
        Assert.AreEqual("line", editor.SelectedRailLine1.LineCode);
        Assert.AreEqual(string.Empty, editor.SelectedRailStation1.StationCode);
        editor.SelectedRailStation1 = new("other", "station", "Station");
        await editor.Save();
        Assert.IsFalse(editor.IsInfoBarErrorOpen, editor.InfoBarErrorMessage);
        var saved = db.Properties.SelectById(property.Id);
        Assert.IsFalse(saved.IsError, saved.Error.FullDump);
        Assert.IsNotNull(saved.Property);
        Assert.AreEqual("line", saved.Property.Train1.RailLine.LineCode);
        Assert.AreEqual(string.Empty, saved.Property.Train1.RailStation.StationCode);
    }

    [TestMethod]
    public async Task PropertyEditor_SavesClonedMediaAndKeepsEntityCollections()
    {
        using var db = new TestDatabase();
        var property = NewProperty($"media-{Guid.NewGuid():N}", "Commercial building");
        property.Pictures.Add(new PropertyPicture("picture", "picture.jpg", EntityStatus.New) { Description = "Original picture", IsMain = true });
        property.Pdfs.Add(new PropertyPdf("pdf", "document.pdf", "thumbnail.png", EntityStatus.New) { Description = "Original PDF", IsMain = true });
        Assert.IsFalse(db.Properties.Upsert(property).IsError);
        var loaded = db.Properties.SelectById(property.Id);
        Assert.IsFalse(loaded.IsError, loaded.Error.FullDump);
        Assert.IsNotNull(loaded.Property);
        var pictures = loaded.Property.Pictures;
        var pdfs = loaded.Property.Pdfs;

        using var editor = new PropertyViewModel(loaded.Property, false, new NavigationStub(), null!, null!, null!, new RentCommercialPropertyService(db.Properties), null!, null!, null!);
        Assert.IsFalse(editor.IsInfoBarErrorOpen);
        editor.Pictures.Single().Description = "Edited picture";
        editor.Pdfs.Single().Description = "Edited PDF";
        Assert.IsTrue(editor.IsDirty);
        Assert.AreEqual("Original picture", pictures.Single().Description);
        Assert.AreEqual("Original PDF", pdfs.Single().Description);
        await editor.Save();
        Assert.IsFalse(editor.IsInfoBarErrorOpen, editor.InfoBarErrorMessage);
        Assert.AreSame(pictures, loaded.Property.Pictures);
        Assert.AreSame(pdfs, loaded.Property.Pdfs);
        Assert.AreNotSame(editor.Pictures.Single().Model, pictures.Single());
        Assert.AreNotSame(editor.Pdfs.Single().Model, pdfs.Single());
        Assert.AreEqual("picture.jpg", loaded.Property.ThumbnailFilename);

        var result = db.Properties.SelectById(property.Id);
        Assert.IsFalse(result.IsError, result.Error.FullDump);
        Assert.IsNotNull(result.Property);
        Assert.AreEqual("Edited picture", result.Property.Pictures.Single().Description);
        Assert.AreEqual("Edited PDF", result.Property.Pdfs.Single().Description);
        Assert.AreEqual("picture.jpg", result.Property.ThumbnailFilename);
    }

    [TestMethod]
    public async Task PropertyEditor_MediaCollectionsTrackAddedAndRemovedWrappers()
    {
        using var db = new TestDatabase();
        var property = NewProperty($"media-collection-{Guid.NewGuid():N}", "Commercial building");
        Assert.IsFalse(db.Properties.Upsert(property).IsError);
        var loaded = db.Properties.SelectById(property.Id);
        Assert.IsFalse(loaded.IsError, loaded.Error.FullDump);
        Assert.IsNotNull(loaded.Property);
        using var editor = new PropertyViewModel(loaded.Property, false, new NavigationStub(), null!, null!, null!, new RentCommercialPropertyService(db.Properties), null!, null!, null!);
        var firstPicture = new PropertyPictureViewModel(new PropertyPicture("picture-1", "picture-1.jpg", EntityStatus.New), editor);
        var secondPicture = new PropertyPictureViewModel(new PropertyPicture("picture-2", "picture-2.jpg", EntityStatus.New), editor);
        var firstPdf = new PropertyPdfViewModel(new PropertyPdf("pdf-1", "pdf-1.pdf", "thumb-1.bmp", EntityStatus.New), editor);
        var secondPdf = new PropertyPdfViewModel(new PropertyPdf("pdf-2", "pdf-2.pdf", "thumb-2.bmp", EntityStatus.New), editor);
        Assert.IsFalse(editor.PropertyPictureDeleteAllCommand.CanExecute(null));
        Assert.IsFalse(editor.PropertyPdfDeleteAllCommand.CanExecute(null));
        editor.Pictures.Add(firstPicture);
        editor.Pictures.Add(secondPicture);
        editor.Pdfs.Add(firstPdf);
        editor.Pdfs.Add(secondPdf);
        Assert.IsTrue(editor.PropertyPictureDeleteAllCommand.CanExecute(null));
        Assert.IsTrue(editor.PropertyPdfDeleteAllCommand.CanExecute(null));
        Assert.IsTrue(editor.IsDirty);
        firstPicture.IsMain = true;
        secondPicture.IsMain = true;
        firstPdf.IsMain = true;
        secondPdf.IsMain = true;
        Assert.IsFalse(firstPicture.IsMain);
        Assert.IsFalse(firstPdf.IsMain);
        Assert.IsTrue(secondPicture.IsMain);
        Assert.IsTrue(secondPdf.IsMain);
        Assert.IsTrue(editor.Pictures.Remove(firstPicture));
        Assert.IsTrue(editor.Pdfs.Remove(firstPdf));
        Assert.IsTrue(editor.PropertyPictureDeleteAllCommand.CanExecute(null));
        Assert.IsTrue(editor.PropertyPdfDeleteAllCommand.CanExecute(null));
        Assert.IsTrue(editor.Pictures.Remove(secondPicture));
        Assert.IsTrue(editor.Pdfs.Remove(secondPdf));
        Assert.IsFalse(editor.PropertyPictureDeleteAllCommand.CanExecute(null));
        Assert.IsFalse(editor.PropertyPdfDeleteAllCommand.CanExecute(null));
        await editor.Save();
        Assert.IsFalse(editor.IsInfoBarErrorOpen, editor.InfoBarErrorMessage);
        Assert.IsFalse(editor.IsDirty);
        firstPicture.Description = "Removed picture";
        firstPdf.Description = "Removed PDF";
        Assert.IsFalse(editor.IsDirty);

        editor.Dispose();
        secondPicture.Description = "Changed after disposal";
        secondPdf.Description = "Changed after disposal";
        Assert.IsFalse(editor.IsDirty);

        if (Directory.Exists(Path.Combine(ZumenSearch.App.PropertyBlobDataFolder, property.Id)))
        {
            Directory.Delete(Path.Combine(ZumenSearch.App.PropertyBlobDataFolder, property.Id), true);
        }
        editor.OpenPropertyBlobDirectoryCommand.Execute(null);
        Assert.IsTrue(editor.IsInfoBarErrorOpen);
        Assert.AreEqual("フォルダーを開けませんでした", editor.InfoBarErrorTitle);
        Assert.IsTrue(editor.InfoBarErrorMessage.Contains("データフォルダーが存在しません"));
    }

    [TestMethod]
    public void PropertyEditor_DuplicateContinuesAfterMediaCopyFailures()
    {
        var property = NewProperty($"duplicate-media-{Guid.NewGuid():N}", "Commercial building");
        var directory = Path.Combine(ZumenSearch.App.PropertyBlobDataFolder, property.Id);
        var sourceDirectory = Path.Combine(Path.GetTempPath(), $"ZumenSearch.Tests-{Guid.NewGuid():N}");
        var sourceFiles = new[] { "picture.jpg", "document.pdf", "thumbnail.bmp", "partial.pdf" };
        try
        {
            Assert.IsFalse(Directory.Exists(directory));
            Assert.IsFalse(Directory.Exists(sourceDirectory));
            Directory.CreateDirectory(sourceDirectory);
            foreach (var filename in sourceFiles) File.WriteAllText(Path.Combine(sourceDirectory, filename), filename);
            property.Pictures.Add(new PropertyPicture("missing-picture", "missing.jpg", EntityStatus.New) { BasePath = sourceDirectory });
            property.Pictures.Add(new PropertyPicture("picture", "picture.jpg", EntityStatus.New) { BasePath = sourceDirectory });
            property.Pdfs.Add(new PropertyPdf("partial", "partial.pdf", "missing-thumb.bmp", EntityStatus.New) { BasePath = sourceDirectory });
            property.Pdfs.Add(new PropertyPdf("pdf", "document.pdf", "thumbnail.bmp", EntityStatus.New) { BasePath = sourceDirectory });

            using var editor = new PropertyViewModel(property, true, new NavigationStub(), null!, null!, null!, null!, null!, null!, null!);
            Assert.IsTrue(editor.IsInfoBarErrorOpen);
            Assert.IsTrue(editor.IsEditorEnabled);
            Assert.IsTrue(editor.IsDirty);
            Assert.AreEqual("picture", editor.Pictures.Single().Id);
            Assert.AreEqual("pdf", editor.Pdfs.Single().Id);
            Assert.IsTrue(editor.Pictures.Single().IsModified);
            Assert.IsTrue(editor.Pdfs.Single().IsModified);
            foreach (var filename in sourceFiles) Assert.AreEqual(filename, File.ReadAllText(Path.Combine(directory, filename)));
            editor.DiscardChanges();
            Assert.IsFalse(Directory.Exists(directory));
            foreach (var filename in sourceFiles) Assert.IsTrue(File.Exists(Path.Combine(sourceDirectory, filename)));
        }
        finally
        {
            foreach (var filename in sourceFiles)
            {
                if (Directory.Exists(directory)) File.Delete(Path.Combine(directory, filename));
                if (Directory.Exists(sourceDirectory)) File.Delete(Path.Combine(sourceDirectory, filename));
            }
            if (Directory.Exists(directory)) Directory.Delete(directory);
            if (Directory.Exists(sourceDirectory)) Directory.Delete(sourceDirectory);
        }
    }

    [TestMethod]
    public async Task PropertyEditor_ImportsPicturesContinuesAfterErrorsAndDiscardsOrSaves()
    {
        using var db = new TestDatabase();
        var property = NewProperty($"media-import-{Guid.NewGuid():N}", "Commercial building");
        Assert.IsFalse(db.Properties.Upsert(property).IsError);
        var loaded = db.Properties.SelectById(property.Id);
        Assert.IsFalse(loaded.IsError, loaded.Error.FullDump);
        Assert.IsNotNull(loaded.Property);
        var directory = Path.Combine(ZumenSearch.App.PropertyBlobDataFolder, property.Id);
        var source = Path.Combine(Path.GetTempPath(), $"ZumenSearch.Tests-{Guid.NewGuid():N}.jpg");
        var missing = Path.Combine(Path.GetTempPath(), $"ZumenSearch.Tests-{Guid.NewGuid():N}.jpg");
        var importedFiles = new List<string>();
        try
        {
            Assert.IsFalse(Directory.Exists(directory));
            File.WriteAllText(source, "picture content");
            using (var editor = new PropertyViewModel(loaded.Property, false, new NavigationStub(), null!, null!, null!, new RentCommercialPropertyService(db.Properties), null!, null!, null!))
            {
                var busyStates = new List<bool>();
                editor.PropertyChanged += (_, e) => { if (e.PropertyName == nameof(editor.IsBusy)) busyStates.Add(editor.IsBusy); };
                await editor.AddNewPropertyPicturesCommand.ExecuteAsync(new List<string> { missing, source });
                importedFiles.AddRange(editor.Pictures.Select(picture => Path.Combine(directory, picture.ImageFilename)));
                CollectionAssert.AreEqual(new[] { true, false }, busyStates);
                Assert.IsTrue(editor.IsInfoBarErrorOpen);
                StringAssert.Contains(editor.InfoBarErrorMessage, Path.GetFileName(missing));
                Assert.AreEqual(1, editor.Pictures.Count);
                Assert.AreEqual("picture content", File.ReadAllText(importedFiles.Single()));
                Assert.IsTrue(editor.IsDirty);
                editor.DiscardChanges();
                Assert.IsFalse(File.Exists(importedFiles.Single()));
                Assert.IsTrue(File.Exists(source));
            }

            using (var editor = new PropertyViewModel(loaded.Property, false, new NavigationStub(), null!, null!, null!, new RentCommercialPropertyService(db.Properties), null!, null!, null!))
            {
                await editor.AddNewPropertyPicturesCommand.ExecuteAsync(new List<string> { source });
                importedFiles.AddRange(editor.Pictures.Select(picture => Path.Combine(directory, picture.ImageFilename)));
                Assert.AreEqual(1, editor.Pictures.Count);
                editor.Pictures.Single().Description = "Imported picture";
                await editor.Save();
                Assert.IsFalse(editor.IsInfoBarErrorOpen, editor.InfoBarErrorMessage);
                Assert.IsFalse(editor.IsDirty);
                Assert.IsTrue(File.Exists(importedFiles.Last()));
            }

            var saved = db.Properties.SelectById(property.Id);
            Assert.IsFalse(saved.IsError, saved.Error.FullDump);
            Assert.IsNotNull(saved.Property);
            Assert.AreEqual(Path.GetFileName(importedFiles.Last()), saved.Property.Pictures.Single().ImageFilename);
            Assert.AreEqual("Imported picture", saved.Property.Pictures.Single().Description);
        }
        finally
        {
            foreach (var file in importedFiles) File.Delete(file);
            File.Delete(source);
            if (Directory.Exists(directory)) Directory.Delete(directory);
        }
    }

    [TestMethod]
    public async Task PropertyEditor_ReportsFailedDeletionWithoutRetryingOnLaterSave()
    {
        using var db = new TestDatabase();
        var property = NewProperty($"media-cleanup-{Guid.NewGuid():N}", "Commercial building");
        Assert.IsFalse(db.Properties.Upsert(property).IsError);
        var loaded = db.Properties.SelectById(property.Id);
        Assert.IsFalse(loaded.IsError, loaded.Error.FullDump);
        Assert.IsNotNull(loaded.Property);
        var directory = Path.Combine(ZumenSearch.App.PropertyBlobDataFolder, property.Id);
        var source = Path.Combine(Path.GetTempPath(), $"ZumenSearch.Tests-{Guid.NewGuid():N}.jpg");
        string? importedPath = null;
        try
        {
            Assert.IsFalse(Directory.Exists(directory));
            File.WriteAllText(source, "picture content");
            var dialog = new DeleteDialogStub { Result = ContentDialogResult.Primary };
            using var editor = new PropertyViewModel(loaded.Property, false, new NavigationStub(), dialog, null!, null!, new RentCommercialPropertyService(db.Properties), null!, null!, null!);
            await editor.AddNewPropertyPicturesCommand.ExecuteAsync(new List<string> { source });
            var imported = editor.Pictures.Single();
            importedPath = Path.Combine(directory, imported.ImageFilename);
            await editor.DeletePropertyPictureCommand.ExecuteAsync(imported);
            editor.Name = "Changed building";
            using (var lockedFile = new FileStream(importedPath, FileMode.Open, FileAccess.Read, FileShare.None))
            {
                await editor.Save();
                Assert.IsTrue(editor.IsInfoBarErrorOpen);
                StringAssert.Contains(editor.InfoBarErrorMessage, importedPath);
                Assert.IsFalse(editor.IsDirty);
                Assert.IsFalse(editor.IsBusy);
                var saved = db.Properties.SelectById(property.Id);
                Assert.IsFalse(saved.IsError, saved.Error.FullDump);
                Assert.IsNotNull(saved.Property);
                Assert.AreEqual("Changed building", saved.Property.Name);
                Assert.AreEqual(0, saved.Property.Pictures.Count);
            }

            editor.Name = "Changed again";
            await editor.Save();
            Assert.IsFalse(editor.IsInfoBarErrorOpen, editor.InfoBarErrorMessage);
            Assert.IsFalse(editor.IsDirty);
            Assert.IsTrue(File.Exists(importedPath));
            var savedAgain = db.Properties.SelectById(property.Id);
            Assert.IsFalse(savedAgain.IsError, savedAgain.Error.FullDump);
            Assert.IsNotNull(savedAgain.Property);
            Assert.AreEqual("Changed again", savedAgain.Property.Name);
            Assert.AreEqual(0, savedAgain.Property.Pictures.Count);
        }
        finally
        {
            if (importedPath is not null) File.Delete(importedPath);
            File.Delete(source);
            if (Directory.Exists(directory)) Directory.Delete(directory);
        }
    }

    [TestMethod]
    public async Task PropertyEditor_ReportsPdfImportErrorsAndReleasesBusyState()
    {
        var property = NewProperty($"pdf-import-{Guid.NewGuid():N}", "Commercial building");
        var directory = Path.Combine(ZumenSearch.App.PropertyBlobDataFolder, property.Id);
        var missing = Path.Combine(Path.GetTempPath(), $"ZumenSearch.Tests-{Guid.NewGuid():N}.pdf");
        try
        {
            Assert.IsFalse(Directory.Exists(directory));
            using var editor = new PropertyViewModel(property, false, new NavigationStub(), null!, null!, null!, null!, null!, null!, null!);
            var busyStates = new List<bool>();
            editor.PropertyChanged += (_, e) => { if (e.PropertyName == nameof(editor.IsBusy)) busyStates.Add(editor.IsBusy); };
            await editor.AddNewPropertyPdfsCommand.ExecuteAsync(new List<string> { "invalid.txt", missing });
            CollectionAssert.AreEqual(new[] { true, false }, busyStates);
            Assert.IsTrue(editor.IsInfoBarErrorOpen);
            StringAssert.Contains(editor.InfoBarErrorMessage, Path.GetFileName(missing));
            Assert.AreEqual(0, editor.Pdfs.Count);
            Assert.IsFalse(editor.IsDirty);
            Assert.IsTrue(editor.IsEditorEnabled);
            Assert.AreEqual(0, Directory.GetFiles(directory).Length);
        }
        finally
        {
            if (Directory.Exists(directory)) Directory.Delete(directory);
        }
    }

    [TestMethod]
    public async Task PropertyEditor_ImportsPdfAndThumbnailAndDiscardsOrSaves()
    {
        using var db = new TestDatabase();
        var property = NewProperty($"pdf-import-{Guid.NewGuid():N}", "Commercial building");
        Assert.IsFalse(db.Properties.Upsert(property).IsError);
        var loaded = db.Properties.SelectById(property.Id);
        Assert.IsFalse(loaded.IsError, loaded.Error.FullDump);
        Assert.IsNotNull(loaded.Property);
        var directory = Path.Combine(ZumenSearch.App.PropertyBlobDataFolder, property.Id);
        var source = Path.Combine(Path.GetTempPath(), $"ZumenSearch.Tests-{Guid.NewGuid():N}.pdf");
        var importedFiles = new List<string>();
        try
        {
            Assert.IsFalse(Directory.Exists(directory));
            var pdf = new System.Text.StringBuilder("%PDF-1.4\n");
            var offsets = new List<int>();
            foreach (var item in new[] { "1 0 obj\n<< /Type /Catalog /Pages 2 0 R >>\nendobj\n", "2 0 obj\n<< /Type /Pages /Kids [3 0 R] /Count 1 >>\nendobj\n", "3 0 obj\n<< /Type /Page /Parent 2 0 R /MediaBox [0 0 100 100] >>\nendobj\n" })
            {
                offsets.Add(pdf.Length);
                pdf.Append(item);
            }
            var xref = pdf.Length;
            pdf.Append("xref\n0 4\n0000000000 65535 f \n");
            foreach (var offset in offsets) pdf.Append($"{offset:D10} 00000 n \n");
            pdf.Append($"trailer\n<< /Size 4 /Root 1 0 R >>\nstartxref\n{xref}\n%%EOF\n");
            File.WriteAllText(source, pdf.ToString(), System.Text.Encoding.ASCII);

            using (var editor = new PropertyViewModel(loaded.Property, false, new NavigationStub(), null!, null!, null!, new RentCommercialPropertyService(db.Properties), null!, null!, null!))
            {
                await editor.AddNewPropertyPdfsCommand.ExecuteAsync(new List<string> { source });
                Assert.IsFalse(editor.IsInfoBarErrorOpen, editor.InfoBarErrorMessage);
                var imported = editor.Pdfs.Single();
                importedFiles.Add(Path.Combine(directory, imported.PdfFilename));
                importedFiles.Add(Path.Combine(directory, imported.Model.ThumbnailFilename));
                Assert.IsTrue(importedFiles.All(File.Exists));
                Assert.IsTrue(editor.IsDirty);
                editor.DiscardChanges();
                Assert.IsTrue(importedFiles.All(file => !File.Exists(file)));
                Assert.IsTrue(File.Exists(source));
            }

            using (var editor = new PropertyViewModel(loaded.Property, false, new NavigationStub(), null!, null!, null!, new RentCommercialPropertyService(db.Properties), null!, null!, null!))
            {
                await editor.AddNewPropertyPdfsCommand.ExecuteAsync(new List<string> { source });
                Assert.IsFalse(editor.IsInfoBarErrorOpen, editor.InfoBarErrorMessage);
                var imported = editor.Pdfs.Single();
                importedFiles.Add(Path.Combine(directory, imported.PdfFilename));
                importedFiles.Add(Path.Combine(directory, imported.Model.ThumbnailFilename));
                imported.Description = "Imported PDF";
                await editor.Save();
                Assert.IsFalse(editor.IsInfoBarErrorOpen, editor.InfoBarErrorMessage);
                Assert.IsFalse(editor.IsDirty);
                Assert.IsTrue(importedFiles.Skip(2).All(File.Exists));
            }

            var saved = db.Properties.SelectById(property.Id);
            Assert.IsFalse(saved.IsError, saved.Error.FullDump);
            Assert.IsNotNull(saved.Property);
            Assert.AreEqual(Path.GetFileName(importedFiles[2]), saved.Property.Pdfs.Single().PdfFilename);
            Assert.AreEqual(Path.GetFileName(importedFiles[3]), saved.Property.Pdfs.Single().ThumbnailFilename);
            Assert.AreEqual("Imported PDF", saved.Property.Pdfs.Single().Description);
        }
        finally
        {
            foreach (var file in importedFiles) File.Delete(file);
            File.Delete(source);
            if (Directory.Exists(directory)) Directory.Delete(directory);
        }
    }

    [TestMethod]
    [DataRow("picture")]
    [DataRow("pictures")]
    [DataRow("pdf")]
    [DataRow("pdfs")]
    public async Task PropertyEditor_MediaDeletionKeepsBusyStateAndPersists(string operation)
    {
        using var db = new TestDatabase();
        var property = NewProperty($"media-delete-{Guid.NewGuid():N}", "Commercial building");
        property.Pictures.Add(new PropertyPicture("picture-1", "picture-1.jpg", EntityStatus.New));
        property.Pictures.Add(new PropertyPicture("picture-2", "picture-2.jpg", EntityStatus.New));
        property.Pdfs.Add(new PropertyPdf("pdf-1", "document-1.pdf", "thumbnail-1.png", EntityStatus.New));
        property.Pdfs.Add(new PropertyPdf("pdf-2", "document-2.pdf", "thumbnail-2.png", EntityStatus.New));
        using var mediaFiles = new TestPropertyMediaFiles(property.Id);
        Assert.IsFalse(db.Properties.Upsert(property).IsError);
        var loaded = db.Properties.SelectById(property.Id);
        Assert.IsFalse(loaded.IsError, loaded.Error.FullDump);
        Assert.IsNotNull(loaded.Property);
        var dialog = new DeleteDialogStub();
        using var editor = new PropertyViewModel(loaded.Property, false, new NavigationStub(), dialog, null!, null!, new RentCommercialPropertyService(db.Properties), null!, null!, null!);
        Assert.IsFalse(editor.IsInfoBarErrorOpen, editor.InfoBarErrorMessage);
        Assert.IsFalse(editor.IsDirty);

        Task Delete() => operation switch
        {
            "picture" => editor.DeletePropertyPictureCommand.ExecuteAsync(editor.Pictures.First()),
            "pictures" => editor.PropertyPictureDeleteAllCommand.ExecuteAsync(null),
            "pdf" => editor.DeletePropertyPdfCommand.ExecuteAsync(editor.Pdfs.First()),
            "pdfs" => editor.PropertyPdfDeleteAllCommand.ExecuteAsync(null),
            _ => throw new InvalidOperationException(operation)
        };

        var confirmation = new TaskCompletionSource<ContentDialogResult>();
        dialog.Confirmation = confirmation.Task;
        var pending = Delete();
        Assert.IsTrue(editor.IsBusy);
        Assert.IsFalse(editor.IsEditorEnabled);
        confirmation.SetResult(ContentDialogResult.None);
        await pending;
        Assert.IsFalse(editor.IsBusy);
        Assert.IsTrue(editor.IsEditorEnabled);
        Assert.IsFalse(editor.IsDirty);
        Assert.AreEqual(2, editor.Pictures.Count);
        Assert.AreEqual(2, editor.Pdfs.Count);

        dialog.Confirmation = Task.FromException<ContentDialogResult>(new InvalidOperationException("confirmation failed"));
        var exception = await Assert.ThrowsExactlyAsync<InvalidOperationException>(Delete);
        Assert.AreEqual("confirmation failed", exception.Message);
        Assert.IsFalse(editor.IsBusy);
        Assert.IsTrue(editor.IsEditorEnabled);
        Assert.IsFalse(editor.IsDirty);
        Assert.AreEqual(2, editor.Pictures.Count);
        Assert.AreEqual(2, editor.Pdfs.Count);

        dialog.Confirmation = Task.FromResult(ContentDialogResult.Primary);
        await Delete();
        Assert.IsFalse(editor.IsBusy);
        Assert.IsTrue(editor.IsDirty);
        var expectedPictures = operation == "picture" ? 1 : operation == "pictures" ? 0 : 2;
        var expectedPdfs = operation == "pdf" ? 1 : operation == "pdfs" ? 0 : 2;
        Assert.AreEqual(expectedPictures, editor.Pictures.Count);
        Assert.AreEqual(expectedPdfs, editor.Pdfs.Count);
        await editor.Save();
        Assert.IsFalse(editor.IsInfoBarErrorOpen, editor.InfoBarErrorMessage);
        var saved = db.Properties.SelectById(property.Id);
        Assert.IsFalse(saved.IsError, saved.Error.FullDump);
        Assert.IsNotNull(saved.Property);
        Assert.AreEqual(expectedPictures, saved.Property.Pictures.Count);
        Assert.AreEqual(expectedPdfs, saved.Property.Pdfs.Count);
        if (operation == "picture") Assert.AreEqual("picture-2", saved.Property.Pictures.Single().Id);
        if (operation == "pdf") Assert.AreEqual("pdf-2", saved.Property.Pdfs.Single().Id);
    }

    [TestMethod]
    public void PropertyEditor_ReceivesMessagesUntilDisposed()
    {
        var property = NewProperty($"messages-{Guid.NewGuid():N}", "Commercial building");
        var editor = new PropertyViewModel(property, false, new NavigationStub(), null!, null!, null!, null!, null!, null!, null!);
        try
        {
            Assert.IsFalse(editor.IsInfoBarErrorOpen);
            Assert.IsTrue(WeakReferenceMessenger.Default.IsRegistered<ListingUpdatedMessage>(editor));
            Assert.IsTrue(WeakReferenceMessenger.Default.IsRegistered<ListingDeletedMessage>(editor));
            Assert.IsTrue(WeakReferenceMessenger.Default.IsRegistered<WindowClosedMessage<Microsoft.UI.Xaml.Window>>(editor));
            Assert.IsTrue(WeakReferenceMessenger.Default.IsRegistered<LessorUpdatedMessage>(editor));
            Assert.IsTrue(WeakReferenceMessenger.Default.IsRegistered<LessorDeletedMessage>(editor));
            Assert.IsTrue(WeakReferenceMessenger.Default.IsRegistered<BrokerUpdatedMessage>(editor));
            Assert.IsTrue(WeakReferenceMessenger.Default.IsRegistered<BrokerDeletedMessage>(editor));

            var listing = NewListing(property, "listing", "Unit A");
            WeakReferenceMessenger.Default.Send(new ListingUpdatedMessage(listing));
            Assert.AreEqual("Unit A", editor.ListingSummaries.Single().Name);
            WeakReferenceMessenger.Default.Send(new ListingDeletedMessage(listing.Id));
            Assert.AreEqual(0, editor.ListingSummaries.Count);
        }
        finally
        {
            editor.Dispose();
        }

        Assert.IsFalse(WeakReferenceMessenger.Default.IsRegistered<ListingUpdatedMessage>(editor));
        Assert.IsFalse(WeakReferenceMessenger.Default.IsRegistered<ListingDeletedMessage>(editor));
        Assert.IsFalse(WeakReferenceMessenger.Default.IsRegistered<WindowClosedMessage<Microsoft.UI.Xaml.Window>>(editor));
        Assert.IsFalse(WeakReferenceMessenger.Default.IsRegistered<LessorUpdatedMessage>(editor));
        Assert.IsFalse(WeakReferenceMessenger.Default.IsRegistered<LessorDeletedMessage>(editor));
        Assert.IsFalse(WeakReferenceMessenger.Default.IsRegistered<BrokerUpdatedMessage>(editor));
        Assert.IsFalse(WeakReferenceMessenger.Default.IsRegistered<BrokerDeletedMessage>(editor));
        WeakReferenceMessenger.Default.Send(new ListingUpdatedMessage(NewListing(property, "other-listing", "Unit B")));
        Assert.AreEqual(0, editor.ListingSummaries.Count);
    }

    [TestMethod]
    public void PropertyAndListingEditors_UpdateContactWrappersFromMessages()
    {
        var propertyId = $"contacts-{Guid.NewGuid():N}";
        var propertyDirectory = Path.Combine(ZumenSearch.App.PropertyBlobDataFolder, propertyId);
        var listingDirectory = Path.Combine(propertyDirectory, "listing");
        var lessor = new PersonNatural($"lessor-{Guid.NewGuid():N}", EntityStatus.Saved) { NameLast = "Lessor" };
        var broker = new PersonNatural($"broker-{Guid.NewGuid():N}", EntityStatus.Saved) { NameLast = "Broker" };
        var property = NewProperty(propertyId, "Commercial building");
        property.Lessors.Add(lessor);
        property.Brokers.Add(broker);
        var listing = NewListing(property, "listing", "Unit A");
        listing.Lessors.Add(lessor);
        listing.Brokers.Add(broker);

        try
        {
            using var propertyEditor = new PropertyViewModel(property, false, new NavigationStub(), null!, null!, null!, null!, null!, null!, null!);
            using var listingEditor = new ListingViewModel(listing, false, new NavigationStub(), null!, null!, null!, null!);
            Assert.IsFalse(propertyEditor.IsInfoBarErrorOpen);
            Assert.AreSame(lessor, propertyEditor.LessorsWrapper.Single().Person);
            Assert.AreSame(broker, propertyEditor.BrokersWrapper.Single().Person);
            Assert.AreSame(lessor, listingEditor.LessorsWrapper.Single().Person);
            Assert.AreSame(broker, listingEditor.BrokersWrapper.Single().Person);
            var propertyLessorWrapper = propertyEditor.LessorsWrapper.Single();
            var listingLessorWrapper = listingEditor.LessorsWrapper.Single();
            var propertyBrokerWrapper = propertyEditor.BrokersWrapper.Single();
            var listingBrokerWrapper = listingEditor.BrokersWrapper.Single();

            var updatedLessor = new PersonNatural(lessor.Id, EntityStatus.Saved) { NameLast = "Updated lessor" };
            var updatedBroker = new PersonNatural(broker.Id, EntityStatus.Saved) { NameLast = "Updated broker" };
            WeakReferenceMessenger.Default.Send(new LessorUpdatedMessage(updatedLessor));
            WeakReferenceMessenger.Default.Send(new BrokerUpdatedMessage(updatedBroker));
            Assert.AreSame(propertyLessorWrapper, propertyEditor.LessorsWrapper.Single());
            Assert.AreSame(listingLessorWrapper, listingEditor.LessorsWrapper.Single());
            Assert.AreSame(updatedLessor, propertyLessorWrapper.Person);
            Assert.AreSame(updatedLessor, listingLessorWrapper.Person);
            Assert.AreSame(propertyBrokerWrapper, propertyEditor.BrokersWrapper.Single());
            Assert.AreSame(listingBrokerWrapper, listingEditor.BrokersWrapper.Single());
            Assert.AreSame(updatedBroker, propertyBrokerWrapper.Person);
            Assert.AreSame(updatedBroker, listingBrokerWrapper.Person);

            var legalLessor = new PersonLegal(lessor.Id, EntityStatus.Saved) { NameCompany = "Lessor company" };
            var legalBroker = new PersonLegal(broker.Id, EntityStatus.Saved) { NameCompany = "Broker company" };
            WeakReferenceMessenger.Default.Send(new LessorUpdatedMessage(legalLessor));
            WeakReferenceMessenger.Default.Send(new BrokerUpdatedMessage(legalBroker));
            Assert.AreNotSame(propertyLessorWrapper, propertyEditor.LessorsWrapper.Single());
            Assert.AreNotSame(listingLessorWrapper, listingEditor.LessorsWrapper.Single());
            Assert.AreNotSame(propertyBrokerWrapper, propertyEditor.BrokersWrapper.Single());
            Assert.AreNotSame(listingBrokerWrapper, listingEditor.BrokersWrapper.Single());
            Assert.AreSame(legalLessor, propertyEditor.LessorsWrapper.Single().Person);
            Assert.AreSame(legalLessor, listingEditor.LessorsWrapper.Single().Person);
            Assert.AreSame(legalBroker, propertyEditor.BrokersWrapper.Single().Person);
            Assert.AreSame(legalBroker, listingEditor.BrokersWrapper.Single().Person);

            WeakReferenceMessenger.Default.Send(new LessorDeletedMessage(lessor.Id));
            WeakReferenceMessenger.Default.Send(new BrokerDeletedMessage(broker.Id));
            Assert.AreEqual(0, propertyEditor.LessorsWrapper.Count);
            Assert.AreEqual(0, listingEditor.LessorsWrapper.Count);
            Assert.AreEqual(0, propertyEditor.BrokersWrapper.Count);
            Assert.AreEqual(0, listingEditor.BrokersWrapper.Count);
        }
        finally
        {
            if (Directory.Exists(listingDirectory)) Directory.Delete(listingDirectory);
            if (Directory.Exists(propertyDirectory)) Directory.Delete(propertyDirectory);
        }
    }

    [TestMethod]
    public async Task PropertyAndListingEditors_ConfirmAndSaveContactRemovals()
    {
        using var db = new TestDatabase();
        var propertyId = $"removals-{Guid.NewGuid():N}";
        var propertyDirectory = Path.Combine(ZumenSearch.App.PropertyBlobDataFolder, propertyId);
        var listingDirectory = Path.Combine(propertyDirectory, "listing");
        var lessor = new PersonNatural($"lessor-{Guid.NewGuid():N}", EntityStatus.New) { NameLast = "Lessor" };
        var broker = new PersonNatural($"broker-{Guid.NewGuid():N}", EntityStatus.New) { NameLast = "Broker" };
        Assert.IsFalse(db.Access.UpsertRentLessor(lessor).IsError);
        Assert.IsFalse(db.Access.UpsertBroker(broker).IsError);
        var property = NewProperty(propertyId, "Commercial building");
        property.Lessors.Add(lessor);
        property.Brokers.Add(broker);
        Assert.IsFalse(db.Properties.Upsert(property).IsError);
        var listing = NewListing(property, "listing", "Unit A");
        listing.Lessors.Add(lessor);
        listing.Brokers.Add(broker);
        Assert.IsFalse(db.Listings.Upsert(propertyId, listing).IsError);
        var loadedProperty = db.Properties.SelectById(propertyId);
        var loadedListing = db.Listings.SelectById(propertyId, listing.Id);
        Assert.IsFalse(loadedProperty.IsError, loadedProperty.Error.FullDump);
        Assert.IsFalse(loadedListing.IsError, loadedListing.Error.FullDump);
        Assert.IsNotNull(loadedProperty.Property);
        Assert.IsNotNull(loadedListing.Listing);
        var dialog = new DeleteDialogStub();

        try
        {
            using var propertyEditor = new PropertyViewModel(loadedProperty.Property, false, new NavigationStub(), dialog, null!, null!, new RentCommercialPropertyService(db.Properties), null!, null!, null!);
            using var listingEditor = new ListingViewModel(loadedListing.Listing, false, new NavigationStub(), dialog, null!, new RentCommercialListingService(db.Listings), null!);
            await propertyEditor.DeleteLessor(propertyEditor.LessorsWrapper.Single());
            await listingEditor.DeleteBrokerCommand.ExecuteAsync(listingEditor.BrokersWrapper.Single());
            Assert.AreEqual(1, propertyEditor.LessorsWrapper.Count);
            Assert.AreEqual(1, listingEditor.BrokersWrapper.Count);
            Assert.IsFalse(propertyEditor.IsDirty);
            Assert.IsFalse(listingEditor.IsDirty);

            dialog.Result = ContentDialogResult.Primary;
            await propertyEditor.DeleteLessor(propertyEditor.LessorsWrapper.Single());
            await propertyEditor.DeleteBroker(propertyEditor.BrokersWrapper.Single());
            await listingEditor.DeleteLessor(listingEditor.LessorsWrapper.Single());
            await listingEditor.DeleteBrokerCommand.ExecuteAsync(listingEditor.BrokersWrapper.Single());
            Assert.IsTrue(propertyEditor.IsDirty);
            Assert.IsTrue(listingEditor.IsDirty);
            Assert.AreEqual(0, propertyEditor.LessorsWrapper.Count);
            Assert.AreEqual(0, propertyEditor.BrokersWrapper.Count);
            Assert.AreEqual(0, listingEditor.LessorsWrapper.Count);
            Assert.AreEqual(0, listingEditor.BrokersWrapper.Count);
            await propertyEditor.Save();
            await listingEditor.Save();
            Assert.IsFalse(propertyEditor.IsInfoBarErrorOpen, propertyEditor.InfoBarErrorMessage);
            Assert.IsFalse(listingEditor.IsInfoBarErrorOpen, listingEditor.InfoBarErrorMessage);
            var savedProperty = db.Properties.SelectById(propertyId);
            var savedListing = db.Listings.SelectById(propertyId, listing.Id);
            Assert.IsFalse(savedProperty.IsError, savedProperty.Error.FullDump);
            Assert.IsFalse(savedListing.IsError, savedListing.Error.FullDump);
            Assert.IsNotNull(savedProperty.Property);
            Assert.IsNotNull(savedListing.Listing);
            Assert.AreEqual(0, savedProperty.Property.Lessors.Count);
            Assert.AreEqual(0, savedProperty.Property.Brokers.Count);
            Assert.AreEqual(0, savedListing.Listing.Lessors.Count);
            Assert.AreEqual(0, savedListing.Listing.Brokers.Count);
            var savedLessor = db.Access.SelectRentLessorById(lessor.Id);
            var savedBroker = db.Access.SelectBrokerById(broker.Id);
            Assert.IsFalse(savedLessor.IsError, savedLessor.Error.FullDump);
            Assert.IsFalse(savedBroker.IsError, savedBroker.Error.FullDump);
            Assert.IsNotNull(savedLessor.Person);
            Assert.IsNotNull(savedBroker.Person);
        }
        finally
        {
            if (Directory.Exists(listingDirectory)) Directory.Delete(listingDirectory);
            if (Directory.Exists(propertyDirectory)) Directory.Delete(propertyDirectory);
        }
    }

    [TestMethod]
    public async Task ListingEditor_AmountsRoundTripBlankZeroAndPositive()
    {
        using var db = new TestDatabase();
        var propertyId = $"amounts-{Guid.NewGuid():N}";
        var propertyDirectory = Path.Combine(ZumenSearch.App.PropertyBlobDataFolder, propertyId);
        var listingDirectory = Path.Combine(propertyDirectory, "listing");
        try
        {
            var property = NewProperty(propertyId, "Commercial building");
            Assert.IsFalse(db.Properties.Upsert(property).IsError);
            var listing = NewListing(property, "listing", "Unit A");
            using var editor = new ListingViewModel(listing, false, new NavigationStub(), null!, null!, new RentCommercialListingService(db.Listings), null!);
            Assert.IsFalse(editor.IsInfoBarErrorOpen, editor.InfoBarErrorMessage);
            AssertAmounts(editor, "");
            editor.Name = "Unit B";
            await editor.Save();
            Assert.IsFalse(editor.IsDirty, editor.InfoBarErrorMessage);
            AssertLoadedAmounts(null, "");

            SetAmounts(editor, "0");
            await editor.Save();
            Assert.IsFalse(editor.IsDirty, editor.InfoBarErrorMessage);
            AssertLoadedAmounts(0m, "0");

            SetAmounts(editor, "12");
            await editor.Save();
            Assert.IsFalse(editor.IsDirty, editor.InfoBarErrorMessage);
            AssertLoadedAmounts(12m, "12");

            SetAmounts(editor, "");
            await editor.Save();
            Assert.IsFalse(editor.IsDirty, editor.InfoBarErrorMessage);
            AssertLoadedAmounts(null, "");

            void AssertLoadedAmounts(decimal? expected, string expectedText)
            {
                var result = db.Listings.SelectById(propertyId, listing.Id);
                Assert.IsFalse(result.IsError, result.Error.FullDump);
                Assert.IsNotNull(result.Listing);
                Assert.AreEqual(expected, result.Listing.Chinryou);
                Assert.AreEqual(expected, result.Listing.ManagementFee);
                Assert.AreEqual(expected, result.Listing.SecurityDeposit);
                Assert.AreEqual(expected, result.Listing.GuaranteeDeposit);
                Assert.AreEqual(expected, result.Listing.KeyMoney);
                Assert.AreEqual(expected, result.Listing.RightsMoney);
                Assert.AreEqual(expected, result.Listing.DepositDeduction);
                Assert.AreEqual(expected, result.Listing.RenewalFee);
                Assert.AreEqual(expected, result.Listing.RecontractFee);
                Assert.AreEqual(expected, result.Listing.ContractPeriodYears);
                using var reopened = new ListingViewModel(result.Listing, false, new NavigationStub(), null!, null!, new RentCommercialListingService(db.Listings), null!);
                Assert.IsFalse(reopened.IsInfoBarErrorOpen, reopened.InfoBarErrorMessage);
                AssertAmounts(reopened, expectedText);
            }
        }
        finally
        {
            if (Directory.Exists(listingDirectory)) Directory.Delete(listingDirectory);
            if (Directory.Exists(propertyDirectory)) Directory.Delete(propertyDirectory);
        }

        static void SetAmounts(ListingViewModel editor, string value)
        {
            editor.Chinryou = value;
            editor.ManagementFee = value;
            editor.SecurityDeposit = value;
            editor.GuaranteeDeposit = value;
            editor.KeyMoney = value;
            editor.RightsMoney = value;
            editor.DepositDeduction = value;
            editor.RenewalFee = value;
            editor.RecontractFee = value;
            editor.ContractPeriodYears = value;
        }

        static void AssertAmounts(ListingViewModel editor, string expected)
        {
            Assert.AreEqual(expected, editor.Chinryou);
            Assert.AreEqual(expected, editor.ManagementFee);
            Assert.AreEqual(expected, editor.SecurityDeposit);
            Assert.AreEqual(expected, editor.GuaranteeDeposit);
            Assert.AreEqual(expected, editor.KeyMoney);
            Assert.AreEqual(expected, editor.RightsMoney);
            Assert.AreEqual(expected, editor.DepositDeduction);
            Assert.AreEqual(expected, editor.RenewalFee);
            Assert.AreEqual(expected, editor.RecontractFee);
            Assert.AreEqual(expected, editor.ContractPeriodYears);
        }
    }

    [TestMethod]
    public async Task ListingEditor_ValidatesAndPersistsDecimalAmounts()
    {
        using var db = new TestDatabase();
        var propertyId = $"validation-{Guid.NewGuid():N}";
        var propertyDirectory = Path.Combine(ZumenSearch.App.PropertyBlobDataFolder, propertyId);
        var listingDirectory = Path.Combine(propertyDirectory, "listing");
        var originalCulture = CultureInfo.CurrentCulture;
        try
        {
            CultureInfo.CurrentCulture = CultureInfo.InvariantCulture;
            var property = NewProperty(propertyId, "Commercial building");
            Assert.IsFalse(db.Properties.Upsert(property).IsError);
            var listing = NewListing(property, "listing", "Unit A");
            using var editor = new ListingViewModel(listing, false, new NavigationStub(), null!, null!, new RentCommercialListingService(db.Listings), null!);
            editor.Chinryou = "-1";
            await editor.Save();
            Assert.IsTrue(editor.IsInfoBarErrorOpen);
            StringAssert.Contains(editor.InfoBarErrorMessage, "賃料");
            Assert.IsTrue(editor.IsDirty);
            Assert.IsNull(db.Listings.SelectById(propertyId, listing.Id).Listing);

            editor.Chinryou = "12.5";
            editor.ManagementFee = "invalid";
            await editor.Save();
            Assert.IsTrue(editor.IsInfoBarErrorOpen);
            StringAssert.Contains(editor.InfoBarErrorMessage, "共益費・管理費");
            Assert.IsTrue(editor.IsDirty);
            Assert.IsNull(db.Listings.SelectById(propertyId, listing.Id).Listing);

            editor.ManagementFee = "2.5";
            await editor.Save();
            StringAssert.Contains(editor.InfoBarErrorMessage, "賃料");
            Assert.IsTrue(editor.IsDirty);
            Assert.IsNull(db.Listings.SelectById(propertyId, listing.Id).Listing);

            editor.Chinryou = "12";
            editor.LessorCommission = "-1";
            await editor.Save();
            StringAssert.Contains(editor.InfoBarErrorMessage, "貸主手数料");
            Assert.IsTrue(editor.IsDirty);
            Assert.IsNull(db.Listings.SelectById(propertyId, listing.Id).Listing);

            editor.Chinryou = "１２";
            editor.LessorCommission = "1.5";
            editor.SecurityDeposit = "1.25";
            editor.GuaranteeDeposit = "2.25";
            editor.KeyMoney = "3.25";
            editor.RightsMoney = "4.25";
            editor.DepositDeduction = "5.25";
            editor.RenewalFee = "6.25";
            editor.RecontractFee = "7.25";
            editor.ContractPeriodYears = "1.5";
            editor.GuarantorCompany = "Guarantor";
            editor.Insurance = "Insurance";
            editor.OtherFees = "Other fees";
            editor.ContractConditions = "Conditions";
            editor.ContractRemarks = "Contract remarks";
            editor.FloorNumber = "2";
            editor.FloorArea = "45.5";
            editor.SelectedFloorGroundType = editor.FloorGroundTypes.First(x => x.Key == ListingFloorGroundType.BelowGround);
            editor.SelectedUseType = editor.UseTypes.First(x => x.Key == ListingUseType.Retail);
            editor.UseDetails = "Retail details";
            editor.SelectedCurrentStatusType = editor.CurrentStatusTypes.First(x => x.Key == ListingAvailabilityStatusType.Vacant);
            editor.SelectedAvailabilityMonthType = editor.AvailabilityMonthTypes.First(x => x.Key == ListingAvailabilityMonthType.June);
            editor.SelectedAvailabilityPeriodType = editor.AvailabilityPeriodTypes.First(x => x.Key == ListingAvailabilityPeriodType.Middle);
            editor.IsAvailableForRentNow = true;
            editor.IsAvailableForImmediateMoveIn = true;
            editor.FudousanIdAdditionalCode = "a123";
            editor.Remarks = "Basic remarks";
            editor.HasAirConditioning = true;
            editor.SelectedUnitCondition = editor.UnitConditionOptions.First(x => x.Key == ListingUnitConditionType.FittedOut);
            editor.SelectedFixtureTransfer = editor.FixtureTransferOptions.First(x => x.Key == ListingFixtureTransferType.Negotiable);
            editor.PermitsRetail = true;
            editor.FacilitiesRemarks = "Facilities";
            editor.SelectedSecurityDepositUnit = editor.FeeUnitTypes.First(x => x.Key == ListingFeeUnitType.Yen);
            editor.SelectedGuaranteeDepositUnit = editor.FeeUnitTypes.First(x => x.Key == ListingFeeUnitType.Yen);
            editor.SelectedKeyMoneyUnit = editor.FeeUnitTypes.First(x => x.Key == ListingFeeUnitType.Yen);
            editor.SelectedRightsMoneyUnit = editor.FeeUnitTypes.First(x => x.Key == ListingFeeUnitType.Yen);
            editor.SelectedDepositDeductionUnit = editor.FeeUnitTypes.First(x => x.Key == ListingFeeUnitType.Yen);
            editor.SelectedRenewalFeeUnit = editor.FeeUnitTypes.First(x => x.Key == ListingFeeUnitType.Yen);
            editor.SelectedRecontractFeeUnit = editor.FeeUnitTypes.First(x => x.Key == ListingFeeUnitType.Months);
            editor.SelectedRentTax = editor.ContractTaxTypes.First(x => x.Key == ListingContractTaxType.Excluded);
            editor.SelectedManagementFeeTax = editor.ContractTaxTypes.First(x => x.Key == ListingContractTaxType.Included);
            editor.SelectedContractType = editor.ContractTypes.First(x => x.Key == ListingContractType.FixedTerm);
            editor.SelectedHandlingType = editor.HandlingTypes.First(x => x.Key == ListingHandlingType.OwnCompanyManagement);
            editor.SelectedTransactionType = editor.TransactionTypes.First(x => x.Key == ListingTransactionType.Agency);
            editor.LesseeCommission = "2.5";
            editor.ListingBrokerCommission = "3.5";
            editor.TenantBrokerCommission = "4.5";
            editor.SelectedLessorCommissionUnit = editor.CommissionUnitTypes.First(x => x.Key == ListingCommissionUnitType.Yen);
            editor.SelectedLesseeCommissionUnit = editor.CommissionUnitTypes.First(x => x.Key == ListingCommissionUnitType.Percent);
            editor.SelectedListingBrokerCommissionUnit = editor.CommissionUnitTypes.First(x => x.Key == ListingCommissionUnitType.Yen);
            editor.SelectedTenantBrokerCommissionUnit = editor.CommissionUnitTypes.First(x => x.Key == ListingCommissionUnitType.Percent);
            editor.TransactionRemarks = "Transaction";
            await editor.Save();
            var result = db.Listings.SelectById(propertyId, listing.Id);
            Assert.IsFalse(result.IsError, result.Error.FullDump);
            Assert.IsNotNull(result.Listing);
            Assert.AreEqual(12m, result.Listing.Chinryou);
            Assert.AreEqual(2.5m, result.Listing.ManagementFee);
            Assert.AreEqual(1.25m, result.Listing.SecurityDeposit);
            Assert.AreEqual(2.25m, result.Listing.GuaranteeDeposit);
            Assert.AreEqual(3.25m, result.Listing.KeyMoney);
            Assert.AreEqual(4.25m, result.Listing.RightsMoney);
            Assert.AreEqual(5.25m, result.Listing.DepositDeduction);
            Assert.AreEqual(6.25m, result.Listing.RenewalFee);
            Assert.AreEqual(7.25m, result.Listing.RecontractFee);
            Assert.AreEqual(1.5m, result.Listing.ContractPeriodYears);
            Assert.AreEqual("1.5", result.Listing.LessorCommission);
            Assert.AreEqual(2, result.Listing.FloorNumber);
            Assert.AreEqual(45.5m, result.Listing.FloorArea);
            Assert.AreEqual(ListingUseType.Retail, result.Listing.Use);
            Assert.IsTrue(result.Listing.HasAirConditioning);
            Assert.AreEqual(ListingUnitConditionType.FittedOut, result.Listing.UnitCondition);
            Assert.AreEqual(ListingFixtureTransferType.Negotiable, result.Listing.FixtureTransfer);
            Assert.IsTrue(result.Listing.PermitsRetail);
            Assert.AreEqual("Facilities", result.Listing.FacilitiesRemarks);
            Assert.AreEqual(ListingFeeUnitType.Yen, result.Listing.SecurityDepositUnit);
            Assert.AreEqual(ListingContractTaxType.Excluded, result.Listing.RentTax);
            Assert.AreEqual(ListingHandlingType.OwnCompanyManagement, result.Listing.HandlingType);
            Assert.AreEqual("Transaction", result.Listing.TransactionRemarks);

            using var reopened = new ListingViewModel(result.Listing, false, new NavigationStub(), null!, null!, new RentCommercialListingService(db.Listings), null!);
            Assert.IsFalse(reopened.IsDirty);
            Assert.AreEqual("2", reopened.FloorNumber);
            Assert.AreEqual("45.5", reopened.FloorArea);
            Assert.AreEqual(ListingFloorGroundType.BelowGround, reopened.SelectedFloorGroundType.Key);
            Assert.AreEqual(ListingUseType.Retail, reopened.SelectedUseType.Key);
            Assert.AreEqual("Retail details", reopened.UseDetails);
            Assert.AreEqual(ListingAvailabilityStatusType.Vacant, reopened.SelectedCurrentStatusType.Key);
            Assert.AreEqual(ListingAvailabilityMonthType.June, reopened.SelectedAvailabilityMonthType.Key);
            Assert.AreEqual(ListingAvailabilityPeriodType.Middle, reopened.SelectedAvailabilityPeriodType.Key);
            Assert.IsTrue(reopened.IsAvailableForRentNow);
            Assert.IsTrue(reopened.IsAvailableForImmediateMoveIn);
            Assert.AreEqual("A123", reopened.FudousanIdAdditionalCode);
            Assert.AreEqual("Basic remarks", reopened.Remarks);
            Assert.IsTrue(reopened.HasAirConditioning);
            Assert.AreEqual(ListingUnitConditionType.FittedOut, reopened.SelectedUnitCondition.Key);
            Assert.AreEqual(ListingFixtureTransferType.Negotiable, reopened.SelectedFixtureTransfer.Key);
            Assert.IsTrue(reopened.PermitsRetail);
            Assert.AreEqual("Facilities", reopened.FacilitiesRemarks);
            Assert.AreEqual("12", reopened.Chinryou);
            Assert.AreEqual("2.5", reopened.ManagementFee);
            Assert.AreEqual("1.25", reopened.SecurityDeposit);
            Assert.AreEqual("2.25", reopened.GuaranteeDeposit);
            Assert.AreEqual("3.25", reopened.KeyMoney);
            Assert.AreEqual("4.25", reopened.RightsMoney);
            Assert.AreEqual("5.25", reopened.DepositDeduction);
            Assert.AreEqual("6.25", reopened.RenewalFee);
            Assert.AreEqual("7.25", reopened.RecontractFee);
            Assert.AreEqual("1.5", reopened.ContractPeriodYears);
            Assert.AreEqual("Guarantor", reopened.GuarantorCompany);
            Assert.AreEqual("Insurance", reopened.Insurance);
            Assert.AreEqual("Other fees", reopened.OtherFees);
            Assert.AreEqual("Conditions", reopened.ContractConditions);
            Assert.AreEqual("Contract remarks", reopened.ContractRemarks);
            Assert.AreEqual(ListingFeeUnitType.Yen, reopened.SelectedSecurityDepositUnit.Key);
            Assert.AreEqual(ListingFeeUnitType.Yen, reopened.SelectedGuaranteeDepositUnit.Key);
            Assert.AreEqual(ListingFeeUnitType.Yen, reopened.SelectedKeyMoneyUnit.Key);
            Assert.AreEqual(ListingFeeUnitType.Yen, reopened.SelectedRightsMoneyUnit.Key);
            Assert.AreEqual(ListingFeeUnitType.Yen, reopened.SelectedDepositDeductionUnit.Key);
            Assert.AreEqual(ListingFeeUnitType.Yen, reopened.SelectedRenewalFeeUnit.Key);
            Assert.AreEqual(ListingFeeUnitType.Months, reopened.SelectedRecontractFeeUnit.Key);
            Assert.AreEqual(ListingContractTaxType.Excluded, reopened.SelectedRentTax.Key);
            Assert.AreEqual(ListingContractTaxType.Included, reopened.SelectedManagementFeeTax.Key);
            Assert.AreEqual(ListingContractType.FixedTerm, reopened.SelectedContractType.Key);
            Assert.AreEqual(ListingHandlingType.OwnCompanyManagement, reopened.SelectedHandlingType.Key);
            Assert.AreEqual(ListingTransactionType.Agency, reopened.SelectedTransactionType.Key);
            Assert.AreEqual("1.5", reopened.LessorCommission);
            Assert.AreEqual("2.5", reopened.LesseeCommission);
            Assert.AreEqual("3.5", reopened.ListingBrokerCommission);
            Assert.AreEqual("4.5", reopened.TenantBrokerCommission);
            Assert.AreEqual(ListingCommissionUnitType.Yen, reopened.SelectedLessorCommissionUnit.Key);
            Assert.AreEqual(ListingCommissionUnitType.Percent, reopened.SelectedLesseeCommissionUnit.Key);
            Assert.AreEqual(ListingCommissionUnitType.Yen, reopened.SelectedListingBrokerCommissionUnit.Key);
            Assert.AreEqual(ListingCommissionUnitType.Percent, reopened.SelectedTenantBrokerCommissionUnit.Key);
            Assert.AreEqual("Transaction", reopened.TransactionRemarks);
        }
        finally
        {
            CultureInfo.CurrentCulture = originalCulture;
            if (Directory.Exists(listingDirectory)) Directory.Delete(listingDirectory);
            if (Directory.Exists(propertyDirectory)) Directory.Delete(propertyDirectory);
        }
    }

    private sealed class LocationStub : ILocationRepository
    {
        public List<ZumenSearch.Domain.ValueObjects.Location.CountyAndCity> GetCountyAndCityByPref(string pref) => [new("city-id", "", "City")];
        public List<ZumenSearch.Domain.ValueObjects.Location.WardAndOaza> GetWardAndOazaByPrefCountyCity(string pref, string county, string city) => [new("town-id", "Ward", "Town")];
        public List<ZumenSearch.Domain.ValueObjects.Location.Choume> GetChoumeByPrefCountyCityWardOaza(string pref, string county, string city, string ward, string oaza) => [new("chou-id", "1")];
    }

    private sealed class NavigationStub : INavigationGenericService
    {
        public void Initialize(Frame frame, List<(string Tag, string Label, Type? Page)> pages) { }
        public Frame? GetFrame() => null;
        public bool NavigateTo(object? selectedPage, object? param) => true;
        public bool IsCurrentPageSameAs(object? selectedPage) => true;
    }

    private sealed class DeleteDialogStub : IDialogGenericService
    {
        public ContentDialogResult Result { get; set; } = ContentDialogResult.None;
        public Task<ContentDialogResult>? Confirmation { get; set; }
        public Task<ZumenSearch.Domain.ValueObjects.Transportation.RailLine?>? RailLine { get; set; }
        public Task<ZumenSearch.Domain.ValueObjects.Transportation.RailStation?>? RailStation { get; set; }
        public string? RequestedLineCode { get; private set; }
        public void Initialize(Microsoft.UI.Xaml.XamlRoot xamlRoot, Microsoft.UI.Xaml.Window window) => throw new NotSupportedException();
        public Task<ContentDialogResult> ShowEditorCloseConfirmationDialog() => throw new NotSupportedException();
        public Task<ContentDialogResult> ShowDeleteConfirmationDialog(string message) => Confirmation ?? Task.FromResult(Result);
        public Task<PersonSearchResultItem?> ShowLessorSelectDialog(ZumenSearch.Presentation.Dialogs.ViewModels.LessorSelectViewModel viewModel) => throw new NotSupportedException();
        public Task<PersonSearchResultItem?> ShowBrokerSelectDialog(ZumenSearch.Presentation.Dialogs.ViewModels.BrokerSelectViewModel viewModel) => throw new NotSupportedException();
        public Task<ZumenSearch.Domain.ValueObjects.Transportation.RailLine?> ShowRailLineSelectDialog() => RailLine ?? throw new NotSupportedException();
        public Task<ZumenSearch.Domain.ValueObjects.Transportation.RailStation?> ShowRailStationSelectDialog(string railLineCode)
        {
            RequestedLineCode = railLineCode;
            return RailStation ?? throw new NotSupportedException();
        }
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

    private sealed class TestPropertyMediaFiles : IDisposable
    {
        private readonly string _directory;
        private readonly string[] _filenames = ["picture-1.jpg", "picture-2.jpg", "document-1.pdf", "document-2.pdf", "thumbnail-1.png", "thumbnail-2.png"];

        public TestPropertyMediaFiles(string propertyId)
        {
            _directory = Path.Combine(ZumenSearch.App.PropertyBlobDataFolder, propertyId);
            if (Directory.Exists(_directory)) throw new IOException($"Test directory already exists: {_directory}");
            Directory.CreateDirectory(_directory);
            foreach (var filename in _filenames) File.WriteAllText(Path.Combine(_directory, filename), "test media");
        }

        public void Dispose()
        {
            foreach (var filename in _filenames) File.Delete(Path.Combine(_directory, filename));
            Directory.Delete(_directory);
        }
    }

    private sealed class TestDatabase : IDisposable
    {
        private readonly string _path = Path.Combine(Path.GetTempPath(), $"ZumenSearch.Tests-{Guid.NewGuid():N}.db");
        public DataAccessService Access { get; } = new();
        public IRentCommercialPropertyRepository Properties => Access;
        public IRentCommercialListingRepository Listings => Access;

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
