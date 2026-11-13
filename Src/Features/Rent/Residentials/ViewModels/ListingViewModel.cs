using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.Messaging;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Globalization;
using Windows.Data.Pdf;
using Windows.Storage;
using Windows.Storage.Streams;
using ZumenSearch.Models;
using ZumenSearch.Models.Rent.Residentials;
using ZumenSearch.Models.Messenger;
using ZumenSearch.Services.Contracts;
using ZumenSearch.Views.Services.Contracts;

namespace ZumenSearch.ViewModels.Rent.Residentials;

public sealed partial class ListingViewModel : ObservableRecipient, 
    IRecipient<PropertyUpdatedMessage>, 
    IRecipient<PropertyIsUnitOwnershipChangedMessage>,
    IRecipient<LessorUpdatedMessage>,
    IRecipient<BrokerUpdatedMessage>,
    IRecipient<LessorDeletedMessage>,
    IRecipient<BrokerDeletedMessage>, 
    IDisposable
{
    #region == Private variables ==

    private const string BasicPageName = "ZumenSearch.Views.Rent.Residentials.Listing.BasicPage";
    private readonly string _id = string.Empty;

    // This property holds the COPY of current entity being edited.
    // Do not use it directly in the UI. Apply changes to this object in Save() to save the changes.
    private readonly Models.Rent.Residentials.Listing _room;

    private readonly string _listingDataDirectoryPath = string.Empty;

    // Tmp file list to hold unsaved picture files. (if entry is not saved, delete on close)
    private readonly List<string> _unsavedRoomPictureFileList = [];
    private readonly List<string> _unsavedRoomPdfFileList = [];
    private readonly List<string> _unsavedRoomPdfThumbnailFileList = [];
    private readonly List<string> _pendingRoomPdfCleanupFiles = [];
    private readonly List<string> _incompleteAttachmentFiles = [];
    private readonly HashSet<string> _committedAttachmentPaths = new(StringComparer.OrdinalIgnoreCase);
    private readonly List<Models.Base.PersonBase> _draftLessorDeletions = [];
    private readonly List<Models.Base.PersonBase> _draftBrokerDeletions = [];
    private int _ownershipVersion;
    private bool _hasPendingFileCleanupAfterSave;
    private bool _discardPending;
    private bool _handedOffToNewParent;
    private bool _pendingHandoffReady;
    private bool _disposed;
    private bool _isClosing;
    private bool _propertyBusy;
    private int _pendingPickers;
    private TaskCompletionSource? _pickersCompleted;
    private readonly Queue<Action> _deferredMessages = new();

    private readonly CancellationTokenSource _cts = new();

    #endregion

    #region == Services ==

    private readonly IDataAccessService _dataAccessService;
    private readonly IDispatcherService _dispatcherService;
    private readonly IDialogGenericService? _dialogService;
    private readonly INavigationGenericService _navigationService;

    #endregion

    public ListingViewModel(
        Models.Rent.Residentials.Listing room, 
        INavigationGenericService navigationService,
        IDialogGenericService dialogService,
        IDispatcherService dispatcherService, 
        IDataAccessService dataAccessService)
    {
        _room = room;
        _id = room.Id;
        var parent = App.GetService<ViewModels.MainViewModel>().RentResidentialEditorList.FirstOrDefault(editor => editor.Id == room.PropertyId)?.ViewModel;
        _handedOffToNewParent = _room.Status == EntityStatus.New && parent?.GetPendingListingDraft(room.Id) is not null;

        _navigationService = navigationService; // _navigationService.NavigateTo("ZumenSearch.Views.Rent.Residentials.RoomListPage", this, new DrillInNavigationTransitionInfo());
        _dialogService = dialogService;

        _dispatcherService = dispatcherService;
        _dataAccessService = dataAccessService;

        _listingDataDirectoryPath = System.IO.Path.Combine(App.PropertyBlobDataFolder, _room.PropertyId, _room.Id);
        if (_room.Status != EntityStatus.New) CaptureCommittedAttachments(ResidentialListingDraft.FromListing(_room));

        PopulateValues();

        // Reset errors
        IsNameHasError = false;

        //HasErrors = false;

        _room.SetIsModified(false);
        IsDirty = false;

        // Ready to receive messages.
#pragma warning disable IL3050 // Disable AOT dynamic code warning
#pragma warning disable IL2026 // Disable Trimming unreferenced code warning
        IsActive = true;
#pragma warning restore IL2026
#pragma warning restore IL3050
    }

    #region == Properties ==

    #region == Internal flags ==

    [ObservableProperty]
    [NotifyCanExecuteChangedFor(nameof(SaveCommand))]
    public partial bool IsDirty { get; private set; }

    public bool IsPropertyUnitOwnership
    {
        get;
        private set
        {
            if (SetProperty(ref field, value))
            {
                //
            }
        }
    }

    #endregion

    #region == Window and other page related stuff ==

    public string WindowTitle
    {
        get
        {
            var str = $"{field}：{_room.PropertyName}";

            if (!string.IsNullOrEmpty(Name))
            {
                str = $"{str}：{Name}";
            }

            if (_room.Status == EntityStatus.New)
            {
                str = $"{str}：新規";
            }
            else
            {
                str = $"{str}：編集";
            }
            /*
            if (string.IsNullOrEmpty(_room.PropertyName))
            {
                string str;

                if (_room.ListingStatus == EnumListingStatus.New)
                {
                    str = $"{field} (新規)";
                }
                else
                {
                    str = $"{field} (編集)";
                }

                return str;
            }
            else
            {
                //var str = $"{field}：{_room.PropertyName}";

                if (!string.IsNullOrEmpty(Name))
                {
                    str = $"{str}：{Name}";
                }

                if (_room.ListingStatus == EnumListingStatus.New)
                {
                    str = $"{str}：(新規)";
                }
                else
                {
                    str = $"{str}：(編集)";
                }

            }
            */

            return str;
        }
        set
        {
            OnPropertyChanged();
        }
    } = "賃貸住居用";

    public ObservableCollection<Breadcrumb> BreadcrumbItems { get; set; } =
    [
        new() { Name = "募集物件", Page = typeof(Views.Rent.Residentials.Listing.BasicPage).FullName! },
        new() { Name = "基本", Page = typeof(Views.Rent.Residentials.Listing.BasicPage).FullName! }
    ];

    #endregion

    #region == Error related ==
    
    /*
    [ObservableProperty]
    public partial bool HasErrors { get; private set; }
    */

    [ObservableProperty]
    public partial bool IsInfoBarErrorOpen { get; set; }

    [ObservableProperty]
    public partial string InfoBarErrorMessage { get; set; } = string.Empty;

    [ObservableProperty]
    public partial bool IsNameHasError { get; private set; }

    #endregion

    #region == 基本情報 == 

    public string Name
    {
        get => field ?? string.Empty; // Ensure a non-null value is returned
        set
        {
            if (SetProperty(ref field, value.Trim()))
            {
                IsDirty = true;

                ValidateName();

                // Update title with dummy value.
                WindowTitle = string.Empty;
            }
        }
    }

    public IReadOnlyList<ListingFloorPlanTypeLabel> FloorPlanTypes { get; } = ListingFloorPlanTypeLabel.GetAll();

    public ListingFloorPlanTypeLabel SelectedFloorPlanType
    {
        get => field ?? new(FloorPlanType.Unspecified);
        set
        {
            if (SetProperty(ref field, value))
            {
                IsDirty = true;
            }
        }
    }

    public IReadOnlyList<ListingFloorPlanExtraTypeLabel> FloorPlanExtraTypes { get; } = ListingFloorPlanExtraTypeLabel.GetAll();

    public ListingFloorPlanExtraTypeLabel SelectedFloorPlanExtraType
    {
        get => field ?? new(FloorPlanExtraType.Unspecified);
        set
        {
            if (SetProperty(ref field, value))
            {
                IsDirty = true;
            }
        }
    }

    public IReadOnlyList<ListingFloorGroundTypeLabel> FloorGroundTypes { get; } = ListingFloorGroundTypeLabel.GetAll();

    public ListingFloorGroundTypeLabel SelectedFloorGroundType
    {
        get => field ?? new(FloorGroundType.AboveGround);
        set
        {
            if (SetProperty(ref field, value))
            {
                IsDirty = true;
            }
        }
    }

    public string FloorExclusiveAreaSqm
    {
        get => field ?? string.Empty; // Ensure a non-null value is returned
        set
        {
            if (SetProperty(ref field, value.Trim()))
            {
                IsDirty = true;
            }
        }
    }

    public string FloorNumber
    {
        get => field ?? string.Empty; // Ensure a non-null value is returned
        set
        {
            if (SetProperty(ref field, value.Trim()))
            {
                IsDirty = true;
            }
        }
    }

    public bool IsCornerRoom
    {
        get;
        set
        {
            if (SetProperty(ref field, value))
            {
                IsDirty = true;
            }
        }
    }

    public IReadOnlyList<ListingExposureDirectionTypeLabel> ExposureDirectionTypes { get; } = ListingExposureDirectionTypeLabel.GetAll();

    public ListingExposureDirectionTypeLabel SelectedExposureDirectionType
    {
        get => field ?? new(ExposureDirectionType.Unspecified);
        set
        {
            if (SetProperty(ref field, value))
            {
                IsDirty = true;
            }
        }
    }

    public IReadOnlyList<ListingAvailabilityStatusTypeLabel> AvailabilityStatusTypes { get; } = ListingAvailabilityStatusTypeLabel.GetAll();

    public ListingAvailabilityStatusTypeLabel SelectedAvailabilityStatusType
    {
        get => field ?? new(AvailabilityStatusType.Unspecified);
        set
        {
            if (SetProperty(ref field, value))
            {
                IsDirty = true;
            }
        }
    }

    public IReadOnlyList<ListingAvailabilityMonthTypeLabel> AvailabilityMonthTypes { get; } = ListingAvailabilityMonthTypeLabel.GetAll();

    public ListingAvailabilityMonthTypeLabel SelectedAvailabilityMonthType
    {
        get => field ?? new(AvailabilityMonthType.Unspecified);
        set
        {
            if (SetProperty(ref field, value))
            {
                IsDirty = true;
            }
        }
    }

    public IReadOnlyList<ListingAvailabilityPeriodTypeLabel> AvailabilityPeriodTypes { get; } = ListingAvailabilityPeriodTypeLabel.GetAll();

    public ListingAvailabilityPeriodTypeLabel SelectedAvailabilityPeriodType
    {
        get => field ?? new(AvailabilityPeriodType.Unspecified);
        set
        {
            if (SetProperty(ref field, value))
            {
                IsDirty = true;
            }
        }
    }

    public IReadOnlyList<ListingOccupancyStatusLabel> OccupancyStatusTypes { get; } = ListingOccupancyStatusLabel.GetAll();

    public ListingOccupancyStatusLabel SelectedOccupancyStatus
    {
        get => field ?? new(OccupancyStatus.Unspecified);
        set
        {
            if (SetProperty(ref field, value))
            {
                IsDirty = true;
            }
        }
    }

    public bool IsAvailableForImmediateMoveIn
    {
        get;
        set
        {
            if (SetProperty(ref field, value))
            {
                IsDirty = true;
            }
        }
    }

    public bool IsAvailableForRentNow
    {
        get;
        set
        {
            if (SetProperty(ref field, value))
            {
                IsDirty = true;
            }
        }
    }

    public DateTimeOffset? OccupancyStatusCheckedAt
    {
        get;
        set
        {
            if (SetProperty(ref field, value))
            {
                IsDirty = true;
            }
        }
    }

    public string Remarks
    {
        get => field ?? string.Empty;
        set
        {
            if (SetProperty(ref field, value))
            {
                IsDirty = true;
            }
        }
    }

    #endregion

    #region == 設備 ==

    public bool HasAirConditioner { get; set => SetContractValue(ref field, value); }
    public bool HasWoodFlooring { get; set => SetContractValue(ref field, value); }
    public bool HasVideoIntercom { get; set => SetContractValue(ref field, value); }
    public bool HasBathReheating { get; set => SetContractValue(ref field, value); }
    public bool HasGasStove { get; set => SetContractValue(ref field, value); }
    public bool HasInductionCooktop { get; set => SetContractValue(ref field, value); }
    public bool HasMultipleBurners { get; set => SetContractValue(ref field, value); }
    public bool HasSystemKitchen { get; set => SetContractValue(ref field, value); }
    public bool HasLoft { get; set => SetContractValue(ref field, value); }
    public bool HasIndoorWashingMachineSpace { get; set => SetContractValue(ref field, value); }
    public bool HasBathroomDryer { get; set => SetContractValue(ref field, value); }
    public bool HasSeparateBathAndToilet { get; set => SetContractValue(ref field, value); }
    public bool HasSeparateWashbasin { get; set => SetContractValue(ref field, value); }
    public bool HasWarmWaterToiletSeat { get; set => SetContractValue(ref field, value); }
    public string ApplianceRemarks { get => field ?? string.Empty; set => SetContractValue(ref field, value); }

    #endregion

    #region == 契約条件 == 

    public IReadOnlyList<ListingFeeUnitTypeLabel> FeeUnitTypes { get; } = ListingFeeUnitTypeLabel.GetAll();
    public IReadOnlyList<ListingContractTypeLabel> ContractTypes { get; } = ListingContractTypeLabel.GetAll();

    public string ManagementFee { get => field ?? string.Empty; set => SetContractValue(ref field, value); }
    public string SecurityDeposit { get => field ?? string.Empty; set => SetContractValue(ref field, value); }
    public string KeyMoney { get => field ?? string.Empty; set => SetContractValue(ref field, value); }
    public string RenewalFee { get => field ?? string.Empty; set => SetContractValue(ref field, value); }
    public string RecontractFee { get => field ?? string.Empty; set => SetContractValue(ref field, value); }
    public string ContractPeriodYears { get => field ?? string.Empty; set => SetContractValue(ref field, value); }
    public string GuarantorCompany { get => field ?? string.Empty; set => SetContractValue(ref field, value); }
    public string HousingInsurance { get => field ?? string.Empty; set => SetContractValue(ref field, value); }
    public string OtherFees { get => field ?? string.Empty; set => SetContractValue(ref field, value); }
    public string OtherConditions { get => field ?? string.Empty; set => SetContractValue(ref field, value); }
    public string ContractRemarks { get => field ?? string.Empty; set => SetContractValue(ref field, value); }
    public bool PetFriendly { get; set => SetContractValue(ref field, value); }
    public bool TwoPersonOccupancy { get; set => SetContractValue(ref field, value); }
    public bool NoGuarantorNeeded { get; set => SetContractValue(ref field, value); }
    public bool OfficeUseAllowed { get; set => SetContractValue(ref field, value); }

    public ListingFeeUnitTypeLabel SelectedManagementFeeUnit { get => field ?? FeeUnitTypes.First(x => x.Key == FeeUnitType.Yen); set { if (value is not null) SetContractValue(ref field, value); } }
    public ListingFeeUnitTypeLabel SelectedSecurityDepositUnit { get => field ?? FeeUnitTypes.First(x => x.Key == FeeUnitType.Months); set { if (value is not null) SetContractValue(ref field, value); } }
    public ListingFeeUnitTypeLabel SelectedKeyMoneyUnit { get => field ?? FeeUnitTypes.First(x => x.Key == FeeUnitType.Months); set { if (value is not null) SetContractValue(ref field, value); } }
    public ListingFeeUnitTypeLabel SelectedRenewalFeeUnit { get => field ?? FeeUnitTypes.First(x => x.Key == FeeUnitType.Months); set { if (value is not null) SetContractValue(ref field, value); } }
    public ListingFeeUnitTypeLabel SelectedRecontractFeeUnit { get => field ?? FeeUnitTypes.First(x => x.Key == FeeUnitType.Yen); set { if (value is not null) SetContractValue(ref field, value); } }
    public ListingContractTypeLabel SelectedContractType { get => field ?? ContractTypes.First(x => x.Key == ContractType.Unspecified); set { if (value is not null) SetContractValue(ref field, value); } }

    public IReadOnlyList<ListingHandlingTypeLabel> HandlingTypes { get; } = ListingHandlingTypeLabel.GetAll();
    public IReadOnlyList<ListingTransactionTypeLabel> TransactionTypes { get; } = ListingTransactionTypeLabel.GetAll();
    public IReadOnlyList<ListingCommissionUnitTypeLabel> CommissionUnitTypes { get; } = ListingCommissionUnitTypeLabel.GetAll();
    public ListingHandlingTypeLabel SelectedHandlingType { get => field ?? HandlingTypes.First(x => x.Key == ListingHandlingType.Unspecified); set { if (value is not null) SetContractValue(ref field, value); } }
    public ListingTransactionTypeLabel SelectedTransactionType { get => field ?? TransactionTypes.First(x => x.Key == ListingTransactionType.Unspecified); set { if (value is not null) SetContractValue(ref field, value); } }
    public string LessorCommission { get => field ?? string.Empty; set => SetContractValue(ref field, value); }
    public string LesseeCommission { get => field ?? string.Empty; set => SetContractValue(ref field, value); }
    public string ListingBrokerCommission { get => field ?? string.Empty; set => SetContractValue(ref field, value); }
    public string TenantBrokerCommission { get => field ?? string.Empty; set => SetContractValue(ref field, value); }
    public ListingCommissionUnitTypeLabel SelectedLessorCommissionUnit { get => field ?? CommissionUnitTypes.First(x => x.Key == ListingCommissionUnitType.Percent); set { if (value is not null) SetContractValue(ref field, value); } }
    public ListingCommissionUnitTypeLabel SelectedLesseeCommissionUnit { get => field ?? CommissionUnitTypes.First(x => x.Key == ListingCommissionUnitType.Percent); set { if (value is not null) SetContractValue(ref field, value); } }
    public ListingCommissionUnitTypeLabel SelectedListingBrokerCommissionUnit { get => field ?? CommissionUnitTypes.First(x => x.Key == ListingCommissionUnitType.Percent); set { if (value is not null) SetContractValue(ref field, value); } }
    public ListingCommissionUnitTypeLabel SelectedTenantBrokerCommissionUnit { get => field ?? CommissionUnitTypes.First(x => x.Key == ListingCommissionUnitType.Percent); set { if (value is not null) SetContractValue(ref field, value); } }
    public string TransactionRemarks { get => field ?? string.Empty; set => SetContractValue(ref field, value); }

    public string Chinryou
    {
        get => field ?? string.Empty; // Ensure a non-null value is returned
        set
        {
            if (field == value)
            {
                return;
            }

            if (value is null)
            {
                return;
            }

            var text = Helpers.Common.NormalizeFullWidthCharacters(value.Trim());
            if (SetProperty(ref field, text))
            {
                IsDirty = true;
            }
        }
    }

    #endregion

    #region == 写真 & PDFプロパティ ==

    public ObservableCollection<Models.Rent.Residentials.ListingPicture> Pictures
    {
        get;
        set
        {
            if (SetProperty(ref field, value))
            {
                IsDirty = true;//?

                OpenBlobDirectoryCommand.NotifyCanExecuteChanged();
            }
        }
    } = [];

    public ObservableCollection<Models.Rent.Residentials.ListingPdf> Pdfs
    {
        get;
        set
        {
            if (SetProperty(ref field, value))
            {
                IsDirty = true;//?

                OpenBlobDirectoryCommand.NotifyCanExecuteChanged();
            }
        }
    } = [];

    #endregion

    #region == 貸主プロパティ ==

    public ObservableCollection<Models.Rent.Residentials.PersonWrapperForListingViewModel> LessorsWrapper
    {
        get;
        set
        {
            if (SetProperty(ref field, value))
            {
                IsDirty = true;//?
            }
        }
    } = [];

    #endregion

    #region == 宅建業者プロパティ ==

    public ObservableCollection<Models.Rent.Residentials.PersonWrapperForListingViewModel> BrokersWrapper
    {
        get;
        set
        {
            if (SetProperty(ref field, value))
            {
                IsDirty = true;//?
            }
        }
    } = [];

    #endregion

    #endregion

    #region == Messages ==

    public void Receive(PropertyUpdatedMessage property)
    {
        if (property.Value is not Models.Rent.Residentials.Property residential || !residential.Id.Equals(_room.PropertyId, StringComparison.Ordinal)) return;

        var status = residential.Status;
        var name = residential.Name;
        var ownership = residential.IsUnitOwnership;
        var parent = App.GetService<ViewModels.MainViewModel>().RentResidentialEditorList.FirstOrDefault(editor => editor.Id == _room.PropertyId)?.ViewModel;
        var committed = status == EntityStatus.Saved && residential.ListingIds.Contains(_id, StringComparer.Ordinal) ? parent?.GetCommittedListingDraft(_id) : null;
        if (DeferMessage(() => ApplyPropertyUpdate(status, name, ownership, committed))) return;
        ApplyPropertyUpdate(status, name, ownership, committed);
    }

    private void ApplyPropertyUpdate(EntityStatus status, string name, bool ownership, ResidentialListingDraft? committed)
    {
        _room.SetPropertyStatus(status);
        _room.SetPropertyName(name);
        SetOwnershipVisibility(ownership);

        if (_handedOffToNewParent && status == EntityStatus.Saved && committed is not null)
        {
            _handedOffToNewParent = false;
            _room.SetStatus(EntityStatus.Saved);
            CaptureCommittedAttachments(committed);
            _unsavedRoomPictureFileList.RemoveAll(_committedAttachmentPaths.Contains);
            _unsavedRoomPdfFileList.RemoveAll(_committedAttachmentPaths.Contains);
            _unsavedRoomPdfThumbnailFileList.RemoveAll(_committedAttachmentPaths.Contains);

            var pictureIds = committed.Pictures.Select(item => item.Id).ToHashSet(StringComparer.Ordinal);
            var pdfIds = committed.Pdfs.Select(item => item.Id).ToHashSet(StringComparer.Ordinal);
            foreach (var picture in Pictures.Concat(_room.PicturesToBeDeleted).Where(item => pictureIds.Contains(item.Id))) picture.SetStatus(EntityStatus.Saved);
            foreach (var pdf in Pdfs.Concat(_room.PdfsToBeDeleted).Where(item => pdfIds.Contains(item.Id))) pdf.SetStatus(EntityStatus.Saved);
            foreach (var deleted in _room.PicturesToBeDeleted.Where(item => committed.PicturesToBeDeleted.Any(saved => saved.Id == item.Id && saved.ImageFilename == item.ImageFilename)).ToList()) _room.PicturesToBeDeleted.Remove(deleted);
            foreach (var deleted in _room.PdfsToBeDeleted.Where(item => committed.PdfsToBeDeleted.Any(saved => saved.Id == item.Id && saved.PdfFilename == item.PdfFilename && saved.ThumbnailFilename == item.ThumbnailFilename)).ToList()) _room.PdfsToBeDeleted.Remove(deleted);
            foreach (var deleted in _room.LessorsToBeDeleted.Where(item => committed.LessorsToBeDeleted.Contains(item.Id, StringComparer.Ordinal)).ToList()) _room.LessorsToBeDeleted.Remove(deleted);
            foreach (var deleted in _room.BrokersToBeDeleted.Where(item => committed.BrokersToBeDeleted.Contains(item.Id, StringComparer.Ordinal)).ToList()) _room.BrokersToBeDeleted.Remove(deleted);
        }
        else if (_handedOffToNewParent && status == EntityStatus.Saved)
        {
            IsDirty = true;
            ShowAttachmentError("建物の保存に募集物件が含まれていません。内容を確認してから募集物件を保存してください。");
        }
        WindowTitle = string.Empty;
    }

    public void Receive(LessorUpdatedMessage person)
    {
        if (DeferMessage(() => Receive(person))) return;
        var lessor = person.Value;
        if (lessor is null)
        {
            return;
        }

        var psn = LessorsWrapper.FirstOrDefault(r => r.Person.Id.Equals(lessor.Id, StringComparison.Ordinal));
        if (psn is null) return;

        psn.Person = lessor;
    }

    public void Receive(BrokerUpdatedMessage corp)
    {
        if (DeferMessage(() => Receive(corp))) return;
        var broker = corp.Value;
        if (broker is null)
        {
            return;
        }

        var psn = BrokersWrapper.FirstOrDefault(r => r.Person.Id.Equals(broker.Id, StringComparison.Ordinal));
        if (psn is null) return;

        psn.Person = broker; //= new PersonWrapperForPropertyViewModel(lessor, this);
    }

    public void Receive(PropertyIsUnitOwnershipChangedMessage isUnitOwnership)
    {
        if (!string.Equals(isUnitOwnership.PropertyId, _room.PropertyId, StringComparison.Ordinal)) return;
        if (DeferMessage(() => Receive(isUnitOwnership))) return;
        SetOwnershipVisibility(isUnitOwnership.Value);
    }

    public void SetOwnershipVisibility(bool isUnitOwnership)
    {
        var ownershipChanged = IsPropertyUnitOwnership != isUnitOwnership;
        if (ownershipChanged) _ownershipVersion++;
        IsPropertyUnitOwnership = isUnitOwnership;
        _room.SetIsPropertyUnitOwnership(isUnitOwnership);

        if (IsPropertyUnitOwnership)
        {
            return;
        }

        if (_navigationService.IsCurrentPageSameAs("ZumenSearch.Views.Rent.Residentials.Listing.ZumenListPage"))
        {
            _navigationService.NavigateTo("ZumenSearch.Views.Rent.Residentials.Listing.BasicPage", this);
        }
        else if (_navigationService.IsCurrentPageSameAs("ZumenSearch.Views.Rent.Residentials.Listing.LessorListPage"))
        {
            _navigationService.NavigateTo("ZumenSearch.Views.Rent.Residentials.Listing.BasicPage", this);
        }
        else if (_navigationService.IsCurrentPageSameAs("ZumenSearch.Views.Rent.Residentials.Listing.BrokerListPage"))
        {
            _navigationService.NavigateTo("ZumenSearch.Views.Rent.Residentials.Listing.BasicPage", this);
        }
    }

    public void Receive(LessorDeletedMessage lessorId)
    {
        if (DeferMessage(() => Receive(lessorId))) return;
        var id = lessorId.Value;
        if (string.IsNullOrEmpty(id))
        {
            return;
        }

        var psn = LessorsWrapper.FirstOrDefault(r => r.Person.Id.Equals(id, StringComparison.Ordinal));
        if (psn is null) return;
        LessorsWrapper.Remove(psn);
    }

    public void Receive(BrokerDeletedMessage lessorId)
    {
        if (DeferMessage(() => Receive(lessorId))) return;
        var id = lessorId.Value;
        if (string.IsNullOrEmpty(id))
        {
            return;
        }

        var psn = BrokersWrapper.FirstOrDefault(r => r.Person.Id.Equals(id, StringComparison.Ordinal));
        if (psn is null) return;
        BrokersWrapper.Remove(psn);
    }

    #endregion

    #region == Public Methods ==

    public bool IsSaving { get; private set; }
    public bool IsClosing => _isClosing;
    public bool IsPropertyBusy => _propertyBusy;
    public bool IsDiscardPending => _discardPending;
    public bool IsEditorEnabled => !IsSaving && !_propertyBusy && !_isClosing && !_discardPending;
    public bool HasPendingEditorOperations => _pendingPickers > 0 || PendingEditorTasks().Count > 0;

    public void BeginPicker()
    {
        if (_pendingPickers++ == 0) _pickersCompleted = new TaskCompletionSource(TaskCreationOptions.RunContinuationsAsynchronously);
    }

    public void EndPicker()
    {
        if (--_pendingPickers == 0)
        {
            _pickersCompleted?.TrySetResult();
            _pickersCompleted = null;
        }
    }

    public void SetPropertyBusy(bool value)
    {
        _propertyBusy = value;
        OnPropertyChanged(nameof(IsEditorEnabled));
        SaveCommand.NotifyCanExecuteChanged();
        if (!value && !IsSaving) ApplyDeferredMessages();
    }

    public void SetClosing(bool value)
    {
        _isClosing = value;
        OnPropertyChanged(nameof(IsEditorEnabled));
        SaveCommand.NotifyCanExecuteChanged();
    }

    public async Task<bool> WaitForEditorOperationsAsync()
    {
        while (true)
        {
            var tasks = PendingEditorTasks();
            if (_pickersCompleted is not null) tasks.Add(_pickersCompleted.Task);
            if (tasks.Count == 0) return true;
            try
            {
                await Task.WhenAll(tasks);
            }
            catch (Exception ex) when (ex is IOException or UnauthorizedAccessException or System.Runtime.InteropServices.COMException or InvalidOperationException or ArgumentException)
            {
                Debug.WriteLine($"Failed to complete listing editor operation: {ex}");
                ShowAttachmentError($"編集中の処理を完了できませんでした: {ex.Message}");
                return false;
            }
        }
    }

    private List<Task> PendingEditorTasks()
    {
        IAsyncRelayCommand[] commands = [AddNewRoomPicturesCommand, AddNewRoomPdfsCommand, OpenRoomPdfCommand, DeleteRoomPictureCommand, DeleteRoomPdfCommand, AddLessorCommand, DeleteLessorCommand, AddBrokerCommand];
        return commands.Where(command => command.ExecutionTask is { IsCompleted: false }).Select(command => command.ExecutionTask!).ToList();
    }

    private bool DeferMessage(Action action)
    {
        if (!IsSaving && !_propertyBusy) return false;
        _deferredMessages.Enqueue(action);
        return true;
    }

    public void ApplyDeferredMessages()
    {
        while (_deferredMessages.TryDequeue(out var message)) message();
    }

    public bool DiscardChanges()
    {
        if (_hasPendingFileCleanupAfterSave)
        {
            ShowAttachmentError("保存済みデータの削除済みファイルが残っています。ファイルの削除を完了するため、もう一度保存してください。");
            return false;
        }

        _discardPending = true;
        OnPropertyChanged(nameof(IsEditorEnabled));
        SaveCommand.NotifyCanExecuteChanged();
        if (!DiscardUnsavedFiles())
        {
            ShowAttachmentError($"{InfoBarErrorMessage} 破棄を完了するには、もう一度画面を閉じてください。");
            return false;
        }

        _discardPending = false;
        IsDirty = false;
        return true;
    }

    public void Dispose()
    {
        if (_disposed)
        {
            return;
        }

        _disposed = true;
        _ownershipVersion++;
        foreach (var picture in Pictures)
        {
            picture.PropertyChanged -= OnPicturePropertyChanged;
        }

        foreach (var pdf in Pdfs)
        {
            pdf.PropertyChanged -= OnPdfPropertyChanged;
        }

#pragma warning disable IL3050 // Disable AOT dynamic code warning
#pragma warning disable IL2026 // Disable Trimming unreferenced code warning
        IsActive = false;
#pragma warning restore IL2026
#pragma warning restore IL3050

        _cts.Cancel();
        _cts.Dispose();

        GC.SuppressFinalize(this);
        Debug.WriteLine($"ListingViewModel.Dispose() called. Id={_id}");
    }

    #endregion

    #region == Private Methods ==

    private IEnumerable<string> AttachmentPaths(ResidentialListingDraft draft, bool includeDeleted)
    {
        var pictures = includeDeleted ? draft.Pictures.Concat(draft.PicturesToBeDeleted) : draft.Pictures;
        var pdfs = includeDeleted ? draft.Pdfs.Concat(draft.PdfsToBeDeleted) : draft.Pdfs;
        return pictures.Select(item => Path.Combine(_listingDataDirectoryPath, item.ImageFilename))
            .Concat(pdfs.SelectMany(item => new[] { Path.Combine(_listingDataDirectoryPath, item.PdfFilename), Path.Combine(_listingDataDirectoryPath, item.ThumbnailFilename) }));
    }

    private void CaptureCommittedAttachments(ResidentialListingDraft draft)
    {
        _committedAttachmentPaths.Clear();
        _committedAttachmentPaths.UnionWith(AttachmentPaths(draft, false));
    }

    private HashSet<string> ProtectedAttachmentPaths()
    {
        var paths = new HashSet<string>(_committedAttachmentPaths, StringComparer.OrdinalIgnoreCase);
        var parent = App.GetService<ViewModels.MainViewModel>().RentResidentialEditorList.FirstOrDefault(editor => editor.Id == _room.PropertyId)?.ViewModel;
        if (parent?.GetPendingListingDraft(_id) is { } pending) paths.UnionWith(AttachmentPaths(pending, true));
        return paths;
    }

    public bool ReferencesAttachmentFile(string path)
    {
        if (string.IsNullOrWhiteSpace(path)) return false;
        return Pictures.Any(item => Path.Combine(_listingDataDirectoryPath, item.ImageFilename).Equals(path, StringComparison.OrdinalIgnoreCase)) ||
            Pdfs.Any(item => Path.Combine(_listingDataDirectoryPath, item.PdfFilename).Equals(path, StringComparison.OrdinalIgnoreCase) || Path.Combine(_listingDataDirectoryPath, item.ThumbnailFilename).Equals(path, StringComparison.OrdinalIgnoreCase)) ||
            _unsavedRoomPictureFileList.Contains(path, StringComparer.OrdinalIgnoreCase) || _unsavedRoomPdfFileList.Contains(path, StringComparer.OrdinalIgnoreCase) ||
            _unsavedRoomPdfThumbnailFileList.Contains(path, StringComparer.OrdinalIgnoreCase) || _incompleteAttachmentFiles.Contains(path, StringComparer.OrdinalIgnoreCase);
    }

    private void SetContractValue<T>(ref T storage, T value, [System.Runtime.CompilerServices.CallerMemberName] string? propertyName = null)
    {
        if (SetProperty(ref storage, value, propertyName))
        {
            IsDirty = true;
        }
    }

    private void ShowErrorInfoBar(Models.ErrorInfo error)
    {
        InfoBarErrorMessage =
            error.Title + Environment.NewLine +
            error.Message + Environment.NewLine +
            error.Description + Environment.NewLine +
            error.Operation + Environment.NewLine +
            error.MethodName;
        IsInfoBarErrorOpen = true;
    }

    private bool ValidateContractNumericValues()
    {
        (string Label, string Value)[] values =
        [
            ("共益費・管理費", ManagementFee),
            ("敷金", SecurityDeposit),
            ("礼金", KeyMoney),
            ("更新料", RenewalFee),
            ("再契約手数料", RecontractFee),
            ("契約期間", ContractPeriodYears)
        ];

        return ValidateNonNegativeNumericValues(values);
    }

    private bool ValidateChinryou()
    {
        var text = Helpers.Common.NormalizeFullWidthCharacters(Chinryou.Trim());
        if (string.IsNullOrEmpty(text))
        {
            return true;
        }

        if (text.Contains(',', StringComparison.Ordinal))
        {
            if (!long.TryParse(text, NumberStyles.AllowThousands, CultureInfo.InvariantCulture, out var parsedWithSeparators) ||
                !string.Equals(parsedWithSeparators.ToString("N0", CultureInfo.InvariantCulture), text, StringComparison.Ordinal))
            {
                InfoBarErrorMessage = "賃料は0以上の整数で入力してください。";
                return false;
            }

            return true;
        }

        if (!long.TryParse(text, NumberStyles.None, CultureInfo.InvariantCulture, out _))
        {
            InfoBarErrorMessage = "賃料は0以上の整数で入力してください。";
            return false;
        }

        return true;
    }

    private bool ValidateTransactionNumericValues()
    {
        (string Label, string Value)[] values =
        [
            ("貸主手数料", LessorCommission),
            ("借主手数料", LesseeCommission),
            ("元付手数料", ListingBrokerCommission),
            ("客付け手数料", TenantBrokerCommission)
        ];

        return ValidateNonNegativeNumericValues(values);
    }

    private bool ValidateNonNegativeNumericValues((string Label, string Value)[] values)
    {
        foreach (var (label, value) in values)
        {
            var text = value.Trim().Normalize(System.Text.NormalizationForm.FormKC);
            if (!string.IsNullOrEmpty(text) && (!decimal.TryParse(text, NumberStyles.Number, CultureInfo.CurrentCulture, out var number) || number < 0))
            {
                InfoBarErrorMessage = $"{label}は0以上の数値で入力してください。";
                return false;
            }
        }

        return true;
    }

    private bool ValidateName()
    {
        if (string.IsNullOrWhiteSpace(Name))
        {
            InfoBarErrorMessage = "部屋名（必須項目）が入力されていません。保存出来ませんでした。";
            IsNameHasError = true;

            return false;
        }

        IsNameHasError = false;
        return true;
    }

    private bool TryParseBasicNumericValues(out decimal floorArea, out int? floorNumber)
    {
        floorArea = 0;
        floorNumber = null;

        var areaText = Helpers.Common.NormalizeFullWidthCharacters(FloorExclusiveAreaSqm.Trim());
        if (!string.IsNullOrEmpty(areaText) &&
            (!decimal.TryParse(areaText, NumberStyles.Number, CultureInfo.CurrentCulture, out floorArea) || floorArea < 0))
        {
            InfoBarErrorMessage = "専有面積は0以上の数値で入力してください。";
            return false;
        }

        var floorText = Helpers.Common.NormalizeFullWidthCharacters(FloorNumber.Trim());
        if (!string.IsNullOrEmpty(floorText))
        {
            if (!int.TryParse(floorText, NumberStyles.Integer, CultureInfo.InvariantCulture, out var parsedFloor) || parsedFloor <= 0)
            {
                InfoBarErrorMessage = "所在階は1以上の整数で入力してください。";
                return false;
            }

            floorNumber = parsedFloor;
        }

        return true;
    }

    private void SetValues()
    {
        if (!IsDirty)
        {
            return;
        }

        if (_room is null)
        {
            Debug.WriteLine("_room is null. Can't set room.");
            return;
        }

        _room.SetName(Name);

        _room.SetFloorPlanTypeLabel(SelectedFloorPlanType);
        _room.SetFloorPlanExtraTypeLabel(SelectedFloorPlanExtraType);
        _room.SetFloorGroundTypeLabel(SelectedFloorGroundType);
        _room.SetFloorExclusiveAreaSqm(FloorExclusiveAreaSqm);
        _room.SetFloorNumber(FloorNumber);
        _room.SetIsCornerRoom(IsCornerRoom);
        _room.SetExposureDirectionTypeLabel(SelectedExposureDirectionType);
        _room.SetAvailabilityStatusTypeLabel(SelectedAvailabilityStatusType);
        _room.SetAvailabilityMonthTypeLabel(SelectedAvailabilityMonthType);
        _room.SetAvailabilityPeriodTypeLabel(SelectedAvailabilityPeriodType);
        _room.SetOccupancyStatusLabel(SelectedOccupancyStatus);
        _room.SetIsAvailableForImmediateMoveIn(IsAvailableForImmediateMoveIn);
        _room.SetIsAvailableForRentNow(IsAvailableForRentNow);
        _room.SetOccupancyStatusCheckedAt(OccupancyStatusCheckedAt);
        _room.SetRemarks(Remarks);
        _room.SetHasAirConditioner(HasAirConditioner);
        _room.SetHasWoodFlooring(HasWoodFlooring);
        _room.SetHasVideoIntercom(HasVideoIntercom);
        _room.SetHasBathReheating(HasBathReheating);
        _room.SetHasGasStove(HasGasStove);
        _room.SetHasInductionCooktop(HasInductionCooktop);
        _room.SetHasMultipleBurners(HasMultipleBurners);
        _room.SetHasSystemKitchen(HasSystemKitchen);
        _room.SetHasLoft(HasLoft);
        _room.SetHasIndoorWashingMachineSpace(HasIndoorWashingMachineSpace);
        _room.SetHasBathroomDryer(HasBathroomDryer);
        _room.SetHasSeparateBathAndToilet(HasSeparateBathAndToilet);
        _room.SetHasSeparateWashbasin(HasSeparateWashbasin);
        _room.SetHasWarmWaterToiletSeat(HasWarmWaterToiletSeat);
        _room.SetApplianceRemarks(ApplianceRemarks);
        _room.SetChinryouFromString(Chinryou);
        _room.SetManagementFee(ManagementFee);
        _room.SetSecurityDeposit(SecurityDeposit);
        _room.SetKeyMoney(KeyMoney);
        _room.SetRenewalFee(RenewalFee);
        _room.SetRecontractFee(RecontractFee);
        _room.SetContractPeriodYears(ContractPeriodYears);
        _room.SetManagementFeeUnitTypeLabel(SelectedManagementFeeUnit);
        _room.SetSecurityDepositUnitTypeLabel(SelectedSecurityDepositUnit);
        _room.SetKeyMoneyUnitTypeLabel(SelectedKeyMoneyUnit);
        _room.SetRenewalFeeUnitTypeLabel(SelectedRenewalFeeUnit);
        _room.SetRecontractFeeUnitTypeLabel(SelectedRecontractFeeUnit);
        _room.SetContractTypeLabel(SelectedContractType);
        _room.SetGuarantorCompany(GuarantorCompany);
        _room.SetHousingInsurance(HousingInsurance);
        _room.SetOtherFees(OtherFees);
        _room.SetPetFriendly(PetFriendly);
        _room.SetTwoPersonOccupancy(TwoPersonOccupancy);
        _room.SetNoGuarantorNeeded(NoGuarantorNeeded);
        _room.SetOfficeUseAllowed(OfficeUseAllowed);
        _room.SetOtherConditions(OtherConditions);
        _room.SetContractRemarks(ContractRemarks);
        _room.SetHandlingTypeLabel(SelectedHandlingType);
        _room.SetTransactionTypeLabel(SelectedTransactionType);
        _room.SetLessorCommission(LessorCommission);
        _room.SetLesseeCommission(LesseeCommission);
        _room.SetListingBrokerCommission(ListingBrokerCommission);
        _room.SetTenantBrokerCommission(TenantBrokerCommission);
        _room.SetLessorCommissionUnitTypeLabel(SelectedLessorCommissionUnit);
        _room.SetLesseeCommissionUnitTypeLabel(SelectedLesseeCommissionUnit);
        _room.SetListingBrokerCommissionUnitTypeLabel(SelectedListingBrokerCommissionUnit);
        _room.SetTenantBrokerCommissionUnitTypeLabel(SelectedTenantBrokerCommissionUnit);
        _room.SetTransactionRemarks(TransactionRemarks);

        _room.Pictures = Pictures;
        _room.Pdfs = Pdfs;

        _room.Lessors.Clear();
        _room.Brokers.Clear();
        foreach (var wrapper in LessorsWrapper)
        {
            _room.Lessors.Add(wrapper.Person);
            foreach (var deleted in _room.LessorsToBeDeleted.Where(person => person.Id == wrapper.Person.Id).ToList()) _room.LessorsToBeDeleted.Remove(deleted);
        }
        foreach (var wrapper in BrokersWrapper)
        {
            _room.Brokers.Add(wrapper.Person);
            foreach (var deleted in _room.BrokersToBeDeleted.Where(person => person.Id == wrapper.Person.Id).ToList()) _room.BrokersToBeDeleted.Remove(deleted);
        }
        foreach (var deleted in _draftLessorDeletions.Where(person => !_room.LessorsToBeDeleted.Any(item => item.Id == person.Id))) _room.LessorsToBeDeleted.Add(deleted);
        foreach (var deleted in _draftBrokerDeletions.Where(person => !_room.BrokersToBeDeleted.Any(item => item.Id == person.Id))) _room.BrokersToBeDeleted.Add(deleted);

    }

    private void PopulateValues()
    {
        if (_room is null)
        {
            return;
        }

        IsPropertyUnitOwnership = _room.IsPropertyUnitOwnership;

        Name = _room.Name; // Set the value to trigger the setter logic if needed.

        SelectedFloorPlanType = _room.FloorPlan;
        SelectedFloorPlanExtraType = _room.FloorPlanExtra;
        SelectedFloorGroundType = _room.FloorGround;
        FloorExclusiveAreaSqm = _room.FloorExclusiveAreaSqm;
        FloorNumber = _room.FloorNumber;
        IsCornerRoom = _room.IsCornerRoom;
        SelectedExposureDirectionType = _room.SelectedExposureDirectionType;
        SelectedAvailabilityStatusType = _room.SelectedAvailabilityStatusType;
        SelectedAvailabilityMonthType = _room.SelectedAvailabilityMonthType;
        SelectedAvailabilityPeriodType = _room.SelectedAvailabilityPeriodType;
        SelectedOccupancyStatus = _room.SelectedOccupancyStatus;
        IsAvailableForImmediateMoveIn = _room.IsAvailableForImmediateMoveIn;
        IsAvailableForRentNow = _room.IsAvailableForRentNow;
        OccupancyStatusCheckedAt = _room.OccupancyStatusCheckedAt;

        Remarks = _room.Remarks;
        HasAirConditioner = _room.HasAirConditioner;
        HasWoodFlooring = _room.HasWoodFlooring;
        HasVideoIntercom = _room.HasVideoIntercom;
        HasBathReheating = _room.HasBathReheating;
        HasGasStove = _room.HasGasStove;
        HasInductionCooktop = _room.HasInductionCooktop;
        HasMultipleBurners = _room.HasMultipleBurners;
        HasSystemKitchen = _room.HasSystemKitchen;
        HasLoft = _room.HasLoft;
        HasIndoorWashingMachineSpace = _room.HasIndoorWashingMachineSpace;
        HasBathroomDryer = _room.HasBathroomDryer;
        HasSeparateBathAndToilet = _room.HasSeparateBathAndToilet;
        HasSeparateWashbasin = _room.HasSeparateWashbasin;
        HasWarmWaterToiletSeat = _room.HasWarmWaterToiletSeat;
        ApplianceRemarks = _room.ApplianceRemarks;

        Chinryou = _room.Chinryou == 0 ? string.Empty : _room.Chinryou.ToString(CultureInfo.InvariantCulture);
        ManagementFee = _room.ManagementFee;
        SecurityDeposit = _room.SecurityDeposit;
        KeyMoney = _room.KeyMoney;
        RenewalFee = _room.RenewalFee;
        RecontractFee = _room.RecontractFee;
        ContractPeriodYears = _room.ContractPeriodYears;
        SelectedManagementFeeUnit = FeeUnitTypes.First(x => x.Key == _room.ManagementFeeUnit.Key);
        SelectedSecurityDepositUnit = FeeUnitTypes.First(x => x.Key == _room.SecurityDepositUnit.Key);
        SelectedKeyMoneyUnit = FeeUnitTypes.First(x => x.Key == _room.KeyMoneyUnit.Key);
        SelectedRenewalFeeUnit = FeeUnitTypes.First(x => x.Key == _room.RenewalFeeUnit.Key);
        SelectedRecontractFeeUnit = FeeUnitTypes.First(x => x.Key == _room.RecontractFeeUnit.Key);
        SelectedContractType = ContractTypes.First(x => x.Key == _room.ContractType.Key);
        GuarantorCompany = _room.GuarantorCompany;
        HousingInsurance = _room.HousingInsurance;
        OtherFees = _room.OtherFees;
        PetFriendly = _room.PetFriendly;
        TwoPersonOccupancy = _room.TwoPersonOccupancy;
        NoGuarantorNeeded = _room.NoGuarantorNeeded;
        OfficeUseAllowed = _room.OfficeUseAllowed;
        OtherConditions = _room.OtherConditions;
        ContractRemarks = _room.ContractRemarks;
        SelectedHandlingType = HandlingTypes.First(x => x.Key == _room.HandlingType.Key);
        SelectedTransactionType = TransactionTypes.First(x => x.Key == _room.TransactionType.Key);
        LessorCommission = _room.LessorCommission;
        LesseeCommission = _room.LesseeCommission;
        ListingBrokerCommission = _room.ListingBrokerCommission;
        TenantBrokerCommission = _room.TenantBrokerCommission;
        SelectedLessorCommissionUnit = CommissionUnitTypes.First(x => x.Key == _room.LessorCommissionUnit.Key);
        SelectedLesseeCommissionUnit = CommissionUnitTypes.First(x => x.Key == _room.LesseeCommissionUnit.Key);
        SelectedListingBrokerCommissionUnit = CommissionUnitTypes.First(x => x.Key == _room.ListingBrokerCommissionUnit.Key);
        SelectedTenantBrokerCommissionUnit = CommissionUnitTypes.First(x => x.Key == _room.TenantBrokerCommissionUnit.Key);
        TransactionRemarks = _room.TransactionRemarks;

        // Pictures
        Pictures = _room.Pictures;
        foreach (var item in Pictures)
        {
            item.BasePath = System.IO.Path.Combine(System.IO.Path.Combine(System.IO.Path.Combine(App.PropertyBlobDataFolder, _room.PropertyId), _room.Id));
            item.ParentViewModel = this;
            item.SetIsModified(false); // Needed this.
            item.PropertyChanged += OnPicturePropertyChanged;
        }

        Pictures.CollectionChanged += (s, e) =>
        {
            // Unsubscribe from removed items
            if (e.OldItems != null)
            {
                foreach (Models.Rent.Residentials.ListingPicture item in e.OldItems)
                {
                    Debug.WriteLine($"Item {item.Id} Removed from Pictures");
                    IsDirty = true;

                    item.PropertyChanged -= OnPicturePropertyChanged;
                }
            }

            // Subscribe to PropertyChanged.
            if (e.NewItems != null)
            {
                foreach (Models.Rent.Residentials.ListingPicture item in e.NewItems)
                {
                    Debug.WriteLine($"Item {item.Id} Added to Pictures");
                    IsDirty = true;

                    item.PropertyChanged += OnPicturePropertyChanged;
                }
            }
        };

        // PDFs
        Pdfs = _room.Pdfs;
        ConfigureRoomPdfCollection(Pdfs);

        LessorsWrapper = new ObservableCollection<Models.Rent.Residentials.PersonWrapperForListingViewModel>(_room.Lessors.Select(person => new Models.Rent.Residentials.PersonWrapperForListingViewModel(person, this)));
        BrokersWrapper = new ObservableCollection<Models.Rent.Residentials.PersonWrapperForListingViewModel>(_room.Brokers.Select(person => new Models.Rent.Residentials.PersonWrapperForListingViewModel(person, this)));

        _room.SetIsModified(false);
        IsDirty = false;
    }

    private void OnPicturePropertyChanged(object? sender, PropertyChangedEventArgs e)
    {
        if (sender is not Models.Rent.Residentials.ListingPicture picUnit)
        {
            Debug.WriteLine("OnPicturePropertyChanged returned non PictureUnit.");
            return;
        }

        if (picUnit.IsModified)
        {
            Debug.WriteLine($"Property {e.PropertyName} changed");
            IsDirty = true;

            var prop = e.PropertyName ?? string.Empty;
            if (prop.Equals("IsMain", StringComparison.Ordinal))
            {
                if (picUnit.IsMain)
                {
                    // Clear all other pics
                    foreach (var item in Pictures)
                    {
                        if (item != picUnit)
                        {
                            item.IsMain = false;
                        }
                    }
                }
            }
        }
    }

    private void ConfigureRoomPdfCollection(ObservableCollection<Models.Rent.Residentials.ListingPdf> pdfs)
    {
        foreach (var item in pdfs)
        {
            item.BasePath = _listingDataDirectoryPath;
            item.ParentViewModel = this;
            item.SetIsModified(false);
            item.PropertyChanged += OnPdfPropertyChanged;
        }

        pdfs.CollectionChanged += (sender, args) =>
        {
            if (args.OldItems is not null)
            {
                foreach (Models.Rent.Residentials.ListingPdf item in args.OldItems)
                {
                    IsDirty = true;
                    item.PropertyChanged -= OnPdfPropertyChanged;
                }
            }

            if (args.NewItems is not null)
            {
                foreach (Models.Rent.Residentials.ListingPdf item in args.NewItems)
                {
                    IsDirty = true;
                    item.PropertyChanged += OnPdfPropertyChanged;
                }
            }
        };
    }

    private void AddPendingPdfCleanupPath(string path)
    {
        if (string.IsNullOrWhiteSpace(path)) return;
        if (!ProtectedAttachmentPaths().Contains(path) && !_pendingRoomPdfCleanupFiles.Contains(path, StringComparer.OrdinalIgnoreCase)) _pendingRoomPdfCleanupFiles.Add(path);
    }

    private void OnPdfPropertyChanged(object? sender, PropertyChangedEventArgs e)
    {
        if (sender is not Models.Rent.Residentials.ListingPdf pdfUnit)
        {
            Debug.WriteLine("OnPdfPropertyChanged returned non PdfUnit.");
            return;
        }

        if (pdfUnit.IsModified)
        {
            Debug.WriteLine($"Property {e.PropertyName} changed");
            IsDirty = true;

            var prop = e.PropertyName ?? string.Empty;
            if (prop.Equals("IsMain", StringComparison.Ordinal))
            {
                if (pdfUnit.IsMain)
                {
                    // Clear all other pics
                    foreach (var item in Pdfs)
                    {
                        if (item != pdfUnit)
                        {
                            item.IsMain = false;
                        }
                    }
                }
            }
        }
    }

    public bool CleanIncompleteAttachments()
    {
        var failures = new List<string>();
        var savedPaths = ProtectedAttachmentPaths();
        _pendingRoomPdfCleanupFiles.RemoveAll(savedPaths.Contains);
        DeleteTrackedFiles(_incompleteAttachmentFiles, failures);
        DeleteTrackedFiles(_pendingRoomPdfCleanupFiles, failures);
        if (failures.Count == 0) return true;
        ShowAttachmentError($"読み込みまたは破棄した図面ファイルを削除できませんでした: {string.Join(", ", failures)}");
        return false;
    }

    private bool DiscardUnsavedFiles()
    {
        if (!CleanIncompleteAttachments()) return false;
        var failures = new List<string>();
        var savedPaths = ProtectedAttachmentPaths();
        _unsavedRoomPictureFileList.RemoveAll(savedPaths.Contains);
        _unsavedRoomPdfFileList.RemoveAll(savedPaths.Contains);
        _unsavedRoomPdfThumbnailFileList.RemoveAll(savedPaths.Contains);
        DeleteTrackedFiles(_unsavedRoomPictureFileList, failures);
        DeleteTrackedFiles(_unsavedRoomPdfFileList, failures);
        DeleteTrackedFiles(_unsavedRoomPdfThumbnailFileList, failures);
        DeleteTrackedFiles(_pendingRoomPdfCleanupFiles, failures);

        if (_room.Status == EntityStatus.New && !_handedOffToNewParent && Directory.Exists(_listingDataDirectoryPath))
        {
            try
            {
                Directory.Delete(_listingDataDirectoryPath, true);
            }
            catch (Exception ex) when (ex is IOException or UnauthorizedAccessException)
            {
                Debug.WriteLine($"Failed to delete listing data directory '{_listingDataDirectoryPath}': {ex}");
                failures.Add(_listingDataDirectoryPath);
            }
        }

        if (failures.Count == 0)
        {
            return true;
        }

        ShowAttachmentError($"変更の破棄中にファイルを削除できませんでした。ファイルを閉じてから再試行してください: {string.Join(", ", failures)}");
        return false;
    }

    private static void DeleteTrackedFiles(List<string> trackedFiles, List<string> failures)
    {
        foreach (var file in trackedFiles.ToList())
        {
            try
            {
                File.Delete(file);
                trackedFiles.Remove(file);
            }
            catch (Exception ex) when (ex is IOException or UnauthorizedAccessException)
            {
                Debug.WriteLine($"Failed to delete file '{file}': {ex}");
                failures.Add(file);
            }
        }
    }

    private static bool TryDeleteFiles(IEnumerable<string> filePaths, List<string> failures)
    {
        var succeeded = true;
        foreach (var filePath in filePaths)
        {
            try
            {
                File.Delete(filePath);
            }
            catch (Exception ex) when (ex is IOException or UnauthorizedAccessException)
            {
                Debug.WriteLine($"Failed to delete file '{filePath}': {ex}");
                failures.Add(filePath);
                succeeded = false;
            }
        }

        return succeeded;
    }

    #endregion

    #region == Commands ==

    #region == Save ==

    [RelayCommand(CanExecute = nameof(CanSave))]
    public async Task Save()
    {
        if (_discardPending)
        {
            ShowAttachmentError("変更の破棄が完了していないため保存できません。もう一度画面を閉じて再試行してください。");
            return;
        }
        if (IsSaving || !IsDirty) return;
        if (HasPendingEditorOperations)
        {
            ShowAttachmentError("実行中の選択またはファイル処理が完了してから、もう一度保存してください。");
            return;
        }

        var main = App.GetService<ViewModels.MainViewModel>();
        if (main.RentResidentialEditorList.Any(editor => editor.Id == _room.PropertyId && editor.ViewModel is { IsDiscardPending: true }))
        {
            ShowAttachmentError("建物の変更の破棄を完了してから、もう一度保存してください。");
            return;
        }
        if (main.RentResidentialEditorList.Any(editor => editor.Id == _room.PropertyId && editor.ViewModel?.IsListingPendingDeletion(_id) == true))
        {
            ShowAttachmentError("建物の編集画面で削除予定の募集物件は保存できません。");
            return;
        }
        if (main.RentResidentialEditorList.Any(editor => editor.Id == _room.PropertyId && editor.ViewModel is { } vm && vm.HasPendingEditorOperations) || main.RentResidentialListingEditorList.Any(editor => editor.PropertyId == _room.PropertyId && editor.ViewModel is { } vm && vm != this && vm.HasPendingEditorOperations))
        {
            ShowAttachmentError("関連する選択またはファイル処理が完了してから、もう一度保存してください。");
            return;
        }
        if (!CleanIncompleteAttachments()) return;
        if (!main.TryBeginResidentialOperation(_room.PropertyId))
        {
            ShowAttachmentError("関連する保存、削除または読み込みが完了してから、もう一度保存してください。");
            return;
        }

        IsSaving = true;
        OnPropertyChanged(nameof(IsEditorEnabled));
        SaveCommand.NotifyCanExecuteChanged();
        try
        {
            await SaveCore();
        }
        finally
        {
            try
            {
                main.EndResidentialOperation(_room.PropertyId);
                if (_pendingHandoffReady) PublishPendingHandoff(main);
            }
            finally
            {
                IsSaving = false;
                OnPropertyChanged(nameof(IsEditorEnabled));
                SaveCommand.NotifyCanExecuteChanged();
                ApplyDeferredMessages();
            }
        }
    }

    private async Task SaveCore()
    {
        if (_room is null)
        {
            Debug.WriteLine("_room is null. Can't save room.");
            return;
        }

        // Validate input.
        if (!ValidateName() || !TryParseBasicNumericValues(out _, out _))
        {
            //InfoBarErrorMessage = "入力項目に誤りがあります。保存出来ませんでした。";
            IsInfoBarErrorOpen = true;

            if (!_navigationService.IsCurrentPageSameAs(BasicPageName))
            {
                _navigationService.NavigateTo(BasicPageName, this);
            }

            return;
        }

        if (!ValidateContractNumericValues() || !ValidateChinryou())
        {
            IsInfoBarErrorOpen = true;
            const string contractPageName = "ZumenSearch.Views.Rent.Residentials.Listing.ContractPage";
            if (!_navigationService.IsCurrentPageSameAs(contractPageName))
            {
                _navigationService.NavigateTo(contractPageName, this);
            }

            return;
        }

        if (!ValidateTransactionNumericValues())
        {
            IsInfoBarErrorOpen = true;
            const string transactionPageName = "ZumenSearch.Views.Rent.Residentials.Listing.TransactionPage";
            if (!_navigationService.IsCurrentPageSameAs(transactionPageName))
            {
                _navigationService.NavigateTo(transactionPageName, this);
            }

            return;
        }

        if (Pdfs.Count(pdf => pdf.IsMain) > 1)
        {
            ShowAttachmentError("メインの図面は1枚だけ選択してください。");
            const string pageName = "ZumenSearch.Views.Rent.Residentials.Listing.ZumenListPage";
            if (IsPropertyUnitOwnership && !_navigationService.IsCurrentPageSameAs(pageName)) _navigationService.NavigateTo(pageName, this);
            return;
        }

        if (_room.PropertyStatus == EntityStatus.New && !App.GetService<ViewModels.MainViewModel>().RentResidentialEditorList.Any(editor => editor.Id == _room.PropertyId && editor.ViewModel is { IsActive: true }))
        {
            ShowAttachmentError("建物の編集画面が開いていないため、募集物件を保存できません。");
            return;
        }

        var lessorsBeforeSave = _room.Lessors.ToList();
        var brokersBeforeSave = _room.Brokers.ToList();
        var lessorDeletesBeforeSave = _room.LessorsToBeDeleted.ToList();
        var brokerDeletesBeforeSave = _room.BrokersToBeDeleted.ToList();
        if (_room.PropertyStatus == EntityStatus.New) _room.SetIsPropertyUnitOwnership(IsPropertyUnitOwnership);
        SetValues();

        bool saveResult;
        if (_room.PropertyStatus == EntityStatus.New)
        {
            // The parent defers messages while the shared property operation is active.
            _pendingHandoffReady = true;
            return;
        }
        else
        {
            var roomToSave = _room;
            roomToSave.SetIsPropertyUnitOwnership(IsPropertyUnitOwnership);

            Models.SearchResult.ResultWrapper resInsert;
            var committed = false;
            try
            {
                resInsert = await Task.Run(() => _dataAccessService.UpsertRentResidentialListing(_room.PropertyId, roomToSave), _cts.Token);
                committed = !resInsert.IsError;
                if (committed) _room.SetIsPropertyUnitOwnership(IsPropertyUnitOwnership);
            }
            finally
            {
                if (!committed)
                {
                    _room.Lessors.Clear();
                    foreach (var lessor in lessorsBeforeSave) _room.Lessors.Add(lessor);
                    _room.Brokers.Clear();
                    foreach (var broker in brokersBeforeSave) _room.Brokers.Add(broker);
                    _room.LessorsToBeDeleted.Clear();
                    foreach (var lessor in lessorDeletesBeforeSave) _room.LessorsToBeDeleted.Add(lessor);
                    _room.BrokersToBeDeleted.Clear();
                    foreach (var broker in brokerDeletesBeforeSave) _room.BrokersToBeDeleted.Add(broker);
                }
            }
            if (resInsert.IsError)
            {
                Debug.WriteLine(
                    resInsert.Error.Title + Environment.NewLine +
                    resInsert.Error.Message + Environment.NewLine +
                    resInsert.Error.Description + Environment.NewLine +
                    resInsert.Error.Operation + Environment.NewLine +
                    resInsert.Error.MethodName + Environment.NewLine +
                    resInsert.Error.FullDump);

                InfoBarErrorMessage =
                    resInsert.Error.Title + Environment.NewLine +
                    resInsert.Error.Message + Environment.NewLine +
                    resInsert.Error.Description + Environment.NewLine +
                    resInsert.Error.Operation + Environment.NewLine +
                    resInsert.Error.MethodName;

                IsInfoBarErrorOpen = true;

                saveResult = false;
            }
            else
            {
                _room.SetStatus(EntityStatus.Saved);
                _room.SetIsModified(false);
                foreach (var picture in Pictures.Where(picture => roomToSave.Pictures.Any(saved => saved.Id == picture.Id)))
                {
                    picture.SetStatus(EntityStatus.Saved);
                    picture.SetIsModified(false);
                }
                foreach (var pdf in Pdfs.Where(pdf => roomToSave.Pdfs.Any(saved => saved.Id == pdf.Id)))
                {
                    pdf.SetStatus(EntityStatus.Saved);
                    pdf.SetIsModified(false);
                }
                CaptureCommittedAttachments(ResidentialListingDraft.FromListing(roomToSave));
                _unsavedRoomPdfFileList.Clear();
                _unsavedRoomPdfThumbnailFileList.Clear();

                roomToSave.SetIsModified(false);
                WeakReferenceMessenger.Default.Send(new Models.Messenger.ListingUpdatedMessage(roomToSave));

                saveResult = true;
            }

        }

        if (saveResult)
        {
            _draftLessorDeletions.Clear();
            _draftBrokerDeletions.Clear();
            IsDirty = false;

            // Clear error infobar.
            IsInfoBarErrorOpen = false;
            
            // Update title with dummy value.
            WindowTitle = string.Empty;

            var cleanupFailures = new List<string>();
            foreach (var file in _room.PicturesToBeDeleted.ToList())
            {
                var picturePath = Path.Combine(App.PropertyBlobDataFolder, _room.PropertyId, _room.Id, file.ImageFilename);
                if (TryDeleteFiles(new[] { picturePath }, cleanupFailures))
                {
                    _room.PicturesToBeDeleted.Remove(file);
                }
            }

            if (_room.PropertyStatus != EntityStatus.New)
            {
                foreach (var file in _room.PdfsToBeDeleted.ToList())
                {
                    var pdfPath = Path.Combine(App.PropertyBlobDataFolder, _room.PropertyId, _room.Id, file.PdfFilename);
                    var thumbnailPath = Path.Combine(App.PropertyBlobDataFolder, _room.PropertyId, _room.Id, file.ThumbnailFilename);
                    if (TryDeleteFiles(new[] { pdfPath, thumbnailPath }, cleanupFailures))
                    {
                        _room.PdfsToBeDeleted.Remove(file);
                    }
                }
            }

            _unsavedRoomPictureFileList.Clear();
            _unsavedRoomPdfFileList.Clear();
            _unsavedRoomPdfThumbnailFileList.Clear();

            _hasPendingFileCleanupAfterSave = cleanupFailures.Count > 0;
            if (_hasPendingFileCleanupAfterSave)
            {
                IsDirty = true;
                ShowAttachmentError($"保存しましたが、一部の削除済みファイルを削除できませんでした。次回の保存時に再試行します: {string.Join(", ", cleanupFailures)}");
            }
                }
            }   
    private void PublishPendingHandoff(ViewModels.MainViewModel main)
    {
        _pendingHandoffReady = false;
        var parent = main.RentResidentialEditorList.FirstOrDefault(editor => editor.Id == _room.PropertyId)?.ViewModel;
        var previous = parent?.GetPendingListingDraft(_id);
        WeakReferenceMessenger.Default.Send(new Models.Messenger.ListingUpdatedMessage(_room));
        var accepted = parent?.GetPendingListingDraft(_id);
        if (accepted is null || ReferenceEquals(previous, accepted))
        {
            IsDirty = true;
            ShowAttachmentError("建物の編集画面に募集物件を引き渡せませんでした。もう一度保存してください。");
            return;
        }

        _handedOffToNewParent = true;
        var handedOffPaths = AttachmentPaths(accepted, true).ToHashSet(StringComparer.OrdinalIgnoreCase);
        _unsavedRoomPictureFileList.RemoveAll(handedOffPaths.Contains);
        _unsavedRoomPdfFileList.RemoveAll(handedOffPaths.Contains);
        _unsavedRoomPdfThumbnailFileList.RemoveAll(handedOffPaths.Contains);
        _draftLessorDeletions.Clear();
        _draftBrokerDeletions.Clear();
        IsDirty = false;
        IsInfoBarErrorOpen = false;
        WindowTitle = string.Empty;
    }

    private bool CanSave()
    {
        return IsDirty && !IsSaving && !_propertyBusy && !_isClosing && !_discardPending;
    }

    #endregion

    #region == Pics and Pdfs ==

    [RelayCommand(CanExecute = nameof(CanAddNewRoomPictures))]
    public async Task AddNewRoomPictures(List<string> filePathList)
    {
        if (filePathList is null) return;
        if (filePathList.Count == 0) return;

        Debug.WriteLine($"destDirectory={_listingDataDirectoryPath}  @AddNewRoomPictures()");
        
        if (!Directory.Exists(_listingDataDirectoryPath))
        {
            Directory.CreateDirectory(_listingDataDirectoryPath);
        }

        //List<string> list = [];

        foreach (var filePath in filePathList)
        {
            if (string.IsNullOrEmpty(filePath.Trim()))
            {
                continue;
            }

            string extension = Path.GetExtension(filePath);
            if (!IsSupportedImageExtension(extension))
            {
                ShowAttachmentError($"画像形式に対応していません: {Path.GetFileName(filePath)}");
                continue;
            }

            using var sourceStream = File.Open(filePath, FileMode.Open);

            string newId = Guid.CreateVersion7().ToString("N");
            string newFilename = newId + extension;
            var destFilePath = Path.Combine(_listingDataDirectoryPath, newFilename);
            //Debug.WriteLine($"{file} to {destFilePath}  @SetNewBuildingPicturesAsync()");

            using var destinationStream = new FileStream(destFilePath, FileMode.CreateNew, FileAccess.Write);
            _incompleteAttachmentFiles.Add(destFilePath);
            await sourceStream.CopyToAsync(destinationStream);

            var pic = new Models.Rent.Residentials.ListingPicture(newId, newFilename, EntityStatus.New)
            {
                BasePath = _listingDataDirectoryPath,
                ParentViewModel = this
            };

            Pictures.Add(pic);

            OpenBlobDirectoryCommand.NotifyCanExecuteChanged();
            DeleteRoomPictureCommand.NotifyCanExecuteChanged();

            IsDirty = true;

            // Keep track of unsaved files to delete them when discarding.
            _unsavedRoomPictureFileList.Add(destFilePath);
            _incompleteAttachmentFiles.Remove(destFilePath);
        }
    }
    private static bool CanAddNewRoomPictures()
    {
        return true;
    }

    private static bool IsSupportedImageExtension(string extension) =>
        extension.Equals(".jpg", StringComparison.OrdinalIgnoreCase) ||
        extension.Equals(".jpeg", StringComparison.OrdinalIgnoreCase) ||
        extension.Equals(".png", StringComparison.OrdinalIgnoreCase) ||
        extension.Equals(".gif", StringComparison.OrdinalIgnoreCase) ||
        extension.Equals(".webp", StringComparison.OrdinalIgnoreCase);

    private void ShowAttachmentError(string message)
    {
        InfoBarErrorMessage = message;
        IsInfoBarErrorOpen = true;
    }

    [RelayCommand(CanExecute = nameof(CanDeleteRoomPicture))]
    private async Task DeleteRoomPicture(Models.Rent.Residentials.ListingPicture picUnit)
    {
        if (_room is null)
        {
            return;
        }

        if (picUnit is null)
        {
            return;
        }

        if (_dialogService is null || await _dialogService.ShowDeleteConfirmationDialog("この募集物件の写真を削除しますか？") != Microsoft.UI.Xaml.Controls.ContentDialogResult.Primary)
        {
            return;
        }

        if (Pictures.Remove(picUnit))
        {
            // TODO: should I? Prob no.
            /*
            if (_room.Pictures.Remove(picUnit))
            {
                
            }
            */
            _room.PicturesToBeDeleted.Add(picUnit);
            IsDirty = true;
        }
    }
    private static bool CanDeleteRoomPicture(Models.Rent.Residentials.ListingPicture picUnit)
    {
        return picUnit is not null;
    }

    [RelayCommand(CanExecute = nameof(CanAddNewRoomPdfs))]
    public async Task AddNewRoomPdfs(List<string> filePathList)
    {
        if (filePathList is null) return;
        if (filePathList.Count == 0 || !IsPropertyUnitOwnership) return;
        var ownershipVersion = _ownershipVersion;

        Debug.WriteLine($"destDirectory={_listingDataDirectoryPath}  @AddNewRoomPdfs()");

        if (!Directory.Exists(_listingDataDirectoryPath))
        {
            Directory.CreateDirectory(_listingDataDirectoryPath);
        }

        //List<string> list = [];

        foreach (var filePath in filePathList)
        {
            if (string.IsNullOrEmpty(filePath.Trim()))
            {
                continue;
            }


            string extension = Path.GetExtension(filePath);
            if (!extension.Equals(".pdf", StringComparison.OrdinalIgnoreCase))
            {
                ShowAttachmentError($"PDFファイルを選択してください: {Path.GetFileName(filePath)}");
                continue;
            }

            var importedPaths = new List<string>();
            try
            {
                StorageFile sfile = await StorageFile.GetFileFromPathAsync(filePath);
                PdfDocument pdfDocument = await PdfDocument.LoadFromFileAsync(sfile);

                if (pdfDocument.PageCount > 0)
                {
                    using PdfPage pdfPage = pdfDocument.GetPage(0);
                    using var stream = new InMemoryRandomAccessStream();
                    var options = new PdfPageRenderOptions { DestinationWidth = 512 };
                    await pdfPage.RenderToStreamAsync(stream, options);

                    string newId = Guid.CreateVersion7().ToString("N");
                    string newThumbnailFilename = newId + ".bmp";
                    var thumbnailDestFilePath = Path.Combine(_listingDataDirectoryPath, newThumbnailFilename);

                    using (var destinationStream = new FileStream(thumbnailDestFilePath, FileMode.CreateNew, FileAccess.Write))
                    {
                        _incompleteAttachmentFiles.Add(thumbnailDestFilePath);
                        importedPaths.Add(thumbnailDestFilePath);
                        using var managedSourceStream = stream.AsStreamForRead();
                        await managedSourceStream.CopyToAsync(destinationStream);
                    }

                    string newFilename = newId + extension;
                    var pdfDestFilePath = Path.Combine(_listingDataDirectoryPath, newFilename);
                    using (var pdfSource = File.OpenRead(filePath))
                    using (var pdfDestination = new FileStream(pdfDestFilePath, FileMode.CreateNew, FileAccess.Write))
                    {
                        _incompleteAttachmentFiles.Add(pdfDestFilePath);
                        importedPaths.Add(pdfDestFilePath);
                        await pdfSource.CopyToAsync(pdfDestination);
                    }

                    var pdf = new Models.Rent.Residentials.ListingPdf(newId, newFilename, newThumbnailFilename, EntityStatus.New)
                    {
                        BasePath = _listingDataDirectoryPath,
                        ParentViewModel = this
                    };

                    Pdfs.Add(pdf);
                    _unsavedRoomPdfThumbnailFileList.Add(thumbnailDestFilePath);
                    _unsavedRoomPdfFileList.Add(pdfDestFilePath);
                    _incompleteAttachmentFiles.Remove(thumbnailDestFilePath);
                    _incompleteAttachmentFiles.Remove(pdfDestFilePath);
                    OpenBlobDirectoryCommand.NotifyCanExecuteChanged();
                    DeleteRoomPdfCommand.NotifyCanExecuteChanged();
                    IsDirty = true;
                }
                else
                {
                    ShowAttachmentError($"PDFにページがありません: {Path.GetFileName(filePath)}");
                }
            }
            catch (Exception ex) when (ex is IOException or UnauthorizedAccessException or System.Runtime.InteropServices.COMException or InvalidOperationException or ArgumentException)
            {
                if (ownershipVersion != _ownershipVersion || !IsPropertyUnitOwnership)
                {
                    foreach (var path in importedPaths)
                    {
                        AddPendingPdfCleanupPath(path);
                        _incompleteAttachmentFiles.Remove(path);
                    }
                    }
                    Debug.WriteLine($"Failed to import PDF '{filePath}': {ex}");
                ShowAttachmentError($"PDFを読み込めませんでした: {Path.GetFileName(filePath)}。{ex.Message}");
            }
        }
    }

    private static bool CanAddNewRoomPdfs()
    {
        return true;
    }

    [RelayCommand]
    private async Task OpenRoomPdf(Models.Rent.Residentials.ListingPdf pdf)
    {
        if (pdf is null || !Pdfs.Contains(pdf) || !IsEditorEnabled) return;

        try
        {
            var file = await StorageFile.GetFileFromPathAsync(Path.Combine(_listingDataDirectoryPath, pdf.PdfFilename));
            if (!await Windows.System.Launcher.LaunchFileAsync(file))
            {
                ShowAttachmentError("PDFを開けませんでした。PDFを開くアプリが設定されているか確認してください。");
            }
        }
        catch (Exception ex) when (ex is IOException or UnauthorizedAccessException or System.Runtime.InteropServices.COMException or InvalidOperationException or ArgumentException)
        {
            Debug.WriteLine($"Failed to open residential listing PDF: {ex}");
            ShowAttachmentError($"PDFを開けませんでした: {ex.Message}");
        }
    }

    [RelayCommand(CanExecute = nameof(CanDeleteRoomPdf))]
    private async Task DeleteRoomPdf(Models.Rent.Residentials.ListingPdf pdf)
    {
        if (pdf is null)
        {
            return;
        }

        if (_dialogService is null || await _dialogService.ShowDeleteConfirmationDialog("この募集物件の図面を削除しますか？") != Microsoft.UI.Xaml.Controls.ContentDialogResult.Primary)
        {
            return;
        }

        if (Pdfs.Remove(pdf))
        {
            _room.PdfsToBeDeleted.Add(pdf);
            IsDirty = true;
        }
    }
    private static bool CanDeleteRoomPdf(Models.Rent.Residentials.ListingPdf pdf)
    {
        return pdf is not null;
    }

    [RelayCommand(CanExecute = nameof(CanOpenBlobDirectory))]
    public void OpenBlobDirectory()
    {
        if (Directory.Exists(_listingDataDirectoryPath))
        {
            try
            {
                Process.Start("explorer.exe", _listingDataDirectoryPath);
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error opening folder: {ex}");
                InfoBarErrorMessage = $"フォルダーを開けませんでした。{ex.Message}";
                IsInfoBarErrorOpen = true;
            }
        }
    }
    private bool CanOpenBlobDirectory()
    {
        if (Directory.Exists(_listingDataDirectoryPath))
        {
            return true;
        }

        return false;
    }

    #endregion

    #region == Lessor ==

    [RelayCommand]
    public async Task AddLessor()
    {
        if (_dialogService is null)
        {
            Debug.WriteLine("_dlgService is null");
            return;
        }

        var lessor = await _dialogService.ShowLessorSelectDialog(new ViewModels.Dialogs.LessorSelectViewModel(_dataAccessService, _cts));

        if (lessor is not null)
        {
            var lessorId = lessor.Id;

            // Check if already exists
            var match = LessorsWrapper.FirstOrDefault(x => x.Person.Id.Equals(lessorId, StringComparison.Ordinal));
            if (match is not null)
            {
                Debug.WriteLine($"lessor {lessor.Name} already in the list.");
                return;
            }

            var res = await Task.Run(() => _dataAccessService.SelectRentLessorById(lessorId), _cts.Token);
            if (res.IsError)
            {
                Debug.WriteLine(
                    res.Error.Title + Environment.NewLine +
                    res.Error.Message + Environment.NewLine +
                    res.Error.Description + Environment.NewLine +
                    res.Error.Operation + Environment.NewLine +
                    res.Error.MethodName + Environment.NewLine +
                    res.Error.FullDump);
                ShowErrorInfoBar(res.Error);
                return;
            }

            if (res.Person is null)
            {
                Debug.WriteLine($"{lessorId} is null. Cannot open editor.");
                InfoBarErrorMessage = "選択した貸主を読み込めませんでした。";
                IsInfoBarErrorOpen = true;
                return;
            }


            var asdf = new Models.Rent.Residentials.PersonWrapperForListingViewModel(res.Person, this);

            _draftLessorDeletions.RemoveAll(person => person.Id == res.Person.Id);
            LessorsWrapper.Add(asdf);

            IsDirty = true;
        }

    }

    [RelayCommand(CanExecute = nameof(CanEditLessor))]
    private void EditLessor(Models.Rent.Residentials.PersonWrapperForListingViewModel lessor)
    {
        if (lessor.Person is not null)
        {
            var mainVm = App.GetService<MainViewModel>();
            if (mainVm.EditRentLessorCommand.CanExecute(lessor.Person as Models.Base.PersonBase))
            {
                mainVm.EditRentLessorCommand.Execute(lessor.Person as Models.Base.PersonBase);
            }
        }
    }
    private static bool CanEditLessor(Models.Rent.Residentials.PersonWrapperForListingViewModel lessor)
    {
        return lessor is not null;
    }

    [RelayCommand(CanExecute = nameof(CanDeleteLessor))]
    public async Task DeleteLessor(Models.Rent.Residentials.PersonWrapperForListingViewModel lessor)
    {
        Debug.WriteLine($"DeleteLessorCommand {lessor.Person.Name}");

        if (lessor is null)
        {
            return;
        }

        if (lessor.Person is null)
        {
            return;
        }

        if (LessorsWrapper.Remove(lessor))
        {
            IsDirty = true;
            if (!_draftLessorDeletions.Any(person => person.Id == lessor.Person.Id)) _draftLessorDeletions.Add(lessor.Person);
        }
    }
    private static bool CanDeleteLessor(Models.Rent.Residentials.PersonWrapperForListingViewModel lessor)
    {
        return lessor is not null;
    }


    #endregion

    #region = Broker ==

    [RelayCommand]
    public async Task AddBroker()
    {
        if (_dialogService is null)
        {
            Debug.WriteLine("_dialogService is null");
            return;
        }

        var selectedBroker = await _dialogService.ShowBrokerSelectDialog(
            new ViewModels.Dialogs.BrokerSelectViewModel(_dataAccessService, _cts));

        if (selectedBroker is null ||
            BrokersWrapper.Any(item => item.Person.Id == selectedBroker.Id))
        {
            return;
        }

        var result = await Task.Run(() => _dataAccessService.SelectBrokerById(selectedBroker.Id), _cts.Token);
        if (result.IsError)
        {
            Debug.WriteLine(
                result.Error.Title + Environment.NewLine +
                result.Error.Message + Environment.NewLine +
                result.Error.Description + Environment.NewLine +
                result.Error.Operation + Environment.NewLine +
                result.Error.MethodName + Environment.NewLine +
                result.Error.FullDump);
            ShowErrorInfoBar(result.Error);
            return;
        }

        if (result.Person is null)
        {
            Debug.WriteLine($"Could not load broker '{selectedBroker.Id}'.");
            InfoBarErrorMessage = "選択した宅建業者を読み込めませんでした。";
            IsInfoBarErrorOpen = true;
            return;
        }

        _draftBrokerDeletions.RemoveAll(person => person.Id == result.Person.Id);
        BrokersWrapper.Add(
            new Models.Rent.Residentials.PersonWrapperForListingViewModel(
                result.Person,
                this));

        IsDirty = true;
    }

    [RelayCommand(CanExecute = nameof(CanEditBroker))]
    private void EditBroker(
        Models.Rent.Residentials.PersonWrapperForListingViewModel? broker)
    {
        if (broker?.Person is null)
        {
            return;
        }

        var mainViewModel = App.GetService<MainViewModel>();
        if (mainViewModel.EditBrokerCommand.CanExecute(broker.Person))
        {
            mainViewModel.EditBrokerCommand.Execute(broker.Person);
        }
    }

    private static bool CanEditBroker(
        Models.Rent.Residentials.PersonWrapperForListingViewModel? broker)
    {
        return broker?.Person is not null;
    }

    [RelayCommand(CanExecute = nameof(CanDeleteBroker))]
    private void DeleteBroker(
        Models.Rent.Residentials.PersonWrapperForListingViewModel? broker)
    {
        if (broker?.Person is null)
        {
            return;
        }

        if (BrokersWrapper.Remove(broker))
        {
            _room.Brokers.Remove(broker.Person);
            if (!_draftBrokerDeletions.Any(person => person.Id == broker.Person.Id)) _draftBrokerDeletions.Add(broker.Person);
            IsDirty = true;
        }
    }

    private static bool CanDeleteBroker(
        Models.Rent.Residentials.PersonWrapperForListingViewModel? broker)
    {
        return broker?.Person is not null;
    }


    #endregion

    [RelayCommand]
    private void OpenPropertyEditorWindow()
    {
        var vm = App.GetService<ViewModels.MainViewModel>();

        if (vm.EditRentResidentialFromIdCommand.CanExecute(_room.PropertyId))
        {
            //await vm.EditRentResidentialFromIdCommand(_room.PropertyId);
            vm.EditRentResidentialFromIdCommand.Execute(_room.PropertyId);
        }
    }

    #endregion


}
