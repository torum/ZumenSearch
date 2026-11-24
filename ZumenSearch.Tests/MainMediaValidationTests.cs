using Microsoft.UI.Xaml.Controls;
using ZumenSearch;
using ZumenSearch.Application.Contracts;
using ZumenSearch.Application.Results;
using ZumenSearch.Application.SearchResults;
using ZumenSearch.Application.Services.Rent.Commercials;
using ZumenSearch.Application.Services.Rent.Residentials;
using ZumenSearch.Domain.Enums;
using ZumenSearch.Presentation.Navigation;
using ZumenSearch.Presentation.Services.Contracts;
using Commercial = ZumenSearch.Domain.Entities.Rent.Commercials;
using Residential = ZumenSearch.Domain.Entities.Rent.Residentials;
using CommercialModels = ZumenSearch.Presentation.Features.Rent.Commercials.ViewModels;
using ResidentialModels = ZumenSearch.Presentation.Features.Rent.Residentials.ViewModels;

namespace ZumenSearch.Tests;

[TestClass]
public sealed class MainMediaValidationTests
{
    [TestMethod]
    [DataRow(false, false)]
    [DataRow(false, true)]
    [DataRow(true, false)]
    [DataRow(true, true)]
    public async Task ResidentialProperty_ValidatesMediaBeforeSave(bool existing, bool hiddenDocuments)
    {
        var id = $"main-media-{Guid.NewGuid():N}";
        var directory = Path.Combine(App.PropertyBlobDataFolder, id);
        var property = new Residential.Property(id, EntityStatus.Saved);
        property.SetName("Original");
        property.SetIsUnitLevelOwnership(hiddenDocuments);
        var pictures = new[] { new Residential.PropertyPicture("p1", "1.jpg", EntityStatus.Saved) { IsMain = true }, new Residential.PropertyPicture("p2", "2.jpg", EntityStatus.Saved) { IsMain = true } };
        var pdfs = new[] { new Residential.PropertyPdf("d1", "1.pdf", "1.png", EntityStatus.Saved) { IsMain = true }, new Residential.PropertyPdf("d2", "2.pdf", "2.png", EntityStatus.Saved) { IsMain = true } };
        if (existing)
        {
            property.Pictures.AddRange(pictures);
            property.Pdfs.AddRange(pdfs);
        }
        var repository = new ResidentialPropertyRepository();
        var navigation = new NavigationStub();
        try
        {
            using var editor = new ResidentialModels.PropertyViewModel(property, false, navigation, null!, null!, null!, new RentResidentialPropertyService(repository), null!, null!, null!);
            if (!existing)
            {
                foreach (var picture in pictures) editor.Pictures.Add(new ResidentialModels.PropertyPictureViewModel(picture, editor));
                foreach (var pdf in pdfs) editor.Pdfs.Add(new ResidentialModels.PropertyPdfViewModel(pdf, editor));
            }
            editor.Name = "Edited";
            await VerifyValidation(editor.Save, () => editor.IsDirty, () => editor.InfoBarErrorMessage, () => repository.Calls, () => property.Name, navigation, NavigationKeys.RentResidentialPropertyPictures, NavigationKeys.RentResidentialPropertyDocuments, hiddenDocuments, () => editor.Pictures[0].IsMain = false, () => editor.Pdfs[0].IsMain = false, () => editor.Pictures[1].IsMain = false, () => editor.Pdfs[1].IsMain = false);
        }
        finally
        {
            if (Directory.Exists(directory)) Directory.Delete(directory);
        }
    }

    [TestMethod]
    [DataRow(false, false)]
    [DataRow(false, true)]
    [DataRow(true, false)]
    [DataRow(true, true)]
    public async Task CommercialProperty_ValidatesMediaBeforeSave(bool existing, bool hiddenDocuments)
    {
        var id = $"main-media-{Guid.NewGuid():N}";
        var directory = Path.Combine(App.PropertyBlobDataFolder, id);
        var property = new Commercial.Property(id, EntityStatus.Saved);
        property.SetName("Original");
        property.SetIsUnitOwnership(hiddenDocuments);
        var pictures = new[] { new Commercial.PropertyPicture("p1", "1.jpg", EntityStatus.Saved) { IsMain = true }, new Commercial.PropertyPicture("p2", "2.jpg", EntityStatus.Saved) { IsMain = true } };
        var pdfs = new[] { new Commercial.PropertyPdf("d1", "1.pdf", "1.png", EntityStatus.Saved) { IsMain = true }, new Commercial.PropertyPdf("d2", "2.pdf", "2.png", EntityStatus.Saved) { IsMain = true } };
        if (existing)
        {
            property.Pictures.AddRange(pictures);
            property.Pdfs.AddRange(pdfs);
        }
        var repository = new CommercialPropertyRepository();
        var navigation = new NavigationStub();
        try
        {
            using var editor = new CommercialModels.PropertyViewModel(property, false, navigation, null!, null!, null!, new RentCommercialPropertyService(repository), null!, null!, null!);
            if (!existing)
            {
                foreach (var picture in pictures) editor.Pictures.Add(new CommercialModels.PropertyPictureViewModel(picture, editor));
                foreach (var pdf in pdfs) editor.Pdfs.Add(new CommercialModels.PropertyPdfViewModel(pdf, editor));
            }
            editor.Name = "Edited";
            await VerifyValidation(editor.Save, () => editor.IsDirty, () => editor.InfoBarErrorMessage, () => repository.Calls, () => property.Name, navigation, NavigationKeys.RentCommercialPropertyPictures, NavigationKeys.RentCommercialPropertyDocuments, hiddenDocuments, () => editor.Pictures[0].IsMain = false, () => editor.Pdfs[0].IsMain = false, () => editor.Pictures[1].IsMain = false, () => editor.Pdfs[1].IsMain = false);
        }
        finally
        {
            if (Directory.Exists(directory)) Directory.Delete(directory);
        }
    }

