using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.UI.Xaml.Media.Animation;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.Globalization;
using System.Text;
using Windows.System;
using ZumenSearch.Models;
using ZumenSearch.Models.Rent.Residentials;
using ZumenSearch.Services.Contracts;

namespace ZumenSearch.ViewModels.Rent.Residentials;

public partial class BldgViewModel : ObservableObject
{
    #region == Private Variables ==

    private readonly ViewModels.Rent.Residentials.MainViewModel _mainViewModel;

    // The Entry property holds the COPY of current RentResidential entry being edited.
    // Do not use it directly in the UI. Apply changes to this object in SaveAsync() to save the changes.
    // MainViewModel creates a new instance of this class and call EditorShell.SetEntry(EntryResidentialFull) and sets this property.
    private readonly Models.Rent.Residentials.EntryResidentialFull _entry;

    // Local directory path to save blob data such as pictures and PDFs.
    private string _entryDataDirectoryPath;

    // Tmp file list to hold unsaved picture files. (if entry is not saved, delete on close)
    private readonly List<string> _unsavedBuildingPictureFileList = [];

    #endregion

    #region == Public Properties ==

    public EnumEntryStatus EntryStatus => _entry.EntryStatus;

    [ObservableProperty]
    [NotifyCanExecuteChangedFor(nameof(SaveCommand))]
    public partial bool IsDirty { get; private set; }

    #region == 建物基本プロパティ == 

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
                _mainViewModel.WindowTitle = string.Empty;

                //EventTitleChanged?.Invoke(this, EventArgs.Empty);//TODO
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
    //private Kind _selectedKind = new(Models.Rent.Residentials.EnumKinds.Unspecified.ToString(), "");
    public Kind SelectedKind
    {
        get => field ?? new(Models.Rent.Residentials.EnumKinds.Unspecified.ToString(), "");
        set
        {
            if (SetProperty(ref field, value))
            {
                IsDirty = true;
            }
        }
    }

