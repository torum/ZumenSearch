using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.Messaging;
using System.Collections.ObjectModel;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using ZumenSearch.Models;
using ZumenSearch.Models.Base;
using ZumenSearch.Models.Messenger;
using ZumenSearch.Models.Rent.Commercials.Listing;
using ZumenSearch.Services.Contracts;
using ZumenSearch.Views.Services.Contracts;

namespace ZumenSearch.ViewModels.Rent.Commercials;

public sealed partial class ListingViewModel : ObservableRecipient, IRecipient<PropertyUpdatedMessage>, IRecipient<PropertyIsUnitOwnershipChangedMessage>, IRecipient<LessorUpdatedMessage>, IRecipient<LessorDeletedMessage>, IRecipient<BrokerUpdatedMessage>, IRecipient<BrokerDeletedMessage>, IDisposable
{
    private const string BasicPageName = "ZumenSearch.Views.Rent.Commercials.Listing.BasicPage";
    private const string ContractPageName = "ZumenSearch.Views.Rent.Commercials.Listing.ContractPage";
    private const string DocumentListPageName = "ZumenSearch.Views.Rent.Commercials.Listing.DocumentListPage";
    private const string LessorPageName = "ZumenSearch.Views.Rent.Commercials.Listing.LessorListPage";
    private const string BrokerPageName = "ZumenSearch.Views.Rent.Commercials.Listing.BrokerListPage";

    private readonly string _listingDataDirectoryPath = string.Empty;
    private readonly CancellationTokenSource _cts = new();
    private bool _disposed;
    private int _editVersion;
    // Saved IDs keep handed-off draft metadata independent from editor-owned media entities.
    private readonly HashSet<string> _committedPdfs = new(StringComparer.Ordinal);
    private readonly Dictionary<string, Models.Base.PersonBase> _committedLessors = new(StringComparer.Ordinal);
    private readonly Dictionary<string, Models.Base.PersonBase> _committedBrokers = new(StringComparer.Ordinal);

    private Models.Rent.Commercials.Listing.Listing _unit;

    private readonly List<string> _unsavedUnitPictureFileList = [];
    private readonly List<string> _unsavedUnitPdfFileList = [];
    private readonly List<string> _unsavedUnitPdfThumbnailFileList = [];

    private readonly IDataAccessService _dataAccessService;
    private readonly INavigationGenericService _navigationService;
    private readonly IDialogGenericService _dialogService;

    public ListingViewModel(Models.Rent.Commercials.Listing.Listing unit, INavigationGenericService navigationService, IDialogGenericService dialogService, IDataAccessService dataAccessService)
    {
        _unit = unit ?? throw new ArgumentNullException(nameof(unit));
        IsPropertyUnitOwnership = _unit.IsPropertyUnitOwnership;
        _committedPdfs.UnionWith(_unit.Pdfs.Select(pdf => pdf.Id));
        foreach (var lessor in _unit.Lessors) _committedLessors[lessor.Id] = lessor;
        foreach (var broker in _unit.Brokers) _committedBrokers[broker.Id] = broker;

        _navigationService = navigationService;
        _dialogService = dialogService;
        _dataAccessService = dataAccessService;
        _listingDataDirectoryPath = System.IO.Path.Combine(App.PropertyBlobDataFolder, _unit.PropertyId, _unit.Id);

        PopulateValues();
        PopulatePictures();
        PopulatePdfs();
        PopulateAssociations();
        var parent = App.GetService<ViewModels.MainViewModel>().RentCommercialEditorList.FirstOrDefault(editor => editor.Id == _unit.PropertyId)?.ViewModel;
        if (parent is not null)
        {
            if (parent.GetCommittedListingData(_unit.Id) is { } committedListing)
            {
                CaptureCommittedPdfs(committedListing.Pdfs);
                CaptureCommittedOwnershipAssociations(committedListing.LessorIds, committedListing.BrokerIds);
            }
        }
        IsDirty = _unit.PropertyStatus == EntityStatus.Saved && (HasPendingPdfEdits() || HasPendingAssociationEdits());

#pragma warning disable IL3050 // Disable AOT dynamic code warning
#pragma warning disable IL2026 // Disable Trimming unreferenced code warning
        IsActive = true;
#pragma warning restore IL2026
#pragma warning restore IL3050
    }

