using System.Collections.ObjectModel;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using ZumenSearch.Application.Contracts;
using ZumenSearch.Application.Results;
using ZumenSearch.Application.SearchResults;
using ZumenSearch.Application.Services.Rent.Commercials;
using ZumenSearch.Application.Services.Rent.Residentials;
using ZumenSearch.Domain.Enums;
using ZumenSearch.Domain.ValueObjects.Transportation;
using ZumenSearch.Presentation.Messaging;
using ZumenSearch.Presentation.Search.ViewModels;
using ZumenSearch.Presentation.Services.Contracts;
using ZumenSearch.Presentation.Shell.ViewModels;

namespace ZumenSearch.Tests;

[TestClass]
public sealed class ListingSearchResultSynchronizationTests
{
    [TestMethod]
    public void SetPropertyName_UpdatesModelAndNotifiesOnlyOnChange()
    {
        var model = new ListingSearchResultItem("listing", "property", PropertyContextType.RentResidential, string.Empty) { PropertyName = "Old building" };
        model.SetName("Unit A");
        model.SetIsModified(false);
        var item = new ListingSearchResultItemViewModel(model);
        var notifications = new List<string?>();
        item.PropertyChanged += (_, args) => notifications.Add(args.PropertyName);

        item.SetPropertyName("New building");
        item.SetPropertyName("New building");

        Assert.AreEqual("New building", item.PropertyName);
        Assert.AreEqual("New building", model.PropertyName);
        Assert.AreEqual("Unit A", item.Name);
        Assert.IsFalse(model.IsModified);
        CollectionAssert.AreEqual(new[] { nameof(ListingSearchResultItemViewModel.PropertyName) }, notifications);
    }

    [TestMethod]
    [DataRow(false, EntityStatus.Saved)]
    [DataRow(true, EntityStatus.Saved)]
    [DataRow(false, EntityStatus.New)]
    [DataRow(true, EntityStatus.New)]
    public void PropertyUpdated_OnlyRenamesResultsForMatchingSavedPropertyAndContext(bool commercial, EntityStatus status)
    {
        using var main = CreateMain(new RepositoryStub());
        var context = Context(commercial);
        var results = Results(main, commercial);
        var first = NewResult("first", "property", context);
        var second = NewResult("second", "property", context);
        var otherProperty = NewResult("other", "other-property", context);
        var otherContext = NewResult("first", "property", Context(!commercial));
        results.Add(first);
        results.Add(second);
        results.Add(otherProperty);
        Results(main, !commercial).Add(otherContext);
        var property = new PropertySearchResultItem("property", context);
        property.SetName("Renamed building");
        property.SetStatus(status);

        main.Receive(new PropertyUpdatedMessage(property));

        var expectedName = status == EntityStatus.Saved ? "Renamed building" : "Old building";
        Assert.AreEqual(expectedName, first.PropertyName);
        Assert.AreEqual(expectedName, second.PropertyName);
        Assert.AreEqual("Old building", otherProperty.PropertyName);
        Assert.AreEqual("Old building", otherContext.PropertyName);
        Assert.AreEqual("first", first.Name);
        Assert.AreEqual(3, results.Count);
    }

    [TestMethod]
    [DataRow(false, false, false)]
    [DataRow(true, false, false)]
    [DataRow(false, true, false)]
    [DataRow(true, true, false)]
    [DataRow(false, false, true)]
    [DataRow(true, false, true)]
    public async Task PropertyDeletion_RemovesAllChildResultsOnlyAfterSuccessfulDeletion(bool commercial, bool fail, bool cancel)
    {
        var repository = new RepositoryStub { Fail = fail };
        var dialog = new DialogStub { Confirmation = cancel ? ContentDialogResult.None : ContentDialogResult.Primary };
        using var main = CreateMain(repository, dialog);
        var propertyId = $"search-delete-{Guid.NewGuid():N}";
        var context = Context(commercial);
        var selected = new PropertySearchResultItemViewModel(new PropertySearchResultItem(propertyId, context));
        main.PropertySearchResult.Add(selected);
        main.RecentProperties.Add(new PropertySearchResultItemViewModel(new PropertySearchResultItem(propertyId, context)));
        var results = Results(main, commercial);
        var first = NewResult("first", propertyId, context);
        var second = NewResult("second", propertyId, context);
        var unrelated = NewResult("unrelated", "other-property", context);
        var otherContext = NewResult("first", propertyId, Context(!commercial));
        results.Add(first);
        results.Add(second);
        results.Add(unrelated);
        Results(main, !commercial).Add(otherContext);

        if (commercial) await main.DeleteRentCommercialCommand.ExecuteAsync(selected);
        else await main.DeleteRentResidentialCommand.ExecuteAsync(selected);

        Assert.AreEqual(cancel ? 0 : 1, repository.DeleteCalls);
        CollectionAssert.AreEqual(fail || cancel ? new[] { first, second, unrelated } : new[] { unrelated }, results);
        CollectionAssert.AreEqual(new[] { otherContext }, Results(main, !commercial));
        Assert.AreEqual(fail || cancel ? 1 : 0, main.PropertySearchResult.Count);
        Assert.AreEqual(fail || cancel ? 1 : 0, main.RecentProperties.Count);
        Assert.AreEqual(fail && !cancel, main.IsInfoBarErrorOpen);
        if (fail && !cancel) Assert.AreEqual(InfoBarSeverity.Error, main.InfoBarSeverity);
    }

