using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using ZumenSearch;
using ZumenSearch.Application.Contracts;
using ZumenSearch.Application.Results;
using ZumenSearch.Application.SearchResults;
using ZumenSearch.Application.Services.Rent.Commercials;
using ZumenSearch.Application.Services.Rent.Residentials;
using ZumenSearch.Domain.Enums;
using ZumenSearch.Domain.ValueObjects.Transportation;
using ZumenSearch.Presentation.Services.Contracts;
using Commercial = ZumenSearch.Domain.Entities.Rent.Commercials.Listing;
using Residential = ZumenSearch.Domain.Entities.Rent.Residentials;

namespace ZumenSearch.Tests;

[TestClass]
public sealed class ListingMediaDeletionTests
{
    [TestMethod]
    public void EditorInfoBars_ReplacePreviousTitleMessageAndSeverity()
    {
        var propertyId = $"infobar-{Guid.NewGuid():N}";
        var propertyDirectory = Path.Combine(App.PropertyBlobDataFolder, propertyId);
        var residentialDirectory = Path.Combine(propertyDirectory, "residential");
        var commercialDirectory = Path.Combine(propertyDirectory, "commercial");
        try
        {
            var residentialProperty = new ZumenSearch.Domain.Entities.Rent.Residentials.Property(propertyId, EntityStatus.New);
            using var residentialPropertyEditor = new ZumenSearch.Presentation.Features.Rent.Residentials.ViewModels.PropertyViewModel(residentialProperty, false, new NavigationStub(), null!, null!, null!, null!, null!, null!, null!);
            var commercialProperty = new ZumenSearch.Domain.Entities.Rent.Commercials.Property(propertyId, EntityStatus.New);
            using var commercialPropertyEditor = new ZumenSearch.Presentation.Features.Rent.Commercials.ViewModels.PropertyViewModel(commercialProperty, false, new NavigationStub(), null!, null!, null!, null!, null!, null!, null!);
            var residentialListing = new Residential.Listing("residential", EntityStatus.New, propertyId, EntityStatus.New, false, "Building", string.Empty);
            using var residentialListingEditor = new ZumenSearch.Presentation.Features.Rent.Residentials.ViewModels.ListingViewModel(residentialListing, false, new NavigationStub(), null!, null!, null!, null!);
            var commercialListing = new Commercial.Listing("commercial", EntityStatus.New, propertyId, EntityStatus.New, false, "Building", string.Empty);
            using var commercialListingEditor = new ZumenSearch.Presentation.Features.Rent.Commercials.ViewModels.ListingViewModel(commercialListing, false, new NavigationStub(), null!, null!, null!, null!);

            AssertInfoBarUpdates(residentialPropertyEditor.ShowSimpleErrorInfoBar, () => (residentialPropertyEditor.InfoBarErrorTitle, residentialPropertyEditor.InfoBarErrorMessage, residentialPropertyEditor.InfoBarSeverity, residentialPropertyEditor.IsInfoBarErrorOpen));
            AssertInfoBarUpdates(commercialPropertyEditor.ShowSimpleErrorInfoBar, () => (commercialPropertyEditor.InfoBarErrorTitle, commercialPropertyEditor.InfoBarErrorMessage, commercialPropertyEditor.InfoBarSeverity, commercialPropertyEditor.IsInfoBarErrorOpen));
            AssertInfoBarUpdates(residentialListingEditor.ShowSimpleErrorInfoBar, () => (residentialListingEditor.InfoBarErrorTitle, residentialListingEditor.InfoBarErrorMessage, residentialListingEditor.InfoBarSeverity, residentialListingEditor.IsInfoBarErrorOpen));
            AssertInfoBarUpdates(commercialListingEditor.ShowSimpleErrorInfoBar, () => (commercialListingEditor.InfoBarErrorTitle, commercialListingEditor.InfoBarErrorMessage, commercialListingEditor.InfoBarSeverity, commercialListingEditor.IsInfoBarErrorOpen));
        }
        finally
        {
            if (Directory.Exists(residentialDirectory)) Directory.Delete(residentialDirectory);
            if (Directory.Exists(commercialDirectory)) Directory.Delete(commercialDirectory);
            if (Directory.Exists(propertyDirectory)) Directory.Delete(propertyDirectory);
        }
    }