    #region == Properties ==

    public string WindowTitle
    {
        get
        {
            var title = $"{field}：{_unit.PropertyName}";
            if (!string.IsNullOrWhiteSpace(Name))
            {
                title += $"：{Name}";
            }

            return $"{title}：{(_unit.Status == EntityStatus.New ? "新規" : "編集")}";
        }
        set => OnPropertyChanged();
    } = "賃貸事業用";

    public ObservableCollection<Breadcrumb> BreadcrumbItems { get; } =
    [
        new()
        {
            Name = "募集物件",
            Page = BasicPageName
        },
        new()
        {
            Name = "基本",
            Page = BasicPageName
        }
    ];

    [ObservableProperty]
    [NotifyCanExecuteChangedFor(nameof(SaveCommand))]
    public partial bool IsDirty { get; private set; }

    [ObservableProperty]
    public partial bool IsInfoBarErrorOpen { get; set; }

    [ObservableProperty]
    public partial string InfoBarErrorMessage { get; set; } = string.Empty;

    [ObservableProperty]
    public partial bool IsNameHasError { get; private set; }

    [ObservableProperty]
    public partial bool IsPropertyUnitOwnership { get; private set; }

    public string Name
    {
        get => field ?? string.Empty;
        set
        {
            var name = value?.Trim() ?? string.Empty;
            if (SetProperty(ref field, name))
            {
                IsNameHasError = false;
                WindowTitle = string.Empty;
                MarkEdited();
            }
        }
    }

    [ObservableProperty] public partial string Chinryou { get; set; } = string.Empty;
    [ObservableProperty] public partial string KyouekiFee { get; set; } = string.Empty;
    [ObservableProperty] public partial string Shikikin { get; set; } = string.Empty;
    [ObservableProperty] public partial string ShikikinUnit { get; set; } = "ヵ月";
    [ObservableProperty] public partial string Reikin { get; set; } = string.Empty;
    [ObservableProperty] public partial string ReikinUnit { get; set; } = "ヵ月";
    [ObservableProperty] public partial string RenewalFeeUnit { get; set; } = "ヵ月";
    [ObservableProperty] public partial string RecontractFeeUnit { get; set; } = "円";
    [ObservableProperty] public partial string FloorArea { get; set; } = string.Empty;
    [ObservableProperty] public partial string FloorNumber { get; set; } = string.Empty;
    [ObservableProperty] public partial string UseDetails { get; set; } = string.Empty;
    [ObservableProperty] public partial bool IsAvailableForRentNow { get; set; }
    [ObservableProperty] public partial bool IsAvailableForImmediateMoveIn { get; set; }
    [ObservableProperty] public partial ListingFloorGroundTypeLabel SelectedFloorGroundType { get; set; } = new(ListingFloorGroundType.AboveGround);
    [ObservableProperty] public partial ListingUseTypeLabel SelectedUseType { get; set; } = new(ListingUseType.Unspecified);
    [ObservableProperty] public partial ListingAvailabilityStatusTypeLabel SelectedAvailabilityStatusType { get; set; } = new(ListingAvailabilityStatusType.Unspecified);
    [ObservableProperty] public partial ListingAvailabilityMonthTypeLabel SelectedAvailabilityMonthType { get; set; } = new(ListingAvailabilityMonthType.Unspecified);
    [ObservableProperty] public partial ListingAvailabilityPeriodTypeLabel SelectedAvailabilityPeriodType { get; set; } = new(ListingAvailabilityPeriodType.Unspecified);
    public IReadOnlyList<ListingFloorGroundTypeLabel> FloorGroundTypes { get; } = ListingFloorGroundTypeLabel.GetAll();
    public IReadOnlyList<ListingUseTypeLabel> UseTypes { get; } = ListingUseTypeLabel.GetAll();
    public IReadOnlyList<ListingAvailabilityStatusTypeLabel> AvailabilityStatusTypes { get; } = ListingAvailabilityStatusTypeLabel.GetAll();
    public IReadOnlyList<ListingAvailabilityMonthTypeLabel> AvailabilityMonthTypes { get; } = ListingAvailabilityMonthTypeLabel.GetAll();
    public IReadOnlyList<ListingAvailabilityPeriodTypeLabel> AvailabilityPeriodTypes { get; } = ListingAvailabilityPeriodTypeLabel.GetAll();
    [ObservableProperty] public partial string BusinessHours { get; set; } = string.Empty;
    [ObservableProperty] public partial bool ParkingAvailable { get; set; }
    [ObservableProperty] public partial string OtherConditions { get; set; } = string.Empty;
    [ObservableProperty] public partial string Remarks { get; set; } = string.Empty;