    [TestMethod]
    [DataRow(false, false)]
    [DataRow(false, true)]
    [DataRow(true, false)]
    [DataRow(true, true)]
    public async Task ResidentialListing_ValidatesMediaBeforeSave(bool existing, bool hiddenDocuments)
    {
        var propertyId = $"main-media-{Guid.NewGuid():N}";
        var propertyDirectory = Path.Combine(App.PropertyBlobDataFolder, propertyId);
        var directory = Path.Combine(propertyDirectory, "listing");
        var listing = new Residential.Listing("listing", EntityStatus.Saved, propertyId, EntityStatus.Saved, !hiddenDocuments, "Building", string.Empty);
        listing.SetName("Original");
        var pictures = new[] { new Residential.ListingPicture("p1", "1.jpg", EntityStatus.Saved) { IsMain = true }, new Residential.ListingPicture("p2", "2.jpg", EntityStatus.Saved) { IsMain = true } };
        var pdfs = new[] { new Residential.ListingPdf("d1", "1.pdf", "1.png", EntityStatus.Saved) { IsMain = true }, new Residential.ListingPdf("d2", "2.pdf", "2.png", EntityStatus.Saved) { IsMain = true } };
        if (existing)
        {
            listing.Pictures.AddRange(pictures);
            listing.Pdfs.AddRange(pdfs);
        }
        var repository = new ResidentialListingRepository();
        var navigation = new NavigationStub();
        try
        {
            using var editor = new ResidentialModels.ListingViewModel(listing, false, navigation, null!, null!, new RentResidentialListingService(repository), null!);
            if (!existing)
            {
                foreach (var picture in pictures) editor.Pictures.Add(new ResidentialModels.ListingPictureViewModel(picture, editor));
                foreach (var pdf in pdfs) editor.Pdfs.Add(new ResidentialModels.ListingPdfViewModel(pdf, editor));
            }
            editor.Name = "Edited";
            await VerifyValidation(editor.Save, () => editor.IsDirty, () => editor.InfoBarErrorMessage, () => repository.Calls, () => listing.Name, navigation, NavigationKeys.RentResidentialListingPictures, NavigationKeys.RentResidentialListingDocuments, hiddenDocuments, () => editor.Pictures[0].IsMain = false, () => editor.Pdfs[0].IsMain = false, () => editor.Pictures[1].IsMain = false, () => editor.Pdfs[1].IsMain = false);
        }
        finally
        {
            if (Directory.Exists(directory)) Directory.Delete(directory);
            if (Directory.Exists(propertyDirectory)) Directory.Delete(propertyDirectory);
        }
    }

    [TestMethod]
    [DataRow(false, false)]
    [DataRow(false, true)]
    [DataRow(true, false)]
    [DataRow(true, true)]
    public async Task CommercialListing_ValidatesMediaBeforeSave(bool existing, bool hiddenDocuments)
    {
        var propertyId = $"main-media-{Guid.NewGuid():N}";
        var propertyDirectory = Path.Combine(App.PropertyBlobDataFolder, propertyId);
        var directory = Path.Combine(propertyDirectory, "listing");
        var listing = new Commercial.Listing.Listing("listing", EntityStatus.Saved, propertyId, EntityStatus.Saved, !hiddenDocuments, "Building", string.Empty);
        listing.SetName("Original");
        var pictures = new[] { new Commercial.Listing.ListingPicture("p1", "1.jpg", EntityStatus.Saved) { IsMain = true }, new Commercial.Listing.ListingPicture("p2", "2.jpg", EntityStatus.Saved) { IsMain = true } };
        var pdfs = new[] { new Commercial.Listing.ListingPdf("d1", "1.pdf", "1.png", EntityStatus.Saved) { IsMain = true }, new Commercial.Listing.ListingPdf("d2", "2.pdf", "2.png", EntityStatus.Saved) { IsMain = true } };
        if (existing)
        {
            listing.Pictures.AddRange(pictures);
            listing.Pdfs.AddRange(pdfs);
        }
        var repository = new CommercialListingRepository();
        var navigation = new NavigationStub();
        try
        {
            using var editor = new CommercialModels.ListingViewModel(listing, false, navigation, null!, null!, new RentCommercialListingService(repository), null!);
            if (!existing)
            {
                foreach (var picture in pictures) editor.Pictures.Add(new CommercialModels.ListingPictureViewModel(picture, editor));
                foreach (var pdf in pdfs) editor.Pdfs.Add(new CommercialModels.ListingPdfViewModel(pdf, editor));
            }
            editor.Name = "Edited";
            await VerifyValidation(editor.Save, () => editor.IsDirty, () => editor.InfoBarErrorMessage, () => repository.Calls, () => listing.Name, navigation, NavigationKeys.RentCommercialListingPictures, NavigationKeys.RentCommercialListingDocuments, hiddenDocuments, () => editor.Pictures[0].IsMain = false, () => editor.Pdfs[0].IsMain = false, () => editor.Pictures[1].IsMain = false, () => editor.Pdfs[1].IsMain = false);
        }
        finally
        {
            if (Directory.Exists(directory)) Directory.Delete(directory);
            if (Directory.Exists(propertyDirectory)) Directory.Delete(propertyDirectory);
        }
    }

