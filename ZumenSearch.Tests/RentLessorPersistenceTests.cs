using Microsoft.Data.Sqlite;
using ZumenSearch.Domain.Entities.Person;
using ZumenSearch.Domain.Entities.Rent.Residentials;
using ZumenSearch.Domain.Enums;
using ZumenSearch.Infrastructure.DataAccess;
using ZumenSearch.Presentation.Features.Rent.Lessors.ViewModels;

namespace ZumenSearch.Tests;

[TestClass]
public sealed class RentLessorPersistenceTests
{
    [TestMethod]
    public async Task SaveAndReload_PreservesSharedAndCurrentPersonKindDetails()
    {
        var databasePath = Path.Combine(Path.GetTempPath(), $"ZumenSearch.Tests-{Guid.NewGuid():N}.db");

        try
        {
            using var dataAccess = new DataAccessService();
            Assert.IsFalse(dataAccess.InitializeDatabase(databasePath).IsError);

            var lessor = new PersonNatural("lessor-test", EntityStatus.New);
            using var viewModel = new LessorViewModel(lessor, null!, null!, null!, dataAccess, null!);
            viewModel.NameLast = "山田";
            viewModel.NameFirst = "太郎";
            viewModel.PhoneNumber = "03-1234-5678";
            viewModel.MobilePhoneNumber = "090-1234-5678";
            viewModel.PostalCode = "123-4567";
            viewModel.Address = "東京都千代田区";
            Assert.IsTrue(viewModel.IsDirty);
            await viewModel.Save();
            Assert.IsFalse(viewModel.IsDirty);

            var naturalResult = dataAccess.SelectRentLessorById(lessor.Id);
            Assert.IsFalse(naturalResult.IsError);
            Assert.IsInstanceOfType<PersonNatural>(naturalResult.Person);
            var natural = (PersonNatural)naturalResult.Person;
            Assert.AreEqual("03-1234-5678", natural.PhoneNumber);
            Assert.AreEqual("090-1234-5678", natural.MobilePhoneNumber);
            Assert.AreEqual("123-4567", naturalResult.Person.PostalCode);
            Assert.AreEqual("東京都千代田区", naturalResult.Person.Address);
            Assert.IsFalse(naturalResult.Person.IsModified);

            viewModel.PersonKindIndex = 1;
            viewModel.NameCompany = "会社";
            viewModel.ContactName = "担当者";
            await viewModel.Save();
            Assert.IsFalse(viewModel.IsDirty);

            var legalResult = dataAccess.SelectRentLessorById(lessor.Id);
            Assert.IsFalse(legalResult.IsError);
            Assert.IsInstanceOfType<PersonLegal>(legalResult.Person);
            var legal = (PersonLegal)legalResult.Person;
            Assert.AreEqual("03-1234-5678", legal.PhoneNumber);
            Assert.AreEqual("担当者", legal.ContactName);
            Assert.AreEqual("123-4567", legalResult.Person.PostalCode);
            Assert.AreEqual("東京都千代田区", legalResult.Person.Address);

            using (var connection = new SqliteConnection($"Data Source={databasePath}"))
            {
                connection.Open();
                using var command = connection.CreateCommand();
                command.CommandText = "SELECT mobile_phone_number FROM rent_lessors WHERE lessor_id = @id";
                command.Parameters.AddWithValue("@id", lessor.Id);
                Assert.AreEqual(string.Empty, command.ExecuteScalar());
            }

            using var reopened = new LessorViewModel(legalResult.Person, null!, null!, null!, dataAccess, null!);
            Assert.IsFalse(reopened.IsDirty);
            Assert.AreEqual("担当者", reopened.ContactName);
            Assert.AreEqual("東京都千代田区", reopened.Address);
            reopened.Address = "大阪府大阪市";
            Assert.IsTrue(reopened.IsDirty);
            await reopened.Save();
            Assert.IsFalse(reopened.IsDirty);
            var updated = dataAccess.SelectRentLessorById(lessor.Id);
            Assert.IsFalse(updated.IsError);
            Assert.IsInstanceOfType<PersonLegal>(updated.Person);
            Assert.AreEqual("大阪府大阪市", updated.Person.Address);
            Assert.AreEqual("担当者", ((PersonLegal)updated.Person).ContactName);

            reopened.PersonKindIndex = 0;
            reopened.NameLast = "山田";
            await reopened.Save();
            Assert.IsFalse(reopened.IsDirty);
            var switchedBack = dataAccess.SelectRentLessorById(lessor.Id);
            Assert.IsFalse(switchedBack.IsError);
            Assert.IsInstanceOfType<PersonNatural>(switchedBack.Person);
            Assert.AreEqual(string.Empty, ((PersonNatural)switchedBack.Person).MobilePhoneNumber);
            Assert.AreEqual("大阪府大阪市", switchedBack.Person.Address);
            using (var connection = new SqliteConnection($"Data Source={databasePath}"))
            {
                connection.Open();
                using var command = connection.CreateCommand();
                command.CommandText = "SELECT contact_name FROM rent_lessors WHERE lessor_id = @id";
                command.Parameters.AddWithValue("@id", lessor.Id);
                Assert.AreEqual(string.Empty, command.ExecuteScalar());
            }
        }
        finally
        {
            SqliteConnection.ClearAllPools();
            File.Delete(databasePath);
        }
    }

