using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.Messaging;
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
using ZumenSearch.Models.Enums;
using ZumenSearch.Models.Location;
using ZumenSearch.Models.Messenger;
using ZumenSearch.Models.Transportation;
using ZumenSearch.Services.Contracts;

namespace ZumenSearch.ViewModels.Rent.Commercials;

public sealed partial class PropertyViewModel : ObservableRecipient, 
    IRecipient<WindowClosedMessage>,
    IRecipient<LessorUpdatedMessage>,
    IRecipient<LessorDeletedMessage>,
    IRecipient<BrokerUpdatedMessage>,
    IRecipient<BrokerDeletedMessage>,
    IRecipient<ListingUpdatedMessage>,
    IDisposable
{
    private const string BasicPageName = "ZumenSearch.Views.Rent.Commercials.BasicPage";
    private readonly CancellationTokenSource _cts = new();
    // Keeps truck of "New" child window because it saves to the parent window/viewmodel.
    public List<Views.Rent.Commercials.Listing.EditorWindow> UnsavedChildEditorList { get; } = [];
    // This property holds the COPY of current entity being edited.
    // Do not use it directly in the UI. Apply changes to this object in Save() to save the changes.
    private readonly Models.Rent.Commercials.Property _building;
    private readonly string _propertyDataDirectoryPath;

    private readonly List<string> _unsavedBuildingPictureFileList = [];
    private readonly List<string> _unsavedBuildingPdfFileList = [];
    private readonly List<string> _unsavedBuildingPdfThumbnailFileList = [];

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
        _navigationService = navigationService;
        _dialogService = dialogService;
        _dispatcherService = dispatcherService;
        _dataAccessService = dataAccessService;
        _dataAccessLocationService = dataAccessLocationService;

        _propertyDataDirectoryPath = Path.Combine(App.PropertyBlobDataFolder, building.Id);

        // Update title with dummy value.
        WindowTitle = string.Empty;
        try
        {
            PopulateValues();

            // Reset errors
            IsNameHasError = false;
            // TODO: more.

            //HasErrors = false;
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"PropertyViewModel: {ex}");
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
                    Messenger.Register<WindowClosedMessage>(this);
                    Messenger.Register<LessorUpdatedMessage>(this);
                    Messenger.Register<LessorDeletedMessage>(this);
                    Messenger.Register<BrokerUpdatedMessage>(this);
                    Messenger.Register<BrokerDeletedMessage>(this);
                    Messenger.Register<ListingUpdatedMessage>(this);
                }
                else
                {
                    Messenger.Unregister<WindowClosedMessage>(this);
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
        new(Models.Rent.Commercials.PropertyType.Office),
        new(Models.Rent.Commercials.PropertyType.Retail),
        new(Models.Rent.Commercials.PropertyType.Warehouse),
        new(Models.Rent.Commercials.PropertyType.Factory),
        new(Models.Rent.Commercials.PropertyType.Clinic),
        new(Models.Rent.Commercials.PropertyType.Restaurant),
        new(Models.Rent.Commercials.PropertyType.Hotel),
        new(Models.Rent.Commercials.PropertyType.Land),
        new(Models.Rent.Commercials.PropertyType.Other)
    ];

    public Models.Rent.Commercials.PropertyTypeLabel SelectedKind
    {
        get => field ??
            new(Models.Rent.Commercials.PropertyType.Unspecified);
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
            }
        }
    }

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
    new(new PrefectureMaster().Prefectures);

    public Prefecture? SelectedPef
    {
        get;
        set
        {
            if (SetProperty(ref field, value))
            {
                IsDirty = true;
                Cities = field is null
                    ? []
                    : [.. _dataAccessLocationService
                    .GetCountyAndCityByPref(field.Name)
                    .DistinctBy(item => item.Combined)];

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
            if (SetProperty(ref field, value))
            {
                IsDirty = true;
                Towns = field is null || SelectedPef is null
                    ? []
                    : [.. _dataAccessLocationService
                    .GetWardAndOazaByPrefCountyCity(
                        SelectedPef.Name,
                        field.County,
                        field.City)
                    .DistinctBy(item => item.Combined)];

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
            if (SetProperty(ref field, value))
            {
                IsDirty = true;
                Chous = field is null || SelectedPef is null || SelectedCity is null
                    ? []
                    : [.. _dataAccessLocationService
                    .GetChoumeByPrefCountyCityWardOaza(
                        SelectedPef.Name,
                        SelectedCity.County,
                        SelectedCity.City,
                        field.Ward,
                        field.Oaza)
                    .DistinctBy(item => item.Chou)];

                OnPropertyChanged(nameof(AddressPreview));
            }
        }
    }

    public ObservableCollection<Choume> Chous { get; private set; } = [];

    public Choume? SelectedChou
    {
        get;
        set
        {
            if (SetProperty(ref field, value))
            {
                IsDirty = true;
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

    public RailLine? SelectedRailLine1
    {
        get;
        set
        {
            if (SetProperty(ref field, value))
            {
                IsDirty = true;
                SelectedRailStation1 = null;
                ShowRailStationSelect1Command.NotifyCanExecuteChanged();
            }
        }
    }

    public RailStation? SelectedRailStation1
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

    public string EkiToho1
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

    public string BusStop
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

    public string BusJyousya1
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

    public string BusStopToho1
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

    #region == 設備プロパティ ==

    #region == 一般 ==

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

                if (field is null)
                {
                    Debug.WriteLine("SelectedKanriShutai is null");
                    return;
                }

                // Toggle Visibilities
                IsKanriUnspecified = field.Key == Models.Rent.Commercials.Property.EnumKanriShutai.Unspecified;
                IsKanriJisya = field.Key == Models.Rent.Commercials.Property.EnumKanriShutai.Jisya;
                IsKanriTasya = field.Key == Models.Rent.Commercials.Property.EnumKanriShutai.Tasya;
                IsKanriKashinushi = field.Key == Models.Rent.Commercials.Property.EnumKanriShutai.Kashinushi;
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

    public ObservableCollection<Models.Rent.Commercials.Listing.Listing> Units
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

    public void Receive(WindowClosedMessage window)
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
        var something = listing.Value;
        if (something is null)
        {
            return;
        }

        if (something is not Models.Rent.Commercials.Listing.Listing unit)
        {
            return;
        }

        var existingUnit = this.Units.FirstOrDefault(r => r.Id.Equals(unit.Id, StringComparison.Ordinal));
        if (existingUnit is not null)
        {
            // Update existing unit
            var index = this.Units.IndexOf(existingUnit);
            this.Units[index] = unit;
        }
        else
        {
            // Add new unit
            this.Units.Add(unit);
        }

        if (unit.Status == EntityStatus.New)
        {
            //Debug.WriteLine("(unit.ListingStatus == EnumListingStatus.New) @PropertyViewModel");
            IsDirty = true;
        }
        else
        {
            // Remove the child editor window for this room if it exists
            // because it is no longer new and has been saved and can be saved independently.
            var win = this.UnsavedChildEditorList.FirstOrDefault(w => w is Views.Rent.Commercials.Listing.EditorWindow ew && ew.Id == unit.Id);
            if (win is not null)
            {
                this.UnsavedChildEditorList.Remove(win);
            }
        }
    }

    public void Receive(LessorUpdatedMessage message)
    {
        var lessor = message.Value;

        if (lessor is null)
        {
            return;
        }

        var wrapper = LessorsWrapper
            .FirstOrDefault(item => item.Person.Id == lessor.Id);

        wrapper?.Person = lessor;
    }

    public void Receive(LessorDeletedMessage message)
    {
        var lessorId = message.Value;

        if (string.IsNullOrWhiteSpace(lessorId))
        {
            return;
        }

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
        var broker = message.Value;

        if (broker is null)
        {
            return;
        }

        var wrapper = BrokersWrapper
            .FirstOrDefault(item => item.Person.Id == broker.Id);

        wrapper?.Person = broker;
    }

    public void Receive(BrokerDeletedMessage message)
    {
        var brokerId = message.Value;

        if (string.IsNullOrWhiteSpace(brokerId))
        {
            return;
        }

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

    public void DiscardChanges()
    {
        DiscardUnsavedFiles();

        _building.PicturesToBeDeleted.Clear();
        _building.PdfsToBeDeleted.Clear();

        IsDirty = false;
        IsInfoBarErrorOpen = false;
    }

    private void DiscardUnsavedFiles()
    {
        if (_unsavedBuildingPictureFileList.Count > 0)
        {
            foreach (var file in _unsavedBuildingPictureFileList)
            {
                if (File.Exists(file))
                {
                    Debug.WriteLine($"Deleting unsaved picture file: {file}");
                    File.Delete(file);
                }
            }
            _unsavedBuildingPictureFileList.Clear();
        }

        if (_unsavedBuildingPdfThumbnailFileList.Count > 0)
        {
            foreach (var file in _unsavedBuildingPdfThumbnailFileList)
            {
                if (File.Exists(file))
                {
                    Debug.WriteLine($"Deleting unsaved PDF Thumbnail file: {file}");
                    File.Delete(file);
                }
            }
            _unsavedBuildingPdfThumbnailFileList.Clear();
        }

        if (_unsavedBuildingPdfFileList.Count > 0)
        {
            foreach (var file in _unsavedBuildingPdfFileList)
            {
                if (File.Exists(file))
                {
                    Debug.WriteLine($"Deleting unsaved PDF file: {file}");
                    File.Delete(file);
                }
            }
            _unsavedBuildingPdfFileList.Clear();
        }

        if (_building.Status == EntityStatus.New)
        {
            if (Directory.Exists(_propertyDataDirectoryPath))
            {
                Debug.WriteLine($"Deleting folder: {_propertyDataDirectoryPath}");
                Directory.Delete(_propertyDataDirectoryPath, true);
            }
        }
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
                Models.Rent.Commercials.PropertyType.Unspecified);

        SelectedStructure =
            Structures.FirstOrDefault(
                item => item.Key == _building.BuildingStructure.Key)
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

        PopulateLocationValues();

        SelectedRailLine1 = _building.RailLine1;
        SelectedRailStation1 = _building.RailStation1;
        EkiToho1 = _building.EkiToho1;
        BusStop = _building.BusStop1;
        BusJyousya1 = _building.BusJyousya1;
        BusStopToho1 = _building.BusStopToho1;


        HasElevator = _building.HasElevator;
        HasAutolock = _building.HasAutolock;
        HasSecurityCamera = _building.HasSecurityCamera;
        HasParcelLocker = _building.HasParcelLocker;

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

        Units = new ObservableCollection<Models.Rent.Commercials.Listing.Listing>(_building.Units); // create a copy.

        foreach (var item in Units)
        {
            //
            item.IsModified = false; // Needed this.
            //item.PropertyChanged += OnRoomPropertyChanged;
        }
    }

    private void PopulateLocationValues()
    {
        SelectedChou = null;
        SelectedTown = null;
        SelectedCity = null;
        SelectedPef = null;

        Cities = [];
        Towns = [];
        Chous = [];

        var prefecture = Prefectures.FirstOrDefault(item =>
            item.MunicipalityCode == _building.LocPrefId ||
            item.Name == _building.LocPrefecture);

        if (prefecture is not null)
        {
            SelectedPef = prefecture;
        }

        var city = Cities.FirstOrDefault(item =>
            item.County == _building.LocCounty &&
            item.City == _building.LocCity);

        if (city is not null)
        {
            SelectedCity = city;
        }

        var town = Towns.FirstOrDefault(item =>
            item.Ward == _building.LocWard &&
            item.Oaza == _building.LocOazaCho);

        if (town is not null)
        {
            SelectedTown = town;
        }

        var choume = Chous.FirstOrDefault(item =>
            item.Chou == _building.LocChoume);

        if (choume is not null)
        {
            SelectedChou = choume;
        }

        Edaban = _building.LocEdaban;

        LocationLatitude = _building.LocationLatitude;
        LocationLongitude = _building.LocationLongitude;
    }

    private void TrackBuildingPicture(Models.Rent.Commercials.PropertyPicture picture)
    {
        picture.BasePath = _propertyDataDirectoryPath;
        picture.ParentViewModel = this;
        picture.PropertyChanged -= OnBuildingMediaPropertyChanged;
        picture.PropertyChanged += OnBuildingMediaPropertyChanged;
        picture.IsModified = false;
    }

    private void TrackBuildingPdf(Models.Rent.Commercials.PropertyPdf pdf)
    {
        pdf.BasePath = _propertyDataDirectoryPath;
        pdf.ParentViewModel = this;
        pdf.PropertyChanged -= OnBuildingMediaPropertyChanged;
        pdf.PropertyChanged += OnBuildingMediaPropertyChanged;
        pdf.IsModified = false;
    }

    private void OnBuildingMediaPropertyChanged(object? sender, PropertyChangedEventArgs e)
    {
        if (sender is Models.Rent.Commercials.PropertyPicture picture)
        {
            picture.IsModified = true;
        }
        else if (sender is Models.Rent.Commercials.PropertyPdf pdf)
        {
            pdf.IsModified = true;
        }

        IsDirty = true;
    }

    private void SetValues()
    {
        _building.SetName(Name);
        _building.CommercialKind = SelectedKind;
        _building.BuildingStructure = SelectedStructure;
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

        // location
        _building.LocPrefId = SelectedPef?.MunicipalityCode ?? string.Empty;
        _building.LocPrefecture = SelectedPef?.Name ?? string.Empty;
        _building.LocMachiazaId =
            SelectedChou?.MachiazaId
            ?? SelectedTown?.MachiazaId
            ?? SelectedCity?.MachiazaId
            ?? string.Empty;
        _building.LocCounty = SelectedCity?.County ?? string.Empty;
        _building.LocCity = SelectedCity?.City ?? string.Empty;
        _building.LocWard = SelectedTown?.Ward ?? string.Empty;
        _building.LocOazaCho = SelectedTown?.Oaza ?? string.Empty;
        _building.LocChoume = SelectedChou?.Chou ?? string.Empty;
        _building.LocEdaban = Edaban;
        _building.LocLocationFull = AddressPreview;
        _building.LocationLatitude = LocationLatitude;
        _building.LocationLongitude = LocationLongitude;

        // transportation
        _building.RailLine1 = SelectedRailLine1;
        _building.RailStation1 = SelectedRailStation1;
        _building.EkiToho1 = EkiToho1;
        _building.BusStop1 = BusStop;
        _building.BusJyousya1 = BusJyousya1;
        _building.BusStopToho1 = BusStopToho1;

        // facilities
        _building.HasElevator = HasElevator;
        _building.HasAutolock = HasAutolock;
        _building.HasSecurityCamera = HasSecurityCamera;
        _building.HasParcelLocker = HasParcelLocker;

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
            var thumbPdf = Pdfs.FirstOrDefault(i => i.IsMain == true);
            if (thumbPdf is not null)
            {
                // 物件写真サムネイルに指定
                _building.ThumbnailFilename = thumbPdf.ThumbnailFilename;
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

        //
        foreach (var unit in Units)
        {
            // TODO: check if this is needed.Commercial property units should have the same ownership type as the building.
            unit.IsPropertyUnitOwnership = IsUnitOwnership;
        }
        _building.Units = Units;

    }

    private static void DeleteFilesSafely(IEnumerable<string> filePaths)
    {
        foreach (var filePath in filePaths)
        {
            if (string.IsNullOrWhiteSpace(filePath))
            {
                continue;
            }

            try
            {
                if (File.Exists(filePath))
                {
                    File.Delete(filePath);
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Failed to delete media file '{filePath}': {ex}");
            }
        }
    }

    private static int ParseInteger(string value)
    {
        var normalized =
            Helpers.Common.ReplaceZenkakuNumbers(value);

        return int.TryParse(
            normalized,
            NumberStyles.Integer,
            CultureInfo.InvariantCulture,
            out var result) && result >= 0
            ? result
            : 0;
    }

    private static decimal ParseDecimal(string value)
    {
        var normalized =
            Helpers.Common.ReplaceZenkakuNumbers(value);

        return decimal.TryParse(
            normalized,
            NumberStyles.Number,
            CultureInfo.InvariantCulture,
            out var result) && result >= 0
            ? result
            : 0;
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

    [RelayCommand(CanExecute = nameof(CanSave))]
    public async Task Save()
    {
        if (!IsDirty)
        {
            return;
        }

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

        SetValues();

        bool saveResult;

        var resInsert = await Task.Run(() => _dataAccessService.UpsertRentCommercial(_building), _cts.Token);
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
            IsDirty = false;

            _building.IsModified = false;
            _building.Status = EntityStatus.Saved;

            // Update title with dummy value.
            WindowTitle = string.Empty;

            // Clear error infobar.
            IsInfoBarErrorOpen = false;

            var filesToDelete = _building.PicturesToBeDeleted
                .Select(picture =>
                    Path.Combine(_propertyDataDirectoryPath, picture.ImageFilename))
                .Concat(
                    _building.PdfsToBeDeleted.SelectMany(pdf =>
                        new[]
                        {
                            Path.Combine(_propertyDataDirectoryPath, pdf.PdfFilename),
                            Path.Combine(_propertyDataDirectoryPath, pdf.ThumbnailFilename)
                        }))
                .ToArray();

            DeleteFilesSafely(filesToDelete);

            _building.PicturesToBeDeleted.Clear();
            _building.PdfsToBeDeleted.Clear();
            _building.BrokersToBeDeleted.Clear();
            _building.LessorsToBeDeleted.Clear();
            _building.UnitsToBeDeleted.Clear();

            _unsavedBuildingPictureFileList.Clear();
            _unsavedBuildingPdfThumbnailFileList.Clear();
            _unsavedBuildingPdfFileList.Clear();

            // Just in case.
            _building.Status = EntityStatus.Saved;
            _building.IsModified = false;

            foreach (var room in Units)
            {
                room.PropertyName = Name;
                room.PropertyStatus = EntityStatus.Saved;
                room.Status = EntityStatus.Saved;
            }

            // Just in case.
            foreach (var room in _building.Units)
            {
                room.PropertyName = Name;
                room.PropertyStatus = EntityStatus.Saved;
                room.Status = EntityStatus.Saved;
            }

            WeakReferenceMessenger.Default.Send(new PropertyUpdatedMessage(_building));
        }
    }
    private bool CanSave()
    {
        return IsDirty;
    }

    #endregion

    #region == Units ==

    [RelayCommand]
    private void AddNewUnit()
    {
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
    private void EditSelectedUnit(Models.Rent.Commercials.Listing.Listing unit)
    {
        if (unit is null) return;

        var rentId = unit.PropertyId;
        var unitId = unit.Id;

        if (string.IsNullOrEmpty(rentId))//if (selected == null)
        {
            Debug.WriteLine("EditSelectedUnitCommand executed but no rentId.");
            return;
        }

        //Debug.WriteLine($"EditRentResidentialCommand executed for {selected.Id}");

        var mainVM = App.GetService<ViewModels.MainViewModel>();

        // Check if the selected item is already being edited in another window.
        foreach (var editWin in mainVM.RentCommercialListingEditorList.ToList())
        {
            if (editWin.Id != unitId)
            {
                continue;
            }

            //Debug.WriteLine($"Editor window for {unitId} is already open. Activating it.");

            try
            {
                editWin.Activate();

                if (editWin.Content is Views.Rent.Commercials.Listing.ShellPage editShell)
                {
                    editShell.Window?.AppWindow.MoveInZOrderBelow(editWin.AppWindow.Id);
                }

                editWin.AppWindow.MoveInZOrderAtTop();

                //isFound = true;

                return;
            }
            catch (COMException)
            {
                // 
                //mainVM.RoomEditorList.Remove(editWin);
                //ChildEditorList.Remove(editWin);
            }
        }

        //var editorShell = _shellFactory.Create(unit);
        var shellFactory = App.GetService<Services.Extensions.AbstractFactory.IAbstractFactory<Models.Rent.Commercials.Listing.Listing, Views.Rent.Commercials.Listing.ShellPage>>();
        var editorShell = shellFactory.Create(unit);

        // Apply the current IsUnitOwnership state because it may not be saved to the _room.
        //editorShell.ViewModel.IsPropertyUnitOwnership = this.IsUnitOwnership;

        var editorWindow = editorShell.Window;
        if (editorWindow == null)
        {
            // EditorWin should be initialized in the EditorShell constructor.
            Debug.WriteLine("EditorWin must be initialized in the EditorShell constructor");
            return;
        }

        mainVM.RentCommercialListingEditorList.Add(editorWindow);

        // Let's not add it to the ChildEditorList because it is already saved which means it's got a own persistent id and can be saved independently.
        //this.ChildEditorList.Add(editorWindow);

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
    public static bool EditSelectedUnitCanExecute(Models.Rent.Commercials.Listing.Listing unit)
    {
        if (unit is null) return false;
        return true;
    }

    [RelayCommand(CanExecute = nameof(DupeSelectedUnitCanExecute))]
    private void DupeSelectedUnit(Models.Rent.Commercials.Listing.Listing room)
    {
        if (room is null) return;

        //
    }
    public static bool DupeSelectedUnitCanExecute(Models.Rent.Commercials.Listing.Listing Unit)
    {
        if (Unit is null) return false;
        return true;
    }

    [RelayCommand(CanExecute = nameof(DeleteSelectedUnitCanExecute))]
    private void DeleteSelectedUnit(Models.Rent.Commercials.Listing.Listing unit)
    {
        if (unit is null)
        {
            return;
        }

        // 
        var mainVM = App.GetService<ViewModels.MainViewModel>();

        // Check if the selected item is already being edited in another window.
        foreach (var editWin in mainVM.RentCommercialListingEditorList.ToList())
        {
            if (editWin.Id != unit.Id)
            {
                continue;
            }

            //Debug.WriteLine($"Editor window for {unitId} is already open. Activating it.");

            try
            {
                editWin.Activate();

                if (editWin.Content is Views.Rent.Commercials.Listing.ShellPage editShell)
                {
                    editShell.Window?.AppWindow.MoveInZOrderBelow(editWin.AppWindow.Id);
                }

                editWin.AppWindow.MoveInZOrderAtTop();

                //isFound = true;

                return;
            }
            catch (COMException)
            {
                // 
                //mainVM.RoomEditorList.Remove(editWin);
                //ChildEditorList.Remove(editWin);
            }
        }


        // TODO: show dialog to comfirm.

        if (Units.Remove(unit))
        {
            // No. Don't delete directly. Just mark it for deletion. It will be deleted when the property is saved.
            //if (_building.Rooms.Remove(room)) { }
            _building.UnitsToBeDeleted.Add(unit);
            IsDirty = true;
        }
    }
    public static bool DeleteSelectedUnitCanExecute(Models.Rent.Commercials.Listing.Listing unit)
    {
        if (unit is null) return false;
        return true;
    }

    #endregion

    #region == Location commands ==

    [RelayCommand(CanExecute = nameof(CanShowGoogleMaps))]
    public async Task ShowGoogleMaps()
    {
        if (string.IsNullOrEmpty(LocationLatitude) ||
            string.IsNullOrEmpty(LocationLongitude))
        {
            return;
        }

        var uriGoogleMaps = new Uri(
            $"https://maps.google.co.jp/?q={LocationLatitude},{LocationLongitude}");

        await Windows.System.Launcher.LaunchUriAsync(uriGoogleMaps);
    }

    private bool CanShowGoogleMaps()
    {
        return !string.IsNullOrEmpty(LocationLatitude) &&
               !string.IsNullOrEmpty(LocationLongitude);
    }

    #endregion

    #region == Transportation commands ==

    [RelayCommand]
    private async Task ShowRailLineSelect1()
    {
        var selected = await _dialogService.ShowRailLineSelectDialog();

        if (selected is not null)
        {
            SelectedRailLine1 = selected;
        }
    }

    [RelayCommand(CanExecute = nameof(CanShowRailStationSelect1))]
    private async Task ShowRailStationSelect1()
    {
        if (SelectedRailLine1 is null)
        {
            return;
        }

        var selected = await _dialogService
            .ShowRailStationSelectDialog(SelectedRailLine1.LineCode);

        if (selected is not null)
        {
            SelectedRailStation1 = selected;
        }
    }

    private bool CanShowRailStationSelect1()
    {
        return SelectedRailLine1 is not null;
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
            var filename = id + extension;
            var destination = Path.Combine(_propertyDataDirectoryPath, filename);

            await using var source = File.OpenRead(filePath);
            await using var target = File.Create(destination);
            await source.CopyToAsync(target);

            /*
            Pictures.Add(new Models.Rent.Commercials.Picture(id, filename)
            {
                BasePath = _propertyDataDirectoryPath,
                IsNew = true,
                ParentViewModel = this
            });
            */
            var picture = new Models.Rent.Commercials.PropertyPicture(id, filename, EntityStatus.New)
            {
                BasePath = _propertyDataDirectoryPath,
                ParentViewModel = this
            };

            TrackBuildingPicture(picture);
            Pictures.Add(picture);


            OpenBuildingBlobDirectoryCommand.NotifyCanExecuteChanged();
            DeleteBuildingPictureCommand.NotifyCanExecuteChanged();

            _unsavedBuildingPictureFileList.Add(destination);
            IsDirty = true;
        }
    }
    private static bool CanAddNewBuildingPictures(List<string> filePathList)
    {
        return filePathList is { Count: > 0 };
    }

    [RelayCommand(CanExecute = nameof(CanDeleteBuildingPicture))]
    private void DeleteBuildingPicture(Models.Rent.Commercials.PropertyPicture picture)
    {
        if (picture is null || !Pictures.Remove(picture))
        {
            return;
        }

        _building.PicturesToBeDeleted.Add(picture);
        IsDirty = true;
    }
    private static bool CanDeleteBuildingPicture(Models.Rent.Commercials.PropertyPicture picture)
    {
        return picture is not null;
    }

    [RelayCommand(CanExecute = nameof(CanAddNewBuildingPdfs))]
    private async Task AddNewBuildingPdfs(List<string> filePathList)
    {
        if (filePathList is null) return;
        if (filePathList.Count == 0) return;

        Debug.WriteLine($"destDirectory={_propertyDataDirectoryPath}  @AddNewBuildingPdfs()");

        if (!Directory.Exists(_propertyDataDirectoryPath))
        {
            Directory.CreateDirectory(_propertyDataDirectoryPath);
        }

        //List<string> list = [];

        foreach (var filePath in filePathList)
        {
            if (string.IsNullOrEmpty(filePath.Trim()))
            {
                continue;
            }

            // TODO: check file ext for valid image type.
            // TODO: set max file size?


            string extension = Path.GetExtension(System.IO.Path.GetFileName(filePath));
            if (!extension.Equals(".pdf", StringComparison.OrdinalIgnoreCase)) // TODO: check case.
            {
                continue;
            }

            //using var sourceStream = File.Open(file, FileMode.Open);

            StorageFile sfile = await StorageFile.GetFileFromPathAsync(filePath);
            PdfDocument pdfDocument = await PdfDocument.LoadFromFileAsync(sfile);

            if (pdfDocument.PageCount > 0)
            {
                using PdfPage pdfPage = pdfDocument.GetPage(0);
                using var stream = new InMemoryRandomAccessStream();

                // Set screen standard DPI
                //float targetDpi = 96f;
                //float scaleFactor = targetDpi / 72f; 
                //uint calculatedWidth = (uint)Math.Round(pdfPage.Size.Width * scaleFactor);

                var options = new PdfPageRenderOptions
                {
                    // Set the desired target width in pixels (e.g., 1024px)
                    // Aspect ratio is locked; height scales automatically.
                    DestinationWidth = 512//calculatedWidth//1024
                };

                await pdfPage.RenderToStreamAsync(stream, options);

                //var bitmapImage = new BitmapImage();
                //await bitmapImage.SetSourceAsync(stream);

                string newId = Guid.CreateVersion7().ToString("N");
                string newThumbnailFilename = newId + ".bmp";
                var thumbnailDestFilePath = Path.Combine(_propertyDataDirectoryPath, newThumbnailFilename);

                using var destinationStream = File.Create(thumbnailDestFilePath);
                using var managedSourceStream = stream.AsStreamForRead();
                await managedSourceStream.CopyToAsync(destinationStream);

                // Keep track of unsaved files to delete them when discarding.
                _unsavedBuildingPdfThumbnailFileList.Add(thumbnailDestFilePath);

                string newFilename = newId + extension;
                var pdfDestFilePath = Path.Combine(_propertyDataDirectoryPath, newFilename);
                File.Copy(filePath, pdfDestFilePath);

                // Keep track of unsaved files to delete them when discarding.
                _unsavedBuildingPdfFileList.Add(pdfDestFilePath);

                var pdf = new Models.Rent.Commercials.PropertyPdf(newId, newFilename, newThumbnailFilename, EntityStatus.New)
                {
                    BasePath = _propertyDataDirectoryPath,//System.IO.Path.Combine(App.PropertyBlobDataFolder, _building.Id),
                    ParentViewModel = this
                };

                TrackBuildingPdf(pdf);
                Pdfs.Add(pdf);

                OpenBuildingBlobDirectoryCommand.NotifyCanExecuteChanged();
                DeleteBuildingPdfCommand.NotifyCanExecuteChanged();

                IsDirty = true;
            }
            else
            {
                Debug.WriteLine("0 page.");
            }
        }
    }
    private static bool CanAddNewBuildingPdfs(List<string> filePathList)
    {
        if (filePathList.Count < 1)
        {
            return false;
        }

        return true;
    }

    [RelayCommand(CanExecute = nameof(CanDeleteBuildingPdf))]
    private void DeleteBuildingPdf(Models.Rent.Commercials.PropertyPdf pdfBldg)
    {
        if (pdfBldg is null)
        {
            return;
        }

        // TODO: show dialog to comfirm.

        if (Pdfs.Remove(pdfBldg))
        {
            _building.PdfsToBeDeleted.Add(pdfBldg);
            IsDirty = true;
        }
    }
    private static bool CanDeleteBuildingPdf(Models.Rent.Commercials.PropertyPdf pdfBldg)
    {
        return pdfBldg is not null;
    }



    [RelayCommand(CanExecute = nameof(CanOpenBuildingBlobDirectory))]
    private void OpenBuildingBlobDirectory()
    {
        if (Directory.Exists(_propertyDataDirectoryPath))
        {
            try
            {
                Process.Start("explorer.exe", _propertyDataDirectoryPath);
            }
            catch (Exception ex)
            {
                // TODO: show error to user.
                Debug.WriteLine($"Error opening folder: {ex.Message}");
            }
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
        using var cts = new CancellationTokenSource();

        var result = await _dialogService.ShowLessorSelectDialog(
            new ViewModels.Dialogs.LessorSelectViewModel(
                _dataAccessService,
                cts));

        if (result is null)
        {
            return;
        }

        if (LessorsWrapper.Any(item => item.Person.Id == result.Id))
        {
            return;
        }

        LessorsWrapper.Add(
            new Models.Rent.Commercials.PersonWrapperForPropertyViewModel(
                result,
                this));

        _building.Lessors.Add(result);
        IsDirty = true;
    }

    [RelayCommand]
    private void DeleteLessor(
    Models.Rent.Commercials.PersonWrapperForPropertyViewModel wrapper)
    {
        if (wrapper is null)
        {
            return;
        }

        if (LessorsWrapper.Remove(wrapper))
        {
            _building.Lessors.Remove(wrapper.Person);
            _building.LessorsToBeDeleted.Add(wrapper.Person);
            IsDirty = true;
        }
    }

    [RelayCommand(CanExecute = nameof(CanEditLessor))]
    private async Task EditLessor(
    Models.Rent.Commercials.PersonWrapperForPropertyViewModel wrapper)
    {
        if (wrapper?.Person is null)
        {
            return;
        }

        var mainViewModel = App.GetService<ViewModels.MainViewModel>();

        await mainViewModel.EditRentLessorCommand
            .ExecuteAsync(wrapper.Person);
    }

    private static bool CanEditLessor(
        Models.Rent.Commercials.PersonWrapperForPropertyViewModel? wrapper)
    {
        return wrapper?.Person is not null;
    }


    #endregion

    #region == Broker commands ==

    [RelayCommand]
    private async Task AddBroker()
    {
        using var cts = new CancellationTokenSource();

        var result = await _dialogService.ShowBrokerSelectDialog(
            new ViewModels.Dialogs.BrokerSelectViewModel(
                _dataAccessService,
                cts));

        if (result is null)
        {
            return;
        }

        if (BrokersWrapper.Any(item => item.Person.Id == result.Id))
        {
            return;
        }

        BrokersWrapper.Add(
            new Models.Rent.Commercials.PersonWrapperForPropertyViewModel(
                result,
                this));

        _building.Brokers.Add(result);
        IsDirty = true;
    }

    [RelayCommand(CanExecute = nameof(CanEditBroker))]
    private async Task EditBroker(
    Models.Rent.Commercials.PersonWrapperForPropertyViewModel wrapper)
    {
        if (wrapper?.Person is null)
        {
            return;
        }

        var mainViewModel = App.GetService<ViewModels.MainViewModel>();

        await mainViewModel.EditBrokerCommand
            .ExecuteAsync(wrapper.Person);
    }

    private static bool CanEditBroker(
        Models.Rent.Commercials.PersonWrapperForPropertyViewModel? wrapper)
    {
        return wrapper?.Person is not null;
    }

    [RelayCommand(CanExecute = nameof(CanDeleteBroker))]
    private async Task DeleteBroker(Models.Rent.Commercials.PersonWrapperForPropertyViewModel wrapper)
    {
        if (wrapper?.Person is not { } person)
        {
            return;
        }

        if (BrokersWrapper.Remove(wrapper))
        {
            _building.Brokers.Remove(person);
            _building.BrokersToBeDeleted.Add(person);
            IsDirty = true;
        }
    }

    private static bool CanDeleteBroker(
        Models.Rent.Commercials.PersonWrapperForPropertyViewModel? wrapper)
    {
        return wrapper?.Person is not null;
    }

    #endregion

    #endregion

    public void Dispose()
    {
        // Unsubscribe
        //WeakReferenceMessenger.Default.UnregisterAll(this);
        //or
        //this.IsActive = false;

        _cts?.Dispose();

        GC.SuppressFinalize(this);
    }
}