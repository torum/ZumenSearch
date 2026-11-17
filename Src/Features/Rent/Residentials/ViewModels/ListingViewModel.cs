using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.Messaging;
using Microsoft.UI.Xaml.Controls;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Globalization;
using System.Text.RegularExpressions;
using Windows.Data.Pdf;
using Windows.Storage;
using Windows.Storage.Streams;
using ZumenSearch.Models;
using ZumenSearch.Models.Messenger;
using ZumenSearch.Models.Rent.Residentials;
using ZumenSearch.Services.Contracts;
using ZumenSearch.Views.Services.Contracts;

namespace ZumenSearch.ViewModels.Rent.Residentials;

/*
 * TODO: 
 * Gather validation logic in one place. Currently, validation is scattered across various methods. Make sure it is consistent with PropertyViewModel.
 * Change sevirity to Error for simple error messages.
 * Add try catch to async and file IO operations and show error message in infobar.
 * Add IsBusy flag to disable the UI commands when some async or IO operations is in progress.
 */

public sealed partial class ListingViewModel : ObservableRecipient, IRecipient<PropertyUpdatedMessage>, IRecipient<PropertyIsUnitOwnershipChangedMessage>, IRecipient<LessorUpdatedMessage>, IRecipient<BrokerUpdatedMessage>, IRecipient<LessorDeletedMessage>, IRecipient<BrokerDeletedMessage>, IDisposable
{
    #region == Private variables ==

    private const string BasicPageName = "ZumenSearch.Views.Rent.Residentials.Listing.BasicPage";
    private readonly string _id = string.Empty;

    // This object is the actual entity being edited and sent to DataAccessService and other window.
    // Populate values on Loading and apply changes to this object in Save().
    private readonly Models.Rent.Residentials.Listing _listing;

    private readonly string _listingDataDirectoryPath = string.Empty;

    // Tmp file list to hold unsaved picture files. (if entry is not saved, delete on close)
    private readonly List<string> _unsavedRoomPictureFileList = [];
    private readonly List<string> _unsavedRoomPdfFileList = [];
    private readonly List<string> _unsavedRoomPdfThumbnailFileList = [];

    private bool _disposed;
    private bool _isClosing;
    private bool _propertyBusy;
    //private bool _isBusyFileOperation;
    //private int _pendingPickers;
    //private TaskCompletionSource? _pickersCompleted;
    private readonly Queue<Action> _deferredMessages = new();

    private readonly CancellationTokenSource _cts = new();

    #endregion

    #region == Services ==

    private readonly IDataAccessService _dataAccessService;
    private readonly IDispatcherService _dispatcherService;
    private readonly IDialogGenericService? _dialogService;
    private readonly INavigationGenericService _navigationService;

    #endregion

    public ListingViewModel(Models.Rent.Residentials.Listing listing, bool isDuplicate, INavigationGenericService navigationService, IDialogGenericService dialogService, IDispatcherService dispatcherService, IDataAccessService dataAccessService)
    {
        _listing = listing;
        _id = listing.Id;

        _navigationService = navigationService;
        _dialogService = dialogService;

        _dispatcherService = dispatcherService;
        _dataAccessService = dataAccessService;

        _listingDataDirectoryPath = System.IO.Path.Combine(App.PropertyBlobDataFolder, _listing.PropertyId, _listing.Id);

        if (!Directory.Exists(_listingDataDirectoryPath))
        {
            Directory.CreateDirectory(_listingDataDirectoryPath);
        }

        PopulateValues(isDuplicate);

        // Reset errors
        IsNameHasError = false;

        //HasErrors = false;

        _listing.SetIsModified(isDuplicate);
        IsDirty = isDuplicate;

        // Ready to receive messages.
        // Add matching unregistrations in Dispose().
        WeakReferenceMessenger.Default.Register<PropertyUpdatedMessage>(this);
        WeakReferenceMessenger.Default.Register<PropertyIsUnitOwnershipChangedMessage>(this);
        WeakReferenceMessenger.Default.Register<LessorUpdatedMessage>(this);
        WeakReferenceMessenger.Default.Register<BrokerUpdatedMessage>(this);
        WeakReferenceMessenger.Default.Register<LessorDeletedMessage>(this);
        WeakReferenceMessenger.Default.Register<BrokerDeletedMessage>(this);
    }

    #region == Properties ==

    #region == Internal flags ==

    [ObservableProperty]
    [NotifyCanExecuteChangedFor(nameof(SaveCommand))]
    public partial bool IsDirty { get; private set; }

    public bool IsPropertyUnitLevelOwnership
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

    // TODO: Consider removing following properties. 
    public bool IsSaving { get; private set; }
    public bool IsClosing => _isClosing;
    public bool IsPropertyBusy => _propertyBusy;
    //public bool IsDiscardPending => _discardPending;
    public bool IsEditorEnabled => !IsSaving && !_propertyBusy && !_isClosing;// && !_discardPending;
    //public bool HasPendingEditorOperations => _pendingPickers > 0 || PendingEditorTasks().Count > 0;
    public bool HasPendingEditorOperations => IsEditorEnabled;//tmp

    #endregion

    #region == Window and other page related stuff ==

    public string WindowTitle
    {
        get
        {
            var str = $"{field}：{_listing.PropertyName}";

            if (!string.IsNullOrEmpty(Name))
            {
                str = $"{str}：{Name}";
            }

            if (_listing.Status == EntityStatus.New)
            {
                str = $"{str}：新規";
            }
            else
            {
                str = $"{str}：編集";
            }
            /*
            if (string.IsNullOrEmpty(_listing.PropertyName))
            {
                string str;

                if (_listing.ListingStatus == EnumListingStatus.New)
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
                //var str = $"{field}：{_listing.PropertyName}";

                if (!string.IsNullOrEmpty(Name))
                {
                    str = $"{str}：{Name}";
                }

                if (_listing.ListingStatus == EnumListingStatus.New)
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
    public partial string InfoBarErrorTitle { get; set; } = string.Empty;

    [ObservableProperty]
    public partial InfoBarSeverity InfoBarSeverity { get; set; } = InfoBarSeverity.Warning;

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

                //ValidateName();

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
            if (value is null) return;
            if (SelectedFloorPlanType.Key == value.Key)
            {
                field = value;
                return;
            }

            field = value;
            OnPropertyChanged();
            IsDirty = true;
            /*
             * DO not use SetProperty. These are reference types without value equality. 
             * During Bindings.Update(), the ComboBox can write null or a different instance with the same key back to the view model. 
             * SetProperty sees a reference change and sets IsDirty = true.
            if (SetProperty(ref field, value))
            {
                IsDirty = true;
            }
            */
        }
    }

