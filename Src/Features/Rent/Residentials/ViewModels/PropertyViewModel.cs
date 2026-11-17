using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.Messaging;
using Microsoft.UI.Windowing;
using Microsoft.UI.Xaml.Controls;
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

/*
 * TODO: 
 * Gather validation logic in one place. Currently, validation is scattered across various methods. Make sure it is consistent with ListingViewModel.
 * Send message to the MainViewModel when some error occurs while closing the window. MainViewModel should show a infobar with the error message.
 * Change sevirity to Error for simple error messages.
 * Add try catch to async and file IO operations and show error message in infobar.
 * Add IsBusy flag to disable the UI commands when some async or IO operations is in progress.
 */

public sealed partial class PropertyViewModel : ObservableRecipient, IRecipient<ListingUpdatedMessage>, IRecipient<WindowClosedMessage<Microsoft.UI.Xaml.Window>>, IRecipient<ListingDeletedMessage>, IRecipient<LessorUpdatedMessage>, IRecipient<BrokerUpdatedMessage>, IRecipient<LessorDeletedMessage>, IRecipient<BrokerDeletedMessage>, IDisposable
{
    #region == Private variables and const ==

    private const string BasicPageName = "ZumenSearch.Views.Rent.Residentials.BasicPage";

    private readonly string _id = string.Empty;

    // This property holds the COPY of current entity being edited.
    // Do not use it directly in the UI. Apply changes to this object in Save() to save the changes.
    private readonly Models.Rent.Residentials.Property _property;

    private readonly string _propertyDataDirectoryPath = string.Empty;

    // Tmp file list to hold unsaved picture files. (if entry is not saved, delete on close)
    private readonly List<string> _unsavedPropertyPictureFileList = [];
    private readonly List<string> _unsavedPropertyPdfFileList = [];
    private readonly List<string> _unsavedPropertyPdfThumbnailFileList = [];

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

    private readonly IAbstractFactory<Models.Rent.Residentials.Listing, bool, Views.Rent.Residentials.Listing.ShellPage> _shellFactory;
    private readonly IDataAccessService _dataAccessService;
    private readonly IDataAccessLocationService _dataAccessLocationService;
    private readonly IDispatcherService _dispatcherService;
    private readonly IDialogGenericService _dialogService;
    private readonly INavigationGenericService _navigationService;

    #endregion

