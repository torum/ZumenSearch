using Microsoft.UI.Xaml.Controls;
using ZumenSearch;
using ZumenSearch.Application.Contracts;
using ZumenSearch.Application.Results;
using ZumenSearch.Application.SearchResults;
using ZumenSearch.Application.Services.Rent.Commercials;
using ZumenSearch.Application.Services.Rent.Residentials;
using ZumenSearch.Domain.Enums;
using ZumenSearch.Presentation.Messaging;
using ZumenSearch.Presentation.Services.Contracts;
using Commercial = ZumenSearch.Domain.Entities.Rent.Commercials;
using Residential = ZumenSearch.Domain.Entities.Rent.Residentials;

namespace ZumenSearch.Tests;

[TestClass]
public sealed class PropertyListingDuplicationTests
{
    [TestMethod]
    public async Task ResidentialPropertyEditor_DisposeUnsubscribesMediaPropertyHandlers()
    {
        var property = new Residential.Property("dispose-media", EntityStatus.New);
        property.SetName("Building");
        using var editor = new ZumenSearch.Presentation.Features.Rent.Residentials.ViewModels.PropertyViewModel(property, false, new NavigationStub(), null!, null!, null!, new RentResidentialPropertyService(new ResidentialPropertyRepository()), new RentResidentialListingService(new ResidentialListingRepository()), null!, null!);
        var picture = new ZumenSearch.Presentation.Features.Rent.Residentials.ViewModels.PropertyPictureViewModel(new Residential.PropertyPicture("picture", "picture.jpg", EntityStatus.New), editor);
        var pdf = new ZumenSearch.Presentation.Features.Rent.Residentials.ViewModels.PropertyPdfViewModel(new Residential.PropertyPdf("pdf", "document.pdf", "thumbnail.jpg", EntityStatus.New), editor);

        editor.Pictures.Add(picture);
        editor.Pdfs.Add(pdf);
        await editor.Save();
        Assert.IsFalse(editor.IsDirty);

        editor.Dispose();
        picture.Description = "Changed after disposal";
        pdf.Description = "Changed after disposal";
        Assert.IsFalse(editor.IsDirty);

        if (Directory.Exists(Path.Combine(App.PropertyBlobDataFolder, property.Id)))
        {
            Directory.Delete(Path.Combine(App.PropertyBlobDataFolder, property.Id), true);
        }
        editor.OpenPropertyBlobDirectoryCommand.Execute(null);
        Assert.IsTrue(editor.IsInfoBarErrorOpen);
        Assert.AreEqual("フォルダーを開けませんでした", editor.InfoBarErrorTitle);
        Assert.IsTrue(editor.InfoBarErrorMessage.Contains("データフォルダーが存在しません"));
    }

    [TestMethod]
    [DataRow(8, true)]
    [DataRow(97, true)]
    [DataRow(98, false)]
    [DataRow(100, false)]
    public async Task ResidentialPropertyDuplication_PreservesDisplayedNameWhenSaved(int nameLength, bool prefixExpected)
    {
        var propertyId = $"duplicate-name-{Guid.NewGuid():N}";
        var directory = Path.Combine(App.PropertyBlobDataFolder, propertyId);
        var property = new Residential.Property(propertyId, EntityStatus.New);
        var originalName = new string('A', nameLength);
        property.SetName(originalName);
        try
        {
            using var editor = new ZumenSearch.Presentation.Features.Rent.Residentials.ViewModels.PropertyViewModel(property, true, new NavigationStub(), null!, null!, null!, new RentResidentialPropertyService(new ResidentialPropertyRepository()), new RentResidentialListingService(new ResidentialListingRepository()), null!, null!);
            var expectedName = prefixExpected ? $"複製_{originalName}" : originalName;
            Assert.AreEqual(expectedName, editor.Name);
            Assert.AreEqual(!prefixExpected, editor.IsInfoBarErrorOpen);
            await editor.Save();
            Assert.IsFalse(editor.IsDirty);
            Assert.AreEqual(expectedName, property.Name);
        }
        finally
        {
            if (Directory.Exists(directory)) Directory.Delete(directory);
        }
    }

