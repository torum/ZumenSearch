using System.Collections.ObjectModel;
using System.Globalization;
using ZumenSearch.Models.Base;
using ZumenSearch.Models.Location;
using ZumenSearch.Models.Transportation;

namespace ZumenSearch.Models.Rent.Commercials;

#pragma warning disable IDE0079 // Remove unnecessary suppression
#pragma warning disable IDE0290 // Use primary constructor

// Aggregate Root entity.

[System.Diagnostics.CodeAnalysis.SuppressMessage("Naming", "CA1716:Identifiers should not match keywords", Justification = "Required for clarity. And I don't care about Visual Basic Keywords.")]
public sealed partial class Property : PropertyBase
{
    public Property(string id, EntityStatus status) : base(id, status, PropertyContextType.RentCommercial)
    {
    }

    #region == Property ==

    #region == 基本 ==

    public PropertyTypeLabel CommercialKind
    {
        get => field ?? new(PropertyKindType.Unspecified);
        set
        {
            field = value;
            IsModified = true;
        }
    }

    public bool IsUnitOwnership
    {
        get;
        set
        {
            field = value;
            IsModified = true;
        }
    }

    public PropertyStructureTypeLabel PropertyStructure
    {
        get => field ?? new(StructureType.Unspecified);
        set
        {
            field = value;
            IsModified = true;
        }
    }

    public int FloorCountAboveGround
    {
        get;
        set
        {
            if (value < 0)
            {
                throw new ArgumentOutOfRangeException(nameof(value), value, "Must be at least 0.");
            }

            field = value;
            IsModified = true;
        }
    }

    public int FloorCountBasement
    {
        get;
        set
        {
            if (value < 0)
            {
                throw new ArgumentOutOfRangeException(nameof(value), value, "Must be at least 0.");
            }

            field = value;
            IsModified = true;
        }
    }

    public decimal TotalFloorArea
    {
        get;
        set
        {
            if (value < 0)
            {
                throw new ArgumentOutOfRangeException(nameof(value), value, "Must be at least 0.");
            }

            field = value;
            IsModified = true;
        }
    }

    public DateTimeOffset BuiltYearAndMonth
    {
        get;
        set
        {
            field = value;
            IsModified = true;
        }
    } = new(1900, 1, 1, 0, 0, 0, TimeSpan.Zero);

    public string FudousanId
    {
        get => field ?? string.Empty;
        set
        {
            field = value;
            IsModified = true;
        }
    }

    public string FudousanIdAdditionalCode
    {
        get => field ?? string.Empty;
        set
        {
            field = value;
            IsModified = true;
        }
    }

    public string Remarks
    {
        get => field ?? string.Empty;
        set
        {
            field = value;
            IsModified = true;
        }
    }

    #endregion

    #region == Location ==

    /*
    public string MachiazaId { get; set; } = string.Empty;

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
    */
    /*
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
    */

    #endregion
    /*
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
    */

    #region == Facilities ==

    #region == 一般 ==

    public bool HasPassengerElevator
    {
        get;
        set { field = value; IsModified = true; }
    }

    public int? PassengerElevatorCount
    {
        get;
        set
        {
            if (value < 0) throw new ArgumentOutOfRangeException(nameof(value));
            field = value;
            IsModified = true;
        }
    }

    public bool HasFreightElevator
    {
        get;
        set { field = value; IsModified = true; }
    }

    public string FreightElevatorDetails
    {
        get => field ?? string.Empty;
        set { field = value; IsModified = true; }
    }

    public bool HasLoadingArea
    {
        get;
        set { field = value; IsModified = true; }
    }

    public string LoadingAreaDetails
    {
        get => field ?? string.Empty;
        set { field = value; IsModified = true; }
    }

    public bool HasSecurity
    {
        get;
        set { field = value; IsModified = true; }
    }

    public bool HasMechanicalSecurity
    {
        get;
        set { field = value; IsModified = true; }
    }

    public bool HasStaffedSecurity
    {
        get;
        set { field = value; IsModified = true; }
    }

    public bool HasOtherSecurity
    {
        get;
        set { field = value; IsModified = true; }
    }

    public string SecurityDetails
    {
        get => field ?? string.Empty;
        set { field = value; IsModified = true; }
    }