    partial void OnChinryouChanged(string value) => MarkEdited();
    partial void OnFloorAreaChanged(string value) => MarkEdited();
    partial void OnFloorNumberChanged(string value) => MarkEdited();
    partial void OnUseDetailsChanged(string value) => MarkEdited();
    partial void OnIsAvailableForRentNowChanged(bool value) => MarkEdited();
    partial void OnIsAvailableForImmediateMoveInChanged(bool value) => MarkEdited();
    partial void OnSelectedFloorGroundTypeChanged(ListingFloorGroundTypeLabel value) => MarkEdited();
    partial void OnSelectedUseTypeChanged(ListingUseTypeLabel value) => MarkEdited();
    partial void OnSelectedAvailabilityStatusTypeChanged(ListingAvailabilityStatusTypeLabel value) => MarkEdited();
    partial void OnSelectedAvailabilityMonthTypeChanged(ListingAvailabilityMonthTypeLabel value) => MarkEdited();
    partial void OnSelectedAvailabilityPeriodTypeChanged(ListingAvailabilityPeriodTypeLabel value) => MarkEdited();
    partial void OnRemarksChanged(string value) => MarkEdited();

    private void MarkEdited()
    {
        _editVersion++;
        IsDirty = true;
    }

    private void MarkPdfEdited()
    {
        _editVersion++;
        IsDirty = true;
    }

    #endregion

    #region == Public Methods ==

    public void Receive(PropertyIsUnitOwnershipChangedMessage message)
    {
        if (!string.Equals(message.PropertyId, _unit.PropertyId, StringComparison.Ordinal)) return;
        if (DeferPropertyMessage(() => Receive(message))) return;
        SetOwnershipVisibility(message.Value);
    }

    public void SetOwnershipVisibility(bool isUnitOwnership)
    {
        IsPropertyUnitOwnership = isUnitOwnership;
        if (!isUnitOwnership && (_navigationService.IsCurrentPageSameAs(DocumentListPageName) || _navigationService.IsCurrentPageSameAs(LessorPageName) || _navigationService.IsCurrentPageSameAs(BrokerPageName)))
        {
            _navigationService.NavigateTo(BasicPageName, this);
        }
    }

    public void Receive(PropertyUpdatedMessage message)
    {
        if (message.Value is not Models.Rent.Commercials.Property property || !property.Id.Equals(_unit.PropertyId, StringComparison.Ordinal))
        {
            return;
        }
        var status = property.Status;
        var isUnitOwnership = property.IsUnitOwnership;
        var propertyName = property.Name;
        var parent = App.GetService<ViewModels.MainViewModel>().RentCommercialEditorList.FirstOrDefault(editor => editor.Id == _unit.PropertyId)?.ViewModel;
        var isListed = property.ListingIds.Contains(_unit.Id, StringComparer.Ordinal);
        var committedListing = isListed ? parent?.GetCommittedListingData(_unit.Id) : null;
        if (DeferPropertyMessage(() => ApplyPropertyUpdate(status, isUnitOwnership, propertyName, committedListing))) return;
        ApplyPropertyUpdate(status, isUnitOwnership, propertyName, committedListing);
    }

