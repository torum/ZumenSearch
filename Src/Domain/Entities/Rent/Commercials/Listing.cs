#pragma warning disable IDE0079 // Remove unnecessary suppression
#pragma warning disable IDE0290 // Use primary constructor

using ZumenSearch.Domain.Entities.Base;
using ZumenSearch.Domain.Enums;

namespace ZumenSearch.Domain.Entities.Rent.Commercials.Listing;

// Aggregate Root entity.
// 賃貸事業用：募集物件を表すエンティティ

public sealed partial class Listing : ListingBase
{
    public Listing(string id, EntityStatus status, string propertyId, EntityStatus propertyStatus, bool isPropertyUnitOwnership, string propertyName, string fudousanId) : base(id, status, propertyId, propertyStatus, PropertyContextType.RentCommercial, fudousanId)
    {
        IsPropertyUnitOwnership = isPropertyUnitOwnership;
        PropertyName = propertyName;
        SetIsModified(false);
    }

    // 物件（建物の名前を保持 - タイトル等に表示）
    public string PropertyName
    {
        get => field ?? string.Empty;
        private set;
    }

    // 募集物件が属する建物が区分所有かどうかをここでも保持（募集物件を直接開いた際に必要）
    public bool IsPropertyUnitOwnership { get; private set; }

    #region == 募集物件の基本情報 ==

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

    public decimal? Chinryou
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

    public ListingFloorGroundType FloorGround { get; private set; } = ListingFloorGroundType.AboveGround;
    public int? FloorNumber { get; private set; }
    public decimal? FloorArea { get; private set; }
    public ListingUseType Use { get; private set; } = ListingUseType.Unspecified;
    public string UseDetails { get; private set; } = string.Empty;
    public ListingAvailabilityStatusType CurrentStatus { get; private set; } = ListingAvailabilityStatusType.Unspecified;
    public bool IsAvailableForRentNow { get; private set; }
    public ListingAvailabilityMonthType AvailabilityMonth { get; private set; } = ListingAvailabilityMonthType.Unspecified;
    public ListingAvailabilityPeriodType AvailabilityPeriod { get; private set; } = ListingAvailabilityPeriodType.Unspecified;
    public bool IsAvailableForImmediateMoveIn { get; private set; }
    public DateTimeOffset? OccupancyStatusCheckedAt { get; private set; }

    public string Remarks
    {
        get => field ?? string.Empty;
        private set { field = value; IsModified = true; }
    }

    #endregion

    #region == other ==

    public decimal? ManagementFee { get; private set; }
    public decimal? SecurityDeposit { get; private set; }
    public decimal? GuaranteeDeposit { get; private set; }
    public decimal? KeyMoney { get; private set; }
    public decimal? RightsMoney { get; private set; }
    public decimal? DepositDeduction { get; private set; }
    public decimal? RenewalFee { get; private set; }
    public decimal? RecontractFee { get; private set; }
    public decimal? ContractPeriodYears { get; private set; }
    public ListingFeeUnitType SecurityDepositUnit { get; private set; } = ListingFeeUnitType.Months;
    public ListingFeeUnitType GuaranteeDepositUnit { get; private set; } = ListingFeeUnitType.Months;
    public ListingFeeUnitType KeyMoneyUnit { get; private set; } = ListingFeeUnitType.Months;
    public ListingFeeUnitType RightsMoneyUnit { get; private set; } = ListingFeeUnitType.Months;
    public ListingFeeUnitType DepositDeductionUnit { get; private set; } = ListingFeeUnitType.Months;
    public ListingFeeUnitType RenewalFeeUnit { get; private set; } = ListingFeeUnitType.Months;
    public ListingFeeUnitType RecontractFeeUnit { get; private set; } = ListingFeeUnitType.Yen;
    public ListingContractTaxType RentTax { get; private set; } = ListingContractTaxType.Unconfirmed;
    public ListingContractTaxType ManagementFeeTax { get; private set; } = ListingContractTaxType.Unconfirmed;
    public ListingContractType ContractType { get; private set; } = ListingContractType.Unspecified;
    public string GuarantorCompany { get; private set; } = string.Empty;
    public string Insurance { get; private set; } = string.Empty;
    public string OtherFees { get; private set; } = string.Empty;
    public string ContractConditions { get; private set; } = string.Empty;
    public string ContractRemarks { get; private set; } = string.Empty;