    [TestMethod]
    [DataRow(8, true)]
    [DataRow(97, true)]
    [DataRow(98, false)]
    [DataRow(100, false)]
    public async Task CommercialPropertyDuplication_PreservesDisplayedNameWhenSaved(int nameLength, bool prefixExpected)
    {
        var propertyId = $"duplicate-name-{Guid.NewGuid():N}";
        var directory = Path.Combine(App.PropertyBlobDataFolder, propertyId);
        var property = new Commercial.Property(propertyId, EntityStatus.New);
        var originalName = new string('A', nameLength);
        property.SetName(originalName);
        try
        {
            using var editor = new ZumenSearch.Presentation.Features.Rent.Commercials.ViewModels.PropertyViewModel(property, true, new NavigationStub(), null!, null!, null!, new RentCommercialPropertyService(new CommercialPropertyRepository()), new RentCommercialListingService(new CommercialListingRepository()), null!, null!);
            var expectedName = prefixExpected ? $"複製_{originalName}" : originalName;
            Assert.AreEqual(expectedName, editor.Name);
            Assert.AreEqual(!prefixExpected, editor.IsInfoBarErrorOpen);
            await editor.Save();
            Assert.IsFalse(editor.IsDirty);
            Assert.AreEqual(expectedName, property.Name);
        }
        finally
        {
            if (Directory.Exists(directory)) Directory.Delete(directory);
        }
    }

    [TestMethod]
    public async Task ResidentialDuplication_DisablesPendingUntilPropertySaveAndRequestsDatabaseVersion()
    {
        var propertyId = $"duplicate-{Guid.NewGuid():N}";
        var directory = Path.Combine(App.PropertyBlobDataFolder, propertyId);
        var property = new Residential.Property(propertyId, EntityStatus.New);
        var repository = new ResidentialListingRepository();
        try
        {
            using var editor = new ZumenSearch.Presentation.Features.Rent.Residentials.ViewModels.PropertyViewModel(property, false, new NavigationStub(), null!, null!, null!, new RentResidentialPropertyService(new ResidentialPropertyRepository()), new RentResidentialListingService(repository), null!, null!);
            editor.Name = "Building";
            var listing = new Residential.Listing("listing", EntityStatus.New, propertyId, EntityStatus.New, false, "Building", string.Empty);
            listing.SetName("Original");
            var notifications = 0;
            editor.DuplicateSelectedListingCommand.CanExecuteChanged += (_, _) => notifications++;
            editor.Receive(new ListingUpdatedMessage(listing));
            var summary = editor.ListingSummaries.Single();
            Assert.IsFalse(editor.DuplicateSelectedListingCommand.CanExecute(summary));
            Assert.IsTrue(notifications > 0);
            await editor.DuplicateSelectedListingCommand.ExecuteAsync(summary);
            Assert.AreEqual(0, repository.Calls);

            await editor.Save();
            Assert.IsFalse(editor.IsInfoBarErrorOpen, editor.InfoBarErrorMessage);
            Assert.AreEqual(EntityStatus.Saved, property.Status);
            Assert.IsTrue(editor.DuplicateSelectedListingCommand.CanExecute(summary));
            listing.SetName("Unsaved editor change");
            await editor.DuplicateSelectedListingCommand.ExecuteAsync(summary);
            Assert.AreEqual(1, repository.Calls);
            Assert.AreEqual(propertyId, repository.PropertyId);
            Assert.AreEqual(listing.Id, repository.ListingId);
            Assert.IsTrue(repository.Duplicate);
            Assert.IsTrue(editor.IsInfoBarErrorOpen);
            Assert.IsFalse(editor.IsBusy);
        }
        finally
        {
            if (Directory.Exists(directory)) Directory.Delete(directory);
        }
    }

