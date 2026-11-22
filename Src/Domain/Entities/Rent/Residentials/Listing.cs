#pragma warning disable IDE0079 // Remove unnecessary suppression
#pragma warning disable IDE0290 // Use primary constructor

using System.Globalization;
using System.Text.RegularExpressions;
using ZumenSearch.Domain.Entities.Base;
using ZumenSearch.Domain.Enums;

namespace ZumenSearch.Domain.Entities.Rent.Residentials;

// Aggregate Root entity.
// 賃貸住居用：募集物件（部屋）を表すエンティティ

public sealed partial class Listing : ListingBase
{
    public Listing(string id, EntityStatus status, string propertyId, EntityStatus propertyStatus, bool isPropertyUnitLevelOwnership, string propertyName, string fudousanId) : base(id, status, propertyId, propertyStatus, PropertyContextType.RentResidential, fudousanId)
    {
        IsPropertyUnitLevelOwnership = isPropertyUnitLevelOwnership;
        PropertyName = propertyName;
        SetIsModified(false);
    }

    // 物件（建物の名前を保持 - タイトル等に表示）
    public string PropertyName
    {
        get => field ?? string.Empty;
        private set;
    }

    // 部屋が属する建物が区分所有化どうかをここでも保持（部屋を直接開いた際に必要）
    public bool IsPropertyUnitLevelOwnership { get; private set; }

    #region == 部屋の基本情報 ==

    public FloorPlanType FloorPlan
    {
        get;
        private set { field = value; IsModified = true; }
    }

    public FloorPlanExtraType FloorPlanExtra
    {
        get;
        private set { field = value; IsModified = true; }
    }

    public FloorGroundType FloorGround
    {
        get;
        private set { field = value; IsModified = true; }
    }

    public string FloorExclusiveAreaSqm
    {
        get => field ?? string.Empty; // Ensure a non-null value is returned
        private set
        {
            field = value;
            IsModified = true;
        }
    }

    public string FloorNumber
    {
        get => field ?? string.Empty; // Ensure a non-null value is returned
        private set
        {
            field = value;
            IsModified = true;
        }
    }

    public bool IsCornerRoom
    {
        get;
        private set
        {
            field = value;
            IsModified = true;
        }
    }

    public ExposureDirectionType SelectedExposureDirectionType
    {
        get;
        private set { field = value; IsModified = true; }
    }

    public CurrentStatusType SelectedCurrentStatusType
    {
        get;
        private set { field = value; IsModified = true; }
    }

    public AvailabilityMonthType SelectedAvailabilityMonthType
    {
        get;
        private set { field = value; IsModified = true; }
    }

    public AvailabilityPeriodType SelectedAvailabilityPeriodType
    {
        get;
        private set { field = value; IsModified = true; }
    }

    public bool IsAvailableForImmediateMoveIn
    {
        get;
        private set
        {
            field = value;
            IsModified = true;
        }
    }

    public bool IsAvailableForRentNow
    {
        get;
        private set
        {
            field = value;
            IsModified = true;
        }
    }

    public DateTimeOffset? OccupancyStatusCheckedAt
    {
        get;
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
    } = "";

    public string Remarks { get; private set; } = string.Empty;


    #endregion

    #region == other ==

    // 賃料（円）
    public decimal Chinryou
    {
        get;
        private set
        {
            if (field == value)
            {
                return;
            }

            if (value > -1)
            {
                field = value;
                //OnPropertyChanged();
                IsModified = true;
            }
            else
            {
                throw new ArgumentOutOfRangeException(nameof(Chinryou), "Must be at least 0.");
            }
        }
    }

    public string ManagementFee
    {
        get => field ?? string.Empty;
        private set { field = value; IsModified = true; }
    }

    public FeeUnitType ManagementFeeUnit
    {
        get;
        private set { field = value; IsModified = true; }
    } = FeeUnitType.Yen;

    public string SecurityDeposit
    {
        get => field ?? string.Empty;
        private set { field = value; IsModified = true; }
    }