    private void ApplyPropertyUpdate(EntityStatus status, bool isUnitOwnership, string propertyName, Models.Rent.Commercials.CommercialListingCommittedData? committedListing)
    {
        _unit.SetPropertyStatus(status);
        _unit.IsPropertyUnitOwnership = isUnitOwnership;
        if (status == EntityStatus.Saved)
        {
            if (committedListing is { } committedData)
            {
                CaptureCommittedPdfs(committedData.Pdfs);
                CaptureCommittedOwnershipAssociations(committedData.LessorIds, committedData.BrokerIds);
                var committedFiles = committedData.Pdfs.SelectMany(pdf => new[] { Path.Combine(_listingDataDirectoryPath, pdf.PdfFilename), Path.Combine(_listingDataDirectoryPath, pdf.ThumbnailFilename) }).ToHashSet(StringComparer.OrdinalIgnoreCase);
                _unsavedUnitPdfFileList.RemoveAll(committedFiles.Contains);
                _unsavedUnitPdfThumbnailFileList.RemoveAll(committedFiles.Contains);
            }
        }

        var parent = App.GetService<ViewModels.MainViewModel>().RentCommercialEditorList.FirstOrDefault(editor => editor.Id == _unit.PropertyId);
        var draftOwnership = parent?.ViewModel?.IsUnitOwnership ?? isUnitOwnership;
        SetOwnershipVisibility(draftOwnership);
        if (status == EntityStatus.Saved && committedListing is not null)
        {
            _unit.SetStatus(EntityStatus.Saved);
            _unit.PicturesToBeDeleted.RemoveAll(picture => _handedOffPictureDeletions.Contains(picture.Id));
            _handedOffPictureDeletions.Clear();
            _handedOffToNewParent = false;
        }
        if (!string.Equals(_unit.PropertyName, propertyName, StringComparison.Ordinal))
        {
            _unit.SetPropertyName(propertyName);
        }

        if (status == EntityStatus.Saved && committedListing is null && _handedOffToNewParent && _unit.Status == EntityStatus.New)
        {
            IsDirty = true;
            InfoBarErrorMessage = "建物の保存に募集物件が含まれていません。内容を確認してから募集物件を保存してください。";
            IsInfoBarErrorOpen = true;
        }
        else if (status == EntityStatus.Saved && HasPendingAssociationEdits())
        {
            IsDirty = true;
            InfoBarErrorMessage = "募集物件の貸主・宅建業者の変更は未保存です。募集物件を保存してください。";
            IsInfoBarErrorOpen = true;
        }
        WindowTitle = string.Empty;
    }

    public void Dispose()
    {
        if (_disposed)
        {
            return;
        }

        _disposed = true;
        foreach (var picture in Pictures) picture.PropertyChanged -= OnListingPictureChanged;
        foreach (var pdf in Pdfs) pdf.PropertyChanged -= OnListingPdfChanged;

#pragma warning disable IL3050 // Disable AOT dynamic code warning
#pragma warning disable IL2026 // Disable Trimming unreferenced code warning
        IsActive = false;
#pragma warning restore IL2026
#pragma warning restore IL3050

        _cts.Cancel();
        _cts.Dispose();

        GC.SuppressFinalize(this);
    }

    #endregion

    #region == Private Methods ==

