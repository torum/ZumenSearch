using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.Messaging;
using Microsoft.UI.Windowing;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Globalization;
using System.Runtime.InteropServices;
using System.Text.RegularExpressions;
using Windows.Data.Pdf;
using Windows.Storage;
using Windows.Storage.Streams;
using Windows.System;
using ZumenSearch.Models;
using ZumenSearch.Models.Location;
using ZumenSearch.Models.Messenger;
using ZumenSearch.Models.Rent.Residentials;
using ZumenSearch.Models.Transportation;
using ZumenSearch.Services.Contracts;
using ZumenSearch.Services.Extensions.AbstractFactory;
using ZumenSearch.Views.Services.Contracts;

namespace ZumenSearch.ViewModels.Rent.Residentials;

public sealed partial class PropertyViewModel : ObservableRecipient, IRecipient<ListingUpdatedMessage>, IRecipient<WindowClosedMessage<Microsoft.UI.Xaml.Window>>, IRecipient<ListingDeletedMessage>, IRecipient<LessorUpdatedMessage>, IRecipient<BrokerUpdatedMessage>, IRecipient<LessorDeletedMessage>, IRecipient<BrokerDeletedMessage>, IDisposable
{
    #region == Private variables and const ==

    private const string BasicPageName = "ZumenSearch.Views.Rent.Residentials.BasicPage";

    private readonly string _id = string.Empty;

    // This property holds the COPY of current entity being edited.
    // Do not use it directly in the UI. Apply changes to this object in Save() to save the changes.
    private readonly Models.Rent.Residentials.Property _building;

    private readonly string _propertyDataDirectoryPath = string.Empty;

    // Tmp file list to hold unsaved picture files. (if entry is not saved, delete on close)
    private readonly List<string> _unsavedBuildingPictureFileList = [];
    private readonly List<string> _unsavedBuildingPdfFileList = [];
    private readonly List<string> _unsavedBuildingPdfThumbnailFileList = [];

    // Tmp listing list to hold unsaved listing where listing reside here while property is not yet saved. (if entry is not saved, delete on close)
    private readonly List<Listing> _unsavedListingList = [];

    private bool _discardPending;
    private bool _isClosing;
    private bool _propertyBusy;

    private readonly Queue<Action> _deferredMessages = new();

    private readonly CancellationTokenSource _cts = new();
    private bool _disposed;

    #endregion

    #region == Services ==

    private readonly IAbstractFactory<Models.Rent.Residentials.Listing, Views.Rent.Residentials.Listing.ShellPage> _shellFactory;
    private readonly IDataAccessService _dataAccessService;
    private readonly IDataAccessLocationService _dataAccessLocationService;
    private readonly IDispatcherService _dispatcherService;
    private readonly IDialogGenericService _dialogService;
    private readonly INavigationGenericService _navigationService;

    #endregion

    public PropertyViewModel(Models.Rent.Residentials.Property building, INavigationGenericService navigationService, IDialogGenericService dialogService, IAbstractFactory<Models.Rent.Residentials.Listing, Views.Rent.Residentials.Listing.ShellPage> shellFactory, IDispatcherService dispatcherService, IDataAccessService dataAccessService, IDataAccessLocationService dataAccessLocationService)
    {
        _building = building;
        _id = building.Id;

        _navigationService = navigationService;
        _dialogService = dialogService;
        _shellFactory = shellFactory;
        _dispatcherService = dispatcherService;
        _dataAccessService = dataAccessService;
        _dataAccessLocationService = dataAccessLocationService;

        IsUnitLevelOwnership = building.IsUnitLevelOwnership;

        _transportationEntries = [new(this, 0), new(this, 1), new(this, 2), new(this, 3)];

        _propertyDataDirectoryPath = System.IO.Path.Combine(App.PropertyBlobDataFolder, _id);

        // Update title with dummy value.
        WindowTitle = string.Empty;

        try
        {
            PopulateValues();
            WeakReferenceMessenger.Default.Send(new PropertyIsUnitOwnershipChangedMessage(_id, IsUnitLevelOwnership));

            // Reset errors
            IsNameHasError = false;
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"PropertyViewModel: {ex}");
        }
        finally
        {
            IsDirty = false;
        }