    public FeeUnitType SecurityDepositUnit
    {
        get;
        private set { field = value; IsModified = true; }
    }

    public string KeyMoney
    {
        get => field ?? string.Empty;
        private set { field = value; IsModified = true; }
    }

    public FeeUnitType KeyMoneyUnit
    {
        get;
        private set { field = value; IsModified = true; }
    }

    public ContractType ContractType
    {
        get;
        private set { field = value; IsModified = true; }
    }

    public string ContractPeriodYears
    {
        get => field ?? string.Empty;
        private set { field = value; IsModified = true; }
    }

    public string RenewalFee
    {
        get => field ?? string.Empty;
        private set { field = value; IsModified = true; }
    }

    public FeeUnitType RenewalFeeUnit
    {
        get;
        private set { field = value; IsModified = true; }
    }

    public string RecontractFee
    {
        get => field ?? string.Empty;
        private set { field = value; IsModified = true; }
    }

    public FeeUnitType RecontractFeeUnit
    {
        get;
        private set { field = value; IsModified = true; }
    } = FeeUnitType.Yen;

    public string GuarantorCompany
    {
        get => field ?? string.Empty;
        private set { field = value; IsModified = true; }
    }

    public string HousingInsurance
    {
        get => field ?? string.Empty;
        private set { field = value; IsModified = true; }
    }

    public string OtherFees
    {
        get => field ?? string.Empty;
        private set { field = value; IsModified = true; }
    }

    public bool PetFriendly
    {
        get;
        private set { field = value; IsModified = true; }
    }

    public bool TwoPersonOccupancy
    {
        get;
        private set { field = value; IsModified = true; }
    }

    public bool NoGuarantorNeeded
    {
        get;
        private set { field = value; IsModified = true; }
    }

    public bool OfficeUseAllowed
    {
        get;
        private set { field = value; IsModified = true; }
    }

    public string OtherConditions
    {
        get => field ?? string.Empty;
        private set { field = value; IsModified = true; }
    }

    public string ContractRemarks
    {
        get => field ?? string.Empty;
        private set { field = value; IsModified = true; }
    }

    public ListingHandlingType HandlingType { get; private set { field = value; IsModified = true; } }
    public ListingTransactionType TransactionType { get; private set { field = value; IsModified = true; } }
    public string LessorCommission { get => field ?? string.Empty; private set { field = value; IsModified = true; } }
    public string LesseeCommission { get => field ?? string.Empty; private set { field = value; IsModified = true; } }
    public string ListingBrokerCommission { get => field ?? string.Empty; private set { field = value; IsModified = true; } }
    public string TenantBrokerCommission { get => field ?? string.Empty; private set { field = value; IsModified = true; } }
    public ListingCommissionUnitType LessorCommissionUnit { get; private set { field = value; IsModified = true; } }
    public ListingCommissionUnitType LesseeCommissionUnit { get; private set { field = value; IsModified = true; } }
    public ListingCommissionUnitType ListingBrokerCommissionUnit { get; private set { field = value; IsModified = true; } }
    public ListingCommissionUnitType TenantBrokerCommissionUnit { get; private set { field = value; IsModified = true; } }
    public string TransactionRemarks { get => field ?? string.Empty; private set { field = value; IsModified = true; } }

    public bool HasAirConditioner { get; private set { field = value; IsModified = true; } }
    public bool HasWoodFlooring { get; private set { field = value; IsModified = true; } }
    public bool HasVideoIntercom { get; private set { field = value; IsModified = true; } }
    public bool HasBathReheating { get; private set { field = value; IsModified = true; } }
    public bool HasGasStove { get; private set { field = value; IsModified = true; } }
    public bool HasInductionCooktop { get; private set { field = value; IsModified = true; } }
    public bool HasMultipleBurners { get; private set { field = value; IsModified = true; } }
    public bool HasSystemKitchen { get; private set { field = value; IsModified = true; } }
    public bool HasLoft { get; private set { field = value; IsModified = true; } }
    public bool HasIndoorWashingMachineSpace { get; private set { field = value; IsModified = true; } }
    public bool HasBathroomDryer { get; private set { field = value; IsModified = true; } }
    public bool HasSeparateBathAndToilet { get; private set { field = value; IsModified = true; } }
    public bool HasSeparateWashbasin { get; private set { field = value; IsModified = true; } }
    public bool HasWarmWaterToiletSeat { get; private set { field = value; IsModified = true; } }
    public string ApplianceRemarks { get => field ?? string.Empty; private set { field = value; IsModified = true; } }


