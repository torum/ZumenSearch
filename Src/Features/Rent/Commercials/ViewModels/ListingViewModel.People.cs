using CommunityToolkit.Mvvm.Input;
using System.Collections.ObjectModel;
using ZumenSearch.Models.Messenger;

namespace ZumenSearch.ViewModels.Rent.Commercials;

public sealed partial class ListingViewModel
{
    private string? _pendingLessorId;
    private Models.Base.PersonBase? _pendingLessorUpdate;
    private bool _pendingLessorDeleted;
    private string? _pendingBrokerId;
    private Models.Base.PersonBase? _pendingBrokerUpdate;
    private bool _pendingBrokerDeleted;
    private readonly List<Models.Base.PersonBase> _draftLessorDeletions = [];
    private readonly List<Models.Base.PersonBase> _draftBrokerDeletions = [];

    public ObservableCollection<Models.Rent.Commercials.PersonWrapperForListingViewModel> LessorsWrapper { get; } = [];
    public ObservableCollection<Models.Rent.Commercials.PersonWrapperForListingViewModel> BrokersWrapper { get; } = [];

    private bool HasPendingAssociationEdits()
    {
        var lessorIds = LessorsWrapper.Select(item => item.Person.Id).ToHashSet(StringComparer.Ordinal);
        var brokerIds = BrokersWrapper.Select(item => item.Person.Id).ToHashSet(StringComparer.Ordinal);
        return !lessorIds.SetEquals(_committedLessors.Keys) || !brokerIds.SetEquals(_committedBrokers.Keys) || _draftLessorDeletions.Count > 0 || _draftBrokerDeletions.Count > 0;
    }

    private void CaptureCommittedOwnershipAssociations(IEnumerable<Models.Base.PersonBase> lessors, IEnumerable<Models.Base.PersonBase> brokers)
    {
        _committedLessors.Clear();
        foreach (var person in lessors) _committedLessors[person.Id] = person;
        _committedBrokers.Clear();
        foreach (var person in brokers) _committedBrokers[person.Id] = person;
    }

    private void CaptureCommittedOwnershipAssociations(IEnumerable<string> lessorIds, IEnumerable<string> brokerIds)
    {
        var lessorIdSet = lessorIds.ToHashSet(StringComparer.Ordinal);
        var brokerIdSet = brokerIds.ToHashSet(StringComparer.Ordinal);
        foreach (var person in _unit.Lessors.Concat(_unit.LessorsToBeDeleted).Concat(LessorsWrapper.Select(item => item.Person)).Where(person => lessorIdSet.Contains(person.Id))) _committedLessors[person.Id] = person;
        foreach (var person in _unit.Brokers.Concat(_unit.BrokersToBeDeleted).Concat(BrokersWrapper.Select(item => item.Person)).Where(person => brokerIdSet.Contains(person.Id))) _committedBrokers[person.Id] = person;
        foreach (var id in _committedLessors.Keys.Where(id => !lessorIdSet.Contains(id)).ToList()) _committedLessors.Remove(id);
        foreach (var id in _committedBrokers.Keys.Where(id => !brokerIdSet.Contains(id)).ToList()) _committedBrokers.Remove(id);
    }

    private void ApplyAssociationDraft()
    {
        _unit.Lessors = new ObservableCollection<Models.Base.PersonBase>(LessorsWrapper.Select(wrapper => wrapper.Person));
        _unit.Brokers = new ObservableCollection<Models.Base.PersonBase>(BrokersWrapper.Select(wrapper => wrapper.Person));
        foreach (var person in _unit.LessorsToBeDeleted.Where(person => _unit.Lessors.Any(current => current.Id.Equals(person.Id, StringComparison.Ordinal))).ToList()) _unit.LessorsToBeDeleted.Remove(person);
        foreach (var person in _unit.BrokersToBeDeleted.Where(person => _unit.Brokers.Any(current => current.Id.Equals(person.Id, StringComparison.Ordinal))).ToList()) _unit.BrokersToBeDeleted.Remove(person);
        foreach (var person in _draftLessorDeletions.Where(person => !_unit.LessorsToBeDeleted.Any(item => item.Id.Equals(person.Id, StringComparison.Ordinal)))) _unit.LessorsToBeDeleted.Add(person);
        foreach (var person in _draftBrokerDeletions.Where(person => !_unit.BrokersToBeDeleted.Any(item => item.Id.Equals(person.Id, StringComparison.Ordinal)))) _unit.BrokersToBeDeleted.Add(person);
    }

