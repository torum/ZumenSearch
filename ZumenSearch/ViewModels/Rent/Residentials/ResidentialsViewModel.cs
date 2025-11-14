using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.Data.Sqlite;
using Microsoft.UI.Windowing;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Media;
using Microsoft.UI.Xaml.Media.Animation;
using Microsoft.UI.Xaml.Navigation;
using Microsoft.VisualBasic;
using Microsoft.Windows.Storage.Pickers;
using Windows.System;
using ZumenSearch.Models;
using ZumenSearch.Models.Rent.Residentials;
using ZumenSearch.Services;
using ZumenSearch.Views;
using ZumenSearch.Views.Rent.Residentials.Bldg;
using static System.Net.Mime.MediaTypeNames;

namespace ZumenSearch.ViewModels.Rent.Residentials;

public partial class ResidentialsViewModel : ObservableObject
{
    #region == プロパティ ==

    public Views.Rent.Residentials.Bldg.EditorWindow? EditorWin {get; private set;}

    // Modal Editor window position and size
    public int ModalWinWidth = 1366;
    public int ModalWinHeight = 768;
    public int ModalWinLeft = 130;
    public int ModalWinTop = 130;

    // The Entry property holds the COPY of current RentResidential entry being edited. Do not use it directly in the UI. Apply changes to this object in SaveAsync() to save the changes.
    // MainViewModel creates a new instance of this class and call EditorShell.SetEntry(EntryResidentialFull) and sets this property.
    private Models.Rent.Residentials.EntryResidentialFull _entry = new();

    private readonly List<string> _unsavedBuildingPictureFileList = [];

    private readonly string _windowTitleBase = "賃貸住居用";

    public string WindowTitle
    {
        get
        {
            if (string.IsNullOrEmpty(_name))
            {
                return _windowTitleBase;
            }
            else
            {
                return $"{_windowTitleBase} - {_name}";
            }
            
        }
    }

    // This flag indicates if the entry is dirty (i.e., has unsaved changes).
    private bool _isEntryDirty;
    public bool IsEntryDirty
    {
        get => _isEntryDirty;
        set
        {
            if (SetProperty(ref _isEntryDirty, value))
            {
                //
                SaveCommand.NotifyCanExecuteChanged();
            }
        }
    }

    #endregion

    #region == 建物基本プロパティ == 

    // 物件名
    private string _name = string.Empty;
    public string Name
    {
        get => _name ?? string.Empty; // Ensure a non-null value is returned
        set
        {
            if (SetProperty(ref _name, value.Trim()))
            {
                IsEntryDirty = true;
                OnPropertyChanged(nameof(WindowTitle));
            }
        }
    }

    // 物件種別
    public ObservableCollection<Kind> Kinds =
    [
        //new Kind(EnumKinds.Unspecified.ToString(), "未指定"),
        new Kind(Models.Rent.Residentials.EnumKinds.Apartment.ToString(), "アパート"),
        new Kind(Models.Rent.Residentials.EnumKinds.Mansion.ToString(), "マンション"),
        new Kind(Models.Rent.Residentials.EnumKinds.House.ToString(), "一戸建て"),
        new Kind(Models.Rent.Residentials.EnumKinds.TerraceHouse.ToString(), "テラスハウス"),
        new Kind(Models.Rent.Residentials.EnumKinds.TownHouse.ToString(), "タウンハウス"),
        new Kind(Models.Rent.Residentials.EnumKinds.ShareHouse.ToString(), "シェアハウス"),
        new Kind(Models.Rent.Residentials.EnumKinds.Dormitory.ToString(), "寮・下宿")
    ];

    // The SelectedKind property holds the currently selected kind from the ComboBox in the UI.
    private Kind _selectedKind = new(Models.Rent.Residentials.EnumKinds.Unspecified.ToString(), "");
    public Kind SelectedKind
    {
        get => _selectedKind;
        set
        {
            if (SetProperty(ref _selectedKind, value))
            {
                IsEntryDirty = true;
            }
        }
    }

    // 区分所有か一括所有か
    private bool _isUnitOwnership;
    public bool IsUnitOwnership
    {
        get => _isUnitOwnership;
        set
        {
            if (SetProperty(ref _isUnitOwnership, value))
            {
                IsEntryDirty = true;
                OnPropertyChanged(nameof(IsNotUnitOwnership));

                // If this is set, then show/hide the owner and zumen from shell menu.
                EventIsUnitOwnership?.Invoke(this, _isUnitOwnership);
            }
        }
    }

    public bool IsNotUnitOwnership => !IsUnitOwnership;

    // 建物構造
    public ObservableCollection<Structure> Structures =
    [
        //new Structure(EnumStructure.Unspecified.ToString(), "未指定"),
        new Structure(Models.Rent.Residentials.EnumStructure.Wood, "木造"),
        new Structure(Models.Rent.Residentials.EnumStructure.Block, "ブロック造"),
        new Structure(Models.Rent.Residentials.EnumStructure.LightSteel, "軽量鉄骨造"),
        new Structure(Models.Rent.Residentials.EnumStructure.Steel, "鉄骨造"),
        new Structure(Models.Rent.Residentials.EnumStructure.RC, "鉄筋コンクリート(RC)造"),
        new Structure(Models.Rent.Residentials.EnumStructure.SRC, "鉄骨鉄筋コンクリート(SRC)造"),
        new Structure(Models.Rent.Residentials.EnumStructure.ALC, "軽量気泡コンクリート(ALC)造"),
        new Structure(Models.Rent.Residentials.EnumStructure.PC, "プレキャストコンクリート(PC)造"),
        new Structure(Models.Rent.Residentials.EnumStructure.HPC, "鉄骨プレキャストコンクリート(HPC)造"),
        new Structure(Models.Rent.Residentials.EnumStructure.RB, "鉄筋ブロック造"),
        new Structure(Models.Rent.Residentials.EnumStructure.CFT, "コンクリート充填鋼管(CFT)造"),
        new Structure(Models.Rent.Residentials.EnumStructure.Other, "その他")
    ];

    private Structure _selectedStructure = new(Models.Rent.Residentials.EnumStructure.Unspecified, "");
    public Structure SelectedStructure
    {
        get => _selectedStructure;
        set
        {
            if (SetProperty(ref _selectedStructure, value))
            {
                IsEntryDirty = true;
            }
        }
    }

