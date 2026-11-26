using Microsoft.Data.Sqlite;
using Microsoft.UI.Xaml.Controls;
using ZumenSearch;
using ZumenSearch.Application.Contracts;
using ZumenSearch.Application.Results;
using ZumenSearch.Application.Services.Rent.Commercials;
using ZumenSearch.Application.Services.Rent.Residentials;
using ZumenSearch.Domain.Entities.Base;
using ZumenSearch.Domain.Enums;
using ZumenSearch.Domain.ValueObjects.Location;
using ZumenSearch.Infrastructure.DataAccess;
using ZumenSearch.Presentation.Services.Contracts;
using Commercial = ZumenSearch.Domain.Entities.Rent.Commercials;
using Residential = ZumenSearch.Domain.Entities.Rent.Residentials;
using CommercialEditor = ZumenSearch.Presentation.Features.Rent.Commercials.ViewModels.PropertyViewModel;
using ResidentialEditor = ZumenSearch.Presentation.Features.Rent.Residentials.ViewModels.PropertyViewModel;

namespace ZumenSearch.Tests;

[TestClass]
public sealed class PropertyLocationMasterTests
{
    public static IEnumerable<(bool Residential, string Scenario, int Stage)> LocationCases
    {
        get
        {
            foreach (var residential in new[] { true, false })
            {
                yield return (residential, "matched", 4);
                yield return (residential, "id-mismatch", 4);
                for (var stage = 0; stage <= 3; stage++) yield return (residential, "mismatch", stage);
                for (var stage = 1; stage <= 3; stage++)
                {
                    yield return (residential, "empty", stage);
                    yield return (residential, "sqlite", stage);
                    yield return (residential, "io", stage);
                }
            }
        }
    }

    [TestMethod]
    [DynamicData(nameof(LocationCases))]
    public async Task StoredAddress_UsesMasterSelectionsAndSavesOnlyDisplayedComponents(bool residential, string scenario, int stage)
    {
        var id = $"location-master-{Guid.NewGuid():N}";
        var directory = Path.Combine(App.PropertyBlobDataFolder, id);
        var property = NewProperty(residential, id);
        var address = property.Address;
        address.SetPrefecture(scenario == "mismatch" && stage == 0 ? new Prefecture("99", "999999", "Unknown") : PrefectureMaster.Prefectures[0]);
        address.SetCountyAndCity("old-city", "County", scenario == "mismatch" && stage == 1 ? "Unknown" : "City");
        address.SetWardAndOaza("old-town", "Ward", scenario == "mismatch" && stage == 2 ? "Unknown" : "Town");
        address.SetChoume("old-chou", scenario == "mismatch" && stage == 3 ? "Unknown" : "1");
        address.SetMachiazaId(scenario == "id-mismatch" ? "stale-id" : "chou-id");
        address.SetEdaban("10");
        address.SetPostalCode("1234567");
        address.SetLocationLatitude("35.5");
        address.SetLocationLongitude("139.5");
        property.SetBusStop1("Bus stop after location");
        var location = new LocationStub(scenario, stage);
        var repository = new RepositoryStub();
        try
        {
            using var editor = CreateEditor(property, location, repository);
            var depth = scenario == "matched" || scenario == "id-mismatch" ? 4 : stage;
            AssertSelections(editor, depth);
            Assert.AreEqual(scenario != "matched", editor.IsLocationWarningOpen);
            if (scenario == "mismatch" || scenario == "id-mismatch") StringAssert.Contains(editor.LocationWarning, "住所マスタと一致しません");
            if (scenario == "empty") StringAssert.Contains(editor.LocationWarning, "選択できるデータがありません");
            if (scenario == "sqlite" || scenario == "io") StringAssert.Contains(editor.LocationWarning, "住所マスタを読み込めません");
            Assert.AreEqual("Original building", editor.Name);
            Assert.AreEqual("10", editor.Edaban);
            Assert.AreEqual("123-4567", editor.PostalCode);
            Assert.AreEqual("35.5", editor.Latitude);
            Assert.AreEqual("139.5", editor.Longitude);
            Assert.AreEqual("Bus stop after location", editor.BusStop1);
            Assert.IsFalse(editor.IsDirty);

            editor.Name = "Edited building";
            await editor.Save();
            Assert.AreEqual(1, repository.SaveCalls);
            Assert.IsFalse(editor.IsDirty);
            Assert.AreEqual("Edited building", property.Name);
            Assert.AreEqual("Bus stop after location", property.BusStop1);
            AssertAddress(address, depth);
            Assert.AreEqual("10", address.Edaban);
            Assert.AreEqual("123-4567", address.PostalCode);
            Assert.AreEqual("35.5", address.LocationLatitude);
            Assert.AreEqual("139.5", address.LocationLongitude);

            using var reopened = CreateEditor(property, new LocationStub("matched", 4), repository);
            AssertSelections(reopened, depth);
            Assert.IsFalse(reopened.IsLocationWarningOpen, reopened.LocationWarning);
            Assert.AreEqual("Edited building", reopened.Name);
            Assert.AreEqual("Bus stop after location", reopened.BusStop1);
        }
        finally
        {
            if (Directory.Exists(directory)) Directory.Delete(directory);
        }
    }