    public IReadOnlyList<ListingFloorPlanExtraTypeLabel> FloorPlanExtraTypes { get; } = ListingFloorPlanExtraTypeLabel.GetAll();

    public ListingFloorPlanExtraTypeLabel SelectedFloorPlanExtraType
    {
        get => field ?? new(FloorPlanExtraType.Unspecified);
        set
        {
            if (value is null) return;
            if (SelectedFloorPlanExtraType.Key == value.Key)
            {
                field = value;
                return;
            }

            field = value;
            OnPropertyChanged();
            IsDirty = true;
            /*
             * DO not use SetProperty. These are reference types without value equality. 
             * During Bindings.Update(), the ComboBox can write null or a different instance with the same key back to the view model. 
             * SetProperty sees a reference change and sets IsDirty = true.
            if (SetProperty(ref field, value))
            {
                IsDirty = true;
            }
            */
        }
    }

    public IReadOnlyList<ListingFloorGroundTypeLabel> FloorGroundTypes { get; } = ListingFloorGroundTypeLabel.GetAll();

    public ListingFloorGroundTypeLabel SelectedFloorGroundType
    {
        get => field ?? new(FloorGroundType.AboveGround);
        set
        {
            if (value is null) return;
            if (SelectedFloorGroundType.Key == value.Key)
            {
                field = value;
                return;
            }

            field = value;
            OnPropertyChanged();
            IsDirty = true;
            /*
             * DO not use SetProperty. These are reference types without value equality. 
             * During Bindings.Update(), the ComboBox can write null or a different instance with the same key back to the view model. 
             * SetProperty sees a reference change and sets IsDirty = true.
            if (SetProperty(ref field, value))
            {
                IsDirty = true;
            }
            */
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
            if (value is null) return;
            if (SelectedExposureDirectionType.Key == value.Key)
            {
                field = value;
                return;
            }

            field = value;
            OnPropertyChanged();
            IsDirty = true;
            /*
             * DO not use SetProperty. These are reference types without value equality. 
             * During Bindings.Update(), the ComboBox can write null or a different instance with the same key back to the view model. 
             * SetProperty sees a reference change and sets IsDirty = true.
            if (SetProperty(ref field, value))
            {
                IsDirty = true;
            }
            */
        }
    }

    public IReadOnlyList<ListingAvailabilityStatusTypeLabel> AvailabilityStatusTypes { get; } = ListingAvailabilityStatusTypeLabel.GetAll();

    public ListingAvailabilityStatusTypeLabel SelectedAvailabilityStatusType
    {
        get => field ?? new(AvailabilityStatusType.Unspecified);
        set
        {
            if (value is null) return;
            if (SelectedAvailabilityStatusType.Key == value.Key)
            {
                field = value;
                return;
            }

            field = value;
            OnPropertyChanged();
            IsDirty = true;
            /*
             * DO not use SetProperty. These are reference types without value equality. 
             * During Bindings.Update(), the ComboBox can write null or a different instance with the same key back to the view model. 
             * SetProperty sees a reference change and sets IsDirty = true.
            if (SetProperty(ref field, value))
            {
                IsDirty = true;
            }
            */
        }
    }

    public IReadOnlyList<ListingAvailabilityMonthTypeLabel> AvailabilityMonthTypes { get; } = ListingAvailabilityMonthTypeLabel.GetAll();

    public ListingAvailabilityMonthTypeLabel SelectedAvailabilityMonthType
    {
        get => field ?? new(AvailabilityMonthType.Unspecified);
        set
        {
            if (value is null) return;
            if (SelectedAvailabilityMonthType.Key == value.Key)
            {
                field = value;
                return;
            }

            field = value;
            OnPropertyChanged();
            IsDirty = true;
            /*
             * DO not use SetProperty. These are reference types without value equality. 
             * During Bindings.Update(), the ComboBox can write null or a different instance with the same key back to the view model. 
             * SetProperty sees a reference change and sets IsDirty = true.
            if (SetProperty(ref field, value))
            {
                IsDirty = true;
            }
            */
        }
    }

    public IReadOnlyList<ListingAvailabilityPeriodTypeLabel> AvailabilityPeriodTypes { get; } = ListingAvailabilityPeriodTypeLabel.GetAll();