    [TestMethod]
    public async Task CommercialDuplication_DisablesPendingUntilPropertySaveAndRequestsDatabaseVersion()
    {
        var propertyId = $"duplicate-{Guid.NewGuid():N}";
        var directory = Path.Combine(App.PropertyBlobDataFolder, propertyId);
        var property = new Commercial.Property(propertyId, EntityStatus.New);
        var repository = new CommercialListingRepository();
        try
        {
            using var editor = new ZumenSearch.Presentation.Features.Rent.Commercials.ViewModels.PropertyViewModel(property, false, new NavigationStub(), null!, null!, null!, new RentCommercialPropertyService(new CommercialPropertyRepository()), new RentCommercialListingService(repository), null!, null!);
            editor.Name = "Building";
            var listing = new Commercial.Listing.Listing("listing", EntityStatus.New, propertyId, EntityStatus.New, false, "Building", string.Empty);
            listing.SetName("Original");
            var notifications = 0;
            editor.DuplicateSelectedListingCommand.CanExecuteChanged += (_, _) => notifications++;
            editor.Receive(new ListingUpdatedMessage(listing));
            var summary = editor.ListingSummaries.Single();
            Assert.IsFalse(editor.DuplicateSelectedListingCommand.CanExecute(summary));
            Assert.IsTrue(notifications > 0);
            await editor.DuplicateSelectedListingCommand.ExecuteAsync(summary);
            Assert.AreEqual(0, repository.Calls);

            await editor.Save();
            Assert.IsFalse(editor.IsInfoBarErrorOpen, editor.InfoBarErrorMessage);
            Assert.AreEqual(EntityStatus.Saved, property.Status);
            Assert.IsTrue(editor.DuplicateSelectedListingCommand.CanExecute(summary));
            listing.SetName("Unsaved editor change");
            await editor.DuplicateSelectedListingCommand.ExecuteAsync(summary);
            Assert.AreEqual(1, repository.Calls);
            Assert.AreEqual(propertyId, repository.PropertyId);
            Assert.AreEqual(listing.Id, repository.ListingId);
            Assert.IsTrue(repository.Duplicate);
            Assert.IsTrue(editor.IsInfoBarErrorOpen);
            Assert.IsFalse(editor.IsBusy);
        }
        finally
        {
            if (Directory.Exists(directory)) Directory.Delete(directory);
        }
    }

    [TestMethod]
    public async Task ResidentialDuplication_DoesNotRunWhilePropertySaveIsBusy()
    {
        var propertyId = $"duplicate-busy-{Guid.NewGuid():N}";
        var directory = Path.Combine(App.PropertyBlobDataFolder, propertyId);
        var property = new Residential.Property(propertyId, EntityStatus.Saved);
        var summary = new Residential.ListingSummary("listing", "Original");
        property.ListingSummaries.Add(summary);
        var repository = new ResidentialListingRepository();
        using var releaseSave = new ManualResetEventSlim();
        try
        {
            using var editor = new ZumenSearch.Presentation.Features.Rent.Residentials.ViewModels.PropertyViewModel(property, false, new NavigationStub(), null!, null!, null!, new RentResidentialPropertyService(new ResidentialPropertyRepository(releaseSave)), new RentResidentialListingService(repository), null!, null!);
            editor.Name = "Building";
            var save = editor.Save();
            try
            {
                Assert.IsTrue(editor.IsBusy);
                Assert.IsTrue(editor.DuplicateSelectedListingCommand.CanExecute(summary));
                await editor.DuplicateSelectedListingCommand.ExecuteAsync(summary);
                Assert.AreEqual(0, repository.Calls);
            }
            finally
            {
                releaseSave.Set();
                await save;
            }
            Assert.IsFalse(editor.IsBusy);
            Assert.IsFalse(editor.IsInfoBarErrorOpen, editor.InfoBarErrorMessage);
        }
        finally
        {
            if (Directory.Exists(directory)) Directory.Delete(directory);
        }
    }

    [TestMethod]
    public async Task CommercialDuplication_DoesNotRunWhilePropertySaveIsBusy()
    {
        var propertyId = $"duplicate-busy-{Guid.NewGuid():N}";
        var directory = Path.Combine(App.PropertyBlobDataFolder, propertyId);
        var property = new Commercial.Property(propertyId, EntityStatus.Saved);
        var summary = new Commercial.ListingSummary("listing", "Original");
        property.ListingSummaries.Add(summary);
        var repository = new CommercialListingRepository();
        using var releaseSave = new ManualResetEventSlim();
        try
        {
            using var editor = new ZumenSearch.Presentation.Features.Rent.Commercials.ViewModels.PropertyViewModel(property, false, new NavigationStub(), null!, null!, null!, new RentCommercialPropertyService(new CommercialPropertyRepository(releaseSave)), new RentCommercialListingService(repository), null!, null!);
            editor.Name = "Building";
            var save = editor.Save();
            try
            {
                Assert.IsTrue(editor.IsBusy);
                Assert.IsTrue(editor.DuplicateSelectedListingCommand.CanExecute(summary));
                await editor.DuplicateSelectedListingCommand.ExecuteAsync(summary);
                Assert.AreEqual(0, repository.Calls);
            }
            finally
            {
                releaseSave.Set();
                await save;
            }
            Assert.IsFalse(editor.IsBusy);
            Assert.IsFalse(editor.IsInfoBarErrorOpen, editor.InfoBarErrorMessage);
        }
        finally
        {
            if (Directory.Exists(directory)) Directory.Delete(directory);
        }
    }

