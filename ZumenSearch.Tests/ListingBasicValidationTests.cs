using Microsoft.UI.Xaml.Controls;
using ZumenSearch;
using ZumenSearch.Domain.Enums;
using ZumenSearch.Presentation.Navigation;
using ZumenSearch.Presentation.Services.Contracts;
using CommercialListing = ZumenSearch.Domain.Entities.Rent.Commercials.Listing.Listing;
using CommercialListingViewModel = ZumenSearch.Presentation.Features.Rent.Commercials.ViewModels.ListingViewModel;
using ResidentialListing = ZumenSearch.Domain.Entities.Rent.Residentials.Listing;
using ResidentialListingViewModel = ZumenSearch.Presentation.Features.Rent.Residentials.ViewModels.ListingViewModel;

namespace ZumenSearch.Tests;

[TestClass]
public sealed class ListingBasicValidationTests
{
    [TestMethod]
    [DataRow(" ", "", "", "部屋名（必須項目）が入力されていません。保存出来ませんでした。", true, false)]
    [DataRow("101", "-1", "", "専有面積は0以上の数値で入力してください。", false, false)]
    [DataRow("101", "", "0", "所在階は1以上の整数で入力してください。", false, false)]
    [DataRow("101", "", "0", "所在階は1以上の整数で入力してください。", false, true)]
    public async Task ResidentialBasicValidation_ShowsSpecificMessageAndNavigates(string name, string area, string floor, string expectedMessage, bool nameHasError, bool alreadyOnBasicPage)
    {
        var propertyId = $"basic-validation-{Guid.NewGuid():N}";
        var propertyDirectory = Path.Combine(App.PropertyBlobDataFolder, propertyId);
        var directory = Path.Combine(propertyDirectory, "listing");
        try
        {
            var listing = new ResidentialListing("listing", EntityStatus.New, propertyId, EntityStatus.Saved, false, "Building", string.Empty);
            var navigation = new NavigationStub(alreadyOnBasicPage);
            using var editor = new ResidentialListingViewModel(listing, false, navigation, null!, null!, null!, null!);
            editor.Name = "101";
            editor.Name = name;
            editor.FloorExclusiveAreaSqm = area;
            editor.FloorNumber = floor;

            await editor.Save();

            Assert.IsTrue(editor.IsInfoBarErrorOpen);
            Assert.AreEqual("入力項目に誤りがあります", editor.InfoBarErrorTitle);
            Assert.AreEqual(expectedMessage, editor.InfoBarErrorMessage);
            Assert.AreEqual(nameHasError, editor.IsNameHasError);
            Assert.IsTrue(editor.IsDirty);
            Assert.AreEqual(EntityStatus.New, listing.Status);
            Assert.AreEqual(alreadyOnBasicPage ? 0 : 1, navigation.NavigateCount);
            if (!alreadyOnBasicPage)
            {
                Assert.AreEqual(NavigationKeys.RentResidentialListingBasic, navigation.SelectedPage);
                Assert.AreSame(editor, navigation.Parameter);
            }
        }
        finally
        {
            if (Directory.Exists(directory)) Directory.Delete(directory);
            if (Directory.Exists(propertyDirectory)) Directory.Delete(propertyDirectory);
        }
    }

    [TestMethod]
    [DataRow(" ", "", "", "区画名（必須項目）が入力されていません。保存出来ませんでした。", true, false)]
    [DataRow("101", "-1", "", "専有面積は0以上の数値で入力してください。", false, false)]
    [DataRow("101", "", "0", "所在階は1以上の整数で入力してください。", false, false)]
    [DataRow("101", "", "0", "所在階は1以上の整数で入力してください。", false, true)]
    public async Task CommercialBasicValidation_ShowsSpecificMessageAndNavigates(string name, string area, string floor, string expectedMessage, bool nameHasError, bool alreadyOnBasicPage)
    {
        var propertyId = $"basic-validation-{Guid.NewGuid():N}";
        var propertyDirectory = Path.Combine(App.PropertyBlobDataFolder, propertyId);
        var directory = Path.Combine(propertyDirectory, "listing");
        try
        {
            var listing = new CommercialListing("listing", EntityStatus.New, propertyId, EntityStatus.Saved, false, "Building", string.Empty);
            var navigation = new NavigationStub(alreadyOnBasicPage);
            using var editor = new CommercialListingViewModel(listing, false, navigation, null!, null!, null!, null!);
            editor.Name = "101";
            editor.Name = name;
            editor.FloorArea = area;
            editor.FloorNumber = floor;

            await editor.Save();

            Assert.IsTrue(editor.IsInfoBarErrorOpen);
            Assert.AreEqual("入力項目に誤りがあります", editor.InfoBarErrorTitle);
            Assert.AreEqual(expectedMessage, editor.InfoBarErrorMessage);
            Assert.AreEqual(nameHasError, editor.IsNameHasError);
            Assert.IsTrue(editor.IsDirty);
            Assert.AreEqual(EntityStatus.New, listing.Status);
            Assert.AreEqual(alreadyOnBasicPage ? 0 : 1, navigation.NavigateCount);
            if (!alreadyOnBasicPage)
            {
                Assert.AreEqual(NavigationKeys.RentCommercialListingBasic, navigation.SelectedPage);
                Assert.AreSame(editor, navigation.Parameter);
            }
        }
        finally
        {
            if (Directory.Exists(directory)) Directory.Delete(directory);
            if (Directory.Exists(propertyDirectory)) Directory.Delete(propertyDirectory);
        }
    }

    private sealed class NavigationStub(bool alreadyOnBasicPage) : INavigationGenericService
    {
        public int NavigateCount { get; private set; }
        public object? SelectedPage { get; private set; }
        public object? Parameter { get; private set; }

        public void Initialize(Frame frame, List<(string Tag, string Label, Type? Page)> pages) { }
        public Frame? GetFrame() => null;
        public bool NavigateTo(object? selectedPage, object? param)
        {
            NavigateCount++;
            SelectedPage = selectedPage;
            Parameter = param;
            return true;
        }
        public bool IsCurrentPageSameAs(object? selectedPage) => alreadyOnBasicPage;
    }
}
