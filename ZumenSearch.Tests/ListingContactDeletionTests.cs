using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using ZumenSearch;
using ZumenSearch.Application.Contracts;
using ZumenSearch.Application.Results;
using ZumenSearch.Application.SearchResults;
using ZumenSearch.Application.Services.Persons;
using ZumenSearch.Application.Services.Rent.Commercials;
using ZumenSearch.Application.Services.Rent.Residentials;
using ZumenSearch.Domain.Entities.Base;
using ZumenSearch.Domain.Entities.Person;
using ZumenSearch.Domain.Enums;
using ZumenSearch.Domain.ValueObjects.Transportation;
using ZumenSearch.Presentation.Services.Contracts;
using Commercial = ZumenSearch.Domain.Entities.Rent.Commercials.Listing;
using Residential = ZumenSearch.Domain.Entities.Rent.Residentials;

namespace ZumenSearch.Tests;

[TestClass]
public sealed class ListingContactDeletionTests
{
    [TestMethod]
    [DataRow(false, 0)]
    [DataRow(false, 1)]
    [DataRow(false, 2)]
    [DataRow(true, 0)]
    [DataRow(true, 1)]
    [DataRow(true, 2)]
    public async Task ResidentialContacts_AreAppliedOnlyOnSave(bool readd, int saveOutcome)
    {
        var propertyId = $"contacts-{Guid.NewGuid():N}";
        var directory = Path.Combine(App.PropertyBlobDataFolder, propertyId, "listing");
        var lessor = new PersonNatural("lessor", EntityStatus.Saved) { NameLast = "Lessor" };
        var broker = new PersonNatural("broker", EntityStatus.Saved) { NameLast = "Broker" };
        var listing = new Residential.Listing("listing", EntityStatus.New, propertyId, saveOutcome == 0 ? EntityStatus.New : EntityStatus.Saved, false, "Building", string.Empty);
        listing.SetName("Unit");
        listing.Lessors.Add(lessor);
        listing.Brokers.Add(broker);
        var repository = new ResidentialRepository { Fail = saveOutcome == 1 };
        try
        {
            using var editor = new ZumenSearch.Presentation.Features.Rent.Residentials.ViewModels.ListingViewModel(listing, false, new NavigationStub(), new DialogStub(lessor, broker), null!, new RentResidentialListingService(repository), new PersonService(new PersonRepository(lessor, broker)), true);
            await editor.DeleteLessor(editor.LessorsWrapper.Single());
            await editor.DeleteBrokerCommand.ExecuteAsync(editor.BrokersWrapper.Single());
            Assert.AreEqual(lessor.Id, listing.Lessors.Single().Id);
            Assert.AreEqual(broker.Id, listing.Brokers.Single().Id);
            Assert.AreEqual(0, listing.LessorsToBeDeleted.Count);
            Assert.AreEqual(0, listing.BrokersToBeDeleted.Count);
            if (readd)
            {
                await editor.AddLessor();
                await editor.AddBroker();
            }
            if (saveOutcome != 0)
            {
                await editor.Save();
                Assert.AreEqual(1, repository.Calls);
                Assert.AreEqual(readd ? 0 : 1, repository.LessorDeletions);
                Assert.AreEqual(readd ? 0 : 1, repository.BrokerDeletions);
                if (saveOutcome == 2)
                {
                    Assert.IsFalse(editor.IsDirty);
                    Assert.AreEqual(readd ? 1 : 0, listing.Lessors.Count);
                    Assert.AreEqual(readd ? 1 : 0, listing.Brokers.Count);
                    return;
                }
                Assert.IsTrue(editor.IsDirty);
                Assert.IsTrue(editor.IsInfoBarErrorOpen);
            }
            editor.DiscardChanges();
            Assert.AreEqual(lessor.Id, listing.Lessors.Single().Id);
            Assert.AreEqual(broker.Id, listing.Brokers.Single().Id);
            Assert.AreEqual(0, listing.LessorsToBeDeleted.Count);
            Assert.AreEqual(0, listing.BrokersToBeDeleted.Count);
        }
        finally
        {
            if (Directory.Exists(directory)) Directory.Delete(directory);
            var parent = Path.GetDirectoryName(directory)!;
            if (Directory.Exists(parent)) Directory.Delete(parent);
        }
    }