    [TestMethod]
    public void LoadBroker_DoesNotRequireLessorContactColumns()
    {
        var databasePath = Path.Combine(Path.GetTempPath(), $"ZumenSearch.Tests-{Guid.NewGuid():N}.db");

        try
        {
            using var dataAccess = new DataAccessService();
            Assert.IsFalse(dataAccess.InitializeDatabase(databasePath).IsError);
            var natural = new PersonNatural("natural-broker", EntityStatus.New) { NameLast = "山田", NameFirst = "太郎", Remarks = "個人" };
            var legal = new PersonLegal("legal-broker", EntityStatus.New) { NameCompany = "会社", NameCompanyType = "株式会社", Remarks = "法人" };
            Assert.IsFalse(dataAccess.UpsertBroker(natural).IsError);
            Assert.IsFalse(dataAccess.UpsertBroker(legal).IsError);

            var naturalResult = dataAccess.SelectBrokerById(natural.Id);
            Assert.IsFalse(naturalResult.IsError);
            Assert.IsInstanceOfType<PersonNatural>(naturalResult.Person);
            Assert.AreEqual("山田", ((PersonNatural)naturalResult.Person).NameLast);
            Assert.AreEqual("太郎", ((PersonNatural)naturalResult.Person).NameFirst);
            Assert.AreEqual("個人", naturalResult.Person.Remarks);

            var legalResult = dataAccess.SelectBrokerById(legal.Id);
            Assert.IsFalse(legalResult.IsError);
            Assert.IsInstanceOfType<PersonLegal>(legalResult.Person);
            Assert.AreEqual("会社", ((PersonLegal)legalResult.Person).NameCompany);
            Assert.AreEqual("株式会社", ((PersonLegal)legalResult.Person).NameCompanyType);
            Assert.AreEqual("法人", legalResult.Person.Remarks);
        }
        finally
        {
            SqliteConnection.ClearAllPools();
            File.Delete(databasePath);
        }
    }

    [TestMethod]
    public void LoadResidentialProperty_LoadsLessorContactDetails()
    {
        var databasePath = Path.Combine(Path.GetTempPath(), $"ZumenSearch.Tests-{Guid.NewGuid():N}.db");

        try
        {
            using var dataAccess = new DataAccessService();
            Assert.IsFalse(dataAccess.InitializeDatabase(databasePath).IsError);
            var lessor = new PersonNatural("lessor-associated", EntityStatus.New)
            {
                NameLast = "山田",
                PhoneNumber = "03-1234-5678",
                MobilePhoneNumber = "090-1234-5678",
                PostalCode = "123-4567",
                Address = "東京都千代田区"
            };
            Assert.IsFalse(dataAccess.UpsertRentLessor(lessor).IsError);

            var property = new Property("property-associated", EntityStatus.New);
            property.SetName("建物");
            property.Lessors.Add(lessor);
            Assert.IsFalse(dataAccess.UpsertRentResidential(property).IsError);

            var result = dataAccess.SelectRentResidentialById(property.Id);
            Assert.IsFalse(result.IsError);
            Assert.IsNotNull(result.Property);
            Assert.AreEqual(1, result.Property.Lessors.Count);
            Assert.IsInstanceOfType<PersonNatural>(result.Property.Lessors[0]);
            var loaded = (PersonNatural)result.Property.Lessors[0];
            Assert.AreEqual(lessor.PhoneNumber, loaded.PhoneNumber);
            Assert.AreEqual(lessor.MobilePhoneNumber, loaded.MobilePhoneNumber);
            Assert.AreEqual(lessor.PostalCode, loaded.PostalCode);
            Assert.AreEqual(lessor.Address, loaded.Address);
        }
        finally
        {
            SqliteConnection.ClearAllPools();
            File.Delete(databasePath);
        }
    }
}
