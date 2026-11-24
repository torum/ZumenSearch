using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Globalization;
using System.Runtime.InteropServices;
using System.Text.RegularExpressions;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.Messaging;
using Microsoft.UI.Windowing;
using Microsoft.UI.Xaml.Controls;
using Windows.Data.Pdf;
using Windows.Storage;
using Windows.Storage.Streams;
using Windows.System;
using ZumenSearch.Domain.Entities.Rent.Residentials;
using ZumenSearch.Domain.Enums;
using ZumenSearch.Domain.ValueObjects.Location;
using ZumenSearch.Domain.ValueObjects.Transportation;
using ZumenSearch.Application.Contracts;
using ZumenSearch.Application.Services.Persons;
using ZumenSearch.Application.Services.Rent.Residentials;
using ZumenSearch.Presentation.Factories.AbstractFactory;
using ZumenSearch.Presentation.Features.Rent.Residentials.Models;
using ZumenSearch.Presentation.Helpers;
using ZumenSearch.Presentation.Messaging;
using ZumenSearch.Presentation.Navigation;
using ZumenSearch.Presentation.Services.Contracts;
using ZumenSearch.Presentation.Shell.Models;
using ZumenSearch.Presentation.Shell.ViewModels;

namespace ZumenSearch.Presentation.Features.Rent.Residentials.ViewModels;

/*
 * TODO: 
 * Use IsBusy to disable the editor commands and show progress ring when saving or loading data.
 */

public sealed partial class PropertyViewModel : ObservableRecipient, IRecipient<ListingUpdatedMessage>, IRecipient<WindowClosedMessage<Microsoft.UI.Xaml.Window>>, IRecipient<ListingDeletedMessage>, IRecipient<LessorUpdatedMessage>, IRecipient<BrokerUpdatedMessage>, IRecipient<LessorDeletedMessage>, IRecipient<BrokerDeletedMessage>, IDisposable
{
    #region == Private variables and const ==

    private const string BasicPageName = NavigationKeys.RentResidentialPropertyBasic;

    private readonly string _id = string.Empty;

    // This property holds the COPY of current entity being edited.
    // Do not use it directly in the UI. Apply changes to this object in Save() to save the changes.
    private readonly ZumenSearch.Domain.Entities.Rent.Residentials.Property _property;

    private readonly string _propertyDataDirectoryPath = string.Empty;

    // Tmp file list to hold unsaved picture files. (if entry is discarded without saving, delete on close)
    private readonly List<string> _unsavedPropertyPictureFileList = [];
    private readonly List<string> _unsavedPropertyPdfFileList = [];
    private readonly List<string> _unsavedPropertyPdfThumbnailFileList = [];
    private readonly List<string> _cleanupFailures = [];

    // While the property is new, this editor owns listings handed off by child editors and saves them with the property.
    // Once the property is saved, it relinquishes ownership and notifies the listing editors.
    private readonly List<Listing> _unsavedListingList = [];

    private bool _isBusy;

    private readonly CancellationTokenSource _cts = new();
    private bool _disposed;

    #endregion

    #region == Services ==

    private readonly IAbstractFactory<ZumenSearch.Domain.Entities.Rent.Residentials.Listing, bool, bool, ZumenSearch.Presentation.Features.Rent.Residentials.Views.Listing.ShellPage> _shellFactory;
    private readonly RentResidentialPropertyService _rentResidentialPropertyService;
    private readonly RentResidentialListingService _rentResidentialListingService;
    private readonly PersonService _personService;
    private readonly ILocationRepository _locationRepository;
    private readonly IDispatcherService _dispatcherService;
    private readonly IDialogGenericService _dialogService;
    private readonly INavigationGenericService _navigationService;

    #endregion

