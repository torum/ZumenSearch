using System.Collections.ObjectModel;
using System.Globalization;
using ZumenSearch.Models.Base;
using ZumenSearch.Models.Location;
using ZumenSearch.Models.Transportation;

namespace ZumenSearch.Models.Rent.Commercials;

#pragma warning disable IDE0079 // Remove unnecessary suppression
#pragma warning disable IDE0290 // Use primary constructor

// Aggregate Root entity.

public sealed partial class Property : PropertyBase
{
    public Property(string id, EnumEntityStatus status) : base(id, status, EnumPropertyKind.RentCommercial)
    {
    }

    #region == Property ==

    #region == 基本 ==

    public PropertyKind CommercialKind
    {
        get => field ?? new(EnumCommercialKinds.Unspecified);
        set
        {
            if (SetProperty(ref field, value))
            {
                IsModified = true;
            }
        }
    }

    public bool IsUnitOwnership
    {
        get;
        set
        {
            if (SetProperty(ref field, value))
            {
                IsModified = true;
            }
        }
    }

    public PropertyStructure BuildingStructure
    {
        get => field ?? new(EnumStructures.Unspecified);
        set
        {
            if (SetProperty(ref field, value))
            {
                IsModified = true;
            }
        }
    }

    public int FloorCountAboveGround
    {
        get;
        set
        {
            if (SetProperty(ref field, value))
            {
                IsModified = true;
            }
        }
    }

    public int FloorCountBasement
    {
        get;
        set
        {
            if (SetProperty(ref field, value))
            {
                IsModified = true;
            }
        }
    }

    public decimal TotalFloorArea
    {
        get;
        set
        {
            if (SetProperty(ref field, value))
            {
                IsModified = true;
            }
        }
    }

    public DateTimeOffset BuiltYearAndMonth
    {
        get;
        set
        {
            if (SetProperty(ref field, value))
            {
                IsModified = true;
            }
        }
    } = new(1900, 1, 1, 0, 0, 0, TimeSpan.Zero);

    public string FudousanId
    {
        get => field ?? string.Empty;
        set
        {
            if (SetProperty(ref field, value))
            {
                IsModified = true;
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
                IsModified = true;
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
                IsModified = true;
            }
        }
    }

    #endregion

    #region == Location ==


    public string MachiazaId = string.Empty;

    public Prefecture? Pref
    {
        get;
        set
        {
            if (SetProperty(ref field, value))
            {
                IsModified = true;
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
                IsModified = true;
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
                IsModified = true;
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
                IsModified = true;
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
                IsModified = true;
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
                IsModified = true;
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

    #region == Transportation ==

    public RailLine? RailLine1
    {
        get;
        set
        {
            if (SetProperty(ref field, value))
            {
                IsModified = true;
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
                IsModified = true;
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
                IsModified = true;
            }
        }
    }

    public string BusStop1
    {
        get => field ?? string.Empty;
        set
        {
            if (SetProperty(ref field, value))
            {
                IsModified = true;
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
                IsModified = true;
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
                IsModified = true;
            }
        }
    }

    #endregion

    #region == Facilities ==

    #region == 一般 ==

    public bool HasAutolock
    {
        get;
        set
        {
            if (SetProperty(ref field, value))
            {
                IsModified = true;
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
                IsModified = true;
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
                IsModified = true;
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
                IsModified = true;
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
                IsModified = true;
            }
        }
    } = false;

    public EnumElectricKind PropertyElectricKind
    {
        get;
        set
        {
            if (SetProperty(ref field, value))
            {
                IsModified = true;
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
                IsModified = true;
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

    #endregion

    #region == 管理 ==

    // KanriShutai：建物管理主体
    public enum EnumKanriShutai
    {
        Unspecified, Jisya, Tasya, Kashinushi
    }

    // TODO: more.

    #endregion

    #region == 写真 & 図面 ==

    public ObservableCollection<PropertyPicture> Pictures
    {
        get;
        set
        {
            if (SetProperty(ref field, value))
            {
                IsModified = true;
            }
        }
    } = [];

    public ObservableCollection<PropertyPicture> PicturesToBeDeleted = [];


    // 図面（建物）リスト
    public ObservableCollection<PropertyPdf> Pdfs
    {
        get;
        set
        {
            if (SetProperty(ref field, value))
            {
                IsModified = true;
            }
        }
    } = [];

    // DBへの更新時にDBから削除されるべき図面のIDリスト
    public ObservableCollection<PropertyPdf> PdfsToBeDeleted = [];
    #endregion

    #region == Lessors ==

    public ObservableCollection<PersonBase> Lessors
    {
        get;
        set
        {
            if (SetProperty(ref field, value))
            {
                IsModified = true;
            }
        }
    } = [];

    public ObservableCollection<PersonBase> LessorsToBeDeleted = [];

    #endregion

    #region == Brokers ==

    public ObservableCollection<PersonBase> Brokers
    {
        get;
        set
        {
            if (SetProperty(ref field, value))
            {
                IsModified = true;
            }
        }
    } = [];

    public ObservableCollection<PersonBase> BrokersToBeDeleted = [];

    #endregion

    #region == Units ==

    // 物件に属する区画のリスト
    public ObservableCollection<Models.Rent.Commercials.Listing.Listing> Units
    {
        get;
        set
        {
            if (SetProperty(ref field, value))
            {
                IsModified = true;
            }
        }
    } = [];

    // DBへの更新時にDBから削除されるべき区画のIDリスト

    public ObservableCollection<Models.Rent.Commercials.Listing.Listing> UnitsToBeDeleted = [];

    #endregion

    #endregion

    #region == Methods ==

    public void SetCommercialKindFromString(string value)
    {
        CommercialKind = Enum.TryParse(
            value,
            out EnumCommercialKinds result)
            ? new PropertyKind(result)
            : new PropertyKind(EnumCommercialKinds.Unspecified);
    }

    public void SetStructureTypeFromString(string value)
    {
        BuildingStructure = Enum.TryParse(
            value,
            out EnumStructures result)
            ? new PropertyStructure(result)
            : new PropertyStructure(EnumStructures.Unspecified);
    }

    public void SetBuildYearMonthFromString(string value)
    {
        BuiltYearAndMonth = string.IsNullOrWhiteSpace(value)
            ? new DateTimeOffset(
                1900,
                1,
                1,
                0,
                0,
                0,
                TimeSpan.Zero)
            : DateTimeOffset.Parse(
                value,
                CultureInfo.InvariantCulture);
    }

    #endregion

}