    #endregion

    #region == 部屋に属するリスト ==

    // 部屋写真リスト
    public List<ListingPicture> Pictures
    {
        get;
        set
        {
            field = value;
            IsModified = true;
        }
    } = [];

    // DBへの更新時にDBから削除されるべき部屋写真のIDリスト
    public List<ListingPicture> PicturesToBeDeleted { get; set; } = [];

    // 部屋図面リスト
    public List<ListingPdf> Pdfs
    {
        get;
        set
        {
            field = value;
            IsModified = true;
        }
    } = [];

    // DBへの更新時にDBから削除されるべき図面のIDリスト
    public List<ListingPdf> PdfsToBeDeleted { get; set; } = [];

    // 貸主のリスト
    public List<ZumenSearch.Domain.Entities.Base.PersonBase> Lessors
    {
        get;
        set
        {
            field = value;
            IsModified = true;
        }
    } = [];

    // DBへの更新時にDBから削除されるべき貸主のIDリスト
    public List<ZumenSearch.Domain.Entities.Base.PersonBase> LessorsToBeDeleted { get; set; } = [];

    // 宅建業者のリスト
    public List<ZumenSearch.Domain.Entities.Base.PersonBase> Brokers
    {
        get;
        set
        {
            field = value;
            IsModified = true;
        }
    } = [];

    // DBへの更新時にDBから削除されるべき宅建業者のIDリスト
    public List<ZumenSearch.Domain.Entities.Base.PersonBase> BrokersToBeDeleted { get; set; } = [];

    #endregion

    #region == Setter Methods ==

    public void SetPropertyName(string propertyName)
    {
        PropertyName = propertyName;
    }

    public void SetIsPropertyUnitLevelOwnership(bool isUnitLevelOwnership)
    {
        IsPropertyUnitLevelOwnership = isUnitLevelOwnership;
    }

    public void SetFudousanIdAdditionalCode(string fudousanIdAdditionalCode)
    {
        FudousanIdAdditionalCode = fudousanIdAdditionalCode;
    }

    public void SetFloorPlanTypeLabel(FloorPlanType floorPlan)
    {
        FloorPlan = floorPlan;
    }
    public void SetFloorPlanTypeLabelFromString(string Str)
    {
        if (string.IsNullOrEmpty(Str))
        {
            FloorPlan = FloorPlanType.Unspecified;
            return;
        }

        if (Enum.TryParse<FloorPlanType>(Str, out var result))
        {
            FloorPlan = result;
        }
    }

    public void SetFloorPlanExtraTypeLabel(FloorPlanExtraType floorPlanExtra)
    {
        FloorPlanExtra = floorPlanExtra;
    }
    public void SetFloorPlanExtraTypeLabelFromString(string Str)
    {
        if (string.IsNullOrEmpty(Str))
        {
            FloorPlanExtra = FloorPlanExtraType.Unspecified;
            return;
        }

        if (Enum.TryParse<FloorPlanExtraType>(Str, out var result))
        {
            FloorPlanExtra = result;
        }
    }

    public void SetFloorGroundTypeLabel(FloorGroundType floorGround)
    {
        FloorGround = floorGround;
    }

    public void SetFloorGroundTypeLabelFromString(string Str)
    {
        if (string.IsNullOrEmpty(Str))
        {
            FloorGround = FloorGroundType.AboveGround;
            return;
        }

        if (Enum.TryParse<FloorGroundType>(Str, out var result))
        {
            FloorGround = result;
        }
    }