    private static async Task VerifyValidation(Func<Task> save, Func<bool> isDirty, Func<string> message, Func<int> calls, Func<string> modelName, NavigationStub navigation, string picturePage, string pdfPage, bool hiddenDocuments, Action fixPicture, Action fixPdf, Action clearPicture, Action clearPdf)
    {
        await save();
        Assert.AreEqual(0, calls());
        Assert.AreEqual("Original", modelName());
        Assert.IsTrue(isDirty());
        Assert.AreEqual("メインの写真は1枚だけ選択してください。", message());
        Assert.AreEqual(picturePage, navigation.Page);

        fixPicture();
        navigation.Page = null;
        await save();
        Assert.AreEqual(0, calls());
        Assert.AreEqual("Original", modelName());
        Assert.IsTrue(isDirty());
        StringAssert.Contains(message(), "メインの図面は1枚だけ選択してください。");
        Assert.AreEqual(hiddenDocuments ? null : pdfPage, navigation.Page);
        if (hiddenDocuments) StringAssert.Contains(message(), "所有形態");

        fixPdf();
        await save();
        Assert.AreEqual(1, calls());
        Assert.AreEqual("Edited", modelName());
        Assert.IsFalse(isDirty());

        clearPicture();
        clearPdf();
        await save();
        Assert.AreEqual(2, calls());
        Assert.IsFalse(isDirty());
    }

    private sealed class NavigationStub : INavigationGenericService
    {
        public string? Page { get; set; }
        public void Initialize(Frame frame, List<(string Tag, string Label, Type? Page)> pages) { }
        public Frame? GetFrame() => null;
        public bool NavigateTo(object? selectedPage, object? param)
        {
            Page = selectedPage as string;
            return true;
        }
        public bool IsCurrentPageSameAs(object? selectedPage) => Equals(Page, selectedPage);
    }

    private sealed class ResidentialPropertyRepository : IRentResidentialPropertyRepository
    {
        public int Calls { get; private set; }
        public ResultWrapper Upsert(Residential.Property property)
        {
            Calls++;
            return new();
        }
        public RentResidentialPropertyResultWrapper SelectById(string propertyId, bool duplicate = false) => throw new NotSupportedException();
        public ResultWrapper Delete(string propertyId) => throw new NotSupportedException();
    }

    private sealed class CommercialPropertyRepository : IRentCommercialPropertyRepository
    {
        public int Calls { get; private set; }
        public ResultWrapper Upsert(Commercial.Property property)
        {
            Calls++;
            return new();
        }
        public RentCommercialPropertyResultWrapper SelectById(string propertyId, bool duplicate = false) => throw new NotSupportedException();
        public ResultWrapper Delete(string propertyId) => throw new NotSupportedException();
    }

    private sealed class ResidentialListingRepository : IRentResidentialListingRepository
    {
        public int Calls { get; private set; }
        public ResultWrapper Upsert(string propertyId, Residential.Listing listing)
        {
            Calls++;
            return new();
        }
        public RentResidentialListingResultWrapper SelectById(string propertyId, string listingId, bool duplicate = false) => throw new NotSupportedException();
        public ListingSearchResultWrapper Search(RentResidentialListingSearchCriteria criteria) => throw new NotSupportedException();
        public ResultWrapper Delete(string listingId) => throw new NotSupportedException();
    }

    private sealed class CommercialListingRepository : IRentCommercialListingRepository
    {
        public int Calls { get; private set; }
        public ResultWrapper Upsert(string propertyId, Commercial.Listing.Listing listing)
        {
            Calls++;
            return new();
        }
        public RentCommercialListingResultWrapper SelectById(string propertyId, string listingId, bool duplicate = false) => throw new NotSupportedException();
        public ListingSearchResultWrapper Search(RentCommercialListingSearchCriteria criteria) => throw new NotSupportedException();
        public ResultWrapper Delete(string listingId) => throw new NotSupportedException();
    }
}