    public ListingHandlingType HandlingType { get; private set; } = ListingHandlingType.Unspecified;
    public ListingTransactionType TransactionType { get; private set; } = ListingTransactionType.Unspecified;
    public string LessorCommission { get; private set; } = string.Empty;
    public string LesseeCommission { get; private set; } = string.Empty;
    public string ListingBrokerCommission { get; private set; } = string.Empty;
    public string TenantBrokerCommission { get; private set; } = string.Empty;
    public ListingCommissionUnitType LessorCommissionUnit { get; private set; } = ListingCommissionUnitType.Percent;
    public ListingCommissionUnitType LesseeCommissionUnit { get; private set; } = ListingCommissionUnitType.Percent;
    public ListingCommissionUnitType ListingBrokerCommissionUnit { get; private set; } = ListingCommissionUnitType.Percent;
    public ListingCommissionUnitType TenantBrokerCommissionUnit { get; private set; } = ListingCommissionUnitType.Percent;
    public string TransactionRemarks { get; private set; } = string.Empty;

    public bool HasAirConditioning { get; private set; }
    public bool HasPrivateToilet { get; private set; }
    public bool HasHotWater { get; private set; }
    public bool HasKitchen { get; private set; }
    public bool HasVentilation { get; private set; }
    public bool HasInternet { get; private set; }
    public string EquipmentDetails { get; private set; } = string.Empty;
    public ListingUnitConditionType UnitCondition { get; private set; } = ListingUnitConditionType.Unspecified;
    public ListingFixtureTransferType FixtureTransfer { get; private set; } = ListingFixtureTransferType.Unconfirmed;
    public string BusinessHoursRestrictions { get; private set; } = string.Empty;
    public string UseConditions { get; private set; } = string.Empty;
    public bool PermitsHeavyFoodService { get; private set; }
    public bool PermitsLightFoodService { get; private set; }
    public bool PermitsRetail { get; private set; }
    public bool PermitsServices { get; private set; }
    public bool PermitsOffice { get; private set; }
    public bool PermitsOther { get; private set; }
    public string PermittedBusinessDetails { get; private set; } = string.Empty;
    public string FacilitiesRemarks { get; private set; } = string.Empty;

    #endregion

    #region == 募集物件に属するリスト ==

    // 募集物件写真リスト
    public List<ListingPicture> Pictures
    {
        get;
        set
        {
            field = value;
            IsModified = true;
        }
    } = [];

    // DBへの更新時にDBから削除されるべき募集物件写真のIDリスト
    public List<ListingPicture> PicturesToBeDeleted { get; set; } = [];

    // 募集物件図面リスト
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

    private static T ParseType<T>(string value) where T : struct, Enum => Enum.TryParse<T>(value, out var result) && Enum.IsDefined(result) && !int.TryParse(value, out _) ? result : throw new ArgumentException($"Invalid {typeof(T).Name}: {value}");

    public void SetFloorGroundTypeLabel(ListingFloorGroundType value)
    {
        FloorGround = value;
        IsModified = true;
    }

    public void SetFloorGroundTypeLabelFromString(string value)
    {
        SetFloorGroundTypeLabel(ParseType<ListingFloorGroundType>(value));
    }

    public void SetFloorNumber(int? value)
    {
        if (value is <= 0) throw new ArgumentOutOfRangeException(nameof(value));
        FloorNumber = value;
        IsModified = true;
    }

    public void SetFloorArea(decimal? value)
    {
        if (value is < 0) throw new ArgumentOutOfRangeException(nameof(value));
        FloorArea = value;
        IsModified = true;
    }