    [TestMethod]
    [DataRow(false, false, false)]
    [DataRow(false, true, false)]
    [DataRow(true, false, false)]
    [DataRow(true, true, false)]
    [DataRow(false, false, true)]
    [DataRow(false, true, true)]
    [DataRow(true, false, true)]
    [DataRow(true, true, true)]
    public async Task MissingListing_OpenAndDuplicateRemoveOnlyMatchingResultsAndWarnWhileErrorsPreserveResults(bool commercial, bool duplicate, bool fail)
    {
        var repository = new RepositoryStub { Fail = fail };
        using var main = CreateMain(repository);
        var context = Context(commercial);
        var results = Results(main, commercial);
        var selected = NewResult("listing", "property", context);
        var repeated = NewResult("listing", "property", context);
        var otherProperty = NewResult("listing", "other-property", context);
        var otherListing = NewResult("other-listing", "property", context);
        var otherContext = NewResult("listing", "property", Context(!commercial));
        results.Add(selected);
        results.Add(repeated);
        results.Add(otherProperty);
        results.Add(otherListing);
        Results(main, !commercial).Add(otherContext);

        if (commercial)
        {
            if (duplicate) await main.DuplicateRentCommercialListingCommand.ExecuteAsync(selected);
            else await main.EditRentCommercialListingCommand.ExecuteAsync(selected);
        }
        else
        {
            if (duplicate) await main.DuplicateRentResidentialListingCommand.ExecuteAsync(selected);
            else await main.EditRentResidentialListingCommand.ExecuteAsync(selected);
        }

        Assert.AreEqual(("property", "listing", duplicate), repository.LastSelection);
        CollectionAssert.AreEqual(fail ? new[] { selected, repeated, otherProperty, otherListing } : new[] { otherProperty, otherListing }, results);
        CollectionAssert.AreEqual(new[] { otherContext }, Results(main, !commercial));
        Assert.IsTrue(main.IsInfoBarErrorOpen);
        Assert.AreEqual(fail ? InfoBarSeverity.Error : InfoBarSeverity.Warning, main.InfoBarSeverity);
        Assert.AreEqual(fail ? "Database failure" : "募集物件が見つかりません", main.InfoBarErrorTitle);
        if (!fail) Assert.AreEqual("募集物件が存在しないため、検索結果から削除しました。再検索して最新の情報を確認してください。", main.InfoBarErrorMessage);
        Assert.AreEqual(0, main.RentResidentialListingEditorList.Count);
        Assert.AreEqual(0, main.RentCommercialListingEditorList.Count);
    }

    [TestMethod]
    [DataRow(false)]
    [DataRow(true)]
    public void PropertyEditorRemoval_RemovesEveryMatchingResultButPreservesOtherListingsAndProperties(bool commercial)
    {
        using var main = CreateMain(new RepositoryStub());
        var context = Context(commercial);
        var results = Results(main, commercial);
        results.Add(NewResult("listing", "property", context));
        results.Add(NewResult("listing", "property", context));
        var otherListing = NewResult("other-listing", "property", context);
        var otherProperty = NewResult("listing", "other-property", context);
        var otherContext = NewResult("listing", "property", Context(!commercial));
        results.Add(otherListing);
        results.Add(otherProperty);
        Results(main, !commercial).Add(otherContext);

        if (commercial) main.RemoveCommercialListingSearchResults("property", "listing");
        else main.RemoveResidentialListingSearchResults("property", "listing");

        CollectionAssert.AreEqual(new[] { otherListing, otherProperty }, results);
        CollectionAssert.AreEqual(new[] { otherContext }, Results(main, !commercial));
    }

    private static PropertyContextType Context(bool commercial) => commercial ? PropertyContextType.RentCommercial : PropertyContextType.RentResidential;

    private static ObservableCollection<ListingSearchResultItemViewModel> Results(MainViewModel main, bool commercial) => commercial ? main.RentCommercialListingSearchResult : main.RentResidentialListingSearchResult;