    public void SetFloorExclusiveAreaSqm(string floorExclusiveAreaSqm)
    {
        FloorExclusiveAreaSqm = floorExclusiveAreaSqm;
    }

    public void SetFloorNumber(string floorNumber)
    {
        FloorNumber = floorNumber;
    }

    public void SetIsCornerRoom(bool isCornerRoom)
    {
        IsCornerRoom = isCornerRoom;
    }

    public void SetExposureDirectionTypeLabel(ExposureDirectionType exposureDirectionType)
    {
        SelectedExposureDirectionType = exposureDirectionType;
    }

    public void SetExposureDirectionTypeLabelFromString(string Str)
    {
        if (string.IsNullOrEmpty(Str))
        {
            SelectedExposureDirectionType = ExposureDirectionType.Unspecified;
            return;
        }

        if (Enum.TryParse<ExposureDirectionType>(Str, out var result))
        {
            SelectedExposureDirectionType = result;
        }
    }

    public void SetCurrentStatusTypeLabel(CurrentStatusType currentStatusType)
    {
        SelectedCurrentStatusType = currentStatusType;
    }

    public void SetAvailabilityStatusTypeLabelFromString(string Str)
    {
        if (string.IsNullOrEmpty(Str))
        {
            SelectedCurrentStatusType = CurrentStatusType.Unspecified;
            return;
        }

        if (Enum.TryParse<CurrentStatusType>(Str, out var result))
        {
            SelectedCurrentStatusType = result;
        }
    }

    public void SetAvailabilityMonthTypeLabel(AvailabilityMonthType availabilityMonthType)
    {
        SelectedAvailabilityMonthType = availabilityMonthType;
    }

    public void SetAvailabilityMonthTypeLabelFromString(string Str)
    {
        if (string.IsNullOrEmpty(Str))
        {
            SelectedAvailabilityMonthType = AvailabilityMonthType.Unspecified;
            return;
        }

        if (Enum.TryParse<AvailabilityMonthType>(Str, out var result))
        {
            SelectedAvailabilityMonthType = result;
        }
    }

    public void SetAvailabilityPeriodTypeLabel(AvailabilityPeriodType availabilityPeriodType)
    {
        SelectedAvailabilityPeriodType = availabilityPeriodType;
    }

    public void SetAvailabilityPeriodTypeLabelFromString(string Str)
    {
        if (string.IsNullOrEmpty(Str))
        {
            SelectedAvailabilityPeriodType = AvailabilityPeriodType.Unspecified;
            return;
        }

        if (Enum.TryParse<AvailabilityPeriodType>(Str, out var result))
        {
            SelectedAvailabilityPeriodType = result;
        }
    }

    public void SetIsAvailableForImmediateMoveIn(bool isAvailable)
    {
        IsAvailableForImmediateMoveIn = isAvailable;
    }

    public void SetIsAvailableForRentNow(bool isAvailable)
    {
        IsAvailableForRentNow = isAvailable;
    }

    public void SetOccupancyStatusCheckedAt(DateTimeOffset? checkedAt)
    {
        OccupancyStatusCheckedAt = checkedAt;
    }

    public void SetOccupancyStatusCheckedAtFromString(string Str)
    {
        if (string.IsNullOrEmpty(Str))
        {
            OccupancyStatusCheckedAt = null;
            return;
        }

        if (DateTimeOffset.TryParse(Str, CultureInfo.InvariantCulture, DateTimeStyles.RoundtripKind, out var result))
        {
            OccupancyStatusCheckedAt = result;
        }
    }