    [TestMethod]
    [DataRow(true, 1)]
    [DataRow(true, 2)]
    [DataRow(true, 3)]
    [DataRow(false, 1)]
    [DataRow(false, 2)]
    [DataRow(false, 3)]
    public async Task ChangingParent_ClearsDescendantsAndSavesCurrentMasterId(bool residential, int parent)
    {
        var id = $"location-parent-{Guid.NewGuid():N}";
        var directory = Path.Combine(App.PropertyBlobDataFolder, id);
        var property = NewProperty(residential, id);
        property.Address.SetPrefecture(PrefectureMaster.Prefectures[0]);
        property.Address.SetCountyAndCity("old-city", "County", "City");
        property.Address.SetWardAndOaza("old-town", "Ward", "Town");
        property.Address.SetChoume("old-chou", "1");
        property.Address.SetMachiazaId("chou-id");
        var repository = new RepositoryStub();
        var location = new LocationStub("matched", 4);
        try
        {
            using var editor = CreateEditor(property, location, repository);
            AssertSelections(editor, 4);
            Assert.IsFalse(editor.IsLocationWarningOpen);
            if (parent == 1) editor.SelectedPrefecture = PrefectureMaster.Prefectures[1];
            if (parent == 2) editor.SelectedCity = editor.Cities!.Single(c => c.City == "Other city");
            if (parent == 3) editor.SelectedTown = editor.Towns!.Single(t => t.Oaza == "Other town");
            Assert.IsNull(editor.SelectedChou);
            if (parent <= 2) Assert.IsNull(editor.SelectedTown);
            if (parent == 1) Assert.IsNull(editor.SelectedCity);
            var expectedId = string.Empty;
            Assert.AreEqual(expectedId, editor.MachiazaId ?? string.Empty);
            await editor.Save();
            Assert.AreEqual(1, repository.SaveCalls);
            Assert.AreEqual(expectedId, property.Address.MachiazaId);
            Assert.IsNull(property.Address.Choume);
            if (parent <= 2) Assert.IsNull(property.Address.WardAndOaza);
            if (parent == 1) Assert.IsNull(property.Address.CountyAndCity);
            using var reopened = CreateEditor(property, location, repository);
            Assert.IsFalse(reopened.IsLocationWarningOpen, reopened.LocationWarning);
            Assert.AreEqual(editor.SelectedPrefecture?.MunicipalityCode, reopened.SelectedPrefecture?.MunicipalityCode);
            Assert.AreEqual(editor.SelectedCity?.City, reopened.SelectedCity?.City);
            Assert.AreEqual(editor.SelectedTown?.Oaza, reopened.SelectedTown?.Oaza);
            Assert.IsNull(reopened.SelectedChou);
            Assert.AreEqual(expectedId, reopened.MachiazaId ?? string.Empty);
        }
        finally
        {
            if (Directory.Exists(directory)) Directory.Delete(directory);
        }
    }

    [TestMethod]
    [DataRow(true)]
    [DataRow(false)]
    public async Task SelectionLookupFailure_IsReportedAndOtherFieldsCanBeSaved(bool residential)
    {
        var id = $"location-selection-failure-{Guid.NewGuid():N}";
        var directory = Path.Combine(App.PropertyBlobDataFolder, id);
        var property = NewProperty(residential, id);
        var repository = new RepositoryStub();
        try
        {
            using var editor = CreateEditor(property, new LocationStub("io", 1), repository);
            editor.SelectedPrefecture = PrefectureMaster.Prefectures[0];
            Assert.IsTrue(editor.IsLocationWarningOpen);
            StringAssert.Contains(editor.LocationWarning, "住所マスタを読み込めません");
            Assert.IsNotNull(editor.Cities);
            Assert.IsFalse(editor.Cities.Any());
            editor.Name = "Edited building";
            await editor.Save();
            Assert.AreEqual(1, repository.SaveCalls);
            Assert.AreEqual("Edited building", property.Name);
            Assert.IsTrue(editor.IsLocationWarningOpen);
            Assert.AreEqual(string.Empty, property.Address.MachiazaId);
        }
        finally
        {
            if (Directory.Exists(directory)) Directory.Delete(directory);
        }
    }