    // 区分所有か一括所有か
    public bool IsUnitOwnership
    {
        get => field;
        set
        {
            if (SetProperty(ref field, value))
            {
                IsDirty = true;
                OnPropertyChanged(nameof(IsNotUnitOwnership));

                // TODO:
                // If this is set, then show/hide the owner and zumen from shell menu.
                //EventIsUnitOwnership?.Invoke(this, field);
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

    //private Structure _selectedStructure = new(Models.Rent.Residentials.EnumStructure.Unspecified, "");
    public Structure SelectedStructure
    {
        get => field ?? new(Models.Rent.Residentials.EnumStructure.Unspecified, "");
        set
        {
            if (SetProperty(ref field, value))
            {
                IsDirty = true;
            }
        }
    }

    // 地上階
    //private string _aboveGroundFloorCount = string.Empty;
    public string AboveGroundFloorCount
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

            var text = Helpers.Common.ReplaceZenkakuNumber(value.Trim());

            if (Helpers.Common.CanConvertToPositiveNumber(text))
            {
                field = text;
                IsDirty = true;
            }
            else
            {
                // TODO: show error
                field = string.Empty;
                //IsDirty = true;
            }

            OnPropertyChanged(nameof(AboveGroundFloorCount));
        }
    }

    // 地下階
    public string BasementFloorCount
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

            var text = Helpers.Common.ReplaceZenkakuNumber(value.Trim());

            if (Helpers.Common.CanConvertToPositiveNumber(text))
            {
                field = text;
                IsDirty = true;
            }
            else
            {
                // TODO: show error
                field = string.Empty;
                //IsDirty = true;
            }

            OnPropertyChanged(nameof(BasementFloorCount));
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

            var text = Helpers.Common.ReplaceZenkakuNumber(value.Trim());

            if (Helpers.Common.CanConvertToPositiveNumber(text))
            {
                field = text;
                IsDirty = true;
            }
            else
            {
                // TODO: show error
                field = string.Empty;
                //IsDirty = true;
            }

            OnPropertyChanged(nameof(TotalUnitCount));
        }
    }

    // 築年月
    public DateTimeOffset? BuiltYearAndMonth
    {
        get => field;
        set
        {
            if (SetProperty(ref field, value))
            {
                IsDirty = true;
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
            if (SetProperty(ref field, value.Trim()))
            {
                IsDirty = true;
            }
        }
    }

    // 特定コード（４桁）建物全体は0000
    public string FudousanIdAdditionalCode
    {
        get => field ?? string.Empty; // keep non-null empty string.
        set
        {
            if (SetProperty(ref field, value.Trim()))
            {
                IsDirty = true;
            }
        }
    } = "0000";

    #endregion

    #region == 所在地プロパティ ==

    public string? MachiazaId;

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

    public Pref? SelectedPef
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

            var text = Helpers.Common.ReplaceZenkakuNumber(value.Trim());

            if (Helpers.Common.CanConvertToPositiveNumber(text))
            {
                field = text;
                IsDirty = true;
            }
            else
            {
                // TODO: show error
                field = string.Empty;
                //IsDirty = true;
            }

            OnPropertyChanged(nameof(EkiToho1));
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

            var text = Helpers.Common.ReplaceZenkakuNumber(value.Trim());

            if (Helpers.Common.CanConvertToPositiveNumber(text))
            {
                field = text;
                IsDirty = true;
            }
            else
            {
                // TODO: show error
                field = string.Empty;
                //IsDirty = true;
            }

            OnPropertyChanged(nameof(BusJyousya1));
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

            var text = Helpers.Common.ReplaceZenkakuNumber(value.Trim());

            if (Helpers.Common.CanConvertToPositiveNumber(text))
            {
                field = text;
                IsDirty = true;
            }
            else
            {
                // TODO: show error
                field = string.Empty;
                //IsDirty = true;
            }

            OnPropertyChanged(nameof(BusStopToho1));
        }
    } = string.Empty;

    #endregion

    #region == 設備プロパティ ==


    public bool Ap_IsAutolock
    {
        get;
        set
        {
            if (SetProperty(ref field, value))
            {
                OnPropertyChanged(nameof(AppliancePreview));
            }
        }
    }

    public bool Ap_IsElevator
    {
        get;
        set
        {
            if (SetProperty(ref field, value))
            {
                OnPropertyChanged(nameof(AppliancePreview));
            }
        }
    }

    public bool Ap_IsSecurityCamera
    {
        get;
        set
        {
            if (SetProperty(ref field, value))
            {
                OnPropertyChanged(nameof(AppliancePreview));
            }
        }
    }

    public bool Ap_IsParcelLocker
    {
        get;
        set
        {
            if (SetProperty(ref field, value))
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

    public string TatemonoMemo
    {
        get;
        set
        {
            if (SetProperty(ref field, value))
            {
                IsDirty = true;
                OnPropertyChanged(nameof(MemoPreview));
            }
        }
    } = string.Empty;

    public string MemoPreview
    {
        get
        {
            string s;
            if (TatemonoMemo.Length > 14)
            {
                s = TatemonoMemo[..14] + "...";
            }
            else
            {
                s = TatemonoMemo;
            }

            return s;
        }
    }

    #endregion

    #region == 写真プロパティ ==

    public ObservableCollection<PictureBuilding> BuildingPictures
    {
        get;
        set
        {
            if (SetProperty(ref field, value))
            {
                IsDirty = true;

                OpenBuildingPictureDirectoryCommand.NotifyCanExecuteChanged();
            }
        }
    } = [];

    public bool IsBuildingPictureEditPaneVisible
    {
        get;
        set
        {
            if (SetProperty(ref field, value))
            {
                //
            }
        }
    } = false;

    public PictureBuilding? SelectedBuildingPicture
    {
        get;
        set
        {
            if (field == value)
            {
                return;
            }

            field = value;

            // "quietly" clear values.
            _buildingPictureIsMain = false;
            _selectedBuildingPictureType = null;// Needed to be null.//new(EnumBuildingPictureType.Unspecified);
            _buildingPictureDescription = string.Empty;

            if (field is not null)
            {
                // "quietly" update values.
                _buildingPictureIsMain = field.IsMain;
                _buildingPictureDescription = field.Description ?? string.Empty;
                var lbl = BuildingPictureTypes.FirstOrDefault(x => x.Key == field.PictureType.Key);
                if (lbl is not null)
                {
                    _selectedBuildingPictureType = lbl;
                }
                else
                {
                    // Unspecified.
                    Debug.WriteLine($"@SelectedBuildingPicture: could not find label for key {field.PictureType.Key}");
                }

                IsBuildingPictureEditPaneVisible = true;
            }
            else
            {
                IsBuildingPictureEditPaneVisible = false;
                Debug.WriteLine($"@SelectedBuildingPicture: value is null");
            }

            OnPropertyChanged(nameof(SelectedBuildingPicture));

            // notify updates.
            OnPropertyChanged(nameof(SelectedBuildingPictureType));
            OnPropertyChanged(nameof(BuildingPictureDescription));
            OnPropertyChanged(nameof(BuildingPictureIsMain));
            //OnPropertyChanged(nameof(BuildingPicturePropertiesIsDirty));

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

    private BuildingPictureType? _selectedBuildingPictureType = new(EnumBuildingPictureType.Unspecified);
    public BuildingPictureType? SelectedBuildingPictureType
    {
        get => _selectedBuildingPictureType;
        set
        {
            if (SelectedBuildingPicture is null)
            {
                // Should not happen.
                Debug.WriteLine($"SelectedBuildingPictureType: SelectedBuildingPicture is null, cannot set value.");
                return;
            }

            if (SetProperty(ref _selectedBuildingPictureType, value))
            {
                if (_selectedBuildingPictureType is not null)
                {
                    //Debug.WriteLine($"_selectedBuildingPictureType changed: {_selectedBuildingPictureType.Label}");

                    SelectedBuildingPicture.PictureType = _selectedBuildingPictureType;
                    IsDirty = true;
                    SelectedBuildingPicture.IsModified = true;
                    //BuildingPicturePropertiesIsDirty = true;
                }
                else
                {
                    Debug.WriteLine($"_selectedBuildingPictureType changed: null");
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
            if (SelectedBuildingPicture is null)
            {
                // Should not happen.
                Debug.WriteLine($"BuildingPictureDescription: SelectedBuildingPicture is null, cannot set value.");
                return;
            }

            if (SetProperty(ref _buildingPictureDescription, value))
            {
                SelectedBuildingPicture.Description = _buildingPictureDescription;
                IsDirty = true;
                SelectedBuildingPicture.IsModified = true;
                //BuildingPicturePropertiesIsDirty = true;
            }
        }
    }

    private bool _buildingPictureIsMain;
    public bool BuildingPictureIsMain
    {
        get => _buildingPictureIsMain;
        set
        {
            if (SelectedBuildingPicture is null)
            {
                // Should not happen.
                Debug.WriteLine($"BuildingPictureIsMain: SelectedBuildingPicture is null, cannot set value.");
                return;

            }

            if (SetProperty(ref _buildingPictureIsMain, value))
            {
                if (_buildingPictureIsMain)
                {
                    foreach (var asdf in BuildingPictures)
                    {
                        asdf.IsMain = false;
                    }
                }

                SelectedBuildingPicture.IsMain = _buildingPictureIsMain;
                IsDirty = true;
                SelectedBuildingPicture.IsModified = true;
                //BuildingPicturePropertiesIsDirty = true;
            }
        }
    }

    #endregion

    #region == 部屋 ==

    public ObservableCollection<Models.Rent.Residentials.Room> Rooms
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

    #region == Services ==

    private readonly IDataAccessService _dataAccessService;
    private readonly IDataAccessLocationService _dataAccessLocationService;

    #endregion

    public BldgViewModel(ViewModels.Rent.Residentials.MainViewModel vm, Models.Rent.Residentials.EntryResidentialFull entry, IDataAccessService dataAccessService, IDataAccessLocationService dataAccessLocationService)
    {
        _mainViewModel = vm;
        _dataAccessService = dataAccessService;
        _dataAccessLocationService = dataAccessLocationService;

        _entry = entry;
        _entryDataDirectoryPath = System.IO.Path.Combine(System.IO.Path.Combine(System.IO.Path.Combine(App.AppDataPictureFolder, "Rent"), "Residential_Building"), _entry.Id);

        try
        {
            PopulateEntryValues();
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"BldgViewModel: {ex}");
        }
        finally
        {
            IsDirty = false;
        }

    }

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

        if (Chous is not null) // Allow an empty string. //&& (!string.IsNullOrEmpty(_entry.LocChoume))
        {
            var hoge = Chous.FirstOrDefault<Choume>(p => p.Chou.Equals(_entry.LocChoume));
            if (hoge is not null)
            {
                SelectedChou = hoge;
            }
        }

        Edaban = _entry.LocEdaban;

        //TODO: Set other properties


        // Pictures TODO:
        BuildingPictures = new ObservableCollection<PictureBuilding>(_entry.BuildingPictures); // create a copy.
        //OpenBuildingPictureDirectoryCommand.NotifyCanExecuteChanged();


        // Rooms
        Rooms = new ObservableCollection<Models.Rent.Residentials.Room>(_entry.Rooms); // create a copy.
        Rooms.CollectionChanged += (s, e) => 
        {
            // Unsubscribe from removed items
            if (e.OldItems != null)
            {
                foreach (Models.Rent.Residentials.Room item in e.OldItems)
                {
                    Debug.WriteLine($"Item {item.RoomName} Removed from Rooms");
                    IsDirty = true;

                    item.PropertyChanged -= OnRoomPropertyChanged;
                }
            }

            // Subscribe to PropertyChanged.
            if (e.NewItems != null)
            {
                foreach (Models.Rent.Residentials.Room item in e.NewItems)
                {
                    Debug.WriteLine($"Item {item.RoomName} Added to Rooms");
                    IsDirty = true;

                    item.PropertyChanged += OnRoomPropertyChanged;
                }
            }
        };

        //Debug.WriteLine($"PopulateEntryValues: Completed populating values from Entry to VM. Entry ID: {_entry.Id}, Rooms Count: {Rooms.Count}");
    }

    private void OnRoomPropertyChanged(object? sender, PropertyChangedEventArgs e)
    {
        if (sender is not Models.Rent.Residentials.Room)
        {
            Debug.WriteLine("OnItemPropertyChanged returned non room.");
        }
        // Catch the specific item property update here
        //var updatedItem = sender as Models.Rent.Residentials.Room;

        Debug.WriteLine($"Property {e.PropertyName} changed");

        IsDirty = true;
    }

    private void SetValuesToEntry()
    {
        if (!IsDirty)
        {
            return;
        }

        // 物件名
        _entry.Name = Name;

        // 所在地
        _entry.LocPrefId = (SelectedPef is not null) ? SelectedPef.MunicipalityCode : string.Empty;
        _entry.LocPrefecture = (SelectedPef is not null) ? SelectedPef.Name : string.Empty;
        _entry.LocMachiazaId = (!string.IsNullOrEmpty(MachiazaId)) ? MachiazaId : string.Empty;
        _entry.LocCounty = (SelectedCity is not null) ? SelectedCity.County : string.Empty;
        _entry.LocCity = (SelectedCity is not null) ? SelectedCity.City : string.Empty;
        _entry.LocWard = (SelectedTown is not null) ? SelectedTown.Ward : string.Empty;
        _entry.LocOazaCho = (SelectedTown is not null) ? SelectedTown.Oaza : string.Empty;
        _entry.LocChoume = (SelectedChou is not null) ? SelectedChou.Chou : string.Empty;
        _entry.LocEdaban = (!string.IsNullOrEmpty(Edaban)) ? Edaban : string.Empty;
        _entry.LocLocationFull = AddressPreview;


        //TODO: Set other properties for Entry.
        //////////////////

        // 写真
        _entry.BuildingPictures = BuildingPictures;

        // 部屋
        _entry.Rooms = Rooms;

    }

    private void SaveAsNew()
    {
        var resInsert = _dataAccessService.InsertRentResidential(_entry);
        if (resInsert.IsError)
        {
            Debug.WriteLine("Error on insert. @SaveAsNew in ResidentialsViewModel");
            Debug.WriteLine(resInsert.Error.ErrText + Environment.NewLine + resInsert.Error.ErrDescription + Environment.NewLine + resInsert.Error.ErrPlace + Environment.NewLine + resInsert.Error.ErrPlaceParent);

            // TODO: return error object.
            return;
        }
        else
        {
            IsDirty = false;
            _entry.IsDirty = false;

            _entry.EntryStatus = EnumEntryStatus.Saved;

            Debug.WriteLine("No errors on insert.");

            // Update title with dummy value.
            _mainViewModel.WindowTitle = string.Empty;
        }

        return;
    }

    private void SaveAsUpdate()
    {
        var resInsert = _dataAccessService.UpdateRentResidential(_entry);
        if (resInsert.IsError)
        {
            Debug.WriteLine("Error on update. @SaveAsUpdate in ResidentialsViewModel");
            Debug.WriteLine(resInsert.Error.ErrText + Environment.NewLine + resInsert.Error.ErrDescription + Environment.NewLine + resInsert.Error.ErrPlace + Environment.NewLine + resInsert.Error.ErrPlaceParent);

            // TODO: return error object.
            return;
        }
        else
        {
            IsDirty = false;
            _entry.IsDirty = false;

            _entry.EntryStatus = EnumEntryStatus.Saved;

            Debug.WriteLine("No errors on update.");

            // Update title with dummy value.
            _mainViewModel.WindowTitle = string.Empty;
        }
    }

    #endregion

    #region == Public Methods ==

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
                Id = Guid.CreateVersion7().ToString("N")

            };

            BuildingPictures.Add(pic);

            OpenBuildingPictureDirectoryCommand.NotifyCanExecuteChanged();
            DeleteSelectedBuildingPictureCommand.NotifyCanExecuteChanged();
            IsDirty = true;
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

        // TODO: More like PDF etc.

        if (Directory.Exists(_entryDataDirectoryPath))
        {
            Debug.WriteLine($"Deleting folder: {_entryDataDirectoryPath}");
            Directory.Delete(_entryDataDirectoryPath, true);
        }
    }

    #endregion

    #region == Commands ==

    #region == Save related commands ==

    [RelayCommand(CanExecute = nameof(CanSave))]
    public void Save()
    {
        // TODO: What if Unit.IsDirty??

        if (!IsDirty)
        {
            return;
        }

        if (string.IsNullOrEmpty(Name))
        {
            Debug.WriteLine("TODO: Name is null or empty. This field is required. Show alart and abort.");
            return;
        }

        SetValuesToEntry();

        if (_entry.EntryStatus == EnumEntryStatus.New)
        {
            SaveAsNew();
        }
        else
        {
            SaveAsUpdate();
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

    #region == Pictures related commands ==

    [RelayCommand]
    public async Task AddNewBuildingPictures()
    {
        Debug.WriteLine($"destDirectory={_entryDataDirectoryPath}  @AddNewBuildingPictures()");
        // TODO:
        /*
        if (!Directory.Exists(_entryDataDirectoryPath))
        {
            Directory.CreateDirectory(_entryDataDirectoryPath);
        }

        var hWnd = WinRT.Interop.WindowNative.GetWindowHandle(_editorWin);
        var windowId = Microsoft.UI.Win32Interop.GetWindowIdFromWindow(hWnd);
        var openPicker = new Microsoft.Windows.Storage.Pickers.FileOpenPicker(windowId);
        //WinRT.Interop.InitializeWithWindow.Initialize(openPicker, hWnd);

        // Set options for your file picker
        openPicker.ViewMode = Microsoft.Windows.Storage.Pickers.PickerViewMode.List;
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
                using var sourceStream = File.Open(file.Path, FileMode.Open);

                // TODO: set max file size?

                var destFilePath = Path.Combine(_entryDataDirectoryPath, System.IO.Path.GetFileName(file.Path));
                //Debug.WriteLine($"{file.Path} to {destFilePath}  @AddNewBuildingPictures()");

                using var destinationStream = File.Create(destFilePath);
                await sourceStream.CopyToAsync(destinationStream);

                list.Add(destFilePath);

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
        */
    }

    [RelayCommand(CanExecute = nameof(CanDeleteSelectedBuildingPicture))]
    public void DeleteSelectedBuildingPicture()
    {
        if (SelectedBuildingPicture is null)
        {
            return;
        }

        if (BuildingPictures.Remove(SelectedBuildingPicture))
        {
            //TODO:
            /*
            if (_entry.BuildingPictures.Remove(SelectedBuildingPicture))
            {
                _entry.BuildingPicturesToBeDeleted.Add(SelectedBuildingPicture);
            }
            */
            IsDirty = true;
            SelectedBuildingPicture = null;
        }
    }
    private bool CanDeleteSelectedBuildingPicture()
    {
        return SelectedBuildingPicture is not null;
    }

    [RelayCommand(CanExecute = nameof(CanOpenBuildingPictureDirectory))]
    public void OpenBuildingPictureDirectory()
    {
        if (Directory.Exists(_entryDataDirectoryPath))
        {
            try
            {
                Process.Start("explorer.exe", _entryDataDirectoryPath);
            }
            catch (Exception ex)
            {
                // TODO: show error to user.
                Debug.WriteLine($"Error opening folder: {ex.Message}");
            }
        }
    }
    private bool CanOpenBuildingPictureDirectory()
    {
        if ((BuildingPictures.Count > 0) && Directory.Exists(_entryDataDirectoryPath))
        {
            return true;
        }

        return false;
    }

    #endregion

    #region == Unit related commands ==

    // Add New Modal window command
    [RelayCommand]
    private void AddNewUnit()
    {
        _mainViewModel.Unit.SetEditUnit(new Room(Guid.CreateVersion7().ToString("N")));

        _mainViewModel.GoToUnitShellPageCommand.Execute(this);
    }

    [RelayCommand]
    private void EditSelectedUnit(Room room)
    {
        _mainViewModel.Unit.SetEditUnit(room);


        _mainViewModel.GoToUnitShellPageCommand.Execute(this);

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
        // TODO: 
        /*
        if (_editorWin is null)
        {
            return;
        }

        var railLine = await _dlgService.ShowRailLineSelectDialog(_editorWin);

        if (railLine is not null)
        {
            SelectedRailLine1 = railLine;
        }

        */
    }

    [RelayCommand(CanExecute = nameof(CanShowRailStationSelect1))]
    public async Task ShowRailStationSelect1()
    {
        // TODO:
        /*
        if (_editorWin is null)
        {
            return;
        }

        if (SelectedRailLine1 is null)
        {
            return;
        }

        var railStation = await _dlgService.ShowRailStationSelectDialog(_editorWin, SelectedRailLine1.LineCode);

        if (railStation is not null)
        {
            SelectedRailStation1 = railStation;
        }
        */
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
}