    [TestMethod]
    [DataRow(false, 0)]
    [DataRow(false, 1)]
    [DataRow(false, 2)]
    [DataRow(true, 0)]
    [DataRow(true, 1)]
    [DataRow(true, 2)]
    public async Task CommercialContacts_AreAppliedOnlyOnSave(bool readd, int saveOutcome)
    {
        var propertyId = $"contacts-{Guid.NewGuid():N}";
        var directory = Path.Combine(App.PropertyBlobDataFolder, propertyId, "listing");
        var lessor = new PersonNatural("lessor", EntityStatus.Saved) { NameLast = "Lessor" };
        var broker = new PersonNatural("broker", EntityStatus.Saved) { NameLast = "Broker" };
        var listing = new Commercial.Listing("listing", EntityStatus.New, propertyId, saveOutcome == 0 ? EntityStatus.New : EntityStatus.Saved, false, "Building", string.Empty);
        listing.SetName("Unit");
        listing.Lessors.Add(lessor);
        listing.Brokers.Add(broker);
        var repository = new CommercialRepository { Fail = saveOutcome == 1 };
        try
        {
            using var editor = new ZumenSearch.Presentation.Features.Rent.Commercials.ViewModels.ListingViewModel(listing, false, new NavigationStub(), new DialogStub(lessor, broker), null!, new RentCommercialListingService(repository), new PersonService(new PersonRepository(lessor, broker)), true);
            await editor.DeleteLessor(editor.LessorsWrapper.Single());
            await editor.DeleteBrokerCommand.ExecuteAsync(editor.BrokersWrapper.Single());
            Assert.AreEqual(lessor.Id, listing.Lessors.Single().Id);
            Assert.AreEqual(broker.Id, listing.Brokers.Single().Id);
            Assert.AreEqual(0, listing.LessorsToBeDeleted.Count);
            Assert.AreEqual(0, listing.BrokersToBeDeleted.Count);
            if (readd)
            {
                await editor.AddLessor();
                await editor.AddBroker();
            }
            if (saveOutcome != 0)
            {
                await editor.Save();
                Assert.AreEqual(1, repository.Calls);
                Assert.AreEqual(readd ? 0 : 1, repository.LessorDeletions);
                Assert.AreEqual(readd ? 0 : 1, repository.BrokerDeletions);
                if (saveOutcome == 2)
                {
                    Assert.IsFalse(editor.IsDirty);
                    Assert.AreEqual(readd ? 1 : 0, listing.Lessors.Count);
                    Assert.AreEqual(readd ? 1 : 0, listing.Brokers.Count);
                    return;
                }
                Assert.IsTrue(editor.IsDirty);
                Assert.IsTrue(editor.IsInfoBarErrorOpen);
            }
            editor.DiscardChanges();
            Assert.AreEqual(lessor.Id, listing.Lessors.Single().Id);
            Assert.AreEqual(broker.Id, listing.Brokers.Single().Id);
            Assert.AreEqual(0, listing.LessorsToBeDeleted.Count);
            Assert.AreEqual(0, listing.BrokersToBeDeleted.Count);
        }
        finally
        {
            if (Directory.Exists(directory)) Directory.Delete(directory);
            var parent = Path.GetDirectoryName(directory)!;
            if (Directory.Exists(parent)) Directory.Delete(parent);
        }
    }