    [TestMethod]
    [DataRow(true, "blank")]
    [DataRow(false, "blank")]
    [DataRow(true, "duplicates")]
    [DataRow(false, "duplicates")]
    public void StoredAddress_MatchesBlankLevelsAndPreservesMatchingDuplicateId(bool residential, string scenario)
    {
        var id = $"location-master-blank-{Guid.NewGuid():N}";
        var directory = Path.Combine(App.PropertyBlobDataFolder, id);
        var property = NewProperty(residential, id);
        property.Address.SetPrefecture(PrefectureMaster.Prefectures[0]);
        property.Address.SetCountyAndCity("chou-id", scenario == "blank" ? "" : "County", scenario == "blank" ? "" : "City");
        property.Address.SetWardAndOaza("chou-id", scenario == "blank" ? "" : "Ward", scenario == "blank" ? "" : "Town");
        property.Address.SetChoume("chou-id", scenario == "blank" ? "" : "1");
        property.Address.SetMachiazaId("chou-id");
        try
        {
            using var editor = CreateEditor(property, new LocationStub(scenario, 4), new RepositoryStub());
            Assert.IsNotNull(editor.SelectedCity);
            Assert.IsNotNull(editor.SelectedTown);
            Assert.IsNotNull(editor.SelectedChou);
            Assert.AreEqual(scenario == "blank" ? "" : "1", editor.SelectedChou.Chou);
            Assert.AreEqual("chou-id", editor.MachiazaId);
            Assert.IsFalse(editor.IsLocationWarningOpen, editor.LocationWarning);
        }
        finally
        {
            if (Directory.Exists(directory)) Directory.Delete(directory);
        }
    }

    [TestMethod]
    [DataRow(true)]
    [DataRow(false)]
    public async Task UnmatchedTown_SaveAndReloadDatabaseStoresDisplayedAddress(bool residential)
    {
        var id = $"location-persistence-{Guid.NewGuid():N}";
        var directory = Path.Combine(App.PropertyBlobDataFolder, id);
        var databasePath = Path.Combine(Path.GetTempPath(), $"ZumenSearch.Tests-{Guid.NewGuid():N}.db");
        try
        {
            using var access = new DataAccessService();
            Assert.IsFalse(access.InitializeDatabase(databasePath).IsError);
            PropertyBase property = residential ? new Residential.Property(id, EntityStatus.New) : new Commercial.Property(id, EntityStatus.New);
            property.SetName("Original building");
            property.Address.SetPrefecture(PrefectureMaster.Prefectures[0]);
            property.Address.SetCountyAndCity("chou-id", "County", "City");
            property.Address.SetWardAndOaza("chou-id", "Ward", "Unknown");
            property.Address.SetChoume("chou-id", "1");
            property.Address.SetMachiazaId("chou-id");
            property.Address.SetEdaban("10");
            var initialSave = residential ? access.UpsertRentResidential((Residential.Property)property) : access.UpsertRentCommercial((Commercial.Property)property);
            Assert.IsFalse(initialSave.IsError, initialSave.Error.FullDump);
            PropertyBase? loaded = residential ? access.SelectRentResidentialById(id).Property : access.SelectRentCommercialById(id).Property;
            Assert.IsNotNull(loaded);
            var location = new LocationStub("matched", 4);
            using var editor = CreateEditor(loaded, location, access, access);
            Assert.IsTrue(editor.IsLocationWarningOpen);
            Assert.IsNull(editor.SelectedTown);
            editor.Name = "Edited building";
            await editor.Save();
            Assert.IsFalse(editor.IsDirty);

            PropertyBase? saved = residential ? access.SelectRentResidentialById(id).Property : access.SelectRentCommercialById(id).Property;
            Assert.IsNotNull(saved);
            Assert.AreEqual("Edited building", saved.Name);
            Assert.AreEqual("City", saved.Address.CountyAndCity?.City);
            Assert.AreEqual(string.Empty, saved.Address.WardAndOaza?.Ward);
            Assert.AreEqual(string.Empty, saved.Address.WardAndOaza?.Oaza);
            Assert.AreEqual(string.Empty, saved.Address.Choume?.Chou);
            Assert.AreEqual(string.Empty, saved.Address.MachiazaId);
            Assert.AreEqual($"{PrefectureMaster.Prefectures[0].Name}CountyCity-10", saved.Address.AddressFull);
            using var reopened = CreateEditor(saved, location, access, access);
            Assert.IsFalse(reopened.IsLocationWarningOpen, reopened.LocationWarning);
            Assert.IsNull(reopened.SelectedTown);
            Assert.IsNull(reopened.SelectedChou);
        }
        finally
        {
            SqliteConnection.ClearAllPools();
            File.Delete(databasePath);
            if (Directory.Exists(directory)) Directory.Delete(directory);
        }
    }

