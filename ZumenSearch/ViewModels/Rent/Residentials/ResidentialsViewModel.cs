using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using System.Xml.Linq;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.Data.Sqlite;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Media;
using Microsoft.UI.Xaml.Media.Animation;
using Microsoft.UI.Xaml.Navigation;
using Windows.System;
using ZumenSearch.Models;
using ZumenSearch.Models.Rent.Residentials;
using ZumenSearch.Services;
using ZumenSearch.Views;
using ZumenSearch.Views.Rent.Residentials.Editor;

namespace ZumenSearch.ViewModels.Rent.Residentials;

public partial class ResidentialsViewModel : ObservableObject
{
    #region == Properties ==

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

    // The Entry property holds the COPY of current RentResidential entry being edited. Do not use it directly in the UI. Apply changes to this object in SaveAsync() to save the changes.
    // MainViewModel creates a new instance of this class and call EditorShell.SetEntry(EntryResidentialFull) and sets this property.
    private Models.Rent.Residentials.EntryResidentialFull _entry = new();

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

    #region == 基本 == 

    // 物件名
    // The Name property holds the name of the entry being edited.
    private string? _name;
    public string Name
    {
        get => _name ?? string.Empty; // Ensure a non-null value is returned
        set
        {
            if (SetProperty(ref _name, value))
            {
                IsEntryDirty = true;
                OnPropertyChanged(nameof(BasicsPreview));
                OnPropertyChanged(nameof(WindowTitle));
            }
        }
    }

    #region == 種別 ==