    [TestMethod]
    public void ResidentialDuplicateEditor_CopiesMediaAndDiscardsOnlyCopies()
    {
        var propertyId = $"duplicate-media-{Guid.NewGuid():N}";
        var propertyDirectory = Path.Combine(App.PropertyBlobDataFolder, propertyId);
        var sourceDirectory = Path.Combine(propertyDirectory, "source");
        var duplicateDirectory = Path.Combine(propertyDirectory, "duplicate");
        Directory.CreateDirectory(sourceDirectory);
        try
        {
            foreach (var filename in MediaFilenames) File.WriteAllText(Path.Combine(sourceDirectory, filename), filename);
            var listing = new Residential.Listing("duplicate", EntityStatus.New, propertyId, EntityStatus.Saved, false, "Building", string.Empty);
            listing.SetName("Original");
            listing.Pictures.Add(new Residential.ListingPicture("picture", "picture.jpg", EntityStatus.Saved) { BasePath = sourceDirectory });
            listing.Pdfs.Add(new Residential.ListingPdf("pdf", "document.pdf", "thumbnail.png", EntityStatus.Saved) { BasePath = sourceDirectory });
            using var editor = new ZumenSearch.Presentation.Features.Rent.Residentials.ViewModels.ListingViewModel(listing, true, new NavigationStub(), null!, null!, null!, null!);
            Assert.IsFalse(editor.IsInfoBarErrorOpen, editor.InfoBarErrorMessage);
            Assert.AreEqual("複製_Original", editor.Name);
            Assert.IsTrue(editor.IsDirty);
            foreach (var filename in MediaFilenames) Assert.AreEqual(filename, File.ReadAllText(Path.Combine(duplicateDirectory, filename)));
            editor.DiscardChanges();
            foreach (var filename in MediaFilenames)
            {
                Assert.IsTrue(File.Exists(Path.Combine(sourceDirectory, filename)));
                Assert.IsFalse(File.Exists(Path.Combine(duplicateDirectory, filename)));
            }
        }
        finally
        {
            foreach (var filename in MediaFilenames) File.Delete(Path.Combine(sourceDirectory, filename));
            if (Directory.Exists(sourceDirectory)) Directory.Delete(sourceDirectory);
            if (Directory.Exists(duplicateDirectory)) Directory.Delete(duplicateDirectory);
            if (Directory.Exists(propertyDirectory)) Directory.Delete(propertyDirectory);
        }
    }

    [TestMethod]
    public void CommercialDuplicateEditor_CopiesMediaAndDiscardsOnlyCopies()
    {
        var propertyId = $"duplicate-media-{Guid.NewGuid():N}";
        var propertyDirectory = Path.Combine(App.PropertyBlobDataFolder, propertyId);
        var sourceDirectory = Path.Combine(propertyDirectory, "source");
        var duplicateDirectory = Path.Combine(propertyDirectory, "duplicate");
        Directory.CreateDirectory(sourceDirectory);
        try
        {
            foreach (var filename in MediaFilenames) File.WriteAllText(Path.Combine(sourceDirectory, filename), filename);
            var listing = new Commercial.Listing.Listing("duplicate", EntityStatus.New, propertyId, EntityStatus.Saved, false, "Building", string.Empty);
            listing.SetName("Original");
            listing.Pictures.Add(new Commercial.Listing.ListingPicture("picture", "picture.jpg", EntityStatus.Saved) { BasePath = sourceDirectory });
            listing.Pdfs.Add(new Commercial.Listing.ListingPdf("pdf", "document.pdf", "thumbnail.png", EntityStatus.Saved) { BasePath = sourceDirectory });
            using var editor = new ZumenSearch.Presentation.Features.Rent.Commercials.ViewModels.ListingViewModel(listing, true, new NavigationStub(), null!, null!, null!, null!);
            Assert.IsFalse(editor.IsInfoBarErrorOpen, editor.InfoBarErrorMessage);
            Assert.AreEqual("複製_Original", editor.Name);
            Assert.IsTrue(editor.IsDirty);
            foreach (var filename in MediaFilenames) Assert.AreEqual(filename, File.ReadAllText(Path.Combine(duplicateDirectory, filename)));
            editor.DiscardChanges();
            foreach (var filename in MediaFilenames)
            {
                Assert.IsTrue(File.Exists(Path.Combine(sourceDirectory, filename)));
                Assert.IsFalse(File.Exists(Path.Combine(duplicateDirectory, filename)));
            }
        }
        finally
        {
            foreach (var filename in MediaFilenames) File.Delete(Path.Combine(sourceDirectory, filename));
            if (Directory.Exists(sourceDirectory)) Directory.Delete(sourceDirectory);
            if (Directory.Exists(duplicateDirectory)) Directory.Delete(duplicateDirectory);
            if (Directory.Exists(propertyDirectory)) Directory.Delete(propertyDirectory);
        }
    }

