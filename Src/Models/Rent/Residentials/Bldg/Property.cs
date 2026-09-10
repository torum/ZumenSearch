using System.Collections.ObjectModel;
using System.Globalization;
using ZumenSearch.Models.Base;
using ZumenSearch.Models.Common;

namespace ZumenSearch.Models.Rent.Residentials.Bldg;

#pragma warning disable IDE0079 // Remove unnecessary suppression
#pragma warning disable IDE0290 // Use primary constructor

// 編集用（建物）
public sealed partial class Property : PropertyBase
{
    public string PropertyDataDirectoryPath { get; init; }

    #region == ステータス ==

    // ステータス（保存済みか新規か）
    public EnumPropertyStatus PropertyStatus { get; set; } = EnumPropertyStatus.New;

    #endregion

    #region == 物件に属するリスト == 

    // 物件に属する部屋のリスト
    public ObservableCollection<Models.Rent.Residentials.Room.Listing> Rooms
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

    // DBへの更新時にDBから削除されるべき部屋のIDリスト
    public ObservableCollection<Models.Rent.Residentials.Room.Listing> RoomsToBeDeleted = [];

    // 物件写真（建物）リスト
    public ObservableCollection<Picture> BuildingPictures
    {
        get;
        set
        {
            if (SetProperty(ref field, value))
            {
                IsDirty = true; //?
            }
        }
    } = [];

    // DBへの更新時にDBから削除されるべき物件写真（建物）のIDリスト
    public ObservableCollection<Picture> BuildingPicturesToBeDeleted = [];

    // 図面（建物）リスト
    public ObservableCollection<Pdf> BuildingPdfs
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

    // DBへの更新時にDBから削除されるべき図面のIDリスト
    public ObservableCollection<Pdf> BuildingPdfsToBeDeleted = [];

    #endregion

    #region == 基本 ==

    // 物件種別
    public Kind BuildingKind
    {
        get => field ?? new(EnumKinds.Unspecified);
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
        get;
        set
        {
            if (SetProperty(ref field, value))
            {
                IsDirty = true;
            }
        }
    }



    // 建物構造
    public Structure BuildingStructure
    {
        get => field ?? new(EnumStructures.Unspecified);
        set
        {
            if (SetProperty(ref field, value))
            {
                IsDirty = true;
            }
        }
    }

    // 地上階
    public int AboveGroundFloorCount
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

            /*
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
                IsDirty = true;
            }
            */