    public void SetUseTypeLabel(ListingUseType value)
    {
        Use = value;
        IsModified = true;
    }

    public void SetUseTypeLabelFromString(string value)
    {
        SetUseTypeLabel(ParseType<ListingUseType>(value));
    }

    public void SetUseDetails(string value)
    {
        UseDetails = value;
        IsModified = true;
    }

    public void SetCurrentStatusTypeLabel(ListingAvailabilityStatusType value)
    {
        CurrentStatus = value;
        IsModified = true;
    }

    public void SetCurrentStatusTypeLabelFromString(string value)
    {
        SetCurrentStatusTypeLabel(ParseType<ListingAvailabilityStatusType>(value));
    }

    public void SetIsAvailableForRentNow(bool value)
    {
        IsAvailableForRentNow = value;
        IsModified = true;
    }

    public void SetAvailabilityMonthTypeLabel(ListingAvailabilityMonthType value)
    {
        AvailabilityMonth = value;
        IsModified = true;
    }

    public void SetAvailabilityMonthTypeLabelFromString(string value)
    {
        SetAvailabilityMonthTypeLabel(ParseType<ListingAvailabilityMonthType>(value));
    }

    public void SetAvailabilityPeriodTypeLabel(ListingAvailabilityPeriodType value)
    {
        AvailabilityPeriod = value;
        IsModified = true;
    }

    public void SetAvailabilityPeriodTypeLabelFromString(string value)
    {
        SetAvailabilityPeriodTypeLabel(ParseType<ListingAvailabilityPeriodType>(value));
    }

    public void SetIsAvailableForImmediateMoveIn(bool value)
    {
        IsAvailableForImmediateMoveIn = value;
        IsModified = true;
    }

    public void SetOccupancyStatusCheckedAt(DateTimeOffset? checkedAt)
    {
        OccupancyStatusCheckedAt = checkedAt;
        IsModified = true;
    }

    public void SetPropertyName(string propertyName)
    {
        PropertyName = propertyName;
    }

    public void SetIsPropertyUnitOwnership(bool isUnitOwnership)
    {
        IsPropertyUnitOwnership = isUnitOwnership;
    }

    public void SetFudousanIdAdditionalCode(string fudousanIdAdditionalCode)
    {
        FudousanIdAdditionalCode = fudousanIdAdditionalCode;
    }

    public void SetChinryou(decimal? chinryou)
    {
        if (chinryou is < 0)
        {
            throw new ArgumentOutOfRangeException(nameof(chinryou), "Must be at least 0.");
        }

        Chinryou = chinryou;
    }

    public void SetRemarks(string remarks)
    {
        Remarks = remarks;
    }