    private static void AssertInfoBarUpdates(Action<string, string, InfoBarSeverity> show, Func<(string Title, string Message, InfoBarSeverity Severity, bool IsOpen)> getState)
    {
        show("Previous warning", "Previous message", InfoBarSeverity.Warning);
        show("Picker error", "Picker failed", InfoBarSeverity.Error);
        Assert.AreEqual(("Picker error", "Picker failed", InfoBarSeverity.Error, true), getState());

        show("Paste warning", "Invalid input", InfoBarSeverity.Warning);
        Assert.AreEqual(("Paste warning", "Invalid input", InfoBarSeverity.Warning, true), getState());
    }

    [TestMethod]
    [DataRow(false, 0)]
    [DataRow(true, 0)]
    [DataRow(false, 1)]
    [DataRow(true, 1)]
    [DataRow(false, 2)]
    [DataRow(true, 2)]
    public async Task ResidentialRemoval_OnlyAppliesOnSuccessfulSave(bool bulk, int saveOutcome)
    {
        var propertyId = $"media-{Guid.NewGuid():N}";
        var directory = Path.Combine(App.PropertyBlobDataFolder, propertyId, "listing");
        var listing = new Residential.Listing("listing", EntityStatus.New, propertyId, saveOutcome == 0 ? EntityStatus.New : EntityStatus.Saved, false, "Building", string.Empty);
        listing.SetName("Unit");
        listing.Pictures.Add(new Residential.ListingPicture("picture", "picture.jpg", EntityStatus.New));
        listing.Pdfs.Add(new Residential.ListingPdf("pdf", "document.pdf", "thumbnail.png", EntityStatus.New));
        var repository = new ResidentialRepository { Fail = saveOutcome == 1 };
        try
        {
            using var editor = new ZumenSearch.Presentation.Features.Rent.Residentials.ViewModels.ListingViewModel(listing, false, new NavigationStub(), new DialogStub(), null!, new RentResidentialListingService(repository), null!, true);
            Assert.IsTrue(editor.ListingPictureDeleteAllCommand.CanExecute(null));
            Assert.IsTrue(editor.ListingPdfDeleteAllCommand.CanExecute(null));
            foreach (var filename in Filenames) File.WriteAllText(Path.Combine(directory, filename), "media");
            if (bulk)
            {
                await editor.ListingPictureDeleteAllCommand.ExecuteAsync(null);
                await editor.ListingPdfDeleteAllCommand.ExecuteAsync(null);
            }
            else
            {
                await editor.DeleteListingPictureCommand.ExecuteAsync(editor.Pictures.Single());
                await editor.DeleteListingPdfCommand.ExecuteAsync(editor.Pdfs.Single());
            }
            Assert.AreEqual(1, listing.Pictures.Count);
            Assert.AreEqual(1, listing.Pdfs.Count);
            Assert.AreEqual(0, listing.PicturesToBeDeleted.Count);
            Assert.AreEqual(0, listing.PdfsToBeDeleted.Count);
            if (saveOutcome != 0)
            {
                await editor.Save();
                Assert.AreEqual(1, repository.Calls);
                Assert.AreEqual(1, repository.PictureDeletions);
                Assert.AreEqual(1, repository.PdfDeletions);
                if (saveOutcome == 2)
                {
                    Assert.IsFalse(editor.IsDirty);
                    Assert.AreEqual(0, listing.Pictures.Count);
                    Assert.AreEqual(0, listing.Pdfs.Count);
                    foreach (var filename in Filenames) Assert.IsFalse(File.Exists(Path.Combine(directory, filename)));
                    return;
                }
                Assert.IsTrue(editor.IsDirty);
                Assert.IsTrue(editor.IsInfoBarErrorOpen);
            }
            editor.DiscardChanges();
            Assert.AreEqual(1, listing.Pictures.Count);
            Assert.AreEqual(1, listing.Pdfs.Count);
            Assert.AreEqual(0, listing.PicturesToBeDeleted.Count);
            Assert.AreEqual(0, listing.PdfsToBeDeleted.Count);
            foreach (var filename in Filenames) Assert.IsTrue(File.Exists(Path.Combine(directory, filename)));
        }
        finally
        {
            foreach (var filename in Filenames) File.Delete(Path.Combine(directory, filename));
            if (Directory.Exists(directory)) Directory.Delete(directory);
            var propertyDirectory = Path.GetDirectoryName(directory)!;
            if (Directory.Exists(propertyDirectory)) Directory.Delete(propertyDirectory);
        }
    }