    public ListingAvailabilityPeriodTypeLabel SelectedAvailabilityPeriodType
    {
        get => field ?? new(AvailabilityPeriodType.Unspecified);
        set
        {
            if (value is null) return;
            if (SelectedAvailabilityPeriodType.Key == value.Key)
            {
                field = value;
                return;
            }

            field = value;
            OnPropertyChanged();
            IsDirty = true;
            /*
             * DO not use SetProperty. These are reference types without value equality. 
             * During Bindings.Update(), the ComboBox can write null or a different instance with the same key back to the view model. 
             * SetProperty sees a reference change and sets IsDirty = true.
            if (SetProperty(ref field, value))
            {
                IsDirty = true;
            }
            */
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

    // 不動産番号 (13桁)
    public string FudousanId
    {
        get => field ?? string.Empty;
        private set
        {
            if (SetProperty(ref field, value))
            {
                // No dirty flag here. This is set from property level and not user editable.
            }
        }
    }

    // 特定コード（４桁）建物全体は0000
    public string FudousanIdAdditionalCode
    {
        get => field ?? string.Empty;
        set
        {
            if (value is null)
            {
                return;
            }

            var text = Helpers.Common.NormalizeFullWidthCharacters(value).Trim().ToUpperInvariant();
            if (field == text || !Regex.IsMatch(text, @"\A[A-Z0-9]{0,4}\z"))
            {
                return;
            }

            field = text;
            IsDirty = true;
            OnPropertyChanged();
        }
    } = "";

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
        if (property.Value is not Models.Rent.Residentials.Property building || !building.Id.Equals(_listing.PropertyId, StringComparison.Ordinal)) return;

        _listing.SetPropertyStatus(building.Status);
        _listing.SetPropertyName(building.Name);
        SetOwnershipVisibility(building.IsUnitLevelOwnership);

        FudousanId = building.FudousanId;
        _listing.SetFudousanId(building.FudousanId);

        // Update window title with dummy string.
        WindowTitle = string.Empty;

        /*
        if (property.Value is not Models.Rent.Residentials.Property residential || !residential.Id.Equals(_listing.PropertyId, StringComparison.Ordinal)) return;
        var status = residential.Status;
        var name = residential.Name;
        var ownership = residential.IsUnitLevelOwnership;
        var parent = App.GetService<ViewModels.MainViewModel>().RentResidentialEditorList.FirstOrDefault(editor => editor.Id == _listing.PropertyId)?.ViewModel;
        var committed = status == EntityStatus.Saved && residential.ListingIds.Contains(_id, StringComparer.Ordinal) ? parent?.GetCommittedListingDraft(_id) : null;
        if (DeferMessage(() => ApplyPropertyUpdate(status, name, ownership, committed))) return;
        ApplyPropertyUpdate(status, name, ownership, committed);
        */
    }

    /*
    private void ApplyPropertyUpdate(EntityStatus status, string name, bool ownership, ResidentialListingDraft? committed)
    {
        _listing.SetPropertyStatus(status);
        _listing.SetPropertyName(name);
        SetOwnershipVisibility(ownership);

        if (_handedOffToNewParent && status == EntityStatus.Saved && committed is not null)
        {
            _handedOffToNewParent = false;
            _listing.SetStatus(EntityStatus.Saved);
            CaptureCommittedAttachments(committed);
            _unsavedRoomPictureFileList.RemoveAll(_committedAttachmentPaths.Contains);
            _unsavedRoomPdfFileList.RemoveAll(_committedAttachmentPaths.Contains);
            _unsavedRoomPdfThumbnailFileList.RemoveAll(_committedAttachmentPaths.Contains);

            var pictureIds = committed.Pictures.Select(item => item.Id).ToHashSet(StringComparer.Ordinal);
            var pdfIds = committed.Pdfs.Select(item => item.Id).ToHashSet(StringComparer.Ordinal);
            foreach (var picture in Pictures.Concat(_listing.PicturesToBeDeleted).Where(item => pictureIds.Contains(item.Id))) picture.SetStatus(EntityStatus.Saved);
            foreach (var pdf in Pdfs.Concat(_listing.PdfsToBeDeleted).Where(item => pdfIds.Contains(item.Id))) pdf.SetStatus(EntityStatus.Saved);
            foreach (var deleted in _listing.PicturesToBeDeleted.Where(item => committed.PicturesToBeDeleted.Any(saved => saved.Id == item.Id && saved.ImageFilename == item.ImageFilename)).ToList()) _listing.PicturesToBeDeleted.Remove(deleted);
            foreach (var deleted in _listing.PdfsToBeDeleted.Where(item => committed.PdfsToBeDeleted.Any(saved => saved.Id == item.Id && saved.PdfFilename == item.PdfFilename && saved.ThumbnailFilename == item.ThumbnailFilename)).ToList()) _listing.PdfsToBeDeleted.Remove(deleted);
            foreach (var deleted in _listing.LessorsToBeDeleted.Where(item => committed.LessorsToBeDeleted.Contains(item.Id, StringComparer.Ordinal)).ToList()) _listing.LessorsToBeDeleted.Remove(deleted);
            foreach (var deleted in _listing.BrokersToBeDeleted.Where(item => committed.BrokersToBeDeleted.Contains(item.Id, StringComparer.Ordinal)).ToList()) _listing.BrokersToBeDeleted.Remove(deleted);
        }
        else if (_handedOffToNewParent && status == EntityStatus.Saved)
        {
            IsDirty = true;
            ShowAttachmentError("建物の保存に募集物件が含まれていません。内容を確認してから募集物件を保存してください。");
        }
        WindowTitle = string.Empty;
    }
    */

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
        if (!string.Equals(isUnitOwnership.PropertyId, _listing.PropertyId, StringComparison.Ordinal)) return;

        if (DeferMessage(() => Receive(isUnitOwnership))) return;
        
        SetOwnershipVisibility(isUnitOwnership.Value);
    }

    public void SetOwnershipVisibility(bool isUnitOwnership)
    {
        IsPropertyUnitLevelOwnership = isUnitOwnership;
        _listing.SetIsPropertyUnitLevelOwnership(isUnitOwnership);

        if (IsPropertyUnitLevelOwnership)
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

    /*
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
    */

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

    /*
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
    */

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

    public void DiscardChanges()
    {
        //_isBusyFileOperation = true;
        OnPropertyChanged(nameof(IsEditorEnabled));
        SaveCommand.NotifyCanExecuteChanged();

        DiscardUnsavedFiles();

        //_isBusyFileOperation = false;
        IsDirty = false;
        return;
    }

    public void Dispose()
    {
        if (_disposed)
        {
            return;
        }

        _disposed = true;

        foreach (var picture in Pictures)
        {
            picture.PropertyChanged -= OnPicturePropertyChanged;
        }

        foreach (var pdf in Pdfs)
        {
            pdf.PropertyChanged -= OnPdfPropertyChanged;
        }

        WeakReferenceMessenger.Default.Unregister<PropertyUpdatedMessage>(this);
        WeakReferenceMessenger.Default.Unregister<PropertyIsUnitOwnershipChangedMessage>(this);
        WeakReferenceMessenger.Default.Unregister<LessorUpdatedMessage>(this);
        WeakReferenceMessenger.Default.Unregister<BrokerUpdatedMessage>(this);
        WeakReferenceMessenger.Default.Unregister<LessorDeletedMessage>(this);
        WeakReferenceMessenger.Default.Unregister<BrokerDeletedMessage>(this);

        _cts.Cancel();
        _cts.Dispose();

        GC.SuppressFinalize(this);

        Debug.WriteLine($"ListingViewModel.Dispose() called. Id={_id}");
    }

    #endregion

    #region == Private Methods ==

    private void SetContractValue<T>(ref T storage, T value, [System.Runtime.CompilerServices.CallerMemberName] string? propertyName = null)
    {
        if (SetProperty(ref storage, value, propertyName))
        {
            IsDirty = true;
        }
    }

    private void ShowSimpleErrorInfoBar(string title, string message, InfoBarSeverity severity = InfoBarSeverity.Warning)
    {
        InfoBarErrorTitle = title;
        InfoBarErrorMessage = message;
        InfoBarSeverity = severity;
        IsInfoBarErrorOpen = true;
    }

    private void ShowErrorInfoBar(Models.ErrorInfo err)
    {
        InfoBarErrorTitle = err.Title;
        InfoBarErrorMessage =
            err.Message + Environment.NewLine +
            err.Description + Environment.NewLine +
            err.Operation + Environment.NewLine +
            err.MethodName;
        InfoBarSeverity = InfoBarSeverity.Error;
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
                ShowSimpleErrorInfoBar("入力項目に誤りがあります","賃料は0以上の整数で入力してください。");
                return false;
            }

            return true;
        }

