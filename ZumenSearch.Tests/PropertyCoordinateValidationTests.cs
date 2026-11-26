using System.Globalization;
using Microsoft.UI.Xaml.Controls;
using ZumenSearch.Application.Contracts;
using ZumenSearch.Application.Results;
using ZumenSearch.Application.Services.Rent.Commercials;
using ZumenSearch.Application.Services.Rent.Residentials;
using ZumenSearch.Domain.Enums;
using ZumenSearch.Domain.ValueObjects.Location;
using ZumenSearch.Presentation.Navigation;
using ZumenSearch.Presentation.Services.Contracts;
using Commercial = ZumenSearch.Domain.Entities.Rent.Commercials;
using Residential = ZumenSearch.Domain.Entities.Rent.Residentials;

namespace ZumenSearch.Tests;

[TestClass]
public sealed class PropertyCoordinateValidationTests
{
    public static (bool Latitude, string Input, string? Expected, bool OnLocationPage)[] CoordinateCases =>
    [
        (true, "-90", "-90", false),
        (true, "90", "90", false),
        (false, "-180", "-180", false),
        (false, "180", "180", false),
        (true, "0", "0", false),
        (false, "0", "0", false),
        (true, " +35.50 ", "35.50", false),
        (false, " -139.75 ", "-139.75", false),
        (true, "", "", false),
        (false, " \t ", "", false),
        (true, "90.0001", null, false),
        (true, "-90.0001", null, true),
        (false, "180.0001", null, false),
        (false, "-180.0001", null, true),
        (true, "abc", null, false),
        (false, "139,5", null, false),
        (true, "1e1", null, false),
        (false, "NaN", null, false),
        (true, "Infinity", null, false),
        (false, "1,000", null, false),
        (true, "３５.５", null, false),
        (false, "79228162514264337593543950336", null, false)
    ];

    [TestMethod]
    [DynamicData(nameof(CoordinateCases))]
    public void AddressValidation_MatchesSetterAcceptanceAndPreservesNormalization(bool latitude, string input, string? expected, bool onLocationPage)
    {
        var previousCulture = CultureInfo.CurrentCulture;
        CultureInfo.CurrentCulture = CultureInfo.GetCultureInfo("de-DE");
        try
        {
            var address = new Address();
            address.SetLocationLatitude("35");
            address.SetLocationLongitude("139");
            address.SetIsModified(false);
            var valid = latitude ? Address.IsValidLatitude(input) : Address.IsValidLongitude(input);
            Assert.AreEqual(expected is not null, valid);
            Assert.IsFalse(address.IsModified);
            Action apply = latitude ? () => address.SetLocationLatitude(input) : () => address.SetLocationLongitude(input);
            if (expected is null)
            {
                var error = Assert.ThrowsExactly<ArgumentException>(apply);
                Assert.AreEqual("value", error.ParamName);
                Assert.IsFalse(address.IsModified);
                Assert.AreEqual("35", address.LocationLatitude);
                Assert.AreEqual("139", address.LocationLongitude);
            }
            else
            {
                apply();
                Assert.AreEqual(expected, latitude ? address.LocationLatitude : address.LocationLongitude);
                Assert.AreEqual(latitude ? "139" : "35", latitude ? address.LocationLongitude : address.LocationLatitude);
                Assert.IsTrue(address.IsModified);
            }
        }
        finally
        {
            CultureInfo.CurrentCulture = previousCulture;
        }
    }