    [TestMethod]
    [DataRow(false, 0)]
    [DataRow(true, 0)]
    [DataRow(false, 1)]
    [DataRow(true, 1)]
    [DataRow(false, 2)]
    [DataRow(true, 2)]
    public async Task CommercialRemoval_OnlyAppliesOnSuccessfulSave(bool bulk, int saveOutcome)
    {
        var propertyId = $"media-{Guid.NewGuid():N}";
        var directory = Path.Combine(App.PropertyBlobDataFolder, propertyId, "listing");
        var listing = new Commercial.Listing("listing", EntityStatus.New, propertyId, saveOutcome == 0 ? EntityStatus.New : EntityStatus.Saved, false, "Building", string.Empty);
        listing.SetName("Unit");
        listing.Pictures.Add(new Commercial.ListingPicture("picture", "picture.jpg", EntityStatus.New));
        listing.Pdfs.Add(new Commercial.ListingPdf("pdf", "document.pdf", "thumbnail.png", EntityStatus.New));
        var repository = new CommercialRepository { Fail = saveOutcome == 1 };
        try
        {
            using var editor = new ZumenSearch.Presentation.Features.Rent.Commercials.ViewModels.ListingViewModel(listing, false, new NavigationStub(), new DialogStub(), null!, new RentCommercialListingService(repository), null!, true);
            Assert.IsTrue(editor.ListingPictureDeleteAllCommand.CanExecute(null));
            Assert.IsTrue(editor.ListingPdfDeleteAllCommand.CanExecute(null));
            foreach (var filename in Filenames) File.WriteAllText(Path.Combine(directory, filename), "media");
            if (bulk)
            {
                await editor.ListingPictureDeleteAllCommand.ExecuteAsync(null);
                await editor.ListingPdfDeleteAllCommand.ExecuteAsync(null);
            }
            else
            {
                await editor.DeleteListingPictureCommand.ExecuteAsync(editor.Pictures.Single());
                await editor.DeleteListingPdfCommand.ExecuteAsync(editor.Pdfs.Single());
            }
            Assert.AreEqual(1, listing.Pictures.Count);
            Assert.AreEqual(1, listing.Pdfs.Count);
            Assert.AreEqual(0, listing.PicturesToBeDeleted.Count);
            Assert.AreEqual(0, listing.PdfsToBeDeleted.Count);
            if (saveOutcome != 0)
            {
                await editor.Save();
                Assert.AreEqual(1, repository.Calls);
                Assert.AreEqual(1, repository.PictureDeletions);
                Assert.AreEqual(1, repository.PdfDeletions);
                if (saveOutcome == 2)
                {
                    Assert.IsFalse(editor.IsDirty);
                    Assert.AreEqual(0, listing.Pictures.Count);
                    Assert.AreEqual(0, listing.Pdfs.Count);
                    foreach (var filename in Filenames) Assert.IsFalse(File.Exists(Path.Combine(directory, filename)));
                    return;
                }
                Assert.IsTrue(editor.IsDirty);
                Assert.IsTrue(editor.IsInfoBarErrorOpen);
            }
            editor.DiscardChanges();
            Assert.AreEqual(1, listing.Pictures.Count);
            Assert.AreEqual(1, listing.Pdfs.Count);
            Assert.AreEqual(0, listing.PicturesToBeDeleted.Count);
            Assert.AreEqual(0, listing.PdfsToBeDeleted.Count);
            foreach (var filename in Filenames) Assert.IsTrue(File.Exists(Path.Combine(directory, filename)));
        }
        finally
        {
            foreach (var filename in Filenames) File.Delete(Path.Combine(directory, filename));
            if (Directory.Exists(directory)) Directory.Delete(directory);
            var propertyDirectory = Path.GetDirectoryName(directory)!;
            if (Directory.Exists(propertyDirectory)) Directory.Delete(propertyDirectory);
        }
    }

