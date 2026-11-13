using System.Collections.ObjectModel;
using System.Globalization;
using System.Text.RegularExpressions;
using ZumenSearch.Models.Base;

#pragma warning disable IDE0079 // Remove unnecessary suppression
#pragma warning disable IDE0290 // Use primary constructor

namespace ZumenSearch.Models.Rent.Residentials;

// Aggregate Root entity.

[System.Diagnostics.CodeAnalysis.SuppressMessage("Naming", "CA1716:Identifiers should not match keywords", Justification = "Required for clarity. And I don't care about Visual Basic Keywords.")]
public sealed partial class Property : PropertyBase
{
    public Property(string id, EntityStatus status) : base(id, status, PropertyContextType.RentResidential)
    {
        //
    }

    #region == Properties ==

    #region == 建物の基本情報 ==

    // 物件種別
    public PropertyKindTypeLabel PropertyKind
    {
        get => field ?? new(PropertyKindType.Unspecified);
        private set
        {
            field = value;
            IsModified = true;
        }
    }

    // 区分所有か一括所有か
    public bool IsUnitOwnership
    {
        get;
        private set
        {
            field = value;
            IsModified = true;
        }
    }

    internal string ThumbnailFilenameOnLastSave { get; set; } = string.Empty;

    // 建物構造
    public PropertyStructureTypeLabel PropertyStructure
    {
        get => field ?? new(StructureType.Unspecified);
        private set
        {
            field = value;
            IsModified = true;
        }
    }

    // 地上階
    public int FloorCountAboveGround
    {
        get;
        private set
        {
            if (field == value)
            {
                return;
            }

            field = value;
            IsModified = true;
        }
    }

    // 地下階
    public int FloorCountBasement
    {
        get;
        private set
        {
            if (field == value)
            {
                return;
            }

            field = value;
            IsModified = true;
        }
    }

    // 総戸数
    public int TotalUnitCount
    {
        get;
        private set
        {
            if (field == value)
            {
                return;
            }

            field = value;
            IsModified = true;
        }
    }

    // 築年月
    public DateTimeOffset BuiltYearAndMonth
    {
        get;
        private set
        {

            field = value;
            IsModified = true;
        }
    } = new DateTimeOffset(1900, 1, 1, 0, 0, 0, TimeSpan.Zero);

    // 不動産ID (13桁)
    public string FudousanId
    {
        get => field ?? string.Empty;
        private set
        {
            field = value;
            IsModified = true;
        }
    }

    // 特定コード（４桁）建物全体は0000
    public string FudousanIdAdditionalCode
    {
        get => field ?? string.Empty; // keep non-null empty string.
        private set
        {
            field = value;
            IsModified = true;
        }
    } = "0000";

    // 備考
    public string Remarks
    {
        get;
        private set
        {
            field = value;
            IsModified = true;
        }
    } = string.Empty;

    #endregion

    #region == 設備 ==

    #region == 一般 ==

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

    #endregion

    #region == 管理 ==

    public enum EnumKanriShutai
    {
        Unspecified, Jisya, Tasya, Kashinushi
    }

    public EnumKanriShutai KanriShutai
    {
        get;
        private set { field = value; IsModified = true; }
    } = EnumKanriShutai.Unspecified;

    public bool KanriIsMaintenanceManagementIfIsKanriJisya { get; private set { field = value; IsModified = true; } }
    public bool KanriIsTenantManagementIfIsKanriJisya { get; private set { field = value; IsModified = true; } }
    public bool KanriIsPaymentManagementIfIsKanriJisya { get; private set { field = value; IsModified = true; } }
    public bool KanriIsOtherManagementIfIsKanriJisya { get; private set { field = value; IsModified = true; } }
    public string KanriRemarkIfIsKanriJisya { get => field ?? string.Empty; private set { field = value; IsModified = true; } } = string.Empty;
    public string KanriNameOfCompanyIfIsKanriTasya { get => field ?? string.Empty; private set { field = value; IsModified = true; } } = string.Empty;
    public string KanriContactInfoOfCompanyIfIsKanriTasya { get => field ?? string.Empty; private set { field = value; IsModified = true; } } = string.Empty;
    public string KanriRemarkIfIsKanriTasya { get => field ?? string.Empty; private set { field = value; IsModified = true; } } = string.Empty;
    public string KanriRemarkIfIsKanriKashinushi { get => field ?? string.Empty; private set { field = value; IsModified = true; } } = string.Empty;

    #endregion

    #region == 物件に属するリスト == 

    public ObservableCollection<string> ListingIds { get; } = [];

    // 物件写真（建物）リスト
    public ObservableCollection<PropertyPicture> Pictures
    {
        get;
        set
        {
            field = value;
            IsModified = true;
        }
    } = [];

    // DBへの更新時にDBから削除されるべき物件写真（建物）のIDリスト
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

    // 貸主のリスト
    public ObservableCollection<Models.Base.PersonBase> Lessors
    {
        get;
        set
        {
            field = value;
            IsModified = true;
        }
    } = [];

    // DBへの更新時にDBから削除されるべき貸主のIDリスト
    public ObservableCollection<Models.Base.PersonBase> LessorsToBeDeleted { get; set; } = [];


    // 宅建業者のリスト
    public ObservableCollection<Models.Base.PersonBase> Brokers
    {
        get;
        set
        {
            field = value;
            IsModified = true;
        }
    } = [];