    // 地上階
    private string _aboveGroundFloorCount = string.Empty;
    public string AboveGroundFloorCount
    {
        get => _aboveGroundFloorCount;
        set
        {
            if (_basementFloorCount == value)
            {
                return;
            }

            if (value is null)
            {
                return;
            }

            var text = ReplaceZenkakuNumber(value.Trim());

            if (CanConvertToPositiveNumber(text))
            {
                _aboveGroundFloorCount = text;
                IsEntryDirty = true;
            }
            else
            {
                // TODO: show error
                _aboveGroundFloorCount = string.Empty;
                //IsEntryDirty = true;
            }

            OnPropertyChanged(nameof(AboveGroundFloorCount));
        }
    }

    // 地下階
    private string _basementFloorCount = string.Empty;
    public string BasementFloorCount
    {
        get => _basementFloorCount;
        set
        {
            if (_basementFloorCount == value)
            {
                return;
            }

            if (value is null)
            {
                return;
            }

            var text = ReplaceZenkakuNumber(value.Trim());

            if (CanConvertToPositiveNumber(text))
            {
                _basementFloorCount = text;
                IsEntryDirty = true;
            }
            else
            {
                // TODO: show error
                _basementFloorCount = string.Empty;
                //IsEntryDirty = true;
            }

            OnPropertyChanged(nameof(BasementFloorCount));
        }
    }

    // 総戸数
    private string _totalUnitCount = string.Empty;
    public string TotalUnitCount
    {
        get => _totalUnitCount;
        set
        {
            if (_totalUnitCount == value)
            {
                return;
            }

            if (value is null)
            {
                return;
            }

            var text = ReplaceZenkakuNumber(value.Trim());

            if (CanConvertToPositiveNumber(text))
            {
                _totalUnitCount = text;
                IsEntryDirty = true;
            }
            else
            {
                // TODO: show error
                _totalUnitCount = string.Empty;
                //IsEntryDirty = true;
            }

            OnPropertyChanged(nameof(TotalUnitCount));
        }
    }

