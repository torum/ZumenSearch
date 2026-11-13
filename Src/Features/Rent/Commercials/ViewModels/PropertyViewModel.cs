using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.Messaging;
using Microsoft.Data.Sqlite;
using Microsoft.UI.Windowing;
using Microsoft.UI.Xaml.Data;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Globalization;
using System.Runtime.InteropServices;
using Windows.Data.Pdf;
using Windows.Storage;
using Windows.Storage.Streams;
using ZumenSearch.Models;
using ZumenSearch.Models.Base;
using ZumenSearch.Models.Location;
using ZumenSearch.Models.Messenger;
using ZumenSearch.Models.Transportation;
using ZumenSearch.Services.Contracts;
using ZumenSearch.Views.Services.Contracts;

namespace ZumenSearch.ViewModels.Rent.Commercials;

public sealed partial class PropertyViewModel : ObservableRecipient, 
    IRecipient<WindowClosedMessage<Microsoft.UI.Xaml.Window>>,
    IRecipient<LessorUpdatedMessage>,
    IRecipient<LessorDeletedMessage>,
    IRecipient<BrokerUpdatedMessage>,
    IRecipient<BrokerDeletedMessage>,
    IRecipient<ListingUpdatedMessage>,
    IRecipient<ListingDeletedMessage>,
    IDisposable
{
    private const string BasicPageName = "ZumenSearch.Views.Rent.Commercials.BasicPage";
    private readonly CancellationTokenSource _cts = new();
    private bool _disposed;
    // Keeps truck of "New" child window because it saves to the parent window/viewmodel.
    public List<Views.Rent.Commercials.Listing.EditorWindow> UnsavedChildEditorList { get; } = [];
    // This property holds the COPY of current entity being edited.
    // Do not use it directly in the UI. Apply changes to this object in Save() to save the changes.
    private readonly Models.Rent.Commercials.Property _building;
    private readonly string _propertyDataDirectoryPath;

    private readonly List<string> _unsavedBuildingPictureFileList = [];
    private readonly List<string> _unsavedBuildingPdfFileList = [];
    private readonly List<string> _unsavedBuildingPdfThumbnailFileList = [];
    private readonly List<Models.Rent.Commercials.PropertyPdf> _committedBuildingPdfs = [];
    // Saved values used to separate handed-off drafts from committed listing data.
    private readonly Dictionary<string, CommittedUnitState> _committedUnits = new(StringComparer.Ordinal);
    private readonly Dictionary<string, Models.Rent.Commercials.CommercialListingDraft> _pendingListingDrafts = new(StringComparer.Ordinal);
    private readonly Dictionary<string, (Dictionary<string, Models.Rent.Commercials.Listing.ListingPicture> Pictures, Dictionary<string, Models.Rent.Commercials.Listing.ListingPdf> Pdfs)> _pendingListingMedia = new(StringComparer.Ordinal);
    private readonly HashSet<string> _stagedListingDeletions = new(StringComparer.Ordinal);
    private readonly HashSet<string> _stagedPendingListingDeletions = new(StringComparer.Ordinal);
    private readonly HashSet<string> _pendingListingDirectoryCleanup = new(StringComparer.Ordinal);
    private readonly HashSet<string> _pendingListingFileCleanup = new(StringComparer.OrdinalIgnoreCase);
    private readonly Dictionary<string, HashSet<string>> _supersededDraftFiles = new(StringComparer.Ordinal);
    private bool _hasPendingFileCleanupAfterSave;
    private bool _discardPending;
    private bool _isClosing;
    private bool _propertyBusy;
    private bool _addressHierarchyChanged;
    private bool _initializationFailed;
    private bool _committedUnitOwnership;
    private int _selectedTransportationIndex;
    private int _pendingPickers;
    private TaskCompletionSource? _pickersCompleted;
    private readonly Queue<Action> _deferredMessages = new();
    private string? _pendingLessorId;
    private string? _pendingBrokerId;
    private Models.Base.PersonBase? _pendingLessorUpdate;
    private Models.Base.PersonBase? _pendingBrokerUpdate;
    private bool _pendingLessorDeleted;
    private bool _pendingBrokerDeleted;

    private readonly INavigationGenericService _navigationService;
    private readonly IDialogGenericService _dialogService;
    private readonly IDispatcherService _dispatcherService;
    private readonly IDataAccessService _dataAccessService;

    private readonly IDataAccessLocationService _dataAccessLocationService;

    public PropertyViewModel(
        Models.Rent.Commercials.Property building,
        INavigationGenericService navigationService,
        IDialogGenericService dialogService,
        IDispatcherService dispatcherService,
        IDataAccessService dataAccessService,
        IDataAccessLocationService dataAccessLocationService)
    {
        _building = building;
        _committedUnitOwnership = building.IsUnitOwnership;
        if (building.Status != EntityStatus.New)
        {
            _committedBuildingPdfs.AddRange(building.Pdfs);
        }
        _navigationService = navigationService;
        _dialogService = dialogService;
        _dispatcherService = dispatcherService;
        _dataAccessService = dataAccessService;
        _dataAccessLocationService = dataAccessLocationService;
        foreach (var entry in TransportationEntries)
        {
            entry.PropertyChanged += TransportationEntryPropertyChanged;
        }

        _propertyDataDirectoryPath = Path.Combine(App.PropertyBlobDataFolder, building.Id);

        // Update title with dummy value.
        WindowTitle = string.Empty;
        try
        {
            PopulateValues();
            _addressHierarchyChanged = false;
            WeakReferenceMessenger.Default.Send(new PropertyIsUnitOwnershipChangedMessage(_building.Id, IsUnitOwnership));

            // Reset errors
            IsNameHasError = false;
            // TODO: more.

            //HasErrors = false;
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"PropertyViewModel: {ex}");
            _initializationFailed = true;
            InfoBarErrorMessage = $"物件情報を読み込めませんでした。編集画面を閉じて再度お試しください: {ex.Message}";
            IsInfoBarErrorOpen = true;
        }
        finally
        {
            IsDirty = false;
        }

        IsDirty = false;

        /*
        // Intercept changes to the inherited IsActive property safely without overriding OnActivated
        this.PropertyChanged += (s, e) =>
        {
            if (e.PropertyName == nameof(IsActive))
            {
                if (IsActive)
                {
                    // This implicitly uses WeakReferenceMessenger.Default under the hood
                    Messenger.Register<WindowClosedMessage<Microsoft.UI.Xaml.Window>>(this);
                    Messenger.Register<LessorUpdatedMessage>(this);
                    Messenger.Register<LessorDeletedMessage>(this);
                    Messenger.Register<BrokerUpdatedMessage>(this);
                    Messenger.Register<BrokerDeletedMessage>(this);
                    Messenger.Register<ListingUpdatedMessage>(this);
                }
                else
                {
                    Messenger.Unregister<WindowClosedMessage<Microsoft.UI.Xaml.Window>>(this);
                    Messenger.Unregister<LessorUpdatedMessage>(this);
                    Messenger.Unregister<LessorDeletedMessage>(this);
                    Messenger.Unregister<BrokerUpdatedMessage>(this);
                    Messenger.Unregister<BrokerDeletedMessage>(this);
                    Messenger.Unregister<ListingUpdatedMessage>(this);
                }
            }
        };
        */
        // Ready to receive messages.
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
            var title = $"{field}";

            if (!string.IsNullOrWhiteSpace(Name))
            {
                title = $"{title}：{Name}";
            }

            title = _building.Status == EntityStatus.New
                ? $"{title}：新規"
                : $"{title}：編集";

            return title;
        }
        set => OnPropertyChanged();
    } = "賃貸事業用";

    public ObservableCollection<Breadcrumb> BreadcrumbItems { get; } =
    [
        new()
        {
            Name = "建物",
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

    #region == Errors ==

    [ObservableProperty]
    public partial bool IsInfoBarErrorOpen { get; set; }

    [ObservableProperty]
    public partial string InfoBarErrorMessage { get; set; } = string.Empty;

    [ObservableProperty]
    public partial bool IsNameHasError { get; private set; }

    #endregion

    #region == 基本ページ ==

    public string Name
    {
        get => field ?? string.Empty;
        set
        {
            if (SetProperty(ref field, value))
            {
                IsDirty = true;
                ValidateName();
                WindowTitle = string.Empty;
            }
        }
    }

    public ObservableCollection<Models.Rent.Commercials.PropertyTypeLabel> Kinds { get; } =
    [
        new(Models.Rent.Commercials.PropertyKindType.Office),
        new(Models.Rent.Commercials.PropertyKindType.Retail),
        new(Models.Rent.Commercials.PropertyKindType.Warehouse),
        new(Models.Rent.Commercials.PropertyKindType.Factory),
        new(Models.Rent.Commercials.PropertyKindType.Clinic),
        new(Models.Rent.Commercials.PropertyKindType.Restaurant),
        new(Models.Rent.Commercials.PropertyKindType.Hotel),
        new(Models.Rent.Commercials.PropertyKindType.Land),
        new(Models.Rent.Commercials.PropertyKindType.Other)
    ];

    public Models.Rent.Commercials.PropertyTypeLabel SelectedKind
    {
        get => field ??
            new(Models.Rent.Commercials.PropertyKindType.Unspecified);
        set
        {
            if (value is null || field?.Key == value.Key)
            {
                return;
            }

            field = value;
            OnPropertyChanged();
            IsDirty = true;
            /*
             * DO not use SetProperty. Kind and Structure are reference types without value equality. 
             * During Bindings.Update(), the ComboBox can write null or a different instance with the same key back to the view model. 
             * SetProperty sees a reference change and sets IsDirty = true.
            if (SetProperty(ref field, value))
            {
                IsDirty = true;
            }
            */
        }
    }

    public ObservableCollection<Models.Rent.Commercials.PropertyStructureTypeLabel> Structures { get; } =
    [
        new(Models.Rent.Commercials.StructureType.Wood),
        new(Models.Rent.Commercials.StructureType.Block),
        new(Models.Rent.Commercials.StructureType.LightSteel),
        new(Models.Rent.Commercials.StructureType.Steel),
        new(Models.Rent.Commercials.StructureType.RC),
        new(Models.Rent.Commercials.StructureType.SRC),
        new(Models.Rent.Commercials.StructureType.ALC),
        new(Models.Rent.Commercials.StructureType.PC),
        new(Models.Rent.Commercials.StructureType.HPC),
        new(Models.Rent.Commercials.StructureType.Other)
    ];

    public Models.Rent.Commercials.PropertyStructureTypeLabel SelectedStructure
    {
        get => field ??
            new(Models.Rent.Commercials.StructureType.Unspecified);
        set
        {
            if (value is null || field?.Key == value.Key)
            {
                return;
            }

            field = value;
            OnPropertyChanged();
            IsDirty = true;
            /*
             * DO not use SetProperty. Kind and Structure are reference types without value equality. 
             * During Bindings.Update(), the ComboBox can write null or a different instance with the same key back to the view model. 
             * SetProperty sees a reference change and sets IsDirty = true.
            if (SetProperty(ref field, value))
            {
                IsDirty = true;
            }
            */
        }
    }

    public bool IsUnitOwnership
    {
        get;
        set
        {
            if (SetProperty(ref field, value))
            {
                IsDirty = true;
                OnPropertyChanged(nameof(IsUnitOwnershipVisible));
                WeakReferenceMessenger.Default.Send(new PropertyIsUnitOwnershipChangedMessage(_building.Id, value));
            }
        }
    }

    public bool IsUnitOwnershipVisible => !IsUnitOwnership;

    public string FloorCountAboveGround
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

    public string FloorCountBasement
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

    public string TotalFloorArea
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

    public DateTimeOffset? BuiltYearAndMonth
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

    public string FudousanId
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

    public string FudousanIdAdditionalCode
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

    #region == Location ==

    public ObservableCollection<Prefecture> Prefectures { get; } =
    new(PrefectureMaster.Prefectures);

    public Prefecture? SelectedPef
    {
        get;
        set
        {
            if (Equals(field, value)) return;
            var cities = value is null ? [] : LoadLocationOptions(() => _dataAccessLocationService.GetCountyAndCityByPref(value.Name).DistinctBy(item => item.Combined).ToList());
            if (cities is null)
            {
                OnPropertyChanged();
                return;
            }
            if (SetProperty(ref field, value))
            {
                IsDirty = true;
                _addressHierarchyChanged = true;
                SelectedCity = null;
                Cities = new(cities);
                OnPropertyChanged(nameof(Cities));
                OnPropertyChanged(nameof(AddressPreview));
            }
        }
    }

    public ObservableCollection<CountyAndCity> Cities { get; private set; } = [];

    public CountyAndCity? SelectedCity
    {
        get;
        set
        {
            if (Equals(field, value)) return;
            var towns = value is null || SelectedPef is null ? [] : LoadLocationOptions(() => _dataAccessLocationService.GetWardAndOazaByPrefCountyCity(SelectedPef.Name, value.County, value.City).DistinctBy(item => item.Combined).ToList());
            if (towns is null)
            {
                OnPropertyChanged();
                return;
            }
            if (SetProperty(ref field, value))
            {
                IsDirty = true;
                _addressHierarchyChanged = true;
                SelectedTown = null;
                Towns = new(towns);
                OnPropertyChanged(nameof(Towns));
                OnPropertyChanged(nameof(AddressPreview));
            }
        }
    }

    public ObservableCollection<WardAndOaza> Towns { get; private set; } = [];

    public WardAndOaza? SelectedTown
    {
        get;
        set
        {
            if (Equals(field, value)) return;
            var chous = value is null || SelectedPef is null || SelectedCity is null ? [] : LoadLocationOptions(() => _dataAccessLocationService.GetChoumeByPrefCountyCityWardOaza(SelectedPef.Name, SelectedCity.County, SelectedCity.City, value.Ward, value.Oaza).DistinctBy(item => item.Chou).ToList());
            if (chous is null)
            {
                OnPropertyChanged();
                return;
            }
            if (SetProperty(ref field, value))
            {
                IsDirty = true;
                _addressHierarchyChanged = true;
                SelectedChou = null;
                Chous = new(chous);
                OnPropertyChanged(nameof(Chous));
                OnPropertyChanged(nameof(AddressPreview));
            }
        }
    }

    public ObservableCollection<Choume> Chous { get; private set; } = [];

    private List<T>? LoadLocationOptions<T>(Func<List<T>> load)
    {
        try
        {
            return load();
        }
        catch (SqliteException ex)
        {
            Debug.WriteLine($"Failed to load address options: {ex}");
            InfoBarErrorMessage = $"住所の選択肢を読み込めませんでした: {ex.Message}";
            IsInfoBarErrorOpen = true;
            return null;
        }
    }

    public Choume? SelectedChou
    {
        get;
        set
        {
            if (SetProperty(ref field, value))
            {
                IsDirty = true;
                _addressHierarchyChanged = true;
                OnPropertyChanged(nameof(AddressPreview));
            }
        }
    }

    public string Edaban
    {
        get => field ?? string.Empty;
        set
        {
            if (SetProperty(ref field, value))
            {
                IsDirty = true;
                OnPropertyChanged(nameof(AddressPreview));
            }
        }
    }

    public string AddressPreview =>
        SelectedPef is null
            ? string.Empty
            : $"{SelectedPef.Name}{SelectedCity?.Combined}" +
              $"{SelectedTown?.Combined}{SelectedChou?.Chou}" +
              (string.IsNullOrWhiteSpace(Edaban) ? string.Empty : $"-{Edaban}");

    // 緯度（Lat）
    public string LocationLatitude
    {
        get;
        set
        {
            if (field == value)
            {
                return;
            }

            field = value;
            IsDirty = true;
            OnPropertyChanged(nameof(LocationLatitude));
            OnPropertyChanged(nameof(GeoUri));
            ShowGoogleMapsCommand.NotifyCanExecuteChanged();
        }
    } = string.Empty;

    // 経度（Lon）
    public string LocationLongitude
    {
        get;
        set
        {
            if (field == value)
            {
                return;
            }

            field = value;
            IsDirty = true;
            OnPropertyChanged(nameof(LocationLongitude));
            OnPropertyChanged(nameof(GeoUri));
            ShowGoogleMapsCommand.NotifyCanExecuteChanged();
        }
    } = string.Empty;

    public string GeoUri
    {
        get
        {
            if (string.IsNullOrEmpty(LocationLatitude) ||
                string.IsNullOrEmpty(LocationLongitude))
            {
                return "https://maps.google.co.jp/";
            }

            return $"https://maps.google.co.jp/?q={LocationLatitude},{LocationLongitude}";
        }
    }

    #endregion

    #region == Transportation Properties ==

    private TransportationEntryViewModel[] TransportationEntries { get; } = [new(), new(), new(), new()];

    public int SelectedTransportationIndex
    {
        get => _selectedTransportationIndex;
        set
        {
            if (value >= 0 && value < TransportationEntries.Length && SetProperty(ref _selectedTransportationIndex, value))
            {
                OnPropertyChanged(nameof(SelectedTransportation));
                ShowRailStationSelectCommand.NotifyCanExecuteChanged();
            }
        }
    }

    public TransportationEntryViewModel SelectedTransportation => TransportationEntries[SelectedTransportationIndex];

    private void TransportationEntryPropertyChanged(object? sender, PropertyChangedEventArgs e)
    {
        IsDirty = true;
        if (e.PropertyName == nameof(TransportationEntryViewModel.RailLine))
        {
            ShowRailStationSelectCommand.NotifyCanExecuteChanged();
        }
    }

    #endregion

    #region == 設備プロパティ ==

    #region == 一般 ==

    public bool HasPassengerElevator
    {
        get;
        set { if (SetProperty(ref field, value)) IsDirty = true; }
    }

    public string PassengerElevatorCount
    {
        get;
        set { if (SetProperty(ref field, value)) IsDirty = true; }
    } = string.Empty;

    public bool HasFreightElevator
    {
        get;
        set { if (SetProperty(ref field, value)) IsDirty = true; }
    }

    public string FreightElevatorDetails
    {
        get;
        set { if (SetProperty(ref field, value)) IsDirty = true; }
    } = string.Empty;

    public bool HasLoadingArea
    {
        get;
        set { if (SetProperty(ref field, value)) IsDirty = true; }
    }

    public string LoadingAreaDetails
    {
        get;
        set { if (SetProperty(ref field, value)) IsDirty = true; }
    } = string.Empty;

    public bool HasSecurity
    {
        get;
        set { if (SetProperty(ref field, value)) IsDirty = true; }
    }

    public bool HasMechanicalSecurity
    {
        get;
        set { if (SetProperty(ref field, value)) IsDirty = true; }
    }

    public bool HasStaffedSecurity
    {
        get;
        set { if (SetProperty(ref field, value)) IsDirty = true; }
    }

    public bool HasOtherSecurity
    {
        get;
        set { if (SetProperty(ref field, value)) IsDirty = true; }
    }

    public string SecurityDetails
    {
        get;
        set { if (SetProperty(ref field, value)) IsDirty = true; }
    } = string.Empty;

    public bool HasAutolock
    {
        get;
        set
        {
            if (SetProperty(ref field, value))
            {
                IsDirty = true;
                OnPropertyChanged(nameof(AppliancePreview));
            }
        }
    }

    public bool HasElevator
    {
        get;
        set
        {
            if (SetProperty(ref field, value))
            {
                IsDirty = true;
                OnPropertyChanged(nameof(AppliancePreview));
            }
        }
    }

    public bool HasSecurityCamera
    {
        get;
        set
        {
            if (SetProperty(ref field, value))
            {
                IsDirty = true;
                OnPropertyChanged(nameof(AppliancePreview));
            }
        }
    }

    public bool HasParcelLocker
    {
        get;
        set
        {
            if (SetProperty(ref field, value))
            {
                IsDirty = true;
                OnPropertyChanged(nameof(AppliancePreview));
            }
        }
    }

    // TODO: Do I use this now?
    public string AppliancePreview
    {
        get
        {
            var s = string.Empty;

            if (HasAutolock)
            {
                s += "オートロック";
            }
            if (HasElevator)
            {
                if (!string.IsNullOrEmpty(s))
                {
                    s += ",";
                }
                s += "エレベーター";
            }
            if (HasSecurityCamera)
            {
                if (!string.IsNullOrEmpty(s))
                {
                    s += ",";
                }
                s += "防犯カメラ";
            }
            if (HasParcelLocker)
            {
                if (!string.IsNullOrEmpty(s))
                {
                    s += ",";
                }
                s += "宅配ボックス";
            }

            return s;
        }
    }

    #endregion

    #region == 電気 ==

    public bool HasElectric
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

    public ObservableCollection<Models.Rent.Commercials.PropertyElectricKind> ElectricKinds { get; set; } =
    [
        new Models.Rent.Commercials.PropertyElectricKind(Models.Rent.Commercials.Property.EnumElectricType.AllElectric, "オール電化"),
        new Models.Rent.Commercials.PropertyElectricKind(Models.Rent.Commercials.Property.EnumElectricType.Unspecified, "未指定")
    ];

    public Models.Rent.Commercials.PropertyElectricKind SelectedElectricKind
    {
        get;
        set
        {
            if (SetProperty(ref field, value))
            {
                IsDirty = true;
            }
        }
    } = new Models.Rent.Commercials.PropertyElectricKind(Models.Rent.Commercials.Property.EnumElectricType.Unspecified, "未指定");

    public string ElectricDetail
    {
        get;
        set
        {
            if (SetProperty(ref field, value))
            {
                IsDirty = true;
            }
        }
    } = string.Empty;

    #endregion

    // TODO: more.

    #endregion

    #region == 管理プロパティ ==

    public ObservableCollection<Models.Rent.Commercials.PropertyKanriShutai> KanriShutais { get; set; } =
    [
        new Models.Rent.Commercials.PropertyKanriShutai(Models.Rent.Commercials.Property.EnumKanriShutai.Unspecified, "未指定"),
        new Models.Rent.Commercials.PropertyKanriShutai(Models.Rent.Commercials.Property.EnumKanriShutai.Jisya, "自社管理"),
        new Models.Rent.Commercials.PropertyKanriShutai(Models.Rent.Commercials.Property.EnumKanriShutai.Tasya, "他社管理"),
        new Models.Rent.Commercials.PropertyKanriShutai(Models.Rent.Commercials.Property.EnumKanriShutai.Kashinushi, "貸主管理")
    ];

    public Models.Rent.Commercials.PropertyKanriShutai? SelectedKanriShutai
    {
        get;
        set
        {
            if (SetProperty(ref field, value))
            {
                IsDirty = true;

                IsKanriUnspecified = field?.Key == Models.Rent.Commercials.Property.EnumKanriShutai.Unspecified;
                IsKanriJisya = field?.Key == Models.Rent.Commercials.Property.EnumKanriShutai.Jisya;
                IsKanriTasya = field?.Key == Models.Rent.Commercials.Property.EnumKanriShutai.Tasya;
                IsKanriKashinushi = field?.Key == Models.Rent.Commercials.Property.EnumKanriShutai.Kashinushi;
            }
        }
    }

    [ObservableProperty]
    public partial bool IsKanriUnspecified { get; set; } = true;

    [ObservableProperty]
    public partial bool IsKanriJisya { get; set; }

    [ObservableProperty]
    public partial bool IsKanriTasya { get; set; }

    [ObservableProperty]
    public partial bool IsKanriKashinushi { get; set; }

    // （自社管理）管理内容:建物維持管理 
    public bool KanriIsMaintenanceManagementIfIsKanriJisya
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

    // （自社管理）管理内容:入居者管理
    public bool KanriIsTenantManagementIfIsKanriJisya
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

    // （自社管理）管理内容:入金管理
    public bool KanriIsPaymentManagementIfIsKanriJisya
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

    // （自社管理）管理内容:その他管理
    public bool KanriIsOtherManagementIfIsKanriJisya
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

    // （自社管理）管理備考
    public string KanriRemarkIfIsKanriJisya
    {
        get => field ?? string.Empty; // Ensure a non-null value is returned
        set
        {
            if (SetProperty(ref field, value))
            {
                IsDirty = true;
            }
        }
    }

    // （他社管理）管理会社名
    public string KanriNameOfCompanyIfIsKanriTasya
    {
        get;
        set
        {
            if (SetProperty(ref field, value))
            {
                IsDirty = true;
            }
        }
    } = string.Empty;

    // （他社管理）管理会社連絡先
    public string KanriContactInfoOfCompanyIfIsKanriTasya
    {
        get;
        set
        {
            if (SetProperty(ref field, value))
            {
                IsDirty = true;
            }
        }
    } = string.Empty;

    // （他社管理）備考
    public string KanriRemarkIfIsKanriTasya
    {
        get => field ?? string.Empty; // Ensure a non-null value is returned
        set
        {
            if (SetProperty(ref field, value))
            {
                IsDirty = true;
            }
        }
    }

    // （貸主管理）備考
    public string KanriRemarkIfIsKanriKashinushi
    {
        get => field ?? string.Empty; // Ensure a non-null value is returned
        set
        {
            if (SetProperty(ref field, value))
            {
                IsDirty = true;
            }
        }
    }


    #endregion

    #region == Units ==

    public ObservableCollection<Models.Rent.Commercials.ListingSummary> Units
    {
        get;
        set
        {
            if (SetProperty(ref field, value))
            {
                IsDirty = true;
            }
        }
    } = [];

    #endregion

    #region == 写真 & PDFプロパティ ==

    public ObservableCollection<Models.Rent.Commercials.PropertyPicture> Pictures
    {
        get;
        set
        {
            if (SetProperty(ref field, value))
            {
                IsDirty = true;//?

                OpenBuildingBlobDirectoryCommand.NotifyCanExecuteChanged();
            }
        }
    } = [];

    public ObservableCollection<Models.Rent.Commercials.PropertyPdf> Pdfs
    {
        get;
        set
        {
            if (SetProperty(ref field, value))
            {
                IsDirty = true;//?

                OpenBuildingBlobDirectoryCommand.NotifyCanExecuteChanged();
            }
        }
    } = [];

    #endregion

    #region == lessors ==

    public ObservableCollection<Models.Rent.Commercials.PersonWrapperForPropertyViewModel> LessorsWrapper
    {
        get;
        set
        {
            if (SetProperty(ref field, value))
            {
                IsDirty = true;
            }
        }
    } = [];

    #endregion

    #region == brokers ==

    public ObservableCollection<Models.Rent.Commercials.PersonWrapperForPropertyViewModel> BrokersWrapper
    {
        get;
        set
        {
            if (SetProperty(ref field, value))
            {
                IsDirty = true;
            }
        }
    } = [];

    #endregion

    #endregion

    #region == Messages ==

    public void Receive(WindowClosedMessage<Microsoft.UI.Xaml.Window> window)
    {
        var ewin = window.Value;

        if (ewin is null)
        {
            return;
        }

        if (ewin is Views.Rent.Commercials.Listing.EditorWindow rcwin)
        {
            this.UnsavedChildEditorList.Remove(rcwin);
        }
    }

    public void Receive(ListingUpdatedMessage listing)
    {
        if (IsSaving && DeferMessage(() => Receive(listing))) return;

        if (listing.Value is not Models.Rent.Commercials.Listing.Listing unit || !unit.PropertyId.Equals(_building.Id, StringComparison.Ordinal)) return;
        if (IsListingPendingDeletion(unit.Id)) return;

        var isPendingDraft = unit.Status == EntityStatus.New && _building.Status == EntityStatus.New;
        if (isPendingDraft)
        {
            var draft = Models.Rent.Commercials.CommercialListingDraft.FromListing(unit);
            if (!_supersededDraftFiles.TryGetValue(unit.Id, out var superseded))
            {
                superseded = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
                _supersededDraftFiles.Add(unit.Id, superseded);
            }
            if (_pendingListingDrafts.TryGetValue(unit.Id, out var previousDraft)) superseded.UnionWith(GetDraftAttachmentFiles(previousDraft));
            superseded.ExceptWith(GetDraftAttachmentFiles(draft));
            _pendingListingDrafts[unit.Id] = draft;
            _pendingListingMedia[unit.Id] = (unit.Pictures.Concat(unit.PicturesToBeDeleted).ToDictionary(picture => picture.Id, StringComparer.Ordinal), unit.Pdfs.Concat(unit.PdfsToBeDeleted).ToDictionary(pdf => pdf.Id, StringComparer.Ordinal));
            if (!_committedUnits.ContainsKey(unit.Id)) StoreEmptyCommittedUnitState(unit.Id);
        }
        else if (unit.Status == EntityStatus.Saved)
        {
            StoreCommittedUnitState(unit);
            var directory = Path.Combine(_propertyDataDirectoryPath, unit.Id);
            foreach (var picture in unit.Pictures) _pendingListingFileCleanup.Remove(Path.Combine(directory, picture.ImageFilename));
            foreach (var pdf in unit.Pdfs)
            {
                _pendingListingFileCleanup.Remove(Path.Combine(directory, pdf.PdfFilename));
                _pendingListingFileCleanup.Remove(Path.Combine(directory, pdf.ThumbnailFilename));
            }
        }

        var summary = new Models.Rent.Commercials.ListingSummary(unit.Id, unit.Name, isPendingDraft);
        var existing = Units.FirstOrDefault(item => item.Id.Equals(unit.Id, StringComparison.Ordinal));
        if (existing is null) Units.Add(summary);
        else Units[Units.IndexOf(existing)] = summary;
        if (!_building.ListingIds.Contains(unit.Id, StringComparer.Ordinal)) _building.ListingIds.Add(unit.Id);

        if (isPendingDraft)
        {
            IsDirty = true;
        }
        else
        {
            var win = UnsavedChildEditorList.FirstOrDefault(w => w is Views.Rent.Commercials.Listing.EditorWindow ew && ew.Id == unit.Id);
            if (win is not null) UnsavedChildEditorList.Remove(win);
        }
    }

    public void Receive(ListingDeletedMessage message)
    {
        if (DeferMessage(() => Receive(message))) return;
        _stagedListingDeletions.Remove(message.Value);
        _stagedPendingListingDeletions.Remove(message.Value);
        RemoveListingSummary(message.Value);
    }

    private IEnumerable<string> GetDraftAttachmentFiles(Models.Rent.Commercials.CommercialListingDraft draft)
    {
        var directory = Path.Combine(_propertyDataDirectoryPath, draft.Id);
        return draft.Pictures.Concat(draft.PicturesToBeDeleted).Select(picture => Path.Combine(directory, picture.ImageFilename))
            .Concat(draft.Pdfs.Concat(draft.PdfsToBeDeleted).SelectMany(pdf => new[] { Path.Combine(directory, pdf.PdfFilename), Path.Combine(directory, pdf.ThumbnailFilename) }));
    }

    private void RemoveListingSummary(string listingId)
    {
        var summary = Units.FirstOrDefault(item => item.Id.Equals(listingId, StringComparison.Ordinal));
        if (summary is not null) Units.Remove(summary);
        while (_building.ListingIds.Remove(listingId)) { }
        _pendingListingDrafts.Remove(listingId);
        _pendingListingMedia.Remove(listingId);
        _supersededDraftFiles.Remove(listingId);
        _committedUnits.Remove(listingId);
    }

    public void Receive(LessorUpdatedMessage message)
    {
        if (DeferMessage(() => Receive(message))) return;

        var lessor = message.Value;

        if (lessor is null)
        {
            return;
        }

        if (_pendingLessorId == lessor.Id) _pendingLessorUpdate = lessor;
        var wrapper = LessorsWrapper.FirstOrDefault(item => item.Person.Id == lessor.Id);
        if (wrapper is null) return;

        if ((wrapper.Person is Models.Person.LegalPerson) != (lessor is Models.Person.LegalPerson))
        {
            LessorsWrapper[LessorsWrapper.IndexOf(wrapper)] = new Models.Rent.Commercials.PersonWrapperForPropertyViewModel(lessor, this);
        }
        else
        {
            wrapper.Person = lessor;
        }
    }

    public void Receive(LessorDeletedMessage message)
    {
        if (DeferMessage(() => Receive(message))) return;

        var lessorId = message.Value;

        if (string.IsNullOrWhiteSpace(lessorId))
        {
            return;
        }

        if (_pendingLessorId == lessorId) _pendingLessorDeleted = true;
        var wrapper = LessorsWrapper
            .FirstOrDefault(item => item.Person.Id == lessorId);

        if (wrapper is not null)
        {
            LessorsWrapper.Remove(wrapper);
            _building.LessorsToBeDeleted.Add(wrapper.Person);
            IsDirty = true;
        }
    }

    public void Receive(BrokerUpdatedMessage message)
    {
        if (DeferMessage(() => Receive(message))) return;

        var broker = message.Value;

        if (broker is null)
        {
            return;
        }

        if (_pendingBrokerId == broker.Id) _pendingBrokerUpdate = broker;
        var wrapper = BrokersWrapper
            .FirstOrDefault(item => item.Person.Id == broker.Id);

        wrapper?.Person = broker;
    }

    public void Receive(BrokerDeletedMessage message)
    {
        if (DeferMessage(() => Receive(message))) return;

        var brokerId = message.Value;

        if (string.IsNullOrWhiteSpace(brokerId))
        {
            return;
        }

        if (_pendingBrokerId == brokerId) _pendingBrokerDeleted = true;
        var wrapper = BrokersWrapper
            .FirstOrDefault(item => item.Person.Id == brokerId);

        if (wrapper is not null)
        {
            BrokersWrapper.Remove(wrapper);
            _building.BrokersToBeDeleted.Add(wrapper.Person);
            IsDirty = true;
        }
    }


    #endregion

    #region == Public Methods ==

    public void SetListingSummaries(IEnumerable<Models.Rent.Commercials.ListingSummary> summaries)
    {
        Units = new ObservableCollection<Models.Rent.Commercials.ListingSummary>(summaries);
        IsDirty = false;
    }

    public bool IsEditorEnabled => !IsSaving && !_isClosing && !_propertyBusy && !_initializationFailed && !_discardPending;
    public bool IsClosing => _isClosing;
    public bool IsPropertyBusy => _propertyBusy;
    public bool IsDiscardPending => _discardPending;
    public bool HasPendingFileCleanupAfterSave => _hasPendingFileCleanupAfterSave;
    public bool HasPendingEditorOperations => _pendingPickers > 0 || PendingEditorTasks().Count > 0;
    public bool HasUncommittedOwnershipChange => IsUnitOwnership != _committedUnitOwnership;
    public bool CommittedUnitOwnership => _committedUnitOwnership;
    public bool IsListingPendingDeletion(string listingId) => _stagedListingDeletions.Contains(listingId) || _stagedPendingListingDeletions.Contains(listingId);

    public Models.Rent.Commercials.CommercialListingDraft? GetPendingListingDraft(string listingId) =>
        _pendingListingDrafts.TryGetValue(listingId, out var draft) ? draft : null;

    public Models.Rent.Commercials.CommercialListingCommittedData? GetCommittedListingData(string listingId)
    {
        return _committedUnits.TryGetValue(listingId, out var committed)
            ? new(committed.Pdfs, committed.LessorIds.ToArray(), committed.BrokerIds.ToArray())
            : null;
    }

    public void SetPropertyBusy(bool value)
    {
        _propertyBusy = value;
        OnPropertyChanged(nameof(IsEditorEnabled));
        SaveCommand.NotifyCanExecuteChanged();
        if (!value && !IsSaving) while (_deferredMessages.TryDequeue(out var message)) message();
    }

    public void BeginPicker()
    {
        if (_pendingPickers++ == 0)
        {
            _pickersCompleted = new TaskCompletionSource(TaskCreationOptions.RunContinuationsAsynchronously);
        }
    }

    public void EndPicker()
    {
        if (--_pendingPickers == 0)
        {
            _pickersCompleted?.TrySetResult();
            _pickersCompleted = null;
        }
    }

    public void SetClosing(bool value)
    {
        _isClosing = value;
        OnPropertyChanged(nameof(IsEditorEnabled));
        SaveCommand.NotifyCanExecuteChanged();
    }

    public async Task WaitForEditorOperationsAsync()
    {
        while (true)
        {
            var tasks = PendingEditorTasks();
            if (_pickersCompleted is not null) tasks.Add(_pickersCompleted.Task);
            if (tasks.Count == 0) return;
            await Task.WhenAll(tasks);
        }
    }

    private List<Task> PendingEditorTasks()
    {
        IAsyncRelayCommand[] commands = [AddNewBuildingPicturesCommand, OpenBuildingPictureCommand, DeleteBuildingPictureCommand, AddNewBuildingPdfsCommand, OpenBuildingPdfCommand, DeleteBuildingPdfCommand, DeleteSelectedUnitCommand, AddLessorCommand, AddBrokerCommand, ShowRailLineSelectCommand, ShowRailStationSelectCommand, EditLessorCommand, EditBrokerCommand];
        return commands.Where(command => command.IsRunning && command.ExecutionTask is not null).Select(command => command.ExecutionTask!).ToList();
    }

    private bool DeferMessage(Action action)
    {
        if (!IsSaving && !_propertyBusy) return false;
        _deferredMessages.Enqueue(action);
        return true;
    }

    public bool DiscardChanges()
    {
        if (_hasPendingFileCleanupAfterSave || _pendingListingFileCleanup.Count > 0)
        {
            InfoBarErrorMessage = "保存済みデータの削除済みファイルが残っています。ファイルの削除を完了するため、もう一度保存してください。";
            IsInfoBarErrorOpen = true;
            return false;
        }

        _discardPending = true;
        OnPropertyChanged(nameof(IsEditorEnabled));
        SaveCommand.NotifyCanExecuteChanged();

        if (!CleanupIncompleteImports()) return false;
        var failures = new List<string>();
        foreach (var tracked in new[] { _unsavedBuildingPictureFileList, _unsavedBuildingPdfThumbnailFileList, _unsavedBuildingPdfFileList })
        {
            foreach (var file in tracked.ToList())
            {
                if (TryDeleteFiles(new[] { file }, failures)) tracked.Remove(file);
            }
        }

        if (_building.Status == EntityStatus.New && Directory.Exists(_propertyDataDirectoryPath))
        {
            try
            {
                Directory.Delete(_propertyDataDirectoryPath, true);
                _pendingListingDirectoryCleanup.Clear();
            }
            catch (Exception ex) when (ex is IOException or UnauthorizedAccessException)
            {
                Debug.WriteLine($"Failed to delete commercial property directory '{_propertyDataDirectoryPath}': {ex}");
                failures.Add(_propertyDataDirectoryPath);
            }
        }

        if (failures.Count > 0)
        {
            InfoBarErrorMessage = $"変更の破棄が完了していません。対象のファイルを閉じ、フォルダーに残っているファイルを確認してから、もう一度画面を閉じて再試行してください: {string.Join(", ", failures)}";
            IsInfoBarErrorOpen = true;
            return false;
        }

        _building.PicturesToBeDeleted.Clear();
        _building.PdfsToBeDeleted.Clear();
        WeakReferenceMessenger.Default.Send(new PropertyIsUnitOwnershipChangedMessage(_building.Id, _committedUnitOwnership));
        _discardPending = false;
        IsDirty = false;
        IsInfoBarErrorOpen = false;
        return true;
    }

    public void Dispose()
    {
        if (_disposed)
        {
            return;
        }

        _disposed = true;
        foreach (var entry in TransportationEntries)
        {
            entry.PropertyChanged -= TransportationEntryPropertyChanged;
        }

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
        Name = _building.Name;
        WindowTitle = string.Empty;

        SelectedKind =
            Kinds.FirstOrDefault(
                item => item.Key == _building.CommercialKind.Key)
            ?? new(
                Models.Rent.Commercials.PropertyKindType.Unspecified);

        SelectedStructure =
            Structures.FirstOrDefault(
                item => item.Key == _building.PropertyStructure.Key)
            ?? new(
                Models.Rent.Commercials.StructureType.Unspecified);

        IsUnitOwnership = _building.IsUnitOwnership;

        FloorCountAboveGround =
            _building.FloorCountAboveGround == 0
                ? string.Empty
                : _building.FloorCountAboveGround.ToString(CultureInfo.InvariantCulture);

        FloorCountBasement =
            _building.FloorCountBasement == 0
                ? string.Empty
                : _building.FloorCountBasement.ToString(CultureInfo.InvariantCulture);

        TotalFloorArea =
            _building.TotalFloorArea == 0
                ? string.Empty
                : _building.TotalFloorArea.ToString(
                    CultureInfo.CurrentCulture);

        BuiltYearAndMonth =
            _building.BuiltYearAndMonth.Year == 1900
                ? null
                : _building.BuiltYearAndMonth;

        FudousanId = _building.FudousanId;
        FudousanIdAdditionalCode =
            _building.FudousanIdAdditionalCode;
        Remarks = _building.Remarks;

        var address = _building.Address;
        if (address.Prefecture is not null)
        {
            var prefecture = Prefectures.FirstOrDefault(item => item.MunicipalityCode == address.Prefecture.MunicipalityCode);
            if (prefecture is null)
            {
                prefecture = address.Prefecture;
                Prefectures.Add(prefecture);
            }
            SelectedPef = prefecture;
            if (SelectedPef is null)
            {
                throw new InvalidOperationException("住所の選択肢を読み込めませんでした。");
            }
        }

        if (SelectedPef is not null && address.CountyAndCity is { } storedCity && (!string.IsNullOrEmpty(storedCity.County) || !string.IsNullOrEmpty(storedCity.City)))
        {
            var city = Cities.FirstOrDefault(item => item.County == storedCity.County && item.City == storedCity.City);
            if (city is null)
            {
                city = storedCity;
                Cities.Add(city);
            }
            SelectedCity = city;
            if (SelectedCity is null)
            {
                throw new InvalidOperationException("住所の選択肢を読み込めませんでした。");
            }
        }

        if (SelectedCity is not null && address.WardAndOaza is { } storedTown && (!string.IsNullOrEmpty(storedTown.Ward) || !string.IsNullOrEmpty(storedTown.Oaza)))
        {
            var town = Towns.FirstOrDefault(item => item.Ward == storedTown.Ward && item.Oaza == storedTown.Oaza);
            if (town is null)
            {
                town = storedTown;
                Towns.Add(town);
            }
            SelectedTown = town;
            if (SelectedTown is null)
            {
                throw new InvalidOperationException("住所の選択肢を読み込めませんでした。");
            }
        }

        if (SelectedTown is not null && address.Choume is { } storedChou)
        {
            var choume = Chous.FirstOrDefault(item => item.Chou == storedChou.Chou);
            if (choume is null)
            {
                choume = storedChou;
                Chous.Add(choume);
            }
            SelectedChou = choume;
        }

        Edaban = address.Edaban;
        LocationLatitude = address.LocationLatitude;
        LocationLongitude = address.LocationLongitude;

        TransportationEntries[0].Load(_building.Train1, _building.BusStop1, _building.BusJyousya1, _building.BusStopToho1);
        TransportationEntries[1].Load(_building.Train2, _building.BusStop2, _building.BusJyousya2, _building.BusStopToho2);
        TransportationEntries[2].Load(_building.Train3, _building.BusStop3, _building.BusJyousya3, _building.BusStopToho3);
        TransportationEntries[3].Load(_building.Train4, _building.BusStop4, _building.BusJyousya4, _building.BusStopToho4);

        HasPassengerElevator = _building.HasPassengerElevator;
        PassengerElevatorCount = _building.PassengerElevatorCount?.ToString(CultureInfo.InvariantCulture) ?? string.Empty;
        HasFreightElevator = _building.HasFreightElevator;
        FreightElevatorDetails = _building.FreightElevatorDetails;
        HasLoadingArea = _building.HasLoadingArea;
        LoadingAreaDetails = _building.LoadingAreaDetails;
        HasSecurity = _building.HasSecurity;
        HasMechanicalSecurity = _building.HasMechanicalSecurity;
        HasStaffedSecurity = _building.HasStaffedSecurity;
        HasOtherSecurity = _building.HasOtherSecurity;
        SecurityDetails = _building.SecurityDetails;
        LoadRemainingFacilities();

        HasElevator = _building.HasElevator;
        HasAutolock = _building.HasAutolock;
        HasSecurityCamera = _building.HasSecurityCamera;
        HasParcelLocker = _building.HasParcelLocker;

        SelectedKanriShutai = KanriShutais.First(item => item.Key == _building.KanriShutai);
        KanriIsMaintenanceManagementIfIsKanriJisya = _building.KanriIsMaintenanceManagementIfIsKanriJisya;
        KanriIsTenantManagementIfIsKanriJisya = _building.KanriIsTenantManagementIfIsKanriJisya;
        KanriIsPaymentManagementIfIsKanriJisya = _building.KanriIsPaymentManagementIfIsKanriJisya;
        KanriIsOtherManagementIfIsKanriJisya = _building.KanriIsOtherManagementIfIsKanriJisya;
        KanriRemarkIfIsKanriJisya = _building.KanriRemarkIfIsKanriJisya;
        KanriNameOfCompanyIfIsKanriTasya = _building.KanriNameOfCompanyIfIsKanriTasya;
        KanriContactInfoOfCompanyIfIsKanriTasya = _building.KanriContactInfoOfCompanyIfIsKanriTasya;
        KanriRemarkIfIsKanriTasya = _building.KanriRemarkIfIsKanriTasya;
        KanriRemarkIfIsKanriKashinushi = _building.KanriRemarkIfIsKanriKashinushi;

        // Pictures:
        /*
        Pictures = new ObservableCollection<Models.Rent.Residentials.Picture>(_building.Pictures); // create a copy.

        foreach (var item in Pictures)
        {
            // Filename to actuall path.
            item.BasePath = System.IO.Path.Combine(App.PropertyBlobDataFolder, _building.Id);
            item.ParentViewModel = this;
            item.IsModified = false; // Needed this.
            item.PropertyChanged += OnBuildingPicturePropertyChanged;
        }
        */
        Pictures = new ObservableCollection<Models.Rent.Commercials.PropertyPicture>(_building.Pictures);

        foreach (var picture in Pictures)
        {
            TrackBuildingPicture(picture);
        }

        Pdfs = new ObservableCollection<Models.Rent.Commercials.PropertyPdf>(_building.Pdfs);

        foreach (var pdf in Pdfs)
        {
            TrackBuildingPdf(pdf);
        }

        // Lessors:
        LessorsWrapper.Clear();

        foreach (var person in _building.Lessors)
        {
            LessorsWrapper.Add(
                new Models.Rent.Commercials.PersonWrapperForPropertyViewModel(
                    person,
                    this));
        }

        BrokersWrapper.Clear();

        foreach (var person in _building.Brokers)
        {
            BrokersWrapper.Add(
                new Models.Rent.Commercials.PersonWrapperForPropertyViewModel(
                    person,
                    this));
        }

        Units = [];
    }

    private void TrackBuildingPicture(Models.Rent.Commercials.PropertyPicture picture)
    {
        picture.BasePath = _propertyDataDirectoryPath;
        picture.ParentViewModel = this;
        picture.PropertyChanged -= OnBuildingMediaPropertyChanged;
        picture.PropertyChanged += OnBuildingMediaPropertyChanged;
        picture.SetIsModified(false);
    }

    private void TrackBuildingPdf(Models.Rent.Commercials.PropertyPdf pdf)
    {
        pdf.BasePath = _propertyDataDirectoryPath;
        pdf.ParentViewModel = this;
        pdf.PropertyChanged -= OnBuildingMediaPropertyChanged;
        pdf.PropertyChanged += OnBuildingMediaPropertyChanged;
        pdf.SetIsModified(false);
    }

    private sealed class CommittedUnitState
    {
        public List<Models.Rent.Commercials.CommercialListingPdfDraft> Pdfs { get; set; } = [];
        public HashSet<string> LessorIds { get; } = new(StringComparer.Ordinal);
        public HashSet<string> BrokerIds { get; } = new(StringComparer.Ordinal);
    }

    private void StoreCommittedUnitState(Models.Rent.Commercials.CommercialListingDraft draft)
    {
        var committed = new CommittedUnitState { Pdfs = draft.Pdfs.ToList() };
        committed.LessorIds.UnionWith(draft.LessorIds);
        committed.BrokerIds.UnionWith(draft.BrokerIds);
        _committedUnits[draft.Id] = committed;
    }

    private void StoreCommittedUnitState(Models.Rent.Commercials.Listing.Listing listing) =>
        StoreCommittedUnitState(Models.Rent.Commercials.CommercialListingDraft.FromListing(listing));

    private void StoreEmptyCommittedUnitState(string unitId)
    {
        _committedUnits[unitId] = new CommittedUnitState();
    }

    private void OnBuildingMediaPropertyChanged(object? sender, PropertyChangedEventArgs e)
    {
        if (sender is Models.Rent.Commercials.PropertyPicture picture)
        {
            picture.SetIsModified(true);
            if (e.PropertyName == nameof(Models.Rent.Commercials.PropertyPicture.IsMain) && picture.IsMain)
            {
                foreach (var other in Pictures)
                {
                    if (other != picture && other.IsMain) other.IsMain = false;
                }
            }
        }
        else if (sender is Models.Rent.Commercials.PropertyPdf pdf)
        {
            pdf.SetIsModified(true);
            if (e.PropertyName == nameof(Models.Rent.Commercials.PropertyPdf.IsMain) && pdf.IsMain)
            {
                foreach (var other in Pdfs)
                {
                    if (other != pdf && other.IsMain) other.IsMain = false;
                }
            }
        }

        IsDirty = true;
    }

    private void ReconcileOwnershipScopedDraftsAfterSave()
    {
        _building.LessorsToBeDeleted.Clear();
        _building.BrokersToBeDeleted.Clear();
        _building.Lessors = new ObservableCollection<Models.Base.PersonBase>(LessorsWrapper.Select(item => item.Person));
        _building.Brokers = new ObservableCollection<Models.Base.PersonBase>(BrokersWrapper.Select(item => item.Person));

        foreach (var draft in _pendingListingDrafts.Values) StoreCommittedUnitState(draft);
        _pendingListingDrafts.Clear();
        _pendingListingMedia.Clear();
        for (var index = 0; index < Units.Count; index++)
        {
            var summary = Units[index];
            if (summary.IsPendingDraft) Units[index] = new(summary.Id, summary.Name, false);
        }
    }

    private void SetValues()
    {
        _building.SetName(Name);
        _building.CommercialKind = SelectedKind;
        _building.PropertyStructure = SelectedStructure;
        _building.IsUnitOwnership = IsUnitOwnership;

        _building.FloorCountAboveGround =
            ParseInteger(FloorCountAboveGround);

        _building.FloorCountBasement =
            ParseInteger(FloorCountBasement);

        _building.TotalFloorArea =
            ParseDecimal(TotalFloorArea);

        _building.BuiltYearAndMonth =
            BuiltYearAndMonth
            ?? new DateTimeOffset(
                1900,
                1,
                1,
                0,
                0,
                0,
                TimeSpan.Zero);

        _building.FudousanId =
            Helpers.Common.ReplaceZenkakuNumbers(FudousanId);

        _building.FudousanIdAdditionalCode =
            Helpers.Common.ReplaceZenkakuNumbers(
                FudousanIdAdditionalCode);

        _building.Remarks = Remarks;

        if (_addressHierarchyChanged)
        {
            _building.Address.SetMachiazaId(SelectedChou?.MachiazaId ?? SelectedTown?.MachiazaId ?? SelectedCity?.MachiazaId ?? string.Empty);
            _building.Address.SetPrefecture(SelectedPef);
            _building.Address.SetCountyAndCity(SelectedCity);
            _building.Address.SetWardAndOaza(SelectedTown);
            _building.Address.SetChoume(SelectedChou);
        }
        _building.Address.SetEdaban(Edaban);
        _building.Address.SetLocationLatitude(LocationLatitude);
        _building.Address.SetLocationLongitude(LocationLongitude);
        TransportationEntries[0].ApplyTrain(_building.Train1);
        TransportationEntries[1].ApplyTrain(_building.Train2);
        TransportationEntries[2].ApplyTrain(_building.Train3);
        TransportationEntries[3].ApplyTrain(_building.Train4);
        _building.SetBusStop1(TransportationEntries[0].BusStop);
        _building.SetBusStop2(TransportationEntries[1].BusStop);
        _building.SetBusStop3(TransportationEntries[2].BusStop);
        _building.SetBusStop4(TransportationEntries[3].BusStop);
        _building.SetBusJyousya1(Helpers.Common.ReplaceZenkakuNumbers(TransportationEntries[0].BusRide));
        _building.SetBusJyousya2(Helpers.Common.ReplaceZenkakuNumbers(TransportationEntries[1].BusRide));
        _building.SetBusJyousya3(Helpers.Common.ReplaceZenkakuNumbers(TransportationEntries[2].BusRide));
        _building.SetBusJyousya4(Helpers.Common.ReplaceZenkakuNumbers(TransportationEntries[3].BusRide));
        _building.SetBusStopToho1(Helpers.Common.ReplaceZenkakuNumbers(TransportationEntries[0].BusWalk));
        _building.SetBusStopToho2(Helpers.Common.ReplaceZenkakuNumbers(TransportationEntries[1].BusWalk));
        _building.SetBusStopToho3(Helpers.Common.ReplaceZenkakuNumbers(TransportationEntries[2].BusWalk));
        _building.SetBusStopToho4(Helpers.Common.ReplaceZenkakuNumbers(TransportationEntries[3].BusWalk));

        // facilities
        _building.HasPassengerElevator = HasPassengerElevator;
        _building.PassengerElevatorCount = string.IsNullOrWhiteSpace(PassengerElevatorCount) ? null : ParseInteger(PassengerElevatorCount);
        _building.HasFreightElevator = HasFreightElevator;
        _building.FreightElevatorDetails = FreightElevatorDetails;
        _building.HasLoadingArea = HasLoadingArea;
        _building.LoadingAreaDetails = LoadingAreaDetails;
        _building.HasSecurity = HasSecurity;
        _building.HasMechanicalSecurity = HasMechanicalSecurity;
        _building.HasStaffedSecurity = HasStaffedSecurity;
        _building.HasOtherSecurity = HasOtherSecurity;
        _building.SecurityDetails = SecurityDetails;
        ApplyRemainingFacilities();
        _building.HasElevator = HasElevator;
        _building.HasAutolock = HasAutolock;
        _building.HasSecurityCamera = HasSecurityCamera;
        _building.HasParcelLocker = HasParcelLocker;

        _building.KanriShutai = SelectedKanriShutai!.Key;
        _building.KanriIsMaintenanceManagementIfIsKanriJisya = KanriIsMaintenanceManagementIfIsKanriJisya;
        _building.KanriIsTenantManagementIfIsKanriJisya = KanriIsTenantManagementIfIsKanriJisya;
        _building.KanriIsPaymentManagementIfIsKanriJisya = KanriIsPaymentManagementIfIsKanriJisya;
        _building.KanriIsOtherManagementIfIsKanriJisya = KanriIsOtherManagementIfIsKanriJisya;
        _building.KanriRemarkIfIsKanriJisya = KanriRemarkIfIsKanriJisya;
        _building.KanriNameOfCompanyIfIsKanriTasya = KanriNameOfCompanyIfIsKanriTasya;
        _building.KanriContactInfoOfCompanyIfIsKanriTasya = KanriContactInfoOfCompanyIfIsKanriTasya;
        _building.KanriRemarkIfIsKanriTasya = KanriRemarkIfIsKanriTasya;
        _building.KanriRemarkIfIsKanriKashinushi = KanriRemarkIfIsKanriKashinushi;

        // Reset main ThumbnailImageFilePath here.
        _building.ThumbnailFilename = string.Empty;
        // Base dir for the thumbnail
        _building.BasePath = System.IO.Path.Combine(App.PropertyBlobDataFolder, _building.Id);

        // 写真
        _building.Pictures = Pictures;
        var thumbImg = Pictures.FirstOrDefault(i => i.IsMain == true);
        if (thumbImg is not null)
        {
            // 物件写真サムネイルに指定
            _building.ThumbnailFilename = thumbImg.ImageFilename;
        }

        _building.Pdfs = Pdfs;
        if (string.IsNullOrWhiteSpace(_building.ThumbnailFilename))
        {
            var thumbPdf = Pdfs.FirstOrDefault(i => i.IsMain);
            if (thumbPdf is not null)
            {
                if (!IsUnitOwnership)
                {
                    // 物件写真サムネイルに指定
                    _building.ThumbnailFilename = thumbPdf.ThumbnailFilename;
                }
            }
        }

        // Lessors
        //_building.Lessors = [.. LessorsWrapper.Select(wrapper => wrapper.Person)];
        _building.Lessors.Clear();
        foreach (var item in this.LessorsWrapper)
        {
            _building.Lessors.Add(item.Person);
        }

        // Brokers
        //_building.Brokers = [.. BrokersWrapper.Select(wrapper => wrapper.Person)];
        _building.Brokers.Clear();
        foreach (var item in this.BrokersWrapper)
        {
            _building.Brokers.Add(item.Person);
        }

        _building.ListingIds.Clear();
        foreach (var summary in Units) _building.ListingIds.Add(summary.Id);

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
                Debug.WriteLine($"Failed to delete media file '{filePath}': {ex}");
                failures.Add(filePath);
                succeeded = false;
            }
        }

        return succeeded;
    }

    private void CleanupListingDirectories(List<string> failures)
    {
        foreach (var listingId in _pendingListingDirectoryCleanup.ToList())
        {
            var directory = Path.Combine(_propertyDataDirectoryPath, listingId);
            try
            {
                if (Directory.Exists(directory)) Directory.Delete(directory, true);
                _pendingListingDirectoryCleanup.Remove(listingId);
            }
            catch (Exception ex) when (ex is IOException or UnauthorizedAccessException)
            {
                Debug.WriteLine($"Failed to delete commercial listing directory '{directory}': {ex}");
                failures.Add(directory);
            }
        }
    }

    public bool CleanupIncompleteImports()
    {
        var referencedFiles = Pictures.Select(picture => Path.Combine(_propertyDataDirectoryPath, picture.ImageFilename))
            .Concat(Pdfs.Concat(_building.PdfsToBeDeleted).SelectMany(pdf => new[] { Path.Combine(_propertyDataDirectoryPath, pdf.PdfFilename), Path.Combine(_propertyDataDirectoryPath, pdf.ThumbnailFilename) }))
            .ToHashSet(StringComparer.OrdinalIgnoreCase);
        var failures = new List<string>();
        foreach (var tracked in new[] { _unsavedBuildingPictureFileList, _unsavedBuildingPdfFileList, _unsavedBuildingPdfThumbnailFileList })
        {
            foreach (var file in tracked.Where(file => !referencedFiles.Contains(file)).ToList())
            {
                if (TryDeleteFiles(new[] { file }, failures)) tracked.Remove(file);
            }
        }

        if (failures.Count == 0) return true;
        InfoBarErrorMessage = $"未保存のファイルを削除できませんでした。ファイルを閉じてから再試行してください: {string.Join(", ", failures)}";
        IsInfoBarErrorOpen = true;
        return false;
    }

    private static int ParseInteger(string value)
    {
        if (TryParseNonNegativeInteger(value, out var result))
        {
            return result;
        }

        throw new FormatException("The value must be a non-negative integer.");
    }

    private static decimal ParseDecimal(string value)
    {
        if (TryParseNonNegativeDecimal(value, out var result))
        {
            return result;
        }

        throw new FormatException("The value must be a non-negative number.");
    }

    private bool ValidateBasicNumericFields()
    {
        if (!TryParseNonNegativeInteger(FloorCountAboveGround, out _))
        {
            return ShowBasicNumericValidationError("地上階数には0以上の整数を入力してください。");
        }

        if (!TryParseNonNegativeInteger(FloorCountBasement, out _))
        {
            return ShowBasicNumericValidationError("地下階数には0以上の整数を入力してください。");
        }

        if (!TryParseNonNegativeDecimal(TotalFloorArea, out _))
        {
            return ShowBasicNumericValidationError("延床面積には0以上の数値を入力してください。");
        }

        return true;
    }

    private static string NormalizeNumericDigits(string value)
    {
        var characters = value.ToCharArray();
        for (var index = 0; index < characters.Length; index++)
        {
            if (characters[index] >= '０' && characters[index] <= '９')
            {
                characters[index] = (char)(characters[index] - '０' + '0');
            }
        }

        return new string(characters);
    }

    private static bool TryParseNonNegativeInteger(string value, out int result)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            result = 0;
            return true;
        }

        return int.TryParse(NormalizeNumericDigits(value), NumberStyles.Integer, CultureInfo.InvariantCulture, out result) && result >= 0;
    }

    private static bool TryParseNonNegativeDecimal(string value, out decimal result)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            result = 0;
            return true;
        }

        return decimal.TryParse(NormalizeNumericDigits(value), NumberStyles.Number, CultureInfo.CurrentCulture, out result) && result >= 0;
    }

    private bool ValidateFacilities()
    {
        if (!TryParseNonNegativeInteger(PassengerElevatorCount, out _)) return ShowFacilitiesValidationError("乗用エレベーター台数には0以上の整数を入力してください。");
        if (!TryParseNonNegativeInteger(ParkingSpaces, out _)) return ShowFacilitiesValidationError("駐車場の空き台数には0以上の整数を入力してください。");
        if (!TryParseNonNegativeDecimal(ParkingMonthlyFee, out _)) return ShowFacilitiesValidationError(IsParkingFree ? "保持する駐車場料金には0以上の数値を入力してください。修正するには無料の選択を解除してください。" : "駐車場の月額料金には0以上の数値を入力してください。");
        return true;
    }

    private bool ShowFacilitiesValidationError(string message)
    {
        InfoBarErrorMessage = message;
        IsInfoBarErrorOpen = true;
        const string pageName = "ZumenSearch.Views.Rent.Commercials.FacilitiesPage";
        if (!_navigationService.IsCurrentPageSameAs(pageName)) _navigationService.NavigateTo(pageName, this);
        return false;
    }

    private bool ValidateTransportation()
    {
        for (var index = 0; index < TransportationEntries.Length; index++)
        {
            var entry = TransportationEntries[index];
            foreach (var (value, label) in new[] { (entry.StationWalk, "駅から徒歩"), (entry.BusRide, "バス乗車時間"), (entry.BusWalk, "バス停から徒歩") })
            {
                var trimmed = value.Trim();
                if (trimmed.Length > 4 || trimmed.Any(character => !(character >= '0' && character <= '9') && !(character >= '０' && character <= '９')))
                {
                    SelectedTransportationIndex = index;
                    InfoBarErrorMessage = $"交通{index + 1}の{label}には0以上の4桁以内の整数を入力してください。";
                    IsInfoBarErrorOpen = true;
                    const string pageName = "ZumenSearch.Views.Rent.Commercials.TransportationPage";
                    if (!_navigationService.IsCurrentPageSameAs(pageName)) _navigationService.NavigateTo(pageName, this);
                    return false;
                }
            }
        }
        return true;
    }

    private bool ValidateCoordinates()
    {
        var address = new Address();
        try
        {
            address.SetLocationLatitude(LocationLatitude);
        }
        catch (ArgumentException)
        {
            return ShowLocationValidationError("緯度は-90以上90以下の数値で入力してください。");
        }

        try
        {
            address.SetLocationLongitude(LocationLongitude);
        }
        catch (ArgumentException)
        {
            return ShowLocationValidationError("経度は-180以上180以下の数値で入力してください。");
        }

        return true;
    }

    private bool ShowLocationValidationError(string message)
    {
        InfoBarErrorMessage = message;
        IsInfoBarErrorOpen = true;
        const string locationPageName = "ZumenSearch.Views.Rent.Commercials.LocationPage";
        if (!_navigationService.IsCurrentPageSameAs(locationPageName))
        {
            _navigationService.NavigateTo(locationPageName, this);
        }

        return false;
    }

    private bool ShowBasicNumericValidationError(string message)
    {
        InfoBarErrorMessage = message;
        IsInfoBarErrorOpen = true;

        if (!_navigationService.IsCurrentPageSameAs(BasicPageName))
        {
            _navigationService.NavigateTo(BasicPageName, this);
        }

        return false;
    }

    private bool ValidateName()
    {
        if (string.IsNullOrWhiteSpace(Name))
        {
            InfoBarErrorMessage =
                "物件名（必須項目）が入力されていません。保存出来ませんでした。";

            IsNameHasError = true;
            return false;
        }

        if (new StringInfo(Name).LengthInTextElements > 100)
        {
            InfoBarErrorMessage =
                "物件名は100文字以内で入力してください。保存出来ませんでした。";

            IsNameHasError = true;
            return false;
        }

        IsNameHasError = false;
        return true;
    }

    #endregion

    #region == Commands ==

    #region == Save ==

    public bool IsSaving
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

    [RelayCommand(CanExecute = nameof(CanSave))]
    public async Task Save()
    {
        if (_initializationFailed)
        {
            InfoBarErrorMessage = "物件情報を読み込めなかったため保存できません。編集画面を閉じて再度お試しください。";
            IsInfoBarErrorOpen = true;
            return;
        }

        if (_discardPending)
        {
            InfoBarErrorMessage = "変更の破棄が完了していないため保存できません。もう一度画面を閉じて再試行してください。";
            IsInfoBarErrorOpen = true;
            return;
        }

        if (IsSaving || !IsDirty)
        {
            return;
        }

        if (HasPendingEditorOperations)
        {
            InfoBarErrorMessage = "実行中の選択またはファイル処理が完了してから、もう一度保存してください。";
            IsInfoBarErrorOpen = true;
            return;
        }

        var mainViewModel = App.GetService<ViewModels.MainViewModel>();
        if (mainViewModel.RentCommercialListingEditorList.Any(editor => editor.PropertyId == _building.Id && editor.ViewModel is { } vm && (vm.HasPendingEditorOperations || vm.IsClosing || vm.IsDiscardPending || vm.HasPendingFileCleanupAfterSave)))
        {
            InfoBarErrorMessage = "募集物件の選択、ファイル処理または変更の破棄が完了してから、もう一度保存してください。";
            IsInfoBarErrorOpen = true;
            return;
        }
        if (!mainViewModel.TryBeginCommercialOperation(_building.Id))
        {
            InfoBarErrorMessage = "関連する保存または削除が完了してから、もう一度保存してください。";
            IsInfoBarErrorOpen = true;
            return;
        }

        IsSaving = true;
        try
        {
            await SaveCore();
        }
        finally
        {
            IsSaving = false;
            mainViewModel.EndCommercialOperation(_building.Id);
            while (_deferredMessages.TryDequeue(out var message))
            {
                message();
            }
        }
    }

    private async Task SaveCore()
    {
        var isNewBuilding = _building.Status == EntityStatus.New;

        // Validate input.
        if (!ValidateName())
        {
            IsInfoBarErrorOpen = true;

            if (!_navigationService.IsCurrentPageSameAs(BasicPageName))
            {
                _navigationService.NavigateTo(BasicPageName, this);
            }

            return;
        }

        if (!ValidateBasicNumericFields() || !ValidateCoordinates() || !ValidateTransportation() || !ValidateFacilities())
        {
            return;
        }

        if (Pictures.Count(picture => picture.IsMain) > 1)
        {
            InfoBarErrorMessage = "メインの写真は1枚だけ選択してください。";
            IsInfoBarErrorOpen = true;
            const string pageName = "ZumenSearch.Views.Rent.Commercials.PictureListPage";
            if (!_navigationService.IsCurrentPageSameAs(pageName)) _navigationService.NavigateTo(pageName, this);
            return;
        }

        if (Pdfs.Count(pdf => pdf.IsMain) > 1)
        {
            InfoBarErrorMessage = "メインの図面は1枚だけ選択してください。";
            IsInfoBarErrorOpen = true;
            const string pageName = "ZumenSearch.Views.Rent.Commercials.ZumenListPage";
            if (!_navigationService.IsCurrentPageSameAs(pageName)) _navigationService.NavigateTo(pageName, this);
            return;
        }

        if (SelectedKanriShutai is null)
        {
            InfoBarErrorMessage = "建物管理主体を選択してください。";
            IsInfoBarErrorOpen = true;
            const string pageName = "ZumenSearch.Views.Rent.Commercials.KanriPage";
            if (!_navigationService.IsCurrentPageSameAs(pageName)) _navigationService.NavigateTo(pageName, this);
            return;
        }

        if (!CleanupIncompleteImports())
        {
            return;
        }

        SetValues();

        var picturesToDelete = _building.PicturesToBeDeleted.ToList();
        var pdfsToDelete = _building.PdfsToBeDeleted.ToList();
        var stagedPictures = _unsavedBuildingPictureFileList.ToList();
        var stagedPdfs = _unsavedBuildingPdfFileList.ToList();
        var stagedThumbnails = _unsavedBuildingPdfThumbnailFileList.ToList();
        bool saveResult;
        bool? expectedOwnership = isNewBuilding ? null : _committedUnitOwnership;

        var pendingListings = isNewBuilding ? _pendingListingDrafts.Values.ToList() : [];
        var deletedListingIds = _stagedListingDeletions.ToArray();
        if (App.GetService<ViewModels.MainViewModel>().RentCommercialListingEditorList.Any(editor => deletedListingIds.Contains(editor.Id, StringComparer.Ordinal)))
        {
            InfoBarErrorMessage = "削除予定の募集物件の編集画面を閉じてから保存してください。";
            IsInfoBarErrorOpen = true;
            return;
        }
        var resInsert = await Task.Run(() => _dataAccessService.UpsertRentCommercial(_building, pendingListings, deletedListingIds, expectedOwnership), _cts.Token);
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
            Debug.WriteLine("No errors on update.");
            saveResult = true;
        }

        if (saveResult)
        {
            _building.SetStatus(EntityStatus.Saved);
            ReconcileOwnershipScopedDraftsAfterSave();
            IsDirty = false;
            _committedUnitOwnership = IsUnitOwnership;
            _committedBuildingPdfs.Clear();
            _committedBuildingPdfs.AddRange(Pdfs);

            _building.SetStatus(EntityStatus.Saved);
            _building.SetIsModified(false);
            _building.Address.SetIsModified(false);
            _addressHierarchyChanged = false;

            // Update title with dummy value.
            WindowTitle = string.Empty;

            var cleanupFailures = new List<string>();
            IsInfoBarErrorOpen = false;
            foreach (var listingId in deletedListingIds)
            {
                App.GetService<ViewModels.MainViewModel>().RemoveCommercialListingSearchResults(_building.Id, listingId);
                _pendingListingDirectoryCleanup.Add(listingId);
                _stagedListingDeletions.Remove(listingId);
                WeakReferenceMessenger.Default.Send(new ListingDeletedMessage(listingId));
            }
            foreach (var listingId in _stagedPendingListingDeletions) _pendingListingDirectoryCleanup.Add(listingId);
            _stagedPendingListingDeletions.Clear();
            foreach (var picture in picturesToDelete)
            {
                var picturePath = Path.Combine(_propertyDataDirectoryPath, picture.ImageFilename);
                if (TryDeleteFiles(new[] { picturePath }, cleanupFailures))
                {
                    _building.PicturesToBeDeleted.Remove(picture);
                }
            }

            foreach (var pdf in pdfsToDelete)
            {
                var pdfPath = Path.Combine(_propertyDataDirectoryPath, pdf.PdfFilename);
                var thumbnailPath = Path.Combine(_propertyDataDirectoryPath, pdf.ThumbnailFilename);
                if (TryDeleteFiles(new[] { pdfPath, thumbnailPath }, cleanupFailures))
                {
                    _building.PdfsToBeDeleted.Remove(pdf);
                }
            }

            foreach (var draft in pendingListings)
            {
                if (_supersededDraftFiles.Remove(draft.Id, out var superseded)) _pendingListingFileCleanup.UnionWith(superseded);
                var removedPicturePaths = draft.PicturesToBeDeleted.Select(picture => Path.Combine(_propertyDataDirectoryPath, draft.Id, picture.ImageFilename));
                var removedPdfPaths = draft.PdfsToBeDeleted.SelectMany(pdf => new[]
                {
                    Path.Combine(_propertyDataDirectoryPath, draft.Id, pdf.PdfFilename),
                    Path.Combine(_propertyDataDirectoryPath, draft.Id, pdf.ThumbnailFilename)
                });
                _pendingListingFileCleanup.UnionWith(removedPicturePaths.Concat(removedPdfPaths));
            }
            var childEditors = App.GetService<ViewModels.MainViewModel>().RentCommercialListingEditorList.Where(editor => editor.PropertyId == _building.Id).ToList();
            foreach (var file in _pendingListingFileCleanup.ToList())
            {
                if (childEditors.Any(editor => editor.ViewModel?.ReferencesAttachmentFile(file) == true)) continue;
                if (TryDeleteFiles(new[] { file }, cleanupFailures)) _pendingListingFileCleanup.Remove(file);
            }
            CleanupListingDirectories(cleanupFailures);

            foreach (var file in stagedPictures) _unsavedBuildingPictureFileList.Remove(file);
            foreach (var file in stagedPdfs) _unsavedBuildingPdfFileList.Remove(file);
            foreach (var file in stagedThumbnails) _unsavedBuildingPdfThumbnailFileList.Remove(file);

            _hasPendingFileCleanupAfterSave = cleanupFailures.Count > 0;
            if (_hasPendingFileCleanupAfterSave)
            {
                IsDirty = true;
                InfoBarErrorMessage = $"保存しましたが、一部の削除済みファイルを削除できませんでした。次回の保存時に再試行します: {string.Join(", ", cleanupFailures)}";
                IsInfoBarErrorOpen = true;
            }
            else if (_pendingListingFileCleanup.Count > 0)
            {
                IsDirty = true;
                InfoBarErrorMessage = "編集中の募集物件で使用中の添付ファイルがあります。編集画面を保存または閉じてから、建物を再度保存してください。";
                IsInfoBarErrorOpen = true;
            }

            _building.SetStatus(EntityStatus.Saved);
            _building.SetIsModified(false);
            WeakReferenceMessenger.Default.Send(new PropertyUpdatedMessage(_building));
        }
    }
    private bool CanSave()
    {
        return IsDirty && !IsSaving && !_isClosing && !_propertyBusy && !_initializationFailed && !_discardPending;
    }

    #endregion

    #region == Units ==

    [RelayCommand]
    private void AddNewUnit()
    {
        if (!IsEditorEnabled) return;

        var unit = new Models.Rent.Commercials.Listing.Listing(
            Guid.CreateVersion7().ToString("N"),
            EntityStatus.New,
            _building.Id,
            _building.Status,
            IsUnitOwnership,
            Name);

        var shellFactory = App.GetService<
            Services.Extensions.AbstractFactory.IAbstractFactory<
                Models.Rent.Commercials.Listing.Listing,
                Views.Rent.Commercials.Listing.ShellPage>>();

        var shell = shellFactory.Create(unit);
        shell.ViewModel.SetOwnershipVisibility(IsUnitOwnership);

        var mainVM = App.GetService<ViewModels.MainViewModel>();
        mainVM.RentCommercialListingEditorList.Add(shell.Window);

        this.UnsavedChildEditorList.Add(shell.Window);

        //Units.Add(unit);
        //IsDirty = true;

        if (shell.Window.AppWindow.Presenter is Microsoft.UI.Windowing.OverlappedPresenter presenter)
        {
            presenter.IsResizable = true;
            presenter.IsModal = false;
            presenter.IsAlwaysOnTop = false;
            presenter.PreferredMinimumWidth = 1000;
            presenter.PreferredMinimumHeight = 700;
        }

        shell.Window.AppWindow.MoveAndResize(new Windows.Graphics.RectInt32(mainVM.RentCommercialListingEditorWinLeft, mainVM.RentCommercialListingEditorWinTop, mainVM.RentCommercialListingEditorWinWidth, mainVM.RentCommercialListingEditorWinHeight));

        shell.Window.Activate();
        shell.Window.AppWindow.MoveInZOrderAtTop();
    }

    [RelayCommand(CanExecute = nameof(EditSelectedUnitCanExecute))]
    private async Task EditSelectedUnit(Models.Rent.Commercials.ListingSummary summary)
    {
        if (summary is null || !Units.Contains(summary) || !IsEditorEnabled) return;

        var mainViewModel = App.GetService<ViewModels.MainViewModel>();
        if (!mainViewModel.TryBeginCommercialOperation(_building.Id))
        {
            InfoBarErrorMessage = "関連する保存、削除または読み込みが完了してから、もう一度操作してください。";
            IsInfoBarErrorOpen = true;
            return;
        }

        try
        {
            await EditSelectedUnitCore(summary);
        }
        finally
        {
            mainViewModel.EndCommercialOperation(_building.Id);
        }
    }

    private async Task EditSelectedUnitCore(Models.Rent.Commercials.ListingSummary summary)
    {
        var unitId = summary.Id;
        var mainVM = App.GetService<ViewModels.MainViewModel>();
        foreach (var editWin in mainVM.RentCommercialListingEditorList.Where(editor => editor.Id == unitId).ToList())
        {
            try
            {
                editWin.Activate();
                editWin.AppWindow.MoveInZOrderAtTop();
                return;
            }
            catch (COMException)
            {
                InfoBarErrorMessage = "募集物件の編集画面を開けませんでした。既存の編集画面を確認してから再試行してください。";
                IsInfoBarErrorOpen = true;
                return;
            }
        }
        var pendingDraft = GetPendingListingDraft(unitId);
        Models.Rent.Commercials.Listing.Listing? unit;
        if (pendingDraft is not null)
        {
            if (!_pendingListingMedia.TryGetValue(unitId, out var media))
            {
                InfoBarErrorMessage = "募集物件の添付ファイルを復元できません。建物を保存してから再度開いてください。";
                IsInfoBarErrorOpen = true;
                return;
            }
            unit = pendingDraft.ToEditorModel(media.Pictures, media.Pdfs);
            foreach (var lessorId in pendingDraft.LessorIds.Concat(pendingDraft.LessorsToBeDeleted).Distinct(StringComparer.Ordinal))
            {
                var lessor = await Task.Run(() => _dataAccessService.SelectRentLessorById(lessorId), _cts.Token);
                if (lessor.IsError || lessor.Person is null)
                {
                    InfoBarErrorMessage = $"貸主 {lessorId} を募集物件の下書きから読み込めませんでした。データを確認してから再試行してください。";
                    IsInfoBarErrorOpen = true;
                    return;
                }
                if (pendingDraft.LessorIds.Contains(lessorId, StringComparer.Ordinal)) unit.Lessors.Add(lessor.Person);
                else unit.LessorsToBeDeleted.Add(lessor.Person);
            }
            foreach (var brokerId in pendingDraft.BrokerIds.Concat(pendingDraft.BrokersToBeDeleted).Distinct(StringComparer.Ordinal))
            {
                var broker = await Task.Run(() => _dataAccessService.SelectBrokerById(brokerId), _cts.Token);
                if (broker.IsError || broker.Person is null)
                {
                    InfoBarErrorMessage = $"宅建業者 {brokerId} を募集物件の下書きから読み込めませんでした。データを確認してから再試行してください。";
                    IsInfoBarErrorOpen = true;
                    return;
                }
                if (pendingDraft.BrokerIds.Contains(brokerId, StringComparer.Ordinal)) unit.Brokers.Add(broker.Person);
                else unit.BrokersToBeDeleted.Add(broker.Person);
            }
        }
        else
        {
            var result = await Task.Run(() => _dataAccessService.SelectRentCommercialListingById(_building.Id, unitId), _cts.Token);
            if (result.IsError || result.Unit is null)
            {
                InfoBarErrorMessage = result.IsError ? result.Error.Message : "募集物件を読み込めませんでした。";
                IsInfoBarErrorOpen = true;
                return;
            }
            unit = result.Unit;
        }

        var rentId = unit.PropertyId;

        if (string.IsNullOrEmpty(rentId))//if (selected == null)
        {
            Debug.WriteLine("EditSelectedUnitCommand executed but no rentId.");
            return;
        }

        //var editorShell = _shellFactory.Create(unit);
        var shellFactory = App.GetService<Services.Extensions.AbstractFactory.IAbstractFactory<Models.Rent.Commercials.Listing.Listing, Views.Rent.Commercials.Listing.ShellPage>>();
        var editorShell = shellFactory.Create(unit);
        editorShell.ViewModel.SetOwnershipVisibility(IsUnitOwnership);

        var editorWindow = editorShell.Window;
        if (editorWindow == null)
        {
            // EditorWin should be initialized in the EditorShell constructor.
            Debug.WriteLine("EditorWin must be initialized in the EditorShell constructor");
            return;
        }

        mainVM.RentCommercialListingEditorList.Add(editorWindow);
        if (_building.Status == EntityStatus.New) UnsavedChildEditorList.Add(editorWindow);

        editorWindow.AppWindow.MoveAndResize(new Windows.Graphics.RectInt32(mainVM.RentCommercialListingEditorWinLeft, mainVM.RentCommercialListingEditorWinTop, mainVM.RentCommercialListingEditorWinWidth, mainVM.RentCommercialListingEditorWinHeight));
        if (editorWindow.AppWindow.Presenter is OverlappedPresenter presenter)
        {
            presenter.IsResizable = true;
            presenter.IsModal = false;
            presenter.IsAlwaysOnTop = false;
            presenter.PreferredMinimumWidth = 1274;
            presenter.PreferredMinimumHeight = 794;
        }

        editorWindow.AppWindow.Show();
        editorWindow.Activate();

        if (editorWindow.Content is Views.Rent.Commercials.Listing.ShellPage shell)
        {
            shell.Window?.AppWindow.MoveInZOrderBelow(editorWindow.AppWindow.Id);
        }
        editorWindow.AppWindow.MoveInZOrderAtTop();

    }
    public static bool EditSelectedUnitCanExecute(Models.Rent.Commercials.ListingSummary summary) => summary is not null;

    [RelayCommand(CanExecute = nameof(DupeSelectedUnitCanExecute))]
    private void DupeSelectedUnit(Models.Rent.Commercials.ListingSummary summary)
    {
        if (summary is null) return;
    }

    public static bool DupeSelectedUnitCanExecute(Models.Rent.Commercials.ListingSummary summary) => summary is not null;

    [RelayCommand(CanExecute = nameof(DeleteSelectedUnitCanExecute))]
    private async Task DeleteSelectedUnit(Models.Rent.Commercials.ListingSummary summary)
    {
        if (summary is null || !Units.Contains(summary) || !IsEditorEnabled) return;

        var mainVM = App.GetService<ViewModels.MainViewModel>();
        if (!mainVM.TryBeginCommercialOperation(_building.Id))
        {
            InfoBarErrorMessage = "関連する保存、削除または読み込みが完了してから、もう一度操作してください。";
            IsInfoBarErrorOpen = true;
            return;
        }

        try
        {
            if (mainVM.RentCommercialListingEditorList.FirstOrDefault(editor => editor.Id == summary.Id) is { } editor)
            {
                InfoBarErrorMessage = "募集物件の編集画面を閉じてから削除してください。";
                IsInfoBarErrorOpen = true;
                editor.Activate();
                return;
            }

            if (await _dialogService.ShowDeleteConfirmationDialog("この募集物件を削除しますか？") != Microsoft.UI.Xaml.Controls.ContentDialogResult.Primary) return;
            if (mainVM.RentCommercialListingEditorList.FirstOrDefault(editor => editor.Id == summary.Id) is { } openedEditor)
            {
                InfoBarErrorMessage = "募集物件の編集画面を閉じてから削除してください。";
                IsInfoBarErrorOpen = true;
                openedEditor.Activate();
                return;
            }

            if (summary.IsPendingDraft) _stagedPendingListingDeletions.Add(summary.Id);
            else _stagedListingDeletions.Add(summary.Id);
            RemoveListingSummary(summary.Id);
            IsDirty = true;
        }
        catch (Exception ex) when (ex is COMException or InvalidOperationException)
        {
            Debug.WriteLine($"Failed to confirm commercial listing deletion: {ex}");
            InfoBarErrorMessage = $"募集物件の削除を確認できませんでした: {ex.Message}";
            IsInfoBarErrorOpen = true;
        }
        finally
        {
            mainVM.EndCommercialOperation(_building.Id);
        }
    }
    public static bool DeleteSelectedUnitCanExecute(Models.Rent.Commercials.ListingSummary summary) => summary is not null;

    #endregion

    #region == Location commands ==

    [RelayCommand(CanExecute = nameof(CanShowGoogleMaps))]
    public async Task ShowGoogleMaps()
    {
        if (string.IsNullOrWhiteSpace(LocationLatitude) ||
            string.IsNullOrWhiteSpace(LocationLongitude))
        {
            return;
        }

        if (!ValidateCoordinates())
        {
            return;
        }

        var address = new Address();
        address.SetLocationLatitude(LocationLatitude);
        address.SetLocationLongitude(LocationLongitude);
        var uriGoogleMaps = new Uri($"https://maps.google.co.jp/?q={address.LocationLatitude},{address.LocationLongitude}");
        if (!await Windows.System.Launcher.LaunchUriAsync(uriGoogleMaps))
        {
            InfoBarErrorMessage = "地図を開けませんでした。";
            IsInfoBarErrorOpen = true;
        }
    }

    private bool CanShowGoogleMaps()
    {
        return !string.IsNullOrWhiteSpace(LocationLatitude) &&
               !string.IsNullOrWhiteSpace(LocationLongitude);
    }

    #endregion

    #region == Transportation commands ==

    [RelayCommand]
    private async Task ShowRailLineSelect()
    {
        var entry = SelectedTransportation;
        var selected = await _dialogService.ShowRailLineSelectDialog();
        if (selected is not null && !string.Equals(entry.RailLine.LineCode, selected.LineCode, StringComparison.Ordinal))
        {
            entry.RailLine = selected;
        }
    }

    [RelayCommand(CanExecute = nameof(CanShowRailStationSelect))]
    private async Task ShowRailStationSelect()
    {
        var entry = SelectedTransportation;
        var lineCode = entry.RailLine.LineCode;
        if (string.IsNullOrWhiteSpace(lineCode)) return;
        var selected = await _dialogService.ShowRailStationSelectDialog(lineCode);
        if (selected is not null && string.Equals(entry.RailLine.LineCode, lineCode, StringComparison.Ordinal) && string.Equals(selected.LineCode, lineCode, StringComparison.Ordinal))
        {
            entry.RailStation = selected;
        }
    }

    private bool CanShowRailStationSelect()
    {
        return !string.IsNullOrWhiteSpace(SelectedTransportation.RailLine.LineCode);
    }

    #endregion

    #region == Pictures and Pdfs related commands ==

    [RelayCommand(CanExecute = nameof(CanAddNewBuildingPictures))]
    private async Task AddNewBuildingPictures(List<string> filePathList)
    {
        if (filePathList is null || filePathList.Count == 0)
        {
            return;
        }

        Directory.CreateDirectory(_propertyDataDirectoryPath);

        foreach (var filePath in filePathList)
        {
            if (string.IsNullOrWhiteSpace(filePath))
            {
                continue;
            }

            var id = Guid.CreateVersion7().ToString("N");
            var extension = Path.GetExtension(filePath);
            if (!IsSupportedPictureExtension(extension))
            {
                InfoBarErrorMessage = $"画像形式に対応していません: {Path.GetFileName(filePath)}";
                IsInfoBarErrorOpen = true;
                continue;
            }

            var filename = id + extension;
            var destination = Path.Combine(_propertyDataDirectoryPath, filename);

            await using (var source = File.OpenRead(filePath))
            await using (var target = new FileStream(destination, FileMode.CreateNew, FileAccess.Write))
            {
                _unsavedBuildingPictureFileList.Add(destination);
                IsDirty = true;
                await source.CopyToAsync(target);
            }

            var picture = new Models.Rent.Commercials.PropertyPicture(id, filename, EntityStatus.New)
            {
                BasePath = _propertyDataDirectoryPath,
                ParentViewModel = this
            };

            TrackBuildingPicture(picture);
            Pictures.Add(picture);


            OpenBuildingBlobDirectoryCommand.NotifyCanExecuteChanged();
            DeleteBuildingPictureCommand.NotifyCanExecuteChanged();

            IsDirty = true;
        }
    }
    private static bool CanAddNewBuildingPictures(List<string> filePathList)
    {
        return filePathList is { Count: > 0 };
    }

    private static bool IsSupportedPictureExtension(string extension) =>
        extension.Equals(".jpg", StringComparison.OrdinalIgnoreCase) ||
        extension.Equals(".jpeg", StringComparison.OrdinalIgnoreCase) ||
        extension.Equals(".png", StringComparison.OrdinalIgnoreCase) ||
        extension.Equals(".gif", StringComparison.OrdinalIgnoreCase) ||
        extension.Equals(".webp", StringComparison.OrdinalIgnoreCase);

    [RelayCommand]
    private async Task OpenBuildingPicture(Models.Rent.Commercials.PropertyPicture picture)
    {
        if (picture is null || !Pictures.Contains(picture) || !IsEditorEnabled) return;

        try
        {
            var file = await StorageFile.GetFileFromPathAsync(Path.Combine(_propertyDataDirectoryPath, picture.ImageFilename));
            if (!await Windows.System.Launcher.LaunchFileAsync(file))
            {
                InfoBarErrorMessage = "写真を開けませんでした。画像を開くアプリが設定されているか確認してください。";
                IsInfoBarErrorOpen = true;
            }
        }
        catch (Exception ex) when (ex is IOException or UnauthorizedAccessException or COMException or InvalidOperationException or ArgumentException)
        {
            Debug.WriteLine($"Failed to open commercial property picture: {ex}");
            InfoBarErrorMessage = $"写真を開けませんでした: {ex.Message}";
            IsInfoBarErrorOpen = true;
        }
    }

    [RelayCommand(CanExecute = nameof(CanDeleteBuildingPicture))]
    private async Task DeleteBuildingPicture(Models.Rent.Commercials.PropertyPicture picture)
    {
        if (picture is null || !Pictures.Contains(picture) || !IsEditorEnabled) return;

        try
        {
            if (await _dialogService.ShowDeleteConfirmationDialog("この建物写真を削除しますか？") != Microsoft.UI.Xaml.Controls.ContentDialogResult.Primary) return;
            if (!Pictures.Remove(picture)) return;

            picture.PropertyChanged -= OnBuildingMediaPropertyChanged;
            picture.ParentViewModel = null;
            _building.PicturesToBeDeleted.Add(picture);
            IsDirty = true;
        }
        catch (Exception ex) when (ex is COMException or InvalidOperationException)
        {
            Debug.WriteLine($"Failed to confirm commercial property picture deletion: {ex}");
            InfoBarErrorMessage = $"写真の削除を確認できませんでした: {ex.Message}";
            IsInfoBarErrorOpen = true;
        }
    }
    private static bool CanDeleteBuildingPicture(Models.Rent.Commercials.PropertyPicture picture)
    {
        return picture is not null;
    }

    [RelayCommand(CanExecute = nameof(CanAddNewBuildingPdfs))]
    private async Task AddNewBuildingPdfs(List<string> filePathList)
    {
        if (filePathList is null || filePathList.Count == 0 || IsUnitOwnership) return;

        Directory.CreateDirectory(_propertyDataDirectoryPath);

        foreach (var filePath in filePathList)
        {
            if (string.IsNullOrWhiteSpace(filePath)) continue;

            var extension = Path.GetExtension(filePath);
            if (!extension.Equals(".pdf", StringComparison.OrdinalIgnoreCase))
            {
                InfoBarErrorMessage = $"PDFファイルを選択してください: {Path.GetFileName(filePath)}";
                IsInfoBarErrorOpen = true;
                continue;
            }

            var file = await StorageFile.GetFileFromPathAsync(filePath);
            var document = await PdfDocument.LoadFromFileAsync(file);
            if (document.PageCount == 0)
            {
                InfoBarErrorMessage = $"PDFにページがありません: {Path.GetFileName(filePath)}";
                IsInfoBarErrorOpen = true;
                continue;
            }

            using PdfPage page = document.GetPage(0);
            using var stream = new InMemoryRandomAccessStream();
            await page.RenderToStreamAsync(stream, new PdfPageRenderOptions { DestinationWidth = 512 });

            var id = Guid.CreateVersion7().ToString("N");
            var thumbnailFilename = id + ".bmp";
            var thumbnailPath = Path.Combine(_propertyDataDirectoryPath, thumbnailFilename);
            var filename = id + extension;
            var destinationPath = Path.Combine(_propertyDataDirectoryPath, filename);
            using (var destination = new FileStream(thumbnailPath, FileMode.CreateNew, FileAccess.Write))
            {
                _unsavedBuildingPdfThumbnailFileList.Add(thumbnailPath);
                IsDirty = true;
                using var source = stream.AsStreamForRead();
                await source.CopyToAsync(destination);
            }

            await using (var source = File.OpenRead(filePath))
            await using (var destination = new FileStream(destinationPath, FileMode.CreateNew, FileAccess.Write))
            {
                _unsavedBuildingPdfFileList.Add(destinationPath);
                await source.CopyToAsync(destination);
            }

            var pdf = new Models.Rent.Commercials.PropertyPdf(id, filename, thumbnailFilename, EntityStatus.New)
            {
                BasePath = _propertyDataDirectoryPath,
                ParentViewModel = this
            };

            TrackBuildingPdf(pdf);
            Pdfs.Add(pdf);
            OpenBuildingBlobDirectoryCommand.NotifyCanExecuteChanged();
            DeleteBuildingPdfCommand.NotifyCanExecuteChanged();
            IsDirty = true;
        }
    }
    private static bool CanAddNewBuildingPdfs(List<string> filePathList)
    {
        return filePathList is { Count: > 0 };
    }

    [RelayCommand]
    private async Task OpenBuildingPdf(Models.Rent.Commercials.PropertyPdf pdf)
    {
        if (pdf is null || !Pdfs.Contains(pdf) || !IsEditorEnabled || IsUnitOwnership) return;

        try
        {
            var file = await StorageFile.GetFileFromPathAsync(Path.Combine(_propertyDataDirectoryPath, pdf.PdfFilename));
            if (!await Windows.System.Launcher.LaunchFileAsync(file))
            {
                InfoBarErrorMessage = "PDFを開けませんでした。PDFを開くアプリが設定されているか確認してください。";
                IsInfoBarErrorOpen = true;
            }
        }
        catch (Exception ex) when (ex is IOException or UnauthorizedAccessException or COMException or InvalidOperationException or ArgumentException)
        {
            Debug.WriteLine($"Failed to open commercial property PDF: {ex}");
            InfoBarErrorMessage = $"PDFを開けませんでした: {ex.Message}";
            IsInfoBarErrorOpen = true;
        }
    }

    [RelayCommand(CanExecute = nameof(CanDeleteBuildingPdf))]
    private async Task DeleteBuildingPdf(Models.Rent.Commercials.PropertyPdf pdf)
    {
        if (pdf is null || !Pdfs.Contains(pdf) || !IsEditorEnabled || IsUnitOwnership) return;

        try
        {
            if (await _dialogService.ShowDeleteConfirmationDialog("この建物図面を削除しますか？") != Microsoft.UI.Xaml.Controls.ContentDialogResult.Primary) return;
            if (!Pdfs.Remove(pdf)) return;

            pdf.PropertyChanged -= OnBuildingMediaPropertyChanged;
            pdf.ParentViewModel = null;
            _building.PdfsToBeDeleted.Add(pdf);
            IsDirty = true;
        }
        catch (Exception ex) when (ex is COMException or InvalidOperationException)
        {
            Debug.WriteLine($"Failed to confirm commercial property PDF deletion: {ex}");
            InfoBarErrorMessage = $"図面の削除を確認できませんでした: {ex.Message}";
            IsInfoBarErrorOpen = true;
        }
    }
    private static bool CanDeleteBuildingPdf(Models.Rent.Commercials.PropertyPdf pdf)
    {
        return pdf is not null;
    }



    [RelayCommand(CanExecute = nameof(CanOpenBuildingBlobDirectory))]
    private void OpenBuildingBlobDirectory()
    {
        try
        {
            if (!Directory.Exists(_propertyDataDirectoryPath))
            {
                InfoBarErrorMessage = "保存場所のフォルダーが見つかりません。";
                IsInfoBarErrorOpen = true;
                return;
            }

            Process.Start("explorer.exe", _propertyDataDirectoryPath);
        }
        catch (Exception ex) when (ex is IOException or UnauthorizedAccessException or Win32Exception or InvalidOperationException)
        {
            Debug.WriteLine($"Error opening folder: {ex}");
            InfoBarErrorMessage = $"フォルダーを開けませんでした。{ex.Message}";
            IsInfoBarErrorOpen = true;
        }
    }
    private bool CanOpenBuildingBlobDirectory()
    {
        if (Directory.Exists(_propertyDataDirectoryPath))
        {
            return true;
        }

        return false;
    }

    #endregion

    #region == Lessor commands ==

    [RelayCommand]
    private async Task AddLessor()
    {
        if (!IsEditorEnabled) return;
        using var cts = new CancellationTokenSource();
        var selected = await _dialogService.ShowLessorSelectDialog(new ViewModels.Dialogs.LessorSelectViewModel(_dataAccessService, cts));
        if (selected is null || LessorsWrapper.Any(item => item.Person.Id == selected.Id)) return;

        _pendingLessorId = selected.Id;
        try
        {
            var result = await Task.Run(() => _dataAccessService.SelectRentLessorById(selected.Id), _cts.Token);
            if (result.IsError)
            {
                Debug.WriteLine(result.Error.FullDump);
                InfoBarErrorMessage = $"貸主を読み込めませんでした: {result.Error.Message}";
                IsInfoBarErrorOpen = true;
                return;
            }
            var person = _pendingLessorUpdate ?? result.Person;
            if (_pendingLessorDeleted || person is null)
            {
                InfoBarErrorMessage = "選択した貸主を読み込めませんでした。";
                IsInfoBarErrorOpen = true;
                return;
            }
            if (LessorsWrapper.Any(item => item.Person.Id == person.Id)) return;

            foreach (var removed in _building.LessorsToBeDeleted.Where(item => item.Id == person.Id).ToList()) _building.LessorsToBeDeleted.Remove(removed);
            LessorsWrapper.Add(new Models.Rent.Commercials.PersonWrapperForPropertyViewModel(person, this));
            IsDirty = true;
        }
        finally
        {
            _pendingLessorId = null;
            _pendingLessorUpdate = null;
            _pendingLessorDeleted = false;
        }
    }

    [RelayCommand]
    private void DeleteLessor(Models.Rent.Commercials.PersonWrapperForPropertyViewModel wrapper)
    {
        if (wrapper is null || !IsEditorEnabled) return;

        if (LessorsWrapper.Remove(wrapper))
        {
            if (!_building.LessorsToBeDeleted.Any(person => person.Id == wrapper.Person.Id)) _building.LessorsToBeDeleted.Add(wrapper.Person);
            IsDirty = true;
        }
    }

    [RelayCommand(CanExecute = nameof(CanEditLessor))]
    private async Task EditLessor(Models.Rent.Commercials.PersonWrapperForPropertyViewModel wrapper)
    {
        if (wrapper?.Person is null)
        {
            return;
        }

        var mainViewModel = App.GetService<ViewModels.MainViewModel>();

        await mainViewModel.EditRentLessorCommand
            .ExecuteAsync(wrapper.Person);
    }

    private static bool CanEditLessor(Models.Rent.Commercials.PersonWrapperForPropertyViewModel? wrapper)
    {
        return wrapper?.Person is not null;
    }


    #endregion

    #region == Broker commands ==

    [RelayCommand]
    private async Task AddBroker()
    {
        if (!IsEditorEnabled) return;
        using var cts = new CancellationTokenSource();
        var selected = await _dialogService.ShowBrokerSelectDialog(new ViewModels.Dialogs.BrokerSelectViewModel(_dataAccessService, cts));
        if (selected is null || BrokersWrapper.Any(item => item.Person.Id == selected.Id)) return;

        _pendingBrokerId = selected.Id;
        try
        {
            var result = await Task.Run(() => _dataAccessService.SelectBrokerById(selected.Id), _cts.Token);
            if (result.IsError)
            {
                Debug.WriteLine(result.Error.FullDump);
                InfoBarErrorMessage = $"宅建業者を読み込めませんでした: {result.Error.Message}";
                IsInfoBarErrorOpen = true;
                return;
            }
            var person = _pendingBrokerUpdate ?? result.Person;
            if (_pendingBrokerDeleted || person is null)
            {
                InfoBarErrorMessage = "選択した宅建業者を読み込めませんでした。";
                IsInfoBarErrorOpen = true;
                return;
            }
            if (BrokersWrapper.Any(item => item.Person.Id == person.Id)) return;

            foreach (var removed in _building.BrokersToBeDeleted.Where(item => item.Id == person.Id).ToList()) _building.BrokersToBeDeleted.Remove(removed);
            BrokersWrapper.Add(new Models.Rent.Commercials.PersonWrapperForPropertyViewModel(person, this));
            IsDirty = true;
        }
        finally
        {
            _pendingBrokerId = null;
            _pendingBrokerUpdate = null;
            _pendingBrokerDeleted = false;
        }
    }

    [RelayCommand(CanExecute = nameof(CanEditBroker))]
    private async Task EditBroker(Models.Rent.Commercials.PersonWrapperForPropertyViewModel wrapper)
    {
        if (wrapper?.Person is null)
        {
            return;
        }

        var mainViewModel = App.GetService<ViewModels.MainViewModel>();

        await mainViewModel.EditBrokerCommand
            .ExecuteAsync(wrapper.Person);
    }

    private static bool CanEditBroker(Models.Rent.Commercials.PersonWrapperForPropertyViewModel? wrapper)
    {
        return wrapper?.Person is not null;
    }

    [RelayCommand(CanExecute = nameof(CanDeleteBroker))]
    private void DeleteBroker(Models.Rent.Commercials.PersonWrapperForPropertyViewModel wrapper)
    {
        if (wrapper?.Person is not { } person || !IsEditorEnabled) return;

        if (BrokersWrapper.Remove(wrapper))
        {
            if (!_building.BrokersToBeDeleted.Any(removed => removed.Id == person.Id)) _building.BrokersToBeDeleted.Add(person);
            IsDirty = true;
        }
    }

    private static bool CanDeleteBroker(Models.Rent.Commercials.PersonWrapperForPropertyViewModel? wrapper)
    {
        return wrapper?.Person is not null;
    }

    #endregion

    #endregion

}