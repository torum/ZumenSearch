using Microsoft.UI.Xaml.Controls;
using ZumenSearch.Application.Contracts;
using ZumenSearch.Application.Results;
using ZumenSearch.Application.SearchResults;
using ZumenSearch.Application.Services.Rent.Commercials;
using ZumenSearch.Application.Services.Rent.Residentials;
using ZumenSearch.Domain.Enums;
using ZumenSearch.Presentation.Navigation;
using ZumenSearch.Presentation.Services.Contracts;
using Commercial = ZumenSearch.Domain.Entities.Rent.Commercials.Listing;
using Residential = ZumenSearch.Domain.Entities.Rent.Residentials;

namespace ZumenSearch.Tests;

[TestClass]
public sealed class ListingAdditionalCodeInputTests
{
    public static IEnumerable<(string Input, string Expected, bool Rejected)> InputCases =>
    [
        ("A-12", "A123", true),
        ("あ１２３", "A123", true),
        ("A 12", "A123", true),
        ("A\n12", "A123", true),
        ("ABCDE", "A123", true),
        ("ＡＢＣ１２", "A123", true),
        ("😀", "A123", true),
        ("a123", "A123", false),
        ("ａｂ１２", "AB12", false),
        (" ab12 ", "AB12", false),
        ("A", "A", false),
        ("AB1", "AB1", false),
        ("", "", false)
    ];

    public static IEnumerable<(string Input, bool OnBasicPage)> SaveCases =>
    [
        ("A", false), ("AB", false), ("AB1", true), ("ａ１", false),
        ("AB12", false), ("ａｂ１２", false), ("", false)
    ];

    [TestMethod]
    [DynamicData(nameof(InputCases))]
    public void ResidentialInput_NormalizesOrRejectsWithFeedbackWithoutChangingAcceptedValue(string input, string expected, bool rejected)
    {
        var propertyId = $"listing-code-{Guid.NewGuid():N}";
        var directory = Path.Combine(App.PropertyBlobDataFolder, propertyId);
        var listing = new Residential.Listing("listing", EntityStatus.Saved, propertyId, EntityStatus.Saved, false, "Building", string.Empty);
        listing.SetName("Original unit");
        listing.SetFudousanIdAdditionalCode("A123");
        try
        {
            using var editor = new ZumenSearch.Presentation.Features.Rent.Residentials.ViewModels.ListingViewModel(listing, false, new NavigationStub(), null!, null!, null!, null!);
            editor.FudousanIdAdditionalCode = input;

            Assert.AreEqual(expected, editor.FudousanIdAdditionalCode);
            Assert.AreEqual(expected != "A123", editor.IsDirty);
            Assert.AreEqual(rejected, editor.IsInfoBarErrorOpen);
            Assert.AreEqual("A123", listing.FudousanIdAdditionalCode);
            Assert.IsFalse(listing.IsModified);
            if (rejected)
            {
                Assert.AreEqual("入力項目に誤りがあります", editor.InfoBarErrorTitle);
                Assert.AreEqual("特定コードは4文字以内の半角英数字で入力してください。英字は大文字に変換されます。", editor.InfoBarErrorMessage);
                Assert.AreEqual(InfoBarSeverity.Warning, editor.InfoBarSeverity);
            }
        }
        finally
        {
            if (Directory.Exists(directory)) Directory.Delete(directory, true);
        }
    }

