using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.Messaging;
using Microsoft.UI.Windowing;
using Microsoft.UI.Xaml.Data;
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
using ZumenSearch.Models.Enums;
using ZumenSearch.Models.Location;
using ZumenSearch.Models.Messenger;
using ZumenSearch.Models.Transportation;
using ZumenSearch.Services.Contracts;
using ZumenSearch.Services.Extensions.AbstractFactory;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace ZumenSearch.ViewModels.Rent.Residentials;

public sealed partial class PropertyViewModel : ObservableRecipient, 
    IRecipient<ListingUpdatedMessage>, 
    IRecipient<WindowClosedMessage>, 
    IRecipient<ListingDeletedMessage>,
    IRecipient<LessorUpdatedMessage>,
    IRecipient<BrokerUpdatedMessage>,
    IRecipient<LessorDeletedMessage>,
    IRecipient<BrokerDeletedMessage>,
    IDisposable
{
    #region == Private variables and const ==

    private const string BasicPageName = "ZumenSearch.Views.Rent.Residentials.BasicPage";

    private readonly string _id = string.Empty;

    // This property holds the COPY of current entity being edited.
    // Do not use it directly in the UI. Apply changes to this object in Save() to save the changes.
    private readonly Models.Rent.Residentials.Property _building;

    private readonly string _propertyDataDirectoryPath = string.Empty;

    // Keeps truck of "New" child window because it saves to the parent window/viewmodel.
    public List<Views.Rent.Residentials.Listing.EditorWindow> UnsavedChildEditorList { get;} = [];

    // Tmp file list to hold unsaved picture files. (if entry is not saved, delete on close)
    private readonly List<string> _unsavedBuildingPictureFileList = [];
    private readonly List<string> _unsavedBuildingPdfFileList = [];
    private readonly List<string> _unsavedBuildingPdfThumbnailFileList = [];

    private readonly CancellationTokenSource _cts = new();

    #endregion

    #region == Services ==

    private readonly IAbstractFactory<Models.Rent.Residentials.Listing, Views.Rent.Residentials.Listing.ShellPage> _shellFactory;
    private readonly IDataAccessService _dataAccessService;
    private readonly IDataAccessLocationService _dataAccessLocationService;
    private readonly IDispatcherService _dispatcherService;
    private readonly IDialogGenericService _dialogService;
    private readonly INavigationGenericService _navigationService;

    #endregion

    public PropertyViewModel(
        Models.Rent.Residentials.Property building, 
        INavigationGenericService navigationService,
        IDialogGenericService dialogService,
        IAbstractFactory<Models.Rent.Residentials.Listing, Views.Rent.Residentials.Listing.ShellPage> shellFactory, 
        IDispatcherService dispatcherService, 
        IDataAccessService dataAccessService, 
        IDataAccessLocationService dataAccessLocationService)
    {
        _building = building;
        _id = building.Id;

        _navigationService = navigationService;
        _dialogService = dialogService;
        _shellFactory = shellFactory;
        _dispatcherService = dispatcherService;
        _dataAccessService = dataAccessService;
        _dataAccessLocationService = dataAccessLocationService;

        _propertyDataDirectoryPath = System.IO.Path.Combine(App.PropertyBlobDataFolder, _id);

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

                ValidateName();

                // Update title with dummy value.
                WindowTitle = string.Empty;

                // Moved to after the save.
                //_selectedSearchResult?.Name = value; // Update the selected search result's name if it exists
            }
        }
    }

    // 物件種別
    public ObservableCollection<Models.Rent.Residentials.PropertyTypeLabel> PropertyTypes { get; set; } =
    [
        //new Kind(EnumKinds.Unspecified.ToString(), "未指定"),
        new Models.Rent.Residentials.PropertyTypeLabel(Models.Rent.Residentials.PropertyType.Apartment),
        new Models.Rent.Residentials.PropertyTypeLabel(Models.Rent.Residentials.PropertyType.Mansion),
        new Models.Rent.Residentials.PropertyTypeLabel(Models.Rent.Residentials.PropertyType.House),
        new Models.Rent.Residentials.PropertyTypeLabel(Models.Rent.Residentials.PropertyType.TerraceHouse),
        new Models.Rent.Residentials.PropertyTypeLabel(Models.Rent.Residentials.PropertyType.TownHouse),
        new Models.Rent.Residentials.PropertyTypeLabel(Models.Rent.Residentials.PropertyType.ShareHouse),
        new Models.Rent.Residentials.PropertyTypeLabel(Models.Rent.Residentials.PropertyType.Dormitory)
    ];

    public Models.Rent.Residentials.PropertyTypeLabel SelectedPropertyType
    {
        get => field ?? new(Models.Rent.Residentials.PropertyType.Unspecified);
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
    public bool IsUnitOwnership
    {
        get;
        set
        {
            if (SetProperty(ref field, value))
            {
                IsDirty = true;

                IsUnitOwnershipVisible = !value;

                // If this is set, then show/hide the owner and zumen from shell menu.
                //EventIsUnitOwnershipChanged?.Invoke(this, field);
                WeakReferenceMessenger.Default.Send(new Models.Messenger.PropertyIsUnitOwnershipChangedMessage(value));
            }
        }
    }

    [ObservableProperty]
    public partial bool IsUnitOwnershipVisible { get; private set; } = true;

    // 建物構造
    public ObservableCollection<Models.Rent.Residentials.PropertyStructureTypeLabel> Structures { get; set; } =
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
    ];

    public Models.Rent.Residentials.PropertyStructureTypeLabel SelectedStructure
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
            if (field == value)
            {
                return;
            }

            if (value is null)
            {
                return;
            }

            var text = value.Trim();
            if (string.IsNullOrEmpty(text))
            {
                field = string.Empty;
                IsDirty = true;
                OnPropertyChanged();
                return;
            }

            /*
            text = Helpers.Common.ReplaceZenkakuNumbers(text);

            if (string.IsNullOrEmpty(text))
            {
                // Do nothing.
                return;
            }
            */

            //var regex = new Regex(@"^\d{0,4}$"); // 4 digit or less. (Alow zenkaku Full-Width)
            var regex = new Regex(@"^(?:\d{0,4})?$");
            if (regex.IsMatch(text))
            {
                field = text;
                IsDirty = true;
                OnPropertyChanged();
            }
            else
            {
                Debug.WriteLine($"FloorCountAboveGround: not * digits");
                // TODO: show err?
            }
            /*
            if (Helpers.Common.CanConvertToPositiveNumber(text))
            {
                field = text;
                IsDirty = true;
                OnPropertyChanged();
            }
            else
            {
                // Do nothing.
                return;
            }
            */
        }
    }

    // 地下階
    public string FloorCountBasement
    {
        get => field ?? string.Empty;
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

            var text = value.Trim();
            if (string.IsNullOrEmpty(text))
            {
                field = string.Empty;
                IsDirty = true;
                OnPropertyChanged();
                return;
            }

            //var regex = new Regex(@"^\d{0,4}$"); // 4 digit or less. (Alow zenkaku Full-Width)
            var regex = new Regex(@"^(?:\d{0,4})?$");
            if (regex.IsMatch(text))
            {
                field = text;
                IsDirty = true;
                OnPropertyChanged();
            }
            else
            {
                Debug.WriteLine($"FloorCountBasement: not * digits");
                // TODO: show err?
            }
        }
    }

    // 総戸数
    public string TotalUnitCount
    {
        get => field ?? string.Empty;
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

            var text = value.Trim();
            if (string.IsNullOrEmpty(text))
            {
                field = string.Empty;
                IsDirty = true;
                OnPropertyChanged();
                return;
            }

            //var regex = new Regex(@"^\d{0,5}$"); // 5 digit or less. (Alow zenkaku Full-Width)
            var regex = new Regex(@"^(?:\d{0,5})?$");
            if (regex.IsMatch(text))
            {
                field = text;
                IsDirty = true;
                OnPropertyChanged();
            }
            else
            {
                Debug.WriteLine($"TotalUnitCount: not * digits");
                // TODO: show err?
            }
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

    // 不動産ID (13桁)
    public string FudousanId
    {
        get => field ?? string.Empty;
        set
        {
            // TODO: check 13桁.

            if (field == value)
            {
                return;
            }

            if (value is null)
            {
                return;
            }

            var text = value.Trim();
            if (string.IsNullOrEmpty(text))
            {
                field = string.Empty;
                IsDirty = true;
                OnPropertyChanged();
                return;
            }

            var regex = new Regex(@"^(?:\d{13})?$");
            if (regex.IsMatch(text))
            {
                field = text;
                IsDirty = true;
                OnPropertyChanged();
            }
            else
            {
                //Debug.WriteLine($"FudousanId: not 13 digits");
                // TODO: show err?
            }

            /*
            if (Helpers.Common.CanConvertToPositiveNumber(text))
            {
                field = text;
                IsDirty = true;
                OnPropertyChanged();
            }
            else
            {
                // Do nothing.
                return;
            }
            */
        }
    }

    // 特定コード（４桁）建物全体は0000
    public string FudousanIdAdditionalCode
    {
        get => field ?? string.Empty; // keep non-null empty string.
        set
        {
            // TODO: check （４桁）

            if (field == value)
            {
                return;
            }

            if (value is null)
            {
                return;
            }

            var text = value.Trim();
            if (string.IsNullOrEmpty(text))
            {
                field = string.Empty;
                IsDirty = true;
                OnPropertyChanged();
                return;
            }

            var regex = new Regex(@"^(?:\d{4})?$");
            if (regex.IsMatch(text))
            {
                field = text;
                IsDirty = true;
                OnPropertyChanged();
            }
            else
            {
                //Debug.WriteLine($"FudousanIdAdditionalCode: not 4 digits");
                // TODO: show err?
            }
            /*
            if (Helpers.Common.CanConvertToPositiveNumber(text))
            {
                field = text;
                IsDirty = true;
                OnPropertyChanged();
            }
            else
            {
                // Do nothing.
                return;
            }
            */
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

    public Prefecture? SelectedPef
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

                if (SelectedPef is null)
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

                dataset = _dataAccessLocationService.GetWardAndOazaByPrefCountyCity(SelectedPef.Name, field.County, field.City);

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

                if (SelectedPef is null)
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

                dataset = _dataAccessLocationService.GetChoumeByPrefCountyCityWardOaza(SelectedPef.Name, SelectedCity.County, SelectedCity.City, field.Ward, field.Oaza);

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
            if (SelectedPef == null)
            {
                return string.Empty;
            }

            var s = string.Empty;
            if (!string.IsNullOrEmpty(Edaban))
            {
                s = "-" + Edaban;
            }
            // TODO:
            return $"{SelectedPef.Name}{SelectedCity?.Combined}{SelectedTown?.Combined}{SelectedChou?.Chou}{s}";
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

    public RailLine? SelectedRailLine1
    {
        get;
        set
        {
            if (SetProperty(ref field, value))
            {
                IsDirty = true;
                // Clear  old value.
                SelectedRailStation1 = null;
            }

            ShowRailStationSelect1Command.NotifyCanExecuteChanged();
        }
    }

    public RailStation? SelectedRailStation1
    {
        get;
        set
        {
            if ((value is null) && (field is not null))
            {
                // Clear old value.
                field.StationName = string.Empty;
                OnPropertyChanged(nameof(SelectedRailStation1));
            }

            if (SetProperty(ref field, value))
            {
                IsDirty = true;
            }
        }
    }

    public string EkiToho1
    {
        get;
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
            var text = value.Trim();
            if (string.IsNullOrEmpty(text))
            {
                field = string.Empty;
                IsDirty = true;
                OnPropertyChanged();
                return;
            }

            //var regex = new Regex(@"^\d{0,4}$"); // 4 digit or less. (Alow zenkaku Full-Width)
            var regex = new Regex(@"^(?:\d{0,4})?$");
            if (regex.IsMatch(text))
            {
                field = text;
                IsDirty = true;
                OnPropertyChanged();
            }
            else
            {
                //Debug.WriteLine($"EkiToho1: not * digits");
                // TODO: show err?
            }
        }
    } = string.Empty;

    public string BusStop
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
            if (field == value)
            {
                return;
            }

            if (value is null)
            {
                return;
            }

            var text = value.Trim();
            if (string.IsNullOrEmpty(text))
            {
                field = string.Empty;
                IsDirty = true;
                OnPropertyChanged();
                return;
            }

            //var regex = new Regex(@"^\d{0,4}$"); // 4 digit or less. (Alow zenkaku Full-Width)
            var regex = new Regex(@"^(?:\d{0,4})?$");
            if (regex.IsMatch(text))
            {
                field = text;
                IsDirty = true;
                OnPropertyChanged();
            }
            else
            {
                //Debug.WriteLine($"BusJyousya1: not * digits");
                // TODO: show err?
            }
        }
    } = string.Empty;

    public string BusStopToho1
    {
        get;
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

            var text = value.Trim();
            if (string.IsNullOrEmpty(text))
            {
                field = string.Empty;
                IsDirty = true;
                OnPropertyChanged();
                return;
            }

            //var regex = new Regex(@"^\d{0,4}$"); // 4 digit or less. (Alow zenkaku Full-Width)
            var regex = new Regex(@"^(?:\d{0,4})?$");
            if (regex.IsMatch(text))
            {
                field = text;
                IsDirty = true;
                OnPropertyChanged();
            }
            else
            {
                //Debug.WriteLine($"BusJyousya1: not * digits");
                // TODO: show err?
            }
        }
    } = string.Empty;

    // TODO: more.

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
        get;
        set
        {
            if (SetProperty(ref field, value))
            {
                IsDirty = true;
            }
        }
    } = new Models.Rent.Residentials.PropertyElectricKind(Models.Rent.Residentials.Property.EnumElectricType.Unspecified, "未指定");

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

    public ObservableCollection<Models.Rent.Residentials.Listing> Rooms
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
        var something = listing.Value;
        if (something is null)
        {
            return;
        }

        if (something is not Models.Rent.Residentials.Listing room)
        {
            return;
        }

        var existingRoom = this.Rooms.FirstOrDefault(r => r.Id.Equals(room.Id, StringComparison.Ordinal));
        if (existingRoom is not null)
        {
            // Update existing room
            var index = this.Rooms.IndexOf(existingRoom);
            this.Rooms[index] = room;
        }
        else
        {
            // Add new room
            this.Rooms.Add(room);
        }

        if (room.Status == EntityStatus.New)
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

        psn.Person = lessor; //= new PersonWrapperForPropertyViewModel(lessor, this);
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

    public void Receive(ListingDeletedMessage listingId)
    {
        var id = listingId.Value;
        if (string.IsNullOrEmpty(id))
        {
            return;
        }

        var room = Rooms.FirstOrDefault(r => r.Id.Equals(id, StringComparison.Ordinal));
        if (room is null) return;
        Rooms.Remove(room);
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

        // should be auto deleted from the table due to "cascade"
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

        // should be auto deleted from the table due to "cascade"
    }

    public void Receive(WindowClosedMessage window)
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

    public void DiscardChanges()
    {
        DiscardUnsavedFiles();

        IsDirty = false;
    }

    public void CleanUp()
    {
        // Unsubscribe
        //WeakReferenceMessenger.Default.UnregisterAll(this);
        //or
        //this.IsActive = false;

        _cts.Cancel();
        _cts.Dispose();
    }

    #endregion

    #region == Private Methods ==

    private void PopulateValues()
    {
        // Basics

        Name = _building.Name;

        //SelectedKind = _building.BuildingKind;
        var propertyTypeKey = PropertyTypes.FirstOrDefault(k => k.Key == _building.BuildingType.Key);
        SelectedPropertyType = propertyTypeKey is null ? new(Models.Rent.Residentials.PropertyType.Unspecified) : propertyTypeKey;
        IsUnitOwnership = _building.IsUnitOwnership;
        var structureKey = Structures.FirstOrDefault(k => k.Key == _building.BuildingStructure.Key);
        SelectedStructure = structureKey is null ? new(Models.Rent.Residentials.StructureType.Unspecified) : structureKey;

        FloorCountAboveGround = _building.FloorCountAboveGround == 0 ? string.Empty : _building.FloorCountAboveGround.ToString(CultureInfo.InvariantCulture);
        FloorCountBasement = _building.FloorCountBasement == 0 ? string.Empty : _building.FloorCountBasement.ToString(CultureInfo.InvariantCulture);
        TotalUnitCount = _building.TotalUnitCount == 0 ? string.Empty : _building.TotalUnitCount.ToString(CultureInfo.InvariantCulture);
        BuiltYearAndMonth = _building.BuiltYearAndMonth.Year != 1900 ? _building.BuiltYearAndMonth : null;
        FudousanId = _building.FudousanId;
        FudousanIdAdditionalCode = _building.FudousanIdAdditionalCode;
        Remarks = _building.Remarks;


        // Location

        MachiazaId = _building.Address.MachiazaId;
        if (!string.IsNullOrEmpty(MachiazaId))
        {
            // TODO: Get Pref, City, Town, Choume from MachiazaId?
        }

        /*
        if (!string.IsNullOrEmpty(_building.LocPrefId))
        {
            var hoge = Prefectures.FirstOrDefault<Prefecture>(p => p.MunicipalityCode.Equals(_building.LocPrefId, StringComparison.Ordinal));
            if (hoge is not null)
            {
                SelectedPef = hoge;
            }
        }

        if ((Cities is not null) && ((!string.IsNullOrEmpty(_building.LocCounty)) || (!string.IsNullOrEmpty(_building.LocCity))))
        {
            foreach (var cty in Cities)
            {
                if (cty.County.Equals(_building.LocCounty, StringComparison.Ordinal) && cty.City.Equals(_building.LocCity, StringComparison.Ordinal))
                {
                    SelectedCity = cty;
                    break;
                }
            }
        }

        if ((Towns is not null) && ((!string.IsNullOrEmpty(_building.LocWard)) || (!string.IsNullOrEmpty(_building.LocOazaCho))))
        {
            foreach (var twn in Towns)
            {
                if (twn.Ward.Equals(_building.LocWard, StringComparison.Ordinal) && twn.Oaza.Equals(_building.LocOazaCho, StringComparison.Ordinal))
                {
                    SelectedTown = twn;
                    break;
                }
            }
        }

        if (Chous is not null) // Allow an empty string. //&& (!string.IsNullOrEmpty(_building.LocChoume))
        {
            var hoge = Chous.FirstOrDefault<Choume>(p => p.Chou.Equals(_building.LocChoume, StringComparison.Ordinal));
            if (hoge is not null)
            {
                SelectedChou = hoge;
            }
        }

        Edaban = _building.LocEdaban;

        LocationLatitude = _building.LocationLatitude;
        LocationLongitude = _building.LocationLongitude;
        */
        //TODO: Set other properties


        // Pictures:
        Pictures = new ObservableCollection<Models.Rent.Residentials.PropertyPicture>(_building.Pictures); // create a copy.

        foreach (var item in Pictures)
        {
            // Filename to actuall path.
            item.BasePath = System.IO.Path.Combine(App.PropertyBlobDataFolder, _building.Id);
            item.ParentViewModel = this;
            item.IsModified = false; // Needed this.
            item.PropertyChanged += OnBuildingPicturePropertyChanged;
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
        Pdfs = new ObservableCollection<Models.Rent.Residentials.PropertyPdf>(_building.Pdfs); // create a copy.

        foreach (var item in Pdfs)
        {
            // Filename to actuall path.
            item.BasePath = System.IO.Path.Combine(App.PropertyBlobDataFolder, _building.Id);
            item.ParentViewModel = this;
            item.IsModified = false; // Needed this.
            item.PropertyChanged += OnBuildingPdfPropertyChanged;
        }

        Pdfs.CollectionChanged += (s, e) =>
        {
            // Unsubscribe from removed items
            if (e.OldItems != null)
            {
                foreach (Models.Rent.Residentials.PropertyPdf item in e.OldItems)
                {
                    Debug.WriteLine($"Item {item.Id} Removed from Pdfs, @CollectionChanged in PopulateEntityValues of Bldg.MainViewModel");
                    IsDirty = true;

                    item.PropertyChanged -= OnBuildingPdfPropertyChanged;
                }
            }

            // Subscribe to PropertyChanged.
            if (e.NewItems != null)
            {
                foreach (Models.Rent.Residentials.PropertyPdf item in e.NewItems)
                {
                    Debug.WriteLine($"Item {item.Id} Added to Pdfs. @CollectionChanged in PopulateEntityValues of Bldg.MainViewModel");
                    IsDirty = true;

                    item.PropertyChanged += OnBuildingPdfPropertyChanged;
                }
            }
        };

        // Lessors
        LessorsWrapper = new ObservableCollection<Models.Rent.Residentials.PersonWrapperForPropertyViewModel>();
        foreach (var item in _building.Lessors)
        {
            LessorsWrapper.Add(new Models.Rent.Residentials.PersonWrapperForPropertyViewModel(item,this));
        }

        // Brokers
        BrokersWrapper = new ObservableCollection<Models.Rent.Residentials.PersonWrapperForPropertyViewModel>();
        foreach (var item in _building.Brokers)
        {
            BrokersWrapper.Add(new Models.Rent.Residentials.PersonWrapperForPropertyViewModel(item, this));
        }

        // Rooms
        Rooms = new ObservableCollection<Models.Rent.Residentials.Listing>(_building.Rooms); // create a copy.

        foreach (var item in Rooms)
        {
            //
            item.IsModified = false; // Needed this.
            item.PropertyChanged += OnRoomPropertyChanged;
        }

        Rooms.CollectionChanged += (s, e) =>
        {
            // Unsubscribe from removed items
            if (e.OldItems != null)
            {
                foreach (Models.Rent.Residentials.Listing item in e.OldItems)
                {
                    //Debug.WriteLine($"Item {item.Id} Removed from Rooms. @Rooms.CollectionChanged in PopulateEntityValues of Bldg.MainViewModel");
                    //IsDirty = true; // Don't

                    item.PropertyChanged -= OnRoomPropertyChanged;
                }
            }

            // Subscribe to PropertyChanged.
            if (e.NewItems != null)
            {
                foreach (Models.Rent.Residentials.Listing item in e.NewItems)
                {
                    //Debug.WriteLine($"Item {item.Id} Added to Rooms. @Rooms.CollectionChanged in PopulateEntityValues of Bldg.MainViewModel");
                    //IsDirty = true; // don't

                    item.PropertyChanged += OnRoomPropertyChanged;
                }
            }
        };

        //Debug.WriteLine($"PopulateEntityValues: Completed populating values from Entity to VM. Entity ID: {_building.Id}, Rooms Count: {Rooms.Count}");
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

    private bool ValidateName()
    {
        if (string.IsNullOrWhiteSpace(Name))
        {
            InfoBarErrorMessage = "物件名（必須項目）が入力されていません。保存出来ませんでした。";

            IsNameHasError = true;

            //HasErrors = true;

            return false;
        }

        var realLength = new StringInfo(Name).LengthInTextElements;
        if (realLength > 100)
        {
            InfoBarErrorMessage = "物件名は100文字以内で入力してください。保存出来ませんでした。";

            IsNameHasError = true;

            //HasErrors = true;

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

    private void SetValues()
    {
        if (!IsDirty)
        {
            return;
        }

        try
        {
            // 物件名
            _building.SetName(Name);

            _building.SetBuildingType(SelectedPropertyType);
            _building.SetIsUnitOwnership(IsUnitOwnership);
            _building.SetBuildingStructure(SelectedStructure);

            if (!string.IsNullOrWhiteSpace(FloorCountAboveGround))
            {
                _building.SetFloorCountAboveGroundFromString(Helpers.Common.ReplaceZenkakuNumbers(FloorCountAboveGround));
                /*
                if (int.TryParse(Helpers.Common.ReplaceZenkakuNumbers(FloorCountAboveGround), out var floorCountAboveGround))
                {
                    _building.SetFloorCountAboveGround(floorCountAboveGround);
                }
                else
                {
                    _building.SetFloorCountAboveGround(0);
                }
                */
            }
            if (!string.IsNullOrWhiteSpace(FloorCountBasement))
            {
                _building.SetFloorCountBasementFromString(Helpers.Common.ReplaceZenkakuNumbers(FloorCountBasement));
            }
            if (!string.IsNullOrWhiteSpace(TotalUnitCount))
            {
                _building.SetTotalUnitCountFromString(Helpers.Common.ReplaceZenkakuNumbers(TotalUnitCount));
            }
            _building.SetBuiltYearAndMonth(BuiltYearAndMonth);
            _building.SetFudousanId(Helpers.Common.ReplaceZenkakuNumbers(FudousanId));
            _building.SetFudousanIdAdditionalCode(Helpers.Common.ReplaceZenkakuNumbers(FudousanIdAdditionalCode));
            _building.SetRemarks(Remarks);

            // 所在地
            _building.Address.SetMachiazaId(MachiazaId ?? string.Empty);
            _building.Address.SetPrefecture(SelectedPef);
            _building.Address.SetCountyAndCity(SelectedCity);
            _building.Address.SetWardAndOaza(SelectedTown);
            _building.Address.SetChoume(SelectedChou);
            _building.Address.SetEdaban(Edaban);

            // TODO: Set other properties

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
                Operation = "入力値チェック",
                MethodName = nameof(SetValues),
                OccuredAt = DateTime.Now
            };

            ShowErrorInfoBar(err,true);
        }



        // TODO: Set other properties
        // TODO: Don't forget to check if Helpers.Common.ReplaceZenkakuNumbers is needed.


        //_building.SetLocLocationFull(AddressPreview);
        //_building.SetLocationLatitude(LocationLatitude);
        //_building.SetLocationLongitude(LocationLongitude);
        
        // TODO: Set other properties
        // TODO: Don't forget to check if Helpers.Common.ReplaceZenkakuNumbers is needed.




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

        // 図面
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

        // 貸主
        _building.Lessors.Clear();
        if (IsUnitOwnership)
        {
            // もし「区分所有」で「建物」に貸主がくっついていたら削除へ
            // if (BuildingLessors.Count > 0)
            foreach (var psn in LessorsWrapper)
            {
                _building.LessorsToBeDeleted.Add(psn.Person);
            }
            LessorsWrapper.Clear();
        }
        else
        {
            // Wraperから「建物」の貸主を取り出して追加
            foreach (var item in this.LessorsWrapper)
            {
                _building.Lessors.Add(item.Person);
            }

            // もし区分所有で「ない」建物の「部屋」に貸主がくっついていたら削除へ
            foreach (var room in Rooms)
            {
                foreach(var psn in room.Lessors)
                {
                    room.LessorsToBeDeleted.Add(psn);
                }
                room.Lessors.Clear();
            }
        }

        // 宅建業者
        _building.Brokers.Clear();
        foreach (var item in this.BrokersWrapper)
        {
            _building.Brokers.Add(item.Person);
        }

        // 部屋
        foreach (var rm in Rooms)
        {
            rm.IsPropertyUnitOwnership = IsUnitOwnership;
        }

        // 部屋set
        _building.Rooms = Rooms;

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

    #region == Commands ==

    #region == Save related commands ==

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
            //InfoBarErrorMessage = "入力項目に誤りがあります。保存出来ませんでした。";
            IsInfoBarErrorOpen = true;

            if (!_navigationService.IsCurrentPageSameAs(BasicPageName))
            {
                _navigationService.NavigateTo(BasicPageName, this);
            }

            return;
        }

        // TODO: more.

        SetValues();

        bool saveResult;

        var resInsert = await Task.Run(() => _dataAccessService.UpsertRentResidential(_building), _cts.Token);
        if (resInsert.IsError)
        {
            Debug.WriteLine(
                resInsert.Error.Title + Environment.NewLine +
                resInsert.Error.Message + Environment.NewLine +
                resInsert.Error.Description + Environment.NewLine +
                resInsert.Error.Operation + Environment.NewLine +
                resInsert.Error.MethodName + Environment.NewLine +
                resInsert.Error.FullDump);

            ShowErrorInfoBar(resInsert.Error, true);

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

            // Clean up deleted picture file.
            if (_building.PicturesToBeDeleted.Count > 0)
            {
                foreach (var file in _building.PicturesToBeDeleted)
                {
                    var delFilePath = System.IO.Path.Combine(System.IO.Path.Combine(App.PropertyBlobDataFolder, _building.Id), file.ImageFilename);
                    File.Delete(delFilePath);
                }
                _building.PicturesToBeDeleted.Clear();
            }

            // Clean up deleted PDF and thumb file.
            if (_building.PdfsToBeDeleted.Count > 0)
            {
                foreach (var file in _building.PdfsToBeDeleted)
                {
                    var delFilePath = System.IO.Path.Combine(System.IO.Path.Combine(App.PropertyBlobDataFolder, _building.Id), file.PdfFilename);
                    File.Delete(delFilePath);
                    delFilePath = System.IO.Path.Combine(System.IO.Path.Combine(App.PropertyBlobDataFolder, _building.Id), file.ThumbnailFilename);
                    File.Delete(delFilePath);
                }
                _building.PdfsToBeDeleted.Clear();
            }

            // Just in case clear LessorsToBeDeleted, BrokersToBeDeleted
            _building.LessorsToBeDeleted.Clear();
            _building.BrokersToBeDeleted.Clear();

            // Clear rooms pic and pdfs and lessors
            if (_building.RoomsToBeDeleted.Count > 0)
            {
                foreach (var room in _building.RoomsToBeDeleted)
                {
                    foreach (var roomPic in room.Pictures)
                    {
                        var delFilePath = System.IO.Path.Combine(System.IO.Path.Combine(App.PropertyBlobDataFolder, _building.Id), roomPic.ImageFilename);
                        File.Delete(delFilePath);
                    }

                    foreach (var roomPdf in room.Pdfs)
                    {
                        var delFilePath = System.IO.Path.Combine(System.IO.Path.Combine(App.PropertyBlobDataFolder, _building.Id), roomPdf.PdfFilename);
                        File.Delete(delFilePath);
                        delFilePath = System.IO.Path.Combine(System.IO.Path.Combine(App.PropertyBlobDataFolder, _building.Id), roomPdf.ThumbnailFilename);
                        File.Delete(delFilePath);
                    }

                    // Just in case clear LessorsToBeDeleted
                    room.LessorsToBeDeleted.Clear();
                }
                _building.RoomsToBeDeleted.Clear();
            }

            _unsavedBuildingPictureFileList.Clear();
            _unsavedBuildingPdfThumbnailFileList.Clear();
            _unsavedBuildingPdfFileList.Clear();

            // Jjust in case.
            _building.Status = EntityStatus.Saved;
            _building.IsModified = false;

            foreach (var room in Rooms)
            {
                room.PropertyName = Name;
                room.PropertyStatus = EntityStatus.Saved;
                room.Status = EntityStatus.Saved;
            }

            // Just in case.
            foreach (var room in _building.Rooms)
            {
                room.PropertyName = Name;
                room.PropertyStatus = EntityStatus.Saved;
                room.Status = EntityStatus.Saved;
            }

            WeakReferenceMessenger.Default.Send(new Models.Messenger.PropertyUpdatedMessage(_building as Models.Base.PropertyBase));
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

            // TODO: check file ext for valid image type.
            // TODO: set max file size?

            // TODO: Create thumbnail image?


            using var sourceStream = File.Open(filePath, FileMode.Open);

            string newId = Guid.CreateVersion7().ToString("N");
            string extension = Path.GetExtension(System.IO.Path.GetFileName(filePath));
            string newFilename = newId + extension;
            var destFilePath = Path.Combine(_propertyDataDirectoryPath, newFilename);
            //Debug.WriteLine($"{file} to {destFilePath}  @SetNewBuildingPicturesAsync()");

            using var destinationStream = File.Create(destFilePath);
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

    [RelayCommand(CanExecute = nameof(CanDeleteBuildingPicture))]
    private void DeleteBuildingPicture(Models.Rent.Residentials.PropertyPicture picBldg)
    {
        if (picBldg is null)
        {
            return;
        }

        // TODO: show dialog to comfirm.

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

                var pdf = new Models.Rent.Residentials.PropertyPdf(newId, newFilename, newThumbnailFilename, EntityStatus.New)
                {
                    BasePath = _propertyDataDirectoryPath,//System.IO.Path.Combine(App.PropertyBlobDataFolder, _building.Id),
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
    private void DeleteBuildingPdf(Models.Rent.Residentials.PropertyPdf pdfBldg)
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

    #region == Rooms related commands ==

    // Add New Modal window command
    [RelayCommand]
    private void AddNewRoom() 
    {
        var newId = Guid.CreateVersion7().ToString("N");
        var editorShell = _shellFactory.Create(new Models.Rent.Residentials.Listing(newId, EntityStatus.New, _building.Id, _building.Status, _building.IsUnitOwnership, Name));

        // Apply the current IsUnitOwnership state because it may not be saved to the _room.
        editorShell.ViewModel.IsPropertyUnitOwnership = this.IsUnitOwnership;

        var mainVM = App.GetService<ViewModels.MainViewModel>();
        mainVM.RentResidentialListingEditorList.Add(editorShell.Window);

        this.UnsavedChildEditorList.Add(editorShell.Window);

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
    private void EditSelectedRoom(Models.Rent.Residentials.Listing room)
    {
        if (room is null) return;

        var rentId = room.PropertyId;
        var unitId = room.Id;

        if (string.IsNullOrEmpty(rentId))//if (selected == null)
        {
            Debug.WriteLine("EditSelectedUnitCommand executed but no rentId.");
            return;
        }

        //Debug.WriteLine($"EditRentResidentialCommand executed for {selected.Id}");

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
                // TODO: should we remove the window from the list? It may have been closed already.
                //mainVM.RoomEditorList.Remove(editWin);
                //ChildEditorList.Remove(editWin);
            }
        }

        var editorShell = _shellFactory.Create(room);

        // Apply the current IsUnitOwnership state because it may not be saved to the _room.
        editorShell.ViewModel.IsPropertyUnitOwnership = this.IsUnitOwnership;

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

        editorWindow.AppWindow.Show();
        editorWindow.Activate();

        if (editorWindow.Content is Views.Rent.Residentials.Listing.ShellPage shell)
        {
            shell.Window?.AppWindow.MoveInZOrderBelow(editorWindow.AppWindow.Id);
        }
        editorWindow.AppWindow.MoveInZOrderAtTop();

    }
    public static bool EditSelectedRoomCanExecute(Models.Rent.Residentials.Listing room)
    {
        if (room is null) return false;
        return true;
    }

    [RelayCommand(CanExecute = nameof(DupeSelectedRoomCanExecute))]
    private void DupeSelectedRoom(Models.Rent.Residentials.Listing room)
    {
        if (room is null) return;

        //
    }
    public static bool DupeSelectedRoomCanExecute(Models.Rent.Residentials.Listing room)
    {
        if (room is null) return false;
        return true;
    }

    [RelayCommand(CanExecute = nameof(DeleteSelectedRoomCanExecute))]
    private void DeleteSelectedRoom(Models.Rent.Residentials.Listing room)
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
                // 
                //mainVM.RoomEditorList.Remove(editWin);
                //ChildEditorList.Remove(editWin);
            }
        }


        // TODO: show dialog to comfirm.

        if (Rooms.Remove(room))
        {
            // No. Don't delete directly. Just mark it for deletion. It will be deleted when the property is saved.
            //if (_building.Rooms.Remove(room)) { }
            _building.RoomsToBeDeleted.Add(room);
            IsDirty = true;
        }
    }
    public static bool DeleteSelectedRoomCanExecute(Models.Rent.Residentials.Listing room)
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

    [RelayCommand]
    public async Task ShowRailLineSelect1()
    {
        if (_dialogService is null)
        {
            Debug.WriteLine("_dlgService is null");
            return;
        }

        var railLine = await _dialogService.ShowRailLineSelectDialog();

        if (railLine is not null)
        {
            SelectedRailLine1 = railLine;
        }
    }

    [RelayCommand(CanExecute = nameof(CanShowRailStationSelect1))]
    public async Task ShowRailStationSelect1()
    {
        if (_dialogService is null)
        {
            Debug.WriteLine("_dlgService is null");
            return;
        }

        if (SelectedRailLine1 is null)
        {
            return;
        }

        var railStation = await _dialogService.ShowRailStationSelectDialog(SelectedRailLine1.LineCode);

        if (railStation is not null)
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
                //ErrorMain = res.Error;
                //IsMainErrorInfoBarVisible = true;

                // TODO: Show error message to user
                return;
            }

            if (res.Person is null)
            {
                Debug.WriteLine($"{lessorId} is null. Cannot open editor.");
                return;
            }


            var asdf = new Models.Rent.Residentials.PersonWrapperForPropertyViewModel(res.Person, this);

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

            _building.LessorsToBeDeleted.Add(lessor.Person);
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

                // TODO: Show error message to user
                return;
            }

            if (res.Person is null)
            {
                Debug.WriteLine($"{brokerId} is null. Cannot open editor.");
                return;
            }

            var wrapper = new Models.Rent.Residentials.PersonWrapperForPropertyViewModel(res.Person, this);

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

            _building.BrokersToBeDeleted.Add(broker.Person);
        }
    }
    private static bool CanDeleteBroker(Models.Rent.Residentials.PersonWrapperForPropertyViewModel broker)
    {
        return broker is not null;
    }

    #endregion

    #endregion

    public void Dispose()
    {
        _cts?.Dispose();

        GC.SuppressFinalize(this);
    }
}