    [TestMethod]
    [DynamicData(nameof(CoordinateCases))]
    public async Task ResidentialCoordinates_ValidateBeforeMutationAndNavigateToLocationPage(bool latitude, string input, string? expected, bool onLocationPage)
    {
        var propertyId = $"coordinate-validation-{Guid.NewGuid():N}";
        var directory = Path.Combine(App.PropertyBlobDataFolder, propertyId);
        var property = new Residential.Property(propertyId, EntityStatus.Saved);
        property.SetName("Original building");
        property.Address.SetEdaban("Original address");
        property.Address.SetLocationLatitude("35");
        property.Address.SetLocationLongitude("139");
        var navigation = new NavigationStub { OnLocationPage = onLocationPage };
        var repository = new RepositoryStub();
        try
        {
            using var editor = new ZumenSearch.Presentation.Features.Rent.Residentials.ViewModels.PropertyViewModel(property, false, navigation, null!, null!, null!, new RentResidentialPropertyService(repository), null!, null!, null!);
            property.Address.SetIsModified(false);
            editor.Name = "Edited building";
            editor.Edaban = "Edited address";
            if (latitude) editor.LocationLatitude = input;
            else editor.LocationLongitude = input;

            await editor.Save();

            if (expected is null)
            {
                Assert.AreEqual(0, repository.SaveCalls);
                Assert.AreEqual("Original building", property.Name);
                Assert.AreEqual("Original address", property.Address.Edaban);
                Assert.AreEqual("35", property.Address.LocationLatitude);
                Assert.AreEqual("139", property.Address.LocationLongitude);
                Assert.IsFalse(property.IsModified);
                Assert.IsFalse(property.Address.IsModified);
                Assert.IsTrue(editor.IsDirty);
                Assert.IsFalse(editor.IsBusy);
                Assert.IsTrue(editor.IsInfoBarErrorOpen);
                Assert.AreEqual("入力項目に誤りがあります", editor.InfoBarErrorTitle);
                Assert.AreEqual(InfoBarSeverity.Warning, editor.InfoBarSeverity);
                Assert.AreEqual(latitude ? "緯度は-90以上90以下の半角数値で入力するか、空欄にしてください。小数点には「.」を使用してください。" : "経度は-180以上180以下の半角数値で入力するか、空欄にしてください。小数点には「.」を使用してください。", editor.InfoBarErrorMessage);
                Assert.AreEqual(onLocationPage ? 0 : 1, navigation.NavigateCalls);
                if (!onLocationPage)
                {
                    Assert.AreEqual(NavigationKeys.RentResidentialPropertyLocation, navigation.LastPage);
                    Assert.AreSame(editor, navigation.LastParameter);
                }
                if (latitude) editor.LocationLatitude = "36.5";
                else editor.LocationLongitude = "140.5";
                await editor.Save();
            }
            else
            {
                Assert.AreEqual(0, navigation.NavigateCalls);
            }

            Assert.AreEqual(1, repository.SaveCalls);
            Assert.AreEqual("Edited building", property.Name);
            Assert.AreEqual("Edited address", property.Address.Edaban);
            Assert.AreEqual(latitude ? expected ?? "36.5" : "35", property.Address.LocationLatitude);
            Assert.AreEqual(latitude ? "139" : expected ?? "140.5", property.Address.LocationLongitude);
            Assert.IsFalse(editor.IsDirty);
            Assert.IsFalse(editor.IsInfoBarErrorOpen);
        }
        finally
        {
            if (Directory.Exists(directory)) Directory.Delete(directory);
        }
    }