    private void PopulateValues()
    {
        Name = _unit.Name;
        Chinryou = Format(_unit.Chinryou);
        FloorNumber = _unit.FloorNumber?.ToString(CultureInfo.InvariantCulture) ?? string.Empty;
        FloorArea = _unit.FloorArea?.ToString(CultureInfo.CurrentCulture) ?? string.Empty;
        UseDetails = _unit.UseDetails;
        IsAvailableForRentNow = _unit.IsAvailableForRentNow;
        IsAvailableForImmediateMoveIn = _unit.IsAvailableForImmediateMoveIn;
        SelectedFloorGroundType = FloorGroundTypes.First(option => option.Key == _unit.FloorGround.Key);
        SelectedUseType = UseTypes.First(option => option.Key == _unit.Use.Key);
        SelectedAvailabilityStatusType = AvailabilityStatusTypes.First(option => option.Key == _unit.AvailabilityStatus.Key);
        SelectedAvailabilityMonthType = AvailabilityMonthTypes.First(option => option.Key == _unit.AvailabilityMonth.Key);
        SelectedAvailabilityPeriodType = AvailabilityPeriodTypes.First(option => option.Key == _unit.AvailabilityPeriod.Key);
        PopulateContractValues();
        PopulateTransactionValues();
        PopulateFacilitiesValues();
        /*
        KyouekiFee = Format(_unit.KyouekiFee);
        Shikikin = Format(_unit.Shikikin);
        ShikikinUnit = _unit.ShikikinUnit;
        Reikin = Format(_unit.Reikin);
        ReikinUnit = _unit.ReikinUnit;
        RenewalFee = Format(_unit.RenewalFee);
        RenewalFeeUnit = _unit.RenewalFeeUnit;
        RecontractFee = Format(_unit.RecontractFee);
        RecontractFeeUnit = _unit.RecontractFeeUnit;
        FloorArea = Format(_unit.FloorArea);
        Usage = _unit.Usage;
        BusinessHours = _unit.BusinessHours;
        ParkingAvailable = _unit.ParkingAvailable;
        OtherConditions = _unit.OtherConditions;
        */
        Remarks = _unit.Remarks;
        WindowTitle = string.Empty;
    }

    private bool ValidateName()
    {
        if (string.IsNullOrWhiteSpace(Name))
        {
            InfoBarErrorMessage = "区画名（必須項目）が入力されていません。保存出来ませんでした。";
            IsNameHasError = true;
            return false;
        }

        if (new StringInfo(Name).LengthInTextElements > 100)
        {
            InfoBarErrorMessage = "区画名は100文字以内で入力してください。";
            IsNameHasError = true;
            return false;
        }

        IsNameHasError = false;
        return true;
    }

    private static string Format(decimal value) =>
        value == 0
            ? string.Empty
            : value.ToString(CultureInfo.CurrentCulture);

    private static bool TryParseRent(string? value, out decimal rent)
    {
        var normalized = Helpers.Common.ReplaceZenkakuNumbers(value ?? string.Empty).Trim();
        if (normalized.Length == 0)
        {
            rent = 0;
            return true;
        }

        return decimal.TryParse(normalized, NumberStyles.Number, CultureInfo.CurrentCulture, out rent) && rent >= 0;
    }

    #endregion

    #region == Commands ==