    public ObservableCollection<Models.Base.PersonBase> BrokersToBeDeleted { get; set; } = [];

    #endregion

    #endregion

    #region == Methods ==

    public void SetBuildingType(PropertyKindTypeLabel value) => PropertyKind = value;

    public void SetPropertyTypeFromString(string Str)
    {
        if (string.IsNullOrEmpty(Str))
        {
            PropertyKind = new PropertyKindTypeLabel(PropertyKindType.Unspecified);
            return;
        }

        if (Enum.TryParse<PropertyKindType>(Str, out var result))
        {
            PropertyKind = new PropertyKindTypeLabel(result);
        }
    }

    public void SetIsUnitOwnership(bool value) => IsUnitOwnership = value;

    public void SetBuildingStructure(PropertyStructureTypeLabel value) => PropertyStructure = value;

    public void SetStructureTypeFromString(string Str)
    {
        if (string.IsNullOrEmpty(Str))
        {
            PropertyStructure = new PropertyStructureTypeLabel(StructureType.Unspecified);
            return;
        }

        if (Enum.TryParse<StructureType>(Str, out var result))
        {
            PropertyStructure = new PropertyStructureTypeLabel(result);
        }
    }

    public void SetFloorCountAboveGroundFromString(string Str)
    {
        if (string.IsNullOrWhiteSpace(Str))
        {
            SetFloorCountAboveGround(0);
            return;
        }

        if (!int.TryParse(Str.Trim(), NumberStyles.Integer, CultureInfo.InvariantCulture, out var result))
        {
            throw new ArgumentException("地上階の値が不正です。整数に変換出来ませんでした。", nameof(Str));
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

    public void SetTotalUnitCount(int value)
    {
        if (value < 0)
        {
            throw new ArgumentException("総戸数の値が不正です。0以上の整数である必要があります。", nameof(value));
        }

        TotalUnitCount = value;
    }

    public void SetTotalUnitCountFromString(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            SetTotalUnitCount(0);
            return;
        }

        if (!int.TryParse(value.Trim(), NumberStyles.Integer, CultureInfo.InvariantCulture, out var result))
        {
            throw new ArgumentException("総戸数の値が不正です。整数に変換出来ませんでした。", nameof(value));
        }

        SetTotalUnitCount(result);
    }
    
    public void SetBuiltYearAndMonth(DateTimeOffset? value)
    {
        BuiltYearAndMonth = value ?? new DateTimeOffset(1900, 1, 1, 0, 0, 0, TimeSpan.Zero);
    }

    public void SetBuiltYearAndMonthFromString(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            BuiltYearAndMonth = new DateTimeOffset(1900, 1, 1, 0, 0, 0, TimeSpan.Zero);
            return;
        }

        if (!DateTimeOffset.TryParse(value.Trim(),CultureInfo.InvariantCulture,DateTimeStyles.None,out var parsedValue))
        {
            throw new ArgumentException("築年月の値が不正です。有効な日付に変換出来ませんでした。", nameof(value));
        }

        BuiltYearAndMonth = parsedValue;
    }

    public void SetFudousanId(string value)
    {
        ArgumentNullException.ThrowIfNull(value);

        var normalizedValue = Helpers.Common.NormalizeFullWidthCharacters(value).Trim();
        ValidateFudousanId(normalizedValue);
        FudousanId = normalizedValue;
    }

    private static void ValidateFudousanId(string value)
    {
        if (value.Length > 0 && !Regex.IsMatch(value, @"\A[0-9]{13}\z"))
        {
            throw new ArgumentException("不動産番号は半角数字13桁で入力してください。", nameof(value));
        }
    }

    public void SetFudousanIdAdditionalCode(string value)
    {
        ArgumentNullException.ThrowIfNull(value);

        var normalizedValue = Helpers.Common.NormalizeFullWidthCharacters(value).Trim().ToUpperInvariant();
        ValidateFudousanIdAdditionalCode(normalizedValue);
        FudousanIdAdditionalCode = normalizedValue;
    }

    private static void ValidateFudousanIdAdditionalCode(string value)
    {
        if (value.Length > 0 && !Regex.IsMatch(value, @"\A[A-Z0-9]{4}\z"))
        {
            throw new ArgumentException("特定コードは半角英数字4桁（英字は大文字）で入力してください。", nameof(value));
        }
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

    public void SetPropertyElectricKind(EnumElectricType value) => PropertyElectricKind = value;
    public void SetElectricDetail(string value) => ElectricDetail = value;

    public void SetPictures(ObservableCollection<PropertyPicture> value) => Pictures = value;
    public void SetPicturesToBeDeleted(ObservableCollection<PropertyPicture> value) => PicturesToBeDeleted = value;
    public void SetPdfs(ObservableCollection<PropertyPdf> value) => Pdfs = value;
    public void SetPdfsToBeDeleted(ObservableCollection<PropertyPdf> value) => PdfsToBeDeleted = value;
    public void SetLessors(ObservableCollection<PersonBase> value) => Lessors = value;
    public void SetLessorsToBeDeleted(ObservableCollection<PersonBase> value) => LessorsToBeDeleted = value;
    public void SetBrokers(ObservableCollection<PersonBase> value) => Brokers = value;
    public void SetBrokersToBeDeleted(ObservableCollection<PersonBase> value) => BrokersToBeDeleted = value;

    #endregion

}