    private void PopulateAssociations()
    {
        LessorsWrapper.Clear();
        foreach (var lessor in _unit.Lessors)
        {
            LessorsWrapper.Add(new Models.Rent.Commercials.PersonWrapperForListingViewModel(lessor, this));
        }

        BrokersWrapper.Clear();
        foreach (var broker in _unit.Brokers)
        {
            BrokersWrapper.Add(new Models.Rent.Commercials.PersonWrapperForListingViewModel(broker, this));
        }
    }

    public void Receive(LessorUpdatedMessage message)
    {
        var lessor = message.Value;
        if (_pendingLessorId == lessor.Id) _pendingLessorUpdate = lessor;
        if (_committedLessors.ContainsKey(lessor.Id)) _committedLessors[lessor.Id] = lessor;
        var wrapper = LessorsWrapper.FirstOrDefault(item => item.Person.Id.Equals(lessor.Id, StringComparison.Ordinal));
        if (wrapper is not null) wrapper.Person = lessor;
    }

    public void Receive(LessorDeletedMessage message)
    {
        var lessorId = message.Value;
        if (string.IsNullOrWhiteSpace(lessorId)) return;
        _committedLessors.Remove(lessorId);
        if (_pendingLessorId == lessorId) _pendingLessorDeleted = true;
        var wrapper = LessorsWrapper.FirstOrDefault(item => item.Person.Id.Equals(lessorId, StringComparison.Ordinal));
        if (wrapper is null) return;
        LessorsWrapper.Remove(wrapper);
        if (!_draftLessorDeletions.Any(person => person.Id.Equals(lessorId, StringComparison.Ordinal))) _draftLessorDeletions.Add(wrapper.Person);
        MarkEdited();
    }

    public void Receive(BrokerUpdatedMessage message)
    {
        var broker = message.Value;
        if (_pendingBrokerId == broker.Id) _pendingBrokerUpdate = broker;
        if (_committedBrokers.ContainsKey(broker.Id)) _committedBrokers[broker.Id] = broker;
        var wrapper = BrokersWrapper.FirstOrDefault(item => item.Person.Id.Equals(broker.Id, StringComparison.Ordinal));
        if (wrapper is not null) wrapper.Person = broker;
    }

    public void Receive(BrokerDeletedMessage message)
    {
        var brokerId = message.Value;
        if (string.IsNullOrWhiteSpace(brokerId)) return;
        _committedBrokers.Remove(brokerId);
        if (_pendingBrokerId == brokerId) _pendingBrokerDeleted = true;
        var wrapper = BrokersWrapper.FirstOrDefault(item => item.Person.Id.Equals(brokerId, StringComparison.Ordinal));
        if (wrapper is null) return;
        BrokersWrapper.Remove(wrapper);
        if (!_draftBrokerDeletions.Any(person => person.Id.Equals(brokerId, StringComparison.Ordinal))) _draftBrokerDeletions.Add(wrapper.Person);
        MarkEdited();
    }

    [RelayCommand]
    private async Task AddLessor()
    {
        if (!IsEditorEnabled) return;
        using var cts = new CancellationTokenSource();
        var selected = await _dialogService.ShowLessorSelectDialog(new ViewModels.Dialogs.LessorSelectViewModel(_dataAccessService, cts));
        if (selected is null || LessorsWrapper.Any(item => item.Person.Id.Equals(selected.Id, StringComparison.Ordinal))) return;

        _pendingLessorId = selected.Id;
        try
        {
            var result = await Task.Run(() => _dataAccessService.SelectRentLessorById(selected.Id), _cts.Token);
            if (result.IsError)
            {
                InfoBarErrorMessage = string.Join(Environment.NewLine, new[] { "貸主を読み込めませんでした。", result.Error.Title, result.Error.Message, result.Error.Description, result.Error.Operation, result.Error.MethodName }.Where(message => !string.IsNullOrWhiteSpace(message)));
                IsInfoBarErrorOpen = true;
                return;
            }

            var lessor = _pendingLessorUpdate ?? result.Person;
            if (_pendingLessorDeleted || lessor is null)
            {
                InfoBarErrorMessage = "選択した貸主を読み込めませんでした。";
                IsInfoBarErrorOpen = true;
                return;
            }
            if (LessorsWrapper.Any(item => item.Person.Id.Equals(lessor.Id, StringComparison.Ordinal))) return;

            _draftLessorDeletions.RemoveAll(person => person.Id.Equals(lessor.Id, StringComparison.Ordinal));
            LessorsWrapper.Add(new Models.Rent.Commercials.PersonWrapperForListingViewModel(lessor, this));
            MarkEdited();
        }
        finally
        {
            _pendingLessorId = null;
            _pendingLessorUpdate = null;
            _pendingLessorDeleted = false;
        }
    }

