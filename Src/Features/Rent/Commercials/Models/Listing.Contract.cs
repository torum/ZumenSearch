namespace ZumenSearch.Models.Rent.Commercials.Listing;

public sealed partial class Listing
{
    public decimal ManagementFee { get; private set; }
    public decimal SecurityDeposit { get; private set; }
    public decimal GuaranteeDeposit { get; private set; }
    public decimal KeyMoney { get; private set; }
    public decimal RightsMoney { get; private set; }
    public decimal DepositDeduction { get; private set; }
    public decimal RenewalFee { get; private set; }
    public decimal RecontractFee { get; private set; }
    public decimal? ContractPeriodYears { get; private set; }
    public ListingFeeUnitTypeLabel SecurityDepositUnit { get; private set; } = new(ListingFeeUnitType.Months);
    public ListingFeeUnitTypeLabel GuaranteeDepositUnit { get; private set; } = new(ListingFeeUnitType.Months);
    public ListingFeeUnitTypeLabel KeyMoneyUnit { get; private set; } = new(ListingFeeUnitType.Months);
    public ListingFeeUnitTypeLabel RightsMoneyUnit { get; private set; } = new(ListingFeeUnitType.Months);
    public ListingFeeUnitTypeLabel DepositDeductionUnit { get; private set; } = new(ListingFeeUnitType.Months);
    public ListingFeeUnitTypeLabel RenewalFeeUnit { get; private set; } = new(ListingFeeUnitType.Months);
    public ListingFeeUnitTypeLabel RecontractFeeUnit { get; private set; } = new(ListingFeeUnitType.Yen);
    public ListingContractTaxTypeLabel RentTax { get; private set; } = new(ListingContractTaxType.Unconfirmed);
    public ListingContractTaxTypeLabel ManagementFeeTax { get; private set; } = new(ListingContractTaxType.Unconfirmed);
    public ListingContractTypeLabel ContractType { get; private set; } = new(ListingContractType.Unspecified);
    public string GuarantorCompany { get; private set; } = string.Empty;
    public string Insurance { get; private set; } = string.Empty;
    public string OtherFees { get; private set; } = string.Empty;
    public string ContractConditions { get; private set; } = string.Empty;
    public string ContractRemarks { get; private set; } = string.Empty;

    private static decimal NonNegative(decimal value) => value >= 0 ? value : throw new ArgumentOutOfRangeException(nameof(value));
    public void SetManagementFee(decimal value) { ManagementFee = NonNegative(value); IsModified = true; }
    public void SetSecurityDeposit(decimal value) { SecurityDeposit = NonNegative(value); IsModified = true; }
    public void SetGuaranteeDeposit(decimal value) { GuaranteeDeposit = NonNegative(value); IsModified = true; }
    public void SetKeyMoney(decimal value) { KeyMoney = NonNegative(value); IsModified = true; }
    public void SetRightsMoney(decimal value) { RightsMoney = NonNegative(value); IsModified = true; }
    public void SetDepositDeduction(decimal value) { DepositDeduction = NonNegative(value); IsModified = true; }
    public void SetRenewalFee(decimal value) { RenewalFee = NonNegative(value); IsModified = true; }
    public void SetRecontractFee(decimal value) { RecontractFee = NonNegative(value); IsModified = true; }
    public void SetContractPeriodYears(decimal? value) { if (value is < 0) throw new ArgumentOutOfRangeException(nameof(value)); ContractPeriodYears = value; IsModified = true; }
    public void SetSecurityDepositUnitTypeLabel(ListingFeeUnitTypeLabel value) { SecurityDepositUnit = value; IsModified = true; }
    public void SetSecurityDepositUnitTypeLabelFromString(string value) => SetSecurityDepositUnitTypeLabel(new(ParseType<ListingFeeUnitType>(value)));
    public void SetGuaranteeDepositUnitTypeLabel(ListingFeeUnitTypeLabel value) { GuaranteeDepositUnit = value; IsModified = true; }
    public void SetGuaranteeDepositUnitTypeLabelFromString(string value) => SetGuaranteeDepositUnitTypeLabel(new(ParseType<ListingFeeUnitType>(value)));
    public void SetKeyMoneyUnitTypeLabel(ListingFeeUnitTypeLabel value) { KeyMoneyUnit = value; IsModified = true; }
    public void SetKeyMoneyUnitTypeLabelFromString(string value) => SetKeyMoneyUnitTypeLabel(new(ParseType<ListingFeeUnitType>(value)));
    public void SetRightsMoneyUnitTypeLabel(ListingFeeUnitTypeLabel value) { RightsMoneyUnit = value; IsModified = true; }
    public void SetRightsMoneyUnitTypeLabelFromString(string value) => SetRightsMoneyUnitTypeLabel(new(ParseType<ListingFeeUnitType>(value)));
    public void SetDepositDeductionUnitTypeLabel(ListingFeeUnitTypeLabel value) { DepositDeductionUnit = value; IsModified = true; }
    public void SetDepositDeductionUnitTypeLabelFromString(string value) => SetDepositDeductionUnitTypeLabel(new(ParseType<ListingFeeUnitType>(value)));
    public void SetRenewalFeeUnitTypeLabel(ListingFeeUnitTypeLabel value) { RenewalFeeUnit = value; IsModified = true; }
    public void SetRenewalFeeUnitTypeLabelFromString(string value) => SetRenewalFeeUnitTypeLabel(new(ParseType<ListingFeeUnitType>(value)));
    public void SetRecontractFeeUnitTypeLabel(ListingFeeUnitTypeLabel value) { RecontractFeeUnit = value; IsModified = true; }
    public void SetRecontractFeeUnitTypeLabelFromString(string value) => SetRecontractFeeUnitTypeLabel(new(ParseType<ListingFeeUnitType>(value)));
    public void SetRentTaxTypeLabel(ListingContractTaxTypeLabel value) { RentTax = value; IsModified = true; }
    public void SetRentTaxTypeLabelFromString(string value) => SetRentTaxTypeLabel(new(ParseType<ListingContractTaxType>(value)));
    public void SetManagementFeeTaxTypeLabel(ListingContractTaxTypeLabel value) { ManagementFeeTax = value; IsModified = true; }
    public void SetManagementFeeTaxTypeLabelFromString(string value) => SetManagementFeeTaxTypeLabel(new(ParseType<ListingContractTaxType>(value)));
    public void SetContractTypeLabel(ListingContractTypeLabel value) { ContractType = value; IsModified = true; }
    public void SetContractTypeLabelFromString(string value) => SetContractTypeLabel(new(ParseType<ListingContractType>(value)));
    public void SetGuarantorCompany(string value) { GuarantorCompany = value; IsModified = true; }
    public void SetInsurance(string value) { Insurance = value; IsModified = true; }
    public void SetOtherFees(string value) { OtherFees = value; IsModified = true; }
    public void SetContractConditions(string value) { ContractConditions = value; IsModified = true; }
    public void SetContractRemarks(string value) { ContractRemarks = value; IsModified = true; }
}