    [TestMethod]
    public void ListingFolderOpen_MissingDirectoryShowsFeedback()
    {
        var propertyId = $"missing-folder-{Guid.NewGuid():N}";
        var residentialListing = new Residential.Listing("residential", EntityStatus.New, propertyId, EntityStatus.New, false, "Building", string.Empty);
        using var residentialEditor = new ZumenSearch.Presentation.Features.Rent.Residentials.ViewModels.ListingViewModel(residentialListing, false, new NavigationStub(), new DialogStub(), null!, new RentResidentialListingService(new ResidentialRepository()), null!, true);
        var residentialDirectory = Path.Combine(App.PropertyBlobDataFolder, propertyId, residentialListing.Id);
        if (Directory.Exists(residentialDirectory)) Directory.Delete(residentialDirectory, true);
        residentialEditor.OpenBlobDirectory();
        Assert.IsTrue(residentialEditor.IsInfoBarErrorOpen);
        Assert.AreEqual("フォルダーを開けませんでした", residentialEditor.InfoBarErrorTitle);

        var commercialListing = new Commercial.Listing("commercial", EntityStatus.New, propertyId, EntityStatus.New, false, "Building", string.Empty);
        using var commercialEditor = new ZumenSearch.Presentation.Features.Rent.Commercials.ViewModels.ListingViewModel(commercialListing, false, new NavigationStub(), new DialogStub(), null!, new RentCommercialListingService(new CommercialRepository()), null!, true);
        var commercialDirectory = Path.Combine(App.PropertyBlobDataFolder, propertyId, commercialListing.Id);
        if (Directory.Exists(commercialDirectory)) Directory.Delete(commercialDirectory, true);
        commercialEditor.OpenBlobDirectory();
        Assert.IsTrue(commercialEditor.IsInfoBarErrorOpen);
        Assert.AreEqual("フォルダーを開けませんでした", commercialEditor.InfoBarErrorTitle);

        var propertyDirectory = Path.Combine(App.PropertyBlobDataFolder, propertyId);
        if (Directory.Exists(propertyDirectory)) Directory.Delete(propertyDirectory, true);
    }

    private static readonly string[] Filenames = ["picture.jpg", "document.pdf", "thumbnail.png"];

    private sealed class ResidentialRepository : IRentResidentialListingRepository
    {
        public bool Fail { get; set; }
        public int Calls { get; private set; }
        public int PictureDeletions { get; private set; }
        public int PdfDeletions { get; private set; }
        public ResultWrapper Upsert(string propertyId, Residential.Listing listing)
        {
            Calls++;
            PictureDeletions = listing.PicturesToBeDeleted.Count;
            PdfDeletions = listing.PdfsToBeDeleted.Count;
            return new ResultWrapper { IsError = Fail };
        }
        public ListingSearchResultWrapper Search(RentResidentialListingSearchCriteria criteria) => throw new NotSupportedException();
        public RentResidentialListingResultWrapper SelectById(string propertyId, string listingId, bool duplicate = false) => throw new NotSupportedException();
        public ResultWrapper Delete(string listingId) => throw new NotSupportedException();
    }

    private sealed class CommercialRepository : IRentCommercialListingRepository
    {
        public bool Fail { get; set; }
        public int Calls { get; private set; }
        public int PictureDeletions { get; private set; }
        public int PdfDeletions { get; private set; }
        public ResultWrapper Upsert(string propertyId, Commercial.Listing listing)
        {
            Calls++;
            PictureDeletions = listing.PicturesToBeDeleted.Count;
            PdfDeletions = listing.PdfsToBeDeleted.Count;
            return new ResultWrapper { IsError = Fail };
        }
        public ListingSearchResultWrapper Search(RentCommercialListingSearchCriteria criteria) => throw new NotSupportedException();
        public RentCommercialListingResultWrapper SelectById(string propertyId, string listingId, bool duplicate = false) => throw new NotSupportedException();
        public ResultWrapper Delete(string listingId) => throw new NotSupportedException();
    }

    private sealed class NavigationStub : INavigationGenericService
    {
        public void Initialize(Frame frame, List<(string Tag, string Label, Type? Page)> pages) { }
        public Frame? GetFrame() => null;
        public bool NavigateTo(object? selectedPage, object? param) => true;
        public bool IsCurrentPageSameAs(object? selectedPage) => true;
    }

    private sealed class DialogStub : IDialogGenericService
    {
        public void Initialize(XamlRoot xamlRoot, Window window) => throw new NotSupportedException();
        public Task<ContentDialogResult> ShowEditorCloseConfirmationDialog() => throw new NotSupportedException();
        public Task<ContentDialogResult> ShowDeleteConfirmationDialog(string message) => Task.FromResult(ContentDialogResult.Primary);
        public Task<PersonSearchResultItem?> ShowLessorSelectDialog(ZumenSearch.Presentation.Dialogs.ViewModels.LessorSelectViewModel viewModel) => throw new NotSupportedException();
        public Task<PersonSearchResultItem?> ShowBrokerSelectDialog(ZumenSearch.Presentation.Dialogs.ViewModels.BrokerSelectViewModel viewModel) => throw new NotSupportedException();
        public Task<RailLine?> ShowRailLineSelectDialog() => throw new NotSupportedException();
        public Task<RailStation?> ShowRailStationSelectDialog(string railLineCode) => throw new NotSupportedException();
    }
}
