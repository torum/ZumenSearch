using Microsoft.Data.Sqlite;
using Microsoft.UI.Xaml.Controls;
using ZumenSearch.Domain.Entities.Person;
using ZumenSearch.Domain.Entities.Rent.Residentials;
using ZumenSearch.Domain.Enums;
using ZumenSearch.Infrastructure.DataAccess;
using ZumenSearch.Presentation.Features.Brokers.ViewModels;
using ZumenSearch.Presentation.Services.Contracts;
using CommercialProperty = ZumenSearch.Domain.Entities.Rent.Commercials.Property;
using CommercialListing = ZumenSearch.Domain.Entities.Rent.Commercials.Listing.Listing;

namespace ZumenSearch.Tests;

[TestClass]
public sealed class BrokerPersistenceTests
{
    [TestMethod]
    public async Task BrokerEditor_SavesBothKindsAndPreservesSharedDetails()
    {
        var databasePath = Path.Combine(Path.GetTempPath(), $"ZumenSearch.Tests-{Guid.NewGuid():N}.db");
        try
        {
            using var dataAccess = new DataAccessService();
            Assert.IsFalse(dataAccess.InitializeDatabase(databasePath).IsError);
            using var editor = new BrokerViewModel(new PersonLegal("broker-editor", EntityStatus.New), new NavigationStub(), null!, null!, dataAccess, null!);
            Assert.AreEqual(1, editor.PersonKindIndex);
            editor.NameCompany = "不動産";
            editor.NameCompanyType = "株式会社";
            editor.PhoneNumber = "03-1111-2222";
            editor.ContactName = "担当者";
            editor.PostalCode = "123-4567";
            editor.Address = "東京都千代田区";
            editor.Remarks = "備考";
            Assert.IsTrue(editor.IsDirty);
            await editor.Save();
            Assert.IsFalse(editor.IsDirty, editor.InfoBarErrorMessage);

            var savedLegal = dataAccess.SelectBrokerById("broker-editor");
            Assert.IsFalse(savedLegal.IsError);
            Assert.IsInstanceOfType<PersonLegal>(savedLegal.Person);
            var legal = (PersonLegal)savedLegal.Person;
            Assert.AreEqual("株式会社不動産", legal.Name);
            Assert.AreEqual("担当者", legal.ContactName);
            Assert.AreEqual("03-1111-2222", legal.PhoneNumber);
            Assert.AreEqual("123-4567", legal.PostalCode);
            Assert.AreEqual("東京都千代田区", legal.Address);
            Assert.AreEqual("備考", legal.Remarks);
            Assert.IsFalse(legal.IsModified);

            editor.PersonKindIndex = 0;
            editor.NameLast = "山田";
            editor.NameFirst = "太郎";
            editor.MobilePhoneNumber = "090-1111-2222";
            Assert.AreEqual("山田 太郎", editor.Name);
            await editor.Save();
            Assert.IsFalse(editor.IsDirty, editor.InfoBarErrorMessage);
            var savedNatural = dataAccess.SelectBrokerById("broker-editor");
            Assert.IsFalse(savedNatural.IsError);
            Assert.IsInstanceOfType<PersonNatural>(savedNatural.Person);
            var natural = (PersonNatural)savedNatural.Person;
            Assert.AreEqual("山田 太郎", natural.Name);
            Assert.AreEqual("090-1111-2222", natural.MobilePhoneNumber);
            Assert.AreEqual("03-1111-2222", natural.PhoneNumber);
            Assert.AreEqual("123-4567", natural.PostalCode);
            Assert.AreEqual("東京都千代田区", natural.Address);
            Assert.AreEqual("備考", natural.Remarks);
            using (var connection = new SqliteConnection($"Data Source={databasePath}"))
            {
                connection.Open();
                using var command = connection.CreateCommand();
                command.CommandText = "SELECT contact_name FROM brokers WHERE broker_id = @id";
                command.Parameters.AddWithValue("@id", natural.Id);
                Assert.AreEqual(string.Empty, command.ExecuteScalar());
            }

            using var reopened = new BrokerViewModel(natural, new NavigationStub(), null!, null!, dataAccess, null!);
            Assert.AreEqual(0, reopened.PersonKindIndex);
            Assert.AreEqual("090-1111-2222", reopened.MobilePhoneNumber);
            Assert.IsFalse(reopened.IsDirty);
            reopened.PersonKindIndex = 1;
            reopened.NameCompany = "新会社";
            reopened.ContactName = "新担当";
            await reopened.Save();
            Assert.IsFalse(reopened.IsDirty, reopened.InfoBarErrorMessage);
            var changedBack = dataAccess.SelectBrokerById(natural.Id);
            Assert.IsFalse(changedBack.IsError);
            Assert.IsInstanceOfType<PersonLegal>(changedBack.Person);
            Assert.AreEqual("新担当", ((PersonLegal)changedBack.Person).ContactName);
            Assert.AreEqual("東京都千代田区", changedBack.Person.Address);
            using var checkConnection = new SqliteConnection($"Data Source={databasePath}");
            checkConnection.Open();
            using var checkCommand = checkConnection.CreateCommand();
            checkCommand.CommandText = "SELECT mobile_phone_number FROM brokers WHERE broker_id = @id";
            checkCommand.Parameters.AddWithValue("@id", natural.Id);
            Assert.AreEqual(string.Empty, checkCommand.ExecuteScalar());
        }
        finally
        {
            SqliteConnection.ClearAllPools();
            File.Delete(databasePath);
        }
    }