    [RelayCommand(CanExecute = nameof(CanEditLessor))]
    private async Task EditLessor(Models.Rent.Commercials.PersonWrapperForListingViewModel? wrapper)
    {
        if (wrapper?.Person is null || !IsEditorEnabled) return;
        await App.GetService<ViewModels.MainViewModel>().EditRentLessorCommand.ExecuteAsync(wrapper.Person);
    }

    private static bool CanEditLessor(Models.Rent.Commercials.PersonWrapperForListingViewModel? wrapper) => wrapper?.Person is not null;

    [RelayCommand]
    private void DeleteLessor(Models.Rent.Commercials.PersonWrapperForListingViewModel? wrapper)
    {
        if (wrapper?.Person is not { } lessor || !IsEditorEnabled || !LessorsWrapper.Remove(wrapper)) return;
        if (!_draftLessorDeletions.Any(person => person.Id.Equals(lessor.Id, StringComparison.Ordinal))) _draftLessorDeletions.Add(lessor);
        MarkEdited();
    }

    [RelayCommand]
    private async Task AddBroker()
    {
        if (!IsEditorEnabled) return;
        using var cts = new CancellationTokenSource();
        var selected = await _dialogService.ShowBrokerSelectDialog(new ViewModels.Dialogs.BrokerSelectViewModel(_dataAccessService, cts));
        if (selected is null || BrokersWrapper.Any(item => item.Person.Id.Equals(selected.Id, StringComparison.Ordinal))) return;

        _pendingBrokerId = selected.Id;
        try
        {
            var result = await Task.Run(() => _dataAccessService.SelectBrokerById(selected.Id), _cts.Token);
            if (result.IsError)
            {
                InfoBarErrorMessage = string.Join(Environment.NewLine, new[] { "宅建業者を読み込めませんでした。", result.Error.Title, result.Error.Message, result.Error.Description, result.Error.Operation, result.Error.MethodName }.Where(message => !string.IsNullOrWhiteSpace(message)));
                IsInfoBarErrorOpen = true;
                return;
            }

            var broker = _pendingBrokerUpdate ?? result.Person;
            if (_pendingBrokerDeleted || broker is null)
            {
                InfoBarErrorMessage = "選択した宅建業者を読み込めませんでした。";
                IsInfoBarErrorOpen = true;
                return;
            }
            if (BrokersWrapper.Any(item => item.Person.Id.Equals(broker.Id, StringComparison.Ordinal))) return;

            _draftBrokerDeletions.RemoveAll(person => person.Id.Equals(broker.Id, StringComparison.Ordinal));
            BrokersWrapper.Add(new Models.Rent.Commercials.PersonWrapperForListingViewModel(broker, this));
            MarkEdited();
        }
        finally
        {
            _pendingBrokerId = null;
            _pendingBrokerUpdate = null;
            _pendingBrokerDeleted = false;
        }
    }

    [RelayCommand(CanExecute = nameof(CanEditBroker))]
    private async Task EditBroker(Models.Rent.Commercials.PersonWrapperForListingViewModel? wrapper)
    {
        if (wrapper?.Person is null || !IsEditorEnabled) return;
        await App.GetService<ViewModels.MainViewModel>().EditBrokerCommand.ExecuteAsync(wrapper.Person);
    }

    private static bool CanEditBroker(Models.Rent.Commercials.PersonWrapperForListingViewModel? wrapper) => wrapper?.Person is not null;

    [RelayCommand]
    private void DeleteBroker(Models.Rent.Commercials.PersonWrapperForListingViewModel? wrapper)
    {
        if (wrapper?.Person is not { } broker || !IsEditorEnabled || !BrokersWrapper.Remove(wrapper)) return;
        if (!_draftBrokerDeletions.Any(person => person.Id.Equals(broker.Id, StringComparison.Ordinal))) _draftBrokerDeletions.Add(broker);
        MarkEdited();
    }
}
