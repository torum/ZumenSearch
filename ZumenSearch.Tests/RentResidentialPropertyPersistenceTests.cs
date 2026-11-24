using Microsoft.Data.Sqlite;
using ZumenSearch.Domain.Entities.Person;
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

    [TestMethod]
    public void FailedSave_RollsBackMediaAndPreservesDirtyStateForRetry()
    {
        var databasePath = Path.Combine(Path.GetTempPath(), $"ZumenSearch.Tests-{Guid.NewGuid():N}.db");

        try
        {
            using var dataAccess = new DataAccessService();
            Assert.IsFalse(dataAccess.InitializeDatabase(databasePath).IsError, "Database initialization should succeed.");

            var property = new Property("residential-media-rollback", EntityStatus.New);
            property.SetName("Test Residential Property");
            var picture = new PropertyPicture("residential-picture-rollback", "property.jpg", EntityStatus.New) { Description = "Exterior" };
            var pdf = new PropertyPdf("residential-pdf-rollback", "property.pdf", "property.png", EntityStatus.New) { Description = "Plan" };
            var missingLessor = new PersonNatural("missing-residential-lessor", EntityStatus.New);
            property.Pictures.Add(picture);
            property.Pdfs.Add(pdf);
            property.Lessors.Add(missingLessor);

            var failedSave = dataAccess.UpsertRentResidential(property);

            Assert.IsTrue(failedSave.IsError, "The missing lessor should cause a foreign-key failure after media writes.");
            Assert.AreEqual(EntityStatus.New, picture.Status);
            Assert.IsTrue(picture.IsModified);
            Assert.AreEqual(EntityStatus.New, pdf.Status);
            Assert.IsTrue(pdf.IsModified);
            Assert.AreEqual(0L, CountRows(databasePath, "SELECT COUNT(*) FROM rent_residential_pictures WHERE picture_id = 'residential-picture-rollback'"));
            Assert.AreEqual(0L, CountRows(databasePath, "SELECT COUNT(*) FROM rent_residential_pdfs WHERE pdf_id = 'residential-pdf-rollback'"));

            property.Lessors.Remove(missingLessor);
            var retrySave = dataAccess.UpsertRentResidential(property);

            Assert.IsFalse(retrySave.IsError, retrySave.Error.FullDump);
            Assert.AreEqual(EntityStatus.Saved, picture.Status);
            Assert.IsFalse(picture.IsModified);
            Assert.AreEqual(EntityStatus.Saved, pdf.Status);
            Assert.IsFalse(pdf.IsModified);
            Assert.AreEqual(1L, CountRows(databasePath, "SELECT COUNT(*) FROM rent_residential_pictures WHERE picture_id = 'residential-picture-rollback'"));
            Assert.AreEqual(1L, CountRows(databasePath, "SELECT COUNT(*) FROM rent_residential_pdfs WHERE pdf_id = 'residential-pdf-rollback'"));
        }
        finally
        {
            SqliteConnection.ClearAllPools();
            File.Delete(databasePath);
        }
    }

    private static long CountRows(string databasePath, string sql)
    {
        using var connection = new SqliteConnection($"Data Source={databasePath}");
        connection.Open();
        using var command = connection.CreateCommand();
        command.CommandText = sql;
        return (long)command.ExecuteScalar()!;
    }
}
