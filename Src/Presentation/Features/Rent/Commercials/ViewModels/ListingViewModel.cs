using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Globalization;
using System.Runtime.InteropServices;
using System.Text;
using System.Text.RegularExpressions;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.Messaging;
using Microsoft.UI.Xaml.Controls;
using Windows.Data.Pdf;
using Windows.Storage;
using Windows.Storage.Streams;
using ZumenSearch.Application.Services.Persons;
using ZumenSearch.Application.Services.Rent.Commercials;
using ZumenSearch.Domain.Enums;
using ZumenSearch.Domain.Entities.Rent.Commercials.Listing;
using ZumenSearch.Presentation.Features.Rent.Commercials.Models;
using ZumenSearch.Presentation.Helpers;
using ZumenSearch.Presentation.Messaging;
using ZumenSearch.Presentation.Navigation;
using ZumenSearch.Presentation.Services.Contracts;
using ZumenSearch.Presentation.Shell.Models;
using ZumenSearch.Presentation.Shell.ViewModels;

namespace ZumenSearch.Presentation.Features.Rent.Commercials.ViewModels;

public sealed partial class ListingViewModel : ObservableRecipient, IRecipient<PropertyUpdatedMessage>, IRecipient<PropertyIsUnitOwnershipChangedMessage>, IRecipient<LessorUpdatedMessage>, IRecipient<LessorDeletedMessage>, IRecipient<BrokerUpdatedMessage>, IRecipient<BrokerDeletedMessage>, IDisposable
{
    #region == Private variables ==

    private const string BasicPageName = NavigationKeys.RentCommercialListingBasic;
    private readonly string _id = string.Empty;

    // This object is the actual entity being edited and sent to DataAccessService and other window.
    // Populate values on Loading and apply changes to this object in Save().
    private readonly ZumenSearch.Domain.Entities.Rent.Commercials.Listing.Listing _listing;

    private readonly string _listingDataDirectoryPath = string.Empty;

    // Tmp file list to hold unsaved picture files. (if entry is not saved, delete on close)
    private readonly List<string> _unsavedListingPictureFileList = [];
    private readonly List<string> _unsavedListingPdfFileList = [];
    private readonly List<string> _unsavedListingPdfThumbnailFileList = [];
    private readonly List<string> _cleanupFailures = [];

    // Keep unsaved removals separate from the listing owned by the parent editor.
    private readonly List<ListingPicture> _picturesToBeDeleted = [];
    private readonly List<ListingPdf> _pdfsToBeDeleted = [];
    private readonly List<ZumenSearch.Domain.Entities.Base.PersonBase> _lessorsToBeDeleted = [];
    private readonly List<ZumenSearch.Domain.Entities.Base.PersonBase> _brokersToBeDeleted = [];

    private bool _isHandedOffToParentEditor;
    private bool _disposed;

    private readonly CancellationTokenSource _cts = new();

    #endregion

    #region == Services ==

    private readonly RentCommercialListingService _rentCommercialListingService;
    private readonly PersonService _personService;
    private readonly IDispatcherService _dispatcherService;
    private readonly IDialogGenericService _dialogService;
    private readonly INavigationGenericService _navigationService;

    #endregion

    public ListingViewModel(ZumenSearch.Domain.Entities.Rent.Commercials.Listing.Listing listing, bool isDuplicate, INavigationGenericService navigationService, IDialogGenericService dialogService, IDispatcherService dispatcherService, RentCommercialListingService rentCommercialListingService, PersonService personService, bool isHandedOffToParentEditor = false)
    {
        _listing = listing;
        _id = listing.Id;
        _isHandedOffToParentEditor = isHandedOffToParentEditor;

        _navigationService = navigationService;
        _dialogService = dialogService;
        _dispatcherService = dispatcherService;
        _rentCommercialListingService = rentCommercialListingService;
        _personService = personService;

        // Create the data folder for this listing. This is where the pictures and pdfs will be stored.
        _listingDataDirectoryPath = System.IO.Path.Combine(App.PropertyBlobDataFolder, _listing.PropertyId, _listing.Id);

        try
        {
            if (!Directory.Exists(_listingDataDirectoryPath))
            {
                Directory.CreateDirectory(_listingDataDirectoryPath);
            }
        }
        catch (Exception ex)
        {
            var err = new ZumenSearch.Application.Results.ErrorInfo
            {
                Title = "予期しないエラーが発生しました。",
                Type = ZumenSearch.Application.Results.ErrorInfo.ErrType.Other,
                Code = "",
                Message = $"募集物件のデータフォルダを作成できませんでした。写真や図面を保存出来ません。",
                Description = $"{ex}",
                Operation = "Directory.CreateDirectory",
                MethodName = nameof(ListingViewModel),
                OccuredAt = DateTime.Now
            };

            ShowErrorInfoBar(err);
        }

        // Populate the view model with the listing values.
        PopulateValues(isDuplicate);

        // Reset errors
        IsNameHasError = false;

        // Reset IsDirty to false after PopulateValues()
        _listing.SetIsModified(isDuplicate);
        IsDirty = isDuplicate;

        // Ready to receive messages.
        // Make sure to add matching unregistrations in Dispose().
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

    public bool IsBusy
    {
        get;
        private set
        {
            if (SetProperty(ref field, value))
            {
                OnPropertyChanged(nameof(IsEditorEnabled));
                SaveCommand.NotifyCanExecuteChanged();
            }
        }
    }

    public bool IsEditorEnabled => !IsBusy;

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
    } = "賃貸事業用";