    [RelayCommand(CanExecute = nameof(CanSave))]
    private async Task Save()
    {
        if (!IsDirty || _isSaving || _discardPending) return;
        if (HasPendingEditorOperations)
        {
            ShowPictureError("実行中の選択またはファイル処理が完了してから、もう一度保存してください。");
            return;
        }
        if (!CleanIncompleteAttachments()) return;

        var mainViewModel = App.GetService<ViewModels.MainViewModel>();
        var parent = mainViewModel.RentCommercialEditorList.FirstOrDefault(editor => editor.Id == _unit.PropertyId);
        if (parent?.ViewModel is { HasPendingEditorOperations: true } || mainViewModel.RentCommercialListingEditorList.Any(editor => editor.PropertyId == _unit.PropertyId && editor.ViewModel != this && editor.ViewModel.HasPendingEditorOperations))
        {
            ShowPictureError("関連する選択またはファイル処理が完了してから、もう一度保存してください。");
            return;
        }
        if (parent?.ViewModel is { } parentViewModel && (parentViewModel.IsDiscardPending || parentViewModel.IsListingPendingDeletion(_unit.Id) || parentViewModel.HasPendingFileCleanupAfterSave))
        {
            InfoBarErrorMessage = "建物の変更の破棄、削除またはファイルの後処理を完了してから、もう一度保存してください。";
            IsInfoBarErrorOpen = true;
            return;
        }
        if (!mainViewModel.TryBeginCommercialOperation(_unit.PropertyId))
        {
            InfoBarErrorMessage = "建物または募集物件の保存・削除が完了してから、もう一度保存してください。";
            IsInfoBarErrorOpen = true;
            return;
        }

        _isSaving = true;
        OnPropertyChanged(nameof(IsEditorEnabled));
        try
        {
            await SaveCore();
        }
        finally
        {
            _isSaving = false;
            OnPropertyChanged(nameof(IsEditorEnabled));
            mainViewModel.EndCommercialOperation(_unit.PropertyId);
            if (!_propertyBusy) ApplyDeferredPropertyMessages();
        }
    }

