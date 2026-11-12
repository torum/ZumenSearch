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
    private bool _hasPendingFileCleanupAfterSave;
    private bool _disposed;

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

        _navigationService = navigationService; // _navigationService.NavigateTo("ZumenSearch.Views.Rent.Residentials.RoomListPage", this, new DrillInNavigationTransitionInfo());
        _dialogService = dialogService;

        _dispatcherService = dispatcherService;
        _dataAccessService = dataAccessService;

        _listingDataDirectoryPath = System.IO.Path.Combine(App.PropertyBlobDataFolder, _room.PropertyId, _room.Id);

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
        var building = property.Value;
        if (building is not null) 
        {
            _room.SetPropertyStatus(building.Status);
            _room.SetPropertyName(building.Name);
            /*
             * Use 
                WeakReferenceMessenger.Default.Send(new Models.Messenger.PropertyIsUnitOwnershipChangedMessage(value));
            if (building is Models.Rent.Residentials.Property residentialProperty)
            {
                _room.SetIsPropertyUnitOwnership(residentialProperty.IsUnitOwnership);
            }
            */
            // Update window title with dummy string.
            WindowTitle = string.Empty;
        }
    }

    public void Receive(LessorUpdatedMessage person)
    {
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
        IsPropertyUnitOwnership = isUnitOwnership.Value;

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

    public bool DiscardChanges()
    {
        if (!DiscardUnsavedFiles())
        {
            return false;
        }

        if (_hasPendingFileCleanupAfterSave)
        {
            ShowAttachmentError("保存済みデータの削除済みファイルが残っています。ファイルの削除を完了するため、もう一度保存してください。");
            return false;
        }

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
        foreach (var picture in Pictures)
        {
            picture.PropertyChanged -= OnPicturePropertyChanged;
        }

        foreach (var pdf in Pdfs)
        {
            pdf.PropertyChanged -= OnPdfPropertyChanged;
        }

        Pictures.Clear();
        Pdfs.Clear();

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

        var areaText = Helpers.Common.ReplaceZenkakuNumbers(FloorExclusiveAreaSqm.Trim());
        if (!string.IsNullOrEmpty(areaText) &&
            (!decimal.TryParse(areaText, NumberStyles.Number, CultureInfo.CurrentCulture, out floorArea) || floorArea < 0))
        {
            InfoBarErrorMessage = "専有面積は0以上の数値で入力してください。";
            return false;
        }

        var floorText = Helpers.Common.ReplaceZenkakuNumbers(FloorNumber.Trim());
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

        // 写真
        //_room.Pictures = Pictures;
        foreach (var pic in Pictures)
        {
            var existingPic = _room.Pictures.FirstOrDefault(r => r.Id == pic.Id);
            if (existingPic is not null)
            {
                // Update existing pic
                var index = _room.Pictures.IndexOf(existingPic);
                _room.Pictures[index] = pic;

            }
            else
            {
                // Add new pic
                _room.Pictures.Add(pic);
            }
        }

        // 図面
        foreach (var pdf in Pdfs)
        {
            var existingPdf = _room.Pdfs.FirstOrDefault(r => r.Id == pdf.Id);
            if (existingPdf is not null)
            {
                // Update existing pdf
                var index = _room.Pdfs.IndexOf(existingPdf);
                _room.Pdfs[index] = pdf;

            }
            else
            {
                // Add new pic
                _room.Pdfs.Add(pdf);
            }
        }

        // 貸主
        // 一旦クリアして、Wraperから「建物」の貸主を取り出して追加
        _room.Lessors.Clear();
        foreach (var item in this.LessorsWrapper)
        {
            _room.Lessors.Add(item.Person);
        }

        // 宅建業者
        _room.Brokers.Clear();
        foreach (var item in BrokersWrapper)
        {
            _room.Brokers.Add(item.Person);
        }

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
        Pictures = new ObservableCollection<Models.Rent.Residentials.ListingPicture>(_room.Pictures); // create a copy.
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
        Pdfs = new ObservableCollection<Models.Rent.Residentials.ListingPdf>(_room.Pdfs); // create a copy.
        foreach (var item in Pdfs)
        {
            item.BasePath = System.IO.Path.Combine(System.IO.Path.Combine(System.IO.Path.Combine(App.PropertyBlobDataFolder, _room.PropertyId), _room.Id));
            item.ParentViewModel = this;
            item.SetIsModified(false); // Needed this.
            item.PropertyChanged += OnPdfPropertyChanged;
        }

        Pdfs.CollectionChanged += (s, e) =>
        {
            // Unsubscribe from removed items
            if (e.OldItems != null)
            {
                foreach (Models.Rent.Residentials.ListingPdf item in e.OldItems)
                {
                    Debug.WriteLine($"Item {item.Id} Removed from Pdfs");
                    IsDirty = true;

                    item.PropertyChanged -= OnPdfPropertyChanged;
                }
            }

            // Subscribe to PropertyChanged.
            if (e.NewItems != null)
            {
                foreach (Models.Rent.Residentials.ListingPdf item in e.NewItems)
                {
                    Debug.WriteLine($"Item {item.Id} Added to Pdfs");
                    IsDirty = true;

                    item.PropertyChanged += OnPdfPropertyChanged;
                }
            }
        };

        // Lessors
        LessorsWrapper = new ObservableCollection<Models.Rent.Residentials.PersonWrapperForListingViewModel>();
        foreach (var item in _room.Lessors)
        {
            LessorsWrapper.Add(new Models.Rent.Residentials.PersonWrapperForListingViewModel(item, this));
        }

        // Brokers
        BrokersWrapper = new ObservableCollection<Models.Rent.Residentials.PersonWrapperForListingViewModel>();
        foreach (var item in _room.Brokers)
        {
            BrokersWrapper.Add(new Models.Rent.Residentials.PersonWrapperForListingViewModel(item, this));
        }


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

    private bool DiscardUnsavedFiles()
    {
        var failures = new List<string>();
        DeleteTrackedFiles(_unsavedRoomPictureFileList, failures);
        DeleteTrackedFiles(_unsavedRoomPdfFileList, failures);
        DeleteTrackedFiles(_unsavedRoomPdfThumbnailFileList, failures);

        if (_room.Status == EntityStatus.New && Directory.Exists(_listingDataDirectoryPath))
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
    private async Task Save()
    {
        if (!IsDirty)
        {
            return;
        }

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

        SetValues();

        bool saveResult;
        if (_room.PropertyStatus == EntityStatus.New)
        {
            Debug.WriteLine("(_room.PropertyStatus == EnumPropertyStatus.New) @ListingViewModel on Save. Sending it to Property editor window");
            // The parent property editor owns persistence until the property is saved.
            // Update the selected search result's values such as name if it exists. Also, update building window's rooms list.
            WeakReferenceMessenger.Default.Send(new Models.Messenger.ListingUpdatedMessage(_room));

            saveResult = true;
        }
        else
        {
            var resInsert = await Task.Run(() => _dataAccessService.UpsertRentResidentialListing(_room.PropertyId, _room), _cts.Token);
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

                // Update the selected search result's values such as name if it exists. Also, update building window's rooms list.
                WeakReferenceMessenger.Default.Send(new Models.Messenger.ListingUpdatedMessage(_room));

                saveResult = true;
            }

        }

        if (saveResult)
        {
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

            foreach (var file in _room.PdfsToBeDeleted.ToList())
            {
                var pdfPath = Path.Combine(App.PropertyBlobDataFolder, _room.PropertyId, _room.Id, file.PdfFilename);
                var thumbnailPath = Path.Combine(App.PropertyBlobDataFolder, _room.PropertyId, _room.Id, file.ThumbnailFilename);
                if (TryDeleteFiles(new[] { pdfPath, thumbnailPath }, cleanupFailures))
                {
                    _room.PdfsToBeDeleted.Remove(file);
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
    private bool CanSave()
    {
        if (IsDirty)
        {
            return true;
        }
        return false;
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

            using var destinationStream = File.Create(destFilePath);
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
        if (filePathList.Count == 0) return;

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

                    using var destinationStream = File.Create(thumbnailDestFilePath);
                    using var managedSourceStream = stream.AsStreamForRead();
                    await managedSourceStream.CopyToAsync(destinationStream);
                    _unsavedRoomPdfThumbnailFileList.Add(thumbnailDestFilePath);

                    string newFilename = newId + extension;
                    var pdfDestFilePath = Path.Combine(_listingDataDirectoryPath, newFilename);
                    File.Copy(filePath, pdfDestFilePath);
                    _unsavedRoomPdfFileList.Add(pdfDestFilePath);

                    var pdf = new Models.Rent.Residentials.ListingPdf(newId, newFilename, newThumbnailFilename, EntityStatus.New)
                    {
                        BasePath = _listingDataDirectoryPath,
                        ParentViewModel = this
                    };

                    Pdfs.Add(pdf);
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
                Debug.WriteLine($"Failed to import PDF '{filePath}': {ex}");
                ShowAttachmentError($"PDFを読み込めませんでした: {Path.GetFileName(filePath)}。{ex.Message}");
            }
        }
    }

    private static bool CanAddNewRoomPdfs()
    {
        return true;
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

            _room.LessorsToBeDeleted.Add(lessor.Person);
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

        // TODO: This needs to be copied to the property editor window's brokers list and some more too, so that when saving the property, it will save the broker association too. (or maybe not needed because we are saving the room and not the property here.)
        // * Check if the broker is already in the list of brokers to be deleted
        // * Remove a matching pending deletion before adding the selected broker, so deleting and then re-adding it before saving won’t delete its association
        var pendingDeletion = _room.BrokersToBeDeleted.FirstOrDefault(person => person.Id == result.Person.Id);
        if (pendingDeletion is not null)
        {
            _room.BrokersToBeDeleted.Remove(pendingDeletion);
        }


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
            _room.BrokersToBeDeleted.Add(broker.Person);
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
