using System.Collections.ObjectModel;
using System.Globalization;
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
        private set
        {
            if (field == value)
            {
                return;
            }

            field = value;
            IsModified = true;

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
            // TODO: check 13桁.

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
            // TODO: check ４桁.


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


    #region == 物件に属するリスト == 

    // 物件に属する部屋のリスト
    public ObservableCollection<Models.Rent.Residentials.Listing> Rooms
    {
        get;
        set
        {
            field = value;
            IsModified = true;
        }
    } = [];

    // DBへの更新時にDBから削除されるべき部屋のIDリスト
    public ObservableCollection<Models.Rent.Residentials.Listing> RoomsToBeDeleted { get; set; } = [];

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
            // TODO:
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
        ValidateFudousanId(value);
        FudousanId = value;
    }
    private static void ValidateFudousanId(string value)
    {
        // TODO: Validate that the trimmed ID contains exactly 13 digits.
    }

    public void SetFudousanIdAdditionalCode(string value)
    {
        ValidateFudousanIdAdditionalCode(value);
        FudousanIdAdditionalCode = value;
    }
    private static void ValidateFudousanIdAdditionalCode(string value)
    {
        // TODO: Validate that the trimmed additional code contains exactly 4 digits.
    }

    public void SetRemarks(string value)
    {
        // TODO: Validate remarks if necessary.
        Remarks = value;
    }


    // 以下TODO:


    public void SetPropertyElectricKind(EnumElectricType value) => PropertyElectricKind = value;
    public void SetElectricDetail(string value) => ElectricDetail = value;

    public void SetRooms(ObservableCollection<Listing> value) => Rooms = value;
    public void SetRoomsToBeDeleted(ObservableCollection<Listing> value) => RoomsToBeDeleted = value;
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