    [TestMethod]
    [DynamicData(nameof(InputCases))]
    public void CommercialInput_NormalizesOrRejectsWithFeedbackWithoutChangingAcceptedValue(string input, string expected, bool rejected)
    {
        var propertyId = $"listing-code-{Guid.NewGuid():N}";
        var directory = Path.Combine(App.PropertyBlobDataFolder, propertyId);
        var listing = new Commercial.Listing("listing", EntityStatus.Saved, propertyId, EntityStatus.Saved, false, "Building", string.Empty);
        listing.SetName("Original unit");
        listing.SetFudousanIdAdditionalCode("A123");
        try
        {
            using var editor = new ZumenSearch.Presentation.Features.Rent.Commercials.ViewModels.ListingViewModel(listing, false, new NavigationStub(), null!, null!, null!, null!);
            editor.FudousanIdAdditionalCode = input;

            Assert.AreEqual(expected, editor.FudousanIdAdditionalCode);
            Assert.AreEqual(expected != "A123", editor.IsDirty);
            Assert.AreEqual(rejected, editor.IsInfoBarErrorOpen);
            Assert.AreEqual("A123", listing.FudousanIdAdditionalCode);
            Assert.IsFalse(listing.IsModified);
            if (rejected)
            {
                Assert.AreEqual("入力項目に誤りがあります", editor.InfoBarErrorTitle);
                Assert.AreEqual("特定コードは4文字以内の半角英数字で入力してください。英字は大文字に変換されます。", editor.InfoBarErrorMessage);
                Assert.AreEqual(InfoBarSeverity.Warning, editor.InfoBarSeverity);
            }
        }
        finally
        {
            if (Directory.Exists(directory)) Directory.Delete(directory, true);
        }
    }

    [TestMethod]
    [DynamicData(nameof(SaveCases))]
    public async Task ResidentialSave_IncompleteCodeBlocksMutationAndPersistenceUntilCorrected(string input, bool onBasicPage)
    {
        var propertyId = $"listing-code-{Guid.NewGuid():N}";
        var directory = Path.Combine(App.PropertyBlobDataFolder, propertyId);
        var listing = new Residential.Listing("listing", EntityStatus.Saved, propertyId, EntityStatus.Saved, false, "Building", string.Empty);
        listing.SetName("Original unit");
        listing.SetFudousanIdAdditionalCode("A123");
        var navigation = new NavigationStub { OnBasicPage = onBasicPage };
        var repository = new RepositoryStub();
        try
        {
            using var editor = new ZumenSearch.Presentation.Features.Rent.Residentials.ViewModels.ListingViewModel(listing, false, navigation, null!, null!, new RentResidentialListingService(repository), null!);
            editor.Name = "Edited unit";
            editor.FudousanIdAdditionalCode = input;
            await editor.Save();

            if (input.Length is > 0 and < 4)
            {
                Assert.AreEqual(0, repository.SaveCalls);
                Assert.AreEqual("Original unit", listing.Name);
                Assert.AreEqual("A123", listing.FudousanIdAdditionalCode);
                Assert.IsFalse(listing.IsModified);
                Assert.IsTrue(editor.IsDirty);
                Assert.IsTrue(editor.IsInfoBarErrorOpen);
                Assert.AreEqual("特定コードは半角英数字4桁（英字は大文字）で入力してください。", editor.InfoBarErrorMessage);
                Assert.AreEqual(InfoBarSeverity.Warning, editor.InfoBarSeverity);
                Assert.AreEqual(onBasicPage ? 0 : 1, navigation.NavigateCalls);
                if (!onBasicPage)
                {
                    Assert.AreEqual(NavigationKeys.RentResidentialListingBasic, navigation.LastPage);
                    Assert.AreSame(editor, navigation.LastParameter);
                }
                editor.FudousanIdAdditionalCode = "ａｂ１２";
                await editor.Save();
            }
            else
            {
                Assert.AreEqual(0, navigation.NavigateCalls);
            }

            Assert.AreEqual(1, repository.SaveCalls);
            Assert.AreEqual(propertyId, repository.LastPropertyId);
            Assert.AreEqual("Edited unit", listing.Name);
            Assert.AreEqual(input.Length == 0 ? "" : "AB12", listing.FudousanIdAdditionalCode);
            Assert.IsFalse(editor.IsDirty);
            Assert.IsFalse(editor.IsInfoBarErrorOpen);
        }
        finally
        {
            if (Directory.Exists(directory)) Directory.Delete(directory, true);
        }
    }