    public ObservableCollection<Breadcrumb> BreadcrumbItems { get; set; } =
    [
        new() { Name = "募集物件", Page = NavigationKeys.RentCommercialListingBasic },
        new() { Name = "基本", Page = NavigationKeys.RentCommercialListingBasic }
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

                // Update title with dummy value.
                WindowTitle = string.Empty;
            }
        }
    }

    public IReadOnlyList<ListingFloorGroundTypeLabel> FloorGroundTypes { get; } = ListingFloorGroundTypeLabel.GetAll();

    public ListingFloorGroundTypeLabel SelectedFloorGroundType
    {
        get => field ?? new(ListingFloorGroundType.AboveGround);
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

    public string FloorArea
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

    public IReadOnlyList<ListingUseTypeLabel> UseTypes { get; } = ListingUseTypeLabel.GetAll();

    public ListingUseTypeLabel SelectedUseType
    {
        get => field ?? new(ListingUseType.Unspecified);
        set
        {
            if (value is null) return;
            if (SelectedUseType.Key == value.Key)
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

    public string UseDetails
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

    public IReadOnlyList<ListingAvailabilityStatusTypeLabel> CurrentStatusTypes { get; } = ListingAvailabilityStatusTypeLabel.GetAll();

    public ListingAvailabilityStatusTypeLabel SelectedCurrentStatusType
    {
        get => field ?? new(ListingAvailabilityStatusType.Unspecified);
        set
        {
            if (value is null) return;
            if (SelectedCurrentStatusType.Key == value.Key)
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
        get => field ?? new(ListingAvailabilityMonthType.Unspecified);
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
        get => field ?? new(ListingAvailabilityPeriodType.Unspecified);
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

    [RelayCommand]
    public void SetOccupancyStatusCheckedAtToToday()
    {
        OccupancyStatusCheckedAt = DateTimeOffset.Now;
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

            var text = ZumenSearch.Presentation.Utilities.Common.NormalizeFullWidthCharacters(value).Trim().ToUpperInvariant();
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

    public IReadOnlyList<ListingUnitConditionTypeLabel> UnitConditionOptions { get; } = ListingUnitConditionTypeLabel.GetAll();
    public IReadOnlyList<ListingFixtureTransferTypeLabel> FixtureTransferOptions { get; } = ListingFixtureTransferTypeLabel.GetAll();

    public bool HasAirConditioning { get; set => SetContractValue(ref field, value); }
    public bool HasPrivateToilet { get; set => SetContractValue(ref field, value); }
    public bool HasHotWater { get; set => SetContractValue(ref field, value); }
    public bool HasKitchen { get; set => SetContractValue(ref field, value); }
    public bool HasVentilation { get; set => SetContractValue(ref field, value); }
    public bool HasInternet { get; set => SetContractValue(ref field, value); }
    public string EquipmentDetails { get => field ?? string.Empty; set => SetContractValue(ref field, value); }
    public ListingUnitConditionTypeLabel SelectedUnitCondition { get => field ?? UnitConditionOptions.First(x => x.Key == ListingUnitConditionType.Unspecified); set { if (value is not null) SetContractValue(ref field, value); } }
    public ListingFixtureTransferTypeLabel SelectedFixtureTransfer { get => field ?? FixtureTransferOptions.First(x => x.Key == ListingFixtureTransferType.Unconfirmed); set { if (value is not null) SetContractValue(ref field, value); } }
    public string BusinessHoursRestrictions { get => field ?? string.Empty; set => SetContractValue(ref field, value); }
    public string UseConditions { get => field ?? string.Empty; set => SetContractValue(ref field, value); }
    public bool PermitsHeavyFoodService { get; set => SetContractValue(ref field, value); }
    public bool PermitsLightFoodService { get; set => SetContractValue(ref field, value); }
    public bool PermitsRetail { get; set => SetContractValue(ref field, value); }
    public bool PermitsServices { get; set => SetContractValue(ref field, value); }
    public bool PermitsOffice { get; set => SetContractValue(ref field, value); }
    public bool PermitsOther { get; set => SetContractValue(ref field, value); }
    public string PermittedBusinessDetails { get => field ?? string.Empty; set => SetContractValue(ref field, value); }
    public string FacilitiesRemarks { get => field ?? string.Empty; set => SetContractValue(ref field, value); }

    #endregion

    #region == 契約条件 ==

    public IReadOnlyList<ListingFeeUnitTypeLabel> FeeUnitTypes { get; } = ListingFeeUnitTypeLabel.GetAll();
    public IReadOnlyList<ListingContractTaxTypeLabel> ContractTaxTypes { get; } = ListingContractTaxTypeLabel.GetAll();
    public IReadOnlyList<ListingContractTypeLabel> ContractTypes { get; } = ListingContractTypeLabel.GetAll();

    public string ManagementFee { get => field ?? string.Empty; set => SetContractValue(ref field, value); }
    public string SecurityDeposit { get => field ?? string.Empty; set => SetContractValue(ref field, value); }
    public string GuaranteeDeposit { get => field ?? string.Empty; set => SetContractValue(ref field, value); }
    public string KeyMoney { get => field ?? string.Empty; set => SetContractValue(ref field, value); }
    public string RightsMoney { get => field ?? string.Empty; set => SetContractValue(ref field, value); }
    public string DepositDeduction { get => field ?? string.Empty; set => SetContractValue(ref field, value); }
    public string RenewalFee { get => field ?? string.Empty; set => SetContractValue(ref field, value); }
    public string RecontractFee { get => field ?? string.Empty; set => SetContractValue(ref field, value); }
    public string ContractPeriodYears { get => field ?? string.Empty; set => SetContractValue(ref field, value); }
    public string GuarantorCompany { get => field ?? string.Empty; set => SetContractValue(ref field, value); }
    public string Insurance { get => field ?? string.Empty; set => SetContractValue(ref field, value); }
    public string OtherFees { get => field ?? string.Empty; set => SetContractValue(ref field, value); }
    public string ContractConditions { get => field ?? string.Empty; set => SetContractValue(ref field, value); }
    public string ContractRemarks { get => field ?? string.Empty; set => SetContractValue(ref field, value); }
    public ListingFeeUnitTypeLabel SelectedSecurityDepositUnit { get => field ?? FeeUnitTypes.First(x => x.Key == ListingFeeUnitType.Months); set { if (value is not null) SetContractValue(ref field, value); } }
    public ListingFeeUnitTypeLabel SelectedGuaranteeDepositUnit { get => field ?? FeeUnitTypes.First(x => x.Key == ListingFeeUnitType.Months); set { if (value is not null) SetContractValue(ref field, value); } }
    public ListingFeeUnitTypeLabel SelectedKeyMoneyUnit { get => field ?? FeeUnitTypes.First(x => x.Key == ListingFeeUnitType.Months); set { if (value is not null) SetContractValue(ref field, value); } }
    public ListingFeeUnitTypeLabel SelectedRightsMoneyUnit { get => field ?? FeeUnitTypes.First(x => x.Key == ListingFeeUnitType.Months); set { if (value is not null) SetContractValue(ref field, value); } }
    public ListingFeeUnitTypeLabel SelectedDepositDeductionUnit { get => field ?? FeeUnitTypes.First(x => x.Key == ListingFeeUnitType.Months); set { if (value is not null) SetContractValue(ref field, value); } }
    public ListingFeeUnitTypeLabel SelectedRenewalFeeUnit { get => field ?? FeeUnitTypes.First(x => x.Key == ListingFeeUnitType.Months); set { if (value is not null) SetContractValue(ref field, value); } }
    public ListingFeeUnitTypeLabel SelectedRecontractFeeUnit { get => field ?? FeeUnitTypes.First(x => x.Key == ListingFeeUnitType.Yen); set { if (value is not null) SetContractValue(ref field, value); } }
    public ListingContractTaxTypeLabel SelectedRentTax { get => field ?? ContractTaxTypes.First(x => x.Key == ListingContractTaxType.Unconfirmed); set { if (value is not null) SetContractValue(ref field, value); } }
    public ListingContractTaxTypeLabel SelectedManagementFeeTax { get => field ?? ContractTaxTypes.First(x => x.Key == ListingContractTaxType.Unconfirmed); set { if (value is not null) SetContractValue(ref field, value); } }
    public ListingContractTypeLabel SelectedContractType { get => field ?? ContractTypes.First(x => x.Key == ListingContractType.Unspecified); set { if (value is not null) SetContractValue(ref field, value); } }

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

            var text = ZumenSearch.Presentation.Utilities.Common.NormalizeFullWidthCharacters(value.Trim());
            if (SetProperty(ref field, text))
            {
                IsDirty = true;
            }
        }
    }

    #endregion

    #region == 写真 & PDFプロパティ ==

    public ObservableCollection<ListingPictureViewModel> Pictures
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

    public ObservableCollection<ListingPdfViewModel> Pdfs
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

    public ObservableCollection<ZumenSearch.Presentation.Features.Rent.Commercials.ViewModels.ListingPersonViewModel> LessorsWrapper
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

    public ObservableCollection<ZumenSearch.Presentation.Features.Rent.Commercials.ViewModels.ListingPersonViewModel> BrokersWrapper
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
        if (property.Value is not ZumenSearch.Domain.Entities.Rent.Commercials.Property building || !building.Id.Equals(_listing.PropertyId, StringComparison.Ordinal)) return;

        _listing.SetPropertyStatus(building.Status);
        if (building.Status == EntityStatus.Saved) _isHandedOffToParentEditor = false;
        _listing.SetPropertyName(building.Name);
        SetOwnershipVisibility(building.IsUnitOwnership);

        FudousanId = building.FudousanId;
        _listing.SetFudousanId(building.FudousanId);

        // Update window title with dummy string.
        WindowTitle = string.Empty;
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

        if (psn.Person.GetType() != lessor.GetType())
        {
            LessorsWrapper[LessorsWrapper.IndexOf(psn)] = new ZumenSearch.Presentation.Features.Rent.Commercials.ViewModels.ListingPersonViewModel(lessor, this);
        }
        else
        {
            psn.Person = lessor;
        }
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

        if (psn.Person.GetType() != broker.GetType())
        {
            BrokersWrapper[BrokersWrapper.IndexOf(psn)] = new ZumenSearch.Presentation.Features.Rent.Commercials.ViewModels.ListingPersonViewModel(broker, this);
        }
        else
        {
            psn.Person = broker;
        }
    }

    public void Receive(PropertyIsUnitOwnershipChangedMessage isUnitOwnership)
    {
        if (!string.Equals(isUnitOwnership.PropertyId, _listing.PropertyId, StringComparison.Ordinal)) return;

        SetOwnershipVisibility(isUnitOwnership.Value);
    }

    public void SetOwnershipVisibility(bool isUnitOwnership)
    {
        IsPropertyUnitLevelOwnership = isUnitOwnership;
        _listing.SetIsPropertyUnitOwnership(isUnitOwnership);

        if (IsPropertyUnitLevelOwnership)
        {
            return;
        }

        if (_navigationService.IsCurrentPageSameAs(NavigationKeys.RentCommercialListingDocuments))
        {
            _navigationService.NavigateTo(NavigationKeys.RentCommercialListingBasic, this);
        }
        else if (_navigationService.IsCurrentPageSameAs(NavigationKeys.RentCommercialListingLessors))
        {
            _navigationService.NavigateTo(NavigationKeys.RentCommercialListingBasic, this);
        }
        else if (_navigationService.IsCurrentPageSameAs(NavigationKeys.RentCommercialListingBrokers))
        {
            _navigationService.NavigateTo(NavigationKeys.RentCommercialListingBasic, this);
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

    public bool TryBeginPicker()
    {
        if (IsBusy) return false;
        IsBusy = true;
        return true;
    }

    public void EndPicker() => IsBusy = false;

    public void DiscardChanges()
    {
        var wasBusy = IsBusy;
        IsBusy = true;
        try
        {
            DiscardUnsavedFiles();
            _picturesToBeDeleted.Clear();
            _pdfsToBeDeleted.Clear();
            _lessorsToBeDeleted.Clear();
            _brokersToBeDeleted.Clear();
            IsDirty = false;
        }
        finally
        {
            IsBusy = wasBusy;
        }
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

    private void ShowErrorInfoBar(ZumenSearch.Application.Results.ErrorInfo err)
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

    private bool ValidateNonNegativeNumericValues((string Label, string Value)[] values)
    {
        foreach (var (label, value) in values)
        {
            var text = value.Trim().Normalize(NormalizationForm.FormKC);
            if (!string.IsNullOrEmpty(text) && (!decimal.TryParse(text, NumberStyles.Number, CultureInfo.CurrentCulture, out var number) || number < 0))
            {
                ShowSimpleErrorInfoBar("入力項目に誤りがあります",$"{label}は0以上の数値で入力してください。");
                return false;
            }
        }

        return true;
    }

    private bool ValidateInput()
    {
        var basicValid = true;
        if (string.IsNullOrWhiteSpace(Name))
        {
            ShowSimpleErrorInfoBar("入力項目に誤りがあります","区画名（必須項目）が入力されていません。保存出来ませんでした。");
            IsNameHasError = true;
            basicValid = false;
        }
        else
        {
            IsNameHasError = false;
            var areaText = ZumenSearch.Presentation.Utilities.Common.NormalizeFullWidthCharacters(FloorArea.Trim());
            if (!string.IsNullOrEmpty(areaText) && (!decimal.TryParse(areaText, NumberStyles.Number, CultureInfo.CurrentCulture, out var floorArea) || floorArea < 0))
            {
                ShowSimpleErrorInfoBar("入力項目に誤りがあります","専有面積は0以上の数値で入力してください。");
                basicValid = false;
            }
            else
            {
                var floorText = ZumenSearch.Presentation.Utilities.Common.NormalizeFullWidthCharacters(FloorNumber.Trim());
                if (!string.IsNullOrEmpty(floorText) && (!int.TryParse(floorText, NumberStyles.Integer, CultureInfo.InvariantCulture, out var floorNumber) || floorNumber <= 0))
                {
                    ShowSimpleErrorInfoBar("入力項目に誤りがあります","所在階は1以上の整数で入力してください。");
                    basicValid = false;
                }
            }
        }

        if (!basicValid)
        {
            ShowSimpleErrorInfoBar("入力項目に誤りがあります", "入力項目に誤りがあります。保存出来ませんでした。");
            if (!_navigationService.IsCurrentPageSameAs(BasicPageName))
            {
                _navigationService.NavigateTo(BasicPageName, this);
            }

            return false;
        }
        (string Label, string Value)[] contractValues =
        [
            ("共益費・管理費", ManagementFee),
            ("敷金", SecurityDeposit),
            ("保証金", GuaranteeDeposit),
            ("礼金", KeyMoney),
            ("権利金", RightsMoney),
            ("敷引・償却金", DepositDeduction),
            ("更新料", RenewalFee),
            ("再契約手数料", RecontractFee),
            ("契約期間", ContractPeriodYears)
        ];

        var contractValid = ValidateNonNegativeNumericValues(contractValues);
        if (contractValid)
        {
            var text = ZumenSearch.Presentation.Utilities.Common.NormalizeFullWidthCharacters(Chinryou.Trim());
            if (!string.IsNullOrEmpty(text))
            {
                if (text.Contains(',', StringComparison.Ordinal))
                {
                    contractValid = long.TryParse(text, NumberStyles.AllowThousands, CultureInfo.InvariantCulture, out var parsedWithSeparators) &&
                        string.Equals(parsedWithSeparators.ToString("N0", CultureInfo.InvariantCulture), text, StringComparison.Ordinal);
                }
                else
                {
                    contractValid = long.TryParse(text, NumberStyles.None, CultureInfo.InvariantCulture, out _);
                }

                if (!contractValid)
                {
                    ShowSimpleErrorInfoBar("入力項目に誤りがあります","賃料は0以上の整数で入力してください。");
                }
            }
        }

        if (!contractValid)
        {
            IsInfoBarErrorOpen = true;
            const string contractPageName = NavigationKeys.RentCommercialListingContract;
            if (!_navigationService.IsCurrentPageSameAs(contractPageName))
            {
                _navigationService.NavigateTo(contractPageName, this);
            }

            return false;
        }

        (string Label, string Value)[] transactionValues =
        [
            ("貸主手数料", LessorCommission),
            ("借主手数料", LesseeCommission),
            ("元付手数料", ListingBrokerCommission),
            ("客付け手数料", TenantBrokerCommission)
        ];

        if (!ValidateNonNegativeNumericValues(transactionValues))
        {
            IsInfoBarErrorOpen = true;
            const string transactionPageName = NavigationKeys.RentCommercialListingTransaction;
            if (!_navigationService.IsCurrentPageSameAs(transactionPageName))
            {
                _navigationService.NavigateTo(transactionPageName, this);
            }

            return false;
        }

        if (Pdfs.Count(pdf => pdf.IsMain) > 1)
        {
            ShowSimpleErrorInfoBar("入力項目に誤りがあります", "メインの図面は1枚だけ選択してください。");
            const string pageName = NavigationKeys.RentCommercialListingDocuments;
            if (IsPropertyUnitLevelOwnership && !_navigationService.IsCurrentPageSameAs(pageName)) _navigationService.NavigateTo(pageName, this);
            return false;
        }

        return true;
    }

    private bool SetValues()
    {
        if (!IsDirty)
        {
            return false;
        }

        if (_listing is null)
        {
            Debug.WriteLine("_listing is null. Can't set listing.");
            return false;
        }

        try
        {
            var floorText = ZumenSearch.Presentation.Utilities.Common.NormalizeFullWidthCharacters(FloorNumber.Trim());
            var areaText = ZumenSearch.Presentation.Utilities.Common.NormalizeFullWidthCharacters(FloorArea.Trim());
            int? floorNumber = null;
            decimal? floorArea = null;
            if (floorText.Length > 0)
            {
                if (!int.TryParse(floorText, NumberStyles.Integer, CultureInfo.InvariantCulture, out var parsedFloor)) return false;
                floorNumber = parsedFloor;
            }
            if (areaText.Length > 0)
            {
                if (!decimal.TryParse(areaText, NumberStyles.Number, CultureInfo.CurrentCulture, out var parsedArea)) return false;
                floorArea = parsedArea;
            }
            var rentText = ZumenSearch.Presentation.Utilities.Common.NormalizeFullWidthCharacters(Chinryou.Trim());
            decimal? rent = null;
            if (rentText.Length > 0)
            {
                if (!long.TryParse(rentText, NumberStyles.AllowThousands, CultureInfo.InvariantCulture, out var parsedRent) || parsedRent < 0)
                {
                    ShowSimpleErrorInfoBar("入力項目に誤りがあります","賃料は0以上の整数で入力してください。");
                    return false;
                }
                rent = parsedRent;
            }
            if (!TryParseRent(ManagementFee, out var managementFee) || !TryParseRent(SecurityDeposit, out var securityDeposit) || !TryParseRent(GuaranteeDeposit, out var guaranteeDeposit) || !TryParseRent(KeyMoney, out var keyMoney) || !TryParseRent(RightsMoney, out var rightsMoney) || !TryParseRent(DepositDeduction, out var depositDeduction) || !TryParseRent(RenewalFee, out var renewalFee) || !TryParseRent(RecontractFee, out var recontractFee) || !TryParseRent(ContractPeriodYears, out var contractPeriodYears))
            {
                ShowSimpleErrorInfoBar("入力項目に誤りがあります", "契約条件は0以上の数値で入力してください。");
                return false;
            }

            _listing.SetName(Name);
            _listing.SetIsPropertyUnitOwnership(IsPropertyUnitLevelOwnership);
            _listing.SetFloorGroundTypeLabel(SelectedFloorGroundType.Key);
            _listing.SetFloorArea(floorArea);
            _listing.SetFloorNumber(floorNumber);
            _listing.SetUseTypeLabel(SelectedUseType.Key);
            _listing.SetUseDetails(UseDetails);
            _listing.SetCurrentStatusTypeLabel(SelectedCurrentStatusType.Key);
            _listing.SetAvailabilityMonthTypeLabel(SelectedAvailabilityMonthType.Key);
            _listing.SetAvailabilityPeriodTypeLabel(SelectedAvailabilityPeriodType.Key);
            _listing.SetIsAvailableForImmediateMoveIn(IsAvailableForImmediateMoveIn);
            _listing.SetIsAvailableForRentNow(IsAvailableForRentNow);
            _listing.SetOccupancyStatusCheckedAt(OccupancyStatusCheckedAt);
            _listing.SetFudousanIdAdditionalCode(FudousanIdAdditionalCode);
            _listing.SetRemarks(Remarks);

            _listing.SetHasAirConditioning(HasAirConditioning);
            _listing.SetHasPrivateToilet(HasPrivateToilet);
            _listing.SetHasHotWater(HasHotWater);
            _listing.SetHasKitchen(HasKitchen);
            _listing.SetHasVentilation(HasVentilation);
            _listing.SetHasInternet(HasInternet);
            _listing.SetEquipmentDetails(EquipmentDetails);
            _listing.SetUnitConditionTypeLabel(SelectedUnitCondition.Key);
            _listing.SetFixtureTransferTypeLabel(SelectedFixtureTransfer.Key);
            _listing.SetBusinessHoursRestrictions(BusinessHoursRestrictions);
            _listing.SetUseConditions(UseConditions);
            _listing.SetPermitsHeavyFoodService(PermitsHeavyFoodService);
            _listing.SetPermitsLightFoodService(PermitsLightFoodService);
            _listing.SetPermitsRetail(PermitsRetail);
            _listing.SetPermitsServices(PermitsServices);
            _listing.SetPermitsOffice(PermitsOffice);
            _listing.SetPermitsOther(PermitsOther);
            _listing.SetPermittedBusinessDetails(PermittedBusinessDetails);
            _listing.SetFacilitiesRemarks(FacilitiesRemarks);

            _listing.SetChinryou(rent);
            _listing.SetManagementFee(managementFee);
            _listing.SetSecurityDeposit(securityDeposit);
            _listing.SetGuaranteeDeposit(guaranteeDeposit);
            _listing.SetKeyMoney(keyMoney);
            _listing.SetRightsMoney(rightsMoney);
            _listing.SetDepositDeduction(depositDeduction);
            _listing.SetRenewalFee(renewalFee);
            _listing.SetRecontractFee(recontractFee);
            _listing.SetContractPeriodYears(contractPeriodYears);
            _listing.SetSecurityDepositUnitTypeLabel(SelectedSecurityDepositUnit.Key);
            _listing.SetGuaranteeDepositUnitTypeLabel(SelectedGuaranteeDepositUnit.Key);
            _listing.SetKeyMoneyUnitTypeLabel(SelectedKeyMoneyUnit.Key);
            _listing.SetRightsMoneyUnitTypeLabel(SelectedRightsMoneyUnit.Key);
            _listing.SetDepositDeductionUnitTypeLabel(SelectedDepositDeductionUnit.Key);
            _listing.SetRenewalFeeUnitTypeLabel(SelectedRenewalFeeUnit.Key);
            _listing.SetRecontractFeeUnitTypeLabel(SelectedRecontractFeeUnit.Key);
            _listing.SetRentTaxTypeLabel(SelectedRentTax.Key);
            _listing.SetManagementFeeTaxTypeLabel(SelectedManagementFeeTax.Key);
            _listing.SetContractTypeLabel(SelectedContractType.Key);
            _listing.SetGuarantorCompany(GuarantorCompany);
            _listing.SetInsurance(Insurance);
            _listing.SetOtherFees(OtherFees);
            _listing.SetContractConditions(ContractConditions);
            _listing.SetContractRemarks(ContractRemarks);
            _listing.SetHandlingTypeLabel(SelectedHandlingType.Key);
            _listing.SetTransactionTypeLabel(SelectedTransactionType.Key);
            _listing.SetLessorCommission(LessorCommission);
            _listing.SetLesseeCommission(LesseeCommission);
            _listing.SetListingBrokerCommission(ListingBrokerCommission);
            _listing.SetTenantBrokerCommission(TenantBrokerCommission);
            _listing.SetLessorCommissionUnitTypeLabel(SelectedLessorCommissionUnit.Key);
            _listing.SetLesseeCommissionUnitTypeLabel(SelectedLesseeCommissionUnit.Key);
            _listing.SetListingBrokerCommissionUnitTypeLabel(SelectedListingBrokerCommissionUnit.Key);
            _listing.SetTenantBrokerCommissionUnitTypeLabel(SelectedTenantBrokerCommissionUnit.Key);
            _listing.SetTransactionRemarks(TransactionRemarks);

            // Recreate the Pictures collection in _listing to ensure it matches the current Pictures collection in the ViewModel.
            _listing.Pictures.Clear();
            foreach (var item in Pictures)
            {
                _listing.Pictures.Add(item.Model.Clone());
            }

            // Recreate the Pdfs collection in _listing to ensure it matches the current Pdfs collection in the ViewModel.
            _listing.Pdfs.Clear();
            foreach (var item in Pdfs)
            {
                _listing.Pdfs.Add(item.Model.Clone());
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

            return true;
        }
        catch (ArgumentException ex)
        {
            var err = new ZumenSearch.Application.Results.ErrorInfo
            {
                Title = "入力値にエラーがあります。物件情報の保存が出来ませんでした。",
                Type = ZumenSearch.Application.Results.ErrorInfo.ErrType.UserInput,
                Code = "",
                Message = ex.Message,
                Description = ex.StackTrace ?? string.Empty,
                Operation = "Input Validation",
                MethodName = nameof(SetValues),
                OccuredAt = DateTime.Now
            };

            ShowErrorInfoBar(err);

            return false;
        }
    }

    private void PopulateValues(bool isDuplicate)
    {
        try
        {
            if (isDuplicate)
            {
                _listing.SetName($"複製_{_listing.Name}");
            }

            Name = _listing.Name; // Set the value to trigger the setter logic if needed.

            // This toggles the visibility of certain fields based on the ownership level of the property.
            IsPropertyUnitLevelOwnership = _listing.IsPropertyUnitOwnership;

            SelectedFloorGroundType = FloorGroundTypes.First(option => option.Key == _listing.FloorGround);
            FloorArea = _listing.FloorArea?.ToString(CultureInfo.CurrentCulture) ?? string.Empty;
            FloorNumber = _listing.FloorNumber?.ToString(CultureInfo.InvariantCulture) ?? string.Empty;
            SelectedUseType = UseTypes.First(option => option.Key == _listing.Use);
            UseDetails = _listing.UseDetails;
            SelectedCurrentStatusType = CurrentStatusTypes.First(option => option.Key == _listing.CurrentStatus);
            SelectedAvailabilityMonthType = AvailabilityMonthTypes.First(option => option.Key == _listing.AvailabilityMonth);
            SelectedAvailabilityPeriodType = AvailabilityPeriodTypes.First(option => option.Key == _listing.AvailabilityPeriod);
            IsAvailableForImmediateMoveIn = _listing.IsAvailableForImmediateMoveIn;
            IsAvailableForRentNow = _listing.IsAvailableForRentNow;
            OccupancyStatusCheckedAt = _listing.OccupancyStatusCheckedAt;
            FudousanIdAdditionalCode = _listing.FudousanIdAdditionalCode;
            Remarks = _listing.Remarks;

            HasAirConditioning = _listing.HasAirConditioning;
            HasPrivateToilet = _listing.HasPrivateToilet;
            HasHotWater = _listing.HasHotWater;
            HasKitchen = _listing.HasKitchen;
            HasVentilation = _listing.HasVentilation;
            HasInternet = _listing.HasInternet;
            EquipmentDetails = _listing.EquipmentDetails;
            SelectedUnitCondition = UnitConditionOptions.First(x => x.Key == _listing.UnitCondition);
            SelectedFixtureTransfer = FixtureTransferOptions.First(x => x.Key == _listing.FixtureTransfer);
            BusinessHoursRestrictions = _listing.BusinessHoursRestrictions;
            UseConditions = _listing.UseConditions;
            PermitsHeavyFoodService = _listing.PermitsHeavyFoodService;
            PermitsLightFoodService = _listing.PermitsLightFoodService;
            PermitsRetail = _listing.PermitsRetail;
            PermitsServices = _listing.PermitsServices;
            PermitsOffice = _listing.PermitsOffice;
            PermitsOther = _listing.PermitsOther;
            PermittedBusinessDetails = _listing.PermittedBusinessDetails;
            FacilitiesRemarks = _listing.FacilitiesRemarks;

            Chinryou = Format(_listing.Chinryou);
            ManagementFee = Format(_listing.ManagementFee);
            SecurityDeposit = Format(_listing.SecurityDeposit);
            GuaranteeDeposit = Format(_listing.GuaranteeDeposit);
            KeyMoney = Format(_listing.KeyMoney);
            RightsMoney = Format(_listing.RightsMoney);
            DepositDeduction = Format(_listing.DepositDeduction);
            RenewalFee = Format(_listing.RenewalFee);
            RecontractFee = Format(_listing.RecontractFee);
            ContractPeriodYears = _listing.ContractPeriodYears?.ToString(System.Globalization.CultureInfo.CurrentCulture) ?? string.Empty;
            SelectedSecurityDepositUnit = FeeUnitTypes.First(x => x.Key == _listing.SecurityDepositUnit);
            SelectedGuaranteeDepositUnit = FeeUnitTypes.First(x => x.Key == _listing.GuaranteeDepositUnit);
            SelectedKeyMoneyUnit = FeeUnitTypes.First(x => x.Key == _listing.KeyMoneyUnit);
            SelectedRightsMoneyUnit = FeeUnitTypes.First(x => x.Key == _listing.RightsMoneyUnit);
            SelectedDepositDeductionUnit = FeeUnitTypes.First(x => x.Key == _listing.DepositDeductionUnit);
            SelectedRenewalFeeUnit = FeeUnitTypes.First(x => x.Key == _listing.RenewalFeeUnit);
            SelectedRecontractFeeUnit = FeeUnitTypes.First(x => x.Key == _listing.RecontractFeeUnit);
            SelectedRentTax = ContractTaxTypes.First(x => x.Key == _listing.RentTax);
            SelectedManagementFeeTax = ContractTaxTypes.First(x => x.Key == _listing.ManagementFeeTax);
            SelectedContractType = ContractTypes.First(x => x.Key == _listing.ContractType);
            GuarantorCompany = _listing.GuarantorCompany;
            Insurance = _listing.Insurance;
            OtherFees = _listing.OtherFees;
            ContractConditions = _listing.ContractConditions;
            ContractRemarks = _listing.ContractRemarks;
            SelectedHandlingType = HandlingTypes.First(x => x.Key == _listing.HandlingType);
            SelectedTransactionType = TransactionTypes.First(x => x.Key == _listing.TransactionType);
            LessorCommission = _listing.LessorCommission;
            LesseeCommission = _listing.LesseeCommission;
            ListingBrokerCommission = _listing.ListingBrokerCommission;
            TenantBrokerCommission = _listing.TenantBrokerCommission;
            SelectedLessorCommissionUnit = CommissionUnitTypes.First(x => x.Key == _listing.LessorCommissionUnit);
            SelectedLesseeCommissionUnit = CommissionUnitTypes.First(x => x.Key == _listing.LesseeCommissionUnit);
            SelectedListingBrokerCommissionUnit = CommissionUnitTypes.First(x => x.Key == _listing.ListingBrokerCommissionUnit);
            SelectedTenantBrokerCommissionUnit = CommissionUnitTypes.First(x => x.Key == _listing.TenantBrokerCommissionUnit);
            TransactionRemarks = _listing.TransactionRemarks;
            WindowTitle = string.Empty;

            // Pictures (don't use directly)
            Pictures.Clear();
            foreach (var item in _listing.Pictures)
            {
                var internalItem = item.Clone();
                internalItem.BasePath = System.IO.Path.Combine(System.IO.Path.Combine(System.IO.Path.Combine(App.PropertyBlobDataFolder, _listing.PropertyId), _listing.Id));
                if (isDuplicate)
                {
                    try
                    {
                        var newFilePath = System.IO.Path.Combine(internalItem.BasePath, internalItem.ImageFilename);
                        // The original item.BasePath should have been set in MainViewModel.
                        File.Copy(System.IO.Path.Combine(item.BasePath, item.ImageFilename), newFilePath, overwrite: true);
                        // Keep track of unsaved files to delete them when discarding.
                        _unsavedListingPictureFileList.Add(newFilePath);

                        internalItem.SetIsModified(true);
                    }
                    catch (Exception ex)
                    {
                        Debug.WriteLine($"Error while copying picture file: {ex}");

                        ShowSimpleErrorInfoBar("写真ファイルのコピー中に予期しないエラーが発生しました。", $"{ex}");
                        continue;
                    }
                }
                else
                {
                    internalItem.SetIsModified(false);
                }

                var pictureViewModel = new ListingPictureViewModel(internalItem, this);
                pictureViewModel.PropertyChanged += OnPicturePropertyChanged;
                Pictures.Add(pictureViewModel);
            }

            Pictures.CollectionChanged += (s, e) =>
            {
                // Unsubscribe from removed items
                if (e.OldItems != null)
                {
                    foreach (ListingPictureViewModel item in e.OldItems)
                    {
                        Debug.WriteLine($"Item {item.Id} Removed from Pictures");
                        IsDirty = true;

                        item.PropertyChanged -= OnPicturePropertyChanged;
                    }
                }

                // Subscribe to PropertyChanged.
                if (e.NewItems != null)
                {
                    foreach (ListingPictureViewModel item in e.NewItems)
                    {
                        Debug.WriteLine($"Item {item.Id} Added to Pictures");
                        IsDirty = true;

                        item.PropertyChanged += OnPicturePropertyChanged;
                    }
                }
            };

            // PDFs (don't use directly)
            Pdfs.Clear();
            foreach (var item in _listing.Pdfs)
            {
                var internalItem = item.Clone();
                internalItem.BasePath = _listingDataDirectoryPath;
                if (isDuplicate)
                {
                    try
                    {
                        var newFilePath = System.IO.Path.Combine(internalItem.BasePath, internalItem.PdfFilename);
                        // The original item.BasePath should have been set in MainViewModel.
                        File.Copy(System.IO.Path.Combine(item.BasePath, item.PdfFilename), newFilePath, overwrite: true);
                        // Keep track of unsaved files to delete them when discarding.
                        _unsavedListingPdfFileList.Add(newFilePath);

                        // Copy the thumbnail file as well
                        if (!string.IsNullOrEmpty(item.ThumbnailFilename))
                        {
                            var thumbnailPath = Path.Combine(internalItem.BasePath, internalItem.ThumbnailFilename);
                            File.Copy(Path.Combine(item.BasePath, item.ThumbnailFilename), thumbnailPath, overwrite: true);
                            _unsavedListingPdfThumbnailFileList.Add(thumbnailPath);
                        }

                        internalItem.SetIsModified(true);
                    }
                    catch (Exception ex)
                    {
                        Debug.WriteLine($"Error while copying PDF file: {ex}");

                        ShowSimpleErrorInfoBar("PDFファイルのコピー中に予期しないエラーが発生しました。", $"{ex}");
                        continue;
                    }
                }
                else
                {
                    internalItem.SetIsModified(false);
                }

                var pdfViewModel = new ListingPdfViewModel(internalItem, this);
                pdfViewModel.PropertyChanged += OnPdfPropertyChanged;
                Pdfs.Add(pdfViewModel);
            }

            Pdfs.CollectionChanged += (sender, args) =>
            {
                if (args.OldItems is not null)
                {
                    foreach (ListingPdfViewModel item in args.OldItems)
                    {
                        IsDirty = true;
                        item.PropertyChanged -= OnPdfPropertyChanged;
                    }
                }

                if (args.NewItems is not null)
                {
                    foreach (ListingPdfViewModel item in args.NewItems)
                    {
                        IsDirty = true;
                        item.PropertyChanged += OnPdfPropertyChanged;
                    }
                }
            };

            // (It is OK to use directly)
            //LessorsWrapper = new ObservableCollection<ZumenSearch.Presentation.Features.Rent.Commercials.Models.ListingPersonViewModel>(_listing.Lessors.Select(person => new ZumenSearch.Presentation.Features.Rent.Commercials.Models.ListingPersonViewModel(person, this)));
            LessorsWrapper.Clear();
            foreach (var person in _listing.Lessors)
            {
                var wrapper = new ZumenSearch.Presentation.Features.Rent.Commercials.ViewModels.ListingPersonViewModel(person, this);
                LessorsWrapper.Add(wrapper);
            }

            // (It is OK to use directly)
            //BrokersWrapper = new ObservableCollection<ZumenSearch.Presentation.Features.Rent.Commercials.Models.ListingPersonViewModel>(_listing.Brokers.Select(person => new ZumenSearch.Presentation.Features.Rent.Commercials.Models.ListingPersonViewModel(person, this)));
            BrokersWrapper.Clear();
            foreach (var person in _listing.Brokers)
            {
                var wrapper = new ZumenSearch.Presentation.Features.Rent.Commercials.ViewModels.ListingPersonViewModel(person, this);
                BrokersWrapper.Add(wrapper);
            }
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"PopulateValues() failed: {ex}");

            var err = new ZumenSearch.Application.Results.ErrorInfo
            {
                Title = "募集物件のデータの取得中に予期しないエラーが発生しました。",
                Type = ZumenSearch.Application.Results.ErrorInfo.ErrType.Other,
                Code = "",
                Message = "このまま保存するとデータに欠損が生じる可能性があります。",
                Description = $"{ex}",
                Operation = "Value Populatation",
                MethodName = nameof(PopulateValues),
                OccuredAt = DateTime.Now
            };

            ShowErrorInfoBar(err);
        }
    }

    private void OnPicturePropertyChanged(object? sender, PropertyChangedEventArgs e)
    {
        if (sender is not ListingPictureViewModel picListing)
        {
            Debug.WriteLine("OnPicturePropertyChanged returned non ListingPicture.");
            return;
        }

        if (picListing.IsModified)
        {
            Debug.WriteLine($"Property {e.PropertyName} changed");
            IsDirty = true;

            var prop = e.PropertyName ?? string.Empty;
            if (prop.Equals("IsMain", StringComparison.Ordinal))
            {
                if (picListing.IsMain)
                {
                    // Clear all other pics
                    foreach (var item in Pictures)
                    {
                        if (item != picListing)
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
        if (sender is not ListingPdfViewModel pdfListing)
        {
            Debug.WriteLine("OnPdfPropertyChanged returned non ListingPdf.");
            return;
        }

        if (pdfListing.IsModified)
        {
            Debug.WriteLine($"Property {e.PropertyName} changed");
            IsDirty = true;

            var prop = e.PropertyName ?? string.Empty;
            if (prop.Equals("IsMain", StringComparison.Ordinal))
            {
                if (pdfListing.IsMain)
                {
                    // Clear all other pics
                    foreach (var item in Pdfs)
                    {
                        if (item != pdfListing)
                        {
                            item.IsMain = false;
                        }
                    }
                }
            }
        }
    }

    private void DiscardUnsavedFiles()
    {
        var failures = new List<string>();

        DeleteTrackedFiles(_unsavedListingPictureFileList, failures);
        DeleteTrackedFiles(_unsavedListingPdfFileList, failures);
        DeleteTrackedFiles(_unsavedListingPdfThumbnailFileList, failures);

        if (_listing.Status == EntityStatus.New && !_isHandedOffToParentEditor && Directory.Exists(_listingDataDirectoryPath))
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
                Debug.WriteLine($"Deleted tracked file '{file}'");
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
                Debug.WriteLine($"Deleted tracked file '{filePath}'");
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

    private static string Format(decimal? value) => value?.ToString(CultureInfo.CurrentCulture) ?? string.Empty;

    private static bool TryParseRent(string? value, out decimal? rent)
    {
        var normalized = ZumenSearch.Presentation.Utilities.Common.NormalizeFullWidthCharacters(value ?? string.Empty).Trim();
        if (normalized.Length == 0)
        {
            rent = null;
            return true;
        }

        if (decimal.TryParse(normalized, NumberStyles.Number, CultureInfo.CurrentCulture, out var parsed) && parsed >= 0)
        {
            rent = parsed;
            return true;
        }

        rent = null;
        return false;
    }

    #endregion

    #region == Commands ==

    #region == Save ==

    public IReadOnlyList<string> CleanupFailures => _cleanupFailures;

    [RelayCommand(CanExecute = nameof(CanSave))]
    public async Task Save()
    {
        if (IsBusy || !IsDirty) return;

        _cleanupFailures.Clear();
        IsBusy = true;
        try
        {
            await SaveCoreAsync();
        }
        finally
        {
            IsBusy = false;
        }
    }

    private async Task SaveCoreAsync()
    {
        if (_listing is null)
        {
            Debug.WriteLine("_listing is null. Can't save listing.");
            return;
        }

        if (!ValidateInput())
        {
            return;
        }

        // This shouldn't happen, but just in case, we check if the parent property editor is open for new listings.
        if (_listing.PropertyStatus == EntityStatus.New && !App.GetService<ZumenSearch.Presentation.Shell.ViewModels.MainViewModel>().RentCommercialEditorList.Any(editor => editor.Id == _listing.PropertyId))
        {
            ShowSimpleErrorInfoBar("予期しないエラーが発生しました", "建物の編集画面が開いていないため、募集物件を保存できません。",InfoBarSeverity.Error);
            return;
        }

        // Preserve the last saved media and contacts if applying values or the database save fails.
        var previousPictures = _listing.Pictures.ToList();
        var previousPdfs = _listing.Pdfs.ToList();
        var previousLessors = _listing.Lessors.ToList();
        var previousBrokers = _listing.Brokers.ToList();
        if (!SetValues())
        {
            _listing.Pictures.Clear();
            _listing.Pictures.AddRange(previousPictures);
            _listing.Pdfs.Clear();
            _listing.Pdfs.AddRange(previousPdfs);
            _listing.Lessors.Clear();
            _listing.Lessors.AddRange(previousLessors);
            _listing.Brokers.Clear();
            _listing.Brokers.AddRange(previousBrokers);
            return;
        }

        var previousPictureDeletions = _listing.PicturesToBeDeleted.ToList();
        var previousPdfDeletions = _listing.PdfsToBeDeleted.ToList();
        var previousLessorDeletions = _listing.LessorsToBeDeleted.ToList();
        var previousBrokerDeletions = _listing.BrokersToBeDeleted.ToList();
        // Publish removals only on save; a contact re-added in the editor cancels its earlier removal.
        _listing.PicturesToBeDeleted.AddRange(_picturesToBeDeleted);
        _listing.PdfsToBeDeleted.AddRange(_pdfsToBeDeleted);
        _listing.LessorsToBeDeleted.RemoveAll(person => _listing.Lessors.Any(item => item.Id.Equals(person.Id, StringComparison.Ordinal)));
        _listing.BrokersToBeDeleted.RemoveAll(person => _listing.Brokers.Any(item => item.Id.Equals(person.Id, StringComparison.Ordinal)));
        foreach (var person in _lessorsToBeDeleted)
        {
            if (!_listing.LessorsToBeDeleted.Any(item => item.Id.Equals(person.Id, StringComparison.Ordinal))) _listing.LessorsToBeDeleted.Add(person);
        }
        foreach (var person in _brokersToBeDeleted)
        {
            if (!_listing.BrokersToBeDeleted.Any(item => item.Id.Equals(person.Id, StringComparison.Ordinal))) _listing.BrokersToBeDeleted.Add(person);
        }

        // PropertyUpdatedMessage updates PropertyStatus when the parent is saved; subsequent listing saves are independent.
        bool isHandedOffToParentEditor;
        if (_listing.PropertyStatus == EntityStatus.New)
        {
            Debug.WriteLine("(_listing.PropertyStatus == EnumPropertyStatus.New) @ListingViewModel on Save. Sending it to Property editor window");

            // The parent property editor owns persistence until the property is saved. Let the parent property editor handle the save operation.
            // This also updates the selected search result and the property's listing list.
            WeakReferenceMessenger.Default.Send(new ZumenSearch.Presentation.Messaging.ListingUpdatedMessage(_listing));

            // The parent now owns the handed-off listing, so discarding this editor must not delete its directory.
            _isHandedOffToParentEditor = true;

            // Stop tracking handed-off files as unsaved by this editor so discard preserves them.
            foreach (var picture in _listing.Pictures)
            {
                _unsavedListingPictureFileList.Remove(Path.Combine(_listingDataDirectoryPath, picture.ImageFilename));
            }
            foreach (var pdf in _listing.Pdfs)
            {
                _unsavedListingPdfFileList.Remove(Path.Combine(_listingDataDirectoryPath, pdf.PdfFilename));
                _unsavedListingPdfThumbnailFileList.Remove(Path.Combine(_listingDataDirectoryPath, pdf.ThumbnailFilename));
            }

            // Protect every filename still referenced by the handed-off listing, including PDF thumbnails.
            var referencedFiles = new HashSet<string>(_listing.Pictures.Select(picture => picture.ImageFilename), StringComparer.OrdinalIgnoreCase);
            foreach (var pdf in _listing.Pdfs)
            {
                referencedFiles.Add(pdf.PdfFilename);
                referencedFiles.Add(pdf.ThumbnailFilename);
            }

            // Delete removed files only if they are no longer referenced; stop tracking each attempted deletion.
            var deletedFiles = _listing.PicturesToBeDeleted.Select(picture => picture.ImageFilename)
                .Concat(_listing.PdfsToBeDeleted.SelectMany(pdf => new[] { pdf.PdfFilename, pdf.ThumbnailFilename }));
            foreach (var filename in deletedFiles.Distinct(StringComparer.OrdinalIgnoreCase))
            {
                if (referencedFiles.Contains(filename)) continue;
                var path = Path.Combine(_listingDataDirectoryPath, filename);
                TryDeleteFiles(new[] { path }, _cleanupFailures);
                _unsavedListingPictureFileList.Remove(path);
                _unsavedListingPdfFileList.Remove(path);
                _unsavedListingPdfThumbnailFileList.Remove(path);
            }
            // These deletions have been processed; failures are reported below rather than retried automatically.
            _listing.PicturesToBeDeleted.Clear();
            _listing.PdfsToBeDeleted.Clear();
            _picturesToBeDeleted.Clear();
            _pdfsToBeDeleted.Clear();
            // Contact deletions remain on the listing for the parent property's database save.
            _lessorsToBeDeleted.Clear();
            _brokersToBeDeleted.Clear();
            if (_cleanupFailures.Count > 0)
            {
                ShowSimpleErrorInfoBar("保存しましたが、一部の削除済みファイルをクリーンアップできませんでした", $"{string.Join(", ", _cleanupFailures)}");
            }
            IsDirty = false;
            isHandedOffToParentEditor = true;
        }
        else
        {
            ZumenSearch.Application.Results.ResultWrapper resUpsert;
            var saved = false;
            try
            {
                resUpsert = await Task.Run(() => _rentCommercialListingService.Save(_listing.PropertyId, _listing), _cts.Token);
                saved = !resUpsert.IsError;
            }
            catch (OperationCanceledException) when (_cts.Token.IsCancellationRequested)
            {
                Debug.WriteLine("Save operation was canceled.");
                // The queued operation did not start. Do not report a database failure.
                return;
            }
            finally
            {
                if (!saved)
                {
                    // Restore the shared media and contact state; the editor retains its changes until saved or discarded.
                    _listing.Pictures.Clear();
                    _listing.Pictures.AddRange(previousPictures);
                    _listing.Pdfs.Clear();
                    _listing.Pdfs.AddRange(previousPdfs);
                    _listing.PicturesToBeDeleted.Clear();
                    _listing.PicturesToBeDeleted.AddRange(previousPictureDeletions);
                    _listing.PdfsToBeDeleted.Clear();
                    _listing.PdfsToBeDeleted.AddRange(previousPdfDeletions);
                    _listing.Lessors.Clear();
                    _listing.Lessors.AddRange(previousLessors);
                    _listing.Brokers.Clear();
                    _listing.Brokers.AddRange(previousBrokers);
                    _listing.LessorsToBeDeleted.Clear();
                    _listing.LessorsToBeDeleted.AddRange(previousLessorDeletions);
                    _listing.BrokersToBeDeleted.Clear();
                    _listing.BrokersToBeDeleted.AddRange(previousBrokerDeletions);
                }
            }

            if (resUpsert.IsError)
            {
                Debug.WriteLine(
                    resUpsert.Error.Title + Environment.NewLine +
                    resUpsert.Error.Message + Environment.NewLine +
                    resUpsert.Error.Description + Environment.NewLine +
                    resUpsert.Error.Operation + Environment.NewLine +
                    resUpsert.Error.MethodName + Environment.NewLine +
                    resUpsert.Error.FullDump);

                ShowErrorInfoBar(resUpsert.Error);
            }
            else
            {
                Debug.WriteLine("No errors on update.");

                _picturesToBeDeleted.Clear();
                _pdfsToBeDeleted.Clear();
                _lessorsToBeDeleted.Clear();
                _brokersToBeDeleted.Clear();
                IsDirty = false;

                isHandedOffToParentEditor = false;
                await PostSaveCleanupAsync(isHandedOffToParentEditor);
            }
        }
    }

    private async Task PostSaveCleanupAsync(bool isHandedOffToParentEditor)
    {
        try
        {
            // Clear error infobar.
            IsInfoBarErrorOpen = false;

            // Update title with dummy value.
            WindowTitle = string.Empty;

            foreach (var picture in Pictures.Where(picture => _listing.Pictures.Any(saved => saved.Id == picture.Id)))
            {
                picture.Model.SetStatus(EntityStatus.Saved);
                picture.Model.SetIsModified(false);
            }

            foreach (var pdf in Pdfs.Where(pdf => _listing.Pdfs.Any(saved => saved.Id == pdf.Id)))
            {
                pdf.Model.SetStatus(EntityStatus.Saved);
                pdf.Model.SetIsModified(false);
            }

            // It is ok to clear the PicturesToBeDeleted and PdfsToBeDeleted lists as well after handed off to parent
            // when EntityStatus.New || isHandedOffToParentEditor,
            // because these are used to delete existing db records which New listings do not have.
            foreach (var file in _listing.PicturesToBeDeleted.ToList())
            {
                var picturePath = Path.Combine(App.PropertyBlobDataFolder, _listing.PropertyId, _listing.Id, file.ImageFilename);
                if (TryDeleteFiles(new[] { picturePath }, _cleanupFailures))
                {
                    _listing.PicturesToBeDeleted.Remove(file);
                }
            }
            _listing.PicturesToBeDeleted.Clear();

            foreach (var file in _listing.PdfsToBeDeleted.ToList())
            {
                var pdfPath = Path.Combine(App.PropertyBlobDataFolder, _listing.PropertyId, _listing.Id, file.PdfFilename);
                var thumbnailPath = Path.Combine(App.PropertyBlobDataFolder, _listing.PropertyId, _listing.Id, file.ThumbnailFilename);
                if (TryDeleteFiles(new[] { pdfPath, thumbnailPath }, _cleanupFailures))
                {
                    _listing.PdfsToBeDeleted.Remove(file);
                }
            }
            _listing.PdfsToBeDeleted.Clear();

            // Clear the LessorsToBeDeleted and BrokersToBeDeleted lists after successful save.
            _listing.LessorsToBeDeleted.Clear();
            _listing.BrokersToBeDeleted.Clear();

            // Clear the unsaved file lists after successful save.
            _unsavedListingPictureFileList.Clear();
            _unsavedListingPdfFileList.Clear();
            _unsavedListingPdfThumbnailFileList.Clear();

            //
            if (_listing.PropertyStatus != EntityStatus.New || !isHandedOffToParentEditor)
            {
                _listing.SetStatus(EntityStatus.Saved);
                _listing.SetIsModified(false);
            }

            //
            WeakReferenceMessenger.Default.Send(new ZumenSearch.Presentation.Messaging.ListingUpdatedMessage(_listing));

            if (_cleanupFailures.Count > 0)
            {
                ShowSimpleErrorInfoBar("保存しましたが、一部の削除済みファイルをクリーンアップできませんでした", $"{string.Join(", ", _cleanupFailures)}");
            }
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"Cleanup() failed: {ex}");

            var err = new ZumenSearch.Application.Results.ErrorInfo
            {
                Title = "募集物件の保存後のクリーンアップ中に予期しないエラーが発生しました。",
                Type = ZumenSearch.Application.Results.ErrorInfo.ErrType.Other,
                Code = "",
                Message = "",
                Description = $"{ex}",
                Operation = "Cleanup",
                MethodName = nameof(PostSaveCleanupAsync),
                OccuredAt = DateTime.Now
            };
            ShowErrorInfoBar(err);
        }
        finally
        {
            IsDirty = false;
        }
    }

    private bool CanSave()
    {
        return IsDirty && !IsBusy;
    }

    #endregion

    #region == Pics and Pdfs ==

    [RelayCommand(CanExecute = nameof(CanAddNewListingPictures))]
    public async Task AddNewListingPictures(List<string> filePathList)
    {
        if (filePathList is null) return;
        if (filePathList.Count == 0 || !IsEditorEnabled) return;

        IsBusy = true;
        try
        {
            Debug.WriteLine($"destDirectory={_listingDataDirectoryPath}  @AddNewListingPictures()");

            foreach (var filePath in filePathList)
            {
                if (string.IsNullOrEmpty(filePath.Trim()))
                {
                    continue;
                }

                string extension = Path.GetExtension(filePath);
                if (!ImageFileHelper.IsSupportedImageExtension(extension))
                {
                    ShowSimpleErrorInfoBar("エラー", $"画像形式に対応していません: {Path.GetFileName(filePath)}");
                    continue;
                }

                string newId = Guid.CreateVersion7().ToString("N");
                string newFilename = newId + extension;
                var destFilePath = Path.Combine(_listingDataDirectoryPath, newFilename);
                bool destinationCreated = false;

                try
                {
                    using var sourceStream = File.Open(filePath, FileMode.Open);

                    using var destinationStream = new FileStream(destFilePath, FileMode.CreateNew, FileAccess.Write);
                    destinationCreated = true;
                    await sourceStream.CopyToAsync(destinationStream);
                }
                catch (Exception ex) when (ex is IOException or UnauthorizedAccessException or COMException or InvalidOperationException or ArgumentException)
                {
                    Debug.WriteLine($"Failed to import image '{filePath}': {ex}");
                    ShowSimpleErrorInfoBar("エラー", $"画像を取り込めませんでした: {Path.GetFileName(filePath)}。{ex.Message}");

                    if (File.Exists(destFilePath) && destinationCreated)
                    {
                        try
                        {
                            File.Delete(destFilePath);
                        }
                        catch (Exception deleteEx) when (deleteEx is IOException or UnauthorizedAccessException)
                        {
                            Debug.WriteLine($"Failed to delete destination file '{destFilePath}' after failed import: {deleteEx}");

                            ShowSimpleErrorInfoBar("エラー", $"画像を取り込めませんでした。さらに一時ファイル削除にも失敗しました: {Path.GetFileName(destFilePath)}。{deleteEx.Message}");
                        }
                    }

                    continue;
                }

                var pic = new ZumenSearch.Domain.Entities.Rent.Commercials.Listing.ListingPicture(newId, newFilename, EntityStatus.New)
                {
                    BasePath = _listingDataDirectoryPath
                };

                Pictures.Add(new ListingPictureViewModel(pic, this));

                OpenBlobDirectoryCommand.NotifyCanExecuteChanged();
                DeleteListingPictureCommand.NotifyCanExecuteChanged();

                IsDirty = true;

                // Keep track of unsaved files to delete them when discarding.
                _unsavedListingPictureFileList.Add(destFilePath);
            }
        }
        finally
        {
            IsBusy = false;
        }
    }
    private static bool CanAddNewListingPictures()
    {
        return true;
    }

    [RelayCommand]
    private async Task OpenListingPicture(ListingPictureViewModel picListing)
    {
        if (picListing is null || !Pictures.Contains(picListing) || !IsEditorEnabled) return;

        IsBusy = true;
        try
        {
            var file = await StorageFile.GetFileFromPathAsync(Path.Combine(_listingDataDirectoryPath, picListing.ImageFilename));
            if (!await Windows.System.Launcher.LaunchFileAsync(file))
            {
                ShowSimpleErrorInfoBar("エラー", "写真を開けませんでした。写真を開くアプリが設定されているか確認してください。");
            }
        }
        catch (Exception ex) when (ex is IOException or UnauthorizedAccessException or COMException or InvalidOperationException or ArgumentException)
        {
            Debug.WriteLine($"Failed to open commercial listing picture: {ex}");
            ShowSimpleErrorInfoBar("エラー", $"写真を開けませんでした: {ex.Message}");
        }
        finally
        {
            IsBusy = false;
        }
    }

    [RelayCommand(CanExecute = nameof(CanDeleteListingPicture))]
    private async Task DeleteListingPicture(ListingPictureViewModel picListing)
    {
        if (_listing is null)
        {
            return;
        }

        if (picListing is null)
        {
            return;
        }

        if (!IsEditorEnabled) return;
        IsBusy = true;
        try
        {
            if (_dialogService is null || await _dialogService.ShowDeleteConfirmationDialog("この募集物件の写真を削除しますか？") != Microsoft.UI.Xaml.Controls.ContentDialogResult.Primary)
            {
                return;
            }

            if (Pictures.Remove(picListing))
            {
                _picturesToBeDeleted.Add(picListing.Model);
                IsDirty = true;
            }
        }
        finally
        {
            IsBusy = false;
        }
    }
    private static bool CanDeleteListingPicture(ListingPictureViewModel picListing)
    {
        return picListing is not null;
    }


    [RelayCommand(CanExecute = nameof(CanDeleteAllListingPictures))]
    private async Task ListingPictureDeleteAll()
    {
        if (!IsEditorEnabled) return;
        IsBusy = true;
        try
        {
            if (await _dialogService.ShowDeleteConfirmationDialog("この物件写真を全て削除しますか？") != Microsoft.UI.Xaml.Controls.ContentDialogResult.Primary) return;
            foreach (var picListing in Pictures.ToList())
            {
                if (Pictures.Remove(picListing))
                {
                    _picturesToBeDeleted.Add(picListing.Model);
                    IsDirty = true;
                }
            }
        }
        finally
        {
            IsBusy = false;
        }
    }
    private bool CanDeleteAllListingPictures()
    {
        if (Pictures.Count == 0) return false;
        return true;
    }

    [RelayCommand(CanExecute = nameof(CanAddNewListingPdfs))]
    public async Task AddNewListingPdfs(List<string> filePathList)
    {
        if (filePathList is null) return;
        if (filePathList.Count == 0 || !IsPropertyUnitLevelOwnership || !IsEditorEnabled) return;
        IsBusy = true;
        try
        {
            Debug.WriteLine($"destDirectory={_listingDataDirectoryPath}  @AddNewListingPdfs()");

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

                //var importedPaths = new List<string>();

                string newId = Guid.CreateVersion7().ToString("N");
                string newThumbnailFilename = newId + ".bmp";
                var thumbnailDestFilePath = Path.Combine(_listingDataDirectoryPath, newThumbnailFilename);
                bool thumbnailDestinationCreated = false;

                string newFilename = newId + extension;
                var destFilePath = Path.Combine(_listingDataDirectoryPath, newFilename);
                bool destinationCreated = false;

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

                        using (var destinationStream = new FileStream(thumbnailDestFilePath, FileMode.CreateNew, FileAccess.Write))
                        {
                            thumbnailDestinationCreated = true;
                            //importedPaths.Add(thumbnailDestFilePath);
                            using var managedSourceStream = stream.AsStreamForRead();
                            await managedSourceStream.CopyToAsync(destinationStream);
                        }

                        using (var pdfSource = File.OpenRead(filePath))
                        using (var pdfDestination = new FileStream(destFilePath, FileMode.CreateNew, FileAccess.Write))
                        {
                            destinationCreated = true;
                            //importedPaths.Add(destFilePath);
                            await pdfSource.CopyToAsync(pdfDestination);
                        }
                    }
                    else
                    {
                        ShowSimpleErrorInfoBar("エラー", $"PDFにページがありません: {Path.GetFileName(filePath)}");
                        continue;
                    }
                }
                catch (Exception ex) when (ex is IOException or UnauthorizedAccessException or System.Runtime.InteropServices.COMException or InvalidOperationException or ArgumentException)
                {
                    Debug.WriteLine($"Failed to import PDF '{filePath}': {ex}");
                    ShowSimpleErrorInfoBar("エラー", $"PDFを取り込めませんでした: {Path.GetFileName(filePath)}。{ex.Message}");

                    if (File.Exists(destFilePath) && destinationCreated)
                    {
                        try
                        {
                            File.Delete(destFilePath);
                        }
                        catch (Exception deleteEx) when (deleteEx is IOException or UnauthorizedAccessException)
                        {
                            Debug.WriteLine($"Failed to delete destination file '{destFilePath}' after failed import: {deleteEx}");
                        }
                    }

                    if (File.Exists(thumbnailDestFilePath) && thumbnailDestinationCreated)
                    {
                        try
                        {
                            File.Delete(thumbnailDestFilePath);
                        }
                        catch (Exception deleteEx) when (deleteEx is IOException or UnauthorizedAccessException)
                        {
                            Debug.WriteLine($"Failed to delete destination file '{thumbnailDestFilePath}' after failed import: {deleteEx}");
                        }
                    }

                    continue;
                }

                var pdf = new ZumenSearch.Domain.Entities.Rent.Commercials.Listing.ListingPdf(newId, newFilename, newThumbnailFilename, EntityStatus.New)
                {
                    BasePath = _listingDataDirectoryPath
                };

                Pdfs.Add(new ListingPdfViewModel(pdf, this));
                _unsavedListingPdfThumbnailFileList.Add(thumbnailDestFilePath);
                _unsavedListingPdfFileList.Add(destFilePath);

                OpenBlobDirectoryCommand.NotifyCanExecuteChanged();
                DeleteListingPdfCommand.NotifyCanExecuteChanged();
                IsDirty = true;
            }
        }
        finally
        {
            IsBusy = false;
        }
    }

    private static bool CanAddNewListingPdfs()
    {
        return true;
    }

    [RelayCommand]
    private async Task OpenListingPdf(ListingPdfViewModel pdf)
    {
        if (pdf is null || !Pdfs.Contains(pdf) || !IsEditorEnabled) return;

        IsBusy = true;
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
            Debug.WriteLine($"Failed to open commercial listing PDF: {ex}");
            ShowSimpleErrorInfoBar("エラー", $"PDFを開けませんでした: {ex.Message}");
        }
        finally
        {
            IsBusy = false;
        }
    }

    [RelayCommand(CanExecute = nameof(CanDeleteListingPdf))]
    private async Task DeleteListingPdf(ListingPdfViewModel pdf)
    {
        if (pdf is null)
        {
            return;
        }

        if (!IsEditorEnabled) return;
        IsBusy = true;
        try
        {
            if (_dialogService is null || await _dialogService.ShowDeleteConfirmationDialog("この募集物件の図面を削除しますか？") != Microsoft.UI.Xaml.Controls.ContentDialogResult.Primary)
            {
                return;
            }

            if (Pdfs.Remove(pdf))
            {
                _pdfsToBeDeleted.Add(pdf.Model);
                IsDirty = true;
            }
        }
        finally
        {
            IsBusy = false;
        }
    }
    private static bool CanDeleteListingPdf(ListingPdfViewModel pdf)
    {
        return pdf is not null;
    }

    [RelayCommand(CanExecute = nameof(CanDeleteAllListingPdfs))]
    private async Task ListingPdfDeleteAll()
    {
        if (!IsEditorEnabled) return;
        IsBusy = true;
        try
        {
            if (await _dialogService.ShowDeleteConfirmationDialog("この物件PDFを全て削除しますか？") != Microsoft.UI.Xaml.Controls.ContentDialogResult.Primary) return;
            foreach (var pdfProp in Pdfs.ToList())
            {
                if (Pdfs.Remove(pdfProp))
                {
                    _pdfsToBeDeleted.Add(pdfProp.Model);
                    IsDirty = true;
                }
            }
        }
        finally
        {
            IsBusy = false;
        }
    }
    private bool CanDeleteAllListingPdfs()
    {
        if (Pdfs.Count == 0) return false;
        return true;
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

        if (!IsEditorEnabled) return;
        IsBusy = true;

        try
        {
            ZumenSearch.Application.SearchResults.PersonSearchResultItem? lessorItem;
            lessorItem = await _dialogService.ShowLessorSelectDialog(new ZumenSearch.Presentation.Dialogs.ViewModels.LessorSelectViewModel(_personService, _cts));

            if (lessorItem is null)
            {
                Debug.WriteLine("lessorItem is null. User cancelled the dialog.");
                return;
            }

            var lessorId = lessorItem.Id;

            // Check if already exists
            var match = LessorsWrapper.FirstOrDefault(x => x.Person.Id.Equals(lessorId, StringComparison.Ordinal));
            if (match is not null)
            {
                Debug.WriteLine($"lessor {lessorItem.Name} already in the list.");
                return;
            }

            // TODO: Catch and ignore catch (OperationCanceledException) when (_cts.Token.IsCancellationRequested). This may not be necessary...but.
            var res = await Task.Run(() => _personService.GetRentLessorById(lessorId), _cts.Token);
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
                ShowSimpleErrorInfoBar("予期しないエラーが発生しました", "選択した貸主を読み込めませんでした。");
                return;
            }

            // Re-adding a contact cancels only this editor's unsaved removal.
            _lessorsToBeDeleted.RemoveAll(person => person.Id.Equals(res.Person.Id, StringComparison.Ordinal));

            var lessorWrapper = new ZumenSearch.Presentation.Features.Rent.Commercials.ViewModels.ListingPersonViewModel(res.Person, this);
            LessorsWrapper.Add(lessorWrapper);

            IsDirty = true;
        }
        finally
        {
            IsBusy = false;
        }
    }

    [RelayCommand(CanExecute = nameof(CanEditLessor))]
    private void EditLessor(ZumenSearch.Presentation.Features.Rent.Commercials.ViewModels.ListingPersonViewModel lessor)
    {
        if (lessor.Person is not null)
        {
            var mainVm = App.GetService<MainViewModel>();
            if (mainVm.EditRentLessorCommand.CanExecute(lessor.Person as ZumenSearch.Domain.Entities.Base.PersonBase))
            {
                mainVm.EditRentLessorCommand.Execute(lessor.Person as ZumenSearch.Domain.Entities.Base.PersonBase);
            }
        }
    }
    private static bool CanEditLessor(ZumenSearch.Presentation.Features.Rent.Commercials.ViewModels.ListingPersonViewModel lessor)
    {
        return lessor is not null;
    }

    [RelayCommand(CanExecute = nameof(CanDeleteLessor))]
    public async Task DeleteLessor(ZumenSearch.Presentation.Features.Rent.Commercials.ViewModels.ListingPersonViewModel lessor)
    {
        if (lessor is null)
        {
            return;
        }

        if (lessor.Person is null)
        {
            return;
        }

        if (await _dialogService.ShowDeleteConfirmationDialog("この貸主を削除しますか？") != Microsoft.UI.Xaml.Controls.ContentDialogResult.Primary) return;


        if (LessorsWrapper.Remove(lessor))
        {
            IsDirty = true;
            if (!_lessorsToBeDeleted.Any(person => person.Id.Equals(lessor.Person.Id, StringComparison.Ordinal))) _lessorsToBeDeleted.Add(lessor.Person);
        }
    }
    private static bool CanDeleteLessor(ZumenSearch.Presentation.Features.Rent.Commercials.ViewModels.ListingPersonViewModel lessor)
    {
        return lessor is not null;
    }

    #endregion

    #region == Broker ==

    [RelayCommand]
    public async Task AddBroker()
    {
        if (_dialogService is null)
        {
            Debug.WriteLine("_dialogService is null");
            return;
        }

        if (!IsEditorEnabled) return;
        IsBusy = true;

        try
        {
            var selectedBroker = await _dialogService.ShowBrokerSelectDialog(new ZumenSearch.Presentation.Dialogs.ViewModels.BrokerSelectViewModel(_personService, _cts));

            if (selectedBroker is null || BrokersWrapper.Any(item => item.Person.Id == selectedBroker.Id))
            {
                return;
            }

            // TODO: Catch and ignore catch (OperationCanceledException) when (_cts.Token.IsCancellationRequested). This may not be necessary...but.
            var res = await Task.Run(() => _personService.GetBrokerById(selectedBroker.Id), _cts.Token);
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
                Debug.WriteLine($"Could not load broker '{selectedBroker.Id}'.");
                ShowSimpleErrorInfoBar("予期しないエラーが発生しました", $"選択した宅建業者を読み込めませんでした。");
                return;
            }

            // Re-adding a contact cancels only this editor's unsaved removal.
            _brokersToBeDeleted.RemoveAll(person => person.Id.Equals(res.Person.Id, StringComparison.Ordinal));

            BrokersWrapper.Add(new ZumenSearch.Presentation.Features.Rent.Commercials.ViewModels.ListingPersonViewModel(res.Person, this));

            IsDirty = true;
        }
        finally
        {
            IsBusy = false;
        }
    }

    [RelayCommand(CanExecute = nameof(CanEditBroker))]
    private void EditBroker(ZumenSearch.Presentation.Features.Rent.Commercials.ViewModels.ListingPersonViewModel? broker)
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

    private static bool CanEditBroker(ZumenSearch.Presentation.Features.Rent.Commercials.ViewModels.ListingPersonViewModel? broker)
    {
        return broker?.Person is not null;
    }

    [RelayCommand(CanExecute = nameof(CanDeleteBroker))]
    private async Task DeleteBroker(ZumenSearch.Presentation.Features.Rent.Commercials.ViewModels.ListingPersonViewModel? broker)
    {
        if (broker?.Person is null)
        {
            return;
        }

        if (await _dialogService.ShowDeleteConfirmationDialog("この宅建業者を削除しますか？") != Microsoft.UI.Xaml.Controls.ContentDialogResult.Primary) return;

        if (BrokersWrapper.Remove(broker))
        {
            if (!_brokersToBeDeleted.Any(person => person.Id.Equals(broker.Person.Id, StringComparison.Ordinal))) _brokersToBeDeleted.Add(broker.Person);

            IsDirty = true;
        }
    }

    private static bool CanDeleteBroker(ZumenSearch.Presentation.Features.Rent.Commercials.ViewModels.ListingPersonViewModel? broker)
    {
        return broker?.Person is not null;
    }

    #endregion

    [RelayCommand]
    private void OpenPropertyEditorWindow()
    {
        var vm = App.GetService<ZumenSearch.Presentation.Shell.ViewModels.MainViewModel>();
        if (vm.EditRentCommercialFromIdCommand.CanExecute(_listing.PropertyId))
        {
            vm.EditRentCommercialFromIdCommand.Execute(_listing.PropertyId);
        }
    }

    #endregion

}