        if (!long.TryParse(text, NumberStyles.None, CultureInfo.InvariantCulture, out _))
        {
            ShowSimpleErrorInfoBar("入力項目に誤りがあります","賃料は0以上の整数で入力してください。");
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
                ShowSimpleErrorInfoBar("入力項目に誤りがあります",$"{label}は0以上の数値で入力してください。");
                return false;
            }
        }

        return true;
    }

    private bool ValidateName()
    {
        if (string.IsNullOrWhiteSpace(Name))
        {
            ShowSimpleErrorInfoBar("入力項目に誤りがあります","部屋名（必須項目）が入力されていません。保存出来ませんでした。");
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
            ShowSimpleErrorInfoBar("入力項目に誤りがあります","専有面積は0以上の数値で入力してください。");
            return false;
        }

        var floorText = Helpers.Common.NormalizeFullWidthCharacters(FloorNumber.Trim());
        if (!string.IsNullOrEmpty(floorText))
        {
            if (!int.TryParse(floorText, NumberStyles.Integer, CultureInfo.InvariantCulture, out var parsedFloor) || parsedFloor <= 0)
            {
                ShowSimpleErrorInfoBar("入力項目に誤りがあります","所在階は1以上の整数で入力してください。");
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

        if (_listing is null)
        {
            Debug.WriteLine("_listing is null. Can't set room.");
            return;
        }

        _listing.SetName(Name);

        _listing.SetFloorPlanTypeLabel(SelectedFloorPlanType);
        _listing.SetFloorPlanExtraTypeLabel(SelectedFloorPlanExtraType);
        _listing.SetFloorGroundTypeLabel(SelectedFloorGroundType);
        _listing.SetFloorExclusiveAreaSqm(FloorExclusiveAreaSqm);
        _listing.SetFloorNumber(FloorNumber);
        _listing.SetIsCornerRoom(IsCornerRoom);
        _listing.SetExposureDirectionTypeLabel(SelectedExposureDirectionType);
        _listing.SetAvailabilityStatusTypeLabel(SelectedAvailabilityStatusType);
        _listing.SetAvailabilityMonthTypeLabel(SelectedAvailabilityMonthType);
        _listing.SetAvailabilityPeriodTypeLabel(SelectedAvailabilityPeriodType);
        _listing.SetOccupancyStatusLabel(SelectedOccupancyStatus);
        _listing.SetIsAvailableForImmediateMoveIn(IsAvailableForImmediateMoveIn);
        _listing.SetIsAvailableForRentNow(IsAvailableForRentNow);
        _listing.SetOccupancyStatusCheckedAt(OccupancyStatusCheckedAt);
        _listing.SetRemarks(Remarks);
        _listing.SetHasAirConditioner(HasAirConditioner);
        _listing.SetHasWoodFlooring(HasWoodFlooring);
        _listing.SetHasVideoIntercom(HasVideoIntercom);
        _listing.SetHasBathReheating(HasBathReheating);
        _listing.SetHasGasStove(HasGasStove);
        _listing.SetHasInductionCooktop(HasInductionCooktop);
        _listing.SetHasMultipleBurners(HasMultipleBurners);
        _listing.SetHasSystemKitchen(HasSystemKitchen);
        _listing.SetHasLoft(HasLoft);
        _listing.SetHasIndoorWashingMachineSpace(HasIndoorWashingMachineSpace);
        _listing.SetHasBathroomDryer(HasBathroomDryer);
        _listing.SetHasSeparateBathAndToilet(HasSeparateBathAndToilet);
        _listing.SetHasSeparateWashbasin(HasSeparateWashbasin);
        _listing.SetHasWarmWaterToiletSeat(HasWarmWaterToiletSeat);
        _listing.SetApplianceRemarks(ApplianceRemarks);
        _listing.SetChinryouFromString(Chinryou);
        _listing.SetManagementFee(ManagementFee);
        _listing.SetSecurityDeposit(SecurityDeposit);
        _listing.SetKeyMoney(KeyMoney);
        _listing.SetRenewalFee(RenewalFee);
        _listing.SetRecontractFee(RecontractFee);
        _listing.SetContractPeriodYears(ContractPeriodYears);
        _listing.SetManagementFeeUnitTypeLabel(SelectedManagementFeeUnit);
        _listing.SetSecurityDepositUnitTypeLabel(SelectedSecurityDepositUnit);
        _listing.SetKeyMoneyUnitTypeLabel(SelectedKeyMoneyUnit);
        _listing.SetRenewalFeeUnitTypeLabel(SelectedRenewalFeeUnit);
        _listing.SetRecontractFeeUnitTypeLabel(SelectedRecontractFeeUnit);
        _listing.SetContractTypeLabel(SelectedContractType);
        _listing.SetGuarantorCompany(GuarantorCompany);
        _listing.SetHousingInsurance(HousingInsurance);
        _listing.SetOtherFees(OtherFees);
        _listing.SetPetFriendly(PetFriendly);
        _listing.SetTwoPersonOccupancy(TwoPersonOccupancy);
        _listing.SetNoGuarantorNeeded(NoGuarantorNeeded);
        _listing.SetOfficeUseAllowed(OfficeUseAllowed);
        _listing.SetOtherConditions(OtherConditions);
        _listing.SetContractRemarks(ContractRemarks);
        _listing.SetHandlingTypeLabel(SelectedHandlingType);
        _listing.SetTransactionTypeLabel(SelectedTransactionType);
        _listing.SetLessorCommission(LessorCommission);
        _listing.SetLesseeCommission(LesseeCommission);
        _listing.SetListingBrokerCommission(ListingBrokerCommission);
        _listing.SetTenantBrokerCommission(TenantBrokerCommission);
        _listing.SetLessorCommissionUnitTypeLabel(SelectedLessorCommissionUnit);
        _listing.SetLesseeCommissionUnitTypeLabel(SelectedLesseeCommissionUnit);
        _listing.SetListingBrokerCommissionUnitTypeLabel(SelectedListingBrokerCommissionUnit);
        _listing.SetTenantBrokerCommissionUnitTypeLabel(SelectedTenantBrokerCommissionUnit);
        _listing.SetTransactionRemarks(TransactionRemarks);

        // Recreate the Pictures collection in _listing to ensure it matches the current Pictures collection in the ViewModel.
        //_listing.Pictures = Pictures;
        _listing.Pictures.Clear();
        foreach (var item in Pictures)
        {
            var newItem = item.Clone();
            newItem.BasePath = item.BasePath;//System.IO.Path.Combine(System.IO.Path.Combine(System.IO.Path.Combine(App.PropertyBlobDataFolder, _listing.PropertyId), _listing.Id));
            //newItem.ParentViewModel = this;
            newItem.SetIsModified(item.IsModified); // Needed this.
            _listing.Pictures.Add(newItem);

            item.PropertyChanged -= OnPicturePropertyChanged;
        }

        // Recreate the Pdfs collection in _listing to ensure it matches the current Pdfs collection in the ViewModel.
        //_listing.Pdfs = Pdfs;
        _listing.Pdfs.Clear();
        foreach (var item in Pdfs)
        {
            var newItem = item.Clone();
            newItem.BasePath = item.BasePath;//_listingDataDirectoryPath;
            //newItem.ParentViewModel = this;
            newItem.SetIsModified(item.IsModified);
            _listing.Pdfs.Add(newItem);

            item.PropertyChanged -= OnPdfPropertyChanged;
        }

        _listing.Lessors.Clear();
        foreach (var wrapper in LessorsWrapper)
        {
            _listing.Lessors.Add(wrapper.Person);
            //foreach (var deleted in _listing.LessorsToBeDeleted.Where(person => person.Id == wrapper.Person.Id).ToList()) _listing.LessorsToBeDeleted.Remove(deleted);
        }
        _listing.Brokers.Clear();
        foreach (var wrapper in BrokersWrapper)
        {
            _listing.Brokers.Add(wrapper.Person);
            //foreach (var deleted in _listing.BrokersToBeDeleted.Where(person => person.Id == wrapper.Person.Id).ToList()) _listing.BrokersToBeDeleted.Remove(deleted);
        }
        //foreach (var deleted in _draftLessorDeletions.Where(person => !_listing.LessorsToBeDeleted.Any(item => item.Id == person.Id))) _listing.LessorsToBeDeleted.Add(deleted);
        //foreach (var deleted in _draftBrokerDeletions.Where(person => !_listing.BrokersToBeDeleted.Any(item => item.Id == person.Id))) _listing.BrokersToBeDeleted.Add(deleted);
    }

    private void PopulateValues(bool isDuplicate)
    {
        if (_listing is null)
        {
            return;
        }

        if (isDuplicate)
        {
            _listing.SetName($"複製_{_listing.Name}");
        }

        Name = _listing.Name; // Set the value to trigger the setter logic if needed.

        // This toggles the visibility of certain fields based on the ownership level of the property.
        IsPropertyUnitLevelOwnership = _listing.IsPropertyUnitLevelOwnership;

        SelectedFloorPlanType = _listing.FloorPlan;
        SelectedFloorPlanExtraType = _listing.FloorPlanExtra;
        SelectedFloorGroundType = _listing.FloorGround;
        FloorExclusiveAreaSqm = _listing.FloorExclusiveAreaSqm;
        FloorNumber = _listing.FloorNumber;
        IsCornerRoom = _listing.IsCornerRoom;
        SelectedExposureDirectionType = _listing.SelectedExposureDirectionType;
        SelectedAvailabilityStatusType = _listing.SelectedAvailabilityStatusType;
        SelectedAvailabilityMonthType = _listing.SelectedAvailabilityMonthType;
        SelectedAvailabilityPeriodType = _listing.SelectedAvailabilityPeriodType;
        SelectedOccupancyStatus = _listing.SelectedOccupancyStatus;
        IsAvailableForImmediateMoveIn = _listing.IsAvailableForImmediateMoveIn;
        IsAvailableForRentNow = _listing.IsAvailableForRentNow;
        OccupancyStatusCheckedAt = _listing.OccupancyStatusCheckedAt;

        Remarks = _listing.Remarks;
        HasAirConditioner = _listing.HasAirConditioner;
        HasWoodFlooring = _listing.HasWoodFlooring;
        HasVideoIntercom = _listing.HasVideoIntercom;
        HasBathReheating = _listing.HasBathReheating;
        HasGasStove = _listing.HasGasStove;
        HasInductionCooktop = _listing.HasInductionCooktop;
        HasMultipleBurners = _listing.HasMultipleBurners;
        HasSystemKitchen = _listing.HasSystemKitchen;
        HasLoft = _listing.HasLoft;
        HasIndoorWashingMachineSpace = _listing.HasIndoorWashingMachineSpace;
        HasBathroomDryer = _listing.HasBathroomDryer;
        HasSeparateBathAndToilet = _listing.HasSeparateBathAndToilet;
        HasSeparateWashbasin = _listing.HasSeparateWashbasin;
        HasWarmWaterToiletSeat = _listing.HasWarmWaterToiletSeat;
        ApplianceRemarks = _listing.ApplianceRemarks;

        Chinryou = _listing.Chinryou == 0 ? string.Empty : _listing.Chinryou.ToString(CultureInfo.InvariantCulture);
        ManagementFee = _listing.ManagementFee;
        SecurityDeposit = _listing.SecurityDeposit;
        KeyMoney = _listing.KeyMoney;
        RenewalFee = _listing.RenewalFee;
        RecontractFee = _listing.RecontractFee;
        ContractPeriodYears = _listing.ContractPeriodYears;
        SelectedManagementFeeUnit = FeeUnitTypes.First(x => x.Key == _listing.ManagementFeeUnit.Key);
        SelectedSecurityDepositUnit = FeeUnitTypes.First(x => x.Key == _listing.SecurityDepositUnit.Key);
        SelectedKeyMoneyUnit = FeeUnitTypes.First(x => x.Key == _listing.KeyMoneyUnit.Key);
        SelectedRenewalFeeUnit = FeeUnitTypes.First(x => x.Key == _listing.RenewalFeeUnit.Key);
        SelectedRecontractFeeUnit = FeeUnitTypes.First(x => x.Key == _listing.RecontractFeeUnit.Key);
        SelectedContractType = ContractTypes.First(x => x.Key == _listing.ContractType.Key);
        GuarantorCompany = _listing.GuarantorCompany;
        HousingInsurance = _listing.HousingInsurance;
        OtherFees = _listing.OtherFees;
        PetFriendly = _listing.PetFriendly;
        TwoPersonOccupancy = _listing.TwoPersonOccupancy;
        NoGuarantorNeeded = _listing.NoGuarantorNeeded;
        OfficeUseAllowed = _listing.OfficeUseAllowed;
        OtherConditions = _listing.OtherConditions;
        ContractRemarks = _listing.ContractRemarks;
        SelectedHandlingType = HandlingTypes.First(x => x.Key == _listing.HandlingType.Key);
        SelectedTransactionType = TransactionTypes.First(x => x.Key == _listing.TransactionType.Key);
        LessorCommission = _listing.LessorCommission;
        LesseeCommission = _listing.LesseeCommission;
        ListingBrokerCommission = _listing.ListingBrokerCommission;
        TenantBrokerCommission = _listing.TenantBrokerCommission;
        SelectedLessorCommissionUnit = CommissionUnitTypes.First(x => x.Key == _listing.LessorCommissionUnit.Key);
        SelectedLesseeCommissionUnit = CommissionUnitTypes.First(x => x.Key == _listing.LesseeCommissionUnit.Key);
        SelectedListingBrokerCommissionUnit = CommissionUnitTypes.First(x => x.Key == _listing.ListingBrokerCommissionUnit.Key);
        SelectedTenantBrokerCommissionUnit = CommissionUnitTypes.First(x => x.Key == _listing.TenantBrokerCommissionUnit.Key);
        TransactionRemarks = _listing.TransactionRemarks;

        // Pictures (don't use directly)
        /*
        Pictures = _listing.Pictures;
        foreach (var item in Pictures)
        {
            item.BasePath = System.IO.Path.Combine(System.IO.Path.Combine(System.IO.Path.Combine(App.PropertyBlobDataFolder, _listing.PropertyId), _listing.Id));
            item.ParentViewModel = this;
            item.SetIsModified(false); // Needed this.
            item.PropertyChanged += OnPicturePropertyChanged;
        }
        */
        Pictures.Clear();
        foreach (var item in _listing.Pictures)
        {
            var internalItem = item.Clone();
            internalItem.BasePath = System.IO.Path.Combine(System.IO.Path.Combine(System.IO.Path.Combine(App.PropertyBlobDataFolder, _listing.PropertyId), _listing.Id));
            internalItem.ParentViewModel = this;
            if (isDuplicate)
            {
                var newFilePath = System.IO.Path.Combine(internalItem.BasePath, internalItem.ImageFilename);
                // The original item.BasePath should have been set in MainViewModel.
                File.Copy(System.IO.Path.Combine(item.BasePath, item.ImageFilename), newFilePath, overwrite: true);
                // Keep track of unsaved files to delete them when discarding.
                _unsavedRoomPictureFileList.Add(newFilePath);
                internalItem.SetIsModified(true);
            }
            else
            {
                internalItem.SetIsModified(false);
            }
            internalItem.PropertyChanged += OnPicturePropertyChanged;
            Pictures.Add(internalItem);
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

        // PDFs (don't use directly)
        /*
        Pdfs = _listing.Pdfs;
        foreach (var item in Pdfs)
        {
            item.BasePath = _listingDataDirectoryPath;
            item.ParentViewModel = this;
            item.SetIsModified(false);
            item.PropertyChanged += OnPdfPropertyChanged;
        }
        */
        Pdfs.Clear();
        foreach (var item in _listing.Pdfs)
        {
            var internalItem = item.Clone();
            internalItem.BasePath = _listingDataDirectoryPath;
            internalItem.ParentViewModel = this;
            if (isDuplicate)
            {
                var newFilePath = System.IO.Path.Combine(internalItem.BasePath, internalItem.PdfFilename);
                // The original item.BasePath should have been set in MainViewModel.
                File.Copy(System.IO.Path.Combine(item.BasePath, item.PdfFilename), newFilePath, overwrite: true);
                // Keep track of unsaved files to delete them when discarding.
                _unsavedRoomPdfFileList.Add(newFilePath);
                internalItem.SetIsModified(true);

                // Copy the thumbnail file as well
                if (!string.IsNullOrEmpty(item.ThumbnailFilename))
                {
                    var thumbnailPath = Path.Combine(internalItem.BasePath, internalItem.ThumbnailFilename);
                    File.Copy(Path.Combine(item.BasePath, item.ThumbnailFilename), thumbnailPath, overwrite: true);
                    _unsavedRoomPdfThumbnailFileList.Add(thumbnailPath);
                }
            }
            else
            {
                internalItem.SetIsModified(false);
            }
            internalItem.PropertyChanged += OnPdfPropertyChanged;
            Pdfs.Add(internalItem);
        }


        Pdfs.CollectionChanged += (sender, args) =>
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

        // (It is OK to use directly)
        //LessorsWrapper = new ObservableCollection<Models.Rent.Residentials.PersonWrapperForListingViewModel>(_listing.Lessors.Select(person => new Models.Rent.Residentials.PersonWrapperForListingViewModel(person, this)));
        LessorsWrapper.Clear();
        foreach (var person in _listing.Lessors)
        {
            var wrapper = new Models.Rent.Residentials.PersonWrapperForListingViewModel(person, this);
            LessorsWrapper.Add(wrapper);
        }

        // (It is OK to use directly)
        //BrokersWrapper = new ObservableCollection<Models.Rent.Residentials.PersonWrapperForListingViewModel>(_listing.Brokers.Select(person => new Models.Rent.Residentials.PersonWrapperForListingViewModel(person, this)));
        BrokersWrapper.Clear();
        foreach (var person in _listing.Brokers)
        {
            var wrapper = new Models.Rent.Residentials.PersonWrapperForListingViewModel(person, this);
            BrokersWrapper.Add(wrapper);
        }

        _listing.SetIsModified(isDuplicate);
        IsDirty = isDuplicate;
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

    /*
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
    */

    private void DiscardUnsavedFiles()
    {
        var failures = new List<string>();

        DeleteTrackedFiles(_unsavedRoomPictureFileList, failures);
        DeleteTrackedFiles(_unsavedRoomPdfFileList, failures);
        DeleteTrackedFiles(_unsavedRoomPdfThumbnailFileList, failures);

        if (_listing.Status == EntityStatus.New && Directory.Exists(_listingDataDirectoryPath))
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
            return;
        }

        // We are closing the window. No use showing an error message here,
        // because the user is discarding changes and we don't want to interrupt that process with an error message.
        // Instead, we show a message in the main window's info bar.
        WeakReferenceMessenger.Default.Send(new ShowErrorInfoBarInMainWindowMessage("変更の破棄中に一時ファイルを削除できませんでした", $"{string.Join(", ", failures)}"));

        return;
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
        /*
        if (_discardPending)
        {
            ShowSimpleErrorInfoBar("変更の破棄が完了していないため保存できません。もう一度画面を閉じて再試行してください。");
            return;
        }
        */
        if (IsSaving || !IsDirty) return;
        /*
        if (HasPendingEditorOperations)
        {
            ShowSimpleErrorInfoBar("実行中の選択またはファイル処理が完了してから、もう一度保存してください。");
            return;
        }
        
        var main = App.GetService<ViewModels.MainViewModel>();
        if (main.RentResidentialEditorList.Any(editor => editor.Id == _listing.PropertyId && editor.ViewModel is { IsDiscardPending: true }))
        {
            ShowSimpleErrorInfoBar("建物の変更の破棄を完了してから、もう一度保存してください。");
            return;
        }

        if (main.RentResidentialEditorList.Any(editor => editor.Id == _listing.PropertyId && editor.ViewModel?.IsListingPendingDeletion(_id) == true))
        {
            ShowSimpleErrorInfoBar("建物の編集画面で削除予定の募集物件は保存できません。");
            return;
        }
        if (main.RentResidentialEditorList.Any(editor => editor.Id == _listing.PropertyId && editor.ViewModel is { } vm && vm.HasPendingEditorOperations) || main.RentResidentialListingEditorList.Any(editor => editor.PropertyId == _listing.PropertyId && editor.ViewModel is { } vm && vm != this && vm.HasPendingEditorOperations))
        {
            ShowSimpleErrorInfoBar("関連する選択またはファイル処理が完了してから、もう一度保存してください。");
            return;
        }
        if (!CleanIncompleteAttachments()) return;
        if (!main.TryBeginResidentialOperation(_listing.PropertyId))
        {
            ShowSimpleErrorInfoBar("関連する保存、削除または読み込みが完了してから、もう一度保存してください。");
            return;
        }
        */

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
                //main.EndResidentialOperation(_listing.PropertyId);
                //if (_pendingHandoffReady) PublishPendingHandoff(main);
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
        if (_listing is null)
        {
            Debug.WriteLine("_listing is null. Can't save room.");
            return;
        }

        // Validate input.
        if (!ValidateName() || !TryParseBasicNumericValues(out _, out _))
        {
            ShowSimpleErrorInfoBar("入力項目に誤りがあります","入力項目に誤りがあります。保存出来ませんでした。");

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
            ShowSimpleErrorInfoBar("入力項目に誤りがあります", "メインの図面は1枚だけ選択してください。");
            const string pageName = "ZumenSearch.Views.Rent.Residentials.Listing.ZumenListPage";
            if (IsPropertyUnitLevelOwnership && !_navigationService.IsCurrentPageSameAs(pageName)) _navigationService.NavigateTo(pageName, this);
            return;
        }

        if (_listing.PropertyStatus == EntityStatus.New && !App.GetService<ViewModels.MainViewModel>().RentResidentialEditorList.Any(editor => editor.Id == _listing.PropertyId && editor.ViewModel is { IsActive: true }))
        {
            ShowSimpleErrorInfoBar("入力項目に誤りがあります", "建物の編集画面が開いていないため、募集物件を保存できません。");
            return;
        }


        SetValues();

        bool saveResult;
        if (_listing.PropertyStatus == EntityStatus.New)
        {
            // The parent defers messages while the shared property operation is active.
            //_pendingHandoffReady = true;
            
            Debug.WriteLine("(_listing.PropertyStatus == EnumPropertyStatus.New) @ListingViewModel on Save. Sending it to Property editor window");

            // The parent property editor owns persistence until the property is saved. Let the parent property editor handle the save operation.
            // This should also update the selected search result's values such as name if it exists. Also, it should update building window's rooms list.
            WeakReferenceMessenger.Default.Send(new Models.Messenger.ListingUpdatedMessage(_listing));

            saveResult = true;
        }
        else
        {
            //var roomToSave = _listing;
            //roomToSave.SetIsPropertyUnitLevelOwnership(IsPropertyUnitLevelOwnership);

            Models.SearchResult.ResultWrapper resInsert;
            var committed = false;
            resInsert = await Task.Run(() => _dataAccessService.UpsertRentResidentialListing(_listing.PropertyId, _listing), _cts.Token);
            committed = !resInsert.IsError;

            if (resInsert.IsError)
            {
                Debug.WriteLine(
                    resInsert.Error.Title + Environment.NewLine +
                    resInsert.Error.Message + Environment.NewLine +
                    resInsert.Error.Description + Environment.NewLine +
                    resInsert.Error.Operation + Environment.NewLine +
                    resInsert.Error.MethodName + Environment.NewLine +
                    resInsert.Error.FullDump);

                ShowErrorInfoBar(resInsert.Error);

                saveResult = false;
            }
            else
            {
                _listing.SetStatus(EntityStatus.Saved);
                _listing.SetIsModified(false);
                foreach (var picture in Pictures.Where(picture => _listing.Pictures.Any(saved => saved.Id == picture.Id)))
                {
                    picture.SetStatus(EntityStatus.Saved);
                    picture.SetIsModified(false);
                }
                foreach (var pdf in Pdfs.Where(pdf => _listing.Pdfs.Any(saved => saved.Id == pdf.Id)))
                {
                    pdf.SetStatus(EntityStatus.Saved);
                    pdf.SetIsModified(false);
                }
                //CaptureCommittedAttachments(ResidentialListingDraft.FromListing(roomToSave));
                _unsavedRoomPictureFileList.Clear();
                _unsavedRoomPdfFileList.Clear();
                _unsavedRoomPdfThumbnailFileList.Clear();

                _listing.SetIsModified(false);
                WeakReferenceMessenger.Default.Send(new Models.Messenger.ListingUpdatedMessage(_listing));

                saveResult = true;
            }

        }

        if (saveResult)
        {
            //_draftLessorDeletions.Clear();
            //_draftBrokerDeletions.Clear();

            // Clear error infobar.
            IsInfoBarErrorOpen = false;
            
            // Update title with dummy value.
            WindowTitle = string.Empty;

            var cleanupFailures = new List<string>();
            foreach (var file in _listing.PicturesToBeDeleted.ToList())
            {
                var picturePath = Path.Combine(App.PropertyBlobDataFolder, _listing.PropertyId, _listing.Id, file.ImageFilename);
                if (TryDeleteFiles(new[] { picturePath }, cleanupFailures))
                {
                    _listing.PicturesToBeDeleted.Remove(file);
                }
            }

            if (_listing.PropertyStatus != EntityStatus.New)
            {
                foreach (var file in _listing.PdfsToBeDeleted.ToList())
                {
                    var pdfPath = Path.Combine(App.PropertyBlobDataFolder, _listing.PropertyId, _listing.Id, file.PdfFilename);
                    var thumbnailPath = Path.Combine(App.PropertyBlobDataFolder, _listing.PropertyId, _listing.Id, file.ThumbnailFilename);
                    if (TryDeleteFiles(new[] { pdfPath, thumbnailPath }, cleanupFailures))
                    {
                        _listing.PdfsToBeDeleted.Remove(file);
                    }
                }
            }

            _unsavedRoomPictureFileList.Clear();
            _unsavedRoomPdfFileList.Clear();
            _unsavedRoomPdfThumbnailFileList.Clear();
            /*
            _hasPendingFileCleanupAfterSave = cleanupFailures.Count > 0;
            if (_hasPendingFileCleanupAfterSave)
            {
                IsDirty = true;
                ShowAttachmentError($"保存しましたが、一部の削除済みファイルを削除できませんでした。次回の保存時に再試行します: {string.Join(", ", cleanupFailures)}");
            }
            */
            if (cleanupFailures.Count>0)
            {
                ShowSimpleErrorInfoBar("保存しましたが、一部の削除済みファイルをクリーンアップできませんでした", $"{string.Join(", ", cleanupFailures)}");
            }

            IsDirty = false;
        }
    }   

    /*
    private void PublishPendingHandoff(ViewModels.MainViewModel main)
    {
        _pendingHandoffReady = false;
        var parent = main.RentResidentialEditorList.FirstOrDefault(editor => editor.Id == _listing.PropertyId)?.ViewModel;
        var previous = parent?.GetPendingListingDraft(_id);
        WeakReferenceMessenger.Default.Send(new Models.Messenger.ListingUpdatedMessage(_listing));
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
    */

    private bool CanSave()
    {
        return IsDirty && !IsSaving && !_propertyBusy && !_isClosing;// && !_discardPending;
    }

    #endregion

    #region == Pics and Pdfs ==

    [RelayCommand(CanExecute = nameof(CanAddNewRoomPictures))]
    public async Task AddNewRoomPictures(List<string> filePathList)
    {
        if (filePathList is null) return;
        if (filePathList.Count == 0) return;

        Debug.WriteLine($"destDirectory={_listingDataDirectoryPath}  @AddNewRoomPictures()");

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
                ShowSimpleErrorInfoBar("エラー", $"画像形式に対応していません: {Path.GetFileName(filePath)}");
                continue;
            }

            using var sourceStream = File.Open(filePath, FileMode.Open);

            string newId = Guid.CreateVersion7().ToString("N");
            string newFilename = newId + extension;
            var destFilePath = Path.Combine(_listingDataDirectoryPath, newFilename);
            //Debug.WriteLine($"{file} to {destFilePath}  @SetNewBuildingPicturesAsync()");

            using var destinationStream = new FileStream(destFilePath, FileMode.CreateNew, FileAccess.Write);
            //_incompleteAttachmentFiles.Add(destFilePath);
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
            //_incompleteAttachmentFiles.Remove(destFilePath);
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

    [RelayCommand(CanExecute = nameof(CanDeleteRoomPicture))]
    private async Task DeleteRoomPicture(Models.Rent.Residentials.ListingPicture picUnit)
    {
        if (_listing is null)
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
            if (_listing.Pictures.Remove(picUnit))
            {
                
            }
            */
            _listing.PicturesToBeDeleted.Add(picUnit);
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
        if (filePathList.Count == 0 || !IsPropertyUnitLevelOwnership) return;
        //var ownershipVersion = _ownershipVersion;

        Debug.WriteLine($"destDirectory={_listingDataDirectoryPath}  @AddNewRoomPdfs()");

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
                ShowSimpleErrorInfoBar("エラー", $"PDFファイルを選択してください: {Path.GetFileName(filePath)}");
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
                        //_incompleteAttachmentFiles.Add(thumbnailDestFilePath);
                        importedPaths.Add(thumbnailDestFilePath);
                        using var managedSourceStream = stream.AsStreamForRead();
                        await managedSourceStream.CopyToAsync(destinationStream);
                    }

                    string newFilename = newId + extension;
                    var pdfDestFilePath = Path.Combine(_listingDataDirectoryPath, newFilename);
                    using (var pdfSource = File.OpenRead(filePath))
                    using (var pdfDestination = new FileStream(pdfDestFilePath, FileMode.CreateNew, FileAccess.Write))
                    {
                        //_incompleteAttachmentFiles.Add(pdfDestFilePath);
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

                    OpenBlobDirectoryCommand.NotifyCanExecuteChanged();
                    DeleteRoomPdfCommand.NotifyCanExecuteChanged();
                    IsDirty = true;
                }
                else
                {
                    ShowSimpleErrorInfoBar("エラー", $"PDFにページがありません: {Path.GetFileName(filePath)}");
                }
            }
            catch (Exception ex) when (ex is IOException or UnauthorizedAccessException or System.Runtime.InteropServices.COMException or InvalidOperationException or ArgumentException)
            {
                /*
                if (ownershipVersion != _ownershipVersion || !IsPropertyUnitLevelOwnership)
                {
                    foreach (var path in importedPaths)
                    {
                        AddPendingPdfCleanupPath(path);
                        _incompleteAttachmentFiles.Remove(path);
                    }
                }
                */
                Debug.WriteLine($"Failed to import PDF '{filePath}': {ex}");
                ShowSimpleErrorInfoBar("エラー", $"PDFを読み込めませんでした: {Path.GetFileName(filePath)}。{ex.Message}");
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
                ShowSimpleErrorInfoBar("エラー", "PDFを開けませんでした。PDFを開くアプリが設定されているか確認してください。");
            }
        }
        catch (Exception ex) when (ex is IOException or UnauthorizedAccessException or System.Runtime.InteropServices.COMException or InvalidOperationException or ArgumentException)
        {
            Debug.WriteLine($"Failed to open residential listing PDF: {ex}");
            ShowSimpleErrorInfoBar("エラー", $"PDFを開けませんでした: {ex.Message}");
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
            _listing.PdfsToBeDeleted.Add(pdf);
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
                ShowSimpleErrorInfoBar("フォルダーを開けませんでした",$"{ex.Message}");
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
                ShowSimpleErrorInfoBar("選択した貸主を読み込めませんでした",$"ID {lessorId}　がありません。");
                return;
            }


            var asdf = new Models.Rent.Residentials.PersonWrapperForListingViewModel(res.Person, this);

            //_draftLessorDeletions.RemoveAll(person => person.Id == res.Person.Id);
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
            //if (!_draftLessorDeletions.Any(person => person.Id == lessor.Person.Id)) _draftLessorDeletions.Add(lessor.Person);
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
            ShowSimpleErrorInfoBar("選択した宅建業者を読み込めませんでした",$"ID {selectedBroker.Id}　がありません。");
            return;
        }

        //_draftBrokerDeletions.RemoveAll(person => person.Id == result.Person.Id);
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
            _listing.Brokers.Remove(broker.Person);
            //if (!_draftBrokerDeletions.Any(person => person.Id == broker.Person.Id)) _draftBrokerDeletions.Add(broker.Person);
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

        if (vm.EditRentResidentialFromIdCommand.CanExecute(_listing.PropertyId))
        {
            //await vm.EditRentResidentialFromIdCommand(_listing.PropertyId);
            vm.EditRentResidentialFromIdCommand.Execute(_listing.PropertyId);
        }
    }

    #endregion


}