    private static ListingSearchResultItemViewModel NewResult(string listingId, string propertyId, PropertyContextType context)
    {
        var model = new ListingSearchResultItem(listingId, propertyId, context, string.Empty) { PropertyName = "Old building" };
        model.SetName(listingId);
        model.SetIsModified(false);
        return new ListingSearchResultItemViewModel(model);
    }

    private static MainViewModel CreateMain(RepositoryStub repository, DialogStub? dialog = null) => new(shellRentResidentialPropertyFactory: null!, shellRentResidentialListingFactory: null!, shellRentCommercialPropertyFactory: null!, shellRentCommercialListingFactory: null!, shellRentLessorFactory: null!, shellSaleResidentialPropertyFactory: null!, shellBrokerFactory: null!, navigationService: null!, dataAccessService: null!, propertySearchService: null!, rentResidentialPropertyService: new RentResidentialPropertyService(repository), rentResidentialListingService: new RentResidentialListingService(repository), rentCommercialPropertyService: new RentCommercialPropertyService(repository), rentCommercialListingService: new RentCommercialListingService(repository), personService: null!, dispatcherService: null!, configurationService: null!, dialogService: dialog ?? new DialogStub());

    private sealed class RepositoryStub : IRentResidentialPropertyRepository, IRentCommercialPropertyRepository, IRentResidentialListingRepository, IRentCommercialListingRepository
    {
        public bool Fail { get; init; }
        public int DeleteCalls { get; private set; }
        public (string PropertyId, string ListingId, bool Duplicate) LastSelection { get; private set; }

        public ResultWrapper Delete(string propertyId)
        {
            DeleteCalls++;
            return new ResultWrapper { IsError = Fail, Error = new ErrorInfo { Title = "Database failure" } };
        }

        RentResidentialListingResultWrapper IRentResidentialListingRepository.SelectById(string propertyId, string listingId, bool duplicate)
        {
            LastSelection = (propertyId, listingId, duplicate);
            return new RentResidentialListingResultWrapper { IsError = Fail, Error = new ErrorInfo { Title = "Database failure" } };
        }

        RentCommercialListingResultWrapper IRentCommercialListingRepository.SelectById(string propertyId, string listingId, bool duplicate)
        {
            LastSelection = (propertyId, listingId, duplicate);
            return new RentCommercialListingResultWrapper { IsError = Fail, Error = new ErrorInfo { Title = "Database failure" } };
        }

        public ResultWrapper Upsert(ZumenSearch.Domain.Entities.Rent.Residentials.Property residentialProperty) => throw new NotSupportedException();
        public ResultWrapper Upsert(ZumenSearch.Domain.Entities.Rent.Commercials.Property commercialProperty) => throw new NotSupportedException();
        public ResultWrapper Upsert(string propertyId, ZumenSearch.Domain.Entities.Rent.Residentials.Listing listing) => throw new NotSupportedException();
        public ResultWrapper Upsert(string propertyId, ZumenSearch.Domain.Entities.Rent.Commercials.Listing.Listing listing) => throw new NotSupportedException();
        RentResidentialPropertyResultWrapper IRentResidentialPropertyRepository.SelectById(string propertyId, bool duplicate) => throw new NotSupportedException();
        RentCommercialPropertyResultWrapper IRentCommercialPropertyRepository.SelectById(string propertyId, bool duplicate) => throw new NotSupportedException();
        public ListingSearchResultWrapper Search(RentResidentialListingSearchCriteria criteria) => throw new NotSupportedException();
        public ListingSearchResultWrapper Search(RentCommercialListingSearchCriteria criteria) => throw new NotSupportedException();
    }

    private sealed class DialogStub : IDialogGenericService
    {
        public ContentDialogResult Confirmation { get; init; } = ContentDialogResult.Primary;

        public Task<ContentDialogResult> ShowDeleteConfirmationDialog(string message) => Task.FromResult(Confirmation);
        public void Initialize(XamlRoot xamlRoot, Window window) => throw new NotSupportedException();
        public Task<ContentDialogResult> ShowEditorCloseConfirmationDialog() => throw new NotSupportedException();
        public Task<PersonSearchResultItem?> ShowLessorSelectDialog(ZumenSearch.Presentation.Dialogs.ViewModels.LessorSelectViewModel viewModel) => throw new NotSupportedException();
        public Task<PersonSearchResultItem?> ShowBrokerSelectDialog(ZumenSearch.Presentation.Dialogs.ViewModels.BrokerSelectViewModel viewModel) => throw new NotSupportedException();
        public Task<RailLine?> ShowRailLineSelectDialog() => throw new NotSupportedException();
        public Task<RailStation?> ShowRailStationSelectDialog(string railLineCode) => throw new NotSupportedException();
    }
}
