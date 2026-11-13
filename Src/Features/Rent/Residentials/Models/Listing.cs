using System.Collections.ObjectModel;
using System.Globalization;
using ZumenSearch.Models.Base;

#pragma warning disable IDE0079 // Remove unnecessary suppression
#pragma warning disable IDE0290 // Use primary constructor

namespace ZumenSearch.Models.Rent.Residentials;

// 部屋（編集用）
public sealed partial class Listing : ListingBase
{
    public Listing(string id, EntityStatus status, string propertyId, EntityStatus propertyStatus, bool isPropertyUnitLevelOwnership, string propertyName) : base(id, status, propertyId, propertyStatus, PropertyContextType.RentResidential)
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

    public ListingFloorPlanTypeLabel FloorPlan
    {
        get => field ?? new(FloorPlanType.Unspecified);
        private set
        {
            field = value;
            IsModified = true;
        }
    }

    public ListingFloorPlanExtraTypeLabel FloorPlanExtra
    {
        get => field ?? new(FloorPlanExtraType.Unspecified);
        private set
        {
            field = value;
            IsModified = true;
        }
    }

    public ListingFloorGroundTypeLabel FloorGround
    {
        get => field ?? new(FloorGroundType.AboveGround);
        private set
        {
            field = value;
            IsModified = true;
        }
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

    public ListingExposureDirectionTypeLabel SelectedExposureDirectionType
    {
        get => field ?? new(ExposureDirectionType.Unspecified);
        private set
        {
            field = value;
            IsModified = true;
        }
    }

    public ListingAvailabilityStatusTypeLabel SelectedAvailabilityStatusType
    {
        get => field ?? new(AvailabilityStatusType.Unspecified);
        private set
        {
            field = value;
            IsModified = true;
        }
    }

    public ListingAvailabilityMonthTypeLabel SelectedAvailabilityMonthType
    {
        get => field ?? new(AvailabilityMonthType.Unspecified);
        private set
        {
            field = value;
            IsModified = true;
        }
    }

    public ListingAvailabilityPeriodTypeLabel SelectedAvailabilityPeriodType
    {
        get => field ?? new(AvailabilityPeriodType.Unspecified);
        private set
        {
            field = value;
            IsModified = true;
        }
    }

    public ListingOccupancyStatusLabel SelectedOccupancyStatus
    {
        get => field ?? new(OccupancyStatus.Unspecified);
        private set
        {
            field = value;
            IsModified = true;
        }
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


    public string ManagementFee
    {
        get => field ?? string.Empty;
        private set { field = value; IsModified = true; }
    }

    public ListingFeeUnitTypeLabel ManagementFeeUnit
    {
        get => field ?? new(FeeUnitType.Yen);
        private set { field = value; IsModified = true; }
    }

    public string SecurityDeposit
    {
        get => field ?? string.Empty;
        private set { field = value; IsModified = true; }
    }

    public ListingFeeUnitTypeLabel SecurityDepositUnit
    {
        get => field ?? new(FeeUnitType.Months);
        private set { field = value; IsModified = true; }
    }

    public string KeyMoney
    {
        get => field ?? string.Empty;
        private set { field = value; IsModified = true; }
    }

    public ListingFeeUnitTypeLabel KeyMoneyUnit
    {
        get => field ?? new(FeeUnitType.Months);
        private set { field = value; IsModified = true; }
    }

    public ListingContractTypeLabel ContractType
    {
        get => field ?? new(global::ZumenSearch.Models.Rent.Residentials.ContractType.Unspecified);
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

    public ListingFeeUnitTypeLabel RenewalFeeUnit
    {
        get => field ?? new(FeeUnitType.Months);
        private set { field = value; IsModified = true; }
    }

    public string RecontractFee
    {
        get => field ?? string.Empty;
        private set { field = value; IsModified = true; }
    }

    public ListingFeeUnitTypeLabel RecontractFeeUnit
    {
        get => field ?? new(FeeUnitType.Yen);
        private set { field = value; IsModified = true; }
    }

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

    public ListingHandlingTypeLabel HandlingType { get => field ?? new(ListingHandlingType.Unspecified); private set { field = value; IsModified = true; } }
    public ListingTransactionTypeLabel TransactionType { get => field ?? new(ListingTransactionType.Unspecified); private set { field = value; IsModified = true; } }
    public string LessorCommission { get => field ?? string.Empty; private set { field = value; IsModified = true; } }
    public string LesseeCommission { get => field ?? string.Empty; private set { field = value; IsModified = true; } }
    public string ListingBrokerCommission { get => field ?? string.Empty; private set { field = value; IsModified = true; } }
    public string TenantBrokerCommission { get => field ?? string.Empty; private set { field = value; IsModified = true; } }
    public ListingCommissionUnitTypeLabel LessorCommissionUnit { get => field ?? new(ListingCommissionUnitType.Percent); private set { field = value; IsModified = true; } }
    public ListingCommissionUnitTypeLabel LesseeCommissionUnit { get => field ?? new(ListingCommissionUnitType.Percent); private set { field = value; IsModified = true; } }
    public ListingCommissionUnitTypeLabel ListingBrokerCommissionUnit { get => field ?? new(ListingCommissionUnitType.Percent); private set { field = value; IsModified = true; } }
    public ListingCommissionUnitTypeLabel TenantBrokerCommissionUnit { get => field ?? new(ListingCommissionUnitType.Percent); private set { field = value; IsModified = true; } }
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

    public string Remarks { get; private set; } = string.Empty;

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


    #endregion

    #region == 部屋に属するリスト ==

    // 部屋写真リスト
    public ObservableCollection<ListingPicture> Pictures
    {
        get;
        set
        {
            field = value;
            IsModified = true;
        }
    } = [];

    // DBへの更新時にDBから削除されるべき部屋写真のIDリスト
    public ObservableCollection<ListingPicture> PicturesToBeDeleted { get; set; } = [];

    // 部屋図面リスト
    public ObservableCollection<ListingPdf> Pdfs
    {
        get;
        set
        {
            field = value;
            IsModified = true;
        }
    } = [];

    // DBへの更新時にDBから削除されるべき図面のIDリスト
    public ObservableCollection<ListingPdf> PdfsToBeDeleted { get; set; } = [];

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

    // DBへの更新時にDBから削除されるべき宅建業者のIDリスト
    public ObservableCollection<Models.Base.PersonBase> BrokersToBeDeleted { get; set; } = [];

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

    public void SetFloorPlanTypeLabel(ListingFloorPlanTypeLabel floorPlan)
    {
        FloorPlan = floorPlan;
    }
    public void SetFloorPlanTypeLabelFromString(string Str)
    {
        if (string.IsNullOrEmpty(Str))
        {
            FloorPlan = new ListingFloorPlanTypeLabel(FloorPlanType.Unspecified);
            return;
        }

        if (Enum.TryParse<FloorPlanType>(Str, out var result))
        {
            FloorPlan = new ListingFloorPlanTypeLabel(result);
        }
    }

    public void SetFloorPlanExtraTypeLabel(ListingFloorPlanExtraTypeLabel floorPlanExtra)
    {
        FloorPlanExtra = floorPlanExtra;
    }
    public void SetFloorPlanExtraTypeLabelFromString(string Str)
    {
        if (string.IsNullOrEmpty(Str))
        {
            FloorPlanExtra = new ListingFloorPlanExtraTypeLabel(FloorPlanExtraType.Unspecified);
            return;
        }

        if (Enum.TryParse<FloorPlanExtraType>(Str, out var result))
        {
            FloorPlanExtra = new ListingFloorPlanExtraTypeLabel(result);
        }
    }

    public void SetFloorGroundTypeLabel(ListingFloorGroundTypeLabel floorGround)
    {
        FloorGround = floorGround;
    }

    public void SetFloorGroundTypeLabelFromString(string Str)
    {
        if (string.IsNullOrEmpty(Str))
        {
            FloorGround = new ListingFloorGroundTypeLabel(FloorGroundType.AboveGround);
            return;
        }

        if (Enum.TryParse<FloorGroundType>(Str, out var result))
        {
            FloorGround = new ListingFloorGroundTypeLabel(result);
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

    public void SetExposureDirectionTypeLabel(ListingExposureDirectionTypeLabel exposureDirectionType)
    {
        SelectedExposureDirectionType = exposureDirectionType;
    }

    public void SetExposureDirectionTypeLabelFromString(string Str)
    {
        if (string.IsNullOrEmpty(Str))
        {
            SelectedExposureDirectionType = new ListingExposureDirectionTypeLabel(ExposureDirectionType.Unspecified);
            return;
        }

        if (Enum.TryParse<ExposureDirectionType>(Str, out var result))
        {
            SelectedExposureDirectionType = new ListingExposureDirectionTypeLabel(result);
        }
    }

    public void SetAvailabilityStatusTypeLabel(ListingAvailabilityStatusTypeLabel availabilityStatusType)
    {
        SelectedAvailabilityStatusType = availabilityStatusType;
    }

    public void SetAvailabilityStatusTypeLabelFromString(string Str)
    {
        if (string.IsNullOrEmpty(Str))
        {
            SelectedAvailabilityStatusType = new ListingAvailabilityStatusTypeLabel(AvailabilityStatusType.Unspecified);
            return;
        }

        if (Enum.TryParse<AvailabilityStatusType>(Str, out var result))
        {
            SelectedAvailabilityStatusType = new ListingAvailabilityStatusTypeLabel(result);
        }
    }

    public void SetAvailabilityMonthTypeLabel(ListingAvailabilityMonthTypeLabel availabilityMonthType)
    {
        SelectedAvailabilityMonthType = availabilityMonthType;
    }

    public void SetAvailabilityMonthTypeLabelFromString(string Str)
    {
        if (string.IsNullOrEmpty(Str))
        {
            SelectedAvailabilityMonthType = new ListingAvailabilityMonthTypeLabel(AvailabilityMonthType.Unspecified);
            return;
        }

        if (Enum.TryParse<AvailabilityMonthType>(Str, out var result))
        {
            SelectedAvailabilityMonthType = new ListingAvailabilityMonthTypeLabel(result);
        }
    }

    public void SetAvailabilityPeriodTypeLabel(ListingAvailabilityPeriodTypeLabel availabilityPeriodType)
    {
        SelectedAvailabilityPeriodType = availabilityPeriodType;
    }

    public void SetAvailabilityPeriodTypeLabelFromString(string Str)
    {
        if (string.IsNullOrEmpty(Str))
        {
            SelectedAvailabilityPeriodType = new ListingAvailabilityPeriodTypeLabel(AvailabilityPeriodType.Unspecified);
            return;
        }

        if (Enum.TryParse<AvailabilityPeriodType>(Str, out var result))
        {
            SelectedAvailabilityPeriodType = new ListingAvailabilityPeriodTypeLabel(result);
        }
    }

    public void SetOccupancyStatusLabel(ListingOccupancyStatusLabel occupancyStatus)
    {
        SelectedOccupancyStatus = occupancyStatus;
    }

    public void SetOccupancyStatusLabelFromString(string Str)
    {
        if (string.IsNullOrEmpty(Str))
        {
            SelectedOccupancyStatus = new ListingOccupancyStatusLabel(OccupancyStatus.Unspecified);
            return;
        }

        if (Enum.TryParse<OccupancyStatus>(Str, out var result))
        {
            SelectedOccupancyStatus = new ListingOccupancyStatusLabel(result);
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
    public void SetHandlingTypeLabel(ListingHandlingTypeLabel value) { HandlingType = value; }
    public void SetTransactionTypeLabel(ListingTransactionTypeLabel value) { TransactionType = value; }
    public void SetLessorCommission(string value) { LessorCommission = value; }
    public void SetLesseeCommission(string value) { LesseeCommission = value; }
    public void SetListingBrokerCommission(string value) { ListingBrokerCommission = value; }
    public void SetTenantBrokerCommission(string value) { TenantBrokerCommission = value; }
    public void SetLessorCommissionUnitTypeLabel(ListingCommissionUnitTypeLabel value) { LessorCommissionUnit = value; }
    public void SetLesseeCommissionUnitTypeLabel(ListingCommissionUnitTypeLabel value) { LesseeCommissionUnit = value; }
    public void SetListingBrokerCommissionUnitTypeLabel(ListingCommissionUnitTypeLabel value) { ListingBrokerCommissionUnit = value; }
    public void SetTenantBrokerCommissionUnitTypeLabel(ListingCommissionUnitTypeLabel value) { TenantBrokerCommissionUnit = value; }
    public void SetTransactionRemarks(string value) { TransactionRemarks = value; }
    public void SetHandlingTypeLabelFromString(string value) { if (Enum.TryParse<ListingHandlingType>(value, out var result) && Enum.IsDefined(result)) SetHandlingTypeLabel(new(result)); else throw new FormatException($"Invalid listing handling type: {value}"); }
    public void SetTransactionTypeLabelFromString(string value) { if (Enum.TryParse<ListingTransactionType>(value, out var result) && Enum.IsDefined(result)) SetTransactionTypeLabel(new(result)); else throw new FormatException($"Invalid listing transaction type: {value}"); }
    public void SetLessorCommissionUnitTypeLabelFromString(string value) { SetLessorCommissionUnitTypeLabel(ParseCommissionUnit(value)); }
    public void SetLesseeCommissionUnitTypeLabelFromString(string value) { SetLesseeCommissionUnitTypeLabel(ParseCommissionUnit(value)); }
    public void SetListingBrokerCommissionUnitTypeLabelFromString(string value) { SetListingBrokerCommissionUnitTypeLabel(ParseCommissionUnit(value)); }
    public void SetTenantBrokerCommissionUnitTypeLabelFromString(string value) { SetTenantBrokerCommissionUnitTypeLabel(ParseCommissionUnit(value)); }
    private static ListingCommissionUnitTypeLabel ParseCommissionUnit(string value) => Enum.TryParse<ListingCommissionUnitType>(value, out var result) && Enum.IsDefined(result) ? new(result) : throw new FormatException($"Invalid commission unit: {value}");
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

    public void SetManagementFeeUnitTypeLabel(ListingFeeUnitTypeLabel value) { ManagementFeeUnit = value; }
    public void SetSecurityDepositUnitTypeLabel(ListingFeeUnitTypeLabel value) { SecurityDepositUnit = value; }
    public void SetKeyMoneyUnitTypeLabel(ListingFeeUnitTypeLabel value) { KeyMoneyUnit = value; }
    public void SetRenewalFeeUnitTypeLabel(ListingFeeUnitTypeLabel value) { RenewalFeeUnit = value; }
    public void SetRecontractFeeUnitTypeLabel(ListingFeeUnitTypeLabel value) { RecontractFeeUnit = value; }
    public void SetContractTypeLabel(ListingContractTypeLabel value) { ContractType = value; }

    public void SetManagementFeeUnitTypeLabelFromString(string value)
    {
        if (string.IsNullOrEmpty(value))
        {
            ManagementFeeUnit = new ListingFeeUnitTypeLabel(FeeUnitType.Yen);
        }
        else if (Enum.TryParse<FeeUnitType>(value, out var result) && Enum.IsDefined(result))
        {
            ManagementFeeUnit = new ListingFeeUnitTypeLabel(result);
        }
    }

    public void SetSecurityDepositUnitTypeLabelFromString(string value)
    {
        if (string.IsNullOrEmpty(value))
        {
            SecurityDepositUnit = new ListingFeeUnitTypeLabel(FeeUnitType.Months);
        }
        else if (Enum.TryParse<FeeUnitType>(value, out var result) && Enum.IsDefined(result))
        {
            SecurityDepositUnit = new ListingFeeUnitTypeLabel(result);
        }
    }

    public void SetKeyMoneyUnitTypeLabelFromString(string value)
    {
        if (string.IsNullOrEmpty(value))
        {
            KeyMoneyUnit = new ListingFeeUnitTypeLabel(FeeUnitType.Months);
        }
        else if (Enum.TryParse<FeeUnitType>(value, out var result) && Enum.IsDefined(result))
        {
            KeyMoneyUnit = new ListingFeeUnitTypeLabel(result);
        }
    }

    public void SetRenewalFeeUnitTypeLabelFromString(string value)
    {
        if (string.IsNullOrEmpty(value))
        {
            RenewalFeeUnit = new ListingFeeUnitTypeLabel(FeeUnitType.Months);
        }
        else if (Enum.TryParse<FeeUnitType>(value, out var result) && Enum.IsDefined(result))
        {
            RenewalFeeUnit = new ListingFeeUnitTypeLabel(result);
        }
    }

    public void SetRecontractFeeUnitTypeLabelFromString(string value)
    {
        if (string.IsNullOrEmpty(value))
        {
            RecontractFeeUnit = new ListingFeeUnitTypeLabel(FeeUnitType.Yen);
        }
        else if (Enum.TryParse<FeeUnitType>(value, out var result) && Enum.IsDefined(result))
        {
            RecontractFeeUnit = new ListingFeeUnitTypeLabel(result);
        }
    }

    public void SetContractTypeLabelFromString(string value)
    {
        if (string.IsNullOrEmpty(value))
        {
            ContractType = new ListingContractTypeLabel(global::ZumenSearch.Models.Rent.Residentials.ContractType.Unspecified);
        }
        else if (Enum.TryParse<global::ZumenSearch.Models.Rent.Residentials.ContractType>(value, out var result) && Enum.IsDefined(result))
        {
            ContractType = new ListingContractTypeLabel(result);
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

        var text = Helpers.Common.ReplaceZenkakuNumbers(Str.Trim());
        if (Helpers.Common.CanConvertToPositiveNumber(text) &&
            long.TryParse(text, NumberStyles.Integer, CultureInfo.InvariantCulture, out var result))
        {
            SetChinryou(result);
        }
    }

    #endregion
}