    [TestMethod]
    [DataRow(false)]
    [DataRow(true)]
    public async Task ResidentialReadd_ChangesEarlierDeletionOnlyOnSuccessfulSave(bool fail)
    {
        var propertyId = $"contacts-{Guid.NewGuid():N}";
        var directory = Path.Combine(App.PropertyBlobDataFolder, propertyId, "listing");
        var lessor = new PersonNatural("lessor", EntityStatus.Saved);
        var broker = new PersonNatural("broker", EntityStatus.Saved);
        var listing = new Residential.Listing("listing", EntityStatus.New, propertyId, EntityStatus.Saved, false, "Building", string.Empty);
        listing.SetName("Unit");
        listing.LessorsToBeDeleted.Add(lessor);
        listing.BrokersToBeDeleted.Add(broker);
        var repository = new ResidentialRepository { Fail = fail };
        try
        {
            using var editor = new ZumenSearch.Presentation.Features.Rent.Residentials.ViewModels.ListingViewModel(listing, false, new NavigationStub(), new DialogStub(lessor, broker), null!, new RentResidentialListingService(repository), new PersonService(new PersonRepository(lessor, broker)), true);
            await editor.AddLessor();
            await editor.AddBroker();
            Assert.AreEqual(lessor.Id, listing.LessorsToBeDeleted.Single().Id);
            Assert.AreEqual(broker.Id, listing.BrokersToBeDeleted.Single().Id);
            await editor.Save();
            Assert.AreEqual(1, repository.Calls);
            Assert.AreEqual(0, repository.LessorDeletions);
            Assert.AreEqual(0, repository.BrokerDeletions);
            if (fail)
            {
                Assert.IsTrue(editor.IsDirty);
                Assert.AreEqual(0, listing.Lessors.Count);
                Assert.AreEqual(0, listing.Brokers.Count);
                Assert.AreEqual(lessor.Id, listing.LessorsToBeDeleted.Single().Id);
                Assert.AreEqual(broker.Id, listing.BrokersToBeDeleted.Single().Id);
            }
            else
            {
                Assert.AreEqual(lessor.Id, listing.Lessors.Single().Id);
                Assert.AreEqual(broker.Id, listing.Brokers.Single().Id);
                Assert.AreEqual(0, listing.LessorsToBeDeleted.Count);
                Assert.AreEqual(0, listing.BrokersToBeDeleted.Count);
            }
        }
        finally
        {
            if (Directory.Exists(directory)) Directory.Delete(directory);
            var parent = Path.GetDirectoryName(directory)!;
            if (Directory.Exists(parent)) Directory.Delete(parent);
        }
    }

    [TestMethod]
    [DataRow(false)]
    [DataRow(true)]
    public async Task CommercialReadd_ChangesEarlierDeletionOnlyOnSuccessfulSave(bool fail)
    {
        var propertyId = $"contacts-{Guid.NewGuid():N}";
        var directory = Path.Combine(App.PropertyBlobDataFolder, propertyId, "listing");
        var lessor = new PersonNatural("lessor", EntityStatus.Saved);
        var broker = new PersonNatural("broker", EntityStatus.Saved);
        var listing = new Commercial.Listing("listing", EntityStatus.New, propertyId, EntityStatus.Saved, false, "Building", string.Empty);
        listing.SetName("Unit");
        listing.LessorsToBeDeleted.Add(lessor);
        listing.BrokersToBeDeleted.Add(broker);
        var repository = new CommercialRepository { Fail = fail };
        try
        {
            using var editor = new ZumenSearch.Presentation.Features.Rent.Commercials.ViewModels.ListingViewModel(listing, false, new NavigationStub(), new DialogStub(lessor, broker), null!, new RentCommercialListingService(repository), new PersonService(new PersonRepository(lessor, broker)), true);
            await editor.AddLessor();
            await editor.AddBroker();
            Assert.AreEqual(lessor.Id, listing.LessorsToBeDeleted.Single().Id);
            Assert.AreEqual(broker.Id, listing.BrokersToBeDeleted.Single().Id);
            await editor.Save();
            Assert.AreEqual(1, repository.Calls);
            Assert.AreEqual(0, repository.LessorDeletions);
            Assert.AreEqual(0, repository.BrokerDeletions);
            if (fail)
            {
                Assert.IsTrue(editor.IsDirty);
                Assert.AreEqual(0, listing.Lessors.Count);
                Assert.AreEqual(0, listing.Brokers.Count);
                Assert.AreEqual(lessor.Id, listing.LessorsToBeDeleted.Single().Id);
                Assert.AreEqual(broker.Id, listing.BrokersToBeDeleted.Single().Id);
            }
            else
            {
                Assert.AreEqual(lessor.Id, listing.Lessors.Single().Id);
                Assert.AreEqual(broker.Id, listing.Brokers.Single().Id);
                Assert.AreEqual(0, listing.LessorsToBeDeleted.Count);
                Assert.AreEqual(0, listing.BrokersToBeDeleted.Count);
            }
        }
        finally
        {
            if (Directory.Exists(directory)) Directory.Delete(directory);
            var parent = Path.GetDirectoryName(directory)!;
            if (Directory.Exists(parent)) Directory.Delete(parent);
        }
    }