    [TestMethod]
    [DynamicData(nameof(SaveCases))]
    public async Task CommercialSave_IncompleteCodeBlocksMutationAndPersistenceUntilCorrected(string input, bool onBasicPage)
    {
        var propertyId = $"listing-code-{Guid.NewGuid():N}";
        var directory = Path.Combine(App.PropertyBlobDataFolder, propertyId);
        var listing = new Commercial.Listing("listing", EntityStatus.Saved, propertyId, EntityStatus.Saved, false, "Building", string.Empty);
        listing.SetName("Original unit");
        listing.SetFudousanIdAdditionalCode("A123");
        var navigation = new NavigationStub { OnBasicPage = onBasicPage };
        var repository = new RepositoryStub();
        try
        {
            using var editor = new ZumenSearch.Presentation.Features.Rent.Commercials.ViewModels.ListingViewModel(listing, false, navigation, null!, null!, new RentCommercialListingService(repository), null!);
            editor.Name = "Edited unit";
            editor.FudousanIdAdditionalCode = input;
            await editor.Save();

            if (input.Length is > 0 and < 4)
            {
                Assert.AreEqual(0, repository.SaveCalls);
                Assert.AreEqual("Original unit", listing.Name);
                Assert.AreEqual("A123", listing.FudousanIdAdditionalCode);
                Assert.IsFalse(listing.IsModified);
                Assert.IsTrue(editor.IsDirty);
                Assert.IsTrue(editor.IsInfoBarErrorOpen);
                Assert.AreEqual("特定コードは半角英数字4桁（英字は大文字）で入力してください。", editor.InfoBarErrorMessage);
                Assert.AreEqual(InfoBarSeverity.Warning, editor.InfoBarSeverity);
                Assert.AreEqual(onBasicPage ? 0 : 1, navigation.NavigateCalls);
                if (!onBasicPage)
                {
                    Assert.AreEqual(NavigationKeys.RentCommercialListingBasic, navigation.LastPage);
                    Assert.AreSame(editor, navigation.LastParameter);
                }
                editor.FudousanIdAdditionalCode = "ａｂ１２";
                await editor.Save();
            }
            else
            {
                Assert.AreEqual(0, navigation.NavigateCalls);
            }

            Assert.AreEqual(1, repository.SaveCalls);
            Assert.AreEqual(propertyId, repository.LastPropertyId);
            Assert.AreEqual("Edited unit", listing.Name);
            Assert.AreEqual(input.Length == 0 ? "" : "AB12", listing.FudousanIdAdditionalCode);
            Assert.IsFalse(editor.IsDirty);
            Assert.IsFalse(editor.IsInfoBarErrorOpen);
        }
        finally
        {
            if (Directory.Exists(directory)) Directory.Delete(directory, true);
        }
    }

    private sealed class NavigationStub : INavigationGenericService
    {
        public bool OnBasicPage { get; init; }
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

        public bool IsCurrentPageSameAs(object? selectedPage) => OnBasicPage;
        public void Initialize(Frame frame, List<(string Tag, string Label, Type? Page)> pages) => throw new NotSupportedException();
        public Frame? GetFrame() => null;
    }

    private sealed class RepositoryStub : IRentResidentialListingRepository, IRentCommercialListingRepository
    {
        public int SaveCalls { get; private set; }
        public string? LastPropertyId { get; private set; }

        public ResultWrapper Upsert(string propertyId, Residential.Listing listing)
        {
            SaveCalls++;
            LastPropertyId = propertyId;
            return new ResultWrapper();
        }

        public ResultWrapper Upsert(string propertyId, Commercial.Listing listing)
        {
            SaveCalls++;
            LastPropertyId = propertyId;
            return new ResultWrapper();
        }

        public ListingSearchResultWrapper Search(RentResidentialListingSearchCriteria criteria) => throw new NotSupportedException();
        public ListingSearchResultWrapper Search(RentCommercialListingSearchCriteria criteria) => throw new NotSupportedException();
        RentResidentialListingResultWrapper IRentResidentialListingRepository.SelectById(string propertyId, string listingId, bool duplicate) => throw new NotSupportedException();
        RentCommercialListingResultWrapper IRentCommercialListingRepository.SelectById(string propertyId, string listingId, bool duplicate) => throw new NotSupportedException();
        public ResultWrapper Delete(string listingId) => throw new NotSupportedException();
    }
}