    // 築年月
    private DateTimeOffset? _builtYearAndMonth;
    public DateTimeOffset? BuiltYearAndMonth
    {
        get => _builtYearAndMonth;
        set
        {
            if (SetProperty(ref _builtYearAndMonth, value))
            {
                IsEntryDirty = true;
                OnPropertyChanged(nameof(BuiltYearAndMonth));
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

            if (_builtYearAndMonth is not null)
            {
                var cultureJp = new CultureInfo("ja-jp", false);
                cultureJp.DateTimeFormat.Calendar = new JapaneseCalendar();

                return _builtYearAndMonth.Value.ToString("ggy年M月", cultureJp);
                //return _builtYearAndMonth.ToString("O"); // For serialization
            }

            return s;
        }
    }

    // 不動産ID (13桁)
    private string _fudousanId = string.Empty;
    public string FudousanId
    {
        get => _fudousanId ?? string.Empty; 
        set
        {
            if (SetProperty(ref _fudousanId, value.Trim()))
            {
                IsEntryDirty = true;
            }
        }
    }

    // 特定コード（４桁）建物全体は0000
    private string _fudousanIdAdditionalCode = "0000";
    public string FudousanIdAdditionalCode
    {
        get => _fudousanIdAdditionalCode ?? string.Empty; 
        set
        {
            if (SetProperty(ref _fudousanIdAdditionalCode, value.Trim()))
            {
                IsEntryDirty = true;
            }
        }
    }

    #endregion

    #region == 所在地プロパティ ==

    private string? MachiazaId;

    public ObservableCollection<Pref> Prefs =
    [
        new Pref("01","010006", "北海道"),
        new Pref("02","020001", "青森県"),
            new Pref("03","030007", "岩手県"),
            new Pref("04","040002", "宮城県"),
            new Pref("05","050008", "秋田県"),
            new Pref("06","060003", "山形県"),
            new Pref("07","070009", "福島県"),
            new Pref("08","080004", "茨城県"),
            new Pref("09","090000", "栃木県"),
            new Pref("10","100005", "群馬県"),
            new Pref("11","110001", "埼玉県"),
            new Pref("12","120006", "千葉県"),
            new Pref("13","130001", "東京都"),
            new Pref("14","140007", "神奈川県"),
            new Pref("15","150002", "新潟県"),
            new Pref("16","160008", "富山県"),
            new Pref("17","170003", "石川県"),
            new Pref("18","180009", "福井県"),
            new Pref("19","190004", "山梨県"),
            new Pref("20","200000", "長野県"),
            new Pref("21","210005", "岐阜県"),
            new Pref("22","220001", "静岡県"),
            new Pref("23","230006", "愛知県"),
            new Pref("24","240001", "三重県"),
            new Pref("25","250007", "滋賀県"),
            new Pref("26","260002", "京都府"),
            new Pref("27","270008", "大阪府"),
            new Pref("28","280003", "兵庫県"),
            new Pref("29","290009", "奈良県"),
            new Pref("30","300004", "和歌山県"),
            new Pref("31","310000", "鳥取県"),
            new Pref("32","320005", "島根県"),
            new Pref("33","330001", "岡山県"),
            new Pref("34","340006", "広島県"),
            new Pref("35","350001", "山口県"),
            new Pref("36","360007", "徳島県"),
            new Pref("37","370002", "香川県"),
            new Pref("38","380008", "愛媛県"),
            new Pref("39","390003", "高知県"),
            new Pref("40","400009", "福岡県"),
            new Pref("41","410004", "佐賀県"),
            new Pref("42","420000", "長崎県"),
            new Pref("43","430005", "熊本県"),
            new Pref("44","440001", "大分県"),
            new Pref("45","450006", "宮崎県"),
            new Pref("46","460001", "鹿児島県"),
            new Pref("47","470007", "沖縄県"),
    ];

    private Pref? _selectedPef;
    public Pref? SelectedPef
    {
        get => _selectedPef;
        set
        {
            if (SetProperty(ref _selectedPef, value))
            {
                IsEntryDirty = true;
                OnPropertyChanged(nameof(AddressPreview));

                if (_selectedPef is null)
                {
                    Cities = null;
                    return;
                }

                var dataset = new List<CountyAndCity>();

                dataset = _dataAccessLocationService.GetCountyAndCityByPref(_selectedPef.Name);

                Cities = [.. dataset.DistinctBy(p => p.Combined)];

            }
        }
    }

    private ObservableCollection<CountyAndCity>? _cities = [];
    public ObservableCollection<CountyAndCity>? Cities
    {
        get => _cities;
        set
        {
            if (SetProperty(ref _cities, value))
            {
                //
            }
        }
    }

    private CountyAndCity? _selectedCity;
    public CountyAndCity? SelectedCity
    {
        get => _selectedCity;
        set
        {
            if (SetProperty(ref _selectedCity, value))
            {
                IsEntryDirty = true;
                MachiazaId = _selectedCity?.MachiazaId;

                OnPropertyChanged(nameof(AddressPreview));

                if (_selectedPef is null)
                {
                    Towns = null;
                    return;
                }

                if (_selectedCity is null)
                {
                    Towns = null;
                    return;
                }

                var dataset = new List<WardAndOaza>();

                dataset = _dataAccessLocationService.GetWardAndOazaByPrefCountyCity(_selectedPef.Name, _selectedCity.County, _selectedCity.City);

                Towns = [.. dataset.DistinctBy(p => p.Combined)];
            }
        }
    }

    private ObservableCollection<WardAndOaza>? _towns = [];
    public ObservableCollection<WardAndOaza>? Towns
    {
        get => _towns;
        set
        {
            if (SetProperty(ref _towns, value))
            {
                //
            }
        }
    }

    private WardAndOaza? _selectedTown;
    public WardAndOaza? SelectedTown
    {
        get => _selectedTown;
        set
        {
            if (SetProperty(ref _selectedTown, value))
            {
                IsEntryDirty = true;
                MachiazaId = _selectedTown?.MachiazaId;
                OnPropertyChanged(nameof(AddressPreview));
            }

            if (_selectedPef is null)
            {
                Chous = null;
                return;
            }
            if (_selectedCity is null)
            {
                Chous = null;
                return;
            }

            if (_selectedTown is null)
            {
                Chous = null;
                return;
            }

            // TODO: move this

            var dataset = new List<Choume>();

            dataset = _dataAccessLocationService.GetChoumeByPrefCountyCityWardOaza(_selectedPef.Name, _selectedCity.County, _selectedCity.City, _selectedTown.Ward, _selectedTown.Oaza);

            Chous = [.. dataset.DistinctBy(p => p.Chou)];
        }
    }

    private ObservableCollection<Choume>? _chous = [];
    public ObservableCollection<Choume>? Chous
    {
        get => _chous;
        set
        {
            if (SetProperty(ref _chous, value))
            {
                //
            }
        }
    }

    private Choume? _selectedChou;
    public Choume? SelectedChou
    {
        get => _selectedChou;
        set
        {
            if (SetProperty(ref _selectedChou, value))
            {
                IsEntryDirty = true;
                MachiazaId = _selectedChou?.MachiazaId;
                OnPropertyChanged(nameof(AddressPreview));
            }
        }
    }

    private string _edaban = string.Empty;
    public string Edaban
    {
        get => _edaban;
        set
        {
            if (SetProperty(ref _edaban, value))
            {
                IsEntryDirty = true;
                OnPropertyChanged(nameof(AddressPreview));
            }
        }
    }

    // TODO:
    private string _postalCode = string.Empty;
    public string PostalCode
    {
        get => _postalCode;
        set
        {
            if (SetProperty(ref _postalCode, value))
            {
                IsEntryDirty = true;
                OnPropertyChanged(nameof(AddressPreview));
            }
        }
    }

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
            if (!string.IsNullOrEmpty(_edaban))
            {
                s = "-" + _edaban;
            }
            // TODO:
            return $"{SelectedPef.Name}{_selectedCity?.Combined}{_selectedTown?.Combined}{_selectedChou?.Chou}{s}";
        }
    }

    // 緯度（Lat）
    private string _locationLatitude = string.Empty;
    public string LocationLatitude
    {
        get => _locationLatitude;
        set
        {
            if (_locationLatitude == value)
            {
                return;
            }

            _locationLatitude = value;
            IsEntryDirty = true;
            OnPropertyChanged(nameof(LocationLatitude));
            OnPropertyChanged(nameof(GeoUri));
            ShowGoogleMapsCommand.NotifyCanExecuteChanged();
        }
    }

    // 経度（Lon）
    private string _locationLongitude = string.Empty;
    public string LocationLongitude
    {
        get => _locationLongitude;
        set
        {
            if (_locationLongitude == value)
            {
                return;
            }

            _locationLongitude = value;
            IsEntryDirty = true;
            OnPropertyChanged(nameof(LocationLongitude));
            OnPropertyChanged(nameof(GeoUri));
            ShowGoogleMapsCommand.NotifyCanExecuteChanged();
        }
    }

    public string GeoUri
    {
        get
        {
            if (string.IsNullOrEmpty(_locationLatitude) || string.IsNullOrEmpty(_locationLongitude))
            {
                return "https://maps.google.co.jp/";
            }

            return $"https://maps.google.co.jp/?q={LocationLatitude},{LocationLongitude}";
        }
    }

    #endregion

    #region == 交通プロパティ ==

    private RailLine? _selectedRailLine1;
    public RailLine? SelectedRailLine1
    {
        get => _selectedRailLine1;
        set
        {
            if (SetProperty(ref _selectedRailLine1, value))
            {
                IsEntryDirty = true;
                // Clear  old value.
                SelectedRailStation1 = null;
            }

            ShowRailStationSelect1Command.NotifyCanExecuteChanged();
        }
    }

    private RailStation? _selectecdRailStation1;
    public RailStation? SelectedRailStation1
    {
        get => _selectecdRailStation1;
        set
        {
            if ((value is null) && (_selectecdRailStation1 is not null))
            {
                // Clear old value.
                _selectecdRailStation1.StationName = string.Empty;
                OnPropertyChanged(nameof(SelectedRailStation1));
            }

            if (SetProperty(ref _selectecdRailStation1, value))
            {
                IsEntryDirty = true;
            }
        }
    }

    private string _ekiToho1 = string.Empty;
    public string EkiToho1
    {
        get => _ekiToho1;
        set
        {
            if (_ekiToho1 == value)
            {
                return;
            }

            if (value is null)
            {
                return;
            }

            var text = ReplaceZenkakuNumber(value.Trim());

            if (CanConvertToPositiveNumber(text))
            {
                _ekiToho1 = text;
                IsEntryDirty = true;
            }
            else
            {
                // TODO: show error
                _ekiToho1 = string.Empty;
                //IsEntryDirty = true;
            }

            OnPropertyChanged(nameof(EkiToho1));
        }
    }

    private string _busStop = string.Empty;
    public string BusStop
    {
        get => _busStop;
        set
        {
            if (SetProperty(ref _busStop, value))
            {
                IsEntryDirty = true;
            }
        }
    }

    private string _busJyousya1 = string.Empty;
    public string BusJyousya1
    {
        get => _busJyousya1;
        set
        {
            if (_busJyousya1 == value)
            {
                return;
            }

            if (value is null)
            {
                return;
            }

            var text = ReplaceZenkakuNumber(value.Trim());

            if (CanConvertToPositiveNumber(text))
            {
                _busJyousya1 = text;
                IsEntryDirty = true;
            }
            else
            {
                // TODO: show error
                _busJyousya1 = string.Empty;
                //IsEntryDirty = true;
            }

            OnPropertyChanged(nameof(BusJyousya1));
        }
    }

    private string _busStopToho1 = string.Empty;
    public string BusStopToho1
    {
        get => _busStopToho1;
        set
        {
            if (_busStopToho1 == value)
            {
                return;
            }

            if (value is null)
            {
                return;
            }

            var text = ReplaceZenkakuNumber(value.Trim());

            if (CanConvertToPositiveNumber(text))
            {
                _busStopToho1 = text;
                IsEntryDirty = true;
            }
            else
            {
                // TODO: show error
                _busStopToho1 = string.Empty;
                //IsEntryDirty = true;
            }

            OnPropertyChanged(nameof(BusStopToho1));
        }
    }

    #endregion

    #region == 設備プロパティ ==


    private bool _ap_IsAutolock;
    public bool Ap_IsAutolock
    {
        get => _ap_IsAutolock;
        set
        {
            if (SetProperty(ref _ap_IsAutolock, value))
            {
                OnPropertyChanged(nameof(AppliancePreview));
            }
        }
    }

    private bool _ap_IsElevator;
    public bool Ap_IsElevator
    {
        get => _ap_IsElevator;
        set
        {
            if (SetProperty(ref _ap_IsElevator, value))
            {
                OnPropertyChanged(nameof(AppliancePreview));
            }
        }
    }

    private bool _ap_IsSecurityCamera;
    public bool Ap_IsSecurityCamera
    {
        get => _ap_IsSecurityCamera;
        set
        {
            if (SetProperty(ref _ap_IsSecurityCamera, value))
            {
                OnPropertyChanged(nameof(AppliancePreview));
            }
        }
    }

    private bool _ap_IsParcelLocker;
    public bool Ap_IsParcelLocker
    {
        get => _ap_IsParcelLocker;
        set
        {
            if (SetProperty(ref _ap_IsParcelLocker, value))
            {
                OnPropertyChanged(nameof(AppliancePreview));
            }
        }
    }


    public string AppliancePreview
    {
        get
        {
            var s = string.Empty;

            if (Ap_IsAutolock)
            {
                s += "オートロック";
            }
            if (Ap_IsElevator)
            {
                if (!string.IsNullOrEmpty(s))
                {
                    s += ",";
                }
                s += "エレベーター";
            }
            if (Ap_IsSecurityCamera)
            {
                if (!string.IsNullOrEmpty(s))
                {
                    s += ",";
                }
                s += "防犯カメラ";
            }
            if (Ap_IsParcelLocker)
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

    #region == 備考プロパティ ==

    private string _tatemonoMemo = string.Empty;
    public string TatemonoMemo
    {
        get => _tatemonoMemo;
        set
        {
            if (SetProperty(ref _tatemonoMemo, value))
            {
                IsEntryDirty = true;
                OnPropertyChanged(nameof(MemoPreview));
            }
        }
    }

    public string MemoPreview
    {
        get
        {
            string s;
            if (_tatemonoMemo.Length > 14)
            {
                s = _tatemonoMemo[..14] + "..." ;
            }
            else
            {
                s = _tatemonoMemo;
            }

            return s;
        }
    }

    #endregion

    #region == 写真プロパティ ==

    private ObservableCollection<PictureBuilding> _buildingPictures = [];
    public ObservableCollection<PictureBuilding> BuildingPictures
    {
        get => _buildingPictures;
        set
        {
            if (SetProperty(ref _buildingPictures, value))
            {
                IsEntryDirty = true;
            }
        }
    }

    private bool _isBuildingPictureEditPaneVisible = false;
    public bool IsBuildingPictureEditPaneVisible
    {
        get => _isBuildingPictureEditPaneVisible;
        set
        {
            if (SetProperty(ref _isBuildingPictureEditPaneVisible, value))
            {
               //
            }
        }
    }

    private bool _buildingPicturePropertiesIsDirty;
    public bool BuildingPicturePropertiesIsDirty
    {
        get => _buildingPicturePropertiesIsDirty;
        set
        {
            if (SetProperty(ref _buildingPicturePropertiesIsDirty, value))
            {
                UpdatedBuildingPicturePropertyCommand.NotifyCanExecuteChanged();
            }
        }
    }

    private PictureBuilding? _selectedBuildingPicture;
    public PictureBuilding? SelectedBuildingPicture
    {
        get => _selectedBuildingPicture;
        set
        {
            if (_selectedBuildingPicture == value)
            {
                return;
            }

            _selectedBuildingPicture = value;

            // "quietly" clear values.
            _buildingPictureIsMain = false;
            _selectedBuildingPictureType = new(EnumBuildingPictureType.Unspecified);
            _buildingPictureDescription = string.Empty;

            if (_selectedBuildingPicture is not null)
            {
                // "quietly" update values.
                _buildingPictureIsMain = _selectedBuildingPicture.IsMain;
                _buildingPictureDescription = _selectedBuildingPicture.Description ?? string.Empty;
                var lbl = BuildingPictureTypes.FirstOrDefault(x => x.Key == _selectedBuildingPicture.PictureType.Key);
                if (lbl is not null)
                {
                    _selectedBuildingPictureType = lbl;
                }
                else
                {
                    Debug.WriteLine($"@SelectedBuildingPicture: could not find label for key {_selectedBuildingPicture.PictureType.Key}");
                }

                IsBuildingPictureEditPaneVisible = true;
            }
            else
            {
                Debug.WriteLine($"@SelectedBuildingPicture: value is null");
            }

            OnPropertyChanged(nameof(SelectedBuildingPicture));
            _buildingPicturePropertiesIsDirty = false;

            //_buildingPicturePropertiesIsDirty = false;
            /*
            if (SetProperty(ref _selectedBuildingPicture, value))
            {
                if (_selectedBuildingPicture is not null)
                {
                    IsBuildingPictureEditPaneVisible = true;

                    Debug.WriteLine($"SelectedBuildingPicture changed: Label={_selectedBuildingPicture.Label.Text}, Description={_selectedBuildingPicture.Description}, IsMain={_selectedBuildingPicture.IsMain}");

                    // "quietly" update values.
                    _buildingPictureIsMain = _selectedBuildingPicture.IsMain;
                    _buildingPictureLabel = _selectedBuildingPicture.Label;
                    _buildingPictureDescription = _selectedBuildingPicture.Description ?? string.Empty;

                    //_buildingPicturePropertiesIsDirty = false;
                }
                else
                {
                    Debug.WriteLine($"SelectedBuildingPicture changed: null");

                    //IsBuildingPictureEditPaneVisible = false;
                }
            }
            */

            // notify updates.
            OnPropertyChanged(nameof(SelectedBuildingPictureType));
            OnPropertyChanged(nameof(BuildingPictureDescription));
            OnPropertyChanged(nameof(BuildingPictureIsMain));
            OnPropertyChanged(nameof(BuildingPicturePropertiesIsDirty));

            UpdatedBuildingPicturePropertyCommand.NotifyCanExecuteChanged();
            DeleteSelectedBuildingPictureCommand.NotifyCanExecuteChanged();
        }
    }

    public ObservableCollection<BuildingPictureType> BuildingPictureTypes =
    [
        //new BuildingPictureType(EnumBuildingPictureType.Unspecified, "未指定"),
        new BuildingPictureType(Models.Rent.Residentials.EnumBuildingPictureType.Madori),
        new BuildingPictureType(Models.Rent.Residentials.EnumBuildingPictureType.Gaikan),
        //new BuildingPictureType(Models.Rent.Residentials.EnumBuildingPictureType.Situnai),
        //new BuildingPictureType(Models.Rent.Residentials.EnumBuildingPictureType.LivingDining),
        //new BuildingPictureType(Models.Rent.Residentials.EnumBuildingPictureType.Bedroom),
        //new BuildingPictureType(Models.Rent.Residentials.EnumBuildingPictureType.Kitchen),
        //new BuildingPictureType(Models.Rent.Residentials.EnumBuildingPictureType.Bathroom),
        //new BuildingPictureType(Models.Rent.Residentials.EnumBuildingPictureType.Restroom),
        //new BuildingPictureType(Models.Rent.Residentials.EnumBuildingPictureType.Washroom),
        //new BuildingPictureType(Models.Rent.Residentials.EnumBuildingPictureType.StorageSpace),
        //new BuildingPictureType(Models.Rent.Residentials.EnumBuildingPictureType.Appliance),
        //new BuildingPictureType(Models.Rent.Residentials.EnumBuildingPictureType.FrontDoor),
        //new BuildingPictureType(Models.Rent.Residentials.EnumBuildingPictureType.Balcony),
        new BuildingPictureType(Models.Rent.Residentials.EnumBuildingPictureType.Entrance),
        new BuildingPictureType(Models.Rent.Residentials.EnumBuildingPictureType.Neighborhood),
        new BuildingPictureType(Models.Rent.Residentials.EnumBuildingPictureType.Other)
    ];

    private BuildingPictureType _selectedBuildingPictureType = new(EnumBuildingPictureType.Unspecified);
    public BuildingPictureType SelectedBuildingPictureType
    {
        get => _selectedBuildingPictureType;
        set 
        {
            /*
            if (_selectedBuildingPictureType == value)
            {
                Debug.WriteLine($"BuildingPictureLabel: same {_selectedBuildingPictureType.Text}");
                return;
            }

            if (value is null)
            {
                Debug.WriteLine($"BuildingPictureLabel: null");
                return;
            }

            _selectedBuildingPictureType = value;
            BuildingPicturePropertiesIsDirty = true;

            Debug.WriteLine($"Setting BuildingPictureLabel to: {(value is not null ? value.Text : "null")}");
            BuildingPicturePropertiesIsDirty = true;

            OnPropertyChanged(nameof(SelectedBuildingPictureType));
            */
            
            if (SetProperty(ref _selectedBuildingPictureType, value))
            {
                if (_selectedBuildingPictureType is not null)
                {
                    Debug.WriteLine($"BuildingPictureLabel changed: {_selectedBuildingPictureType.Text}");

                    BuildingPicturePropertiesIsDirty = true;
                }
                else
                {
                    Debug.WriteLine($"BuildingPictureLabel changed: null");
                }
            }

        }
    }

    private string _buildingPictureDescription = string.Empty;
    public string BuildingPictureDescription
    {
        get => _buildingPictureDescription;
        set
        {
            if (SetProperty(ref _buildingPictureDescription, value))
            {
                BuildingPicturePropertiesIsDirty = true;
            }
        }
    }

    private bool _buildingPictureIsMain;
    public bool BuildingPictureIsMain
    {
        get => _buildingPictureIsMain;
        set
        {
            if (SetProperty(ref _buildingPictureIsMain, value))
            {
                BuildingPicturePropertiesIsDirty = true;
            }
        }
    }

    #endregion

    #region == Events

    // The event handlers below are used to notify the UI about various actions that can be performed in the editor.
    // EditorShell subscribes to these events to handle the actions accordingly.
    public event EventHandler? EventBackToSummary;
    public event EventHandler? EventEditLocation;
    public event EventHandler? EventEditTransportation;
    public event EventHandler? EventEditAppliance;
    public event EventHandler? EventEditPictures;
    public event EventHandler? EventEditUnits;

    public event EventHandler<bool>? EventIsUnitOwnership; // show or hides navigationview' menu accordingly.

    // Who subscribes to this event?
    public event EventHandler? EventGoBack;

    #endregion

    #region == Services ==
    
    // The IDataAccessService is used to access the data layer for saving and updating entries.
    private readonly IDataAccessService _dataAccessService;
    private readonly IDataAccessLocationService _dataAccessLocationService;
    private readonly IModalDialogService _dlg;

    #endregion

    #region == Constructor ==

    // Constructor for the EditorViewModel class, initializes the data access service and the entry.
    #pragma warning disable IDE0290
    public ResidentialsViewModel(IDataAccessService dataAccessService, IModalDialogService modalDialog, IDataAccessLocationService dataAccessLocationService)
    {
        _dataAccessService = dataAccessService;
        _dataAccessLocationService = dataAccessLocationService;
        _dlg = modalDialog;

    }
    #pragma warning restore IDE0290

    #endregion

    #region == Public Methods ==

    public void SetEditorWin(Views.Rent.Residentials.Bldg.EditorWindow win)
    {
        EditorWin = win;
    }

    public void SetEntry(Models.Rent.Residentials.EntryResidentialFull entry)
    {
        if (entry is null)
        {
            return;
        }

        _entry = entry;

        PopulateEntryValues();

        IsEntryDirty = false;
    }

    public void SetNewBuildingPictures(List<string> filePathList)
    {
        if (filePathList.Count <= 0)
        {
            return;
        }

        foreach (var filePath in filePathList) 
        {
            if (string.IsNullOrEmpty(filePath.Trim()))
            {
                continue;
            }

            var pic = new Models.Rent.Residentials.PictureBuilding(filePath)
            {
                IsNew = true,
                Id = Guid.CreateVersion7().ToString()
            };

            BuildingPictures.Add(pic);

            IsEntryDirty = true;
        }
    }

    public void DiscardUnsavedFiles()
    {
        if (_unsavedBuildingPictureFileList.Count <= 0)
        {
            return;
        }
        
        foreach (var file in _unsavedBuildingPictureFileList)
        {
            if (File.Exists(file))
            {
                Debug.WriteLine($"Deleting unsaved file: {file}");
                File.Delete(file);
            }
        }
    }

    #endregion

    #region == Private Methods ==

    private void PopulateEntryValues()
    {
        // Basics

        Name = _entry.Name;
                
        // Location

        if (!string.IsNullOrEmpty(_entry.LocPrefId))
        {
            var hoge = Prefs.FirstOrDefault<Pref>(p => p.MunicipalityCode.Equals(_entry.LocPrefId));
            if (hoge is not null)
            {
                SelectedPef = hoge;
            }
        }

        if ((Cities is not null) && ((!string.IsNullOrEmpty(_entry.LocCounty)) || (!string.IsNullOrEmpty(_entry.LocCity))))
        {
            foreach (var cty in Cities)
            {
                if (cty.County.Equals(_entry.LocCounty) && cty.City.Equals(_entry.LocCity))
                {
                    SelectedCity = cty;
                    break;
                }
            }
        }

        if ((Towns is not null) && ((!string.IsNullOrEmpty(_entry.LocWard)) || (!string.IsNullOrEmpty(_entry.LocOazaCho))))
        {
            foreach (var twn in Towns)
            {
                if (twn.Ward.Equals(_entry.LocWard) && twn.Oaza.Equals(_entry.LocOazaCho))
                {
                    SelectedTown = twn;
                    break;
                }
            }
        }

        if ((Chous is not null) && (!string.IsNullOrEmpty(_entry.LocChoume)))
        {
            var hoge = Chous.FirstOrDefault<Choume>(p => p.Chou.Equals(_entry.LocChoume));
            if (hoge is not null)
            {
                SelectedChou = hoge;
            }
        }

        Edaban = _entry.LocEdaban;

        //TODO: Set other properties

        // Pictures

        BuildingPictures = new ObservableCollection<PictureBuilding>(_entry.BuildingPictures); // create a copy.

    }

    private void SetValuesToEntry()
    {
        // 物件名
        _entry.Name = Name;

        // 所在地
        _entry.LocPrefId = (_selectedPef is not null) ? _selectedPef.MunicipalityCode : string.Empty;
        _entry.LocPrefecture = (_selectedPef is not null) ? _selectedPef.Name : string.Empty;
        _entry.LocMachiazaId = (!string.IsNullOrEmpty(MachiazaId)) ? MachiazaId : string.Empty;
        _entry.LocCounty = (_selectedCity is not null) ? _selectedCity.County : string.Empty;
        _entry.LocCity = (_selectedCity is not null) ? _selectedCity.City : string.Empty;
        _entry.LocWard = (_selectedTown is not null) ? _selectedTown.Ward : string.Empty;
        _entry.LocOazaCho = (_selectedTown is not null) ? _selectedTown.Oaza : string.Empty;
        _entry.LocChoume = (_selectedChou is not null) ? _selectedChou.Chou : string.Empty;
        _entry.LocEdaban = (!string.IsNullOrEmpty(_edaban)) ? _edaban : string.Empty;
        _entry.LocLocationFull = AddressPreview;


        //TODO: Set other properties for Entry.
        //////////////////

        // 写真
        _entry.BuildingPictures = BuildingPictures;
    }

    private static string ReplaceZenkakuNumber(string text)
    {
        return text
        .Replace('０', '0')
        .Replace('１', '1')
        .Replace('２', '2')
        .Replace('３', '3')
        .Replace('４', '4')
        .Replace('５', '5')
        .Replace('６', '6')
        .Replace('７', '7')
        .Replace('８', '8')
        .Replace('９', '9')
        .Replace("，", "")
        .Replace(",", "");
    }

    private static bool CanConvertToPositiveNumber(string text)
    {
        if (int.TryParse(text, out var result))
        {
            if (result > -1)
            {
                return true;
            }
            else
            {
                Debug.WriteLine("整数変換に失敗。（マイナス）");
                return false;
            }
        }
        else
        {
            Debug.WriteLine("整数変換に失敗。");
            return false;
        }
    }

    #endregion

    #region == Commands ==

    #region == Save ==

    [RelayCommand(CanExecute = nameof(CanSave))]
    public void Save()
    {
        if (!IsEntryDirty)
        {
            return;
        }

        if (string.IsNullOrEmpty(Name))
        {
            Debug.WriteLine("TODO: Name is null or empty. This field is required. Show alart and abort.");
            return;
        }

        if (EditorWin is null)
        {
            Debug.WriteLine("TODO: Editor Window is null. ");
        }

        // Apply values of vm to the entry object.
        SetValuesToEntry();

        if (string.IsNullOrEmpty(_entry.Id))
        {
            // If the Entry.Id is empty, generate a new ID and save as new.
            _entry.SetId(Guid.CreateVersion7().ToString());
            var res = SaveAsNew();
            if (res)
            {
                // Clear these dirty flags.
                IsEntryDirty = false;
                _entry.IsDirty = false;

                // Clear unsaved file list.
                _unsavedBuildingPictureFileList.Clear();

                if (EditorWin is not null)
                {
                    EditorWin.Id = _entry.Id;
                }
                else
                {
                    // TODO: error.
                }
            }
            else
            {
                // Filed to save as new, so reset.
                _entry.ClearId();
            }
        }
        else
        {
            // If the Entry.Id is not empty, update the existing entry.
            var res = SaveAsUpdate();
            if (res)
            {
                // Clear these dirty flags.
                IsEntryDirty = false;
                _entry.IsDirty = false;

                // Clear unsaved file list.
                _unsavedBuildingPictureFileList.Clear();
            }
        }
    }
    private bool CanSave()
    {
        return IsEntryDirty;
    }

    private bool SaveAsNew()
    {
        // In case..
        if (string.IsNullOrEmpty(_entry.Id))
        {
            // If the Entry.Id is empty, generate a new ID and save as new.
            _entry.SetId(Guid.CreateVersion7().ToString());
        }

         var resInsert = _dataAccessService.InsertRentResidential(_entry);
        if (resInsert.IsError)
        {
            Debug.WriteLine("Error on insert. @SaveAsNew in ResidentialsViewModel");
            Debug.WriteLine(resInsert.Error.ErrText + Environment.NewLine + resInsert.Error.ErrDescription + Environment.NewLine + resInsert.Error.ErrPlace + Environment.NewLine + resInsert.Error.ErrPlaceParent);

            // TODO: return error object.
            return false;
        }
        else
        {
            // Clear these dirty flags.
            IsEntryDirty = false;
            _entry.IsDirty = false;

            Debug.WriteLine("No errors on insert.");
        }

        // Return a completed task to satisfy the method's return type
        return true;
    }
    private bool SaveAsUpdate()
    {
        var resInsert = _dataAccessService.UpdateRentResidential(_entry);
        if (resInsert.IsError)
        {
            Debug.WriteLine("Error on update. @SaveAsUpdate in ResidentialsViewModel");
            Debug.WriteLine(resInsert.Error.ErrText + Environment.NewLine + resInsert.Error.ErrDescription + Environment.NewLine + resInsert.Error.ErrPlace + Environment.NewLine + resInsert.Error.ErrPlaceParent);

            // TODO: return error object.
            return false;
        }
        else
        {
            // Clear these dirty flags.
            IsEntryDirty = false;
            _entry.IsDirty = false;

            Debug.WriteLine("No errors on update.");
        }

        // Return a completed task to satisfy the method's return type
        return true;
    }

    #endregion

    #region == Unit Window ==

    // Add New Modal window command
    [RelayCommand]
    private void AddNewUnit()
    {
        //TODO: 
        
        // Stupid WinUI3... 
        // https://github.com/microsoft/microsoft-ui-xaml/issues/10396
        // https://github.com/microsoft/WindowsAppSDK/discussions/3680

        Debug.WriteLine("ModalDialogService: ShowUnitDialog method called.");

        if (EditorWin == null)
        {
            // EditorWin should be initialized in the EditorShell constructor.
            Debug.WriteLine("EditorWin should be initialized in the EditorShell constructor.");
            return;
        }

        var dialogWin = new Views.Rent.Residentials.Bldg.Unit.ModalWindow();
        dialogWin.Content = new Views.Rent.Residentials.Bldg.Unit.ModalShell(dialogWin, new ViewModels.Rent.Residentials.Unit.ModalViewModel(), this);

        var hWndDialog = WinRT.Interop.WindowNative.GetWindowHandle(dialogWin);
        //Microsoft.UI.WindowId windowId1 = Microsoft.UI.Win32Interop.GetWindowIdFromWindow(hWnd1);
        //Microsoft.UI.Windowing.AppWindow appWindow = Microsoft.UI.Windowing.AppWindow.GetFromWindowId(windowId1);
        //Microsoft.UI.Windowing.OverlappedPresenter presenter = appWindow.Presenter as Microsoft.UI.Windowing.OverlappedPresenter;
        var hWndEditor = WinRT.Interop.WindowNative.GetWindowHandle(EditorWin);
        SetWindowLong(hWndDialog, GWL_HWNDPARENT, hWndEditor);

        var appWindow = dialogWin.AppWindow;

        var presenter = OverlappedPresenter.Create();

        presenter.IsModal = true;
        presenter.IsResizable = true;
        presenter.PreferredMinimumWidth = 1274;
        presenter.PreferredMinimumHeight = 794;

        appWindow.SetPresenter(presenter);

        dialogWin.Closed += (sender, e) =>
        {
            //ModalWinWidth = 

            // Activate the editor window again.
            EditorWin.Activate();
        };

        // Close the dialog when the editor window is closed.
        EditorWin.Closed += (sender, e) =>
        {
            dialogWin.Close();
        };

        appWindow.MoveAndResize(new Windows.Graphics.RectInt32(ModalWinLeft, ModalWinTop, ModalWinWidth, ModalWinHeight));
        appWindow.Show();
        //dialogWin.Activate();
    }

    #endregion

    #region == Navigation == 

    // Go Back command (don't use this?)
    [RelayCommand]
    private void GoBack()
    {
        EventGoBack?.Invoke(this, EventArgs.Empty);
    }

    [RelayCommand]
    public void GoBackToSummary()
    { 
        EventBackToSummary?.Invoke(this, EventArgs.Empty);
    }

    [RelayCommand]
    public void EditLocation()
    {
        EventEditLocation?.Invoke(this, EventArgs.Empty);
    }

    [RelayCommand]
    private void EditTransportation()
    {
        EventEditTransportation?.Invoke(this, EventArgs.Empty);
    }

    [RelayCommand]
    public void EditAppliance()
    {
        EventEditAppliance?.Invoke(this, EventArgs.Empty);
    }

    [RelayCommand]
    public void EditPictures()
    {
        EventEditPictures?.Invoke(this, EventArgs.Empty);
    }

    [RelayCommand]
    public void EditUnits()
    {
        EventEditUnits?.Invoke(this, EventArgs.Empty);
    }

    #endregion

    #region == Pictures ==

    [RelayCommand]
    public async Task AddNewBuildingPictures()
    {
        // TODO: 
        var destDirectory = System.IO.Path.Combine(System.IO.Path.Combine(System.IO.Path.Combine(App.AppDataPictureFolder, "Rent"), "Residential_Building"), Guid.NewGuid().ToString("N"));

        Debug.WriteLine($"destDirectory={destDirectory}  @AddNewBuildingPictures()");

        if (!Directory.Exists(destDirectory))
        {
            Directory.CreateDirectory(destDirectory);
        }

        var hWnd = WinRT.Interop.WindowNative.GetWindowHandle(EditorWin);
        Microsoft.UI.WindowId windowId = Microsoft.UI.Win32Interop.GetWindowIdFromWindow(hWnd);
        var openPicker = new Microsoft.Windows.Storage.Pickers.FileOpenPicker(windowId);
        //WinRT.Interop.InitializeWithWindow.Initialize(openPicker, hWnd);

        // Set options for your file picker
        openPicker.ViewMode = PickerViewMode.List;
        openPicker.SuggestedStartLocation = Microsoft.Windows.Storage.Pickers.PickerLocationId.PicturesLibrary;
        openPicker.FileTypeFilter.Add(".jpg");
        openPicker.FileTypeFilter.Add(".jpeg");
        openPicker.FileTypeFilter.Add(".png");
        openPicker.FileTypeFilter.Add(".gif");
        openPicker.FileTypeFilter.Add(".webp");

        // Open the picker for the user to pick a file
        var files = await openPicker.PickMultipleFilesAsync();
        if (files.Count > 0)
        {
            //StringBuilder output = new("");
            List<string> list = [];
            foreach (var file in files)
            {
                //output.Append(file.Path + "\n");
                list.Add(file.Path);

                using var sourceStream = File.Open(file.Path, FileMode.Open);
                
                // TODO: set max file size?

                var destFilePath = Path.Combine(destDirectory, System.IO.Path.GetFileName(file.Path));
                //Debug.WriteLine($"{file.Path} to {destFilePath}  @AddNewBuildingPictures()");

                using var destinationStream = File.Create(destFilePath);
                await sourceStream.CopyToAsync(destinationStream);

                // Keep track of unsaved files to delete them when discarding.
                _unsavedBuildingPictureFileList.Add(destFilePath);
            }

            //Debug.WriteLine(output.ToString());
            SetNewBuildingPictures(list);
        }
        else
        {
            Debug.WriteLine("Operation cancelled.");
        }

    }

    [RelayCommand(CanExecute = nameof(CanDeleteSelectedBuildingPicture))]
    public void DeleteSelectedBuildingPicture()
    {
        if (SelectedBuildingPicture == null)
        {
            return;
        }

        if (_entry.BuildingPictures.Remove(SelectedBuildingPicture))
        {
            _entry.BuildingPicturesToBeDeleted.Add(SelectedBuildingPicture);

            IsEntryDirty = true;

            BuildingPictures.Remove(SelectedBuildingPicture);

            SelectedBuildingPicture = null;
        }
    }
    private bool CanDeleteSelectedBuildingPicture()
    {
        return SelectedBuildingPicture != null;
    }

    [RelayCommand(CanExecute = nameof(CanUpdatedBuildingPictureProperty))]
    public void UpdatedBuildingPictureProperty()
    {
        if (SelectedBuildingPicture == null)
        {
            return;
        }

        if (BuildingPictureIsMain)
        {
            foreach (var asdf in BuildingPictures)
            {
                asdf.IsMain = false;
            }
        }

        SelectedBuildingPicture.IsMain = BuildingPictureIsMain;
        SelectedBuildingPicture.Description = BuildingPictureDescription;
        //SelectedBuildingPicture.Label = BuildingPictureLabel;
        var lbl = BuildingPictureTypes.FirstOrDefault(x => x.Key == SelectedBuildingPictureType.Key);
        if (lbl is not null)
        {
            SelectedBuildingPicture.PictureType = lbl;
        }
        else
        {
            Debug.WriteLine($"@UpdatedBuildingPictureProperty: could not find label for key {SelectedBuildingPictureType.Key}");
        }

        SelectedBuildingPicture.IsModified = true;

        IsEntryDirty = true;

        BuildingPicturePropertiesIsDirty = false;
        UpdatedBuildingPicturePropertyCommand.NotifyCanExecuteChanged();
    }
    private bool CanUpdatedBuildingPictureProperty()
    {
        if (SelectedBuildingPicture != null)
        {
            if (BuildingPicturePropertiesIsDirty)
            {
                return true;
            }
        }
        
        return false;
    }

    #endregion

    #region == Location ==

    [RelayCommand(CanExecute = nameof(CanShowGoogleMaps))]
    public async Task ShowGoogleMaps()
    {
        if (string.IsNullOrEmpty(_locationLatitude) || string.IsNullOrEmpty(_locationLongitude))
        {
            return;
        }

        var uriGoogleMaps = new Uri($"https://maps.google.co.jp/?q={LocationLatitude},{LocationLongitude}");
        await Launcher.LaunchUriAsync(uriGoogleMaps);

    }
    private bool CanShowGoogleMaps()
    {
        if (string.IsNullOrEmpty(_locationLatitude) || string.IsNullOrEmpty(_locationLongitude))
        {
            return false;
        }

        return true;
    }

    #endregion

    #region == Transportation ==

    [RelayCommand]
    public async Task ShowRailLineSelect1()
    {
        if (EditorWin is null)
        {
            return;
        }

        var railLine = await _dlg.ShowRailLineSelectDialog(EditorWin);

        if (railLine is not null)
        {
            SelectedRailLine1 = railLine;
        }
    }

    [RelayCommand(CanExecute = nameof(CanShowRailStationSelect1))]
    public async Task ShowRailStationSelect1()
    {
        if (EditorWin is null)
        {
            return;
        }

        if (SelectedRailLine1 is null)
        {
            return;
        }

        var railStation = await _dlg.ShowRailStationSelectDialog(EditorWin, SelectedRailLine1.LineCode);

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

    #endregion

    #region == TEMP code for modal window(for setting an owner) ==

#pragma warning disable IDE0079
#pragma warning disable SYSLIB1054

    [DllImport("User32.dll", SetLastError = true, CharSet = CharSet.Unicode)]

    internal static extern bool EnableWindow(IntPtr hWnd, bool bEnable);

    internal const int GWL_HWNDPARENT = (-8);

    internal static IntPtr SetWindowLong(IntPtr hWnd, int nIndex, IntPtr dwNewLong)
    {
        if (IntPtr.Size == 4)
        {
            return SetWindowLongPtr32(hWnd, nIndex, dwNewLong);
        }
        return SetWindowLongPtr64(hWnd, nIndex, dwNewLong);
    }

    // Import the Windows API function SetWindowLong for modifying window properties on 32-bit systems.
    [DllImport("User32.dll", CharSet = CharSet.Auto, EntryPoint = "SetWindowLong")]
    internal static extern IntPtr SetWindowLongPtr32(IntPtr hWnd, int nIndex, IntPtr dwNewLong);

    // Import the Windows API function SetWindowLongPtr for modifying window properties on 64-bit systems.
    [DllImport("User32.dll", CharSet = CharSet.Auto, EntryPoint = "SetWindowLongPtr")]
    internal static extern IntPtr SetWindowLongPtr64(IntPtr hWnd, int nIndex, IntPtr dwNewLong);

#pragma warning restore SYSLIB1054
#pragma warning restore IDE0079

    #endregion
}