        // Ready to receive messages.
#pragma warning disable IL3050 // Disable AOT dynamic code warning
#pragma warning disable IL2026 // Disable Trimming unreferenced code warning
        IsActive = true;
#pragma warning restore IL2026
#pragma warning restore IL3050
    }

    #region == Properties ==

    [ObservableProperty]
    [NotifyCanExecuteChangedFor(nameof(SaveCommand))]
    public partial bool IsDirty { get; private set; }

    // Keeps truck of "New" child window because it saves to the parent window/viewmodel.
    public List<Views.Rent.Residentials.Listing.EditorWindow> UnsavedChildEditorList { get; } = [];

    // TODO: Need to review following properties. 
    public bool IsSaving { get; private set; }
    public bool IsClosing => _isClosing;
    public bool IsPropertyBusy => _propertyBusy;
    public bool IsDiscardPending => _discardPending;
    public bool IsEditorEnabled => !IsSaving && !_isClosing && !_propertyBusy && !_discardPending;
    //public bool HasPendingEditorOperations => _pendingPickers > 0 || PendingEditorTasks().Count > 0;
    public bool HasPendingEditorOperations => IsEditorEnabled;//tmp


    #region == 画面表示関連 ==

    public string WindowTitle
    {
        get
        {
            var str = $"{field}";

            if (!string.IsNullOrEmpty(Name))
            {
                str = $"{field}：{Name}";
            }

            if (_building.Status == EntityStatus.New)
            {
                str = $"{str}：新規";
            }
            else
            {
                str = $"{str}：編集";
            }

            return str;
        }
        set
        {
            OnPropertyChanged();
        }
    } = "賃貸住居用";

    public ObservableCollection<Breadcrumb> BreadcrumbItems { get; set; } =
    [
        new() { Name = "建物", Page = typeof(Views.Rent.Residentials.BasicPage).FullName! },
        new() { Name = "基本", Page = typeof(Views.Rent.Residentials.BasicPage).FullName! }
    ];

    #endregion

    #region == エラー関連 ==

    /*
    // TODO: update this to hold more info such as page so that it can be navigated to the page.
    [ObservableProperty]
    public partial bool HasErrors { get; private set; }
    */

    // InfoBarError is researved only for unsavable error.
    [ObservableProperty]
    public partial bool IsInfoBarErrorOpen { get; set; }

    [ObservableProperty]
    public partial string InfoBarErrorMessage { get; set; } = string.Empty;

    [ObservableProperty]
    public partial bool IsNameHasError { get; private set; }

    #endregion

    #region == 基本物件プロパティ == 

    // 物件名
    public string Name
    {
        get => field ?? string.Empty; // Ensure a non-null value is returned
        set
        {
            if (SetProperty(ref field, value.Trim()))
            {
                IsDirty = true;

                ValidateName(false);

                // Update title with dummy value.
                WindowTitle = string.Empty;

                // Moved to after the save.
                //_selectedSearchResult?.Name = value; // Update the selected search result's name if it exists
            }
        }
    }

    // 物件種別
    public IReadOnlyCollection<Models.Rent.Residentials.PropertyKindTypeLabel> PropertyTypes { get; set; } = PropertyKindTypeLabel.GetAll();
    /*
    [
        //new Kind(EnumKinds.Unspecified.ToString(), "未指定"),
        new Models.Rent.Residentials.PropertyKindTypeLabel(Models.Rent.Residentials.PropertyKindType.Apartment),
        new Models.Rent.Residentials.PropertyKindTypeLabel(Models.Rent.Residentials.PropertyKindType.Mansion),
        new Models.Rent.Residentials.PropertyKindTypeLabel(Models.Rent.Residentials.PropertyKindType.House),
        new Models.Rent.Residentials.PropertyKindTypeLabel(Models.Rent.Residentials.PropertyKindType.TerraceHouse),
        new Models.Rent.Residentials.PropertyKindTypeLabel(Models.Rent.Residentials.PropertyKindType.TownHouse),
        new Models.Rent.Residentials.PropertyKindTypeLabel(Models.Rent.Residentials.PropertyKindType.ShareHouse),
        new Models.Rent.Residentials.PropertyKindTypeLabel(Models.Rent.Residentials.PropertyKindType.Dormitory)
    ];*/

    public Models.Rent.Residentials.PropertyKindTypeLabel SelectedPropertyType
    {
        get => field ?? new(Models.Rent.Residentials.PropertyKindType.Unspecified);
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

    // 区分所有か一括所有か
    public bool IsUnitLevelOwnership
    {
        get;
        set
        {
            if (SetProperty(ref field, value))
            {
                //_ownershipVersion++;
                IsDirty = true;

                IsNotUnitLevelOwnership = !value;
                if (value)
                {
                    if (_navigationService.IsCurrentPageSameAs("ZumenSearch.Views.Rent.Residentials.ZumenListPage") ||
                        _navigationService.IsCurrentPageSameAs("ZumenSearch.Views.Rent.Residentials.LessorListPage") ||
                        _navigationService.IsCurrentPageSameAs("ZumenSearch.Views.Rent.Residentials.BrokerListPage"))
                    {
                        _navigationService.NavigateTo(BasicPageName, this);
                    }
                }

                // If this is set, then show/hide the owner and zumen from shell menu.
                WeakReferenceMessenger.Default.Send(new Models.Messenger.PropertyIsUnitOwnershipChangedMessage(_id, value));
            }
        }
    }

    [ObservableProperty]
    public partial bool IsNotUnitLevelOwnership { get; private set; } = true;

    // 建物構造
    public IReadOnlyCollection<Models.Rent.Residentials.PropertyStructureTypeLabel> StructureTypes { get; set; } = PropertyStructureTypeLabel.GetAll();
    /*
    [
        //new Structure(StructureType.Unspecified.ToString(), "未指定"),
        new Models.Rent.Residentials.PropertyStructureTypeLabel(Models.Rent.Residentials.StructureType.Wood),
        new Models.Rent.Residentials.PropertyStructureTypeLabel(Models.Rent.Residentials.StructureType.Block),
        new Models.Rent.Residentials.PropertyStructureTypeLabel(Models.Rent.Residentials.StructureType.LightSteel),
        new Models.Rent.Residentials.PropertyStructureTypeLabel(Models.Rent.Residentials.StructureType.Steel),
        new Models.Rent.Residentials.PropertyStructureTypeLabel(Models.Rent.Residentials.StructureType.RC),
        new Models.Rent.Residentials.PropertyStructureTypeLabel(Models.Rent.Residentials.StructureType.SRC),
        new Models.Rent.Residentials.PropertyStructureTypeLabel(Models.Rent.Residentials.StructureType.ALC),
        new Models.Rent.Residentials.PropertyStructureTypeLabel(Models.Rent.Residentials.StructureType.PC),
        new Models.Rent.Residentials.PropertyStructureTypeLabel(Models.Rent.Residentials.StructureType.HPC),
        new Models.Rent.Residentials.PropertyStructureTypeLabel(Models.Rent.Residentials.StructureType.RB),
        new Models.Rent.Residentials.PropertyStructureTypeLabel(Models.Rent.Residentials.StructureType.CFT),
        new Models.Rent.Residentials.PropertyStructureTypeLabel(Models.Rent.Residentials.StructureType.Other)
    ];*/

    public Models.Rent.Residentials.PropertyStructureTypeLabel SelectedStructureType
    {
        get => field ?? new(Models.Rent.Residentials.StructureType.Unspecified);
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

    // 地上階
    public string FloorCountAboveGround
    {
        get => field ?? string.Empty;
        set
        {
            if (SetProperty(ref field, value)) IsDirty = true;
        }
    }

    // 地下階
    public string FloorCountBasement
    {
        get => field ?? string.Empty;
        set
        {
            if (SetProperty(ref field, value)) IsDirty = true;
        }
    }

    // 総戸数
    public string TotalUnitCount
    {
        get => field ?? string.Empty;
        set
        {
            if (SetProperty(ref field, value)) IsDirty = true;
        }
    }

    // 築年月
    public DateTimeOffset? BuiltYearAndMonth
    {
        get;
        set
        {
            if (SetProperty(ref field, value))
            {
                IsDirty = true;
                OnPropertyChanged();
                OnPropertyChanged(nameof(BuiltYearAndMonthPreview));
            }
        }
    }

    // 築年月（和暦表示）
    public string BuiltYearAndMonthPreview
    {
        get
        {
            var s = string.Empty;

            if (BuiltYearAndMonth is not null)
            {
                var cultureJp = new CultureInfo("ja-jp", false);
                cultureJp.DateTimeFormat.Calendar = new JapaneseCalendar();

                return BuiltYearAndMonth.Value.ToString("ggy年M月", cultureJp);
                //return _builtYearAndMonth.ToString("O"); // For serialization
            }

            return s;
        }
    }

    // 不動産番号 (13桁)
    public string FudousanId
    {
        get => field ?? string.Empty;
        set
        {
            if (value is null)
            {
                return;
            }

            var text = Helpers.Common.NormalizeFullWidthCharacters(value).Trim();
            if (field == text || !Regex.IsMatch(text, @"\A[0-9]{0,13}\z"))
            {
                return;
            }

            field = text;
            IsDirty = true;
            OnPropertyChanged();
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
    } = "0000";

    // 備考
    public string Remarks
    {
        get;
        set
        {
            if (SetProperty(ref field, value))
            {
                IsDirty = true;
                OnPropertyChanged(nameof(RemarksPreview));
            }
        }
    } = string.Empty;

    public string RemarksPreview
    {
        get
        {
            string s;
            if (Remarks.Length > 14)
            {
                s = Remarks[..14] + "...";
            }
            else
            {
                s = Remarks;
            }

            return s;
        }
    }

    #endregion

    #region == 所在地プロパティ ==

    public string? MachiazaId { get; set; }

    public ObservableCollection<Prefecture> Prefectures { get; init; } = new(PrefectureMaster.Prefectures);

    public Prefecture? SelectedPrefecture
    {
        get;
        set
        {
            if (SetProperty(ref field, value))
            {
                if (field is not null)
                {
                    IsDirty = true;
                    MachiazaId = null; // Reset MachiazaId
                }

                OnPropertyChanged(nameof(AddressPreview));

                if (field is null)
                {
                    Cities = null;
                    return;
                }

                var dataset = new List<CountyAndCity>();

                dataset = _dataAccessLocationService.GetCountyAndCityByPref(field.Name);

                Cities = [.. dataset.DistinctBy(p => p.Combined)];

            }
        }
    }

    public ObservableCollection<CountyAndCity>? Cities
    {
        get;
        set
        {
            if (SetProperty(ref field, value))
            {
                //
            }
        }
    } = [];

    public CountyAndCity? SelectedCity
    {
        get;
        set
        {
            if (SetProperty(ref field, value))
            {
                if (field is not null)
                {
                    IsDirty = true;
                    MachiazaId = field?.MachiazaId;
                }

                OnPropertyChanged(nameof(AddressPreview));

                if (SelectedPrefecture is null)
                {
                    Towns = null;
                    return;
                }

                if (field is null)
                {
                    Towns = null;
                    return;
                }

                var dataset = new List<WardAndOaza>();

                dataset = _dataAccessLocationService.GetWardAndOazaByPrefCountyCity(SelectedPrefecture.Name, field.County, field.City);

                Towns = [.. dataset.DistinctBy(p => p.Combined)];
            }
        }
    }

    public ObservableCollection<WardAndOaza>? Towns
    {
        get;
        set
        {
            if (SetProperty(ref field, value))
            {
                //
            }
        }
    } = [];

    public WardAndOaza? SelectedTown
    {
        get;
        set
        {
            if (SetProperty(ref field, value))
            {
                if (field is not null)
                {
                    IsDirty = true;
                    MachiazaId = field?.MachiazaId;
                }

                OnPropertyChanged(nameof(AddressPreview));

                if (SelectedPrefecture is null)
                {
                    Chous = null;
                    return;
                }
                if (SelectedCity is null)
                {
                    Chous = null;
                    return;
                }

                if (field is null)
                {
                    Chous = null;
                    return;
                }

                // TODO: move this

                var dataset = new List<Choume>();

                dataset = _dataAccessLocationService.GetChoumeByPrefCountyCityWardOaza(SelectedPrefecture.Name, SelectedCity.County, SelectedCity.City, field.Ward, field.Oaza);

                Chous = [.. dataset.DistinctBy(p => p.Chou)];
            }
        }
    }

    public ObservableCollection<Choume>? Chous
    {
        get;
        set
        {
            if (SetProperty(ref field, value))
            {
                //
            }
        }
    } = [];

    public Choume? SelectedChou
    {
        get;
        set
        {
            if (SetProperty(ref field, value))
            {
                if (field is not null)
                {
                    // The value (_selectedChou) may be an empty string (and it is OK).
                    IsDirty = true;
                    MachiazaId = field?.MachiazaId;
                }

                OnPropertyChanged(nameof(AddressPreview));
            }
        }
    }

    public string Edaban
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
    } = string.Empty;

    // TODO:
    public string PostalCode
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
    } = string.Empty;

    public string AddressPreview
    {
        get
        {
            // If the SelectedPef is not set, return an empty string.
            if (SelectedPrefecture == null)
            {
                return string.Empty;
            }

            var s = string.Empty;
            if (!string.IsNullOrEmpty(Edaban))
            {
                s = "-" + Edaban;
            }
            // TODO:
            return $"{SelectedPrefecture.Name}{SelectedCity?.Combined}{SelectedTown?.Combined}{SelectedChou?.Chou}{s}";
        }
    }

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
            if (string.IsNullOrEmpty(LocationLatitude) || string.IsNullOrEmpty(LocationLongitude))
            {
                return "https://maps.google.co.jp/";
            }

            return $"https://maps.google.co.jp/?q={LocationLatitude},{LocationLongitude}";
        }
    }

    #endregion

    #region == 交通プロパティ ==

    private int _selectedTransportationIndex;

    public int SelectedTransportationIndex
    {
        get => _selectedTransportationIndex;
        set
        {
            if (value >= 0 && value < _transportationEntries.Length && SetProperty(ref _selectedTransportationIndex, value))
            {
                OnPropertyChanged(nameof(SelectedTransportation));
            }
        }
    }

    private readonly TransportationEntryViewModel[] _transportationEntries;
    public TransportationEntryViewModel SelectedTransportation => _transportationEntries[SelectedTransportationIndex];

    // one
    public RailLine SelectedRailLine1
    {
        get;
        set
        {
            if (SetProperty(ref field, value))
            {
                IsDirty = true;
                // Clear  old value.
                SelectedRailStation1.Clear();
                OnPropertyChanged(nameof(SelectedRailStation1));
            }

            ShowRailStationSelect1Command.NotifyCanExecuteChanged();
        }
    } = new("","");

    public RailStation SelectedRailStation1
    {
        get;
        set
        {
            if (SetProperty(ref field, value))
            {
                IsDirty = true;
            }
        }
    } = new("", "", "");

    public string EkiToho1
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

    public string BusStop1
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

    public string BusJyousya1
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

    public string BusStopToho1
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

    // two
    public RailLine SelectedRailLine2
    {
        get;
        set
        {
            if (SetProperty(ref field, value))
            {
                IsDirty = true;
                // Clear  old value.
                SelectedRailStation2.Clear();
                OnPropertyChanged(nameof(SelectedRailStation2));
            }

            ShowRailStationSelect2Command.NotifyCanExecuteChanged();
        }
    } = new("", "");

    public RailStation SelectedRailStation2
    {
        get;
        set
        {
            if (SetProperty(ref field, value))
            {
                IsDirty = true;
            }
        }
    } = new("", "", "");

    public string EkiToho2
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

    public string BusStop2
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

    public string BusJyousya2
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

    public string BusStopToho2
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

    // three
    public RailLine SelectedRailLine3
    {
        get;
        set
        {
            if (SetProperty(ref field, value))
            {
                IsDirty = true;
                // Clear  old value.
                SelectedRailStation3.Clear();
                OnPropertyChanged(nameof(SelectedRailStation3));
            }

            ShowRailStationSelect3Command.NotifyCanExecuteChanged();
        }
    } = new("", "");

    public RailStation SelectedRailStation3
    {
        get;
        set
        {
            if (SetProperty(ref field, value))
            {
                IsDirty = true;
            }
        }
    } = new("", "", "");

    public string EkiToho3
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

    public string BusStop3
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

    public string BusJyousya3
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

    public string BusStopToho3
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

    // four
    public RailLine SelectedRailLine4
    {
        get;
        set
        {
            if (SetProperty(ref field, value))
            {
                IsDirty = true;
                // Clear  old value.
                SelectedRailStation4.Clear();
                OnPropertyChanged(nameof(SelectedRailStation4));
            }

            ShowRailStationSelect4Command.NotifyCanExecuteChanged();
        }
    } = new("", "");

    public RailStation SelectedRailStation4
    {
        get;
        set
        {
            if (SetProperty(ref field, value))
            {
                IsDirty = true;
            }
        }
    } = new("", "", "");

    public string EkiToho4
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

    public string BusStop4
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

    public string BusJyousya4
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

    public string BusStopToho4
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

    public ObservableCollection<Models.Rent.Residentials.PropertyElectricKind> ElectricKinds { get; set; } =
    [
        new Models.Rent.Residentials.PropertyElectricKind(Models.Rent.Residentials.Property.EnumElectricType.AllElectric, "オール電化"),
        new Models.Rent.Residentials.PropertyElectricKind(Models.Rent.Residentials.Property.EnumElectricType.Unspecified, "未指定")
    ];

    public Models.Rent.Residentials.PropertyElectricKind SelectedElectricKind
    {
        get => field ?? ElectricKinds[1];
        set
        {
            if (value is null)
            {
                return;
            }

            var selected = ElectricKinds.FirstOrDefault(item => item.Key == value.Key);
            if (selected is null)
            {
                return;
            }

            if (field?.Key == selected.Key)
            {
                if (!ReferenceEquals(field, selected))
                {
                    field = selected;
                    OnPropertyChanged();
                }
                return;
            }

            field = selected;
            OnPropertyChanged();
            IsDirty = true;
        }
    }

    public string ElectricDetail
    {
        get;
        set
        {
            if (SetProperty(ref field, value ?? string.Empty))
            {
                IsDirty = true;
            }
        }
    } = string.Empty;

    #endregion

    #region == ガス・水道 ==

    public bool HasGas
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

    public string GasDetail
    {
        get;
        set
        {
            if (SetProperty(ref field, value ?? string.Empty))
            {
                IsDirty = true;
            }
        }
    } = string.Empty;

    public IReadOnlyList<FacilityGasTypeLabel> GasTypes { get; } = new FacilityGasTypeLabel[]
    {
        new(FacilityGasType.CityGas),
        new(FacilityGasType.Propane),
        new(FacilityGasType.Unspecified)
    };

    public FacilityGasTypeLabel SelectedGasType
    {
        get => field ?? GasTypes[2];
        set
        {
            if (value is null)
            {
                return;
            }

            var selected = GasTypes.FirstOrDefault(item => item.Key == value.Key);
            if (selected is null)
            {
                return;
            }

            if (field?.Key == selected.Key)
            {
                if (!ReferenceEquals(field, selected))
                {
                    field = selected;
                    OnPropertyChanged();
                }
                return;
            }

            field = selected;
            OnPropertyChanged();
            IsDirty = true;
        }
    }

    public string TapWaterDetail
    {
        get;
        set
        {
            if (SetProperty(ref field, value ?? string.Empty))
            {
                IsDirty = true;
            }
        }
    } = string.Empty;

    public IReadOnlyList<FacilityTapWaterTypeLabel> TapWaterTypes { get; } = new FacilityTapWaterTypeLabel[]
    {
        new(FacilityTapWaterType.Public),
        new(FacilityTapWaterType.Private),
        new(FacilityTapWaterType.Other),
        new(FacilityTapWaterType.Unspecified)
    };

    public FacilityTapWaterTypeLabel SelectedTapWaterType
    {
        get => field ?? TapWaterTypes[3];
        set
        {
            if (value is null)
            {
                return;
            }

            var selected = TapWaterTypes.FirstOrDefault(item => item.Key == value.Key);
            if (selected is null)
            {
                return;
            }

            if (field?.Key == selected.Key)
            {
                if (!ReferenceEquals(field, selected))
                {
                    field = selected;
                    OnPropertyChanged();
                }
                return;
            }

            field = selected;
            OnPropertyChanged();
            IsDirty = true;
        }
    }

    public string SewerageDetail
    {
        get;
        set
        {
            if (SetProperty(ref field, value ?? string.Empty))
            {
                IsDirty = true;
            }
        }
    } = string.Empty;

    public IReadOnlyList<FacilitySewerageTypeLabel> SewerageTypes { get; } = new FacilitySewerageTypeLabel[]
    {
        new(FacilitySewerageType.Public),
        new(FacilitySewerageType.SepticTank),
        new(FacilitySewerageType.Other),
        new(FacilitySewerageType.Unspecified)
    };

    public FacilitySewerageTypeLabel SelectedSewerageType
    {
        get => field ?? SewerageTypes[3];
        set
        {
            if (value is null)
            {
                return;
            }

            var selected = SewerageTypes.FirstOrDefault(item => item.Key == value.Key);
            if (selected is null)
            {
                return;
            }

            if (field?.Key == selected.Key)
            {
                if (!ReferenceEquals(field, selected))
                {
                    field = selected;
                    OnPropertyChanged();
                }
                return;
            }

            field = selected;
            OnPropertyChanged();
            IsDirty = true;
        }
    }

    #endregion

    #region == 共聴設備・インターネット ==

    public bool GroundDigital
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
    public bool BsAntenna
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
    public bool CsAntenna
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
    public bool Catv
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
    public bool Usen
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
    public bool Internet
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
    public bool Fiber
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
    public bool InternetFree
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

    public string TvAntennaDetail
    {
        get;
        set
        {
            if (SetProperty(ref field, value ?? string.Empty))
            {
                IsDirty = true;
            }
        }
    } = string.Empty;

    public string InternetDetail
    {
        get;
        set
        {
            if (SetProperty(ref field, value ?? string.Empty))
            {
                IsDirty = true;
            }
        }
    } = string.Empty;

    #endregion

    #region == 駐車場・駐輪場 ==

    public bool HasParking
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
    public bool IsParkingFree
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

    public IReadOnlyList<FacilityParkingContractTypeLabel> ParkingContractTypes { get; } = FacilityParkingContractTypeLabel.GetAll();

    public FacilityParkingContractTypeLabel SelectedParkingContractType
    {
        get => field ?? ParkingContractTypes.First(item => item.Key == FacilityParkingContractType.Optional);
        set
        {
            if (value is null)
            {
                return;
            }

            var selected = ParkingContractTypes.FirstOrDefault(item => item.Key == value.Key);
            if (selected is null)
            {
                return;
            }

            if (field?.Key == selected.Key)
            {
                if (!ReferenceEquals(field, selected))
                {
                    field = selected;
                    OnPropertyChanged();
                }
                return;
            }

            field = selected;
            OnPropertyChanged();
            IsDirty = true;
        }
    }

    public string ParkingFee
    {
        get;
        set
        {
            if (SetProperty(ref field, value ?? string.Empty))
            {
                IsDirty = true;
            }
        }
    } = string.Empty;

    public IReadOnlyList<FacilityTaxTypeLabel> TaxTypes { get; } = FacilityTaxTypeLabel.GetAll();

    public FacilityTaxTypeLabel SelectedParkingFeeTaxType
    {
        get => field ?? TaxTypes.First(item => item.Key == FacilityTaxType.Included);
        set
        {
            if (value is null)
            {
                return;
            }

            var selected = TaxTypes.FirstOrDefault(item => item.Key == value.Key);
            if (selected is null)
            {
                return;
            }

            if (field?.Key == selected.Key)
            {
                if (!ReferenceEquals(field, selected))
                {
                    field = selected;
                    OnPropertyChanged();
                }
                return;
            }

            field = selected;
            OnPropertyChanged();
            IsDirty = true;
        }
    }

    public IReadOnlyList<FacilityAvailabilityTypeLabel> AvailabilityTypes { get; } = FacilityAvailabilityTypeLabel.GetAll();

    public FacilityAvailabilityTypeLabel SelectedParkingAvailability
    {
        get => field ?? AvailabilityTypes.First(item => item.Key == FacilityAvailabilityType.Unspecified);
        set
        {
            if (value is null)
            {
                return;
            }

            var selected = AvailabilityTypes.FirstOrDefault(item => item.Key == value.Key);
            if (selected is null)
            {
                return;
            }

            if (field?.Key == selected.Key)
            {
                if (!ReferenceEquals(field, selected))
                {
                    field = selected;
                    OnPropertyChanged();
                }
                return;
            }

            field = selected;
            OnPropertyChanged();
            IsDirty = true;
        }
    }

    public string ParkingSpaces
    {
        get;
        set
        {
            if (SetProperty(ref field, value ?? string.Empty))
            {
                IsDirty = true;
            }
        }
    } = string.Empty;

    public string ParkingDistance
    {
        get;
        set
        {
            if (SetProperty(ref field, value ?? string.Empty))
            {
                IsDirty = true;
            }
        }
    } = string.Empty;

    public string ParkingNotes
    {
        get;
        set
        {
            if (SetProperty(ref field, value ?? string.Empty))
            {
                IsDirty = true;
            }
        }
    } = string.Empty;

    public bool HasBicycleParking
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
    public bool IsBicycleParkingFree
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

    public FacilityAvailabilityTypeLabel SelectedBicycleParkingAvailability
    {
        get => field ?? AvailabilityTypes.First(item => item.Key == FacilityAvailabilityType.Unspecified);
        set
        {
            if (value is null)
            {
                return;
            }

            var selected = AvailabilityTypes.FirstOrDefault(item => item.Key == value.Key);
            if (selected is null)
            {
                return;
            }

            if (field?.Key == selected.Key)
            {
                if (!ReferenceEquals(field, selected))
                {
                    field = selected;
                    OnPropertyChanged();
                }
                return;
            }

            field = selected;
            OnPropertyChanged();
            IsDirty = true;
        }
    }

    public string BicycleParkingFee
    {
        get;
        set
        {
            if (SetProperty(ref field, value ?? string.Empty))
            {
                IsDirty = true;
            }
        }
    } = string.Empty;

    public FacilityTaxTypeLabel SelectedBicycleParkingFeeTaxType
    {
        get => field ?? TaxTypes.First(item => item.Key == FacilityTaxType.Included);
        set
        {
            if (value is null)
            {
                return;
            }

            var selected = TaxTypes.FirstOrDefault(item => item.Key == value.Key);
            if (selected is null)
            {
                return;
            }

            if (field?.Key == selected.Key)
            {
                if (!ReferenceEquals(field, selected))
                {
                    field = selected;
                    OnPropertyChanged();
                }
                return;
            }

            field = selected;
            OnPropertyChanged();
            IsDirty = true;
        }
    }

    public string BicycleParkingNotes
    {
        get;
        set
        {
            if (SetProperty(ref field, value ?? string.Empty))
            {
                IsDirty = true;
            }
        }
    } = string.Empty;

    public bool HasMotorcycleParking
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
    public bool IsMotorcycleParkingFree
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

    public FacilityAvailabilityTypeLabel SelectedMotorcycleParkingAvailability
    {
        get => field ?? AvailabilityTypes.First(item => item.Key == FacilityAvailabilityType.Unspecified);
        set
        {
            if (value is null)
            {
                return;
            }

            var selected = AvailabilityTypes.FirstOrDefault(item => item.Key == value.Key);
            if (selected is null)
            {
                return;
            }

            if (field?.Key == selected.Key)
            {
                if (!ReferenceEquals(field, selected))
                {
                    field = selected;
                    OnPropertyChanged();
                }
                return;
            }

            field = selected;
            OnPropertyChanged();
            IsDirty = true;
        }
    }

    public string MotorcycleParkingFee
    {
        get;
        set
        {
            if (SetProperty(ref field, value ?? string.Empty))
            {
                IsDirty = true;
            }
        }
    } = string.Empty;

    public FacilityTaxTypeLabel SelectedMotorcycleParkingFeeTaxType
    {
        get => field ?? TaxTypes.First(item => item.Key == FacilityTaxType.Included);
        set
        {
            if (value is null)
            {
                return;
            }

            var selected = TaxTypes.FirstOrDefault(item => item.Key == value.Key);
            if (selected is null)
            {
                return;
            }

            if (field?.Key == selected.Key)
            {
                if (!ReferenceEquals(field, selected))
                {
                    field = selected;
                    OnPropertyChanged();
                }
                return;
            }

            field = selected;
            OnPropertyChanged();
            IsDirty = true;
        }
    }

    public string MotorcycleParkingNotes
    {
        get;
        set
        {
            if (SetProperty(ref field, value ?? string.Empty))
            {
                IsDirty = true;
            }
        }
    } = string.Empty;

    #endregion

    #endregion

    #region == 管理プロパティ ==

    public ObservableCollection<Models.Rent.Residentials.PropertyKanriShutai> KanriShutais { get; set; } =
    [
        new Models.Rent.Residentials.PropertyKanriShutai(Models.Rent.Residentials.Property.EnumKanriShutai.Unspecified, "未指定"),
        new Models.Rent.Residentials.PropertyKanriShutai(Models.Rent.Residentials.Property.EnumKanriShutai.Jisya, "自社管理"),
        new Models.Rent.Residentials.PropertyKanriShutai(Models.Rent.Residentials.Property.EnumKanriShutai.Tasya, "他社管理"),
        new Models.Rent.Residentials.PropertyKanriShutai(Models.Rent.Residentials.Property.EnumKanriShutai.Kashinushi, "貸主管理")
    ];

    public Models.Rent.Residentials.PropertyKanriShutai? SelectedKanriShutai
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
                IsKanriUnspecified = field.Key == Models.Rent.Residentials.Property.EnumKanriShutai.Unspecified;
                IsKanriJisya = field.Key == Models.Rent.Residentials.Property.EnumKanriShutai.Jisya;
                IsKanriTasya = field.Key == Models.Rent.Residentials.Property.EnumKanriShutai.Tasya;
                IsKanriKashinushi = field.Key == Models.Rent.Residentials.Property.EnumKanriShutai.Kashinushi;
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

    #region == 写真プロパティ ==

    public ObservableCollection<Models.Rent.Residentials.PropertyPicture> Pictures
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

    #region == PDFプロパティ ==

    public ObservableCollection<Models.Rent.Residentials.PropertyPdf> Pdfs
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

    #region == 貸主プロパティ ==

    public ObservableCollection<Models.Rent.Residentials.PersonWrapperForPropertyViewModel> LessorsWrapper
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

    public ObservableCollection<Models.Rent.Residentials.PersonWrapperForPropertyViewModel> BrokersWrapper
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

    #region == 部屋プロパティ ==

    public ObservableCollection<ListingSummary> ListingSummaries
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

    /*
    [ObservableProperty]
    [NotifyCanExecuteChangedFor(nameof(EditSelectedUnitCommand))]
    public partial Models.Rent.Residentials.Listing? SelectedRoom { get; set; }
    */

    #endregion

    #endregion

    #region == Messages ==

    public void Receive(ListingUpdatedMessage listing)
    {
        if (DeferMessage(() => Receive(listing))) return;
        var something = listing.Value;
        if (something is null)
        {
            return;
        }

        if (something is not Models.Rent.Residentials.Listing room || !room.PropertyId.Equals(_id, StringComparison.Ordinal))
        {
            return;
        }

        // Get the existing summary for this room, if it exists, and update or add it to the collection.
        var existingSummary = ListingSummaries.FirstOrDefault(item => item.Id.Equals(room.Id, StringComparison.Ordinal));
        var summary = new ListingSummary(room.Id, room.Name);
        if (existingSummary is null)
        {
            ListingSummaries.Add(summary);
        }
        else
        {
            ListingSummaries[ListingSummaries.IndexOf(existingSummary)] = summary;
        }

        // Update the unsaved listing list with the new room data.
        var existingListing = _unsavedListingList.FirstOrDefault(item => item.Id.Equals(room.Id, StringComparison.Ordinal));
        if (existingListing is not null)
        {
            Debug.WriteLine($"Updating existing room {room.Id} in unsaved listing list.");
            existingListing = room;
            IsDirty = true;
        }
        else
        {
            if (room.Status == EntityStatus.New)
            {
                Debug.WriteLine($"Adding new room {room.Id} to unsaved listing list.");
                _unsavedListingList.Add(room);
                IsDirty = true;
            }
            else
            {
                // Remove the child editor window for this room if it exists
                // because it is no longer new and has been saved and can be saved independently.
                var win = this.UnsavedChildEditorList.FirstOrDefault(w => w is Views.Rent.Residentials.Listing.EditorWindow ew && ew.Id == room.Id);
                if (win is not null)
                {
                    Debug.WriteLine($"Removing child editor window for room {room.Id} because it is no longer new and has been saved.");
                    this.UnsavedChildEditorList.Remove(win);
                }
            }
        }

        /*
        if (IsListingPendingDeletion(room.Id)) return;
        var isPendingDraft = room.Status == EntityStatus.New && _building.Status == EntityStatus.New;
        if (isPendingDraft)
        {
            var draft = ResidentialListingDraft.FromListing(room);
            if (!_supersededDraftFiles.TryGetValue(room.Id, out var superseded))
            {
                superseded = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
                _supersededDraftFiles.Add(room.Id, superseded);
            }
            if (_pendingListingDrafts.TryGetValue(room.Id, out var previousDraft)) superseded.UnionWith(GetDraftAttachmentFiles(previousDraft));
            superseded.ExceptWith(GetDraftAttachmentFiles(draft));
            _pendingListingDrafts[room.Id] = draft;
            _pendingListingObjects[room.Id] = (room.Pictures.Concat(room.PicturesToBeDeleted).ToList(), room.Pdfs.Concat(room.PdfsToBeDeleted).ToList(), room.Lessors.Concat(room.LessorsToBeDeleted).DistinctBy(person => person.Id).ToList(), room.Brokers.Concat(room.BrokersToBeDeleted).DistinctBy(person => person.Id).ToList());
        }
        else if (room.Status == EntityStatus.Saved)
        {
            _committedListingDrafts[room.Id] = ResidentialListingDraft.FromListing(room);
            var directory = Path.Combine(_propertyDataDirectoryPath, room.Id);
            var retained = room.Pictures.Select(picture => Path.Combine(directory, picture.ImageFilename)).Concat(room.Pdfs.SelectMany(pdf => new[] { Path.Combine(directory, pdf.PdfFilename), Path.Combine(directory, pdf.ThumbnailFilename) })).ToHashSet(StringComparer.OrdinalIgnoreCase);
            _pendingListingPdfCleanupFiles.RemoveAll(retained.Contains);
        }
        
        var existingRoom = Rooms.FirstOrDefault(item => item.Id.Equals(room.Id, StringComparison.Ordinal));
        var summary = new ListingSummary(room.Id, room.Name, isPendingDraft);
        if (existingRoom is null) Rooms.Add(summary);
        else Rooms[Rooms.IndexOf(existingRoom)] = summary;
        if (!_building.ListingIds.Contains(room.Id, StringComparer.Ordinal)) _building.ListingIds.Add(room.Id);

        if (isPendingDraft)
        {
            //Debug.WriteLine("(room.ListingStatus == EnumListingStatus.New) @PropertyViewModel");
            IsDirty = true;
        }
        else
        {
            // Remove the child editor window for this room if it exists
            // because it is no longer new and has been saved and can be saved independently.
            var win = this.UnsavedChildEditorList.FirstOrDefault(w => w is Views.Rent.Residentials.Listing.EditorWindow ew && ew.Id == room.Id);
            if (win is not null)
            {
                this.UnsavedChildEditorList.Remove(win);
            }
        }
        */
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

        psn.Person = lessor; //= new PersonWrapperForPropertyViewModel(lessor, this);
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

    public void Receive(ListingDeletedMessage listingId)
    {
        if (DeferMessage(() => Receive(listingId))) return;
        var id = listingId.Value;
        if (string.IsNullOrEmpty(id))
        {
            return;
        }

        var room = ListingSummaries.FirstOrDefault(r => r.Id.Equals(id, StringComparison.Ordinal));
        if (room is not null) ListingSummaries.Remove(room);
        var summary = _building.ListingSummaries.FirstOrDefault(ls => ls.Id.Equals(id, StringComparison.Ordinal));
        if (summary is not null) _building.ListingSummaries.Remove(summary);
        /*
        _pendingListingDrafts.Remove(id);
        _pendingListingObjects.Remove(id);
        _committedListingDrafts.Remove(id);
        _supersededDraftFiles.Remove(id);
        _stagedListingDeletions.Remove(id);
        _stagedPendingListingDeletions.Remove(id);
        */
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

        // Should be auto deleted from the database table thanks to "cascade".
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

        // Should be auto deleted from the database table thanks to "cascade".
    }

    public void Receive(WindowClosedMessage<Microsoft.UI.Xaml.Window> window)
    {
        var ewin = window.Value;

        if (ewin is null)
        {
            return;
        }

        if (ewin is Views.Rent.Residentials.Listing.EditorWindow rlwin)
        {
            this.UnsavedChildEditorList.Remove(rlwin);
        }
    }

    #endregion

    #region == Public Methods ==

    public void SetPropertyBusy(bool value)
    {
        _propertyBusy = value;
        OnPropertyChanged(nameof(IsEditorEnabled));
        SaveCommand.NotifyCanExecuteChanged();
        if (!value && !IsSaving) while (_deferredMessages.TryDequeue(out var message)) message();
    }

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
            catch (Exception ex) when (ex is IOException or UnauthorizedAccessException or COMException or InvalidOperationException or ArgumentException)
            {
                Debug.WriteLine($"Failed to complete property editor operation: {ex}");
                ShowAttachmentError($"編集中の処理を完了できませんでした: {ex.Message}");
                return false;
            }
        }
    }
    */

    private List<Task> PendingEditorTasks()
    {
        IAsyncRelayCommand[] commands = [AddNewBuildingPicturesCommand, AddNewBuildingPdfsCommand, OpenBuildingPdfCommand, DeleteBuildingPictureCommand, DeleteBuildingPdfCommand, DeleteSelectedRoomCommand, AddLessorCommand, DeleteLessorCommand, AddBrokerCommand, DeleteBrokerCommand, ShowRailLineSelect1Command, ShowRailStationSelect1Command, ShowRailLineSelect2Command, ShowRailStationSelect2Command, ShowRailLineSelect3Command, ShowRailStationSelect3Command, ShowRailLineSelect4Command, ShowRailStationSelect4Command];
        return commands.Where(command => command.ExecutionTask is { IsCompleted: false }).Select(command => command.ExecutionTask!).ToList();
    }

    private bool DeferMessage(Action action)
    {
        if (!IsSaving && !_propertyBusy) return false;
        _deferredMessages.Enqueue(action);
        return true;
    }

    public bool DiscardChanges()
    {
        _discardPending = true;
        OnPropertyChanged(nameof(IsEditorEnabled));
        SaveCommand.NotifyCanExecuteChanged();

        bool result = DiscardUnsavedFiles();

        // Need to restore the original values from unsaved _building because IsUnitLevelOwnership change is real-time not save-time update.
        WeakReferenceMessenger.Default.Send(new PropertyIsUnitOwnershipChangedMessage(_id, _building.IsUnitLevelOwnership));

        _discardPending = false;
        IsDirty = false;

        return result;
    }

    public void Dispose()
    {
        // Currently, this Dispose method is (not decided) called when the window is closed. It is (also) called when the application exits.
        if (_disposed)
        {
            return;
        }

        _disposed = true;

        foreach (var entry in _transportationEntries)
        {
            entry.Dispose();
        }

#pragma warning disable IL3050 // Disable AOT dynamic code warning
#pragma warning disable IL2026 // Disable Trimming unreferenced code warning
        IsActive = false;
#pragma warning restore IL2026
#pragma warning restore IL3050

        // or Unsubscribe
        //WeakReferenceMessenger.Default.UnregisterAll(this);

        _cts?.Cancel();
        _cts?.Dispose();

        GC.SuppressFinalize(this);

        Debug.WriteLine($"PropertyViewModel.Dispose() called. Id: {this._id}");
    }

    #endregion

    #region == Private Methods ==

    private void PopulateValues()
    {
        // Basics

        Name = _building.Name;

        //SelectedKind = _building.BuildingKind;
        var propertyTypeKey = PropertyTypes.FirstOrDefault(k => k.Key == _building.PropertyKind.Key);
        SelectedPropertyType = propertyTypeKey is null ? new(Models.Rent.Residentials.PropertyKindType.Unspecified) : propertyTypeKey;
        IsUnitLevelOwnership = _building.IsUnitLevelOwnership;
        var structureKey = StructureTypes.FirstOrDefault(k => k.Key == _building.PropertyStructure.Key);
        SelectedStructureType = structureKey is null ? new(Models.Rent.Residentials.StructureType.Unspecified) : structureKey;

        FloorCountAboveGround = _building.FloorCountAboveGround == 0 ? string.Empty : _building.FloorCountAboveGround.ToString(CultureInfo.InvariantCulture);
        FloorCountBasement = _building.FloorCountBasement == 0 ? string.Empty : _building.FloorCountBasement.ToString(CultureInfo.InvariantCulture);
        TotalUnitCount = _building.TotalUnitCount == 0 ? string.Empty : _building.TotalUnitCount.ToString(CultureInfo.InvariantCulture);
        BuiltYearAndMonth = _building.BuiltYearAndMonth.Year != 1900 ? _building.BuiltYearAndMonth : null;
        FudousanId = _building.FudousanId;
        FudousanIdAdditionalCode = _building.FudousanIdAdditionalCode;
        Remarks = _building.Remarks;

        // Location
        if (!string.IsNullOrEmpty(_building.Address.Prefecture?.MunicipalityCode))
        {
            var hoge = Prefectures.FirstOrDefault<Prefecture>(p => p.MunicipalityCode.Equals(_building.Address.Prefecture.MunicipalityCode, StringComparison.Ordinal));
            if (hoge is not null)
            {
                SelectedPrefecture = hoge;
            }
        }
        if ((Cities is not null) && ((!string.IsNullOrEmpty(_building.Address.CountyAndCity?.County)) || (!string.IsNullOrEmpty(_building.Address.CountyAndCity?.City))))
        {
            foreach (var cty in Cities)
            {
                if (cty.County.Equals(_building.Address.CountyAndCity.County, StringComparison.Ordinal) && cty.City.Equals(_building.Address.CountyAndCity.City, StringComparison.Ordinal))
                {
                    SelectedCity = cty;
                    break;
                }
            }
        }
        if ((Towns is not null) && ((!string.IsNullOrEmpty(_building.Address.WardAndOaza?.Ward)) || (!string.IsNullOrEmpty(_building.Address.WardAndOaza?.Oaza))))
        {
            foreach (var twn in Towns)
            {
                if (twn.Ward.Equals(_building.Address.WardAndOaza.Ward, StringComparison.Ordinal) && twn.Oaza.Equals(_building.Address.WardAndOaza.Oaza, StringComparison.Ordinal))
                {
                    SelectedTown = twn;
                    break;
                }
            }
        }
        if (Chous is not null) // Allow an empty string. //&& (!string.IsNullOrEmpty(_building.LocChoume))
        {
            var hoge = Chous.FirstOrDefault<Choume>(p => p.Chou.Equals(_building.Address.Choume?.Chou, StringComparison.Ordinal));
            if (hoge is not null)
            {
                SelectedChou = hoge;
            }
        }
        Edaban = _building.Address.Edaban;
        PostalCode = _building.Address.PostalCode;
        MachiazaId = _building.Address.MachiazaId; // Must be at the end of the location properties, because it is set and cleared by the other properties.
        LocationLatitude = _building.Address.LocationLatitude;
        LocationLongitude = _building.Address.LocationLongitude;

        // 交通
        SelectedRailLine1.SetLineCode(_building.Train1.RailLine?.LineCode ?? "");
        SelectedRailLine1.SetLineName(_building.Train1.RailLine?.LineName ?? "");
        if (SelectedRailLine1.LineCode.Equals(_building.Train1.RailStation?.LineCode ?? "", StringComparison.Ordinal))
        {
            SelectedRailStation1.SetLineCode(_building.Train1.RailStation?.LineCode ?? "");
            SelectedRailStation1.SetStationCode(_building.Train1.RailStation?.StationCode ?? "");
            SelectedRailStation1.SetStationName(_building.Train1.RailStation?.StationName ?? "");
        }
        EkiToho1 = _building.Train1.EkiToho <= 0 ? "" : _building.Train1.EkiToho.ToString(CultureInfo.InvariantCulture);
        BusStop1 = _building.BusStop1;
        BusJyousya1 = _building.BusJyousya1 == 0 ? string.Empty : _building.BusJyousya1.ToString(CultureInfo.InvariantCulture);
        BusStopToho1 = _building.BusStopToho1 == 0 ? string.Empty : _building.BusStopToho1.ToString(CultureInfo.InvariantCulture);

        SelectedRailLine2.SetLineCode(_building.Train2.RailLine?.LineCode ?? "");
        SelectedRailLine2.SetLineName(_building.Train2.RailLine?.LineName ?? "");
        if (SelectedRailLine2.LineCode.Equals(_building.Train2.RailStation?.LineCode ?? "", StringComparison.Ordinal))
        {
            SelectedRailStation2.SetLineCode(_building.Train2.RailStation?.LineCode ?? "");
            SelectedRailStation2.SetStationCode(_building.Train2.RailStation?.StationCode ?? "");
            SelectedRailStation2.SetStationName(_building.Train2.RailStation?.StationName ?? "");
        }
        EkiToho2 = _building.Train2.EkiToho <= 0 ? "" : _building.Train2.EkiToho.ToString(CultureInfo.InvariantCulture);
        BusStop2 = _building.BusStop2;
        BusJyousya2 = _building.BusJyousya2 == 0 ? string.Empty : _building.BusJyousya2.ToString(CultureInfo.InvariantCulture);
        BusStopToho2 = _building.BusStopToho2 == 0 ? string.Empty : _building.BusStopToho2.ToString(CultureInfo.InvariantCulture);

        SelectedRailLine3.SetLineCode(_building.Train3.RailLine?.LineCode ?? "");
        SelectedRailLine3.SetLineName(_building.Train3.RailLine?.LineName ?? "");
        if (SelectedRailLine3.LineCode.Equals(_building.Train3.RailStation?.LineCode ?? "", StringComparison.Ordinal))
        {
            SelectedRailStation3.SetLineCode(_building.Train3.RailStation?.LineCode ?? "");
            SelectedRailStation3.SetStationCode(_building.Train3.RailStation?.StationCode ?? "");
            SelectedRailStation3.SetStationName(_building.Train3.RailStation?.StationName ?? "");
        }
        EkiToho3 = _building.Train3.EkiToho <= 0 ? "" : _building.Train3.EkiToho.ToString(CultureInfo.InvariantCulture);
        BusStop3 = _building.BusStop3;
        BusJyousya3 = _building.BusJyousya3 == 0 ? string.Empty : _building.BusJyousya3.ToString(CultureInfo.InvariantCulture);
        BusStopToho3 = _building.BusStopToho3 == 0 ? string.Empty : _building.BusStopToho3.ToString(CultureInfo.InvariantCulture);

        SelectedRailLine4.SetLineCode(_building.Train4.RailLine?.LineCode ?? "");
        SelectedRailLine4.SetLineName(_building.Train4.RailLine?.LineName ?? "");
        if (SelectedRailLine4.LineCode.Equals(_building.Train4.RailStation?.LineCode ?? "", StringComparison.Ordinal))
        {
            SelectedRailStation4.SetLineCode(_building.Train4.RailStation?.LineCode ?? "");
            SelectedRailStation4.SetStationCode(_building.Train4.RailStation?.StationCode ?? "");
            SelectedRailStation4.SetStationName(_building.Train4.RailStation?.StationName ?? "");
        }
        EkiToho4 = _building.Train4.EkiToho <= 0 ? "" : _building.Train4.EkiToho.ToString(CultureInfo.InvariantCulture);
        BusStop4 = _building.BusStop4;
        BusJyousya4 = _building.BusJyousya4 == 0 ? string.Empty : _building.BusJyousya4.ToString(CultureInfo.InvariantCulture);
        BusStopToho4 = _building.BusStopToho4 == 0 ? string.Empty : _building.BusStopToho4.ToString(CultureInfo.InvariantCulture);

        // Facilities
        HasAutolock = _building.HasAutolock;
        HasElevator = _building.HasElevator;
        HasSecurityCamera = _building.HasSecurityCamera;
        HasParcelLocker = _building.HasParcelLocker;
        HasElectric = _building.HasElectric;
        var electricKind = ElectricKinds.FirstOrDefault(item => item.Key == _building.PropertyElectricKind);
        if (electricKind is not null) SelectedElectricKind = electricKind;
        ElectricDetail = _building.ElectricDetail;
        HasGas = _building.HasGas;
        GasDetail = _building.GasDetail;
        SelectedGasType = GasTypes.First(item => item.Key == _building.GasType);
        TapWaterDetail = _building.TapWaterDetail;
        SelectedTapWaterType = TapWaterTypes.First(item => item.Key == _building.TapWaterType);
        SewerageDetail = _building.SewerageDetail;
        SelectedSewerageType = SewerageTypes.First(item => item.Key == _building.SewerageType);
        GroundDigital = _building.GroundDigital;
        BsAntenna = _building.BsAntenna;
        CsAntenna = _building.CsAntenna;
        Catv = _building.Catv;
        Usen = _building.Usen;
        Internet = _building.Internet;
        Fiber = _building.Fiber;
        InternetFree = _building.InternetFree;
        TvAntennaDetail = _building.TvAntennaDetail;
        InternetDetail = _building.InternetDetail;
        HasParking = _building.HasParking;
        IsParkingFree = _building.IsParkingFree;
        SelectedParkingContractType = ParkingContractTypes.First(item => item.Key == _building.ParkingContractType);
        ParkingFee = _building.ParkingFee;
        SelectedParkingFeeTaxType = TaxTypes.First(item => item.Key == _building.ParkingFeeTaxType);
        SelectedParkingAvailability = AvailabilityTypes.First(item => item.Key == _building.ParkingAvailability);
        ParkingSpaces = _building.ParkingSpaces;
        ParkingDistance = _building.ParkingDistance;
        ParkingNotes = _building.ParkingNotes;
        HasBicycleParking = _building.HasBicycleParking;
        IsBicycleParkingFree = _building.IsBicycleParkingFree;
        SelectedBicycleParkingAvailability = AvailabilityTypes.First(item => item.Key == _building.BicycleParkingAvailability);
        BicycleParkingFee = _building.BicycleParkingFee;
        SelectedBicycleParkingFeeTaxType = TaxTypes.First(item => item.Key == _building.BicycleParkingFeeTaxType);
        BicycleParkingNotes = _building.BicycleParkingNotes;
        HasMotorcycleParking = _building.HasMotorcycleParking;
        IsMotorcycleParkingFree = _building.IsMotorcycleParkingFree;
        SelectedMotorcycleParkingAvailability = AvailabilityTypes.First(item => item.Key == _building.MotorcycleParkingAvailability);
        MotorcycleParkingFee = _building.MotorcycleParkingFee;
        SelectedMotorcycleParkingFeeTaxType = TaxTypes.First(item => item.Key == _building.MotorcycleParkingFeeTaxType);
        MotorcycleParkingNotes = _building.MotorcycleParkingNotes;

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

        // Pictures:(don't use directly)
        /*
        Pictures = new ObservableCollection<Models.Rent.Residentials.PropertyPicture>(_building.Pictures); // create a copy.
        foreach (var item in Pictures)
        {
            // Filename to actuall path.
            item.BasePath = System.IO.Path.Combine(App.PropertyBlobDataFolder, _building.Id);
            item.ParentViewModel = this;
            item.SetIsModified(false); // Needed this.
            item.PropertyChanged += OnBuildingPicturePropertyChanged;
        }
        */
        Pictures.Clear();
        foreach (var item in _building.Pictures)
        {
            var internalItem = new PropertyPicture(item.Id, item.ImageFilename, item.Status)
            {
                IsMain = item.IsMain,
                PictureType = item.PictureType,
                Description = item.Description,
                ThumbImagePath = item.ThumbImagePath,
                BasePath = System.IO.Path.Combine(App.PropertyBlobDataFolder, _building.Id),
                ParentViewModel = this
            };
            internalItem.SetIsModified(false); // Needed this.
            internalItem.PropertyChanged += OnBuildingPicturePropertyChanged;
            Pictures.Add(internalItem);
        }

        Pictures.CollectionChanged += (s, e) =>
        {
            // Unsubscribe from removed items
            if (e.OldItems != null)
            {
                foreach (Models.Rent.Residentials.PropertyPicture item in e.OldItems)
                {
                    Debug.WriteLine($"Item {item.Id} Removed from Pictures. @CollectionChanged in PopulateEntityValues of Bldg.MainViewModel");
                    IsDirty = true;

                    item.PropertyChanged -= OnBuildingPicturePropertyChanged;
                }
            }

            // Subscribe to PropertyChanged.
            if (e.NewItems != null)
            {
                foreach (Models.Rent.Residentials.PropertyPicture item in e.NewItems)
                {
                    Debug.WriteLine($"Item {item.Id} Added to Pictures. @CollectionChanged in PopulateEntityValues of Bldg.MainViewModel");
                    IsDirty = true;

                    item.PropertyChanged += OnBuildingPicturePropertyChanged;
                }
            }
        };

        // PDFs
        /*
        Pdfs = new ObservableCollection<Models.Rent.Residentials.PropertyPdf>(_building.Pdfs);
        foreach (var item in Pdfs)
        {
            item.BasePath = System.IO.Path.Combine(App.PropertyBlobDataFolder, _building.Id);
            item.ParentViewModel = this;
            item.SetIsModified(false);
            item.PropertyChanged += OnBuildingPdfPropertyChanged;
        }
        */
        Pdfs.Clear();
        foreach (var item in _building.Pdfs)
        {
            var internalItem = new PropertyPdf(item.Id, item.PdfFilename, item.ThumbnailFilename, item.Status)
            {
                IsMain = item.IsMain,
                PdfType = item.PdfType,
                Description = item.Description,
                BasePath = System.IO.Path.Combine(App.PropertyBlobDataFolder, _building.Id),
                ParentViewModel = this
            };
            internalItem.SetIsModified(false);
            internalItem.PropertyChanged += OnBuildingPdfPropertyChanged;
            Pdfs.Add(internalItem);
        }

        Pdfs.CollectionChanged += (sender, args) =>
        {
            if (args.OldItems is not null)
            {
                foreach (Models.Rent.Residentials.PropertyPdf item in args.OldItems)
                {
                    IsDirty = true;
                    item.PropertyChanged -= OnBuildingPdfPropertyChanged;
                }
            }

            if (args.NewItems is not null)
            {
                foreach (Models.Rent.Residentials.PropertyPdf item in args.NewItems)
                {
                    IsDirty = true;
                    item.PropertyChanged += OnBuildingPdfPropertyChanged;
                }
            }
        };

        // Lessors
        LessorsWrapper = [];
        foreach (var item in _building.Lessors)
        {
            LessorsWrapper.Add(new Models.Rent.Residentials.PersonWrapperForPropertyViewModel(item,this));
        }

        // Brokers
        BrokersWrapper = [];
        foreach (var item in _building.Brokers)
        {
            BrokersWrapper.Add(new Models.Rent.Residentials.PersonWrapperForPropertyViewModel(item, this));
        }

        ListingSummaries = new ObservableCollection<Models.Rent.Residentials.ListingSummary>(_building.ListingSummaries);
    }

    private void OnBuildingPicturePropertyChanged(object? sender, PropertyChangedEventArgs e)
    {
        if (sender is not Models.Rent.Residentials.PropertyPicture picBldg)
        {
            Debug.WriteLine("OnBuildingPicturePropertyChanged returned non PictureBldg.");
            return;
        }

        //Debug.WriteLine($"Property {e.PropertyName} changed");

        if (picBldg.IsModified)
        {
            IsDirty = true;

            var prop = e.PropertyName ?? string.Empty;
            if (prop.Equals("IsMain", StringComparison.Ordinal))
            {
                if (picBldg.IsMain)
                {
                    // Clear all other pics
                    foreach (var item in Pictures)
                    {
                        if (item != picBldg)
                        {
                            item.IsMain = false;
                        }
                    }
                }
            }
        }
    }

    private void OnBuildingPdfPropertyChanged(object? sender, PropertyChangedEventArgs e)
    {
        if (sender is not Models.Rent.Residentials.PropertyPdf pdfBldg)
        {
            Debug.WriteLine("OnBuildingPdfPropertyChanged returned non PdfBldg.");
            return;
        }

        //Debug.WriteLine($"Property {e.PropertyName} changed");

        if (pdfBldg.IsModified)
        {
            IsDirty = true;

            var prop = e.PropertyName ?? string.Empty;
            if (prop.Equals("IsMain", StringComparison.Ordinal))
            {
                if (pdfBldg.IsMain)
                {
                    // Clear all other pdfs
                    foreach (var item in Pdfs)
                    {
                        if (item != pdfBldg)
                        {
                            item.IsMain = false;
                        }
                    }
                }
            }
        }
    }

    private void OnRoomPropertyChanged(object? sender, PropertyChangedEventArgs e)
    {
        if (sender is not Models.Rent.Residentials.Listing room)
        {
            Debug.WriteLine("OnRoomPropertyChanged returned non Room.");
            return;
        }

        //Debug.WriteLine($"Property {e.PropertyName} changed");

        if (room.IsModified)
        {
            /*
            IsDirty = true;

            var prop = e.PropertyName ?? string.Empty;
            
            if (prop.Equals("IsMain"))
            {
                if (picBldg.IsMain)
                {
                    // Clear all other pics
                    foreach (var item in Pictures)
                    {
                        if (item != picBldg)
                        {
                            item.IsMain = false;
                        }
                    }
                }
            }
            */
        }
    }

    private bool ValidateName(bool isShowError = true)
    {
        if (string.IsNullOrWhiteSpace(Name))
        {
            InfoBarErrorMessage = "物件名（必須項目）が入力されていません。保存出来ませんでした。";

            IsNameHasError = true;

            if (isShowError)
            {
                IsInfoBarErrorOpen = true;
            }

            return false;
        }

        var realLength = new StringInfo(Name).LengthInTextElements;
        if (realLength > 100)
        {
            InfoBarErrorMessage = "物件名は100文字以内で入力してください。保存出来ませんでした。";

            IsNameHasError = true;

            if (isShowError)
            {
                IsInfoBarErrorOpen = true;
            }

            return false;
        }

        IsNameHasError = false;
        return true;
    }

    private void ShowErrorInfoBar(Models.ErrorInfo err, bool isShowBar = true)
    {
        InfoBarErrorMessage =
            err.Title + Environment.NewLine +
            err.Message + Environment.NewLine +
            err.Description + Environment.NewLine +
            err.Operation + Environment.NewLine +
            err.MethodName;

        IsInfoBarErrorOpen = isShowBar;
    }

    private void ShowSimpleErrorInfoBar(string message)
    {
        InfoBarErrorMessage = message;
        IsInfoBarErrorOpen = true;
    }

    private static bool IsValidOptionalDigits(string value, int maxLength)
    {
        var normalized = Helpers.Common.NormalizeFullWidthCharacters(value.Trim());
        return normalized.Length <= maxLength && normalized.All(character => character >= '0' && character <= '9');
    }

    private bool ValidatePropertyNumericValues()
    {
        (string Label, string Value, int MaxLength)[] basicValues =
        [
            ("地上階数", FloorCountAboveGround, 4),
            ("地下階数", FloorCountBasement, 4),
            ("総戸数", TotalUnitCount, 5)
        ];

        foreach (var (label, value, maxLength) in basicValues)
        {
            if (!IsValidOptionalDigits(value, maxLength))
            {
                ShowSimpleErrorInfoBar($"{label}は0以上の{maxLength}桁以内の整数で入力してください。");
                if (!_navigationService.IsCurrentPageSameAs(BasicPageName)) _navigationService.NavigateTo(BasicPageName, this);
                return false;
            }
        }

        (string Label, string Value)[] transportationValues =
        [
            ("交通1の駅から徒歩", EkiToho1), ("交通1のバス乗車時間", BusJyousya1), ("交通1のバス停から徒歩", BusStopToho1),
            ("交通2の駅から徒歩", EkiToho2), ("交通2のバス乗車時間", BusJyousya2), ("交通2のバス停から徒歩", BusStopToho2),
            ("交通3の駅から徒歩", EkiToho3), ("交通3のバス乗車時間", BusJyousya3), ("交通3のバス停から徒歩", BusStopToho3),
            ("交通4の駅から徒歩", EkiToho4), ("交通4のバス乗車時間", BusJyousya4), ("交通4のバス停から徒歩", BusStopToho4)
        ];

        for (var index = 0; index < transportationValues.Length; index++)
        {
            var (label, value) = transportationValues[index];

            if (!IsValidOptionalDigits(value, 4))
            {
                SelectedTransportationIndex = index / 3;
                
                ShowSimpleErrorInfoBar($"{label}は0以上の4桁以内の整数で入力してください。");
                
                const string pageName = "ZumenSearch.Views.Rent.Residentials.TransportationPage";
                
                if (!_navigationService.IsCurrentPageSameAs(pageName)) _navigationService.NavigateTo(pageName, this);

                return false;
            }
        }

        return true;
    }

    private bool SetValues()
    {
        if (!IsDirty)
        {
            return true;
        }

        try
        {
            // 物件名
            _building.SetName(Name);

            _building.SetBuildingType(SelectedPropertyType);
            _building.SetIsUnitLevelOwnership(IsUnitLevelOwnership);
            _building.SetBuildingStructure(SelectedStructureType);

            _building.SetFloorCountAboveGroundFromString(Helpers.Common.NormalizeFullWidthCharacters(FloorCountAboveGround));
            _building.SetFloorCountBasementFromString(Helpers.Common.NormalizeFullWidthCharacters(FloorCountBasement));
            _building.SetTotalUnitCountFromString(Helpers.Common.NormalizeFullWidthCharacters(TotalUnitCount));
            _building.SetBuiltYearAndMonth(BuiltYearAndMonth);
            // Don't strip non numeric chars. Should be validated and stripped in the UI and domain layer, not here.
            _building.SetFudousanId(FudousanId);
            // Don't strip non numeric chars. Should be validated and stripped in the UI and domain layer, not here.
            _building.SetFudousanIdAdditionalCode(FudousanIdAdditionalCode);
            _building.SetRemarks(Remarks);

            // 所在地
            _building.Address.SetMachiazaId(MachiazaId ?? string.Empty);
            _building.Address.SetPrefecture(SelectedPrefecture);
            _building.Address.SetCountyAndCity(SelectedCity);
            _building.Address.SetWardAndOaza(SelectedTown);
            _building.Address.SetChoume(SelectedChou);
            _building.Address.SetEdaban(Edaban);
            _building.Address.SetPostalCode(PostalCode);
            _building.Address.SetLocationLatitude(LocationLatitude);
            _building.Address.SetLocationLongitude(LocationLongitude);
            // 交通1
            _building.Train1.SetRailLine(SelectedRailLine1.LineCode, SelectedRailLine1.LineName);
            if (SelectedRailLine1.LineCode.Equals(SelectedRailStation1.LineCode, StringComparison.Ordinal))
            {
                _building.Train1.SetRailStation(SelectedRailStation1.LineCode, SelectedRailStation1.StationCode, SelectedRailStation1.StationName);
            }
            else
            {
                _building.Train1.SetRailStation(string.Empty, string.Empty, string.Empty);
            }
            _building.Train1.SetEkiToho(Helpers.Common.NormalizeFullWidthCharacters(EkiToho1));
            _building.SetBusStop1(BusStop1);
            _building.SetBusJyousya1(Helpers.Common.NormalizeFullWidthCharacters(BusJyousya1));
            _building.SetBusStopToho1(Helpers.Common.NormalizeFullWidthCharacters(BusStopToho1));
            // 交通2
            _building.Train2.SetRailLine(SelectedRailLine2.LineCode, SelectedRailLine2.LineName);
            if (SelectedRailLine2.LineCode.Equals(SelectedRailStation2.LineCode, StringComparison.Ordinal))
            {
                _building.Train2.SetRailStation(SelectedRailStation2.LineCode, SelectedRailStation2.StationCode, SelectedRailStation2.StationName);
            }
            else
            {
                _building.Train2.SetRailStation(string.Empty, string.Empty, string.Empty);
            }
            _building.Train2.SetEkiToho(Helpers.Common.NormalizeFullWidthCharacters(EkiToho2));
            _building.SetBusStop2(BusStop2);
            _building.SetBusJyousya2(Helpers.Common.NormalizeFullWidthCharacters(BusJyousya2));
            _building.SetBusStopToho2(Helpers.Common.NormalizeFullWidthCharacters(BusStopToho2));
            // 交通3
            _building.Train3.SetRailLine(SelectedRailLine3.LineCode, SelectedRailLine3.LineName);
            if (SelectedRailLine3.LineCode.Equals(SelectedRailStation3.LineCode, StringComparison.Ordinal))
            {
                _building.Train3.SetRailStation(SelectedRailStation3.LineCode, SelectedRailStation3.StationCode, SelectedRailStation3.StationName);
            }
            else
            {
                _building.Train3.SetRailStation(string.Empty, string.Empty, string.Empty);
            }
            _building.Train3.SetEkiToho(Helpers.Common.NormalizeFullWidthCharacters(EkiToho3));
            _building.SetBusStop3(BusStop3);
            _building.SetBusJyousya3(Helpers.Common.NormalizeFullWidthCharacters(BusJyousya3));
            _building.SetBusStopToho3(Helpers.Common.NormalizeFullWidthCharacters(BusStopToho3));
            // 交通4
            _building.Train4.SetRailLine(SelectedRailLine4.LineCode, SelectedRailLine4.LineName);
            if (SelectedRailLine4.LineCode.Equals(SelectedRailStation4.LineCode, StringComparison.Ordinal))
            {
                _building.Train4.SetRailStation(SelectedRailStation4.LineCode, SelectedRailStation4.StationCode, SelectedRailStation4.StationName);
            }
            else
            {
                _building.Train4.SetRailStation(string.Empty, string.Empty, string.Empty);
            }
            _building.Train4.SetEkiToho(Helpers.Common.NormalizeFullWidthCharacters(EkiToho4));
            _building.SetBusStop4(BusStop4);
            _building.SetBusJyousya4(Helpers.Common.NormalizeFullWidthCharacters(BusJyousya4));
            _building.SetBusStopToho4(Helpers.Common.NormalizeFullWidthCharacters(BusStopToho4));

            // Facilities
            _building.HasAutolock = HasAutolock;
            _building.HasElevator = HasElevator;
            _building.HasSecurityCamera = HasSecurityCamera;
            _building.HasParcelLocker = HasParcelLocker;
            _building.HasElectric = HasElectric;
            _building.PropertyElectricKind = SelectedElectricKind.Key;
            _building.ElectricDetail = ElectricDetail;
            _building.HasGas = HasGas;
            _building.GasDetail = GasDetail;
            _building.GasType = SelectedGasType.Key;
            _building.TapWaterDetail = TapWaterDetail;
            _building.TapWaterType = SelectedTapWaterType.Key;
            _building.SewerageDetail = SewerageDetail;
            _building.SewerageType = SelectedSewerageType.Key;
            _building.GroundDigital = GroundDigital;
            _building.BsAntenna = BsAntenna;
            _building.CsAntenna = CsAntenna;
            _building.Catv = Catv;
            _building.Usen = Usen;
            _building.Internet = Internet;
            _building.Fiber = Fiber;
            _building.InternetFree = InternetFree;
            _building.TvAntennaDetail = TvAntennaDetail;
            _building.InternetDetail = InternetDetail;
            _building.HasParking = HasParking;
            _building.IsParkingFree = IsParkingFree;
            _building.ParkingContractType = SelectedParkingContractType.Key;
            _building.ParkingFee = ParkingFee;
            _building.ParkingFeeTaxType = SelectedParkingFeeTaxType.Key;
            _building.ParkingAvailability = SelectedParkingAvailability.Key;
            _building.ParkingSpaces = ParkingSpaces;
            _building.ParkingDistance = ParkingDistance;
            _building.ParkingNotes = ParkingNotes;
            _building.HasBicycleParking = HasBicycleParking;
            _building.IsBicycleParkingFree = IsBicycleParkingFree;
            _building.BicycleParkingAvailability = SelectedBicycleParkingAvailability.Key;
            _building.BicycleParkingFee = BicycleParkingFee;
            _building.BicycleParkingFeeTaxType = SelectedBicycleParkingFeeTaxType.Key;
            _building.BicycleParkingNotes = BicycleParkingNotes;
            _building.HasMotorcycleParking = HasMotorcycleParking;
            _building.IsMotorcycleParkingFree = IsMotorcycleParkingFree;
            _building.MotorcycleParkingAvailability = SelectedMotorcycleParkingAvailability.Key;
            _building.MotorcycleParkingFee = MotorcycleParkingFee;
            _building.MotorcycleParkingFeeTaxType = SelectedMotorcycleParkingFeeTaxType.Key;
            _building.MotorcycleParkingNotes = MotorcycleParkingNotes;

            _building.SetKanriShutai(SelectedKanriShutai?.Key ?? Models.Rent.Residentials.Property.EnumKanriShutai.Unspecified);
            _building.SetKanriIsMaintenanceManagementIfIsKanriJisya(KanriIsMaintenanceManagementIfIsKanriJisya);
            _building.SetKanriIsTenantManagementIfIsKanriJisya(KanriIsTenantManagementIfIsKanriJisya);
            _building.SetKanriIsPaymentManagementIfIsKanriJisya(KanriIsPaymentManagementIfIsKanriJisya);
            _building.SetKanriIsOtherManagementIfIsKanriJisya(KanriIsOtherManagementIfIsKanriJisya);
            _building.SetKanriRemarkIfIsKanriJisya(KanriRemarkIfIsKanriJisya);
            _building.SetKanriNameOfCompanyIfIsKanriTasya(KanriNameOfCompanyIfIsKanriTasya);
            _building.SetKanriContactInfoOfCompanyIfIsKanriTasya(KanriContactInfoOfCompanyIfIsKanriTasya);
            _building.SetKanriRemarkIfIsKanriTasya(KanriRemarkIfIsKanriTasya);
            _building.SetKanriRemarkIfIsKanriKashinushi(KanriRemarkIfIsKanriKashinushi);
        }
        catch (ArgumentException ex)
        {
            var err = new Models.ErrorInfo
            {
                Title = "入力値にエラーがあります。物件情報の保存が出来ませんでした。",
                Type = Models.ErrorInfo.ErrType.UserInput,
                Code = "",
                Message = ex.Message,
                Description = ex.StackTrace ?? string.Empty,
                Operation = "Input Validation",
                MethodName = nameof(SetValues),
                OccuredAt = DateTime.Now
            };

            ShowErrorInfoBar(err,true);

            return false;
        }

        // Reset main ThumbnailImageFilePath here.
        _building.ThumbnailFilename = string.Empty;
        // Base dir for the thumbnail
        _building.BasePath = System.IO.Path.Combine(App.PropertyBlobDataFolder, _building.Id);

        // 写真
        //_building.Pictures = Pictures;
        _building.Pictures.Clear();
        foreach (var item in Pictures)
        {
            var newItem = new PropertyPicture(item.Id, item.ImageFilename, item.Status)
            {
                IsMain = item.IsMain,
                PictureType = item.PictureType,
                Description = item.Description,
                ThumbImagePath = item.ThumbImagePath,
                BasePath = System.IO.Path.Combine(App.PropertyBlobDataFolder, _building.Id),
                ParentViewModel = this
            };
            newItem.SetIsModified(item.IsModified); // Needed this.
            _building.Pictures.Add(newItem);
        }

        var thumbImg = Pictures.FirstOrDefault(i => i.IsMain == true);
        if (thumbImg is not null)
        {
            // 物件写真サムネイルに指定
            _building.ThumbnailFilename = thumbImg.ImageFilename;
        }

        // 図面
        //_building.Pdfs = Pdfs;
        _building.Pdfs.Clear();
        foreach (var item in Pdfs)
        {
            var newItem = new PropertyPdf(item.Id, item.PdfFilename, item.ThumbnailFilename, item.Status)
            {
                IsMain = item.IsMain,
                PdfType = item.PdfType,
                Description = item.Description,
                BasePath = System.IO.Path.Combine(App.PropertyBlobDataFolder, _building.Id),
                ParentViewModel = this
            };
            newItem.SetIsModified(false);
            _building.Pdfs.Add(newItem);
        }

        if (string.IsNullOrWhiteSpace(_building.ThumbnailFilename))
        {
            var thumbPdf = Pdfs.FirstOrDefault(i => i.IsMain);
            if (thumbPdf is not null)
            {
                if (!IsUnitLevelOwnership)
                {
                    // 物件写真サムネイルに指定
                    _building.ThumbnailFilename = thumbPdf.ThumbnailFilename;
                }
            }
        }

        _building.Lessors.Clear();
        foreach (var item in LessorsWrapper)
        {
            _building.Lessors.Add(item.Person);
            //foreach (var deleted in _building.LessorsToBeDeleted.Where(person => person.Id == item.Person.Id).ToList()) _building.LessorsToBeDeleted.Remove(deleted);
        }
        _building.Brokers.Clear();
        foreach (var item in BrokersWrapper)
        {
            _building.Brokers.Add(item.Person);
            //foreach (var deleted in _building.BrokersToBeDeleted.Where(person => person.Id == item.Person.Id).ToList()) _building.BrokersToBeDeleted.Remove(deleted);
        }

        _building.ListingsToBeSaved = _unsavedListingList;

        return true;
    }

    private bool DiscardUnsavedFiles()
    {
        var failures = new List<string>();

        DeleteTrackedFiles(_unsavedBuildingPictureFileList, failures);
        DeleteTrackedFiles(_unsavedBuildingPdfThumbnailFileList, failures);
        DeleteTrackedFiles(_unsavedBuildingPdfFileList, failures);

        if (_building.Status == EntityStatus.New && Directory.Exists(_propertyDataDirectoryPath))
        {
            try
            {
                Directory.Delete(_propertyDataDirectoryPath, true);
            }
            catch (Exception ex) when (ex is IOException or UnauthorizedAccessException)
            {
                Debug.WriteLine($"Failed to delete property data directory '{_propertyDataDirectoryPath}': {ex}");
                failures.Add(_propertyDataDirectoryPath);
            }
        }

        if (failures.Count == 0)
        {
            return true;
        }

        ShowSimpleErrorInfoBar($"変更の破棄中に一時ファイルを削除できませんでした: {string.Join(", ", failures)}");
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

    #region == Save related commands ==

    [RelayCommand(CanExecute = nameof(CanSave))]
    public async Task Save()
    {
        if (_discardPending)
        {
            ShowSimpleErrorInfoBar("変更の破棄が完了していないため保存できません。もう一度画面を閉じて再試行してください。");
            return;
        }
        if (IsSaving || !IsDirty) return;
        /*
        var main = App.GetService<ViewModels.MainViewModel>();
        if (main.RentResidentialListingEditorList.Any(editor => editor.PropertyId == _id && editor.ViewModel is { IsDiscardPending: true }))
        {
            ShowSimpleErrorInfoBar("募集物件の変更の破棄を完了してから、もう一度保存してください。");
            return;
        }
        if (HasPendingEditorOperations || main.RentResidentialListingEditorList.Any(editor => editor.PropertyId == _id && editor.ViewModel is { } vm && (vm.HasPendingEditorOperations || vm.IsClosing)))
        {
            ShowSimpleErrorInfoBar("関連する選択、ファイル処理または編集画面の終了が完了してから、もう一度保存してください。");
            return;
        }
        if (!CleanIncompleteAttachments()) return;
        if (!main.TryBeginResidentialOperation(_id))
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
            IsSaving = false;
            //main.EndResidentialOperation(_id);
            OnPropertyChanged(nameof(IsEditorEnabled));
            SaveCommand.NotifyCanExecuteChanged();
            //while (_deferredMessages.TryDequeue(out var message)) message();
            //foreach (var editor in main.RentResidentialListingEditorList.Where(editor => editor.PropertyId == _id).ToList()) editor.ViewModel?.ApplyDeferredMessages();
        }
    }

    private async Task SaveCore()
    {
        // Validate input.
        if (!ValidateName(true))
        {
            //InfoBarErrorMessage = "入力項目に誤りがあります。保存出来ませんでした。";
            //IsInfoBarErrorOpen = true;

            if (!_navigationService.IsCurrentPageSameAs(BasicPageName))
            {
                _navigationService.NavigateTo(BasicPageName, this);
            }

            return;
        }

        if (!ValidatePropertyNumericValues())
        {
            return;
        }

        if (Pdfs.Count(pdf => pdf.IsMain) > 1)
        {
            ShowSimpleErrorInfoBar("メインの図面は1枚だけ選択してください。");
            const string pageName = "ZumenSearch.Views.Rent.Residentials.ZumenListPage";
            if (!_navigationService.IsCurrentPageSameAs(pageName)) _navigationService.NavigateTo(pageName, this);
            return;
        }
        /*
        var pendingListings = _building.Status == EntityStatus.New ? _pendingListingDrafts.Values.ToList() : [];
        var deletedListingIds = _stagedListingDeletions.ToArray();
        var listingWithMultipleMainPdfs = pendingListings.FirstOrDefault(room => room.Pdfs.Count(pdf => pdf.IsMain) > 1);
        if (listingWithMultipleMainPdfs is not null)
        {
            ShowInputValidationError($"募集物件「{listingWithMultipleMainPdfs.Name}」のメインの図面は1枚だけ選択してください。募集物件の編集画面で修正してください。");
            const string pageName = "ZumenSearch.Views.Rent.Residentials.RoomListPage";
            if (!_navigationService.IsCurrentPageSameAs(pageName)) _navigationService.NavigateTo(pageName, this);
            return;
        }
        if (App.GetService<ViewModels.MainViewModel>().RentResidentialListingEditorList.Any(editor => deletedListingIds.Contains(editor.Id, StringComparer.Ordinal)))
        {
            ShowInputValidationError("削除する募集物件の編集画面を閉じてから保存してください。");
            return;
        }

        var lessorsBeforeSave = LessorsWrapper.ToList();
        var buildingLessorsBeforeSave = _building.Lessors.ToList();
        var buildingBrokersBeforeSave = _building.Brokers.ToList();
        var buildingLessorDeletesBeforeSave = _building.LessorsToBeDeleted.ToList();
        var buildingBrokerDeletesBeforeSave = _building.BrokersToBeDeleted.ToList();
        var buildingThumbnailBeforeSave = _building.ThumbnailFilename;
        */
        if (!SetValues())
        {
            //_building.SetIsUnitLevelOwnership(_committedUnitOwnership);
            return;
        }

        var saveResult = false;
        Models.SearchResult.ResultWrapper resInsert;
        try
        {
            resInsert = await Task.Run(() => _dataAccessService.UpsertRentResidential(_building), _cts.Token);//, pendingListings, deletedListingIds
            saveResult = !resInsert.IsError;
        }
        finally
        {
            if (!saveResult)
            {
                /*
                _building.SetIsUnitLevelOwnership(_committedUnitOwnership);
                _building.ThumbnailFilename = buildingThumbnailBeforeSave;
                LessorsWrapper.Clear();
                foreach (var lessor in lessorsBeforeSave) LessorsWrapper.Add(lessor);
                _building.Lessors.Clear();
                foreach (var lessor in buildingLessorsBeforeSave) _building.Lessors.Add(lessor);
                _building.Brokers.Clear();
                foreach (var broker in buildingBrokersBeforeSave) _building.Brokers.Add(broker);
                _building.LessorsToBeDeleted.Clear();
                foreach (var lessor in buildingLessorDeletesBeforeSave) _building.LessorsToBeDeleted.Add(lessor);
                _building.BrokersToBeDeleted.Clear();
                foreach (var broker in buildingBrokerDeletesBeforeSave) _building.BrokersToBeDeleted.Add(broker);
                */
            }
        }
        if (!saveResult)
        {
            Debug.WriteLine(
                resInsert.Error.Title + Environment.NewLine +
                resInsert.Error.Message + Environment.NewLine +
                resInsert.Error.Description + Environment.NewLine +
                resInsert.Error.Operation + Environment.NewLine +
                resInsert.Error.MethodName + Environment.NewLine +
                resInsert.Error.FullDump);

            ShowErrorInfoBar(resInsert.Error, true);
            return;
        }
        else
        {
            Debug.WriteLine("No errors on update.");
        }

        if (saveResult)
        {
            IsDirty = false;

            _building.SetIsModified(false);
            _building.SetStatus(EntityStatus.Saved);

            // Update title with dummy value.
            WindowTitle = string.Empty;

            // Clear error infobar.
            IsInfoBarErrorOpen = false;

            // Clean up deleted picture file.
            if (_building.PicturesToBeDeleted.Count > 0)
            {
                foreach (var file in _building.PicturesToBeDeleted)
                {
                    var delFilePath = System.IO.Path.Combine(App.PropertyBlobDataFolder, _building.Id, file.ImageFilename);
                    File.Delete(delFilePath);
                }
                _building.PicturesToBeDeleted.Clear();
            }

            // Clean up deleted PDF and thumb file.
            if (_building.PdfsToBeDeleted.Count > 0)
            {
                foreach (var file in _building.PdfsToBeDeleted)
                {
                    var delFilePath = System.IO.Path.Combine(App.PropertyBlobDataFolder, _building.Id, file.PdfFilename);
                    File.Delete(delFilePath);
                    delFilePath = System.IO.Path.Combine(App.PropertyBlobDataFolder, _building.Id, file.ThumbnailFilename);
                    File.Delete(delFilePath);
                }
                _building.PdfsToBeDeleted.Clear();
            }

            // Just in case clear LessorsToBeDeleted, BrokersToBeDeleted
            _building.LessorsToBeDeleted.Clear();
            _building.BrokersToBeDeleted.Clear();

            // Clear rooms, pic and pdfs and lessors

            if (_building.ListingsToBeDeleted.Count > 0)
            {
                // TODO: Clean up deleted listing files and directories
                /*
                foreach (var listing in _building.ListingsToBeDeleted)
                {
                    foreach (var listingPic in listing.Pictures)
                    {
                        var delFilePath = System.IO.Path.Combine(App.PropertyBlobDataFolder, _building.Id, listingPic.ImageFilename);
                        File.Delete(delFilePath);
                    }

                    foreach (var listingPdf in listing.Pdfs)
                    {
                        var delFilePath = System.IO.Path.Combine(App.PropertyBlobDataFolder, _building.Id, listingPdf.PdfFilename);
                        File.Delete(delFilePath);
                        delFilePath = System.IO.Path.Combine(App.PropertyBlobDataFolder, _building.Id, listingPdf.ThumbnailFilename);
                        File.Delete(delFilePath);
                    }

                    // Just in case clear LessorsToBeDeleted
                    listing.LessorsToBeDeleted.Clear();
                }
                */
                _building.ListingsToBeDeleted.Clear();
            }

            _unsavedBuildingPictureFileList.Clear();
            _unsavedBuildingPdfThumbnailFileList.Clear();
            _unsavedBuildingPdfFileList.Clear();

            // Just in case.
            _building.SetStatus(EntityStatus.Saved);
            _building.SetIsModified(false);

            // Clear unsaved rooms and set status to saved.
            foreach (var room in _unsavedListingList)
            {
                room.SetPropertyName(Name);
                room.SetPropertyStatus(EntityStatus.Saved);
                room.SetStatus(EntityStatus.Saved);
                room.SetIsModified(false);
            }
            _unsavedListingList.Clear();

            // Clear unsaved child editors and quit tracking them.
            UnsavedChildEditorList.Clear();

            /*
            foreach (var room in Rooms)
            {
                room.SetPropertyName(Name);
                room.SetPropertyStatus(EntityStatus.Saved);
                room.SetStatus(EntityStatus.Saved);
                room.SetIsModified(false);
            }

            // Just in case.
            foreach (var room in _building.Rooms)
            {
                room.SetPropertyName(Name);
                room.SetPropertyStatus(EntityStatus.Saved);
                room.SetStatus(EntityStatus.Saved);
                room.SetIsModified(false);
            }
            */
            /*
            _building.ThumbnailFilename = _building.ThumbnailFilenameOnLastSave;
            IsDirty = false;
            _committedBuildingPdfs = _building.Pdfs.ToList();
            _committedUnitOwnership = IsUnitLevelOwnership;

            _building.SetIsModified(false);
            _building.SetStatus(EntityStatus.Saved);

            // Update title with dummy value.
            WindowTitle = string.Empty;

            // Clear error infobar.
            IsInfoBarErrorOpen = false;

            var mainViewModel = App.GetService<ViewModels.MainViewModel>();
            foreach (var listingId in deletedListingIds)
            {
                mainViewModel.RemoveResidentialListingSearchResults(_building.Id, listingId);
                _stagedListingDeletions.Remove(listingId);
                _pendingListingDirectoryCleanup.Add(listingId);
                WeakReferenceMessenger.Default.Send(new ListingDeletedMessage(listingId));
            }
            foreach (var listingId in _stagedPendingListingDeletions) _pendingListingDirectoryCleanup.Add(listingId);
            _stagedPendingListingDeletions.Clear();

            var cleanupFailures = new List<string>();
            foreach (var file in _building.PicturesToBeDeleted.ToList())
            {
                var picturePath = Path.Combine(App.PropertyBlobDataFolder, _building.Id, file.ImageFilename);
                if (TryDeleteFiles(new[] { picturePath }, cleanupFailures))
                {
                    _building.PicturesToBeDeleted.Remove(file);
                }
            }

            foreach (var file in _building.PdfsToBeDeleted.ToList())
            {
                var pdfPath = Path.Combine(App.PropertyBlobDataFolder, _building.Id, file.PdfFilename);
                var thumbnailPath = Path.Combine(App.PropertyBlobDataFolder, _building.Id, file.ThumbnailFilename);
                if (TryDeleteFiles(new[] { pdfPath, thumbnailPath }, cleanupFailures))
                {
                    _building.PdfsToBeDeleted.Remove(file);
                }
            }

            foreach (var draft in pendingListings)
            {
                if (_supersededDraftFiles.Remove(draft.Id, out var superseded))
                {
                    foreach (var path in superseded)
                    {
                        if (!_pendingListingPdfCleanupFiles.Contains(path, StringComparer.OrdinalIgnoreCase)) _pendingListingPdfCleanupFiles.Add(path);
                    }
                }
                var directory = Path.Combine(_propertyDataDirectoryPath, draft.Id);
                var paths = draft.PicturesToBeDeleted.Select(picture => Path.Combine(directory, picture.ImageFilename)).Concat(draft.PdfsToBeDeleted.SelectMany(pdf => new[] { Path.Combine(directory, pdf.PdfFilename), Path.Combine(directory, pdf.ThumbnailFilename) }));
                foreach (var path in paths)
                {
                    if (!_pendingListingPdfCleanupFiles.Contains(path, StringComparer.OrdinalIgnoreCase)) _pendingListingPdfCleanupFiles.Add(path);
                }
                _committedListingDrafts[draft.Id] = draft;
            }
            _pendingListingDrafts.Clear();
            _pendingListingObjects.Clear();
            for (var index = 0; index < Rooms.Count; index++)
            {
                var summary = Rooms[index];
                if (summary.IsPendingDraft) Rooms[index] = new ListingSummary(summary.Id, summary.Name, false);
            }
            CleanupListingFiles(cleanupFailures);
            CleanupListingDirectories(cleanupFailures);

            _unsavedBuildingPictureFileList.Clear();
            _unsavedBuildingPdfThumbnailFileList.Clear();
            _unsavedBuildingPdfFileList.Clear();

            // Just in case.
            _building.SetStatus(EntityStatus.Saved);
            _building.SetIsModified(false);

            _hasPendingFileCleanupAfterSave = cleanupFailures.Count > 0 || _pendingListingPdfCleanupFiles.Count > 0 || _pendingListingDirectoryCleanup.Count > 0;
            if (_hasPendingFileCleanupAfterSave)
            {
                IsDirty = true;
                ShowInputValidationError(cleanupFailures.Count > 0 ? $"保存しましたが、一部の削除済みファイルを削除できませんでした。次回の保存時に再試行します: {string.Join(", ", cleanupFailures)}" : "編集中の募集物件で使用中の添付ファイルがあります。編集画面を保存または閉じてから、建物を再度保存してください。");
            }
            */

            WeakReferenceMessenger.Default.Send(new Models.Messenger.PropertyUpdatedMessage(_building as Models.Base.PropertyBase));
        }
    }
    private bool CanSave()
    {
        return IsDirty && !IsSaving && !_isClosing && !_propertyBusy && !_discardPending;
    }

    #endregion

    #region == Pictures and Pdfs related commands ==

    [RelayCommand(CanExecute = nameof(CanAddNewBuildingPictures))]
    private async Task AddNewBuildingPictures(List<string> filePathList)
    {
        // File open is handleed in the code behind since this ViewModel does not know about the Window which is required for the Picker.

        if (filePathList is null) return;
        if (filePathList.Count == 0) return;

        //Debug.WriteLine($"destDirectory={_building.PropertyDataDirectoryPath}  @SetNewBuildingPicturesAsync()");

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

            string extension = Path.GetExtension(filePath);
            if (!IsSupportedImageExtension(extension))
            {
                ShowSimpleErrorInfoBar($"画像形式に対応していません: {Path.GetFileName(filePath)}");
                continue;
            }

            using var sourceStream = File.Open(filePath, FileMode.Open);

            string newId = Guid.CreateVersion7().ToString("N");
            string newFilename = newId + extension;
            var destFilePath = Path.Combine(_propertyDataDirectoryPath, newFilename);
            //Debug.WriteLine($"{file} to {destFilePath}  @SetNewBuildingPicturesAsync()");

            using var destinationStream = new FileStream(destFilePath, FileMode.CreateNew, FileAccess.Write);
            //_incompleteAttachmentFiles.Add(destFilePath);
            await sourceStream.CopyToAsync(destinationStream);

            var pic = new Models.Rent.Residentials.PropertyPicture(newId, newFilename, EntityStatus.New)
            {
                BasePath = _propertyDataDirectoryPath,//System.IO.Path.Combine(App.PropertyBlobDataFolder, _building.Id),
                ParentViewModel = this
            };

            Pictures.Add(pic);

            OpenBuildingBlobDirectoryCommand.NotifyCanExecuteChanged();
            DeleteBuildingPictureCommand.NotifyCanExecuteChanged();

            IsDirty = true;

            // Keep track of unsaved files to delete them when discarding.
            _unsavedBuildingPictureFileList.Add(destFilePath);
            //_incompleteAttachmentFiles.Remove(destFilePath);
        }
    }
    private static bool CanAddNewBuildingPictures(List<string> filePathList)
    {
        if (filePathList.Count < 1)
        {
            return false;
        }

        return true;
    }

    private static bool IsSupportedImageExtension(string extension) =>
        extension.Equals(".jpg", StringComparison.OrdinalIgnoreCase) ||
        extension.Equals(".jpeg", StringComparison.OrdinalIgnoreCase) ||
        extension.Equals(".png", StringComparison.OrdinalIgnoreCase) ||
        extension.Equals(".gif", StringComparison.OrdinalIgnoreCase) ||
        extension.Equals(".webp", StringComparison.OrdinalIgnoreCase);

    [RelayCommand(CanExecute = nameof(CanDeleteBuildingPicture))]
    private async Task DeleteBuildingPicture(Models.Rent.Residentials.PropertyPicture picBldg)
    {
        if (picBldg is null)
        {
            return;
        }

        if (await _dialogService.ShowDeleteConfirmationDialog("この建物写真を削除しますか？") != Microsoft.UI.Xaml.Controls.ContentDialogResult.Primary)
        {
            return;
        }

        if (Pictures.Remove(picBldg))
        {
            _building.PicturesToBeDeleted.Add(picBldg);
            IsDirty = true;
        }
    }
    private static bool CanDeleteBuildingPicture(Models.Rent.Residentials.PropertyPicture picBldg)
    {
        return picBldg is not null;
    }

    [RelayCommand(CanExecute = nameof(CanAddNewBuildingPdfs))]
    private async Task AddNewBuildingPdfs(List<string> filePathList)
    {
        if (filePathList is null) return;
        if (filePathList.Count == 0 || IsUnitLevelOwnership) return;
        //var ownershipVersion = _ownershipVersion;

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

            string extension = Path.GetExtension(filePath);
            if (!extension.Equals(".pdf", StringComparison.OrdinalIgnoreCase))
            {
                ShowSimpleErrorInfoBar($"PDFファイルを選択してください: {Path.GetFileName(filePath)}");
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
                    var thumbnailDestFilePath = Path.Combine(_propertyDataDirectoryPath, newThumbnailFilename);

                    using (var destinationStream = new FileStream(thumbnailDestFilePath, FileMode.CreateNew, FileAccess.Write))
                    {
                        //_incompleteAttachmentFiles.Add(thumbnailDestFilePath);
                        importedPaths.Add(thumbnailDestFilePath);
                        using var managedSourceStream = stream.AsStreamForRead();
                        await managedSourceStream.CopyToAsync(destinationStream);
                    }

                    string newFilename = newId + extension;
                    var pdfDestFilePath = Path.Combine(_propertyDataDirectoryPath, newFilename);
                    using (var pdfSource = File.OpenRead(filePath))
                    using (var pdfDestination = new FileStream(pdfDestFilePath, FileMode.CreateNew, FileAccess.Write))
                    {
                        //_incompleteAttachmentFiles.Add(pdfDestFilePath);
                        importedPaths.Add(pdfDestFilePath);
                        await pdfSource.CopyToAsync(pdfDestination);
                    }

                    if (IsUnitLevelOwnership)
                    {
                        _unsavedBuildingPdfThumbnailFileList.Add(thumbnailDestFilePath);
                        _unsavedBuildingPdfFileList.Add(pdfDestFilePath);
                        IsDirty = true;
                        continue;
                    }

                    var pdf = new Models.Rent.Residentials.PropertyPdf(newId, newFilename, newThumbnailFilename, EntityStatus.New)
                    {
                        BasePath = _propertyDataDirectoryPath,
                        ParentViewModel = this
                    };

                    Pdfs.Add(pdf);
                    _unsavedBuildingPdfThumbnailFileList.Add(thumbnailDestFilePath);
                    _unsavedBuildingPdfFileList.Add(pdfDestFilePath);
                    OpenBuildingBlobDirectoryCommand.NotifyCanExecuteChanged();
                    DeleteBuildingPdfCommand.NotifyCanExecuteChanged();
                    IsDirty = true;
                }
                else
                {
                    ShowSimpleErrorInfoBar($"PDFにページがありません: {Path.GetFileName(filePath)}");
                }
            }
            catch (Exception ex) when (ex is IOException or UnauthorizedAccessException or COMException or InvalidOperationException or ArgumentException)
            {
                /*
                if (ownershipVersion != _ownershipVersion || IsUnitLevelOwnership)
                {
                    foreach (var path in importedPaths)
                    {
                        if (!_pendingBuildingPdfCleanupFiles.Contains(path, StringComparer.Ordinal)) _pendingBuildingPdfCleanupFiles.Add(path);
                        _incompleteAttachmentFiles.Remove(path);
                    }
                }
                */
                Debug.WriteLine($"Failed to import PDF '{filePath}': {ex}");
                ShowSimpleErrorInfoBar($"PDFを読み込めませんでした: {Path.GetFileName(filePath)}。{ex.Message}");
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

    [RelayCommand]
    private async Task OpenBuildingPdf(Models.Rent.Residentials.PropertyPdf pdf)
    {
        if (pdf is null || !Pdfs.Contains(pdf) || !IsEditorEnabled) return;

        try
        {
            var file = await StorageFile.GetFileFromPathAsync(Path.Combine(_propertyDataDirectoryPath, pdf.PdfFilename));
            if (!await Windows.System.Launcher.LaunchFileAsync(file))
            {
                ShowSimpleErrorInfoBar("PDFを開けませんでした。PDFを開くアプリが設定されているか確認してください。");
            }
        }
        catch (Exception ex) when (ex is IOException or UnauthorizedAccessException or COMException or InvalidOperationException or ArgumentException)
        {
            Debug.WriteLine($"Failed to open residential property PDF: {ex}");
            ShowSimpleErrorInfoBar($"PDFを開けませんでした: {ex.Message}");
        }
    }

    [RelayCommand(CanExecute = nameof(CanDeleteBuildingPdf))]
    private async Task DeleteBuildingPdf(Models.Rent.Residentials.PropertyPdf pdfBldg)
    {
        if (pdfBldg is null)
        {
            return;
        }

        if (await _dialogService.ShowDeleteConfirmationDialog("この建物図面を削除しますか？") != Microsoft.UI.Xaml.Controls.ContentDialogResult.Primary)
        {
            return;
        }

        if (Pdfs.Remove(pdfBldg))
        {
            _building.PdfsToBeDeleted.Add(pdfBldg);
            IsDirty = true;
        }
    }
    private static bool CanDeleteBuildingPdf(Models.Rent.Residentials.PropertyPdf pdfBldg)
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
                Debug.WriteLine($"Error opening folder: {ex}");
                ShowSimpleErrorInfoBar($"フォルダーを開けませんでした。{ex.Message}");
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

    #region == Rooms related commands ==

    // Add New Modal window command
    [RelayCommand]
    private void AddNewRoom() 
    {
        var newId = Guid.CreateVersion7().ToString("N");
        var editorShell = _shellFactory.Create(new Models.Rent.Residentials.Listing(newId, EntityStatus.New, _building.Id, _building.Status, _building.IsUnitLevelOwnership, Name));

        // Apply the current IsUnitOwnership state because it may not be saved to the _room YET.
        //editorShell.ViewModel.SetIsPropertyUnitOwnership(this.IsUnitOwnership);
        // Use message. If this is set, then show/hide the owner and zumen from shell menu.
        WeakReferenceMessenger.Default.Send(new Models.Messenger.PropertyIsUnitOwnershipChangedMessage(_id, this.IsUnitLevelOwnership));

        var mainVM = App.GetService<ViewModels.MainViewModel>();
        mainVM.RentResidentialListingEditorList.Add(editorShell.Window);

        // Track unsaved child editor windows so that they can be closed when the parent is closed 
        // because the building/property is not saved yet, the child editor windows/listing is dependent of this building/property.
        if (_building.Status == EntityStatus.New)
        {
            this.UnsavedChildEditorList.Add(editorShell.Window);
        }

        if (editorShell.Window.AppWindow.Presenter is OverlappedPresenter presenter)
        {
            presenter.IsResizable = true;
            presenter.IsModal = false;
            presenter.IsAlwaysOnTop = false;
            presenter.PreferredMinimumWidth = 1274;
            presenter.PreferredMinimumHeight = 794;
        }

        //var dpi = Windows.Win32.PInvoke.GetDpiForWindow(new Windows.Win32.Foundation.HWND(WinRT.Interop.WindowNative.GetWindowHandle(this)));
        //var scalingFactor = (float)dpi / 96;
        //AppWindow.Resize(new Windows.Graphics.SizeInt32((int)(400.0f * scalingFactor), (int)(300.0f * scalingFactor)));

        editorShell.Window.AppWindow.MoveAndResize(new Windows.Graphics.RectInt32(mainVM.RentResidentialListingEditorWinLeft, mainVM.RentResidentialListingEditorWinTop, mainVM.RentResidentialListingEditorWinWidth, mainVM.RentResidentialListingEditorWinHeight));

        //editorWindow.AppWindow.Show();
        editorShell.Window.Activate();
        editorShell.Window.AppWindow.MoveInZOrderAtTop();
    }

    [RelayCommand(CanExecute = nameof(EditSelectedRoomCanExecute))]
    private async Task EditSelectedRoom(ListingSummary summary)
    {
        if (summary is null || !ListingSummaries.Contains(summary) || !IsEditorEnabled) return;
        var mainVM = App.GetService<ViewModels.MainViewModel>();
        if (!mainVM.TryBeginResidentialOperation(_id))
        {
            ShowSimpleErrorInfoBar("関連する保存、削除または読み込みが完了してから、もう一度操作してください。");
            return;
        }
        try
        {
            await EditSelectedRoomCore(summary);
        }
        finally
        {
            mainVM.EndResidentialOperation(_id);
        }
    }

    private async Task EditSelectedRoomCore(ListingSummary summary)
    {
        var unitId = summary.Id;
        var mainVM = App.GetService<ViewModels.MainViewModel>();

        // Check if the selected item is already being edited in another window.
        foreach (var editWin in mainVM.RentResidentialListingEditorList.ToList())
        {
            if (editWin.Id != unitId)
            {
                continue;
            }

            //Debug.WriteLine($"Editor window for {unitId} is already open. Activating it.");

            try
            {
                editWin.Activate();

                if (editWin.Content is Views.Rent.Residentials.Listing.ShellPage editShell)
                {
                    editShell.Window?.AppWindow.MoveInZOrderBelow(editWin.AppWindow.Id);
                }

                editWin.AppWindow.MoveInZOrderAtTop();

                //isFound = true;

                return;
            }
            catch (COMException)
            {
                ShowSimpleErrorInfoBar("募集物件の編集画面を開けませんでした。既存の編集画面を確認してから再試行してください。");

                WeakReferenceMessenger.Default.Send(new WindowClosedMessage<Microsoft.UI.Xaml.Window>(editWin));

                return;
            }
        }

        Models.Rent.Residentials.Listing room;
        /*
        if (_pendingListingDrafts.TryGetValue(unitId, out var draft))
        {
            if (!_pendingListingObjects.TryGetValue(unitId, out var retained))
            {
                ShowSimpleErrorInfoBar("募集物件の下書きを復元できません。建物を保存してから再試行してください。");
                return;
            }
            room = draft.ToEditorModel(retained.Pictures, retained.Pdfs, retained.Lessors, retained.Brokers);
        }
        else
        {
            var result = await Task.Run(() => _dataAccessService.SelectRentResidentialListingById(_id, unitId), _cts.Token);
            if (result.IsError)
            {
                ShowErrorInfoBar(result.Error, true);
                return;
            }
            if (result.Room is null)
            {
                ShowSimpleErrorInfoBar("募集物件が見つかりませんでした。再読み込みしてください。");
                return;
            }
            room = result.Room;
        }
        */
        var result = await Task.Run(() => _dataAccessService.SelectRentResidentialListingById(_id, unitId), _cts.Token);
        if (result.IsError)
        {
            ShowErrorInfoBar(result.Error, true);
            return;
        }
        if (result.Room is null)
        {
            ShowSimpleErrorInfoBar("募集物件が見つかりませんでした。再読み込みしてください。");
            return;
        }
        room = result.Room;
        var editorShell = _shellFactory.Create(room);

        // Apply the current IsUnitOwnership state because it may not be saved to the _room YET.
        //editorShell.ViewModel.SetIsPropertyUnitOwnership(this.IsUnitOwnership);
        // Use message. If this is set, then show/hide the owner and zumen from shell menu.
        WeakReferenceMessenger.Default.Send(new Models.Messenger.PropertyIsUnitOwnershipChangedMessage(_id, this.IsUnitLevelOwnership));

        var editorWindow = editorShell.Window;
        if (editorWindow == null)
        {
            // EditorWin should be initialized in the EditorShell constructor.
            Debug.WriteLine("EditorWin must be initialized in the EditorShell constructor");
            return;
        }

        mainVM.RentResidentialListingEditorList.Add(editorWindow);

        // Let's not add it to the ChildEditorList because it is already saved which means it's got a own persistent id and can be saved independently.
        //this.ChildEditorList.Add(editorWindow);

        editorWindow.AppWindow.MoveAndResize(new Windows.Graphics.RectInt32(mainVM.RentResidentialListingEditorWinLeft, mainVM.RentResidentialListingEditorWinTop, mainVM.RentResidentialListingEditorWinWidth, mainVM.RentResidentialListingEditorWinHeight));
        if (editorWindow.AppWindow.Presenter is OverlappedPresenter presenter)
        {
            presenter.IsResizable = true;
            presenter.IsModal = false;
            presenter.IsAlwaysOnTop = false;
            presenter.PreferredMinimumWidth = 1274;
            presenter.PreferredMinimumHeight = 794;
        }

        //editorWindow.AppWindow.Show();
        editorWindow.Activate();

        if (editorWindow.Content is Views.Rent.Residentials.Listing.ShellPage shell)
        {
            shell.Window?.AppWindow.MoveInZOrderBelow(editorWindow.AppWindow.Id);
        }
        editorWindow.AppWindow.MoveInZOrderAtTop();

    }
    public static bool EditSelectedRoomCanExecute(ListingSummary room)
    {
        if (room is null) return false;
        return true;
    }

    [RelayCommand(CanExecute = nameof(DupeSelectedRoomCanExecute))]
    private void DupeSelectedRoom(ListingSummary room)
    {
        if (room is null) return;
    }
    public static bool DupeSelectedRoomCanExecute(ListingSummary room)
    {
        if (room is null) return false;
        return true;
    }

    [RelayCommand(CanExecute = nameof(DeleteSelectedRoomCanExecute))]
    private async Task DeleteSelectedRoom(ListingSummary room)
    {
        if (room is null)
        {
            return;
        }

        // 
        var mainVM = App.GetService<ViewModels.MainViewModel>();

        // Check if the selected item is already being edited in another window.
        foreach (var editWin in mainVM.RentResidentialListingEditorList.ToList())
        {
            if (editWin.Id != room.Id)
            {
                continue;
            }

            //Debug.WriteLine($"Editor window for {unitId} is already open. Activating it.");

            try
            {
                InfoBarErrorMessage = "募集物件の編集画面を閉じてから、建物を削除してください。";
                IsInfoBarErrorOpen = true;

                editWin.Activate();

                if (editWin.Content is Views.Rent.Residentials.Listing.ShellPage editShell)
                {
                    editShell.Window?.AppWindow.MoveInZOrderBelow(editWin.AppWindow.Id);
                }

                editWin.AppWindow.MoveInZOrderAtTop();

                //isFound = true;

                return;
            }
            catch (COMException)
            {
                WeakReferenceMessenger.Default.Send(new WindowClosedMessage<Microsoft.UI.Xaml.Window>(editWin));
            }
        }

        if (!mainVM.TryBeginResidentialOperation(_id))
        {
            ShowSimpleErrorInfoBar("関連する保存、削除または読み込みが完了してから、もう一度操作してください。");
            return;
        }
        try
        {
            if (await _dialogService.ShowDeleteConfirmationDialog("この募集物件を削除しますか？") != Microsoft.UI.Xaml.Controls.ContentDialogResult.Primary) return;
            if (mainVM.RentResidentialListingEditorList.FirstOrDefault(editor => editor.Id == room.Id) is { } openedEditor)
            {
                ShowSimpleErrorInfoBar("募集物件の編集画面を閉じてから削除してください。");
                openedEditor.Activate();
                return;
            }

            if (ListingSummaries.Remove(room))
            {
                // Don't delete directly. Just mark it for deletion. It will be deleted when the property is saved.
                //if (_building.Rooms.Remove(room)) { }
                _building.ListingsToBeDeleted.Add(room.Id);

                /*
                if (room.IsPendingDraft) _stagedPendingListingDeletions.Add(room.Id);
                else _stagedListingDeletions.Add(room.Id);
                */
                _building.ListingSummaries.Remove(room);
                /*
                _pendingListingDrafts.Remove(room.Id);
                _pendingListingObjects.Remove(room.Id);
                _supersededDraftFiles.Remove(room.Id);
                */
                IsDirty = true;
            }
        }
        finally
        {
            mainVM.EndResidentialOperation(_id);
        }
    }
    public static bool DeleteSelectedRoomCanExecute(ListingSummary room)
    {
        if (room is null) return false;
        return true;
    }


    #endregion

    #region == Location related commands ==

    [RelayCommand(CanExecute = nameof(CanShowGoogleMaps))]
    public async Task ShowGoogleMaps()
    {
        if (string.IsNullOrEmpty(LocationLatitude) || string.IsNullOrEmpty(LocationLongitude))
        {
            return;
        }

        var uriGoogleMaps = new Uri($"https://maps.google.co.jp/?q={LocationLatitude},{LocationLongitude}");
        await Launcher.LaunchUriAsync(uriGoogleMaps);

    }
    private bool CanShowGoogleMaps()
    {
        if (string.IsNullOrEmpty(LocationLatitude) || string.IsNullOrEmpty(LocationLongitude))
        {
            return false;
        }

        return true;
    }

    #endregion

    #region == Transportation related commands ==

    // one
    [RelayCommand]
    public async Task ShowRailLineSelect1()
    {
        if (_dialogService is null)
        {
            return;
        }

        var railLine = await _dialogService.ShowRailLineSelectDialog();

        if (railLine is not null && !string.Equals(SelectedRailLine1.LineCode, railLine.LineCode, StringComparison.Ordinal))
        {
            SelectedRailLine1 = railLine;
        }
    }

    [RelayCommand(CanExecute = nameof(CanShowRailStationSelect1))]
    public async Task ShowRailStationSelect1()
    {
        if (_dialogService is null)
        {
            return;
        }

        if (SelectedRailLine1 is null)
        {
            return;
        }

        var lineCode = SelectedRailLine1.LineCode;
        var railStation = await _dialogService.ShowRailStationSelectDialog(lineCode);

        if (railStation is not null && string.Equals(SelectedRailLine1.LineCode, lineCode, StringComparison.Ordinal) && string.Equals(railStation.LineCode, lineCode, StringComparison.Ordinal))
        {
            SelectedRailStation1 = railStation;
        }
    }
    private bool CanShowRailStationSelect1()
    {
        if (SelectedRailLine1 is null)
        {
            return false;
        }

        return true;
    }

    // two
    [RelayCommand]
    public async Task ShowRailLineSelect2()
    {
        if (_dialogService is null)
        {
            return;
        }

        var railLine = await _dialogService.ShowRailLineSelectDialog();

        if (railLine is not null && !string.Equals(SelectedRailLine2.LineCode, railLine.LineCode, StringComparison.Ordinal))
        {
            SelectedRailLine2 = railLine;
        }
    }

    [RelayCommand(CanExecute = nameof(CanShowRailStationSelect2))]
    public async Task ShowRailStationSelect2()
    {
        if (_dialogService is null)
        {
            return;
        }

        if (SelectedRailLine2 is null)
        {
            return;
        }

        var lineCode = SelectedRailLine2.LineCode;
        var railStation = await _dialogService.ShowRailStationSelectDialog(lineCode);

        if (railStation is not null && string.Equals(SelectedRailLine2.LineCode, lineCode, StringComparison.Ordinal) && string.Equals(railStation.LineCode, lineCode, StringComparison.Ordinal))
        {
            SelectedRailStation2 = railStation;
        }
    }
    private bool CanShowRailStationSelect2()
    {
        if (SelectedRailLine2 is null)
        {
            return false;
        }

        return true;
    }

    // three
    [RelayCommand]
    public async Task ShowRailLineSelect3()
    {
        if (_dialogService is null)
        {
            return;
        }

        var railLine = await _dialogService.ShowRailLineSelectDialog();

        if (railLine is not null && !string.Equals(SelectedRailLine3.LineCode, railLine.LineCode, StringComparison.Ordinal))
        {
            SelectedRailLine3 = railLine;
        }
    }

    [RelayCommand(CanExecute = nameof(CanShowRailStationSelect3))]
    public async Task ShowRailStationSelect3()
    {
        if (_dialogService is null)
        {
            return;
        }

        if (SelectedRailLine3 is null)
        {
            return;
        }

        var lineCode = SelectedRailLine3.LineCode;
        var railStation = await _dialogService.ShowRailStationSelectDialog(lineCode);

        if (railStation is not null && string.Equals(SelectedRailLine3.LineCode, lineCode, StringComparison.Ordinal) && string.Equals(railStation.LineCode, lineCode, StringComparison.Ordinal))
        {
            SelectedRailStation3 = railStation;
        }
    }
    private bool CanShowRailStationSelect3()
    {
        if (SelectedRailLine3 is null)
        {
            return false;
        }

        return true;
    }

    // four
    [RelayCommand]
    public async Task ShowRailLineSelect4()
    {
        if (_dialogService is null)
        {
            return;
        }

        var railLine = await _dialogService.ShowRailLineSelectDialog();

        if (railLine is not null && !string.Equals(SelectedRailLine4.LineCode, railLine.LineCode, StringComparison.Ordinal))
        {
            SelectedRailLine4 = railLine;
        }
    }

    [RelayCommand(CanExecute = nameof(CanShowRailStationSelect4))]
    public async Task ShowRailStationSelect4()
    {
        if (_dialogService is null)
        {
            return;
        }

        if (SelectedRailLine4 is null)
        {
            return;
        }

        var lineCode = SelectedRailLine4.LineCode;
        var railStation = await _dialogService.ShowRailStationSelectDialog(lineCode);

        if (railStation is not null && string.Equals(SelectedRailLine4.LineCode, lineCode, StringComparison.Ordinal) && string.Equals(railStation.LineCode, lineCode, StringComparison.Ordinal))
        {
            SelectedRailStation4 = railStation;
        }
    }
    private bool CanShowRailStationSelect4()
    {
        if (SelectedRailLine4 is null)
        {
            return false;
        }

        return true;
    }


    #endregion

    #region == Lessor related commands ==

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
                ShowSimpleErrorInfoBar("選択した貸主を読み込めませんでした。");
                return;
            }


            var asdf = new Models.Rent.Residentials.PersonWrapperForPropertyViewModel(res.Person, this);

            foreach (var deleted in _building.LessorsToBeDeleted.Where(person => person.Id == res.Person.Id).ToList()) _building.LessorsToBeDeleted.Remove(deleted);
            LessorsWrapper.Add(asdf);

            IsDirty = true;
        }

    }

    [RelayCommand(CanExecute = nameof(CanEditLessor))]
    private void EditLessor(Models.Rent.Residentials.PersonWrapperForPropertyViewModel lessor)
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
    private static bool CanEditLessor(Models.Rent.Residentials.PersonWrapperForPropertyViewModel lessor)
    {
        return lessor is not null;
    }

    [RelayCommand(CanExecute = nameof(CanDeleteLessor))]
    public async Task DeleteLessor(Models.Rent.Residentials.PersonWrapperForPropertyViewModel lessor)
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
            if (!_building.LessorsToBeDeleted.Any(person => person.Id == lessor.Person.Id)) _building.LessorsToBeDeleted.Add(lessor.Person);
        }
    }
    private static bool CanDeleteLessor(Models.Rent.Residentials.PersonWrapperForPropertyViewModel lessor)
    {
        return lessor is not null;
    }

    #endregion

    #region == Broker related commands ==

    [RelayCommand]
    public async Task AddBroker()
    {
        if (_dialogService is null)
        {
            Debug.WriteLine("_dlgService is null");
            return;
        }

        var broker = await _dialogService.ShowBrokerSelectDialog(new ViewModels.Dialogs.BrokerSelectViewModel(_dataAccessService, _cts));

        if (broker is not null)
        {
            var brokerId = broker.Id;

            // Check if already exists
            var match = BrokersWrapper.FirstOrDefault(x => x.Person.Id.Equals(brokerId, StringComparison.Ordinal));
            if (match is not null)
            {
                Debug.WriteLine($"broker {broker.Name} already in the list.");
                return;
            }

            var res = await Task.Run(() => _dataAccessService.SelectBrokerById(brokerId), _cts.Token);
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
                Debug.WriteLine($"{brokerId} is null. Cannot open editor.");
                ShowSimpleErrorInfoBar("選択した宅建業者を読み込めませんでした。");
                return;
            }

            var wrapper = new Models.Rent.Residentials.PersonWrapperForPropertyViewModel(res.Person, this);

            foreach (var deleted in _building.BrokersToBeDeleted.Where(person => person.Id == res.Person.Id).ToList()) _building.BrokersToBeDeleted.Remove(deleted);
            BrokersWrapper.Add(wrapper);

            IsDirty = true;
        }
    }

    [RelayCommand(CanExecute = nameof(CanEditBroker))]
    private void EditBroker(Models.Rent.Residentials.PersonWrapperForPropertyViewModel broker)
    {
        if (broker.Person is not null)
        {
            var mainVm = App.GetService<MainViewModel>();
            if (mainVm.EditBrokerCommand.CanExecute(broker.Person as Models.Base.PersonBase))
            {
                mainVm.EditBrokerCommand.Execute(broker.Person as Models.Base.PersonBase);
            }
        }
    }
    private static bool CanEditBroker(Models.Rent.Residentials.PersonWrapperForPropertyViewModel broker)
    {
        return broker is not null;
    }

    [RelayCommand(CanExecute = nameof(CanDeleteBroker))]
    public async Task DeleteBroker(Models.Rent.Residentials.PersonWrapperForPropertyViewModel broker)
    {
        if (broker is null)
        {
            return;
        }

        if (broker.Person is null)
        {
            return;
        }

        Debug.WriteLine($"DeleteBrokerCommand {broker.Person.Name}");

        if (BrokersWrapper.Remove(broker))
        {
            IsDirty = true;
            if (!_building.BrokersToBeDeleted.Any(person => person.Id == broker.Person.Id)) _building.BrokersToBeDeleted.Add(broker.Person);
        }
    }
    private static bool CanDeleteBroker(Models.Rent.Residentials.PersonWrapperForPropertyViewModel broker)
    {
        return broker is not null;
    }

    #endregion

    #endregion

}