    private async Task SaveCore()
    {
        if (!ValidateName())
        {
            IsInfoBarErrorOpen = true;
            _navigationService.NavigateTo(BasicPageName, this);
            return;
        }

        var floorText = Helpers.Common.NormalizeFullWidthCharacters(FloorNumber.Trim());
        int? floorNumber = null;
        if (floorText.Length > 0)
        {
            if (!int.TryParse(floorText, NumberStyles.Integer, CultureInfo.InvariantCulture, out var parsedFloor) || parsedFloor <= 0)
            {
                InfoBarErrorMessage = "所在階は1以上の整数で入力してください。";
                IsInfoBarErrorOpen = true;
                _navigationService.NavigateTo(BasicPageName, this);
                return;
            }
            floorNumber = parsedFloor;
        }
        var areaText = Helpers.Common.NormalizeFullWidthCharacters(FloorArea.Trim());
        decimal? floorArea = null;
        if (areaText.Length > 0)
        {
            if (!decimal.TryParse(areaText, NumberStyles.Number, CultureInfo.CurrentCulture, out var parsedArea) || parsedArea < 0)
            {
                InfoBarErrorMessage = "専有面積は0以上の数値で入力してください。";
                IsInfoBarErrorOpen = true;
                _navigationService.NavigateTo(BasicPageName, this);
                return;
            }
            floorArea = parsedArea;
        }
        if (SelectedFloorGroundType is null || SelectedUseType is null || SelectedAvailabilityStatusType is null || SelectedAvailabilityMonthType is null || SelectedAvailabilityPeriodType is null)
        {
            InfoBarErrorMessage = "基本情報の選択項目を確認してください。";
            IsInfoBarErrorOpen = true;
            _navigationService.NavigateTo(BasicPageName, this);
            return;
        }
        if (!TryParseRent(Chinryou, out var rent))
        {
            InfoBarErrorMessage = "賃料には0以上の数値を入力してください。";
            IsInfoBarErrorOpen = true;
            _navigationService.NavigateTo(ContractPageName, this);
            return;
        }

        if (Pictures.Count(picture => picture.IsMain) > 1)
        {
            ShowPictureError("メインの写真は1枚だけ選択してください。");
            _navigationService.NavigateTo("ZumenSearch.Views.Rent.Commercials.Listing.PictureListPage", this);
            return;
        }
        if (HasMultipleMainPdfs)
        {
            ReportMultipleMainPdfs();
            return;
        }
        var version = _editVersion;
        var unit = _unit;
        unit.IsPropertyUnitOwnership = IsPropertyUnitOwnership;
        unit.Pictures = Pictures;
        unit.Pdfs = Pdfs;
        var stagedPictures = _unsavedUnitPictureFileList.ToList();
        var stagedPdfs = _unsavedUnitPdfFileList.ToList();
        var stagedPdfThumbnails = _unsavedUnitPdfThumbnailFileList.ToList();
        var deletedPdfs = unit.PdfsToBeDeleted.ToList();
        var savedLessorDeletions = _draftLessorDeletions.ToList();
        var savedBrokerDeletions = _draftBrokerDeletions.ToList();
        if (!SetContractValues(unit) || !SetTransactionValues(unit)) return;
        SetFacilitiesValues(unit);
        unit.SetName(Name);
        unit.SetChinryou(rent);
        unit.SetFloorGroundTypeLabel(SelectedFloorGroundType);
        unit.SetFloorNumber(floorNumber);
        unit.SetFloorArea(floorArea);
        unit.SetUseTypeLabel(SelectedUseType);
        unit.SetUseDetails(UseDetails);
        unit.SetAvailabilityStatusTypeLabel(SelectedAvailabilityStatusType);
        unit.SetIsAvailableForRentNow(IsAvailableForRentNow);
        unit.SetAvailabilityMonthTypeLabel(SelectedAvailabilityMonthType);
        unit.SetAvailabilityPeriodTypeLabel(SelectedAvailabilityPeriodType);
        unit.SetIsAvailableForImmediateMoveIn(IsAvailableForImmediateMoveIn);
        unit.SetRemarks(Remarks);

        bool saveResult;
        var directDatabaseSave = false;

        if (unit.PropertyStatus == EntityStatus.New)
        {
            var parent = App.GetService<ViewModels.MainViewModel>().RentCommercialEditorList.FirstOrDefault(editor => editor.Id.Equals(unit.PropertyId, StringComparison.Ordinal) && editor.ViewModel.IsActive);
            if (parent is null)
            {
                InfoBarErrorMessage = "建物の編集画面が開いていないため、募集物件を保存できません。";
                IsInfoBarErrorOpen = true;
                return;
            }
            if (parent.ViewModel.IsSaving)
            {
                InfoBarErrorMessage = "建物の保存が完了してから、募集物件を保存してください。";
                IsInfoBarErrorOpen = true;
                return;
            }

            ApplyAssociationDraft();
            _handedOffToNewParent = true;
            _handedOffPictureDeletions.Clear();
            _handedOffPictureDeletions.UnionWith(unit.PicturesToBeDeleted.Select(picture => picture.Id));
            saveResult = true;
        }
        else
        {
            var previousLessors = unit.Lessors;
            var previousBrokers = unit.Brokers;
            var previousLessorDeletions = unit.LessorsToBeDeleted.ToList();
            var previousBrokerDeletions = unit.BrokersToBeDeleted.ToList();
            ApplyAssociationDraft();
            Models.SearchResult.ResultWrapper result;
            var committed = false;
            try
            {
                result = await Task.Run(() => _dataAccessService.UpsertRentCommercialListing(unit.PropertyId, unit), _cts.Token);
                committed = !result.IsError;
            }
            finally
            {
                if (!committed)
                {
                    unit.Lessors = previousLessors;
                    unit.Brokers = previousBrokers;
                    unit.LessorsToBeDeleted.Clear();
                    foreach (var person in previousLessorDeletions) unit.LessorsToBeDeleted.Add(person);
                    unit.BrokersToBeDeleted.Clear();
                    foreach (var person in previousBrokerDeletions) unit.BrokersToBeDeleted.Add(person);
                }
            }

            if (result.IsError)
            {
                InfoBarErrorMessage = string.Join(
                    Environment.NewLine,
                    new[]
                    {
                result.Error.Title,
                result.Error.Message,
                result.Error.Description,
                result.Error.Operation,
                result.Error.MethodName
                    }.Where(message => !string.IsNullOrWhiteSpace(message)));

                IsInfoBarErrorOpen = true;

                saveResult = false;
            }
            else
            {
                directDatabaseSave = true;
                unit.SetStatus(EntityStatus.Saved);
                unit.SetIsModified(false);
                saveResult = true;
            }

        }

        if (saveResult)
        {
            foreach (var person in savedLessorDeletions) _draftLessorDeletions.Remove(person);
            foreach (var person in savedBrokerDeletions) _draftBrokerDeletions.Remove(person);
            unit.SetPropertyStatus(_unit.PropertyStatus);
            if (!string.Equals(unit.PropertyName, _unit.PropertyName, StringComparison.Ordinal))
            {
                unit.SetPropertyName(_unit.PropertyName);
            }
            if (directDatabaseSave)
            {
                CaptureCommittedPdfs(unit.Pdfs);
                CaptureCommittedOwnershipAssociations(unit.Lessors, unit.Brokers);
            }
            IsDirty = _editVersion != version;

            // Clear error infobar.
            IsInfoBarErrorOpen = false;

            // Update title with dummy value.
            WindowTitle = string.Empty;

            foreach (var file in stagedPictures) _unsavedUnitPictureFileList.Remove(file);
            // A new-parent handoff transfers these files to the parent's listing draft, not to the database.
            foreach (var file in stagedPdfs) _unsavedUnitPdfFileList.Remove(file);
            foreach (var file in stagedPdfThumbnails) _unsavedUnitPdfThumbnailFileList.Remove(file);
            if (unit.PropertyStatus == EntityStatus.New)
            {
                // The parent owns this draft until the new property is saved.
                WeakReferenceMessenger.Default.Send(new ListingUpdatedMessage(_unit));
            }
            else
            {
                var pictureFailures = new List<string>();
                foreach (var picture in _unit.PicturesToBeDeleted.ToList())
                {
                    if (DeletePictureFiles(new[] { Path.Combine(_listingDataDirectoryPath, picture.ImageFilename) }, pictureFailures)) _unit.PicturesToBeDeleted.Remove(picture);
                }
                _hasPendingPictureCleanup = pictureFailures.Count > 0;

                var pdfFailures = new List<string>();
                foreach (var pdf in deletedPdfs)
                {
                    var files = new[]
                    {
                        Path.Combine(_listingDataDirectoryPath, pdf.PdfFilename),
                        Path.Combine(_listingDataDirectoryPath, pdf.ThumbnailFilename)
                    };
                    if (DeleteAttachmentFiles(files, pdfFailures)) _unit.PdfsToBeDeleted.Remove(pdf);
                    else if (!_unit.PdfsToBeDeleted.Any(item => item.Id.Equals(pdf.Id, StringComparison.Ordinal))) _unit.PdfsToBeDeleted.Add(pdf);
                }
                _hasPendingPdfCleanup = pdfFailures.Count > 0;
                if (HasPendingFileCleanupAfterSave)
                {
                    IsDirty = true;
                    ShowAttachmentError($"保存しましたが、一部の削除済み添付ファイルを削除できませんでした。次回の保存時に再試行します: {string.Join(", ", pictureFailures.Concat(pdfFailures))}");
                }

                // Refresh the property window and search results with the shared listing.
                WeakReferenceMessenger.Default.Send(new ListingUpdatedMessage(_unit));
            }
        }

    }

    private bool CanSave() => IsDirty;

    [RelayCommand]
    private void OpenPropertyEditorWindow()
    {
        var vm = App.GetService<ViewModels.MainViewModel>();

        if (vm.EditRentCommercialFromIdCommand.CanExecute(_unit.PropertyId))
        {
            //await vm.EditRentCommercialFromIdCommand(_unit.PropertyId);
            vm.EditRentCommercialFromIdCommand.Execute(_unit.PropertyId);
        }
    }

    #endregion

}