    public bool HasAutolock
    {
        get;
        set
        {
            field = value;
            IsModified = true;
        }
    }

    public bool HasElevator
    {
        get;
        set
        {
            field = value;
            IsModified = true;
        }
    }

    public bool HasSecurityCamera
    {
        get;
        set
        {
            field = value;
            IsModified = true;
        }
    }

    public bool HasParcelLocker
    {
        get;
        set
        {
            field = value;
            IsModified = true;
        }
    }

    #endregion

    #region == 電気 ==

    public enum EnumElectricType
    {
        Unspecified, AllElectric,
    }

    public bool HasElectric
    {
        get;
        set
        {
            field = value;
            IsModified = true;
        }
    }

    public EnumElectricType PropertyElectricKind
    {
        get;
        set
        {
            field = value;
            IsModified = true;
        }
    } = EnumElectricType.Unspecified;

    public string ElectricDetail
    {
        get;
        set
        {
            field = value;
            IsModified = true;
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

    public EnumKanriShutai KanriShutai
    {
        get;
        set
        {
            if (!Enum.IsDefined(value)) throw new ArgumentOutOfRangeException(nameof(value));
            field = value;
            IsModified = true;
        }
    }

    public bool KanriIsMaintenanceManagementIfIsKanriJisya { get; set { field = value; IsModified = true; } }
    public bool KanriIsTenantManagementIfIsKanriJisya { get; set { field = value; IsModified = true; } }
    public bool KanriIsPaymentManagementIfIsKanriJisya { get; set { field = value; IsModified = true; } }
    public bool KanriIsOtherManagementIfIsKanriJisya { get; set { field = value; IsModified = true; } }
    public string KanriRemarkIfIsKanriJisya { get => field ?? string.Empty; set { field = value; IsModified = true; } }
    public string KanriNameOfCompanyIfIsKanriTasya { get => field ?? string.Empty; set { field = value; IsModified = true; } }
    public string KanriContactInfoOfCompanyIfIsKanriTasya { get => field ?? string.Empty; set { field = value; IsModified = true; } }
    public string KanriRemarkIfIsKanriTasya { get => field ?? string.Empty; set { field = value; IsModified = true; } }
    public string KanriRemarkIfIsKanriKashinushi { get => field ?? string.Empty; set { field = value; IsModified = true; } }

    #endregion

    #region == 写真 & 図面 ==

    public ObservableCollection<PropertyPicture> Pictures
    {
        get;
        set
        {
            field = value;
            IsModified = true;
        }
    } = [];

    public ObservableCollection<PropertyPicture> PicturesToBeDeleted { get; set; } = [];


    // 図面（建物）リスト
    public ObservableCollection<PropertyPdf> Pdfs
    {
        get;
        set
        {
            field = value;
            IsModified = true;
        }
    } = [];

    // DBへの更新時にDBから削除されるべき図面のIDリスト
    public ObservableCollection<PropertyPdf> PdfsToBeDeleted { get; set; } = [];
    #endregion

    #region == Lessors ==

    public ObservableCollection<PersonBase> Lessors
    {
        get;
        set
        {
            field = value;
            IsModified = true;
        }
    } = [];

    public ObservableCollection<PersonBase> LessorsToBeDeleted { get; set; } = [];

    #endregion

    #region == Brokers ==

    public ObservableCollection<PersonBase> Brokers
    {
        get;
        set
        {
            field = value;
            IsModified = true;
        }
    } = [];

    public ObservableCollection<PersonBase> BrokersToBeDeleted { get; set; } = [];

    #endregion

    #region == Units ==

    public ObservableCollection<string> ListingIds
    {
        get;
        set
        {
            field = value;
            IsModified = true;
        }
    } = [];

    #endregion

    #endregion

    #region == Methods ==

    public void SetCommercialKindFromString(string value)
    {
        CommercialKind = Enum.TryParse(
            value,
            out PropertyKindType result)
            ? new PropertyTypeLabel(result)
            : new PropertyTypeLabel(PropertyKindType.Unspecified);
    }

    public void SetStructureTypeFromString(string value)
    {
        PropertyStructure = Enum.TryParse(
            value,
            out StructureType result)
            ? new PropertyStructureTypeLabel(result)
            : new PropertyStructureTypeLabel(StructureType.Unspecified);
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