    [TestMethod]
    [DynamicData(nameof(CoordinateCases))]
    public async Task CommercialCoordinates_ValidateBeforeMutationAndNavigateToLocationPage(bool latitude, string input, string? expected, bool onLocationPage)
    {
        var propertyId = $"coordinate-validation-{Guid.NewGuid():N}";
        var directory = Path.Combine(App.PropertyBlobDataFolder, propertyId);
        var property = new Commercial.Property(propertyId, EntityStatus.Saved);
        property.SetName("Original building");
        property.Address.SetEdaban("Original address");
        property.Address.SetLocationLatitude("35");
        property.Address.SetLocationLongitude("139");
        var navigation = new NavigationStub { OnLocationPage = onLocationPage };
        var repository = new RepositoryStub();
        try
        {
            using var editor = new ZumenSearch.Presentation.Features.Rent.Commercials.ViewModels.PropertyViewModel(property, false, navigation, null!, null!, null!, new RentCommercialPropertyService(repository), null!, null!, null!);
            property.Address.SetIsModified(false);
            editor.Name = "Edited building";
            editor.Edaban = "Edited address";
            if (latitude) editor.LocationLatitude = input;
            else editor.LocationLongitude = input;

            await editor.Save();

            if (expected is null)
            {
                Assert.AreEqual(0, repository.SaveCalls);
                Assert.AreEqual("Original building", property.Name);
                Assert.AreEqual("Original address", property.Address.Edaban);
                Assert.AreEqual("35", property.Address.LocationLatitude);
                Assert.AreEqual("139", property.Address.LocationLongitude);
                Assert.IsFalse(property.IsModified);
                Assert.IsFalse(property.Address.IsModified);
                Assert.IsTrue(editor.IsDirty);
                Assert.IsFalse(editor.IsBusy);
                Assert.IsTrue(editor.IsInfoBarErrorOpen);
                Assert.AreEqual("入力項目に誤りがあります", editor.InfoBarErrorTitle);
                Assert.AreEqual(InfoBarSeverity.Warning, editor.InfoBarSeverity);
                Assert.AreEqual(latitude ? "緯度は-90以上90以下の半角数値で入力するか、空欄にしてください。小数点には「.」を使用してください。" : "経度は-180以上180以下の半角数値で入力するか、空欄にしてください。小数点には「.」を使用してください。", editor.InfoBarErrorMessage);
                Assert.AreEqual(onLocationPage ? 0 : 1, navigation.NavigateCalls);
                if (!onLocationPage)
                {
                    Assert.AreEqual(NavigationKeys.RentCommercialPropertyLocation, navigation.LastPage);
                    Assert.AreSame(editor, navigation.LastParameter);
                }
                if (latitude) editor.LocationLatitude = "36.5";
                else editor.LocationLongitude = "140.5";
                await editor.Save();
            }
            else
            {
                Assert.AreEqual(0, navigation.NavigateCalls);
            }

            Assert.AreEqual(1, repository.SaveCalls);
            Assert.AreEqual("Edited building", property.Name);
            Assert.AreEqual("Edited address", property.Address.Edaban);
            Assert.AreEqual(latitude ? expected ?? "36.5" : "35", property.Address.LocationLatitude);
            Assert.AreEqual(latitude ? "139" : expected ?? "140.5", property.Address.LocationLongitude);
            Assert.IsFalse(editor.IsDirty);
            Assert.IsFalse(editor.IsInfoBarErrorOpen);
        }
        finally
        {
            if (Directory.Exists(directory)) Directory.Delete(directory);
        }
    }

    private sealed class NavigationStub : INavigationGenericService
    {
        public bool OnLocationPage { get; init; }
        public int NavigateCalls { get; private set; }
        public object? LastPage { get; private set; }
        public object? LastParameter { get; private set; }

        public bool NavigateTo(object? selectedPage, object? param)
        {
            NavigateCalls++;
            LastPage = selectedPage;
            LastParameter = param;
            return true;
        }

        public bool IsCurrentPageSameAs(object? selectedPage) => OnLocationPage;
        public void Initialize(Frame frame, List<(string Tag, string Label, Type? Page)> pages) => throw new NotSupportedException();
        public Frame? GetFrame() => null;
    }

    private sealed class RepositoryStub : IRentResidentialPropertyRepository, IRentCommercialPropertyRepository
    {
        public int SaveCalls { get; private set; }

        public ResultWrapper Upsert(Residential.Property residentialProperty)
        {
            SaveCalls++;
            return new ResultWrapper();
        }

        public ResultWrapper Upsert(Commercial.Property commercialProperty)
        {
            SaveCalls++;
            return new ResultWrapper();
        }

        RentResidentialPropertyResultWrapper IRentResidentialPropertyRepository.SelectById(string propertyId, bool duplicate) => throw new NotSupportedException();
        RentCommercialPropertyResultWrapper IRentCommercialPropertyRepository.SelectById(string propertyId, bool duplicate) => throw new NotSupportedException();
        public ResultWrapper Delete(string propertyId) => throw new NotSupportedException();
    }
}