    // For the use of the ComboBox in the UI, we define a collection of kinds.
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
                OnPropertyChanged(nameof(BasicsPreview));
            }
        }
    }
    #endregion

    // 区分所有か一括所有かを示すプロパティ
    // The IsUnitOwnership property distinguish the building ownership type.
    private bool _isUnitOwnership;
    public bool IsUnitOwnership
    {
        get => _isUnitOwnership;
        set
        {
            if (SetProperty(ref _isUnitOwnership, value))
            {
                IsEntryDirty = true;
                OnPropertyChanged(nameof(BasicsPreview));

                // If this is set, then show/hide the owner and zumen from shell menu.
                EventIsUnitOwnership?.Invoke(this, _isUnitOwnership);
            }
        }
    }

    public string BasicsPreview
    {
        get
        {
            var s = string.Empty;

            if (!string.IsNullOrEmpty(_name))
            {
                s = _name;
            }

            if (!string.IsNullOrEmpty(_selectedKind.Label))
            {
                if (!string.IsNullOrEmpty(s))
                {
                    s += ", ";
                }
                s += _selectedKind.Label;
            }

            if (_isUnitOwnership)
            {
                if (!string.IsNullOrEmpty(s))
                {
                    s += ", ";
                }
                s += "区分所有";
            }

            return s;
        }
    }

    #endregion

    #region == 所在地 ==

    private readonly SqliteConnectionStringBuilder connectionStringBuilder = new("Data Source=" + "mt_town_all.db");

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

                // TODO: move this to service. with async and try catch.
                var dataset = new List<CountyAndCity>();

                using var connection = new SqliteConnection(connectionStringBuilder.ConnectionString);
                connection.Open();
                using var cmd = connection.CreateCommand();
                cmd.CommandText = string.Format("SELECT machiaza_id, county, city FROM mt_town_all WHERE pref LIKE '{0}'", _selectedPef.Name);
                using var reader = cmd.ExecuteReader();
                while (reader.Read())
                {
                    var county = Convert.ToString(reader["county"]) ?? "";
                    var city = Convert.ToString(reader["city"]) ?? "";
                    var id = Convert.ToString(reader["machiaza_id"]);
                    if (id is not null)
                    {
                        var ccty = new CountyAndCity(id, county, city);

                        dataset.Add(ccty);
                    }
                }

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

                // TODO: move this.

                var dataset = new List<WardAndOaza>();

                using var connection = new SqliteConnection(connectionStringBuilder.ConnectionString);
                connection.Open();
                using var cmd = connection.CreateCommand();
                cmd.CommandText = string.Format("SELECT machiaza_id, ward, oaza_cho FROM mt_town_all WHERE pref LIKE '{0}' AND county LIKE '{1}' AND city LIKE '{2}'", _selectedPef.Name, _selectedCity.County, _selectedCity.City);
                using var reader = cmd.ExecuteReader();
                while (reader.Read())
                {
                    var ward = Convert.ToString(reader["ward"]) ?? "";
                    var oaza = Convert.ToString(reader["oaza_cho"]) ?? "";
                    var id = Convert.ToString(reader["machiaza_id"]);
                    if (id is not null)
                    {
                        var ccty = new WardAndOaza(id, ward, oaza);

                        dataset.Add(ccty);
                    }
                }

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

            using var connection = new SqliteConnection(connectionStringBuilder.ConnectionString);
            connection.Open();
            using var cmd = connection.CreateCommand();
            cmd.CommandText = string.Format("SELECT machiaza_id, chome FROM mt_town_all WHERE pref LIKE '{0}' AND county LIKE '{1}' AND city LIKE '{2}' AND ward LIKE '{3}' AND oaza_cho LIKE '{4}'", _selectedPef.Name, _selectedCity.County, _selectedCity.City, _selectedTown.Ward, _selectedTown.Oaza);
            using var reader = cmd.ExecuteReader();
            while (reader.Read())
            {
                var cho = Convert.ToString(reader["chome"]) ?? "";
                var id = Convert.ToString(reader["machiaza_id"]);
                if (id is not null)
                {
                    var ccty = new Choume(id, cho);

                    dataset.Add(ccty);
                }
            }

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

    // db:loc_edaban
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

    #endregion

    #region == 交通 ==

    private string _ensen = string.Empty;
    public string Ensen
    {
        get => _ensen;
        set
        {
            if (SetProperty(ref _ensen, value))
            {
                IsEntryDirty = true;
                OnPropertyChanged(nameof(TransportationPreview));
            }
        }
    }

    private string _eki = string.Empty;
    public string Eki
    {
        get => _eki;
        set
        {
            if (SetProperty(ref _eki, value))
            {
                IsEntryDirty = true;
                OnPropertyChanged(nameof(TransportationPreview));
            }
        }
    }

    private int _ekiToho = 0;
    public int EkiToho
    {
        get => _ekiToho;
        set
        {
            if (SetProperty(ref _ekiToho, value))
            {
                IsEntryDirty = true;
                OnPropertyChanged(nameof(TransportationPreview));
            }
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
                OnPropertyChanged(nameof(TransportationPreview));
            }
        }
    }

    private int __busJyousya = 0;
    public int BusJyousya
    {
        get => __busJyousya;
        set
        {
            if (SetProperty(ref __busJyousya, value))
            {
                IsEntryDirty = true;
                OnPropertyChanged(nameof(TransportationPreview));
            }
        }
    }

    private int _busStopToho = 0;
    public int BusStopToho
    {
        get => _busStopToho;
        set
        {
            if (SetProperty(ref _busStopToho, value))
            {
                IsEntryDirty = true;
                OnPropertyChanged(nameof(TransportationPreview));
            }
        }
    }

    public string TransportationPreview
    {
        get
        {
            var s = string.Empty;

            if (!string.IsNullOrEmpty(_ensen))
            {
                s = _ensen;
            }

            if (!string.IsNullOrEmpty(_eki))
            {
                if (!string.IsNullOrEmpty(s))
                {
                    s += ", ";
                }
                s += $"{_eki}駅";
            }

            if (_ekiToho > 0)
            {
                if (!string.IsNullOrEmpty(s))
                {
                    s += ", ";
                }
                s += $"徒歩{_ekiToho}分";
            }

            return s;
        }
    }

    #endregion

    #region == 構造 ==

    private Structure _selectedStructure = new(Models.Rent.Residentials.EnumStructure.Unspecified, "");
    public Structure SelectedStructure
    {
        get => _selectedStructure;
        set
        {
            if (SetProperty(ref _selectedStructure, value))
            {
                IsEntryDirty = true;
                OnPropertyChanged(nameof(StructurePreview));
            }
        }
    }

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

    private int _basementFloorCount = 0;
    public int BasementFloorCount
    {
        get => _basementFloorCount;
        set
        {
            if (SetProperty(ref _basementFloorCount, value))
            {
                IsEntryDirty = true;
                OnPropertyChanged(nameof(StructurePreview));
            }
        }
    }

    private int _aboveGroundFloorCount = 0;
    public int AboveGroundFloorCount
    {
        get => _aboveGroundFloorCount;
        set
        {
            if (SetProperty(ref _aboveGroundFloorCount, value))
            {
                IsEntryDirty = true;
                OnPropertyChanged(nameof(StructurePreview));
            }
        }
    }

    private int _totalUnitCount = 0;
    public int TotalUnitCount
    {
        get => _totalUnitCount;
        set
        {
            if (SetProperty(ref _totalUnitCount, value))
            {
                IsEntryDirty = true;
                OnPropertyChanged(nameof(StructurePreview));
            }
        }
    }

    private DateTimeOffset? _selectedBuildingBuiltMonthYear;// = new DateTime(1950, 1, 1);
    public DateTimeOffset? SelectedBuildingBuiltMonthYear
    {
        get => _selectedBuildingBuiltMonthYear;
        set
        {
            if (SetProperty(ref _selectedBuildingBuiltMonthYear, value))
            {
                IsEntryDirty = true;
                OnPropertyChanged(nameof(StructurePreview));
            }
        }
    }

    public string StructurePreview
    {
        get
        {
            var s = string.Empty;

            if (_selectedStructure != null)
            {
                if (!string.IsNullOrEmpty(_selectedStructure.Label))
                {
                    s = _selectedStructure.Label;
                }
            }
            if (AboveGroundFloorCount > 0)
            {
                if (!string.IsNullOrEmpty(s))
                {
                    s += ", ";
                }

                s += $"地上{AboveGroundFloorCount}階";
            }
            if (BasementFloorCount > 0)
            {
                if (AboveGroundFloorCount > 0)
                {
                    s += ", ";
                }
                s += $"地下{BasementFloorCount}階";
            }
            if (TotalUnitCount > 0)
            {
                if (!string.IsNullOrEmpty(s))
                {
                    s += ", ";
                }
                s += $"総戸数{TotalUnitCount}戸";
            }
            if (SelectedBuildingBuiltMonthYear != null)
            {
                if (!string.IsNullOrEmpty(s))
                {
                    s += ", ";
                }
                s += $"{SelectedBuildingBuiltMonthYear:yyyy年M月}築";
            }

                
            return s;//$"{SelectedStructure.Label}, 地上{AboveGroundFloorCount}, 地下{BasementFloorCount}階建て, 総戸数{TotalUnitCount}戸, {BuildingBuiltMonthYear:yyyy年M月}築";
        }
    }

    #endregion

    #region == 設備 ==


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

    #region == 管理 ==

    /*
    public ObservableCollection<KanriShutai> KanriShutais =
    [
        new KanriShutai(Models.Rent.Residentials.EnumKanriShutai.Owner.ToString(), "貸主"),
        new KanriShutai(Models.Rent.Residentials.EnumKanriShutai.Jisya.ToString(), "自社"),
        new KanriShutai(Models.Rent.Residentials.EnumKanriShutai.Tasha.ToString(), "他社"),
        new KanriShutai(EnumKanriShutai.Unspecified.ToString(), "未指定")
    ];
    */

    private Models.Rent.Residentials.EnumKanriShutai _selectedKanriShutai = Models.Rent.Residentials.EnumKanriShutai.Unspecified;
    public Models.Rent.Residentials.EnumKanriShutai SelectedKanriShutai
    {
        get => _selectedKanriShutai;
        set
        {
            if (SetProperty(ref _selectedKanriShutai, value))
            {
                IsEntryDirty = true;
                OnPropertyChanged(nameof(KanriPreview));
            }
        }
    }

    private readonly bool _isKanriOwnerOptionSelected = false;
    public bool IsKanriOwnerOptionSelected
    {
        get => _isKanriOwnerOptionSelected;
        set
        {
            if (value)
            {
                SelectedKanriShutai = Models.Rent.Residentials.EnumKanriShutai.Owner;
            }
        }
    }

    private readonly bool _isKanriJisyaOptionSelected = false;
    public bool IsKanriJisyaOptionSelected
    {
        get => _isKanriJisyaOptionSelected;
        set
        {
            if (value)
            {
                SelectedKanriShutai = Models.Rent.Residentials.EnumKanriShutai.Jisya;
            }
        }
    }

    private readonly bool _isKanriTasyaOptionSelected = false;
    public bool IsKanriTasyaOptionSelected
    {
        get => _isKanriTasyaOptionSelected;
        set
        {
            if (value)
            {
                SelectedKanriShutai = Models.Rent.Residentials.EnumKanriShutai.Tasha;
            }
        }
    }

    private readonly bool _isKanriUnspecifiedOptionSelected = true;
    public bool IsKanriUnspecifiedOptionSelected
    {
        get => _isKanriUnspecifiedOptionSelected;
        set
        {
            if (value)
            {
                SelectedKanriShutai = Models.Rent.Residentials.EnumKanriShutai.Unspecified;
            }
        }
    }

    private string _kanriTashaName = string.Empty;
    public string KanriTashaName
    {
        get => _kanriTashaName;
        set
        {
            if (SetProperty(ref _kanriTashaName, value))
            {
                IsEntryDirty = true;
                OnPropertyChanged(nameof(KanriPreview));
            }
        }
    }

    private string _kanriTashaContactInfo = string.Empty;
    public string KanriTashaContactInfo
    {
        get => _kanriTashaContactInfo;
        set
        {
            if (SetProperty(ref _kanriTashaContactInfo, value))
            {
                IsEntryDirty = true;
                OnPropertyChanged(nameof(KanriPreview));
            }
        }
    }

    public string KanriPreview
    {
        get
        {
            var s = string.Empty;

            if (SelectedKanriShutai == EnumKanriShutai.Owner)
            {
                s += "貸主管理";
            }
            else if (SelectedKanriShutai == EnumKanriShutai.Jisya)
            {
                s += "自社管理";
            }
            else if (SelectedKanriShutai == EnumKanriShutai.Tasha)
            {
                s += "他社管理";

                if ((!string.IsNullOrEmpty(KanriTashaName.Trim())) && (!string.IsNullOrEmpty(KanriTashaContactInfo.Trim())))
                {
                    s += $" ({KanriTashaName}, {KanriTashaContactInfo})";
                }
                else if (!string.IsNullOrEmpty(KanriTashaName))
                {
                    s += $" ({KanriTashaName})";
                }
            }
            else if (SelectedKanriShutai == EnumKanriShutai.Unspecified)
            {
                s += "";//管理主体未指定
            }

            return s;
        }
    }

    #endregion

    #region == 備考 ==

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

    #region == 写真（建物） ==

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

    private PictureBuilding? _selectedBuildingPicture;
    public PictureBuilding? SelectedBuildingPicture
    {
        get => _selectedBuildingPicture;
        set
        {
            // "quietly" clear values.
            _buildingPictureIsMain = false;
            _buildingPictureTitle = string.Empty;
            _buildingPictureDescription = string.Empty;

            _buildingPicturePropertiesIsDirty = false;

            if (SetProperty(ref _selectedBuildingPicture, value))
            {
                if (_selectedBuildingPicture != null)
                {
                    IsBuildingPictureEditPaneVisible = true;

                    // "quietly" update values.
                    _buildingPictureIsMain = _selectedBuildingPicture.IsMain;
                    _buildingPictureTitle = _selectedBuildingPicture.Title ?? string.Empty;
                    _buildingPictureDescription = _selectedBuildingPicture.Description ?? string.Empty;

                    _buildingPicturePropertiesIsDirty = false;
                }
                else
                {
                    IsBuildingPictureEditPaneVisible = false;
                }
            }

            // notify updates.
            OnPropertyChanged(nameof(BuildingPictureTitle));
            OnPropertyChanged(nameof(BuildingPictureDescription));
            OnPropertyChanged(nameof(BuildingPictureIsMain));
            OnPropertyChanged(nameof(BuildingPicturePropertiesIsDirty));

            UpdatedBuildingPicturePropertyCommand.NotifyCanExecuteChanged();

            DeleteSelectedBuildingPictureCommand.NotifyCanExecuteChanged();
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

    private string _buildingPictureTitle = string.Empty;
    public string BuildingPictureTitle
    {
        get => _buildingPictureTitle;
        set 
        {
            if (SetProperty(ref _buildingPictureTitle, value))
            {
                BuildingPicturePropertiesIsDirty = true;
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

    #endregion


    #region == Events

    // The event handlers below are used to notify the UI about various actions that can be performed in the editor.

    // EditorShell subscribes to these events to handle the actions accordingly.
    public event EventHandler? EventAddNewUnit;
    public event EventHandler? EventBackToSummary;
    public event EventHandler? EventEditLocation;
    public event EventHandler? EventEditTransportation;
    public event EventHandler? EventEditAppliance;
    public event EventHandler? EventEditPictures;
    public event EventHandler? EventEditUnits;
    public event EventHandler? EventAddNewBuildingPictures;

    public event EventHandler<bool>? EventIsUnitOwnership; // show or hides navigationview' menu accordingly.

    // Who subscribes to this event?
    public event EventHandler? EventGoBack;

    #endregion

    #region == Services ==
    
    // The IDataAccessService is used to access the data layer for saving and updating entries.
    private readonly IDataAccessService _dataAccessService;
    private readonly IModalDialogService _dlg;

    #endregion

    #region == Constructor ==

    // Constructor for the EditorViewModel class, initializes the data access service and the entry.
    #pragma warning disable IDE0290
    public ResidentialsViewModel(IDataAccessService dataAccessService, IModalDialogService modalDialog )
    {
        _dataAccessService = dataAccessService;
        _dlg = modalDialog;

    }
    #pragma warning restore IDE0290

    #endregion

    #region == Methods ==

    public void SetEntry(Models.Rent.Residentials.EntryResidentialFull entry)
    {
        if (entry is null)
        {
            return;
        }

        _entry = entry;

        Name = _entry.Name;

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

        ////////////////

        //TODO: Set other properties

        BuildingPictures = new ObservableCollection<PictureBuilding>(_entry.BuildingPictures); // create a copy.
        

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

    /*
    public void SetEditorWindow(Views.Rent.Residentials.Editor.EditorWindow editorWin)
    {
        _editorWindow = editorWin;

        // Set the Id property of the window to the Entry.Id if _entry is not null (most likely is for now).
        if (_entry != null)
        {
            _editorWindow.Id = _entry.Id;
        }
    }
    */

    #endregion

    #region == Commands ==

    [RelayCommand(CanExecute = nameof(CanSave))]
    private void Save()
    {
        SaveAsync();
    }
    private bool CanSave()
    {
        return IsEntryDirty;
    }
    private async void SaveAsync()
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


        if (string.IsNullOrEmpty(_entry.Id))
        {
            // If the Entry.Id is empty, generate a new ID and save as new.
            _entry.SetId = Guid.CreateVersion7().ToString();
            await SaveAsNew();
        }
        else
        {
            // If the Entry.Id is not empty, update the existing entry.
            await SaveAsUpdate();
        }
    }
    private Task SaveAsNew()
    {
        var resInsert = _dataAccessService.InsertRentResidential(_entry);
        if (resInsert.IsError)
        {
            Debug.WriteLine("Error on insert. @SaveAsNew in ResidentialsViewModel");
            Debug.WriteLine(resInsert.Error.ErrText + Environment.NewLine + resInsert.Error.ErrDescription + Environment.NewLine + resInsert.Error.ErrPlace + Environment.NewLine + resInsert.Error.ErrPlaceParent);

            App.CurrentDispatcherQueue?.TryEnqueue(() =>
            {
                // TODO: Show error message to user
            });
        }
        else
        {
            // Clear these dirty flags.
            IsEntryDirty = false;
            _entry.IsDirty = false;

            Debug.WriteLine("No errors on insert.");
        }

        // Return a completed task to satisfy the method's return type
        return Task.CompletedTask;
    }
    private Task SaveAsUpdate()
    {
        var resInsert = _dataAccessService.UpdateRentResidential(_entry);
        if (resInsert.IsError)
        {
            Debug.WriteLine("Error on update. @SaveAsUpdate in ResidentialsViewModel");
            Debug.WriteLine(resInsert.Error.ErrText + Environment.NewLine + resInsert.Error.ErrDescription + Environment.NewLine + resInsert.Error.ErrPlace + Environment.NewLine + resInsert.Error.ErrPlaceParent);

            App.CurrentDispatcherQueue?.TryEnqueue(() =>
            {
                // TODO: Show error message to user
            });
        }
        else
        {
            // Clear these dirty flags.
            IsEntryDirty = false;
            _entry.IsDirty = false;

            Debug.WriteLine("No errors on update.");
        }

        // Return a completed task to satisfy the method's return type
        return Task.CompletedTask;
    }

    // Add New Modal window command
    [RelayCommand]
    private void AddNewUnit()
    {
        //NavigationService.NavigateTo(typeof(RentLivingEditShellViewModel).FullName!, "test");
        //_dlg.ShowUnitDialog(this,);
        EventAddNewUnit?.Invoke(this, EventArgs.Empty);
    }

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


    [RelayCommand]
    public void AddNewBuildingPictures()
    {
        EventAddNewBuildingPictures?.Invoke(this, EventArgs.Empty);
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
        SelectedBuildingPicture.Title = BuildingPictureTitle;
        SelectedBuildingPicture.Description = BuildingPictureDescription;

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
}