    private sealed class ResidentialRepository : IRentResidentialListingRepository
    {
        public bool Fail { get; set; }
        public int Calls { get; private set; }
        public int LessorDeletions { get; private set; }
        public int BrokerDeletions { get; private set; }
        public ResultWrapper Upsert(string propertyId, Residential.Listing listing)
        {
            Calls++;
            LessorDeletions = listing.LessorsToBeDeleted.Count;
            BrokerDeletions = listing.BrokersToBeDeleted.Count;
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
        public int LessorDeletions { get; private set; }
        public int BrokerDeletions { get; private set; }
        public ResultWrapper Upsert(string propertyId, Commercial.Listing listing)
        {
            Calls++;
            LessorDeletions = listing.LessorsToBeDeleted.Count;
            BrokerDeletions = listing.BrokersToBeDeleted.Count;
            return new ResultWrapper { IsError = Fail };
        }
        public ListingSearchResultWrapper Search(RentCommercialListingSearchCriteria criteria) => throw new NotSupportedException();
        public RentCommercialListingResultWrapper SelectById(string propertyId, string listingId, bool duplicate = false) => throw new NotSupportedException();
        public ResultWrapper Delete(string listingId) => throw new NotSupportedException();
    }

    private sealed class PersonRepository(PersonBase lessor, PersonBase broker) : IPersonRepository
    {
        public PersonResultWrapper SelectRentLessorById(string lessorId) => new() { Person = lessor };
        public PersonResultWrapper SelectBrokerById(string brokerId) => new() { Person = broker };
        public PersonsSearchResultWrapper SelectRentLessorsByKeyword(string keyword) => new() { PersonSearchResult = [new PersonSearchResultItem(lessor.Id, lessor.PersonKind)] };
        public PersonsSearchResultWrapper SelectBrokersByKeyword(string keyword) => new() { PersonSearchResult = [new PersonSearchResultItem(broker.Id, broker.PersonKind)] };
        public ResultWrapper DeleteRentLessor(string lessorId) => throw new NotSupportedException();
        public ResultWrapper UpsertBroker(PersonBase person) => throw new NotSupportedException();
        public ResultWrapper DeleteBroker(string brokerId) => throw new NotSupportedException();
    }

    private sealed class NavigationStub : INavigationGenericService
    {
        public void Initialize(Frame frame, List<(string Tag, string Label, Type? Page)> pages) { }
        public Frame? GetFrame() => null;
        public bool NavigateTo(object? selectedPage, object? param) => true;
        public bool IsCurrentPageSameAs(object? selectedPage) => true;
    }

    private sealed class DialogStub(PersonBase lessor, PersonBase broker) : IDialogGenericService
    {
        public void Initialize(XamlRoot xamlRoot, Window window) => throw new NotSupportedException();
        public Task<ContentDialogResult> ShowEditorCloseConfirmationDialog() => throw new NotSupportedException();
        public Task<ContentDialogResult> ShowDeleteConfirmationDialog(string message) => Task.FromResult(ContentDialogResult.Primary);
        public async Task<PersonSearchResultItem?> ShowLessorSelectDialog(ZumenSearch.Presentation.Dialogs.ViewModels.LessorSelectViewModel viewModel)
        {
            var loaded = new TaskCompletionSource(TaskCreationOptions.RunContinuationsAsynchronously);
            viewModel.PropertyChanged += (_, e) => { if (e.PropertyName == nameof(viewModel.SuggestedLessors)) loaded.TrySetResult(); };
            if (viewModel.SuggestedLessors?.Count > 0) loaded.TrySetResult();
            await loaded.Task.WaitAsync(TimeSpan.FromSeconds(10));
            return new PersonSearchResultItem(lessor.Id, lessor.PersonKind);
        }
        public async Task<PersonSearchResultItem?> ShowBrokerSelectDialog(ZumenSearch.Presentation.Dialogs.ViewModels.BrokerSelectViewModel viewModel)
        {
            var loaded = new TaskCompletionSource(TaskCreationOptions.RunContinuationsAsynchronously);
            viewModel.PropertyChanged += (_, e) => { if (e.PropertyName == nameof(viewModel.SuggestedBrokers)) loaded.TrySetResult(); };
            if (viewModel.SuggestedBrokers?.Count > 0) loaded.TrySetResult();
            await loaded.Task.WaitAsync(TimeSpan.FromSeconds(10));
            return new PersonSearchResultItem(broker.Id, broker.PersonKind);
        }
        public Task<RailLine?> ShowRailLineSelectDialog() => throw new NotSupportedException();
        public Task<RailStation?> ShowRailStationSelectDialog(string railLineCode) => throw new NotSupportedException();
    }
}
