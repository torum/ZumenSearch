using Microsoft.Data.Sqlite;
using ZumenSearch.Domain.Entities.Rent.Residentials;
using ZumenSearch.Domain.Enums;
using ZumenSearch.Infrastructure.DataAccess;

namespace ZumenSearch.Tests;

[TestClass]
public sealed class RentResidentialListingPersistenceTests
{
    [TestMethod]
    public void SaveAndReload_RoundTripsResidentialListing()
    {
        var databasePath = Path.Combine(Path.GetTempPath(), $"ZumenSearch.Tests-{Guid.NewGuid():N}.db");

        try
        {
            using var dataAccess = new DataAccessService();
            var initialization = dataAccess.InitializeDatabase(databasePath);
            Assert.IsFalse(initialization.IsError, "Database initialization should succeed.");

            const string propertyId = "residential-listing-property-test";
            const string listingId = "residential-listing-test";
            var property = new Property(propertyId, EntityStatus.New);
            property.SetName("Test Residential Property");

            var propertySaveResult = dataAccess.UpsertRentResidential(property);
            Assert.IsFalse(propertySaveResult.IsError, "Saving the parent property should succeed.");

            var listing = new Listing(listingId, EntityStatus.New, propertyId, EntityStatus.Saved, false, property.Name, property.FudousanId);
            listing.SetName("203");
            listing.SetFloorPlanTypeLabel(FloorPlanType.TwoLDK);
            listing.SetFloorPlanExtraTypeLabel(FloorPlanExtraType.Maisonette);
            listing.SetFloorGroundTypeLabel(FloorGroundType.AboveGround);
            listing.SetFloorExclusiveAreaSqm("54.32");
            listing.SetFloorNumber("2");
            listing.SetIsCornerRoom(true);
            listing.SetExposureDirectionTypeLabel(ExposureDirectionType.South);
            listing.SetAvailabilityStatusTypeLabel(AvailabilityStatusType.Vacant);
            listing.SetAvailabilityMonthTypeLabel(AvailabilityMonthType.December);
            listing.SetAvailabilityPeriodTypeLabel(AvailabilityPeriodType.Middle);
            listing.SetIsAvailableForRentNow(true);
            listing.SetIsAvailableForImmediateMoveIn(true);
            var statusCheckedAt = new DateTimeOffset(2026, 11, 22, 0, 0, 0, TimeSpan.FromHours(9));
            listing.SetOccupancyStatusCheckedAt(statusCheckedAt);
            listing.SetFudousanIdAdditionalCode("2030");
            listing.SetRemarks("Listing remarks");
            listing.SetChinryou(125000);
            listing.SetManagementFee("5000");
            listing.SetManagementFeeUnitTypeLabel(FeeUnitType.Yen);
            listing.SetSecurityDeposit("1");
            listing.SetSecurityDepositUnitTypeLabel(FeeUnitType.Months);
            listing.SetKeyMoney("2");
            listing.SetKeyMoneyUnitTypeLabel(FeeUnitType.Months);
            listing.SetContractTypeLabel(ContractType.FixedTerm);
            listing.SetContractPeriodYears("2");
            listing.SetHandlingTypeLabel(ListingHandlingType.OwnCompanyBrokerage);
            listing.SetTransactionTypeLabel(ListingTransactionType.ExclusiveAgencyBrokerage);
            listing.SetLessorCommission("50");
            listing.SetLessorCommissionUnitTypeLabel(ListingCommissionUnitType.Percent);
            listing.SetPetFriendly(true);
            listing.SetHasAirConditioner(true);
            listing.SetHasSeparateBathAndToilet(true);
            listing.SetApplianceRemarks("Appliance remarks");

            var saveResult = dataAccess.UpsertRentResidentialListing(propertyId, listing);
            Assert.IsFalse(saveResult.IsError, "Saving the residential listing should succeed.");
            Assert.AreEqual(EntityStatus.Saved, listing.Status);
            Assert.IsFalse(listing.IsModified);

            var loadResult = dataAccess.SelectRentResidentialListingById(propertyId, listingId);
            Assert.IsFalse(loadResult.IsError, "Loading the saved residential listing should succeed.");
            Assert.IsNotNull(loadResult.Listing, "The saved residential listing should be returned.");

            var loadedListing = loadResult.Listing;
            Assert.AreEqual(EntityStatus.Saved, loadedListing.Status);
            Assert.IsFalse(loadedListing.IsModified);
            Assert.AreEqual(listingId, loadedListing.Id);
            Assert.AreEqual(propertyId, loadedListing.PropertyId);
            Assert.AreEqual("Test Residential Property", loadedListing.PropertyName);
            Assert.AreEqual("203", loadedListing.Name);
            Assert.AreEqual(FloorPlanType.TwoLDK, loadedListing.FloorPlan);
            Assert.AreEqual(FloorPlanExtraType.Maisonette, loadedListing.FloorPlanExtra);
            Assert.AreEqual("54.32", loadedListing.FloorExclusiveAreaSqm);
            Assert.IsTrue(loadedListing.IsCornerRoom);
            Assert.AreEqual(AvailabilityStatusType.Vacant, loadedListing.SelectedAvailabilityStatusType);
            Assert.IsTrue(loadedListing.IsAvailableForRentNow);
            Assert.IsTrue(loadedListing.IsAvailableForImmediateMoveIn);
            Assert.AreEqual(statusCheckedAt, loadedListing.OccupancyStatusCheckedAt);
            Assert.AreEqual("2030", loadedListing.FudousanIdAdditionalCode);
            Assert.AreEqual("Listing remarks", loadedListing.Remarks);
            Assert.AreEqual(125000m, loadedListing.Chinryou);
            Assert.AreEqual("5000", loadedListing.ManagementFee);
            Assert.AreEqual(FeeUnitType.Yen, loadedListing.ManagementFeeUnit);
            Assert.AreEqual("1", loadedListing.SecurityDeposit);
            Assert.AreEqual(FeeUnitType.Months, loadedListing.SecurityDepositUnit);
            Assert.AreEqual(ContractType.FixedTerm, loadedListing.ContractType);
            Assert.AreEqual("2", loadedListing.ContractPeriodYears);
            Assert.AreEqual(ListingHandlingType.OwnCompanyBrokerage, loadedListing.HandlingType);
            Assert.AreEqual(ListingTransactionType.ExclusiveAgencyBrokerage, loadedListing.TransactionType);
            Assert.AreEqual("50", loadedListing.LessorCommission);
            Assert.AreEqual(ListingCommissionUnitType.Percent, loadedListing.LessorCommissionUnit);
            Assert.IsTrue(loadedListing.PetFriendly);
            Assert.IsTrue(loadedListing.HasAirConditioner);
            Assert.IsTrue(loadedListing.HasSeparateBathAndToilet);
            Assert.AreEqual("Appliance remarks", loadedListing.ApplianceRemarks);

            using var connection = new SqliteConnection($"Data Source={databasePath}");
            connection.Open();
            using var command = connection.CreateCommand();
            command.CommandText = "SELECT COUNT(*) FROM pragma_table_info('rent_residential_listings') WHERE name IN ('availability_status', 'is_recruiting')";
            Assert.AreEqual(0L, (long)command.ExecuteScalar()!);
            command.CommandText = "SELECT is_available_for_rent FROM rent_residential_listings WHERE listing_id = @listing_id";
            command.Parameters.AddWithValue("@listing_id", listingId);
            Assert.AreEqual(1L, (long)command.ExecuteScalar()!);
            listing.SetIsAvailableForRentNow(false);
            Assert.IsFalse(dataAccess.UpsertRentResidentialListing(propertyId, listing).IsError);
            Assert.AreEqual(0L, (long)command.ExecuteScalar()!);
            var unavailable = dataAccess.SelectRentResidentialListingById(propertyId, listingId);
            Assert.IsFalse(unavailable.IsError);
            Assert.IsNotNull(unavailable.Listing);
            Assert.IsFalse(unavailable.Listing.IsAvailableForRentNow);
            Assert.IsTrue(unavailable.Listing.IsAvailableForImmediateMoveIn);
            command.CommandText = "SELECT current_status FROM rent_residential_listings WHERE listing_id = @listing_id";
            Assert.AreEqual(nameof(AvailabilityStatusType.Vacant), command.ExecuteScalar());

            foreach (var status in Enum.GetValues<AvailabilityStatusType>())
            {
                listing.SetAvailabilityStatusTypeLabel(status);
                Assert.IsFalse(dataAccess.UpsertRentResidentialListing(propertyId, listing).IsError);
                Assert.AreEqual(status.ToString(), command.ExecuteScalar());
                var updated = dataAccess.SelectRentResidentialListingById(propertyId, listingId);
                Assert.IsFalse(updated.IsError);
                Assert.IsNotNull(updated.Listing);
                Assert.AreEqual(status, updated.Listing.SelectedAvailabilityStatusType);
                Assert.AreEqual(statusCheckedAt, updated.Listing.OccupancyStatusCheckedAt);
            }
        }
        finally
        {
            SqliteConnection.ClearAllPools();
            File.Delete(databasePath);
        }
    }
}