    public PropertyViewModel(Models.Rent.Residentials.Property property, bool isDuplicate, INavigationGenericService navigationService, IDialogGenericService dialogService, IAbstractFactory<Models.Rent.Residentials.Listing, bool, Views.Rent.Residentials.Listing.ShellPage> shellFactory, IDispatcherService dispatcherService, IDataAccessService dataAccessService, IDataAccessLocationService dataAccessLocationService)
    {
        _property = property;
        _id = property.Id;

        _navigationService = navigationService;
        _dialogService = dialogService;
        _shellFactory = shellFactory;
        _dispatcherService = dispatcherService;
        _dataAccessService = dataAccessService;
        _dataAccessLocationService = dataAccessLocationService;

        IsUnitLevelOwnership = property.IsUnitLevelOwnership;

        _transportationEntries = [new(this, 0), new(this, 1), new(this, 2), new(this, 3)];

        _propertyDataDirectoryPath = System.IO.Path.Combine(App.PropertyBlobDataFolder, _id);

        if (!Directory.Exists(_propertyDataDirectoryPath))
        {
            Directory.CreateDirectory(_propertyDataDirectoryPath);
        }

        // Update title with dummy value.
        WindowTitle = string.Empty;

        try
        {
            PopulateValues(isDuplicate);
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
            IsDirty = isDuplicate;
        }

        // Ready to receive messages.
        WeakReferenceMessenger.Default.Register<ListingUpdatedMessage>(this);
        WeakReferenceMessenger.Default.Register<WindowClosedMessage<Microsoft.UI.Xaml.Window>>(this);
        WeakReferenceMessenger.Default.Register<ListingDeletedMessage>(this);
        WeakReferenceMessenger.Default.Register<LessorUpdatedMessage>(this);
        WeakReferenceMessenger.Default.Register<BrokerUpdatedMessage>(this);
        WeakReferenceMessenger.Default.Register<LessorDeletedMessage>(this);
        WeakReferenceMessenger.Default.Register<BrokerDeletedMessage>(this);
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

            if (_property.Status == EntityStatus.New)
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
    public partial string InfoBarErrorTitle { get; set; } = string.Empty;

    [ObservableProperty]
    public partial InfoBarSeverity InfoBarSeverity { get; set; } = InfoBarSeverity.Warning;

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

                //ValidateName(false);

                // Update title with dummy value.
                WindowTitle = string.Empty;

                // Moved to after the save.
                //_selectedSearchResult?.Name = value; // Update the selected search result's name if it exists
            }
        }
    }

    // 物件種別
    public IReadOnlyCollection<Models.Rent.Residentials.PropertyKindTypeLabel> PropertyKinds { get; set; } = PropertyKindTypeLabel.GetAll();
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

    public Models.Rent.Residentials.PropertyKindTypeLabel SelectedPropertyKind
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

                OpenPropertyBlobDirectoryCommand.NotifyCanExecuteChanged();
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

                OpenPropertyBlobDirectoryCommand.NotifyCanExecuteChanged();
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

    public void Receive(ListingUpdatedMessage message)
    {
        if (DeferMessage(() => Receive(message))) return;
        var something = message.Value;
        if (something is null)
        {
            return;
        }

        if (something is not Models.Rent.Residentials.Listing listing || !listing.PropertyId.Equals(_id, StringComparison.Ordinal))
        {
            return;
        }

        // Get the existing summary for this listing, if it exists, and update or add it to the collection.
        var existingSummary = ListingSummaries.FirstOrDefault(item => item.Id.Equals(listing.Id, StringComparison.Ordinal));
        var summary = new ListingSummary(listing.Id, listing.Name);
        if (existingSummary is null)
        {
            ListingSummaries.Add(summary);
        }
        else
        {
            // TODO: Let's not replace the existing summary object.
            // Instead, we should update the existing summary's properties because it is bound to the UI and replacing it will deselect it in the UI.
            ListingSummaries[ListingSummaries.IndexOf(existingSummary)] = summary;
        }

        // Update the unsaved listing list with the new listing data.
        var existingListing = _unsavedListingList.FirstOrDefault(item => item.Id.Equals(listing.Id, StringComparison.Ordinal));
        if (existingListing is not null)
        {
            Debug.WriteLine($"Updating existing listing {listing.Id} in unsaved listing list.");
            existingListing = listing;
            IsDirty = true;
        }
        else
        {
            if (listing.Status == EntityStatus.New)
            {
                Debug.WriteLine($"Adding new listing {listing.Id} to unsaved listing list.");
                _unsavedListingList.Add(listing);
                IsDirty = true;
            }
            else
            {
                // Remove the child editor window for this listing if it exists
                // because it is no longer new and has been saved and can be saved independently.
                var win = this.UnsavedChildEditorList.FirstOrDefault(w => w is Views.Rent.Residentials.Listing.EditorWindow ew && ew.Id == listing.Id);
                if (win is not null)
                {
                    Debug.WriteLine($"Removing child editor window for listing {listing.Id} because it is no longer new and has been saved.");
                    this.UnsavedChildEditorList.Remove(win);
                }
            }
        }
    }

    public void Receive(LessorUpdatedMessage message)
    {
        if (DeferMessage(() => Receive(message))) return;
        var lessor = message.Value;
        if (lessor is null)
        {
            return;
        }

        var psn = LessorsWrapper.FirstOrDefault(r => r.Person.Id.Equals(lessor.Id, StringComparison.Ordinal));
        if (psn is null) return;

        psn.Person = lessor; //= new PersonWrapperForPropertyViewModel(lessor, this);
    }

    public void Receive(BrokerUpdatedMessage message)
    {
        if (DeferMessage(() => Receive(message))) return;
        var broker = message.Value;
        if (broker is null)
        {
            return;
        }

        var psn = BrokersWrapper.FirstOrDefault(r => r.Person.Id.Equals(broker.Id, StringComparison.Ordinal));
        if (psn is null) return;

        psn.Person = broker; //= new PersonWrapperForPropertyViewModel(lessor, this);
    }

    public void Receive(ListingDeletedMessage message)
    {
        if (DeferMessage(() => Receive(message))) return;
        var listingId = message.Value;
        if (string.IsNullOrEmpty(listingId))
        {
            return;
        }

        var listing = ListingSummaries.FirstOrDefault(r => r.Id.Equals(listingId, StringComparison.Ordinal));
        if (listing is not null) ListingSummaries.Remove(listing);
        var summary = _property.ListingSummaries.FirstOrDefault(ls => ls.Id.Equals(listingId, StringComparison.Ordinal));
        if (summary is not null) _property.ListingSummaries.Remove(summary);

    }

    public void Receive(LessorDeletedMessage message)
    {
        if (DeferMessage(() => Receive(message))) return;
        var lessorId = message.Value;
        if (string.IsNullOrEmpty(lessorId))
        {
            return;
        }

        var psn = LessorsWrapper.FirstOrDefault(r => r.Person.Id.Equals(lessorId, StringComparison.Ordinal));
        if (psn is null) return;
        LessorsWrapper.Remove(psn);

        // Should be auto deleted from the database table thanks to "cascade".
    }

    public void Receive(BrokerDeletedMessage message)
    {
        if (DeferMessage(() => Receive(message))) return;
        var brokerId = message.Value;
        if (string.IsNullOrEmpty(brokerId))
        {
            return;
        }

        var psn = BrokersWrapper.FirstOrDefault(r => r.Person.Id.Equals(brokerId, StringComparison.Ordinal));
        if (psn is null) return;
        BrokersWrapper.Remove(psn);

        // Should be auto deleted from the database table thanks to "cascade".
    }

    public void Receive(WindowClosedMessage<Microsoft.UI.Xaml.Window> message)
    {
        var window = message.Value;

        if (window is null)
        {
            return;
        }

        if (window is Views.Rent.Residentials.Listing.EditorWindow rlwin)
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

    public void SetClosing(bool value)
    {
        _isClosing = value;
        OnPropertyChanged(nameof(IsEditorEnabled));
        SaveCommand.NotifyCanExecuteChanged();
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

        // Need to restore the original values from unsaved _property because IsUnitLevelOwnership change is real-time not save-time update.
        WeakReferenceMessenger.Default.Send(new PropertyIsUnitOwnershipChangedMessage(_id, _property.IsUnitLevelOwnership));

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

        WeakReferenceMessenger.Default.Unregister<ListingUpdatedMessage>(this);
        WeakReferenceMessenger.Default.Unregister<WindowClosedMessage<Microsoft.UI.Xaml.Window>>(this);
        WeakReferenceMessenger.Default.Unregister<ListingDeletedMessage>(this);
        WeakReferenceMessenger.Default.Unregister<LessorUpdatedMessage>(this);
        WeakReferenceMessenger.Default.Unregister<BrokerUpdatedMessage>(this);
        WeakReferenceMessenger.Default.Unregister<LessorDeletedMessage>(this);
        WeakReferenceMessenger.Default.Unregister<BrokerDeletedMessage>(this);

        _cts?.Cancel();
        _cts?.Dispose();

        GC.SuppressFinalize(this);

        Debug.WriteLine($"PropertyViewModel.Dispose() called. Id: {this._id}");
    }

    #endregion

    #region == Private Methods ==

    private void PopulateValues(bool isDuplicate)
    {
        // Basics

        Name = _property.Name;

        if (isDuplicate)
        {
            _property.SetName($"複製_{_property.Name}");
        }

        //SelectedKind = _property.PropertyKind;
        var propertyKindKey = PropertyKinds.FirstOrDefault(k => k.Key == _property.PropertyKind.Key);
        SelectedPropertyKind = propertyKindKey is null ? new(Models.Rent.Residentials.PropertyKindType.Unspecified) : propertyKindKey;
        IsUnitLevelOwnership = _property.IsUnitLevelOwnership;
        var structureKey = StructureTypes.FirstOrDefault(k => k.Key == _property.PropertyStructure.Key);
        SelectedStructureType = structureKey is null ? new(Models.Rent.Residentials.StructureType.Unspecified) : structureKey;

        FloorCountAboveGround = _property.FloorCountAboveGround == 0 ? string.Empty : _property.FloorCountAboveGround.ToString(CultureInfo.InvariantCulture);
        FloorCountBasement = _property.FloorCountBasement == 0 ? string.Empty : _property.FloorCountBasement.ToString(CultureInfo.InvariantCulture);
        TotalUnitCount = _property.TotalUnitCount == 0 ? string.Empty : _property.TotalUnitCount.ToString(CultureInfo.InvariantCulture);
        BuiltYearAndMonth = _property.BuiltYearAndMonth.Year != 1900 ? _property.BuiltYearAndMonth : null;
        FudousanId = _property.FudousanId;
        FudousanIdAdditionalCode = _property.FudousanIdAdditionalCode;
        Remarks = _property.Remarks;

        // Location
        if (!string.IsNullOrEmpty(_property.Address.Prefecture?.MunicipalityCode))
        {
            var hoge = Prefectures.FirstOrDefault<Prefecture>(p => p.MunicipalityCode.Equals(_property.Address.Prefecture.MunicipalityCode, StringComparison.Ordinal));
            if (hoge is not null)
            {
                SelectedPrefecture = hoge;
            }
        }
        if ((Cities is not null) && ((!string.IsNullOrEmpty(_property.Address.CountyAndCity?.County)) || (!string.IsNullOrEmpty(_property.Address.CountyAndCity?.City))))
        {
            foreach (var cty in Cities)
            {
                if (cty.County.Equals(_property.Address.CountyAndCity.County, StringComparison.Ordinal) && cty.City.Equals(_property.Address.CountyAndCity.City, StringComparison.Ordinal))
                {
                    SelectedCity = cty;
                    break;
                }
            }
        }
        if ((Towns is not null) && ((!string.IsNullOrEmpty(_property.Address.WardAndOaza?.Ward)) || (!string.IsNullOrEmpty(_property.Address.WardAndOaza?.Oaza))))
        {
            foreach (var twn in Towns)
            {
                if (twn.Ward.Equals(_property.Address.WardAndOaza.Ward, StringComparison.Ordinal) && twn.Oaza.Equals(_property.Address.WardAndOaza.Oaza, StringComparison.Ordinal))
                {
                    SelectedTown = twn;
                    break;
                }
            }
        }
        if (Chous is not null) // Allow an empty string. //&& (!string.IsNullOrEmpty(_property.LocChoume))
        {
            var hoge = Chous.FirstOrDefault<Choume>(p => p.Chou.Equals(_property.Address.Choume?.Chou, StringComparison.Ordinal));
            if (hoge is not null)
            {
                SelectedChou = hoge;
            }
        }
        Edaban = _property.Address.Edaban;
        PostalCode = _property.Address.PostalCode;
        MachiazaId = _property.Address.MachiazaId; // Must be at the end of the location properties, because it is set and cleared by the other properties.
        LocationLatitude = _property.Address.LocationLatitude;
        LocationLongitude = _property.Address.LocationLongitude;

        // 交通
        SelectedRailLine1.SetLineCode(_property.Train1.RailLine?.LineCode ?? "");
        SelectedRailLine1.SetLineName(_property.Train1.RailLine?.LineName ?? "");
        if (SelectedRailLine1.LineCode.Equals(_property.Train1.RailStation?.LineCode ?? "", StringComparison.Ordinal))
        {
            SelectedRailStation1.SetLineCode(_property.Train1.RailStation?.LineCode ?? "");
            SelectedRailStation1.SetStationCode(_property.Train1.RailStation?.StationCode ?? "");
            SelectedRailStation1.SetStationName(_property.Train1.RailStation?.StationName ?? "");
        }
        EkiToho1 = _property.Train1.EkiToho <= 0 ? "" : _property.Train1.EkiToho.ToString(CultureInfo.InvariantCulture);
        BusStop1 = _property.BusStop1;
        BusJyousya1 = _property.BusJyousya1 == 0 ? string.Empty : _property.BusJyousya1.ToString(CultureInfo.InvariantCulture);
        BusStopToho1 = _property.BusStopToho1 == 0 ? string.Empty : _property.BusStopToho1.ToString(CultureInfo.InvariantCulture);

        SelectedRailLine2.SetLineCode(_property.Train2.RailLine?.LineCode ?? "");
        SelectedRailLine2.SetLineName(_property.Train2.RailLine?.LineName ?? "");
        if (SelectedRailLine2.LineCode.Equals(_property.Train2.RailStation?.LineCode ?? "", StringComparison.Ordinal))
        {
            SelectedRailStation2.SetLineCode(_property.Train2.RailStation?.LineCode ?? "");
            SelectedRailStation2.SetStationCode(_property.Train2.RailStation?.StationCode ?? "");
            SelectedRailStation2.SetStationName(_property.Train2.RailStation?.StationName ?? "");
        }
        EkiToho2 = _property.Train2.EkiToho <= 0 ? "" : _property.Train2.EkiToho.ToString(CultureInfo.InvariantCulture);
        BusStop2 = _property.BusStop2;
        BusJyousya2 = _property.BusJyousya2 == 0 ? string.Empty : _property.BusJyousya2.ToString(CultureInfo.InvariantCulture);
        BusStopToho2 = _property.BusStopToho2 == 0 ? string.Empty : _property.BusStopToho2.ToString(CultureInfo.InvariantCulture);

        SelectedRailLine3.SetLineCode(_property.Train3.RailLine?.LineCode ?? "");
        SelectedRailLine3.SetLineName(_property.Train3.RailLine?.LineName ?? "");
        if (SelectedRailLine3.LineCode.Equals(_property.Train3.RailStation?.LineCode ?? "", StringComparison.Ordinal))
        {
            SelectedRailStation3.SetLineCode(_property.Train3.RailStation?.LineCode ?? "");
            SelectedRailStation3.SetStationCode(_property.Train3.RailStation?.StationCode ?? "");
            SelectedRailStation3.SetStationName(_property.Train3.RailStation?.StationName ?? "");
        }
        EkiToho3 = _property.Train3.EkiToho <= 0 ? "" : _property.Train3.EkiToho.ToString(CultureInfo.InvariantCulture);
        BusStop3 = _property.BusStop3;
        BusJyousya3 = _property.BusJyousya3 == 0 ? string.Empty : _property.BusJyousya3.ToString(CultureInfo.InvariantCulture);
        BusStopToho3 = _property.BusStopToho3 == 0 ? string.Empty : _property.BusStopToho3.ToString(CultureInfo.InvariantCulture);

        SelectedRailLine4.SetLineCode(_property.Train4.RailLine?.LineCode ?? "");
        SelectedRailLine4.SetLineName(_property.Train4.RailLine?.LineName ?? "");
        if (SelectedRailLine4.LineCode.Equals(_property.Train4.RailStation?.LineCode ?? "", StringComparison.Ordinal))
        {
            SelectedRailStation4.SetLineCode(_property.Train4.RailStation?.LineCode ?? "");
            SelectedRailStation4.SetStationCode(_property.Train4.RailStation?.StationCode ?? "");
            SelectedRailStation4.SetStationName(_property.Train4.RailStation?.StationName ?? "");
        }
        EkiToho4 = _property.Train4.EkiToho <= 0 ? "" : _property.Train4.EkiToho.ToString(CultureInfo.InvariantCulture);
        BusStop4 = _property.BusStop4;
        BusJyousya4 = _property.BusJyousya4 == 0 ? string.Empty : _property.BusJyousya4.ToString(CultureInfo.InvariantCulture);
        BusStopToho4 = _property.BusStopToho4 == 0 ? string.Empty : _property.BusStopToho4.ToString(CultureInfo.InvariantCulture);

        // Facilities
        HasAutolock = _property.HasAutolock;
        HasElevator = _property.HasElevator;
        HasSecurityCamera = _property.HasSecurityCamera;
        HasParcelLocker = _property.HasParcelLocker;
        HasElectric = _property.HasElectric;
        var electricKind = ElectricKinds.FirstOrDefault(item => item.Key == _property.PropertyElectricKind);
        if (electricKind is not null) SelectedElectricKind = electricKind;
        ElectricDetail = _property.ElectricDetail;
        HasGas = _property.HasGas;
        GasDetail = _property.GasDetail;
        SelectedGasType = GasTypes.First(item => item.Key == _property.GasType);
        TapWaterDetail = _property.TapWaterDetail;
        SelectedTapWaterType = TapWaterTypes.First(item => item.Key == _property.TapWaterType);
        SewerageDetail = _property.SewerageDetail;
        SelectedSewerageType = SewerageTypes.First(item => item.Key == _property.SewerageType);
        GroundDigital = _property.GroundDigital;
        BsAntenna = _property.BsAntenna;
        CsAntenna = _property.CsAntenna;
        Catv = _property.Catv;
        Usen = _property.Usen;
        Internet = _property.Internet;
        Fiber = _property.Fiber;
        InternetFree = _property.InternetFree;
        TvAntennaDetail = _property.TvAntennaDetail;
        InternetDetail = _property.InternetDetail;
        HasParking = _property.HasParking;
        IsParkingFree = _property.IsParkingFree;
        SelectedParkingContractType = ParkingContractTypes.First(item => item.Key == _property.ParkingContractType);
        ParkingFee = _property.ParkingFee;
        SelectedParkingFeeTaxType = TaxTypes.First(item => item.Key == _property.ParkingFeeTaxType);
        SelectedParkingAvailability = AvailabilityTypes.First(item => item.Key == _property.ParkingAvailability);
        ParkingSpaces = _property.ParkingSpaces;
        ParkingDistance = _property.ParkingDistance;
        ParkingNotes = _property.ParkingNotes;
        HasBicycleParking = _property.HasBicycleParking;
        IsBicycleParkingFree = _property.IsBicycleParkingFree;
        SelectedBicycleParkingAvailability = AvailabilityTypes.First(item => item.Key == _property.BicycleParkingAvailability);
        BicycleParkingFee = _property.BicycleParkingFee;
        SelectedBicycleParkingFeeTaxType = TaxTypes.First(item => item.Key == _property.BicycleParkingFeeTaxType);
        BicycleParkingNotes = _property.BicycleParkingNotes;
        HasMotorcycleParking = _property.HasMotorcycleParking;
        IsMotorcycleParkingFree = _property.IsMotorcycleParkingFree;
        SelectedMotorcycleParkingAvailability = AvailabilityTypes.First(item => item.Key == _property.MotorcycleParkingAvailability);
        MotorcycleParkingFee = _property.MotorcycleParkingFee;
        SelectedMotorcycleParkingFeeTaxType = TaxTypes.First(item => item.Key == _property.MotorcycleParkingFeeTaxType);
        MotorcycleParkingNotes = _property.MotorcycleParkingNotes;

        SelectedKanriShutai = KanriShutais.First(item => item.Key == _property.KanriShutai);
        KanriIsMaintenanceManagementIfIsKanriJisya = _property.KanriIsMaintenanceManagementIfIsKanriJisya;
        KanriIsTenantManagementIfIsKanriJisya = _property.KanriIsTenantManagementIfIsKanriJisya;
        KanriIsPaymentManagementIfIsKanriJisya = _property.KanriIsPaymentManagementIfIsKanriJisya;
        KanriIsOtherManagementIfIsKanriJisya = _property.KanriIsOtherManagementIfIsKanriJisya;
        KanriRemarkIfIsKanriJisya = _property.KanriRemarkIfIsKanriJisya;
        KanriNameOfCompanyIfIsKanriTasya = _property.KanriNameOfCompanyIfIsKanriTasya;
        KanriContactInfoOfCompanyIfIsKanriTasya = _property.KanriContactInfoOfCompanyIfIsKanriTasya;
        KanriRemarkIfIsKanriTasya = _property.KanriRemarkIfIsKanriTasya;
        KanriRemarkIfIsKanriKashinushi = _property.KanriRemarkIfIsKanriKashinushi;

        // Pictures:(don't use directly)
        /*
        Pictures = new ObservableCollection<Models.Rent.Residentials.PropertyPicture>(_property.Pictures); // create a copy.
        foreach (var item in Pictures)
        {
            // Filename to actuall path.
            item.BasePath = System.IO.Path.Combine(App.PropertyBlobDataFolder, _property.Id);
            item.ParentViewModel = this;
            item.SetIsModified(false); // Needed this.
            item.PropertyChanged += OnPropertyPicturePropertyChanged;
        }
        */
        /*
        Pictures.Clear();
        foreach (var item in _property.Pictures)
        {
            var internalItem = new PropertyPicture(item.Id, item.ImageFilename, item.Status)
            {
                IsMain = item.IsMain,
                PictureType = item.PictureType,
                Description = item.Description,
                ThumbImagePath = item.ThumbImagePath,
                BasePath = System.IO.Path.Combine(App.PropertyBlobDataFolder, _property.Id),
                ParentViewModel = this
            };
            internalItem.SetIsModified(false); // Needed this.
            internalItem.PropertyChanged += OnPropertyPicturePropertyChanged;
            Pictures.Add(internalItem);
        }
        */
        Pictures.Clear();
        foreach (var item in _property.Pictures)
        {
            var internalItem = item.Clone();
            internalItem.BasePath = System.IO.Path.Combine(App.PropertyBlobDataFolder, _property.Id);
            internalItem.ParentViewModel = this;
            if (isDuplicate)
            {
                var newFilePath = System.IO.Path.Combine(internalItem.BasePath, internalItem.ImageFilename);
                // The original item.BasePath should have been set in MainViewModel.
                File.Copy(System.IO.Path.Combine(item.BasePath, item.ImageFilename), newFilePath, overwrite: true);
                // Keep track of unsaved files to delete them when discarding.
                _unsavedPropertyPictureFileList.Add(newFilePath);
                internalItem.SetIsModified(true);
            }
            else
            {
                internalItem.SetIsModified(false);
            }
            internalItem.PropertyChanged += OnPropertyPicturePropertyChanged;
            Pictures.Add(internalItem);
        }


        Pictures.CollectionChanged += (s, e) =>
        {
            // Unsubscribe from removed items
            if (e.OldItems != null)
            {
                foreach (Models.Rent.Residentials.PropertyPicture item in e.OldItems)
                {
                    Debug.WriteLine($"Item {item.Id} Removed from Pictures. @CollectionChanged in PopulateValues of PropertyViewModel");
                    IsDirty = true;

                    item.PropertyChanged -= OnPropertyPicturePropertyChanged;
                }
            }

            // Subscribe to PropertyChanged.
            if (e.NewItems != null)
            {
                foreach (Models.Rent.Residentials.PropertyPicture item in e.NewItems)
                {
                    Debug.WriteLine($"Item {item.Id} Added to Pictures. @CollectionChanged in PopulateValues of PropertyViewModel");
                    IsDirty = true;

                    item.PropertyChanged += OnPropertyPicturePropertyChanged;
                }
            }
        };

        // PDFs
        /*
        Pdfs = new ObservableCollection<Models.Rent.Residentials.PropertyPdf>(_property.Pdfs);
        foreach (var item in Pdfs)
        {
            item.BasePath = System.IO.Path.Combine(App.PropertyBlobDataFolder, _property.Id);
            item.ParentViewModel = this;
            item.SetIsModified(false);
            item.PropertyChanged += OnPropertyPdfPropertyChanged;
        }
        */
        Pdfs.Clear();
        foreach (var item in _property.Pdfs)
        {
            var internalItem = item.Clone();
            internalItem.BasePath = System.IO.Path.Combine(App.PropertyBlobDataFolder, _property.Id);
            internalItem.ParentViewModel = this;
            if (isDuplicate)
            {
                var newFilePath = System.IO.Path.Combine(internalItem.BasePath, internalItem.PdfFilename);
                // The original item.BasePath should have been set in MainViewModel.
                File.Copy(System.IO.Path.Combine(item.BasePath, item.PdfFilename), newFilePath, overwrite: true);
                // Keep track of unsaved files to delete them when discarding.
                _unsavedPropertyPdfFileList.Add(newFilePath);
                internalItem.SetIsModified(true);

                // Copy the thumbnail file as well
                if (!string.IsNullOrEmpty(item.ThumbnailFilename))
                {
                    var thumbnailPath = Path.Combine(internalItem.BasePath, internalItem.ThumbnailFilename);
                    File.Copy(Path.Combine(item.BasePath, item.ThumbnailFilename), thumbnailPath, overwrite: true);
                    _unsavedPropertyPdfThumbnailFileList.Add(thumbnailPath);
                }
            }
            else
            {
                internalItem.SetIsModified(false);
            }
            internalItem.PropertyChanged += OnPropertyPdfPropertyChanged;
            Pdfs.Add(internalItem);
        }

        Pdfs.CollectionChanged += (sender, args) =>
        {
            if (args.OldItems is not null)
            {
                foreach (Models.Rent.Residentials.PropertyPdf item in args.OldItems)
                {
                    IsDirty = true;
                    item.PropertyChanged -= OnPropertyPdfPropertyChanged;
                }
            }

            if (args.NewItems is not null)
            {
                foreach (Models.Rent.Residentials.PropertyPdf item in args.NewItems)
                {
                    IsDirty = true;
                    item.PropertyChanged += OnPropertyPdfPropertyChanged;
                }
            }
        };

        // Lessors
        LessorsWrapper = [];
        foreach (var item in _property.Lessors)
        {
            LessorsWrapper.Add(new Models.Rent.Residentials.PersonWrapperForPropertyViewModel(item,this));
        }

        // Brokers
        BrokersWrapper = [];
        foreach (var item in _property.Brokers)
        {
            BrokersWrapper.Add(new Models.Rent.Residentials.PersonWrapperForPropertyViewModel(item, this));
        }

        ListingSummaries = new ObservableCollection<Models.Rent.Residentials.ListingSummary>(_property.ListingSummaries);

        IsDirty = isDuplicate;
    }

    private void OnPropertyPicturePropertyChanged(object? sender, PropertyChangedEventArgs e)
    {
        if (sender is not Models.Rent.Residentials.PropertyPicture picBldg)
        {
            Debug.WriteLine("OnPropertyPicturePropertyChanged returned non PictureBldg.");
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

    private void OnPropertyPdfPropertyChanged(object? sender, PropertyChangedEventArgs e)
    {
        if (sender is not Models.Rent.Residentials.PropertyPdf pdfBldg)
        {
            Debug.WriteLine("OnPropertyPdfPropertyChanged returned non PdfBldg.");
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

    private bool ValidateName(bool isShowError = true)
    {
        if (string.IsNullOrWhiteSpace(Name))
        {
            ShowSimpleErrorInfoBar("入力項目に誤りがあります","物件名（必須項目）が入力されていません。保存出来ませんでした。");

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
            ShowSimpleErrorInfoBar("入力項目に誤りがあります","物件名は100文字以内で入力してください。保存出来ませんでした。");

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
                ShowSimpleErrorInfoBar("入力項目に誤りがあります", $"{label}は0以上の{maxLength}桁以内の整数で入力してください。");
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
                
                ShowSimpleErrorInfoBar("入力項目に誤りがあります", $"{label}は0以上の4桁以内の整数で入力してください。");
                
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
            _property.SetName(Name);

            _property.SetPropertyKind(SelectedPropertyKind);
            _property.SetIsUnitLevelOwnership(IsUnitLevelOwnership);
            _property.SetPropertyStructure(SelectedStructureType);

            _property.SetFloorCountAboveGroundFromString(Helpers.Common.NormalizeFullWidthCharacters(FloorCountAboveGround));
            _property.SetFloorCountBasementFromString(Helpers.Common.NormalizeFullWidthCharacters(FloorCountBasement));
            _property.SetTotalUnitCountFromString(Helpers.Common.NormalizeFullWidthCharacters(TotalUnitCount));
            _property.SetBuiltYearAndMonth(BuiltYearAndMonth);
            // Don't strip non numeric chars. Should be validated and stripped in the UI and domain layer, not here.
            _property.SetFudousanId(FudousanId);
            // Don't strip non numeric chars. Should be validated and stripped in the UI and domain layer, not here.
            _property.SetFudousanIdAdditionalCode(FudousanIdAdditionalCode);
            _property.SetRemarks(Remarks);

            // 所在地
            _property.Address.SetMachiazaId(MachiazaId ?? string.Empty);
            _property.Address.SetPrefecture(SelectedPrefecture);
            _property.Address.SetCountyAndCity(SelectedCity);
            _property.Address.SetWardAndOaza(SelectedTown);
            _property.Address.SetChoume(SelectedChou);
            _property.Address.SetEdaban(Edaban);
            _property.Address.SetPostalCode(PostalCode);
            _property.Address.SetLocationLatitude(LocationLatitude);
            _property.Address.SetLocationLongitude(LocationLongitude);
            // 交通1
            _property.Train1.SetRailLine(SelectedRailLine1.LineCode, SelectedRailLine1.LineName);
            if (SelectedRailLine1.LineCode.Equals(SelectedRailStation1.LineCode, StringComparison.Ordinal))
            {
                _property.Train1.SetRailStation(SelectedRailStation1.LineCode, SelectedRailStation1.StationCode, SelectedRailStation1.StationName);
            }
            else
            {
                _property.Train1.SetRailStation(string.Empty, string.Empty, string.Empty);
            }
            _property.Train1.SetEkiToho(Helpers.Common.NormalizeFullWidthCharacters(EkiToho1));
            _property.SetBusStop1(BusStop1);
            _property.SetBusJyousya1(Helpers.Common.NormalizeFullWidthCharacters(BusJyousya1));
            _property.SetBusStopToho1(Helpers.Common.NormalizeFullWidthCharacters(BusStopToho1));
            // 交通2
            _property.Train2.SetRailLine(SelectedRailLine2.LineCode, SelectedRailLine2.LineName);
            if (SelectedRailLine2.LineCode.Equals(SelectedRailStation2.LineCode, StringComparison.Ordinal))
            {
                _property.Train2.SetRailStation(SelectedRailStation2.LineCode, SelectedRailStation2.StationCode, SelectedRailStation2.StationName);
            }
            else
            {
                _property.Train2.SetRailStation(string.Empty, string.Empty, string.Empty);
            }
            _property.Train2.SetEkiToho(Helpers.Common.NormalizeFullWidthCharacters(EkiToho2));
            _property.SetBusStop2(BusStop2);
            _property.SetBusJyousya2(Helpers.Common.NormalizeFullWidthCharacters(BusJyousya2));
            _property.SetBusStopToho2(Helpers.Common.NormalizeFullWidthCharacters(BusStopToho2));
            // 交通3
            _property.Train3.SetRailLine(SelectedRailLine3.LineCode, SelectedRailLine3.LineName);
            if (SelectedRailLine3.LineCode.Equals(SelectedRailStation3.LineCode, StringComparison.Ordinal))
            {
                _property.Train3.SetRailStation(SelectedRailStation3.LineCode, SelectedRailStation3.StationCode, SelectedRailStation3.StationName);
            }
            else
            {
                _property.Train3.SetRailStation(string.Empty, string.Empty, string.Empty);
            }
            _property.Train3.SetEkiToho(Helpers.Common.NormalizeFullWidthCharacters(EkiToho3));
            _property.SetBusStop3(BusStop3);
            _property.SetBusJyousya3(Helpers.Common.NormalizeFullWidthCharacters(BusJyousya3));
            _property.SetBusStopToho3(Helpers.Common.NormalizeFullWidthCharacters(BusStopToho3));
            // 交通4
            _property.Train4.SetRailLine(SelectedRailLine4.LineCode, SelectedRailLine4.LineName);
            if (SelectedRailLine4.LineCode.Equals(SelectedRailStation4.LineCode, StringComparison.Ordinal))
            {
                _property.Train4.SetRailStation(SelectedRailStation4.LineCode, SelectedRailStation4.StationCode, SelectedRailStation4.StationName);
            }
            else
            {
                _property.Train4.SetRailStation(string.Empty, string.Empty, string.Empty);
            }
            _property.Train4.SetEkiToho(Helpers.Common.NormalizeFullWidthCharacters(EkiToho4));
            _property.SetBusStop4(BusStop4);
            _property.SetBusJyousya4(Helpers.Common.NormalizeFullWidthCharacters(BusJyousya4));
            _property.SetBusStopToho4(Helpers.Common.NormalizeFullWidthCharacters(BusStopToho4));

            // Facilities
            _property.HasAutolock = HasAutolock;
            _property.HasElevator = HasElevator;
            _property.HasSecurityCamera = HasSecurityCamera;
            _property.HasParcelLocker = HasParcelLocker;
            _property.HasElectric = HasElectric;
            _property.PropertyElectricKind = SelectedElectricKind.Key;
            _property.ElectricDetail = ElectricDetail;
            _property.HasGas = HasGas;
            _property.GasDetail = GasDetail;
            _property.GasType = SelectedGasType.Key;
            _property.TapWaterDetail = TapWaterDetail;
            _property.TapWaterType = SelectedTapWaterType.Key;
            _property.SewerageDetail = SewerageDetail;
            _property.SewerageType = SelectedSewerageType.Key;
            _property.GroundDigital = GroundDigital;
            _property.BsAntenna = BsAntenna;
            _property.CsAntenna = CsAntenna;
            _property.Catv = Catv;
            _property.Usen = Usen;
            _property.Internet = Internet;
            _property.Fiber = Fiber;
            _property.InternetFree = InternetFree;
            _property.TvAntennaDetail = TvAntennaDetail;
            _property.InternetDetail = InternetDetail;
            _property.HasParking = HasParking;
            _property.IsParkingFree = IsParkingFree;
            _property.ParkingContractType = SelectedParkingContractType.Key;
            _property.ParkingFee = ParkingFee;
            _property.ParkingFeeTaxType = SelectedParkingFeeTaxType.Key;
            _property.ParkingAvailability = SelectedParkingAvailability.Key;
            _property.ParkingSpaces = ParkingSpaces;
            _property.ParkingDistance = ParkingDistance;
            _property.ParkingNotes = ParkingNotes;
            _property.HasBicycleParking = HasBicycleParking;
            _property.IsBicycleParkingFree = IsBicycleParkingFree;
            _property.BicycleParkingAvailability = SelectedBicycleParkingAvailability.Key;
            _property.BicycleParkingFee = BicycleParkingFee;
            _property.BicycleParkingFeeTaxType = SelectedBicycleParkingFeeTaxType.Key;
            _property.BicycleParkingNotes = BicycleParkingNotes;
            _property.HasMotorcycleParking = HasMotorcycleParking;
            _property.IsMotorcycleParkingFree = IsMotorcycleParkingFree;
            _property.MotorcycleParkingAvailability = SelectedMotorcycleParkingAvailability.Key;
            _property.MotorcycleParkingFee = MotorcycleParkingFee;
            _property.MotorcycleParkingFeeTaxType = SelectedMotorcycleParkingFeeTaxType.Key;
            _property.MotorcycleParkingNotes = MotorcycleParkingNotes;

            _property.SetKanriShutai(SelectedKanriShutai?.Key ?? Models.Rent.Residentials.Property.EnumKanriShutai.Unspecified);
            _property.SetKanriIsMaintenanceManagementIfIsKanriJisya(KanriIsMaintenanceManagementIfIsKanriJisya);
            _property.SetKanriIsTenantManagementIfIsKanriJisya(KanriIsTenantManagementIfIsKanriJisya);
            _property.SetKanriIsPaymentManagementIfIsKanriJisya(KanriIsPaymentManagementIfIsKanriJisya);
            _property.SetKanriIsOtherManagementIfIsKanriJisya(KanriIsOtherManagementIfIsKanriJisya);
            _property.SetKanriRemarkIfIsKanriJisya(KanriRemarkIfIsKanriJisya);
            _property.SetKanriNameOfCompanyIfIsKanriTasya(KanriNameOfCompanyIfIsKanriTasya);
            _property.SetKanriContactInfoOfCompanyIfIsKanriTasya(KanriContactInfoOfCompanyIfIsKanriTasya);
            _property.SetKanriRemarkIfIsKanriTasya(KanriRemarkIfIsKanriTasya);
            _property.SetKanriRemarkIfIsKanriKashinushi(KanriRemarkIfIsKanriKashinushi);
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

            ShowErrorInfoBar(err);

            return false;
        }

        // Reset main ThumbnailImageFilePath here.
        _property.ThumbnailFilename = string.Empty;
        // Base dir for the thumbnail
        _property.BasePath = System.IO.Path.Combine(App.PropertyBlobDataFolder, _property.Id);

        // 写真
        //_property.Pictures = Pictures;
        _property.Pictures.Clear();
        foreach (var item in Pictures)
        {
            var newItem = item.Clone();
            newItem.BasePath = item.BasePath;//System.IO.Path.Combine(App.PropertyBlobDataFolder, _property.Id);
            //newItem.ParentViewModel = this;
            newItem.SetIsModified(item.IsModified);
            _property.Pictures.Add(newItem);

            item.PropertyChanged -= OnPropertyPicturePropertyChanged;
        }

        var thumbImg = Pictures.FirstOrDefault(i => i.IsMain == true);
        if (thumbImg is not null)
        {
            // 物件写真サムネイルに指定
            _property.ThumbnailFilename = thumbImg.ImageFilename;
        }

        // 図面
        //_property.Pdfs = Pdfs;
        _property.Pdfs.Clear();
        foreach (var item in Pdfs)
        {
            var newItem = item.Clone();
            newItem.BasePath = item.BasePath;//System.IO.Path.Combine(App.PropertyBlobDataFolder, _property.Id);
            //newItem.ParentViewModel = this;
            newItem.SetIsModified(false);
            _property.Pdfs.Add(newItem);

            item.PropertyChanged -= OnPropertyPdfPropertyChanged;
        }

        if (string.IsNullOrWhiteSpace(_property.ThumbnailFilename))
        {
            var thumbPdf = Pdfs.FirstOrDefault(i => i.IsMain);
            if (thumbPdf is not null)
            {
                if (!IsUnitLevelOwnership)
                {
                    // 物件写真サムネイルに指定
                    _property.ThumbnailFilename = thumbPdf.ThumbnailFilename;
                }
            }
        }

        _property.Lessors.Clear();
        foreach (var item in LessorsWrapper)
        {
            _property.Lessors.Add(item.Person);
            //foreach (var deleted in _property.LessorsToBeDeleted.Where(person => person.Id == item.Person.Id).ToList()) _property.LessorsToBeDeleted.Remove(deleted);
        }
        _property.Brokers.Clear();
        foreach (var item in BrokersWrapper)
        {
            _property.Brokers.Add(item.Person);
            //foreach (var deleted in _property.BrokersToBeDeleted.Where(person => person.Id == item.Person.Id).ToList()) _property.BrokersToBeDeleted.Remove(deleted);
        }

        _property.ListingsToBeSaved = _unsavedListingList;

        return true;
    }

    private bool DiscardUnsavedFiles()
    {
        var failures = new List<string>();

        DeleteTrackedFiles(_unsavedPropertyPictureFileList, failures);
        DeleteTrackedFiles(_unsavedPropertyPdfThumbnailFileList, failures);
        DeleteTrackedFiles(_unsavedPropertyPdfFileList, failures);

        if (_property.Status == EntityStatus.New && Directory.Exists(_propertyDataDirectoryPath))
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

        ShowSimpleErrorInfoBar("エラー", $"変更の破棄中に一時ファイルを削除できませんでした: {string.Join(", ", failures)}");
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
                Debug.WriteLine($"Deleted file '{filePath}'");
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

    private static bool IsSupportedImageExtension(string extension) =>
        extension.Equals(".jpg", StringComparison.OrdinalIgnoreCase) ||
        extension.Equals(".jpeg", StringComparison.OrdinalIgnoreCase) ||
        extension.Equals(".png", StringComparison.OrdinalIgnoreCase) ||
        extension.Equals(".gif", StringComparison.OrdinalIgnoreCase) ||
        extension.Equals(".webp", StringComparison.OrdinalIgnoreCase);

    #endregion

    #region == Commands ==

    #region == Save related commands ==

    [RelayCommand(CanExecute = nameof(CanSave))]
    public async Task Save()
    {
        if (_discardPending)
        {
            ShowSimpleErrorInfoBar("エラー", "変更の破棄が完了していないため保存できません。もう一度画面を閉じて再試行してください。");
            return;
        }
        if (IsSaving || !IsDirty) return;
        /*
        var main = App.GetService<ViewModels.MainViewModel>();
        if (main.RentResidentialListingEditorList.Any(editor => editor.PropertyId == _id && editor.ViewModel is { IsDiscardPending: true }))
        {
            ShowSimpleErrorInfoBar("エラー", "募集物件の変更の破棄を完了してから、もう一度保存してください。");
            return;
        }
        if (HasPendingEditorOperations || main.RentResidentialListingEditorList.Any(editor => editor.PropertyId == _id && editor.ViewModel is { } vm && (vm.HasPendingEditorOperations || vm.IsClosing)))
        {
            ShowSimpleErrorInfoBar("エラー", "関連する選択、ファイル処理または編集画面の終了が完了してから、もう一度保存してください。");
            return;
        }
        if (!CleanIncompleteAttachments()) return;
        if (!main.TryBeginResidentialOperation(_id))
        {
            ShowSimpleErrorInfoBar("エラー", "関連する保存、削除または読み込みが完了してから、もう一度保存してください。");
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

        if (!SetValues())
        {
            return;
        }

        var saveResult = false;
        Models.SearchResult.ResultWrapper resInsert;
        resInsert = await Task.Run(() => _dataAccessService.UpsertRentResidential(_property), _cts.Token);
        saveResult = !resInsert.IsError;

        if (!saveResult)
        {
            Debug.WriteLine(
                resInsert.Error.Title + Environment.NewLine +
                resInsert.Error.Message + Environment.NewLine +
                resInsert.Error.Description + Environment.NewLine +
                resInsert.Error.Operation + Environment.NewLine +
                resInsert.Error.MethodName + Environment.NewLine +
                resInsert.Error.FullDump);

            ShowErrorInfoBar(resInsert.Error);

            return;
        }
        else
        {
            Debug.WriteLine("No errors on update.");
        }

        if (saveResult)
        {
            IsDirty = false;

            _property.SetIsModified(false);
            _property.SetStatus(EntityStatus.Saved);

            // Update title with dummy value.
            WindowTitle = string.Empty;

            // Clear error infobar.
            IsInfoBarErrorOpen = false;

            // Clean up deleted picture file.
            if (_property.PicturesToBeDeleted.Count > 0)
            {
                foreach (var file in _property.PicturesToBeDeleted)
                {
                    var delFilePath = System.IO.Path.Combine(App.PropertyBlobDataFolder, _property.Id, file.ImageFilename);
                    try
                    {
                        File.Delete(delFilePath);
                        Debug.WriteLine($"Deleted tracked file '{delFilePath}'");
                    }
                    catch (Exception ex) when (ex is IOException or UnauthorizedAccessException)
                    {
                        Debug.WriteLine($"Failed to delete file '{delFilePath}': {ex}");
                    }
                }
                _property.PicturesToBeDeleted.Clear();
            }

            // Clean up deleted PDF and thumb file.
            if (_property.PdfsToBeDeleted.Count > 0)
            {
                foreach (var file in _property.PdfsToBeDeleted)
                {
                    var delFilePath = System.IO.Path.Combine(App.PropertyBlobDataFolder, _property.Id, file.PdfFilename);
                    if (File.Exists(delFilePath))
                    {
                        try
                        {
                            File.Delete(delFilePath);
                            Debug.WriteLine($"Deleted tracked file '{delFilePath}'");
                        }
                        catch (Exception ex) when (ex is IOException or UnauthorizedAccessException)
                        {
                            Debug.WriteLine($"Failed to delete file '{delFilePath}': {ex}");
                        }
                    }
                    var thumbFilePath = System.IO.Path.Combine(App.PropertyBlobDataFolder, _property.Id, file.ThumbnailFilename);
                    if (File.Exists(thumbFilePath))
                    {
                        try
                        {
                            File.Delete(thumbFilePath);
                            Debug.WriteLine($"Deleted tracked file '{thumbFilePath}'");
                        }
                        catch (Exception ex) when (ex is IOException or UnauthorizedAccessException)
                        {
                            Debug.WriteLine($"Failed to delete file '{thumbFilePath}': {ex}");
                        }
                    }
                }
                _property.PdfsToBeDeleted.Clear();
            }

            // Just in case clear LessorsToBeDeleted, BrokersToBeDeleted
            _property.LessorsToBeDeleted.Clear();
            _property.BrokersToBeDeleted.Clear();

            // Clear rooms, pic and pdfs and lessors

            if (_property.ListingsToBeDeleted.Count > 0)
            {
                // Clean up deleted listing files and directories
                foreach (var listing in _property.ListingsToBeDeleted)
                {
                    var delFolderPath = System.IO.Path.Combine(App.PropertyBlobDataFolder, _property.Id, listing);
                    if (Directory.Exists(delFolderPath))
                    {
                        try
                        {
                            Directory.Delete(delFolderPath, true);
                            Debug.WriteLine($"Deleted tracked directory '{delFolderPath}'");
                        }
                        catch (Exception ex) when (ex is IOException or UnauthorizedAccessException)
                        {
                            Debug.WriteLine($"Failed to delete directory '{delFolderPath}': {ex}");
                        }
                    }
                }
                _property.ListingsToBeDeleted.Clear();
            }

            _unsavedPropertyPictureFileList.Clear();
            _unsavedPropertyPdfThumbnailFileList.Clear();
            _unsavedPropertyPdfFileList.Clear();

            // Just in case.
            _property.SetStatus(EntityStatus.Saved);
            _property.SetIsModified(false);

            // Clear unsaved listings and set status to saved.
            foreach (var listing in _unsavedListingList)
            {
                listing.SetPropertyName(Name);
                listing.SetPropertyStatus(EntityStatus.Saved);
                listing.SetStatus(EntityStatus.Saved);
                listing.SetIsModified(false);
            }
            _unsavedListingList.Clear();

            // Clear unsaved child editors and quit tracking them.
            UnsavedChildEditorList.Clear();

            WeakReferenceMessenger.Default.Send(new Models.Messenger.PropertyUpdatedMessage(_property as Models.Base.PropertyBase));
        }
    }
    private bool CanSave()
    {
        return IsDirty && !IsSaving && !_isClosing && !_propertyBusy && !_discardPending;
    }

    #endregion

    #region == Pictures and Pdfs related commands ==

    [RelayCommand(CanExecute = nameof(CanAddNewPropertyPictures))]
    private async Task AddNewPropertyPictures(List<string> filePathList)
    {
        // File open is handleed in the code behind since this ViewModel does not know about the Window which is required for the Picker.

        if (filePathList is null) return;
        if (filePathList.Count == 0) return;

        //Debug.WriteLine($"destDirectory={_property.PropertyDataDirectoryPath}  @SetNewPropertyPicturesAsync()");

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
            var destFilePath = Path.Combine(_propertyDataDirectoryPath, newFilename);
            //Debug.WriteLine($"{file} to {destFilePath}  @SetNewPropertyPicturesAsync()");

            using var destinationStream = new FileStream(destFilePath, FileMode.CreateNew, FileAccess.Write);
            //_incompleteAttachmentFiles.Add(destFilePath);
            await sourceStream.CopyToAsync(destinationStream);

            var pic = new Models.Rent.Residentials.PropertyPicture(newId, newFilename, EntityStatus.New)
            {
                BasePath = _propertyDataDirectoryPath,//System.IO.Path.Combine(App.PropertyBlobDataFolder, _property.Id),
                ParentViewModel = this
            };

            Pictures.Add(pic);

            OpenPropertyBlobDirectoryCommand.NotifyCanExecuteChanged();
            DeletePropertyPictureCommand.NotifyCanExecuteChanged();

            IsDirty = true;

            // Keep track of unsaved files to delete them when discarding.
            _unsavedPropertyPictureFileList.Add(destFilePath);
        }
    }
    private static bool CanAddNewPropertyPictures(List<string> filePathList)
    {
        if (filePathList.Count < 1)
        {
            return false;
        }

        return true;
    }

    [RelayCommand(CanExecute = nameof(CanDeletePropertyPicture))]
    private async Task DeletePropertyPicture(Models.Rent.Residentials.PropertyPicture picProp)
    {
        if (picProp is null)
        {
            return;
        }

        if (await _dialogService.ShowDeleteConfirmationDialog("この物件写真を削除しますか？") != Microsoft.UI.Xaml.Controls.ContentDialogResult.Primary)
        {
            return;
        }

        if (Pictures.Remove(picProp))
        {
            _property.PicturesToBeDeleted.Add(picProp);
            IsDirty = true;
        }
    }
    private static bool CanDeletePropertyPicture(Models.Rent.Residentials.PropertyPicture picProp)
    {
        return picProp is not null;
    }

    [RelayCommand(CanExecute = nameof(CanAddNewPropertyPdfs))]
    private async Task AddNewPropertyPdfs(List<string> filePathList)
    {
        if (filePathList is null) return;
        if (filePathList.Count == 0 || IsUnitLevelOwnership) return;
        //var ownershipVersion = _ownershipVersion;

        Debug.WriteLine($"destDirectory={_propertyDataDirectoryPath}  @AddNewPropertyPdfs()");

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

                    // TODO: Revisit this.
                    if (IsUnitLevelOwnership)
                    {
                        _unsavedPropertyPdfThumbnailFileList.Add(thumbnailDestFilePath);
                        _unsavedPropertyPdfFileList.Add(pdfDestFilePath);
                        IsDirty = true;
                        continue;
                    }

                    var pdf = new Models.Rent.Residentials.PropertyPdf(newId, newFilename, newThumbnailFilename, EntityStatus.New)
                    {
                        BasePath = _propertyDataDirectoryPath,
                        ParentViewModel = this
                    };

                    Pdfs.Add(pdf);
                    _unsavedPropertyPdfThumbnailFileList.Add(thumbnailDestFilePath);
                    _unsavedPropertyPdfFileList.Add(pdfDestFilePath);

                    OpenPropertyBlobDirectoryCommand.NotifyCanExecuteChanged();
                    DeletePropertyPdfCommand.NotifyCanExecuteChanged();
                    IsDirty = true;
                }
                else
                {
                    ShowSimpleErrorInfoBar("エラー", $"PDFにページがありません: {Path.GetFileName(filePath)}");
                }
            }
            catch (Exception ex) when (ex is IOException or UnauthorizedAccessException or COMException or InvalidOperationException or ArgumentException)
            {
                /*
                if (ownershipVersion != _ownershipVersion || IsUnitLevelOwnership)
                {
                    foreach (var path in importedPaths)
                    {
                        if (!_pendingPropertyPdfCleanupFiles.Contains(path, StringComparer.Ordinal)) _pendingPropertyPdfCleanupFiles.Add(path);
                        _incompleteAttachmentFiles.Remove(path);
                    }
                }
                */
                Debug.WriteLine($"Failed to import PDF '{filePath}': {ex}");
                ShowSimpleErrorInfoBar("エラー", $"PDFを読み込めませんでした: {Path.GetFileName(filePath)}。{ex.Message}");
            }
        }
    }

    private static bool CanAddNewPropertyPdfs(List<string> filePathList)
    {
        if (filePathList.Count < 1)
        {
            return false;
        }

        return true;
    }

    [RelayCommand]
    private async Task OpenPropertyPdf(Models.Rent.Residentials.PropertyPdf pdf)
    {
        if (pdf is null || !Pdfs.Contains(pdf) || !IsEditorEnabled) return;

        try
        {
            var file = await StorageFile.GetFileFromPathAsync(Path.Combine(_propertyDataDirectoryPath, pdf.PdfFilename));
            if (!await Windows.System.Launcher.LaunchFileAsync(file))
            {
                ShowSimpleErrorInfoBar("エラー", "PDFを開けませんでした。PDFを開くアプリが設定されているか確認してください。");
            }
        }
        catch (Exception ex) when (ex is IOException or UnauthorizedAccessException or COMException or InvalidOperationException or ArgumentException)
        {
            Debug.WriteLine($"Failed to open residential property PDF: {ex}");
            ShowSimpleErrorInfoBar("エラー", $"PDFを開けませんでした: {ex.Message}");
        }
    }

    [RelayCommand(CanExecute = nameof(CanDeletePropertyPdf))]
    private async Task DeletePropertyPdf(Models.Rent.Residentials.PropertyPdf pdfProp)
    {
        if (pdfProp is null)
        {
            return;
        }

        if (await _dialogService.ShowDeleteConfirmationDialog("この建物図面を削除しますか？") != Microsoft.UI.Xaml.Controls.ContentDialogResult.Primary)
        {
            return;
        }

        if (Pdfs.Remove(pdfProp))
        {
            _property.PdfsToBeDeleted.Add(pdfProp);
            IsDirty = true;
        }
    }
    private static bool CanDeletePropertyPdf(Models.Rent.Residentials.PropertyPdf pdfProp)
    {
        return pdfProp is not null;
    }

    [RelayCommand(CanExecute = nameof(CanOpenPropertyBlobDirectory))]
    private void OpenPropertyBlobDirectory()
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
                ShowSimpleErrorInfoBar("エラー", $"フォルダーを開けませんでした。{ex.Message}");
            }
        }
    }
    private bool CanOpenPropertyBlobDirectory()
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
    private void AddNewListing() 
    {
        var newId = Guid.CreateVersion7().ToString("N");
        var editorShell = _shellFactory.Create(new Models.Rent.Residentials.Listing(newId, EntityStatus.New, _property.Id, _property.Status, _property.IsUnitLevelOwnership, Name, _property.FudousanId), false);

        // Apply the current IsUnitOwnership state because it may not be saved to the _room YET.
        //editorShell.ViewModel.SetIsPropertyUnitOwnership(this.IsUnitOwnership);
        // Use message. If this is set, then show/hide the owner and zumen from shell menu.
        WeakReferenceMessenger.Default.Send(new Models.Messenger.PropertyIsUnitOwnershipChangedMessage(_id, this.IsUnitLevelOwnership));

        var mainVM = App.GetService<ViewModels.MainViewModel>();
        mainVM.RentResidentialListingEditorList.Add(editorShell.Window);

        // Track unsaved child editor windows so that they can be closed when the parent is closed 
        // because the property is not saved yet, the child editor windows/listing is dependent of this property.
        if (_property.Status == EntityStatus.New)
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

    [RelayCommand(CanExecute = nameof(EditSelectedListingCanExecute))]
    private async Task EditSelectedListing(ListingSummary listingSummary)
    {
        if (listingSummary is null || !ListingSummaries.Contains(listingSummary) || !IsEditorEnabled) return;

        await EditSelectedListingCore(listingSummary);
    }

    private async Task EditSelectedListingCore(ListingSummary listingSummary)
    {
        var unitId = listingSummary.Id;
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
                ShowSimpleErrorInfoBar("エラー", "募集物件の編集画面を開けませんでした。既存の編集画面を確認してから再試行してください。");

                WeakReferenceMessenger.Default.Send(new WindowClosedMessage<Microsoft.UI.Xaml.Window>(editWin));

                return;
            }
        }

        Models.Rent.Residentials.Listing listing;

        var result = await Task.Run(() => _dataAccessService.SelectRentResidentialListingById(_id, unitId), _cts.Token);
        if (result.IsError)
        {
            ShowErrorInfoBar(result.Error);
            return;
        }
        if (result.Listing is null)
        {
            ShowSimpleErrorInfoBar("エラー", "募集物件が見つかりませんでした。再読み込みしてください。");
            return;
        }
        listing = result.Listing;
        var editorShell = _shellFactory.Create(listing, false);

        // Apply the current IsUnitOwnership state because it may not be saved to the _listing YET.
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
    public static bool EditSelectedListingCanExecute(ListingSummary listingSummary)
    {
        if (listingSummary is null) return false;
        return true;
    }

    [RelayCommand(CanExecute = nameof(DupeSelectedListingCanExecute))]
    private void DupeSelectedListing(ListingSummary listingSummary)
    {
        if (listingSummary is null) return;
    }
    public static bool DupeSelectedListingCanExecute(ListingSummary listingSummary)
    {
        if (listingSummary is null) return false;
        return true;
    }

    [RelayCommand(CanExecute = nameof(DeleteSelectedListingCanExecute))]
    private async Task DeleteSelectedListing(ListingSummary listingSummary)
    {
        if (listingSummary is null)
        {
            return;
        }

        // 
        var mainVM = App.GetService<ViewModels.MainViewModel>();

        // Check if the selected item is already being edited in another window.
        foreach (var editWin in mainVM.RentResidentialListingEditorList.ToList())
        {
            if (editWin.Id != listingSummary.Id)
            {
                continue;
            }

            //Debug.WriteLine($"Editor window for {unitId} is already open. Activating it.");

            try
            {
                ShowSimpleErrorInfoBar("募集物件の編集画面が開いています","募集物件の編集画面を閉じてから、建物を削除してください。");

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

        if (mainVM.RentResidentialListingEditorList.FirstOrDefault(editor => editor.Id == listingSummary.Id) is { } openedEditor)
        {
            ShowSimpleErrorInfoBar("編集画面が開いています", "募集物件の編集画面を閉じてから削除してください。");
            openedEditor.Activate();
            return;
        }

        if (await _dialogService.ShowDeleteConfirmationDialog("この募集物件を削除しますか？") != Microsoft.UI.Xaml.Controls.ContentDialogResult.Primary) return;

        if (ListingSummaries.Remove(listingSummary))
        {
            // Don't delete directly. Just mark it for deletion. It will be deleted when the property is saved.
            //if (_property.Rooms.Remove(listing)) { }
            _property.ListingsToBeDeleted.Add(listingSummary.Id);

            _property.ListingSummaries.Remove(listingSummary);

            Debug.WriteLine($"Marked listing {listingSummary.Id} for deletion.");

            IsDirty = true;
        }
    }
    public static bool DeleteSelectedListingCanExecute(ListingSummary listingSummary)
    {
        if (listingSummary is null) return false;
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
                ShowSimpleErrorInfoBar("エラー", "選択した貸主を読み込めませんでした。");
                return;
            }

            var asdf = new Models.Rent.Residentials.PersonWrapperForPropertyViewModel(res.Person, this);

            foreach (var deleted in _property.LessorsToBeDeleted.Where(person => person.Id == res.Person.Id).ToList()) _property.LessorsToBeDeleted.Remove(deleted);
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
            if (!_property.LessorsToBeDeleted.Any(person => person.Id == lessor.Person.Id)) _property.LessorsToBeDeleted.Add(lessor.Person);
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
                ShowSimpleErrorInfoBar("エラー", "選択した宅建業者を読み込めませんでした。");
                return;
            }

            var wrapper = new Models.Rent.Residentials.PersonWrapperForPropertyViewModel(res.Person, this);

            foreach (var deleted in _property.BrokersToBeDeleted.Where(person => person.Id == res.Person.Id).ToList()) _property.BrokersToBeDeleted.Remove(deleted);
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
            if (!_property.BrokersToBeDeleted.Any(person => person.Id == broker.Person.Id)) _property.BrokersToBeDeleted.Add(broker.Person);
        }
    }
    private static bool CanDeleteBroker(Models.Rent.Residentials.PersonWrapperForPropertyViewModel broker)
    {
        return broker is not null;
    }

    #endregion

    #endregion

}