    private static decimal? NonNegative(decimal? value) => value is < 0 ? throw new ArgumentOutOfRangeException(nameof(value)) : value;
    public void SetManagementFee(decimal? value) { ManagementFee = NonNegative(value); IsModified = true; }
    public void SetSecurityDeposit(decimal? value) { SecurityDeposit = NonNegative(value); IsModified = true; }
    public void SetGuaranteeDeposit(decimal? value) { GuaranteeDeposit = NonNegative(value); IsModified = true; }
    public void SetKeyMoney(decimal? value) { KeyMoney = NonNegative(value); IsModified = true; }
    public void SetRightsMoney(decimal? value) { RightsMoney = NonNegative(value); IsModified = true; }
    public void SetDepositDeduction(decimal? value) { DepositDeduction = NonNegative(value); IsModified = true; }
    public void SetRenewalFee(decimal? value) { RenewalFee = NonNegative(value); IsModified = true; }
    public void SetRecontractFee(decimal? value) { RecontractFee = NonNegative(value); IsModified = true; }
    public void SetContractPeriodYears(decimal? value) { if (value is < 0) throw new ArgumentOutOfRangeException(nameof(value)); ContractPeriodYears = value; IsModified = true; }
    public void SetSecurityDepositUnitTypeLabel(ListingFeeUnitType value) { SecurityDepositUnit = value; IsModified = true; }
    public void SetSecurityDepositUnitTypeLabelFromString(string value) => SetSecurityDepositUnitTypeLabel(ParseType<ListingFeeUnitType>(value));
    public void SetGuaranteeDepositUnitTypeLabel(ListingFeeUnitType value) { GuaranteeDepositUnit = value; IsModified = true; }
    public void SetGuaranteeDepositUnitTypeLabelFromString(string value) => SetGuaranteeDepositUnitTypeLabel(ParseType<ListingFeeUnitType>(value));
    public void SetKeyMoneyUnitTypeLabel(ListingFeeUnitType value) { KeyMoneyUnit = value; IsModified = true; }
    public void SetKeyMoneyUnitTypeLabelFromString(string value) => SetKeyMoneyUnitTypeLabel(ParseType<ListingFeeUnitType>(value));
    public void SetRightsMoneyUnitTypeLabel(ListingFeeUnitType value) { RightsMoneyUnit = value; IsModified = true; }
    public void SetRightsMoneyUnitTypeLabelFromString(string value) => SetRightsMoneyUnitTypeLabel(ParseType<ListingFeeUnitType>(value));
    public void SetDepositDeductionUnitTypeLabel(ListingFeeUnitType value) { DepositDeductionUnit = value; IsModified = true; }
    public void SetDepositDeductionUnitTypeLabelFromString(string value) => SetDepositDeductionUnitTypeLabel(ParseType<ListingFeeUnitType>(value));
    public void SetRenewalFeeUnitTypeLabel(ListingFeeUnitType value) { RenewalFeeUnit = value; IsModified = true; }
    public void SetRenewalFeeUnitTypeLabelFromString(string value) => SetRenewalFeeUnitTypeLabel(ParseType<ListingFeeUnitType>(value));
    public void SetRecontractFeeUnitTypeLabel(ListingFeeUnitType value) { RecontractFeeUnit = value; IsModified = true; }
    public void SetRecontractFeeUnitTypeLabelFromString(string value) => SetRecontractFeeUnitTypeLabel(ParseType<ListingFeeUnitType>(value));
    public void SetRentTaxTypeLabel(ListingContractTaxType value) { RentTax = value; IsModified = true; }
    public void SetRentTaxTypeLabelFromString(string value) => SetRentTaxTypeLabel(ParseType<ListingContractTaxType>(value));
    public void SetManagementFeeTaxTypeLabel(ListingContractTaxType value) { ManagementFeeTax = value; IsModified = true; }
    public void SetManagementFeeTaxTypeLabelFromString(string value) => SetManagementFeeTaxTypeLabel(ParseType<ListingContractTaxType>(value));
    public void SetContractTypeLabel(ListingContractType value) { ContractType = value; IsModified = true; }
    public void SetContractTypeLabelFromString(string value) => SetContractTypeLabel(ParseType<ListingContractType>(value));
    public void SetGuarantorCompany(string value) { GuarantorCompany = value; IsModified = true; }
    public void SetInsurance(string value) { Insurance = value; IsModified = true; }
    public void SetOtherFees(string value) { OtherFees = value; IsModified = true; }
    public void SetContractConditions(string value) { ContractConditions = value; IsModified = true; }
    public void SetContractRemarks(string value) { ContractRemarks = value; IsModified = true; }