    private static readonly string[] MediaFilenames = ["picture.jpg", "document.pdf", "thumbnail.png"];

    private sealed class ResidentialPropertyRepository(ManualResetEventSlim? releaseSave = null) : IRentResidentialPropertyRepository
    {
        public ResultWrapper Upsert(Residential.Property property)
        {
            if (releaseSave is not null && !releaseSave.Wait(TimeSpan.FromSeconds(10))) throw new TimeoutException("Property save was not released.");
            return new();
        }
        public RentResidentialPropertyResultWrapper SelectById(string propertyId, bool duplicate = false) => throw new NotSupportedException();
        public ResultWrapper Delete(string propertyId) => throw new NotSupportedException();
    }

    private sealed class CommercialPropertyRepository(ManualResetEventSlim? releaseSave = null) : IRentCommercialPropertyRepository
    {
        public ResultWrapper Upsert(Commercial.Property property)
        {
            if (releaseSave is not null && !releaseSave.Wait(TimeSpan.FromSeconds(10))) throw new TimeoutException("Property save was not released.");
            return new();
        }
        public RentCommercialPropertyResultWrapper SelectById(string propertyId, bool duplicate = false) => throw new NotSupportedException();
        public ResultWrapper Delete(string propertyId) => throw new NotSupportedException();
    }

    private sealed class ResidentialListingRepository : IRentResidentialListingRepository
    {
        public int Calls { get; private set; }
        public string PropertyId { get; private set; } = string.Empty;
        public string ListingId { get; private set; } = string.Empty;
        public bool Duplicate { get; private set; }
        public RentResidentialListingResultWrapper SelectById(string propertyId, string listingId, bool duplicate = false)
        {
            Calls++;
            PropertyId = propertyId;
            ListingId = listingId;
            Duplicate = duplicate;
            return new();
        }
        public ResultWrapper Upsert(string propertyId, Residential.Listing listing) => throw new NotSupportedException();
        public ListingSearchResultWrapper Search(RentResidentialListingSearchCriteria criteria) => throw new NotSupportedException();
        public ResultWrapper Delete(string listingId) => throw new NotSupportedException();
    }

    private sealed class CommercialListingRepository : IRentCommercialListingRepository
    {
        public int Calls { get; private set; }
        public string PropertyId { get; private set; } = string.Empty;
        public string ListingId { get; private set; } = string.Empty;
        public bool Duplicate { get; private set; }
        public RentCommercialListingResultWrapper SelectById(string propertyId, string listingId, bool duplicate = false)
        {
            Calls++;
            PropertyId = propertyId;
            ListingId = listingId;
            Duplicate = duplicate;
            return new();
        }
        public ResultWrapper Upsert(string propertyId, Commercial.Listing.Listing listing) => throw new NotSupportedException();
        public ListingSearchResultWrapper Search(RentCommercialListingSearchCriteria criteria) => throw new NotSupportedException();
        public ResultWrapper Delete(string listingId) => throw new NotSupportedException();
    }

    private sealed class NavigationStub : INavigationGenericService
    {
        public void Initialize(Frame frame, List<(string Tag, string Label, Type? Page)> pages) { }
        public Frame? GetFrame() => null;
        public bool NavigateTo(object? selectedPage, object? param) => true;
        public bool IsCurrentPageSameAs(object? selectedPage) => true;
    }
}