    [TestMethod]
    public async Task BrokerEditor_RequiresNameForSelectedKind()
    {
        var databasePath = Path.Combine(Path.GetTempPath(), $"ZumenSearch.Tests-{Guid.NewGuid():N}.db");
        try
        {
            using var dataAccess = new DataAccessService();
            Assert.IsFalse(dataAccess.InitializeDatabase(databasePath).IsError);
            using var editor = new BrokerViewModel(new PersonLegal("broker-validation", EntityStatus.New), new NavigationStub(), null!, null!, dataAccess, null!);
            editor.NameCompanyType = "株式会社";
            await editor.Save();
            Assert.IsTrue(editor.IsNameCompanyHasError);
            Assert.IsTrue(editor.IsInfoBarErrorOpen);
            Assert.IsTrue(editor.IsDirty);
            editor.PersonKindIndex = 0;
            editor.NameFirst = "太郎";
            await editor.Save();
            Assert.IsTrue(editor.IsNameLastHasError);
            Assert.IsTrue(editor.IsDirty);
            Assert.IsNull(dataAccess.SelectBrokerById("broker-validation").Person);
        }
        finally
        {
            SqliteConnection.ClearAllPools();
            File.Delete(databasePath);
        }
    }

    [TestMethod]
    public void BrokerAssociations_LoadContactDetails()
    {
        var databasePath = Path.Combine(Path.GetTempPath(), $"ZumenSearch.Tests-{Guid.NewGuid():N}.db");
        try
        {
            using var dataAccess = new DataAccessService();
            Assert.IsFalse(dataAccess.InitializeDatabase(databasePath).IsError);
            var broker = new PersonLegal("broker-associated", EntityStatus.New) { NameCompany = "会社", ContactName = "担当者", PhoneNumber = "03-1111-2222", PostalCode = "123-4567", Address = "東京都千代田区" };
            Assert.IsFalse(dataAccess.UpsertBroker(broker).IsError);

            var residential = new Property("residential-associated", EntityStatus.New);
            residential.SetName("住居");
            residential.Brokers.Add(broker);
            Assert.IsFalse(dataAccess.UpsertRentResidential(residential).IsError);
            var residentialListing = new Listing("residential-listing", EntityStatus.New, residential.Id, EntityStatus.Saved, false, residential.Name, string.Empty);
            residentialListing.SetName("部屋");
            residentialListing.Brokers.Add(broker);
            Assert.IsFalse(dataAccess.UpsertRentResidentialListing(residential.Id, residentialListing).IsError);

            var commercial = new CommercialProperty("commercial-associated", EntityStatus.New);
            commercial.SetName("事業用");
            commercial.Brokers.Add(broker);
            Assert.IsFalse(dataAccess.UpsertRentCommercial(commercial, [], [], null).IsError);
            var commercialListing = new CommercialListing("commercial-listing", EntityStatus.New, commercial.Id, EntityStatus.Saved, false, commercial.Name, string.Empty);
            commercialListing.SetName("事業用部屋");
            commercialListing.Brokers.Add(broker);
            Assert.IsFalse(dataAccess.UpsertRentCommercialListing(commercial.Id, commercialListing).IsError);

            var residentialResult = dataAccess.SelectRentResidentialById(residential.Id);
            Assert.IsFalse(residentialResult.IsError);
            Assert.IsNotNull(residentialResult.Property);
            Assert.AreEqual(1, residentialResult.Property.Brokers.Count);
            Assert.IsInstanceOfType<PersonLegal>(residentialResult.Property.Brokers[0]);
            Assert.AreEqual("担当者", ((PersonLegal)residentialResult.Property.Brokers[0]).ContactName);
            Assert.AreEqual("東京都千代田区", residentialResult.Property.Brokers[0].Address);

            var residentialListingResult = dataAccess.SelectRentResidentialListingById(residential.Id, residentialListing.Id);
            Assert.IsFalse(residentialListingResult.IsError);
            Assert.IsNotNull(residentialListingResult.Listing);
            Assert.AreEqual(1, residentialListingResult.Listing.Brokers.Count);
            Assert.IsInstanceOfType<PersonLegal>(residentialListingResult.Listing.Brokers[0]);
            Assert.AreEqual("担当者", ((PersonLegal)residentialListingResult.Listing.Brokers[0]).ContactName);
            Assert.AreEqual("123-4567", residentialListingResult.Listing.Brokers[0].PostalCode);

            var commercialResult = dataAccess.SelectRentCommercialById(commercial.Id);
            Assert.IsFalse(commercialResult.IsError);
            Assert.IsNotNull(commercialResult.Property);
            Assert.AreEqual(1, commercialResult.Property.Brokers.Count);
            Assert.IsInstanceOfType<PersonLegal>(commercialResult.Property.Brokers[0]);
            Assert.AreEqual("担当者", ((PersonLegal)commercialResult.Property.Brokers[0]).ContactName);
            Assert.AreEqual("03-1111-2222", commercialResult.Property.Brokers[0].PhoneNumber);

            var commercialListingResult = dataAccess.SelectRentCommercialListingById(commercial.Id, commercialListing.Id);
            Assert.IsFalse(commercialListingResult.IsError);
            Assert.IsNotNull(commercialListingResult.Unit);
            Assert.AreEqual(1, commercialListingResult.Unit.Brokers.Count);
            Assert.IsInstanceOfType<PersonLegal>(commercialListingResult.Unit.Brokers[0]);
            Assert.AreEqual("担当者", ((PersonLegal)commercialListingResult.Unit.Brokers[0]).ContactName);
            Assert.AreEqual("東京都千代田区", commercialListingResult.Unit.Brokers[0].Address);
        }
        finally
        {
            SqliteConnection.ClearAllPools();
            File.Delete(databasePath);
        }
    }

    private sealed class NavigationStub : INavigationGenericService
    {
        public void Initialize(Frame frame, List<(string Tag, string Label, Type? Page)> pages) { }
        public Frame? GetFrame() => null;
        public bool NavigateTo(object? selectedPage, object? param) => true;
        public bool IsCurrentPageSameAs(object? selectedPage) => true;
    }
}