            OnPropertyChanged();
        }
    }

    // 地下階
    public int BasementFloorCount
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

            OnPropertyChanged();
        }
        /*
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
                IsDirty = true;
            }

            OnPropertyChanged();
        }
        */
    }

    // 総戸数
    public int TotalUnitCount
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

            OnPropertyChanged();
        }
        /*
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
                IsDirty = true;
            }

            OnPropertyChanged();
        }
        */
    }

    // 築年月
    public DateTimeOffset BuiltYearAndMonth
    {
        get;
        set
        {
            if (SetProperty(ref field, value))
            {
                IsDirty = true;
                OnPropertyChanged();
            }
        }
    } = new DateTimeOffset(1900, 1, 1, 0, 0, 0, TimeSpan.Zero);

    // 不動産ID (13桁)
    public string FudousanId
    {
        get => field ?? string.Empty;
        set
        {
            // TODO: check 13桁.

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
            // TODO: check ４桁.

            if (SetProperty(ref field, value.Trim()))
            {
                IsDirty = true;
            }
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

    #region == 所在地 ==

    public string MachiazaId = string.Empty;

    public Prefecture? Pref
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

    public CountyAndCity? City
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

    public WardAndOaza? Town
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

    public Choume? Chou
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

    public string Edaban
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

    // TODO:
    public string PostalCode
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

    // 緯度（Lat）
    public string LocationLatitude
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

    // 経度（Lon）
    public string LocationLongitude
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

    #region == 交通 ==

    public RailLine? RailLine1
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

    public RailStation? RailStation1
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

    // TODO: more.

    #endregion

    #region == 設備 ==

    #region == 一般 ==

    public bool HasAutolock
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

    public bool HasElevator
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

    public bool HasSecurityCamera
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

    public bool HasParcelLocker
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

    #endregion

    #region == 電気 ==

    public enum EnumElectricKind
    {
        Unspecified, AllElectric,
    }

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

    public EnumElectricKind ElectricKind
    {
        get;
        set
        {
            if (SetProperty(ref field, value))
            {
                IsDirty = true;
            }
        }
    } = EnumElectricKind.Unspecified;

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

    #region == ガス ==

    /*
    public bool Ap_HasGas
    {
        get => _hasGas;
        set => SetProperty(ref _hasGas, value);
    }

    public string Kind
    {
        get => _kind;
        set => SetProperty(ref _kind, value);
    }

    public string Detail
    {
        get => _detail;
        set => SetProperty(ref _detail, value);
    }
    */

    #endregion

    #region == 水道 ==

    /*
    public string TapWaterKind
    {
        get => _tapWaterKind;
        set => SetProperty(ref _tapWaterKind, value);
    }

    public string SewerageKind
    {
        get => _sewerageKind;
        set => SetProperty(ref _sewerageKind, value);
    }

    public string TapWaterDetail
    {
        get => _tapWaterDetail;
        set => SetProperty(ref _tapWaterDetail, value);
    }

    public string SewerageDetail
    {
        get => _sewerageDetail;
        set => SetProperty(ref _sewerageDetail, value);
    }
    */

    #endregion

    #region == 共聴設備 ==

    /*
    public bool GroundDigital
    {
        get => _groundDigital;
        set => SetProperty(ref _groundDigital, value);
    }

    public bool BsAntenna
    {
        get => _bsAntenna;
        set => SetProperty(ref _bsAntenna, value);
    }

    public bool CsAntenna
    {
        get => _csAntenna;
        set => SetProperty(ref _csAntenna, value);
    }

    public bool Catv
    {
        get => _catv;
        set => SetProperty(ref _catv, value);
    }

    public bool Usen
    {
        get => _usen;
        set => SetProperty(ref _usen, value);
    }

    public bool Internet
    {
        get => _internet;
        set => SetProperty(ref _internet, value);
    }

    public bool Fiber
    {
        get => _fiber;
        set => SetProperty(ref _fiber, value);
    }

    public bool InternetFree
    {
        get => _internetFree;
        set => SetProperty(ref _internetFree, value);
    }

    public string TvAntennaDetail
    {
        get => _tvAntennaDetail;
        set => SetProperty(ref _tvAntennaDetail, value);
    }

    public string InternetDetail
    {
        get => _internetDetail;
        set => SetProperty(ref _internetDetail, value);
    }
    */

    #endregion

    #region == 駐車場 ==

    /*
    public bool Available
    {
        get => _available;
        set => SetProperty(ref _available, value);
    }

    public bool IsFree
    {
        get => _isFree;
        set => SetProperty(ref _isFree, value);
    }

    public string ContractType
    {
        get => _contractType;
        set => SetProperty(ref _contractType, value);
    }

    public string Fee
    {
        get => _fee;
        set => SetProperty(ref _fee, value);
    }

    public string FeeTaxType
    {
        get => _feeTaxType;
        set => SetProperty(ref _feeTaxType, value);
    }

    public string Availability
    {
        get => _availability;
        set => SetProperty(ref _availability, value);
    }

    public string Spaces
    {
        get => _spaces;
        set => SetProperty(ref _spaces, value);
    }

    public string Distance
    {
        get => _distance;
        set => SetProperty(ref _distance, value);
    }

    public string Notes
    {
        get => _notes;
        set => SetProperty(ref _notes, value);
    }
    */

    #endregion

    #region == 駐輪場 ==



    #endregion

    #region == バイク置き場 ==



    #endregion

    #endregion

    #region == 管理 ==

    // KanriShutai：建物管理主体
    public enum EnumKanriShutai
    {
        Unspecified, Jisya, Tasya, Kashinushi
    }

    // TODO: more.

    #endregion

    #region == 写真 ==



    #endregion

    #region == 図面 ==



    #endregion

    #region == 貸主 ==



    #endregion

    #region == 宅建業者 ==



    #endregion

    // TODO: More.


    public Property(string id, EnumPropertyStatus status) : base(id)
    {
        PropertyStatus = status;

        PropertyDataDirectoryPath = System.IO.Path.Combine(System.IO.Path.Combine(System.IO.Path.Combine(App.AppDataPictureFolder, "Rent"), "Residential_Building"), id);
    }

    public void SetKindTypeFromString(string Str)
    {
        if (string.IsNullOrEmpty(Str))
        {
            BuildingKind = new Kind(EnumKinds.Unspecified);
            return;
        }

        if (Enum.TryParse<EnumKinds>(Str, out var result))
        {
            BuildingKind = new Kind(result);
        }
    }

    public void SetStructureTypeFromString(string Str)
    {
        if (string.IsNullOrEmpty(Str))
        {
            BuildingStructure = new Structure(EnumStructures.Unspecified);
            return;
        }

        if (Enum.TryParse<EnumStructures>(Str, out var result))
        {
            BuildingStructure = new Structure(result);
        }
    }

    public void SetBuildYearMonthFromString(string str)
    {
        if (string.IsNullOrEmpty(str))
        {
            BuiltYearAndMonth = new DateTimeOffset(1900, 1, 1, 0, 0, 0, TimeSpan.Zero); ;
            return;
        }
        
        BuiltYearAndMonth = DateTimeOffset.Parse(str, CultureInfo.InvariantCulture);
    }
}