    public void SetManagementFee(string value) { ManagementFee = value; }
    public void SetSecurityDeposit(string value) { SecurityDeposit = value; }
    public void SetKeyMoney(string value) { KeyMoney = value; }
    public void SetRenewalFee(string value) { RenewalFee = value; }
    public void SetRecontractFee(string value) { RecontractFee = value; }
    public void SetContractPeriodYears(string value) { ContractPeriodYears = value; }
    public void SetGuarantorCompany(string value) { GuarantorCompany = value; }
    public void SetHousingInsurance(string value) { HousingInsurance = value; }
    public void SetOtherFees(string value) { OtherFees = value; }
    public void SetOtherConditions(string value) { OtherConditions = value; }
    public void SetContractRemarks(string value) { ContractRemarks = value; }
    public void SetHandlingTypeLabel(ListingHandlingType value) { HandlingType = value; }
    public void SetTransactionTypeLabel(ListingTransactionType value) { TransactionType = value; }
    public void SetLessorCommission(string value) { LessorCommission = value; }
    public void SetLesseeCommission(string value) { LesseeCommission = value; }
    public void SetListingBrokerCommission(string value) { ListingBrokerCommission = value; }
    public void SetTenantBrokerCommission(string value) { TenantBrokerCommission = value; }
    public void SetLessorCommissionUnitTypeLabel(ListingCommissionUnitType value) { LessorCommissionUnit = value; }
    public void SetLesseeCommissionUnitTypeLabel(ListingCommissionUnitType value) { LesseeCommissionUnit = value; }
    public void SetListingBrokerCommissionUnitTypeLabel(ListingCommissionUnitType value) { ListingBrokerCommissionUnit = value; }
    public void SetTenantBrokerCommissionUnitTypeLabel(ListingCommissionUnitType value) { TenantBrokerCommissionUnit = value; }
    public void SetTransactionRemarks(string value) { TransactionRemarks = value; }
    public void SetHandlingTypeLabelFromString(string value) { if (Enum.TryParse<ListingHandlingType>(value, out var result) && Enum.IsDefined(result)) SetHandlingTypeLabel(result); else throw new FormatException($"Invalid listing handling type: {value}"); }
    public void SetTransactionTypeLabelFromString(string value) { if (Enum.TryParse<ListingTransactionType>(value, out var result) && Enum.IsDefined(result)) SetTransactionTypeLabel(result); else throw new FormatException($"Invalid listing transaction type: {value}"); }
    public void SetLessorCommissionUnitTypeLabelFromString(string value) { SetLessorCommissionUnitTypeLabel(ParseCommissionUnit(value)); }
    public void SetLesseeCommissionUnitTypeLabelFromString(string value) { SetLesseeCommissionUnitTypeLabel(ParseCommissionUnit(value)); }
    public void SetListingBrokerCommissionUnitTypeLabelFromString(string value) { SetListingBrokerCommissionUnitTypeLabel(ParseCommissionUnit(value)); }
    public void SetTenantBrokerCommissionUnitTypeLabelFromString(string value) { SetTenantBrokerCommissionUnitTypeLabel(ParseCommissionUnit(value)); }
    private static ListingCommissionUnitType ParseCommissionUnit(string value) => Enum.TryParse<ListingCommissionUnitType>(value, out var result) && Enum.IsDefined(result) ? result : throw new FormatException($"Invalid commission unit: {value}");
    public void SetPetFriendly(bool value) { PetFriendly = value; }
    public void SetTwoPersonOccupancy(bool value) { TwoPersonOccupancy = value; }
    public void SetNoGuarantorNeeded(bool value) { NoGuarantorNeeded = value; }
    public void SetOfficeUseAllowed(bool value) { OfficeUseAllowed = value; }

    public void SetHasAirConditioner(bool value) { HasAirConditioner = value; }
    public void SetHasWoodFlooring(bool value) { HasWoodFlooring = value; }
    public void SetHasVideoIntercom(bool value) { HasVideoIntercom = value; }
    public void SetHasBathReheating(bool value) { HasBathReheating = value; }
    public void SetHasGasStove(bool value) { HasGasStove = value; }
    public void SetHasInductionCooktop(bool value) { HasInductionCooktop = value; }
    public void SetHasMultipleBurners(bool value) { HasMultipleBurners = value; }
    public void SetHasSystemKitchen(bool value) { HasSystemKitchen = value; }
    public void SetHasLoft(bool value) { HasLoft = value; }
    public void SetHasIndoorWashingMachineSpace(bool value) { HasIndoorWashingMachineSpace = value; }
    public void SetHasBathroomDryer(bool value) { HasBathroomDryer = value; }
    public void SetHasSeparateBathAndToilet(bool value) { HasSeparateBathAndToilet = value; }
    public void SetHasSeparateWashbasin(bool value) { HasSeparateWashbasin = value; }
    public void SetHasWarmWaterToiletSeat(bool value) { HasWarmWaterToiletSeat = value; }
    public void SetApplianceRemarks(string value) { ApplianceRemarks = value; }