    private static PropertyBase NewProperty(bool residential, string id)
    {
        PropertyBase property = residential ? new Residential.Property(id, EntityStatus.Saved) : new Commercial.Property(id, EntityStatus.Saved);
        property.SetName("Original building");
        return property;
    }

    private static Editor CreateEditor(PropertyBase property, LocationStub location, RepositoryStub repository) => CreateEditor(property, location, repository, repository);

    private static Editor CreateEditor(PropertyBase property, LocationStub location, IRentResidentialPropertyRepository residentialRepository, IRentCommercialPropertyRepository commercialRepository)
    {
        if (property is Residential.Property residential)
        {
            return new ResidentialAdapter(new ResidentialEditor(residential, false, new NavigationStub(), null!, null!, null!, new RentResidentialPropertyService(residentialRepository), null!, null!, location));
        }
        return new CommercialAdapter(new CommercialEditor((Commercial.Property)property, false, new NavigationStub(), null!, null!, null!, new RentCommercialPropertyService(commercialRepository), null!, null!, location));
    }

    private static void AssertSelections(Editor editor, int depth)
    {
        Assert.AreEqual(depth >= 1 ? PrefectureMaster.Prefectures[0].MunicipalityCode : null, editor.SelectedPrefecture?.MunicipalityCode);
        Assert.AreEqual(depth >= 2 ? "City" : null, editor.SelectedCity?.City);
        Assert.AreEqual(depth >= 3 ? "Town" : null, editor.SelectedTown?.Oaza);
        Assert.AreEqual(depth >= 4 ? "1" : null, editor.SelectedChou?.Chou);
        Assert.AreEqual(depth >= 4 ? "chou-id" : string.Empty, editor.MachiazaId ?? string.Empty);
    }

    private static void AssertAddress(Address address, int depth)
    {
        Assert.AreEqual(depth >= 1 ? PrefectureMaster.Prefectures[0].MunicipalityCode : null, address.Prefecture?.MunicipalityCode);
        Assert.AreEqual(depth >= 2 ? "County" : null, address.CountyAndCity?.County);
        Assert.AreEqual(depth >= 2 ? "City" : null, address.CountyAndCity?.City);
        Assert.AreEqual(depth >= 3 ? "Ward" : null, address.WardAndOaza?.Ward);
        Assert.AreEqual(depth >= 3 ? "Town" : null, address.WardAndOaza?.Oaza);
        Assert.AreEqual(depth >= 4 ? "1" : null, address.Choume?.Chou);
        Assert.AreEqual(depth >= 4 ? "chou-id" : string.Empty, address.MachiazaId);
    }

    private sealed class LocationStub(string scenario, int stage) : ILocationRepository
    {
        private List<T> Get<T>(int currentStage, List<T> values)
        {
            if (stage != currentStage) return values;
            return scenario switch
            {
                "empty" => [],
                "sqlite" => throw new SqliteException("Lookup failed", 1),
                "io" => throw new IOException("Lookup failed"),
                _ => values
            };
        }

        public List<CountyAndCity> GetCountyAndCityByPref(string pref) => scenario == "blank" ? [new("chou-id", "", "")] : Get<CountyAndCity>(1, [new("city-id", "County", "City"), new("other-city-id", "County", "Other city")]);
        public List<WardAndOaza> GetWardAndOazaByPrefCountyCity(string pref, string county, string city) => scenario == "blank" ? [new("chou-id", "", "")] : Get<WardAndOaza>(2, [new("town-id", "Ward", "Town"), new("other-town-id", "Ward", "Other town")]);
        public List<Choume> GetChoumeByPrefCountyCityWardOaza(string pref, string county, string city, string ward, string oaza) => scenario == "blank" ? [new("chou-id", "")] : scenario == "duplicates" ? [new("other-chou-id", "1"), new("chou-id", "1")] : Get<Choume>(3, [new("chou-id", "1"), new("other-chou-id", "2")]);
    }

