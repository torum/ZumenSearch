using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.Messaging;
using Microsoft.UI.Windowing;
using System.Collections.ObjectModel;
using System.Globalization;
using System.Runtime.InteropServices;
using Windows.Data.Pdf;
using Windows.Storage;
using Windows.Storage.Streams;
using ZumenSearch.Models.Base;
using ZumenSearch.Models.Common;
using ZumenSearch.Models.Messenger;
using ZumenSearch.Services.Contracts;

namespace ZumenSearch.ViewModels.Rent.Commercials;

public sealed partial class PropertyViewModel : ObservableRecipient, 
    IRecipient<WindowClosedMessage>,
    IRecipient<LessorUpdatedMessage>,
    IRecipient<LessorDeletedMessage>,
    IRecipient<BrokerUpdatedMessage>,
    IRecipient<BrokerDeletedMessage>
{
    private const string BasicPageName = "ZumenSearch.Views.Rent.Commercials.BasicPage";

    public readonly List<Views.Rent.Commercials.Listing.EditorWindow> ChildEditorList = [];

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
        IsActive = true;
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

            title = _building.Status == EnumEntryStatus.New
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

    public ObservableCollection<Models.Rent.Commercials.Kind> Kinds { get; } =
    [
        new(Models.Rent.Commercials.EnumCommercialKinds.Office),
        new(Models.Rent.Commercials.EnumCommercialKinds.Retail),
        new(Models.Rent.Commercials.EnumCommercialKinds.Warehouse),
        new(Models.Rent.Commercials.EnumCommercialKinds.Factory),
        new(Models.Rent.Commercials.EnumCommercialKinds.Clinic),
        new(Models.Rent.Commercials.EnumCommercialKinds.Restaurant),
        new(Models.Rent.Commercials.EnumCommercialKinds.Hotel),
        new(Models.Rent.Commercials.EnumCommercialKinds.Land),
        new(Models.Rent.Commercials.EnumCommercialKinds.Other)
    ];

    public Models.Rent.Commercials.Kind SelectedKind
    {
        get => field ??
            new(Models.Rent.Commercials.EnumCommercialKinds.Unspecified);
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

    public ObservableCollection<Models.Rent.Commercials.Structure> Structures { get; } =
    [
        new(Models.Rent.Commercials.EnumStructures.Wood),
        new(Models.Rent.Commercials.EnumStructures.Block),
        new(Models.Rent.Commercials.EnumStructures.LightSteel),
        new(Models.Rent.Commercials.EnumStructures.Steel),
        new(Models.Rent.Commercials.EnumStructures.RC),
        new(Models.Rent.Commercials.EnumStructures.SRC),
        new(Models.Rent.Commercials.EnumStructures.ALC),
        new(Models.Rent.Commercials.EnumStructures.PC),
        new(Models.Rent.Commercials.EnumStructures.HPC),
        new(Models.Rent.Commercials.EnumStructures.Other)
    ];

    public Models.Rent.Commercials.Structure SelectedStructure
    {
        get => field ??
            new(Models.Rent.Commercials.EnumStructures.Unspecified);
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
    new(new PrefectureList().Prefectures);

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
    } = false;

    public ObservableCollection<Models.Rent.Commercials.ElectricKind> ElectricKinds =
    [
        new Models.Rent.Commercials.ElectricKind(Models.Rent.Commercials.Property.EnumElectricKind.AllElectric, "オール電化"),
        new Models.Rent.Commercials.ElectricKind(Models.Rent.Commercials.Property.EnumElectricKind.Unspecified, "未指定")
    ];

    public Models.Rent.Commercials.ElectricKind SelectedElectricKind
    {
        get;
        set
        {
            if (SetProperty(ref field, value))
            {
                IsDirty = true;
            }
        }
    } = new Models.Rent.Commercials.ElectricKind(Models.Rent.Commercials.Property.EnumElectricKind.Unspecified, "未指定");

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

    public ObservableCollection<Models.Rent.Commercials.KanriShutai> KanriShutais =
    [
        new Models.Rent.Commercials.KanriShutai(Models.Rent.Commercials.Property.EnumKanriShutai.Unspecified, "未指定"),
        new Models.Rent.Commercials.KanriShutai(Models.Rent.Commercials.Property.EnumKanriShutai.Jisya, "自社管理"),
        new Models.Rent.Commercials.KanriShutai(Models.Rent.Commercials.Property.EnumKanriShutai.Tasya, "他社管理"),
        new Models.Rent.Commercials.KanriShutai(Models.Rent.Commercials.Property.EnumKanriShutai.Kashinushi, "貸主管理")
    ];

    public Models.Rent.Commercials.KanriShutai? SelectedKanriShutai
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

    public ObservableCollection<Models.Rent.Commercials.Listing.Listing> Units { get; } = [];


    #endregion

    #region == 写真 & PDFプロパティ ==

    public ObservableCollection<Models.Rent.Commercials.Picture> Pictures
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

    public ObservableCollection<Models.Rent.Commercials.Pdf> Pdfs
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
            this.ChildEditorList.Remove(rcwin);
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

        if (wrapper is not null)
        {
            wrapper.Person = lessor;
        }
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

        if (wrapper is not null)
        {
            wrapper.Person = broker;
        }
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
        PopulateValues();
        IsDirty = false;
        IsInfoBarErrorOpen = false;
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
                Models.Rent.Commercials.EnumCommercialKinds.Unspecified);

        SelectedStructure =
            Structures.FirstOrDefault(
                item => item.Key == _building.BuildingStructure.Key)
            ?? new(
                Models.Rent.Commercials.EnumStructures.Unspecified);

        IsUnitOwnership = _building.IsUnitOwnership;

        FloorCountAboveGround =
            _building.FloorCountAboveGround == 0
                ? string.Empty
                : _building.FloorCountAboveGround.ToString();

        FloorCountBasement =
            _building.FloorCountBasement == 0
                ? string.Empty
                : _building.FloorCountBasement.ToString();

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

        /*
        // Pictures:
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
    }

    private void SetValues()
    {
        _building.Name = Name;
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


        _building.RailLine1 = SelectedRailLine1;
        _building.RailStation1 = SelectedRailStation1;
        _building.EkiToho1 = EkiToho1;
        _building.BusStop1 = BusStop;
        _building.BusJyousya1 = BusJyousya1;
        _building.BusStopToho1 = BusStopToho1;


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


        _building.Lessors = [.. LessorsWrapper.Select(wrapper => wrapper.Person)];

        _building.Brokers = [.. BrokersWrapper.Select(wrapper => wrapper.Person)];

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
    public void Save()
    {
        if (!IsDirty)
        {
            return;
        }

        if (!ValidateName())
        {
            if (IsNameHasError)
            {
                IsInfoBarErrorOpen = true;
                _navigationService.NavigateTo(BasicPageName, this);
            }

            return;
        }

        SetValues();

        var result = _dataAccessService.UpsertRentCommercial(_building);

        if (result.IsError)
        {
            Debug.WriteLine(
                result.Error.Title + Environment.NewLine +
                result.Error.Message + Environment.NewLine +
                result.Error.Description + Environment.NewLine +
                result.Error.Operation + Environment.NewLine +
                result.Error.MethodName + Environment.NewLine +
                result.Error.FullDump);

            InfoBarErrorMessage =
                result.Error.Title + Environment.NewLine +
                result.Error.Message + Environment.NewLine +
                result.Error.Description + Environment.NewLine +
                result.Error.Operation + Environment.NewLine +
                result.Error.MethodName;

            IsInfoBarErrorOpen = true;
            return;
        }

        IsDirty = false;
        _building.IsModified = false;
        _building.Status = EnumEntryStatus.Saved;
        IsInfoBarErrorOpen = false;
        WindowTitle = string.Empty;
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
            EnumEntryStatus.New,
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

        this.ChildEditorList.Add(shell.Window);

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
                // 既に閉じられたウィンドウをリストから除去
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

        this.ChildEditorList.Add(editorWindow);

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
                // 既に閉じられたウィンドウをリストから除去
                //mainVM.RoomEditorList.Remove(editWin);
                //ChildEditorList.Remove(editWin);
            }
        }


        // TODO: show dialog to comfirm.

        if (Units.Remove(unit))
        {
            // No. Don't
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

            Pictures.Add(new Models.Rent.Commercials.Picture(id, filename)
            {
                BasePath = _propertyDataDirectoryPath,
                IsNew = true,
                ParentViewModel = this
            });

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
    private void DeleteBuildingPicture(Models.Rent.Commercials.Picture picture)
    {
        if (picture is null || !Pictures.Remove(picture))
        {
            return;
        }

        _building.PicturesToBeDeleted.Add(picture);
        IsDirty = true;
    }
    private static bool CanDeleteBuildingPicture(Models.Rent.Commercials.Picture picture)
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
            if (!extension.Equals(".pdf")) // TODO: check case.
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

                var pdf = new Models.Rent.Commercials.Pdf(newId, newFilename, newThumbnailFilename)
                {
                    BasePath = _propertyDataDirectoryPath,//System.IO.Path.Combine(App.PropertyBlobDataFolder, _building.Id),
                    IsNew = true,
                    ParentViewModel = this
                };

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
    private void DeleteBuildingPdf(Models.Rent.Commercials.Pdf pdfBldg)
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
    private static bool CanDeleteBuildingPdf(Models.Rent.Commercials.Pdf pdfBldg)
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
    private async Task DeleteBroker(
        Models.Rent.Commercials.PersonWrapperForPropertyViewModel wrapper)
    {
        if (wrapper?.Person is null)
        {
            return;
        }

        var mainViewModel = App.GetService<ViewModels.MainViewModel>();

        await mainViewModel.DeleteBrokerCommand
            .ExecuteAsync(wrapper.Person);

        if (BrokersWrapper.Remove(wrapper))
        {
            _building.Brokers.Remove(wrapper.Person);
            _building.BrokersToBeDeleted.Add(wrapper.Person);
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
}