    public void SetManagementFeeUnitTypeLabel(FeeUnitType value) { ManagementFeeUnit = value; }
    public void SetSecurityDepositUnitTypeLabel(FeeUnitType value) { SecurityDepositUnit = value; }
    public void SetKeyMoneyUnitTypeLabel(FeeUnitType value) { KeyMoneyUnit = value; }
    public void SetRenewalFeeUnitTypeLabel(FeeUnitType value) { RenewalFeeUnit = value; }
    public void SetRecontractFeeUnitTypeLabel(FeeUnitType value) { RecontractFeeUnit = value; }
    public void SetContractTypeLabel(ContractType value) { ContractType = value; }

    public void SetManagementFeeUnitTypeLabelFromString(string value)
    {
        if (string.IsNullOrEmpty(value))
        {
            ManagementFeeUnit = FeeUnitType.Yen;
        }
        else if (Enum.TryParse<FeeUnitType>(value, out var result) && Enum.IsDefined(result))
        {
            ManagementFeeUnit = result;
        }
    }

    public void SetSecurityDepositUnitTypeLabelFromString(string value)
    {
        if (string.IsNullOrEmpty(value))
        {
            SecurityDepositUnit = FeeUnitType.Months;
        }
        else if (Enum.TryParse<FeeUnitType>(value, out var result) && Enum.IsDefined(result))
        {
            SecurityDepositUnit = result;
        }
    }

    public void SetKeyMoneyUnitTypeLabelFromString(string value)
    {
        if (string.IsNullOrEmpty(value))
        {
            KeyMoneyUnit = FeeUnitType.Months;
        }
        else if (Enum.TryParse<FeeUnitType>(value, out var result) && Enum.IsDefined(result))
        {
            KeyMoneyUnit = result;
        }
    }

    public void SetRenewalFeeUnitTypeLabelFromString(string value)
    {
        if (string.IsNullOrEmpty(value))
        {
            RenewalFeeUnit = FeeUnitType.Months;
        }
        else if (Enum.TryParse<FeeUnitType>(value, out var result) && Enum.IsDefined(result))
        {
            RenewalFeeUnit = result;
        }
    }

    public void SetRecontractFeeUnitTypeLabelFromString(string value)
    {
        if (string.IsNullOrEmpty(value))
        {
            RecontractFeeUnit = FeeUnitType.Yen;
        }
        else if (Enum.TryParse<FeeUnitType>(value, out var result) && Enum.IsDefined(result))
        {
            RecontractFeeUnit = result;
        }
    }

    public void SetContractTypeLabelFromString(string value)
    {
        if (string.IsNullOrEmpty(value))
        {
            ContractType = ContractType.Unspecified;
        }
        else if (Enum.TryParse<ContractType>(value, out var result) && Enum.IsDefined(result))
        {
            ContractType = result;
        }
    }

    public void SetRemarks(string remarks)
    {
        Remarks = remarks;
    }

    public void SetChinryou(decimal chinryou)
    {
        Chinryou = chinryou;
    }

    public void SetChinryouFromString(string Str)
    {
        if (string.IsNullOrWhiteSpace(Str))
        {
            SetChinryou(0);
            return;
        }

        var convertedText = Regex.Replace(Str.Trim(), "[０-９]", match => ((char)(match.Value[0] - '０' + '0')).ToString());
        var text = Regex.Replace(convertedText, "[^0-9]", string.Empty);
        if (long.TryParse(text, NumberStyles.Integer, CultureInfo.InvariantCulture, out var result) && result >= 0)
        {
            SetChinryou(result);
        }
    }

    #endregion

}