    public PropertyViewModel(ZumenSearch.Domain.Entities.Rent.Residentials.Property property, bool isDuplicate, INavigationGenericService navigationService, IDialogGenericService dialogService, IAbstractFactory<ZumenSearch.Domain.Entities.Rent.Residentials.Listing, bool, bool, ZumenSearch.Presentation.Features.Rent.Residentials.Views.Listing.ShellPage> shellFactory, IDispatcherService dispatcherService, RentResidentialPropertyService rentResidentialPropertyService, RentResidentialListingService rentResidentialListingService, PersonService personService, ILocationRepository locationRepository)
    {
        _property = property;
        _id = property.Id;

        _navigationService = navigationService;
        _dialogService = dialogService;
        _shellFactory = shellFactory;
        _dispatcherService = dispatcherService;
        _rentResidentialPropertyService = rentResidentialPropertyService;
        _rentResidentialListingService = rentResidentialListingService;
        _personService = personService;
        _locationRepository = locationRepository;

        //
        IsUnitLevelOwnership = property.IsUnitLevelOwnership;
        WeakReferenceMessenger.Default.Send(new PropertyIsUnitOwnershipChangedMessage(_id, IsUnitLevelOwnership));

        // Create the data folder for this property. This is where the pictures and pdfs will be stored.
        _propertyDataDirectoryPath = System.IO.Path.Combine(App.PropertyBlobDataFolder, _id);

        try
        {
            if (!Directory.Exists(_propertyDataDirectoryPath))
            {
                Directory.CreateDirectory(_propertyDataDirectoryPath);
            }
        }
        catch (Exception ex)
        {
            var err = new ZumenSearch.Application.Results.ErrorInfo
            {
                Title = "予期しないエラーが発生しました。",
                Type = ZumenSearch.Application.Results.ErrorInfo.ErrType.Other,
                Code = "",
                Message = $"建物のデータフォルダを作成できませんでした。写真や図面を保存出来ません。",
                Description = $"{ex}",
                Operation = "Directory.CreateDirectory",
                MethodName = nameof(PropertyViewModel),
                OccuredAt = DateTime.Now
            };

            ShowErrorInfoBar(err);
        }

        // Initialize the transportation entries for the four transportation slots.
        _transportationEntries = [new(this, 0), new(this, 1), new(this, 2), new(this, 3)];

        // Populate the view model with the property values.
        PopulateValues(isDuplicate);

        // Reset errors
        IsNameHasError = false;

        // Reset IsDirty to false after PopulateValues() because PopulateValues() automatically sets IsDirty = true when it copies values from the property to the view model.
        _property.SetIsModified(isDuplicate);
        IsDirty = isDuplicate;

        // Ready to receive messages.
        // Make sure to add matching unregistrations in Dispose().
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
    public List<ZumenSearch.Presentation.Features.Rent.Residentials.Views.Listing.EditorWindow> UnsavedChildEditorList { get; } = [];

    public bool IsBusy
    {
        get => _isBusy;
        private set
        {
            if (SetProperty(ref _isBusy, value))
            {
                OnPropertyChanged(nameof(IsEditorEnabled));
                SaveCommand.NotifyCanExecuteChanged();
            }
        }
    }

    public bool IsEditorEnabled => !IsBusy;


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
        new() { Name = "建物", Page = NavigationKeys.RentResidentialPropertyBasic },
        new() { Name = "基本", Page = NavigationKeys.RentResidentialPropertyBasic }
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

                // Update title with dummy value.
                WindowTitle = string.Empty;

                // Moved to after the save.
                //_selectedSearchResult?.Name = value; // Update the selected search result's name if it exists
            }
        }
    }

    // 物件種別
    public IReadOnlyCollection<ZumenSearch.Presentation.Features.Rent.Residentials.Models.PropertyKindTypeLabel> PropertyKinds { get; set; } = PropertyKindTypeLabel.GetAll();
    /*
    [
        //new Kind(EnumKinds.Unspecified.ToString(), "未指定"),
        new ZumenSearch.Presentation.Features.Rent.Residentials.Models.PropertyKindTypeLabel(ZumenSearch.Presentation.Features.Rent.Residentials.Models.PropertyKindType.Apartment),
        new ZumenSearch.Presentation.Features.Rent.Residentials.Models.PropertyKindTypeLabel(ZumenSearch.Presentation.Features.Rent.Residentials.Models.PropertyKindType.Mansion),
        new ZumenSearch.Presentation.Features.Rent.Residentials.Models.PropertyKindTypeLabel(ZumenSearch.Presentation.Features.Rent.Residentials.Models.PropertyKindType.House),
        new ZumenSearch.Presentation.Features.Rent.Residentials.Models.PropertyKindTypeLabel(ZumenSearch.Presentation.Features.Rent.Residentials.Models.PropertyKindType.TerraceHouse),
        new ZumenSearch.Presentation.Features.Rent.Residentials.Models.PropertyKindTypeLabel(ZumenSearch.Presentation.Features.Rent.Residentials.Models.PropertyKindType.TownHouse),
        new ZumenSearch.Presentation.Features.Rent.Residentials.Models.PropertyKindTypeLabel(ZumenSearch.Presentation.Features.Rent.Residentials.Models.PropertyKindType.ShareHouse),
        new ZumenSearch.Presentation.Features.Rent.Residentials.Models.PropertyKindTypeLabel(ZumenSearch.Presentation.Features.Rent.Residentials.Models.PropertyKindType.Dormitory)
    ];*/

    public ZumenSearch.Presentation.Features.Rent.Residentials.Models.PropertyKindTypeLabel SelectedPropertyKind
    {
        get => field ?? new(PropertyKindType.Unspecified);
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
                    if (_navigationService.IsCurrentPageSameAs(NavigationKeys.RentResidentialPropertyDocuments) ||
                        _navigationService.IsCurrentPageSameAs(NavigationKeys.RentResidentialPropertyLessors) ||
                        _navigationService.IsCurrentPageSameAs(NavigationKeys.RentResidentialPropertyBrokers))
                    {
                        _navigationService.NavigateTo(BasicPageName, this);
                    }
                }

                // If this is set, then show/hide the owner and zumen from shell menu.
                WeakReferenceMessenger.Default.Send(new ZumenSearch.Presentation.Messaging.PropertyIsUnitOwnershipChangedMessage(_id, value));
            }
        }
    }

    [ObservableProperty]
    public partial bool IsNotUnitLevelOwnership { get; private set; } = true;

    // 建物構造
    public IReadOnlyCollection<ZumenSearch.Presentation.Features.Rent.Residentials.Models.PropertyStructureTypeLabel> StructureTypes { get; set; } = PropertyStructureTypeLabel.GetAll();
    /*
    [
        //new Structure(StructureType.Unspecified.ToString(), "未指定"),
        new ZumenSearch.Presentation.Features.Rent.Residentials.Models.PropertyStructureTypeLabel(ZumenSearch.Presentation.Features.Rent.Residentials.Models.StructureType.Wood),
        new ZumenSearch.Presentation.Features.Rent.Residentials.Models.PropertyStructureTypeLabel(ZumenSearch.Presentation.Features.Rent.Residentials.Models.StructureType.Block),
        new ZumenSearch.Presentation.Features.Rent.Residentials.Models.PropertyStructureTypeLabel(ZumenSearch.Presentation.Features.Rent.Residentials.Models.StructureType.LightSteel),
        new ZumenSearch.Presentation.Features.Rent.Residentials.Models.PropertyStructureTypeLabel(ZumenSearch.Presentation.Features.Rent.Residentials.Models.StructureType.Steel),
        new ZumenSearch.Presentation.Features.Rent.Residentials.Models.PropertyStructureTypeLabel(ZumenSearch.Presentation.Features.Rent.Residentials.Models.StructureType.RC),
        new ZumenSearch.Presentation.Features.Rent.Residentials.Models.PropertyStructureTypeLabel(ZumenSearch.Presentation.Features.Rent.Residentials.Models.StructureType.SRC),
        new ZumenSearch.Presentation.Features.Rent.Residentials.Models.PropertyStructureTypeLabel(ZumenSearch.Presentation.Features.Rent.Residentials.Models.StructureType.ALC),
        new ZumenSearch.Presentation.Features.Rent.Residentials.Models.PropertyStructureTypeLabel(ZumenSearch.Presentation.Features.Rent.Residentials.Models.StructureType.PC),
        new ZumenSearch.Presentation.Features.Rent.Residentials.Models.PropertyStructureTypeLabel(ZumenSearch.Presentation.Features.Rent.Residentials.Models.StructureType.HPC),
        new ZumenSearch.Presentation.Features.Rent.Residentials.Models.PropertyStructureTypeLabel(ZumenSearch.Presentation.Features.Rent.Residentials.Models.StructureType.RB),
        new ZumenSearch.Presentation.Features.Rent.Residentials.Models.PropertyStructureTypeLabel(ZumenSearch.Presentation.Features.Rent.Residentials.Models.StructureType.CFT),
        new ZumenSearch.Presentation.Features.Rent.Residentials.Models.PropertyStructureTypeLabel(ZumenSearch.Presentation.Features.Rent.Residentials.Models.StructureType.Other)
    ];*/

    public ZumenSearch.Presentation.Features.Rent.Residentials.Models.PropertyStructureTypeLabel SelectedStructureType
    {
        get => field ?? new(StructureType.Unspecified);
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

            var text = ZumenSearch.Presentation.Utilities.Common.NormalizeFullWidthCharacters(value).Trim();
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

            var text = ZumenSearch.Presentation.Utilities.Common.NormalizeFullWidthCharacters(value).Trim().ToUpperInvariant();
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
            }
        }
    } = string.Empty;

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

                dataset = _locationRepository.GetCountyAndCityByPref(field.Name);

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

                dataset = _locationRepository.GetWardAndOazaByPrefCountyCity(SelectedPrefecture.Name, field.County, field.City);

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

                dataset = _locationRepository.GetChoumeByPrefCountyCityWardOaza(SelectedPrefecture.Name, SelectedCity.County, SelectedCity.City, field.Ward, field.Oaza);

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

    public ObservableCollection<ZumenSearch.Presentation.Features.Rent.Residentials.Models.PropertyElectricKind> ElectricKinds { get; set; } =
    [
        new ZumenSearch.Presentation.Features.Rent.Residentials.Models.PropertyElectricKind(ZumenSearch.Domain.Entities.Rent.Residentials.Property.EnumElectricType.AllElectric, "オール電化"),
        new ZumenSearch.Presentation.Features.Rent.Residentials.Models.PropertyElectricKind(ZumenSearch.Domain.Entities.Rent.Residentials.Property.EnumElectricType.Unspecified, "未指定")
    ];

    public ZumenSearch.Presentation.Features.Rent.Residentials.Models.PropertyElectricKind SelectedElectricKind
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

    public ObservableCollection<ZumenSearch.Presentation.Features.Rent.Residentials.Models.PropertyKanriShutai> KanriShutais { get; set; } =
    [
        new ZumenSearch.Presentation.Features.Rent.Residentials.Models.PropertyKanriShutai(ZumenSearch.Domain.Entities.Rent.Residentials.Property.EnumKanriShutai.Unspecified, "未指定"),
        new ZumenSearch.Presentation.Features.Rent.Residentials.Models.PropertyKanriShutai(ZumenSearch.Domain.Entities.Rent.Residentials.Property.EnumKanriShutai.Jisya, "自社管理"),
        new ZumenSearch.Presentation.Features.Rent.Residentials.Models.PropertyKanriShutai(ZumenSearch.Domain.Entities.Rent.Residentials.Property.EnumKanriShutai.Tasya, "他社管理"),
        new ZumenSearch.Presentation.Features.Rent.Residentials.Models.PropertyKanriShutai(ZumenSearch.Domain.Entities.Rent.Residentials.Property.EnumKanriShutai.Kashinushi, "貸主管理")
    ];

    public ZumenSearch.Presentation.Features.Rent.Residentials.Models.PropertyKanriShutai? SelectedKanriShutai
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
                IsKanriUnspecified = field.Key == ZumenSearch.Domain.Entities.Rent.Residentials.Property.EnumKanriShutai.Unspecified;
                IsKanriJisya = field.Key == ZumenSearch.Domain.Entities.Rent.Residentials.Property.EnumKanriShutai.Jisya;
                IsKanriTasya = field.Key == ZumenSearch.Domain.Entities.Rent.Residentials.Property.EnumKanriShutai.Tasya;
                IsKanriKashinushi = field.Key == ZumenSearch.Domain.Entities.Rent.Residentials.Property.EnumKanriShutai.Kashinushi;
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

    public ObservableCollection<PropertyPictureViewModel> Pictures
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

    public ObservableCollection<PropertyPdfViewModel> Pdfs
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

    public ObservableCollection<ZumenSearch.Presentation.Features.Rent.Residentials.ViewModels.PropertyPersonViewModel> LessorsWrapper
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

    public ObservableCollection<ZumenSearch.Presentation.Features.Rent.Residentials.ViewModels.PropertyPersonViewModel> BrokersWrapper
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
    public partial ZumenSearch.Domain.Entities.Rent.Residentials.Listing? SelectedRoom { get; set; }
    */

    #endregion

    #endregion

    #region == Messages ==

    public void Receive(ListingUpdatedMessage message)
    {
        var updated = message.Value;
        if (updated is null)
        {
            return;
        }

        if (updated is not ZumenSearch.Domain.Entities.Rent.Residentials.Listing listing || !listing.PropertyId.Equals(_id, StringComparison.Ordinal))
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
        var existingListingIndex = _unsavedListingList.FindIndex(item => item.Id.Equals(listing.Id, StringComparison.Ordinal));
        if (existingListingIndex >= 0)
        {
            Debug.WriteLine($"Updating existing listing {listing.Id} in unsaved listing list.");
            _unsavedListingList[existingListingIndex] = listing;
            IsDirty = true;
        }
        else
        {
            if (listing.Status == EntityStatus.New)
            {
                Debug.WriteLine($"Adding new listing {listing.Id} to unsaved listing list.");
                _unsavedListingList.Add(listing);
                DuplicateSelectedListingCommand.NotifyCanExecuteChanged();
                IsDirty = true;
            }
            else
            {
                // Remove the child editor window for this listing if it exists
                // because it is no longer new and has been saved and can be saved independently.
                var win = this.UnsavedChildEditorList.FirstOrDefault(w => w is ZumenSearch.Presentation.Features.Rent.Residentials.Views.Listing.EditorWindow ew && ew.Id == listing.Id);
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
        var lessor = message.Value;
        if (lessor is null)
        {
            return;
        }

        var psn = LessorsWrapper.FirstOrDefault(r => r.Person.Id.Equals(lessor.Id, StringComparison.Ordinal));
        if (psn is null) return;

        if (psn.Person.GetType() != lessor.GetType())
        {
            LessorsWrapper[LessorsWrapper.IndexOf(psn)] = new PropertyPersonViewModel(lessor, this);
        }
        else
        {
            psn.Person = lessor;
        }
    }

    public void Receive(BrokerUpdatedMessage message)
    {
        var broker = message.Value;
        if (broker is null)
        {
            return;
        }

        var psn = BrokersWrapper.FirstOrDefault(r => r.Person.Id.Equals(broker.Id, StringComparison.Ordinal));
        if (psn is null) return;

        if (psn.Person.GetType() != broker.GetType())
        {
            BrokersWrapper[BrokersWrapper.IndexOf(psn)] = new PropertyPersonViewModel(broker, this);
        }
        else
        {
            psn.Person = broker;
        }
    }

    public void Receive(ListingDeletedMessage message)
    {
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

        if (window is ZumenSearch.Presentation.Features.Rent.Residentials.Views.Listing.EditorWindow rlwin)
        {
            this.UnsavedChildEditorList.Remove(rlwin);
        }
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
        if (IsBusy) return;
        IsBusy = true;

        try
        {
            DiscardUnsavedFiles();

            // Need to restore the original values from unsaved _property because IsUnitLevelOwnership change is real-time not save-time update.
            WeakReferenceMessenger.Default.Send(new PropertyIsUnitOwnershipChangedMessage(_id, _property.IsUnitLevelOwnership));

            IsDirty = false;
        }
        finally
        {
            IsBusy = false;
        }
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
        try
        {
            // Basics
            if (isDuplicate)
            {
                var duplicateName = $"複製_{_property.Name}";
                if (duplicateName.Length <= 100)
                {
                    _property.SetName(duplicateName);
                }
                else
                {
                    ShowSimpleErrorInfoBar("物件名を確認してください", "物件名が長いため、複製の接頭辞を追加できませんでした。必要に応じて名前を変更してください。");
                }
            }

            Name = _property.Name;

            //SelectedKind = _property.PropertyKind;
            var propertyKindKey = PropertyKinds.FirstOrDefault(k => k.Key == _property.PropertyKind);
            SelectedPropertyKind = propertyKindKey is null ? new(PropertyKindType.Unspecified) : propertyKindKey;
            IsUnitLevelOwnership = _property.IsUnitLevelOwnership;
            var structureKey = StructureTypes.FirstOrDefault(k => k.Key == _property.PropertyStructure);
            SelectedStructureType = structureKey is null ? new(StructureType.Unspecified) : structureKey;

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

            // Pictures (don't use directly)
            Pictures.Clear();
            foreach (var item in _property.Pictures)
            {
                var internalItem = item.Clone();
                internalItem.BasePath = System.IO.Path.Combine(App.PropertyBlobDataFolder, _property.Id);
                if (isDuplicate)
                {
                    try
                    {
                        var newFilePath = System.IO.Path.Combine(internalItem.BasePath, internalItem.ImageFilename);
                        // The original item.BasePath should have been set in MainViewModel.
                        File.Copy(System.IO.Path.Combine(item.BasePath, item.ImageFilename), newFilePath, overwrite: true);
                        // Keep track of unsaved files to delete them when discarding.
                        _unsavedPropertyPictureFileList.Add(newFilePath);

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

                var pictureViewModel = new PropertyPictureViewModel(internalItem, this);
                pictureViewModel.PropertyChanged += OnPropertyPicturePropertyChanged;
                Pictures.Add(pictureViewModel);
            }

            Pictures.CollectionChanged += (s, e) =>
            {
                // Unsubscribe from removed items
                if (e.OldItems != null)
                {
                    foreach (PropertyPictureViewModel item in e.OldItems)
                    {
                        Debug.WriteLine($"Item {item.Id} Removed from Pictures. @CollectionChanged in PopulateValues of PropertyViewModel");
                        IsDirty = true;

                        item.PropertyChanged -= OnPropertyPicturePropertyChanged;
                    }
                }

                // Subscribe to PropertyChanged.
                if (e.NewItems != null)
                {
                    foreach (PropertyPictureViewModel item in e.NewItems)
                    {
                        Debug.WriteLine($"Item {item.Id} Added to Pictures. @CollectionChanged in PopulateValues of PropertyViewModel");
                        IsDirty = true;

                        item.PropertyChanged += OnPropertyPicturePropertyChanged;
                    }
                }
            };

            // PDFs (don't use directly)
            Pdfs.Clear();
            foreach (var item in _property.Pdfs)
            {
                var internalItem = item.Clone();
                internalItem.BasePath = System.IO.Path.Combine(App.PropertyBlobDataFolder, _property.Id);
                if (isDuplicate)
                {
                    try
                    {
                        var newFilePath = System.IO.Path.Combine(internalItem.BasePath, internalItem.PdfFilename);
                        // The original item.BasePath should have been set in MainViewModel.
                        File.Copy(System.IO.Path.Combine(item.BasePath, item.PdfFilename), newFilePath, overwrite: true);
                        // Keep track of unsaved files to delete them when discarding.
                        _unsavedPropertyPdfFileList.Add(newFilePath);

                        // Copy the thumbnail file as well
                        if (!string.IsNullOrEmpty(item.ThumbnailFilename))
                        {
                            var thumbnailPath = Path.Combine(internalItem.BasePath, internalItem.ThumbnailFilename);
                            File.Copy(Path.Combine(item.BasePath, item.ThumbnailFilename), thumbnailPath, overwrite: true);
                            _unsavedPropertyPdfThumbnailFileList.Add(thumbnailPath);
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

                var pdfViewModel = new PropertyPdfViewModel(internalItem, this);
                pdfViewModel.PropertyChanged += OnPropertyPdfPropertyChanged;
                Pdfs.Add(pdfViewModel);
            }

            Pdfs.CollectionChanged += (sender, args) =>
            {
                if (args.OldItems is not null)
                {
                    foreach (PropertyPdfViewModel item in args.OldItems)
                    {
                        IsDirty = true;
                        item.PropertyChanged -= OnPropertyPdfPropertyChanged;
                    }
                }

                if (args.NewItems is not null)
                {
                    foreach (PropertyPdfViewModel item in args.NewItems)
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
                LessorsWrapper.Add(new ZumenSearch.Presentation.Features.Rent.Residentials.ViewModels.PropertyPersonViewModel(item, this));
            }

            // Brokers
            BrokersWrapper = [];
            foreach (var item in _property.Brokers)
            {
                BrokersWrapper.Add(new ZumenSearch.Presentation.Features.Rent.Residentials.ViewModels.PropertyPersonViewModel(item, this));
            }

            ListingSummaries = new ObservableCollection<ListingSummary>(_property.ListingSummaries);

        }
        catch (Exception ex)
        {
            Debug.WriteLine($"PopulateValues() failed: {ex}");

            var err = new ZumenSearch.Application.Results.ErrorInfo
            {
                Title = "建物のデータの取得中に予期しないエラーが発生しました。",
                Type = ZumenSearch.Application.Results.ErrorInfo.ErrType.Other,
                Code = "",
                Message = $"このまま保存するとデータに欠損が生じる可能性があります。",
                Description = $"{ex}",
                Operation = "Value Populatation",
                MethodName = nameof(PopulateValues),
                OccuredAt = DateTime.Now
            };

            ShowErrorInfoBar(err);
        }

        _property.SetIsModified(isDuplicate);
        IsDirty = isDuplicate;
    }

    private void OnPropertyPicturePropertyChanged(object? sender, PropertyChangedEventArgs e)
    {
        if (sender is not PropertyPictureViewModel picBldg)
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
        if (sender is not PropertyPdfViewModel pdfBldg)
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

    private static bool IsValidOptionalDigits(string value, int maxLength)
    {
        var normalized = ZumenSearch.Presentation.Utilities.Common.NormalizeFullWidthCharacters(value.Trim());
        return normalized.Length <= maxLength && normalized.All(character => character >= '0' && character <= '9');
    }

    private bool ValidateInput()
    {
        if (string.IsNullOrWhiteSpace(Name))
        {
            ShowSimpleErrorInfoBar("入力項目に誤りがあります","物件名（必須項目）が入力されていません。保存出来ませんでした。");
            IsNameHasError = true;
            if (!_navigationService.IsCurrentPageSameAs(BasicPageName))
            {
                _navigationService.NavigateTo(BasicPageName, this);
            }

            return false;
        }

        var realLength = new StringInfo(Name).LengthInTextElements;
        if (realLength > 100)
        {
            ShowSimpleErrorInfoBar("入力項目に誤りがあります","物件名は100文字以内で入力してください。保存出来ませんでした。");
            IsNameHasError = true;
            if (!_navigationService.IsCurrentPageSameAs(BasicPageName))
            {
                _navigationService.NavigateTo(BasicPageName, this);
            }

            return false;
        }

        IsNameHasError = false;

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

                const string pageName = NavigationKeys.RentResidentialPropertyTransportation;
                
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

        if (_property is null)
        {
            Debug.WriteLine("_property is null. Can't set room.");
            return false;
        }

        try
        {
            // 物件名
            _property.SetName(Name);

            _property.SetPropertyKind(SelectedPropertyKind.Key);
            _property.SetIsUnitLevelOwnership(IsUnitLevelOwnership);
            _property.SetPropertyStructure(SelectedStructureType.Key);

            _property.SetFloorCountAboveGroundFromString(ZumenSearch.Presentation.Utilities.Common.NormalizeFullWidthCharacters(FloorCountAboveGround));
            _property.SetFloorCountBasementFromString(ZumenSearch.Presentation.Utilities.Common.NormalizeFullWidthCharacters(FloorCountBasement));
            _property.SetTotalUnitCountFromString(ZumenSearch.Presentation.Utilities.Common.NormalizeFullWidthCharacters(TotalUnitCount));
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
            _property.Train1.SetEkiToho(ZumenSearch.Presentation.Utilities.Common.NormalizeFullWidthCharacters(EkiToho1));
            _property.SetBusStop1(BusStop1);
            _property.SetBusJyousya1(ZumenSearch.Presentation.Utilities.Common.NormalizeFullWidthCharacters(BusJyousya1));
            _property.SetBusStopToho1(ZumenSearch.Presentation.Utilities.Common.NormalizeFullWidthCharacters(BusStopToho1));
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
            _property.Train2.SetEkiToho(ZumenSearch.Presentation.Utilities.Common.NormalizeFullWidthCharacters(EkiToho2));
            _property.SetBusStop2(BusStop2);
            _property.SetBusJyousya2(ZumenSearch.Presentation.Utilities.Common.NormalizeFullWidthCharacters(BusJyousya2));
            _property.SetBusStopToho2(ZumenSearch.Presentation.Utilities.Common.NormalizeFullWidthCharacters(BusStopToho2));
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
            _property.Train3.SetEkiToho(ZumenSearch.Presentation.Utilities.Common.NormalizeFullWidthCharacters(EkiToho3));
            _property.SetBusStop3(BusStop3);
            _property.SetBusJyousya3(ZumenSearch.Presentation.Utilities.Common.NormalizeFullWidthCharacters(BusJyousya3));
            _property.SetBusStopToho3(ZumenSearch.Presentation.Utilities.Common.NormalizeFullWidthCharacters(BusStopToho3));
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
            _property.Train4.SetEkiToho(ZumenSearch.Presentation.Utilities.Common.NormalizeFullWidthCharacters(EkiToho4));
            _property.SetBusStop4(BusStop4);
            _property.SetBusJyousya4(ZumenSearch.Presentation.Utilities.Common.NormalizeFullWidthCharacters(BusJyousya4));
            _property.SetBusStopToho4(ZumenSearch.Presentation.Utilities.Common.NormalizeFullWidthCharacters(BusStopToho4));

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

            _property.SetKanriShutai(SelectedKanriShutai?.Key ?? ZumenSearch.Domain.Entities.Rent.Residentials.Property.EnumKanriShutai.Unspecified);
            _property.SetKanriIsMaintenanceManagementIfIsKanriJisya(KanriIsMaintenanceManagementIfIsKanriJisya);
            _property.SetKanriIsTenantManagementIfIsKanriJisya(KanriIsTenantManagementIfIsKanriJisya);
            _property.SetKanriIsPaymentManagementIfIsKanriJisya(KanriIsPaymentManagementIfIsKanriJisya);
            _property.SetKanriIsOtherManagementIfIsKanriJisya(KanriIsOtherManagementIfIsKanriJisya);
            _property.SetKanriRemarkIfIsKanriJisya(KanriRemarkIfIsKanriJisya);
            _property.SetKanriNameOfCompanyIfIsKanriTasya(KanriNameOfCompanyIfIsKanriTasya);
            _property.SetKanriContactInfoOfCompanyIfIsKanriTasya(KanriContactInfoOfCompanyIfIsKanriTasya);
            _property.SetKanriRemarkIfIsKanriTasya(KanriRemarkIfIsKanriTasya);
            _property.SetKanriRemarkIfIsKanriKashinushi(KanriRemarkIfIsKanriKashinushi);


            // Reset main ThumbnailImageFilePath here.
            _property.ThumbnailFilename = string.Empty;
            // Base dir for the thumbnail
            _property.BasePath = System.IO.Path.Combine(App.PropertyBlobDataFolder, _property.Id);

            // 写真
            _property.Pictures.Clear();
            foreach (var item in Pictures)
            {
                _property.Pictures.Add(item.Model.Clone());
            }

            var thumbImg = Pictures.FirstOrDefault(i => i.IsMain == true);
            if (thumbImg is not null)
            {
                // 物件写真サムネイルに指定
                _property.ThumbnailFilename = thumbImg.ImageFilename;
            }

            // 図面
            _property.Pdfs.Clear();
            foreach (var item in Pdfs)
            {
                _property.Pdfs.Add(item.Model.Clone());
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
        catch (Exception ex)
        {
            var err = new ZumenSearch.Application.Results.ErrorInfo
            {
                Title = "予期しないエラーが発生しました。",
                Type = ZumenSearch.Application.Results.ErrorInfo.ErrType.Other,
                Code = "",
                Message = ex.Message,
                Description = $"{ex}",
                Operation = "Input Validation",
                MethodName = nameof(SetValues),
                OccuredAt = DateTime.Now
            };
            ShowErrorInfoBar(err);
            return false;
        }

    }

    private void DiscardUnsavedFiles()
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

        /*
        if (failures.Count > 0)
        {
            ShowSimpleErrorInfoBar("エラー", $"変更の破棄中に一時ファイルを削除できませんでした: {string.Join(", ", failures)}");
        }
        */
        if (failures.Count == 0)
        {
            return;
        }

        // We are closing the window. No use showing an error message here,
        // because the user is discarding changes and we don't want to interrupt that process with an error message.
        // Instead, we show a message in the main window's info bar.
        WeakReferenceMessenger.Default.Send(new ShowErrorInfoBarInMainWindowMessage("変更の破棄中に一時ファイルを削除できませんでした", $"{string.Join(", ", failures)}"));

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

    #endregion

    #region == Commands ==

    #region == Save related commands ==

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
        if (!ValidateInput())
        {
            return;
        }

        if (!SetValues())
        {
            return;
        }

        ZumenSearch.Application.Results.ResultWrapper resUpsert;
        try
        {
            resUpsert = await Task.Run(() => _rentResidentialPropertyService.Save(_property), _cts.Token);
        }
        catch (OperationCanceledException) when (_cts.Token.IsCancellationRequested)
        {
            Debug.WriteLine("Save operation was canceled.");
            // The queued operation did not start. Do not report a database failure.

            return;
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

            return;
        }
        else
        {
            Debug.WriteLine("No errors on update.");

            IsDirty = false;

            await PostSaveCleanupAsync();
        }
    }

    private async Task PostSaveCleanupAsync()
    {
        try
        {
            // Update title with dummy value.
            WindowTitle = string.Empty;

            // Clear error infobar.
            IsInfoBarErrorOpen = false;

            // Clean up deleted picture file.
            if (_property.PicturesToBeDeleted.Count > 0)
            {
                foreach (var file in _property.PicturesToBeDeleted.ToList())
                {
                    var delFilePath = System.IO.Path.Combine(App.PropertyBlobDataFolder, _property.Id, file.ImageFilename);
                    if (TryDeleteFiles(new[] { delFilePath }, _cleanupFailures))
                    {
                        _property.PicturesToBeDeleted.Remove(file);// no need.
                    }
                }
                _property.PicturesToBeDeleted.Clear();
            }

            // Clean up deleted PDF and thumb file.
            if (_property.PdfsToBeDeleted.Count > 0)
            {
                foreach (var file in _property.PdfsToBeDeleted.ToList())
                {
                    var delFilePath = System.IO.Path.Combine(App.PropertyBlobDataFolder, _property.Id, file.PdfFilename);
                    var thumbFilePath = System.IO.Path.Combine(App.PropertyBlobDataFolder, _property.Id, file.ThumbnailFilename);
                    if (TryDeleteFiles(new[] { delFilePath, thumbFilePath }, _cleanupFailures))
                    {
                        _property.PdfsToBeDeleted.Remove(file);//no need
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
                foreach (var listing in _property.ListingsToBeDeleted.ToList())
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
                            _cleanupFailures.Add(delFolderPath);
                        }
                    }
                }
                _property.ListingsToBeDeleted.Clear();
            }

            // Clear unsaved files list since they are now saved.
            _unsavedPropertyPictureFileList.Clear();
            _unsavedPropertyPdfThumbnailFileList.Clear();
            _unsavedPropertyPdfFileList.Clear();

            //
            foreach (var picture in Pictures.Where(picture => _property.Pictures.Any(saved => saved.Id == picture.Id)))
            {
                picture.Model.SetStatus(EntityStatus.Saved);
                picture.Model.SetIsModified(false);
            }
            foreach (var pdf in Pdfs.Where(pdf => _property.Pdfs.Any(saved => saved.Id == pdf.Id)))
            {
                pdf.Model.SetStatus(EntityStatus.Saved);
                pdf.Model.SetIsModified(false);
            }

            // Clear unsaved listings and set status to saved.
            // These listings are handed off to the property when the property is New and the listings are new as well.
            foreach (var listing in _unsavedListingList)
            {
                // PicturesToBeDeleted, PdfsToBeDeleted, LessorsToBeDeleted, BrokersToBeDeleted are cleaned up in ListingViewModel.

                listing.SetPropertyName(Name);
                listing.SetPropertyStatus(EntityStatus.Saved);
                listing.SetStatus(EntityStatus.Saved);
                listing.SetIsModified(false);
            }
            _unsavedListingList.Clear();
            DuplicateSelectedListingCommand.NotifyCanExecuteChanged();

            // Clear unsaved child editors and quit tracking them.
            UnsavedChildEditorList.Clear();

            // 
            _property.SetStatus(EntityStatus.Saved);
            _property.SetIsModified(false);

            // Notify listing windows that the property has been updated.
            WeakReferenceMessenger.Default.Send(new ZumenSearch.Presentation.Messaging.PropertyUpdatedMessage(_property as ZumenSearch.Domain.Entities.Base.PropertyBase));

            if (_cleanupFailures.Count > 0)
            {
                ShowSimpleErrorInfoBar("保存しましたが、一部の削除済みファイルをクリーンアップできませんでした", $"{string.Join(", ", _cleanupFailures)}");
            }
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"Save() failed: {ex}");
            var err = new ZumenSearch.Application.Results.ErrorInfo
            {
                Title = "募集物件の保存中に予期しないエラーが発生しました。",
                Type = ZumenSearch.Application.Results.ErrorInfo.ErrType.Other,
                Code = "",
                Message = "",
                Description = $"{ex}",
                Operation = "SaveCore",
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

    #region == Pictures and Pdfs related commands ==

    [RelayCommand(CanExecute = nameof(CanAddNewPropertyPictures))]
    private async Task AddNewPropertyPictures(List<string> filePathList)
    {
        // File open is handleed in the code behind since this ViewModel does not know about the Window which is required for the Picker.

        if (filePathList is null) return;
        if (filePathList.Count == 0 || IsBusy) return;

        IsBusy = true;
        try
        {
            Debug.WriteLine($"destDirectory={_propertyDataDirectoryPath}  @AddNewPropertyPictures()");

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
                var destFilePath = Path.Combine(_propertyDataDirectoryPath, newFilename);
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

                var pic = new ZumenSearch.Domain.Entities.Rent.Residentials.PropertyPicture(newId, newFilename, EntityStatus.New)
                {
                    BasePath = _propertyDataDirectoryPath
                };

                Pictures.Add(new PropertyPictureViewModel(pic, this));

                OpenPropertyBlobDirectoryCommand.NotifyCanExecuteChanged();
                DeletePropertyPictureCommand.NotifyCanExecuteChanged();

                IsDirty = true;

                // Keep track of unsaved files to delete them when discarding.
                _unsavedPropertyPictureFileList.Add(destFilePath);
            }
        }
        finally
        {
            IsBusy = false;
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

    [RelayCommand]
    private async Task OpenPropertyPicture(PropertyPictureViewModel picProp)
    {
        if (picProp is null || !Pictures.Contains(picProp) || !IsEditorEnabled) return;

        IsBusy = true;
        try
        {
            var file = await StorageFile.GetFileFromPathAsync(Path.Combine(_propertyDataDirectoryPath, picProp.ImageFilename));
            if (!await Windows.System.Launcher.LaunchFileAsync(file))
            {
                ShowSimpleErrorInfoBar("エラー", "写真を開けませんでした。写真を開くアプリが設定されているか確認してください。");
            }
        }
        catch (Exception ex) when (ex is IOException or UnauthorizedAccessException or COMException or InvalidOperationException or ArgumentException)
        {
            Debug.WriteLine($"Failed to open residential property image: {ex}");
            ShowSimpleErrorInfoBar("エラー", $"写真を開けませんでした: {ex.Message}");
        }
        finally
        {
            IsBusy = false;
        }
    }

    [RelayCommand(CanExecute = nameof(CanDeletePropertyPicture))]
    private async Task DeletePropertyPicture(PropertyPictureViewModel picProp)
    {
        if (picProp is null)
        {
            return;
        }

        if (!IsEditorEnabled) return;
        IsBusy = true;
        try
        {
            if (await _dialogService.ShowDeleteConfirmationDialog("この物件写真を削除しますか？") != Microsoft.UI.Xaml.Controls.ContentDialogResult.Primary) return;

            if (Pictures.Remove(picProp))
            {
                _property.PicturesToBeDeleted.Add(picProp.Model);
                IsDirty = true;
            }
        }
        finally
        {
            IsBusy = false;
        }
    }
    private static bool CanDeletePropertyPicture(PropertyPictureViewModel picProp)
    {
        return picProp is not null;
    }

    [RelayCommand(CanExecute = nameof(CanDeleteAllPropertyPictures))]
    private async Task PropertyPictureDeleteAll()
    {
        if (!IsEditorEnabled) return;
        IsBusy = true;
        try
        {
            if (await _dialogService.ShowDeleteConfirmationDialog("この物件写真を全て削除しますか？") != Microsoft.UI.Xaml.Controls.ContentDialogResult.Primary) return;
            foreach (var picProp in Pictures.ToList())
            {
                if (Pictures.Remove(picProp))
                {
                    _property.PicturesToBeDeleted.Add(picProp.Model);
                    IsDirty = true;
                }
            }
        }
        finally
        {
            IsBusy = false;
        }
    }
    private bool CanDeleteAllPropertyPictures()
    {
        if (Pictures.Count == 0) return false;
        return true;
    }

    [RelayCommand(CanExecute = nameof(CanAddNewPropertyPdfs))]
    private async Task AddNewPropertyPdfs(List<string> filePathList)
    {
        if (filePathList is null) return;
        if (filePathList.Count == 0 || IsUnitLevelOwnership || IsBusy) return;
        IsBusy = true;
        try
        {
            Debug.WriteLine($"destDirectory={_propertyDataDirectoryPath}  @AddNewPropertyPdfs()");

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
                var thumbnailDestFilePath = Path.Combine(_propertyDataDirectoryPath, newThumbnailFilename);
                bool thumbnailDestinationCreated = false;

                string newFilename = newId + extension;
                var destFilePath = Path.Combine(_propertyDataDirectoryPath, newFilename);
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
                catch (Exception ex) when (ex is IOException or UnauthorizedAccessException or COMException or InvalidOperationException or ArgumentException)
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

                var pdf = new ZumenSearch.Domain.Entities.Rent.Residentials.PropertyPdf(newId, newFilename, newThumbnailFilename, EntityStatus.New)
                {
                    BasePath = _propertyDataDirectoryPath
                };

                Pdfs.Add(new PropertyPdfViewModel(pdf, this));
                _unsavedPropertyPdfThumbnailFileList.Add(thumbnailDestFilePath);
                _unsavedPropertyPdfFileList.Add(destFilePath);

                OpenPropertyBlobDirectoryCommand.NotifyCanExecuteChanged();
                DeletePropertyPdfCommand.NotifyCanExecuteChanged();
                IsDirty = true;
            }
        }
        finally
        {
            IsBusy = false;
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
    private async Task OpenPropertyPdf(PropertyPdfViewModel pdfProp)
    {
        if (pdfProp is null || !Pdfs.Contains(pdfProp) || !IsEditorEnabled) return;

        IsBusy = true;
        try
        {
            var file = await StorageFile.GetFileFromPathAsync(Path.Combine(_propertyDataDirectoryPath, pdfProp.PdfFilename));
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
        finally
        {
            IsBusy = false;
        }
    }

    [RelayCommand(CanExecute = nameof(CanDeletePropertyPdf))]
    private async Task DeletePropertyPdf(PropertyPdfViewModel pdfProp)
    {
        if (pdfProp is null)
        {
            return;
        }

        if (!IsEditorEnabled) return;
        IsBusy = true;
        try
        {
            if (await _dialogService.ShowDeleteConfirmationDialog("この建物図面を削除しますか？") != Microsoft.UI.Xaml.Controls.ContentDialogResult.Primary) return;

            if (Pdfs.Remove(pdfProp))
            {
                _property.PdfsToBeDeleted.Add(pdfProp.Model);
                IsDirty = true;
            }
        }
        finally
        {
            IsBusy = false;
        }
    }
    private static bool CanDeletePropertyPdf(PropertyPdfViewModel pdfProp)
    {
        return pdfProp is not null;
    }

    [RelayCommand(CanExecute = nameof(CanDeleteAllPropertyPdfs))]
    private async Task PropertyPdfDeleteAll()
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
                    _property.PdfsToBeDeleted.Add(pdfProp.Model);
                    IsDirty = true;
                }
            }
        }
        finally
        {
            IsBusy = false;
        }
    }
    private bool CanDeleteAllPropertyPdfs()
    {
        if (Pdfs.Count == 0) return false;
        return true;
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
        var editorShell = _shellFactory.Create(new ZumenSearch.Domain.Entities.Rent.Residentials.Listing(newId, EntityStatus.New, _property.Id, _property.Status, _property.IsUnitLevelOwnership, Name, _property.FudousanId), false, false);

        // Apply the current IsUnitOwnership state because it may not be saved to the _room YET.
        //editorShell.ViewModel.SetIsPropertyUnitOwnership(this.IsUnitOwnership);
        // Use message. If this is set, then show/hide the owner and zumen from shell menu.
        WeakReferenceMessenger.Default.Send(new ZumenSearch.Presentation.Messaging.PropertyIsUnitOwnershipChangedMessage(_id, this.IsUnitLevelOwnership));

        var mainVM = App.GetService<ZumenSearch.Presentation.Shell.ViewModels.MainViewModel>();
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

        IsBusy = true;
        try
        {
            await EditSelectedListingCore(listingSummary);
        }
        finally
        {
            IsBusy = false;
        }
    }

    private async Task EditSelectedListingCore(ListingSummary listingSummary)
    {
        var unitId = listingSummary.Id;
        var mainVM = App.GetService<ZumenSearch.Presentation.Shell.ViewModels.MainViewModel>();

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

                if (editWin.Content is ZumenSearch.Presentation.Features.Rent.Residentials.Views.Listing.ShellPage editShell)
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

        var listing = _unsavedListingList.FirstOrDefault(item => item.Id.Equals(unitId, StringComparison.Ordinal));
        var isHandedOffToParentEditor = listing is not null;
        if (listing is null)
        {
            var result = await Task.Run(() => _rentResidentialListingService.GetById(_id, unitId), _cts.Token);
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
        }
        var editorShell = _shellFactory.Create(listing, false, isHandedOffToParentEditor);

        // Apply the current IsUnitOwnership state because it may not be saved to the _listing YET.
        //editorShell.ViewModel.SetIsPropertyUnitOwnership(this.IsUnitOwnership);
        // Use message. If this is set, then show/hide the owner and zumen from shell menu.
        WeakReferenceMessenger.Default.Send(new ZumenSearch.Presentation.Messaging.PropertyIsUnitOwnershipChangedMessage(_id, this.IsUnitLevelOwnership));

        var editorWindow = editorShell.Window;
        if (editorWindow == null)
        {
            // EditorWin should be initialized in the EditorShell constructor.
            Debug.WriteLine("EditorWin must be initialized in the EditorShell constructor");
            return;
        }

        mainVM.RentResidentialListingEditorList.Add(editorWindow);

        if (isHandedOffToParentEditor)
        {
            UnsavedChildEditorList.Add(editorWindow);
        }

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

        if (editorWindow.Content is ZumenSearch.Presentation.Features.Rent.Residentials.Views.Listing.ShellPage shell)
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

    [RelayCommand(CanExecute = nameof(DuplicateSelectedListingCanExecute))]
    private async Task DuplicateSelectedListing(ListingSummary listingSummary)
    {
        if (!DuplicateSelectedListingCanExecute(listingSummary) || !ListingSummaries.Contains(listingSummary) || !IsEditorEnabled) return;

        IsBusy = true;
        try
        {
            // Load the database version, not unsaved changes in an open listing editor.
            var result = await Task.Run(() => _rentResidentialListingService.GetById(_id, listingSummary.Id, true), _cts.Token);
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

            // PopulateValues copies the original media into the duplicate's directory.
            var sourceDirectory = Path.Combine(App.PropertyBlobDataFolder, _id, listingSummary.Id);
            foreach (var picture in result.Listing.Pictures) picture.BasePath = sourceDirectory;
            foreach (var pdf in result.Listing.Pdfs) pdf.BasePath = sourceDirectory;

            var editorShell = _shellFactory.Create(result.Listing, true, false);
            editorShell.ViewModel.SetOwnershipVisibility(IsUnitLevelOwnership);
            var editorWindow = editorShell.Window;
            var mainVM = App.GetService<ZumenSearch.Presentation.Shell.ViewModels.MainViewModel>();
            mainVM.RentResidentialListingEditorList.Add(editorWindow);
            editorWindow.AppWindow.MoveAndResize(new Windows.Graphics.RectInt32(mainVM.RentResidentialListingEditorWinLeft, mainVM.RentResidentialListingEditorWinTop, mainVM.RentResidentialListingEditorWinWidth, mainVM.RentResidentialListingEditorWinHeight));
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
            editorWindow.AppWindow.MoveInZOrderAtTop();
        }
        finally
        {
            IsBusy = false;
        }
    }
    public bool DuplicateSelectedListingCanExecute(ListingSummary? listingSummary)
    {
        return listingSummary is not null && !_unsavedListingList.Any(item => item.Id.Equals(listingSummary.Id, StringComparison.Ordinal));
    }

    [RelayCommand(CanExecute = nameof(DeleteSelectedListingCanExecute))]
    private async Task DeleteSelectedListing(ListingSummary listingSummary)
    {
        if (listingSummary is null)
        {
            return;
        }

        // 
        var mainVM = App.GetService<ZumenSearch.Presentation.Shell.ViewModels.MainViewModel>();

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
                ShowSimpleErrorInfoBar("募集物件の編集画面が開いています","募集物件の編集画面を閉じてから、募集物件を削除してください。");

                editWin.Activate();

                if (editWin.Content is ZumenSearch.Presentation.Features.Rent.Residentials.Views.Listing.ShellPage editShell)
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

        if (!IsEditorEnabled || !ListingSummaries.Contains(listingSummary)) return;
        IsBusy = true;
        try
        {
            if (await _dialogService.ShowDeleteConfirmationDialog("この募集物件を直ちに削除しますか？建物の変更を破棄しても元に戻せません。") != Microsoft.UI.Xaml.Controls.ContentDialogResult.Primary) return;

            var isUnsaved = _unsavedListingList.Any(item => item.Id.Equals(listingSummary.Id, StringComparison.Ordinal));
            if (!isUnsaved)
            {
                var result = await Task.Run(() => _rentResidentialListingService.Delete(listingSummary.Id), _cts.Token);
                if (result.IsError)
                {
                    ShowErrorInfoBar(result.Error);
                    return;
                }
            }

            ListingSummaries.Remove(listingSummary);
            _property.ListingSummaries.RemoveAll(item => item.Id.Equals(listingSummary.Id, StringComparison.Ordinal));
            _unsavedListingList.RemoveAll(item => item.Id.Equals(listingSummary.Id, StringComparison.Ordinal));
            DuplicateSelectedListingCommand.NotifyCanExecuteChanged();
            mainVM.RemoveResidentialListingSearchResults(_id, listingSummary.Id);

            var listingDataDirectoryPath = Path.Combine(_propertyDataDirectoryPath, listingSummary.Id);
            try
            {
                if (Directory.Exists(listingDataDirectoryPath)) Directory.Delete(listingDataDirectoryPath, true);
            }
            catch (Exception ex) when (ex is IOException or UnauthorizedAccessException)
            {
                Debug.WriteLine($"Failed to delete listing attachments '{listingDataDirectoryPath}': {ex}");
                ShowSimpleErrorInfoBar("添付ファイルの削除に失敗しました", $"募集物件は削除されましたが、添付ファイルを削除できませんでした: {listingDataDirectoryPath}。{ex.Message}");
            }
        }
        finally
        {
            IsBusy = false;
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

        if (!IsEditorEnabled) return;
        IsBusy = true;
        try
        {
            var uriGoogleMaps = new Uri($"https://maps.google.co.jp/?q={LocationLatitude},{LocationLongitude}");
            await Launcher.LaunchUriAsync(uriGoogleMaps);
        }
        finally
        {
            IsBusy = false;
        }
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

        if (!IsEditorEnabled) return;
        IsBusy = true;
        try
        {
            var railLine = await _dialogService.ShowRailLineSelectDialog();
            if (railLine is not null && !string.Equals(SelectedRailLine1.LineCode, railLine.LineCode, StringComparison.Ordinal)) SelectedRailLine1 = railLine;
        }
        finally
        {
            IsBusy = false;
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

        if (!IsEditorEnabled) return;
        IsBusy = true;
        try
        {
            var lineCode = SelectedRailLine1.LineCode;
            var railStation = await _dialogService.ShowRailStationSelectDialog(lineCode);
            if (railStation is not null && string.Equals(SelectedRailLine1.LineCode, lineCode, StringComparison.Ordinal) && string.Equals(railStation.LineCode, lineCode, StringComparison.Ordinal)) SelectedRailStation1 = railStation;
        }
        finally
        {
            IsBusy = false;
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

        if (!IsEditorEnabled) return;
        IsBusy = true;
        try
        {
            var railLine = await _dialogService.ShowRailLineSelectDialog();
            if (railLine is not null && !string.Equals(SelectedRailLine2.LineCode, railLine.LineCode, StringComparison.Ordinal)) SelectedRailLine2 = railLine;
        }
        finally
        {
            IsBusy = false;
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

        if (!IsEditorEnabled) return;
        IsBusy = true;
        try
        {
            var lineCode = SelectedRailLine2.LineCode;
            var railStation = await _dialogService.ShowRailStationSelectDialog(lineCode);
            if (railStation is not null && string.Equals(SelectedRailLine2.LineCode, lineCode, StringComparison.Ordinal) && string.Equals(railStation.LineCode, lineCode, StringComparison.Ordinal)) SelectedRailStation2 = railStation;
        }
        finally
        {
            IsBusy = false;
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

        if (!IsEditorEnabled) return;
        IsBusy = true;
        try
        {
            var railLine = await _dialogService.ShowRailLineSelectDialog();
            if (railLine is not null && !string.Equals(SelectedRailLine3.LineCode, railLine.LineCode, StringComparison.Ordinal)) SelectedRailLine3 = railLine;
        }
        finally
        {
            IsBusy = false;
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

        if (!IsEditorEnabled) return;
        IsBusy = true;
        try
        {
            var lineCode = SelectedRailLine3.LineCode;
            var railStation = await _dialogService.ShowRailStationSelectDialog(lineCode);
            if (railStation is not null && string.Equals(SelectedRailLine3.LineCode, lineCode, StringComparison.Ordinal) && string.Equals(railStation.LineCode, lineCode, StringComparison.Ordinal)) SelectedRailStation3 = railStation;
        }
        finally
        {
            IsBusy = false;
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

        if (!IsEditorEnabled) return;
        IsBusy = true;
        try
        {
            var railLine = await _dialogService.ShowRailLineSelectDialog();
            if (railLine is not null && !string.Equals(SelectedRailLine4.LineCode, railLine.LineCode, StringComparison.Ordinal)) SelectedRailLine4 = railLine;
        }
        finally
        {
            IsBusy = false;
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

        if (!IsEditorEnabled) return;
        IsBusy = true;
        try
        {
            var lineCode = SelectedRailLine4.LineCode;
            var railStation = await _dialogService.ShowRailStationSelectDialog(lineCode);
            if (railStation is not null && string.Equals(SelectedRailLine4.LineCode, lineCode, StringComparison.Ordinal) && string.Equals(railStation.LineCode, lineCode, StringComparison.Ordinal)) SelectedRailStation4 = railStation;
        }
        finally
        {
            IsBusy = false;
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

            foreach (var deleted in _property.LessorsToBeDeleted.Where(person => person.Id == res.Person.Id).ToList()) _property.LessorsToBeDeleted.Remove(deleted);

            var lessorWrapper = new ZumenSearch.Presentation.Features.Rent.Residentials.ViewModels.PropertyPersonViewModel(res.Person, this);
            LessorsWrapper.Add(lessorWrapper);

            IsDirty = true;
        }
        finally
        {
            IsBusy = false;
        }
    }

    [RelayCommand(CanExecute = nameof(CanEditLessor))]
    private void EditLessor(ZumenSearch.Presentation.Features.Rent.Residentials.ViewModels.PropertyPersonViewModel lessor)
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
    private static bool CanEditLessor(ZumenSearch.Presentation.Features.Rent.Residentials.ViewModels.PropertyPersonViewModel lessor)
    {
        return lessor is not null;
    }

    [RelayCommand(CanExecute = nameof(CanDeleteLessor))]
    public async Task DeleteLessor(ZumenSearch.Presentation.Features.Rent.Residentials.ViewModels.PropertyPersonViewModel lessor)
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
            if (!_property.LessorsToBeDeleted.Any(person => person.Id == lessor.Person.Id)) _property.LessorsToBeDeleted.Add(lessor.Person);
        }
    }
    private static bool CanDeleteLessor(ZumenSearch.Presentation.Features.Rent.Residentials.ViewModels.PropertyPersonViewModel lessor)
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

        if (!IsEditorEnabled) return;
        IsBusy = true;

        try
        {
            var broker = await _dialogService.ShowBrokerSelectDialog(new ZumenSearch.Presentation.Dialogs.ViewModels.BrokerSelectViewModel(_personService, _cts));

            if (broker is null)
            {
                return;
            }

            var brokerId = broker.Id;

            // Check if already exists
            var match = BrokersWrapper.FirstOrDefault(x => x.Person.Id.Equals(brokerId, StringComparison.Ordinal));
            if (match is not null)
            {
                Debug.WriteLine($"broker {broker.Name} already in the list.");
                return;
            }

            // TODO: Catch and ignore catch (OperationCanceledException) when (_cts.Token.IsCancellationRequested). This may not be necessary...but.
            var res = await Task.Run(() => _personService.GetBrokerById(brokerId), _cts.Token);
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
                ShowSimpleErrorInfoBar("予期しないエラーが発生しました", "選択した宅建業者を読み込めませんでした。");
                return;
            }

            foreach (var deleted in _property.BrokersToBeDeleted.Where(person => person.Id == res.Person.Id).ToList()) _property.BrokersToBeDeleted.Remove(deleted);

            BrokersWrapper.Add(new ZumenSearch.Presentation.Features.Rent.Residentials.ViewModels.PropertyPersonViewModel(res.Person, this));

            IsDirty = true;
        }
        finally
        {
            IsBusy = false;
        }
    }

    [RelayCommand(CanExecute = nameof(CanEditBroker))]
    private void EditBroker(ZumenSearch.Presentation.Features.Rent.Residentials.ViewModels.PropertyPersonViewModel broker)
    {
        if (broker.Person is not null)
        {
            var mainVm = App.GetService<MainViewModel>();
            if (mainVm.EditBrokerCommand.CanExecute(broker.Person as ZumenSearch.Domain.Entities.Base.PersonBase))
            {
                mainVm.EditBrokerCommand.Execute(broker.Person as ZumenSearch.Domain.Entities.Base.PersonBase);
            }
        }
    }
    private static bool CanEditBroker(ZumenSearch.Presentation.Features.Rent.Residentials.ViewModels.PropertyPersonViewModel broker)
    {
        return broker is not null;
    }

    [RelayCommand(CanExecute = nameof(CanDeleteBroker))]
    public async Task DeleteBroker(ZumenSearch.Presentation.Features.Rent.Residentials.ViewModels.PropertyPersonViewModel broker)
    {
        if (broker is null)
        {
            return;
        }

        if (broker.Person is null)
        {
            return;
        }

        if (await _dialogService.ShowDeleteConfirmationDialog("この宅建業者を削除しますか？") != Microsoft.UI.Xaml.Controls.ContentDialogResult.Primary) return;

        Debug.WriteLine($"DeleteBrokerCommand {broker.Person.Name}");

        if (BrokersWrapper.Remove(broker))
        {
            IsDirty = true;
            if (!_property.BrokersToBeDeleted.Any(person => person.Id == broker.Person.Id)) _property.BrokersToBeDeleted.Add(broker.Person);
        }
    }
    private static bool CanDeleteBroker(ZumenSearch.Presentation.Features.Rent.Residentials.ViewModels.PropertyPersonViewModel broker)
    {
        return broker is not null;
    }

    #endregion

    #endregion

}
