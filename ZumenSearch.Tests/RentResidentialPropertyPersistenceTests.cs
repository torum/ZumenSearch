using Microsoft.Data.Sqlite;
using ZumenSearch.Domain.Entities.Rent.Residentials;
using ZumenSearch.Domain.Enums;
using ZumenSearch.Infrastructure.DataAccess;

namespace ZumenSearch.Tests;

[TestClass]
public sealed class RentResidentialPropertyPersistenceTests
{
    [TestMethod]
    public void SaveAndReload_RoundTripsResidentialProperty()
    {
        var databasePath = Path.Combine(Path.GetTempPath(), $"ZumenSearch.Tests-{Guid.NewGuid():N}.db");

        try
        {
            using (var dataAccess = new DataAccessService())
            {
                var initialization = dataAccess.InitializeDatabase(databasePath);
                Assert.IsFalse(initialization.IsError, "Database initialization should succeed.");

                const string propertyId = "residential-property-test";
                var property = new Property(propertyId, EntityStatus.New);
                property.SetName("Test Residential Property");
                property.SetFudousanId("1234567890123");
                property.SetIsUnitLevelOwnership(true);
                property.HasElevator = true;

                var saveResult = dataAccess.UpsertRentResidential(property);
                Assert.IsFalse(saveResult.IsError, "Saving the residential property should succeed.");
                Assert.AreEqual(1, saveResult.AffectedCount);
                Assert.AreEqual(EntityStatus.Saved, property.Status);
                Assert.IsFalse(property.IsModified);

                var loadResult = dataAccess.SelectRentResidentialById(propertyId);
                Assert.IsFalse(loadResult.IsError, "Loading the saved residential property should succeed.");
                Assert.IsNotNull(loadResult.Property, "The saved residential property should be returned.");
                Assert.AreEqual(EntityStatus.Saved, loadResult.Property.Status);
                Assert.IsFalse(loadResult.Property.IsModified);
                Assert.AreEqual(propertyId, loadResult.Property.Id);
                Assert.AreEqual("Test Residential Property", loadResult.Property.Name);
                Assert.AreEqual("1234567890123", loadResult.Property.FudousanId);
                Assert.IsTrue(loadResult.Property.IsUnitLevelOwnership);
                Assert.IsTrue(loadResult.Property.HasElevator);
            }
        }
        finally
        {
            SqliteConnection.ClearAllPools();
            File.Delete(databasePath);
        }
    }
}