    public void SetHandlingTypeLabel(ListingHandlingType value) { HandlingType = value; IsModified = true; }
    public void SetHandlingTypeLabelFromString(string value) => SetHandlingTypeLabel(ParseType<ListingHandlingType>(value));
    public void SetTransactionTypeLabel(ListingTransactionType value) { TransactionType = value; IsModified = true; }
    public void SetTransactionTypeLabelFromString(string value) => SetTransactionTypeLabel(ParseType<ListingTransactionType>(value));
    public void SetLessorCommission(string value) { LessorCommission = value; IsModified = true; }
    public void SetLesseeCommission(string value) { LesseeCommission = value; IsModified = true; }
    public void SetListingBrokerCommission(string value) { ListingBrokerCommission = value; IsModified = true; }
    public void SetTenantBrokerCommission(string value) { TenantBrokerCommission = value; IsModified = true; }
    public void SetLessorCommissionUnitTypeLabel(ListingCommissionUnitType value) { LessorCommissionUnit = value; IsModified = true; }
    public void SetLessorCommissionUnitTypeLabelFromString(string value) => SetLessorCommissionUnitTypeLabel(ParseType<ListingCommissionUnitType>(value));
    public void SetLesseeCommissionUnitTypeLabel(ListingCommissionUnitType value) { LesseeCommissionUnit = value; IsModified = true; }
    public void SetLesseeCommissionUnitTypeLabelFromString(string value) => SetLesseeCommissionUnitTypeLabel(ParseType<ListingCommissionUnitType>(value));
    public void SetListingBrokerCommissionUnitTypeLabel(ListingCommissionUnitType value) { ListingBrokerCommissionUnit = value; IsModified = true; }
    public void SetListingBrokerCommissionUnitTypeLabelFromString(string value) => SetListingBrokerCommissionUnitTypeLabel(ParseType<ListingCommissionUnitType>(value));
    public void SetTenantBrokerCommissionUnitTypeLabel(ListingCommissionUnitType value) { TenantBrokerCommissionUnit = value; IsModified = true; }
    public void SetTenantBrokerCommissionUnitTypeLabelFromString(string value) => SetTenantBrokerCommissionUnitTypeLabel(ParseType<ListingCommissionUnitType>(value));
    public void SetTransactionRemarks(string value) { TransactionRemarks = value; IsModified = true; }

    public void SetHasAirConditioning(bool value) { HasAirConditioning = value; IsModified = true; }
    public void SetHasPrivateToilet(bool value) { HasPrivateToilet = value; IsModified = true; }
    public void SetHasHotWater(bool value) { HasHotWater = value; IsModified = true; }
    public void SetHasKitchen(bool value) { HasKitchen = value; IsModified = true; }
    public void SetHasVentilation(bool value) { HasVentilation = value; IsModified = true; }
    public void SetHasInternet(bool value) { HasInternet = value; IsModified = true; }
    public void SetEquipmentDetails(string value) { EquipmentDetails = value; IsModified = true; }
    public void SetUnitConditionTypeLabel(ListingUnitConditionType value) { UnitCondition = value; IsModified = true; }
    public void SetUnitConditionTypeLabelFromString(string value) => SetUnitConditionTypeLabel(ParseType<ListingUnitConditionType>(value));
    public void SetFixtureTransferTypeLabel(ListingFixtureTransferType value) { FixtureTransfer = value; IsModified = true; }
    public void SetFixtureTransferTypeLabelFromString(string value) => SetFixtureTransferTypeLabel(ParseType<ListingFixtureTransferType>(value));
    public void SetBusinessHoursRestrictions(string value) { BusinessHoursRestrictions = value; IsModified = true; }
    public void SetUseConditions(string value) { UseConditions = value; IsModified = true; }
    public void SetPermitsHeavyFoodService(bool value) { PermitsHeavyFoodService = value; IsModified = true; }
    public void SetPermitsLightFoodService(bool value) { PermitsLightFoodService = value; IsModified = true; }
    public void SetPermitsRetail(bool value) { PermitsRetail = value; IsModified = true; }
    public void SetPermitsServices(bool value) { PermitsServices = value; IsModified = true; }
    public void SetPermitsOffice(bool value) { PermitsOffice = value; IsModified = true; }
    public void SetPermitsOther(bool value) { PermitsOther = value; IsModified = true; }
    public void SetPermittedBusinessDetails(string value) { PermittedBusinessDetails = value; IsModified = true; }
    public void SetFacilitiesRemarks(string value) { FacilitiesRemarks = value; IsModified = true; }

    #endregion
}