    private sealed class RepositoryStub : IRentResidentialPropertyRepository, IRentCommercialPropertyRepository
    {
        public int SaveCalls { get; private set; }
        public ResultWrapper Upsert(Residential.Property property) { SaveCalls++; return new ResultWrapper(); }
        public ResultWrapper Upsert(Commercial.Property property) { SaveCalls++; return new ResultWrapper(); }
        RentResidentialPropertyResultWrapper IRentResidentialPropertyRepository.SelectById(string propertyId, bool duplicate) => throw new NotSupportedException();
        RentCommercialPropertyResultWrapper IRentCommercialPropertyRepository.SelectById(string propertyId, bool duplicate) => throw new NotSupportedException();
        public ResultWrapper Delete(string propertyId) => throw new NotSupportedException();
    }

    private sealed class NavigationStub : INavigationGenericService
    {
        public void Initialize(Frame frame, List<(string Tag, string Label, Type? Page)> pages) { }
        public Frame? GetFrame() => null;
        public bool NavigateTo(object? selectedPage, object? param) => true;
        public bool IsCurrentPageSameAs(object? selectedPage) => true;
    }

    private abstract class Editor : IDisposable
    {
        public abstract string Name { get; set; }
        public abstract string Edaban { get; }
        public abstract string PostalCode { get; }
        public abstract string Latitude { get; }
        public abstract string Longitude { get; }
        public abstract string BusStop1 { get; }
        public abstract bool IsDirty { get; }
        public abstract string LocationWarning { get; }
        public abstract bool IsLocationWarningOpen { get; }
        public abstract string? MachiazaId { get; }
        public abstract Prefecture? SelectedPrefecture { get; set; }
        public abstract CountyAndCity? SelectedCity { get; set; }
        public abstract WardAndOaza? SelectedTown { get; set; }
        public abstract Choume? SelectedChou { get; }
        public abstract IEnumerable<CountyAndCity>? Cities { get; }
        public abstract IEnumerable<WardAndOaza>? Towns { get; }
        public abstract Task Save();
        public abstract void Dispose();
    }

    private sealed class ResidentialAdapter(ResidentialEditor editor) : Editor
    {
        public override string Name { get => editor.Name; set => editor.Name = value; }
        public override string Edaban => editor.Edaban;
        public override string PostalCode => editor.PostalCode;
        public override string Latitude => editor.LocationLatitude;
        public override string Longitude => editor.LocationLongitude;
        public override string BusStop1 => editor.BusStop1;
        public override bool IsDirty => editor.IsDirty;
        public override string LocationWarning => editor.LocationWarning;
        public override bool IsLocationWarningOpen => editor.IsLocationWarningOpen;
        public override string? MachiazaId => editor.MachiazaId;
        public override Prefecture? SelectedPrefecture { get => editor.SelectedPrefecture; set => editor.SelectedPrefecture = value; }
        public override CountyAndCity? SelectedCity { get => editor.SelectedCity; set => editor.SelectedCity = value; }
        public override WardAndOaza? SelectedTown { get => editor.SelectedTown; set => editor.SelectedTown = value; }
        public override Choume? SelectedChou => editor.SelectedChou;
        public override IEnumerable<CountyAndCity>? Cities => editor.Cities;
        public override IEnumerable<WardAndOaza>? Towns => editor.Towns;
        public override Task Save() => editor.Save();
        public override void Dispose() => editor.Dispose();
    }

    private sealed class CommercialAdapter(CommercialEditor editor) : Editor
    {
        public override string Name { get => editor.Name; set => editor.Name = value; }
        public override string Edaban => editor.Edaban;
        public override string PostalCode => editor.PostalCode;
        public override string Latitude => editor.LocationLatitude;
        public override string Longitude => editor.LocationLongitude;
        public override string BusStop1 => editor.BusStop1;
        public override bool IsDirty => editor.IsDirty;
        public override string LocationWarning => editor.LocationWarning;
        public override bool IsLocationWarningOpen => editor.IsLocationWarningOpen;
        public override string? MachiazaId => editor.MachiazaId;
        public override Prefecture? SelectedPrefecture { get => editor.SelectedPrefecture; set => editor.SelectedPrefecture = value; }
        public override CountyAndCity? SelectedCity { get => editor.SelectedCity; set => editor.SelectedCity = value; }
        public override WardAndOaza? SelectedTown { get => editor.SelectedTown; set => editor.SelectedTown = value; }
        public override Choume? SelectedChou => editor.SelectedChou;
        public override IEnumerable<CountyAndCity>? Cities => editor.Cities;
        public override IEnumerable<WardAndOaza>? Towns => editor.Towns;
        public override Task Save() => editor.Save();
        public override void Dispose() => editor.Dispose();
    }
}
