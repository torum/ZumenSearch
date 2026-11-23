using System.Globalization;
using ZumenSearch.Domain.Entities.Base;
using ZumenSearch.Domain.Enums;

namespace ZumenSearch.Domain.Entities.Rent.Commercials;

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

    public PropertyKindType CommercialKind
    {
        get;
        private set
        {
            field = value;
            IsModified = true;
        }
    } = PropertyKindType.Unspecified;

    public bool IsUnitOwnership
    {
        get;
        private set
        {
            field = value;
            IsModified = true;
        }
    }

    public StructureType PropertyStructure
    {
        get;
        private set
        {
            field = value;
            IsModified = true;
        }
    } = StructureType.Unspecified;

    public int FloorCountAboveGround
    {
        get;
        private set
        {
            if (value < 0)
            {
                throw new ArgumentOutOfRangeException(nameof(value), value, "Must be at least 0.");
            }

            if (field == value)
            {
                return;
            }

            field = value;
            IsModified = true;
        }
    }

    public int FloorCountBasement
    {
        get;
        private set
        {
            if (value < 0)
            {
                throw new ArgumentOutOfRangeException(nameof(value), value, "Must be at least 0.");
            }

            if (field == value)
            {
                return;
            }

            field = value;
            IsModified = true;
        }
    }

    public decimal TotalFloorArea
    {
        get;
        private set
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
        private set
        {
            field = value;
            IsModified = true;
        }
    } = new(1900, 1, 1, 0, 0, 0, TimeSpan.Zero);

    public string FudousanId
    {
        get => field ?? string.Empty;
        private set
        {
            field = value;
            IsModified = true;
        }
    }

    public string FudousanIdAdditionalCode
    {
        get => field ?? string.Empty;
        private set
        {
            field = value;
            IsModified = true;
        }
    } = "0000";

    public string Remarks
    {
        get => field ?? string.Empty;
        private set
        {
            field = value;
            IsModified = true;
        }
    }

    #endregion

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

    public bool HasSecurityCamera
    {
        get;
        set
        {
            field = value;
            IsModified = true;
        }
    }

    #endregion

    public bool HasElectricity { get; set { field = value; IsModified = true; } }
    public string ElectricityDetails { get => field ?? string.Empty; set { field = value; IsModified = true; } }
    public bool HasGas { get; set { field = value; IsModified = true; } }
    public GasSupplyType GasType { get; set { ValidateFacilityEnum(value); field = value; IsModified = true; } }
    public string GasDetails { get => field ?? string.Empty; set { field = value; IsModified = true; } }
    public bool HasWater { get; set { field = value; IsModified = true; } }
    public WaterSupplyType WaterType { get; set { ValidateFacilityEnum(value); field = value; IsModified = true; } }
    public string WaterDetails { get => field ?? string.Empty; set { field = value; IsModified = true; } }
    public SewerageType SewerageType { get; set { ValidateFacilityEnum(value); field = value; IsModified = true; } }
    public string SewerageDetails { get => field ?? string.Empty; set { field = value; IsModified = true; } }
    public bool HasInternet { get; set { field = value; IsModified = true; } }
    public bool HasFiber { get; set { field = value; IsModified = true; } }
    public string InternetDetails { get => field ?? string.Empty; set { field = value; IsModified = true; } }
    public bool HasFireSafety { get; set { field = value; IsModified = true; } }
    public string FireSafetyDetails { get => field ?? string.Empty; set { field = value; IsModified = true; } }
    public bool HasParking { get; set { field = value; IsModified = true; } }
    public ParkingVacancyType ParkingVacancy { get; set { ValidateFacilityEnum(value); field = value; IsModified = true; } }
    public int? ParkingSpaces { get; set { if (value < 0) throw new ArgumentOutOfRangeException(nameof(value)); field = value; IsModified = true; } }
    public decimal? ParkingMonthlyFee { get; set { if (value < 0) throw new ArgumentOutOfRangeException(nameof(value)); field = value; IsModified = true; } }
    public ParkingTaxType ParkingTax { get; set { ValidateFacilityEnum(value); field = value; IsModified = true; } }
    public bool IsParkingFree { get; set { field = value; IsModified = true; } }
    public string ParkingDetails { get => field ?? string.Empty; set { field = value; IsModified = true; } }

    private static void ValidateFacilityEnum<T>(T value) where T : struct, Enum
    {
        if (!Enum.IsDefined(value)) throw new ArgumentOutOfRangeException(nameof(value));
    }

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
        private set
        {
            if (!Enum.IsDefined(value)) throw new ArgumentOutOfRangeException(nameof(value));
            field = value;
            IsModified = true;
        }
    }

    public bool KanriIsMaintenanceManagementIfIsKanriJisya { get; private set { field = value; IsModified = true; } }
    public bool KanriIsTenantManagementIfIsKanriJisya { get; private set { field = value; IsModified = true; } }
    public bool KanriIsPaymentManagementIfIsKanriJisya { get; private set { field = value; IsModified = true; } }
    public bool KanriIsOtherManagementIfIsKanriJisya { get; private set { field = value; IsModified = true; } }
    public string KanriRemarkIfIsKanriJisya { get => field ?? string.Empty; private set { field = value; IsModified = true; } }
    public string KanriNameOfCompanyIfIsKanriTasya { get => field ?? string.Empty; private set { field = value; IsModified = true; } }
    public string KanriContactInfoOfCompanyIfIsKanriTasya { get => field ?? string.Empty; private set { field = value; IsModified = true; } }
    public string KanriRemarkIfIsKanriTasya { get => field ?? string.Empty; private set { field = value; IsModified = true; } }
    public string KanriRemarkIfIsKanriKashinushi { get => field ?? string.Empty; private set { field = value; IsModified = true; } }

    #endregion

    #region == 物件に属するリスト ==

    // 物件に属する未保存の募集物件リスト
    public List<Listing.Listing> ListingsToBeSaved { get; set; } = [];

    // 物件に属する募集物件のサマリーリスト（一覧表示用）
    public List<ListingSummary> ListingSummaries { get; } = [];

    // 物件の保存時にDBから削除する募集物件のIDリスト
    public List<string> ListingsToBeDeleted { get; set; } = [];

    // 物件写真（建物）リスト
    public List<PropertyPicture> Pictures
    {
        get;
        set
        {
            field = value;
            IsModified = true;
        }
    } = [];

    // DBへの更新時にDBから削除されるべき物件写真（建物）のIDリスト
    public List<PropertyPicture> PicturesToBeDeleted { get; set; } = [];

    // 図面（建物）リスト
    public List<PropertyPdf> Pdfs
    {
        get;
        set
        {
            field = value;
            IsModified = true;
        }
    } = [];

    // DBへの更新時にDBから削除されるべき図面のIDリスト
    public List<PropertyPdf> PdfsToBeDeleted { get; set; } = [];

    // 貸主のリスト
    public List<PersonBase> Lessors
    {
        get;
        set
        {
            field = value;
            IsModified = true;
        }
    } = [];

    // DBへの更新時にDBから削除されるべき貸主のIDリスト
    public List<PersonBase> LessorsToBeDeleted { get; set; } = [];

    // 宅建業者のリスト
    public List<PersonBase> Brokers
    {
        get;
        set
        {
            field = value;
            IsModified = true;
        }
    } = [];

    // DBへの更新時にDBから削除されるべき宅建業者のIDリスト
    public List<PersonBase> BrokersToBeDeleted { get; set; } = [];

    #endregion

    #endregion

    #region == Methods ==

    public void SetCommercialKindTypeLabel(PropertyKindType value) => CommercialKind = value;

    public void SetIsUnitOwnership(bool value) => IsUnitOwnership = value;

    public void SetPropertyStructureTypeLabel(StructureType value) => PropertyStructure = value;

    public void SetFloorCountAboveGroundFromString(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            SetFloorCountAboveGround(0);
            return;
        }

        if (!int.TryParse(value.Trim(), NumberStyles.Integer, CultureInfo.InvariantCulture, out var result))
        {
            throw new ArgumentException("地上階の値が不正です。整数に変換出来ませんでした。", nameof(value));
        }

        SetFloorCountAboveGround(result);
    }

    public void SetFloorCountAboveGround(int value)
    {
        if (value < 0)
        {
            throw new ArgumentException("地上階の値が不正です。0以上の整数である必要があります。", nameof(value));
        }

        FloorCountAboveGround = value;
    }

    public void SetFloorCountBasementFromString(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            SetFloorCountBasement(0);
            return;
        }

        if (!int.TryParse(value.Trim(), NumberStyles.Integer, CultureInfo.InvariantCulture, out var result))
        {
            throw new ArgumentException("地下階の値が不正です。整数に変換出来ませんでした。", nameof(value));
        }

        SetFloorCountBasement(result);
    }

    public void SetFloorCountBasement(int value)
    {
        if (value < 0)
        {
            throw new ArgumentException("地下階の値が不正です。0以上の整数である必要があります。", nameof(value));
        }

        FloorCountBasement = value;
    }

    public void SetTotalFloorArea(decimal value) => TotalFloorArea = value;

    public void SetBuiltYearAndMonth(DateTimeOffset? value) => BuiltYearAndMonth = value ?? new DateTimeOffset(1900, 1, 1, 0, 0, 0, TimeSpan.Zero);

    public void SetFudousanId(string value)
    {
        ArgumentNullException.ThrowIfNull(value);
        var normalizedValue = value.Normalize(System.Text.NormalizationForm.FormKC).Trim();
        if (normalizedValue.Length > 0 && !System.Text.RegularExpressions.Regex.IsMatch(normalizedValue, @"\A[0-9]{13}\z"))
        {
            throw new ArgumentException("不動産番号は半角数字13桁で入力してください。", nameof(value));
        }
        FudousanId = normalizedValue;
    }

    public void SetFudousanIdAdditionalCode(string value)
    {
        ArgumentNullException.ThrowIfNull(value);
        var normalizedValue = value.Normalize(System.Text.NormalizationForm.FormKC).Trim().ToUpperInvariant();
        if (normalizedValue.Length > 0 && !System.Text.RegularExpressions.Regex.IsMatch(normalizedValue, @"\A[A-Z0-9]{4}\z"))
        {
            throw new ArgumentException("特定コードは半角英数字4桁（英字は大文字）で入力してください。", nameof(value));
        }
        FudousanIdAdditionalCode = normalizedValue;
    }

    public void SetRemarks(string value)
    {
        Remarks = value;
    }

    public void SetKanriShutai(EnumKanriShutai value)
    {
        if (!Enum.IsDefined(value))
        {
            throw new ArgumentOutOfRangeException(nameof(value), value, "Invalid management type.");
        }

        KanriShutai = value;
    }

    public void SetKanriShutaiFromString(string value)
    {
        if (!Enum.TryParse<EnumKanriShutai>(value, out var result) || !Enum.IsDefined(result) || !string.Equals(value, result.ToString(), StringComparison.Ordinal))
        {
            throw new FormatException($"Invalid management type: {value}");
        }

        SetKanriShutai(result);
    }

    public void SetKanriIsMaintenanceManagementIfIsKanriJisya(bool value) => KanriIsMaintenanceManagementIfIsKanriJisya = value;
    public void SetKanriIsTenantManagementIfIsKanriJisya(bool value) => KanriIsTenantManagementIfIsKanriJisya = value;
    public void SetKanriIsPaymentManagementIfIsKanriJisya(bool value) => KanriIsPaymentManagementIfIsKanriJisya = value;
    public void SetKanriIsOtherManagementIfIsKanriJisya(bool value) => KanriIsOtherManagementIfIsKanriJisya = value;
    public void SetKanriRemarkIfIsKanriJisya(string value) => KanriRemarkIfIsKanriJisya = value;
    public void SetKanriNameOfCompanyIfIsKanriTasya(string value) => KanriNameOfCompanyIfIsKanriTasya = value;
    public void SetKanriContactInfoOfCompanyIfIsKanriTasya(string value) => KanriContactInfoOfCompanyIfIsKanriTasya = value;
    public void SetKanriRemarkIfIsKanriTasya(string value) => KanriRemarkIfIsKanriTasya = value;
    public void SetKanriRemarkIfIsKanriKashinushi(string value) => KanriRemarkIfIsKanriKashinushi = value;

    public void SetCommercialKindTypeLabelFromString(string value)
    {
        SetCommercialKindTypeLabel(Enum.TryParse(value, out PropertyKindType result) ? result : PropertyKindType.Unspecified);
    }

    public void SetPropertyStructureTypeLabelFromString(string value)
    {
        SetPropertyStructureTypeLabel(Enum.TryParse(value, out StructureType result) ? result : StructureType.Unspecified);
    }

    public void SetBuiltYearAndMonthFromString(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            SetBuiltYearAndMonth(null);
            return;
        }

        if (!DateTimeOffset.TryParse(value.Trim(), CultureInfo.InvariantCulture, DateTimeStyles.None, out var parsedValue))
        {
            throw new ArgumentException("築年月の値が不正です。有効な日付に変換出来ませんでした。", nameof(value));
        }

        SetBuiltYearAndMonth(parsedValue);
    }

    public void SetPictures(List<PropertyPicture> value) => Pictures = value;
    public void SetPicturesToBeDeleted(List<PropertyPicture> value) => PicturesToBeDeleted = value;
    public void SetPdfs(List<PropertyPdf> value) => Pdfs = value;
    public void SetPdfsToBeDeleted(List<PropertyPdf> value) => PdfsToBeDeleted = value;
    public void SetLessors(List<PersonBase> value) => Lessors = value;
    public void SetLessorsToBeDeleted(List<PersonBase> value) => LessorsToBeDeleted = value;
    public void SetBrokers(List<PersonBase> value) => Brokers = value;
    public void SetBrokersToBeDeleted(List<PersonBase> value) => BrokersToBeDeleted = value;

    #endregion

}
